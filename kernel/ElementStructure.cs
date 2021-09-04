using kernel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace kernel
{

    /*
      A structure object represents a structure in a grammar.

    */
    public class ElementStructure : ElementBase
    {        
        public override ELEMENT_TYPE GetElementType()
        {
            return ELEMENT_TYPE.ELEMENT_STRUCTURE;
        }

        private List<ElementBase> _subElements = new List<ElementBase>(); // structure        

        

        public void RemoveElement(ElementBase elementBase)
        {
            _subElements.Remove(elementBase);
        }

        public void AddElement(ElementBase elementBase)
        {
            _subElements.Add(elementBase);
        }

        public void InsertElement(int index, ElementBase elementBase)
        {
            _subElements.Insert(index, elementBase);
        }

        public IReadOnlyList<ElementBase> elements(bool containsDisabled)
        {            
            if (containsDisabled)
            {
                return _subElements;
            }
            return _subElements.Where(e => e.enabled).ToList();                            
        }

        public void ExchangeElementAtIndex(int index1, int index2)
        {
            var tmp = _subElements[index1];
            _subElements[index1] = _subElements[index2];
            _subElements[index2] = tmp;
        }

        public List<ElementStructure> ExtendsList()
        {
            List<ElementStructure> structures = new List<ElementStructure>();
            ElementStructure start = this;
            while (start.derived_from != null)
            {
                ElementStructure derivedFromStructure = derived_from as ElementStructure;
                structures.Add(derivedFromStructure);
                if (start == derivedFromStructure)
                {
                    break;
                }
                start = derivedFromStructure;
            }
            return structures;
        }


        public void AddSubElementFromFile(ElementBase element)
        {
            _subElements.Add(element);
        }

        private static void LoadedSubElementsFromDerivedForStructure(ElementStructure extends_to_structure, ElementStructure extend_from_structure)
        {
            
            if ((extends_to_structure == null) || (extend_from_structure == null))
            {
                return;
            }

            extends_to_structure.derived_from = extend_from_structure;

            List<ElementBase> extend_from_elements = new List<ElementBase>(extend_from_structure.elements(true));
            List<ElementBase> old_current_elements = new List<ElementBase>(extends_to_structure.elements(true));
            Dictionary<string, ElementBase> extend_from_dic = extend_from_elements.ToDictionary(e => e.name);
            Dictionary<string, ElementBase> old_current_dic = old_current_elements.ToDictionary(e => e.name);

            List<String> extend_from_names = extend_from_elements.Select(e => e.name).ToList();
            List<String> old_current_names = old_current_elements.Select(e => e.name).ToList();
            int derived_count = 0;
            List<string> joinedKeys = JoinKeys(extend_from_names, old_current_names, out derived_count);
            List<ElementBase> joinedElementBase = new List<ElementBase>();
            int element_index = 0;
            foreach (string name in joinedKeys)
            {
                ElementBase toBeAddedElementBase = null;
                if (old_current_dic.TryGetValue(name, out toBeAddedElementBase) == false)
                {
                    ElementBase extend_from_element_base = extend_from_dic[name];
                    toBeAddedElementBase = extends_to_structure.grammar.CreateDefaultElement(extend_from_element_base.GetElementType());
                    toBeAddedElementBase.grammar = extend_from_element_base.grammar;
                    toBeAddedElementBase.setName(name);
                    toBeAddedElementBase.parentStructure = extends_to_structure;
                }
                toBeAddedElementBase.derived_from = (element_index < derived_count) ? extend_from_dic[name] : null;
                joinedElementBase.Add(toBeAddedElementBase);

                element_index++;
            }
            extends_to_structure._subElements = joinedElementBase;


            // sub structure
            for(int i = 0; i < derived_count; i ++)
            {
                ElementStructure sub_structure = extends_to_structure._subElements[i] as ElementStructure;
                if (sub_structure == null)
                {
                    continue;
                }
                LoadedSubElementsFromDerivedForStructure(sub_structure, sub_structure.derived_from as ElementStructure);
            }
        }

        public void LoadedSubElementsFromDerived()
        {
            string extends_from = GetValue(ElementKey.extends);
            if (extends_from.Length == 0)
            {
                return;
            }
            ElementStructure extend_from_structure = grammar.GetStructureByIdWithPrefix(extends_from);
            LoadedSubElementsFromDerivedForStructure(this, extend_from_structure);
        }

        private static List<string> JoinKeys(List<string> parent, List<string> current, out int derived_count)
        {
            derived_count = 0;
            List<string> joined = new List<string>();
            while (parent.Count > 0 || current.Count > 0)
            {
                if (parent.Count == 0)
                {
                    joined.AddRange(current);
                    break;
                }
                else if (current.Count == 0)
                {
                    derived_count += parent.Count;
                    joined.AddRange(parent);
                    break;
                }

                string parent_first = parent.First();
                string current_first = current.First();

                if (parent_first == current_first)
                {
                    derived_count++;
                    joined.Add(parent_first);
                    parent.RemoveAt(0);
                    current.RemoveAt(0);
                }
                else
                {
                    if (current.Contains(parent_first))
                    {
                        joined.AddRange(current);
                        break;
                    }
                    else
                    {
                        derived_count++;
                        joined.Add(parent_first);
                        parent.RemoveAt(0);
                    }
                }
            }
            return joined;
        }

        public static void TestJoinKeys()
        {
            List<(string, string, string)> data = new List<(string, string, string)>()
            {
                ("","",""),
                ("1","","1"),
                ("1,2,3","","1,2,3"),
                ("1,2,3","1,2,3","1,2,3"),
                ("1,2,3","1,2,3,4","1,2,3,4"),
                ("1,2,3","2","1,2,3"),
                ("1,2,3","2,3","1,2,3"),
                ("1,2,3","2,3,4","1,2,3,4"),
                ("","1,2,3","1,2,3"),
                ("1,2,3","1,3","1,2,3"),
                ("1,2,3,4,5","2,4","1,2,3,4,5"),
                ("1,2,3,4,5","1","1,2,3,4,5"),
                ("1,2,3,4,5","1,2","1,2,3,4,5"),
                ("1,2,3,4,5","5","1,2,3,4,5"),
                ("1,2,3,4,5","7,8","1,2,3,4,5,7,8"),
                ("1,2,3,4,5","1,2,7,8","1,2,3,4,5,7,8"),
                ("1,2,3,4,5","1,3,2","1,3,2"),
                ("1,2,3,4,5","3,2","1,3,2"),
                ("1,2,3,4,5,6,7,8","3,5,7","1,2,3,4,5,6,7,8"),
                ("1,2,3,4,5,6,7,8","2,4,9,10,11,12","1,2,3,4,5,6,7,8,9,10,11,12"),
            };
            foreach(var dataItem in data)
            {
                var parent = dataItem.Item1.Split(',').Where(s => s.Length > 0).ToList();
                var current = dataItem.Item2.Split(',').Where(s => s.Length > 0).ToList();
                var result = dataItem.Item3.Split(',').Where(s => s.Length > 0).ToList();
                int derived_count = 0;
                var joined = JoinKeys(parent, current, out derived_count);
                Debug.Assert(joined.Count == result.Count);
                foreach(var i in Enumerable.Range(0, joined.Count))
                {
                    Debug.Assert(joined[i] == result[i]);
                }
            }
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

        // null: Use as much as possible
        // != null: Can only use this long






        //public override bool CheckFixValues(Result current_result, out string match_name)
        //{
        //    match_name = "";
        //    if (new HashSet<ElementBase>(elements).Except(current_result.results.Select(r => r.elementBase)).Count() != 0)            
        //    {
        //        return false;
        //    }

        //    foreach(var i in Enumerable.Range(0, current_result.results.Count))
        //    {
        //        Result subResult = current_result.results[i];
        //        if (subResult.elementBase.CheckFixValues(subResult, out match_name) == false)
        //        {
        //            return false;
        //        }
        //    }
        //    return true;
        //}

        private MapResult mapByteView_Fixed(ByteView byteView, Result parentResult, MapContext mapContext, string showName)
        {
            MapResult mapResult = base.mapByteView(byteView, parentResult, mapContext, showName);
            return mapResult;
        }

        private MapResult mapByteView_Variable(ByteView byteView, Result parentResult, MapContext mapContext, string showName)
        {
            Int64? repeat_fixed = repeat;
            long fixed_length_bits = lengthInBits(byteView.count_of_bits);
            long how_long_can_use_bits = (fixed_length_bits == 0) ? byteView.count_of_bits : fixed_length_bits;

            MapResult total_mapResult = new MapResult();

            Result current_varible_result = new Result()
            {
                elementBase = this,
                parent = parentResult,
                level = parentResult.level + 1,
                name = this.name,
                start_parse_bits_position = byteView.index_of_bits,
                fillColor = this.GetValueDerivedWithDefault(ElementKey.fillcolor),
                strokeColor = this.GetValueDerivedWithDefault(ElementKey.strokecolor),
                isFragment = false,
            };
            current_varible_result.value.valueType = VALUE_TYPE.VALUE_TYPE_STRUCTURE;

            Int64 repeat_min = repeatMin;
            Int64 repeat_max = repeatMax;
            long repeated_count = 0;

            for (Int64 repeat_index = 0; repeat_index < (repeat_fixed.HasValue ? repeat_fixed.Value : repeat_max); repeat_index++)
            {
                Func<(string, bool)> funcException = () => ($"parsing structure element({this.name}), path: {parentResult.GetErrorPath()} > {this.name}[{repeat_index}]", true);
                ByteView repeat_index_bytes_view = byteView.SkipBits(total_mapResult.used_bits, funcException)
                                                    .TakeBits(how_long_can_use_bits - total_mapResult.used_bits, funcException);
                if (repeat_index_bytes_view.count_of_bits <= 0)
                {
                    break;
                }

                Result alignment_padding_result = null;
                // padding
                if (this is ElementStructure)
                {
                    Int64 alignmentInBits = this.alignment_in_bits;
                    Int64 mod = repeat_index_bytes_view.index_of_bits % alignmentInBits;
                    if (mod != 0)
                    {
                        Int64 alignment_skip_bits_count = alignmentInBits - mod;
                        Func<(string, bool)> funcPaddingException = () => ($"adding padding to structure({this.name}), path: {parentResult.GetErrorPath()} > {this.name}[{repeat_index}]", true);
                        repeat_index_bytes_view = repeat_index_bytes_view.SkipBits(alignment_skip_bits_count, funcPaddingException);
                        alignment_padding_result = Result.CreateStructurePaddingResult(repeat_index_bytes_view.TakeBits(alignment_skip_bits_count, funcPaddingException),
                                                                                        parentResult,
                                                                                        (parentResult.elementBase is ElementStructureRef) ? parentResult.level : parentResult.level + 1);

                    }
                }


                Result current_result = new Result()
                {
                    elementBase = this,
                    parent = current_varible_result,
                    level = current_varible_result.level,
                    name = String.IsNullOrEmpty(showName) ? this.name : showName,
                    start_parse_bits_position = repeat_index_bytes_view.index_of_bits,
                    iteration = (int)repeat_index,
                    iteration_valided = (repeatMax > 1),
                    fillColor = this.GetValueDerivedWithDefault(ElementKey.fillcolor),
                    strokeColor = this.GetValueDerivedWithDefault(ElementKey.strokecolor),
                };
                AddFragmentBeforeBreak(current_varible_result, alignment_padding_result, current_result);
                MapResult mapResult = mapByteViewOnce(repeat_index_bytes_view, current_result, mapContext, showName);
                if (mapResult.Breaked())
                {
                    if (repeated_count < repeat_min)
                    {
                        total_mapResult.AppendResult(mapResult);
                    }
                    else
                    {
                        current_varible_result.RemoveFragment();
                    }
                    break;
                }

                if (mapResult.used_bits == 0)
                {
                    current_varible_result.RemoveFragment();
                    break;
                }

                Debug.Assert(current_result.value.byteView != null);


                SetScriptValues(current_result);

                current_varible_result.RemoveFragment();
                current_varible_result.AddSubResult(alignment_padding_result);

                current_varible_result.AddSubResults(current_result.results);
                if (alignment_padding_result != null)
                {
                    total_mapResult.AppendResult(MapResult.CreateWithLength(alignment_padding_result.value.byteView.count_of_bits));
                }
                total_mapResult.AppendResult(mapResult);
                repeated_count++;
            }
            if (total_mapResult.Breaked() == false && current_varible_result.IsZeroCountOfBits() == false)
            {
                current_varible_result.value.byteView = byteView.TakeBits(total_mapResult.used_bits, () => ($"parsing structure({this.name}), path: {current_varible_result.GetErrorPath()}", true));
                parentResult.AddSubResult(current_varible_result);
            }

            if (repeated_count < repeatMin)
            {
                total_mapResult.AppendResult(MapResult.CreateWithError(MapError.not_reach_repeat_count,
                    $"repeat count({repeated_count}) not reached to repeat min value({repeatMin}) for name {this.name}, path: {parentResult.GetErrorPath()}"));
            }

            if (repeat_fixed.HasValue && repeat_fixed.Value != repeated_count)
            {
                string repeat_fixed_error = $"repeat count({repeated_count}) not reached to repeat value({repeat_fixed.Value}) for name {this.name}, path: {parentResult.GetErrorPath()}";
                // total_mapResult.AppendResult(MapResult.CreateWithError(MapError.not_reach_repeat_count, repeat_fixed_error));
                mapContext.log.addLog(EnumLogLevel.warning, repeat_fixed_error);
            }

            return total_mapResult;

        }

        public override MapResult mapByteView(ByteView byteView, Result parentResult, MapContext mapContext, string showName)
        {
            bool parent_is_root_result = (parentResult.parent == null);
            this.mapContext = mapContext;
            if (orderType == ORDER_TYPE.FIXED)
            {
                return mapByteView_Fixed(byteView, parentResult, mapContext, showName);
            }
            else
            {
                return mapByteView_Variable(byteView, parentResult, mapContext, showName);                
            }
        }

        private void EvalValueExpressionForStructure(Result result, MapContext mapContext)
        {
            string valueExpression = this.valueExpression;
            if (valueExpression.Length == 0)
            {
                return;
            }

            long? value = mapContext.scriptInstance.GetScript(ScriptEnv.python).EvalExpression(valueExpression);
            if (value.HasValue)
            {
                result.value_from_valueexpression = value.GetValueOrDefault().ToString();
            }
            else
            {
                Result subResultMatched = result.results.Where(item => item.name == valueExpression).FirstOrDefault();
                if (subResultMatched != null)
                {
                    result.value_from_valueexpression = subResultMatched.value.Value_UI("");
                }
                else
                {
                    mapContext.log.addLog(EnumLogLevel.warning, $"Unable to calculate value expression({valueExpression}) for structure({result.name}), path: {result.GetErrorPath()}");
                }
            }
        }

        public MapResult mapByteViewOnce_OrderFixed(ByteView byteView, Result result, MapContext mapContext)
        {
            List<ElementBase> subElements = elements(false).ToList();
            StructLengthParser structLengthParser = new StructLengthParser(this.lengthUnit);
            List<string> must_fix_names = subElements.Where(e => e.fixedValues.Count > 0 && e.mustMatch).Select(e => e.name).ToList();
            List<string> lst_just_finished_sub_element_name = new List<string>();
            MapResult element_total_map_result = new MapResult();
            HashSet<ElementBase> must_match_elements = new HashSet<ElementBase>(subElements.Where(e => e.fixedValues.Count > 0 && e.mustMatch));
            List<Result> offsetResults = new List<Result>();            

            foreach (ElementBase elementBase in subElements)
            {
                structLengthParser.try_get_element_key_long(this, ElementKey.length, byteView.count_of_bits, mapContext, result, lst_just_finished_sub_element_name);
                long how_long_can_use_bits = structLengthParser.GetBitsLength(byteView.count_of_bits);
                if ((how_long_can_use_bits < element_total_map_result.used_bits) || (how_long_can_use_bits > byteView.count_of_bits))
                {
                    offsetResults.Clear();
                    result.ClearSubResults();
                    return MapResult.CreateWithError(MapError.not_enough_data, $"Not enough data while parsing element({elementBase.name}) in structure({this.name}), path: {result.GetErrorPath()}");
                }
                ByteView elementBaseByteView = byteView.SkipBits(element_total_map_result.used_bits, () => ($"parsing element({elementBase.name}) in structure({this.name}), path: {result.GetErrorPath()}", true))
                                                         .TakeBits(how_long_can_use_bits - element_total_map_result.used_bits, () => ($"parsing element({elementBase.name} in structure({this.name}), path: {result.GetErrorPath()}", true));
                if (elementBaseByteView.count_of_bits == 0)
                {
                    break;
                }
                List<Result> oldResults = new List<Result>(result.results);
                MapResult mapResult = elementBase.mapByteView(elementBaseByteView, result, mapContext, "");
                // Child element break, struct                    
                if (mapResult.Breaked())
                {
                    return mapResult;
                }
                if (elementBase.GetElementType() == ELEMENT_TYPE.ELEMENT_OFFSET)
                {
                    List<Result> currentResults = new List<Result>(result.results);
                    oldResults.ForEach(r => currentResults.Remove(r));
                    offsetResults.AddRange(currentResults);
                }
                element_total_map_result.AppendResult(mapResult);
                must_match_elements.Remove(elementBase);

                lst_just_finished_sub_element_name = get_sub_element_names(elementBase);
            }

            if (must_match_elements.Count != 0)
            {
                return MapResult.CreateWithError(MapError.must_match_failed, $"Must match assertion failed while parsing structure, path: {result.GetErrorPath()}");
            }

            structLengthParser.try_get_element_key_long(this, ElementKey.length, byteView.count_of_bits, mapContext, result, lst_just_finished_sub_element_name);

            

            // offset
            IEnumerable<ElementOffset> elementOffsets = elements(false).Where(e => e.GetElementType() == ELEMENT_TYPE.ELEMENT_OFFSET)
                                                                .Select(e => e as ElementOffset);

            foreach (Result offsetResult in offsetResults)
            {
                MapResult offsetMapResult = (offsetResult.elementBase as ElementOffset).ApplyOffsetStructure(offsetResult, mapContext);
                if (offsetMapResult.Breaked())
                {
                    return offsetMapResult;
                }
            }

            if (structLengthParser.continue_get == false)
            {
                if (structLengthParser.defined_length_bits > element_total_map_result.used_bits)
                {
                    Int64 padding_length = structLengthParser.defined_length_bits - element_total_map_result.used_bits;
                    Result paddingResult = Result.CreateStructurePaddingResult(
                        byteView.SkipBits(element_total_map_result.used_bits, () => ($"parsing structur({this.name}), path: {result.GetErrorPath()}", true))
                                .TakeBits(padding_length, () => ($"parsing structure({this.name}), path: {result.GetErrorPath()}", true)),
                                                        result,
                                                        result.level + 1);
                    result.AddSubResult(paddingResult);
                    element_total_map_result.used_bits = structLengthParser.defined_length_bits;
                }
            }
            EvalValueExpressionForStructure(result, mapContext);

            Debug.Assert(structLengthParser.continue_get == false);
            long final_bits_length = structLengthParser.GetBitsLength(element_total_map_result.used_bits);
            result.value.SetContent(VALUE_TYPE.VALUE_TYPE_STRUCTURE, byteView.TakeBits(final_bits_length, () => ($"parsing structure({this.name}), path: {result.GetErrorPath()}", true)));
            MapResult mapResult2 = MapResult.CreateWithLength(final_bits_length);
            return mapResult2;
        }

        public MapResult mapByteViewOnce_OrderVariable(ByteView byteView, Result result, MapContext mapContext)
        {
            MapResult element_map_result = new MapResult();
            bool matched = false;
            IReadOnlyList<ElementBase> subElements = elements(false);
            foreach (ElementBase elementBase in subElements)
            {
                result.RemoveFragment();
                result.elementBase = elementBase;
                if (byteView.count_of_bits == 0)
                {
                    break;
                }
                element_map_result = elementBase.mapByteView(byteView, result, mapContext, elementBase.name);
                if (element_map_result.used_bits == 0 || element_map_result.Breaked())
                {
                    continue;
                }
                else
                {
                }
                matched = true;
                break;
            }
            if (matched)
            {
                long final_bits_length = element_map_result.used_bits;
                Debug.Assert(final_bits_length != 0);
                result.value.SetContent(VALUE_TYPE.VALUE_TYPE_STRUCTURE, byteView.TakeBits(final_bits_length, () => ($"parsing structure({this.name}), path: {result.GetErrorPath()}", true)));
                return MapResult.CreateWithLength(final_bits_length);
            }
            else
            {
                return element_map_result;
            }
        }

        public override MapResult mapByteViewOnce(ByteView byteView, Result result, MapContext mapContext, string name)
        {
            MapResult mapResult = null;
            mapContext.parseStructureStack.Push((this, result, byteView));
            if (orderType == ORDER_TYPE.FIXED)
            {
                mapResult = mapByteViewOnce_OrderFixed(byteView, result, mapContext);
            }
            else
            {
                mapResult = mapByteViewOnce_OrderVariable(byteView, result, mapContext);                
            }
            mapContext.parseStructureStack.Pop();
            return mapResult;
        }

        public ElementStructure extendsElementStructure()
        {
            ElementStructure structure = grammar.GetStructureByIdWithPrefix(GetValue(ElementKey.extends));
            return structure;
        }

        public ElementStructure consistsOfElementStructure()
        {
            ElementStructure structure = grammar.GetStructureByIdWithPrefix(GetValue(ElementKey.consists_of));
            return structure;
        }

        public int getElementCount()
        {
            return elements(true).Count();
        }

        public ElementBase getElementByIndex(int index)
        {
            return elements(true)[index];
        }

        public ElementBase getElementByName(string name)
        {
            return elements(true).Where(e => e.name == name).FirstOrDefault();
        }

        public int appendElement(ElementBase element)
        {            
            _subElements.Add(element);
            return elements(true).Count;
        }

        public void insertElementAtIndex(ElementBase element, int index)
        {
            _subElements.Insert(index, element);            
        }

        public void deleteElementAtIndex(int index)
        {
            _subElements.RemoveAt(index);
        }
        
        public void RebuildSubElementDerivedFromConnection()
        {
            List<ElementBase> elementBaseToBeDeleted = new List<ElementBase>();
            foreach(ElementBase elementBase in _subElements)
            {                
                if (elementBase.IsTotalyDerivedContent())
                {
                    elementBaseToBeDeleted.Add(elementBase);
                }
                elementBase.derived_from = null;
            }

            foreach (ElementBase elementBase in elementBaseToBeDeleted)
            {
                _subElements.Remove(elementBase);                
            }

            LoadedSubElementsFromDerived();
        }
    }
}


