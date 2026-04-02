
using GixtApiBackend.Application.DTos;
using GixtApiBackend.Application.UseCases.Cost;
using Microsoft.AspNetCore.Mvc;


namespace GixtApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CostsController : ControllerBase
    {
        private readonly CreateCost _createCost;
        private readonly GetCost _getCost;
        private readonly GetCostById _getCostById;


        public CostsController(
            CreateCost createCost,
            GetCost getCost,
            GetCostById getCostById
        )
        {
            _createCost = createCost;
            _getCost = getCost;
            _getCostById = getCostById;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CostsDtos dto)
        {
            try
            {
                await _createCost.Execute(dto);
                return Ok(new { message = "Cost created successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
               var result =  await _getCost.Execute();
                return Ok(result);
            }
            catch(Exception ex) {
                    return BadRequest(new {message = ex.Message});
            }
        }

        [HttpGet("id/{id}")]
        public async Task<IActionResult> GetById (Guid id)
        {
            try
            {
                var result = await _getCostById.Execute(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


    }

}
