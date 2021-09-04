using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace System.Collections.Generic
{
    public class SimpleDictionaryEnumerator<TKey, TValue> : IDictionaryEnumerator
    {
        // A copy of the SimpleDictionary object's key/value pairs.
        DictionaryEntry[] items;
        Int32 index = -1;

        public SimpleDictionaryEnumerator(List<KeyValuePair<TKey, TValue>> keyValuePairs)
        {
            // Make a copy of the dictionary entries currently in the SimpleDictionary object.
            items = new DictionaryEntry[keyValuePairs.Count];
            int copyIndex = 0;
            foreach(var pair in keyValuePairs)
            {
                items[copyIndex] = new DictionaryEntry() { Key = pair.Key, Value = pair.Value, };
                copyIndex++;
            }            
        }

        // Return the current item.
        public Object Current { get { ValidateIndex(); return items[index]; } }

        // Return the current dictionary entry.
        public DictionaryEntry Entry
        {
            get { return (DictionaryEntry)Current; }
        }

        // Return the key of the current item.
        public Object Key { get { ValidateIndex(); return items[index].Key; } }

        // Return the value of the current item.
        public Object Value { get { ValidateIndex(); return items[index].Value; } }

        // Advance to the next item.
        public Boolean MoveNext()
        {
            if (index < items.Length - 1)
            { index++; return true; }
            return false;
        }

        // Validate the enumeration index and throw an exception if the index is out of range.
        private void ValidateIndex()
        {
            if (index < 0 || index >= items.Length)
                throw new InvalidOperationException("Enumerator is before or after the collection.");
        }

        // Reset the index to restart the enumeration.
        public void Reset()
        {
            index = -1;
        }
    }
}
