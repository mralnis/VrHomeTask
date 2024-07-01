namespace ASN.Infrastructure.Data.Models
{
    public class BoxHeader
    {
        public int Id { get; set; }
        public string SupplierIdentifier { get; set; }
        public string Identifier { get; set; }

        public ICollection<BoxLine> BoxLines { get; } = new List<BoxLine>();
    }
}