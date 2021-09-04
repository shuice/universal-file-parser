using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kernel
{
    public enum ORDER_TYPE
    {
        UNKNOWN,
        FIXED,      // all elements in the structure have to appear in a fixed order
        VARIABLE,   // only a single element of many is expected
    }
}
