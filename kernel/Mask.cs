using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kernel
{

    /*
        A Mask object represents a binary mask
     */
    public class Mask : ICloneable
    {
        public string name = "";
        public string value = "";
        public List<FixedValue> fixedValues = new List<FixedValue>();

        public object Clone()
        {
            Mask mask = new Mask
            {
                name = this.name,
                value = this.value,
                fixedValues = new List<FixedValue>(this.fixedValues)
            };
            return mask;            
        }
    }
}
