using MediatR;
using VenueSpeed.Core.CQRS;
using VenueSpeed.Core.DTOs;
using VenueSpeed.Core.Interfaces;

namespace VenueSpeed.Api.Handlers;

public class CreateRegistrationHandler : IRequestHandler<CreateRegistrationCommand, Guid>
{
    private readonly IEventRegistrationRepository _repo;
    public CreateRegistrationHandler(IEventRegistrationRepository repo) => _repo = repo;

    public Task<Guid> Handle(CreateRegistrationCommand request, CancellationToken ct)
        => _repo.CreateAsync(request.Request, request.AuthProviderId);
}

public class CancelRegistrationHandler : IRequestHandler<CancelRegistrationCommand>
{
    private readonly IEventRegistrationRepository _repo;
    public CancelRegistrationHandler(IEventRegistrationRepository repo) => _repo = repo;

    public Task Handle(CancelRegistrationCommand request, CancellationToken ct)
        => _repo.CancelAsync(request.ExternalId, request.AuthProviderId);
}

public class GetMyEventsHandler : IRequestHandler<GetMyEventsQuery, IReadOnlyList<MyEventDto>>
{
    private readonly IEventRegistrationRepository _repo;
    public GetMyEventsHandler(IEventRegistrationRepository repo) => _repo = repo;

    public Task<IReadOnlyList<MyEventDto>> Handle(GetMyEventsQuery request, CancellationToken ct)
        => _repo.GetUpcomingByParticipantAsync(request.AuthProviderId);
}
