using MediatR;
using VenueSpeed.Core.CQRS;
using VenueSpeed.Core.DTOs;
using VenueSpeed.Core.Interfaces;

namespace VenueSpeed.Api.Handlers;

public class GetBracketsHandler : IRequestHandler<GetBracketsQuery, IReadOnlyList<BracketDto>>
{
    private readonly IEventBracketRepository _repo;
    public GetBracketsHandler(IEventBracketRepository repo) => _repo = repo;

    public Task<IReadOnlyList<BracketDto>> Handle(GetBracketsQuery request, CancellationToken ct)
        => _repo.GetByEventAsync(request.EventExternalId, request.VenueId);
}

public class CreateBracketHandler : IRequestHandler<CreateBracketCommand, Guid>
{
    private readonly IEventBracketRepository _repo;
    public CreateBracketHandler(IEventBracketRepository repo) => _repo = repo;

    public Task<Guid> Handle(CreateBracketCommand request, CancellationToken ct)
        => _repo.CreateAsync(request.EventExternalId, request.VenueId, request.Request);
}

public class UpdateBracketHandler : IRequestHandler<UpdateBracketCommand>
{
    private readonly IEventBracketRepository _repo;
    public UpdateBracketHandler(IEventBracketRepository repo) => _repo = repo;

    public Task Handle(UpdateBracketCommand request, CancellationToken ct)
        => _repo.UpdateAsync(request.BracketExternalId, request.VenueId, request.Request);
}

public class DeleteBracketHandler : IRequestHandler<DeleteBracketCommand>
{
    private readonly IEventBracketRepository _repo;
    public DeleteBracketHandler(IEventBracketRepository repo) => _repo = repo;

    public Task Handle(DeleteBracketCommand request, CancellationToken ct)
        => _repo.SoftDeleteAsync(request.BracketExternalId, request.VenueId);
}
