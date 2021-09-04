using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kernel
{
    public enum ELEMENT_TYPE
    {
        ELEMENT_NONE, // Undefined element type
        ELEMENT_BINARY, // Binary element
        ELEMENT_CUSTOM, // Custom element(uses scripted data type)
        ELEMENT_GRAMMAR_REF, // Grammar reference element
        ELEMENT_NUMBER, // Number element
        ELEMENT_STRING, // String element
        ELEMENT_OFFSET, // File offset element
        ELEMENT_SCRIPT, // Script element
        ELEMENT_STRUCTURE, // Structure element
        ELEMENT_STRUCTURE_REF, // Structure reference element        
    }
}
