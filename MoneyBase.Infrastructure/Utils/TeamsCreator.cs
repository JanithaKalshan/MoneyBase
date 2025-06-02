using MoneyBase.Domain.Entities;
using MoneyBase.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace MoneyBase.Infrastructure.Utils;

public class TeamsCreator
{
    private readonly List<Team>? teams;

    public TeamsCreator()
    {
        teams = new List<Team>();

        // Team A is going to be work in office time from 08.00 to 16.00
        Team teamA = new Team
        {
            TeamName = "Team A",
            Agents = new List<Agent>
            {
                new Agent { Id = Guid.NewGuid(), AgentLevel = Domain.Enums.AgentsLevel.TeamLead },
                new Agent { Id = Guid.NewGuid(), AgentLevel = Domain.Enums.AgentsLevel.Mid },
                new Agent { Id = Guid.NewGuid(), AgentLevel = Domain.Enums.AgentsLevel.Mid },
                new Agent { Id = Guid.NewGuid(), AgentLevel = Domain.Enums.AgentsLevel.Junior },
            },
            Shift = new Shift { Type = ShiftTypes.OfficeTime, Start = TimeSpan.FromHours(8), End = TimeSpan.FromHours(16) }
        };

        // Team B is going to be work in office time from 17.00 to 00.00
        Team teamB = new Team
        {
            TeamName = "Team B",
            Agents = new List<Agent>
            {
                new Agent { Id = Guid.NewGuid(), AgentLevel = Domain.Enums.AgentsLevel.Senior },
                new Agent { Id = Guid.NewGuid(), AgentLevel = Domain.Enums.AgentsLevel.Mid },
                new Agent { Id = Guid.NewGuid(), AgentLevel = Domain.Enums.AgentsLevel.Junior },
                new Agent { Id = Guid.NewGuid(), AgentLevel = Domain.Enums.AgentsLevel.Junior },
            },
            Shift = new Shift { Type = ShiftTypes.NightShift, Start = TimeSpan.FromHours(17), End = TimeSpan.FromHours(0) }
        };

        // Team c is going to be work in office time from 00.00 to 08.00
        Team teamC = new Team
        {
            TeamName = "Team C",
            Agents = new List<Agent>
            {
                new Agent { Id = Guid.NewGuid(), AgentLevel = Domain.Enums.AgentsLevel.Mid },
                new Agent { Id = Guid.NewGuid(), AgentLevel = Domain.Enums.AgentsLevel.Mid }
            },
            Shift = new Shift { Type = ShiftTypes.MidNightShift, Start = TimeSpan.FromHours(0), End = TimeSpan.FromHours(8) }
        };

        // Team overflow is going to be work in office time from 00.00 to 08.00
        Team teamOverflow = new Team
        {
            TeamName = "Team Overflow",
            Agents = new List<Agent>
            {
                new Agent { Id = Guid.NewGuid(), AgentLevel = Domain.Enums.AgentsLevel.Junior },
                new Agent { Id = Guid.NewGuid(), AgentLevel = Domain.Enums.AgentsLevel.Junior },
                new Agent { Id = Guid.NewGuid(), AgentLevel = Domain.Enums.AgentsLevel.Junior },
                new Agent { Id = Guid.NewGuid(), AgentLevel = Domain.Enums.AgentsLevel.Junior },
                new Agent { Id = Guid.NewGuid(), AgentLevel = Domain.Enums.AgentsLevel.Junior },
                new Agent { Id = Guid.NewGuid(), AgentLevel = Domain.Enums.AgentsLevel.Junior }
            },
            Shift = new Shift { Type = ShiftTypes.OfficeTime, Start = TimeSpan.FromHours(8), End = TimeSpan.FromHours(17) }
        };

        teams.Add(teamA);
        teams.Add(teamB);
        teams.Add(teamC);
        teams.Add(teamOverflow);
    }

    public ImmutableList<Team> GetTeams()
    {
        return teams!.ToImmutableList()!;
    }

    public Team? GetActiveTeamInCurrentTime()
    {
        var timeNow = DateTime.Now.TimeOfDay;

        foreach (var team in teams!.OrderByDescending(x => x.Shift.Start))
        {
            var start = team.Shift.Start;
            var end = team.Shift.End;

            bool currentWorkingTeam = start < end
                ? timeNow >= start && timeNow <= end
                : timeNow >= start || timeNow <= end;

            if (currentWorkingTeam)
            {
                return team;
            }
        }
        return null;
    }

    public int GetMaxQueueSize()
    {
        var currentTeam = GetActiveTeamInCurrentTime();
        return QueueSize(currentTeam);
    }
    public int GetOverFlowQueueSize()
    {
        var currentTeam = GetTeams().FirstOrDefault(i => i.TeamName == "Team Overflow");
        return QueueSize(currentTeam);
    }
    public Team? GetOverFlowTeam()
    {
        var currentTeam = GetTeams().FirstOrDefault(i => i.TeamName == "Team Overflow");
        return currentTeam;
    }

    public void DecrementAgentChatCount(Guid? agentId)
    {
        var currentTeam = GetActiveTeamInCurrentTime();
        var teamAgent = currentTeam?.Agents.FirstOrDefault(x =>x.Id == agentId);
        if (teamAgent != null)
        {
            teamAgent.CurrentChatCount--;
        }
    }
    private static int QueueSize(Team? currentTeam)
    {
        var groups = currentTeam?.Agents.GroupBy(c => c.AgentLevel);
        double amount = 0;
        foreach (var group in groups ?? Enumerable.Empty<IGrouping<AgentsLevel, Agent>>())
        {
            var count = group.Count();
            var item = group.FirstOrDefault();
            if (item is not null)
            {
                amount += (item.Efficiency * count * 10);
            }
        }

        return (int)(amount*1.5);
    }
    
}
