
using GixtApiBackend.Application.DTos;
using GixtApiBackend.Application.UseCases.Review;
using Microsoft.AspNetCore.Mvc;


namespace GixtApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewController : ControllerBase
    {
        private readonly CreateReview _createReview;
        private readonly GetReviewById _getReviewById;
        private readonly GetReviewWorkerById _getReviewWorkerById;

        public ReviewController(
            CreateReview createReview,
            GetReviewById getReviewById,
            GetReviewWorkerById getReviewWorkerById

        )
        {
            _createReview = createReview;
            _getReviewById = getReviewById;
            _getReviewWorkerById = getReviewWorkerById;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromForm] ReviewDTO dto)
        {
            try
            {
                await _createReview.Execute(dto);
                return Ok(new { message = "Review created successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }



        [HttpGet("{id}")]
        public async Task<IActionResult> GetJobById(Guid id)
        {
            try
            {
                var Review = await _getReviewById.Execute(id);

                if (Review == null)
                    return NotFound();

                return Ok(Review);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        [HttpGet("Worker/{id}")]
        public async Task<IActionResult> GetWorkerById(Guid id)
        {
            try
            {

                var Review = await _getReviewWorkerById.Execute(id);

                if (Review == null)
                    return NotFound();

                return Ok(Review);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            
        }
    }

}
