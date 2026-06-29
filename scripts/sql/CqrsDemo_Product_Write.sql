-- Auto-generated from EF Core models. Database: CqrsDemo_Product_Write
IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = N'CqrsDemo_Product_Write')
    CREATE DATABASE [CqrsDemo_Product_Write];
GO

USE [CqrsDemo_Product_Write];
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


CREATE TABLE [Products] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(200) NOT NULL,
    [Price] decimal(18,2) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Products] PRIMARY KEY ([Id])
);
GO


CREATE INDEX [IX_OutboxMessages_ProcessedAt_OccurredOn] ON [OutboxMessages] ([ProcessedAt], [OccurredOn]);
GO



