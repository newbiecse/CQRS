-- Auto-generated from EF Core models. Database: CqrsDemo_Product_Read
IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = N'CqrsDemo_Product_Read')
    CREATE DATABASE [CqrsDemo_Product_Read];
GO

USE [CqrsDemo_Product_Read];
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



