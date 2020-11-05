using AddressesAPI.V2.Boundary.Requests;
using AddressesAPI.V2.Boundary.Responses;
using AddressesAPI.V2.Boundary.Responses.Metadata;
using AddressesAPI.V2.UseCase.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AddressesAPI.V2.Controllers
{
    [ApiVersion("1")]
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
        /// <param name="addressId"></param>
        /// <returns></returns>
        [HttpGet, MapToApiVersion("1")]
        [Route("{addressID}")]
        [ProducesResponseType(typeof(APIResponse<SearchAddressResponse>), 200)]
        public IActionResult GetAddress(string addressId)
        {
            var request = new GetAddressRequest { addressID = addressId };
            var response = _getAddressUseCase.ExecuteAsync(request);

            return HandleResponse(response);
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
