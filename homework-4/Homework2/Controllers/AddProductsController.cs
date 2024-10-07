using Domain.DTO.Requests;
using Domain.Services.Interfaces;
using FluentValidation;
using Homework2.Controllers.DTO.Requests;
using Homework2.Controllers.DTO.Responses;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Homework2.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AddProductController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IValidator<AddProductRequest> _addValidator;

        public AddProductController(IProductService productService,
            IValidator<AddProductRequest> addValidator) 
        {
            _productService = productService;
            _addValidator = addValidator;
        }

        [HttpPost("/AddNewProduct")]
        [SwaggerOperation("Добавить новый товар")]
        public ActionResult<AddProductResponse> AddProduct(AddProductRequest addProductRequest)
        {
            try
            {
                return Ok(new AddProductResponse()
                {
                    Id = _productService.Add(new AddProductModel()
                    {
                        Name = addProductRequest.Name,
                        Price = addProductRequest.Price,
                        Weight = addProductRequest.Weight,
                        ProductType = addProductRequest.ProductType,
                        CreatedDate = addProductRequest.CreatedDate,
                        WarehouseId = addProductRequest.WarehouseId
                    })
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error {ex.Message}");
            }
        }

        
    }
}
