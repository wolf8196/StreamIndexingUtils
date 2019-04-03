using System.Collections.Generic;
using System.Linq;

namespace StreamIndexingUtils.Models
{
    public class ContentIndex : Dictionary<string, ContentPointer>
    {
        public ContentIndex()
            : base()
        {
        }

        public ContentIndex(ContentIndex index)
            : base(index)
        {
        }

        public void Add(string id, long start, long length)
        {
            Add(id, new ContentPointer(start, length));
        }

        public void Update(string id, long start, long length)
        {
            var item = this[id];

            item.Start = start;
            item.Length = length;
        }

        public void AddOrUpdate(string id, long start, long length)
        {
            if (!TryGetValue(id, out ContentPointer value))
            {
                Add(id, start, length);
            }
            else
            {
                value.Start = start;
                value.Length = length;
            }
        }

        public string GetLastItemId()
        {
            return GetLastItem().Key;
        }

        public ContentPointer GetLastItemContentPointer()
        {
            return GetLastItem().Value;
        }

        public KeyValuePair<string, ContentPointer> GetLastItem()
        {
            return this
                 .Where(x => x.Value.Start == this.Max(y => y.Value.Start))
                 .FirstOrDefault();
        }
    }
}