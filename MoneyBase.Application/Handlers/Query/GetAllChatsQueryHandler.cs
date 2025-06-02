using MediatR;
using MoneyBase.Application.Services.Interfaces;
using MoneyBase.Domain.Entities;

namespace MoneyBase.Application.Handlers.Query;

public record GetAllChatsQuery : IRequest<List<ChatSession>>
{

}
public class GetAllChatsQueryHandler : IRequestHandler<GetAllChatsQuery, List<ChatSession>>
{
    private readonly IChatQueueService _queueService;
    public GetAllChatsQueryHandler(IChatQueueService queueService) 
    {
        _queueService = queueService;
    } 
    public Task<List<ChatSession>> Handle(GetAllChatsQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(_queueService.GetAllChats());
    }
}
