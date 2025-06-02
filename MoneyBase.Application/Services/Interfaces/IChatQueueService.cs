using MoneyBase.Domain.Entities;

namespace MoneyBase.Application.Services.Interfaces;

public interface IChatQueueService
{
    bool AddToQueue(Guid chatId, string userId);
    ChatSession? RemoveFromQueue();
    List<ChatSession> GetActiveChats();
    void UpdateChatSessionPoll(Guid chatId);
    bool UpdateCurrentChatAgent(Guid ChatId, Agent agent);
    int RemoveInactivefromChatActiveList(Guid chatId);
    int ChatQueueCurrentSize();
    ChatSession? GetChatById(Guid chatId);
    List<ChatSession> GetAllChats();

}
