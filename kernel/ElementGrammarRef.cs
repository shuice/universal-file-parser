using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace kernel
{
    public class ElementGrammarRef : ElementBase
    {
        public string fileName = "";
        public string uti = "";
        public string fileExtension = "";
        public override ELEMENT_TYPE GetElementType()
        {
            return ELEMENT_TYPE.ELEMENT_GRAMMAR_REF;
        }


        public override MapResult mapByteViewOnce(ByteView byteView, Result result, MapContext mapContext, string showName)
        {
            string fileName = this.GetValue(ElementKey.filename);
            Grammar grammar = this.grammar.loadGrammarRef(fileName);            
            StructureMapper structureMapper = new StructureMapper(grammar, mapContext.log, byteView, result.level, false);
            long bytesProcessed = structureMapper.process();
            long bitsProcessed = ByteView.ConvertBytesCount2BitsCount(bytesProcessed);
            if (bitsProcessed > 0)
            {
                result.value.byteView = byteView.TakeBits(bitsProcessed, ()=>($"parsing grammar reference element({this.name}), path: {result.GetErrorPath()}", true));
                result.AddSubResults(structureMapper.results.rootResult.results);
                return MapResult.CreateWithLength(bitsProcessed);
            }
            return MapResult.CreateWithError(MapError.grammra_ref_map_error, $"Failed to parsing grammar reference element({this.name}), path: {result.GetErrorPath()}");            
        }
    }
}
