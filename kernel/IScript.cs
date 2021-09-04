using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kernel
{
    public interface IScript
    {
        void SetPredefineVariable(SCRIPT_PERDEFINE_GLOBAL_VAR_NAME name, Object value);
        Int64? EvalExpression(string expression);

        void EvalScriptWithException(string strScript);

        void SetValue(string name, UInt64 value);

        void SetValue(string name, Int64 value);

        void SetValue(string name, Object value);

    }
}
