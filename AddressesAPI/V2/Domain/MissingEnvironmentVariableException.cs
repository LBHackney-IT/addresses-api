using System;

namespace AddressesAPI.V2.Domain
{
    public class MissingEnvironmentVariableException : Exception
    {
        public MissingEnvironmentVariableException(string message) : base(message)
        {

        }
    }
}
