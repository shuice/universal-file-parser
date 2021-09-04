using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace kernel
{
    /*
        A grammar with all structures and their elements
    */
    public class Grammar
    {
        public delegate Grammar LoadGrammarRefDelegate(string fileName);

        private List<ElementStructure> _structures = new List<ElementStructure>();

        public static readonly string script_type_generic = "Generic";
        public static readonly string script_type_grammar = "Grammar";
        public static readonly string script_type_file = "File";
        public static readonly string script_type_process_results = "ProcessResults";
        public static readonly string script_type_datatype = "DataType";
        public static readonly string script_type_selection = "Selection";
        public List<EmbedScript> embedScripts = new List<EmbedScript>();
        public IReadOnlyList<EmbedScript> GetScript(string script_type)
        {
            return embedScripts.Where(item => item.type == script_type).ToList();
        }

        public static Dictionary<string, string> script_type_2_ui()
        {
            return new Dictionary<string, string>()
            {
                {script_type_generic,   "Generic"},
                {script_type_grammar,   "Grammar"},
                {script_type_file,      "File"},
                {script_type_process_results,      "Results"},
                {script_type_datatype,  "Data Type"},
                {script_type_selection, "Selection"},
            };
        }

        public EmbedScript CreateNewScript(string script_type)
        {
            EmbedScript embedScript = new EmbedScript();
            embedScript.name = "<New Script>";
            if (script_type_2_ui().Keys.Contains(script_type??""))
            {
                embedScript.type = script_type;
            }
            else
            {
                embedScript.type = Grammar.script_type_datatype;
            }
            embedScript.language = "Python";
            embedScript.source = "";
            embedScript.id = next_id.ToString();
            embedScript.description = "";
            next_id++;
            return embedScript;
        }

        public void AddEmbedScript(EmbedScript embedScript)
        {
            embedScripts.Add(embedScript);
        }

        public void RemoveEmbedScript(EmbedScript embedScript)
        {
            embedScripts.Remove(embedScript);
        }

        public Dictionary<string, string> attributes = new Dictionary<string, string>();
        public string description = "";

        public static readonly string attribute_name = "name";
        public static readonly string attribute_start = "start";
        public static readonly string attribute_author = "author";
        public static readonly string attribute_email = "email";
        public static readonly string attribute_fileextension = "fileextension";
        public static readonly string attribute_uti = "uti";
        public static readonly string attribute_complete = "complete";

        public string GetAttribute(string attribute)
        {
            string value;
            attributes.TryGetValue(attribute, out value);
            return value ?? "";
        }

        public void SetAttribute(string attribute, string value)
        {
            if (String.IsNullOrEmpty(value))
            {
                attributes.Remove(attribute);
            }
            else
            {
                attributes[attribute] = value;
            }
        }

        public string strStartStructure
        {
            get => GetAttribute(attribute_start);
            set
            {
                SetAttribute(attribute_start, value);
            }
        }


        public IReadOnlyList<ElementStructure> rootStructures(bool containDisabled)
        {
            if (containDisabled)
            {
                return _structures;
            }
            return _structures.Where(item => item.enabled).ToList();
        }

        public void ExchangeRootStructureAtIndex(int index1, int index2)
        {
            var tmp = _structures[index1];
            _structures[index1] = _structures[index2];
            _structures[index2] = tmp;
        }
        public void AddStructure(ElementStructure structure)
        {
            _structures.Add(structure);
        }

        public void RemoveStructure(ElementStructure structure)
        {
            _structures.Remove(structure);
        }

        public static Grammar CreateDefaultGrammar()
        {
            Grammar grammar = new Grammar();
            ElementStructure firstStructure = grammar.CreateDefaultElement(ELEMENT_TYPE.ELEMENT_STRUCTURE) as ElementStructure;
            grammar.strStartStructure = firstStructure.IdWithPrefix();
            grammar.AddStructure(firstStructure);
            grammar.SetAttribute(Grammar.attribute_name, "<New Grammar>");
            return grammar;
        }

        public void MakesureStartStructure()
        {
            Debug.Assert(_structures.Count > 0);
            
            if (_structures.Any(item => item.IdWithPrefix() == strStartStructure) == false)
            {
                strStartStructure = _structures.First().IdWithPrefix();
            }
        }

        public void SetStartStructure(ElementStructure structure)
        {
            strStartStructure = structure.IdWithPrefix();
        }

        public LoadGrammarRefDelegate loadGrammarRefDelegate;

        private string fileFolderName = "";

        private int next_id = 1;
        

        public Grammar loadGrammarRef(string fileName)
        {
            if (String.IsNullOrEmpty(fileFolderName))
            {
                return loadGrammarRefDelegate(fileName);
            }
            else
            {
                string fullPath = System.IO.Path.Combine(fileFolderName, fileName);
                Grammar grammar = new Grammar();
                grammar.readFromFile(fullPath);
                return grammar;
            }            
        }
        
        /*
             Parameters:
                    fileName The grammar file
        */
        public void readFromString(string xmlContent)
        {
            if (String.IsNullOrEmpty(xmlContent))
            {
                return;
            }
            var xmlDocument = new XmlDocument();
            try
            {
                xmlDocument.LoadXml(xmlContent);
            }
            catch(Exception)
            {
                return;
            }

            var rootElement = xmlDocument.DocumentElement;            
            XmlElement grammarXmlElment = XmlHelper.GetChildNodesByName(rootElement, "grammar").FirstOrDefault();
            Debug.Assert(grammarXmlElment != null);
            if (grammarXmlElment == null)
            {
                return;
            }

            foreach (XmlAttribute attribute in grammarXmlElment.Attributes)
            {
                attributes[attribute.Name] = attribute.Value;
            }            

            XmlElement descriptionXmlElment = XmlHelper.GetChildNodesByName(grammarXmlElment, "description").FirstOrDefault();
            if (descriptionXmlElment != null)
            {
                this.description = descriptionXmlElment.InnerText;
            }
            foreach(XmlElement structureXmlElement in XmlHelper.GetChildNodesByName(grammarXmlElment, "structure"))
            {
                ElementStructure structure = new ElementStructure();
                structure.grammar = this;
                structure.LoadFromXmlElement(structureXmlElement);
                MakesureHasId(structure);
                _structures.Add(structure);
            }

            XmlElement scriptsXmlElment = XmlHelper.GetChildNodesByName(grammarXmlElment, "scripts").FirstOrDefault();
            if (scriptsXmlElment != null)
            {
                foreach (XmlElement scriptXmlElment in XmlHelper.GetChildNodesByName(scriptsXmlElment, "script"))
                {
                    
                    string description = "";
                    XmlElement descriptionScriptXmlElment = XmlHelper.GetChildNodesByName(scriptXmlElment, "description").FirstOrDefault();
                    if (descriptionScriptXmlElment != null)
                    {
                        description = descriptionScriptXmlElment.InnerText ?? "";
                    }
                    
                    XmlElement sourceXmlElment = XmlHelper.GetChildNodesByName(scriptXmlElment, "source").FirstOrDefault();
                    EmbedScript embedScript = new EmbedScript()
                    {
                        name = scriptXmlElment.GetAttribute("name"),
                        type = scriptXmlElment.GetAttribute("type"),
                        id = scriptXmlElment.GetAttribute("id"),
                        language = (sourceXmlElment != null) ? sourceXmlElment.GetAttribute("language") : "",
                        source = (sourceXmlElment != null) ? (sourceXmlElment.InnerText ?? "") : "",
                        description = description,
                    };
                    embedScripts.Add(embedScript);                    
                }
            }

            

            foreach(ElementStructure structure in _structures)
            {
                structure.LoadedSubElementsFromDerived();                
            }

            List<ElementBase> all = allElements(true);
            if (all.Count > 0)
            {
                int max_id = all.Select(item => item.idValue).Max();
                next_id = Math.Max(next_id, max_id + 1);
            }
        }

        public void readFromFile(String fileName)
        {
            fileFolderName = System.IO.Path.GetDirectoryName(fileName);
            string text = System.IO.File.ReadAllText(fileName);
            readFromString(text);           
        }

        

        public string SaveToString()
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.CreateXmlDeclaration("1.0", "utf-8", "yes");
            XmlElement rootNode = xmlDocument.CreateElement("ufwb");
            xmlDocument.AppendChild(rootNode);
            rootNode.SetAttribute("version", "1.23.4");

            XmlElement grammarXmlElment = XmlHelper.AddChildNode(xmlDocument, rootNode, "grammar");
            foreach(var soredKey in attributes.Keys.ToList().OrderBy(item => item))
            {
                grammarXmlElment.SetAttribute(soredKey, attributes[soredKey]);
            }            

            if (String.IsNullOrEmpty(this.description) == false)
            {
                XmlElement descriptionXmlElment = XmlHelper.AddChildNode(xmlDocument, grammarXmlElment, "description");
                descriptionXmlElment.InnerText = this.description.Replace("\r\n", "\n").Replace('\r', '\n');
            }

            if (embedScripts.Count > 0)
            {
                XmlElement scriptsXmlElment = XmlHelper.AddChildNode(xmlDocument, grammarXmlElment, "scripts");
                foreach (EmbedScript embedScript in embedScripts)
                {
                    XmlElement scriptXmlElment = XmlHelper.AddChildNode(xmlDocument, scriptsXmlElment, "script");
                    if (String.IsNullOrEmpty(embedScript.description) == false)
                    {
                        XmlElement descriptionScriptXmlElment = XmlHelper.AddChildNode(xmlDocument, scriptXmlElment, "description");
                        descriptionScriptXmlElment.InnerText = embedScript.description;
                    }
                    XmlElement sourceXmlElment = XmlHelper.AddChildNode(xmlDocument, scriptXmlElment, "source");
                    scriptXmlElment.SetAttribute("name", embedScript.name);
                    scriptXmlElment.SetAttribute("type", embedScript.type);
                    scriptXmlElment.SetAttribute("id", embedScript.id);

                    sourceXmlElment.SetAttribute("language", embedScript.language);
                    sourceXmlElment.InnerText = embedScript.source.Replace("\r\n", "\n").Replace('\r', '\n');
                }
            }

            foreach(ElementStructure structure in _structures)
            {
                XmlElement structureXmlElement = XmlHelper.AddChildNode(xmlDocument, grammarXmlElment, "structure");
                structure.SaveToXmlElement(xmlDocument, structureXmlElement);
            }
            string strDocument = XmlHelper.SaveDocument(xmlDocument);
            return strDocument;
        }

        public IEnumerable<EmbedScript> getCustomScript(string script_id)
        {
            return embedScripts.Where(script => $"id:{script.id}" == script_id);
        }



        /*
           Parameters:
                fileName The grammar file
        */
        public void writeToFile(string fileName)
        {
            throw new NotImplementedException();
        }

        public ElementStructure GetStructureByIdWithPrefix(string idWithPrefix)
        {
            ElementStructure structure = _structures.FirstOrDefault(s => s.IdWithPrefix() == idWithPrefix);
            return structure;
        }
                

        public ElementBase GetElementByIdWithPrefix(string idWithPrefix)
        {            
            ElementBase element = allElements(true).FirstOrDefault(s => s.IdWithPrefix() == idWithPrefix);
            return element;
        }

        public ElementBase GetElementByIdWithPrefix(string idWithPrefix, ElementStructure inParentStructure)
        {
            ElementBase element = inParentStructure.elements(true).FirstOrDefault(s => s.IdWithPrefix() == idWithPrefix);
            return element;
        }

        private void listStructureElements(ElementStructure structure, bool containDisabled, ref List<ElementBase> elmentBase)
        {
            elmentBase.Add(structure);
            foreach (var e in structure.elements(containDisabled))
            {                
                var s  = e as ElementStructure;
                if (s != null)
                {
                    listStructureElements(s, containDisabled, ref elmentBase);
                }
                else
                {
                    elmentBase.Add(e);
                }
            }
        }


        public List<ElementBase> allElements(bool containsDisabled)
        {
            List<ElementBase> elements = new List<ElementBase>();
            foreach (var s in rootStructures(containsDisabled))
            {
                listStructureElements(s, containsDisabled, ref elements);
            }
            return elements;
        }


        public ElementBase CreateDefaultElement(ELEMENT_TYPE elementType)
        {
            Dictionary<ELEMENT_TYPE, Type> dicElementType2Type = new Dictionary<ELEMENT_TYPE, Type>()
            {
                {ELEMENT_TYPE.ELEMENT_OFFSET,           typeof(ElementOffset) },
                {ELEMENT_TYPE.ELEMENT_BINARY,           typeof(ElementBinary) },
                {ELEMENT_TYPE.ELEMENT_CUSTOM,           typeof(ElementCustom) },
                {ELEMENT_TYPE.ELEMENT_GRAMMAR_REF,      typeof(ElementGrammarRef) },
                {ELEMENT_TYPE.ELEMENT_NUMBER,           typeof(ElementNumber) },
                {ELEMENT_TYPE.ELEMENT_SCRIPT,           typeof(ElementScript) },
                {ELEMENT_TYPE.ELEMENT_STRING,           typeof(ElementString) },
                {ELEMENT_TYPE.ELEMENT_STRUCTURE,        typeof(ElementStructure) },
                {ELEMENT_TYPE.ELEMENT_STRUCTURE_REF,    typeof(ElementStructureRef) },
            };

            Type type = dicElementType2Type[elementType];
            ElementBase elementBase = System.Activator.CreateInstance(type) as ElementBase;
            elementBase.grammar = this;
            MakesureHasId(elementBase);
            elementBase.setName($"{elementBase.GetElementTypeUI().Replace(" ", "")}_{elementBase.id}");
            if (elementBase is ElementBinary)
            {
                elementBase.SetAttribute(ElementKey.length, "remaining");
            }
            else if (elementBase is ElementNumber)
            {
                elementBase.SetAttribute(ElementKey.length, "1");
            }
            else if (elementBase is ElementString)
            {
                elementBase.SetAttribute(ElementKey.type, "fixed-length");
                elementBase.SetAttribute(ElementKey.length, "0");
            }
            return elementBase;
        }

        public void MakesureHasId(ElementBase elementBase)
        {
            if (elementBase.idValue == 0)
            {
                elementBase.SetAttribute(ElementKey.id, next_id.ToString());
                next_id++;
            }
        }
        public ElementStructure getStructureByName(String name)
        {
            ElementStructure es = (ElementStructure)allElements(true).Where(s => s is ElementStructure).Where(s => s.name == name).FirstOrDefault();
            return es;
        }
        public void setDynamicEndianness(ENDIAN_TYPE endianness)
        {
            allElements(true).ForEach(e =>
            {
                if (String2Enum.translateStringToEndianType(e.GetValue(ElementKey.endian)) == ENDIAN_TYPE.ENDIAN_DYNAMIC)                
                {
                    e.SetAttribute(ElementKey.endian, endianness.ToString());                    
                }
            });
        }
    }
}
