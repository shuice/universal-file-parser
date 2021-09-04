using System;
using System.Collections.Generic;
using System.Text;

namespace System.Collections.Generic
{
    //
    // Summary:
    //     Represents a generic collection of key/value pairs.
    //
    // Type parameters:
    //   TKey:
    //     The type of keys in the dictionary.
    //
    //   TValue:
    //     The type of values in the dictionary.
    public interface IMyDictionary<TKey, TValue> : ICollection<MyKeyValuePair<TKey, TValue>>, IEnumerable<MyKeyValuePair<TKey, TValue>>, IEnumerable
    {

        TValue this[TKey key] { get; set; }


        ICollection<TKey> Keys { get; }

        ICollection<TValue> Values { get; }


        void Add(TKey key, TValue value);

        bool ContainsKey(TKey key);

        bool Remove(TKey key);

        bool TryGetValue(TKey key, out TValue value);
    }
}