namespace Asn.FileProcessor.Data
{
    public class Box
    {
        public string SupplierIdentifier { get; set; }
        public string Identifier { get; set; }

        public List<Content> Contents { get; set; }      
    }
}
