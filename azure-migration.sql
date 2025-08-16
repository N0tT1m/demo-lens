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
CREATE TABLE [DemoFiles] (
    [Id] int NOT NULL IDENTITY,
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
    CONSTRAINT [PK_DemoFiles] PRIMARY KEY ([Id])
);

CREATE TABLE [Matches] (
    [Id] int NOT NULL IDENTITY,
    [DemoFileId] int NOT NULL,
    [MapName] nvarchar(100) NULL,
    [GameMode] nvarchar(50) NULL,
    [StartTime] datetime2 NOT NULL,
    [EndTime] datetime2 NULL,
    [TotalRounds] int NOT NULL,
    [CTScore] int NOT NULL,
    [TScore] int NOT NULL,
    [CTScoreFirstHalf] int NOT NULL,
    [TScoreFirstHalf] int NOT NULL,
    [CTScoreSecondHalf] int NOT NULL,
    [TScoreSecondHalf] int NOT NULL,
    [CTScoreOvertime] int NULL,
    [TScoreOvertime] int NULL,
    [IsOvertime] bit NOT NULL,
    [IsFinished] bit NOT NULL,
    [WinnerTeam] nvarchar(50) NULL,
    [WinCondition] nvarchar(100) NULL,
    [MaxRounds] int NOT NULL,
    [RoundTimeLimit] real NOT NULL,
    [FreezeTime] real NOT NULL,
    [BuyTime] real NOT NULL,
    CONSTRAINT [PK_Matches] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Matches_DemoFiles_DemoFileId] FOREIGN KEY ([DemoFileId]) REFERENCES [DemoFiles] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Players] (
    [Id] int NOT NULL IDENTITY,
    [DemoFileId] int NOT NULL,
    [PlayerSlot] int NOT NULL,
    [UserId] int NOT NULL,
    [SteamId] decimal(20,0) NOT NULL,
    [PlayerName] nvarchar(100) NULL,
    [Team] nvarchar(50) NULL,
    [IsBot] bit NOT NULL,
    [IsHltv] bit NOT NULL,
    [IsConnected] bit NOT NULL,
    [Rank] int NOT NULL,
    [Wins] int NOT NULL,
    [ClanTag] nvarchar(200) NULL,
    [ConnectedAt] datetime2 NULL,
    [DisconnectedAt] datetime2 NULL,
    [DisconnectReason] nvarchar(500) NULL,
    [PingAverage] int NOT NULL,
    [PingMax] int NOT NULL,
    [PingMin] int NOT NULL,
    [PacketLoss] real NOT NULL,
    CONSTRAINT [PK_Players] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Players_DemoFiles_DemoFileId] FOREIGN KEY ([DemoFileId]) REFERENCES [DemoFiles] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Rounds] (
    [Id] int NOT NULL IDENTITY,
    [DemoFileId] int NOT NULL,
    [MatchId] int NOT NULL,
    [RoundNumber] int NOT NULL,
    [StartTick] int NOT NULL,
    [EndTick] int NULL,
    [StartTime] datetime2 NOT NULL,
    [EndTime] datetime2 NULL,
    [Duration] real NOT NULL,
    [WinnerTeam] nvarchar(50) NULL,
    [EndReason] nvarchar(100) NULL,
    [CTScore] int NOT NULL,
    [TScore] int NOT NULL,
    [CTLivePlayers] int NOT NULL,
    [TLivePlayers] int NOT NULL,
    [CTStartMoney] int NOT NULL,
    [TStartMoney] int NOT NULL,
    [CTEquipmentValue] int NOT NULL,
    [TEquipmentValue] int NOT NULL,
    [BombPlanted] bit NOT NULL,
    [BombDefused] bit NOT NULL,
    [BombExploded] bit NOT NULL,
    [BombSite] int NULL,
    [IsEcoRound] bit NOT NULL,
    [IsForceBuyRound] bit NOT NULL,
    [IsAntiEcoRound] bit NOT NULL,
    [IsPistolRound] bit NOT NULL,
    [IsOvertime] bit NOT NULL,
    CONSTRAINT [PK_Rounds] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Rounds_DemoFiles_DemoFileId] FOREIGN KEY ([DemoFileId]) REFERENCES [DemoFiles] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Rounds_Matches_MatchId] FOREIGN KEY ([MatchId]) REFERENCES [Matches] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [ChatMessages] (
    [Id] int NOT NULL IDENTITY,
    [DemoFileId] int NOT NULL,
    [PlayerId] int NULL,
    [Tick] int NOT NULL,
    [GameTime] real NOT NULL,
    [Message] nvarchar(1000) NULL,
    [SenderName] nvarchar(100) NULL,
    [Team] nvarchar(100) NULL,
    [IsTeamMessage] bit NOT NULL,
    [IsAllChat] bit NOT NULL,
    [IsDeadChat] bit NOT NULL,
    [IsSystemMessage] bit NOT NULL,
    [IsRadioMessage] bit NOT NULL,
    [MessageType] nvarchar(50) NULL,
    [RadioCommand] nvarchar(100) NULL,
    [IsMuted] bit NOT NULL,
    [IsSpam] bit NOT NULL,
    [Timestamp] datetime2 NOT NULL,
    CONSTRAINT [PK_ChatMessages] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ChatMessages_DemoFiles_DemoFileId] FOREIGN KEY ([DemoFileId]) REFERENCES [DemoFiles] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ChatMessages_Players_PlayerId] FOREIGN KEY ([PlayerId]) REFERENCES [Players] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [Equipment] (
    [Id] int NOT NULL IDENTITY,
    [DemoFileId] int NOT NULL,
    [PlayerId] int NOT NULL,
    [Tick] int NOT NULL,
    [GameTime] real NOT NULL,
    [ItemName] nvarchar(100) NULL,
    [ItemType] nvarchar(50) NULL,
    [ItemCategory] nvarchar(50) NULL,
    [Action] nvarchar(50) NULL,
    [Cost] int NOT NULL,
    [Ammo] int NOT NULL,
    [AmmoReserve] int NOT NULL,
    [IsActive] bit NOT NULL,
    [IsDropped] bit NOT NULL,
    [IsPurchased] bit NOT NULL,
    [IsPickedUp] bit NOT NULL,
    [PositionX] decimal(18,2) NOT NULL,
    [PositionY] decimal(18,2) NOT NULL,
    [PositionZ] decimal(18,2) NOT NULL,
    [Team] nvarchar(100) NULL,
    [RoundNumber] int NOT NULL,
    [Quality] int NOT NULL,
    [Wear] int NOT NULL,
    [SkinName] nvarchar(100) NULL,
    [StatTrak] nvarchar(50) NULL,
    [IsStattrak] bit NOT NULL,
    [IsSouvenir] bit NOT NULL,
    [FloatValue] real NOT NULL,
    [PaintSeed] int NOT NULL,
    [ItemId] int NOT NULL,
    [InventoryId] decimal(20,0) NOT NULL,
    [AccountId] decimal(20,0) NOT NULL,
    [Origin] nvarchar(100) NULL,
    [IsDefault] bit NOT NULL,
    [Stickers] int NOT NULL,
    [StickerInfo] nvarchar(500) NULL,
    [IsNameTag] bit NOT NULL,
    [CustomName] nvarchar(100) NULL,
    CONSTRAINT [PK_Equipment] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Equipment_DemoFiles_DemoFileId] FOREIGN KEY ([DemoFileId]) REFERENCES [DemoFiles] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Equipment_Players_PlayerId] FOREIGN KEY ([PlayerId]) REFERENCES [Players] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [GameEvents] (
    [Id] int NOT NULL IDENTITY,
    [DemoFileId] int NOT NULL,
    [Tick] int NOT NULL,
    [GameTime] real NOT NULL,
    [EventName] nvarchar(100) NOT NULL,
    [EventData] nvarchar(2000) NULL,
    [PlayerId] int NULL,
    [PlayerName] nvarchar(100) NULL,
    [Team] nvarchar(50) NULL,
    [RoundNumber] int NULL,
    [Category] nvarchar(100) NULL,
    [Description] nvarchar(500) NULL,
    [IsImportant] bit NOT NULL,
    [PositionX] decimal(18,2) NULL,
    [PositionY] decimal(18,2) NULL,
    [PositionZ] decimal(18,2) NULL,
    [SubCategory] nvarchar(100) NULL,
    [Value1] int NULL,
    [Value2] int NULL,
    [Value3] int NULL,
    [StringValue1] nvarchar(100) NULL,
    [StringValue2] nvarchar(100) NULL,
    [StringValue3] nvarchar(100) NULL,
    [FloatValue1] real NULL,
    [FloatValue2] real NULL,
    [FloatValue3] real NULL,
    [BoolValue1] bit NULL,
    [BoolValue2] bit NULL,
    [BoolValue3] bit NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_GameEvents] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_GameEvents_DemoFiles_DemoFileId] FOREIGN KEY ([DemoFileId]) REFERENCES [DemoFiles] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_GameEvents_Players_PlayerId] FOREIGN KEY ([PlayerId]) REFERENCES [Players] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [PlayerMatchStats] (
    [Id] int NOT NULL IDENTITY,
    [PlayerId] int NOT NULL,
    [MatchId] int NOT NULL,
    [Kills] int NOT NULL,
    [Deaths] int NOT NULL,
    [Assists] int NOT NULL,
    [Score] int NOT NULL,
    [MVPs] int NOT NULL,
    [HeadshotKills] int NOT NULL,
    [HeadshotPercentage] real NOT NULL,
    [TotalDamageDealt] int NOT NULL,
    [TotalDamageReceived] int NOT NULL,
    [ADR] real NOT NULL,
    [FirstKills] int NOT NULL,
    [FirstDeaths] int NOT NULL,
    [TradeKills] int NOT NULL,
    [TradeDeaths] int NOT NULL,
    [SurvivalTime] int NOT NULL,
    [KDRatio] real NOT NULL,
    [Rating] real NOT NULL,
    [HLTV2Rating] real NOT NULL,
    [RoundsPlayed] int NOT NULL,
    [RoundsWon] int NOT NULL,
    [ClutchWins1v1] int NOT NULL,
    [ClutchWins1v2] int NOT NULL,
    [ClutchWins1v3] int NOT NULL,
    [ClutchWins1v4] int NOT NULL,
    [ClutchWins1v5] int NOT NULL,
    [ClutchAttempts1v1] int NOT NULL,
    [ClutchAttempts1v2] int NOT NULL,
    [ClutchAttempts1v3] int NOT NULL,
    [ClutchAttempts1v4] int NOT NULL,
    [ClutchAttempts1v5] int NOT NULL,
    [FlashAssists] int NOT NULL,
    [UtilityDamage] int NOT NULL,
    [EnemiesFlashed] int NOT NULL,
    [TeammatesFlashed] int NOT NULL,
    [FlashDuration] real NOT NULL,
    [BombPlants] int NOT NULL,
    [BombDefuses] int NOT NULL,
    [HostageRescues] int NOT NULL,
    [MoneySpent] int NOT NULL,
    [MoneyEarned] int NOT NULL,
    [ShotsHit] int NOT NULL,
    [ShotsFired] int NOT NULL,
    [Accuracy] real NOT NULL,
    [WallbangKills] int NOT NULL,
    [CollateralKills] int NOT NULL,
    [NoScopeKills] int NOT NULL,
    [BlindKills] int NOT NULL,
    [SmokeKills] int NOT NULL,
    [KASTPercentage] real NOT NULL,
    [MultiKillRounds2K] int NOT NULL,
    [MultiKillRounds3K] int NOT NULL,
    [MultiKillRounds4K] int NOT NULL,
    [MultiKillRounds5K] int NOT NULL,
    CONSTRAINT [PK_PlayerMatchStats] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PlayerMatchStats_Matches_MatchId] FOREIGN KEY ([MatchId]) REFERENCES [Matches] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_PlayerMatchStats_Players_PlayerId] FOREIGN KEY ([PlayerId]) REFERENCES [Players] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [PlayerPositions] (
    [Id] int NOT NULL IDENTITY,
    [DemoFileId] int NOT NULL,
    [PlayerId] int NOT NULL,
    [Tick] int NOT NULL,
    [GameTime] real NOT NULL,
    [PositionX] decimal(18,2) NOT NULL,
    [PositionY] decimal(18,2) NOT NULL,
    [PositionZ] decimal(18,2) NOT NULL,
    [ViewAngleX] decimal(18,2) NOT NULL,
    [ViewAngleY] decimal(18,2) NOT NULL,
    [ViewAngleZ] decimal(18,2) NOT NULL,
    [VelocityX] decimal(18,2) NOT NULL,
    [VelocityY] decimal(18,2) NOT NULL,
    [VelocityZ] decimal(18,2) NOT NULL,
    [Speed] real NOT NULL,
    [IsAlive] bit NOT NULL,
    [Health] int NOT NULL,
    [Armor] int NOT NULL,
    [HasHelmet] bit NOT NULL,
    [HasDefuseKit] bit NOT NULL,
    [IsScoped] bit NOT NULL,
    [IsWalking] bit NOT NULL,
    [IsCrouching] bit NOT NULL,
    [IsReloading] bit NOT NULL,
    [IsDefusing] bit NOT NULL,
    [IsPlanting] bit NOT NULL,
    [ActiveWeapon] nvarchar(100) NULL,
    [Team] nvarchar(100) NULL,
    [Money] int NOT NULL,
    [FlashDuration] int NOT NULL,
    [IsBlind] bit NOT NULL,
    [InSmoke] bit NOT NULL,
    [OnLadder] bit NOT NULL,
    [InAir] bit NOT NULL,
    [IsDucking] bit NOT NULL,
    [StaminaPercentage] real NOT NULL,
    [LookDistance] real NOT NULL,
    CONSTRAINT [PK_PlayerPositions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PlayerPositions_DemoFiles_DemoFileId] FOREIGN KEY ([DemoFileId]) REFERENCES [DemoFiles] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_PlayerPositions_Players_PlayerId] FOREIGN KEY ([PlayerId]) REFERENCES [Players] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [AdvancedPlayerStats] (
    [Id] int NOT NULL IDENTITY,
    [DemoFileId] int NOT NULL,
    [PlayerId] int NOT NULL,
    [MatchId] int NULL,
    [RoundId] int NULL,
    [RoundNumber] int NOT NULL,
    [StatsType] nvarchar(50) NOT NULL,
    [HLTVRating] real NOT NULL,
    [HLTVRating1] real NOT NULL,
    [ImpactRating] real NOT NULL,
    [KASTPercentage] real NOT NULL,
    [KillsPerRound] real NOT NULL,
    [DeathsPerRound] real NOT NULL,
    [AssistsPerRound] real NOT NULL,
    [KillDeathRatio] real NOT NULL,
    [KillAssistSurviveTradeRatio] real NOT NULL,
    [AverageDamagePerRound] real NOT NULL,
    [DamagePerRound] real NOT NULL,
    [EffectiveDamagePerRound] real NOT NULL,
    [WastedDamage] real NOT NULL,
    [DamageEfficiency] real NOT NULL,
    [FirstKillsPerRound] real NOT NULL,
    [FirstDeathsPerRound] real NOT NULL,
    [FirstKillRatio] real NOT NULL,
    [OpeningDuelSuccessRate] real NOT NULL,
    [Clutch1v1Attempts] int NOT NULL,
    [Clutch1v1Wins] int NOT NULL,
    [Clutch1v2Attempts] int NOT NULL,
    [Clutch1v2Wins] int NOT NULL,
    [Clutch1v3Attempts] int NOT NULL,
    [Clutch1v3Wins] int NOT NULL,
    [Clutch1v4Attempts] int NOT NULL,
    [Clutch1v4Wins] int NOT NULL,
    [Clutch1v5Attempts] int NOT NULL,
    [Clutch1v5Wins] int NOT NULL,
    [OverallClutchSuccessRate] real NOT NULL,
    [DoubleKills] int NOT NULL,
    [TripleKills] int NOT NULL,
    [QuadKills] int NOT NULL,
    [PentaKills] int NOT NULL,
    [MultiKillsPerRound] real NOT NULL,
    [SurvivalRate] real NOT NULL,
    [TradeKillPercentage] real NOT NULL,
    [TradeFragPercentage] real NOT NULL,
    [SupportRoundPercentage] real NOT NULL,
    [HeadshotPercentage] real NOT NULL,
    [RifleKillsPercentage] real NOT NULL,
    [PistolKillsPercentage] real NOT NULL,
    [SniperKillsPercentage] real NOT NULL,
    [AwpKillsPerRound] real NOT NULL,
    [ShotAccuracy] real NOT NULL,
    [KillsPerShot] real NOT NULL,
    [UtilityDamagePerRound] real NOT NULL,
    [FlashAssistsPerRound] real NOT NULL,
    [UtilitySuccessRate] real NOT NULL,
    [EnemiesFlashedPerRound] real NOT NULL,
    [TeamFlashesPerRound] real NOT NULL,
    [EconomicImpact] real NOT NULL,
    [SavedRoundsImpact] real NOT NULL,
    [ForceRoundsImpact] real NOT NULL,
    [EcoRoundsImpact] real NOT NULL,
    [EntryFragPercentage] real NOT NULL,
    [LurkKillsPercentage] real NOT NULL,
    [RotationTimingScore] real NOT NULL,
    [DecisionMakingScore] real NOT NULL,
    [RoundsWithKill] real NOT NULL,
    [RoundsWithMultiKill] real NOT NULL,
    [RoundsWithZeroKills] real NOT NULL,
    [HighImpactRounds] real NOT NULL,
    [TeamPlayScore] real NOT NULL,
    [CommunicationScore] real NOT NULL,
    [LeadershipScore] real NOT NULL,
    [PerformanceVariance] real NOT NULL,
    [ClutchConsistency] real NOT NULL,
    [EconomyAdaptability] real NOT NULL,
    [CalculatedAt] datetime2 NOT NULL,
    [SampleSize] int NOT NULL,
    [Notes] nvarchar(255) NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_AdvancedPlayerStats] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AdvancedPlayerStats_DemoFiles_DemoFileId] FOREIGN KEY ([DemoFileId]) REFERENCES [DemoFiles] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_AdvancedPlayerStats_Matches_MatchId] FOREIGN KEY ([MatchId]) REFERENCES [Matches] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_AdvancedPlayerStats_Players_PlayerId] FOREIGN KEY ([PlayerId]) REFERENCES [Players] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_AdvancedPlayerStats_Rounds_RoundId] FOREIGN KEY ([RoundId]) REFERENCES [Rounds] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [AdvancedUserMessages] (
    [Id] int NOT NULL IDENTITY,
    [DemoFileId] int NOT NULL,
    [RoundId] int NULL,
    [PlayerId] int NULL,
    [Tick] int NOT NULL,
    [GameTime] real NOT NULL,
    [MessageType] nvarchar(50) NOT NULL,
    [MessageSubType] nvarchar(50) NULL,
    [MessageContent] nvarchar(255) NULL,
    [MessageData] nvarchar(500) NULL,
    [DamageGiven] int NULL,
    [DamageTaken] int NULL,
    [HitsGiven] int NULL,
    [HitsTaken] int NULL,
    [TargetPlayerId] int NULL,
    [XpGained] int NULL,
    [XpTotal] int NULL,
    [QuestProgress] int NULL,
    [QuestGoal] int NULL,
    [QuestName] nvarchar(100) NULL,
    [VoteTarget] nvarchar(100) NULL,
    [VoteType] nvarchar(50) NULL,
    [VoteReason] nvarchar(100) NULL,
    [VotesRequired] int NULL,
    [VotesFor] int NULL,
    [VotesAgainst] int NULL,
    [VotePassed] bit NULL,
    [StatCategory] nvarchar(50) NULL,
    [StatValue] real NULL,
    [StatPercentile] real NULL,
    [StatComparison] nvarchar(50) NULL,
    [LeaderboardRank] int NULL,
    [LeaderboardScore] int NULL,
    [LeaderboardType] nvarchar(50) NULL,
    [MoneyChange] int NULL,
    [MoneyTotal] int NULL,
    [MoneyReason] nvarchar(50) NULL,
    [DisplayDuration] real NULL,
    [DisplayLocation] nvarchar(50) NULL,
    [IsImportant] bit NOT NULL,
    [IsServerMessage] bit NOT NULL,
    [Team] nvarchar(50) NOT NULL,
    [RoundNumber] int NOT NULL,
    [Description] nvarchar(255) NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_AdvancedUserMessages] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AdvancedUserMessages_DemoFiles_DemoFileId] FOREIGN KEY ([DemoFileId]) REFERENCES [DemoFiles] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_AdvancedUserMessages_Players_PlayerId] FOREIGN KEY ([PlayerId]) REFERENCES [Players] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_AdvancedUserMessages_Players_TargetPlayerId] FOREIGN KEY ([TargetPlayerId]) REFERENCES [Players] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_AdvancedUserMessages_Rounds_RoundId] FOREIGN KEY ([RoundId]) REFERENCES [Rounds] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [Bombs] (
    [Id] int NOT NULL IDENTITY,
    [DemoFileId] int NOT NULL,
    [RoundId] int NOT NULL,
    [EventType] nvarchar(50) NULL,
    [Tick] int NOT NULL,
    [GameTime] real NOT NULL,
    [PlayerId] int NULL,
    [Site] nvarchar(10) NULL,
    [PositionX] decimal(18,2) NOT NULL,
    [PositionY] decimal(18,2) NOT NULL,
    [PositionZ] decimal(18,2) NOT NULL,
    [PlantTime] real NULL,
    [DefuseTime] real NULL,
    [ExplodeTime] real NULL,
    [HasKit] bit NOT NULL,
    [IsDefuseStarted] bit NOT NULL,
    [IsDefuseCancelled] bit NOT NULL,
    [IsPlantStarted] bit NOT NULL,
    [IsPlantCancelled] bit NOT NULL,
    [CTPlayersInRange] int NOT NULL,
    [TPlayersInRange] int NOT NULL,
    [DefuseProgress] real NOT NULL,
    [PlantProgress] real NOT NULL,
    [TimeRemaining] real NOT NULL,
    [Team] nvarchar(100) NULL,
    [IsClutch] bit NOT NULL,
    [ClutchSize] int NOT NULL,
    [HasSmoke] bit NOT NULL,
    [HasFlash] bit NOT NULL,
    [UnderFire] bit NOT NULL,
    [NearestEnemyDistance] real NOT NULL,
    [RoundNumber] int NOT NULL,
    [Description] nvarchar(500) NULL,
    CONSTRAINT [PK_Bombs] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Bombs_DemoFiles_DemoFileId] FOREIGN KEY ([DemoFileId]) REFERENCES [DemoFiles] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Bombs_Players_PlayerId] FOREIGN KEY ([PlayerId]) REFERENCES [Players] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Bombs_Rounds_RoundId] FOREIGN KEY ([RoundId]) REFERENCES [Rounds] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [BulletImpacts] (
    [Id] int NOT NULL IDENTITY,
    [DemoFileId] int NOT NULL,
    [RoundId] int NULL,
    [PlayerId] int NOT NULL,
    [Tick] int NOT NULL,
    [GameTime] real NOT NULL,
    [Weapon] nvarchar(100) NOT NULL,
    [ShooterPositionX] decimal(18,6) NOT NULL,
    [ShooterPositionY] decimal(18,6) NOT NULL,
    [ShooterPositionZ] decimal(18,6) NOT NULL,
    [ImpactPositionX] decimal(18,6) NOT NULL,
    [ImpactPositionY] decimal(18,6) NOT NULL,
    [ImpactPositionZ] decimal(18,6) NOT NULL,
    [ShootAngleX] decimal(18,6) NOT NULL,
    [ShootAngleY] decimal(18,6) NOT NULL,
    [Distance] real NOT NULL,
    [Team] nvarchar(50) NOT NULL,
    [IsScoped] bit NOT NULL,
    [IsMoving] bit NOT NULL,
    [IsCrouching] bit NOT NULL,
    [PenetrationCount] int NOT NULL,
    [SurfaceType] nvarchar(100) NULL,
    [HitPlayer] bit NOT NULL,
    [HitPlayerId] int NULL,
    [HitGroup] nvarchar(50) NULL,
    [DamageDealt] real NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_BulletImpacts] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_BulletImpacts_DemoFiles_DemoFileId] FOREIGN KEY ([DemoFileId]) REFERENCES [DemoFiles] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_BulletImpacts_Players_HitPlayerId] FOREIGN KEY ([HitPlayerId]) REFERENCES [Players] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_BulletImpacts_Players_PlayerId] FOREIGN KEY ([PlayerId]) REFERENCES [Players] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_BulletImpacts_Rounds_RoundId] FOREIGN KEY ([RoundId]) REFERENCES [Rounds] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [CommunicationPatterns] (
    [Id] int NOT NULL IDENTITY,
    [DemoFileId] int NOT NULL,
    [RoundId] int NULL,
    [PatternType] nvarchar(50) NOT NULL,
    [Team] nvarchar(50) NOT NULL,
    [StartTick] int NOT NULL,
    [EndTick] int NOT NULL,
    [StartTime] real NOT NULL,
    [EndTime] real NOT NULL,
    [Duration] real NOT NULL,
    [RoundNumber] int NOT NULL,
    [ParticipantCount] int NOT NULL,
    [ParticipantIds] nvarchar(255) NOT NULL,
    [PrimaryLeaderId] int NULL,
    [CommunicationDensity] real NOT NULL,
    [ResponseRate] real NOT NULL,
    [OverlapPercentage] real NOT NULL,
    [CoordinationQuality] real NOT NULL,
    [InformationQuality] real NOT NULL,
    [LeadershipClarity] real NOT NULL,
    [TeamResponsiveness] real NOT NULL,
    [IsExecutePattern] bit NOT NULL,
    [IsRetakePattern] bit NOT NULL,
    [IsRotationPattern] bit NOT NULL,
    [IsInformationChain] bit NOT NULL,
    [IsLeadershipSequence] bit NOT NULL,
    [AchievedObjective] bit NOT NULL,
    [ImprovedCoordination] bit NOT NULL,
    [CausedConfusion] bit NOT NULL,
    [WastedTime] bit NOT NULL,
    [EffectivenessScore] real NOT NULL,
    [ImpactOnRound] real NOT NULL,
    [PrimaryTopic] nvarchar(100) NULL,
    [SecondaryTopic] nvarchar(100) NULL,
    [CalloutCount] int NOT NULL,
    [OrderCount] int NOT NULL,
    [QuestionCount] int NOT NULL,
    [ResponseCount] int NOT NULL,
    [ConfirmationCount] int NOT NULL,
    [OptimalTiming] nvarchar(100) NULL,
    [TimingScore] real NOT NULL,
    [IsInnovativePattern] bit NOT NULL,
    [IsAdaptiveResponse] bit NOT NULL,
    [IsStandardProtocol] bit NOT NULL,
    [PatternDescription] nvarchar(255) NULL,
    [AdditionalAnalysis] nvarchar(255) NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_CommunicationPatterns] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CommunicationPatterns_DemoFiles_DemoFileId] FOREIGN KEY ([DemoFileId]) REFERENCES [DemoFiles] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_CommunicationPatterns_Players_PrimaryLeaderId] FOREIGN KEY ([PrimaryLeaderId]) REFERENCES [Players] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_CommunicationPatterns_Rounds_RoundId] FOREIGN KEY ([RoundId]) REFERENCES [Rounds] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [Damages] (
    [Id] int NOT NULL IDENTITY,
    [DemoFileId] int NOT NULL,
    [RoundId] int NOT NULL,
    [Tick] int NOT NULL,
    [GameTime] real NOT NULL,
    [AttackerId] int NULL,
    [VictimId] int NOT NULL,
    [Weapon] nvarchar(100) NULL,
    [WeaponClass] nvarchar(50) NULL,
    [HitGroup] nvarchar(50) NULL,
    [DamageAmount] int NOT NULL,
    [DamageArmor] int NOT NULL,
    [Health] int NOT NULL,
    [Armor] int NOT NULL,
    [IsHeadshot] bit NOT NULL,
    [IsWallbang] bit NOT NULL,
    [IsFatal] bit NOT NULL,
    [Distance] real NOT NULL,
    [Penetration] int NOT NULL,
    [AttackerPositionX] decimal(18,2) NOT NULL,
    [AttackerPositionY] decimal(18,2) NOT NULL,
    [AttackerPositionZ] decimal(18,2) NOT NULL,
    [VictimPositionX] decimal(18,2) NOT NULL,
    [VictimPositionY] decimal(18,2) NOT NULL,
    [VictimPositionZ] decimal(18,2) NOT NULL,
    [AttackerViewAngleX] decimal(18,2) NOT NULL,
    [AttackerViewAngleY] decimal(18,2) NOT NULL,
    [VictimViewAngleX] decimal(18,2) NOT NULL,
    [VictimViewAngleY] decimal(18,2) NOT NULL,
    [AttackerTeam] nvarchar(100) NULL,
    [VictimTeam] nvarchar(100) NULL,
    [IsTeamDamage] bit NOT NULL,
    [ThroughSmoke] bit NOT NULL,
    [AttackerBlind] bit NOT NULL,
    [VictimBlind] bit NOT NULL,
    [FlashDuration] int NOT NULL,
    [IsNoScope] bit NOT NULL,
    CONSTRAINT [PK_Damages] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Damages_DemoFiles_DemoFileId] FOREIGN KEY ([DemoFileId]) REFERENCES [DemoFiles] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Damages_Players_AttackerId] FOREIGN KEY ([AttackerId]) REFERENCES [Players] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Damages_Players_VictimId] FOREIGN KEY ([VictimId]) REFERENCES [Players] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Damages_Rounds_RoundId] FOREIGN KEY ([RoundId]) REFERENCES [Rounds] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [DroppedItems] (
    [Id] int NOT NULL IDENTITY,
    [DemoFileId] int NOT NULL,
    [RoundId] int NULL,
    [DropperPlayerId] int NULL,
    [PickerPlayerId] int NULL,
    [ItemType] nvarchar(100) NOT NULL,
    [ItemName] nvarchar(100) NOT NULL,
    [EntityId] int NOT NULL,
    [DropTick] int NOT NULL,
    [DropTime] real NOT NULL,
    [DropPositionX] decimal(18,6) NOT NULL,
    [DropPositionY] decimal(18,6) NOT NULL,
    [DropPositionZ] decimal(18,6) NOT NULL,
    [DropVelocityX] decimal(18,6) NOT NULL,
    [DropVelocityY] decimal(18,6) NOT NULL,
    [DropVelocityZ] decimal(18,6) NOT NULL,
    [DropReason] nvarchar(50) NOT NULL,
    [PickupTick] int NULL,
    [PickupTime] real NULL,
    [PickupPositionX] decimal(18,6) NULL,
    [PickupPositionY] decimal(18,6) NULL,
    [PickupPositionZ] decimal(18,6) NULL,
    [TimeOnGround] real NULL,
    [WasPickedUp] bit NOT NULL,
    [Expired] bit NOT NULL,
    [AmmoClip] int NOT NULL,
    [AmmoReserve] int NOT NULL,
    [Durability] real NOT NULL,
    [Value] int NOT NULL,
    [Quality] int NOT NULL,
    [FloatValue] real NOT NULL,
    [SkinName] nvarchar(100) NULL,
    [IsStattrak] bit NOT NULL,
    [DropperTeam] nvarchar(50) NULL,
    [PickerTeam] nvarchar(50) NULL,
    [RoundNumber] int NOT NULL,
    [Properties] nvarchar(255) NULL,
    [Description] nvarchar(255) NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_DroppedItems] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_DroppedItems_DemoFiles_DemoFileId] FOREIGN KEY ([DemoFileId]) REFERENCES [DemoFiles] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_DroppedItems_Players_DropperPlayerId] FOREIGN KEY ([DropperPlayerId]) REFERENCES [Players] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_DroppedItems_Players_PickerPlayerId] FOREIGN KEY ([PickerPlayerId]) REFERENCES [Players] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_DroppedItems_Rounds_RoundId] FOREIGN KEY ([RoundId]) REFERENCES [Rounds] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [EconomyEvents] (
    [Id] int NOT NULL IDENTITY,
    [DemoFileId] int NOT NULL,
    [RoundId] int NULL,
    [PlayerId] int NOT NULL,
    [Tick] int NOT NULL,
    [GameTime] real NOT NULL,
    [EventType] nvarchar(50) NOT NULL,
    [ItemName] nvarchar(100) NULL,
    [ItemCategory] nvarchar(50) NULL,
    [ItemCost] int NULL,
    [MoneyBefore] int NOT NULL,
    [MoneyAfter] int NOT NULL,
    [MoneyChange] int NOT NULL,
    [ItemQuantity] int NULL,
    [PositionX] decimal(18,6) NOT NULL,
    [PositionY] decimal(18,6) NOT NULL,
    [PositionZ] decimal(18,6) NOT NULL,
    [Team] nvarchar(50) NOT NULL,
    [RoundNumber] int NOT NULL,
    [IsInBuyZone] bit NOT NULL,
    [IsBuyTimeActive] bit NOT NULL,
    [Description] nvarchar(255) NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_EconomyEvents] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_EconomyEvents_DemoFiles_DemoFileId] FOREIGN KEY ([DemoFileId]) REFERENCES [DemoFiles] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_EconomyEvents_Players_PlayerId] FOREIGN KEY ([PlayerId]) REFERENCES [Players] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_EconomyEvents_Rounds_RoundId] FOREIGN KEY ([RoundId]) REFERENCES [Rounds] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [EconomyStates] (
    [Id] int NOT NULL IDENTITY,
    [DemoFileId] int NOT NULL,
    [RoundId] int NULL,
    [Tick] int NOT NULL,
    [GameTime] real NOT NULL,
    [Team] nvarchar(50) NOT NULL,
    [RoundNumber] int NOT NULL,
    [Phase] nvarchar(50) NOT NULL,
    [TotalMoney] int NOT NULL,
    [AverageMoney] int NOT NULL,
    [MaxMoney] int NOT NULL,
    [MinMoney] int NOT NULL,
    [MoneySpent] int NOT NULL,
    [MoneyLost] int NOT NULL,
    [RoundType] nvarchar(50) NOT NULL,
    [BuyPercentage] real NOT NULL,
    [PlayersCanFullBuy] int NOT NULL,
    [PlayersOnEco] int NOT NULL,
    [TotalWeaponValue] int NOT NULL,
    [TotalUtilityValue] int NOT NULL,
    [TotalArmorValue] int NOT NULL,
    [TotalDefuseKitValue] int NOT NULL,
    [DamagePerDollar] real NOT NULL,
    [KillsPerDollar] real NOT NULL,
    [UtilityEfficiency] real NOT NULL,
    [NextRoundMoney] int NOT NULL,
    [CanFullBuyNextRound] bit NOT NULL,
    [RoundsUntilFullBuy] int NOT NULL,
    [ConsecutiveLosses] int NOT NULL,
    [ConsecutiveWins] int NOT NULL,
    [LossBonus] int NOT NULL,
    [EconomicPressure] nvarchar(255) NULL,
    [Description] nvarchar(255) NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_EconomyStates] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_EconomyStates_DemoFiles_DemoFileId] FOREIGN KEY ([DemoFileId]) REFERENCES [DemoFiles] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_EconomyStates_Rounds_RoundId] FOREIGN KEY ([RoundId]) REFERENCES [Rounds] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [EntityEffects] (
    [Id] int NOT NULL IDENTITY,
    [DemoFileId] int NOT NULL,
    [RoundId] int NULL,
    [SourcePlayerId] int NULL,
    [SourceEntityType] nvarchar(100) NOT NULL,
    [SourceEntityName] nvarchar(100) NOT NULL,
    [EffectType] nvarchar(50) NOT NULL,
    [StartTick] int NOT NULL,
    [EndTick] int NULL,
    [StartTime] real NOT NULL,
    [EndTime] real NULL,
    [Duration] real NOT NULL,
    [CenterX] decimal(18,6) NOT NULL,
    [CenterY] decimal(18,6) NOT NULL,
    [CenterZ] decimal(18,6) NOT NULL,
    [Radius] real NOT NULL,
    [MaxIntensity] real NOT NULL,
    [CurrentIntensity] real NOT NULL,
    [PlayersAffected] int NOT NULL,
    [EnemiesAffected] int NOT NULL,
    [TeammatesAffected] int NOT NULL,
    [TotalDamageDealt] real NOT NULL,
    [MaxDamageToSinglePlayer] real NOT NULL,
    [BlocksVision] bit NOT NULL,
    [CausesDamage] bit NOT NULL,
    [ImpairsMovement] bit NOT NULL,
    [Team] nvarchar(50) NULL,
    [RoundNumber] int NOT NULL,
    [Properties] nvarchar(255) NULL,
    [Description] nvarchar(255) NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_EntityEffects] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_EntityEffects_DemoFiles_DemoFileId] FOREIGN KEY ([DemoFileId]) REFERENCES [DemoFiles] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_EntityEffects_Players_SourcePlayerId] FOREIGN KEY ([SourcePlayerId]) REFERENCES [Players] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_EntityEffects_Rounds_RoundId] FOREIGN KEY ([RoundId]) REFERENCES [Rounds] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [EntityInteractions] (
    [Id] int NOT NULL IDENTITY,
    [DemoFileId] int NOT NULL,
    [RoundId] int NULL,
    [InitiatorPlayerId] int NULL,
    [TargetPlayerId] int NULL,
    [InteractionType] nvarchar(50) NOT NULL,
    [SourceEntityType] nvarchar(100) NOT NULL,
    [SourceEntityName] nvarchar(100) NOT NULL,
    [TargetEntityType] nvarchar(100) NULL,
    [TargetEntityName] nvarchar(100) NULL,
    [Tick] int NOT NULL,
    [GameTime] real NOT NULL,
    [PositionX] decimal(18,6) NOT NULL,
    [PositionY] decimal(18,6) NOT NULL,
    [PositionZ] decimal(18,6) NOT NULL,
    [Distance] real NULL,
    [Angle] real NULL,
    [Force] real NULL,
    [Result] nvarchar(255) NULL,
    [IsSuccessful] bit NOT NULL,
    [Properties] nvarchar(255) NULL,
    [Description] nvarchar(255) NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_EntityInteractions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_EntityInteractions_DemoFiles_DemoFileId] FOREIGN KEY ([DemoFileId]) REFERENCES [DemoFiles] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_EntityInteractions_Players_InitiatorPlayerId] FOREIGN KEY ([InitiatorPlayerId]) REFERENCES [Players] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_EntityInteractions_Players_TargetPlayerId] FOREIGN KEY ([TargetPlayerId]) REFERENCES [Players] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_EntityInteractions_Rounds_RoundId] FOREIGN KEY ([RoundId]) REFERENCES [Rounds] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [EntityLifecycles] (
    [Id] int NOT NULL IDENTITY,
    [DemoFileId] int NOT NULL,
    [RoundId] int NULL,
    [PlayerId] int NULL,
    [EntityType] nvarchar(100) NOT NULL,
    [EntityName] nvarchar(100) NOT NULL,
    [EventType] nvarchar(50) NOT NULL,
    [Tick] int NOT NULL,
    [GameTime] real NOT NULL,
    [PositionX] decimal(18,6) NOT NULL,
    [PositionY] decimal(18,6) NOT NULL,
    [PositionZ] decimal(18,6) NOT NULL,
    [Team] nvarchar(50) NULL,
    [RoundNumber] int NOT NULL,
    [EntityId] int NOT NULL,
    [IsActive] bit NOT NULL,
    [Durability] real NULL,
    [Value] int NULL,
    [Properties] nvarchar(255) NULL,
    [Description] nvarchar(255) NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_EntityLifecycles] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_EntityLifecycles_DemoFiles_DemoFileId] FOREIGN KEY ([DemoFileId]) REFERENCES [DemoFiles] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_EntityLifecycles_Players_PlayerId] FOREIGN KEY ([PlayerId]) REFERENCES [Players] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_EntityLifecycles_Rounds_RoundId] FOREIGN KEY ([RoundId]) REFERENCES [Rounds] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [EntityPropertyChanges] (
    [Id] int NOT NULL IDENTITY,
    [DemoFileId] int NOT NULL,
    [RoundId] int NULL,
    [PlayerId] int NULL,
    [Tick] int NOT NULL,
    [GameTime] real NOT NULL,
    [EntityIndex] int NOT NULL,
    [EntityType] nvarchar(100) NOT NULL,
    [PropertyName] nvarchar(100) NOT NULL,
    [OldValue] nvarchar(255) NULL,
    [NewValue] nvarchar(255) NULL,
    [ChangeType] nvarchar(50) NULL,
    [NumericOldValue] real NULL,
    [NumericNewValue] real NULL,
    [ChangeDelta] real NULL,
    [IsSignificantChange] bit NOT NULL,
    [IsGameplayRelevant] bit NOT NULL,
    [ChangeContext] nvarchar(100) NULL,
    [TriggerEvent] nvarchar(100) NULL,
    [CausedByPlayerId] int NULL,
    [Team] nvarchar(50) NOT NULL,
    [RoundNumber] int NOT NULL,
    [AdditionalData] nvarchar(255) NULL,
    [Description] nvarchar(255) NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_EntityPropertyChanges] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_EntityPropertyChanges_DemoFiles_DemoFileId] FOREIGN KEY ([DemoFileId]) REFERENCES [DemoFiles] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_EntityPropertyChanges_Players_CausedByPlayerId] FOREIGN KEY ([CausedByPlayerId]) REFERENCES [Players] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_EntityPropertyChanges_Players_PlayerId] FOREIGN KEY ([PlayerId]) REFERENCES [Players] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_EntityPropertyChanges_Rounds_RoundId] FOREIGN KEY ([RoundId]) REFERENCES [Rounds] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [EntityVisibilities] (
    [Id] int NOT NULL IDENTITY,
    [DemoFileId] int NOT NULL,
    [RoundId] int NULL,
    [ObserverPlayerId] int NOT NULL,
    [TargetPlayerId] int NULL,
    [EntityType] nvarchar(100) NOT NULL,
    [EntityName] nvarchar(100) NULL,
    [Tick] int NOT NULL,
    [GameTime] real NOT NULL,
    [ObserverPositionX] decimal(18,6) NOT NULL,
    [ObserverPositionY] decimal(18,6) NOT NULL,
    [ObserverPositionZ] decimal(18,6) NOT NULL,
    [ObserverViewAngleX] decimal(18,6) NOT NULL,
    [ObserverViewAngleY] decimal(18,6) NOT NULL,
    [TargetPositionX] decimal(18,6) NOT NULL,
    [TargetPositionY] decimal(18,6) NOT NULL,
    [TargetPositionZ] decimal(18,6) NOT NULL,
    [IsVisible] bit NOT NULL,
    [HasLineOfSight] bit NOT NULL,
    [IsInFieldOfView] bit NOT NULL,
    [Distance] real NOT NULL,
    [ViewAngle] real NOT NULL,
    [ObstructionType] nvarchar(100) NULL,
    [VisibilityPercentage] real NULL,
    [Description] nvarchar(255) NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_EntityVisibilities] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_EntityVisibilities_DemoFiles_DemoFileId] FOREIGN KEY ([DemoFileId]) REFERENCES [DemoFiles] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_EntityVisibilities_Players_ObserverPlayerId] FOREIGN KEY ([ObserverPlayerId]) REFERENCES [Players] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_EntityVisibilities_Players_TargetPlayerId] FOREIGN KEY ([TargetPlayerId]) REFERENCES [Players] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_EntityVisibilities_Rounds_RoundId] FOREIGN KEY ([RoundId]) REFERENCES [Rounds] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [FireAreas] (
    [Id] int NOT NULL IDENTITY,
    [DemoFileId] int NOT NULL,
    [RoundId] int NULL,
    [ThrowerPlayerId] int NULL,
    [GrenadeEntityId] int NOT NULL,
    [GrenadeType] nvarchar(50) NOT NULL,
    [StartTick] int NOT NULL,
    [EndTick] int NULL,
    [StartTime] real NOT NULL,
    [EndTime] real NULL,
    [Duration] real NOT NULL,
    [CenterX] decimal(18,6) NOT NULL,
    [CenterY] decimal(18,6) NOT NULL,
    [CenterZ] decimal(18,6) NOT NULL,
    [MaxRadius] real NOT NULL,
    [CurrentRadius] real NOT NULL,
    [Intensity] real NOT NULL,
    [SpreadPattern] nvarchar(255) NULL,
    [SpreadTime] real NOT NULL,
    [PeakTime] real NOT NULL,
    [BurnoutTime] real NOT NULL,
    [DamagePerSecond] real NOT NULL,
    [TotalDamageDealt] real NOT NULL,
    [PlayersAffected] int NOT NULL,
    [TeammatesAffected] int NOT NULL,
    [EnemiesAffected] int NOT NULL,
    [BlocksPath] bit NOT NULL,
    [ForcesCrouch] bit NOT NULL,
    [PreventsBombPlant] bit NOT NULL,
    [PreventsBombDefuse] bit NOT NULL,
    [TacticalPurpose] nvarchar(100) NULL,
    [ExtinguishedBySmoke] bit NOT NULL,
    [ExtinguishingGrenadeId] int NULL,
    [ExtinguishTime] real NULL,
    [Team] nvarchar(50) NOT NULL,
    [RoundNumber] int NOT NULL,
    [Properties] nvarchar(255) NULL,
    [Description] nvarchar(255) NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_FireAreas] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_FireAreas_DemoFiles_DemoFileId] FOREIGN KEY ([DemoFileId]) REFERENCES [DemoFiles] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_FireAreas_Players_ThrowerPlayerId] FOREIGN KEY ([ThrowerPlayerId]) REFERENCES [Players] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_FireAreas_Rounds_RoundId] FOREIGN KEY ([RoundId]) REFERENCES [Rounds] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [FlashEvents] (
    [Id] int NOT NULL IDENTITY,
    [DemoFileId] int NOT NULL,
    [RoundId] int NULL,
    [FlashedPlayerId] int NOT NULL,
    [FlasherPlayerId] int NULL,
    [Tick] int NOT NULL,
    [GameTime] real NOT NULL,
    [FlashDuration] real NOT NULL,
    [FlashAlpha] real NOT NULL,
    [FlashedPlayerPositionX] decimal(18,6) NOT NULL,
    [FlashedPlayerPositionY] decimal(18,6) NOT NULL,
    [FlashedPlayerPositionZ] decimal(18,6) NOT NULL,
    [GrenadePositionX] decimal(18,6) NULL,
    [GrenadePositionY] decimal(18,6) NULL,
    [GrenadePositionZ] decimal(18,6) NULL,
    [Distance] real NULL,
    [FlashedPlayerTeam] nvarchar(50) NOT NULL,
    [FlasherPlayerTeam] nvarchar(50) NULL,
    [IsTeamFlash] bit NOT NULL,
    [IsSelfFlash] bit NOT NULL,
    [RoundNumber] int NOT NULL,
    [Description] nvarchar(255) NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_FlashEvents] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_FlashEvents_DemoFiles_DemoFileId] FOREIGN KEY ([DemoFileId]) REFERENCES [DemoFiles] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_FlashEvents_Players_FlashedPlayerId] FOREIGN KEY ([FlashedPlayerId]) REFERENCES [Players] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_FlashEvents_Players_FlasherPlayerId] FOREIGN KEY ([FlasherPlayerId]) REFERENCES [Players] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_FlashEvents_Rounds_RoundId] FOREIGN KEY ([RoundId]) REFERENCES [Rounds] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [Grenades] (
    [Id] int NOT NULL IDENTITY,
    [DemoFileId] int NOT NULL,
    [RoundId] int NOT NULL,
    [PlayerId] int NOT NULL,
    [ThrowTick] int NOT NULL,
    [ThrowTime] real NOT NULL,
    [DetonateTick] int NULL,
    [DetonateTime] real NULL,
    [GrenadeType] nvarchar(50) NULL,
    [ThrowPositionX] decimal(18,2) NOT NULL,
    [ThrowPositionY] decimal(18,2) NOT NULL,
    [ThrowPositionZ] decimal(18,2) NOT NULL,
    [DetonatePositionX] decimal(18,2) NULL,
    [DetonatePositionY] decimal(18,2) NULL,
    [DetonatePositionZ] decimal(18,2) NULL,
    [ThrowVelocityX] decimal(18,2) NOT NULL,
    [ThrowVelocityY] decimal(18,2) NOT NULL,
    [ThrowVelocityZ] decimal(18,2) NOT NULL,
    [ThrowAngleX] decimal(18,2) NOT NULL,
    [ThrowAngleY] decimal(18,2) NOT NULL,
    [Team] nvarchar(100) NULL,
    [FlightTime] real NOT NULL,
    [EffectRadius] real NOT NULL,
    [PlayersAffected] int NOT NULL,
    [EnemiesAffected] int NOT NULL,
    [TeammatesAffected] int NOT NULL,
    [TotalDamage] real NOT NULL,
    [TotalFlashDuration] real NOT NULL,
    [IsLineup] bit NOT NULL,
    [IsBounce] bit NOT NULL,
    [BounceCount] int NOT NULL,
    [IsRunThrow] bit NOT NULL,
    [IsJumpThrow] bit NOT NULL,
    [IsStandingThrow] bit NOT NULL,
    [IsCrouchThrow] bit NOT NULL,
    [ThrowStyle] nvarchar(100) NULL,
    [ThrowStrength] real NOT NULL,
    CONSTRAINT [PK_Grenades] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Grenades_DemoFiles_DemoFileId] FOREIGN KEY ([DemoFileId]) REFERENCES [DemoFiles] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Grenades_Players_PlayerId] FOREIGN KEY ([PlayerId]) REFERENCES [Players] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Grenades_Rounds_RoundId] FOREIGN KEY ([RoundId]) REFERENCES [Rounds] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [GrenadeTrajectories] (
    [Id] int NOT NULL IDENTITY,
    [DemoFileId] int NOT NULL,
    [RoundId] int NULL,
    [PlayerId] int NOT NULL,
    [ThrowTick] int NOT NULL,
    [ThrowTime] real NOT NULL,
    [GrenadeType] nvarchar(50) NOT NULL,
    [ThrowPositionX] decimal(18,6) NOT NULL,
    [ThrowPositionY] decimal(18,6) NOT NULL,
    [ThrowPositionZ] decimal(18,6) NOT NULL,
    [ThrowVelocityX] decimal(18,6) NOT NULL,
    [ThrowVelocityY] decimal(18,6) NOT NULL,
    [ThrowVelocityZ] decimal(18,6) NOT NULL,
    [ThrowAngleX] decimal(18,6) NOT NULL,
    [ThrowAngleY] decimal(18,6) NOT NULL,
    [DetonateTick] int NULL,
    [DetonateTime] real NULL,
    [DetonatePositionX] decimal(18,6) NULL,
    [DetonatePositionY] decimal(18,6) NULL,
    [DetonatePositionZ] decimal(18,6) NULL,
    [FlightTime] real NULL,
    [BounceCount] int NULL,
    [Team] nvarchar(50) NOT NULL,
    [IsRunThrow] bit NOT NULL,
    [IsJumpThrow] bit NOT NULL,
    [IsCrouchThrow] bit NOT NULL,
    [ThrowStyle] nvarchar(100) NULL,
    [PlayersAffected] int NOT NULL,
    [EnemiesAffected] int NOT NULL,
    [TeammatesAffected] int NOT NULL,
    [EffectRadius] real NULL,
    [DamageDealt] real NULL,
    [FlashDuration] real NULL,
    [IsLineup] bit NOT NULL,
    [Description] nvarchar(255) NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_GrenadeTrajectories] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_GrenadeTrajectories_DemoFiles_DemoFileId] FOREIGN KEY ([DemoFileId]) REFERENCES [DemoFiles] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_GrenadeTrajectories_Players_PlayerId] FOREIGN KEY ([PlayerId]) REFERENCES [Players] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_GrenadeTrajectories_Rounds_RoundId] FOREIGN KEY ([RoundId]) REFERENCES [Rounds] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [HostageEvents] (
    [Id] int NOT NULL IDENTITY,
    [DemoFileId] int NOT NULL,
    [RoundId] int NULL,
    [PlayerId] int NULL,
    [Tick] int NOT NULL,
    [GameTime] real NOT NULL,
    [EventType] nvarchar(50) NOT NULL,
    [HostageEntityId] int NOT NULL,
    [HostageName] nvarchar(50) NULL,
    [PositionX] decimal(18,6) NOT NULL,
    [PositionY] decimal(18,6) NOT NULL,
    [PositionZ] decimal(18,6) NOT NULL,
    [DamageDealt] int NULL,
    [HealthRemaining] int NULL,
    [AttackerWeapon] nvarchar(50) NULL,
    [IsHeadshot] bit NOT NULL,
    [RescueTime] real NULL,
    [DistanceToRescueZone] real NULL,
    [FollowDuration] real NULL,
    [FollowDistance] real NULL,
    [HostageState] nvarchar(50) NULL,
    [WasBeingFollowed] bit NOT NULL,
    [WasBeingRescued] bit NOT NULL,
    [IsRoundWinning] bit NOT NULL,
    [IsLastHostage] bit NOT NULL,
    [Team] nvarchar(50) NOT NULL,
    [RoundNumber] int NOT NULL,
    [Description] nvarchar(255) NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_HostageEvents] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_HostageEvents_DemoFiles_DemoFileId] FOREIGN KEY ([DemoFileId]) REFERENCES [DemoFiles] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_HostageEvents_Players_PlayerId] FOREIGN KEY ([PlayerId]) REFERENCES [Players] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_HostageEvents_Rounds_RoundId] FOREIGN KEY ([RoundId]) REFERENCES [Rounds] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [InfernoEvents] (
    [Id] int NOT NULL IDENTITY,
    [DemoFileId] int NOT NULL,
    [RoundId] int NULL,
    [ThrowerPlayerId] int NULL,
    [StartTick] int NOT NULL,
    [EndTick] int NULL,
    [StartTime] real NOT NULL,
    [EndTime] real NULL,
    [Duration] real NULL,
    [InfernoEntityId] int NOT NULL,
    [EventType] nvarchar(50) NOT NULL,
    [GrenadeType] nvarchar(50) NULL,
    [OriginX] decimal(18,6) NOT NULL,
    [OriginY] decimal(18,6) NOT NULL,
    [OriginZ] decimal(18,6) NOT NULL,
    [SpreadRadius] real NULL,
    [SpreadArea] real NULL,
    [SpreadDirections] int NULL,
    [SpreadPattern] nvarchar(500) NULL,
    [MaxIntensity] real NULL,
    [AverageIntensity] real NULL,
    [DamageDealt] int NULL,
    [PlayersAffected] int NULL,
    [BlockedPath] bit NOT NULL,
    [ClearedPosition] bit NOT NULL,
    [WastedFire] bit NOT NULL,
    [AreaDenied] nvarchar(100) NULL,
    [TacticalPurpose] nvarchar(100) NULL,
    [SurfaceType] nvarchar(50) NULL,
    [HasWaterNearby] bit NOT NULL,
    [WasExtinguished] bit NOT NULL,
    [ExtinguishedByPlayerId] int NULL,
    [EffectivenessScore] real NULL,
    [PlacementQuality] real NULL,
    [TimingScore] real NULL,
    [ContributedToRoundWin] bit NOT NULL,
    [CausedRoundLoss] bit NOT NULL,
    [KillsEnabled] int NULL,
    [DeathsCaused] int NULL,
    [Team] nvarchar(50) NOT NULL,
    [RoundNumber] int NOT NULL,
    [Description] nvarchar(255) NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_InfernoEvents] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_InfernoEvents_DemoFiles_DemoFileId] FOREIGN KEY ([DemoFileId]) REFERENCES [DemoFiles] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_InfernoEvents_Players_ExtinguishedByPlayerId] FOREIGN KEY ([ExtinguishedByPlayerId]) REFERENCES [Players] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_InfernoEvents_Players_ThrowerPlayerId] FOREIGN KEY ([ThrowerPlayerId]) REFERENCES [Players] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_InfernoEvents_Rounds_RoundId] FOREIGN KEY ([RoundId]) REFERENCES [Rounds] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [Kills] (
    [Id] int NOT NULL IDENTITY,
    [DemoFileId] int NOT NULL,
    [RoundId] int NOT NULL,
    [Tick] int NOT NULL,
    [GameTime] real NOT NULL,
    [KillerId] int NULL,
    [VictimId] int NOT NULL,
    [AssisterId] int NULL,
    [Weapon] nvarchar(100) NULL,
    [WeaponClass] nvarchar(50) NULL,
    [HitGroup] nvarchar(50) NULL,
    [IsHeadshot] bit NOT NULL,
    [IsWallbang] bit NOT NULL,
    [IsNoScope] bit NOT NULL,
    [IsBlind] bit NOT NULL,
    [IsSmoke] bit NOT NULL,
    [IsFlash] bit NOT NULL,
    [IsCollateral] bit NOT NULL,
    [IsFirstKill] bit NOT NULL,
    [IsTradeKill] bit NOT NULL,
    [IsClutch] bit NOT NULL,
    [ClutchSize] int NOT NULL,
    [Distance] real NOT NULL,
    [Damage] int NOT NULL,
    [Penetration] int NOT NULL,
    [KillerPositionX] decimal(18,2) NOT NULL,
    [KillerPositionY] decimal(18,2) NOT NULL,
    [KillerPositionZ] decimal(18,2) NOT NULL,
    [VictimPositionX] decimal(18,2) NOT NULL,
    [VictimPositionY] decimal(18,2) NOT NULL,
    [VictimPositionZ] decimal(18,2) NOT NULL,
    [KillerViewAngleX] decimal(18,2) NOT NULL,
    [KillerViewAngleY] decimal(18,2) NOT NULL,
    [VictimViewAngleX] decimal(18,2) NOT NULL,
    [VictimViewAngleY] decimal(18,2) NOT NULL,
    [KillerHealth] int NOT NULL,
    [KillerArmor] int NOT NULL,
    [VictimHealth] int NOT NULL,
    [VictimArmor] int NOT NULL,
    [AssistType] nvarchar(100) NULL,
    [AssistDistance] real NOT NULL,
    [TimeSinceLastDamage] real NOT NULL,
    [IsRevengeKill] bit NOT NULL,
    [IsDominating] bit NOT NULL,
    [IsRevenge] bit NOT NULL,
    [KillerTeam] nvarchar(100) NULL,
    [VictimTeam] nvarchar(100) NULL,
    [IsTeamKill] bit NOT NULL,
    [FlashDuration] int NOT NULL,
    [ThroughSmoke] bit NOT NULL,
    [AttackerBlind] bit NOT NULL,
    [VictimBlind] bit NOT NULL,
    CONSTRAINT [PK_Kills] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Kills_DemoFiles_DemoFileId] FOREIGN KEY ([DemoFileId]) REFERENCES [DemoFiles] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Kills_Players_AssisterId] FOREIGN KEY ([AssisterId]) REFERENCES [Players] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Kills_Players_KillerId] FOREIGN KEY ([KillerId]) REFERENCES [Players] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Kills_Players_VictimId] FOREIGN KEY ([VictimId]) REFERENCES [Players] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Kills_Rounds_RoundId] FOREIGN KEY ([RoundId]) REFERENCES [Rounds] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [MapControls] (
    [Id] int NOT NULL IDENTITY,
    [DemoFileId] int NOT NULL,
    [RoundId] int NULL,
    [Tick] int NOT NULL,
    [GameTime] real NOT NULL,
    [RoundNumber] int NOT NULL,
    [MapName] nvarchar(100) NOT NULL,
    [CTAreaControl] real NOT NULL,
    [TAreaControl] real NOT NULL,
    [NeutralAreaControl] real NOT NULL,
    [ASiteControl] real NOT NULL,
    [BSiteControl] real NOT NULL,
    [MidControl] real NOT NULL,
    [CTPlayersInAsite] int NOT NULL,
    [TPlayersInAsite] int NOT NULL,
    [CTPlayersInBsite] int NOT NULL,
    [TPlayersInBsite] int NOT NULL,
    [CTPlayersInMid] int NOT NULL,
    [TPlayersInMid] int NOT NULL,
    [ControlledChokes] nvarchar(255) NULL,
    [AdvantageousPositions] int NOT NULL,
    [DisadvantageousPositions] int NOT NULL,
    [SmokesCoveringAreas] int NOT NULL,
    [FlashesBlindingAreas] int NOT NULL,
    [HEGrenadesControllingAreas] int NOT NULL,
    [MolotovsBlockingAreas] int NOT NULL,
    [ControlMomentum] real NOT NULL,
    [IsShiftingControl] bit NOT NULL,
    [TimeInControl] real NOT NULL,
    [DominantTeamZone] nvarchar(100) NULL,
    [TerritoryBalance] real NOT NULL,
    [CTRotatingToA] bit NOT NULL,
    [CTRotatingToB] bit NOT NULL,
    [TRotatingToA] bit NOT NULL,
    [TRotatingToB] bit NOT NULL,
    [CTStackedOneSite] bit NOT NULL,
    [TCommittedToSite] bit NOT NULL,
    [ExpectedStrategy] nvarchar(100) NULL,
    [ControlNotes] nvarchar(255) NULL,
    [Description] nvarchar(255) NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_MapControls] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_MapControls_DemoFiles_DemoFileId] FOREIGN KEY ([DemoFileId]) REFERENCES [DemoFiles] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_MapControls_Rounds_RoundId] FOREIGN KEY ([RoundId]) REFERENCES [Rounds] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [PerformanceMetrics] (
    [Id] int NOT NULL IDENTITY,
    [DemoFileId] int NOT NULL,
    [PlayerId] int NOT NULL,
    [RoundId] int NULL,
    [Tick] int NOT NULL,
    [GameTime] real NOT NULL,
    [MetricType] nvarchar(100) NOT NULL,
    [MetricName] nvarchar(100) NOT NULL,
    [Value] real NOT NULL,
    [NormalizedValue] real NOT NULL,
    [Confidence] real NOT NULL,
    [Context] nvarchar(50) NULL,
    [Situation] nvarchar(100) NULL,
    [RoundNumber] int NOT NULL,
    [TeamAverage] real NOT NULL,
    [MatchAverage] real NOT NULL,
    [PercentileRank] real NOT NULL,
    [MovingAverage] real NOT NULL,
    [Trend] real NOT NULL,
    [IsImproving] bit NOT NULL,
    [IsDecreasing] bit NOT NULL,
    [ImpactScore] real NOT NULL,
    [PositiveImpact] bit NOT NULL,
    [NegativeImpact] bit NOT NULL,
    [AdditionalData] nvarchar(255) NULL,
    [Description] nvarchar(255) NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_PerformanceMetrics] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PerformanceMetrics_DemoFiles_DemoFileId] FOREIGN KEY ([DemoFileId]) REFERENCES [DemoFiles] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_PerformanceMetrics_Players_PlayerId] FOREIGN KEY ([PlayerId]) REFERENCES [Players] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_PerformanceMetrics_Rounds_RoundId] FOREIGN KEY ([RoundId]) REFERENCES [Rounds] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [PlayerBehaviorEvents] (
    [Id] int NOT NULL IDENTITY,
    [DemoFileId] int NOT NULL,
    [RoundId] int NULL,
    [PlayerId] int NOT NULL,
    [Tick] int NOT NULL,
    [GameTime] real NOT NULL,
    [BehaviorType] nvarchar(50) NOT NULL,
    [BehaviorSubType] nvarchar(50) NULL,
    [PositionX] decimal(18,6) NOT NULL,
    [PositionY] decimal(18,6) NOT NULL,
    [PositionZ] decimal(18,6) NOT NULL,
    [VelocityX] decimal(18,6) NULL,
    [VelocityY] decimal(18,6) NULL,
    [VelocityZ] decimal(18,6) NULL,
    [Speed] real NULL,
    [Direction] real NULL,
    [ViewAngleX] decimal(18,6) NULL,
    [ViewAngleY] decimal(18,6) NULL,
    [SoundVolume] real NULL,
    [SoundRadius] real NULL,
    [SoundType] nvarchar(50) NULL,
    [SurfaceMaterial] nvarchar(50) NULL,
    [WeaponName] nvarchar(50) NULL,
    [IsWeaponInspection] bit NOT NULL,
    [IsWeaponReload] bit NOT NULL,
    [IsWeaponDraw] bit NOT NULL,
    [IsWeaponHolster] bit NOT NULL,
    [JumpHeight] real NULL,
    [FallDistance] real NULL,
    [FallDamage] real NULL,
    [LandingImpact] real NULL,
    [IsWalking] bit NOT NULL,
    [IsRunning] bit NOT NULL,
    [IsCrouching] bit NOT NULL,
    [IsInAir] bit NOT NULL,
    [IsOnLadder] bit NOT NULL,
    [IsInWater] bit NOT NULL,
    [IsSilentMovement] bit NOT NULL,
    [IsAudibleToEnemies] bit NOT NULL,
    [StealthScore] real NULL,
    [TacticalContext] nvarchar(50) NULL,
    [IsPeeking] bit NOT NULL,
    [IsRetreating] bit NOT NULL,
    [IsAdvancing] bit NOT NULL,
    [IsHoldingAngle] bit NOT NULL,
    [TimeSinceLastAction] real NULL,
    [ActionDuration] real NULL,
    [WasCompromising] bit NOT NULL,
    [WasTactical] bit NOT NULL,
    [Team] nvarchar(50) NOT NULL,
    [RoundNumber] int NOT NULL,
    [Description] nvarchar(255) NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_PlayerBehaviorEvents] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PlayerBehaviorEvents_DemoFiles_DemoFileId] FOREIGN KEY ([DemoFileId]) REFERENCES [DemoFiles] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_PlayerBehaviorEvents_Players_PlayerId] FOREIGN KEY ([PlayerId]) REFERENCES [Players] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_PlayerBehaviorEvents_Rounds_RoundId] FOREIGN KEY ([RoundId]) REFERENCES [Rounds] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [PlayerMovements] (
    [Id] int NOT NULL IDENTITY,
    [DemoFileId] int NOT NULL,
    [RoundId] int NULL,
    [PlayerId] int NOT NULL,
    [Tick] int NOT NULL,
    [GameTime] real NOT NULL,
    [MovementType] nvarchar(50) NOT NULL,
    [PositionX] decimal(18,6) NOT NULL,
    [PositionY] decimal(18,6) NOT NULL,
    [PositionZ] decimal(18,6) NOT NULL,
    [VelocityX] decimal(18,6) NOT NULL,
    [VelocityY] decimal(18,6) NOT NULL,
    [VelocityZ] decimal(18,6) NOT NULL,
    [Speed] real NOT NULL,
    [SpeedHorizontal] real NOT NULL,
    [ViewAngleX] decimal(18,6) NOT NULL,
    [ViewAngleY] decimal(18,6) NOT NULL,
    [Team] nvarchar(50) NOT NULL,
    [IsOnGround] bit NOT NULL,
    [IsInAir] bit NOT NULL,
    [IsDucking] bit NOT NULL,
    [IsWalking] bit NOT NULL,
    [JumpHeight] real NULL,
    [FallDistance] real NULL,
    [IsBhopping] bit NOT NULL,
    [IsStrafing] bit NOT NULL,
    [IsSurfing] bit NOT NULL,
    [MovementTechnique] nvarchar(255) NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_PlayerMovements] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PlayerMovements_DemoFiles_DemoFileId] FOREIGN KEY ([DemoFileId]) REFERENCES [DemoFiles] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_PlayerMovements_Players_PlayerId] FOREIGN KEY ([PlayerId]) REFERENCES [Players] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_PlayerMovements_Rounds_RoundId] FOREIGN KEY ([RoundId]) REFERENCES [Rounds] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [PlayerRoundStats] (
    [Id] int NOT NULL IDENTITY,
    [PlayerId] int NOT NULL,
    [RoundId] int NOT NULL,
    [Kills] int NOT NULL,
    [Deaths] int NOT NULL,
    [Assists] int NOT NULL,
    [Damage] int NOT NULL,
    [UtilityDamage] int NOT NULL,
    [StartMoney] int NOT NULL,
    [MoneySpent] int NOT NULL,
    [EndMoney] int NOT NULL,
    [EquipmentValue] int NOT NULL,
    [IsAlive] bit NOT NULL,
    [Health] int NOT NULL,
    [Armor] int NOT NULL,
    [HasHelmet] bit NOT NULL,
    [HasDefuseKit] bit NOT NULL,
    [HasBomb] bit NOT NULL,
    [Rating] real NOT NULL,
    [ShotsFired] int NOT NULL,
    [ShotsHit] int NOT NULL,
    [Accuracy] real NOT NULL,
    [KAST] bit NOT NULL,
    [MVP] bit NOT NULL,
    [FlashAssists] int NOT NULL,
    [EnemiesFlashed] int NOT NULL,
    [TeammatesFlashed] int NOT NULL,
    [FlashDuration] real NOT NULL,
    [SurvivalTime] real NOT NULL,
    [ObjectiveTime] int NOT NULL,
    [IsClutch] bit NOT NULL,
    [ClutchSize] int NOT NULL,
    [ClutchWon] bit NOT NULL,
    [PositionX] decimal(18,2) NOT NULL,
    [PositionY] decimal(18,2) NOT NULL,
    [PositionZ] decimal(18,2) NOT NULL,
    [ViewAngleX] decimal(18,2) NOT NULL,
    [ViewAngleY] decimal(18,2) NOT NULL,
    [ViewAngleZ] decimal(18,2) NOT NULL,
    CONSTRAINT [PK_PlayerRoundStats] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PlayerRoundStats_Players_PlayerId] FOREIGN KEY ([PlayerId]) REFERENCES [Players] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_PlayerRoundStats_Rounds_RoundId] FOREIGN KEY ([RoundId]) REFERENCES [Rounds] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [RadioCommands] (
    [Id] int NOT NULL IDENTITY,
    [DemoFileId] int NOT NULL,
    [RoundId] int NULL,
    [PlayerId] int NOT NULL,
    [Tick] int NOT NULL,
    [GameTime] real NOT NULL,
    [Command] nvarchar(100) NOT NULL,
    [CommandCategory] nvarchar(50) NOT NULL,
    [PositionX] decimal(18,6) NOT NULL,
    [PositionY] decimal(18,6) NOT NULL,
    [PositionZ] decimal(18,6) NOT NULL,
    [Team] nvarchar(50) NOT NULL,
    [RoundNumber] int NOT NULL,
    [Context] nvarchar(255) NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_RadioCommands] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_RadioCommands_DemoFiles_DemoFileId] FOREIGN KEY ([DemoFileId]) REFERENCES [DemoFiles] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_RadioCommands_Players_PlayerId] FOREIGN KEY ([PlayerId]) REFERENCES [Players] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_RadioCommands_Rounds_RoundId] FOREIGN KEY ([RoundId]) REFERENCES [Rounds] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [RoundImpacts] (
    [Id] int NOT NULL IDENTITY,
    [DemoFileId] int NOT NULL,
    [PlayerId] int NOT NULL,
    [RoundId] int NOT NULL,
    [RoundNumber] int NOT NULL,
    [OverallImpact] real NOT NULL,
    [PositiveImpact] real NOT NULL,
    [NegativeImpact] real NOT NULL,
    [NetImpact] real NOT NULL,
    [FraggingImpact] real NOT NULL,
    [UtilityImpact] real NOT NULL,
    [PositionalImpact] real NOT NULL,
    [EconomicImpact] real NOT NULL,
    [TacticalImpact] real NOT NULL,
    [HasEntryFrag] bit NOT NULL,
    [HasClutchAttempt] bit NOT NULL,
    [HasClutchWin] bit NOT NULL,
    [HasMultiKill] bit NOT NULL,
    [HasCriticalSave] bit NOT NULL,
    [HasGameChangingPlay] bit NOT NULL,
    [EarlyRoundImpact] real NOT NULL,
    [MidRoundImpact] real NOT NULL,
    [LateRoundImpact] real NOT NULL,
    [WinRoundContribution] real NOT NULL,
    [LossRoundImpact] real NOT NULL,
    [RoundOutcomePrediction] real NOT NULL,
    [DecisionQuality] real NOT NULL,
    [GoodDecisions] int NOT NULL,
    [BadDecisions] int NOT NULL,
    [CriticalDecisions] int NOT NULL,
    [TeamSupportImpact] real NOT NULL,
    [LeadershipImpact] real NOT NULL,
    [FollowupImpact] real NOT NULL,
    [RiskTaken] real NOT NULL,
    [RewardAchieved] real NOT NULL,
    [RiskRewardRatio] real NOT NULL,
    [MomentumGenerated] real NOT NULL,
    [MomentumLost] real NOT NULL,
    [MomentumShift] real NOT NULL,
    [RoundType] nvarchar(50) NOT NULL,
    [RoundTypeImpact] real NOT NULL,
    [KeyMoment] nvarchar(100) NULL,
    [KeyMomentImpact] real NOT NULL,
    [WinProbabilityContribution] real NOT NULL,
    [ExpectedValue] real NOT NULL,
    [PerformanceVsExpected] real NOT NULL,
    [ImpactSummary] nvarchar(255) NULL,
    [Notes] nvarchar(255) NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_RoundImpacts] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_RoundImpacts_DemoFiles_DemoFileId] FOREIGN KEY ([DemoFileId]) REFERENCES [DemoFiles] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_RoundImpacts_Players_PlayerId] FOREIGN KEY ([PlayerId]) REFERENCES [Players] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_RoundImpacts_Rounds_RoundId] FOREIGN KEY ([RoundId]) REFERENCES [Rounds] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [SmokeClouds] (
    [Id] int NOT NULL IDENTITY,
    [DemoFileId] int NOT NULL,
    [RoundId] int NULL,
    [ThrowerPlayerId] int NULL,
    [GrenadeEntityId] int NOT NULL,
    [StartTick] int NOT NULL,
    [EndTick] int NULL,
    [StartTime] real NOT NULL,
    [EndTime] real NULL,
    [Duration] real NOT NULL,
    [CenterX] decimal(18,6) NOT NULL,
    [CenterY] decimal(18,6) NOT NULL,
    [CenterZ] decimal(18,6) NOT NULL,
    [MaxRadius] real NOT NULL,
    [CurrentRadius] real NOT NULL,
    [Opacity] real NOT NULL,
    [Phase] nvarchar(50) NOT NULL,
    [ExpansionTime] real NOT NULL,
    [FullTime] real NOT NULL,
    [DissipationTime] real NOT NULL,
    [PlayersObscured] int NOT NULL,
    [SightLinesBlocked] int NOT NULL,
    [BlocksBombsiteView] bit NOT NULL,
    [BlocksChoke] bit NOT NULL,
    [EnabledPlantDefuse] bit NOT NULL,
    [TacticalPurpose] nvarchar(100) NULL,
    [Team] nvarchar(50) NOT NULL,
    [RoundNumber] int NOT NULL,
    [AffectedByWind] bit NOT NULL,
    [WindDirection] real NOT NULL,
    [WindStrength] real NOT NULL,
    [Properties] nvarchar(255) NULL,
    [Description] nvarchar(255) NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_SmokeClouds] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_SmokeClouds_DemoFiles_DemoFileId] FOREIGN KEY ([DemoFileId]) REFERENCES [DemoFiles] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_SmokeClouds_Players_ThrowerPlayerId] FOREIGN KEY ([ThrowerPlayerId]) REFERENCES [Players] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_SmokeClouds_Rounds_RoundId] FOREIGN KEY ([RoundId]) REFERENCES [Rounds] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [TacticalEvents] (
    [Id] int NOT NULL IDENTITY,
    [DemoFileId] int NOT NULL,
    [RoundId] int NULL,
    [InitiatorPlayerId] int NULL,
    [Tick] int NOT NULL,
    [GameTime] real NOT NULL,
    [EventType] nvarchar(100) NOT NULL,
    [Team] nvarchar(50) NOT NULL,
    [RoundNumber] int NOT NULL,
    [TargetArea] nvarchar(100) NULL,
    [SecondaryArea] nvarchar(100) NULL,
    [PlayersInvolved] int NOT NULL,
    [Coordination] real NOT NULL,
    [Timing] real NOT NULL,
    [SmokesUsed] int NOT NULL,
    [FlashesUsed] int NOT NULL,
    [HEGrenadesUsed] int NOT NULL,
    [MolotovsUsed] int NOT NULL,
    [DecoysUsed] int NOT NULL,
    [WasSuccessful] bit NOT NULL,
    [SuccessRate] real NOT NULL,
    [ExecutionQuality] real NOT NULL,
    [StartTime] real NOT NULL,
    [EndTime] real NULL,
    [Duration] real NOT NULL,
    [WasRushed] bit NOT NULL,
    [WasDelayed] bit NOT NULL,
    [CounterTactic] nvarchar(100) NULL,
    [WasCountered] bit NOT NULL,
    [CounterEffectiveness] real NOT NULL,
    [KillsGenerated] int NOT NULL,
    [DeathsCaused] int NOT NULL,
    [DamageDealt] real NOT NULL,
    [AchievedObjective] bit NOT NULL,
    [RoundContext] nvarchar(100) NULL,
    [StrategicIntent] nvarchar(100) NULL,
    [IsInnovativePlay] bit NOT NULL,
    [IsAdaptation] bit NOT NULL,
    [Unpredictability] real NOT NULL,
    [TacticalNotes] nvarchar(255) NULL,
    [Description] nvarchar(255) NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_TacticalEvents] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_TacticalEvents_DemoFiles_DemoFileId] FOREIGN KEY ([DemoFileId]) REFERENCES [DemoFiles] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_TacticalEvents_Players_InitiatorPlayerId] FOREIGN KEY ([InitiatorPlayerId]) REFERENCES [Players] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_TacticalEvents_Rounds_RoundId] FOREIGN KEY ([RoundId]) REFERENCES [Rounds] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [TeamStates] (
    [Id] int NOT NULL IDENTITY,
    [DemoFileId] int NOT NULL,
    [RoundId] int NULL,
    [Tick] int NOT NULL,
    [GameTime] real NOT NULL,
    [Team] nvarchar(50) NOT NULL,
    [RoundNumber] int NOT NULL,
    [TotalMoney] int NOT NULL,
    [AverageMoney] int NOT NULL,
    [TotalEquipmentValue] int NOT NULL,
    [LivingPlayers] int NOT NULL,
    [PlayersWithArmor] int NOT NULL,
    [PlayersWithHelmet] int NOT NULL,
    [PlayersWithDefuseKit] int NOT NULL,
    [RifleCount] int NOT NULL,
    [PistolCount] int NOT NULL,
    [SniperCount] int NOT NULL,
    [SMGCount] int NOT NULL,
    [ShotgunCount] int NOT NULL,
    [HEGrenadeCount] int NOT NULL,
    [FlashbangCount] int NOT NULL,
    [SmokegrenadeCount] int NOT NULL,
    [MolotovCount] int NOT NULL,
    [DecoyCount] int NOT NULL,
    [PrimaryArea] nvarchar(100) NULL,
    [SecondaryArea] nvarchar(100) NULL,
    [TeamSpread] real NOT NULL,
    [IsStacked] bit NOT NULL,
    [IsRotating] bit NOT NULL,
    [IsSaveRound] bit NOT NULL,
    [IsForceRound] bit NOT NULL,
    [IsEcoRound] bit NOT NULL,
    [IsFullBuyRound] bit NOT NULL,
    [IsAntiEcoRound] bit NOT NULL,
    [TeamCohesion] real NOT NULL,
    [TradeKillPotential] real NOT NULL,
    [SiteControl] real NOT NULL,
    [TacticalNotes] nvarchar(255) NULL,
    [Description] nvarchar(255) NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_TeamStates] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_TeamStates_DemoFiles_DemoFileId] FOREIGN KEY ([DemoFileId]) REFERENCES [DemoFiles] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_TeamStates_Rounds_RoundId] FOREIGN KEY ([RoundId]) REFERENCES [Rounds] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [TemporaryEntities] (
    [Id] int NOT NULL IDENTITY,
    [DemoFileId] int NOT NULL,
    [RoundId] int NULL,
    [PlayerId] int NULL,
    [Tick] int NOT NULL,
    [GameTime] real NOT NULL,
    [EntityType] nvarchar(50) NOT NULL,
    [SubType] nvarchar(50) NULL,
    [PositionX] decimal(18,6) NOT NULL,
    [PositionY] decimal(18,6) NOT NULL,
    [PositionZ] decimal(18,6) NOT NULL,
    [DirectionX] decimal(18,6) NULL,
    [DirectionY] decimal(18,6) NULL,
    [DirectionZ] decimal(18,6) NULL,
    [Intensity] real NULL,
    [Duration] real NULL,
    [Scale] real NULL,
    [Material] nvarchar(50) NULL,
    [WeaponName] nvarchar(50) NULL,
    [EndPositionX] decimal(18,6) NULL,
    [EndPositionY] decimal(18,6) NULL,
    [EndPositionZ] decimal(18,6) NULL,
    [TargetEntityId] int NULL,
    [Color] nvarchar(20) NULL,
    [Alpha] real NULL,
    [ImpactForce] real NULL,
    [IsWallbang] bit NOT NULL,
    [PenetrationCount] int NULL,
    [ExplosionRadius] real NULL,
    [DamageRadius] real NULL,
    [Team] nvarchar(50) NOT NULL,
    [RoundNumber] int NOT NULL,
    [AdditionalData] nvarchar(255) NULL,
    [Description] nvarchar(255) NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_TemporaryEntities] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_TemporaryEntities_DemoFiles_DemoFileId] FOREIGN KEY ([DemoFileId]) REFERENCES [DemoFiles] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_TemporaryEntities_Players_PlayerId] FOREIGN KEY ([PlayerId]) REFERENCES [Players] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_TemporaryEntities_Rounds_RoundId] FOREIGN KEY ([RoundId]) REFERENCES [Rounds] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [VoiceCommunications] (
    [Id] int NOT NULL IDENTITY,
    [DemoFileId] int NOT NULL,
    [RoundId] int NULL,
    [SpeakerId] int NOT NULL,
    [StartTick] int NOT NULL,
    [EndTick] int NOT NULL,
    [StartTime] real NOT NULL,
    [EndTime] real NOT NULL,
    [Duration] real NOT NULL,
    [CommunicationType] nvarchar(50) NOT NULL,
    [RadioCommand] nvarchar(100) NULL,
    [CommandCategory] nvarchar(100) NULL,
    [CommandPurpose] nvarchar(100) NULL,
    [VoiceIntensity] real NOT NULL,
    [IsUrgent] bit NOT NULL,
    [IsCalm] bit NOT NULL,
    [IsEmotional] bit NOT NULL,
    [DuringAction] bit NOT NULL,
    [PreRound] bit NOT NULL,
    [MidRound] bit NOT NULL,
    [PostRound] bit NOT NULL,
    [SituationalContext] nvarchar(100) NULL,
    [ToTeam] bit NOT NULL,
    [ToSpecific] bit NOT NULL,
    [TargetPlayerId] int NULL,
    [WasFollowed] bit NOT NULL,
    [WasCorrect] bit NOT NULL,
    [EffectivenessScore] real NOT NULL,
    [ClarityScore] real NOT NULL,
    [SpeakerPositionX] decimal(18,6) NOT NULL,
    [SpeakerPositionY] decimal(18,6) NOT NULL,
    [SpeakerPositionZ] decimal(18,6) NOT NULL,
    [SpeakerArea] nvarchar(100) NULL,
    [TranscribedContent] nvarchar(255) NULL,
    [ContentSummary] nvarchar(255) NULL,
    [TriggeredRotation] bit NOT NULL,
    [TriggeredRegroup] bit NOT NULL,
    [TriggeredExecute] bit NOT NULL,
    [TriggeredSave] bit NOT NULL,
    [Team] nvarchar(50) NOT NULL,
    [RoundNumber] int NOT NULL,
    [IsLeadershipCommunication] bit NOT NULL,
    [IsQuestion] bit NOT NULL,
    [IsOrder] bit NOT NULL,
    [IsResponse] bit NOT NULL,
    [IsCallout] bit NOT NULL,
    [InterruptedOther] bit NOT NULL,
    [WasInterrupted] bit NOT NULL,
    [InterruptedCommunicationId] int NULL,
    [AdditionalData] nvarchar(255) NULL,
    [Description] nvarchar(255) NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_VoiceCommunications] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_VoiceCommunications_DemoFiles_DemoFileId] FOREIGN KEY ([DemoFileId]) REFERENCES [DemoFiles] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_VoiceCommunications_Players_SpeakerId] FOREIGN KEY ([SpeakerId]) REFERENCES [Players] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_VoiceCommunications_Players_TargetPlayerId] FOREIGN KEY ([TargetPlayerId]) REFERENCES [Players] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_VoiceCommunications_Rounds_RoundId] FOREIGN KEY ([RoundId]) REFERENCES [Rounds] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [WeaponFires] (
    [Id] int NOT NULL IDENTITY,
    [DemoFileId] int NOT NULL,
    [RoundId] int NOT NULL,
    [Tick] int NOT NULL,
    [GameTime] real NOT NULL,
    [PlayerId] int NOT NULL,
    [Weapon] nvarchar(100) NULL,
    [WeaponClass] nvarchar(50) NULL,
    [PositionX] decimal(18,2) NOT NULL,
    [PositionY] decimal(18,2) NOT NULL,
    [PositionZ] decimal(18,2) NOT NULL,
    [ViewAngleX] decimal(18,2) NOT NULL,
    [ViewAngleY] decimal(18,2) NOT NULL,
    [ViewAngleZ] decimal(18,2) NOT NULL,
    [Team] nvarchar(100) NULL,
    [IsScoped] bit NOT NULL,
    [IsSilenced] bit NOT NULL,
    [Ammo] int NOT NULL,
    [AmmoReserve] int NOT NULL,
    [RecoilIndex] real NOT NULL,
    [Accuracy] real NOT NULL,
    [Velocity] real NOT NULL,
    [ThroughSmoke] bit NOT NULL,
    [IsBlind] bit NOT NULL,
    [FlashDuration] int NOT NULL,
    CONSTRAINT [PK_WeaponFires] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_WeaponFires_DemoFiles_DemoFileId] FOREIGN KEY ([DemoFileId]) REFERENCES [DemoFiles] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_WeaponFires_Players_PlayerId] FOREIGN KEY ([PlayerId]) REFERENCES [Players] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_WeaponFires_Rounds_RoundId] FOREIGN KEY ([RoundId]) REFERENCES [Rounds] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [WeaponStates] (
    [Id] int NOT NULL IDENTITY,
    [DemoFileId] int NOT NULL,
    [RoundId] int NULL,
    [PlayerId] int NOT NULL,
    [Tick] int NOT NULL,
    [GameTime] real NOT NULL,
    [EventType] nvarchar(50) NOT NULL,
    [WeaponName] nvarchar(100) NOT NULL,
    [AmmoClip] int NOT NULL,
    [AmmoReserve] int NOT NULL,
    [IsScoped] bit NOT NULL,
    [ZoomLevel] int NOT NULL,
    [IsSilenced] bit NOT NULL,
    [IsReloading] bit NOT NULL,
    [PositionX] decimal(18,6) NOT NULL,
    [PositionY] decimal(18,6) NOT NULL,
    [PositionZ] decimal(18,6) NOT NULL,
    [Team] nvarchar(50) NOT NULL,
    [RoundNumber] int NOT NULL,
    [ReloadDuration] real NULL,
    [Description] nvarchar(255) NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_WeaponStates] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_WeaponStates_DemoFiles_DemoFileId] FOREIGN KEY ([DemoFileId]) REFERENCES [DemoFiles] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_WeaponStates_Players_PlayerId] FOREIGN KEY ([PlayerId]) REFERENCES [Players] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_WeaponStates_Rounds_RoundId] FOREIGN KEY ([RoundId]) REFERENCES [Rounds] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [ZoneEvents] (
    [Id] int NOT NULL IDENTITY,
    [DemoFileId] int NOT NULL,
    [RoundId] int NULL,
    [PlayerId] int NOT NULL,
    [Tick] int NOT NULL,
    [GameTime] real NOT NULL,
    [EventType] nvarchar(50) NOT NULL,
    [ZoneType] nvarchar(50) NOT NULL,
    [PositionX] decimal(18,6) NOT NULL,
    [PositionY] decimal(18,6) NOT NULL,
    [PositionZ] decimal(18,6) NOT NULL,
    [Team] nvarchar(50) NOT NULL,
    [RoundNumber] int NOT NULL,
    [TimeInZone] real NULL,
    [Description] nvarchar(255) NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_ZoneEvents] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ZoneEvents_DemoFiles_DemoFileId] FOREIGN KEY ([DemoFileId]) REFERENCES [DemoFiles] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ZoneEvents_Players_PlayerId] FOREIGN KEY ([PlayerId]) REFERENCES [Players] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ZoneEvents_Rounds_RoundId] FOREIGN KEY ([RoundId]) REFERENCES [Rounds] ([Id]) ON DELETE NO ACTION
);

