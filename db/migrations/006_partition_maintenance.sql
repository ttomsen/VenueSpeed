-- =============================================================================
-- VenueSpeed Database Schema V1.0
-- Migration 006: Partition Maintenance Infrastructure
-- Azure SQL · Run AFTER 001_partition_setup.sql and 002_create_tables.sql
--
-- Creates:
--   dbo.PartitionMaintenanceLog          — audit trail for every maintenance run
--   dbo.usp_MaintainAuditLogPartitions   — daily rolling-window upkeep
--   dbo.usp_MaintainTokenTransactionPartitions — monthly rolling-window upkeep
--
-- SQL Agent job block at the bottom schedules the nightly AuditLog job at
-- 02:00 UTC.  Requires SQL Server Agent (SQL Managed Instance or on-prem).
-- For Azure SQL Database (PaaS), replace with an Azure Elastic Job targeting
-- the same EXEC statement on the same schedule.
--
-- Idempotent: safe to run multiple times without errors.
-- =============================================================================


-- =============================================================================
-- Maintenance log table
-- =============================================================================

IF OBJECT_ID(N'dbo.PartitionMaintenanceLog', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.PartitionMaintenanceLog
    (
        LogID          INT           NOT NULL IDENTITY(1,1) CONSTRAINT PK_PartitionMaintenanceLog PRIMARY KEY,
        ProcedureName  NVARCHAR(128) NOT NULL,
        ActionTaken    NVARCHAR(500) NOT NULL,
        OldBoundary    INT               NULL,   -- value that was merged out (NULL when skipped)
        NewBoundary    INT               NULL,   -- value that was split in  (NULL when skipped)
        ExecutedAt     DATETIME2(3)  NOT NULL CONSTRAINT DF_PML_ExecutedAt DEFAULT SYSUTCDATETIME()
    );
    PRINT 'Created dbo.PartitionMaintenanceLog.';
END
ELSE
    PRINT 'dbo.PartitionMaintenanceLog already exists — skipped.';
GO


-- =============================================================================
-- usp_MaintainAuditLogPartitions
--
-- Called nightly by the SQL Agent job below (see bottom of this file).
-- Logic:
--   1. Compute today's target forward boundary  (today + 3 days, YYYYMMDD INT).
--   2. Idempotency guard — if that boundary already exists, log and return.
--   3. Merge (remove) the current oldest boundary.
--      This collapses the pre-window overflow partition (partition 1, always
--      empty) and the oldest-day partition into one.  Data older than the
--      window should be purged before calling this proc; see note below.
--   4. Split in the new boundary, extending the window by one day.
--   5. Log both boundary values to PartitionMaintenanceLog.
--
-- Data-purge note:
--   MERGE RANGE does not delete rows — it merges two adjacent partitions.
--   To keep partition 1 (the pre-window sink) empty before each merge, add
--   a DELETE / TRUNCATE on AuditLog filtered to the oldest boundary before
--   calling this proc, or wrap it in a larger ETL job that archives first.
-- =============================================================================

CREATE OR ALTER PROCEDURE dbo.usp_MaintainAuditLogPartitions
AS
BEGIN
    SET NOCOUNT ON;

    -- -------------------------------------------------------------------------
    -- Compute the new boundary value: today (UTC) + 3 days as YYYYMMDD INT
    -- -------------------------------------------------------------------------
    DECLARE @FutureDate     DATE = DATEADD(DAY, 3, CAST(GETUTCDATE() AS DATE));
    DECLARE @NewBoundary    INT  = YEAR(@FutureDate)  * 10000
                                 + MONTH(@FutureDate) * 100
                                 + DAY(@FutureDate);

    -- -------------------------------------------------------------------------
    -- Idempotency: if the boundary for today+3 already exists, skip and log
    -- -------------------------------------------------------------------------
    IF EXISTS (
        SELECT 1
        FROM   sys.partition_range_values prv
        INNER JOIN sys.partition_functions pf ON prv.function_id = pf.function_id
        WHERE  pf.name = N'pf_AuditLog_ByDate'
          AND  CAST(prv.value AS INT) = @NewBoundary
    )
    BEGIN
        INSERT INTO dbo.PartitionMaintenanceLog (ProcedureName, ActionTaken, OldBoundary, NewBoundary)
        VALUES (N'usp_MaintainAuditLogPartitions',
                N'Skipped — boundary ' + CAST(@NewBoundary AS NVARCHAR(8)) + N' already exists.',
                NULL, @NewBoundary);
        RETURN;
    END

    -- -------------------------------------------------------------------------
    -- Capture the current oldest boundary (minimum range value)
    -- -------------------------------------------------------------------------
    DECLARE @OldestBoundary INT;

    SELECT @OldestBoundary = MIN(CAST(prv.value AS INT))
    FROM   sys.partition_range_values prv
    INNER JOIN sys.partition_functions pf ON prv.function_id = pf.function_id
    WHERE  pf.name = N'pf_AuditLog_ByDate';

    -- -------------------------------------------------------------------------
    -- Log the start of this maintenance run; capture the row ID for update
    -- -------------------------------------------------------------------------
    DECLARE @LogID INT;

    INSERT INTO dbo.PartitionMaintenanceLog (ProcedureName, ActionTaken, OldBoundary, NewBoundary)
    VALUES (N'usp_MaintainAuditLogPartitions',
            N'Started — will merge ' + CAST(@OldestBoundary AS NVARCHAR(8)) +
            N', split '              + CAST(@NewBoundary    AS NVARCHAR(8)),
            @OldestBoundary, @NewBoundary);

    SET @LogID = SCOPE_IDENTITY();

    -- -------------------------------------------------------------------------
    -- Step 1: Drop the oldest partition boundary
    -- MERGE RANGE collapses partition 1 (<OldestBoundary) and
    -- partition 2 (OldestBoundary ≤ x < NextBoundary) into one.
    -- -------------------------------------------------------------------------
    ALTER PARTITION FUNCTION pf_AuditLog_ByDate() MERGE RANGE (@OldestBoundary);

    -- -------------------------------------------------------------------------
    -- Step 2: Create the new partition 3 days ahead of today
    -- SPLIT RANGE inserts a new boundary point at @NewBoundary.
    -- -------------------------------------------------------------------------
    ALTER PARTITION FUNCTION pf_AuditLog_ByDate() SPLIT RANGE (@NewBoundary);

    -- -------------------------------------------------------------------------
    -- Update the log row with completion status
    -- -------------------------------------------------------------------------
    UPDATE dbo.PartitionMaintenanceLog
    SET    ActionTaken = N'Completed — merged boundary ' + CAST(@OldestBoundary AS NVARCHAR(8)) +
                         N', split new boundary '        + CAST(@NewBoundary    AS NVARCHAR(8))
    WHERE  LogID = @LogID;
END;
GO


-- =============================================================================
-- usp_MaintainTokenTransactionPartitions
--
-- Intended to run monthly (e.g. on the 1st of each month via a separate job).
-- Logic mirrors the daily AuditLog procedure but works in YYYYMM INT space.
--   1. Compute target forward boundary: current month + 2.
--   2. Idempotency guard.
--   3. Merge the oldest boundary.
--   4. Split in the new boundary.
--   5. Log both boundaries.
-- =============================================================================

CREATE OR ALTER PROCEDURE dbo.usp_MaintainTokenTransactionPartitions
AS
BEGIN
    SET NOCOUNT ON;

    -- -------------------------------------------------------------------------
    -- Compute new boundary: first day of (this month + 2) as YYYYMM INT
    -- -------------------------------------------------------------------------
    DECLARE @FutureMonth    DATE = DATEADD(MONTH, 2,
                                      DATEFROMPARTS(YEAR(GETUTCDATE()), MONTH(GETUTCDATE()), 1));
    DECLARE @NewBoundary    INT  = YEAR(@FutureMonth) * 100 + MONTH(@FutureMonth);

    -- -------------------------------------------------------------------------
    -- Idempotency guard
    -- -------------------------------------------------------------------------
    IF EXISTS (
        SELECT 1
        FROM   sys.partition_range_values prv
        INNER JOIN sys.partition_functions pf ON prv.function_id = pf.function_id
        WHERE  pf.name = N'pf_TokenTransaction_ByMonth'
          AND  CAST(prv.value AS INT) = @NewBoundary
    )
    BEGIN
        INSERT INTO dbo.PartitionMaintenanceLog (ProcedureName, ActionTaken, OldBoundary, NewBoundary)
        VALUES (N'usp_MaintainTokenTransactionPartitions',
                N'Skipped — boundary ' + CAST(@NewBoundary AS NVARCHAR(6)) + N' already exists.',
                NULL, @NewBoundary);
        RETURN;
    END

    -- -------------------------------------------------------------------------
    -- Capture oldest boundary
    -- -------------------------------------------------------------------------
    DECLARE @OldestBoundary INT;

    SELECT @OldestBoundary = MIN(CAST(prv.value AS INT))
    FROM   sys.partition_range_values prv
    INNER JOIN sys.partition_functions pf ON prv.function_id = pf.function_id
    WHERE  pf.name = N'pf_TokenTransaction_ByMonth';

    -- -------------------------------------------------------------------------
    -- Log start
    -- -------------------------------------------------------------------------
    DECLARE @LogID INT;

    INSERT INTO dbo.PartitionMaintenanceLog (ProcedureName, ActionTaken, OldBoundary, NewBoundary)
    VALUES (N'usp_MaintainTokenTransactionPartitions',
            N'Started — will merge ' + CAST(@OldestBoundary AS NVARCHAR(6)) +
            N', split '              + CAST(@NewBoundary    AS NVARCHAR(6)),
            @OldestBoundary, @NewBoundary);

    SET @LogID = SCOPE_IDENTITY();

    -- -------------------------------------------------------------------------
    -- Step 1: Drop oldest monthly boundary
    -- -------------------------------------------------------------------------
    ALTER PARTITION FUNCTION pf_TokenTransaction_ByMonth() MERGE RANGE (@OldestBoundary);

    -- -------------------------------------------------------------------------
    -- Step 2: Add new boundary 2 months ahead
    -- -------------------------------------------------------------------------
    ALTER PARTITION FUNCTION pf_TokenTransaction_ByMonth() SPLIT RANGE (@NewBoundary);

    -- -------------------------------------------------------------------------
    -- Update log
    -- -------------------------------------------------------------------------
    UPDATE dbo.PartitionMaintenanceLog
    SET    ActionTaken = N'Completed — merged boundary ' + CAST(@OldestBoundary AS NVARCHAR(6)) +
                         N', split new boundary '        + CAST(@NewBoundary    AS NVARCHAR(6))
    WHERE  LogID = @LogID;
END;
GO


-- =============================================================================
-- SQL Agent Job: nightly AuditLog partition maintenance at 02:00 UTC
--
-- Prerequisites:
--   · SQL Server Agent must be running (available on SQL Managed Instance
--     and on-premises SQL Server; NOT available on Azure SQL Database PaaS).
--   · The login executing this block needs membership in SQLAgentOperatorRole
--     (or higher) in msdb.
--   · Change @db_name below to match your actual database name.
--   · Change @owner_login to an appropriate service login.
--
-- For Azure SQL Database (PaaS), remove this block and instead create an
-- Azure Elastic Job that executes:
--     EXEC dbo.usp_MaintainAuditLogPartitions;
-- on the nightly-2am-utc schedule against the target database.
-- =============================================================================

USE msdb;
GO

DECLARE @job_name      NVARCHAR(128) = N'VenueSpeed - AuditLog Partition Maintenance';
DECLARE @step_name     NVARCHAR(128) = N'Execute usp_MaintainAuditLogPartitions';
DECLARE @schedule_name NVARCHAR(128) = N'VenueSpeed - Nightly 02:00 UTC';
DECLARE @db_name       NVARCHAR(128) = N'VenueSpeed';   -- ← change to your database name
DECLARE @owner_login   NVARCHAR(128) = N'sa';            -- ← change to appropriate login

-- -------------------------------------------------------------------------
-- Create the job (idempotent)
-- -------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM msdb.dbo.sysjobs WHERE name = @job_name)
BEGIN
    EXEC msdb.dbo.sp_add_job
        @job_name             = @job_name,
        @description          = N'Rolls the AuditLog daily partition window: merges the oldest boundary, splits a new boundary 3 days ahead. Logs each run to dbo.PartitionMaintenanceLog.',
        @category_name        = N'[Uncategorized (Local)]',
        @owner_login_name     = @owner_login,
        @enabled              = 1,
        @notify_level_eventlog = 2;   -- log failures to Windows Event Log
    PRINT 'Created SQL Agent job: ' + @job_name;
