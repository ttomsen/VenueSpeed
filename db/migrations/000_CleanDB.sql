-- =============================================================================
-- VenueSpeed Database Schema V1.0
-- Migration 000: Clean / Teardown
-- Azure SQL · Drops EVERYTHING created by migrations 001-006 in safe
--             reverse-dependency order so the full migration set can be
--             re-run from scratch.
--
-- Idempotent: every DROP is guarded by an existence check.
-- Run this script against the APPLICATION database, then switch to msdb
-- at the bottom to remove the SQL Agent job.
--
-- ORDER OF OPERATIONS
--   1. Stored procedures (006)
--   2. Views              (005)
--   3. Triggers           (004)    -- must precede table drops
--   4. Tables             (002)    -- leaf → root; indexes / constraints
--                                     are dropped automatically with tables
--   5. Partition schemes  (001)    -- must follow tables that used them
--   6. Partition functions(001)    -- must follow schemes
--   7. Maintenance log    (006)    -- standalone; drop last so it captures
--                                     anything that ran during teardown
--   8. SQL Agent job      (006)    -- msdb context
-- =============================================================================


-- =============================================================================
-- 1. Stored Procedures (migration 006)
-- =============================================================================

IF OBJECT_ID(N'dbo.usp_MaintainAuditLogPartitions', N'P') IS NOT NULL
BEGIN
    DROP PROCEDURE dbo.usp_MaintainAuditLogPartitions;
    PRINT 'Dropped: dbo.usp_MaintainAuditLogPartitions';
END
GO

IF OBJECT_ID(N'dbo.usp_MaintainTokenTransactionPartitions', N'P') IS NOT NULL
BEGIN
    DROP PROCEDURE dbo.usp_MaintainTokenTransactionPartitions;
    PRINT 'Dropped: dbo.usp_MaintainTokenTransactionPartitions';
END
GO


-- =============================================================================
-- 2. Views (migration 005)
-- =============================================================================

IF OBJECT_ID(N'dbo.vw_ActiveVenues',        N'V') IS NOT NULL DROP VIEW dbo.vw_ActiveVenues;
IF OBJECT_ID(N'dbo.vw_ActiveEvents',        N'V') IS NOT NULL DROP VIEW dbo.vw_ActiveEvents;
IF OBJECT_ID(N'dbo.vw_ActiveEventBrackets', N'V') IS NOT NULL DROP VIEW dbo.vw_ActiveEventBrackets;
IF OBJECT_ID(N'dbo.vw_ActiveParticipants',  N'V') IS NOT NULL DROP VIEW dbo.vw_ActiveParticipants;
IF OBJECT_ID(N'dbo.vw_EventRegistrations',  N'V') IS NOT NULL DROP VIEW dbo.vw_EventRegistrations;
PRINT 'Dropped: views';
GO


-- =============================================================================
-- 3. Triggers (migration 004)
--    Must be dropped before tables are dropped (though SQL Server drops them
--    automatically with the table; explicit drops make the script clearer and
--    allow partial re-runs).
-- =============================================================================

IF OBJECT_ID(N'dbo.trg_Venue_UpdatedAt',             N'TR') IS NOT NULL DROP TRIGGER dbo.trg_Venue_UpdatedAt;
IF OBJECT_ID(N'dbo.trg_VenueSettings_UpdatedAt',     N'TR') IS NOT NULL DROP TRIGGER dbo.trg_VenueSettings_UpdatedAt;
IF OBJECT_ID(N'dbo.trg_Event_UpdatedAt',             N'TR') IS NOT NULL DROP TRIGGER dbo.trg_Event_UpdatedAt;
IF OBJECT_ID(N'dbo.trg_EventBracket_UpdatedAt',      N'TR') IS NOT NULL DROP TRIGGER dbo.trg_EventBracket_UpdatedAt;
IF OBJECT_ID(N'dbo.trg_Participant_UpdatedAt',        N'TR') IS NOT NULL DROP TRIGGER dbo.trg_Participant_UpdatedAt;
IF OBJECT_ID(N'dbo.trg_EventRegistration_UpdatedAt', N'TR') IS NOT NULL DROP TRIGGER dbo.trg_EventRegistration_UpdatedAt;
IF OBJECT_ID(N'dbo.trg_DrinkToken_UpdatedAt',        N'TR') IS NOT NULL DROP TRIGGER dbo.trg_DrinkToken_UpdatedAt;
IF OBJECT_ID(N'dbo.trg_Payout_UpdatedAt',            N'TR') IS NOT NULL DROP TRIGGER dbo.trg_Payout_UpdatedAt;
PRINT 'Dropped: triggers';
GO


-- =============================================================================
-- 4. Tables — leaf to root (migration 002)
--    Foreign-key order:
--      RoundInterest → BracketRound, Participant, EventBracket, Venue
--      BracketRound  → EventRegistration, EventTable, Participant, EventBracket, Venue
--      AuditLog      → Participant, Venue          (partitioned)
--      TokenTransaction → DrinkToken, Venue        (partitioned)
--      Payout        → EventBracket, Venue
--      EventRegistration → Participant, EventBracket, Venue
--      DrinkToken    → Participant, EventBracket, Venue
--      EventTable    → EventBracket, Venue
--      BracketRound  (already above)
--      EventBracket  → Event, Venue
--      VenueSettings → Venue
--      Event         → Venue
--      Participant   (root)
--      Venue         (root)
--
--    Nonclustered indexes (migration 003) are dropped automatically with
--    their parent tables — no separate DROP INDEX step needed.
-- =============================================================================

