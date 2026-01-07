namespace AiStilPlay1.Tests;

public class CreateAppointmentTests
{
    private static AppointmentRequest CreateAppointmentRequest()
    {
        var startDate = new DateTime(2024, 6, 1, 10, 0, 0);
        var endDate = startDate.AddHours(1);
        var stylistId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var slot = new Slot(startDate, endDate, stylistId);
        var clientId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        return new AppointmentRequest(slot, clientId);
    }

    [Fact]
    public void ShouldCreateAppointmentWhenSlotAvailable()
    {
        //Arrange
        var request = CreateAppointmentRequest();
        IList<Slot> bookedSlots = [];

        //Act
        AppointmentResponse response = new CreateAppointmentCommand(bookedSlots).Execute(request);

        //Assert
        Assert.NotNull(response);
        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Appointment);     
        Assert.Equal(request.Slot, response.Appointment.Slot);
        Assert.Equal(request.ClientId, response.Appointment.ClientId);   
    }

    [Fact]
    public void ShouldRejectAppointmentWhenSlotNotAvailable()
    {
        //Arrange
        var request = CreateAppointmentRequest();
        IList<Slot> bookedSlots = [];

        new CreateAppointmentCommand(bookedSlots).Execute(request);

        //Act
        AppointmentResponse response = new CreateAppointmentCommand(bookedSlots).Execute(request);

        //Assert
        Assert.NotNull(response);
        Assert.False(response.IsSuccess);     
        Assert.Null(response.Appointment);
    }
}

internal sealed class AppointmentResponse
{
    public bool IsSuccess { get; set; }
    internal Appointment? Appointment { get; set; }
}

internal sealed record Slot(DateTime StartDate, DateTime EndDate, Guid StylistId);

internal sealed record AppointmentRequest(Slot Slot, Guid ClientId);

internal sealed class Appointment
{
    internal required Slot Slot { get; set; }
    public Guid ClientId { get; set; }
}

internal sealed class CreateAppointmentCommand(IList<Slot> bookedSlots)
{
    public AppointmentResponse Execute(AppointmentRequest request)
    {
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