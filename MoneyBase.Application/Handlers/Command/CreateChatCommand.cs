using MediatR;
using Microsoft.Extensions.Logging;
using MoneyBase.Application.Services.Interfaces;

namespace MoneyBase.Application.Handlers.Command;

public record CreateChatCommand(string UserId) : IRequest<Guid>
{

}

public class CreateChatCommandHandler(IChatQueueService _queueService,ILogger<CreateChatCommandHandler> _logger) : IRequestHandler<CreateChatCommand, Guid>
{
    public Task<Guid> Handle(CreateChatCommand request, CancellationToken cancellationToken)
    {
        var id = Guid.NewGuid();
        var result = _queueService.AddToQueue(id, request.UserId);
        if(result)
        {
            return Task.FromResult(id);
        }
        else
        {
            _logger.LogError("Queue is full, please try again later.");
            return Task.FromResult(Guid.Empty);
        }
    }
}
