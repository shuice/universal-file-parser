using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kernel
{
    public enum STRING_LENGTH_TYPE
    {
        STRING_LENGTH_FIXED,// Fixed-length string
        STRING_LENGTH_ZERO_TERMINATED,//  Zero-terminated string
        STRING_LENGTH_PASCAL,//  Pascal(length-prefixed) string
        STRING_LENGTH_DELIMITER_TERMINATED,//  Delimiter-terminated string
    }
}
