using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kernel
{
    public class String2Enum
    {

        public static NUMBER_TYPE translateStringToNumberType(string strNumberType)
        {
            if (strNumberType == "integer")
            {
                return NUMBER_TYPE.NUMBER_INTEGER;
            }
            else if (strNumberType == "float")
            {
                return NUMBER_TYPE.NUMBER_FLOAT;
            }
            throw new NotSupportedException();
        }


        public static ENDIAN_TYPE translateStringToEndianType(string strEndianType)
        {
            if (strEndianType == "" || strEndianType == "big" || strEndianType == "ENDIAN_BIG")
            {
                return ENDIAN_TYPE.ENDIAN_BIG;
            }
            else if (strEndianType == "little" || strEndianType == "ENDIAN_LITTLE")
            {
                return ENDIAN_TYPE.ENDIAN_LITTLE;
            }
            else if (strEndianType == "dynamic")
            {
                return ENDIAN_TYPE.ENDIAN_DYNAMIC;
            }
            throw new NotSupportedException();
        }

        public static ORDER_TYPE translateStringToOrderType(string strOrderType)
        {
            if (strOrderType == "" || strOrderType == "fixed")
            {
                return ORDER_TYPE.FIXED;
            }
            else if (strOrderType == "variable")
            {
                return ORDER_TYPE.VARIABLE;
            }
            return ORDER_TYPE.UNKNOWN;
        }

        public static NUMBER_DISPLAY_TYPE translateStringToNumberDisplayType(string strNumberDisplayType)
        {
            if (String.IsNullOrEmpty(strNumberDisplayType))
            {
                return NUMBER_DISPLAY_TYPE.NUMBER_DISPLAY_DECIMAL;
            }
            else if (strNumberDisplayType == "hex")
            {
                return NUMBER_DISPLAY_TYPE.NUMBER_DISPLAY_HEX;
            }
            else if (strNumberDisplayType == "octal")
            {
                return NUMBER_DISPLAY_TYPE.NUMBER_DISPLAY_OCTAL;
            }
            else if (strNumberDisplayType == "exponent")
            {
                return NUMBER_DISPLAY_TYPE.NUMBER_DISPLAY_EXPONENT;
            }
            else if (strNumberDisplayType == "decimal")
            {
                return NUMBER_DISPLAY_TYPE.NUMBER_DISPLAY_DECIMAL;
            }
            else if (strNumberDisplayType == "binary")
            {
                return NUMBER_DISPLAY_TYPE.NUMBER_DISPLAY_BINARY;
            }
            throw new NotSupportedException();
        }

        public static LENGTH_UNIT translateStringToLengthUnit(string strLengthUnit)
        {
            if (strLengthUnit == "" || strLengthUnit == "byte")
            {
                return LENGTH_UNIT.LENGTH_BYTE;
            }
            else if (strLengthUnit == "bit")
            {
                return LENGTH_UNIT.LENGTH_BIT;
            }
            return LENGTH_UNIT.LENGTH_UNSPECIFIED;
        }

        public static STRING_LENGTH_TYPE translateStringToStringLengthType(string strStringLengthType)
        {
            if (strStringLengthType == "delimiter-terminated")
            {
                return STRING_LENGTH_TYPE.STRING_LENGTH_DELIMITER_TERMINATED;
            }
            else if (strStringLengthType == "fixed-length")
            {
                return STRING_LENGTH_TYPE.STRING_LENGTH_FIXED;
            }
            else if (strStringLengthType == "pascal")
            {
                return STRING_LENGTH_TYPE.STRING_LENGTH_PASCAL;
            }
            else if (strStringLengthType == "zero-terminated")
            {
                return STRING_LENGTH_TYPE.STRING_LENGTH_ZERO_TERMINATED;
            }
            throw new NotSupportedException();
        }
    }
}
