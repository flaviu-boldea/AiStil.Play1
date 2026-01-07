namespace AiStilPlay1.App;

public interface ISlotsRepository
{
    bool IsSlotBooked(Guid stylistId, Slot slot);
    void BookSlot(Guid stylistId, Slot slot);
}

public interface IStylistRepository
{
    Stylist GetById(Guid id);
}

public sealed class CreateAppointmentCommand(IStylistRepository stylistRepository)
{
    public AppointmentResponse Execute(AppointmentRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var stylist = stylistRepository.GetById(request.Slot.StylistId);
        var appointment = stylist.MakeAppointment(request.Slot, request.ClientId);
        
        return new AppointmentResponse
        {
            Appointment = appointment,
            IsSuccess = appointment != null
        };
    }
}
