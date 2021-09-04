using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kernel
{
    /*
        Result objects are created during the structure mapping process.Depending on their type they 
        refer to a structure or struct element and a value.
    */


    [DebuggerDisplay("{name}")]
    public class Result : IEnumerable<Result>
    {
        public bool addSizeToEnclosing = false;
        public ElementBase elementBase = null;
        public Value value = new Value();
        public string matched_fixed_name = "";
        private string _name = "";
        public bool isMask = false;
        public string fillColor = "";
        public string strokeColor = "";
        public bool isFragment = true;
        public string value_from_valueexpression;
               
        public Result()
        {

        }

        public String name 
        {
            set
            {
                _name = value;
                this.value.name = value;
            }
            get => _name;
        }
        private List<Result> _results = new List<Result>();   // for structure

        public IReadOnlyList<Result> results => _results;
        public Result parent = null;
        public int level = 0;
        public long start_parse_bits_position;
        public int iteration = 0;
        public bool iteration_valided = false;
        public int index_in_structure = 0;
        public long start_parse_byte_position => ByteView.ConvertBitsCount2BytesCount(start_parse_bits_position);

        // Currently only the length field uses dic_attribute_depends_map, other fields are to be seen
        // If dic_attribute_depends_map does not contain key length, it means that it is the first time to analysis
        public Dictionary<string, HashSet<string>> dic_attribute_depends_map = new Dictionary<string, HashSet<string>>();


        public static Result CreateStructurePaddingResult(ByteView byteView, Result parent, int level)
        {
            Result r = new Result();
            r.level = level;
            r.value.byteView = byteView;
            r.value.SetContent(VALUE_TYPE.VALUE_TYPE_BINARY, byteView);
            r.start_parse_bits_position = byteView.index_of_bits;
            r.parent = parent;
            r.elementBase = ElementBinary.paddingElementBinary;
            if (r.parent.elementBase != null)
            {
                r.fillColor = r.parent.elementBase.GetValueDerivedWithDefault(ElementKey.fillcolor);
                r.strokeColor = r.parent.elementBase.GetValueDerivedWithDefault(ElementKey.strokecolor);
            }
            return r;
        }

        public string Name_UI()
        {
            string index = (iteration_valided || (elementBase is ElementStructure) || (elementBase is ElementStructureRef)) ? $"[{iteration}] " : "";
            string ret = $"{(name.Length > 0 ? name : elementBase.name)}{index}";
            return ret;
        }

        public string Value_UI()
        {
            if (string.IsNullOrEmpty(value_from_valueexpression) == false)
            {
                if (string.IsNullOrEmpty(matched_fixed_name) == false)
                {
                    return $"{value_from_valueexpression}: {matched_fixed_name}";
                }
                return value_from_valueexpression;
            }
            string ret = value.Value_UI(matched_fixed_name);
            return ret;
        }

        public string description_self()
        {
            string empty = "    ";
            string prefix = "";
            for (int i = 1; i < level; i++)
            {
                prefix += empty;
            }
            string id = (elementBase != null) ? elementBase.id + " " : "-";
            string index = (iteration_valided || (elementBase is ElementStructure) || (elementBase is ElementStructureRef)) ? $"[{iteration}] " : "";
            string ret = $"{value.bits_range()} {prefix}{id}{(name.Length > 0 ? name : elementBase.name)} {index}  {value.description(matched_fixed_name)}\n";
            return ret;
        }

        public string description()
        {
            string empty = "    ";
            string prefix = "";
            for (int i = 1; i < level; i++)
            {
                prefix += empty;
            }
            string id = (elementBase != null) ? elementBase.id + " " : "-";
            string index = (iteration_valided || (elementBase is ElementStructure) || (elementBase is ElementStructureRef)) ? $"[{iteration}] " : "";            
            string ret = $"{value.bits_range()} {prefix}{id}{(name.Length > 0 ? name : elementBase.name)} {index}  {value.description(matched_fixed_name)}\n";
            foreach (Result r in results)
            {
                ret += r.description();
            }
            return ret;
        }

        public string EmptyWithLevel()
        {
            string empty = "    ";
            string prefix = "";
            for (int i = 1; i < level; i++)
            {
                prefix += empty;
            }
            return prefix;
        }

        /*
            Each result has a value.
        */
        public Value getValue()
        {
            return value;
        }

        /*
           Mask results refer to a mask object.
        */
        public Mask getMask()
        {
            throw new NotImplementedException();
        }

        public ByteView getByteView()
        {
            return value.byteView;
        }






        /*
           Returns the level of a result in the results tree.
        */
        public int getLevel()
        {
            return level;
        }

        /*
           Returns the interation of a result in a sequence of repeated elements.
        */
        public int getIteration()
        {
            return iteration;
        }

        /*
          Returns the byte position of the result in the input file.
        */
        public Int64 getStartBytePos()
        {
            return start_parse_byte_position;
        }

        /*
          Returns the bit position of the result in the input file.
        */
        public Int64 getStartBitPos()
        {
            return start_parse_bits_position;
        }

        /*
          Returns the byte length of the result in the input file.
        */
        public Int64 getByteLength()
        {
            return value.byteView.count_of_bytes;
        }

        /*   
         *   Returns the bit length of the result in the input file.

        */
        public Int64 getBitLength()
        {
            return value.byteView.count_of_bits;
        }


        /*
          Returns the name of the result.
        */
        public String getName()
        {
            return name;
        }

        /*
          Returns the structure of the result.This is only valid if the result ist of type structure.

        */
        public ElementStructure getStructure()
        {
            return elementBase as ElementStructure;
        }

        /*
          Returns the structure elementof the result.This is only valid if the result ist of type structure.
        */
        public ElementBase getElement()
        {
            return elementBase;
        }


        /*
         * Modify byte array with new value.
            Parameters:
                value New value for result
        */
        public void update(Value value)
        {
            throw new NotImplementedException();
        }


        /*
        Returns type of a result.This can be RESULT_STRUCTURE_START_TYPE,
               RESULT_STRUCTURE_END_TYPE, RESULT_STRUCTURE_ELEMENT_TYPE,
               RESULT_MASK_TYPE or RESULT_MULTI.
        */
        public RESULT_TYPE getType()
        {
            throw new NotImplementedException();
        }

        IEnumerable<Result> ResultEnumerator()
        {
            yield return this;
            foreach (Result subItem in results)
            {                
                foreach(var r in subItem.ResultEnumerator())
                {
                    yield return r;
                }
            }
        }

        public IEnumerator<Result> GetEnumerator()
        {
            return ResultEnumerator().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ResultEnumerator().GetEnumerator();
        }

        public bool IsZeroCountOfBits()
        {
            if (this.value.count_of_bits > 0)
            {
                return false;
            }
            foreach(Result subResult in results)
            {
                if (subResult.IsZeroCountOfBits() == false)
                {
                    return false;
                }
            }
            return true;
        }

        public bool CanReverseFind()
        {
            VALUE_TYPE valueType = value.valueType;
            if (isMask)
            {
                return false;
            }
            if (value.count_of_bits == 0)
            {
                return false;
            }
            return ((valueType == VALUE_TYPE.VALUE_TYPE_BINARY)
                || (valueType == VALUE_TYPE.VALUE_TYPE_BOOLEAN)
                || (valueType == VALUE_TYPE.VALUE_TYPE_NUMBER_FLOAT)
                || (valueType == VALUE_TYPE.VALUE_TYPE_NUMBER_SIGNED)
                || (valueType == VALUE_TYPE.VALUE_TYPE_NUMBER_UNSIGNED)
                || (valueType == VALUE_TYPE.VALUE_TYPE_STRING));
        }

        public void AddSubResultFragment(Result result)
        {
            if (result == null)
            {
                return;
            }
            Debug.Assert(_results.Contains(result) == false);
            result.index_in_structure = results.Count;
            result.parent = this;
            _results.Add(result);

        }

        public void RemoveFragment()
        {
            IEnumerable<Result> fragments = _results.Where(r => r.isFragment);
            foreach (Result fragmentResult in fragments.ToList())
            {
                _results.Remove(fragmentResult);
            }

        }

        public void AddSubResult(Result result)
        {
            if (result == null)
            {
                return;
            }
            if (result.IsZeroCountOfBits())
            {
                return;
            }
            Debug.Assert(_results.Contains(result) == false);
            result.isFragment = false;
            result.index_in_structure = results.Count;
            result.parent = this;
            _results.Add(result);
        }

        public void AddSubResults(IEnumerable<Result> results)
        {
            foreach(var subResult in results)
            {
                AddSubResult(subResult);
            }
        }
        public void ClearSubResults()
        {
            _results.Clear();
        }

        public string GetErrorPath()
        {
            List<Result> pathList = new List<Result>();            
            for (Result tmp = this; tmp != null; tmp = tmp.parent)
            {
                pathList.Add(tmp);
            }
            pathList = pathList.Reverse<Result>().Where(r => (r.elementBase == null || (r.elementBase is ElementStructureRef == false))).ToList();
            
            string Path = String.Join(" > ", pathList.Select(r => r.Name_UI()));
            return Path;
        }

        public int CompareToInt64(Int64 toBeCompared)
        {
            if (value == null)
            {
                return 0;
            }

            Int64 innerValue = value.getSignedNumber();
            if (value_from_valueexpression != null)
            {
                innerValue = value_from_valueexpression.ToInt64(ToolsExtention.EnumStringFormatType.Decimal).GetValueOrDefault(0);
            }
            return innerValue.CompareTo(toBeCompared);
        }
    }
}
