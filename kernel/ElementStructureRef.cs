using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace kernel
{
    public class ElementStructureRef : ElementBase
    {
        public override ELEMENT_TYPE GetElementType()
        {
            return ELEMENT_TYPE.ELEMENT_STRUCTURE_REF;
        }

        public override MapResult mapByteViewOnce (ByteView byteView, Result result, MapContext mapContext, string showName)
        {
            string structure_id = GetValue(ElementKey.structure);
            ElementStructure element = grammar.GetStructureByIdWithPrefix(structure_id);            
            if (element != null)
            {
                MapResult mapResult = element.mapByteView(byteView, result, mapContext, showName);
                if (mapResult.Breaked() == false)
                {
                    result.value.SetContent(VALUE_TYPE.VALUE_TYPE_STRUCTURE_REF, byteView.TakeBits(mapResult.used_bits, ()=>($"parsing structure reference element({this.name}), path: {result.GetErrorPath()}", true)));
                }
                return mapResult;
            }
            return MapResult.CreateWithError(MapError.gramma_error, $"Can not find structrue with id \"{structure_id}\"");
        }
    }
}
