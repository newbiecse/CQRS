-- Auto-generated from EF Core models. Database: CqrsDemo_Cart_Write
IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = N'CqrsDemo_Cart_Write')
    CREATE DATABASE [CqrsDemo_Cart_Write];
GO

USE [CqrsDemo_Cart_Write];
GO

CREATE TABLE [Carts] (
    [Id] uniqueidentifier NOT NULL,
    [CustomerId] uniqueidentifier NOT NULL,
    [Status] nvarchar(50) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Carts] PRIMARY KEY ([Id])
);
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


CREATE TABLE [CartItems] (
    [CartId] uniqueidentifier NOT NULL,
    [ProductId] uniqueidentifier NOT NULL,
    [ProductName] nvarchar(200) NOT NULL,
    [UnitPrice] decimal(18,2) NOT NULL,
    [Quantity] int NOT NULL,
    CONSTRAINT [PK_CartItems] PRIMARY KEY ([CartId], [ProductId]),
    CONSTRAINT [FK_CartItems_Carts_CartId] FOREIGN KEY ([CartId]) REFERENCES [Carts] ([Id]) ON DELETE CASCADE
);
GO


CREATE INDEX [IX_OutboxMessages_ProcessedAt_OccurredOn] ON [OutboxMessages] ([ProcessedAt], [OccurredOn]);
GO



