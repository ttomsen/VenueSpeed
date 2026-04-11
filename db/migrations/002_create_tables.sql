-- =============================================================================
-- VenueSpeed Database Schema V1.0
-- Migration 002: Create Tables (dependency order per Implementation Notes)
-- Azure SQL · Run AFTER 001_partition_setup.sql
-- =============================================================================

-- =============================================================================
-- Group 1: No dependencies — Venue, Participant
-- =============================================================================

CREATE TABLE dbo.Venue (
    Id                        BIGINT IDENTITY(1,1)   NOT NULL,
    ExternalId                UNIQUEIDENTIFIER        NOT NULL CONSTRAINT DF_Venue_ExternalId DEFAULT NEWID(),
    VenueName                 NVARCHAR(200)           NOT NULL,
    Slug                      NVARCHAR(100)           NOT NULL,
    Email                     NVARCHAR(255)           NOT NULL,
    PhoneNumber               NVARCHAR(30)            NULL,
    AddressLine1              NVARCHAR(200)           NOT NULL,
    AddressLine2              NVARCHAR(200)           NULL,
    City                      NVARCHAR(100)           NOT NULL,
    StateCode                 CHAR(2)                 NOT NULL,
    PostalCode                NVARCHAR(20)            NOT NULL,
    Latitude                  DECIMAL(9,6)            NULL,
    Longitude                 DECIMAL(9,6)            NULL,
    VenueType                 NVARCHAR(50)            NOT NULL,
    Capacity                  INT                     NULL,
    StripeAccountId           NVARCHAR(100)           NULL,
    StripeOnboardingComplete  BIT                     NOT NULL CONSTRAINT DF_Venue_StripeOnboardingComplete DEFAULT 0,
    TokenPriceUsd             DECIMAL(6,2)            NOT NULL CONSTRAINT DF_Venue_TokenPriceUsd DEFAULT 10.00,
    IsActive                  BIT                     NOT NULL CONSTRAINT DF_Venue_IsActive DEFAULT 1,
    IsDeleted                 BIT                     NOT NULL CONSTRAINT DF_Venue_IsDeleted DEFAULT 0,
    DeletedAtUtc              DATETIME2               NULL,
    CreatedAtUtc              DATETIME2               NOT NULL CONSTRAINT DF_Venue_CreatedAtUtc DEFAULT GETUTCDATE(),
    UpdatedAtUtc              DATETIME2               NOT NULL CONSTRAINT DF_Venue_UpdatedAtUtc DEFAULT GETUTCDATE(),
    CONSTRAINT PK_Venue PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UK_Venue_ExternalId UNIQUE (ExternalId),
    CONSTRAINT UK_Venue_Slug UNIQUE (Slug),
    CONSTRAINT UK_Venue_Email UNIQUE (Email)
);
GO

CREATE TABLE dbo.Participant (
    Id                BIGINT IDENTITY(1,1)   NOT NULL,
    ExternalId        UNIQUEIDENTIFIER        NOT NULL CONSTRAINT DF_Participant_ExternalId DEFAULT NEWID(),
    AuthProviderId    NVARCHAR(255)           NOT NULL,
    AuthProvider      NVARCHAR(20)            NOT NULL,
    FirstName         NVARCHAR(100)           NOT NULL,
    DateOfBirth       DATE                    NOT NULL,
    Gender            CHAR(1)                 NOT NULL,
    City              NVARCHAR(100)           NULL,
    StateCode         CHAR(2)                 NULL,
    HeadlineText      NVARCHAR(140)           NULL,
    ProfilePhotoUrl   NVARCHAR(500)           NULL,
    Interests         NVARCHAR(500)           NULL,
    IsActive          BIT                     NOT NULL CONSTRAINT DF_Participant_IsActive DEFAULT 1,
    IsDeleted         BIT                     NOT NULL CONSTRAINT DF_Participant_IsDeleted DEFAULT 0,
    DeletedAtUtc      DATETIME2               NULL,
    CreatedAtUtc      DATETIME2               NOT NULL CONSTRAINT DF_Participant_CreatedAtUtc DEFAULT GETUTCDATE(),
    UpdatedAtUtc      DATETIME2               NOT NULL CONSTRAINT DF_Participant_UpdatedAtUtc DEFAULT GETUTCDATE(),
    CONSTRAINT PK_Participant PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UK_Participant_ExternalId UNIQUE (ExternalId),
    CONSTRAINT UK_Participant_AuthProviderId UNIQUE (AuthProviderId)
);
GO

