using Domain.DTO.Requests;
using Domain.Exeptions;
using Domain.Services.Interfaces;
using Homework2.Controllers.DTO.Requests;
using Homework2.Controllers.DTO.Responses;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Homework2.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GetProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public GetProductController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpPost("/GetProductById")]
        [SwaggerOperation("Получить товар по ID")]
        public ActionResult<GetProductResponse> GetProduct(GetProductRequest getProductRequest)
        {
            try
            {
                var product = _productService.GetById(new GetProductModel()
                {
                    Id = getProductRequest.Id
                });
                return Ok(new GetProductResponse()
                {
                    Name = product.Name,
                    Price = product.Price,
                    Weight = product.Weight,
                    ProductType = product.ProductType,
                    CreatedDate = product.CreatedDate,
                    WarehouseId = product.WarehouseId
                });
            }
            catch (NotFoundExeption ex)
            {
                return BadRequest($"Error {ex.Message}");
            }
        }
    }
}
