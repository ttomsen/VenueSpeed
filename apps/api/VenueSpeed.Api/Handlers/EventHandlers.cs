using MediatR;
using VenueSpeed.Core.CQRS;
using VenueSpeed.Core.DTOs;
using VenueSpeed.Core.Interfaces;

namespace VenueSpeed.Api.Handlers;

public class GetEventsHandler : IRequestHandler<GetEventsQuery, IReadOnlyList<EventDto>>
{
    private readonly IEventRepository _repo;
    public GetEventsHandler(IEventRepository repo) => _repo = repo;

    public Task<IReadOnlyList<EventDto>> Handle(GetEventsQuery request, CancellationToken ct)
        => _repo.GetAllByVenueAsync(request.VenueId);
}

public class GetEventHandler : IRequestHandler<GetEventQuery, EventDto?>
{
    private readonly IEventRepository _repo;
    public GetEventHandler(IEventRepository repo) => _repo = repo;

    public Task<EventDto?> Handle(GetEventQuery request, CancellationToken ct)
        => _repo.GetByExternalIdAsync(request.ExternalId, request.VenueId);
}

public class CreateEventHandler : IRequestHandler<CreateEventCommand, Guid>
{
    private readonly IEventRepository _repo;
    public CreateEventHandler(IEventRepository repo) => _repo = repo;

    public Task<Guid> Handle(CreateEventCommand request, CancellationToken ct)
        => _repo.CreateAsync(request.VenueId, request.Request);
}

public class UpdateEventHandler : IRequestHandler<UpdateEventCommand>
{
    private readonly IEventRepository _repo;
    public UpdateEventHandler(IEventRepository repo) => _repo = repo;

    public Task Handle(UpdateEventCommand request, CancellationToken ct)
        => _repo.UpdateAsync(request.ExternalId, request.VenueId, request.Request);
}

public class DeleteEventHandler : IRequestHandler<DeleteEventCommand>
{
    private readonly IEventRepository _repo;
    public DeleteEventHandler(IEventRepository repo) => _repo = repo;

    public Task Handle(DeleteEventCommand request, CancellationToken ct)
        => _repo.SoftDeleteAsync(request.ExternalId, request.VenueId);
}
