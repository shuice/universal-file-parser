using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kernel
{
    public enum ENDIAN_TYPE
    {
        ENDIAN_UNDEFINED,   // Undefined endianness
        ENDIAN_UNKNOWN,     // Unknown endianness
        ENDIAN_BIG,         // Big endian (see glossary)
        ENDIAN_LITTLE,      // Little endian (see glossary)
        ENDIAN_DYNAMIC,     // Dynamic endianness. Can be set via script while parsing file
    }
}
