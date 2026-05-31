-- Auto-generated from EF Core models. Database: CqrsDemo_Read
IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = N'CqrsDemo_Read')
    CREATE DATABASE [CqrsDemo_Read];
GO

USE [CqrsDemo_Read];
GO

CREATE TABLE [Carts] (
    [Id] uniqueidentifier NOT NULL,
    [CustomerId] uniqueidentifier NOT NULL,
    [Status] nvarchar(50) NOT NULL,
    [Subtotal] decimal(18,2) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [LastUpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Carts] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [Orders] (
    [Id] uniqueidentifier NOT NULL,
    [CustomerId] uniqueidentifier NOT NULL,
    [CartId] uniqueidentifier NOT NULL,
    [Status] nvarchar(50) NOT NULL,
    [TotalAmount] decimal(18,2) NOT NULL,
    [PaymentId] uniqueidentifier NULL,
    [CreatedAt] datetime2 NOT NULL,
    [LastUpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Orders] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [Payments] (
    [Id] uniqueidentifier NOT NULL,
    [OrderId] uniqueidentifier NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [Status] nvarchar(50) NOT NULL,
    [FailureReason] nvarchar(500) NULL,
    [InitiatedAt] datetime2 NOT NULL,
    [LastUpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Payments] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [Products] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(200) NOT NULL,
    [Price] decimal(18,2) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [LastUpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Products] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [CartLines] (
    [Id] uniqueidentifier NOT NULL,
    [CartId] uniqueidentifier NOT NULL,
    [ProductId] uniqueidentifier NOT NULL,
    [ProductName] nvarchar(200) NOT NULL,
    [UnitPrice] decimal(18,2) NOT NULL,
    [Quantity] int NOT NULL,
    CONSTRAINT [PK_CartLines] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CartLines_Carts_CartId] FOREIGN KEY ([CartId]) REFERENCES [Carts] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [OrderLines] (
    [Id] uniqueidentifier NOT NULL,
    [OrderId] uniqueidentifier NOT NULL,
    [ProductId] uniqueidentifier NOT NULL,
    [ProductName] nvarchar(200) NOT NULL,
    [UnitPrice] decimal(18,2) NOT NULL,
    [Quantity] int NOT NULL,
    CONSTRAINT [PK_OrderLines] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_OrderLines_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([Id]) ON DELETE CASCADE
);
GO


CREATE UNIQUE INDEX [IX_CartLines_CartId_ProductId] ON [CartLines] ([CartId], [ProductId]);
GO


CREATE INDEX [IX_OrderLines_OrderId] ON [OrderLines] ([OrderId]);
GO


CREATE INDEX [IX_Payments_OrderId] ON [Payments] ([OrderId]);
GO


CREATE INDEX [IX_Products_Name] ON [Products] ([Name]);
GO



