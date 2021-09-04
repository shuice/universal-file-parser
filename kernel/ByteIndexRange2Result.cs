using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Diagnostics;

namespace kernel
{
    public class ByteIndexRange
    {
        public Int64 start_index_bits;
        public Int64 end_index_bits;
        public Result result;
    }

    public class ByteIndexRangeCompareA : IComparer<ByteIndexRange>
    {
        public static ByteIndexRangeCompareA instance = new ByteIndexRangeCompareA();
        public static bool IsByteIndexRangeMatch(ByteIndexRange to_be_matched, ByteIndexRange inList)
        {
            if ((inList.start_index_bits >= to_be_matched.start_index_bits) && (inList.start_index_bits <= to_be_matched.end_index_bits))
            {
                return true;
            }
            return false;
        }

        public int Compare(ByteIndexRange x, ByteIndexRange y)
        {
            bool to_be_find_is_x = (x.result == null);
            if (to_be_find_is_x)
            {
                if (IsByteIndexRangeMatch(x, y))
                {
                    return 0;
                }
            }
            else
            {
                if (IsByteIndexRangeMatch(y, x))
                {
                    return 0;
                }
            }
            return x.start_index_bits.CompareTo(y.start_index_bits);
        }
    }

    public class ByteIndexRangeCompareB : IComparer<ByteIndexRange>
    {
        public static ByteIndexRangeCompareB instance = new ByteIndexRangeCompareB();
        public static bool IsByteIndexRangeMatch(ByteIndexRange to_be_matched, ByteIndexRange inList)
        {
            if ((inList.start_index_bits < to_be_matched.start_index_bits) && (inList.end_index_bits >= to_be_matched.start_index_bits))
            {
                return true;
            }
            return false;
        }

        public int Compare(ByteIndexRange x, ByteIndexRange y)
        {
            bool to_be_find_is_x = (x.result == null);
            if (to_be_find_is_x)
            {
                if (IsByteIndexRangeMatch(x, y))
                {
                    return 0;
                }
            }
            else
            {
                if (IsByteIndexRangeMatch(y, x))
                {
                    return 0;
                }
            }
            return x.start_index_bits.CompareTo(y.start_index_bits);
        }
    }

    public class ByteIndexRange2Result
    {
        private List<ByteIndexRange> byteIndexRanges;
        private int lastFindedIndex = int.MinValue;
        private static List<ByteIndexRange> emptyList = new List<ByteIndexRange>();
        public ByteIndexRange2Result(IEnumerable<Result> sorted_result)
        {
            byteIndexRanges = new List<ByteIndexRange>(sorted_result.Count());
            foreach (Result r in sorted_result)
            {
                ByteIndexRange next = new ByteIndexRange()
                {
                    start_index_bits = r.value.index_of_bits,
                    end_index_bits = r.value.index_of_bits + r.value.count_of_bits - 1,
                    result = r,                    
                };

                ByteIndexRange last = byteIndexRanges.LastOrDefault();
                if (last == null)                
                {
                    byteIndexRanges.Add(next);
                    continue;                
                }               

                if (next.start_index_bits <= last.end_index_bits)
                {                    
                    if (next.end_index_bits > last.end_index_bits)
                    {
                        next.start_index_bits = last.end_index_bits + 1;
                        byteIndexRanges.Add(next);
                        Debug.Assert(false, "?");
                    }
                    else if (next.end_index_bits == last.end_index_bits)
                    {
                        // same, abandon
                        Debug.Assert(false, "?");
                    }                    
                } 
                else
                {
                    byteIndexRanges.Add(next);
                }
            }
        }

        

        public int FindOneRelatedByByteIndex(Int64 byteIndex)
        {
            ByteIndexRange tmp = new ByteIndexRange()
            {
                start_index_bits = byteIndex * ByteView.BITS_PER_BYTE,
                end_index_bits = byteIndex * ByteView.BITS_PER_BYTE + ByteView.BITS_PER_BYTE - 1,
                result = null,
            };

            // cache & nearby
            if (lastFindedIndex > 0)
            {
                if (ByteIndexRangeCompareA.IsByteIndexRangeMatch(tmp, byteIndexRanges[lastFindedIndex])
                    || ByteIndexRangeCompareB.IsByteIndexRangeMatch(tmp, byteIndexRanges[lastFindedIndex]))
                {
                    return lastFindedIndex;
                }
                // skip to next byte
                Int64 skiped_bits_count = 0;
                int lastFindedIndexTmp = lastFindedIndex;
                while (skiped_bits_count <= 8)
                {
                    int nextLastFindIndex = lastFindedIndexTmp + 1;                    
                    if (nextLastFindIndex < byteIndexRanges.Count)
                    {
                        skiped_bits_count += byteIndexRanges[lastFindedIndexTmp].result.value.count_of_bits;
                        if (ByteIndexRangeCompareA.IsByteIndexRangeMatch(tmp, byteIndexRanges[nextLastFindIndex])
                            || ByteIndexRangeCompareB.IsByteIndexRangeMatch(tmp, byteIndexRanges[nextLastFindIndex]))
                        {
                            lastFindedIndex = nextLastFindIndex;
                            return lastFindedIndex;
                        }
                        else
                        {
                            lastFindedIndexTmp = nextLastFindIndex;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }

            // fully search
            int findedA = byteIndexRanges.BinarySearch(tmp, ByteIndexRangeCompareA.instance);
            if (findedA >= 0)
            {
                lastFindedIndex = findedA;
                return findedA;
            }

            int findedB = byteIndexRanges.BinarySearch(tmp, ByteIndexRangeCompareB.instance);
            if (findedB >= 0)
            {
                lastFindedIndex = findedB;
                return findedB;
            }
                       
            return -1;
        }

        public List<ByteIndexRange> FindRelatedByByteIndex(Int64 byteIndex)
        {
            int index = FindOneRelatedByByteIndex(byteIndex);
            if (index < 0)
            {
                return emptyList;
            }
            ByteIndexRange byteIndexRange = byteIndexRanges[index];

            List<ByteIndexRange> results = new List<ByteIndexRange>();
            results.Add(byteIndexRange);

            if (byteIndexRange.start_index_bits > byteIndex * ByteView.BITS_PER_BYTE)
            {
                Int64 byte_start_bits = byteIndex * ByteView.BITS_PER_BYTE;
                foreach (int prevIndex in Enumerable.Range(0, index).Reverse())
                {
                    ByteIndexRange prevByteIndexRange = byteIndexRanges[prevIndex];
                    if (prevByteIndexRange.end_index_bits >= byte_start_bits)
                    {
                        results.Insert(0, prevByteIndexRange);
                    }
                    else
                    {
                        break;   
                    }
                }
            }

            Int64 byte_end_bits = (byteIndex + 1) * ByteView.BITS_PER_BYTE - 1;
            if (byteIndexRange.end_index_bits < byte_end_bits)
            {                
                foreach (ByteIndexRange nextByteIndexRange in byteIndexRanges.Skip(index + 1))
                {
                    if (nextByteIndexRange.start_index_bits <= byte_end_bits)
                    {
                        results.Add(nextByteIndexRange);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            return results;
        }
    }
}
