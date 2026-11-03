using ApplicationServices.Commands;
using ApplicationServices.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("users")]
    public class UsersController(IMediator mediator) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterUserCommand command)
        {
            string id = await mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id }, new { id });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var user = await mediator.Send(new GetUserByIdQuery(id));
            return user is null ? NotFound() : Ok(user);
        }
    }
}