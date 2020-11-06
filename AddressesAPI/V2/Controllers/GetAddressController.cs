using System.Linq;
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
                if (response?.Addresses == null || !response.Addresses.Any())
                {
                    return new NotFoundResult();
                }
                return new OkObjectResult(new APIResponse<SearchAddressResponse>(response));
            }
            catch (BadRequestException)
            {
                return new BadRequestResult();
            }
        }

        ///<summary>
        ///return options
        /// </summary>
        [HttpOptions]
        public IActionResult Options()
        {
            Response.Headers.Add("Access-Control-Allow-Methods", "GET,OPTIONS");
            Response.Headers.Add("Access-Control-Origin", "*");
            Response.Headers.Add("Access-Control-Allow-Headers", "*");
            return HandleResponse("OK");
        }
    }
}