CREATE INDEX [IX_AdvancedPlayerStats_DemoFileId_RoundNumber] ON [AdvancedPlayerStats] ([DemoFileId], [RoundNumber]);

CREATE INDEX [IX_AdvancedPlayerStats_MatchId] ON [AdvancedPlayerStats] ([MatchId]);

CREATE INDEX [IX_AdvancedPlayerStats_PlayerId_StatsType] ON [AdvancedPlayerStats] ([PlayerId], [StatsType]);

CREATE INDEX [IX_AdvancedPlayerStats_RoundId] ON [AdvancedPlayerStats] ([RoundId]);

CREATE INDEX [IX_AdvancedUserMessages_DemoFileId] ON [AdvancedUserMessages] ([DemoFileId]);

CREATE INDEX [IX_AdvancedUserMessages_PlayerId_Tick] ON [AdvancedUserMessages] ([PlayerId], [Tick]);

CREATE INDEX [IX_AdvancedUserMessages_RoundId_MessageType] ON [AdvancedUserMessages] ([RoundId], [MessageType]);

CREATE INDEX [IX_AdvancedUserMessages_TargetPlayerId] ON [AdvancedUserMessages] ([TargetPlayerId]);

CREATE INDEX [IX_Bombs_DemoFileId] ON [Bombs] ([DemoFileId]);

