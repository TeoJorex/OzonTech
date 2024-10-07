using Domain.Services.IServices;
using DTO.Mappers;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Homework1.Controllers
{
    [ApiController]
    [Route("/Homework2/[controller]")]
    public class PredictionController : ControllerBase
    {
        private readonly ISaleService _saleServices;

        public PredictionController(ISaleService saleServices)
        {
            _saleServices = saleServices;
        }

        [HttpGet("/GetPrediction")]
        public IActionResult CalculatePrediction([Required] int id, [Required] int days)
        {
            try
            {

                return Ok(_saleServices.PredictionCalculate(id, days).ToModel());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
