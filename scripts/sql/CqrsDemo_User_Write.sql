-- Auto-generated from EF Core models. Database: CqrsDemo_User_Write
IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = N'CqrsDemo_User_Write')
    CREATE DATABASE [CqrsDemo_User_Write];
GO

USE [CqrsDemo_User_Write];
GO

CREATE TABLE [OutboxMessages] (
    [Id] uniqueidentifier NOT NULL,
    [EventType] nvarchar(200) NOT NULL,
    [Payload] nvarchar(max) NOT NULL,
    [OccurredOn] datetime2 NOT NULL,
    [ProcessedAt] datetime2 NULL,
    [AttemptCount] int NOT NULL,
    [LastError] nvarchar(2000) NULL,
    CONSTRAINT [PK_OutboxMessages] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [Users] (
    [Id] uniqueidentifier NOT NULL,
    [Email] nvarchar(320) NOT NULL,
    [DisplayName] nvarchar(200) NOT NULL,
    [IsActive] bit NOT NULL,
    [RegisteredAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
);
GO


CREATE INDEX [IX_OutboxMessages_ProcessedAt_OccurredOn] ON [OutboxMessages] ([ProcessedAt], [OccurredOn]);
GO



