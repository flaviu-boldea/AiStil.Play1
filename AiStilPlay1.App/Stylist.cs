namespace AiStilPlay1.App;

public sealed class Stylist
{
    private readonly ISlotsRepository _slotsRepository;
    
    public Guid Id { get; }
    public string Name { get; }
    
    public Stylist(Guid id, string name, ISlotsRepository slotsRepository)
    {
        Id = id;
        Name = name;
        _slotsRepository = slotsRepository;
    }
    
    public Appointment? MakeAppointment(Slot slot, Guid clientId)
    {
        if (_slotsRepository.IsSlotBooked(Id, slot))
        {
            return null;
        }
        
        _slotsRepository.BookSlot(Id, slot);
        
        return new Appointment
        {
            Slot = slot,
            ClientId = clientId
        };
    }
}
