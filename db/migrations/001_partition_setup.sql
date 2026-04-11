-- =============================================================================
-- VenueSpeed Database Schema V1.0
-- Migration 001: Partition Functions and Schemes
-- Azure SQL · Must run BEFORE creating AuditLog and TokenTransaction tables
--
-- Boundaries are generated dynamically from GETUTCDATE() at run-time, so
-- this script stays correct when re-run on any date.
--
-- Idempotent: skips creation if the function / scheme already exists.
-- To rebuild from scratch, drop the scheme then the function manually.
-- =============================================================================

-- -----------------------------------------------------------------------------
-- AuditLog partition: daily by LogDateKey (YYYYMMDD INT)
--
-- Rolling window: 90 days back → 3 days forward (94 boundary points →
-- 95 partitions; partitions 2-94 each own one calendar day; partitions 1
-- and 95 are the before/after overflow sinks kept empty by maintenance).
-- usp_MaintainAuditLogPartitions (migration 006) merges the oldest boundary
-- and splits a new one each night to keep the window rolling.
-- -----------------------------------------------------------------------------

IF NOT EXISTS (SELECT 1 FROM sys.partition_functions WHERE name = N'pf_AuditLog_ByDate')
BEGIN
    -- Build the VALUES list: one YYYYMMDD integer per day in [today-90, today+3]
    DECLARE @al_values  NVARCHAR(MAX) = N'';
    DECLARE @al_day     DATE          = DATEADD(DAY, -90, CAST(GETUTCDATE() AS DATE));
    DECLARE @al_end     DATE          = DATEADD(DAY,   3, CAST(GETUTCDATE() AS DATE));
    DECLARE @al_first   BIT           = 1;

    WHILE @al_day <= @al_end
    BEGIN
        IF @al_first = 0 SET @al_values += N', ';
        SET @al_values += CAST(
            YEAR(@al_day)  * 10000 +
            MONTH(@al_day) * 100   +
            DAY(@al_day)
        AS NVARCHAR(8));
        SET @al_first = 0;
        SET @al_day   = DATEADD(DAY, 1, @al_day);
    END

    DECLARE @al_sql NVARCHAR(MAX) =
        N'CREATE PARTITION FUNCTION pf_AuditLog_ByDate (INT) AS RANGE RIGHT FOR VALUES (' + @al_values + N');';
    EXEC sp_executesql @al_sql;

    PRINT 'Created pf_AuditLog_ByDate with boundaries ' +
          CAST(DATEADD(DAY, -90, CAST(GETUTCDATE() AS DATE)) AS NVARCHAR(10)) +
          ' → ' +
          CAST(DATEADD(DAY,   3, CAST(GETUTCDATE() AS DATE)) AS NVARCHAR(10));
END
ELSE
    PRINT 'pf_AuditLog_ByDate already exists — skipped.';
GO

-- All partitions land on [PRIMARY] for V1; point to a dedicated filegroup
-- in production (e.g. ALTER PARTITION SCHEME ... NEXT USED [fg_archive]).
IF NOT EXISTS (SELECT 1 FROM sys.partition_schemes WHERE name = N'ps_AuditLog_ByDate')
BEGIN
    CREATE PARTITION SCHEME ps_AuditLog_ByDate
        AS PARTITION pf_AuditLog_ByDate
        ALL TO ([PRIMARY]);
    PRINT 'Created ps_AuditLog_ByDate.';
END
ELSE
    PRINT 'ps_AuditLog_ByDate already exists — skipped.';
GO

-- -----------------------------------------------------------------------------
-- TokenTransaction partition: monthly by TransactionMonthKey (YYYYMM INT)
--
-- Rolling window: 12 months back → 2 months forward (14 boundary points →
-- 15 partitions; partitions 2-14 each own one calendar month).
-- usp_MaintainTokenTransactionPartitions (migration 006) rolls the window
-- monthly.
-- -----------------------------------------------------------------------------

IF NOT EXISTS (SELECT 1 FROM sys.partition_functions WHERE name = N'pf_TokenTransaction_ByMonth')
BEGIN
    DECLARE @tt_values  NVARCHAR(MAX) = N'';
    -- Anchor to the first of this month, then step back 12 and forward 2
    DECLARE @tt_month   DATE          = DATEADD(MONTH, -12,
                             DATEFROMPARTS(YEAR(GETUTCDATE()), MONTH(GETUTCDATE()), 1));
    DECLARE @tt_end     DATE          = DATEADD(MONTH,   2,
                             DATEFROMPARTS(YEAR(GETUTCDATE()), MONTH(GETUTCDATE()), 1));
    DECLARE @tt_first   BIT           = 1;

    WHILE @tt_month <= @tt_end
    BEGIN
        IF @tt_first = 0 SET @tt_values += N', ';
        SET @tt_values += CAST(YEAR(@tt_month) * 100 + MONTH(@tt_month) AS NVARCHAR(6));
        SET @tt_first  = 0;
        SET @tt_month  = DATEADD(MONTH, 1, @tt_month);
    END

    DECLARE @tt_sql NVARCHAR(MAX) =
        N'CREATE PARTITION FUNCTION pf_TokenTransaction_ByMonth (INT) AS RANGE RIGHT FOR VALUES (' + @tt_values + N');';
    EXEC sp_executesql @tt_sql;

    PRINT 'Created pf_TokenTransaction_ByMonth.';
END
ELSE
    PRINT 'pf_TokenTransaction_ByMonth already exists — skipped.';
GO

IF NOT EXISTS (SELECT 1 FROM sys.partition_schemes WHERE name = N'ps_TokenTransaction_ByMonth')
BEGIN
    CREATE PARTITION SCHEME ps_TokenTransaction_ByMonth
        AS PARTITION pf_TokenTransaction_ByMonth
        ALL TO ([PRIMARY]);
    PRINT 'Created ps_TokenTransaction_ByMonth.';
END
ELSE
    PRINT 'ps_TokenTransaction_ByMonth already exists — skipped.';
GO
