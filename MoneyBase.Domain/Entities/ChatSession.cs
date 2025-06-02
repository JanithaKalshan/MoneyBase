namespace MoneyBase.Domain.Entities;

public class ChatSession
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserId { get; set; } = null!;
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public int MissedPolls { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public DateTime LastPolledAt { get; set; } = DateTime.Now;
    public Agent? AssignedAgent { get; set; } = null;
}