-- =============================================================================
-- Group 2: Depends on Venue — VenueSettings, Event
-- =============================================================================

CREATE TABLE dbo.VenueSettings (
    Id                        BIGINT IDENTITY(1,1)   NOT NULL,
    VenueId                   BIGINT                  NOT NULL,
    LogoUrl                   NVARCHAR(500)           NULL,
    CoverPhotoUrl             NVARCHAR(500)           NULL,
    Description               NVARCHAR(1000)          NULL,
    GoogleMapsUrl             NVARCHAR(500)           NULL,
    WhiteLabelEnabled         BIT                     NOT NULL CONSTRAINT DF_VenueSettings_WhiteLabelEnabled DEFAULT 0,
    WhiteLabelPrimaryColor    CHAR(7)                 NULL,
    WhiteLabelLogoUrl         NVARCHAR(500)           NULL,
    NotifyOnRegistration      BIT                     NOT NULL CONSTRAINT DF_VenueSettings_NotifyOnRegistration DEFAULT 1,
    NotifyOnPayout            BIT                     NOT NULL CONSTRAINT DF_VenueSettings_NotifyOnPayout DEFAULT 1,
    CreatedAtUtc              DATETIME2               NOT NULL CONSTRAINT DF_VenueSettings_CreatedAtUtc DEFAULT GETUTCDATE(),
    UpdatedAtUtc              DATETIME2               NOT NULL CONSTRAINT DF_VenueSettings_UpdatedAtUtc DEFAULT GETUTCDATE(),
    CONSTRAINT PK_VenueSettings PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_VenueSettings_Venue FOREIGN KEY (VenueId) REFERENCES dbo.Venue (Id)
);
GO

CREATE TABLE dbo.Event (
    Id              BIGINT IDENTITY(1,1)   NOT NULL,
    ExternalId      UNIQUEIDENTIFIER        NOT NULL CONSTRAINT DF_Event_ExternalId DEFAULT NEWID(),
    VenueId         BIGINT                  NOT NULL,
    EventName       NVARCHAR(200)           NOT NULL,
    EventDateUtc    DATETIME2               NOT NULL,
    DoorsOpenUtc    DATETIME2               NOT NULL,
    Status          NVARCHAR(30)            NOT NULL CONSTRAINT DF_Event_Status DEFAULT 'Draft',
    IsDeleted       BIT                     NOT NULL CONSTRAINT DF_Event_IsDeleted DEFAULT 0,
    DeletedAtUtc    DATETIME2               NULL,
    CreatedAtUtc    DATETIME2               NOT NULL CONSTRAINT DF_Event_CreatedAtUtc DEFAULT GETUTCDATE(),
    UpdatedAtUtc    DATETIME2               NOT NULL CONSTRAINT DF_Event_UpdatedAtUtc DEFAULT GETUTCDATE(),
    CONSTRAINT PK_Event PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UK_Event_ExternalId UNIQUE (ExternalId),
    CONSTRAINT FK_Event_Venue FOREIGN KEY (VenueId) REFERENCES dbo.Venue (Id)
);
GO

-- =============================================================================
-- Group 3: Depends on Venue, Event — EventBracket
-- =============================================================================

