-- =============================================================================
-- VenueSpeed Database Schema V1.0
-- Migration 004: UpdatedAtUtc Triggers
-- Azure SQL · Run AFTER 002_create_tables.sql
-- Applied to every table that has an UpdatedAtUtc column
-- =============================================================================

CREATE OR ALTER TRIGGER trg_Venue_UpdatedAt
ON dbo.Venue AFTER UPDATE AS
    UPDATE dbo.Venue SET UpdatedAtUtc = GETUTCDATE()
    WHERE Id IN (SELECT Id FROM inserted);
GO

CREATE OR ALTER TRIGGER trg_VenueSettings_UpdatedAt
ON dbo.VenueSettings AFTER UPDATE AS
    UPDATE dbo.VenueSettings SET UpdatedAtUtc = GETUTCDATE()
    WHERE Id IN (SELECT Id FROM inserted);
GO

CREATE OR ALTER TRIGGER trg_Event_UpdatedAt
ON dbo.Event AFTER UPDATE AS
    UPDATE dbo.Event SET UpdatedAtUtc = GETUTCDATE()
    WHERE Id IN (SELECT Id FROM inserted);
GO

CREATE OR ALTER TRIGGER trg_EventBracket_UpdatedAt
ON dbo.EventBracket AFTER UPDATE AS
    UPDATE dbo.EventBracket SET UpdatedAtUtc = GETUTCDATE()
    WHERE Id IN (SELECT Id FROM inserted);
GO

CREATE OR ALTER TRIGGER trg_Participant_UpdatedAt
ON dbo.Participant AFTER UPDATE AS
    UPDATE dbo.Participant SET UpdatedAtUtc = GETUTCDATE()
    WHERE Id IN (SELECT Id FROM inserted);
GO

CREATE OR ALTER TRIGGER trg_EventRegistration_UpdatedAt
ON dbo.EventRegistration AFTER UPDATE AS
    UPDATE dbo.EventRegistration SET UpdatedAtUtc = GETUTCDATE()
    WHERE Id IN (SELECT Id FROM inserted);
GO

CREATE OR ALTER TRIGGER trg_DrinkToken_UpdatedAt
ON dbo.DrinkToken AFTER UPDATE AS
    UPDATE dbo.DrinkToken SET UpdatedAtUtc = GETUTCDATE()
    WHERE Id IN (SELECT Id FROM inserted);
GO

CREATE OR ALTER TRIGGER trg_Payout_UpdatedAt
ON dbo.Payout AFTER UPDATE AS
    UPDATE dbo.Payout SET UpdatedAtUtc = GETUTCDATE()
    WHERE Id IN (SELECT Id FROM inserted);
GO
