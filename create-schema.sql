-- Create Schema for CS2 Demo Parser
-- This script creates the basic tables needed for demo parsing

CREATE TABLE [dbo].[DemoFiles] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [FileName] nvarchar(500) NOT NULL,
    [FilePath] nvarchar(1000) NOT NULL,
    [FileSize] bigint NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [ParsedAt] datetime2 NOT NULL,
    [MapName] nvarchar(100) NULL,
    [GameMode] nvarchar(50) NULL,
    [TickRate] int NOT NULL,
    [TotalTicks] int NOT NULL,
    [Duration] real NOT NULL,
    [ServerName] nvarchar(50) NULL,
    [DemoType] nvarchar(500) NULL,
    [NetworkProtocol] int NOT NULL,
    [ClientName] nvarchar(100) NULL,
    [SignonState] nvarchar(500) NULL,
    [DemoSource] nvarchar(50) NULL,
    CONSTRAINT [PK_DemoFiles] PRIMARY KEY ([Id])
);

CREATE TABLE [dbo].[Matches] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [DemoFileId] int NOT NULL,
    [MapName] nvarchar(100) NULL,
    [GameMode] nvarchar(50) NULL,
    [StartTime] datetime2 NOT NULL,
    [EndTime] datetime2 NOT NULL,
    CONSTRAINT [PK_Matches] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Matches_DemoFiles_DemoFileId] FOREIGN KEY ([DemoFileId]) REFERENCES [dbo].[DemoFiles] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [dbo].[Players] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [DemoFileId] int NOT NULL,
    [SteamId] bigint NOT NULL,
    [PlayerName] nvarchar(100) NOT NULL,
    [PlayerSlot] int NOT NULL,
    [Team] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_Players] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Players_DemoFiles_DemoFileId] FOREIGN KEY ([DemoFileId]) REFERENCES [dbo].[DemoFiles] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [dbo].[Rounds] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [MatchId] int NOT NULL,
    [RoundNumber] int NOT NULL,
    [StartTime] datetime2 NOT NULL,
    [EndTime] datetime2 NOT NULL,
    [WinnerTeam] nvarchar(50) NOT NULL,
    [EndReason] nvarchar(100) NOT NULL,
    CONSTRAINT [PK_Rounds] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Rounds_Matches_MatchId] FOREIGN KEY ([MatchId]) REFERENCES [dbo].[Matches] ([Id]) ON DELETE CASCADE
);

-- Core event tables
CREATE TABLE [dbo].[Kills] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [RoundId] int NOT NULL,
    [KillerId] int NULL,
    [VictimId] int NOT NULL,
    [AssisterId] int NULL,
    [Weapon] nvarchar(50) NOT NULL,
    [IsHeadshot] bit NOT NULL,
    [KillerPosX] float NOT NULL,
    [KillerPosY] float NOT NULL,
    [KillerPosZ] float NOT NULL,
    [VictimPosX] float NOT NULL,
    [VictimPosY] float NOT NULL,
    [VictimPosZ] float NOT NULL,
    [Tick] int NOT NULL,
    [Timestamp] datetime2 NOT NULL,
    [PenetratedObjects] int NOT NULL,
    [IsWallbang] bit NOT NULL,
    [Distance] float NOT NULL,
    CONSTRAINT [PK_Kills] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Kills_Rounds_RoundId] FOREIGN KEY ([RoundId]) REFERENCES [dbo].[Rounds] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Kills_Players_KillerId] FOREIGN KEY ([KillerId]) REFERENCES [dbo].[Players] ([Id]),
    CONSTRAINT [FK_Kills_Players_VictimId] FOREIGN KEY ([VictimId]) REFERENCES [dbo].[Players] ([Id]),
    CONSTRAINT [FK_Kills_Players_AssisterId] FOREIGN KEY ([AssisterId]) REFERENCES [dbo].[Players] ([Id])
);