END
ELSE
    PRINT 'Job already exists — skipped create: ' + @job_name;

-- -------------------------------------------------------------------------
-- Add job step (idempotent)
-- -------------------------------------------------------------------------
IF NOT EXISTS (
    SELECT 1
    FROM   msdb.dbo.sysjobsteps js
    INNER JOIN msdb.dbo.sysjobs  j  ON js.job_id = j.job_id
    WHERE  j.name      = @job_name
      AND  js.step_name = @step_name
)
BEGIN
    EXEC msdb.dbo.sp_add_jobstep
        @job_name          = @job_name,
        @step_name         = @step_name,
        @subsystem         = N'TSQL',
        @command           = N'EXEC dbo.usp_MaintainAuditLogPartitions;',
        @database_name     = @db_name,
        @retry_attempts    = 1,
        @retry_interval    = 5,       -- minutes between retries
        @on_success_action = 1,       -- 1 = quit with success
        @on_fail_action    = 2;       -- 2 = quit with failure
    PRINT 'Added job step: ' + @step_name;
END
ELSE
    PRINT 'Job step already exists — skipped: ' + @step_name;

-- -------------------------------------------------------------------------
-- Create schedule (idempotent) and attach to job
-- -------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM msdb.dbo.sysschedules WHERE name = @schedule_name)
BEGIN
    -- freq_type 4   = daily
    -- freq_interval = every 1 day
    -- active_start_time 20000 = 02:00:00 (stored as HHMMSS integer)
    EXEC msdb.dbo.sp_add_schedule
        @schedule_name      = @schedule_name,
        @freq_type          = 4,
        @freq_interval      = 1,
        @active_start_time  = 20000;

    EXEC msdb.dbo.sp_attach_schedule
        @job_name      = @job_name,
        @schedule_name = @schedule_name;

    PRINT 'Created and attached schedule: ' + @schedule_name;
END
ELSE
    PRINT 'Schedule already exists — skipped: ' + @schedule_name;

-- -------------------------------------------------------------------------
-- Register job on the local server (idempotent)
-- -------------------------------------------------------------------------
IF NOT EXISTS (
    SELECT 1
    FROM   msdb.dbo.sysjobservers js
    INNER JOIN msdb.dbo.sysjobs    j ON js.job_id = j.job_id
    WHERE  j.name = @job_name
)
BEGIN
    EXEC msdb.dbo.sp_add_jobserver
        @job_name    = @job_name,
        @server_name = N'(LOCAL)';
    PRINT 'Registered job on local server.';
END
ELSE
    PRINT 'Job server registration already exists — skipped.';
GO

USE VenueSpeed;  -- restore context to application database
GO
