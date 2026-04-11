using MediatR;
using VenueSpeed.Core.CQRS;
using VenueSpeed.Core.DTOs;
using VenueSpeed.Core.Interfaces;

namespace VenueSpeed.Api.Handlers;

public class GetParticipantProfileHandler : IRequestHandler<GetParticipantProfileQuery, ParticipantDto?>
{
    private readonly IParticipantRepository _repo;
    public GetParticipantProfileHandler(IParticipantRepository repo) => _repo = repo;

    public Task<ParticipantDto?> Handle(GetParticipantProfileQuery request, CancellationToken ct)
        => _repo.GetByAuthProviderIdAsync(request.AuthProviderId);
}

public class UpdateParticipantHandler : IRequestHandler<UpdateParticipantCommand>
{
    private readonly IParticipantRepository _repo;
    public UpdateParticipantHandler(IParticipantRepository repo) => _repo = repo;

    public Task Handle(UpdateParticipantCommand request, CancellationToken ct)
        => _repo.UpdateAsync(request.AuthProviderId, request.Request);
}

public class DeleteParticipantHandler : IRequestHandler<DeleteParticipantCommand>
{
    private readonly IParticipantRepository _repo;
    public DeleteParticipantHandler(IParticipantRepository repo) => _repo = repo;

    public Task Handle(DeleteParticipantCommand request, CancellationToken ct)
        => _repo.GdprDeleteAsync(request.AuthProviderId);
}
