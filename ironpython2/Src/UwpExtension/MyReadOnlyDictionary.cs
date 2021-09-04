using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Collections.Generic
{
    public class MyReadOnlyDictionary<TKey, TValue> : ICollection<MyKeyValuePair<TKey, TValue>>,
                                                        IEnumerable<MyKeyValuePair<TKey, TValue>>, 
                                                        IEnumerable, 
                                                        IMyDictionary<TKey, TValue>, 
                                                        IReadOnlyCollection<MyKeyValuePair<TKey, TValue>>, 
                                                        IMyReadOnlyDictionary<TKey, TValue>,
                                                        ICollection, 
                                                        IDictionary
    {        
        private Dictionary<TKey, TValue> dic;
        public MyReadOnlyDictionary()
        {
            dic = new Dictionary<TKey, TValue>();
        }

        public MyReadOnlyDictionary(IMyDictionary<TKey, TValue> inDic)
        {
            dic = new Dictionary<TKey, TValue>();
            foreach (MyKeyValuePair<TKey, TValue> pair in inDic)
            {
                dic.Add(pair.Key, pair.Value);
            }
        }

        public TValue this[TKey key] 
        {
            get => dic[key];
            set => dic[key] = value;
        }
        public object this[object key]
        {
            get => dic[(TKey)key];
            set => dic[(TKey)key] = (TValue)value;
        }

        public int Count => dic.Count;

        public bool IsReadOnly => true;

        public ICollection<TKey> Keys => dic.Keys;

        public ICollection<TValue> Values => dic.Values;

        public bool IsSynchronized => false;

        public object SyncRoot => this;

        public bool IsFixedSize => true;

        IEnumerable<TKey> IMyReadOnlyDictionary<TKey, TValue>.Keys => dic.Keys;

        ICollection IDictionary.Keys => dic.Keys;

        IEnumerable<TValue> IMyReadOnlyDictionary<TKey, TValue>.Values => dic.Values;

        ICollection IDictionary.Values => dic.Values;

        public void Add(MyKeyValuePair<TKey, TValue> item)
        {
            dic[item.Key] = item.Value;
        }

        public void Add(TKey key, TValue value)
        {
            dic[key] = value;
        }

        public void Add(object key, object value)
        {
            dic[(TKey)key] = (TValue)value;
        }

        public void Clear()
        {
            dic.Clear();
        }

        public bool Contains(MyKeyValuePair<TKey, TValue> item)
        {
            TValue value;
            if (dic.TryGetValue(item.Key, out value))
            {
                return item.Value.Equals(value);
            }
            return false;
        }

        public bool Contains(object key)
        {
            return dic.ContainsKey((TKey)key);
        }

        public bool ContainsKey(TKey key)
        {
            return dic.ContainsKey(key);
        }

        public void CopyTo(MyKeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            foreach(KeyValuePair<TKey, TValue> pair in dic)
            {
                array[arrayIndex] = new MyKeyValuePair<TKey, TValue>(pair.Key, pair.Value);
                arrayIndex++;
            }
        }

        public void CopyTo(Array array, int index)
        {
            foreach (KeyValuePair<TKey, TValue> pair in dic)
            {
                array.SetValue(new MyKeyValuePair<TKey, TValue>(pair.Key, pair.Value), index);
                index++;
            }
        }

        public IEnumerator<MyKeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return dic.Select(item => new MyKeyValuePair<TKey, TValue>(item.Key, item.Value)).GetEnumerator();
        }

        public bool Remove(MyKeyValuePair<TKey, TValue> item)
        {
            if (Contains(item))
            {
                dic.Remove(item.Key);
            }
            return false;
        }

        public bool Remove(TKey key)
        {
            return dic.Remove(key);
        }

        public void Remove(object key)
        {
            dic.Remove((TKey)key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return dic.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return dic.Select(item => new MyKeyValuePair<TKey, TValue>(item.Key, item.Value)).GetEnumerator();
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return new SimpleDictionaryEnumerator<TKey, TValue>(dic.Take(dic.Count).ToList());
        }
    }
}
