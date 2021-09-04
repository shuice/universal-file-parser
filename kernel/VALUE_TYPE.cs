using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kernel
{
    public enum VALUE_TYPE
    {
        VALUE_TYPE_UNDEFINED, // Defaults
        VALUE_TYPE_BINARY,// Binary value
        VALUE_TYPE_BOOLEAN,// Boolean value
        VALUE_TYPE_NUMBER_UNSIGNED,// Unsigned number value
        VALUE_TYPE_NUMBER_SIGNED,// Signed number value
        VALUE_TYPE_NUMBER_FLOAT,// Floating-point value
        VALUE_TYPE_STRING,// String value
        VALUE_TYPE_STRUCTURE, // structure, script, custom
        VALUE_TYPE_STRUCTURE_REF, // structure, script, custom
    }
}
