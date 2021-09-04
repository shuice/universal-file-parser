using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kernel
{
    public static class ToolsExtention
    {
        [System.Flags]
        public enum EnumStringFormatType
        {
            Decimal = 1,
            hexWith0xPrefix = 2,
            hexWithout0xPrefix = 4,            
            hex = (hexWith0xPrefix | hexWithout0xPrefix),
            all = (Decimal | hexWith0xPrefix | hexWithout0xPrefix),
        }
        public static Int64? ToInt64(this string s, EnumStringFormatType type  = EnumStringFormatType.all)
        {
            Int64 r = 0;

            if (type.HasFlag(EnumStringFormatType.Decimal))
            {
                if (Int64.TryParse(s, out r))
                {
                    return r;
                }
            }

            if (type.HasFlag(EnumStringFormatType.hexWithout0xPrefix))
            {
                if (Int64.TryParse(s, NumberStyles.HexNumber, null, out r))
                {
                    return r;
                }
            }

            if (type.HasFlag(EnumStringFormatType.hexWith0xPrefix))
            {
                if (s.ToLower().StartsWith("0x"))
                {
                    if (Int64.TryParse(s.Substring(2), NumberStyles.HexNumber, null, out r))
                    {
                        return r;
                    }
                }
            }
            return null;
        }

        public static UInt64? ToUInt64(this string s, EnumStringFormatType type = EnumStringFormatType.all)
        {
            UInt64 r = 0;

            if (type.HasFlag(EnumStringFormatType.Decimal))
            {                
                if (UInt64.TryParse(s, out r))
                {
                    return r;
                }
            }

            if (type.HasFlag(EnumStringFormatType.hexWithout0xPrefix))
            {
                if (UInt64.TryParse(s, NumberStyles.HexNumber, null, out r))
                {
                    return r;
                }
            }

            if (type.HasFlag(EnumStringFormatType.hexWith0xPrefix))
            {
                if (s.ToLower().StartsWith("0x"))
                {
                    if (UInt64.TryParse(s.Substring(2), NumberStyles.HexNumber, null, out r))
                    {
                        return r;
                    }
                }
            }
            return null;
        }
    }
}
