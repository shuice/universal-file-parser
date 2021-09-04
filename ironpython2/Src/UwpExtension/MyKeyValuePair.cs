using System;
using System.Collections.Generic;
using System.Text;

namespace System.Collections.Generic
{
    public class MyKeyValuePair<TKey, TValue>
    {
        public TKey Key { get; set; }

        public TValue Value { get; set; }

        public MyKeyValuePair()
        {

        }
        public MyKeyValuePair(TKey key, TValue value)
        {
            this.Key = key;
            this.Value = value;
        }

        public override string ToString()
        {
            return $"{Key} - {Value}";
        }
    }
}