using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IronPython.Modules;

namespace kernel
{
    public class FileStructurePluginHelper : file_structure_plugin.IFileStructurePluginHelper
    {
        public static FileStructurePluginHelper instance = new FileStructurePluginHelper();
        public object CreateElement(string element_type, string name, bool autosetDefaults)
        {
            Grammar grammar = MapContext.mapContexts.Peek().gramma;
            ElementBase r = null;
            if (element_type == file_structure_plugin.ELEMENT_NUMBER)
            {
                r = grammar.CreateDefaultElement(ELEMENT_TYPE.ELEMENT_NUMBER);
            }
            else if (element_type == file_structure_plugin.ELEMENT_STRING)
            {
                r = grammar.CreateDefaultElement(ELEMENT_TYPE.ELEMENT_STRING);
            }
            else
            {
                throw new NotSupportedException();
            }
            r.SetAttribute(ElementKey.name, name);
            r.create_from_script = true;
            r.mapContext = MapContext.mapContexts.Peek();
            return r;            
        }

        public object CreateValue()
        {
            return Value.CreateValueDirectly();            
        }

        public void logMessage(string module, int messageID, string severity, string message)
        {
            throw new NotImplementedException();
        }
    }
}
