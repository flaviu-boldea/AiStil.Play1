namespace AiStilPlay1.App;

public interface ISlotsRepository
{
    bool IsSlotBooked(Slot slot);
    void BookSlot(Slot slot);
}

public sealed class CreateAppointmentCommand(ISlotsRepository slotsRepository)
{
    public AppointmentResponse Execute(AppointmentRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (slotsRepository.IsSlotBoocked(request.Slot))
        {
            return new AppointmentResponse 
            { 
                IsSuccess = false 
            };
        }
        slotsRepository.BookSlot(request.Slot);

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
