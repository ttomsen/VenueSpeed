-- =============================================================================
-- VenueSpeed Database Schema V1.0
-- Migration 007: Development Seed Data
-- Run AFTER 005_views.sql
-- Inserts a test Venue with Id=1 so local API calls have something to return.
-- Safe to run in production — the IF NOT EXISTS guard makes it a no-op.
-- =============================================================================

SET IDENTITY_INSERT dbo.Venue ON;

IF NOT EXISTS (SELECT 1 FROM dbo.Venue WHERE Id = 1)
BEGIN
    INSERT INTO dbo.Venue (
        Id, ExternalId, VenueName, Slug, Email,
        AddressLine1, City, StateCode, PostalCode,
        VenueType, TokenPriceUsd,
        StripeOnboardingComplete, IsActive, IsDeleted
    )
    VALUES (
        1,
        'a1b2c3d4-e5f6-7890-abcd-ef1234567890',
        'Dev Venue',
        'dev-venue',
        'dev@venuespeed.com',
        '123 Main St', 'Austin', 'TX', '78701',
        'Arena', 10.00,
        0, 1, 0
    );
END

SET IDENTITY_INSERT dbo.Venue OFF;
GO
