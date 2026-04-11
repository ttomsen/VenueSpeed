using MediatR;
using VenueSpeed.Core.DTOs;

namespace VenueSpeed.Core.CQRS;

public record GetParticipantProfileQuery(string AuthProviderId) : IRequest<ParticipantDto?>;

public record UpdateParticipantCommand(string AuthProviderId, UpdateParticipantRequest Request) : IRequest;

public record DeleteParticipantCommand(string AuthProviderId) : IRequest;
