using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kernel
{
    public class ElementFactory
    {
        public static ElementBase CreateElementByName(string name)
        {
            if (name == "structure")
            {
                return new ElementStructure();
            }
            else if (name == "binary")
            {
                return new ElementBinary();
            }
            else if (name == "number")
            {
                return new ElementNumber();
            }
            else if (name == "string")
            {
                return new ElementString();
            }
            else if (name == "scriptelement")
            {
                return new ElementScript();
            }
            else if (name == "grammarref")
            {
                return new ElementGrammarRef();
            }
            else if (name == "structref")
            {
                return new ElementStructureRef();
            }
            else if (name == "offset")
            {
                return new ElementOffset();
            }
            else if (name == "custom")
            {
                return new ElementCustom();
            }
            else if (name == "description")
            {
                return null;
            }   
            Debug.Assert(false);
            return null;
        }

        public static string GetXmlNodeNameByElement(ElementBase elementBase)
        {
            if (elementBase is ElementStructure)
            {
                return "structure";
            }
            else if (elementBase is ElementBinary)
            {
                return "binary";
            }
            else if (elementBase is ElementNumber)
            {
                return "number";
            }
            else if (elementBase is ElementString)
            {
                return "string";
            }
            else if (elementBase is ElementScript)
            {
                return "scriptelement";
            }
            else if (elementBase is ElementGrammarRef)
            {
                return "grammarref";
            }
            else if (elementBase is ElementStructureRef)
            {
                return "structref";
            }
            else if (elementBase is ElementOffset)
            {
                return "offset";
            }
            else if (elementBase is ElementCustom)
            {
                return "custom";
            }
            Debug.Assert(false);
            return "";
        }
    }
}
