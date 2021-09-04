using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kernel
{
    public enum EnumLogLevel
    {           
        debug, // Debug log severity
        verbose,  // Verbose log severity
        warning,  // Warning log severity
        error,  // Error log severity
        fatal,  // Fatal log severity
    }
    public interface ILog
    {
        void addLog(EnumLogLevel level, String message);
    }
}