CREATE TABLE dbo.EventBracket (
    Id                        BIGINT IDENTITY(1,1)   NOT NULL,
    ExternalId                UNIQUEIDENTIFIER        NOT NULL CONSTRAINT DF_EventBracket_ExternalId DEFAULT NEWID(),
    VenueId                   BIGINT                  NOT NULL,
    EventId                   BIGINT                  NOT NULL,
    BracketName               NVARCHAR(100)           NOT NULL,
    AgeRangeMin               INT                     NULL,
    AgeRangeMax               INT                     NULL,
    GenderComposition         NVARCHAR(10)            NOT NULL,
    StartTimeUtc              DATETIME2               NOT NULL,
    EndTimeUtc                DATETIME2               NOT NULL,
    RoundDurationSeconds      INT                     NOT NULL CONSTRAINT DF_EventBracket_RoundDurationSeconds DEFAULT 180,
    MaxParticipantsPerSide    INT                     NOT NULL CONSTRAINT DF_EventBracket_MaxParticipantsPerSide DEFAULT 12,
    TicketPriceUsd            DECIMAL(6,2)            NOT NULL,
    Status                    NVARCHAR(30)            NOT NULL CONSTRAINT DF_EventBracket_Status DEFAULT 'Scheduled',
    CheckInOpenUtc            DATETIME2               NULL,
    ActualStartUtc            DATETIME2               NULL,
    ActualEndUtc              DATETIME2               NULL,
    CurrentRoundNumber        INT                     NULL CONSTRAINT DF_EventBracket_CurrentRoundNumber DEFAULT 0,
    IsDeleted                 BIT                     NOT NULL CONSTRAINT DF_EventBracket_IsDeleted DEFAULT 0,
    DeletedAtUtc              DATETIME2               NULL,
    CreatedAtUtc              DATETIME2               NOT NULL CONSTRAINT DF_EventBracket_CreatedAtUtc DEFAULT GETUTCDATE(),
    UpdatedAtUtc              DATETIME2               NOT NULL CONSTRAINT DF_EventBracket_UpdatedAtUtc DEFAULT GETUTCDATE(),
    CONSTRAINT PK_EventBracket PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UK_EventBracket_ExternalId UNIQUE (ExternalId),
    CONSTRAINT FK_EventBracket_Venue FOREIGN KEY (VenueId) REFERENCES dbo.Venue (Id),
    CONSTRAINT FK_EventBracket_Event FOREIGN KEY (EventId) REFERENCES dbo.Event (Id)
);
GO

-- =============================================================================
-- Group 4: Depends on Venue, EventBracket — EventTable
-- =============================================================================

CREATE TABLE dbo.EventTable (
    Id                BIGINT IDENTITY(1,1)   NOT NULL,
    VenueId           BIGINT                  NOT NULL,
    EventBracketId    BIGINT                  NOT NULL,
    TableNumber       INT                     NOT NULL,
    TableLabel        NVARCHAR(50)            NULL,
    CreatedAtUtc      DATETIME2               NOT NULL CONSTRAINT DF_EventTable_CreatedAtUtc DEFAULT GETUTCDATE(),
    CONSTRAINT PK_EventTable PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_EventTable_Venue FOREIGN KEY (VenueId) REFERENCES dbo.Venue (Id),
    CONSTRAINT FK_EventTable_EventBracket FOREIGN KEY (EventBracketId) REFERENCES dbo.EventBracket (Id)
);
GO

-- =============================================================================
-- Group 5: Depends on Venue, EventBracket, Participant — EventRegistration
-- =============================================================================

