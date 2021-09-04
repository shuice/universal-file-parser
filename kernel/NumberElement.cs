using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kernel
{

    /*
      A number element object represents one number item in a structure.
     */
    public class NumberElement
    {
        private NUMBER_DISPLAY_TYPE _numberDisplayType = NUMBER_DISPLAY_TYPE.NUMBER_DISPLAY_DECIMAL;
        private NUMBER_TYPE _numberType = NUMBER_TYPE.NUMBER_INTEGER;
        private ENDIAN_TYPE _endianness = ENDIAN_TYPE.ENDIAN_UNDEFINED;
        private bool _signed = true;


        /*
         Get number display type.This determines how a number is displayed and can be one of
            • NUMBER_DISPLAY_DECIMAL
            • NUMBER_DISPLAY_EXPONENT
            • NUMBER_DISPLAY_HEX
            • NUMBER_DISPLAY_OCTAL
            • NUMBER_DISPLAY_BINARY
         */
        public NUMBER_DISPLAY_TYPE getNumberDisplayType()
        {
            return _numberDisplayType;
        }


        /*
        Set number display type.This determines how a number is displayed and can be one of
                • NUMBER_DISPLAY_DECIMAL
                • NUMBER_DISPLAY_EXPONENT
                • NUMBER_DISPLAY_HEX
                • NUMBER_DISPLAY_OCTAL
                • NUMBER_DISPLAY_BINARY
                Parameters:
                    numberDisplayType Type for number display in parsing results
         */
        public void setNumberDisplayType(NUMBER_DISPLAY_TYPE numberDisplayType)
        {
            _numberDisplayType = numberDisplayType;
        }



        /*
         Set number type of number element.This can be one of
            • NUMBER_INTEGER
            • NUMBER_FLOAT

            @author Andreas Pehnack @date 2015-09-01 @see getNumberType
         */
        public void setNumberType(NUMBER_TYPE numberType)
        {
            _numberType = numberType;
        }



        /*
         Get number type of number element.This can be one of
            • NUMBER_INTEGER
            • NUMBER_FLOAT
         */
        public NUMBER_TYPE getNumberType()
        {
            return _numberType;
        }

        /*
            Get endianness of number element.
         */
        public ENDIAN_TYPE getEndianness()
        {
            return _endianness;
        }

        /*
            Set endianness of number element.@author Andreas Pehnack @date 2015-09-01 @see                
        */
        public void setEndianness(ENDIAN_TYPE endianness)
        {
            _endianness = endianness;
        }


        /*
            Set if the number element parses a signed or an unsigned value
        */
        public void setSigned(bool signed)
        {
            _signed = signed;
        }

        /*
             Query if the number element parses a signed or an unsigned value
         */
        public bool isSigned()
        {
            return _signed;
        }
    }
}
