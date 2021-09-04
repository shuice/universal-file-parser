// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.


using System;
using System.Collections.Generic;
using System.Diagnostics;
using AnyPrefix.Microsoft.Scripting;
using System.Threading;

namespace IronPython.Runtime {
    [Serializable]
    internal class StringDictionaryStorage : DictionaryStorage {
        private MyDictionary<string, object> _data;

        public StringDictionaryStorage() {
        }

        public StringDictionaryStorage(int count) {
            _data = new MyDictionary<string, object>(count, StringComparer.Ordinal);
        }

        public override void Add(ref DictionaryStorage storage, object key, object value) {
            Add(key, value);
        }

        public void Add(object key, object value) {
            lock (this) {
                AddNoLock(key, value);
            }
        }

        public override void AddNoLock(ref DictionaryStorage storage, object key, object value) {
            AddNoLock(key, value);
        }

        public void AddNoLock(object key, object value) {
            EnsureData();

            if (key is string strKey) {
                _data[strKey] = value;
            } else {
                GetObjectDictionary()[key] = value;
            }
        }

        public override bool Contains(object key) {
            if (_data == null) return false;

            lock (this) {
                if (key is string strKey) {
                    return _data.ContainsKey(strKey);
                } else {
                    MyDictionary<object, object> dict = TryGetObjectDictionary();
                    if (dict != null) {
                        return dict.ContainsKey(key);
                    }

                    return false;
                }
            }
        }

        public override bool Remove(ref DictionaryStorage storage, object key) {
            return Remove(key);
        }

        public bool Remove(object key) {
            if (_data == null) return false;

            lock (this) {
                if (key is string strKey) {
                    return _data.Remove(strKey);
                } else {
                    MyDictionary<object, object> dict = TryGetObjectDictionary();
                    if (dict != null) {
                        return dict.Remove(key);
                    }

                    return false;
                }
            }
        }

        public override bool TryGetValue(object key, out object value) {
            if (_data != null) {
                lock (this) {
                    if (key is string strKey) {
                        return _data.TryGetValue(strKey, out value);
                    }

                    MyDictionary<object, object> dict = TryGetObjectDictionary();

                    if (dict != null) {
                        return dict.TryGetValue(key, out value);
                    }
                }
            }

            value = null;
            return false;
        }

        public override int Count {
            get {
                if (_data == null) return 0;

                lock (this) {
                    if (_data == null) return 0;

                    int count = _data.Count;
                    MyDictionary<object, object> dict = TryGetObjectDictionary();
                    if (dict != null) {
                        // plus the object keys, minus the object dictionary key
                        count += dict.Count - 1;
                    }
                    return count;
                }
            }
        }

        public override void Clear(ref DictionaryStorage storage) {
            _data = null;
        }

        public override List<MyKeyValuePair<object, object>> GetItems() {
            List<MyKeyValuePair<object, object>> res = new List<MyKeyValuePair<object, object>>();

            if (_data != null) {
                lock (this) {
                    foreach (MyKeyValuePair<string, object> kvp in _data) {
                        if (String.IsNullOrEmpty(kvp.Key)) continue;

                        res.Add(new MyKeyValuePair<object, object>(kvp.Key, kvp.Value));
                    }

                    MyDictionary<object, object> dataDict = TryGetObjectDictionary();
                    if (dataDict != null) {
                        foreach (MyKeyValuePair<object, object> kvp in GetObjectDictionary()) {
                            res.Add(kvp);
                        }
                    }
                }
            }

            return res;
        }

        public override bool HasNonStringAttributes() {
            if (_data != null) {
                lock (this) {
                    if (TryGetObjectDictionary() != null) {
                        return true;
                    }
                }
            }
            
            return false;
        }

        private MyDictionary<object, object> TryGetObjectDictionary() {
            if (_data != null) {
                object dict;
                if (_data.TryGetValue(string.Empty, out dict)) {
                    return (MyDictionary<object, object>)dict;
                }
            }

            return null;
        }

        private MyDictionary<object, object> GetObjectDictionary() {
            lock (this) {
                EnsureData();

                object dict;
                if (_data.TryGetValue(string.Empty, out dict)) {
                    return (MyDictionary<object, object>)dict;
                }

                MyDictionary<object, object> res = new MyDictionary<object, object>();
                _data[string.Empty] = res;

                return res;
            }
        }

        private void EnsureData() {
            if (_data == null) {
                _data = new MyDictionary<string, object>();
            }
        }
    }
}
