using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kernel
{
    public class ElementKey
    {
        public static string disabled = "disabled"; // yes

        public static string name = "name";
        public static string id = "id";
        public static string strokecolor = "strokecolor";
        public static string fillcolor = "fillcolor";
        public static string repeatmin = "repeatmin";
        public static string repeatmax = "repeatmax";        
        public static string repeat = "repeat";
        public static string length = "length";

        public static string mustmatch = "mustmatch";

        // number
        public static string type = "type";
        public static string lengthunit = "lengthunit";
        public static string signed = "signed";
        public static string endian = "endian";
        public static string display = "display";
        public static string minval = "minval";
        public static string maxval = "maxval";
        public static string valueexpression = "valueexpression";


        // grammar ref
        public static string filename = "filename";
        public static string uti = "uti";
        public static string fileextension = "fileextension";

        // string
        public static string delimiter = "delimiter";

        // structure
        public static string alignment = "alignment";
        public static string encoding = "encoding";
        public static string order = "order";
        public static string consists_of = "consists-of";
        public static string extends = "extends";
        public static string debug = "debug";
        public static string derived = "derived";

        // structure ref
        public static string structure = "structure";

        // offset
        public static string references = "references";
        public static string referenced_size = "referenced-size";
        public static string follownullreference = "follownullreference";   // "yes" "no"
        public static string relative_to = "relative-to";
        public static string additional = "additional"; // It is a numeric value or an expression like length, and the value of the expression may be in the script of the same structure
        // display

        // custom
        public static string script = "script";


        public static readonly IReadOnlyDictionary<string, string> dicDefaultAttributeName2Value = new Dictionary<string, string>()
            {
                { ElementKey.disabled, "no" },
                { ElementKey.name, "" },
                ////{ ElementKey.id, "0" },
                { ElementKey.strokecolor, "" },
                { ElementKey.fillcolor, "" },

                { ElementKey.repeatmin, "1" },
                { ElementKey.repeatmax, "1" },
                { ElementKey.repeat, "" },
                { ElementKey.length, "" },
                { ElementKey.mustmatch, "no" },
                ////{ ElementKey.type, "0" },

                { ElementKey.lengthunit, "byte" },
                { ElementKey.signed, "" },
                { ElementKey.endian, "" },
                { ElementKey.display, "decimal" },
                { ElementKey.minval, "" },

                { ElementKey.maxval, "" },
                { ElementKey.valueexpression, "" },
                { ElementKey.filename, "" },
                { ElementKey.uti, "" },
                { ElementKey.fileextension, "" },

                { ElementKey.delimiter, "" },
                { ElementKey.alignment, "0" },
                { ElementKey.encoding, "" },
                { ElementKey.order, "fixed" },
                { ElementKey.consists_of, "" },

                { ElementKey.extends, "" },
                { ElementKey.debug, "no" },
                { ElementKey.derived, "" },
                { ElementKey.structure, "" },
                { ElementKey.references, "" },

                { ElementKey.referenced_size, "" },
                { ElementKey.follownullreference, "" },
                { ElementKey.relative_to, "" },
                { ElementKey.additional, "" },
                { ElementKey.script, "" },
            };

    }
}
