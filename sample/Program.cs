using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using kernel;
using System.Linq;
using System.Globalization;

using AnyPrefix.Microsoft.Scripting.Hosting;
using IronPython;
using IronPython.Hosting;
using AnyPrefix.Microsoft.Scripting;
using System.Text;
using IronPython.Modules;
using Windows.UI.Xaml;
using Newtonsoft.Json;

namespace sample
{
    public class GrammarRecordInProgram
    {

        // abc.grammar
        public string fileName;
        public string grammarName;
        public string extName;
        public bool isBuildin;
    }
    class Log : ILog
    {
        public void addLog(EnumLogLevel level, string message)
        {
            Debug.WriteLine($"{level}:{message}");
        }
    }

    class Program
    {
        static int count = 0;
        static int python_count = 0;
        static int lua_count = 0;

        static string grammar_path = "";
        static string sample_path = "";
        static bool isglobalidnumber(Grammar grammar, ElementBase elementBase, string idstring, string a)
        {
            if (idstring.StartsWith("id:") == false)
            {
                return false;
            }
            var element = grammar.allElements(true).FirstOrDefault(e => e.IdWithPrefix() == idstring);
            if (element == null)
            {
                return false;
            }
            if (type_check(element, a))
            {
                return true;
            }
            return false;
        }

        static bool isprenumber(ElementBase elementBase, string preNodename, string a)
        {
            if (preNodename.StartsWith("prev.") == false)
            {
                return false;
            }
            string nodeName = preNodename.Substring(5);
            if (elementBase.parentStructure == null)
            {
                return false;
            }
            int thisElementIndex = elementBase.parentStructure.elements(true).ToList().IndexOf(elementBase);
            int elementIndex = elementBase.parentStructure.elements(true).ToList().FindIndex(e => e.name == nodeName);
            if ((thisElementIndex == -1) || (elementIndex == -1) || (thisElementIndex < elementIndex))
            {
                return false;
            }
            ElementBase element = elementBase.parentStructure.elements(true)[elementIndex];
            if (type_check(element, a))
            {
                return true;
            }
            return false;
        }

        static bool isbronumber(ElementBase elementBase, string broNodeName, string a)
        {
            if (elementBase.parentStructure == null)
            {
                return false;
            }
            int thisElementIndex = elementBase.parentStructure.elements(true).ToList().IndexOf(elementBase);
            int elementIndex = elementBase.parentStructure.elements(true).ToList().FindIndex(e => e.name == broNodeName);
            if ((thisElementIndex == -1) || (elementIndex == -1) || (thisElementIndex < elementIndex))
            {
                return false;
            }
            ElementBase element = elementBase.parentStructure.elements(true)[elementIndex];
            if (type_check(element, a))
            {
                return true;
            }
            return false;
        }

        static bool isthisnumberwithoutthis(ElementBase elementBase, string subNodeName, string a)
        {
            ElementStructure elementStructure = (elementBase as ElementStructure);
            if (elementStructure == null)
            {
                return false;
            }
            var element = elementStructure.elements(true).ToList().FirstOrDefault(e => e.name == subNodeName);
            if (element == null)
            {
                return false;
            }
            if (type_check(element, a))
            {
                return true;
            }
            return false;
        }
        static bool isthisnumber(ElementBase elementBase, string subNodeName, string a)
        {
            if (subNodeName.StartsWith("this.") == false)
            {
                return false;
            }
            subNodeName = subNodeName.Substring(5);
            ElementStructure elementStructure = (elementBase as ElementStructure);
            if (elementStructure == null)
            {
                return false;
            }
            var element = elementStructure.elements(true).ToList().FirstOrDefault(e => e.name == subNodeName);
            if (element == null)
            {
                return false;
            }
            if (type_check(element, a))
            {
                return true;
            }
            return false;
        }

        static bool isglobalnamenumber(Grammar grammar, ElementBase elementBase, string subNodeName, string a)
        {
            var elements = grammar.allElements(true);
            var element = elements.FirstOrDefault(e => e.name == subNodeName);
            if (element == null)
            {
                return false;
            }
            if (type_check(element, a))
            {
                return true;
            }
            return false;
        }

