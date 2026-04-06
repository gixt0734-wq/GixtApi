
using GixtApiBackend.Application.DTos;
using GixtApiBackend.Application.UseCases.Paymentss;
using GixtApiBackend.Domain.Entities;
using Microsoft.AspNetCore.Mvc;


namespace GixtApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly UpdatePayment _updatePayment;
        private readonly GetPayment _getPayment;
        private readonly GetPaymentById _getPaymentById;


        public PaymentController(
            UpdatePayment updatePayment,
            GetPayment getPayment,
            GetPaymentById getPaymentById
        )
        {
            _updatePayment = updatePayment;
            _getPayment = getPayment;
            _getPaymentById = getPaymentById;
        }

        [HttpPost]
        public async Task<IActionResult> Update([FromBody] PaymentDtos dto)
        {
            try
            {
                await _updatePayment.Execute(dto);
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
               var result =  await _getPayment.Execute();
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
                var result = await _getPaymentById.Execute(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


    }

}
