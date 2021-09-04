using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Collections.Generic
{
    public class MyDictionary<TKey, TValue> : IEnumerable<MyKeyValuePair<TKey, TValue>>, IMyDictionary<TKey, TValue>, IMyReadOnlyDictionary<TKey, TValue>
    {
        
        private Dictionary<TKey, TValue> dic;
        public MyDictionary()
        {
            dic = new Dictionary<TKey, TValue>();
        }

      
        public MyDictionary(int capacity)
        {
            dic = new Dictionary<TKey, TValue>(capacity);
        }
        public MyDictionary(int capacity, IEqualityComparer<TKey> comparer)
        {
            dic = new Dictionary<TKey, TValue>(capacity, comparer);
        }

        public MyDictionary(IMyDictionary<TKey, TValue> inDic)
        {
            dic = new Dictionary<TKey, TValue>();
            foreach (MyKeyValuePair<TKey, TValue> pair in inDic)
            {
                dic.Add(pair.Key, pair.Value);
            }
        }

  
        public MyDictionary(IEqualityComparer<TKey> comparer)
        {
            dic = new Dictionary<TKey, TValue>(comparer);
        }
        

        public TValue this[TKey key]
        {
            get => dic[key];
            set => dic[key] = value;
        }

        public ICollection<TKey> Keys => dic.Keys;

        public ICollection<TValue> Values => dic.Values;

        public int Count => dic.Count;

        public bool IsReadOnly => false;

        IEnumerable<TKey> IMyReadOnlyDictionary<TKey, TValue>.Keys => dic.Keys;

        IEnumerable<TValue> IMyReadOnlyDictionary<TKey, TValue>.Values => dic.Values;

        public void Add(TKey key, TValue value)
        {
            dic.Add(key, value);
        }



        public void Clear()
        {
            dic.Clear();
        }


        public bool ContainsKey(TKey key)
        {
            return dic.ContainsKey(key);
        }



        public IEnumerator<MyKeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return dic.Select(item => new MyKeyValuePair<TKey, TValue>(item.Key, item.Value)).GetEnumerator();
        }

        public bool Remove(TKey key)
        {
            return dic.Remove(key);
        }

        public bool Remove(MyKeyValuePair<TKey, TValue> item)
        {
            return Remove(item.Key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return dic.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return dic.Select(item => new MyKeyValuePair<TKey, TValue>(item.Key, item.Value)).GetEnumerator();
        }



        public void Add(MyKeyValuePair<TKey, TValue> item)
        {
            dic.Add(item.Key, item.Value);
        }

        public bool Contains(MyKeyValuePair<TKey, TValue> item)
        {
            return dic.Contains(new KeyValuePair<TKey, TValue>(item.Key, item.Value));
        }

        public bool ContainsValue(TValue value)
        {
            return dic.ContainsValue(value);
        }

        public void CopyTo(MyKeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            foreach (KeyValuePair<TKey, TValue> pair in dic)
            {
                array[arrayIndex] = new MyKeyValuePair<TKey, TValue>(pair.Key, pair.Value);
                arrayIndex++;
            }
        }
    }
}