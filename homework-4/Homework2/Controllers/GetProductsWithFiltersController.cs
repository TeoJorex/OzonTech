using Domain.DTO.Requests;
using Domain.Services.Interfaces;
using FluentValidation;
using Homework2.Controllers.DTO.Requests;
using Homework2.Controllers.DTO.Responses;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using GetProductsWithFiltersModel = Homework2.Controllers.DTO.Responses.GetProductsWithFiltersModel;

namespace Homework2.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GetProductsWithFiltersController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IValidator<GetProductsWithFiltersRequest> _pageValidator;

        public GetProductsWithFiltersController(IProductService productService,
            IValidator<GetProductsWithFiltersRequest> pageValidator)
        {
            _productService = productService;
            _pageValidator = pageValidator;
        }

        [HttpPost("/GetProductsWithFilters")]
        [SwaggerOperation("Получить товары с фильтрами")]
        public ActionResult<GetProductsWithFiltersResponse> GetProductsByFilters([FromBody] GetProductsWithFiltersRequest getProductsWithFiltersRequest)
        {
            try
            {
                var getProductsWithFiltersResponse = new GetProductsWithFiltersResponse();
                var products = _productService.GetProductsByFilter(new Domain.DTO.Requests.GetProductsWithFiltersModel()
                {
                    ProductType = getProductsWithFiltersRequest.ProductType,
                    DateTime = getProductsWithFiltersRequest.DateTime,
                    WarehouseId = getProductsWithFiltersRequest.WarehouseId,
                    PageNumber = getProductsWithFiltersRequest.PageNumber,
                    PageSize = getProductsWithFiltersRequest.PageSize
                });

                foreach (var product in products)
                {
                    getProductsWithFiltersResponse.getProductsWithFiltersModels.Add(new GetProductsWithFiltersModel()
                    {
                        Name = product.Name,
                        Price = product.Price,
                        Weight = product.Weight,
                        ProductType = product.ProductType,
                        CreatedDate = product.CreatedDate,
                        WarehouseId = product.WarehouseId
                    });
                }
                return Ok(getProductsWithFiltersResponse.getProductsWithFiltersModels);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error {ex.Message}");
            }
        }
    }
}
