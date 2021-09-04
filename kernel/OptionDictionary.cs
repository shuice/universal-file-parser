using System;
using System.Collections.Generic;
using System.Text;

namespace kernel
{
    public abstract class OptionDictionary
    {         
        private Dictionary<string, string> dic_value2ui = null;
        private Dictionary<string, string> dic_ui2value = null;
        public OptionDictionary()
        {
            dic_value2ui = new Dictionary<string, string>(DictionaryValue2UI());
            dic_ui2value = new Dictionary<string, string>();
            foreach(KeyValuePair<string, string> pair in dic_value2ui)
            {
                dic_ui2value[pair.Value] = pair.Key;
            }
        }

        protected abstract Dictionary<string, string> DictionaryValue2UI();
        public string Value2Ui(string value)
        { 
            string ui = "";
            if (dic_value2ui.TryGetValue(value, out ui))
            {
                return ui;
            }
            return value;
        }
        public string Ui2Value(string ui)
        {
            string value = "";
            if (dic_ui2value.TryGetValue(ui, out value))
            {
                return value;
            }
            return ui;
        }
    }
    public class OptionDictionaryRepeatMax : OptionDictionary
    {
        protected override Dictionary<string, string> DictionaryValue2UI()
        {
            return new Dictionary<string, string>
            {
                { "-1", "Unlimited" }
            };
        }
    }

    public class OptionDictionaryLength : OptionDictionary
    {
        protected override Dictionary<string, string> DictionaryValue2UI()
        {
            return new Dictionary<string, string>
            {
                { "remaining", "Remaining" },
                { "actual", "Actual" }
            };
        }
    }



    public class OptionDictionaryLengthUnit : OptionDictionary
    {
        protected override Dictionary<string, string> DictionaryValue2UI()
        {
            return new Dictionary<string, string>
            {
                { "byte", "Bytes" },
                { "bit", "Bits" }
            };
        }
    }

    public class OptionDictionaryNumberType : OptionDictionary
    {
        protected override Dictionary<string, string> DictionaryValue2UI()
        {
            return new Dictionary<string, string>
            {
                { "integer", "Integer" },
                { "float", "Floating Point" }
            };
        }
    }

    public class OptionDictionaryStringType : OptionDictionary
    {
        protected override Dictionary<string, string> DictionaryValue2UI()
        {
            return new Dictionary<string, string>
            {
                { "zero-terminated", "Zero terminated" },
                { "fixed-length", "Fixed length" },
                { "pascal", "Length prefixed (Pascal)" },
                { "delimiter-terminated", "Delimiter terminated" }
            };
        }
    }

    public class OptionDictionaryEndianness : OptionDictionary
    {
        protected override Dictionary<string, string> DictionaryValue2UI()
        {
            return new Dictionary<string, string>
            {
                { "big", "Big" },
                { "little", "Little" },
                { "dynamic", "Dynamic" }
            };
        }
    }

    public class OptionDictionaryYesNo : OptionDictionary
    {
        protected override Dictionary<string, string> DictionaryValue2UI()
        {
            return new Dictionary<string, string>
            {
                { "yes", "Yes" },
                { "no", "No" },
            };
        }
    }

    public class OptionDictionaryDisplay : OptionDictionary
    {
        protected override Dictionary<string, string> DictionaryValue2UI()
        {
            return new Dictionary<string, string>
            {
                { "decimal", "Decimal" },
                { "hex", "Hexadecimal" },
                { "octal", "Octal" },
                { "binary", "Binary" },
            };
        }
    }


    public class OptionDictionaryElementOrder : OptionDictionary
    {
        protected override Dictionary<string, string> DictionaryValue2UI()
        {
            return new Dictionary<string, string>
            {
                { "fixed", "Fixed" },
                { "variable", "Variable" }
            };
        }
    }


    public class OptionDictionaryScriptType : OptionDictionary
    {
        protected override Dictionary<string, string> DictionaryValue2UI()
        {
            return Grammar.script_type_2_ui();
        }
    }
}
