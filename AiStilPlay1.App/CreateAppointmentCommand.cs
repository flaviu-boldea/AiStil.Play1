namespace AiStilPlay1.App;

public sealed class CreateAppointmentCommand(IList<Slot> bookedSlots)
{
    public AppointmentResponse Execute(AppointmentRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (bookedSlots.Contains(request.Slot))
        {
            return new AppointmentResponse 
            { 
                IsSuccess = false 
            };
        }
        bookedSlots.Add(request.Slot);

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
