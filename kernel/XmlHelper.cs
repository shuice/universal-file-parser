using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace kernel
{
    public class XmlHelper
    {
        public static List<XmlElement> GetChildNodesByName(XmlElement xmlElement, string childName)
        {
            List<XmlElement> finded = new List<XmlElement>();
            foreach (XmlNode childNode in xmlElement)
            {
                XmlElement childXmlElement = childNode as XmlElement;
                if (childXmlElement == null)
                {
                    continue;
                }
                if (childXmlElement.Name == childName)
                {
                    finded.Add(childXmlElement);
                }
            }
            return finded;
        }

        public static XmlElement AddChildNode(XmlDocument xmlDocument, XmlElement xmlElement, string childNodeName)
        {
            XmlElement xmlChildElement = xmlDocument.CreateElement(childNodeName);
            xmlElement.AppendChild(xmlChildElement);
            return xmlChildElement;
        }

        public static string SaveDocument(XmlDocument xmlDoc)
        {
            using MemoryStream stream = new MemoryStream();
            XmlTextWriter writer = new XmlTextWriter(stream, null);
            writer.Formatting = Formatting.Indented;
            xmlDoc.Save(writer);
            using StreamReader sr = new StreamReader(stream, System.Text.Encoding.UTF8);
            stream.Position = 0;
            string xmlString = sr.ReadToEnd();
            return xmlString;
        }
    }
}
