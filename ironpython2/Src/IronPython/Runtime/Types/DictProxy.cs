// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.


using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using IronPython.Runtime.Operations;
using AnyPrefix.Microsoft.Scripting.Runtime;
using AnyPrefix.Microsoft.Scripting.Utils;

namespace IronPython.Runtime.Types {
    [PythonType("dictproxy")]
    public class DictProxy : IDictionary, IEnumerable, IMyDictionary<object, object> {
        private readonly PythonType/*!*/ _dt;
        
        public DictProxy(PythonType/*!*/ dt) {
            Debug.Assert(dt != null);
            _dt = dt;
        }

        #region Python Public API Surface

        public int __len__(CodeContext context) {
            return _dt.GetMemberDictionary(context, false).Count;
        }

        public bool __contains__(CodeContext/*!*/ context, object value) {
            return has_key(context, value);
        }

        public string/*!*/ __str__(CodeContext/*!*/ context) {
            return DictionaryOps.__repr__(context, this);
        }

        public bool has_key(CodeContext/*!*/ context, object key) {
            object dummy;
            return TryGetValue(context, key, out dummy);
        }

        public object get(CodeContext/*!*/ context, [NotNull]object k, [DefaultParameterValue(null)]object d) {
            object res;
            if (!TryGetValue(context, k, out res)) {
                res = d;
            }

            return res;
        }

        public object keys(CodeContext context) {
            return _dt.GetMemberDictionary(context, false).keys();
        }

        public object values(CodeContext context) {
            return _dt.GetMemberDictionary(context, false).values();
        }

        public List items(CodeContext context) {
            return _dt.GetMemberDictionary(context, false).items();
        }

        public PythonDictionary copy(CodeContext/*!*/ context) {
            return new PythonDictionary(context, this);
        }

        public IEnumerator iteritems(CodeContext/*!*/ context) {
            return new DictionaryItemEnumerator(_dt.GetMemberDictionary(context, false)._storage);
        }

        public IEnumerator iterkeys(CodeContext/*!*/ context) {
            return new DictionaryKeyEnumerator(_dt.GetMemberDictionary(context, false)._storage);
        }

        public IEnumerator itervalues(CodeContext/*!*/ context) {
            return new DictionaryValueEnumerator(_dt.GetMemberDictionary(context, false)._storage);
        }

        #endregion

        #region Object overrides

        public override bool Equals(object obj) {
            if (!(obj is DictProxy proxy)) return false;

            return proxy._dt == _dt;
        }

        public override int GetHashCode() {
            return ~_dt.GetHashCode();
        }

        #endregion

        #region IDictionary Members
      
        public object this[object key] {
            get {
                return GetIndex(DefaultContext.Default, key);
            }
            [PythonHidden]
            set {
                throw PythonOps.TypeError("cannot assign to dictproxy");
            }
        }

        bool IDictionary.Contains(object key) {
            return has_key(DefaultContext.Default, key);
        }

        #endregion              

        #region IEnumerable Members

        System.Collections.IEnumerator IEnumerable.GetEnumerator() {
            return DictionaryOps.iterkeys(_dt.GetMemberDictionary(DefaultContext.Default, false));
        }

        #endregion

        #region IDictionary Members

        [PythonHidden]
        public void Add(object key, object value) {
            this[key] = value;
        }

        [PythonHidden]
        public void Clear() {
            throw new InvalidOperationException("dictproxy is read-only");
        }

        IDictionaryEnumerator IDictionary.GetEnumerator() {
            return new PythonDictionary.DictEnumerator(_dt.GetMemberDictionary(DefaultContext.Default, false).GetEnumerator());
        }

        bool IDictionary.IsFixedSize {
            get { return true; }
        }

        bool IDictionary.IsReadOnly {
            get { return true; }
        }

        ICollection IDictionary.Keys {
            get {
                ICollection<object> res = _dt.GetMemberDictionary(DefaultContext.Default, false).Keys;
                if (res is ICollection coll) {
                    return coll;
                }

                return new List<object>(res);
            }
        }

