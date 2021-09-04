using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kernel
{

    /*
       A string element object represents one string item in a structure.
    */
    public class StringElement
    {

        /*
           Get encoding of string element.

        */
        public String getEncoding()
        {
            throw new NotImplementedException();
        }


        /*
        Set string encoding of element.
                Parameters:
                    encoding The new string encoding of the element
        */
        public void setEncoding(String encoding)
        {
            throw new NotImplementedException();
        }


        /*
            Get the number of bytes a character needs.This information is derived from the selected string encoding
        */
        public int getBytesPerChar()
        {
            throw new NotImplementedException();
        }




        /*
          Get length type of string element.Valid types are:
                • STRING_LENGTH_FIXED
                • STRING_LENGTH_ZERO_TERMINATED
                • STRING_LENGTH_PASCAL
                • STRING_LENGTH_DELIMITER_TERMINATED
        */
        public STRING_LENGTH_TYPE getLengthType()
        {
            throw new NotImplementedException();
        }
    }
}
