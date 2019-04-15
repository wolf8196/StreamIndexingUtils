using System;
using System.IO;
using StreamIndexingUtils.Models;

namespace StreamIndexingUtils
{
    public class IndexedWriteStream : IndexedStream
    {
        public IndexedWriteStream(Stream stream, ContentIndex index, string id)
            : this(stream, index, id, false)
        {
        }

        public IndexedWriteStream(Stream stream, ContentIndex index, string id, bool leaveOpen)
            : base(stream, index, id, leaveOpen)
        {
        }

        public override bool CanWrite => true;

        public override void Flush()
        {
            BaseStream.Flush();
        }

        public override void Initialize(ContentIndex index, string id)
        {
            if (index.ContainsKey(id))
            {
                throw new ArgumentException($@"An element with the same id already exists in the content index.
Id: {id}");
            }

            var lastItemPointer = index.GetLastItemContentPointer();

            var sourceStartPos = lastItemPointer == null
                ? index.Offset
                : lastItemPointer.Start + lastItemPointer.Length;

            ContentPointer = new ContentPointer(sourceStartPos, 0);
            Position = 0;

            base.Initialize(index, id);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            BaseStream.Write(buffer, offset, count);
            ContentPointer.Length += count;
            PositionInternal += count;
            Index.AddOrUpdate(Id, ContentPointer.Start, ContentPointer.Length);
        }
    }
}