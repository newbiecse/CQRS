-- Auto-generated from EF Core models. Database: CqrsDemo_Payment_Read
IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = N'CqrsDemo_Payment_Read')
    CREATE DATABASE [CqrsDemo_Payment_Read];
GO

USE [CqrsDemo_Payment_Read];
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


CREATE INDEX [IX_Payments_OrderId] ON [Payments] ([OrderId]);
GO



