using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StreamIndexingUtils.Extensions;
using StreamIndexingUtils.Models;
using StreamIndexingUtils.Utils;

namespace StreamIndexingUtils
{
    public class IndexSerializer
    {
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

            var sizeOfPositionPointer = sizeof(long);
            if (source.Length > sizeOfPositionPointer)
            {
                source.Seek(-sizeOfPositionPointer, SeekOrigin.End);
            }
            else
            {
                throw new FormatException("The base stream is not in correct format");
            }

            var positionPointerBytes = new byte[sizeOfPositionPointer];
            await source.ReadAsync(positionPointerBytes, 0, sizeOfPositionPointer).ConfigureAwait(false);
            var position = BitConverter.ToInt64(positionPointerBytes, 0);

            source.Seek(position, SeekOrigin.Begin);

            try
            {
                var transformationSource = new MemoryStream();
                await source
                    .CopyToAsync(transformationSource, source.Length - position - sizeOfPositionPointer)
                    .ConfigureAwait(false);
                transformationSource.Seek(0, SeekOrigin.Begin);
                var transformationDestination = new MemoryStream();

                await transformation(transformationSource, transformationDestination).ConfigureAwait(false);

                transformationDestination.Seek(0, SeekOrigin.Begin);
                using (var streamReader = new StreamReader(transformationDestination))
                {
                    var serializer = JsonSerializer.CreateDefault();
                    return (ContentIndex)serializer.Deserialize(streamReader, typeof(ContentIndex));
                }
            }
            catch (Exception ex)
            {
                throw new FormatException("The base stream is not in correct format", ex);
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