CREATE TABLE dbo.EventRegistration (
    Id                      BIGINT IDENTITY(1,1)   NOT NULL,
    ExternalId              UNIQUEIDENTIFIER        NOT NULL CONSTRAINT DF_EventRegistration_ExternalId DEFAULT NEWID(),
    VenueId                 BIGINT                  NOT NULL,
    EventBracketId          BIGINT                  NOT NULL,
    ParticipantId           BIGINT                  NOT NULL,
    TicketPriceUsd          DECIMAL(6,2)            NOT NULL,
    StripePaymentIntentId   NVARCHAR(100)           NOT NULL,
    PaymentStatus           NVARCHAR(30)            NOT NULL CONSTRAINT DF_EventRegistration_PaymentStatus DEFAULT 'Pending',
    CheckInStatus           NVARCHAR(30)            NOT NULL CONSTRAINT DF_EventRegistration_CheckInStatus DEFAULT 'NotCheckedIn',
    CheckInMethod           NVARCHAR(20)            NULL,
    CheckedInAtUtc          DATETIME2               NULL,
    RemovalReason           NVARCHAR(30)            NULL,
    RemovedAtUtc            DATETIME2               NULL,
    RemovedByVenueUserId    BIGINT                  NULL,
    RefundAmountUsd         DECIMAL(6,2)            NULL,
    WaitlistPosition        INT                     NULL,
    CreatedAtUtc            DATETIME2               NOT NULL CONSTRAINT DF_EventRegistration_CreatedAtUtc DEFAULT GETUTCDATE(),
    UpdatedAtUtc            DATETIME2               NOT NULL CONSTRAINT DF_EventRegistration_UpdatedAtUtc DEFAULT GETUTCDATE(),
    CONSTRAINT PK_EventRegistration PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UK_EventRegistration_ExternalId UNIQUE (ExternalId),
    CONSTRAINT FK_EventRegistration_Venue FOREIGN KEY (VenueId) REFERENCES dbo.Venue (Id),
    CONSTRAINT FK_EventRegistration_EventBracket FOREIGN KEY (EventBracketId) REFERENCES dbo.EventBracket (Id),
    CONSTRAINT FK_EventRegistration_Participant FOREIGN KEY (ParticipantId) REFERENCES dbo.Participant (Id)
);
GO

-- =============================================================================
-- Group 6: Depends on Venue, EventBracket, EventRegistration, EventTable,
--          Participant — BracketRound
-- =============================================================================

CREATE TABLE dbo.BracketRound (
    Id                BIGINT IDENTITY(1,1)   NOT NULL,
    VenueId           BIGINT                  NOT NULL,
    EventBracketId    BIGINT                  NOT NULL,
    RegistrationId    BIGINT                  NOT NULL,
    ParticipantId     BIGINT                  NOT NULL,
    RoundNumber       INT                     NOT NULL,
    TableId           BIGINT                  NOT NULL,
    SeatLabel         CHAR(1)                 NOT NULL,
    IsByeRound        BIT                     NOT NULL CONSTRAINT DF_BracketRound_IsByeRound DEFAULT 0,
    PartnerId         BIGINT                  NULL,
    CreatedAtUtc      DATETIME2               NOT NULL CONSTRAINT DF_BracketRound_CreatedAtUtc DEFAULT GETUTCDATE(),
    CONSTRAINT PK_BracketRound PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_BracketRound_Venue FOREIGN KEY (VenueId) REFERENCES dbo.Venue (Id),
    CONSTRAINT FK_BracketRound_EventBracket FOREIGN KEY (EventBracketId) REFERENCES dbo.EventBracket (Id),
    CONSTRAINT FK_BracketRound_Registration FOREIGN KEY (RegistrationId) REFERENCES dbo.EventRegistration (Id),
    CONSTRAINT FK_BracketRound_EventTable FOREIGN KEY (TableId) REFERENCES dbo.EventTable (Id),
    CONSTRAINT FK_BracketRound_Participant FOREIGN KEY (ParticipantId) REFERENCES dbo.Participant (Id),
    CONSTRAINT FK_BracketRound_Partner FOREIGN KEY (PartnerId) REFERENCES dbo.Participant (Id)
);
GO

-- =============================================================================
-- Group 7: Depends on Venue, EventBracket, BracketRound, Participant
--          — RoundInterest
-- =============================================================================

CREATE TABLE dbo.RoundInterest (
    Id                  BIGINT IDENTITY(1,1)   NOT NULL,
    VenueId             BIGINT                  NOT NULL,
    EventBracketId      BIGINT                  NOT NULL,
    BracketRoundId      BIGINT                  NOT NULL,
    FromParticipantId   BIGINT                  NOT NULL,
    ToParticipantId     BIGINT                  NOT NULL,
    InterestLevel       NVARCHAR(10)            NOT NULL,
    IsTopChoice         BIT                     NOT NULL CONSTRAINT DF_RoundInterest_IsTopChoice DEFAULT 0,
    TappedAtUtc         DATETIME2               NOT NULL CONSTRAINT DF_RoundInterest_TappedAtUtc DEFAULT GETUTCDATE(),
    CONSTRAINT PK_RoundInterest PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_RoundInterest_Venue FOREIGN KEY (VenueId) REFERENCES dbo.Venue (Id),
    CONSTRAINT FK_RoundInterest_EventBracket FOREIGN KEY (EventBracketId) REFERENCES dbo.EventBracket (Id),
    CONSTRAINT FK_RoundInterest_BracketRound FOREIGN KEY (BracketRoundId) REFERENCES dbo.BracketRound (Id),
    CONSTRAINT FK_RoundInterest_FromParticipant FOREIGN KEY (FromParticipantId) REFERENCES dbo.Participant (Id),
    CONSTRAINT FK_RoundInterest_ToParticipant FOREIGN KEY (ToParticipantId) REFERENCES dbo.Participant (Id)
);
GO

