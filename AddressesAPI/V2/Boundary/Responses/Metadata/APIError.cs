using System.Collections.Generic;

namespace AddressesAPI.V2.Boundary.Responses.Metadata
{
    public class APIError
    {
        public bool IsValid { get; set; }
        public IList<Error> ValidationErrors { get; set; }
    }
}