CREATE INDEX [IX_Bombs_PlayerId] ON [Bombs] ([PlayerId]);

CREATE INDEX [IX_Bombs_RoundId_Tick] ON [Bombs] ([RoundId], [Tick]);

CREATE INDEX [IX_BulletImpacts_DemoFileId] ON [BulletImpacts] ([DemoFileId]);

CREATE INDEX [IX_BulletImpacts_HitPlayerId] ON [BulletImpacts] ([HitPlayerId]);

CREATE INDEX [IX_BulletImpacts_PlayerId_Tick] ON [BulletImpacts] ([PlayerId], [Tick]);

CREATE INDEX [IX_BulletImpacts_RoundId_Weapon] ON [BulletImpacts] ([RoundId], [Weapon]);

CREATE INDEX [IX_ChatMessages_DemoFileId_Tick] ON [ChatMessages] ([DemoFileId], [Tick]);

CREATE INDEX [IX_ChatMessages_PlayerId] ON [ChatMessages] ([PlayerId]);

CREATE INDEX [IX_CommunicationPatterns_DemoFileId_Team] ON [CommunicationPatterns] ([DemoFileId], [Team]);

CREATE INDEX [IX_CommunicationPatterns_PrimaryLeaderId] ON [CommunicationPatterns] ([PrimaryLeaderId]);

