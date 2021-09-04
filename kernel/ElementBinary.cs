using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Linq;

namespace kernel
{
    public class ElementBinary : ElementBase
    {
        public static readonly ElementBinary paddingElementBinary = CreatePadding();
        private static ElementBinary CreatePadding()
        {
            ElementBinary n = new ElementBinary();
            n.attributes[ElementKey.name] = "Padding";
            return n;
        }


        public override ELEMENT_TYPE GetElementType()
        {
            return ELEMENT_TYPE.ELEMENT_BINARY;
        }



        public override MapResult mapByteViewOnce(ByteView byteView, Result result, MapContext mapContext, string showName)
        {
            Int64 iLengthInBits = lengthInBits(byteView.count_of_bits);
            if (iLengthInBits == Int64.MaxValue)
            {
                result.value.SetContent(VALUE_TYPE.VALUE_TYPE_BINARY, byteView);                
                result.value.valueType = VALUE_TYPE.VALUE_TYPE_BINARY;
                return MapResult.CreateWithLength(byteView.count_of_bits);
            }
            long max_length_bits = byteView.count_of_bits;
            long fixed_length_bits = Math.Min(iLengthInBits, byteView.count_of_bits);
            

            foreach(FixedValue fixedValue in fixedValues)
            {
                // fixedValue == binary width length
            }

            result.value.SetContent(VALUE_TYPE.VALUE_TYPE_BINARY, byteView.TakeBits(fixed_length_bits, () => ($"paring binary element({this.name}), path: {result.GetErrorPath()}", true)));
            return MapResult.CreateWithLength(fixed_length_bits);
        }
    }
}
