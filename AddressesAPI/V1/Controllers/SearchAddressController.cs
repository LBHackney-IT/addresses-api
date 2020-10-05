using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AddressesAPI.V1.Boundary.Requests;
using AddressesAPI.V1.Boundary.Responses;
using AddressesAPI.V1.Boundary.Responses.Metadata;
using AddressesAPI.V1.UseCase.Interfaces;
using LBHAddressesAPI.Infrastructure.V1.Validation;
using Microsoft.AspNetCore.Mvc;

namespace AddressesAPI.V1.Controllers
{
    [ApiVersion("1")]
    [Produces("application/json")]
    [Route("api/v1/addresses")]
    [ProducesResponseType(typeof(APIResponse<object>), 400)]
    [ProducesResponseType(typeof(APIResponse<object>), 500)]
    public class SearchAddressController : BaseController
    {
        private readonly ISearchAddressUseCase _searchAddressUseCase;
        private readonly ISearchAddressValidator _searchAddressValidator;


        public SearchAddressController(ISearchAddressUseCase searchAddressUseCase, ISearchAddressValidator searchAddressValidator)
        {
            _searchAddressUseCase = searchAddressUseCase;
            _searchAddressValidator = searchAddressValidator;
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
        public async Task<IActionResult> GetAddresses([FromQuery] SearchAddressRequest request)
        {

            request.RequestFields = Request.Query.Keys.ToList();
            var validationResults = _searchAddressValidator.Validate(request);

            if (validationResults.IsValid)
            {
                if (!ModelState.IsValid)
                {
                    var errors = new List<ValidationError>();
                    foreach (var state in ModelState)
                    {
                        ValidationError err = new ValidationError();
                        foreach (var error in state.Value.Errors)
                        {
                            err.FieldName = state.Key;
                            err.Message = error.ErrorMessage;
                            errors.Add(err);
                        }
                    }
                    request.Errors = errors;
                }

                var response = await _searchAddressUseCase.ExecuteAsync(request).ConfigureAwait(false);
                return HandleResponse(response);
            }

            return new BadRequestObjectResult(new APIResponse<BadRequestException>(new BadRequestException(new RequestValidationResponse(validationResults))));
        }

    }
}
