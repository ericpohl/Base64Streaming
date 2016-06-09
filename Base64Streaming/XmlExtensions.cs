using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;

namespace Base64Streaming
{
    public static class XmlExtensions
    {
        /// <summary>
        /// Return an enumeration of XElements that match the given element name
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="elementName"></param>
        /// <returns></returns>
        public static IEnumerable<XElement> ElementsNamed(this XmlReader reader, string elementName)
        {
            // See http://stackoverflow.com/a/19165632
            reader.MoveToContent(); // will not advance reader if already on a content node; if successful, ReadState is Interactive
            reader.Read();          // this is needed, even with MoveToContent and ReadState.Interactive
            while (!reader.EOF && reader.ReadState == ReadState.Interactive)
            {
                // corrected for bug noted by Wes below...
                if (reader.NodeType == XmlNodeType.Element && reader.Name.Equals(elementName))
                {
                    // this advances the reader...so it's either XNode.ReadFrom() or reader.Read(), but not both
                    var matchedElement = XNode.ReadFrom(reader) as XElement;
                    if (matchedElement != null)
                        yield return matchedElement;
                }
                else
                {
                    reader.Read();
                }
            }
        }

        // Next two are from http://blogs.msdn.com/b/ericwhite/archive/2008/12/22/convert-xelement-to-xmlnode-and-convert-xmlnode-to-xelement.aspx
        public static XElement ToXElement(this XmlNode node)
        {
            var xDoc = new XDocument();
            using (XmlWriter xmlWriter = xDoc.CreateWriter())
            {
                node.WriteTo(xmlWriter);                
            }
            return xDoc.Root;
        }

        public static XmlNode ToXmlNode(this XElement element)
        {
            using (XmlReader xmlReader = element.CreateReader())
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlReader);
                return xmlDoc;
            }
        }
    }
}
