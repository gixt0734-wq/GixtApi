
using GixtApiBackend.Application.DTos;
using GixtApiBackend.Application.UseCases.Jobs;
using Microsoft.AspNetCore.Mvc;


namespace GixtApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobsController : ControllerBase
    {
        private readonly CreateJob _createJob;
        private readonly GetJob _getJobs;
        private readonly DeleteJob _deleteJob;
        private readonly GetJobById _getJobById;
        private readonly GetJobByUserId _getJobByUserId;
        private readonly GetJobByWorkerId _getJobByWorkerId;
        private readonly UpdateJobStatus _updateJobStatus;
        private readonly CancelJob _cancelJob;
        private readonly GetJobWorkerById _getJobWorkerById;

        public JobsController(
            CreateJob createJob,
            GetJob getJobs,
            DeleteJob deleteJob,
            GetJobById getJobById,
            GetJobByWorkerId getJobByWorkerId,
            GetJobByUserId getJobByUserId,
            UpdateJobStatus updateJobStatus,
            CancelJob calcelJob,
            GetJobWorkerById getJobWorkerById
        )
        {
            _createJob = createJob;
            _getJobs = getJobs;
            _deleteJob = deleteJob;
            _cancelJob = calcelJob;
            _getJobById = getJobById;
            _getJobByUserId = getJobByUserId;
            _getJobByWorkerId = getJobByWorkerId;
            _updateJobStatus = updateJobStatus;
            _getJobWorkerById = getJobWorkerById;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromForm] JobDTO dto)
        {
            try
            {
                await _createJob.Execute(dto);
                return Ok(new { message = "Job created successfully" });
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
                var jobs = await _getJobs.Execute();
                return Ok(jobs);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("id/{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var job = await _getJobById.Execute(id);

            if (job == null)
                return NotFound();

            return Ok(job);
        }

        [HttpGet("user/{id}")]
        public async Task<IActionResult> GetByUserId(Guid id)
        {
            var job = await _getJobByUserId.Execute(id);

            if (job == null)
                return NotFound();

            return Ok(job);
        }

        [HttpGet("worker/{id}")]
        public async Task<IActionResult> GetByWorkerId(Guid id)
        {
            var job = await _getJobByWorkerId.Execute(id);

            if (job == null)
                return NotFound();

            return Ok(job);
        }

        [HttpGet("worker/id/{id}")]
        public async Task<IActionResult> GetJobWorkerById(Guid id)
        {
            var job = await _getJobWorkerById.Execute(id);

            if (job == null)
                return NotFound();

            return Ok(job);
        }



        [HttpPatch("Status")]
        public async Task<IActionResult> UpdateStatus([FromForm] Guid id, [FromForm] string action)
        {
            try
            {

                await _updateJobStatus.Execute(id, action);
                return Ok(new { message = "Job Status Updated successfully" });
            }

            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _deleteJob.Execute(id);
            return Ok(new { message = "Job deleted successfully" });
        }

        [HttpPut("cancel/{id}")]
        public async Task<IActionResult> Cancel(Guid id)
        {
            await _cancelJob.Execute(id);
            return Ok(new { message = "Job cancel successfully" });
        }
    }

}
