using System;

namespace Base64Streaming
{
    public class CompiledDocumentReferenceDocument
    {
        public int Id { get; set; }

        public int DocumentTypeId { get; set; }

        public string DocumentTypeString { get; set; }

        public Guid Guid { get; set; }

        public bool Possible { get; set; }

        public int CategoryId { get; set; }

        public string DocumentVersion { get; set; }

        public string Name { get; set; }

        public string CategoryName { get; set; }

        public string SpecialtyName { get; set; }
    }
}
