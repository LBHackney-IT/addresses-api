using System.Collections.Generic;
using AddressesAPI.V2.Boundary.Requests;
using AddressesAPI.V2.Boundary.Responses;
using AddressesAPI.V2.Boundary.Responses.Metadata;
using AddressesAPI.V2.UseCase.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AddressesAPI.V2.Controllers
{
    [ApiVersion("2")]
    [Produces("application/json")]
    [Route("api/v2/addresses")]
    public class GetAddressController : V1.Controllers.BaseController
    {
        private readonly IGetSingleAddressUseCase _getAddressUseCase;

        public GetAddressController(IGetSingleAddressUseCase addressUseCase)
        {
            _getAddressUseCase = addressUseCase;
        }

        /// <summary>
        /// Returns an address from the given addressID or LPI_Key
        /// </summary>
        /// <param name="addressKey"></param>
        /// <returns></returns>
        [HttpGet, MapToApiVersion("1")]
        [Route("{addressKey}")]
        [ProducesResponseType(typeof(APIResponse<SearchAddressResponse>), 200)]
        public IActionResult GetAddress(string addressKey)
        {
            try
            {
                var request = new GetAddressRequest { addressID = addressKey };
                var response = _getAddressUseCase.ExecuteAsync(request);
                if (response?.Address == null)
                {
                    return new NotFoundObjectResult(new ErrorResponse(404, new List<Error>
                    {
                        new Error("An address could not be found for the given key")
                    }));
                }
                return new OkObjectResult(new APIResponse<GetAddressResponse>(response));
            }
            catch (BadRequestException)
            {
                return new BadRequestObjectResult(new ErrorResponse(400, new List<Error>
                {
                    new Error("Address Key is invalid. It should by 14 characters long string.")
                }));
            }
        }

        ///<summary>
        ///return options
        /// </summary>
        [HttpOptions]
        public IActionResult Options()
        {
            Response.Headers.AccessControlAllowMethods = "GET,OPTIONS";
            Response.Headers.AccessControlAllowOrigin = "*";
            Response.Headers.AccessControlAllowHeaders = "*";
            return HandleResponse("OK");
        }
    }
}
