using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace kernel
{
    public class ElementString : ElementBase
    {
        
        public override ELEMENT_TYPE GetElementType()
        {
            return ELEMENT_TYPE.ELEMENT_STRING;
        }

        
        private int zero_end_length(Encoding encoding)
        {
            if (encoding.CodePage == 1201 || encoding.CodePage == 1200)
            {
                return 2;
            }
            if (encoding.CodePage == 12000 || encoding.CodePage == 12001)
            {
                return 4;
            }
            return 1;
        }

        public byte[] ConvertDelimiterToBytes(string delimiter)
        {
            if (delimiter.Length % 2 != 0)
            {
                delimiter = "0" + delimiter;
            }

            List<byte> r = new List<byte>();
            for (int used_count = 0; used_count < delimiter.Length; used_count += 2)
            {
                r.Add((byte)delimiter.Substring(used_count, 2).ToInt64(ToolsExtention.EnumStringFormatType.hexWithout0xPrefix).GetValueOrDefault(0));
            }
            return r.ToArray();
        }

        public int FindByteArray(byte[] from, byte[] sub)
        {
            int find_count = from.Length - sub.Length + 1;
            int sub_length = sub.Length;
            for (int index = 0; index < find_count; index ++)
            {
                for (int sub_index = 0; sub_index < sub_length; sub_index ++)
                {
                    if (from[index + sub_index] != sub[sub_index])
                    {
                        break;
                    }
                    
                    if (sub_index == sub_length - 1)
                    {
                        return index;
                    }
                }
            }
            return -1;

        }

        public override MapResult mapByteViewOnce(ByteView byteView, Result result, MapContext mapContext, string showName)
        {            
            long used_len_bits = 0;
            long fixed_length_bits = this.lengthInBits(byteView.count_of_bits);

            Encoding encoding = StringEncodingWrapper.GetEncodingByName(this.GetValueDerivedWithDefault(ElementKey.encoding), 
                mapContext,
                () => $"Can not find string encoding({name}) for element({this.name}), use UTF-8 instead. Path: {result.GetErrorPath()} "
                );

            if (stringLengthType == STRING_LENGTH_TYPE.STRING_LENGTH_FIXED)
            {
                used_len_bits = Math.Min(fixed_length_bits, byteView.count_of_bits);
            }
            else if (stringLengthType == STRING_LENGTH_TYPE.STRING_LENGTH_PASCAL)
            {
                if (fixed_length_bits > 0)
                {
                    used_len_bits = Math.Min(fixed_length_bits, byteView.count_of_bits);
                }
                else
                {
                    IEnumerable<byte> enumerableBytes = byteView.GetReader().GetEnumerator(false).Take(1);
                    if (enumerableBytes.Count() == 0)
                    {
                        string errorMsg = $"Failed to get length of pascal string({this.name}), path: {result.GetErrorPath()}";
                        mapContext.log.addLog(EnumLogLevel.error, errorMsg);
                        return MapResult.CreateWithError(MapError.gramma_error, errorMsg);
                    }
                    else
                    {
                        byte prefix_length = enumerableBytes.FirstOrDefault();
                        long toal_can_be_used_bytes = ByteView.ConvertBitsCount2BytesCount(byteView.count_of_bits);
                        if (prefix_length + 1 > toal_can_be_used_bytes)
                        {
                            string errorMsg = $"The pascal string needs a length of {prefix_length}, and the actual length is {toal_can_be_used_bytes}. Current parsing element({this.name}), path: {result.GetErrorPath()}";
                            mapContext.log.addLog(EnumLogLevel.error, errorMsg);
                            return MapResult.CreateWithError(MapError.gramma_error, errorMsg);
                        }
                        used_len_bits = ByteView.ConvertBytesCount2BitsCount(prefix_length + 1);
                    }
                }
                
            }
            else if (stringLengthType == STRING_LENGTH_TYPE.STRING_LENGTH_DELIMITER_TERMINATED)
            {
                string delimiter = this.GetValueDerivedWithDefault(ElementKey.delimiter);
                byte[] bytesDelimiter = ConvertDelimiterToBytes(delimiter);
                if (bytesDelimiter.Length == 0)
                {
                    string errorMsg = $"The delimiter({delimiter}) of element({this.name}) is invalided, path: {result.GetErrorPath()}";
                    mapContext.log.addLog(EnumLogLevel.error, errorMsg);
                    return MapResult.CreateWithError(MapError.gramma_error, errorMsg);
                }

                ByteViewReader reader = byteView.GetReader();
                reader.AssumeBytesAligned(1);
                byte[] to_be_finded = reader.GetEnumerator(false).ToArray();
                int index = FindByteArray(to_be_finded, bytesDelimiter);
                if (index != -1)
                {
                    used_len_bits = ByteView.ConvertBytesCount2BitsCount(index + bytesDelimiter.Length);
                }
                else
                {
                    used_len_bits = byteView.count_of_bits;
                }
            }
            else if (stringLengthType == STRING_LENGTH_TYPE.STRING_LENGTH_ZERO_TERMINATED)
            {
                ByteViewReader reader = byteView.GetReader();                                    
                reader.AssumeBytesAligned(1);
                int i_zero_end_length = zero_end_length(encoding);                
                byte[] to_be_finded = reader.GetEnumerator(false).ToArray();
                int index = FindByteArray(to_be_finded, new byte[] { 0x00, 0x00, 0x00, 0x00 }.Take(i_zero_end_length).ToArray());
                if (index != -1)
                {
                    used_len_bits = ByteView.ConvertBytesCount2BitsCount(index + i_zero_end_length);
                }
                else
                {
                    used_len_bits = byteView.count_of_bits;
                }
            }

            result.value.SetContent(VALUE_TYPE.VALUE_TYPE_STRING, byteView.TakeBits(used_len_bits, () => ($"parsing string element({this.name}), path: {result.GetErrorPath()}", true)));
            result.value.stringEncoding = encoding;
            result.value.stringLengthType = stringLengthType;
            return MapResult.CreateWithLength(used_len_bits);            
        }
    }
}
