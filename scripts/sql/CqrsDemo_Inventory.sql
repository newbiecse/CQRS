-- Database: CqrsDemo_Inventory
IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = N'CqrsDemo_Inventory')
    CREATE DATABASE [CqrsDemo_Inventory];
GO

USE [CqrsDemo_Inventory];
GO

CREATE TABLE [InventoryItems] (
    [ProductId] uniqueidentifier NOT NULL,
    [ProductName] nvarchar(200) NOT NULL,
    [OnHand] int NOT NULL,
    [Reserved] int NOT NULL,
    [LastUpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_InventoryItems] PRIMARY KEY ([ProductId])
);
GO

CREATE TABLE [OrderReservations] (
    [OrderId] uniqueidentifier NOT NULL,
    [ProductId] uniqueidentifier NOT NULL,
    [Quantity] int NOT NULL,
    CONSTRAINT [PK_OrderReservations] PRIMARY KEY ([OrderId], [ProductId])
);
GO

CREATE INDEX [IX_OrderReservations_OrderId] ON [OrderReservations] ([OrderId]);
GO
