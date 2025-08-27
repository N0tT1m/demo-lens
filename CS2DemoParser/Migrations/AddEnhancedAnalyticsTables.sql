-- Enhanced Analytics Tables for Advanced CS2 Demo Analysis
-- This migration adds comprehensive data capture for advanced gameplay insights

-- Player Input Tracking Table
CREATE TABLE [dbo].[PlayerInputs] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [DemoFileId] int NOT NULL,
    [RoundId] int NOT NULL,
    [PlayerId] int NOT NULL,
    [Tick] int NOT NULL,
    [GameTime] real NOT NULL,
    
    -- Input button states
    [Forward] bit NOT NULL DEFAULT 0,
    [Backward] bit NOT NULL DEFAULT 0,
    [Left] bit NOT NULL DEFAULT 0,
    [Right] bit NOT NULL DEFAULT 0,
    [Jump] bit NOT NULL DEFAULT 0,
    [Duck] bit NOT NULL DEFAULT 0,
    [Attack] bit NOT NULL DEFAULT 0,
    [Attack2] bit NOT NULL DEFAULT 0,
    [Reload] bit NOT NULL DEFAULT 0,
    [Use] bit NOT NULL DEFAULT 0,
    [Walk] bit NOT NULL DEFAULT 0,
    [Speed] bit NOT NULL DEFAULT 0,
    
    -- Position and view angles
    [PositionX] decimal(18,2) NOT NULL DEFAULT 0,
    [PositionY] decimal(18,2) NOT NULL DEFAULT 0,
    [PositionZ] decimal(18,2) NOT NULL DEFAULT 0,
    [ViewAngleX] decimal(18,2) NOT NULL DEFAULT 0,
    [ViewAngleY] decimal(18,2) NOT NULL DEFAULT 0,
    
    -- Movement analysis
    [Velocity] real NOT NULL DEFAULT 0,
    [VelocityX] real NOT NULL DEFAULT 0,
    [VelocityY] real NOT NULL DEFAULT 0,
    [VelocityZ] real NOT NULL DEFAULT 0,
    
    -- Advanced movement detection
    [IsCounterStrafing] bit NOT NULL DEFAULT 0,
    [IsPeeking] bit NOT NULL DEFAULT 0,
    [IsJigglePeeking] bit NOT NULL DEFAULT 0,
    [IsBhopping] bit NOT NULL DEFAULT 0,
    [MovementType] nvarchar(50) NULL,
    
    CONSTRAINT [PK_PlayerInputs] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PlayerInputs_DemoFiles] FOREIGN KEY ([DemoFileId]) REFERENCES [dbo].[DemoFiles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_PlayerInputs_Rounds] FOREIGN KEY ([RoundId]) REFERENCES [dbo].[Rounds] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_PlayerInputs_Players] FOREIGN KEY ([PlayerId]) REFERENCES [dbo].[Players] ([Id]) ON DELETE CASCADE
);

-- Weapon State Change Tracking Table
CREATE TABLE [dbo].[WeaponStateChanges] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [DemoFileId] int NOT NULL,
    [RoundId] int NOT NULL,
    [PlayerId] int NOT NULL,
    [Tick] int NOT NULL,
    [GameTime] real NOT NULL,
    
    [WeaponName] nvarchar(100) NOT NULL,
    [WeaponClass] nvarchar(50) NULL,
    [EventType] nvarchar(50) NOT NULL,  -- 'Pickup', 'Drop', 'Switch', 'Reload', 'Fire', 'Zoom'
    
    -- Weapon state information
    [AmmoClip] int NOT NULL DEFAULT 0,
    [AmmoReserve] int NOT NULL DEFAULT 0,
    [IsReloading] bit NOT NULL DEFAULT 0,
    [IsZoomed] bit NOT NULL DEFAULT 0,
    [ZoomLevel] real NOT NULL DEFAULT 0,
    
    -- Position and view angle when event occurred
    [PositionX] decimal(18,2) NOT NULL DEFAULT 0,
    [PositionY] decimal(18,2) NOT NULL DEFAULT 0,
    [PositionZ] decimal(18,2) NOT NULL DEFAULT 0,
    [ViewAngleX] decimal(18,2) NOT NULL DEFAULT 0,
    [ViewAngleY] decimal(18,2) NOT NULL DEFAULT 0,
    
    -- Weapon switch analysis
    [PreviousWeapon] nvarchar(100) NULL,
    [SwitchTime] real NOT NULL DEFAULT 0,
    
    -- Enhanced tracking
    [WeaponItemId] nvarchar(100) NULL,
    [OriginalOwnerSteamId] nvarchar(100) NULL,
    [IsDropped] bit NOT NULL DEFAULT 0,
    [IsThrown] bit NOT NULL DEFAULT 0,
    
    -- Advanced weapon analytics
    [ShotsFiredSinceLastEvent] int NOT NULL DEFAULT 0,
    [AccuracySinceLastEvent] real NOT NULL DEFAULT 0,
    [WasKillShot] bit NOT NULL DEFAULT 0,
    
    CONSTRAINT [PK_WeaponStateChanges] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_WeaponStateChanges_DemoFiles] FOREIGN KEY ([DemoFileId]) REFERENCES [dbo].[DemoFiles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_WeaponStateChanges_Rounds] FOREIGN KEY ([RoundId]) REFERENCES [dbo].[Rounds] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_WeaponStateChanges_Players] FOREIGN KEY ([PlayerId]) REFERENCES [dbo].[Players] ([Id]) ON DELETE CASCADE
);

