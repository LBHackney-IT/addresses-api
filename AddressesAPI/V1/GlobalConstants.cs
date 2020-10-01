namespace AddressesAPI.V1
{
    public static class GlobalConstants
    {

        public const int Limit = 50;

        public const int Offset = 0;


        //?#? ToDo: Organise these better, and get the streets connection
        public const string LlpgAddressesJson = "ConnectionSettings:LLPG:DEV";

        public const string NlpgAddressesJson = "ConnectionSettings:NLPG:DEV";

        public const string NlpgcombinedAddressesJson = "ConnectionSettings:NLPG_COMBINED:DEV";

        public const string LlpgStreetsJson = "ConnectionSettings:LLPG_STREETS:DEV";

        public enum Format
        {
            Simple,
            Detailed
        };

        public enum Gazetteer
        {
            Local,
            Both
        };
    }
}
