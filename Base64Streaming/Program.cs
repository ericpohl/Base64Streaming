using System;
using System.IO;

namespace Base64Streaming
{
    class Program
    {
        static void Main(string[] args)
        {
            string xmlblob =
                File.ReadAllText(@"C:\Users\epohl.SRC_CORPORATE\Dropbox\projects\Base64Streaming\XmlBlob.xml");

            CompiledDocumentReferences references = XmlBlobHelper.GetCompiledDocumentReferences(xmlblob);
            Console.WriteLine("references: categoryId = {0}, parentId = {1}, dapId = {2}",
                references.CategoryId, references.ParentId, references.DapId);
            foreach (var doc in references.Documents)
            {
                Console.WriteLine("\tDocument: Id = {0}, Guid = {1}, Name = '{2}', SpecialtyName = '{3}', CategoryName = '{4}'",
                    doc.Id, doc.Guid, doc.Name, doc.SpecialtyName, doc.CategoryName);
            }
        }
    }
}
