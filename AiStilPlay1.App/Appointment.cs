namespace AiStilPlay1.App;

public sealed class Appointment
{
    public required Slot Slot { get; set; }
    public Guid ClientId { get; set; }
}
