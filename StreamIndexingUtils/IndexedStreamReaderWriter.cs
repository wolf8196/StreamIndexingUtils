using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using StreamIndexingUtils.Extensions;
using StreamIndexingUtils.Models;
using StreamIndexingUtils.Extensions;
using static StreamIndexingUtils.Extensions.StreamExtensions;

namespace StreamIndexingUtils
{
    public sealed class IndexedStreamReaderWriter : IDisposable
    {
        private readonly bool leaveOpen;

        public IndexedStreamReaderWriter(Stream stream)
            : this(stream, null, false)
        {
        }

        public IndexedStreamReaderWriter(Stream stream, ContentIndex index)
            : this(stream, null, false)
        {
        }

        public IndexedStreamReaderWriter(Stream stream, ContentIndex index, bool leaveOpen)
        {
            BaseStream = stream.ThrowIfNull(nameof(stream));
            CurrentContentIndex = index;
            this.leaveOpen = leaveOpen;
        }

        public Stream BaseStream { get; }

        public ContentIndex CurrentContentIndex { get; set; }

        public void Dispose()
        {
            Flush();

            if (!leaveOpen)
            {
                BaseStream.Dispose();
            }
        }

        public void Flush()
        {
            BaseStream.Flush();
        }

        public async Task FlushAsync()
        {
            await BaseStream.FlushAsync().ConfigureAwait(false);
        }

        public async Task ReadAsync(Stream destination, string id)
        {
            destination.ThrowIfNull(nameof(destination));
            id.ThrowIfNull(nameof(id));

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

            await BaseStream.CopyToAsync(destination, itemPointer.Length).ConfigureAwait(false);
        }

        public async Task WriteAsync(Stream source, string id)
        {
            source.ThrowIfNull(nameof(source));
            id.ThrowIfNull(nameof(id));

            if (CurrentContentIndex == null)
            {
                throw new ArgumentNullException(nameof(CurrentContentIndex), "The current content index is not set");
            }

            if (CurrentContentIndex.ContainsKey(id))
            {
                throw new ArgumentException($@"An element with the same id already exists in the content index.
Id: {id}");
            }

            var lastItemPointer = CurrentContentIndex.GetLastItemContentPointer();

            var sourceStartPos = lastItemPointer == null
                ? CurrentContentIndex.Offset
                : lastItemPointer.Start + lastItemPointer.Length;

            var sourceLength = source.Length - source.Position;

            BaseStream.Seek(sourceStartPos, SeekOrigin.Begin);

            await source.CopyToAsync(BaseStream).ConfigureAwait(false);

            CurrentContentIndex.AddOrUpdate(id, sourceStartPos, sourceLength);
        }

        public async Task RemoveAsync(string id)
        {
            id.ThrowIfNull(nameof(id));

            if (!CurrentContentIndex.ContainsKey(id))
            {
                throw new ArgumentException($@"Id does not exist in the index.
Id: {id}");
            }

            if (CurrentContentIndex.GetLastItemId() == id)
            {
                CurrentContentIndex.Remove(id);
                var newLastItem = CurrentContentIndex.GetLastItemContentPointer();

                BaseStream.SetLength(newLastItem == null
                    ? CurrentContentIndex.Offset
                    : newLastItem.Start + newLastItem.Length);

                return;
            }

            var orderedCopy = CurrentContentIndex
                .OrderBy(x => x.Value.Start)
                .ToList();

            var itemToRemoveIndex = orderedCopy.FindIndex(x => x.Key == id);
            var itemToRemove = orderedCopy[itemToRemoveIndex];
            var nextItem = orderedCopy[itemToRemoveIndex + 1];

            long bytes = BaseStream.Length - nextItem.Value.Start;
            byte[] buffer = new byte[Math.Min(DefaultCopyBufferSize, bytes)];
            long sourcePosition = nextItem.Value.Start;
            long destinationPosition = itemToRemove.Value.Start;
            int read;

            BaseStream.Seek(sourcePosition, SeekOrigin.Begin);
            while (bytes > 0 &&
                   (read = await BaseStream
                        .ReadAsync(buffer, 0, (int)Math.Min(buffer.Length, bytes))
                        .ConfigureAwait(false)) > 0)
            {
                sourcePosition = BaseStream.Position;
                BaseStream.Seek(destinationPosition, SeekOrigin.Begin);
                await BaseStream.WriteAsync(buffer, 0, read).ConfigureAwait(false);
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

        public async Task LoadContentIndexAsync()
        {
            CurrentContentIndex = await ReadContentIndexAsync().ConfigureAwait(false);
        }

        public async Task<ContentIndex> ReadContentIndexAsync()
        {
            return await new IndexSerializer().DeserializeAsync(BaseStream).ConfigureAwait(false);
        }

        public async Task SaveContentIndexAsync()
        {
            if (CurrentContentIndex == null)
            {
                throw new ArgumentNullException(nameof(CurrentContentIndex), "The current content index is not set");
            }

            await new IndexSerializer().SerializeAsync(CurrentContentIndex, BaseStream).ConfigureAwait(false);
        }
    }
}