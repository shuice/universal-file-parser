using System;
using System.Collections.Generic;
using System.Text;

namespace System.Collections.Generic
{
 
    public interface IMyReadOnlyDictionary<TKey, TValue> : IEnumerable<MyKeyValuePair<TKey, TValue>>, IEnumerable, IReadOnlyCollection<MyKeyValuePair<TKey, TValue>>
    {
        TValue this[TKey key] { get; }
        IEnumerable<TKey> Keys { get; }
        IEnumerable<TValue> Values { get; }
        bool ContainsKey(TKey key);
        bool TryGetValue(TKey key, out TValue value);
    }
}