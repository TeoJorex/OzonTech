using Domain.Services.IServices;
using DTO.Mappers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sales.Services;
using System.ComponentModel.DataAnnotations;

namespace Homework1.Controllers
{
    [ApiController]
    [Route("/Homework2/[controller]")]
    public class ADSController : ControllerBase
    {
        private readonly ISaleService _saleServices;

        public ADSController(ISaleService saleServices)
        {
            _saleServices = saleServices;
        }

        [HttpGet("/GetADS")]
        public IActionResult CalculateADS([Required]int id)
        {
            try
            {
                return Ok(_saleServices.ADSCalculate(id).ToModel());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
