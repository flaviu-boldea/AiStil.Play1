namespace AiStilPlay1.App;

public interface ISlotsRepository
{
    bool IsSlotBooked(Slot slot);
    void BookSlot(Slot slot);
}

public sealed class Slots(ISlotsRepository repository)
{
    private IList<Slot> _bookedSlots = [];
    public bool IsSlotBooked(Slot slot) => _bookedSlots.Contains(slot);

    public void BookSlot(Slot slot) => _bookedSlots.Add(slot);
}

public sealed class CreateAppointmentCommand(Slots slots)
{
    public AppointmentResponse Execute(AppointmentRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (slots.IsSlotBooked(request.Slot))
        {
            return new AppointmentResponse 
            { 
                IsSuccess = false 
            };
        }
        slots.BookSlot(request.Slot);

        Appointment appointment = new()
        {
            Slot = request.Slot,
            ClientId = request.ClientId
        };
        return new AppointmentResponse 
        { 
            Appointment = appointment,
            IsSuccess = true 
        };
    }
}
