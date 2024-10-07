using Domain.Services.IServices;
using DTO.Mappers;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Homework1.Controllers
{
    [ApiController]
    [Route("/Homework2/[controller]")]
    public class DemandController : ControllerBase
    {
        private readonly ISaleService _saleServices;

        public DemandController(ISaleService saleServices)
        {
            _saleServices = saleServices;
        }

        [HttpGet("/GetDemand")]
        public IActionResult CalculateDemand([Required] int id, [Required] int days)
        {
            try
            {             
                return Ok(_saleServices.DemandCalculate(id,days).ToModel());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
