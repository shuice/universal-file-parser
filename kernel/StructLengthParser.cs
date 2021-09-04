using kernel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

namespace kernel
{
    public enum LengthValueType
    {
        use_remaining,  // "", unlimited, does not exist, depends on child elements
        fixed_size_defined,    // 0: has a size of 0, >0: has a size of >0
    }
    public class StructLengthParser
    {
        public LengthValueType lengthValueType = LengthValueType.use_remaining;    // Default dependent child elements
        public long defined_length_bits = 0;
        public bool continue_get = true;
        private LENGTH_UNIT lengthUnit;
        public StructLengthParser(LENGTH_UNIT lengthUnit)
        {
            this.lengthUnit = lengthUnit;
        }

        public long GetBitsLength(long remaing_bits_length)
        {
            if (lengthValueType == LengthValueType.use_remaining)
            {
                return remaing_bits_length;
            }
            return defined_length_bits;
        }

        public void try_get_element_key_long(ElementStructure elementStructure, string elementKey, Int64 count_of_bits, MapContext mapContext, Result current_result, List<string> lst_just_finished_sub_element_name)
        {
            if (continue_get == false)
            {
                return;
            }
            string s = elementStructure.GetValueDerivedWithDefault(elementKey);
            List<string> to_be_removed_prefixs = new List<string>() { "this.", "prev." };
            foreach (String to_be_removed_prefix in to_be_removed_prefixs)
            {
                if (s.StartsWith(to_be_removed_prefix))
                {
                    s = s.Substring(to_be_removed_prefix.Length);
                    break;
                }
            }
            if ((lst_just_finished_sub_element_name == null) || (lst_just_finished_sub_element_name.Count == 0))
            {
                // first time,      
                Debug.Assert(s != "unlimited");
                if (s == "remaining")
                {
                    lengthValueType = LengthValueType.fixed_size_defined;
                    defined_length_bits = count_of_bits;
                    continue_get = false;
                    return;
                }
                if (s.Length == 0)
                {
                    continue_get = false;
                    return;
                }
                Int64? r = s.ToInt64();
                if (r.HasValue)
                {
                    continue_get = false;
                    if (r.Value != 0)
                    {
                        SetDefinedLength(r.Value);
                    }
                    return;
                }


                List<String> vars = GetVarsFromExpression(s);
                List<string> sub_names = get_sub_element_names(elementStructure);
                HashSet<string> depends = new HashSet<string>(vars);
                depends.IntersectWith(sub_names);
                if (depends.Count == 0)
                {
                    // Not dependent on child elements
                    long? tmp = mapContext.scriptInstance.GetScript(ScriptEnv.python).EvalExpression(s);
                    Debug.Assert(tmp != null, "Global variables are missing");
                    SetDefinedLength(tmp.GetValueOrDefault(0));
                    continue_get = false;
                    return;
                }
                else
                {
                    current_result.dic_attribute_depends_map[elementKey] = depends;
                    // Wait to parse the child element before parsing itself
                    return;
                }
            }
            else
            {
                // xx
                HashSet<string> depends;
                current_result.dic_attribute_depends_map.TryGetValue(elementKey, out depends);
                Debug.Assert(depends != null);
                Debug.Assert(depends.Count != 0);
                depends.ExceptWith(lst_just_finished_sub_element_name);
                if (depends.Count == 0)
                {
                    long? tmp = mapContext.scriptInstance.GetScript(ScriptEnv.python).EvalExpression(s);
                    Debug.Assert(tmp != null, "Global variables are missing");
                    SetDefinedLength(tmp.GetValueOrDefault(0));
                    continue_get = false;
                    return;
                }
                else
                {
                    // continue waiting
                    return;
                }
            }
        }

        private void SetDefinedLength(long defined_length)
        {
            lengthValueType = LengthValueType.fixed_size_defined;
            this.defined_length_bits = (lengthUnit == LENGTH_UNIT.LENGTH_BIT)
                                       ? defined_length
                                       : ByteView.ConvertBytesCount2BitsCount(defined_length);
        }

        static List<String> GetVarsFromExpression(string expression)
        {
            HashSet<string> functionNames = new HashSet<string>() { "ceil", "pow", "mod", "select", "if", "abs", "prev", "this", "Math.", "this." };
            List<String> r = expression.Split(new char[] { '+', '-', '*', '/', '(', ')', ',', '^', '.' })
                .Select(item => item.Trim())
                .Distinct()
                .Where(item => item.Length > 0)
                .Where(item => functionNames.Contains(item) == false)
                .Where(item => Char.IsDigit(item[0]) == false).ToList();
            return r;
        }

        private List<string> get_sub_element_names(ElementBase elementBase)
        {
            List<String> names = new List<string>() { elementBase.name };
            foreach (FixedValue fixValue in elementBase.fixedValues)
            {
                names.Add(fixValue.name);
            }
            ElementStructure structure = elementBase as ElementStructure;    // Do not consider REF
            if (structure != null)
            {
                foreach (ElementBase subElement in structure.elements(false))
                {
                    names.AddRange(get_sub_element_names(subElement));
                }
            }
            return names;
        }
    }
}
