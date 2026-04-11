using MediatR;
using VenueSpeed.Core.DTOs;

namespace VenueSpeed.Core.CQRS;

public record GetBracketsQuery(Guid EventExternalId, long VenueId) : IRequest<IReadOnlyList<BracketDto>>;

public record CreateBracketCommand(Guid EventExternalId, long VenueId, CreateBracketRequest Request) : IRequest<Guid>;

public record UpdateBracketCommand(Guid BracketExternalId, long VenueId, UpdateBracketRequest Request) : IRequest;

public record DeleteBracketCommand(Guid BracketExternalId, long VenueId) : IRequest;
