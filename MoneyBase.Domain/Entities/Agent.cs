using MoneyBase.Domain.Enums;

namespace MoneyBase.Domain.Entities;

public class Agent
{
    public Guid Id { get; set; }
    public AgentsLevel AgentLevel { get; set; }
    public double Efficiency =>
            AgentLevel switch
            {
                AgentsLevel.Junior => 0.4,
                AgentsLevel.Mid => 0.6,
                AgentsLevel.Senior => 0.8,
                AgentsLevel.TeamLead => 0.5,
                _ => 0.0
            };
    public int MaxChatCount => Convert.ToInt32(Efficiency* 10);
    public int CurrentChatCount { get; set; }
    public bool IsAvailableForChat => CurrentChatCount < MaxChatCount;
}
