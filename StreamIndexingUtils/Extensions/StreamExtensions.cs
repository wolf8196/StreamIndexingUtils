using System;
using System.IO;
using System.Threading.Tasks;

namespace StreamIndexingUtils.Extensions
{
    public static class StreamExtensions
    {
        // Same value for buffer as Stream.CopyTo uses by default
        public const int DefaultCopyBufferSize = 81920;

        public static void CopyTo(this Stream source, Stream destination, long length)
        {
            byte[] buffer = new byte[Math.Min(DefaultCopyBufferSize, length)];

            int read;
            while (length > 0 && (read = source.Read(buffer, 0, (int)Math.Min(buffer.Length, length))) > 0)
            {
                destination.Write(buffer, 0, read);
                length -= read;
            }
        }

        public static async Task CopyToAsync(this Stream source, Stream destination, long length)
        {
            byte[] buffer = new byte[Math.Min(DefaultCopyBufferSize, length)];
            int read;

            while (length > 0 && (read = await source.ReadAsync(buffer, 0, (int)Math.Min(buffer.Length, length))) > 0)
            {
                await destination.WriteAsync(buffer, 0, read);
                length -= read;
            }
        }
    }
}