        void IDictionary.Remove(object key) {
            throw new InvalidOperationException("dictproxy is read-only");
        }

        ICollection IDictionary.Values {
            get {
                List<object> res = new List<object>();
                foreach (MyKeyValuePair<object, object> kvp in _dt.GetMemberDictionary(DefaultContext.Default, false)) {
                    res.Add(kvp.Value);
                }
                return res;
            }
        }

        #endregion

        #region ICollection Members

        void ICollection.CopyTo(Array array, int index) {
            foreach (DictionaryEntry de in (IDictionary)this) {
                array.SetValue(de, index++);
            }
        }

        int ICollection.Count {
            get { return __len__(DefaultContext.Default); }
        }

        bool ICollection.IsSynchronized {
            get { return false; }
        }

        object ICollection.SyncRoot {
            get { return this; }
        }

        #endregion

        #region IMyDictionary<object,object> Members

        bool IMyDictionary<object, object>.ContainsKey(object key) {
            return has_key(DefaultContext.Default, key);
        }

        ICollection<object> IMyDictionary<object, object>.Keys {
            get {
                return _dt.GetMemberDictionary(DefaultContext.Default, false).Keys;
            }
        }

        bool IMyDictionary<object, object>.Remove(object key) {
            throw new InvalidOperationException("dictproxy is read-only");
        }

        bool IMyDictionary<object, object>.TryGetValue(object key, out object value) {
            return TryGetValue(DefaultContext.Default, key, out value);
        }

        ICollection<object> IMyDictionary<object, object>.Values {
            get {
                return _dt.GetMemberDictionary(DefaultContext.Default, false).Values;
            }
        }

        #endregion

        #region ICollection<MyKeyValuePair<object,object>> Members

        void ICollection<MyKeyValuePair<object, object>>.Add(MyKeyValuePair<object, object> item) {
            this[item.Key] = item.Value;
        }

        bool ICollection<MyKeyValuePair<object, object>>.Contains(MyKeyValuePair<object, object> item) {
            return has_key(DefaultContext.Default, item.Key);
        }

        void ICollection<MyKeyValuePair<object, object>>.CopyTo(MyKeyValuePair<object, object>[] array, int arrayIndex) {
            foreach (MyKeyValuePair<object, object> de in (IEnumerable<MyKeyValuePair<object, object>>)this) {
                array.SetValue(de, arrayIndex++);
            }
        }

        int ICollection<MyKeyValuePair<object, object>>.Count {
            get { return __len__(DefaultContext.Default); }
        }

        bool ICollection<MyKeyValuePair<object, object>>.IsReadOnly {
            get { return true; }
        }

        bool ICollection<MyKeyValuePair<object, object>>.Remove(MyKeyValuePair<object, object> item) {
            return ((IMyDictionary<object, object>)this).Remove(item.Key);
        }

        #endregion

        #region IEnumerable<MyKeyValuePair<object,object>> Members

        IEnumerator<MyKeyValuePair<object, object>> IEnumerable<MyKeyValuePair<object, object>>.GetEnumerator() {
            return _dt.GetMemberDictionary(DefaultContext.Default, false).GetEnumerator();
        }

        #endregion

        #region Internal implementation details

        private object GetIndex(CodeContext context, object index) {
            if (index is string strIndex) {
                PythonTypeSlot dts;
                if (_dt.TryLookupSlot(context, strIndex, out dts)) {
                    if (dts is PythonTypeUserDescriptorSlot uds) {
                        return uds.Value;
                    }

                    return dts;
                }
            }

            throw PythonOps.KeyError(index.ToString());
        }

        private bool TryGetValue(CodeContext/*!*/ context, object key, out object value) {
            if (key is string strIndex) {
                PythonTypeSlot dts;
                if (_dt.TryLookupSlot(context, strIndex, out dts)) {
                    if (dts is PythonTypeUserDescriptorSlot uds) {
                        value = uds.Value;
                        return true;
                    }

                    value = dts;
                    return true;
                }
            }

            value = null;
            return false;
        }
        
        internal PythonType Type {
            get {
                return _dt;
            }
        }
        
        #endregion
    }
}
