﻿namespace StreamIndexingUtils.Models
{
    public class ContentPointer
    {
        public ContentPointer(long start, long length)
        {
            Start = start;
            Length = length;
        }

        public long Start { get; set; }

        public long Length { get; set; }
    }
}