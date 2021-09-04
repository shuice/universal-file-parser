// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using AnyPrefix.Microsoft.Scripting;
using AnyPrefix.Microsoft.Scripting.Runtime;
using System.Threading;

namespace IronPython.Runtime {
    /// <summary>
    /// Adapts an IDictionary[object, object] for use as a PythonDictionary used for
    /// our debug frames.  Also hides the special locals which start with $.
    /// </summary>
    [Serializable]
    internal class DebuggerDictionaryStorage : DictionaryStorage {
        private IMyDictionary<object, object> _data;
        private readonly CommonDictionaryStorage _hidden;

        public DebuggerDictionaryStorage(IMyDictionary<object, object> data) {
            Debug.Assert(data != null);

            _hidden = new CommonDictionaryStorage();
            foreach (var key in data.Keys) {
                if (key is string strKey && strKey.Length > 0 && strKey[0] == '$') {
                    _hidden.Add(strKey, null);
                }
            }

            _data = data;
        }

        public override void Add(ref DictionaryStorage storage, object key, object value) {
            AddNoLock(ref storage, key, value);
        }

        public override void AddNoLock(ref DictionaryStorage storage, object key, object value) {
            _hidden.Remove(key);

            _data[key] = value;
        }

        public override bool Contains(object key) {
            if (_hidden.Contains(key)) {
                return false;
            }

            return _data.ContainsKey(key);
        }

        public override bool Remove(ref DictionaryStorage storage, object key) {
            if (_hidden.Contains(key)) {
                return false;
            }

            return _data.Remove(key);
        }

        public override bool TryGetValue(object key, out object value) {
            if (_hidden.Contains(key)) {
                value = null;
                return false;
            }

            return _data.TryGetValue(key, out value);
        }

        public override int Count {
            get {
                return _data.Count - _hidden.Count;
            }
        }

        public override void Clear(ref DictionaryStorage storage) {
            _data = new MyDictionary<object, object>();
            _hidden.Clear();
        }

        public override List<MyKeyValuePair<object, object>> GetItems() {
            List<MyKeyValuePair<object, object>> res = new List<MyKeyValuePair<object, object>>(Count);
            foreach (var kvp in _data) {
                if (!_hidden.Contains(kvp.Key)) {
                    res.Add(kvp);
                }
            }
            return res;
        }

        public override bool HasNonStringAttributes() {
            return true;
        }
    }
}
