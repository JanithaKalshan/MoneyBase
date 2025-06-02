namespace MoneyBase.Domain.Entities
{
    public class Team
    {
        public string TeamName { get; set; } = null!;
        public List<Agent> Agents { get; set; } = null!;
        public Shift Shift { get; set; } = null!;
    }
}
