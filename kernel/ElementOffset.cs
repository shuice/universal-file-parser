using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;

namespace kernel
{
    public class ElementOffset : ElementBase
    {        
        public override ELEMENT_TYPE GetElementType()
        {
            return ELEMENT_TYPE.ELEMENT_OFFSET;
        }


        public override MapResult mapByteViewOnce(ByteView byteView, Result result, MapContext mapContext, string showName)
        {
            // The current brothers and sisters have finished analyzing, then come to analyze me
            Int64 iLength = lengthInBits(byteView.count_of_bits);
            if (iLength == 0)
            {
                return MapResult.CreateWithError(MapError.gramma_error, $"Unable to parse offset elements with a length value of zero, path: {result.GetErrorPath()}");
            }
            result.value.SetContent(VALUE_TYPE.VALUE_TYPE_NUMBER_UNSIGNED, byteView.TakeBits(iLength, () => ($"parsing offset element({this.name}), path: {result.GetErrorPath()}", true)));
            result.value.endianType = this.endianType;
            result.value.number_display_type = NUMBER_DISPLAY_TYPE.NUMBER_DISPLAY_HEX;
            return MapResult.CreateWithLength(iLength);
        }

        private long? find_relative_to_start_bits(Result current_result, string relative_to)
        {
            Result from = current_result;
            for (Result currentStruct = current_result.parent;
                currentStruct != null;
                from = currentStruct, currentStruct = currentStruct.parent)
            {
                var subResults = new List<Result>(currentStruct.results);
                int take_count = subResults.IndexOf(from) + 1;
                foreach (Result option in subResults.Take(take_count).Reverse<Result>())
                {
                    if (option.elementBase != null && option.elementBase.IdWithPrefix() == relative_to)
                    {
                        return option.value.index_of_bits;
                    }
                }
            }
            return null;
        }

        public MapResult ApplyOffsetStructure(Result current_result, MapContext mapContext)
        {
            Debug.Assert(current_result.elementBase == this);

            long target_position_bits = ByteView.ConvertBytesCount2BitsCount(current_result.value.ToInt64FromValue());
            if ((target_position_bits == 0) && this.GetValueDerivedWithDefault(ElementKey.follownullreference) != "yes")
            {
                return MapResult.CreateWithLength(0);
            }
            string relative_to = GetValueDerivedWithDefault(ElementKey.relative_to);
            if (String.IsNullOrEmpty(relative_to) == false)
            {
                long? relative_to_start_bits = find_relative_to_start_bits(current_result, relative_to);
                if (relative_to_start_bits.HasValue == false)
                {
                    string msg = $"Failed to resolve \"relative to\" field({relative_to}) of offset element({name}), path: {current_result.GetErrorPath()}";
                    mapContext.log.addLog(EnumLogLevel.warning, msg);
                    return MapResult.CreateWithError(MapError.gramma_error, msg);                    
                }
                target_position_bits += relative_to_start_bits.GetValueOrDefault(0);
            }
            string references = GetValueDerivedWithDefault(ElementKey.references);
            ElementStructure target_structure = null;
            if (references.StartsWith("id:"))
            {
                target_structure = mapContext.gramma.GetStructureByIdWithPrefix(references);
            }
            else
            {
                target_structure = mapContext.gramma.getStructureByName(references);
            }
            Debug.Assert(target_structure != null);

            Int64 target_size_bits = 0;
            string referenced_size = GetValueDerivedWithDefault(ElementKey.referenced_size);
            if (referenced_size.Length > 0)
            {
                ElementNumber elementNumber = this.grammar.GetElementByIdWithPrefix(referenced_size, parentStructure) as ElementNumber;
                Debug.Assert(elementNumber != null);
                Int64 target_size_bytes = mapContext.scriptInstance.GetScript(ScriptEnv.python).EvalExpression(elementNumber.name).GetValueOrDefault(0);
                target_size_bits = ByteView.ConvertBytesCount2BitsCount(target_size_bytes);
            }

            Int64 addtionalBytes = 0;
            string strAddtionalBytes = GetValueDerivedWithDefault(ElementKey.additional);
            if (strAddtionalBytes.Length > 0)
            {
                addtionalBytes = ConvertString2Int64(strAddtionalBytes).GetValueOrDefault(0);                
            }

            target_position_bits += ByteView.ConvertBytesCount2BitsCount(addtionalBytes);
            MapResult mapResult = mapContext.sturctureMapper.mapStructureAtPositionBits(target_structure, current_result.parent, target_position_bits, target_size_bits);
            return mapResult;
        }
    }
}
