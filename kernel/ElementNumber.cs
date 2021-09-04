using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;

namespace kernel
{
    public class ElementNumber : ElementBase
    {         
        public override ELEMENT_TYPE GetElementType()
        {
            return ELEMENT_TYPE.ELEMENT_NUMBER;
        }





        public override MapResult mapByteViewOnce(ByteView byteView, Result result, MapContext mapContext, string showName)
        {    
            Int64 length_bits = lengthInBits(byteView.count_of_bits);

            result.value.SetContent(VALUE_TYPE.VALUE_TYPE_NUMBER_SIGNED, byteView.TakeBits(length_bits, () => ($"parsing number element({this.name}), path: {result.GetErrorPath()}", true)));
            result.value.number_display_type = numberDisplayType;
            result.value.endianType = this.endianType;
            AddSubMaskValues(result.value.getUnsignedNumber(), result);
            return MapResult.CreateWithLength(length_bits);
        }

        private UInt64 hexNumberToUint64(string hexNumber)
        {
            UInt64 r = 0;
            UInt64.TryParse(hexNumber.Substring(2), NumberStyles.HexNumber, null, out r);
            return r;
        }

        private void AddSubMaskValues(UInt64 number, Result result)
        {
            foreach(Mask mask in masks)
            {
                UInt64 maskValue = hexNumberToUint64(mask.value);
                UInt64 maskedvalue = number & maskValue;
                FixedValue fixedValue =  mask.fixedValues.FirstOrDefault(fixedValue => hexNumberToUint64(fixedValue.value) == maskedvalue);
                string matched_name = "";
                if (fixedValue != null)
                {
                    matched_name = fixedValue.name;
                }
                Result subResult = new Result();
                subResult.isMask = true;
                subResult.level = result.level + 1;
                subResult.value = Value.CreateValueDirectly();
                subResult.elementBase = this;
                subResult.value.setString($"0x{maskedvalue.ToString("X")}");
                subResult.matched_fixed_name = matched_name;
                subResult.value.index_of_bits = result.value.index_of_bits;
                subResult.value.count_of_bits = result.value.count_of_bits;
                subResult.name = mask.name;
                subResult.isFragment = false;

                result.AddSubResult(subResult);
            }
        }
    }
}
