using MoneyBase.Domain.Enums;

namespace MoneyBase.Domain.Entities;

public class Shift
{
    public ShiftTypes Type { get; set; }
    public TimeSpan Start { get; set; }
    public TimeSpan End { get; set; }
    public bool IsActiveNow() => DateTime.Now.TimeOfDay >= Start && DateTime.Now.TimeOfDay <= End;
}
