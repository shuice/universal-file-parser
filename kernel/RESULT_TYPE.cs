using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kernel
{
    public enum RESULT_TYPE
    {
        RESULT_STRUCTURE_START_TYPE, // Start of structure
        RESULT_STRUCTURE_END_TYPE, //  End of structure
        RESULT_STRUCTURE_ELEMENT_TYPE, //  Structure element
        RESULT_MASK_TYPE, // Result for mask value
    }
}