CREATE TABLE [dbo].[Damages] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [RoundId] int NOT NULL,
    [AttackerId] int NULL,
    [VictimId] int NOT NULL,
    [Weapon] nvarchar(50) NOT NULL,
    [Damage] int NOT NULL,
    [ArmorDamage] int NOT NULL,
    [Health] int NOT NULL,
    [Armor] int NOT NULL,
    [Hitgroup] nvarchar(50) NOT NULL,
    [AttackerPosX] float NOT NULL,
    [AttackerPosY] float NOT NULL,
    [AttackerPosZ] float NOT NULL,
    [VictimPosX] float NOT NULL,
    [VictimPosY] float NOT NULL,
    [VictimPosZ] float NOT NULL,
    [Tick] int NOT NULL,
    [Timestamp] datetime2 NOT NULL,
    [PenetratedObjects] int NOT NULL,
    [IsWallbang] bit NOT NULL,
    [Distance] float NOT NULL,
    CONSTRAINT [PK_Damages] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Damages_Rounds_RoundId] FOREIGN KEY ([RoundId]) REFERENCES [dbo].[Rounds] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Damages_Players_AttackerId] FOREIGN KEY ([AttackerId]) REFERENCES [dbo].[Players] ([Id]),
    CONSTRAINT [FK_Damages_Players_VictimId] FOREIGN KEY ([VictimId]) REFERENCES [dbo].[Players] ([Id])
);

CREATE TABLE [dbo].[WeaponFires] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [RoundId] int NOT NULL,
    [PlayerId] int NOT NULL,
    [Weapon] nvarchar(50) NOT NULL,
    [PosX] float NOT NULL,
    [PosY] float NOT NULL,
    [PosZ] float NOT NULL,
    [ViewAngleX] float NOT NULL,
    [ViewAngleY] float NOT NULL,
    [Tick] int NOT NULL,
    [Timestamp] datetime2 NOT NULL,
    CONSTRAINT [PK_WeaponFires] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_WeaponFires_Rounds_RoundId] FOREIGN KEY ([RoundId]) REFERENCES [dbo].[Rounds] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_WeaponFires_Players_PlayerId] FOREIGN KEY ([PlayerId]) REFERENCES [dbo].[Players] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [dbo].[Grenades] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [RoundId] int NOT NULL,
    [PlayerId] int NOT NULL,
    [GrenadeType] nvarchar(50) NOT NULL,
    [ThrowPosX] float NOT NULL,
    [ThrowPosY] float NOT NULL,
    [ThrowPosZ] float NOT NULL,
    [DetonatePosX] float NOT NULL,
    [DetonatePosY] float NOT NULL,
    [DetonatePosZ] float NOT NULL,
    [ThrowTick] int NOT NULL,
    [DetonateTick] int NOT NULL,
    [ThrowTimestamp] datetime2 NOT NULL,
    [DetonateTimestamp] datetime2 NOT NULL,
    CONSTRAINT [PK_Grenades] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Grenades_Rounds_RoundId] FOREIGN KEY ([RoundId]) REFERENCES [dbo].[Rounds] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Grenades_Players_PlayerId] FOREIGN KEY ([PlayerId]) REFERENCES [dbo].[Players] ([Id]) ON DELETE NO ACTION
);

-- Create minimal additional tables to avoid errors
CREATE TABLE [dbo].[Bombs] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [RoundId] int NOT NULL,
    [PlayerId] int NULL,
    [EventType] nvarchar(50) NOT NULL,
    [PosX] float NOT NULL,
    [PosY] float NOT NULL,
    [PosZ] float NOT NULL,
    [Tick] int NOT NULL,
    [Timestamp] datetime2 NOT NULL,
    [Site] nvarchar(10) NULL,
    CONSTRAINT [PK_Bombs] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Bombs_Rounds_RoundId] FOREIGN KEY ([RoundId]) REFERENCES [dbo].[Rounds] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Bombs_Players_PlayerId] FOREIGN KEY ([PlayerId]) REFERENCES [dbo].[Players] ([Id])
);

