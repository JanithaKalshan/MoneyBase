using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MoneyBase.Application.Services.Interfaces;
using MoneyBase.Domain.Entities;
using MoneyBase.Infrastructure.Utils;

namespace MoneyBase.Infrastructure.BackgroundServices;

public class ChatAgentAllocateService : BackgroundService
{
    private readonly TeamsCreator _teamsCreator;
    private readonly IChatQueueService _chatQueue;
    private readonly ILogger<ChatAgentAllocateService> _logger;
    public ChatAgentAllocateService(IChatQueueService chatQueue, ILogger<ChatAgentAllocateService> logger, TeamsCreator teamsCreator)
    {
        _teamsCreator = teamsCreator;
        _chatQueue = chatQueue;
        _logger = logger;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var currentTeams = _teamsCreator.GetActiveTeamInCurrentTime();
                if (currentTeams == null)
                {
                    _logger.LogWarning("No active team found for the current time.");
                }
                else
                {
                    var queueSize = _teamsCreator.GetMaxQueueSize();
                    var actualQueSize = _chatQueue.ChatQueueCurrentSize();
                    if (currentTeams.Shift.Type == Domain.Enums.ShiftTypes.OfficeTime && !currentTeams.Agents.Any(x => x.IsAvailableForChat) && actualQueSize >= queueSize)
                    {

                        var overFlowTeam = _teamsCreator.GetOverFlowTeam();
                        currentTeams = overFlowTeam;
                        _logger.LogInformation("Overflow team used. {Count} agents available.", currentTeams?.Agents.Count);

                    }

                    foreach (var agent in currentTeams?.Agents.OrderBy(x => x.AgentLevel).ToList()!)
                    {
                        if (agent != null && agent.IsAvailableForChat)
                        {
                            var chat = _chatQueue.RemoveFromQueue();
                            var updateCurrentChatStatus = _chatQueue.GetActiveChats().FirstOrDefault(c => c.Id == chat?.Id);
                            if (updateCurrentChatStatus == null)
                            {
                                continue;
                            }
                            _chatQueue.UpdateCurrentChatAgent(updateCurrentChatStatus.Id, agent);
                            agent.CurrentChatCount++;
                            _logger.LogInformation("Agent {AgentId} assigned to chat {ChatId}.", agent.Id, updateCurrentChatStatus.Id);
                        }
                        else
                        {
                            _logger.LogInformation("Agent {AgentId} (Level: {AgentLevel}) is not available for chat.", agent?.Id, agent?.AgentLevel.ToString());
                        }

                    }
                }
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, "An error occurred in ChatAgentAllocateService.");
            }
            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken); // Adjust delay as needed

        }
    }
}
