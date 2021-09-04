using Neo.IronLua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kernel
{
    public class ScriptLua : IScript
    {
        private Lua lua;
        LuaGlobal env;

        public ScriptLua()
        {
            lua = new Lua(LuaIntegerType.Int64, LuaFloatType.Double);
            env = lua.CreateEnvironment();
            setup_global_functions();
        }

        private void setup_global_functions()
        {
            LuaTable synalysisTable = new LuaTable();
            synalysisTable.SetValue("ENDIAN_UNDEFINED", "ENDIAN_UNDEFINED");
            synalysisTable.SetValue("ENDIAN_UNKNOWN", "ENDIAN_UNKNOWN");
            synalysisTable.SetValue("ENDIAN_BIG", "ENDIAN_BIG");
            synalysisTable.SetValue("ENDIAN_LITTLE", "ENDIAN_LITTLE");
            synalysisTable.SetValue("ENDIAN_DYNAMIC", "ENDIAN_DYNAMIC");


            synalysisTable.SetValue("LENGTH_UNSPECIFIED", "LENGTH_UNSPECIFIED");
            synalysisTable.SetValue("LENGTH_BYTE", "LENGTH_BYTE");
            synalysisTable.SetValue("LENGTH_BIT", "LENGTH_BIT");

            synalysisTable.SetValue("ELEMENT_NONE", "ELEMENT_NONE");
            synalysisTable.SetValue("ELEMENT_BINARY", "ELEMENT_BINARY");
            synalysisTable.SetValue("ELEMENT_CUSTOM", "ELEMENT_CUSTOM");
            synalysisTable.SetValue("ELEMENT_GRAMMAR_REF", "ELEMENT_GRAMMAR_REF");
            synalysisTable.SetValue("ELEMENT_NUMBER", "ELEMENT_NUMBER");
            synalysisTable.SetValue("ELEMENT_STRING", "ELEMENT_STRING");
            synalysisTable.SetValue("ELEMENT_OFFSET", "ELEMENT_OFFSET");
            synalysisTable.SetValue("ELEMENT_SCRIPT", "ELEMENT_SCRIPT");
            synalysisTable.SetValue("ELEMENT_STRUCTURE", "ELEMENT_STRUCTURE");
            synalysisTable.SetValue("ELEMENT_STRUCTURE_REF", "ELEMENT_STRUCTURE_REF");

            synalysisTable.SetValue("SEVERITY_UNKNOWN", "SEVERITY_UNKNOWN");
            synalysisTable.SetValue("SEVERITY_FATAL", "SEVERITY_FATAL");
            synalysisTable.SetValue("SEVERITY_ERROR", "SEVERITY_ERROR");
            synalysisTable.SetValue("SEVERITY_WARNING", "SEVERITY_WARNING");
            synalysisTable.SetValue("SEVERITY_INFO", "SEVERITY_INFO");
            synalysisTable.SetValue("SEVERITY_VERBOSE", "SEVERITY_VERBOSE");
            synalysisTable.SetValue("SEVERITY_DEBUG", "SEVERITY_DEBUG");

            synalysisTable.SetValue("RESULT_STRUCTURE_START_TYPE", "RESULT_STRUCTURE_START_TYPE");
            synalysisTable.SetValue("RESULT_STRUCTURE_END_TYPE", "RESULT_STRUCTURE_END_TYPE");
            synalysisTable.SetValue("RESULT_STRUCTURE_ELEMENT_TYPE", "RESULT_STRUCTURE_ELEMENT_TYPE");
            synalysisTable.SetValue("RESULT_MASK_TYPE", "RESULT_MASK_TYPE");

            synalysisTable.SetValue("VALUE_TYPE_BINARY", "VALUE_TYPE_BINARY");
            synalysisTable.SetValue("VALUE_TYPE_BOOLEAN", "VALUE_TYPE_BOOLEAN");
            synalysisTable.SetValue("VALUE_TYPE_NUMBER_UNSIGNED", "VALUE_TYPE_NUMBER_UNSIGNED");
            synalysisTable.SetValue("VALUE_TYPE_NUMBER_SIGNED", "VALUE_TYPE_NUMBER_SIGNED");
            synalysisTable.SetValue("VALUE_TYPE_NUMBER_FLOAT", "VALUE_TYPE_NUMBER_FLOAT");
            synalysisTable.SetValue("VALUE_TYPE_STRING", "VALUE_TYPE_STRING");

            synalysisTable.SetValue("NUMBER_INTEGER", "NUMBER_INTEGER");
            synalysisTable.SetValue("NUMBER_FLOAT", "NUMBER_FLOAT");

            synalysisTable.SetValue("NUMBER_DISPLAY_DECIMAL", "NUMBER_DISPLAY_DECIMAL");
            synalysisTable.SetValue("NUMBER_DISPLAY_EXPONENT", "NUMBER_DISPLAY_EXPONENT");
            synalysisTable.SetValue("NUMBER_DISPLAY_HEX", "NUMBER_DISPLAY_HEX");
            synalysisTable.SetValue("NUMBER_DISPLAY_OCTAL", "NUMBER_DISPLAY_OCTAL");
            synalysisTable.SetValue("NUMBER_DISPLAY_BINARY", "NUMBER_DISPLAY_BINARY");

            synalysisTable.SetValue("STRING_LENGTH_FIXED", "STRING_LENGTH_FIXED");
            synalysisTable.SetValue("STRING_LENGTH_ZERO_TERMINATED", "STRING_LENGTH_ZERO_TERMINATED");
            synalysisTable.SetValue("STRING_LENGTH_PASCAL", "STRING_LENGTH_PASCAL");
            synalysisTable.SetValue("STRING_LENGTH_DELIMITER_TERMINATED", "STRING_LENGTH_DELIMITER_TERMINATED");



            SetValue("synalysis", synalysisTable);
        }

        public long? EvalExpression(string expression)
        {
            throw new InvalidOperationException();
        }

        public void EvalScriptWithException(string strScript)
        {
            dynamic dEnv = env;
            dEnv.dochunk(strScript);
        }

        public void SetPredefineVariable(SCRIPT_PERDEFINE_GLOBAL_VAR_NAME name, object value)
        {
            env[name.ToString()]  = value;                 
        }

        public void SetValue(string name, ulong value)
        {
            env[name] = value;
        }

        public void SetValue(string name, long value)
        {
            env[name] = value;
        }

        public void SetValue(string name, Object value)
        {
            env[name] = value;
        }
    }
}
