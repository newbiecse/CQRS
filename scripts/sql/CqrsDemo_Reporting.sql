-- Auto-generated from EF Core models. Database: CqrsDemo_Reporting
IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = N'CqrsDemo_Reporting')
    CREATE DATABASE [CqrsDemo_Reporting];
GO

USE [CqrsDemo_Reporting];
GO

CREATE TABLE [OrderFacts] (
    [OrderId] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [UserEmail] nvarchar(320) NULL,
    [UserDisplayName] nvarchar(200) NULL,
    [TotalAmount] decimal(18,2) NOT NULL,
    [Status] nvarchar(50) NOT NULL,
    [OrderCreatedAt] datetime2 NOT NULL,
    [LastUpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_OrderFacts] PRIMARY KEY ([OrderId])
);
GO


CREATE TABLE [UserProfiles] (
    [UserId] uniqueidentifier NOT NULL,
    [Email] nvarchar(320) NOT NULL,
    [DisplayName] nvarchar(200) NOT NULL,
    [IsActive] bit NOT NULL,
    [LastUpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_UserProfiles] PRIMARY KEY ([UserId])
);
GO


CREATE INDEX [IX_OrderFacts_OrderCreatedAt] ON [OrderFacts] ([OrderCreatedAt]);
GO


CREATE INDEX [IX_OrderFacts_UserId_OrderCreatedAt] ON [OrderFacts] ([UserId], [OrderCreatedAt]);
GO


CREATE UNIQUE INDEX [IX_UserProfiles_Email] ON [UserProfiles] ([Email]);
GO