CREATE INDEX [IX_CommunicationPatterns_RoundId_PatternType] ON [CommunicationPatterns] ([RoundId], [PatternType]);

CREATE INDEX [IX_Damages_AttackerId] ON [Damages] ([AttackerId]);

CREATE INDEX [IX_Damages_DemoFileId] ON [Damages] ([DemoFileId]);

CREATE INDEX [IX_Damages_RoundId_Tick] ON [Damages] ([RoundId], [Tick]);

CREATE INDEX [IX_Damages_VictimId] ON [Damages] ([VictimId]);

CREATE INDEX [IX_DroppedItems_DemoFileId] ON [DroppedItems] ([DemoFileId]);

CREATE INDEX [IX_DroppedItems_DropperPlayerId_DropTick] ON [DroppedItems] ([DropperPlayerId], [DropTick]);

CREATE UNIQUE INDEX [IX_DroppedItems_EntityId] ON [DroppedItems] ([EntityId]);

CREATE INDEX [IX_DroppedItems_PickerPlayerId] ON [DroppedItems] ([PickerPlayerId]);

CREATE INDEX [IX_DroppedItems_RoundId_ItemType] ON [DroppedItems] ([RoundId], [ItemType]);

CREATE INDEX [IX_EconomyEvents_DemoFileId] ON [EconomyEvents] ([DemoFileId]);

