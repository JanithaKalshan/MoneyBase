using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MoneyBase.Application.Services.Interfaces;
using MoneyBase.Domain.Entities;
using MoneyBase.Infrastructure.Utils;

namespace MoneyBase.Infrastructure.BackgroundServices;

public class PollingService: BackgroundService
{
    private readonly IChatQueueService _queueService;
    private readonly ILogger<PollingService> _logger;
    private readonly TeamsCreator _teamsCreator;
    public PollingService(IChatQueueService queueService, ILogger<PollingService> logger, TeamsCreator teamsCreator) 
    {
        _queueService = queueService;
        _logger = logger;
       _teamsCreator = teamsCreator;
    } 
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var currentTime = DateTime.Now;
            _queueService.GetActiveChats().ForEach(chat =>
            {
                if(currentTime - chat.LastPolledAt > TimeSpan.FromSeconds(10)) 
                {
                    chat.MissedPolls++;
                    if (chat.MissedPolls >= 3) 
                    {
                        chat.IsActive = false;
                        var agent = chat.AssignedAgent;
                        _teamsCreator.DecrementAgentChatCount(agent?.Id);

                        _logger.LogWarning("Chat {ChatId} has been marked as inactive due to missed polls", chat.Id);
                    }
                    else
                    {
                        _logger.LogWarning("Chat {ChatId} has been polled. Missed polls: {MissedPolls}", chat.Id, chat.MissedPolls);
                    }
                }
            });

            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }
    }
}
