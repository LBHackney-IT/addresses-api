using System;

namespace AddressesAPI.V2.UseCase
{
    public class TestOpsErrorException : Exception
    {
        public TestOpsErrorException() : base("This is a test exception to test our integrations") { }
    }
}
