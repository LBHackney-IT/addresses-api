using System;

namespace AddressesAPI.V1.Boundary.Responses
{
    public class ExecutionError
    {
        public string Message { get; set; }

        public ExecutionError(Exception ex)
        {
            Message = ex?.Message;
        }
    }
}
