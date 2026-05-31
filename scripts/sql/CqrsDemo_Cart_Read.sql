-- Auto-generated from EF Core models. Database: CqrsDemo_Cart_Read
IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = N'CqrsDemo_Cart_Read')
    CREATE DATABASE [CqrsDemo_Cart_Read];
GO

USE [CqrsDemo_Cart_Read];
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


CREATE UNIQUE INDEX [IX_CartLines_CartId_ProductId] ON [CartLines] ([CartId], [ProductId]);
GO



