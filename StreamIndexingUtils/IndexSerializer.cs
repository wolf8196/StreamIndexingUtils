using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StreamIndexingUtils.Extensions;
using StreamIndexingUtils.Models;
using StreamIndexingUtils.Extensions;

namespace StreamIndexingUtils
{
    public sealed class IndexSerializer
    {
        private const string FormatExceptionMessage = "Source stream has incorrect format";

        public async Task<ContentIndex> DeserializeAsync(Stream source)
        {
            return await DeserializeAsync(
                source,
                async (src, destination) =>
                {
                    await src.CopyToAsync(destination).ConfigureAwait(false);
                });
        }

        public async Task<ContentIndex> DeserializeAsync(
            Stream source,
            Func<Stream, Stream, Task> transformation)
        {
            source.ThrowIfNull(nameof(source));
            if (!source.CanSeek || !source.CanRead)
            {
                throw new ArgumentException(nameof(source), "Source stream must support Seek and Read");
            }

            var sizeOfPositionPointer = sizeof(long);
            if (source.Length > sizeOfPositionPointer)
            {
                source.Seek(-sizeOfPositionPointer, SeekOrigin.End);
            }
            else
            {
                throw new FormatException(FormatExceptionMessage);
            }

            var positionPointerBytes = new byte[sizeOfPositionPointer];
            await source.ReadAsync(positionPointerBytes, 0, sizeOfPositionPointer).ConfigureAwait(false);
            var position = BitConverter.ToInt64(positionPointerBytes, 0);

            source.Seek(position, SeekOrigin.Begin);

            var transformationSource = new MemoryStream();

            await source
                .CopyToAsync(transformationSource, source.Length - position - sizeOfPositionPointer)
                .ConfigureAwait(false);

            transformationSource.Seek(0, SeekOrigin.Begin);
            var transformationDestination = new MemoryStream();

            await transformation(transformationSource, transformationDestination).ConfigureAwait(false);

            transformationDestination.Seek(0, SeekOrigin.Begin);

            try
            {
                using (var streamReader = new StreamReader(transformationDestination))
                {
                    var serializer = JsonSerializer.CreateDefault();
                    return (ContentIndex)serializer.Deserialize(streamReader, typeof(ContentIndex));
                }
            }
            catch (JsonException ex)
            {
                throw new FormatException(FormatExceptionMessage, ex);
            }
        }

        public async Task SerializeAsync(ContentIndex index, Stream destination)
        {
            await SerializeAsync(
                index,
                destination,
                async (source, dest) =>
                {
                    await source.CopyToAsync(dest).ConfigureAwait(false);
                })
                .ConfigureAwait(false);
        }

        public async Task SerializeAsync(
            ContentIndex index,
            Stream destination,
            Func<Stream, Stream, Task> transformation)
        {
            index.ThrowIfNull(nameof(index));
            destination.ThrowIfNull(nameof(destination));
            transformation.ThrowIfNull(nameof(transformation));
            if (!destination.CanSeek || !destination.CanWrite)
            {
                throw new ArgumentException(nameof(destination), "Destination stream must support Seek and Write");
            }

            using (var memoryStream = new MemoryStream())
            using (var streamWriter = new StreamWriter(memoryStream))
            {
                var serializer = JsonSerializer.CreateDefault();
                serializer.Serialize(streamWriter, index);
                await streamWriter.FlushAsync().ConfigureAwait(false);
                memoryStream.Seek(0, SeekOrigin.Begin);

                var lastItemPointer = index.GetLastItemContentPointer();

                var indexStartPos = lastItemPointer == null
                    ? index.Offset
                    : lastItemPointer.Start + lastItemPointer.Length;

                var positionPointerBytes = BitConverter.GetBytes(indexStartPos);

                destination.Seek(indexStartPos, SeekOrigin.Begin);

                await transformation(memoryStream, destination).ConfigureAwait(false);

                await destination.WriteAsync(positionPointerBytes, 0, positionPointerBytes.Length).ConfigureAwait(false);

                destination.SetLength(destination.Position);
            }
        }
    }
}