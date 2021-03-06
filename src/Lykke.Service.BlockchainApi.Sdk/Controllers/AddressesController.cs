using System.Threading.Tasks;
using Lykke.Service.BlockchainApi.Contract;
using Lykke.Service.BlockchainApi.Contract.Addresses;
using Lykke.Service.BlockchainApi.Sdk.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.BlockchainApi.Sdk.Controllers
{
    [ApiController]
    [Route("/api/addresses")]
    public class AddressesController : ControllerBase 
    {    
        readonly IBlockchainApi _api;

        public AddressesController(IBlockchainApi api) => _api = api;

        [HttpGet("{address}/explorer-url")]
        public ActionResult<string[]> GetExplorerUrl(string address) 
        {
            if (!Validators.ValidateAzureKey(address) || 
                !_api.AddressIsValid(address))
            {
                return BadRequest(BlockchainErrorResponse.Create("'address' must be valid blockchain address"));
            }

            return _api.GetExplorerUrl(address);
        }

        [HttpGet("{address}/validity")]
        public async Task<ActionResult<AddressValidationResponse>> IsValid(string address) 
        {
            return new AddressValidationResponse 
            {
                IsValid = Validators.ValidateAzureKey(address) &&
                    _api.AddressIsValid(address) &&
                    await _api.AddressIsExistAsync(address)
            };
        }
    }
}