// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

using System.Collections;
using System.Collections.Generic;

namespace AnyPrefix.Microsoft.Scripting.Utils {

    /// <summary>
    /// Dictionary[TKey, TValue] is not thread-safe in the face of concurrent reads and writes. SynchronizedDictionary
    /// provides a thread-safe implementation. It holds onto a Dictionary[TKey, TValue] instead of inheriting from
    /// it so that users who need to do manual synchronization can access the underlying Dictionary[TKey, TValue].
    /// </summary>
    public class SynchronizedDictionary<TKey, TValue> :
        IMyDictionary<TKey, TValue>,
        ICollection<MyKeyValuePair<TKey, TValue>>,
        IEnumerable<MyKeyValuePair<TKey, TValue>> {

        MyDictionary<TKey, TValue> _dictionary;

        /// <summary>
        /// This returns the raw unsynchronized Dictionary[TKey, TValue]. Users are responsible for locking
        /// on it before accessing it. Also, it should not be arbitrarily handed out to other code since deadlocks
        /// can be caused if other code incorrectly locks on it.
        /// </summary>
        public MyDictionary<TKey, TValue> UnderlyingDictionary {
            get { return _dictionary; }
        }

        public SynchronizedDictionary() 
            : this(new MyDictionary<TKey, TValue>()) {
        }

        public SynchronizedDictionary(MyDictionary<TKey, TValue> dictionary) {
            _dictionary = dictionary;
        }

        #region IMyDictionary<TKey,TValue> Members

        public void Add(TKey key, TValue value) {
            lock (_dictionary) {
                _dictionary.Add(key, value);
            }
        }

        public bool ContainsKey(TKey key) {
            lock (_dictionary) {
                return _dictionary.ContainsKey(key);
            }
        }

        public ICollection<TKey> Keys {
            get {
                lock (_dictionary) {
                    return _dictionary.Keys;
                }
            }
        }

        public bool Remove(TKey key) {
            lock (_dictionary) {
                return _dictionary.Remove(key);
            }
        }

        public bool TryGetValue(TKey key, out TValue value) {
            lock (_dictionary) {
                return _dictionary.TryGetValue(key, out value);
            }
        }

        public ICollection<TValue> Values {
            get {
                lock (_dictionary) {
                    return _dictionary.Values;
                }
            }
        }

        public TValue this[TKey key] {
            get {
                lock (_dictionary) {
                    return _dictionary[key];
                }
            }
            set {
                lock (_dictionary) {
                    _dictionary[key] = value;
                }
            }
        }

        #endregion

        #region ICollection<MyKeyValuePair<TKey,TValue>> Members

        private ICollection<MyKeyValuePair<TKey, TValue>> AsICollection() {
            return ((ICollection<MyKeyValuePair<TKey, TValue>>)_dictionary);
        }

        public void Add(MyKeyValuePair<TKey, TValue> item) {
            lock (_dictionary) {
                AsICollection().Add(item);
            }
        }

        public void Clear() {
            lock (_dictionary) {
                AsICollection().Clear();
            }
        }

        public bool Contains(MyKeyValuePair<TKey, TValue> item) {
            lock (_dictionary) {
                return AsICollection().Contains(item);
            }
        }

        public void CopyTo(MyKeyValuePair<TKey, TValue>[] array, int arrayIndex) {
            lock (_dictionary) {
                AsICollection().CopyTo(array, arrayIndex);
            }
        }

        public int Count {
            get {
                lock (_dictionary) {
                    return AsICollection().Count;
                }
            }
        }

        public bool IsReadOnly {
            get {
                lock (_dictionary) {
                    return AsICollection().IsReadOnly;
                }
            }
        }

        public bool Remove(MyKeyValuePair<TKey, TValue> item) {
            lock (_dictionary) {
                return AsICollection().Remove(item);
            }
        }

        #endregion

        #region IEnumerable<MyKeyValuePair<TKey,TValue>> Members

        public IEnumerator<MyKeyValuePair<TKey, TValue>> GetEnumerator() {
            lock (_dictionary) {
                return _dictionary.GetEnumerator();
            }
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator() {
            lock (_dictionary) {
                return _dictionary.GetEnumerator();
            }
        }

        #endregion
    }
}
