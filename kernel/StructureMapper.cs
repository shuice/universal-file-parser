using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kernel
{

    /*
        A structure mapper object maps the structures of a grammar to a file(ByteView / ByteArray).
     */
    public class StructureMapper
    {
        ByteView _byteView;
        Grammar grammar;
        ScriptInstance scriptInstance;
        ILog log;
        public Results results;

        MapContext mapContext;
        bool padding_rest = true;

        public StructureMapper(Grammar grammar, ILog log, ByteView bytesView, int root_level = 0, bool padding_rest = true)
        {
            this.padding_rest = padding_rest;
            this.grammar = grammar;                    
            this.log = log;
            this._byteView = bytesView;
            this.results = new Results(root_level);
            scriptInstance = new ScriptInstance();            
            scriptInstance.SetPredefineVariable(SCRIPT_PERDEFINE_GLOBAL_VAR_NAME.currentMapper, this);
            ElementStructure root_structure = grammar.GetStructureByIdWithPrefix(grammar.strStartStructure);
            mapContext = new MapContext();
            mapContext.scriptInstance = scriptInstance;
            mapContext.results = results;
            mapContext.gramma = grammar;
            mapContext.sturctureMapper = this;
            mapContext.totalByteView = _byteView;            
            mapContext.parseStructureStack = new Stack<(ElementStructure, Result, ByteView)>();
            mapContext.parseStructureStack.Push((root_structure, results.getRootResult(), _byteView));
            mapContext.parseElementStack = new Stack<(ElementBase, Result, ByteView)>();
            mapContext.parseElementStack.Push((root_structure, results.getRootResult(), _byteView));
            mapContext.log = log;
            mapContext.stringEncodingNotFound = new HashSet<string>();

            mapContext.results.mapContext = mapContext;
            mapContext.results._byteView = _byteView;

            IronPython.Modules.file_structure_plugin.helper = FileStructurePluginHelper.instance;
        }

        public Results getCurrentResults()
        {
            return results;
        }



        public long mapElement(ElementBase elementBase, ByteView byteView)
        {   
            MapResult mapResult = elementBase.mapByteView(byteView, results.rootResult, mapContext, "");
            Debug.Assert(mapResult.used_bits % ByteView.BITS_PER_BYTE == 0);
            return ByteView.ConvertBitsCount2BytesCount(mapResult.used_bits);
        }

        /*

            The length of the structure is added in the results to the enclosing structure.
                Parameters:
                 structure The structure to apply
         */
        public long mapStructure(ElementStructure structure)
        {
            long position_bytes = getCurrentOffset();
            long size_bytes = Int64.MaxValue;
            MapResult mapResult = mapStructureAtPositionWithError(structure, position_bytes, size_bytes);
            return ByteView.ConvertBitsCount2BytesCount(mapResult.used_bits);
        }


        public MapResult mapStructureWithError(ElementStructure structure)
        {
            long position_bytes = getCurrentOffset();
            long size_bytes = Int64.MaxValue;
            return mapStructureAtPositionWithError(structure, position_bytes, size_bytes);
        }



        /*
        The length of the structure is not added in the results to the enclosing structure.
           Parameters:
               structure The structure to apply
               position Where to apply the structure
               size Maximum space the structure can consume
        */
        public long mapStructureAtPosition(ElementStructure structure, long position_bytes, long size_bytes)
        {
            (_, Result structureResult, _) = mapContext.parseStructureStack.Peek();
            MapResult mapResult = mapStructureAtPositionBits(structure,
                                                        structureResult,
                                                        ByteView.ConvertBytesCount2BitsCount(position_bytes),
                                                        ByteView.ConvertBytesCount2BitsCount(size_bytes));
            List<MapErrorItem> mapErrorItems;
            if (mapResult.Breaked(out mapErrorItems))
            {
                string message = MapResult.ErrorMessageStacks(mapErrorItems);
                log.addLog(EnumLogLevel.error, message);
            }
            return ByteView.ConvertBitsCount2BytesCount(mapResult.used_bits);
        }
        public MapResult mapStructureAtPositionWithError(ElementStructure structure, long position_bytes, long size_bytes)
        {
            (_, Result structureResult, _) = mapContext.parseStructureStack.Peek();
            MapResult mapResult = mapStructureAtPositionBits(structure,
                                                        structureResult,
                                                        ByteView.ConvertBytesCount2BitsCount(position_bytes),
                                                        ByteView.ConvertBytesCount2BitsCount(size_bytes));
            List<MapErrorItem> mapErrorItems;
            if (mapResult.Breaked(out mapErrorItems))
            {
                string message = MapResult.ErrorMessageStacks(mapErrorItems);
                log.addLog(EnumLogLevel.error, message);
            }
            return mapResult;            
        }

        

        public MapResult mapStructureAtPositionBits(ElementStructure structure, Result parent_result, long position_in_bits, long size_in_bits)
        {
            ByteView byteView = _byteView.SkipToBits(position_in_bits, () => ($"mapping structure({structure.name}) at specified position, path: {parent_result.GetErrorPath()}", true));
            if ((size_in_bits != 0) && (size_in_bits != -1) && (size_in_bits != Int64.MaxValue))
            {
                byteView = byteView.TakeBits(size_in_bits, () => ($"mapping structure({structure.name}) at specified position, path: {parent_result.GetErrorPath()}", true));
            }
            
            MapResult mapResult = structure.mapByteView(byteView, parent_result, mapContext, "");
            return mapResult;
        }




        /*
            The maximum length the element may take is also passed.
                Parameters:
                    element The element to be applied
                    maxSize The maximum size the element may have in bytes
         */
        public long mapElementWithSize(ElementBase element, int maxSize_bytes)
        {
            long used_bits = mapElementWithSizeBits(element, (int)ByteView.ConvertBytesCount2BitsCount(maxSize_bytes));
            long used_bytes = ByteView.ConvertBitsCount2BytesCount(used_bits);
            return used_bytes;
        }



        /*
         The maximum length the element may take is also passed. (in bits)
            Parameters:
                element The element to be applied
                maxSizeBits The maximum size the element may have in bits
         */
        public long mapElementWithSizeBits(ElementBase element, int maxSize_bits)
        {
            (_, Result structureResult, _) = mapContext.parseStructureStack.Peek();
            Result result = mapContext.results.getLastResult();
            long next_index_of_bits = result.value.index_of_bits + result.value.count_of_bits;
            ByteView mapByteView = _byteView.SkipToBits(next_index_of_bits, () => ($"mapping element({element.name}) with specified size, path: {result.GetErrorPath()}", true))
                        .TakeBits(maxSize_bits, () => ($"mapping element({element.name}) with specified size, path: {result.GetErrorPath()}", true));
            MapResult mapResult = element.mapByteView(mapByteView, structureResult, mapContext, "");
            return mapResult.used_bits;
        }



        /*
           This endianness will be used by structure elements having set their endianness to dynamic.
                Parameters:
                     endianness The endianness to use from now on
        */

        // script use
        public void setDynamicEndianness(string strEndianness)
        {
            ENDIAN_TYPE endianType = String2Enum.translateStringToEndianType(strEndianness);
            if ((endianType == ENDIAN_TYPE.ENDIAN_BIG) || (endianType == ENDIAN_TYPE.ENDIAN_LITTLE))
            {
                grammar.setDynamicEndianness(endianType);
            }
        }

        /*                 
            The returned endianness is used by structure elements having set their endianness to dynamic.
        */
        public string getDynamicEndianness( )
        {
            (ElementStructure currentMappingStructure, _, _) = mapContext.parseStructureStack.Peek();
            return currentMappingStructure.endianType.ToString();
        }




        /*

         */
        public Grammar getCurrentGrammar()
        {
            return grammar;
        }



        /*

         */
        public ElementStructure getCurrentStructure()
        {
            (ElementStructure currentMappingStructure, _, _) = mapContext.parseStructureStack.Peek();
            return currentMappingStructure;            
        }


        /*

         */
        public ElementBase getCurrentElement()
        {
            (ElementBase elementBase, _, _) = mapContext.parseElementStack.Peek();
            return elementBase;
        }


        /*

         */
        public long getCurrentOffset()
        {
            (ElementBase elementBase, _, ByteView byteview) = mapContext.parseElementStack.Peek();
            return ByteView.ConvertBitsCount2BytesCount(byteview.index_of_bits);
        }


        /*

        Parameters:
            offset New offset to continue processing after script
         */
        public void setCurrentOffset(ulong offset_bytes)
        {
            var pair = mapContext.parseElementStack.Peek();
            pair.byteView = _byteView.SkipToBits(ByteView.ConvertBytesCount2BitsCount((long)offset_bytes), () => ($"setting current offset({offset_bytes}), path: {pair.result.GetErrorPath()}", true));
            var pair2 = mapContext.parseElementStack.Peek();
            Debug.Assert(pair2.byteView.index_of_bits/ 8 == (long)offset_bytes);
        }



        /*

         */
        public long getCurrentRemainingSize()
        {
            (ElementStructure currentMappingStructure, Result structureResult, ByteView byteView) = mapContext.parseStructureStack.Peek();
            Result result = mapContext.results.getLastResult();
            long next_index_of_bits = result.value.index_of_bits + result.value.count_of_bits;
            long remain_bytes = ByteView.ConvertBitsCount2BytesCount((byteView.count_of_bits - next_index_of_bits));
            return remain_bytes;
        }


        /*

         */
        public long process()
        {
            MapContext.mapContexts.Push(mapContext);
            ElementStructure root_structure = grammar.GetStructureByIdWithPrefix(grammar.strStartStructure);
            MapResult mapResult = mapStructureWithError(root_structure);
            Int64 bytesProcessed = ByteView.ConvertBitsCount2BytesCount(mapResult.used_bits);
            if (mapResult.Breaked() == false && padding_rest)
            {
                long max_position_parsed = results.max_position_parsed();
                if (_byteView.count_of_bytes > max_position_parsed)
                {
                    Result parent_result = results.rootResult.results.LastOrDefault();
                    if (parent_result != null)
                    {
                        ByteView padding_bytesView = _byteView.SkipToBits(ByteView.ConvertBytesCount2BitsCount(max_position_parsed), () => ($"adding padding bytes, path: {parent_result.GetErrorPath()}", true));                            
                        Result padding = Result.CreateStructurePaddingResult(padding_bytesView, parent_result, parent_result.level + 1);
                        parent_result.AddSubResult(padding);
                        parent_result.value.byteView = parent_result.value.byteView.ExpandToLength(parent_result.value.byteView.count_of_bits + padding.value.byteView.count_of_bits,
                                                                                                        ()=> ($"adding padding bytes, path: {parent_result.GetErrorPath()}", true));
                        bytesProcessed = _byteView.count_of_bytes;
                    }
                }
            }
            (_, Result structureResult, _) = mapContext.parseStructureStack.Peek();
            structureResult.value.SetContent(VALUE_TYPE.VALUE_TYPE_STRUCTURE, _byteView.TakeBits(ByteView.ConvertBytesCount2BitsCount(bytesProcessed), () => ("processing file", true)));

            MapContext.mapContexts.Pop();
            return bytesProcessed;
        }


        /*
             Parameters:
                outputFile Name of output file
         */
        public void dump(string outputFile)
        {
            throw new NotImplementedException();
        }

        public ByteView getCurrentByteView()
        {
            (_, _, ByteView b) = mapContext.parseStructureStack.Peek();
            Debug.Assert(b != null);
            return b;
        }

    }
}
