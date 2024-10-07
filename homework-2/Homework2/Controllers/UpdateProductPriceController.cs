using Domain.DTO.Requests;
using Domain.Exeptions;
using Domain.Services.Interfaces;
using FluentValidation;
using Homework2.Controllers.DTO.Requests;
using Homework2.Controllers.DTO.Responses;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Homework2.Controllers
{
    [ApiController]
    [Route("/Homework2/UpdateProductPrice/[controller]")]
    public class UpdateProductPriceController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IValidator<UpdateProductPriceRequest> _newPriceValidator;

        public UpdateProductPriceController(IProductService productService,
            IValidator<UpdateProductPriceRequest> newPriceValidator)
        {
            _productService = productService;
            _newPriceValidator = newPriceValidator;
        }

        [HttpPatch("/UpdatePriceById")]
        [SwaggerOperation("Обновить цену товара с данным ID")]
        public ActionResult<UpdateProductPriceResponse> UpdateProductPrice(UpdateProductPriceRequest updateProductPriceRequest)
        {
            try
            {
                var response = _productService.UpdatePrice(new UpdateProductPriceModel()
                {
                    Id = updateProductPriceRequest.Id,
                    Price = updateProductPriceRequest.Price
                });

                return Ok(new UpdateProductPriceResponse() { Message = response });
            }
            catch (NotFoundExeption ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error {ex.Message}");
            }
        }
    }
}