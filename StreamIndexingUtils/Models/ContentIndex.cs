using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace StreamIndexingUtils.Models
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public sealed class ContentIndex : IEnumerable<KeyValuePair<string, ContentPointer>>, IEnumerable
    {
        [JsonProperty]
        private readonly Dictionary<string, ContentPointer> index;

        [JsonProperty]
        private readonly long offset;

        public ContentIndex()
            : this(0)
        {
        }

        public ContentIndex(long offset)
        {
            this.offset = offset;
            index = new Dictionary<string, ContentPointer>();
        }

        public ContentIndex(ContentIndex contentIndex)
        {
            offset = contentIndex.offset;
            index = new Dictionary<string, ContentPointer>();
            foreach (var item in contentIndex.index)
            {
                index.Add(item.Key, new ContentPointer(item.Value));
            }
        }

        public int Count => index.Count;

        public ICollection<string> Keys => index.Keys;

        public ICollection<ContentPointer> Values { get => index.Values; }

        public long Offset { get => offset; }

        public ContentPointer this[string key]
        {
            get => index[key];
            set => index[key] = value;
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

        public KeyValuePair<string, ContentPointer> GetLastItem()
        {
            return index
                 .Where(x => x.Value.Start == index.Max(y => y.Value.Start))
                 .FirstOrDefault();
        }

        public ContentPointer GetLastItemContentPointer()
        {
            return GetLastItem().Value;
        }

        public string GetLastItemId()
        {
            return GetLastItem().Key;
        }

        public void Add(string key, ContentPointer value)
        {
            index.Add(key, value);
        }

        public bool ContainsKey(string key)
        {
            return index.ContainsKey(key);
        }

        public bool Remove(string key)
        {
            return index.Remove(key);
        }

        public bool TryGetValue(string key, out ContentPointer value)
        {
            return index.TryGetValue(key, out value);
        }

        public IEnumerator<KeyValuePair<string, ContentPointer>> GetEnumerator()
        {
            return index.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}