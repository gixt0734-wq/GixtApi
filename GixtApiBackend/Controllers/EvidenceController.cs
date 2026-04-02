
using GixtApiBackend.Application.DTos;
using GixtApiBackend.Application.UseCases.Evidence;
using Microsoft.AspNetCore.Mvc;


namespace GixtApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EvidenceController : ControllerBase
    {
        private readonly CreateEvidence _createEvidence;
        //private readonly GetCost _getCost;
        //private readonly GetCostById _getCostById;


        public EvidenceController(
            CreateEvidence createEvidence
            //GetCost getCost,
            //GetCostById getCostById
        )
        {
            _createEvidence = createEvidence;
            //_getCost = getCost;
            //_getCostById = getCostById;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromForm] EvidenceDTO dto)
        {
            try
            {
                await _createEvidence.Execute(dto);
                return Ok(new { message = "Cost created successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        //[HttpGet]
        //public async Task<IActionResult> Get()
        //{
        //    try
        //    {
        //       var result =  await _getCost.Execute();
        //        return Ok(result);
        //    }
        //    catch(Exception ex) {
        //            return BadRequest(new {message = ex.Message});
        //    }
        //}

        //[HttpGet("id/{id}")]
        //public async Task<IActionResult> GetById (Guid id)
        //{
        //    try
        //    {
        //        var result = await _getCostById.Execute(id);
        //        return Ok(result);
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(new { message = ex.Message });
        //    }
        //}


    }

}
