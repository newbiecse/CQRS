-- Auto-generated from EF Core models. Database: CqrsDemo_User_Read
IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = N'CqrsDemo_User_Read')
    CREATE DATABASE [CqrsDemo_User_Read];
GO

USE [CqrsDemo_User_Read];
GO

CREATE TABLE [Users] (
    [Id] uniqueidentifier NOT NULL,
    [Email] nvarchar(320) NOT NULL,
    [DisplayName] nvarchar(200) NOT NULL,
    [IsActive] bit NOT NULL,
    [RegisteredAt] datetime2 NOT NULL,
    [LastUpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
);
GO


CREATE UNIQUE INDEX [IX_Users_Email] ON [Users] ([Email]);
GO



