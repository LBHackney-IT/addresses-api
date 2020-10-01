namespace AddressesAPI.V1.Domain
{
    public class SimpleAddress
    {
        public string Line1 { get; set; }
        public string Line2 { get; set; }
        public string Line3 { get; set; }
        public string Line4 { get; set; }
        public string Town { get; set; }
        public string Postcode { get; set; }
        public long UPRN { get; set; }
    }
}
