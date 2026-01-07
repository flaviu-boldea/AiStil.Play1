namespace AiStilPlay1.Tests;

public class CreateAppointmentTests
{
    [Fact]
    public void CanCreateAppointment()
    {
        //Arrange
        var startDate = new DateTime(2024, 6, 1, 10, 0, 0);
        var endDate = startDate.AddHours(1);
        var stylistId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var slot = new Slot(startDate, endDate, stylistId);
        var clientId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        //Act
        AppointmentResponse response = CreateAppointmentCommand.Execute(slot, clientId);

        //Assert
        Assert.NotNull(response);
        Assert.True(response.IsSuccess);        
    }
}

internal sealed class AppointmentResponse
{
    public bool IsSuccess { get; set; }
    internal required Appointment Appointment { get; set; }
}

internal sealed record Slot(DateTime StartDate, DateTime EndDate, Guid StylistId);


internal sealed class Appointment
{
    internal required Slot Slot { get; set; }
    public Guid ClientId { get; set; }
}

internal static class CreateAppointmentCommand
{
    public static AppointmentResponse Execute(Slot slot, Guid clientId)
    {
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