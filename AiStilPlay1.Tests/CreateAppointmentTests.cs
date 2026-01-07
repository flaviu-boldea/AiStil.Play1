namespace AiStilPlay1.Tests;

using AiStilPlay1.App;

internal sealed class SlotsRepositoryStub: ISlotsRepository
{
    private IList<Slot> _bookedSlots = [];

    public bool IsSlotBooked(Slot slot)
    {
        return _bookedSlots.Contains(slot);
    }

    public void BookSlot(Slot slot)
    {
        _bookedSlots.Add(slot);
    }
}

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

        //Act
        AppointmentResponse response = new CreateAppointmentCommand(new Slots(new SlotsRepositoryStub())).Execute(request);

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
        var slots = new Slots(new SlotsRepositoryStub());

        new CreateAppointmentCommand(slots).Execute(request);

        //Act
        AppointmentResponse response = new CreateAppointmentCommand(slots).Execute(request);

        //Assert
        Assert.NotNull(response);
        Assert.False(response.IsSuccess);     
        Assert.Null(response.Appointment);
    }

    [Fact]
    public void ShouldThrowWhenRequestIsNull()
    {
        //Arrange
        var command = new CreateAppointmentCommand(new Slots(new SlotsRepositoryStub()));

        //Act & Assert
        Assert.Throws<ArgumentNullException>(() => command.Execute(null!));
    }
}