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
        var clientId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        //Act
        AppointmentResponse response = new CreateAppointmentCommand().Execute(startDate, endDate, stylistId, clientId);

        //Assert
        Assert.NotNull(response);
        Assert.True(response.IsSuccess);        
    }
}

public class AppointmentResponse
{
    public bool IsSuccess { get; set; }
}

public class CreateAppointmentCommand
{
    public AppointmentResponse Execute(DateTime startDate, DateTime endDate, Guid stylistId, Guid clientId)
    {
        return new AppointmentResponse { IsSuccess = true };
    }
}
