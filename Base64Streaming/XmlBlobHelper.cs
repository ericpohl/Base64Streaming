using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Base64Streaming
{
    public class XmlBlobHelper
    {
        /// <summary>
        /// Efficiently return the document references from StoredXmlContents XmlBlob string
        /// </summary>
        /// <param name="xmlBlob"></param>
        /// <returns></returns>

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        public static CompiledDocumentReferences GetCompiledDocumentReferences(string xmlBlob)
        {
            // See http://stackoverflow.com/questions/3831676/ca2202-how-to-solve-this-case for an explanation of why 
            // the warning is being suppressed

            TextReader outerTextReader = new StringReader(xmlBlob);
            string outerCdata = null;
            using (var outerXmlReader = new XmlTextReader(outerTextReader))
            {
                while (outerXmlReader.Read())
                {
                    if (outerXmlReader.NodeType == XmlNodeType.CDATA)
                    {
                        outerCdata = outerXmlReader.Value;
                        break;
                    }
                }
            }

            if (outerCdata == null)
            {
                throw new Exception("Outer CDATA not found");
            }

            // It would still be nice to have a Base64DecodingStream that took a string or textwriter and did this in one step.
            using (Stream encodedStream = new TextEncodingStream(outerCdata))
            {
                using (var base64DecodedStream = new CryptoStream(encodedStream, new FromBase64Transform(), CryptoStreamMode.Read))
                {
                    using (TextReader compiledDocumentTextReader = new StreamReader(base64DecodedStream, Encoding.Default))
                    {
                        CompiledDocumentReferences references = GetCompiledDocumentReferencesFromCompiledDocument(compiledDocumentTextReader);
                        return references;
                    }
                }
            }
        }

        private static CompiledDocumentReferences GetCompiledDocumentReferencesFromCompiledDocument(TextReader textReader)
        {
            using (var xmlReader = XmlReader.Create(textReader))
            {
                return GetCompiledDocumentReferencesFromCompiledDocument(xmlReader);
            }
        }

        private static CompiledDocumentReferences GetCompiledDocumentReferencesFromCompiledDocument(XmlReader xmlReader)
        {
            XElement referencesElement = xmlReader.ElementsNamed("references").First();
            CompiledDocumentReferences references = CreateCompiledDocumentReferences(referencesElement);
            return references;
        }

        // We could convert the XElement to an XmlElement here and call iMed's CompiledDocumentReferences constructor.
        // See http://blogs.msdn.com/b/ericwhite/archive/2008/12/22/convert-xelement-to-xmlnode-and-convert-xmlnode-to-xelement.aspx

        private static CompiledDocumentReferences CreateCompiledDocumentReferences(XElement element)
        {
            var references = new CompiledDocumentReferences
            {
                CategoryId = (int)element.Attribute("categoryId"),
                DapId = (int)element.Attribute("dapId"),
                ParentId = (int)element.Attribute("parentId")
            };

            foreach (XElement documentElement in element.Elements("document"))
            {
                references.Documents.Add(CreateCompiledDocumentReferenceDocument(documentElement));
            }
            return references;
        }

        private static CompiledDocumentReferenceDocument CreateCompiledDocumentReferenceDocument(XElement element)
        {
            //<document id="9914" type="5" guid="f675c680-83f1-40b9-9f06-ebdc78cc62a3" possible="False" categoryId="366" documentVersion="1.19" typeString="Parent">
            //   <name><![CDATA[ UCLA Template]]></name>
            //</document>
            var doc = new CompiledDocumentReferenceDocument
            {
                Id = (int)element.Attribute("id"),
                DocumentTypeId = (int)element.Attribute("type"),
                Guid = (Guid)element.Attribute("guid"),
                Possible = (bool)element.Attribute("possible"),
                CategoryId = (int)element.Attribute("categoryId"),
                DocumentVersion = (string)element.Attribute("documentVersion"),
                DocumentTypeString = (string)element.Attribute("typeString")
            };

            Console.WriteLine(string.Join(", ", element.Elements().Select(el => el.Name)));
            doc.Name = SubelementValue(element, "name");
            doc.CategoryName = SubelementValue(element, "categoryName");
            doc.SpecialtyName = SubelementValue(element, "specialtyName");

            return doc;
        }

        private static string SubelementValue(XElement element, string subelementName)
        {
            XElement subelement = element.Element(subelementName);
            if (subelement != null)
            {
                return subelement.Value;
            }
            return null;
        }

    }
}