CREATE INDEX [IX_EconomyEvents_PlayerId_Tick] ON [EconomyEvents] ([PlayerId], [Tick]);

CREATE INDEX [IX_EconomyEvents_RoundId_EventType] ON [EconomyEvents] ([RoundId], [EventType]);

CREATE INDEX [IX_EconomyStates_DemoFileId_Phase] ON [EconomyStates] ([DemoFileId], [Phase]);

CREATE INDEX [IX_EconomyStates_RoundId_Team] ON [EconomyStates] ([RoundId], [Team]);

CREATE INDEX [IX_EntityEffects_DemoFileId] ON [EntityEffects] ([DemoFileId]);

CREATE INDEX [IX_EntityEffects_RoundId_EffectType] ON [EntityEffects] ([RoundId], [EffectType]);

CREATE INDEX [IX_EntityEffects_SourcePlayerId_StartTick] ON [EntityEffects] ([SourcePlayerId], [StartTick]);

CREATE INDEX [IX_EntityInteractions_DemoFileId] ON [EntityInteractions] ([DemoFileId]);

CREATE INDEX [IX_EntityInteractions_InitiatorPlayerId_Tick] ON [EntityInteractions] ([InitiatorPlayerId], [Tick]);

CREATE INDEX [IX_EntityInteractions_RoundId_InteractionType] ON [EntityInteractions] ([RoundId], [InteractionType]);

