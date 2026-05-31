-- Auto-generated from EF Core models. Database: CqrsDemo_Saga
IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = N'CqrsDemo_Saga')
    CREATE DATABASE [CqrsDemo_Saga];
GO

USE [CqrsDemo_Saga];
GO

CREATE TABLE [CheckoutSagas] (
    [Id] uniqueidentifier NOT NULL,
    [CartId] uniqueidentifier NOT NULL,
    [OrderId] uniqueidentifier NULL,
    [PaymentId] uniqueidentifier NULL,
    [SimulatePaymentFailure] bit NOT NULL,
    [State] nvarchar(64) NOT NULL,
    [FailureReason] nvarchar(2000) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_CheckoutSagas] PRIMARY KEY ([Id])
);
GO


CREATE INDEX [IX_CheckoutSagas_CartId] ON [CheckoutSagas] ([CartId]);
GO


CREATE INDEX [IX_CheckoutSagas_OrderId] ON [CheckoutSagas] ([OrderId]);
GO



