using MoneyBase.Application.Services.Interfaces;
using MoneyBase.Domain.Entities;
using MoneyBase.Infrastructure.Utils;
using System.Collections.Concurrent;
using System.Data;

namespace MoneyBase.Infrastructure.Services;

public class ChatQueueService : IChatQueueService
{
    private readonly ConcurrentQueue<ChatSession> _chatQueue;
    private readonly List<ChatSession> _activeChats;
    private int _maxQueueSize;
    private readonly int _maxQueueSizeWithOverFlow;
    private readonly Team? _currentTeam;
    public ChatQueueService(TeamsCreator teamsCreator)
    {
        _chatQueue = new ConcurrentQueue<ChatSession>();
        _activeChats = new List<ChatSession>();
        _currentTeam = teamsCreator.GetActiveTeamInCurrentTime();
        _maxQueueSize = teamsCreator.GetMaxQueueSize();
        _maxQueueSizeWithOverFlow = _maxQueueSize + teamsCreator.GetOverFlowQueueSize();
    }
    public bool AddToQueue(Guid chatId, string userId)
    {

        if (_chatQueue.Count > _maxQueueSize)
        {
            if (_currentTeam?.Shift.Type != Domain.Enums.ShiftTypes.OfficeTime)
            {
                return false;
            }
            else
            {
                if (_chatQueue.Count < _maxQueueSizeWithOverFlow)
                {
                    var size = _maxQueueSizeWithOverFlow;
                    if (_maxQueueSize >= _maxQueueSizeWithOverFlow)
                    {
                        return false;
                    }
                    _maxQueueSize = size;
                }
                else
                {
                    return false;
                }
            }

        }

        var chatSession = new ChatSession
        {
            Id = chatId,
            UserId = userId
        };
        _chatQueue.Enqueue(chatSession);
        _activeChats.Add(chatSession);
        return true;
    }

    public ChatSession? RemoveFromQueue()
    {
        if (_chatQueue.TryDequeue(out var chat))
        {
            return chat;
        }
        return null;
    }

    public List<ChatSession> GetActiveChats()
    {
        return _activeChats.Where(c => c.IsActive).ToList();
    }

    public List<ChatSession> GetAllChats()
    {
        return _activeChats.ToList();
    }

    public void UpdateChatSessionPoll(Guid chatId)
    {
        var chatSession = _activeChats.FirstOrDefault(c => c.Id == chatId);
        if (chatSession != null)
        {
            chatSession.LastPolledAt = DateTime.Now;
            chatSession.MissedPolls = 0;
        }
    }

    public bool UpdateCurrentChatAgent(Guid ChatId, Agent agent)
    {
        var chatSession = _activeChats.FirstOrDefault(c => c.Id == ChatId);
        if (chatSession != null)
        {
            chatSession.AssignedAgent = agent;
            return true;
        }
        else
        {
            return false;
        }
    }

    public int RemoveInactivefromChatActiveList(Guid chatId)
    {
        return _activeChats.RemoveAll(x => !x.IsActive);
    }

    public int ChatQueueCurrentSize()
    {
        return _chatQueue.Count;
    }

    public ChatSession? GetChatById(Guid chatId)
    {
        return _activeChats.FirstOrDefault(i => i.Id == chatId);
    }
}
