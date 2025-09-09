IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [Specializations] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(100) NOT NULL,
    CONSTRAINT [PK_Specializations] PRIMARY KEY ([Id])
);

CREATE TABLE [Users] (
    [Id] int NOT NULL IDENTITY,
    [Username] nvarchar(100) NOT NULL,
    [Email] nvarchar(200) NOT NULL,
    [PasswordHash] nvarchar(max) NOT NULL,
    [Role] nvarchar(max) NOT NULL,
    [ProfileImagePath] nvarchar(max) NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
);

CREATE TABLE [Doctors] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(100) NOT NULL,
    [City] nvarchar(100) NOT NULL,
    [SpecializationId] int NOT NULL,
    [Rating] float NOT NULL,
    [StartTime] time NOT NULL,
    [EndTime] time NOT NULL,
    [SlotDurationMinutes] int NOT NULL,
    [ProfileImagePath] nvarchar(300) NOT NULL DEFAULT N'default.png',
    CONSTRAINT [PK_Doctors] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Doctors_Specializations_SpecializationId] FOREIGN KEY ([SpecializationId]) REFERENCES [Specializations] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Appointments] (
    [Id] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [DoctorId] int NOT NULL,
    [AppointmentDate] datetime2 NOT NULL,
    [TimeSlot] nvarchar(50) NOT NULL,
    [Status] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_Appointments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Appointments_Doctors_DoctorId] FOREIGN KEY ([DoctorId]) REFERENCES [Doctors] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Appointments_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Ratings] (
    [Id] int NOT NULL IDENTITY,
    [DoctorId] int NOT NULL,
    [UserId] int NOT NULL,
    [Score] int NOT NULL,
    CONSTRAINT [PK_Ratings] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Ratings_Doctors_DoctorId] FOREIGN KEY ([DoctorId]) REFERENCES [Doctors] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Ratings_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_Appointments_DoctorId] ON [Appointments] ([DoctorId]);

CREATE INDEX [IX_Appointments_UserId] ON [Appointments] ([UserId]);

CREATE INDEX [IX_Doctors_SpecializationId] ON [Doctors] ([SpecializationId]);

CREATE INDEX [IX_Ratings_DoctorId] ON [Ratings] ([DoctorId]);

CREATE INDEX [IX_Ratings_UserId] ON [Ratings] ([UserId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250903003011_InitialCreate', N'9.0.8');

DECLARE @var sysname;
SELECT @var = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Doctors]') AND [c].[name] = N'Rating');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [Doctors] DROP CONSTRAINT [' + @var + '];');
ALTER TABLE [Doctors] ALTER COLUMN [Rating] decimal(18,2) NOT NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250904062332_AddRatingsAndDoctorAverage', N'9.0.8');

COMMIT;
GO

