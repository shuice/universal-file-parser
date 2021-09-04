using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kernel
{

    /*
       A results object contains the results of the structure mapping process
     */
    public class Results
    {
        public Result rootResult;
        public MapContext mapContext;
        public ByteView _byteView;
        private Result _last_result = null;
        private Dictionary<string, Result> dicName2Result = new Dictionary<string, Result>();
        private Int64 _max_position_parsed = 0;
        public List<Result> elementScriptAddedResults = new List<Result>();
        public bool inElementScript = false;

        public Results(int root_level = 0)            
        {
            rootResult = new Result() { level = root_level, isFragment = false, name="Root" };            
        }

        private void AddAllSubResult(Result r, ref List<Result> all_results)
        {
            foreach(Result sub_result in r.results)
            {
                all_results.Add(sub_result);
                AddAllSubResult(sub_result, ref all_results);
            }
        }

        public List<Result> allResults()
        {
            List<Result> all_results = new List<Result>();
            foreach (var r in rootResult.results)
            {
                all_results.Add(r);
                AddAllSubResult(r, ref all_results);
            }
            return all_results;
        }

        public string description()
        {
            StringBuilder sb = new StringBuilder();
            foreach(var r in rootResult.results)
            {
                sb.AppendLine(r.description());
            }
            return sb.ToString();            
        }

        public List<Result> sub_results = null;

        /*
            The returned result can be used to remove all results from this on using the cut method.This
            method is used usually when the structure is enclosed in another structure.
                Parameters:
                    structure The structure that was mapped
                    startPos Where in the file was the structure mapped? (bytes)
                    iteration How often was this structure mapped consecutively? (Array of structures)
                    name Name to show for the result
                    addSizeToEnclosing Add size to the enclosing structure result? Set this to true if the
                        structure is actually contained in the enclosing structure in the
                        result tree.
         */
        public Result addStructureStart(ElementStructure structure, long startPos, int iteration, String name, bool addSizeToEnclosing)
        {
            (_, Result structureResult, _) = mapContext.parseStructureStack.Peek();


            Result r = new Result()
            {
                elementBase = structure,
                parent = structureResult,
                level = structureResult.level + 1,
                name = name,
                start_parse_bits_position = ByteView.ConvertBytesCount2BitsCount(startPos),
                value = Value.CreateValueDirectly(),
                iteration = iteration,
                iteration_valided = true,
                fillColor = structure.GetValueDerivedWithDefault(ElementKey.fillcolor),
                strokeColor = structure.GetValueDerivedWithDefault(ElementKey.strokecolor),
                isFragment = false,
            };
            r.value.index_of_bits = r.start_parse_bits_position;
            r.value.count_of_bits = 0;
            

            structureResult.AddSubResult(r);
            
            mapContext.parseStructureStack.Push((structure, r, _byteView.SkipToBits(r.start_parse_bits_position, () => ($"adding structure({structure.name}) start, path: {r.GetErrorPath()}", true))));
            return r;
        }



        /*
        The returned result can be used to remove all results from this on using the cut method.This
        method is used usually when the structure is referenced from another position and not enclosed
        in another structure.
            Parameters:
                structure The structure that was mapped
                startPos Where in the file was the structure mapped? (bytes)
                iteration How often was this structure mapped consecutively? (for
                arrays of structures)
                name Name to show for the result
         */
        public Result addStructureStartAtPosition(ElementStructure structure, long startPos, int iteration, String name)
        {            
            Result result = addStructureStart(structure, startPos, iteration, name, false);
            return result;
        }

 




        /*

        The returned result can be used to remove all results from this on using the cut method.
            Parameters:
                endPos Where in the file did the structure end? Padding bytes are calculated automatically
         */
        public Result addStructureEnd(long endPos)
        {
            var pair = mapContext.parseStructureStack.Pop();
            pair.result.value.count_of_bits = (ByteView.ConvertBytesCount2BitsCount(endPos) - pair.result.value.index_of_bits);
            pair.result.value.byteView = _byteView.SkipToBits(pair.result.value.index_of_bits, () => ($"adding structure({pair.elementStructure.name}) end, path: {pair.result.GetErrorPath()}", true))
                .TakeBits(pair.result.value.count_of_bits, () => ($"adding structure({pair.elementStructure.name}) end, path: {pair.result.GetErrorPath()}", true));
            setLastResult(pair.result);
            return pair.result;
        }


        /*
        The returned result can be used to remove all results from this on using the cut method.
        Parameters:
            element The structure element that was mapped
                length Length of the element in bytes
                iteration How often was this structure element mapped consecutively? (Array of structures)
                value The value resulting of the element being mapped to the file
        */
        public Result addElement(ElementBase element, long length, int iteration, Value value)
        {
            Result r = addElementBits(element, ByteView.ConvertBytesCount2BitsCount(length), iteration, value);
            return r;
        }


        /*
         The returned result can be used to remove all results from this on using the cut method.
            Parameters:
                element The structure element that was mapped
                length Length of the element in bits
                iteration How often was this structure element mapped consecutively? (Array of structures)
                value The value resulting of the element being mapped to the file
        */
        public Result addElementBits(ElementBase element, long length_in_bits, int iteration, Value value)
        {
            (_, Result structureResult, _) = element.mapContext.parseStructureStack.Peek();

            Result r = new Result()
            {
                elementBase = element,
                parent = structureResult,
                level = structureResult.level + 1,
                name = element.name,
                start_parse_bits_position = (_last_result != null) ? (_last_result.value.index_of_bits + _last_result.value.count_of_bits) : 0,
                value = value,
                iteration = iteration,
                iteration_valided = true,
                fillColor = element.GetValueDerivedWithDefault(ElementKey.fillcolor),
                strokeColor = element.GetValueDerivedWithDefault(ElementKey.strokecolor),
                isFragment = false,
            };
            value.index_of_bits = r.start_parse_bits_position;
            value.count_of_bits = length_in_bits;

            structureResult.AddSubResult(r);
            element.SetScriptValues(r);
            setLastResult(r);

            return _last_result;
        }

        /*
            This is the result that was added most recently.
        */
        public Result getLastResult()
        {
            return _last_result;
        }

        public Int64 max_position_parsed()
        {
            return _max_position_parsed;
        }

        public void setLastResult(Result last_result)
        {
            if (last_result != null)
            {
                dicName2Result[last_result.name] = last_result;
            }
            _last_result = last_result;

            Int64 result_end_position = ByteView.ConvertBitsCount2BytesCount(last_result.value.index_of_bits + last_result.value.count_of_bits);
            _max_position_parsed = Math.Max(result_end_position, _max_position_parsed);

            if (inElementScript)
            {
                elementScriptAddedResults.Add(last_result);
            }
        }

        /*
         Pass here the successor of the result you want.
        */
        public Result getPrevResult(Result result)
        {
            int index = result.index_in_structure;
            if (index <= 0)
            {
                return null;
            }
            return result.parent.results[index - 1];
        }

        /*
         You usually do this when you parsed in a wrong way and need to reparse from a certain position.
            So save a reference to the result where you may want to restart.
            Parameters:
                result First result 
        */
        public void cut(Result result)
        {
            throw new NotImplementedException();
        }



        /*
         The search starts at the end.
            Parameters:
                name Name of the result you're looking for
        */
        public Result getResultByName(String name)
        {
            Result r;
            dicName2Result.TryGetValue(name, out r);
            return r;            
        }





        /*
         Get root result which contains the first layer of the parsing results.
         */
        public Result getRootResult()
        {
            return rootResult;
        }



        /*
        Export tree of parsing results to XML or text file.
            Parameters:
                fileName Output file
                    format Export format. Can be EXPORT_RESULTS_FORMAT_TEXT or
                    EXPORT_RESULTS_FORMAT_XML
        */
        public void exportToFile(String fileName, Format format)
        {
            throw new NotImplementedException();
        }

    }
}
