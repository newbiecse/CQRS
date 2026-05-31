-- Auto-generated from EF Core models. Database: CqrsDemo_Order_Read
IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = N'CqrsDemo_Order_Read')
    CREATE DATABASE [CqrsDemo_Order_Read];
GO

USE [CqrsDemo_Order_Read];
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


CREATE INDEX [IX_OrderLines_OrderId] ON [OrderLines] ([OrderId]);
GO



