using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kernel
{

    /*
             An offset element object represents one file offset item in a structure.
     */
    public class OffsetElement
    {
        string _additionalOffset = "";
        /*
         Set expression which result to be added to parsed offset value
            Parameters:
                additionalOffset String with expression
        */
        public void setAdditionalOffset(String additionalOffset)
        {
            _additionalOffset = additionalOffset;
        }

    }
}
