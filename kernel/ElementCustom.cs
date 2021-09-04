using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml;

namespace kernel
{
    public class ElementCustom : ElementBase
    {
        public override ELEMENT_TYPE GetElementType()
        {
            return ELEMENT_TYPE.ELEMENT_CUSTOM;
        }



        public override MapResult mapByteViewOnce(ByteView byteView, Result result, MapContext mapContext, string showName)
        {
            string script_id = GetValue(ElementKey.script);
            if (script_id == "")
            {
                return MapResult.CreateWithLength(ByteView.ConvertBytesCount2BitsCount(this.lengthInBytes(byteView.count_of_bits)));
            }

            List<EmbedScript> embedScripts = mapContext.gramma.getCustomScript(GetValue(ElementKey.script)).ToList();
            if (embedScripts.Count == 0)
            {
                return MapResult.CreateWithError(MapError.gramma_error, $"Script does not exist in grammar file while parsing custom element({this.name}), path: {result.GetErrorPath()}");
            }
            EmbedScript embedScript = embedScripts.First();

            try
            {
                IScript script = mapContext.scriptInstance.GetScript(embedScript.language == "Python" ? ScriptEnv.python : ScriptEnv.lua);
                script.SetValue("element", this);
                script.SetValue("byteView", byteView);
                script.SetValue("bitPos", byteView.index_of_bits);
                script.SetValue("bitLength", byteView.count_of_bits);
                script.SetValue("results", mapContext.results);
                script.EvalScriptWithException(embedScript.source);

                bool unit_is_byte = (embedScript.source.Contains("parseByteRange("));
                string call_function_string = $"{(unit_is_byte ? "parseByteRange" : "parseBitRange")}(element, byteView, bitPos, bitLength, results)";
                long fuction_return = script.EvalExpression(call_function_string).GetValueOrDefault();

                long used_in_bits = unit_is_byte ? ByteView.ConvertBytesCount2BitsCount(fuction_return) : fuction_return;

                result.value.byteView = byteView;
                return MapResult.CreateWithLength(used_in_bits);
            }
            catch(Exception e)
            {
                return MapResult.CreateWithError(MapError.gramma_error, $"An exception occurred while parsing path: {result.GetErrorPath()}\n{e.ToString()}");
            }
        }
    }
}
