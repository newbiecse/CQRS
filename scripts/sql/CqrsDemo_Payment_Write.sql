-- Auto-generated from EF Core models. Database: CqrsDemo_Payment_Write
IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = N'CqrsDemo_Payment_Write')
    CREATE DATABASE [CqrsDemo_Payment_Write];
GO

USE [CqrsDemo_Payment_Write];
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


CREATE TABLE [StoredEvents] (
    [Id] uniqueidentifier NOT NULL,
    [StreamId] uniqueidentifier NOT NULL,
    [StreamType] nvarchar(100) NOT NULL,
    [Version] bigint NOT NULL,
    [EventType] nvarchar(200) NOT NULL,
    [Payload] nvarchar(max) NOT NULL,
    [OccurredOn] datetime2 NOT NULL,
    CONSTRAINT [PK_StoredEvents] PRIMARY KEY ([Id])
);
GO


CREATE INDEX [IX_OutboxMessages_ProcessedAt_OccurredOn] ON [OutboxMessages] ([ProcessedAt], [OccurredOn]);
GO


CREATE UNIQUE INDEX [IX_StoredEvents_StreamId_StreamType_Version] ON [StoredEvents] ([StreamId], [StreamType], [Version]);
GO