-- =============================================================================
-- Group 8: Depends on Venue, Participant, EventBracket — DrinkToken
-- =============================================================================

CREATE TABLE dbo.DrinkToken (
    Id                      BIGINT IDENTITY(1,1)   NOT NULL,
    ExternalId              UNIQUEIDENTIFIER        NOT NULL CONSTRAINT DF_DrinkToken_ExternalId DEFAULT NEWID(),
    VenueId                 BIGINT                  NOT NULL,
    ParticipantId           BIGINT                  NOT NULL,
    EventBracketId          BIGINT                  NULL,
    TokenValueUsd           DECIMAL(6,2)            NOT NULL,
    PlatformFeeUsd          DECIMAL(6,2)            NOT NULL,
    VenueAmountUsd          DECIMAL(6,2)            NOT NULL,
    StripePaymentIntentId   NVARCHAR(100)           NOT NULL,
    Status                  NVARCHAR(20)            NOT NULL CONSTRAINT DF_DrinkToken_Status DEFAULT 'Available',
    SentToParticipantId     BIGINT                  NULL,
    SentAtUtc               DATETIME2               NULL,
    RedemptionCode          CHAR(8)                 NOT NULL,
    RedeemedAtUtc           DATETIME2               NULL,
    RedeemedByVenueUserId   BIGINT                  NULL,
    ExpiresAtUtc            DATETIME2               NOT NULL,
    CreatedAtUtc            DATETIME2               NOT NULL CONSTRAINT DF_DrinkToken_CreatedAtUtc DEFAULT GETUTCDATE(),
    UpdatedAtUtc            DATETIME2               NOT NULL CONSTRAINT DF_DrinkToken_UpdatedAtUtc DEFAULT GETUTCDATE(),
    CONSTRAINT PK_DrinkToken PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UK_DrinkToken_ExternalId UNIQUE (ExternalId),
    CONSTRAINT UK_DrinkToken_RedemptionCode UNIQUE (RedemptionCode),
    CONSTRAINT FK_DrinkToken_Venue FOREIGN KEY (VenueId) REFERENCES dbo.Venue (Id),
    CONSTRAINT FK_DrinkToken_Participant FOREIGN KEY (ParticipantId) REFERENCES dbo.Participant (Id),
    CONSTRAINT FK_DrinkToken_EventBracket FOREIGN KEY (EventBracketId) REFERENCES dbo.EventBracket (Id),
    CONSTRAINT FK_DrinkToken_SentTo FOREIGN KEY (SentToParticipantId) REFERENCES dbo.Participant (Id)
);
GO

-- =============================================================================
-- Group 9: Depends on Venue, DrinkToken — TokenTransaction (partitioned)
-- =============================================================================

