using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;


namespace kernel
{
    public enum SCRIPT_PERDEFINE_GLOBAL_VAR_NAME
    {
        currentMapper,
    }

    public enum ScriptEnv
    {
        python,        
        lua,
    }


    public class ScriptInstance
    {
        private IScript pythonScript;        
        private IScript luaScript;

       

        public ScriptInstance()
        {
            pythonScript = new ScriptPython();            
            luaScript = new ScriptLua();            
        }

        public void SetPredefineVariable(SCRIPT_PERDEFINE_GLOBAL_VAR_NAME name, object value)
        {
            pythonScript.SetPredefineVariable(name, value);
            luaScript.SetPredefineVariable(name, value);
        }

        public void SetValue(string name, UInt64 value)
        {
            pythonScript.SetValue(name, value);
            luaScript.SetValue(name, value);
        }
          

        public void SetValue(string name, Int64 value)
        {
            pythonScript.SetValue(name, value);
            luaScript.SetValue(name, value);
        }

        public void SetValue(string name, Object value)
        {
            pythonScript.SetValue(name, value);
            luaScript.SetValue(name, value);
        }

        public IScript GetScript(ScriptEnv env)
        {
            if (env == ScriptEnv.lua)
            {
                return luaScript;
            }
            return pythonScript;
        }
    }
}
