-- Database: CqrsDemo_Identity
IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = N'CqrsDemo_Identity')
    CREATE DATABASE [CqrsDemo_Identity];
GO

USE [CqrsDemo_Identity];
GO

CREATE TABLE [IdentityUsers] (
    [Id] uniqueidentifier NOT NULL,
    [Email] nvarchar(320) NOT NULL,
    [DisplayName] nvarchar(200) NOT NULL,
    [RolesCsv] nvarchar(500) NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_IdentityUsers] PRIMARY KEY ([Id])
);
GO

CREATE UNIQUE INDEX [IX_IdentityUsers_Email] ON [IdentityUsers] ([Email]);
GO

CREATE TABLE [LocalCredentials] (
    [UserId] uniqueidentifier NOT NULL,
    [PasswordHash] nvarchar(500) NOT NULL,
    CONSTRAINT [PK_LocalCredentials] PRIMARY KEY ([UserId])
);
GO

CREATE TABLE [ExternalLogins] (
    [Provider] nvarchar(50) NOT NULL,
    [ProviderUserId] nvarchar(200) NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [Email] nvarchar(320) NULL,
    [LinkedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_ExternalLogins] PRIMARY KEY ([Provider], [ProviderUserId])
);
GO

CREATE INDEX [IX_ExternalLogins_UserId] ON [ExternalLogins] ([UserId]);
GO
