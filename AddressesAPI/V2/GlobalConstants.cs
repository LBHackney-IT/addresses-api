namespace AddressesAPI.V2
{
    public static class GlobalConstants
    {

        public const int Limit = 50;

        public const int Offset = 0;

        public enum AddressScope
        {
            HackneyBorough,
            HackneyGazetteer,
            National
        }

        public enum Format
        {
            Simple,
            Detailed
        };

        public enum Gazetteer
        {
            Hackney,
            Both
        };
    }
}