CREATE TABLE [dbo].[PlayerPositions] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [RoundId] int NOT NULL,
    [PlayerId] int NOT NULL,
    [PosX] float NOT NULL,
    [PosY] float NOT NULL,
    [PosZ] float NOT NULL,
    [ViewAngleX] float NOT NULL,
    [ViewAngleY] float NOT NULL,
    [Tick] int NOT NULL,
    [Timestamp] datetime2 NOT NULL,
    CONSTRAINT [PK_PlayerPositions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PlayerPositions_Rounds_RoundId] FOREIGN KEY ([RoundId]) REFERENCES [dbo].[Rounds] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_PlayerPositions_Players_PlayerId] FOREIGN KEY ([PlayerId]) REFERENCES [dbo].[Players] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [dbo].[Equipment] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [RoundId] int NOT NULL,
    [PlayerId] int NOT NULL,
    [Equipment] nvarchar(50) NOT NULL,
    [EventType] nvarchar(20) NOT NULL,
    [Tick] int NOT NULL,
    [Timestamp] datetime2 NOT NULL,
    CONSTRAINT [PK_Equipment] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Equipment_Rounds_RoundId] FOREIGN KEY ([RoundId]) REFERENCES [dbo].[Rounds] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Equipment_Players_PlayerId] FOREIGN KEY ([PlayerId]) REFERENCES [dbo].[Players] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [dbo].[ChatMessages] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [RoundId] int NOT NULL,
    [PlayerId] int NOT NULL,
    [Message] nvarchar(500) NOT NULL,
    [IsTeamMessage] bit NOT NULL,
    [Tick] int NOT NULL,
    [Timestamp] datetime2 NOT NULL,
    CONSTRAINT [PK_ChatMessages] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ChatMessages_Rounds_RoundId] FOREIGN KEY ([RoundId]) REFERENCES [dbo].[Rounds] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ChatMessages_Players_PlayerId] FOREIGN KEY ([PlayerId]) REFERENCES [dbo].[Players] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [dbo].[GameEvents] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [RoundId] int NOT NULL,
    [EventName] nvarchar(100) NOT NULL,
    [EventData] nvarchar(max) NOT NULL,
    [Tick] int NOT NULL,
    [Timestamp] datetime2 NOT NULL,
    CONSTRAINT [PK_GameEvents] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_GameEvents_Rounds_RoundId] FOREIGN KEY ([RoundId]) REFERENCES [dbo].[Rounds] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [dbo].[PlayerMatchStats] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [PlayerId] int NOT NULL,
    [Kills] int NOT NULL,
    [Deaths] int NOT NULL,
    [Assists] int NOT NULL,
    [Score] int NOT NULL,
    [Mvps] int NOT NULL,
    CONSTRAINT [PK_PlayerMatchStats] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PlayerMatchStats_Players_PlayerId] FOREIGN KEY ([PlayerId]) REFERENCES [dbo].[Players] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [dbo].[PlayerRoundStats] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [PlayerId] int NOT NULL,
    [RoundId] int NOT NULL,
    [Kills] int NOT NULL,
    [Deaths] int NOT NULL,
    [Assists] int NOT NULL,
    [Damage] int NOT NULL,
    [EquipmentValue] int NOT NULL,
    [MoneySpent] int NOT NULL,
    [KillReward] int NOT NULL,
    [LiveTime] float NOT NULL,
    CONSTRAINT [PK_PlayerRoundStats] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PlayerRoundStats_Players_PlayerId] FOREIGN KEY ([PlayerId]) REFERENCES [dbo].[Players] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_PlayerRoundStats_Rounds_RoundId] FOREIGN KEY ([RoundId]) REFERENCES [dbo].[Rounds] ([Id]) ON DELETE CASCADE
);

