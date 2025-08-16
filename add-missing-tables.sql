-- Add missing tables to complete the schema

-- Check and create WeaponFires if not exists
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'WeaponFires')
BEGIN
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
END;

-- Check and create other missing tables
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Grenades')
BEGIN
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
END;

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Bombs')
BEGIN
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
END;

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'PlayerPositions')
BEGIN
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
END;

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Equipment')
BEGIN
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
END;

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ChatMessages')
BEGIN
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
END;

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'GameEvents')
BEGIN
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
END;

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'PlayerMatchStats')
BEGIN
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
END;

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'PlayerRoundStats')
BEGIN
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
END;

-- Create all the other advanced tables that may be referenced
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'GrenadeTrajectories')
CREATE TABLE [dbo].[GrenadeTrajectories] ([Id] int IDENTITY(1,1) PRIMARY KEY);

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'EconomyEvents')
CREATE TABLE [dbo].[EconomyEvents] ([Id] int IDENTITY(1,1) PRIMARY KEY);

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'BulletImpacts')
CREATE TABLE [dbo].[BulletImpacts] ([Id] int IDENTITY(1,1) PRIMARY KEY);

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'PlayerMovements')
CREATE TABLE [dbo].[PlayerMovements] ([Id] int IDENTITY(1,1) PRIMARY KEY);

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ZoneEvents')
CREATE TABLE [dbo].[ZoneEvents] ([Id] int IDENTITY(1,1) PRIMARY KEY);

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'RadioCommands')
CREATE TABLE [dbo].[RadioCommands] ([Id] int IDENTITY(1,1) PRIMARY KEY);

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'WeaponStates')
CREATE TABLE [dbo].[WeaponStates] ([Id] int IDENTITY(1,1) PRIMARY KEY);

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'FlashEvents')
CREATE TABLE [dbo].[FlashEvents] ([Id] int IDENTITY(1,1) PRIMARY KEY);

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'EntityLifecycles')
CREATE TABLE [dbo].[EntityLifecycles] ([Id] int IDENTITY(1,1) PRIMARY KEY);

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'EntityInteractions')
CREATE TABLE [dbo].[EntityInteractions] ([Id] int IDENTITY(1,1) PRIMARY KEY);

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'EntityVisibilities')
CREATE TABLE [dbo].[EntityVisibilities] ([Id] int IDENTITY(1,1) PRIMARY KEY);

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'EntityEffects')
CREATE TABLE [dbo].[EntityEffects] ([Id] int IDENTITY(1,1) PRIMARY KEY);

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'DroppedItems')
CREATE TABLE [dbo].[DroppedItems] ([Id] int IDENTITY(1,1) PRIMARY KEY);

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'SmokeClouds')
CREATE TABLE [dbo].[SmokeClouds] ([Id] int IDENTITY(1,1) PRIMARY KEY);

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'FireAreas')
CREATE TABLE [dbo].[FireAreas] ([Id] int IDENTITY(1,1) PRIMARY KEY);

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'TeamStates')
CREATE TABLE [dbo].[TeamStates] ([Id] int IDENTITY(1,1) PRIMARY KEY);

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'EconomyStates')
CREATE TABLE [dbo].[EconomyStates] ([Id] int IDENTITY(1,1) PRIMARY KEY);

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'MapControls')
CREATE TABLE [dbo].[MapControls] ([Id] int IDENTITY(1,1) PRIMARY KEY);

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'TacticalEvents')
CREATE TABLE [dbo].[TacticalEvents] ([Id] int IDENTITY(1,1) PRIMARY KEY);

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'AdvancedPlayerStats')
CREATE TABLE [dbo].[AdvancedPlayerStats] ([Id] int IDENTITY(1,1) PRIMARY KEY);

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'PerformanceMetrics')
CREATE TABLE [dbo].[PerformanceMetrics] ([Id] int IDENTITY(1,1) PRIMARY KEY);

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'RoundImpacts')
CREATE TABLE [dbo].[RoundImpacts] ([Id] int IDENTITY(1,1) PRIMARY KEY);

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'VoiceCommunications')
CREATE TABLE [dbo].[VoiceCommunications] ([Id] int IDENTITY(1,1) PRIMARY KEY);

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'CommunicationPatterns')
CREATE TABLE [dbo].[CommunicationPatterns] ([Id] int IDENTITY(1,1) PRIMARY KEY);

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'TemporaryEntities')
CREATE TABLE [dbo].[TemporaryEntities] ([Id] int IDENTITY(1,1) PRIMARY KEY);

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'EntityPropertyChanges')
CREATE TABLE [dbo].[EntityPropertyChanges] ([Id] int IDENTITY(1,1) PRIMARY KEY);

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'HostageEvents')
CREATE TABLE [dbo].[HostageEvents] ([Id] int IDENTITY(1,1) PRIMARY KEY);

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'AdvancedUserMessages')
CREATE TABLE [dbo].[AdvancedUserMessages] ([Id] int IDENTITY(1,1) PRIMARY KEY);

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'PlayerBehaviorEvents')
CREATE TABLE [dbo].[PlayerBehaviorEvents] ([Id] int IDENTITY(1,1) PRIMARY KEY);

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'InfernoEvents')
CREATE TABLE [dbo].[InfernoEvents] ([Id] int IDENTITY(1,1) PRIMARY KEY);

PRINT 'Missing tables created successfully!';