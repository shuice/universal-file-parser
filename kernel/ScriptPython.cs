using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IronPython;
using IronPython.Hosting;
using AnyPrefix.Microsoft.Scripting;
using AnyPrefix.Microsoft.Scripting.Hosting;
using AnyPrefix.Microsoft.Scripting.Runtime;

namespace kernel
{
    public class ScriptPython : IScript
    {
        private ScriptEngine engine;
        private ScriptScope scope;

        public ScriptPython()
        {
            engine = Python.CreateEngine();
            scope = engine.CreateScope();

            EvalScriptWithException("from file_structure_plugin import *");
        }
        public long? EvalExpression(string expression)
        {
            Int64 r = 0;
            try
            {
                List<string> to_be_removed_prefixs = new List<string>() { "this.", "prev." };
                foreach (String to_be_removed_prefix in to_be_removed_prefixs)
                {
                    if (expression.StartsWith(to_be_removed_prefix))
                    {
                        expression = expression.Substring(to_be_removed_prefix.Length);
                        break;
                    }
                }

                scope.SetVariable("my_private_express_result", 0);
                var script = engine.CreateScriptSourceFromString($"my_private_express_result={expression}", SourceCodeKind.Statements);
                script.Execute(scope);

                dynamic dynamic_value = scope.GetVariable("my_private_express_result");
                Int64 int64_value = (Int64)dynamic_value;
                r = int64_value;
            }
            catch (Exception)
            {
                return null;
            }
            return r;
        }

        public void EvalScriptWithException(string strScript)
        {
            var script = engine.CreateScriptSourceFromString(strScript, SourceCodeKind.Statements);            
            script.Execute(scope);
        }

        public void SetPredefineVariable(SCRIPT_PERDEFINE_GLOBAL_VAR_NAME name, object value)
        {
            string strName = name.ToString();
            scope.SetVariable(name.ToString(), value);
        }

        public void SetValue(string name, ulong value)
        {
            scope.SetVariable(name, value);
        }

        public void SetValue(string name, long value)
        {
            scope.SetVariable(name, value);
        }

        public void SetValue(string name, Object value)
        {
            scope.SetVariable(name, value);
        }
    }
}
