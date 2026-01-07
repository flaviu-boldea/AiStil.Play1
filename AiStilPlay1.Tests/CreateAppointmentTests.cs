namespace AiStilPlay1.Tests;

public class CreateAppointmentTests
{
    [Fact]
    public void ShouldCreateAppointmentWhenSlotAvailable()
    {
        //Arrange
        var startDate = new DateTime(2024, 6, 1, 10, 0, 0);
        var endDate = startDate.AddHours(1);
        var stylistId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var slot = new Slot(startDate, endDate, stylistId);
        var clientId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        IList<Slot> bookedSlots = [];

        //Act
        AppointmentResponse response = CreateAppointmentCommand.Execute(slot, clientId, bookedSlots);

        //Assert
        Assert.NotNull(response);
        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Appointment);     
        Assert.Equal(slot, response.Appointment.Slot);
        Assert.Equal(clientId, response.Appointment.ClientId);   
    }

    [Fact]
    public void ShouldRejectAppointmentWhenSlotNotAvailable()
    {
        //Arrange
        var startDate = new DateTime(2024, 6, 1, 10, 0, 0);
        var endDate = startDate.AddHours(1);
        var stylistId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var slot = new Slot(startDate, endDate, stylistId);
        var clientId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        IList<Slot> bookedSlots = [];

        CreateAppointmentCommand.Execute(slot, clientId, bookedSlots);

        //Act
        AppointmentResponse response = CreateAppointmentCommand.Execute(slot, clientId, bookedSlots);

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


internal sealed class Appointment
{
    internal required Slot Slot { get; set; }
    public Guid ClientId { get; set; }
}

internal static class CreateAppointmentCommand
{
    public static AppointmentResponse Execute(Slot slot, Guid clientId, IList<Slot> bookedSlots)
    {
        if (bookedSlots.Contains(slot))
        {
            return new AppointmentResponse 
            { 
                IsSuccess = false 
            };
        }
        bookedSlots.Add(slot);

        Appointment appointment = new()
        {
            Slot = slot,
            ClientId = clientId
        };
        return new AppointmentResponse 
        { 
            Appointment = appointment,
            IsSuccess = true 
        };
    }
}