-- =============================================================================
-- VenueSpeed Database Schema V1.0
-- Migration 005: Soft-Delete Views
-- Azure SQL · Run AFTER 002_create_tables.sql
-- The C# API queries these views — never the base tables directly.
-- Tables without IsDeleted (VenueSettings, EventTable, BracketRound,
-- RoundInterest, DrinkToken, TokenTransaction, Payout, AuditLog) do not
-- need a soft-delete view.
-- =============================================================================

CREATE OR ALTER VIEW dbo.vw_ActiveVenues AS
    SELECT * FROM dbo.Venue WHERE IsDeleted = 0;
GO

CREATE OR ALTER VIEW dbo.vw_ActiveEvents AS
    SELECT * FROM dbo.Event WHERE IsDeleted = 0;
GO

CREATE OR ALTER VIEW dbo.vw_ActiveEventBrackets AS
    SELECT * FROM dbo.EventBracket WHERE IsDeleted = 0;
GO

CREATE OR ALTER VIEW dbo.vw_ActiveParticipants AS
    SELECT * FROM dbo.Participant WHERE IsDeleted = 0;
GO

-- EventRegistration has no IsDeleted column; expose all rows via view
-- so the API always queries through a consistent view layer.
CREATE OR ALTER VIEW dbo.vw_EventRegistrations AS
    SELECT * FROM dbo.EventRegistration;
GO
