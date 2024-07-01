namespace ASN.Infrastructure.Data.Models
{
    public class BoxLine
    {
        public int Id { get; set; }
        public string PoNumber { get; set; }
        public string Isbn { get; set; }
        public int Quantity { get; set; }

        public int BoxHeaderId { get; set; }
        public BoxHeader header { get; }
    }
}