CREATE INDEX [IX_EntityInteractions_TargetPlayerId] ON [EntityInteractions] ([TargetPlayerId]);

CREATE INDEX [IX_EntityLifecycles_DemoFileId] ON [EntityLifecycles] ([DemoFileId]);

CREATE INDEX [IX_EntityLifecycles_EntityType_EntityId] ON [EntityLifecycles] ([EntityType], [EntityId]);

CREATE INDEX [IX_EntityLifecycles_PlayerId] ON [EntityLifecycles] ([PlayerId]);

CREATE INDEX [IX_EntityLifecycles_RoundId_EventType] ON [EntityLifecycles] ([RoundId], [EventType]);

CREATE INDEX [IX_EntityPropertyChanges_CausedByPlayerId] ON [EntityPropertyChanges] ([CausedByPlayerId]);

CREATE INDEX [IX_EntityPropertyChanges_DemoFileId] ON [EntityPropertyChanges] ([DemoFileId]);

CREATE INDEX [IX_EntityPropertyChanges_EntityIndex_PropertyName] ON [EntityPropertyChanges] ([EntityIndex], [PropertyName]);

CREATE INDEX [IX_EntityPropertyChanges_PlayerId_Tick] ON [EntityPropertyChanges] ([PlayerId], [Tick]);

