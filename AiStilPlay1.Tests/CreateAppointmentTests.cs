namespace AiStilPlay1.Tests;

using AiStilPlay1.App;

internal sealed class SlotsRepositoryStub : ISlotsRepository
{
    private readonly Dictionary<Guid, List<Slot>> _bookedSlotsByStylist = new();

    public bool IsSlotBooked(Guid stylistId, Slot slot)
    {
        if (!_bookedSlotsByStylist.TryGetValue(stylistId, out var slots))
            return false;
        
        return slots.Contains(slot);
    }

    public void BookSlot(Guid stylistId, Slot slot)
    {
        if (!_bookedSlotsByStylist.TryGetValue(stylistId, out var slots))
        {
            slots = new List<Slot>();
            _bookedSlotsByStylist[stylistId] = slots;
        }
        
        slots.Add(slot);
    }
}

internal sealed class StylistRepositoryStub : IStylistRepository
{
    private readonly ISlotsRepository _slotsRepository;

    public StylistRepositoryStub(ISlotsRepository slotsRepository)
    {
        _slotsRepository = slotsRepository;
    }

    public Stylist GetById(Guid id)
    {
        return new Stylist(id, $"Stylist {id}", _slotsRepository);
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
        var slotsRepo = new SlotsRepositoryStub();
        var stylistRepo = new StylistRepositoryStub(slotsRepo);
        var command = new CreateAppointmentCommand(stylistRepo);

        //Act
        AppointmentResponse response = command.Execute(request);

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
        var slotsRepo = new SlotsRepositoryStub();
        var stylistRepo = new StylistRepositoryStub(slotsRepo);
        var command = new CreateAppointmentCommand(stylistRepo);

        // Book the slot first
        command.Execute(request);

        //Act
        AppointmentResponse response = command.Execute(request);

        //Assert
        Assert.NotNull(response);
        Assert.False(response.IsSuccess);     
        Assert.Null(response.Appointment);
    }

    [Fact]
    public void ShouldThrowWhenRequestIsNull()
    {
        //Arrange
        var slotsRepo = new SlotsRepositoryStub();
        var stylistRepo = new StylistRepositoryStub(slotsRepo);
        var command = new CreateAppointmentCommand(stylistRepo);

        //Act & Assert
        Assert.Throws<ArgumentNullException>(() => command.Execute(null!));
    }
}