using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace System.Linq
{
    public static class EnumerableExtension
    {
        public static MyDictionary<TKey, TElement> ToMyDictionary<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
        {
            MyDictionary<TKey, TElement> dic = new MyDictionary<TKey, TElement>();
            foreach(TSource item in source)
            {
                TKey key = keySelector(item);
                TElement value = elementSelector(item);
                dic[key] = value;
            }
            return dic;
        }

        
        public static MyDictionary<TKey, TSource> ToMyDictionary<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            MyDictionary<TKey, TSource> dic = new MyDictionary<TKey, TSource>();
            foreach (TSource item in source)
            {
                TKey key = keySelector(item);                
                dic[key] = item;
            }
            return dic;
        }
    }
}
