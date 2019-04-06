using System;
using System.IO;
using StreamIndexingUtils.Models;

namespace StreamIndexingUtils
{
    public class IndexedReadOnlyStream : IndexedStream
    {
        public IndexedReadOnlyStream(Stream stream, ContentIndex index, string id)
            : base(stream, index, id)
        {
        }

        public override bool CanWrite => false;

        public override void Flush()
        {
        }

        public override void Initialize(ContentIndex index, string id)
        {
            ContentPointer = index[id];
            BaseStream.Seek(ContentPointer.Start, SeekOrigin.Begin);
            Position = 0;
            base.Initialize(index, id);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException("Stream does not support writing");
        }
    }
}