-- Enhanced Player Position Tracking Table
CREATE TABLE [dbo].[EnhancedPlayerPositions] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [DemoFileId] int NOT NULL,
    [RoundId] int NOT NULL,
    [PlayerId] int NOT NULL,
    [Tick] int NOT NULL,
    [GameTime] real NOT NULL,
    
    -- Position and view data
    [PositionX] decimal(18,2) NOT NULL DEFAULT 0,
    [PositionY] decimal(18,2) NOT NULL DEFAULT 0,
    [PositionZ] decimal(18,2) NOT NULL DEFAULT 0,
    [ViewAngleX] decimal(18,2) NOT NULL DEFAULT 0,
    [ViewAngleY] decimal(18,2) NOT NULL DEFAULT 0,
    
    -- Velocity data
    [VelocityX] real NOT NULL DEFAULT 0,
    [VelocityY] real NOT NULL DEFAULT 0,
    [VelocityZ] real NOT NULL DEFAULT 0,
    [Speed] real NOT NULL DEFAULT 0,
    
    -- Player state
    [Health] int NOT NULL DEFAULT 0,
    [Armor] int NOT NULL DEFAULT 0,
    [HasHelmet] bit NOT NULL DEFAULT 0,
    [IsAlive] bit NOT NULL DEFAULT 1,
    [IsDefusing] bit NOT NULL DEFAULT 0,
    [IsPlanting] bit NOT NULL DEFAULT 0,
    [IsReloading] bit NOT NULL DEFAULT 0,
    [IsScoped] bit NOT NULL DEFAULT 0,
    [IsWalking] bit NOT NULL DEFAULT 0,
    [IsDucking] bit NOT NULL DEFAULT 0,
    [IsBlinded] bit NOT NULL DEFAULT 0,
    
    -- Current weapon information
    [ActiveWeapon] nvarchar(100) NULL,
    [ActiveWeaponClass] nvarchar(50) NULL,
    [AmmoClip] int NOT NULL DEFAULT 0,
    [AmmoReserve] int NOT NULL DEFAULT 0,
    
    -- Money and equipment
    [Money] int NOT NULL DEFAULT 0,
    [EquipmentValue] int NOT NULL DEFAULT 0,
    
    -- Advanced positional analysis
    [MapArea] nvarchar(50) NULL,
    [PositionType] nvarchar(50) NULL,
    [DistanceToNearestEnemy] real NOT NULL DEFAULT 0,
    [DistanceToNearestTeammate] real NOT NULL DEFAULT 0,
    
    -- Line of sight information
    [VisibleEnemies] int NOT NULL DEFAULT 0,
    [VisibleTeammates] int NOT NULL DEFAULT 0,
    
    -- Tactical state
    [IsInSmokeArea] bit NOT NULL DEFAULT 0,
    [IsInFlashArea] bit NOT NULL DEFAULT 0,
    [IsInFireArea] bit NOT NULL DEFAULT 0,
    [HasLineOfSightToBomb] bit NOT NULL DEFAULT 0,
    
    -- Movement pattern analysis
    [MovementAcceleration] real NOT NULL DEFAULT 0,
    [ViewAngleChangeRate] real NOT NULL DEFAULT 0,
    [IsCounterStrafing] bit NOT NULL DEFAULT 0,
    [IsPeeking] bit NOT NULL DEFAULT 0,
    
    -- Team coordination
    [IsWithTeammates] bit NOT NULL DEFAULT 0,
    [TeammatesNearby] int NOT NULL DEFAULT 0,
    
    CONSTRAINT [PK_EnhancedPlayerPositions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_EnhancedPlayerPositions_DemoFiles] FOREIGN KEY ([DemoFileId]) REFERENCES [dbo].[DemoFiles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_EnhancedPlayerPositions_Rounds] FOREIGN KEY ([RoundId]) REFERENCES [dbo].[Rounds] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_EnhancedPlayerPositions_Players] FOREIGN KEY ([PlayerId]) REFERENCES [dbo].[Players] ([Id]) ON DELETE CASCADE
);

-- Indexes for performance optimization
CREATE INDEX [IX_PlayerInputs_DemoFileId_RoundId] ON [dbo].[PlayerInputs] ([DemoFileId], [RoundId]);
CREATE INDEX [IX_PlayerInputs_PlayerId_Tick] ON [dbo].[PlayerInputs] ([PlayerId], [Tick]);
CREATE INDEX [IX_PlayerInputs_GameTime] ON [dbo].[PlayerInputs] ([GameTime]);

CREATE INDEX [IX_WeaponStateChanges_DemoFileId_RoundId] ON [dbo].[WeaponStateChanges] ([DemoFileId], [RoundId]);
CREATE INDEX [IX_WeaponStateChanges_PlayerId_EventType] ON [dbo].[WeaponStateChanges] ([PlayerId], [EventType]);
CREATE INDEX [IX_WeaponStateChanges_WeaponName] ON [dbo].[WeaponStateChanges] ([WeaponName]);

CREATE INDEX [IX_EnhancedPlayerPositions_DemoFileId_RoundId] ON [dbo].[EnhancedPlayerPositions] ([DemoFileId], [RoundId]);
CREATE INDEX [IX_EnhancedPlayerPositions_PlayerId_Tick] ON [dbo].[EnhancedPlayerPositions] ([PlayerId], [Tick]);
CREATE INDEX [IX_EnhancedPlayerPositions_GameTime] ON [dbo].[EnhancedPlayerPositions] ([GameTime]);
CREATE INDEX [IX_EnhancedPlayerPositions_MapArea] ON [dbo].[EnhancedPlayerPositions] ([MapArea]);