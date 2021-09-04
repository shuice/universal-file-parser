using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace kernel
{



    [DebuggerDisplay("Type:{this.getType()} name:{name} id:{id}")]
    public abstract class ElementBase
    {
        protected Dictionary<string, string> attributes = new Dictionary<string, string>();        

        public string description = "";
        private List<FixedValue> _fixedValues = new List<FixedValue>(); // binary || number || string
        public bool fixedValueDerivid = true;   // Do not write any fixed content to the file by default
        public bool maskValueDerived = true;
        private List<Mask> _masks = new List<Mask>(); // number
        public ElementStructure parentStructure = null;
        public Grammar grammar = null;        
        public string scriptContent = "";
        public string scriptLanguage = "";
        public MapContext mapContext = null;
        public bool create_from_script = false;
        public ElementBase derived_from = null;
        
        public bool is_derived => (derived_from != null);
        public bool enabled
        {
            get
            {
                string disableValue = GetValueDerivedWithDefault(ElementKey.disabled);
                return disableValue != "yes";
            }
            set
            {
                if (value)
                {
                    RemoveAttribute(ElementKey.disabled);
                }
                else
                {
                    SetAttribute(ElementKey.disabled, "yes");
                }
                
            }
        }

        public bool debug
        {
            get
            {
                string debugValue = GetValue(ElementKey.debug);
                return debugValue == "yes";                
            }
            set
            {
                if (value)
                {
                    SetAttribute(ElementKey.debug, "yes");
                }
                else
                {
                    RemoveAttribute(ElementKey.debug);
                }
            }
        }

        public string IdWithPrefix()
        {
            string idValue = id;
            Debug.Assert(idValue.Length > 0);
            return $"id:{idValue}";
        }

        public void ClearFixedValues()
        {
            _fixedValues.Clear();
        }

        public void ClearMaskValues()
        {
            _masks.Clear();
        }

        public void AddMaskValue(Mask mask)
        {
            _masks.Add(mask);
        }

        public void ReplaceMaskValue(int index, Mask mask)
        {
            _masks[index] = mask;
        }

        public void RemoveMaskAtIndex(int index)
        {
            _masks.RemoveAt(index);
        }

        public IReadOnlyList<Mask> masks
        {
            get
            {
                if (maskValueDerived == false)
                {
                    return _masks;
                }
                if (derived_from != null)
                {
                    return derived_from.masks;
                }
                return new List<Mask>();
            }
        }

        public IReadOnlyList<FixedValue> fixedValues
        {
            get
            {
                if (fixedValueDerivid == false)
                {
                    return _fixedValues;
                }
                if (derived_from != null)
                {
                    return derived_from.fixedValues;
                }
                return new List<FixedValue>();
            }
        }
        public IEnumerable<string> GetAttributeKeys()
        {
            return attributes.Keys;
        }

        public bool HasAttribute(string elementKey)
        {
            return attributes.ContainsKey(elementKey);
        }

        public string GetValue(string elementKey, string defaultValue = "")
        {
            string v;
            if (attributes.TryGetValue(elementKey, out v))
            {
                return v;
            }
            return defaultValue;
        }

        public void SetAttribute(string elementKey, string value)
        {
            attributes[elementKey] = value;
        }

        public void RemoveAttribute(string elementKey)
        {
            attributes.Remove(elementKey);
        }

        public void setName(string name)
        {
            attributes[ElementKey.name] = name;
        }

        public string getName()
        {
            return GetValue(ElementKey.name);
        }

        public string getDescription()
        {
            return description ?? "";
        }

        public void setDescription(string name)
        {
            description = name ?? "";
        }

        public void setDisabled(bool disabled)
        {
            this.enabled = !disabled;
        }

        public void setLength(String length, string strLengthUnit)
        {
            SetAttribute(ElementKey.length, length);
            LENGTH_UNIT lu = String2Enum.translateStringToLengthUnit(strLengthUnit);
            if (lu != LENGTH_UNIT.LENGTH_UNSPECIFIED)
            {
                SetAttribute(ElementKey.lengthunit, strLengthUnit);
            }
        }

        public void setRepeatMin(String repeatMin)
        {
            SetAttribute(ElementKey.repeatmin, repeatMin);
        }
        public void setRepeatMax(String repeatMax)
        {
            SetAttribute(ElementKey.repeatmax, repeatMax);
        }

        public void setDefaultEncoding(String defaultEncoding)
        {
            SetAttribute(ElementKey.encoding, defaultEncoding);
        }

        public String getDefaultEncoding()
        {
            return GetValue(ElementKey.encoding);
        }

        public void setElementOrder(string strOrder)
        {
            ORDER_TYPE orderType = String2Enum.translateStringToOrderType(strOrder);
            if (orderType != ORDER_TYPE.UNKNOWN)
            {
                SetAttribute(ElementKey.order, strOrder);
            }

        }

        public void AddFixedValue(FixedValue fixedValue)
        {
            _fixedValues.Add(fixedValue);
        }
        public void RemoveFixedValueAt(int index)
        {
            _fixedValues.RemoveAt(index);
        }

        public void SetFixedValueAtIndex(int index, string name, string value)
        {
            _fixedValues[index].name = name;
            _fixedValues[index].value = value;
        }


        public void addFixedValue(Value value)
        {
            // in pdf document
            // Add fixed value to element.
            throw new NotImplementedException();
        }

        public ElementBase getEnclosingStructure()
        {
            return this.parentStructure;
        }

        public String getLength()
        {
            // in document
            // Get length of element. For binary or string elements a length of zero means to fill the enclosing
            // structure.Be aware that lengths can be fractions of bytes so call additionally getLengthUnit()
            throw new NotImplementedException();
        }

        // script does not recognize LENGTH_UNIT type
        public string getLengthUnit()
        {
            return this.lengthUnit.ToString();
        }

        public void setColorRgb(float red, float green, float blue)
        {
            int r =  Math.Max(Math.Min(((int)red * 255), 255), 0);
            int g = Math.Max(Math.Min(((int)green * 255), 255), 0);
            int b = Math.Max(Math.Min(((int)blue * 255), 255), 0);
            string rgbValue = String.Format("{0:X2}{1:X2}{2:X2}", r, g, b);
            SetAttribute(ElementKey.fillcolor, rgbValue);
        }

        public string getType()
        {
            return this.GetElementType().ToString();
        }

        public Value getMinValue()
        {
            Value value = Value.CreateValueDirectly();
            value.setSigned(this.minVal.GetValueOrDefault(0));
            return value;
        }

        public Value getMaxValue()
        {
            Value value = Value.CreateValueDirectly();
            value.setSigned(this.maxVal.GetValueOrDefault(0));
            return value;
        }
        public ElementBase getParent()
        {
            return parentStructure;
        }

        public void setAdditionalOffset(String additionalOffset)
        {
            throw new NotImplementedException();
        }

        public Grammar getGrammar()
        {
            return grammar;
        }
        public void setAlignment(int alignment)
        {
            SetAttribute(ElementKey.alignment, alignment.ToString());
        }

        public int getAlignment()
        {
            return (int)ByteView.ConvertBitsCount2BytesCount(alignment_in_bits);
        }


        public ElementStructure getStructure()
        {
            return parentStructure;
        }


        /*
        Get type of element.This can be one of
                • ELEMENT_BINARY
                • ELEMENT_GRAMMAR_REF
                • ELEMENT_NUMBER
                • ELEMENT_STRING
                • ELEMENT_OFFSET
                • ELEMENT_SCRIPT
                • ELEMENT_STRUCTURE
                • ELEMENT_STRUCTURE_REF
        */
        public abstract ELEMENT_TYPE GetElementType();

        public string GetElementTypeUI()
        {
            Dictionary<ELEMENT_TYPE, string> dicType2String = new Dictionary<ELEMENT_TYPE, string>
            {
                { ELEMENT_TYPE.ELEMENT_BINARY, "Binary" },
                { ELEMENT_TYPE.ELEMENT_CUSTOM, "Custom" },
                { ELEMENT_TYPE.ELEMENT_GRAMMAR_REF, "Grammar Reference" },
                { ELEMENT_TYPE.ELEMENT_NUMBER, "Number" },
                { ELEMENT_TYPE.ELEMENT_STRING, "String" },
                { ELEMENT_TYPE.ELEMENT_OFFSET, "Offset" },
                { ELEMENT_TYPE.ELEMENT_SCRIPT, "Script" },
                { ELEMENT_TYPE.ELEMENT_STRUCTURE, "Structure" },
                { ELEMENT_TYPE.ELEMENT_STRUCTURE_REF, "Structure Reference"},
            };
            ELEMENT_TYPE type = GetElementType();
            string typeString = "";
            dicType2String.TryGetValue(type, out typeString);
            return typeString ?? "";
        }


        public string GetValueDerivedWithDefault(string elementKey, string defaultValue = "")
        {
            string value = GetValueDerived(elementKey);            
            if (value == string.Empty)
            {
                return defaultValue;
            }
            return value;
        }

        private string GetValueDerived(string elementKey)
        {
            string elementValue = "";
            if (attributes.TryGetValue(elementKey, out elementValue))
            {
                return elementValue ?? "";
            }

            ElementStructure elementStructure = this as ElementStructure;
            ElementStructureRef elementStructureRef = this as ElementStructureRef;

            if (derived_from != null)
            {
                return derived_from.GetValueDerived(elementKey);
            }
            else
            {
                if ((elementKey == ElementKey.endian) || (elementKey == ElementKey.encoding) || (elementKey == ElementKey.signed) || (elementKey == ElementKey.debug))
                {
                    if (parentStructure != null)
                    {
                        return parentStructure.GetValueDerived(elementKey);
                    }
                }
                return String.Empty;
            }
        }

        public int idValue
        {
            get
            {
                int v = 0;
                int.TryParse(id, out v);
                return v;
            }
        }
       
        public string id
        {
            get => GetValue(ElementKey.id);
        }


        public string name
        {
            get => GetValue(ElementKey.name);
        }


        public Int64 repeatMin
        {
            get
            {
                string repeat_min = GetValueDerivedWithDefault(ElementKey.repeatmin);
                return ConvertString2Int64(repeat_min).GetValueOrDefault(1);
            }
        }

        public Int64 repeatMax
        {
            get
            {
                string repeat_max = GetValueDerivedWithDefault(ElementKey.repeatmax);                
                Int64 v = ConvertString2Int64(repeat_max).GetValueOrDefault(1);
                if (v == -1)
                {
                    v = Int64.MaxValue;
                }
                return v;
            }
        }

        public Int64? repeat
        {
            get
            {
                string repeat = GetValueDerivedWithDefault(ElementKey.repeat);
                if (repeat.StartsWith("id:") == false)
                {
                    return null;
                }                
                ElementBase elementBase = this.grammar.GetElementByIdWithPrefix(repeat);
                Debug.Assert(elementBase != null);
                if (elementBase == null)
                {
                    return null;
                }

                if (this.parentStructure == null)
                {
                    return null;
                }

                // repeat Must be of the same level, has been resolved, not in the nephew sector
                List<ElementBase> enabledElements = this.parentStructure.elements(false).ToList();
                int i1 = enabledElements.IndexOf(elementBase);
                int i2 = enabledElements.IndexOf(this);
                if ((i1 == -1) || (i2 == -1) || (i1 >= i2))
                {
                    return null;                
                }

                Result parent_result = mapContext.results.getLastResult().parent;
                if (parent_result != null)
                {
                    Result matched_sub_result = parent_result.results.Reverse().FirstOrDefault(r => r.elementBase == elementBase);
                    if (matched_sub_result != null)
                    {
                        Int64 v = matched_sub_result.value.ToInt64FromValue();
                        return v;
                    }
                }
                return null;
            }
        }

        NUMBER_TYPE numberType
        {
            get
            {
                string number_type = GetValueDerivedWithDefault(ElementKey.type);
                return String2Enum.translateStringToNumberType(number_type);
            }
        }

        public LENGTH_UNIT lengthUnit
        {
            get
            {
                string length_unit = GetValueDerivedWithDefault(ElementKey.lengthunit);
                return String2Enum.translateStringToLengthUnit(length_unit);
            }
        }

        public ENDIAN_TYPE endianType
        {
            get
            {
                string endian_type = GetValueDerivedWithDefault(ElementKey.endian);
                return String2Enum.translateStringToEndianType(endian_type);
            }
        }

        public NUMBER_DISPLAY_TYPE numberDisplayType
        {
            get
            {
                string number_display_type = GetValueDerivedWithDefault(ElementKey.display);
                return String2Enum.translateStringToNumberDisplayType(number_display_type);
            }
        }
        public long? minVal
        {
            get
            {
                string min_val = GetValueDerivedWithDefault(ElementKey.minval);
                return ConvertString2Int64(min_val);
            }
        }

        public long? maxVal
        {
            get
            {
                string max_val = GetValueDerivedWithDefault(ElementKey.maxval);
                return ConvertString2Int64(max_val);
            }
        }

        public bool mustMatch
        {
            get
            {
                string mustMatch = GetValueDerivedWithDefault(ElementKey.mustmatch);
                return "yes" == mustMatch;
            }
            set
            {
                SetAttribute(ElementKey.mustmatch, value ? "yes" : "no");
            }
        }

        public string valueExpression
        {
            get => GetValueDerivedWithDefault(ElementKey.valueexpression);
        }

        public Int64 alignment_in_bits
        {
            get
            {
                string string_alignment = GetValueDerivedWithDefault(ElementKey.alignment);
                Int64 alignment = ConvertString2Int64(string_alignment).GetValueOrDefault(0);
                alignment = ByteView.ConvertBytesCount2BitsCount(alignment);
                if (alignment == 0)
                {
                    alignment = 1;
                }
                return alignment;
            }
        }


        public STRING_LENGTH_TYPE stringLengthType
        {
            get
            {
                string string_length_type = GetValueDerivedWithDefault(ElementKey.type);
                return String2Enum.translateStringToStringLengthType(string_length_type);
            }
        }

        public string delimiter
        {
            get
            {
                string string_length_type = GetValueDerivedWithDefault(ElementKey.delimiter);
                return string_length_type;
            }
        }

        
        private Int64 lengthWithoutUnit(Int64 remainBits, out bool isRemaining)
        {
            string s = GetValueDerivedWithDefault(ElementKey.length);
            isRemaining = (s == "remaining");
            if (isRemaining)
            {
                return remainBits;
            }
            return ConvertString2Int64(s).GetValueOrDefault(0);
        }

        public Int64 lengthInBits(Int64 remainBits)
        {
            bool isRemaining = false;
            Int64 i = lengthWithoutUnit(remainBits, out isRemaining);
            if (isRemaining)
            {
                return i;
            }
            bool length_unit_is_bits = (this.lengthUnit == LENGTH_UNIT.LENGTH_BIT);
            if (length_unit_is_bits)
            {
                return i;
            }
            else
            {
                return ByteView.ConvertBytesCount2BitsCount(i);
            }
        }

        public Int64 lengthInBytes(Int64 lengthInBits) => ByteView.ConvertBitsCount2BytesCount(lengthInBits);




        public ORDER_TYPE orderType
        {
            get
            {
                Debug.Assert(this is ElementStructure || this is ElementStructureRef);
                string order_type = GetValueDerivedWithDefault(ElementKey.order);
                return String2Enum.translateStringToOrderType(order_type);
            }
        }

        public string GetErrorPath(Result parentResult, Int64 repeat_index)
        {
            string parent_path = parentResult.GetErrorPath();
            return ((parent_path.Length == 0) ? $"" : (parent_path + " > ")) + this.name + $"[repeat_index]";
        }

        public void AddFragmentBeforeBreak(Result parentResult, Result paddingFragment, Result fragment)
        {
            if ((false == (this is ElementCustom)) && (false == (this is ElementScript)))
            {
                parentResult.AddSubResultFragment(paddingFragment);
                parentResult.AddSubResultFragment(fragment);                
            }
        }
        public string EmptyWithLevel(int level)
        {
            string empty = "    ";
            string prefix = "";
            for (int i = 1; i < level; i++)
            {
                prefix += empty;
            }
            return prefix;
        }

        private void EvalValueExpressionForNumber(Result current_result, MapContext mapContext)
        {
            if ((this is ElementNumber) == false)
            {
                return;
            }
            string valueExpression = this.valueExpression;
            if (valueExpression.Length == 0)
            {
                return;
            }

            long? value = mapContext.scriptInstance.GetScript(ScriptEnv.python).EvalExpression(valueExpression);
            if (value.HasValue == false)
            {
                mapContext.log.addLog(EnumLogLevel.warning, $"Unable to calculate value expression({valueExpression}) for number({current_result.name}), path: {current_result.GetErrorPath()}");
            }
            current_result.value_from_valueexpression = value.GetValueOrDefault(0).ToString();
        }


        public virtual MapResult mapByteView(ByteView byteView, Result parentResult, MapContext mapContext, string showName)
        {
            this.mapContext = mapContext;

            Int64 repeat_min = repeatMin;            
            Int64 repeat_max = repeatMax;
            Int64? repeat_fixed = repeat;
            Int64 repeated_count = 0;

            MapResult total_mapResult = new MapResult();


            if (parentResult.parent == null)
            {
                // is root 
                //Debug.Assert(repeat_max == 1);
            }
            for (Int64 repeat_index = 0; repeat_index < (repeat_fixed.HasValue ? repeat_fixed.Value : repeat_max); repeat_index++)
            {
                Func<(string, bool)> funcPaddingException = () => ($"parsing element({this.name}), path: {GetErrorPath(parentResult, repeat_index)}", true);
                ByteView repeat_index_bytes_view = byteView.SkipBits(total_mapResult.used_bits, funcPaddingException);
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
                        Func<(string, bool)> funcException = () => ($"adding padding to structure({this.name}), path: {GetErrorPath(parentResult, repeat_index)}", true);
                        repeat_index_bytes_view = repeat_index_bytes_view.SkipBits(alignment_skip_bits_count, funcException );
                        alignment_padding_result = Result.CreateStructurePaddingResult(repeat_index_bytes_view.TakeBits(alignment_skip_bits_count, funcException),
                                                                                        parentResult,
                                                                                        (parentResult.elementBase is ElementStructureRef) ? parentResult.level : parentResult.level + 1);

                    }
                }


                Result current_result = new Result()
                {
                    elementBase = this,
                    parent = parentResult,
                    //level = (parentResult.elementBase is ElementStructureRef) ? parentResult.level : parentResult.level + 1,
                    level = parentResult.level + 1,
                    name = String.IsNullOrEmpty(showName) ? this.name : showName,
                    start_parse_bits_position = repeat_index_bytes_view.index_of_bits,
                    iteration = (int)repeat_index,
                    iteration_valided = (repeatMax > 1),
                    fillColor = this.GetValueDerivedWithDefault(ElementKey.fillcolor),
                    strokeColor = this.GetValueDerivedWithDefault(ElementKey.strokecolor),
                };

                mapContext.parseElementStack.Push((this, current_result, repeat_index_bytes_view));
                AddFragmentBeforeBreak(parentResult, alignment_padding_result, current_result);

                MapResult mapResult = MapResult.CreateWithLength(0);
                try
                {
                    mapResult = mapByteViewOnce(repeat_index_bytes_view, current_result, mapContext, showName);
                }
                catch(MapException mapException)
                {
                    mapResult = MapResult.CreateWithError(MapError.not_enough_data, mapException.Message);
                }
                mapContext.parseElementStack.Pop();
                if (mapResult.Breaked())
                {
                    if (repeated_count < repeat_min)
                    {
                        total_mapResult.AppendResult(mapResult);
                    }
                    else
                    {
                        parentResult.RemoveFragment();
                    }
                    break;
                }

                // Possible scenario
                // 1. It is currently a script, it must be a break                
                // 2. It may be a repeatmin==0, and there happens to be no matching structure
                if (mapResult.used_bits == 0)
                {
                    if (this is ElementScript)
                    {
                        break;                        
                    }
                    //if ((this is ElementStructure) || (this is ElementStructureRef))
                    //{
                    //    if (current_result.IsZeroCountOfBits())
                    //    {
                    //        break;                            
                    //    }
                    //}
                }

                SetScriptValues(current_result);
                EvalValueExpressionForNumber(current_result, mapContext);

                string match_name = "";
                List <(Func<bool> chekFunc, MapError mapError, Func<string> errorMsgFunc)> minMaxFixeds = new List<(Func<bool>, MapError, Func<string>)>
                {
                    (()=> CheckFixValues(current_result, out match_name),   MapError.must_match_failed, ()=>createMustMatchErrorString(current_result)),
                    (()=> CheckMinLimit(current_result, minVal),            MapError.min_max_limit_error, ()=>createMinLimitErrorString(current_result)),
                    (()=> CheckMaxLimit(current_result, maxVal),            MapError.must_match_failed, ()=>createMaxLimitErrorString(current_result)),
                };
                bool breaked = false;
                foreach(var minMaxFixedItem in minMaxFixeds)
                {
                    if (minMaxFixedItem.chekFunc() == false)
                    {
                        if (mustMatch)
                        {
                            if (repeated_count < repeat_min)
                            {
                                total_mapResult.AppendResult(MapResult.CreateWithError(minMaxFixedItem.mapError, minMaxFixedItem.errorMsgFunc()));
                            }
                            else
                            {
                                parentResult.RemoveFragment();
                            }
                            breaked = true;
                            break;
                        }
                    }
                }
                if (breaked)
                {
                    break;
                }

                current_result.matched_fixed_name = match_name;

                


                if ((this is ElementScript) == false)
                {
                    Debug.Assert(current_result.value.byteView != null);
                }

  
                

                parentResult.RemoveFragment();
                parentResult.AddSubResult(alignment_padding_result);
                
                if ((false == (this is ElementCustom)) && (false == (this is ElementScript)))
                {
                    Result trimedResult = trimStructureOrGrammraRef(current_result);
                    if (trimedResult != null)
                    {
                        mapContext.results.setLastResult(trimedResult);
                        parentResult.AddSubResult(trimedResult);
                        if (alignment_padding_result != null)
                        {
                            total_mapResult.AppendResult(MapResult.CreateWithLength(alignment_padding_result.value.byteView.count_of_bits));
                        }
                    }
                }
                
                total_mapResult.AppendResult(mapResult);
                repeated_count++;
            }

            if ((this is not ElementScript))
            {
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
            }
            return total_mapResult;
        }

        public string createMustMatchErrorString(Result current_result)
        {
            String value_ui = current_result.value.Value_UI("");
            String can_be_matched_value = String.Join("\n\t\t", fixedValues.Select(v => v.value));
            return $"Fixed value not matched for element({current_result.name}), path: {current_result.GetErrorPath()}, parsed value: {value_ui}, to be matched value: {can_be_matched_value}";
        }

        public string createMinLimitErrorString(Result current_result)
        {
            return $"Max value limit error for element({current_result.name}), path: {current_result.GetErrorPath()}, parsed value: {current_result.Value_UI()}, min value: {minVal}";
        }

        public string createMaxLimitErrorString(Result current_result)
        {
            return $"Max value limit error for element({current_result.name}), path: {current_result.GetErrorPath()}, parsed value: {current_result.Value_UI()}, max value: {maxVal}";
        }

        private Result trimStructureOrGrammraRef(Result result)
        {
            if (this is ElementStructureRef || this is ElementGrammarRef)
            {
                if (result.results.Count == 0)
                {
                    return null;
                }
                Debug.Assert(result.results.Count == 1);
                Result trimedStructure = result.results[0];
                foreach (Result r in trimedStructure)
                {
                    r.level -= 1;
                }
                trimedStructure.name = result.name;
                return trimedStructure;
            }
            return result;
        }

        public void SetScriptValues(Result result)
        {
            Value value = result.value;            
            if (value.valueType == VALUE_TYPE.VALUE_TYPE_NUMBER_SIGNED)
            {
                mapContext.scriptInstance.SetValue(result.name, value.ToInt64FromValue());
            }
            else if (value.valueType == VALUE_TYPE.VALUE_TYPE_NUMBER_UNSIGNED)
            {
                mapContext.scriptInstance.SetValue(result.name, value.ToUInt64FromValue());
            }
            else if (value.valueType == VALUE_TYPE.VALUE_TYPE_NUMBER_FLOAT)
            {
                mapContext.scriptInstance.SetValue(result.name, (long)value.ToDoubleFromValue());
            }
            else if (value.valueType == VALUE_TYPE.VALUE_TYPE_STRING)
            {
                mapContext.scriptInstance.SetValue(result.name, ConvertString2Int64(value.ToStringFromValue(), false).GetValueOrDefault(0));
            }
        }

        private bool CheckMinLimit(Result current_result, long? minValue)
        {
            if ((this is ElementNumber) == false)
            {
                return true;
            }
            if (minVal != null)
            {
                if (current_result.CompareToInt64(minVal.Value) == -1)
                {
                    return false;
                }
            }           
            return true;
        }

        private bool CheckMaxLimit(Result current_result, long? maxValue)
        {
            if ((this is ElementNumber) == false)
            {
                return true;
            }   
            if (maxValue != null)
            {
                if (current_result.CompareToInt64(maxValue.Value) == 1)
                {
                    return false;
                }
            }
            return true;
        }


        public virtual bool CheckFixValuesWithValueFromExpression(string value_from_valueexpression, out string match_name)
        {
            Debug.Assert(this is ElementNumber);
            match_name = "";
            if (fixedValues.Count == 0)
            {
                return true;
            }
            long long_value_from_valueexpression = value_from_valueexpression.ToInt64(ToolsExtention.EnumStringFormatType.Decimal).GetValueOrDefault(0);

            bool must_match = this.mustMatch;
            List<MapError> mapErrors = new List<MapError>();
            FixedValue fixedValue = fixedValues.FirstOrDefault(fixValue => fixValue.value.ToInt64().GetValueOrDefault(0) == long_value_from_valueexpression);
            if (must_match && fixedValue == null)
            {
                return false;
            }
            if (fixedValue != null)
            {
                match_name = fixedValue.name;
            }
            return true;
        }

        public virtual bool CheckFixValues(Result current_result, out string match_name)
        {
            match_name = "";


            if ((this is not ElementNumber) && (this is not ElementString) && (this is not ElementBinary))
            {
                Debug.Assert(fixedValues.Count == 0);
                return true;
            }

            if (fixedValues.Count == 0)
            {
                return true;
            }

            if (current_result.value_from_valueexpression != null)
            {
                return CheckFixValuesWithValueFromExpression(current_result.value_from_valueexpression, out match_name);
            }

                       
            bool must_match = this.mustMatch;
            List<MapError> mapErrors = new List<MapError>();
            FixedValue fixedValue = fixedValues.FirstOrDefault(fixValue => current_result.value.Math(fixValue.value));
            if (must_match && fixedValue == null)
            {
                return false;
            }
            if (fixedValue != null)
            {
                match_name = fixedValue.name;
            }
            return true;
        }

        public Int64? ConvertString2Int64(String s,  bool check_express = true)
        {
            if (s.Length == 0)
            {
                return null;
            }
            Debug.Assert(s != "remaining");                
            
            if (s == "unlimited")
            {
                return Int64.MaxValue;
            }
            
            Int64 r = 0;
            if (Int64.TryParse(s, out r))
            {
                return r;
            }
            if (s.ToLower().StartsWith("0x"))
            {
                if (Int64.TryParse(s.Substring(2), NumberStyles.HexNumber, null, out r))
                {
                    return r;
                }
            }

            if (check_express)
            {
                return mapContext.scriptInstance.GetScript(ScriptEnv.python).EvalExpression(s);
            }
            return null;
        }

        
        public abstract MapResult mapByteViewOnce(ByteView byteView, Result result, MapContext mapContext, string showName);

        private bool IsValueSameAsNotExist(string attributeName, string attributeValue)
        {
            string defaultValue;
            if (ElementKey.dicDefaultAttributeName2Value.TryGetValue(attributeName, out defaultValue))
            {
                if (defaultValue == attributeValue)
                {
                    return true;
                }
            }
            return false;
        }

        private void DeleteUnusedAttributeForString()
        {
            if (this is not ElementString)
            {
                return;
            }

            string type = GetValue(ElementKey.type);
            if (type.Length == 0)
            {
                return;
            }

            // length
            // delimiter
            STRING_LENGTH_TYPE eType = String2Enum.translateStringToStringLengthType(type);
            if (eType == STRING_LENGTH_TYPE.STRING_LENGTH_DELIMITER_TERMINATED)
            {                
                this.RemoveAttribute(ElementKey.length);
            }
            else if (eType == STRING_LENGTH_TYPE.STRING_LENGTH_FIXED)
            {
                this.RemoveAttribute(ElementKey.delimiter);
            }
            else if(eType == STRING_LENGTH_TYPE.STRING_LENGTH_ZERO_TERMINATED)
            {
                this.RemoveAttribute(ElementKey.delimiter);
                this.RemoveAttribute(ElementKey.length);
            }
            else if (eType == STRING_LENGTH_TYPE.STRING_LENGTH_PASCAL)
            {
                this.RemoveAttribute(ElementKey.delimiter);
                string length = GetValue(ElementKey.length);
                if (length.Length == 0)
                {
                    this.SetAttribute(ElementKey.length, "remaining");
                }
            }

        }

        public void SaveToXmlElement(XmlDocument xmlDocument, XmlElement xmlElement)
        {
            DeleteUnusedAttributeForString();
            foreach (var sortedKey in attributes.Keys.ToList().OrderBy(item => item))
            {
                string attributeValue = attributes[sortedKey];
                if (IsValueSameAsNotExist(sortedKey, attributeValue) == false)
                {
                    xmlElement.SetAttribute(sortedKey, attributeValue);
                }
            }

            if (String.IsNullOrEmpty(description) == false)
            {
                XmlElement descriptionXmlElement = XmlHelper.AddChildNode(xmlDocument, xmlElement, "description");
                descriptionXmlElement.InnerText = description.Replace("\r\n", "\n").Replace('\r', '\n');
            }

            if ((this is ElementBinary) || (this is ElementNumber) || (this is ElementString))
            {
                if (fixedValueDerivid == false)
                {
                    if (_fixedValues.Count > 0)
                    {
                        XmlElement fixedValuesXmlElement = XmlHelper.AddChildNode(xmlDocument, xmlElement, "fixedvalues");
                        SaveXmlNodeFixedValues(xmlDocument, fixedValuesXmlElement, _fixedValues);
                    }
                    else  if (this.is_derived)
                    {
                        XmlElement fixedValuesXmlElement = XmlHelper.AddChildNode(xmlDocument, xmlElement, "fixedvalues");
                    }                    
                }
            }
            if (this is ElementNumber)
            {
                if (maskValueDerived == false)
                {
                    if (_masks.Count == 0)
                    {
                        // Indicates not to derive
                        XmlHelper.AddChildNode(xmlDocument, xmlElement, "mask");
                    }
                    else
                    {
                        foreach (Mask mask in _masks)
                        {
                            XmlElement maskXmlElement = XmlHelper.AddChildNode(xmlDocument, xmlElement, "mask");
                            SaveXmlChildNodeMaskValue(xmlDocument, maskXmlElement, mask);
                        }
                    }
                }
            }


            ElementStructure elementStructure = this as ElementStructure;
            if (elementStructure != null)
            {
                foreach (ElementBase elementBase in elementStructure.elements(true))
                {
                    XmlElement child = XmlHelper.AddChildNode(xmlDocument, xmlElement, ElementFactory.GetXmlNodeNameByElement(elementBase));
                    elementBase.SaveToXmlElement(xmlDocument, child);
                }
            
            }
            ElementScript elementScript = this as ElementScript;
            if (elementScript != null)
            {
                XmlElement scriptNode = XmlHelper.AddChildNode(xmlDocument, xmlElement, "script");
                XmlElement sourceNode = XmlHelper.AddChildNode(xmlDocument, scriptNode, "source");
                sourceNode.SetAttribute("language", scriptLanguage);
                sourceNode.InnerText = this.scriptContent.Replace("\r\n", "\n").Replace('\r', '\n');
            }
        }

        public void LoadFromXmlElement(XmlElement xmlElement)
        {
            foreach(XmlAttribute attribute in xmlElement.Attributes)
            {
                attributes[attribute.Name] = attribute.Value;
            }
            XmlElement descriptionXmlElement = GetChildNodesByName(xmlElement, "description").FirstOrDefault();
            if (descriptionXmlElement != null)
            {
                description = descriptionXmlElement.InnerText;
            }
            XmlElement fixedValuesXmlElement = GetChildNodesByName(xmlElement, "fixedvalues").FirstOrDefault();
            this.fixedValueDerivid = (fixedValuesXmlElement == null);
            if (fixedValuesXmlElement != null)
            {
                _fixedValues = ParseXmlNodeFixedValues(fixedValuesXmlElement);
            }
            List<XmlElement> maskXmlElements = GetChildNodesByName(xmlElement, "mask");
            this.maskValueDerived = (maskXmlElements.Count == 0);
            foreach (XmlElement maskXmlElement in maskXmlElements)
            {
                Mask mask = ParseXmlChildNodeMaskValue(maskXmlElement);
                if (mask != null)
                {
                    _masks.Add(mask);
                }
            }

            ElementStructure elementStructure = this as ElementStructure;
            if (elementStructure != null)
            {
                foreach (XmlNode child in xmlElement.ChildNodes)
                {
                    XmlElement childXmlElement = child as XmlElement;
                    if (childXmlElement == null)
                    {
                        continue;
                    }
                    ElementBase elementBase = ElementFactory.CreateElementByName(childXmlElement.Name);
                    if (elementBase != null)
                    {                        
                        elementBase.grammar = this.grammar;
                        elementBase.parentStructure = elementStructure;
                        elementBase.LoadFromXmlElement(childXmlElement);
                        elementStructure.AddSubElementFromFile(elementBase);

                        grammar.MakesureHasId(elementBase);
                    }
                }
            }
            ElementScript elementScript = this as ElementScript;
            if (elementScript != null)
            {
                XmlElement scriptNode = GetChildNodesByName(xmlElement, "script").FirstOrDefault();
                if (scriptNode != null)
                {
                    XmlElement sourceNode = GetChildNodesByName(scriptNode, "source").FirstOrDefault();
                    if (sourceNode != null)
                    {
                        scriptLanguage = sourceNode.GetAttribute("language");
                        this.scriptContent = sourceNode.InnerText;
                    }
                }
            }
        }


        public List<XmlElement> GetChildNodesByName(XmlElement xmlElement, string childName)
        {
            return XmlHelper.GetChildNodesByName(xmlElement, childName);
        }

        public void SaveXmlNodeFixedValues(XmlDocument xmlDocument, XmlElement xmlElement, List<FixedValue> fixedValues)
        {            
            foreach (FixedValue fixedValue in fixedValues)
            {
                XmlElement child = XmlHelper.AddChildNode(xmlDocument, xmlElement, "fixedvalue");
                child.SetAttribute("name", fixedValue.name);
                child.SetAttribute("value", fixedValue.value);
            }            
        }

        public List<FixedValue> ParseXmlNodeFixedValues(XmlElement xmlElement)
        {
            List<FixedValue> fixedValues = new List<FixedValue>();            
            foreach (XmlElement child in GetChildNodesByName(xmlElement, "fixedvalue"))
            {                
                string name = child.GetAttribute("name");
                string value = child.GetAttribute("value");
                fixedValues.Add(new FixedValue() { name = name, value = value });
            }
            return fixedValues;
        }

        public void SaveXmlChildNodeMaskValue(XmlDocument xmlDocument, XmlElement xmlElement, Mask mask)
        {
            xmlElement.SetAttribute("name", mask.name);
            xmlElement.SetAttribute("value", mask.value);
            foreach (FixedValue fixedValue in mask.fixedValues)
            {
                XmlElement child = XmlHelper.AddChildNode(xmlDocument, xmlElement, "fixedvalue");
                child.SetAttribute("name", fixedValue.name);
                child.SetAttribute("value", fixedValue.value);
            }
        }

        public Mask ParseXmlChildNodeMaskValue(XmlElement xmlElement)
        {
            Mask mask = new Mask();
            if (xmlElement.HasAttribute("name") == false 
                || xmlElement.HasAttribute("value") == false)
            {
                return null;
            }
            mask.name = xmlElement.GetAttribute("name");
            mask.value = xmlElement.GetAttribute("value");
            foreach (XmlElement child in GetChildNodesByName(xmlElement, "fixedvalue"))
            {
                string name = child.GetAttribute("name");
                string value = child.GetAttribute("value");
                mask.fixedValues.Add(new FixedValue() { name = name, value = value });
            }
            return mask;
        }

        public bool IsTotalyDerivedContent()
        {
            if (derived_from == null)
            {
                return false;
            }

            if (this.name != derived_from.name)
            {
                return false;
            }

            HashSet<string> keys = new HashSet<string>(attributes.Keys);
            keys.ExceptWith(new List<string> { ElementKey.id, ElementKey.name });
            if (keys.Count() > 0)
            {
                return false;
            }

            if ((description.Length > 0) 
                || (fixedValueDerivid == false)
                || (maskValueDerived == false)
                || scriptContent.Length > 0)
            {
                return false;
            }
            return true;
        }
    }
}
