using MediatR;
using MoneyBase.Application.Services.Interfaces;

namespace MoneyBase.Application.Handlers.Command;

public record ChatPollCommand(Guid ChatId):IRequest<Unit>
{

}
public class ChatPollCommandHandler(IChatQueueService _queueService) : IRequestHandler<ChatPollCommand, Unit>
{
    public Task<Unit> Handle(ChatPollCommand request, CancellationToken cancellationToken)
    {
        _queueService.UpdateChatSessionPoll(request.ChatId);
        return Task.FromResult(Unit.Value);
    }
}
