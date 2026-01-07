# Architecture Decisions - Appointment Booking System

## Date: January 7, 2026

---

## Decision 1: CreateAppointmentCommand Dependencies

### Context
`CreateAppointmentCommand` needs access to booked slots stored in a database. Two options were considered:
1. Direct dependency on `ISlotsRepository`
2. Dependency on `Slots` abstraction that wraps `ISlotsRepository`

### Decision
**Keep the `Slots` abstraction** (Option 2)

### Rationale
- Provides single place for slot-specific business logic (validation, time ranges, etc.)
- Keeps command focused on orchestration rather than data access details
- Better testability - can mock `Slots` with predictable behavior
- Natural home for future features (caching, validation, complex slot logic)
- Separates slot domain logic from infrastructure concerns

### Implementation
```csharp
public sealed class Slots(ISlotsRepository repository)
{
    public bool IsSlotBooked(Slot slot) => repository.IsSlotBooked(slot);
    public void BookSlot(Slot slot) => repository.BookSlot(slot);
}

public sealed class CreateAppointmentCommand(Slots slots)
{
    public AppointmentResponse Execute(AppointmentRequest request)
    {
        // Command orchestrates, Slots handles slot operations
    }
}
```

---

## Decision 2: Caching Strategy

### Context
Need to cache slot availability data to reduce database load. Considered:
1. In-memory collection in `Slots` with load/save methods
2. Repository decorator pattern
3. Distributed cache (Redis)
4. Request-scoped caching

### Decision
**Use Decorator Pattern** (Option 1) for initial implementation

### Rationale
- **Critical risk with in-memory approach**: Concurrency issues where two users could book the same slot
- **Decorator benefits**:
  - Separation of concerns (caching separate from business logic)
  - Easy to enable/disable
  - Thread-safe (MemoryCache handles locking)
  - No changes to `Slots` or command classes
- Can upgrade to distributed cache later if needed

### Implementation
```csharp
public class CachedSlotsRepository : ISlotsRepository
{
    private readonly ISlotsRepository _inner;
    private readonly IMemoryCache _cache;
    
    public bool IsSlotBooked(Slot slot)
    {
        string key = $"slot:{slot.StylistId}:{slot.StartTime}";
        return _cache.GetOrCreate(key, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            return _inner.IsSlotBooked(slot);
        });
    }
    
    public void BookSlot(Slot slot)
    {
        _inner.BookSlot(slot);
        // Invalidate cache after write
        _cache.Remove($"slot:{slot.StylistId}:{slot.StartTime}");
    }
}
```

### Rejected: Load/Save Pattern
In-memory collection with explicit load/save creates race conditions in concurrent booking scenarios and unclear lifecycle management.

---

## Decision 3: Introduce Stylist Domain Object

### Context
`CreateAppointmentCommand` was doing too much:
- Querying slot status (Ask)
- Making booking decisions
- Creating appointments

This violates "Tell, Don't Ask" principle and leads to anemic domain model.

### Decision
**Introduce `Stylist` class** to own appointment booking logic

### Rationale
- **Tell, Don't Ask**: Command tells stylist to make appointment, doesn't ask about status
- **Information Expert**: Stylist knows their schedule and makes decisions
- **Rich Domain Model**: Business logic lives in domain objects, not commands
- **Better encapsulation**: Slots are private to the stylist
- **Natural extensibility**: Easy to add stylist-specific rules (working hours, skills, break times)

### Implementation
```csharp
public sealed class Stylist
{
    private readonly ISlotsRepository _slotsRepository;
    public string Id { get; }
    
    public Stylist(string id, string name, ISlotsRepository slotsRepository)
    {
        Id = id;
        Name = name;
        _slotsRepository = slotsRepository;
    }
    
    public Result<Appointment> MakeAppointment(Slot slot, string clientId)
    {
        // Stylist owns the decision-making
        if (_slotsRepository.IsSlotBooked(Id, slot))
            return Result<Appointment>.Failure("Slot already booked");
        
        _slotsRepository.BookSlot(Id, slot);
        
        return Result<Appointment>.Success(new Appointment
        {
            Slot = slot,
            ClientId = clientId,
            StylistId = Id
        });
    }
}

// Command becomes pure orchestration
public sealed class CreateAppointmentCommand(IStylistRepository stylistRepo)
{
    public AppointmentResponse Execute(AppointmentRequest request)
    {
        var stylist = stylistRepo.GetById(request.StylistId);
        var result = stylist.MakeAppointment(request.Slot, request.ClientId);
        
        return new AppointmentResponse 
        { 
            Appointment = result.Value,
            IsSuccess = result.IsSuccess
        };
    }
}
```

---

## Decision 4: Repository Usage in Domain Objects

### Context
Initial concern about repository calls appearing in multiple domain classes. Considered:
1. Direct repository usage in domain objects
2. Load data in command, pass to domain objects
3. Higher-level service abstraction

### Decision
**Domain objects use repositories directly** (Option 1)

### Rationale
- **Standard DDD practice**: Repositories are infrastructure, like DB-backed collections
- **Transactional safety**: Repository reads/writes are atomic
- **Testability**: Easy to mock repositories
- **Simplicity**: No unnecessary layers

### Rejected Alternatives
- **Loading in command**: Reintroduces concurrency issues and unclear state management
- **Service layer**: Adds complexity without clear benefit for this use case

### Key Insight
Discomfort with repository calls is often unfamiliarity - it's actually the correct pattern. Domain objects should contain business logic and delegate data access to repositories.

---

## Decision 5: CQRS - Queries vs Repositories

### Context
Need to display available slots to users. Question: Should domain objects use queries or repositories?

### Decision
- **Use Queries at use case layer** for read operations (display, reports)
- **Use Repositories in domain objects** for write operations

### Rationale

**Queries for Reads:**
- Optimized for display (DTOs, denormalized views)
- Can be cached aggressively
- Read-heavy operations
- Eventually consistent is acceptable

**Repositories for Writes:**
- Transactional consistency required
- Real-time state for business decisions
- Avoids race conditions (query might be stale)
- Single transaction for check-and-write operations

### Example
```csharp
// READ SIDE - Query for display
public interface IGetAvailableSlotsQuery
{
    IEnumerable<SlotDto> Execute(string stylistId, DateOnly date);
}

// WRITE SIDE - Repository for domain logic
public class Stylist
{
    private readonly ISlotsRepository _slotsRepo; // NOT Query!
    
    public Result MakeAppointment(Slot slot, string clientId)
    {
        if (_slotsRepo.IsSlotBooked(Id, slot)) // Transactionally safe
            return Failure();
        
        _slotsRepo.BookSlot(Id, slot);
        return Success();
    }
}
```

### Anti-Pattern to Avoid
```csharp
// RACE CONDITION:
if (!_query.IsAvailable(slot))  // Read from query (possibly cached/stale)
    _repo.BookSlot(slot);        // Write - slot might already be booked!
```

---

## Summary of Key Principles

1. **Separation of Concerns**: Keep caching, data access, and business logic separate
2. **Tell, Don't Ask**: Domain objects make decisions, commands orchestrate
3. **Concurrency Safety**: Use repositories for transactional operations in write paths
4. **CQRS**: Separate read (queries) and write (repositories) models
5. **Rich Domain Model**: Business logic lives in domain objects, not commands/services
6. **Testability**: Design with testing in mind (dependency injection, mocking)

---

## Future Considerations

- Implement decorator pattern for caching when performance metrics indicate need
- Add optimistic concurrency control for high-traffic scenarios
- Consider event sourcing if audit trail of all bookings is required
- Evaluate distributed cache (Redis) when scaling beyond single instance
