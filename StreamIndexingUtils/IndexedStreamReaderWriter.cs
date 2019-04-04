using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StreamIndexingUtils.Extensions;
using StreamIndexingUtils.Models;

namespace StreamIndexingUtils
{
    public class IndexedStreamReaderWriter : IDisposable
    {
        // Same value for buffer as Stream.CopyTo uses by default
        private const int DefaultCopyBufferSize = 81920;

        public IndexedStreamReaderWriter(Stream stream)
            : this(stream, null)
        {
        }

        public IndexedStreamReaderWriter(Stream stream, ContentIndex index)
        {
            BaseStream = stream ?? throw new ArgumentNullException(nameof(stream));
            CurrentContentIndex = index;
        }

        public Stream BaseStream { get; }

        public ContentIndex CurrentContentIndex { get; set; }

        public void Dispose()
        {
            if (BaseStream != null)
            {
                BaseStream.Dispose();
            }
        }

        public async Task FlushAsync()
        {
            await BaseStream.FlushAsync();
        }

        public async Task LoadContentIndexAsync()
        {
            CurrentContentIndex = await ReadContentIndexAsync().ConfigureAwait(false);
        }

        public async Task ReadAsync(Stream destination, string id)
        {
            if (CurrentContentIndex == null)
            {
                throw new ArgumentNullException(nameof(CurrentContentIndex), "The current content index is not set");
            }

            if (!CurrentContentIndex.TryGetValue(id, out ContentPointer itemPointer))
            {
                throw new ArgumentException($@"Id does not exist in the index.
Id: {id}");
            }

            BaseStream.Seek(itemPointer.Start, SeekOrigin.Begin);

            await BaseStream.CopyToAsync(destination, itemPointer.Length);
        }

        public async Task<ContentIndex> ReadContentIndexAsync()
        {
            var sizeOfPositionPointer = sizeof(long);
            if (BaseStream.Length > sizeOfPositionPointer)
            {
                BaseStream.Seek(-sizeOfPositionPointer, SeekOrigin.End);
            }
            else
            {
                throw new FormatException("The base stream is not in correct format");
            }

            var positionPointerBytes = new byte[sizeOfPositionPointer];
            BaseStream.Read(positionPointerBytes, 0, sizeOfPositionPointer);
            var position = BitConverter.ToInt64(positionPointerBytes, 0);

            BaseStream.Seek(position, SeekOrigin.Begin);

            try
            {
                var buffer = new byte[BaseStream.Length - position - sizeOfPositionPointer];
                await BaseStream.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);

                using (var streamReader = new StreamReader(new MemoryStream(buffer)))
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

        public Task SaveContentIndexAsync()
        {
            return SaveContentIndexAsync(0);
        }

        public async Task SaveContentIndexAsync(long offset)
        {
            if (CurrentContentIndex == null)
            {
                throw new ArgumentNullException(nameof(CurrentContentIndex), "The current content index is not set");
            }

            using (var memoryStream = new MemoryStream())
            using (var streamWriter = new StreamWriter(memoryStream))
            {
                var serializer = JsonSerializer.CreateDefault();
                serializer.Serialize(streamWriter, CurrentContentIndex);
                await streamWriter.FlushAsync().ConfigureAwait(false);

                var lastItemPointer = CurrentContentIndex.GetLastItemContentPointer();

                var indexStartPos = lastItemPointer == null
                    ? offset : lastItemPointer.Start + lastItemPointer.Length;

                var positionPointerBytes = BitConverter.GetBytes(indexStartPos);

                BaseStream.Seek(indexStartPos, SeekOrigin.Begin);

                memoryStream.Seek(0, SeekOrigin.Begin);
                await memoryStream.CopyToAsync(BaseStream).ConfigureAwait(false);

                await BaseStream.WriteAsync(positionPointerBytes, 0, positionPointerBytes.Length).ConfigureAwait(false);

                BaseStream.SetLength(BaseStream.Position);
            }
        }

        public Task WriteAsync(Stream source, string id)
        {
            return WriteAsync(source, id, false, 0);
        }

        public Task WriteAsync(Stream source, string id, bool overwrite)
        {
            return WriteAsync(source, id, overwrite, 0);
        }

        public Task WriteAsync(Stream source, string id, long offset)
        {
            return WriteAsync(source, id, false, offset);
        }

        public async Task WriteAsync(Stream source, string id, bool overwrite, long offset)
        {
            if (CurrentContentIndex == null)
            {
                throw new ArgumentNullException(nameof(CurrentContentIndex), "The current content index is not set");
            }

            if (!overwrite && CurrentContentIndex.ContainsKey(id))
            {
                throw new ArgumentException($@"An element with the same id already exists in the content index.
Id: {id}");
            }

            var lastItemPointer = CurrentContentIndex.GetLastItemContentPointer();

            var sourceStartPos = lastItemPointer == null
                ? offset : lastItemPointer.Start + lastItemPointer.Length;

            var sourceLength = source.Length - source.Position;

            try
            {
                BaseStream.Seek(sourceStartPos, SeekOrigin.Begin);

                await source.CopyToAsync(BaseStream).ConfigureAwait(false);
            }
            catch (Exception)
            {
                throw;
            }

            CurrentContentIndex.AddOrUpdate(id, sourceStartPos, sourceLength);
        }

        public async Task RemoveAsync(string id)
        {
            if (!CurrentContentIndex.ContainsKey(id))
            {
                throw new ArgumentException($@"Id does not exist in the index.
Id: {id}");
            }

            if (CurrentContentIndex.GetLastItemId() == id)
            {
                CurrentContentIndex.Remove(id);
                return;
            }

            var orderedCopy = CurrentContentIndex
                .OrderBy(x => x.Value.Start)
                .ToList();

            var itemToRemoveIndex = orderedCopy.FindIndex(x => x.Key == id);
            var itemToRemove = orderedCopy[itemToRemoveIndex];
            var nextItem = orderedCopy[itemToRemoveIndex + 1];

            byte[] buffer = new byte[DefaultCopyBufferSize];
            long bytes = BaseStream.Length - nextItem.Value.Start;
            long sourcePosition = nextItem.Value.Start;
            long destinationPosition = itemToRemove.Value.Start;
            int read;

            BaseStream.Seek(sourcePosition, SeekOrigin.Begin);
            while (bytes > 0 &&
                   (read = await BaseStream.ReadAsync(buffer, 0, (int)Math.Min(buffer.Length, bytes))) > 0)
            {
                sourcePosition = BaseStream.Position;
                BaseStream.Seek(destinationPosition, SeekOrigin.Begin);
                await BaseStream.WriteAsync(buffer, 0, read);
                destinationPosition = BaseStream.Position;
                BaseStream.Seek(sourcePosition, SeekOrigin.Begin);
                bytes -= read;
            }

            BaseStream.SetLength(destinationPosition);

            for (int i = itemToRemoveIndex + 1; i < orderedCopy.Count; i++)
            {
                orderedCopy[i].Value.Start -= itemToRemove.Value.Length;
            }

            CurrentContentIndex.Remove(id);
        }
    }
}