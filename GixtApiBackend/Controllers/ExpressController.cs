
using GixtApiBackend.Application.DTos;
using GixtApiBackend.Application.UseCases.Expresss;
using Microsoft.AspNetCore.Mvc;


namespace GixtApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExpresssController : ControllerBase
    {
        private readonly CreateExpress _createExpress;
        private readonly GetExpress _getExpresss;
        private readonly GetExpressReviewById _getExpressReviewById;
        private readonly DeleteExpress _deleteExpress;
        private readonly CancelExpress _cancelExpress;
        private readonly GetExpressById _getExpressById;
        //private readonly GetExpressByUserId _getExpressByUserId;
        private readonly GetExpressByWorkerId _getExpressByWorkerId;
        private readonly UpdateExpressStatus _updateExpressStatus;
        private readonly SendAccept _sendAccept;
        private readonly AcceptExpress _acceptExpress;
        //private readonly GetExpressWorkerById _getExpressWorkerById;

        public ExpresssController(
            CreateExpress createExpress,
            GetExpress getExpresss,
            DeleteExpress deleteExpress,
            CancelExpress cancelExpress,
            GetExpressById getExpressById,
            GetExpressByWorkerId getExpressByWorkerId,
            //GetExpressByUserId getExpressByUserId,
            UpdateExpressStatus updateExpressStatus,
            SendAccept sendAccept,
            AcceptExpress acceptExpress,
            GetExpressReviewById getExpressReviewById
            //GetExpressWorkerById getExpressWorkerById
        )
        {
            _createExpress = createExpress;
            _getExpresss = getExpresss;
            _deleteExpress = deleteExpress;
            _cancelExpress = cancelExpress;
            _getExpressReviewById = getExpressReviewById;
            _getExpressById = getExpressById;
            //_getExpressByUserId = getExpressByUserId;
            _getExpressByWorkerId = getExpressByWorkerId;
            _updateExpressStatus = updateExpressStatus;
            _sendAccept = sendAccept;
            _acceptExpress = acceptExpress;
            //_getExpressWorkerById = getExpressWorkerById;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromForm] ExpressDTO dto)
        {
            try
            {
                var id =  await _createExpress.Execute(dto);
                return Ok(new { message = "Express created successfully", express_id = id });
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
                var jobs = await _getExpresss.Execute();
                return Ok(jobs);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        
        [HttpGet("review/{id}")]
        public async Task<IActionResult> GetReviewById(Guid id, Guid idworker)
        {
            var job = await _getExpressReviewById.Execute(id,idworker);

            if (job == null)
                return NotFound();

            return Ok(job);
        }

        [HttpGet("id/{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var job = await _getExpressById.Execute(id);

            if (job == null)
                return NotFound();

            return Ok(job);
        }

        //[HttpGet("user/{id}")]
        //public async Task<IActionResult> GetByUserId(Guid id)
        //{
        //    var job = await _getExpressByUserId.Execute(id);

        //    if (job == null)
        //        return NotFound();

        //    return Ok(job);
        //}

        [HttpGet("worker/{id}")]
        public async Task<IActionResult> GetByWorkerId(Guid id)
        {
            var job = await _getExpressByWorkerId.Execute(id);

            if (job == null)
                return NotFound();

            return Ok(job);
        }

        //[HttpGet("worker/id/{id}")]
        //public async Task<IActionResult> GetExpressWorkerById(Guid id)
        //{
        //    var job = await _getExpressWorkerById.Execute(id);

        //    if (job == null)
        //        return NotFound();

        //    return Ok(job);
        //}



        [HttpPatch("Status")]
        public async Task<IActionResult> UpdateStatus([FromForm] Guid id, [FromForm] string action)
        {
            try
            {

                await _updateExpressStatus.Execute(id, action);
                return Ok(new { message = "Express Status Updated successfully" });
            }

            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPatch("Accept")]
        public async Task<IActionResult> Accept([FromForm] Guid express_id, [FromForm] Guid worker_id, [FromForm] decimal km_cost, [FromForm]  decimal labor_price)
        {
            try
            {
                await _acceptExpress.Execute(express_id, worker_id,km_cost,labor_price);
                return Ok(new { message = "Express Accept successfully" });
            }

            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        [HttpPost("Send")]
        public async Task<IActionResult> SendAccept([FromForm] Guid worker, [FromForm] Guid id, [FromForm] decimal km_cost, [FromForm] decimal labor_price)
        {
            try
            {

                await _sendAccept.Execute(worker,id,km_cost,labor_price);
                return Ok(new { message = "Send successfully" });
            }

            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _deleteExpress.Execute(id);
            return Ok(new { message = "Express deleted successfully" });
        }

        [HttpPut("cancel/{id}")]
        public async Task<IActionResult> Cancel(Guid id)
        {
            await _cancelExpress.Execute(id);
            return Ok(new { message = "Express cancel successfully" });
        }

    }

}