CREATE INDEX [IX_EntityPropertyChanges_RoundId_ChangeType] ON [EntityPropertyChanges] ([RoundId], [ChangeType]);

CREATE INDEX [IX_EntityVisibilities_DemoFileId] ON [EntityVisibilities] ([DemoFileId]);

CREATE INDEX [IX_EntityVisibilities_ObserverPlayerId_Tick] ON [EntityVisibilities] ([ObserverPlayerId], [Tick]);

CREATE INDEX [IX_EntityVisibilities_RoundId_EntityType] ON [EntityVisibilities] ([RoundId], [EntityType]);

CREATE INDEX [IX_EntityVisibilities_TargetPlayerId] ON [EntityVisibilities] ([TargetPlayerId]);

CREATE INDEX [IX_Equipment_DemoFileId] ON [Equipment] ([DemoFileId]);

CREATE INDEX [IX_Equipment_PlayerId_Tick] ON [Equipment] ([PlayerId], [Tick]);

CREATE INDEX [IX_FireAreas_DemoFileId] ON [FireAreas] ([DemoFileId]);

CREATE INDEX [IX_FireAreas_RoundId_GrenadeType] ON [FireAreas] ([RoundId], [GrenadeType]);

CREATE INDEX [IX_FireAreas_ThrowerPlayerId_StartTick] ON [FireAreas] ([ThrowerPlayerId], [StartTick]);

