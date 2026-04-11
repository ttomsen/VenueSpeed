-- =============================================================================
-- VenueSpeed Database Schema V1.0
-- Migration 003: Indexes
-- Azure SQL · Run AFTER 002_create_tables.sql
-- =============================================================================

-- Venue
CREATE NONCLUSTERED INDEX IX_Venue_ExternalId ON dbo.Venue (ExternalId);
CREATE NONCLUSTERED INDEX IX_Venue_Slug       ON dbo.Venue (Slug);
GO

-- Event
CREATE NONCLUSTERED INDEX IX_Event_VenueId_Status  ON dbo.Event (VenueId, Status) INCLUDE (EventName, EventDateUtc, IsDeleted);
CREATE NONCLUSTERED INDEX IX_Event_EventDateUtc     ON dbo.Event (EventDateUtc) WHERE IsDeleted = 0;
GO

-- EventBracket
CREATE NONCLUSTERED INDEX IX_EventBracket_EventId          ON dbo.EventBracket (EventId);
CREATE NONCLUSTERED INDEX IX_EventBracket_VenueId_Status   ON dbo.EventBracket (VenueId, Status) INCLUDE (EventId, BracketName, StartTimeUtc);
GO

-- Participant
CREATE NONCLUSTERED INDEX IX_Participant_AuthProviderId ON dbo.Participant (AuthProviderId);
GO

-- EventRegistration
CREATE NONCLUSTERED INDEX IX_EventRegistration_BracketId_ParticipantId
    ON dbo.EventRegistration (EventBracketId, ParticipantId) INCLUDE (PaymentStatus, CheckInStatus);

CREATE NONCLUSTERED INDEX IX_EventRegistration_VenueId_CheckInStatus
    ON dbo.EventRegistration (VenueId, CheckInStatus) INCLUDE (EventBracketId, ParticipantId, WaitlistPosition);
GO

-- BracketRound
CREATE NONCLUSTERED INDEX IX_BracketRound_BracketId_RoundNumber
    ON dbo.BracketRound (EventBracketId, RoundNumber) INCLUDE (ParticipantId, TableId, SeatLabel, PartnerId, IsByeRound);
GO

-- RoundInterest
CREATE NONCLUSTERED INDEX IX_RoundInterest_From_To_Bracket
    ON dbo.RoundInterest (FromParticipantId, ToParticipantId, EventBracketId) INCLUDE (InterestLevel, IsTopChoice);
GO

-- DrinkToken
CREATE NONCLUSTERED INDEX IX_DrinkToken_RedemptionCode       ON dbo.DrinkToken (RedemptionCode);
CREATE NONCLUSTERED INDEX IX_DrinkToken_ParticipantId_Status ON dbo.DrinkToken (ParticipantId, Status) INCLUDE (VenueId, ExpiresAtUtc, RedemptionCode);
GO

-- Payout
CREATE NONCLUSTERED INDEX IX_Payout_VenueId_Status ON dbo.Payout (VenueId, Status) INCLUDE (EventBracketId, GrossTicketRevenueUsd, VenueShareUsd, TransferredAtUtc);
GO

-- AuditLog (partitioned — index is partition-aligned)
CREATE NONCLUSTERED INDEX IX_AuditLog_VenueId_LogDateKey
    ON dbo.AuditLog (VenueId, LogDateKey) INCLUDE (ActorType, Action, EntityType, EntityId)
    ON ps_AuditLog_ByDate (LogDateKey);
GO
