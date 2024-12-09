namespace AddressesAPI.V1
{
    public static class GlobalConstants
    {

        public const int Limit = 50;

        public const int Offset = 0;

        public enum Format
        {
            Simple,
            Detailed
        };

        public const string GazetteerDatabaseValueForLocal = "Hackney";

        public enum Gazetteer
        {
            Local,
            Both
        };

        public enum Structure
        {
            Flat,
            Hierarchy
        }
    }
}
