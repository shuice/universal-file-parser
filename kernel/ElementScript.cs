using IronPython.Modules;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Xml;

namespace kernel
{
    public class ElementScript : ElementBase
    {
        public override ELEMENT_TYPE GetElementType()
        {
            return ELEMENT_TYPE.ELEMENT_SCRIPT;
        }



        public override MapResult mapByteViewOnce(ByteView byteView, Result result, MapContext mapContext, string showName)
        {
            IScript script = mapContext.scriptInstance.GetScript(this.scriptLanguage == "Python" 
                                                                    ? ScriptEnv.python 
                                                                    : ScriptEnv.lua);

            mapContext.results.elementScriptAddedResults.Clear();
            mapContext.results.inElementScript = true;


            script.SetValue("currentOffset", ByteView.ConvertBitsCount2BytesCount(byteView.index_of_bits));
            script.EvalScriptWithException(this.scriptContent);

            mapContext.results.inElementScript = false;
            List<Result> scriptAddedResults = mapContext.results.elementScriptAddedResults;
            Int64 max_position = -1;
            scriptAddedResults.ForEach(r => max_position = Math.Max(max_position, r.value.index_of_bits + r.value.count_of_bits));
            if ((byteView.index_of_bits + byteView.count_of_bits > max_position) && (max_position > byteView.index_of_bits))
            {
                Int64 bits_used = max_position - byteView.index_of_bits;
                return MapResult.CreateWithLength(bits_used);
            }
            return MapResult.CreateWithLength(0);
        }
    }
}