IF OBJECT_ID(N'dbo.RoundInterest',     N'U') IS NOT NULL DROP TABLE dbo.RoundInterest;
IF OBJECT_ID(N'dbo.BracketRound',      N'U') IS NOT NULL DROP TABLE dbo.BracketRound;
IF OBJECT_ID(N'dbo.AuditLog',          N'U') IS NOT NULL DROP TABLE dbo.AuditLog;
IF OBJECT_ID(N'dbo.TokenTransaction',  N'U') IS NOT NULL DROP TABLE dbo.TokenTransaction;
IF OBJECT_ID(N'dbo.Payout',            N'U') IS NOT NULL DROP TABLE dbo.Payout;
IF OBJECT_ID(N'dbo.EventRegistration', N'U') IS NOT NULL DROP TABLE dbo.EventRegistration;
IF OBJECT_ID(N'dbo.DrinkToken',        N'U') IS NOT NULL DROP TABLE dbo.DrinkToken;
IF OBJECT_ID(N'dbo.EventTable',        N'U') IS NOT NULL DROP TABLE dbo.EventTable;
IF OBJECT_ID(N'dbo.EventBracket',      N'U') IS NOT NULL DROP TABLE dbo.EventBracket;
IF OBJECT_ID(N'dbo.VenueSettings',     N'U') IS NOT NULL DROP TABLE dbo.VenueSettings;
IF OBJECT_ID(N'dbo.Event',             N'U') IS NOT NULL DROP TABLE dbo.Event;
IF OBJECT_ID(N'dbo.Participant',       N'U') IS NOT NULL DROP TABLE dbo.Participant;
IF OBJECT_ID(N'dbo.Venue',             N'U') IS NOT NULL DROP TABLE dbo.Venue;
PRINT 'Dropped: tables (and their indexes/constraints)';
GO


-- =============================================================================
-- 5. Partition Schemes (migration 001)
--    Must be dropped AFTER the tables that use them.
-- =============================================================================

IF EXISTS (SELECT 1 FROM sys.partition_schemes WHERE name = N'ps_AuditLog_ByDate')
BEGIN
    DROP PARTITION SCHEME ps_AuditLog_ByDate;
    PRINT 'Dropped: ps_AuditLog_ByDate';
END

IF EXISTS (SELECT 1 FROM sys.partition_schemes WHERE name = N'ps_TokenTransaction_ByMonth')
BEGIN
    DROP PARTITION SCHEME ps_TokenTransaction_ByMonth;
    PRINT 'Dropped: ps_TokenTransaction_ByMonth';
END
GO


-- =============================================================================
-- 6. Partition Functions (migration 001)
--    Must be dropped AFTER the schemes that reference them.
-- =============================================================================

IF EXISTS (SELECT 1 FROM sys.partition_functions WHERE name = N'pf_AuditLog_ByDate')
BEGIN
    DROP PARTITION FUNCTION pf_AuditLog_ByDate;
    PRINT 'Dropped: pf_AuditLog_ByDate';
END

IF EXISTS (SELECT 1 FROM sys.partition_functions WHERE name = N'pf_TokenTransaction_ByMonth')
BEGIN
    DROP PARTITION FUNCTION pf_TokenTransaction_ByMonth;
    PRINT 'Dropped: pf_TokenTransaction_ByMonth';
END
GO


-- =============================================================================
-- 7. Maintenance log table (migration 006)
--    Dropped last so it can capture any final log entries from this session.
-- =============================================================================

IF OBJECT_ID(N'dbo.PartitionMaintenanceLog', N'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.PartitionMaintenanceLog;
    PRINT 'Dropped: dbo.PartitionMaintenanceLog';
END
GO


-- =============================================================================
-- 8. SQL Agent job (migration 006) — requires msdb context
--    Removes the schedule attachment, the job step, and the job itself.
--    The schedule is also removed if no other jobs use it.
-- =============================================================================

USE msdb;
GO

DECLARE @job_name      NVARCHAR(128) = N'VenueSpeed - AuditLog Partition Maintenance';
DECLARE @schedule_name NVARCHAR(128) = N'VenueSpeed - Nightly 02:00 UTC';

-- Detach and delete the schedule (sp_delete_schedule with @force_delete=1
-- removes even if attached; safe because this is a VenueSpeed-only schedule).
IF EXISTS (SELECT 1 FROM msdb.dbo.sysschedules WHERE name = @schedule_name)
BEGIN
    EXEC msdb.dbo.sp_delete_schedule
        @schedule_name = @schedule_name,
        @force_delete  = 1;
    PRINT 'Dropped schedule: VenueSpeed - Nightly 02:00 UTC';
END

-- Delete the job (cascades to job steps and server registration).
IF EXISTS (SELECT 1 FROM msdb.dbo.sysjobs WHERE name = @job_name)
BEGIN
    EXEC msdb.dbo.sp_delete_job
        @job_name              = @job_name,
        @delete_unused_schedule = 1;
    PRINT 'Dropped SQL Agent job: VenueSpeed - AuditLog Partition Maintenance';
END
GO

-- Restore context to application database so any subsequent script runs
-- in the correct database without requiring an explicit USE statement.
USE VenueSpeed;
GO

PRINT '=== VenueSpeed CleanDB complete — safe to run 001 through 006 ==='
GO
