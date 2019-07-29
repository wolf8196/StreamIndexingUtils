using System;

namespace StreamIndexingUtils.Extensions
{
    internal static class ObjectExtensions
    {
        internal static T ThrowIfNull<T>(this T obj, string paramName)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(paramName);
            }

            return obj;
        }
    }
}