        static bool isglobalnamenumberExcept(Grammar grammar, ElementBase elementBase, string subNodeName, string a, ElementBase except)
        {
            var elements = grammar.allElements(true);
            var element = elements.FirstOrDefault(e => (e.name == subNodeName) && (e != except));
            if (element == null)
            {
                return false;
            }
            if (type_check(element, a))
            {
                return true;
            }
            return false;
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

        static bool issimpleexpression(Grammar grammar, ElementBase elementBase, string v, string a)
        {
            List<String> vars = GetVarsFromExpression(v);
            foreach (var var in vars)
            {
                if (isRecognizedValue(grammar, elementBase, var, a, true) == false)
                {
                    Debug.WriteLine($"issimpleexpression->isRecognizedValue({a}  {var})");
                    return false;
                }
            }
            return true;
        }

        static bool type_check(ElementBase element, string a)
        {
            if ((a != "valueexpression")
                && (element.GetElementType() != ELEMENT_TYPE.ELEMENT_NUMBER)
                && (element.GetElementType() != ELEMENT_TYPE.ELEMENT_CUSTOM)
                && (element.GetElementType() != ELEMENT_TYPE.ELEMENT_SCRIPT))
            {
                return false;
            }
            return true;
        }


        static bool isRecognizedValue(Grammar grammar, ElementBase elementBase, string v, string a, bool skipExpression)
        {
            if (v.Length == 0)
            {
                return true;
            }
            Int64 i = 0;
            if (Int64.TryParse(v, out i))
            {
                return true;
            }
            if (v.ToLower().StartsWith("0x"))
            {
                if (Int64.TryParse(v.Substring(2), NumberStyles.HexNumber, null, out i))
                {
                    return true;
                }
            }

            if (v.ToLower() == "unlimited")
            {
                return true;
            }
            if (v.ToLower() == "remaining")
            {
                return true;
            }

            if (isglobalidnumber(grammar, elementBase, v, a))   // id:xx 同级别前面已经分析过的
            {
                return true;
            }
            if (isprenumber(elementBase, v, a))
            {
                return true;
            }
            if (isthisnumber(elementBase, v, a))
            {
                return true;
            }
            if (isthisnumberwithoutthis(elementBase, v, a))
            {
                return true;
            }
            if (isbronumber(elementBase, v, a))
            {
                return true;
            }
            if (isglobalnamenumber(grammar, elementBase, v, a))
            {
                return true;
            }
            if (skipExpression == false)
            {
                if (issimpleexpression(grammar, elementBase, v, a))
                {
                    return true;
                }
            }
            return false;
        }

        //static HashSet<String> attributes2 = new HashSet<string>();
        //static Dictionary<string, HashSet<string>> attribute_2_express = new Dictionary<string, HashSet<string>>();
        static void test_element_express_is_number(Grammar grammar, string file_name, ElementBase elementBase)
        {
            List<string> attributes = new List<string> { "repeatmin", "repeatmax", "repeat", "length", "minval", "maxval", "valueexpression", "alignment" };
            foreach (var a in attributes)
            {
                string v = elementBase.GetValue(a);
                if (isRecognizedValue(grammar, elementBase, v, a, false) == false)
                {
                    //attributes2.Add(a);
                    //HashSet<string> expressSet;
                    //if (attribute_2_express.TryGetValue(a, out expressSet) == false)
                    //{
                    //    expressSet = new HashSet<string>();
                    //    attribute_2_express[a] = expressSet;
                    //}
                    //expressSet.Add(v);
                    count++;
                    Debug.WriteLine($"-------{count} {file_name} -> \"{elementBase.id}\"->{elementBase.name}, {a}={v}");
                    // Debug.WriteLine($"{String.Join(' ', attributes2)}");
                }
                else
                {
                    //if (isthisnumberwithoutthis(elementBase, v, a))
                    //{
                    //    ElementStructure elementStructure = (elementBase as ElementStructure);
                    //    var element = elementStructure.elements.FirstOrDefault(e => e.name == v);
                    //    if (isglobalnamenumberExcept(elementBase, v, a, element))
                    //    {
                    //        Debug.WriteLine($"+++++++++{count} {file_name} -> \"{elementBase.id}\"->{elementBase.name}, {a}={v}");
                    //    }                        
                    //}

                }
            }
            ElementStructure s = elementBase as ElementStructure;
            if (s != null)
            {
                foreach (var inner in s.elements(true))
                {
                    test_element_express_is_number(grammar, file_name, inner);
                }
            }
            // var type = elementBase.GetElementType();
            //if ((type == ELEMENT_TYPE.ELEMENT_SCRIPT) || (type == ELEMENT_TYPE.ELEMENT_GRAMMAR_REF))
            //{
            //    Debug.WriteLine($"-------u File:{file_name}, name:{elementBase.name}, type:{type}");
            //}
        }

        static void test_all_element_express_is_number()
        {

            string[] files = System.IO.Directory.GetFiles(grammar_path);
            foreach (String file in files)
            {
                Grammar grammar = new Grammar();
                if (file.EndsWith(".grammar") == false)
                {
                    continue;
                }
                grammar.readFromFile(file);

                foreach (var s in grammar.rootStructures(true))
                {
                    test_element_express_is_number(grammar, Path.GetFileName(file), s);
                }
            }
        }

        

        static void test_parse_one_file(string grammmar_file_name, string sample_file_name)
        {
            Grammar grammar = new Grammar();
            grammar.readFromFile(Path.Join(grammar_path, grammmar_file_name));
            byte[] sample_file_bytes = System.IO.File.ReadAllBytes(Path.Join(sample_path, sample_file_name));
            StructureMapper structureMapper = new StructureMapper(grammar, new Log(), new ByteView(sample_file_bytes));
            structureMapper.process();
            var results = structureMapper.results.description();
            Console.WriteLine(results);
            Console.WriteLine($"-------------END({grammmar_file_name} - {sample_file_name})-------------");
            Console.ReadKey();
        }

        static void test_all_script_content_item(string file_name, ElementBase elementBase)
        {
            var type = elementBase.GetElementType();

            if ((type == ELEMENT_TYPE.ELEMENT_SCRIPT) || (type == ELEMENT_TYPE.ELEMENT_GRAMMAR_REF))
            {
                if (String.IsNullOrWhiteSpace(elementBase.scriptContent))
                {
                    return;
                }
                count++;
                if (elementBase.scriptContent.Contains("=\"Python\""))
                {
                    python_count++; // 108
                }
                else if (elementBase.scriptContent.Contains("=\"Lua\""))
                {
                    lua_count++;    // 29
                }
                else
                {

                }
                //  137 - 108 - 29
                Debug.WriteLine($"\n--------------------------------------------------------\n {count} - {python_count} - {lua_count}\nFile:{file_name}\n{elementBase.scriptContent}\n\n\n\n");
            }
        }

        static void test_all_script_content()
        {
            string[] files = System.IO.Directory.GetFiles(grammar_path);
            foreach (String file in files)
            {
                Grammar grammar = new Grammar();
                if (file.EndsWith(".grammar") == false)
                {
                    continue;
                }
                grammar.readFromFile(file);

                foreach (var s in grammar.allElements(true))
                {
                    test_all_script_content_item(Path.GetFileName(file), s);
                }
            }
        }

        static void testIronPython()
        {
            StringBuilder sb = new StringBuilder();
            string a = "";
            string strType = a.GetReleaseType().ToString();
            string strStrType = a.GetReleaseType().GetReleaseType().ToString();

            sb.AppendLine($"strType:{strType}");
            sb.AppendLine($"strStrType:{strStrType}");

            Int64 i = 0;
            Type[] interfaces = i.GetReleaseType().GetInterfaces();


            foreach (var t in interfaces)
            {
                string tt = t.ToString();
                Type ttt = t.GetReleaseType();
                Type tttt = t.GetReleaseType().GetReleaseType();
                sb.AppendLine($"{t.ToString()} > type:{ttt.ToString()} > type->type{tttt}       |");
            }
            Debug.WriteLine(sb);

            ScriptEngine engine = Python.CreateEngine();


            MapContext mapContext = new MapContext();
            mapContext.scriptInstance = new ScriptInstance();
            mapContext.results = new Results();
            mapContext.gramma = null;


            IronPython.Modules.file_structure_plugin.helper = FileStructurePluginHelper.instance;
            MapContext.mapContexts.Push(mapContext);

            {
                var script2 = engine.CreateScriptSourceFromString("from file_structure_plugin import *\nv=Value()\n", SourceCodeKind.Statements);
                script2.Execute(engine.CreateScope());
            }

            {
                var script2 = engine.CreateScriptSourceFromString("from datetime import datetime, timedelta\n", SourceCodeKind.Statements);
                script2.Execute(engine.CreateScope());
            }
        }

        static public Int64 ToInt64FromValue(ByteView byteView, ENDIAN_TYPE endianType)
        {
            if (endianType == ENDIAN_TYPE.ENDIAN_DYNAMIC)
            {
                Debug.Assert(byteView.count_of_bits <= ByteView.BITS_PER_BYTE);
            }
            byte[] bytes = byteView.GetReader().GetEnumerator(endianType == ENDIAN_TYPE.ENDIAN_BIG).Take(8).ToArray();
            if (endianType == ENDIAN_TYPE.ENDIAN_LITTLE)
            {
                Array.Reverse(bytes);
            }
            Int64 v = 0;
            foreach (byte b in bytes.Take(8))
            {
                Int64 t = b;
                v = v * 256 + t;
            }
            return v;
        }

        static void test_bit_padding()
        {
            {
                ByteView bv = new ByteView(new byte[] { 0xff, 0xff });
                ByteView nbv = bv.SkipBits(2).TakeBits(2);
                long big = ToInt64FromValue(nbv, ENDIAN_TYPE.ENDIAN_BIG);
                Debug.Assert(big == 0x3);
                long little = ToInt64FromValue(nbv, ENDIAN_TYPE.ENDIAN_LITTLE);
                Debug.Assert(little == 0x3);
                long dynamic = ToInt64FromValue(nbv, ENDIAN_TYPE.ENDIAN_DYNAMIC);
                Debug.Assert(dynamic == 0x3);
            }

            {


                ByteView bv = new ByteView(new byte[] { 0xff, 0xff });
                ByteView nbv = bv.SkipBits(2).TakeBits(7);
                long big = ToInt64FromValue(nbv, ENDIAN_TYPE.ENDIAN_BIG);
                Debug.Assert(big == 0x7F);
                long little = ToInt64FromValue(nbv, ENDIAN_TYPE.ENDIAN_LITTLE);
                Debug.Assert(little == 0x7F);
                long dynamic = ToInt64FromValue(nbv, ENDIAN_TYPE.ENDIAN_DYNAMIC);
                Debug.Assert(dynamic == 0x7F);
            }

            {

                ByteView bv = new ByteView(new byte[] { 0xff, 0xff });
                ByteView nbv = bv.SkipBits(2).TakeBits(8);
                long big = ToInt64FromValue(nbv, ENDIAN_TYPE.ENDIAN_BIG);
                Debug.Assert(big == 0xFF);
                long little = ToInt64FromValue(nbv, ENDIAN_TYPE.ENDIAN_LITTLE);
                Debug.Assert(little == 0xFF);
                long dynamic = ToInt64FromValue(nbv, ENDIAN_TYPE.ENDIAN_DYNAMIC);
                Debug.Assert(dynamic == 0xFF);
            }


            {

                ByteView bv = new ByteView(new byte[] { 0xff, 0xff });
                ByteView nbv = bv.SkipBits(2).TakeBits(9);
                long big = ToInt64FromValue(nbv, ENDIAN_TYPE.ENDIAN_BIG);
                Debug.Assert(big == 0x1FF);
                long little = ToInt64FromValue(nbv, ENDIAN_TYPE.ENDIAN_LITTLE);
                Debug.Assert(little == 0x80FF);
            }



            {
                ByteView bv = new ByteView(new byte[] { 0x12, 0x34 });
                ByteView nbv = bv.SkipBits(2).TakeBits(10);
                long big = ToInt64FromValue(nbv, ENDIAN_TYPE.ENDIAN_BIG);
                Debug.Assert(big == 0x123);
                long little = ToInt64FromValue(nbv, ENDIAN_TYPE.ENDIAN_LITTLE);
                Debug.Assert(little == 0xC048);

            }
            {
                ByteView bv = new ByteView(new byte[] { 0x12, 0x34 });
                ByteView bbv = bv.SkipBits(1).TakeBits(9);
                byte[] bytes = bbv.GetReader().GetEnumerator(true).Take(8).ToArray();
                Debug.Assert(bytes[0] == 0x00);
                Debug.Assert(bytes[1] == 0x48);
            }
        }

        static void test_offset_elements()
        {
            string[] files = System.IO.Directory.GetFiles(grammar_path);
            HashSet<string> offset_keys = new HashSet<string>();
            foreach (String file in files)
            {
                Grammar grammar = new Grammar();
                if (file.EndsWith(".grammar") == false)
                {
                    continue;
                }
                grammar.readFromFile(file);

                List<ElementBase> offsetElements = grammar.allElements(true).Where(e => e.GetElementType() == ELEMENT_TYPE.ELEMENT_OFFSET).ToList();
                foreach (var s in offsetElements)
                {
                    s.GetAttributeKeys().ToList().ForEach(attributeName => offset_keys.Add(attributeName));

                    // element name or "id:xx"
                    string references = s.GetValue(ElementKey.references);
                    if (references.StartsWith("id:"))
                    {
                        ElementStructure es = grammar.GetStructureByIdWithPrefix(references);
                        Debug.Assert(es != null);
                    }
                    else
                    {
                        ElementStructure es = grammar.getStructureByName(references);
                        Debug.Assert(es != null);
                    }


                    // "id:xx"
                    string referenced_size = s.GetValue(ElementKey.referenced_size);
                    if (referenced_size.Length > 0)
                    {
                        ElementBase es = grammar.GetElementByIdWithPrefix(referenced_size);
                        Debug.Assert(s.parentStructure.elements(true).Contains(es));
                        Debug.Assert(es.GetElementType() == ELEMENT_TYPE.ELEMENT_NUMBER);
                    }

                    string relative_to = s.GetValue(ElementKey.relative_to);
                    if (relative_to.Length > 0)
                    {
                        ElementStructure es = grammar.GetStructureByIdWithPrefix(relative_to);
                        Debug.Assert(es != null);
                    }
                    // relative-to
                }
            }

            Debug.WriteLine(String.Join(",", offset_keys));
        }

        static void generate_buildin_grammar_record()
        {
            List<GrammarRecordInProgram> records = new List<GrammarRecordInProgram>();
            string[] files = System.IO.Directory.GetFiles(grammar_path);
            HashSet<string> offset_keys = new HashSet<string>();
            foreach (String file in files)
            {
                Grammar grammar = new Grammar();
                if (file.EndsWith(".grammar") == false)
                {
                    continue;
                }
                grammar.readFromFile(file);
                GrammarRecordInProgram record = new GrammarRecordInProgram()
                {
                    fileName = Path.GetFileName(file),
                    grammarName = grammar.GetAttribute(Grammar.attribute_name),
                    extName = grammar.GetAttribute(Grammar.attribute_fileextension),
                    isBuildin = true,
                };
                records.Add(record);
            }
            records.Sort((a, b) => a.fileName.CompareTo(b.fileName));
            string s = JsonConvert.SerializeObject(records, Formatting.None);
            Debug.WriteLine(s);
        }

        static void find_all_exist_text_encoding()
        {
            List<GrammarRecordInProgram> records = new List<GrammarRecordInProgram>();
            string[] files = System.IO.Directory.GetFiles(grammar_path);
            HashSet<string> encodings = new HashSet<string>();
            foreach (String file in files)
            {
                Grammar grammar = new Grammar();
                if (file.EndsWith(".grammar") == false)
                {
                    continue;
                }
                grammar.readFromFile(file);
                var strings = grammar.allElements(true).Where(item => item is ElementString).ToList();
                foreach(var s in strings)
                {
                    string e = s.GetValue(ElementKey.encoding);
                    encodings.Add(e);
                }
            }
            
            Debug.WriteLine(String.Join(", ", encodings));
        }

        static void test_all_text_encoding()
        {
            

        }
        static void Main(string[] args)
        {
            grammar_path = @"D:\Code\file_structure\gramma\Grammars";  // change to your own
            sample_path = @"D:\Code\file_structure\gramma\Samples";
            //ElementStructure.TestJoinKeys();        // Structure derivation

            //test_bit_padding();                     // Padding when the number of bits is not a multiple of 8           
            //test_all_element_express_is_number();   // Whether all expressions can be recognized
            //test_all_script_content();              // all types of scripts
            //test_offset_elements();                 // Offset element
            //testIronPython();                       // Does IronPython support import syntax
            // generate_buildin_grammar_record();
            Console.WriteLine($"Using grammar path:{grammar_path}\nUsing sample path:{sample_path}");
            find_all_exist_text_encoding();
            test_all_text_encoding();
            test_parse_one_file("mach-o.grammar", "1.mach-o");
            test_parse_one_file("zip.grammar", "1.zip");   // lua script
            test_parse_one_file("bitmap.grammar", "1.bmp");   // offset
            
            test_parse_one_file("elf.grammar", "1.elf");   // lua enum，Dynamic setting endian

            test_parse_one_file("wav.grammar", "1.wav");
            test_parse_one_file("mp3.grammar", "1.mp3");   // python script
            
            
            
            
            test_parse_one_file("pe.grammar", "1.dll");   // Complex structure
            test_parse_one_file("bplist.grammar", "1.plist");   // script with bits

            // test_parse_one_file("container.grammar", "1.elf");   // grammar ref
            
            test_parse_one_file("tiff.grammar", "1.tiff");          // lua enum，Dynamic setting endian

            test_parse_one_file("jpeg.grammar", "1.jpg");

            test_parse_one_file("gzip.grammar", "1.gz");    // custom script
            test_parse_one_file("png.grammar", "1.png");   // "must_match == true"
            
            
            test_parse_one_file("qt.grammar", "1.mp4");   //  Complex structure
            test_parse_one_file("flac.grammar", "1.flac");  // parse bit
            
            // test_parse_one_file("test_padding.grammar", "1.flac");  // alignment
            


            Debug.WriteLine("Hello World!");
        }

    }



}
