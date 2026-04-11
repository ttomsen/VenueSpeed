using MediatR;
using VenueSpeed.Core.CQRS;
using VenueSpeed.Core.DTOs;
using VenueSpeed.Core.Interfaces;

namespace VenueSpeed.Api.Handlers;

public class GetVenueProfileHandler : IRequestHandler<GetVenueProfileQuery, VenueDto?>
{
    private readonly IVenueRepository _repo;
    public GetVenueProfileHandler(IVenueRepository repo) => _repo = repo;

    public Task<VenueDto?> Handle(GetVenueProfileQuery request, CancellationToken ct)
        => _repo.GetByExternalIdAsync(request.ExternalId, request.VenueId);
}

public class UpdateVenueHandler : IRequestHandler<UpdateVenueCommand>
{
    private readonly IVenueRepository _repo;
    public UpdateVenueHandler(IVenueRepository repo) => _repo = repo;

    public Task Handle(UpdateVenueCommand request, CancellationToken ct)
        => _repo.UpdateAsync(request.VenueId, request.Request);
}

public class GetAllVenuesHandler : IRequestHandler<GetAllVenuesQuery, IReadOnlyList<AdminVenueDto>>
{
    private readonly IVenueRepository _repo;
    public GetAllVenuesHandler(IVenueRepository repo) => _repo = repo;

    public Task<IReadOnlyList<AdminVenueDto>> Handle(GetAllVenuesQuery request, CancellationToken ct)
        => _repo.GetAllAsync();
}

public class GetAdminVenueDetailHandler : IRequestHandler<GetAdminVenueDetailQuery, VenueDto?>
{
    private readonly IVenueRepository _repo;
    public GetAdminVenueDetailHandler(IVenueRepository repo) => _repo = repo;

    public Task<VenueDto?> Handle(GetAdminVenueDetailQuery request, CancellationToken ct)
        => _repo.GetBySlugAsync(request.ExternalId.ToString());
}

public class SuspendVenueHandler : IRequestHandler<SuspendVenueCommand>
{
    private readonly IVenueRepository _repo;
    public SuspendVenueHandler(IVenueRepository repo) => _repo = repo;

    public Task Handle(SuspendVenueCommand request, CancellationToken ct)
        => _repo.SuspendAsync(request.ExternalId);
}

public class ReinstateVenueHandler : IRequestHandler<ReinstateVenueCommand>
{
    private readonly IVenueRepository _repo;
    public ReinstateVenueHandler(IVenueRepository repo) => _repo = repo;

    public Task Handle(ReinstateVenueCommand request, CancellationToken ct)
        => _repo.ReinstateAsync(request.ExternalId);
}
