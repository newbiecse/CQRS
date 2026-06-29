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

CREATE TABLE [Roles] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(64) NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [IsSystem] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Roles] PRIMARY KEY ([Id])
);
GO

CREATE UNIQUE INDEX [IX_Roles_Name] ON [Roles] ([Name]);
GO

CREATE TABLE [Permissions] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(128) NOT NULL,
    [Description] nvarchar(256) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Permissions] PRIMARY KEY ([Id])
);
GO

CREATE UNIQUE INDEX [IX_Permissions_Name] ON [Permissions] ([Name]);
GO

CREATE TABLE [RolePermissions] (
    [RoleId] uniqueidentifier NOT NULL,
    [PermissionId] uniqueidentifier NOT NULL,
    CONSTRAINT [PK_RolePermissions] PRIMARY KEY ([RoleId], [PermissionId]),
    CONSTRAINT [FK_RolePermissions_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Roles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_RolePermissions_Permissions_PermissionId] FOREIGN KEY ([PermissionId]) REFERENCES [Permissions] ([Id]) ON DELETE CASCADE
);
GO
