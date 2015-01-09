using System.Collections.Generic;

namespace Base64Streaming
{
    public class CompiledDocumentReferences
    {
        public CompiledDocumentReferences()
        {
            Documents = new List<CompiledDocumentReferenceDocument>();
        }

        public int CategoryId { get; set; }
        public int ParentId { get; set; }
        public int DapId { get; set; }

        public IList<CompiledDocumentReferenceDocument> Documents { get; private set; }
    }
}
