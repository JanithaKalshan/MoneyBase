using MediatR;
using Microsoft.AspNetCore.Mvc;
using MoneyBase.API.Controllers.Common;
using MoneyBase.Application.Handlers.Command;
using MoneyBase.Application.Handlers.Query;
using MoneyBase.Domain.Entities;
using Swashbuckle.AspNetCore.Annotations;

namespace MoneyBase.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiVersion("1")]
    public class ChatController : ApiControllerBase
    {

        public ChatController(ISender mediator) : base(mediator)
        {

        }


        [HttpPost("start-chat")]
        [SwaggerOperation(
        Summary = "Starts a new chat session with an available agent",
        Description = "Creates and assigns a new chat session to an available agent based on team allocation and availability.")]
        [SwaggerResponse(StatusCodes.Status201Created, "Chat created successfully", typeof(Guid))]
        public async Task<IActionResult> StartChat([FromBody] CreateChatCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(new { ChatId = result });
        }

        [HttpPost("poll-chat")]
        [SwaggerOperation(
        Summary = "Poll the status of an ongoing chat session",
        Description = "Sends a poll request to check the current state of a chat session")]
        [SwaggerResponse(StatusCodes.Status200OK, "Chat session polled successfully")]
        public async Task<IActionResult> Poll([FromBody] ChatPollCommand command)
        {
            await _mediator.Send(command);
            return Ok();
        }

        [HttpGet("get-all-chats")]
        [SwaggerOperation(
        Summary = "Retrieve all active and historical chat sessions",
        Description = "Returns a list of all chat sessions that have been started")]
        [SwaggerResponse(StatusCodes.Status200OK, "List of chat sessions retrieved successfully", typeof(IEnumerable<ChatSession>))]
        public async Task<IActionResult> GetAllChats()
        {
            var result = await _mediator.Send(new GetAllChatsQuery());
            return Ok(result);
        }
    }
}
