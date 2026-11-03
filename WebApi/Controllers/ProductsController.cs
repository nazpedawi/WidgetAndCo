using ApplicationServices.Commands;
using ApplicationServices.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebApi.Requests;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("products")]
    public class ProductsController(IMediator mediator) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProductCommand command)
        {
            string id = await mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id }, new { id });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var product = await mediator.Send(new GetProductByIdQuery(id));
            return product is null ? NotFound() : Ok(product);
        }

        [HttpGet]
        public async Task<IActionResult> Search([FromQuery] string? category,
                                                [FromQuery] decimal? minPrice,
                                                [FromQuery] decimal? maxPrice)
            => Ok(await mediator.Send(new SearchProductsQuery(category, minPrice, maxPrice)));

        [HttpPost("{id}/image")]
        public async Task<IActionResult> UploadImage(string id, IFormFile file)
        {
            await using Stream stream = file.OpenReadStream();
            string blobName = await mediator.Send(
                new UploadProductImageCommand(id, stream, file.FileName, file.ContentType, file.Length));
            return Ok(new { blobName });
        }

        [HttpPost("{id}/reviews")]
        public async Task<IActionResult> AddReview(string id, [FromBody] AddReviewRequest request)
        {
            string reviewId = await mediator.Send(new AddReviewCommand(id, request.Rating, request.Comment));
            return Created($"/products/{id}/reviews", new { reviewId });
        }

        [HttpGet("{id}/reviews")]
        public async Task<IActionResult> GetReviews(string id, [FromQuery] int max = 10)
            => Ok(await mediator.Send(new GetReviewsForProductQuery(id, max)));
    }
}