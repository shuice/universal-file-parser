using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kernel
{

    /*
             A Value object holds a single value being created in the mapping process

     */
    public class Value
    {
        public string name = "";
        public ByteView byteView = null;
        public ENDIAN_TYPE endianType = ENDIAN_TYPE.ENDIAN_UNKNOWN;
        public NUMBER_DISPLAY_TYPE number_display_type = NUMBER_DISPLAY_TYPE.NUMBER_DISPLAY_DECIMAL;


        private bool create_directly = false;
        private UInt64? uint64_from_directly;
        private Int64? int64_from_directly;
        private double? double_from_directly;
        private string string_from_directly;
        private Int64? _index_of_bits;
        private Int64? _count_of_bits;
        public Encoding stringEncoding;
        public STRING_LENGTH_TYPE stringLengthType = STRING_LENGTH_TYPE.STRING_LENGTH_FIXED;

        public static Value CreateValueDirectly()
        {
            Value r = new Value()
            {
                create_directly = true,
            };
            return r;
        }

        private bool IsDefined()
        {
            return (create_directly == true) || (byteView != null);
        }
            


        public Int64 count_of_bits
        {
            get
            {
                if (IsDefined() == false)
                {
                    return 0;
                }

                if (byteView != null)
                {
                    return byteView.count_of_bits;
                }
                Debug.Assert(create_directly);
                Debug.Assert(_count_of_bits != null);
                return _count_of_bits.GetValueOrDefault(0);
            }

            set
            {
                Debug.Assert(value >= 0);
                Debug.Assert(create_directly);
                _count_of_bits = value;
                Debug.Assert(_count_of_bits != null);
            }
        }

        public Int64 index_of_bits
        {
            get
            {
                if (IsDefined() == false)
                {
                    return 0;
                }

                if (byteView != null)
                {
                    return byteView.index_of_bits;
                }
                Debug.Assert(create_directly);
                Debug.Assert(_index_of_bits != null);
                return _index_of_bits.GetValueOrDefault(0);
            }

            set
            {
                Debug.Assert(create_directly);                
                _index_of_bits = value;
                Debug.Assert(_index_of_bits != null);
            }
        }


        public void SetContent(VALUE_TYPE t, ByteView v)
        {
            byteView = v;
            valueType = t;
        }
        /*
            • VALUE_TYPE_BINARY
            • VALUE_TYPE_BOOLEAN
            • VALUE_TYPE_NUMBER_UNSIGNED
            • VALUE_TYPE_NUMBER_SIGNED
            • VALUE_TYPE_NUMBER_FLOAT
            • VALUE_TYPE_STRING
         */
        public VALUE_TYPE valueType = VALUE_TYPE.VALUE_TYPE_UNDEFINED;
        
        public bool Math(string match_string)
        {
            bool matched = false;
            switch (valueType)
            {
                case VALUE_TYPE.VALUE_TYPE_BINARY:
                    matched = ByteArrayEqual(ToBinanryFromValue(), ToBinanryFromString(match_string));
                    break;

                case VALUE_TYPE.VALUE_TYPE_BOOLEAN:
                    matched = ToBoolFromValue() == ToBoolFromString(match_string);
                    break;

                case VALUE_TYPE.VALUE_TYPE_NUMBER_FLOAT:
                    matched = ToDoubleFromValue() == ToDoubleFromString(match_string);
                    break;

                case VALUE_TYPE.VALUE_TYPE_NUMBER_UNSIGNED:
                    matched = ToUInt64FromValue() == ToUInt64FromString(match_string);
                    break;

                case VALUE_TYPE.VALUE_TYPE_NUMBER_SIGNED:
                    Int64 a = ToInt64FromValue();
                    Int64 b = ToInt64FromString(match_string);
                    matched =  (a == b);
                    break;

                case VALUE_TYPE.VALUE_TYPE_STRING:
                    matched = ToStringFromValue() == ToStringFromString(match_string);
                    break;

                case VALUE_TYPE.VALUE_TYPE_STRUCTURE:
                case VALUE_TYPE.VALUE_TYPE_STRUCTURE_REF:
                    Debug.Assert(false);
                    break;
            }
            return matched;
        }

        public Int64 ToInt64FromValue()
        {
            if (create_directly)
            {
                if (valueType == VALUE_TYPE.VALUE_TYPE_NUMBER_UNSIGNED)
                {
                    return (Int64)uint64_from_directly.GetValueOrDefault(0);
                }
                else if (valueType == VALUE_TYPE.VALUE_TYPE_NUMBER_SIGNED)
                {
                    return int64_from_directly.GetValueOrDefault(0);
                }
                else if (valueType == VALUE_TYPE.VALUE_TYPE_NUMBER_FLOAT)
                {
                    return (Int64)double_from_directly.GetValueOrDefault(0.0);
                }
                else
                {
                    Debug.Assert(valueType == VALUE_TYPE.VALUE_TYPE_STRING);
                    return String2Int64(string_from_directly);
                }
            }
            else
            {
                if (endianType == ENDIAN_TYPE.ENDIAN_DYNAMIC)
                {
                    Debug.Assert(byteView.count_of_bits <= ByteView.BITS_PER_BYTE);
                }

                byte[] bytes = byteView.GetReader().GetEnumerator(endianType == ENDIAN_TYPE.ENDIAN_BIG).Take(8).ToArray();
                if (endianType == ENDIAN_TYPE.ENDIAN_LITTLE)
                {
                    Array.Reverse(bytes);
                }
                Int64 v = 0;
                foreach (byte b in bytes.Take(8))
                {
                    Int64 t = b;
                    v = v * 256 + t;
                }
                return v;
            }
        }

        public Int64 ToInt64FromString(string match_string)
        {
            Int64 v = 0;
            if (Int64.TryParse(match_string, out v) == false)
            {
                try
                {                    
                    v = Convert.ToInt64(match_string, 16);
                }
                catch (Exception)
                {

                }
            }
            return v;
        }


        private UInt64 String2Uint64(string s)
        {
            if (String.IsNullOrEmpty(s))
            {
                return 0;
            }
            s = s.Trim();
            int index = s.ToList().FindIndex((c) => (Char.IsDigit(c) == false && (Char.ToUpper(c) < 'A' || Char.ToUpper(c) > 'F')));
            if (index == 0)
            {
                return 0;
            }
            if (index != -1)
            {
                s = s.Substring(0, index);
            }

            UInt64 r;
            UInt64.TryParse(s, out r);
            return r;
        }

        private Int64 String2Int64(string s)
        {
            if (String.IsNullOrEmpty(s))
            {
                return 0;
            }
            s = s.Trim();
            int index = s.ToList().FindIndex((c) => (Char.IsDigit(c) == false && (Char.ToUpper(c) < 'A' || Char.ToUpper(c) > 'F')));
            if (index == 0)
            {
                return 0;
            }
            if (index != -1)
            {
                s = s.Substring(0, index);
            }

            Int64 r;
            Int64.TryParse(s, out r);
            return r;
        }

        public UInt64 ToUInt64FromValue()
        {
            if (create_directly)
            {
                if (valueType == VALUE_TYPE.VALUE_TYPE_NUMBER_UNSIGNED)
                {
                    return uint64_from_directly.GetValueOrDefault(0);
                }
                else if (valueType == VALUE_TYPE.VALUE_TYPE_NUMBER_SIGNED)
                {
                    return (UInt64)int64_from_directly.GetValueOrDefault(0);
                }
                else if (valueType == VALUE_TYPE.VALUE_TYPE_NUMBER_FLOAT)
                {
                    return (UInt64)double_from_directly.GetValueOrDefault(0.0);
                }
                else
                {
                    Debug.Assert(valueType == VALUE_TYPE.VALUE_TYPE_STRING);                    
                    return String2Uint64(string_from_directly);
                }
                
            }
            else
            {
                if (endianType == ENDIAN_TYPE.ENDIAN_DYNAMIC)
                {
                    Debug.Assert(byteView.count_of_bits <= ByteView.BITS_PER_BYTE);
                }

                byte[] bytes = byteView.GetReader().GetEnumerator(endianType == ENDIAN_TYPE.ENDIAN_BIG).Take(8).ToArray();
                if (endianType == ENDIAN_TYPE.ENDIAN_LITTLE)
                {
                    Array.Reverse(bytes);
                }
                UInt64 v = 0;
                foreach (byte b in bytes.Take(8))
                {
                    UInt64 t = b;
                    v = v * 256 + t;
                }
                return v;
            }
        }

        public string FormatNumberToDisplayType(UInt64 n)
        {
            if (number_display_type == NUMBER_DISPLAY_TYPE.NUMBER_DISPLAY_HEX)
            {
                return "0x" + n.ToString("X");
            }
            else if (number_display_type == NUMBER_DISPLAY_TYPE.NUMBER_DISPLAY_DECIMAL)
            {
                return n.ToString();
            }
            else if (number_display_type == NUMBER_DISPLAY_TYPE.NUMBER_DISPLAY_BINARY)
            {
                return Convert.ToString((Int64)n, 2) + "b";
            }
            return n.ToString();
        }

        public string FormatNumberToDisplayType(Int64 n)
        {
            if (number_display_type == NUMBER_DISPLAY_TYPE.NUMBER_DISPLAY_HEX)
            {
                return "0x" + n.ToString("X");
            }
            else if (number_display_type == NUMBER_DISPLAY_TYPE.NUMBER_DISPLAY_DECIMAL)
            {
                return n.ToString();
            }
            else if (number_display_type == NUMBER_DISPLAY_TYPE.NUMBER_DISPLAY_BINARY)
            {
                return Convert.ToString((Int64)n, 2) + "b";
            }
            return n.ToString();
        }

        public UInt64 ToUInt64FromString(string match_string)
        {
            UInt64 v = 0;
            if (UInt64.TryParse(match_string, out v) == false)
            {
                try
                {
                    v = Convert.ToUInt64(match_string, 16);
                }
                catch (Exception)
                {

                }
            }
            return v;
        }

        public double ToDoubleFromValue()
        {
            if (create_directly)
            {
                if (valueType == VALUE_TYPE.VALUE_TYPE_NUMBER_UNSIGNED)
                {
                    return uint64_from_directly.GetValueOrDefault(0);
                }
                else if (valueType == VALUE_TYPE.VALUE_TYPE_NUMBER_SIGNED)
                {
                    return (UInt64)int64_from_directly.GetValueOrDefault(0);
                }
                else if (valueType == VALUE_TYPE.VALUE_TYPE_NUMBER_FLOAT)
                {                    
                    return double_from_directly.GetValueOrDefault(0.0);
                }
                else
                {
                    double double_value;
                    double.TryParse(string_from_directly, out double_value);
                    return double_value;
                }

            }
            else
            {
                if (endianType == ENDIAN_TYPE.ENDIAN_DYNAMIC)
                {
                    Debug.Assert(byteView.count_of_bits <= ByteView.BITS_PER_BYTE);
                }

                byte[] bytes = byteView.GetReader().GetEnumerator(endianType == ENDIAN_TYPE.ENDIAN_BIG).Take(8).ToArray();
                if (endianType == ENDIAN_TYPE.ENDIAN_LITTLE)
                {
                    Array.Reverse(bytes);
                }
                double v = BitConverter.ToDouble(bytes, 0);
                return v;
            }
        }

        public double ToDoubleFromString(string match_string)
        {
            throw new NotImplementedException();
        }

        public bool ToBoolFromValue()
        {
            throw new NotImplementedException();
        }

        public bool ToBoolFromString(string match_string)
        {
            throw new NotImplementedException();
        }

        public string ToStringFromValue()
        {
            if (create_directly)
            {
                if (valueType == VALUE_TYPE.VALUE_TYPE_NUMBER_UNSIGNED)
                {
                    return uint64_from_directly.GetValueOrDefault(0).ToString();
                }
                else if (valueType == VALUE_TYPE.VALUE_TYPE_NUMBER_SIGNED)
                {
                    return int64_from_directly.GetValueOrDefault(0).ToString();
                }
                else if (valueType == VALUE_TYPE.VALUE_TYPE_NUMBER_FLOAT)
                {
                    return double_from_directly.GetValueOrDefault(0).ToString();
                }
                else
                {
                    return string_from_directly ?? "";
                }

            }
            else
            {
                bool skip_first_byte = (stringLengthType == STRING_LENGTH_TYPE.STRING_LENGTH_PASCAL);
                byte[] bytes = byteView.GetReader().GetEnumerator(true).Skip(skip_first_byte ? 1 : 0).ToArray();
                string s = (stringEncoding ?? Encoding.UTF8).GetString(bytes);
                s = s.Trim('\0');
                return s;
            }
        }

        public string ToStringFromString(string match_string)
        {
            return match_string;
        }

        public byte[] ToBinanryFromValue()
        {
            byte[] r =  byteView.GetReader().GetEnumerator(false).Take(100).ToArray();
            return r;
        }

        public byte[] ToBinanryFromString(string match_string)
        {
            byte[] buff = new byte[(match_string.Length + 1)/2];
            for (int i = 0; i < buff.Length; i++)
            {
                buff[i] = Convert.ToByte(match_string.Substring(i*2, 2), 16);
            }
            return buff;
        }

        private bool ByteArrayEqual(byte[] b1, byte[] b2)
        {
            if (b1.Length != b2.Length)
            {
                return false;
            }
            if (b1 == null || b2 == null)
            {
                return false;
            }
            for (int i = 0; i < b1.Length; i++)
            {
                if (b1[i] != b2[i])
                {
                    return false;
                }
            }
            return true;
        }

        public string Value_UI(string match)
        {
            string value_ui = "";
            switch (valueType)
            {
                case VALUE_TYPE.VALUE_TYPE_BINARY:
                    int sample_byte_count = 10;
                    string suffix = byteView.count_of_bytes > sample_byte_count ? " ..." : "";
                    value_ui = $"{String.Join(" ", byteView.GetReader().GetEnumerator(false).Take(sample_byte_count).Select(i => i.ToString("X2")))}{suffix}";
                    break;

                case VALUE_TYPE.VALUE_TYPE_BOOLEAN:
                    value_ui = $"{ToBinanryFromValue()}";
                    break;

                case VALUE_TYPE.VALUE_TYPE_NUMBER_FLOAT:
                    value_ui = $"{ToDoubleFromValue()}";
                    break;

                case VALUE_TYPE.VALUE_TYPE_NUMBER_UNSIGNED:
                    value_ui = $"{FormatNumberToDisplayType(ToUInt64FromValue())}";
                    break;

                case VALUE_TYPE.VALUE_TYPE_NUMBER_SIGNED:
                    value_ui = $"{FormatNumberToDisplayType(ToInt64FromValue())}";
                    break;

                case VALUE_TYPE.VALUE_TYPE_STRING:
                    value_ui = $"{ToStringFromValue()}";
                    break;

                case VALUE_TYPE.VALUE_TYPE_STRUCTURE:
                    break;

                case VALUE_TYPE.VALUE_TYPE_STRUCTURE_REF:
                    break;
            }
            if (match.Length > 0)
            {
                match += ": ";
            }
            return match + value_ui;
        }

        public string description(string match)
        {
            string value_ui = Value_UI(match);
            string bits_mod = (index_of_bits % 8) == 0 ? "" : $".{(index_of_bits % 8)}";            
            string range = bits_range();
            return $"{value_type_desc()}  {value_ui}";
        }

        private string value_type_desc()
        {
            string desc = "";
            switch (valueType)
            {
                case VALUE_TYPE.VALUE_TYPE_BINARY:
                    desc = "bin";
                    break;

                case VALUE_TYPE.VALUE_TYPE_BOOLEAN:
                    desc = "bool";
                    break;

                case VALUE_TYPE.VALUE_TYPE_NUMBER_FLOAT:
                    desc = "double";
                    break;

                case VALUE_TYPE.VALUE_TYPE_NUMBER_UNSIGNED:
                    desc = "num";
                    break;

                case VALUE_TYPE.VALUE_TYPE_NUMBER_SIGNED:
                    desc = "num";
                    break;

                case VALUE_TYPE.VALUE_TYPE_STRING:
                    desc = "str";
                    break;

                case VALUE_TYPE.VALUE_TYPE_STRUCTURE:
                    desc = "structure";
                    break;

                case VALUE_TYPE.VALUE_TYPE_STRUCTURE_REF:
                    desc = "structure ref";
                    break;
                default:
                    desc = valueType.ToString();
                    break;
            }
            return desc;
        }

        public Int64 getSignedNumber()
        {
            return ToInt64FromValue();
        }

        public Int64 getSigned()
        {
            return ToInt64FromValue();
        }


        public UInt64 getUnsigned()
        {
            return ToUInt64FromValue();
        }

        public UInt64 getUnsignedNumber()
        {
            return ToUInt64FromValue();
        }

        public void setUnsigned(UInt64 v)
        {
            valueType = VALUE_TYPE.VALUE_TYPE_NUMBER_UNSIGNED;
            uint64_from_directly = v;
        }

        public void setSigned(Int64 v)
        {
            valueType = VALUE_TYPE.VALUE_TYPE_NUMBER_SIGNED;
            int64_from_directly = v;
        }
        public void setFloat(double v)
        {
            valueType = VALUE_TYPE.VALUE_TYPE_NUMBER_FLOAT;
            double_from_directly = v;
        }

        public double getFloat()
        {
            return ToDoubleFromValue();
        }

        public void setString(string s)
        {
            valueType = VALUE_TYPE.VALUE_TYPE_STRING;
            string_from_directly = s;
        }

        public string getString()
        {
            return ToStringFromValue();
        }

        public string getName()
        {
            return name;
        }

        public void setName(string name)
        {
            this.name = name;
        }

        public string getType()
        {
            return valueType.ToString();
        }

        public string bits_range()
        {
            return $"{ByteView.format_bit_index(index_of_bits)} - {ByteView.format_bit_index(count_of_bits)}";
        }
    }
}