-- Create basic empty tables for other entities to prevent errors
CREATE TABLE [dbo].[GrenadeTrajectories] ([Id] int IDENTITY(1,1) PRIMARY KEY);
CREATE TABLE [dbo].[EconomyEvents] ([Id] int IDENTITY(1,1) PRIMARY KEY);
CREATE TABLE [dbo].[BulletImpacts] ([Id] int IDENTITY(1,1) PRIMARY KEY);
CREATE TABLE [dbo].[PlayerMovements] ([Id] int IDENTITY(1,1) PRIMARY KEY);
CREATE TABLE [dbo].[ZoneEvents] ([Id] int IDENTITY(1,1) PRIMARY KEY);
CREATE TABLE [dbo].[RadioCommands] ([Id] int IDENTITY(1,1) PRIMARY KEY);
CREATE TABLE [dbo].[WeaponStates] ([Id] int IDENTITY(1,1) PRIMARY KEY);
CREATE TABLE [dbo].[FlashEvents] ([Id] int IDENTITY(1,1) PRIMARY KEY);
CREATE TABLE [dbo].[EntityLifecycles] ([Id] int IDENTITY(1,1) PRIMARY KEY);
CREATE TABLE [dbo].[EntityInteractions] ([Id] int IDENTITY(1,1) PRIMARY KEY);
CREATE TABLE [dbo].[EntityVisibilities] ([Id] int IDENTITY(1,1) PRIMARY KEY);
CREATE TABLE [dbo].[EntityEffects] ([Id] int IDENTITY(1,1) PRIMARY KEY);
CREATE TABLE [dbo].[DroppedItems] ([Id] int IDENTITY(1,1) PRIMARY KEY);
CREATE TABLE [dbo].[SmokeClouds] ([Id] int IDENTITY(1,1) PRIMARY KEY);
CREATE TABLE [dbo].[FireAreas] ([Id] int IDENTITY(1,1) PRIMARY KEY);
CREATE TABLE [dbo].[TeamStates] ([Id] int IDENTITY(1,1) PRIMARY KEY);
CREATE TABLE [dbo].[EconomyStates] ([Id] int IDENTITY(1,1) PRIMARY KEY);
CREATE TABLE [dbo].[MapControls] ([Id] int IDENTITY(1,1) PRIMARY KEY);
CREATE TABLE [dbo].[TacticalEvents] ([Id] int IDENTITY(1,1) PRIMARY KEY);
CREATE TABLE [dbo].[AdvancedPlayerStats] ([Id] int IDENTITY(1,1) PRIMARY KEY);
CREATE TABLE [dbo].[PerformanceMetrics] ([Id] int IDENTITY(1,1) PRIMARY KEY);
CREATE TABLE [dbo].[RoundImpacts] ([Id] int IDENTITY(1,1) PRIMARY KEY);
CREATE TABLE [dbo].[VoiceCommunications] ([Id] int IDENTITY(1,1) PRIMARY KEY);
CREATE TABLE [dbo].[CommunicationPatterns] ([Id] int IDENTITY(1,1) PRIMARY KEY);
CREATE TABLE [dbo].[TemporaryEntities] ([Id] int IDENTITY(1,1) PRIMARY KEY);
CREATE TABLE [dbo].[EntityPropertyChanges] ([Id] int IDENTITY(1,1) PRIMARY KEY);
CREATE TABLE [dbo].[HostageEvents] ([Id] int IDENTITY(1,1) PRIMARY KEY);
CREATE TABLE [dbo].[AdvancedUserMessages] ([Id] int IDENTITY(1,1) PRIMARY KEY);
CREATE TABLE [dbo].[PlayerBehaviorEvents] ([Id] int IDENTITY(1,1) PRIMARY KEY);
CREATE TABLE [dbo].[InfernoEvents] ([Id] int IDENTITY(1,1) PRIMARY KEY);

-- Create EF Migrations table
CREATE TABLE [dbo].[__EFMigrationsHistory] (
    [MigrationId] nvarchar(150) NOT NULL,
    [ProductVersion] nvarchar(32) NOT NULL,
    CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
);

-- Insert migration records
INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES 
    ('20250629134818_InitialSqlServerFinal', '8.0.0'),
    ('20250630224256_AddDemoSource', '8.0.0');

-- Create indexes for performance
CREATE INDEX [IX_Matches_DemoFileId] ON [dbo].[Matches] ([DemoFileId]);
CREATE INDEX [IX_Players_DemoFileId] ON [dbo].[Players] ([DemoFileId]);
CREATE INDEX [IX_Rounds_MatchId] ON [dbo].[Rounds] ([MatchId]);
CREATE INDEX [IX_Kills_RoundId] ON [dbo].[Kills] ([RoundId]);
CREATE INDEX [IX_Damages_RoundId] ON [dbo].[Damages] ([RoundId]);
CREATE INDEX [IX_WeaponFires_RoundId] ON [dbo].[WeaponFires] ([RoundId]);
CREATE INDEX [IX_Grenades_RoundId] ON [dbo].[Grenades] ([RoundId]);

PRINT 'Database schema created successfully!';