CREATE INDEX [IX_FlashEvents_DemoFileId] ON [FlashEvents] ([DemoFileId]);

CREATE INDEX [IX_FlashEvents_FlashedPlayerId_Tick] ON [FlashEvents] ([FlashedPlayerId], [Tick]);

CREATE INDEX [IX_FlashEvents_FlasherPlayerId] ON [FlashEvents] ([FlasherPlayerId]);

CREATE INDEX [IX_FlashEvents_RoundId_FlashDuration] ON [FlashEvents] ([RoundId], [FlashDuration]);

CREATE INDEX [IX_GameEvents_DemoFileId_Tick] ON [GameEvents] ([DemoFileId], [Tick]);

CREATE INDEX [IX_GameEvents_EventName] ON [GameEvents] ([EventName]);

CREATE INDEX [IX_GameEvents_PlayerId] ON [GameEvents] ([PlayerId]);

CREATE INDEX [IX_Grenades_DemoFileId] ON [Grenades] ([DemoFileId]);

CREATE INDEX [IX_Grenades_PlayerId] ON [Grenades] ([PlayerId]);

CREATE INDEX [IX_Grenades_RoundId_ThrowTick] ON [Grenades] ([RoundId], [ThrowTick]);

CREATE INDEX [IX_GrenadeTrajectories_DemoFileId] ON [GrenadeTrajectories] ([DemoFileId]);

CREATE INDEX [IX_GrenadeTrajectories_PlayerId_ThrowTick] ON [GrenadeTrajectories] ([PlayerId], [ThrowTick]);

CREATE INDEX [IX_GrenadeTrajectories_RoundId_GrenadeType] ON [GrenadeTrajectories] ([RoundId], [GrenadeType]);

CREATE INDEX [IX_HostageEvents_DemoFileId] ON [HostageEvents] ([DemoFileId]);

CREATE INDEX [IX_HostageEvents_HostageEntityId_EventType] ON [HostageEvents] ([HostageEntityId], [EventType]);

CREATE INDEX [IX_HostageEvents_PlayerId_Tick] ON [HostageEvents] ([PlayerId], [Tick]);

CREATE INDEX [IX_HostageEvents_RoundId_EventType] ON [HostageEvents] ([RoundId], [EventType]);

CREATE INDEX [IX_InfernoEvents_DemoFileId] ON [InfernoEvents] ([DemoFileId]);

CREATE INDEX [IX_InfernoEvents_ExtinguishedByPlayerId] ON [InfernoEvents] ([ExtinguishedByPlayerId]);

CREATE INDEX [IX_InfernoEvents_InfernoEntityId_EventType] ON [InfernoEvents] ([InfernoEntityId], [EventType]);

CREATE INDEX [IX_InfernoEvents_RoundId_EventType] ON [InfernoEvents] ([RoundId], [EventType]);

CREATE INDEX [IX_InfernoEvents_ThrowerPlayerId_StartTick] ON [InfernoEvents] ([ThrowerPlayerId], [StartTick]);

CREATE INDEX [IX_Kills_AssisterId] ON [Kills] ([AssisterId]);

CREATE INDEX [IX_Kills_DemoFileId] ON [Kills] ([DemoFileId]);

CREATE INDEX [IX_Kills_KillerId] ON [Kills] ([KillerId]);

CREATE INDEX [IX_Kills_RoundId_Tick] ON [Kills] ([RoundId], [Tick]);

CREATE INDEX [IX_Kills_VictimId] ON [Kills] ([VictimId]);

CREATE INDEX [IX_MapControls_DemoFileId_MapName] ON [MapControls] ([DemoFileId], [MapName]);

CREATE INDEX [IX_MapControls_RoundId_Tick] ON [MapControls] ([RoundId], [Tick]);

CREATE INDEX [IX_Matches_DemoFileId] ON [Matches] ([DemoFileId]);

CREATE INDEX [IX_PerformanceMetrics_DemoFileId] ON [PerformanceMetrics] ([DemoFileId]);

CREATE INDEX [IX_PerformanceMetrics_PlayerId_MetricType] ON [PerformanceMetrics] ([PlayerId], [MetricType]);

CREATE INDEX [IX_PerformanceMetrics_RoundId_MetricName] ON [PerformanceMetrics] ([RoundId], [MetricName]);

CREATE INDEX [IX_PlayerBehaviorEvents_DemoFileId_BehaviorType] ON [PlayerBehaviorEvents] ([DemoFileId], [BehaviorType]);

CREATE INDEX [IX_PlayerBehaviorEvents_PlayerId_Tick] ON [PlayerBehaviorEvents] ([PlayerId], [Tick]);

CREATE INDEX [IX_PlayerBehaviorEvents_RoundId_BehaviorType] ON [PlayerBehaviorEvents] ([RoundId], [BehaviorType]);

CREATE INDEX [IX_PlayerMatchStats_MatchId] ON [PlayerMatchStats] ([MatchId]);

CREATE UNIQUE INDEX [IX_PlayerMatchStats_PlayerId_MatchId] ON [PlayerMatchStats] ([PlayerId], [MatchId]);

CREATE INDEX [IX_PlayerMovements_DemoFileId] ON [PlayerMovements] ([DemoFileId]);

CREATE INDEX [IX_PlayerMovements_PlayerId_Tick] ON [PlayerMovements] ([PlayerId], [Tick]);

CREATE INDEX [IX_PlayerMovements_RoundId_MovementType] ON [PlayerMovements] ([RoundId], [MovementType]);

CREATE INDEX [IX_PlayerPositions_DemoFileId] ON [PlayerPositions] ([DemoFileId]);

CREATE INDEX [IX_PlayerPositions_PlayerId_Tick] ON [PlayerPositions] ([PlayerId], [Tick]);

CREATE UNIQUE INDEX [IX_PlayerRoundStats_PlayerId_RoundId] ON [PlayerRoundStats] ([PlayerId], [RoundId]);

CREATE INDEX [IX_PlayerRoundStats_RoundId] ON [PlayerRoundStats] ([RoundId]);

CREATE UNIQUE INDEX [IX_Players_DemoFileId_PlayerSlot] ON [Players] ([DemoFileId], [PlayerSlot]);

CREATE INDEX [IX_Players_SteamId] ON [Players] ([SteamId]);

CREATE INDEX [IX_RadioCommands_DemoFileId] ON [RadioCommands] ([DemoFileId]);

CREATE INDEX [IX_RadioCommands_PlayerId_Tick] ON [RadioCommands] ([PlayerId], [Tick]);

CREATE INDEX [IX_RadioCommands_RoundId_Command] ON [RadioCommands] ([RoundId], [Command]);

CREATE INDEX [IX_RoundImpacts_DemoFileId_OverallImpact] ON [RoundImpacts] ([DemoFileId], [OverallImpact]);

CREATE UNIQUE INDEX [IX_RoundImpacts_PlayerId_RoundId] ON [RoundImpacts] ([PlayerId], [RoundId]);

CREATE INDEX [IX_RoundImpacts_RoundId] ON [RoundImpacts] ([RoundId]);

CREATE INDEX [IX_Rounds_DemoFileId] ON [Rounds] ([DemoFileId]);

CREATE UNIQUE INDEX [IX_Rounds_MatchId_RoundNumber] ON [Rounds] ([MatchId], [RoundNumber]);

CREATE INDEX [IX_SmokeClouds_DemoFileId] ON [SmokeClouds] ([DemoFileId]);

CREATE INDEX [IX_SmokeClouds_RoundId_Phase] ON [SmokeClouds] ([RoundId], [Phase]);

CREATE INDEX [IX_SmokeClouds_ThrowerPlayerId_StartTick] ON [SmokeClouds] ([ThrowerPlayerId], [StartTick]);

CREATE INDEX [IX_TacticalEvents_DemoFileId] ON [TacticalEvents] ([DemoFileId]);

CREATE INDEX [IX_TacticalEvents_InitiatorPlayerId_Tick] ON [TacticalEvents] ([InitiatorPlayerId], [Tick]);

CREATE INDEX [IX_TacticalEvents_RoundId_EventType] ON [TacticalEvents] ([RoundId], [EventType]);

CREATE INDEX [IX_TeamStates_DemoFileId_Tick] ON [TeamStates] ([DemoFileId], [Tick]);

CREATE INDEX [IX_TeamStates_RoundId_Team] ON [TeamStates] ([RoundId], [Team]);

CREATE INDEX [IX_TemporaryEntities_DemoFileId_EntityType] ON [TemporaryEntities] ([DemoFileId], [EntityType]);

CREATE INDEX [IX_TemporaryEntities_PlayerId_Tick] ON [TemporaryEntities] ([PlayerId], [Tick]);

CREATE INDEX [IX_TemporaryEntities_RoundId_EntityType] ON [TemporaryEntities] ([RoundId], [EntityType]);

CREATE INDEX [IX_VoiceCommunications_DemoFileId] ON [VoiceCommunications] ([DemoFileId]);

CREATE INDEX [IX_VoiceCommunications_RoundId_CommunicationType] ON [VoiceCommunications] ([RoundId], [CommunicationType]);

CREATE INDEX [IX_VoiceCommunications_SpeakerId_StartTick] ON [VoiceCommunications] ([SpeakerId], [StartTick]);

CREATE INDEX [IX_VoiceCommunications_TargetPlayerId] ON [VoiceCommunications] ([TargetPlayerId]);

CREATE INDEX [IX_WeaponFires_DemoFileId] ON [WeaponFires] ([DemoFileId]);

CREATE INDEX [IX_WeaponFires_PlayerId] ON [WeaponFires] ([PlayerId]);

CREATE INDEX [IX_WeaponFires_RoundId_Tick] ON [WeaponFires] ([RoundId], [Tick]);

CREATE INDEX [IX_WeaponStates_DemoFileId] ON [WeaponStates] ([DemoFileId]);

CREATE INDEX [IX_WeaponStates_PlayerId_Tick] ON [WeaponStates] ([PlayerId], [Tick]);

CREATE INDEX [IX_WeaponStates_RoundId_EventType] ON [WeaponStates] ([RoundId], [EventType]);

CREATE INDEX [IX_ZoneEvents_DemoFileId] ON [ZoneEvents] ([DemoFileId]);

CREATE INDEX [IX_ZoneEvents_PlayerId_Tick] ON [ZoneEvents] ([PlayerId], [Tick]);

CREATE INDEX [IX_ZoneEvents_RoundId_ZoneType] ON [ZoneEvents] ([RoundId], [ZoneType]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250629134818_InitialSqlServerFinal', N'9.0.6');

ALTER TABLE [DemoFiles] ADD [DemoSource] nvarchar(50) NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250630224256_AddDemoSource', N'9.0.6');

COMMIT;
GO

