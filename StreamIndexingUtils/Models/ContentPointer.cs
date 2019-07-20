namespace StreamIndexingUtils.Models
{
    public sealed class ContentPointer
    {
        public ContentPointer()
            : this(0, 0)
        {
        }

        public ContentPointer(long start, long length)
        {
            Start = start;
            Length = length;
        }

        public ContentPointer(ContentPointer contentPointer)
        {
            Start = contentPointer.Start;
            Length = contentPointer.Length;
        }

        public long Start { get; set; }

        public long Length { get; set; }
    }
}