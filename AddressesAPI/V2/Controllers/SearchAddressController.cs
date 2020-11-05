using System.Collections.Generic;
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
    public class SearchAddressController : V1.Controllers.BaseController
    {
        private readonly ISearchAddressUseCase _searchAddressUseCase;


        public SearchAddressController(ISearchAddressUseCase searchAddressUseCase)
        {
            _searchAddressUseCase = searchAddressUseCase;
        }

        /// <summary>
        /// Search Controller V1 to search for addresses
        /// </summary>
        /// <remarks>
        ///
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [ProducesResponseType(typeof(APIResponse<SearchAddressResponse>), 200)]
        [ProducesResponseType(typeof(APIResponse<BadRequestException>), 400)]
        [HttpGet, MapToApiVersion("1")]
        public IActionResult GetAddresses([FromQuery] SearchAddressRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = new List<ValidationError>();
                foreach (var (key, value) in ModelState)
                {
                    var err = new ValidationError();
                    foreach (var error in value.Errors)
                    {
                        err.FieldName = key;
                        err.Message = error.ErrorMessage;
                        errors.Add(err);
                    }
                }

                return new BadRequestObjectResult(new ErrorResponse(errors));
            }

            try
            {
                var response = _searchAddressUseCase.ExecuteAsync(request);
                return new OkObjectResult(new APIResponse<SearchAddressResponse>(response));
            }
            catch (BadRequestException e)
            {
                return new BadRequestObjectResult(new ErrorResponse(e.ValidationResponse));
            }
        }
    }
}
