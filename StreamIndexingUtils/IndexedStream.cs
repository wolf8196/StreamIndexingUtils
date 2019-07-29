using System;
using System.IO;
using StreamIndexingUtils.Models;
using StreamIndexingUtils.Extensions;

namespace StreamIndexingUtils
{
    public abstract class IndexedStream : Stream
    {
        private readonly bool leaveOpen;

        protected IndexedStream(Stream stream, ContentIndex index, string id)
            : this(stream, index, id, false)
        {
        }

        protected IndexedStream(Stream stream, ContentIndex index, string id, bool leaveOpen)
        {
            this.leaveOpen = leaveOpen;
            BaseStream = stream.ThrowIfNull(nameof(stream));

            if (!BaseStream.CanSeek || !BaseStream.CanRead)
            {
                throw new ArgumentException(nameof(stream), "Stream must support Seek and Read");
            }

            Initialize(index, id);
        }

        public sealed override bool CanRead => true;

        public sealed override bool CanSeek => true;

        public string Id { get; private set; }

        public ContentIndex Index { get; private set; }

        public sealed override long Length => ContentPointer?.Length ?? 0;

        public sealed override long Position
        {
            get
            {
                return PositionInternal;
            }

            set
            {
                PositionInternal = value;
                BaseStream.Position = ContentPointer.Start + value;
            }
        }

        protected Stream BaseStream { get; }

        protected ContentPointer ContentPointer { get; set; }

        protected long PositionInternal { get; set; }

        public virtual void Initialize(ContentIndex index, string id)
        {
            Index = index;
            Id = id;
        }

        public sealed override int Read(byte[] buffer, int offset, int count)
        {
            var readBytes = Length - Position < 0 ? 0 : Length - Position;
            var read = BaseStream.Read(buffer, offset, (int)Math.Min(count, readBytes));

            PositionInternal += read;

            return read;
        }

        public sealed override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    {
                        Position = offset;
                    }

                    break;

                case SeekOrigin.Current:
                    {
                        Position += offset;
                    }

                    break;

                case SeekOrigin.End:
                    {
                        Position = ContentPointer.Length + offset;
                    }

                    break;
            }

            return Position;
        }

        public sealed override void SetLength(long value)
        {
            throw new NotSupportedException("Stream does not support setting length");
        }

        protected sealed override void Dispose(bool disposing)
        {
            Flush();

            if (disposing && !leaveOpen)
            {
                BaseStream.Dispose();
            }
        }
    }
}