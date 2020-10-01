using System;

namespace AddressesAPI.V1.Domain
{
    public class MissingEnvironmentVariableException : Exception
    {
        public MissingEnvironmentVariableException(string message) : base(message)
        {

        }
    }
}