CREATE TABLE dbo.TokenTransaction (
    Id                    BIGINT IDENTITY(1,1)   NOT NULL,
    VenueId               BIGINT                  NOT NULL,
    DrinkTokenId          BIGINT                  NOT NULL,
    TransactionType       NVARCHAR(20)            NOT NULL,
    GrossAmountUsd        DECIMAL(8,2)            NOT NULL,
    PlatformFeeUsd        DECIMAL(8,2)            NOT NULL,
    VenueAmountUsd        DECIMAL(8,2)            NOT NULL,
    StripeReference       NVARCHAR(100)           NULL,
    TransactionMonthKey   INT                     NOT NULL,
    CreatedAtUtc          DATETIME2               NOT NULL CONSTRAINT DF_TokenTransaction_CreatedAtUtc DEFAULT GETUTCDATE(),
    CONSTRAINT PK_TokenTransaction PRIMARY KEY CLUSTERED (Id, TransactionMonthKey),
    CONSTRAINT FK_TokenTransaction_Venue FOREIGN KEY (VenueId) REFERENCES dbo.Venue (Id),
    CONSTRAINT FK_TokenTransaction_DrinkToken FOREIGN KEY (DrinkTokenId) REFERENCES dbo.DrinkToken (Id)
) ON ps_TokenTransaction_ByMonth (TransactionMonthKey);
GO

-- =============================================================================
-- Group 10: Depends on Venue, EventBracket — Payout
-- =============================================================================

CREATE TABLE dbo.Payout (
    Id                        BIGINT IDENTITY(1,1)   NOT NULL,
    ExternalId                UNIQUEIDENTIFIER        NOT NULL CONSTRAINT DF_Payout_ExternalId DEFAULT NEWID(),
    VenueId                   BIGINT                  NOT NULL,
    EventBracketId            BIGINT                  NOT NULL,
    GrossTicketRevenueUsd     DECIMAL(8,2)            NOT NULL,
    VenueShareUsd             DECIMAL(8,2)            NOT NULL,
    PlatformShareUsd          DECIMAL(8,2)            NOT NULL,
    StripeTransferId          NVARCHAR(100)           NULL,
    Status                    NVARCHAR(20)            NOT NULL CONSTRAINT DF_Payout_Status DEFAULT 'Pending',
    FailureReason             NVARCHAR(500)           NULL,
    TransferredAtUtc          DATETIME2               NULL,
    CreatedAtUtc              DATETIME2               NOT NULL CONSTRAINT DF_Payout_CreatedAtUtc DEFAULT GETUTCDATE(),
    UpdatedAtUtc              DATETIME2               NOT NULL CONSTRAINT DF_Payout_UpdatedAtUtc DEFAULT GETUTCDATE(),
    CONSTRAINT PK_Payout PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UK_Payout_ExternalId UNIQUE (ExternalId),
    CONSTRAINT FK_Payout_Venue FOREIGN KEY (VenueId) REFERENCES dbo.Venue (Id),
    CONSTRAINT FK_Payout_EventBracket FOREIGN KEY (EventBracketId) REFERENCES dbo.EventBracket (Id)
);
GO

-- =============================================================================
-- Group 11: Depends on Venue (nullable), Participant (nullable) — AuditLog
--           (partitioned)
-- =============================================================================

CREATE TABLE dbo.AuditLog (
    Id              BIGINT IDENTITY(1,1)   NOT NULL,
    VenueId         BIGINT                  NULL,
    ParticipantId   BIGINT                  NULL,
    ActorType       NVARCHAR(20)            NOT NULL,
    ActorId         BIGINT                  NOT NULL,
    Action          NVARCHAR(100)           NOT NULL,
    EntityType      NVARCHAR(50)            NULL,
    EntityId        BIGINT                  NULL,
    OldValueJson    NVARCHAR(MAX)           NULL,
    NewValueJson    NVARCHAR(MAX)           NULL,
    IpAddress       NVARCHAR(45)            NULL,
    UserAgent       NVARCHAR(500)           NULL,
    LogDateKey      INT                     NOT NULL,
    CreatedAtUtc    DATETIME2               NOT NULL CONSTRAINT DF_AuditLog_CreatedAtUtc DEFAULT GETUTCDATE(),
    CONSTRAINT PK_AuditLog PRIMARY KEY CLUSTERED (Id, LogDateKey),
    CONSTRAINT FK_AuditLog_Venue FOREIGN KEY (VenueId) REFERENCES dbo.Venue (Id),
    CONSTRAINT FK_AuditLog_Participant FOREIGN KEY (ParticipantId) REFERENCES dbo.Participant (Id)
) ON ps_AuditLog_ByDate (LogDateKey);
GO
