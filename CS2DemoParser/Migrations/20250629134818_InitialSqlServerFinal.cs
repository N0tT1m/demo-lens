using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CS2DemoParser.Migrations
{
    /// <inheritdoc />
    public partial class InitialSqlServerFinal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DemoFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ParsedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MapName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    GameMode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TickRate = table.Column<int>(type: "int", nullable: false),
                    TotalTicks = table.Column<int>(type: "int", nullable: false),
                    Duration = table.Column<float>(type: "real", nullable: false),
                    ServerName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DemoType = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    NetworkProtocol = table.Column<int>(type: "int", nullable: false),
                    ClientName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SignonState = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DemoFiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Matches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DemoFileId = table.Column<int>(type: "int", nullable: false),
                    MapName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    GameMode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TotalRounds = table.Column<int>(type: "int", nullable: false),
                    CTScore = table.Column<int>(type: "int", nullable: false),
                    TScore = table.Column<int>(type: "int", nullable: false),
                    CTScoreFirstHalf = table.Column<int>(type: "int", nullable: false),
                    TScoreFirstHalf = table.Column<int>(type: "int", nullable: false),
                    CTScoreSecondHalf = table.Column<int>(type: "int", nullable: false),
                    TScoreSecondHalf = table.Column<int>(type: "int", nullable: false),
                    CTScoreOvertime = table.Column<int>(type: "int", nullable: true),
                    TScoreOvertime = table.Column<int>(type: "int", nullable: true),
                    IsOvertime = table.Column<bool>(type: "bit", nullable: false),
                    IsFinished = table.Column<bool>(type: "bit", nullable: false),
                    WinnerTeam = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    WinCondition = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MaxRounds = table.Column<int>(type: "int", nullable: false),
                    RoundTimeLimit = table.Column<float>(type: "real", nullable: false),
                    FreezeTime = table.Column<float>(type: "real", nullable: false),
                    BuyTime = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Matches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Matches_DemoFiles_DemoFileId",
                        column: x => x.DemoFileId,
                        principalTable: "DemoFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Players",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DemoFileId = table.Column<int>(type: "int", nullable: false),
                    PlayerSlot = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    SteamId = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    PlayerName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Team = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsBot = table.Column<bool>(type: "bit", nullable: false),
                    IsHltv = table.Column<bool>(type: "bit", nullable: false),
                    IsConnected = table.Column<bool>(type: "bit", nullable: false),
                    Rank = table.Column<int>(type: "int", nullable: false),
                    Wins = table.Column<int>(type: "int", nullable: false),
                    ClanTag = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ConnectedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DisconnectedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DisconnectReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PingAverage = table.Column<int>(type: "int", nullable: false),
                    PingMax = table.Column<int>(type: "int", nullable: false),
                    PingMin = table.Column<int>(type: "int", nullable: false),
                    PacketLoss = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Players_DemoFiles_DemoFileId",
                        column: x => x.DemoFileId,
                        principalTable: "DemoFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Rounds",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DemoFileId = table.Column<int>(type: "int", nullable: false),
                    MatchId = table.Column<int>(type: "int", nullable: false),
                    RoundNumber = table.Column<int>(type: "int", nullable: false),
                    StartTick = table.Column<int>(type: "int", nullable: false),
                    EndTick = table.Column<int>(type: "int", nullable: true),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Duration = table.Column<float>(type: "real", nullable: false),
                    WinnerTeam = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EndReason = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CTScore = table.Column<int>(type: "int", nullable: false),
                    TScore = table.Column<int>(type: "int", nullable: false),
                    CTLivePlayers = table.Column<int>(type: "int", nullable: false),
                    TLivePlayers = table.Column<int>(type: "int", nullable: false),
                    CTStartMoney = table.Column<int>(type: "int", nullable: false),
                    TStartMoney = table.Column<int>(type: "int", nullable: false),
                    CTEquipmentValue = table.Column<int>(type: "int", nullable: false),
                    TEquipmentValue = table.Column<int>(type: "int", nullable: false),
                    BombPlanted = table.Column<bool>(type: "bit", nullable: false),
                    BombDefused = table.Column<bool>(type: "bit", nullable: false),
                    BombExploded = table.Column<bool>(type: "bit", nullable: false),
                    BombSite = table.Column<int>(type: "int", nullable: true),
                    IsEcoRound = table.Column<bool>(type: "bit", nullable: false),
                    IsForceBuyRound = table.Column<bool>(type: "bit", nullable: false),
                    IsAntiEcoRound = table.Column<bool>(type: "bit", nullable: false),
                    IsPistolRound = table.Column<bool>(type: "bit", nullable: false),
                    IsOvertime = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rounds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Rounds_DemoFiles_DemoFileId",
                        column: x => x.DemoFileId,
                        principalTable: "DemoFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Rounds_Matches_MatchId",
                        column: x => x.MatchId,
                        principalTable: "Matches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChatMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DemoFileId = table.Column<int>(type: "int", nullable: false),
                    PlayerId = table.Column<int>(type: "int", nullable: true),
                    Tick = table.Column<int>(type: "int", nullable: false),
                    GameTime = table.Column<float>(type: "real", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    SenderName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Team = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsTeamMessage = table.Column<bool>(type: "bit", nullable: false),
                    IsAllChat = table.Column<bool>(type: "bit", nullable: false),
                    IsDeadChat = table.Column<bool>(type: "bit", nullable: false),
                    IsSystemMessage = table.Column<bool>(type: "bit", nullable: false),
                    IsRadioMessage = table.Column<bool>(type: "bit", nullable: false),
                    MessageType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RadioCommand = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsMuted = table.Column<bool>(type: "bit", nullable: false),
                    IsSpam = table.Column<bool>(type: "bit", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatMessages_DemoFiles_DemoFileId",
                        column: x => x.DemoFileId,
                        principalTable: "DemoFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChatMessages_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Equipment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DemoFileId = table.Column<int>(type: "int", nullable: false),
                    PlayerId = table.Column<int>(type: "int", nullable: false),
                    Tick = table.Column<int>(type: "int", nullable: false),
                    GameTime = table.Column<float>(type: "real", nullable: false),
                    ItemName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ItemType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ItemCategory = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Action = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Cost = table.Column<int>(type: "int", nullable: false),
                    Ammo = table.Column<int>(type: "int", nullable: false),
                    AmmoReserve = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDropped = table.Column<bool>(type: "bit", nullable: false),
                    IsPurchased = table.Column<bool>(type: "bit", nullable: false),
                    IsPickedUp = table.Column<bool>(type: "bit", nullable: false),
                    PositionX = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PositionY = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PositionZ = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Team = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RoundNumber = table.Column<int>(type: "int", nullable: false),
                    Quality = table.Column<int>(type: "int", nullable: false),
                    Wear = table.Column<int>(type: "int", nullable: false),
                    SkinName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    StatTrak = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsStattrak = table.Column<bool>(type: "bit", nullable: false),
                    IsSouvenir = table.Column<bool>(type: "bit", nullable: false),
                    FloatValue = table.Column<float>(type: "real", nullable: false),
                    PaintSeed = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    InventoryId = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    AccountId = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    Origin = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    Stickers = table.Column<int>(type: "int", nullable: false),
                    StickerInfo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsNameTag = table.Column<bool>(type: "bit", nullable: false),
                    CustomName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Equipment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Equipment_DemoFiles_DemoFileId",
                        column: x => x.DemoFileId,
                        principalTable: "DemoFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Equipment_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GameEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DemoFileId = table.Column<int>(type: "int", nullable: false),
                    Tick = table.Column<int>(type: "int", nullable: false),
                    GameTime = table.Column<float>(type: "real", nullable: false),
                    EventName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EventData = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    PlayerId = table.Column<int>(type: "int", nullable: true),
                    PlayerName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Team = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RoundNumber = table.Column<int>(type: "int", nullable: true),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsImportant = table.Column<bool>(type: "bit", nullable: false),
                    PositionX = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PositionY = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PositionZ = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    SubCategory = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Value1 = table.Column<int>(type: "int", nullable: true),
                    Value2 = table.Column<int>(type: "int", nullable: true),
                    Value3 = table.Column<int>(type: "int", nullable: true),
                    StringValue1 = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    StringValue2 = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    StringValue3 = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FloatValue1 = table.Column<float>(type: "real", nullable: true),
                    FloatValue2 = table.Column<float>(type: "real", nullable: true),
                    FloatValue3 = table.Column<float>(type: "real", nullable: true),
                    BoolValue1 = table.Column<bool>(type: "bit", nullable: true),
                    BoolValue2 = table.Column<bool>(type: "bit", nullable: true),
                    BoolValue3 = table.Column<bool>(type: "bit", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameEvents_DemoFiles_DemoFileId",
                        column: x => x.DemoFileId,
                        principalTable: "DemoFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GameEvents_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlayerMatchStats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlayerId = table.Column<int>(type: "int", nullable: false),
                    MatchId = table.Column<int>(type: "int", nullable: false),
                    Kills = table.Column<int>(type: "int", nullable: false),
                    Deaths = table.Column<int>(type: "int", nullable: false),
                    Assists = table.Column<int>(type: "int", nullable: false),
                    Score = table.Column<int>(type: "int", nullable: false),
                    MVPs = table.Column<int>(type: "int", nullable: false),
                    HeadshotKills = table.Column<int>(type: "int", nullable: false),
                    HeadshotPercentage = table.Column<float>(type: "real", nullable: false),
                    TotalDamageDealt = table.Column<int>(type: "int", nullable: false),
                    TotalDamageReceived = table.Column<int>(type: "int", nullable: false),
                    ADR = table.Column<float>(type: "real", nullable: false),
                    FirstKills = table.Column<int>(type: "int", nullable: false),
                    FirstDeaths = table.Column<int>(type: "int", nullable: false),
                    TradeKills = table.Column<int>(type: "int", nullable: false),
                    TradeDeaths = table.Column<int>(type: "int", nullable: false),
                    SurvivalTime = table.Column<int>(type: "int", nullable: false),
                    KDRatio = table.Column<float>(type: "real", nullable: false),
                    Rating = table.Column<float>(type: "real", nullable: false),
                    HLTV2Rating = table.Column<float>(type: "real", nullable: false),
                    RoundsPlayed = table.Column<int>(type: "int", nullable: false),
                    RoundsWon = table.Column<int>(type: "int", nullable: false),
                    ClutchWins1v1 = table.Column<int>(type: "int", nullable: false),
                    ClutchWins1v2 = table.Column<int>(type: "int", nullable: false),
                    ClutchWins1v3 = table.Column<int>(type: "int", nullable: false),
                    ClutchWins1v4 = table.Column<int>(type: "int", nullable: false),
                    ClutchWins1v5 = table.Column<int>(type: "int", nullable: false),
                    ClutchAttempts1v1 = table.Column<int>(type: "int", nullable: false),
                    ClutchAttempts1v2 = table.Column<int>(type: "int", nullable: false),
                    ClutchAttempts1v3 = table.Column<int>(type: "int", nullable: false),
                    ClutchAttempts1v4 = table.Column<int>(type: "int", nullable: false),
                    ClutchAttempts1v5 = table.Column<int>(type: "int", nullable: false),
                    FlashAssists = table.Column<int>(type: "int", nullable: false),
                    UtilityDamage = table.Column<int>(type: "int", nullable: false),
                    EnemiesFlashed = table.Column<int>(type: "int", nullable: false),
                    TeammatesFlashed = table.Column<int>(type: "int", nullable: false),
                    FlashDuration = table.Column<float>(type: "real", nullable: false),
                    BombPlants = table.Column<int>(type: "int", nullable: false),
                    BombDefuses = table.Column<int>(type: "int", nullable: false),
                    HostageRescues = table.Column<int>(type: "int", nullable: false),
                    MoneySpent = table.Column<int>(type: "int", nullable: false),
                    MoneyEarned = table.Column<int>(type: "int", nullable: false),
                    ShotsHit = table.Column<int>(type: "int", nullable: false),
                    ShotsFired = table.Column<int>(type: "int", nullable: false),
                    Accuracy = table.Column<float>(type: "real", nullable: false),
                    WallbangKills = table.Column<int>(type: "int", nullable: false),
                    CollateralKills = table.Column<int>(type: "int", nullable: false),
                    NoScopeKills = table.Column<int>(type: "int", nullable: false),
                    BlindKills = table.Column<int>(type: "int", nullable: false),
                    SmokeKills = table.Column<int>(type: "int", nullable: false),
                    KASTPercentage = table.Column<float>(type: "real", nullable: false),
                    MultiKillRounds2K = table.Column<int>(type: "int", nullable: false),
                    MultiKillRounds3K = table.Column<int>(type: "int", nullable: false),
                    MultiKillRounds4K = table.Column<int>(type: "int", nullable: false),
                    MultiKillRounds5K = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerMatchStats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerMatchStats_Matches_MatchId",
                        column: x => x.MatchId,
                        principalTable: "Matches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlayerMatchStats_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlayerPositions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DemoFileId = table.Column<int>(type: "int", nullable: false),
                    PlayerId = table.Column<int>(type: "int", nullable: false),
                    Tick = table.Column<int>(type: "int", nullable: false),
                    GameTime = table.Column<float>(type: "real", nullable: false),
                    PositionX = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PositionY = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PositionZ = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ViewAngleX = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ViewAngleY = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ViewAngleZ = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    VelocityX = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    VelocityY = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    VelocityZ = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Speed = table.Column<float>(type: "real", nullable: false),
                    IsAlive = table.Column<bool>(type: "bit", nullable: false),
                    Health = table.Column<int>(type: "int", nullable: false),
                    Armor = table.Column<int>(type: "int", nullable: false),
                    HasHelmet = table.Column<bool>(type: "bit", nullable: false),
                    HasDefuseKit = table.Column<bool>(type: "bit", nullable: false),
                    IsScoped = table.Column<bool>(type: "bit", nullable: false),
                    IsWalking = table.Column<bool>(type: "bit", nullable: false),
                    IsCrouching = table.Column<bool>(type: "bit", nullable: false),
                    IsReloading = table.Column<bool>(type: "bit", nullable: false),
                    IsDefusing = table.Column<bool>(type: "bit", nullable: false),
                    IsPlanting = table.Column<bool>(type: "bit", nullable: false),
                    ActiveWeapon = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Team = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Money = table.Column<int>(type: "int", nullable: false),
                    FlashDuration = table.Column<int>(type: "int", nullable: false),
                    IsBlind = table.Column<bool>(type: "bit", nullable: false),
                    InSmoke = table.Column<bool>(type: "bit", nullable: false),
                    OnLadder = table.Column<bool>(type: "bit", nullable: false),
                    InAir = table.Column<bool>(type: "bit", nullable: false),
                    IsDucking = table.Column<bool>(type: "bit", nullable: false),
                    StaminaPercentage = table.Column<float>(type: "real", nullable: false),
                    LookDistance = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerPositions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerPositions_DemoFiles_DemoFileId",
                        column: x => x.DemoFileId,
                        principalTable: "DemoFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlayerPositions_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AdvancedPlayerStats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DemoFileId = table.Column<int>(type: "int", nullable: false),
                    PlayerId = table.Column<int>(type: "int", nullable: false),
                    MatchId = table.Column<int>(type: "int", nullable: true),
                    RoundId = table.Column<int>(type: "int", nullable: true),
                    RoundNumber = table.Column<int>(type: "int", nullable: false),
                    StatsType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    HLTVRating = table.Column<float>(type: "real", nullable: false),
                    HLTVRating1 = table.Column<float>(type: "real", nullable: false),
                    ImpactRating = table.Column<float>(type: "real", nullable: false),
                    KASTPercentage = table.Column<float>(type: "real", nullable: false),
                    KillsPerRound = table.Column<float>(type: "real", nullable: false),
                    DeathsPerRound = table.Column<float>(type: "real", nullable: false),
                    AssistsPerRound = table.Column<float>(type: "real", nullable: false),
                    KillDeathRatio = table.Column<float>(type: "real", nullable: false),
                    KillAssistSurviveTradeRatio = table.Column<float>(type: "real", nullable: false),
                    AverageDamagePerRound = table.Column<float>(type: "real", nullable: false),
                    DamagePerRound = table.Column<float>(type: "real", nullable: false),
                    EffectiveDamagePerRound = table.Column<float>(type: "real", nullable: false),
                    WastedDamage = table.Column<float>(type: "real", nullable: false),
                    DamageEfficiency = table.Column<float>(type: "real", nullable: false),
                    FirstKillsPerRound = table.Column<float>(type: "real", nullable: false),
                    FirstDeathsPerRound = table.Column<float>(type: "real", nullable: false),
                    FirstKillRatio = table.Column<float>(type: "real", nullable: false),
                    OpeningDuelSuccessRate = table.Column<float>(type: "real", nullable: false),
                    Clutch1v1Attempts = table.Column<int>(type: "int", nullable: false),
                    Clutch1v1Wins = table.Column<int>(type: "int", nullable: false),
                    Clutch1v2Attempts = table.Column<int>(type: "int", nullable: false),
                    Clutch1v2Wins = table.Column<int>(type: "int", nullable: false),
                    Clutch1v3Attempts = table.Column<int>(type: "int", nullable: false),
                    Clutch1v3Wins = table.Column<int>(type: "int", nullable: false),
                    Clutch1v4Attempts = table.Column<int>(type: "int", nullable: false),
                    Clutch1v4Wins = table.Column<int>(type: "int", nullable: false),
                    Clutch1v5Attempts = table.Column<int>(type: "int", nullable: false),
                    Clutch1v5Wins = table.Column<int>(type: "int", nullable: false),
                    OverallClutchSuccessRate = table.Column<float>(type: "real", nullable: false),
                    DoubleKills = table.Column<int>(type: "int", nullable: false),
                    TripleKills = table.Column<int>(type: "int", nullable: false),
                    QuadKills = table.Column<int>(type: "int", nullable: false),
                    PentaKills = table.Column<int>(type: "int", nullable: false),
                    MultiKillsPerRound = table.Column<float>(type: "real", nullable: false),
                    SurvivalRate = table.Column<float>(type: "real", nullable: false),
                    TradeKillPercentage = table.Column<float>(type: "real", nullable: false),
                    TradeFragPercentage = table.Column<float>(type: "real", nullable: false),
                    SupportRoundPercentage = table.Column<float>(type: "real", nullable: false),
                    HeadshotPercentage = table.Column<float>(type: "real", nullable: false),
                    RifleKillsPercentage = table.Column<float>(type: "real", nullable: false),
                    PistolKillsPercentage = table.Column<float>(type: "real", nullable: false),
                    SniperKillsPercentage = table.Column<float>(type: "real", nullable: false),
                    AwpKillsPerRound = table.Column<float>(type: "real", nullable: false),
                    ShotAccuracy = table.Column<float>(type: "real", nullable: false),
                    KillsPerShot = table.Column<float>(type: "real", nullable: false),
                    UtilityDamagePerRound = table.Column<float>(type: "real", nullable: false),
                    FlashAssistsPerRound = table.Column<float>(type: "real", nullable: false),
                    UtilitySuccessRate = table.Column<float>(type: "real", nullable: false),
                    EnemiesFlashedPerRound = table.Column<float>(type: "real", nullable: false),
                    TeamFlashesPerRound = table.Column<float>(type: "real", nullable: false),
                    EconomicImpact = table.Column<float>(type: "real", nullable: false),
                    SavedRoundsImpact = table.Column<float>(type: "real", nullable: false),
                    ForceRoundsImpact = table.Column<float>(type: "real", nullable: false),
                    EcoRoundsImpact = table.Column<float>(type: "real", nullable: false),
                    EntryFragPercentage = table.Column<float>(type: "real", nullable: false),
                    LurkKillsPercentage = table.Column<float>(type: "real", nullable: false),
                    RotationTimingScore = table.Column<float>(type: "real", nullable: false),
                    DecisionMakingScore = table.Column<float>(type: "real", nullable: false),
                    RoundsWithKill = table.Column<float>(type: "real", nullable: false),
                    RoundsWithMultiKill = table.Column<float>(type: "real", nullable: false),
                    RoundsWithZeroKills = table.Column<float>(type: "real", nullable: false),
                    HighImpactRounds = table.Column<float>(type: "real", nullable: false),
                    TeamPlayScore = table.Column<float>(type: "real", nullable: false),
                    CommunicationScore = table.Column<float>(type: "real", nullable: false),
                    LeadershipScore = table.Column<float>(type: "real", nullable: false),
                    PerformanceVariance = table.Column<float>(type: "real", nullable: false),
                    ClutchConsistency = table.Column<float>(type: "real", nullable: false),
                    EconomyAdaptability = table.Column<float>(type: "real", nullable: false),
                    CalculatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SampleSize = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdvancedPlayerStats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdvancedPlayerStats_DemoFiles_DemoFileId",
                        column: x => x.DemoFileId,
                        principalTable: "DemoFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AdvancedPlayerStats_Matches_MatchId",
                        column: x => x.MatchId,
                        principalTable: "Matches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AdvancedPlayerStats_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AdvancedPlayerStats_Rounds_RoundId",
                        column: x => x.RoundId,
                        principalTable: "Rounds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AdvancedUserMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DemoFileId = table.Column<int>(type: "int", nullable: false),
                    RoundId = table.Column<int>(type: "int", nullable: true),
                    PlayerId = table.Column<int>(type: "int", nullable: true),
                    Tick = table.Column<int>(type: "int", nullable: false),
                    GameTime = table.Column<float>(type: "real", nullable: false),
                    MessageType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MessageSubType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MessageContent = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    MessageData = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DamageGiven = table.Column<int>(type: "int", nullable: true),
                    DamageTaken = table.Column<int>(type: "int", nullable: true),
                    HitsGiven = table.Column<int>(type: "int", nullable: true),
                    HitsTaken = table.Column<int>(type: "int", nullable: true),
                    TargetPlayerId = table.Column<int>(type: "int", nullable: true),
                    XpGained = table.Column<int>(type: "int", nullable: true),
                    XpTotal = table.Column<int>(type: "int", nullable: true),
                    QuestProgress = table.Column<int>(type: "int", nullable: true),
                    QuestGoal = table.Column<int>(type: "int", nullable: true),
                    QuestName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    VoteTarget = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    VoteType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    VoteReason = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    VotesRequired = table.Column<int>(type: "int", nullable: true),
                    VotesFor = table.Column<int>(type: "int", nullable: true),
                    VotesAgainst = table.Column<int>(type: "int", nullable: true),
                    VotePassed = table.Column<bool>(type: "bit", nullable: true),
                    StatCategory = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    StatValue = table.Column<float>(type: "real", nullable: true),
                    StatPercentile = table.Column<float>(type: "real", nullable: true),
                    StatComparison = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LeaderboardRank = table.Column<int>(type: "int", nullable: true),
                    LeaderboardScore = table.Column<int>(type: "int", nullable: true),
                    LeaderboardType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MoneyChange = table.Column<int>(type: "int", nullable: true),
                    MoneyTotal = table.Column<int>(type: "int", nullable: true),
                    MoneyReason = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DisplayDuration = table.Column<float>(type: "real", nullable: true),
                    DisplayLocation = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsImportant = table.Column<bool>(type: "bit", nullable: false),
                    IsServerMessage = table.Column<bool>(type: "bit", nullable: false),
                    Team = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RoundNumber = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdvancedUserMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdvancedUserMessages_DemoFiles_DemoFileId",
                        column: x => x.DemoFileId,
                        principalTable: "DemoFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AdvancedUserMessages_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AdvancedUserMessages_Players_TargetPlayerId",
                        column: x => x.TargetPlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AdvancedUserMessages_Rounds_RoundId",
                        column: x => x.RoundId,
                        principalTable: "Rounds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Bombs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DemoFileId = table.Column<int>(type: "int", nullable: false),
                    RoundId = table.Column<int>(type: "int", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Tick = table.Column<int>(type: "int", nullable: false),
                    GameTime = table.Column<float>(type: "real", nullable: false),
                    PlayerId = table.Column<int>(type: "int", nullable: true),
                    Site = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    PositionX = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PositionY = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PositionZ = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PlantTime = table.Column<float>(type: "real", nullable: true),
                    DefuseTime = table.Column<float>(type: "real", nullable: true),
                    ExplodeTime = table.Column<float>(type: "real", nullable: true),
                    HasKit = table.Column<bool>(type: "bit", nullable: false),
                    IsDefuseStarted = table.Column<bool>(type: "bit", nullable: false),
                    IsDefuseCancelled = table.Column<bool>(type: "bit", nullable: false),
                    IsPlantStarted = table.Column<bool>(type: "bit", nullable: false),
                    IsPlantCancelled = table.Column<bool>(type: "bit", nullable: false),
                    CTPlayersInRange = table.Column<int>(type: "int", nullable: false),
                    TPlayersInRange = table.Column<int>(type: "int", nullable: false),
                    DefuseProgress = table.Column<float>(type: "real", nullable: false),
                    PlantProgress = table.Column<float>(type: "real", nullable: false),
                    TimeRemaining = table.Column<float>(type: "real", nullable: false),
                    Team = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsClutch = table.Column<bool>(type: "bit", nullable: false),
                    ClutchSize = table.Column<int>(type: "int", nullable: false),
                    HasSmoke = table.Column<bool>(type: "bit", nullable: false),
                    HasFlash = table.Column<bool>(type: "bit", nullable: false),
                    UnderFire = table.Column<bool>(type: "bit", nullable: false),
                    NearestEnemyDistance = table.Column<float>(type: "real", nullable: false),
                    RoundNumber = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bombs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bombs_DemoFiles_DemoFileId",
                        column: x => x.DemoFileId,
                        principalTable: "DemoFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Bombs_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Bombs_Rounds_RoundId",
                        column: x => x.RoundId,
                        principalTable: "Rounds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BulletImpacts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DemoFileId = table.Column<int>(type: "int", nullable: false),
                    RoundId = table.Column<int>(type: "int", nullable: true),
                    PlayerId = table.Column<int>(type: "int", nullable: false),
                    Tick = table.Column<int>(type: "int", nullable: false),
                    GameTime = table.Column<float>(type: "real", nullable: false),
                    Weapon = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ShooterPositionX = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    ShooterPositionY = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    ShooterPositionZ = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    ImpactPositionX = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    ImpactPositionY = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    ImpactPositionZ = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    ShootAngleX = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    ShootAngleY = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    Distance = table.Column<float>(type: "real", nullable: false),
                    Team = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsScoped = table.Column<bool>(type: "bit", nullable: false),
                    IsMoving = table.Column<bool>(type: "bit", nullable: false),
                    IsCrouching = table.Column<bool>(type: "bit", nullable: false),
                    PenetrationCount = table.Column<int>(type: "int", nullable: false),
                    SurfaceType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    HitPlayer = table.Column<bool>(type: "bit", nullable: false),
                    HitPlayerId = table.Column<int>(type: "int", nullable: true),
                    HitGroup = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DamageDealt = table.Column<float>(type: "real", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BulletImpacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BulletImpacts_DemoFiles_DemoFileId",
                        column: x => x.DemoFileId,
                        principalTable: "DemoFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BulletImpacts_Players_HitPlayerId",
                        column: x => x.HitPlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BulletImpacts_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BulletImpacts_Rounds_RoundId",
                        column: x => x.RoundId,
                        principalTable: "Rounds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CommunicationPatterns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DemoFileId = table.Column<int>(type: "int", nullable: false),
                    RoundId = table.Column<int>(type: "int", nullable: true),
                    PatternType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Team = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StartTick = table.Column<int>(type: "int", nullable: false),
                    EndTick = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<float>(type: "real", nullable: false),
                    EndTime = table.Column<float>(type: "real", nullable: false),
                    Duration = table.Column<float>(type: "real", nullable: false),
                    RoundNumber = table.Column<int>(type: "int", nullable: false),
                    ParticipantCount = table.Column<int>(type: "int", nullable: false),
                    ParticipantIds = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PrimaryLeaderId = table.Column<int>(type: "int", nullable: true),
                    CommunicationDensity = table.Column<float>(type: "real", nullable: false),
                    ResponseRate = table.Column<float>(type: "real", nullable: false),
                    OverlapPercentage = table.Column<float>(type: "real", nullable: false),
                    CoordinationQuality = table.Column<float>(type: "real", nullable: false),
                    InformationQuality = table.Column<float>(type: "real", nullable: false),
                    LeadershipClarity = table.Column<float>(type: "real", nullable: false),
                    TeamResponsiveness = table.Column<float>(type: "real", nullable: false),
                    IsExecutePattern = table.Column<bool>(type: "bit", nullable: false),
                    IsRetakePattern = table.Column<bool>(type: "bit", nullable: false),
                    IsRotationPattern = table.Column<bool>(type: "bit", nullable: false),
                    IsInformationChain = table.Column<bool>(type: "bit", nullable: false),
                    IsLeadershipSequence = table.Column<bool>(type: "bit", nullable: false),
                    AchievedObjective = table.Column<bool>(type: "bit", nullable: false),
                    ImprovedCoordination = table.Column<bool>(type: "bit", nullable: false),
                    CausedConfusion = table.Column<bool>(type: "bit", nullable: false),
                    WastedTime = table.Column<bool>(type: "bit", nullable: false),
                    EffectivenessScore = table.Column<float>(type: "real", nullable: false),
                    ImpactOnRound = table.Column<float>(type: "real", nullable: false),
                    PrimaryTopic = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SecondaryTopic = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CalloutCount = table.Column<int>(type: "int", nullable: false),
                    OrderCount = table.Column<int>(type: "int", nullable: false),
                    QuestionCount = table.Column<int>(type: "int", nullable: false),
                    ResponseCount = table.Column<int>(type: "int", nullable: false),
                    ConfirmationCount = table.Column<int>(type: "int", nullable: false),
                    OptimalTiming = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TimingScore = table.Column<float>(type: "real", nullable: false),
                    IsInnovativePattern = table.Column<bool>(type: "bit", nullable: false),
                    IsAdaptiveResponse = table.Column<bool>(type: "bit", nullable: false),
                    IsStandardProtocol = table.Column<bool>(type: "bit", nullable: false),
                    PatternDescription = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    AdditionalAnalysis = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommunicationPatterns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommunicationPatterns_DemoFiles_DemoFileId",
                        column: x => x.DemoFileId,
                        principalTable: "DemoFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CommunicationPatterns_Players_PrimaryLeaderId",
                        column: x => x.PrimaryLeaderId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CommunicationPatterns_Rounds_RoundId",
                        column: x => x.RoundId,
                        principalTable: "Rounds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Damages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DemoFileId = table.Column<int>(type: "int", nullable: false),
                    RoundId = table.Column<int>(type: "int", nullable: false),
                    Tick = table.Column<int>(type: "int", nullable: false),
                    GameTime = table.Column<float>(type: "real", nullable: false),
                    AttackerId = table.Column<int>(type: "int", nullable: true),
                    VictimId = table.Column<int>(type: "int", nullable: false),
                    Weapon = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    WeaponClass = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    HitGroup = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DamageAmount = table.Column<int>(type: "int", nullable: false),
                    DamageArmor = table.Column<int>(type: "int", nullable: false),
                    Health = table.Column<int>(type: "int", nullable: false),
                    Armor = table.Column<int>(type: "int", nullable: false),
                    IsHeadshot = table.Column<bool>(type: "bit", nullable: false),
                    IsWallbang = table.Column<bool>(type: "bit", nullable: false),
                    IsFatal = table.Column<bool>(type: "bit", nullable: false),
                    Distance = table.Column<float>(type: "real", nullable: false),
                    Penetration = table.Column<int>(type: "int", nullable: false),
                    AttackerPositionX = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AttackerPositionY = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AttackerPositionZ = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    VictimPositionX = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    VictimPositionY = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    VictimPositionZ = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AttackerViewAngleX = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AttackerViewAngleY = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    VictimViewAngleX = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    VictimViewAngleY = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AttackerTeam = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    VictimTeam = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsTeamDamage = table.Column<bool>(type: "bit", nullable: false),
                    ThroughSmoke = table.Column<bool>(type: "bit", nullable: false),
                    AttackerBlind = table.Column<bool>(type: "bit", nullable: false),
                    VictimBlind = table.Column<bool>(type: "bit", nullable: false),
                    FlashDuration = table.Column<int>(type: "int", nullable: false),
                    IsNoScope = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Damages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Damages_DemoFiles_DemoFileId",
                        column: x => x.DemoFileId,
                        principalTable: "DemoFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Damages_Players_AttackerId",
                        column: x => x.AttackerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Damages_Players_VictimId",
                        column: x => x.VictimId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Damages_Rounds_RoundId",
                        column: x => x.RoundId,
                        principalTable: "Rounds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DroppedItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DemoFileId = table.Column<int>(type: "int", nullable: false),
                    RoundId = table.Column<int>(type: "int", nullable: true),
                    DropperPlayerId = table.Column<int>(type: "int", nullable: true),
                    PickerPlayerId = table.Column<int>(type: "int", nullable: true),
                    ItemType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ItemName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    DropTick = table.Column<int>(type: "int", nullable: false),
                    DropTime = table.Column<float>(type: "real", nullable: false),
                    DropPositionX = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    DropPositionY = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    DropPositionZ = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    DropVelocityX = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    DropVelocityY = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    DropVelocityZ = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    DropReason = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PickupTick = table.Column<int>(type: "int", nullable: true),
                    PickupTime = table.Column<float>(type: "real", nullable: true),
                    PickupPositionX = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                    PickupPositionY = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                    PickupPositionZ = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                    TimeOnGround = table.Column<float>(type: "real", nullable: true),
                    WasPickedUp = table.Column<bool>(type: "bit", nullable: false),
                    Expired = table.Column<bool>(type: "bit", nullable: false),
                    AmmoClip = table.Column<int>(type: "int", nullable: false),
                    AmmoReserve = table.Column<int>(type: "int", nullable: false),
                    Durability = table.Column<float>(type: "real", nullable: false),
                    Value = table.Column<int>(type: "int", nullable: false),
                    Quality = table.Column<int>(type: "int", nullable: false),
                    FloatValue = table.Column<float>(type: "real", nullable: false),
                    SkinName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsStattrak = table.Column<bool>(type: "bit", nullable: false),
                    DropperTeam = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PickerTeam = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RoundNumber = table.Column<int>(type: "int", nullable: false),
                    Properties = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DroppedItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DroppedItems_DemoFiles_DemoFileId",
                        column: x => x.DemoFileId,
                        principalTable: "DemoFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DroppedItems_Players_DropperPlayerId",
                        column: x => x.DropperPlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DroppedItems_Players_PickerPlayerId",
                        column: x => x.PickerPlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DroppedItems_Rounds_RoundId",
                        column: x => x.RoundId,
                        principalTable: "Rounds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EconomyEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DemoFileId = table.Column<int>(type: "int", nullable: false),
                    RoundId = table.Column<int>(type: "int", nullable: true),
                    PlayerId = table.Column<int>(type: "int", nullable: false),
                    Tick = table.Column<int>(type: "int", nullable: false),
                    GameTime = table.Column<float>(type: "real", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ItemName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ItemCategory = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ItemCost = table.Column<int>(type: "int", nullable: true),
                    MoneyBefore = table.Column<int>(type: "int", nullable: false),
                    MoneyAfter = table.Column<int>(type: "int", nullable: false),
                    MoneyChange = table.Column<int>(type: "int", nullable: false),
                    ItemQuantity = table.Column<int>(type: "int", nullable: true),
                    PositionX = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    PositionY = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    PositionZ = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    Team = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RoundNumber = table.Column<int>(type: "int", nullable: false),
                    IsInBuyZone = table.Column<bool>(type: "bit", nullable: false),
                    IsBuyTimeActive = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EconomyEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EconomyEvents_DemoFiles_DemoFileId",
                        column: x => x.DemoFileId,
                        principalTable: "DemoFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EconomyEvents_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EconomyEvents_Rounds_RoundId",
                        column: x => x.RoundId,
                        principalTable: "Rounds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EconomyStates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DemoFileId = table.Column<int>(type: "int", nullable: false),
                    RoundId = table.Column<int>(type: "int", nullable: true),
                    Tick = table.Column<int>(type: "int", nullable: false),
                    GameTime = table.Column<float>(type: "real", nullable: false),
                    Team = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RoundNumber = table.Column<int>(type: "int", nullable: false),
                    Phase = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TotalMoney = table.Column<int>(type: "int", nullable: false),
                    AverageMoney = table.Column<int>(type: "int", nullable: false),
                    MaxMoney = table.Column<int>(type: "int", nullable: false),
                    MinMoney = table.Column<int>(type: "int", nullable: false),
                    MoneySpent = table.Column<int>(type: "int", nullable: false),
                    MoneyLost = table.Column<int>(type: "int", nullable: false),
                    RoundType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BuyPercentage = table.Column<float>(type: "real", nullable: false),
                    PlayersCanFullBuy = table.Column<int>(type: "int", nullable: false),
                    PlayersOnEco = table.Column<int>(type: "int", nullable: false),
                    TotalWeaponValue = table.Column<int>(type: "int", nullable: false),
                    TotalUtilityValue = table.Column<int>(type: "int", nullable: false),
                    TotalArmorValue = table.Column<int>(type: "int", nullable: false),
                    TotalDefuseKitValue = table.Column<int>(type: "int", nullable: false),
                    DamagePerDollar = table.Column<float>(type: "real", nullable: false),
                    KillsPerDollar = table.Column<float>(type: "real", nullable: false),
                    UtilityEfficiency = table.Column<float>(type: "real", nullable: false),
                    NextRoundMoney = table.Column<int>(type: "int", nullable: false),
                    CanFullBuyNextRound = table.Column<bool>(type: "bit", nullable: false),
                    RoundsUntilFullBuy = table.Column<int>(type: "int", nullable: false),
                    ConsecutiveLosses = table.Column<int>(type: "int", nullable: false),
                    ConsecutiveWins = table.Column<int>(type: "int", nullable: false),
                    LossBonus = table.Column<int>(type: "int", nullable: false),
                    EconomicPressure = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EconomyStates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EconomyStates_DemoFiles_DemoFileId",
                        column: x => x.DemoFileId,
                        principalTable: "DemoFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EconomyStates_Rounds_RoundId",
                        column: x => x.RoundId,
                        principalTable: "Rounds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EntityEffects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DemoFileId = table.Column<int>(type: "int", nullable: false),
                    RoundId = table.Column<int>(type: "int", nullable: true),
                    SourcePlayerId = table.Column<int>(type: "int", nullable: true),
                    SourceEntityType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SourceEntityName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EffectType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StartTick = table.Column<int>(type: "int", nullable: false),
                    EndTick = table.Column<int>(type: "int", nullable: true),
                    StartTime = table.Column<float>(type: "real", nullable: false),
                    EndTime = table.Column<float>(type: "real", nullable: true),
                    Duration = table.Column<float>(type: "real", nullable: false),
                    CenterX = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    CenterY = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    CenterZ = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    Radius = table.Column<float>(type: "real", nullable: false),
                    MaxIntensity = table.Column<float>(type: "real", nullable: false),
                    CurrentIntensity = table.Column<float>(type: "real", nullable: false),
                    PlayersAffected = table.Column<int>(type: "int", nullable: false),
                    EnemiesAffected = table.Column<int>(type: "int", nullable: false),
                    TeammatesAffected = table.Column<int>(type: "int", nullable: false),
                    TotalDamageDealt = table.Column<float>(type: "real", nullable: false),
                    MaxDamageToSinglePlayer = table.Column<float>(type: "real", nullable: false),
                    BlocksVision = table.Column<bool>(type: "bit", nullable: false),
                    CausesDamage = table.Column<bool>(type: "bit", nullable: false),
                    ImpairsMovement = table.Column<bool>(type: "bit", nullable: false),
                    Team = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RoundNumber = table.Column<int>(type: "int", nullable: false),
                    Properties = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntityEffects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EntityEffects_DemoFiles_DemoFileId",
                        column: x => x.DemoFileId,
                        principalTable: "DemoFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EntityEffects_Players_SourcePlayerId",
                        column: x => x.SourcePlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EntityEffects_Rounds_RoundId",
                        column: x => x.RoundId,
                        principalTable: "Rounds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EntityInteractions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DemoFileId = table.Column<int>(type: "int", nullable: false),
                    RoundId = table.Column<int>(type: "int", nullable: true),
                    InitiatorPlayerId = table.Column<int>(type: "int", nullable: true),
                    TargetPlayerId = table.Column<int>(type: "int", nullable: true),
                    InteractionType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SourceEntityType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SourceEntityName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TargetEntityType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TargetEntityName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Tick = table.Column<int>(type: "int", nullable: false),
                    GameTime = table.Column<float>(type: "real", nullable: false),
                    PositionX = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    PositionY = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    PositionZ = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    Distance = table.Column<float>(type: "real", nullable: true),
                    Angle = table.Column<float>(type: "real", nullable: true),
                    Force = table.Column<float>(type: "real", nullable: true),
                    Result = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IsSuccessful = table.Column<bool>(type: "bit", nullable: false),
                    Properties = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntityInteractions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EntityInteractions_DemoFiles_DemoFileId",
                        column: x => x.DemoFileId,
                        principalTable: "DemoFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EntityInteractions_Players_InitiatorPlayerId",
                        column: x => x.InitiatorPlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EntityInteractions_Players_TargetPlayerId",
                        column: x => x.TargetPlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EntityInteractions_Rounds_RoundId",
                        column: x => x.RoundId,
                        principalTable: "Rounds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EntityLifecycles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DemoFileId = table.Column<int>(type: "int", nullable: false),
                    RoundId = table.Column<int>(type: "int", nullable: true),
                    PlayerId = table.Column<int>(type: "int", nullable: true),
                    EntityType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntityName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Tick = table.Column<int>(type: "int", nullable: false),
                    GameTime = table.Column<float>(type: "real", nullable: false),
                    PositionX = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    PositionY = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    PositionZ = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    Team = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RoundNumber = table.Column<int>(type: "int", nullable: false),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Durability = table.Column<float>(type: "real", nullable: true),
                    Value = table.Column<int>(type: "int", nullable: true),
                    Properties = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntityLifecycles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EntityLifecycles_DemoFiles_DemoFileId",
                        column: x => x.DemoFileId,
                        principalTable: "DemoFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EntityLifecycles_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EntityLifecycles_Rounds_RoundId",
                        column: x => x.RoundId,
                        principalTable: "Rounds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EntityPropertyChanges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DemoFileId = table.Column<int>(type: "int", nullable: false),
                    RoundId = table.Column<int>(type: "int", nullable: true),
                    PlayerId = table.Column<int>(type: "int", nullable: true),
                    Tick = table.Column<int>(type: "int", nullable: false),
                    GameTime = table.Column<float>(type: "real", nullable: false),
                    EntityIndex = table.Column<int>(type: "int", nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PropertyName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OldValue = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    NewValue = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ChangeType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NumericOldValue = table.Column<float>(type: "real", nullable: true),
                    NumericNewValue = table.Column<float>(type: "real", nullable: true),
                    ChangeDelta = table.Column<float>(type: "real", nullable: true),
                    IsSignificantChange = table.Column<bool>(type: "bit", nullable: false),
                    IsGameplayRelevant = table.Column<bool>(type: "bit", nullable: false),
                    ChangeContext = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TriggerEvent = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CausedByPlayerId = table.Column<int>(type: "int", nullable: true),
                    Team = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RoundNumber = table.Column<int>(type: "int", nullable: false),
                    AdditionalData = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntityPropertyChanges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EntityPropertyChanges_DemoFiles_DemoFileId",
                        column: x => x.DemoFileId,
                        principalTable: "DemoFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EntityPropertyChanges_Players_CausedByPlayerId",
                        column: x => x.CausedByPlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EntityPropertyChanges_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EntityPropertyChanges_Rounds_RoundId",
                        column: x => x.RoundId,
                        principalTable: "Rounds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EntityVisibilities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DemoFileId = table.Column<int>(type: "int", nullable: false),
                    RoundId = table.Column<int>(type: "int", nullable: true),
                    ObserverPlayerId = table.Column<int>(type: "int", nullable: false),
                    TargetPlayerId = table.Column<int>(type: "int", nullable: true),
                    EntityType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntityName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Tick = table.Column<int>(type: "int", nullable: false),
                    GameTime = table.Column<float>(type: "real", nullable: false),
                    ObserverPositionX = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    ObserverPositionY = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    ObserverPositionZ = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    ObserverViewAngleX = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    ObserverViewAngleY = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    TargetPositionX = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    TargetPositionY = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    TargetPositionZ = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    IsVisible = table.Column<bool>(type: "bit", nullable: false),
                    HasLineOfSight = table.Column<bool>(type: "bit", nullable: false),
                    IsInFieldOfView = table.Column<bool>(type: "bit", nullable: false),
                    Distance = table.Column<float>(type: "real", nullable: false),
                    ViewAngle = table.Column<float>(type: "real", nullable: false),
                    ObstructionType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    VisibilityPercentage = table.Column<float>(type: "real", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntityVisibilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EntityVisibilities_DemoFiles_DemoFileId",
                        column: x => x.DemoFileId,
                        principalTable: "DemoFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EntityVisibilities_Players_ObserverPlayerId",
                        column: x => x.ObserverPlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EntityVisibilities_Players_TargetPlayerId",
                        column: x => x.TargetPlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EntityVisibilities_Rounds_RoundId",
                        column: x => x.RoundId,
                        principalTable: "Rounds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FireAreas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DemoFileId = table.Column<int>(type: "int", nullable: false),
                    RoundId = table.Column<int>(type: "int", nullable: true),
                    ThrowerPlayerId = table.Column<int>(type: "int", nullable: true),
                    GrenadeEntityId = table.Column<int>(type: "int", nullable: false),
                    GrenadeType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StartTick = table.Column<int>(type: "int", nullable: false),
                    EndTick = table.Column<int>(type: "int", nullable: true),
                    StartTime = table.Column<float>(type: "real", nullable: false),
                    EndTime = table.Column<float>(type: "real", nullable: true),
                    Duration = table.Column<float>(type: "real", nullable: false),
                    CenterX = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    CenterY = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    CenterZ = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    MaxRadius = table.Column<float>(type: "real", nullable: false),
                    CurrentRadius = table.Column<float>(type: "real", nullable: false),
                    Intensity = table.Column<float>(type: "real", nullable: false),
                    SpreadPattern = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    SpreadTime = table.Column<float>(type: "real", nullable: false),
                    PeakTime = table.Column<float>(type: "real", nullable: false),
                    BurnoutTime = table.Column<float>(type: "real", nullable: false),
                    DamagePerSecond = table.Column<float>(type: "real", nullable: false),
                    TotalDamageDealt = table.Column<float>(type: "real", nullable: false),
                    PlayersAffected = table.Column<int>(type: "int", nullable: false),
                    TeammatesAffected = table.Column<int>(type: "int", nullable: false),
                    EnemiesAffected = table.Column<int>(type: "int", nullable: false),
                    BlocksPath = table.Column<bool>(type: "bit", nullable: false),
                    ForcesCrouch = table.Column<bool>(type: "bit", nullable: false),
                    PreventsBombPlant = table.Column<bool>(type: "bit", nullable: false),
                    PreventsBombDefuse = table.Column<bool>(type: "bit", nullable: false),
                    TacticalPurpose = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ExtinguishedBySmoke = table.Column<bool>(type: "bit", nullable: false),
                    ExtinguishingGrenadeId = table.Column<int>(type: "int", nullable: true),
                    ExtinguishTime = table.Column<float>(type: "real", nullable: true),
                    Team = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RoundNumber = table.Column<int>(type: "int", nullable: false),
                    Properties = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FireAreas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FireAreas_DemoFiles_DemoFileId",
                        column: x => x.DemoFileId,
                        principalTable: "DemoFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FireAreas_Players_ThrowerPlayerId",
                        column: x => x.ThrowerPlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FireAreas_Rounds_RoundId",
                        column: x => x.RoundId,
                        principalTable: "Rounds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FlashEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DemoFileId = table.Column<int>(type: "int", nullable: false),
                    RoundId = table.Column<int>(type: "int", nullable: true),
                    FlashedPlayerId = table.Column<int>(type: "int", nullable: false),
                    FlasherPlayerId = table.Column<int>(type: "int", nullable: true),
                    Tick = table.Column<int>(type: "int", nullable: false),
                    GameTime = table.Column<float>(type: "real", nullable: false),
                    FlashDuration = table.Column<float>(type: "real", nullable: false),
                    FlashAlpha = table.Column<float>(type: "real", nullable: false),
                    FlashedPlayerPositionX = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    FlashedPlayerPositionY = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    FlashedPlayerPositionZ = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    GrenadePositionX = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                    GrenadePositionY = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                    GrenadePositionZ = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                    Distance = table.Column<float>(type: "real", nullable: true),
                    FlashedPlayerTeam = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FlasherPlayerTeam = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsTeamFlash = table.Column<bool>(type: "bit", nullable: false),
                    IsSelfFlash = table.Column<bool>(type: "bit", nullable: false),
                    RoundNumber = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlashEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FlashEvents_DemoFiles_DemoFileId",
                        column: x => x.DemoFileId,
                        principalTable: "DemoFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FlashEvents_Players_FlashedPlayerId",
                        column: x => x.FlashedPlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FlashEvents_Players_FlasherPlayerId",
                        column: x => x.FlasherPlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FlashEvents_Rounds_RoundId",
                        column: x => x.RoundId,
                        principalTable: "Rounds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Grenades",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DemoFileId = table.Column<int>(type: "int", nullable: false),
                    RoundId = table.Column<int>(type: "int", nullable: false),
                    PlayerId = table.Column<int>(type: "int", nullable: false),
                    ThrowTick = table.Column<int>(type: "int", nullable: false),
                    ThrowTime = table.Column<float>(type: "real", nullable: false),
                    DetonateTick = table.Column<int>(type: "int", nullable: true),
                    DetonateTime = table.Column<float>(type: "real", nullable: true),
                    GrenadeType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ThrowPositionX = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ThrowPositionY = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ThrowPositionZ = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DetonatePositionX = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DetonatePositionY = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DetonatePositionZ = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ThrowVelocityX = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ThrowVelocityY = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ThrowVelocityZ = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ThrowAngleX = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ThrowAngleY = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Team = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FlightTime = table.Column<float>(type: "real", nullable: false),
                    EffectRadius = table.Column<float>(type: "real", nullable: false),
                    PlayersAffected = table.Column<int>(type: "int", nullable: false),
                    EnemiesAffected = table.Column<int>(type: "int", nullable: false),
                    TeammatesAffected = table.Column<int>(type: "int", nullable: false),
                    TotalDamage = table.Column<float>(type: "real", nullable: false),
                    TotalFlashDuration = table.Column<float>(type: "real", nullable: false),
                    IsLineup = table.Column<bool>(type: "bit", nullable: false),
                    IsBounce = table.Column<bool>(type: "bit", nullable: false),
                    BounceCount = table.Column<int>(type: "int", nullable: false),
                    IsRunThrow = table.Column<bool>(type: "bit", nullable: false),
                    IsJumpThrow = table.Column<bool>(type: "bit", nullable: false),
                    IsStandingThrow = table.Column<bool>(type: "bit", nullable: false),
                    IsCrouchThrow = table.Column<bool>(type: "bit", nullable: false),
                    ThrowStyle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ThrowStrength = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Grenades", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Grenades_DemoFiles_DemoFileId",
                        column: x => x.DemoFileId,
                        principalTable: "DemoFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Grenades_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Grenades_Rounds_RoundId",
                        column: x => x.RoundId,
                        principalTable: "Rounds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GrenadeTrajectories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DemoFileId = table.Column<int>(type: "int", nullable: false),
                    RoundId = table.Column<int>(type: "int", nullable: true),
                    PlayerId = table.Column<int>(type: "int", nullable: false),
                    ThrowTick = table.Column<int>(type: "int", nullable: false),
                    ThrowTime = table.Column<float>(type: "real", nullable: false),
                    GrenadeType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ThrowPositionX = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    ThrowPositionY = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    ThrowPositionZ = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    ThrowVelocityX = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    ThrowVelocityY = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    ThrowVelocityZ = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    ThrowAngleX = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    ThrowAngleY = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    DetonateTick = table.Column<int>(type: "int", nullable: true),
                    DetonateTime = table.Column<float>(type: "real", nullable: true),
                    DetonatePositionX = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                    DetonatePositionY = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                    DetonatePositionZ = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                    FlightTime = table.Column<float>(type: "real", nullable: true),
                    BounceCount = table.Column<int>(type: "int", nullable: true),
                    Team = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsRunThrow = table.Column<bool>(type: "bit", nullable: false),
                    IsJumpThrow = table.Column<bool>(type: "bit", nullable: false),
                    IsCrouchThrow = table.Column<bool>(type: "bit", nullable: false),
                    ThrowStyle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PlayersAffected = table.Column<int>(type: "int", nullable: false),
                    EnemiesAffected = table.Column<int>(type: "int", nullable: false),
                    TeammatesAffected = table.Column<int>(type: "int", nullable: false),
                    EffectRadius = table.Column<float>(type: "real", nullable: true),
                    DamageDealt = table.Column<float>(type: "real", nullable: true),
                    FlashDuration = table.Column<float>(type: "real", nullable: true),
                    IsLineup = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrenadeTrajectories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GrenadeTrajectories_DemoFiles_DemoFileId",
                        column: x => x.DemoFileId,
                        principalTable: "DemoFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GrenadeTrajectories_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GrenadeTrajectories_Rounds_RoundId",
                        column: x => x.RoundId,
                        principalTable: "Rounds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HostageEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DemoFileId = table.Column<int>(type: "int", nullable: false),
                    RoundId = table.Column<int>(type: "int", nullable: true),
                    PlayerId = table.Column<int>(type: "int", nullable: true),
                    Tick = table.Column<int>(type: "int", nullable: false),
                    GameTime = table.Column<float>(type: "real", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    HostageEntityId = table.Column<int>(type: "int", nullable: false),
                    HostageName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PositionX = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    PositionY = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    PositionZ = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    DamageDealt = table.Column<int>(type: "int", nullable: true),
                    HealthRemaining = table.Column<int>(type: "int", nullable: true),
                    AttackerWeapon = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsHeadshot = table.Column<bool>(type: "bit", nullable: false),
                    RescueTime = table.Column<float>(type: "real", nullable: true),
                    DistanceToRescueZone = table.Column<float>(type: "real", nullable: true),
                    FollowDuration = table.Column<float>(type: "real", nullable: true),
                    FollowDistance = table.Column<float>(type: "real", nullable: true),
                    HostageState = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    WasBeingFollowed = table.Column<bool>(type: "bit", nullable: false),
                    WasBeingRescued = table.Column<bool>(type: "bit", nullable: false),
                    IsRoundWinning = table.Column<bool>(type: "bit", nullable: false),
                    IsLastHostage = table.Column<bool>(type: "bit", nullable: false),
                    Team = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RoundNumber = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HostageEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HostageEvents_DemoFiles_DemoFileId",
                        column: x => x.DemoFileId,
                        principalTable: "DemoFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HostageEvents_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HostageEvents_Rounds_RoundId",
                        column: x => x.RoundId,
                        principalTable: "Rounds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InfernoEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DemoFileId = table.Column<int>(type: "int", nullable: false),
                    RoundId = table.Column<int>(type: "int", nullable: true),
                    ThrowerPlayerId = table.Column<int>(type: "int", nullable: true),
                    StartTick = table.Column<int>(type: "int", nullable: false),
                    EndTick = table.Column<int>(type: "int", nullable: true),
                    StartTime = table.Column<float>(type: "real", nullable: false),
                    EndTime = table.Column<float>(type: "real", nullable: true),
                    Duration = table.Column<float>(type: "real", nullable: true),
                    InfernoEntityId = table.Column<int>(type: "int", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    GrenadeType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    OriginX = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    OriginY = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    OriginZ = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    SpreadRadius = table.Column<float>(type: "real", nullable: true),
                    SpreadArea = table.Column<float>(type: "real", nullable: true),
                    SpreadDirections = table.Column<int>(type: "int", nullable: true),
                    SpreadPattern = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MaxIntensity = table.Column<float>(type: "real", nullable: true),
                    AverageIntensity = table.Column<float>(type: "real", nullable: true),
                    DamageDealt = table.Column<int>(type: "int", nullable: true),
                    PlayersAffected = table.Column<int>(type: "int", nullable: true),
                    BlockedPath = table.Column<bool>(type: "bit", nullable: false),
                    ClearedPosition = table.Column<bool>(type: "bit", nullable: false),
                    WastedFire = table.Column<bool>(type: "bit", nullable: false),
                    AreaDenied = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TacticalPurpose = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SurfaceType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    HasWaterNearby = table.Column<bool>(type: "bit", nullable: false),
                    WasExtinguished = table.Column<bool>(type: "bit", nullable: false),
                    ExtinguishedByPlayerId = table.Column<int>(type: "int", nullable: true),
                    EffectivenessScore = table.Column<float>(type: "real", nullable: true),
                    PlacementQuality = table.Column<float>(type: "real", nullable: true),
                    TimingScore = table.Column<float>(type: "real", nullable: true),
                    ContributedToRoundWin = table.Column<bool>(type: "bit", nullable: false),
                    CausedRoundLoss = table.Column<bool>(type: "bit", nullable: false),
                    KillsEnabled = table.Column<int>(type: "int", nullable: true),
                    DeathsCaused = table.Column<int>(type: "int", nullable: true),
                    Team = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RoundNumber = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InfernoEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InfernoEvents_DemoFiles_DemoFileId",
                        column: x => x.DemoFileId,
                        principalTable: "DemoFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InfernoEvents_Players_ExtinguishedByPlayerId",
                        column: x => x.ExtinguishedByPlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InfernoEvents_Players_ThrowerPlayerId",
                        column: x => x.ThrowerPlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InfernoEvents_Rounds_RoundId",
                        column: x => x.RoundId,
                        principalTable: "Rounds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Kills",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DemoFileId = table.Column<int>(type: "int", nullable: false),
                    RoundId = table.Column<int>(type: "int", nullable: false),
                    Tick = table.Column<int>(type: "int", nullable: false),
                    GameTime = table.Column<float>(type: "real", nullable: false),
                    KillerId = table.Column<int>(type: "int", nullable: true),
                    VictimId = table.Column<int>(type: "int", nullable: false),
                    AssisterId = table.Column<int>(type: "int", nullable: true),
                    Weapon = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    WeaponClass = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    HitGroup = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsHeadshot = table.Column<bool>(type: "bit", nullable: false),
                    IsWallbang = table.Column<bool>(type: "bit", nullable: false),
                    IsNoScope = table.Column<bool>(type: "bit", nullable: false),
                    IsBlind = table.Column<bool>(type: "bit", nullable: false),
                    IsSmoke = table.Column<bool>(type: "bit", nullable: false),
                    IsFlash = table.Column<bool>(type: "bit", nullable: false),
                    IsCollateral = table.Column<bool>(type: "bit", nullable: false),
                    IsFirstKill = table.Column<bool>(type: "bit", nullable: false),
                    IsTradeKill = table.Column<bool>(type: "bit", nullable: false),
                    IsClutch = table.Column<bool>(type: "bit", nullable: false),
                    ClutchSize = table.Column<int>(type: "int", nullable: false),
                    Distance = table.Column<float>(type: "real", nullable: false),
                    Damage = table.Column<int>(type: "int", nullable: false),
                    Penetration = table.Column<int>(type: "int", nullable: false),
                    KillerPositionX = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    KillerPositionY = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    KillerPositionZ = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    VictimPositionX = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    VictimPositionY = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    VictimPositionZ = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    KillerViewAngleX = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    KillerViewAngleY = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    VictimViewAngleX = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    VictimViewAngleY = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    KillerHealth = table.Column<int>(type: "int", nullable: false),
                    KillerArmor = table.Column<int>(type: "int", nullable: false),
                    VictimHealth = table.Column<int>(type: "int", nullable: false),
                    VictimArmor = table.Column<int>(type: "int", nullable: false),
                    AssistType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AssistDistance = table.Column<float>(type: "real", nullable: false),
                    TimeSinceLastDamage = table.Column<float>(type: "real", nullable: false),
                    IsRevengeKill = table.Column<bool>(type: "bit", nullable: false),
                    IsDominating = table.Column<bool>(type: "bit", nullable: false),
                    IsRevenge = table.Column<bool>(type: "bit", nullable: false),
                    KillerTeam = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    VictimTeam = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsTeamKill = table.Column<bool>(type: "bit", nullable: false),
                    FlashDuration = table.Column<int>(type: "int", nullable: false),
                    ThroughSmoke = table.Column<bool>(type: "bit", nullable: false),
                    AttackerBlind = table.Column<bool>(type: "bit", nullable: false),
                    VictimBlind = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kills", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Kills_DemoFiles_DemoFileId",
                        column: x => x.DemoFileId,
                        principalTable: "DemoFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Kills_Players_AssisterId",
                        column: x => x.AssisterId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Kills_Players_KillerId",
                        column: x => x.KillerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Kills_Players_VictimId",
                        column: x => x.VictimId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Kills_Rounds_RoundId",
                        column: x => x.RoundId,
                        principalTable: "Rounds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MapControls",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DemoFileId = table.Column<int>(type: "int", nullable: false),
                    RoundId = table.Column<int>(type: "int", nullable: true),
                    Tick = table.Column<int>(type: "int", nullable: false),
                    GameTime = table.Column<float>(type: "real", nullable: false),
                    RoundNumber = table.Column<int>(type: "int", nullable: false),
                    MapName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CTAreaControl = table.Column<float>(type: "real", nullable: false),
                    TAreaControl = table.Column<float>(type: "real", nullable: false),
                    NeutralAreaControl = table.Column<float>(type: "real", nullable: false),
                    ASiteControl = table.Column<float>(type: "real", nullable: false),
                    BSiteControl = table.Column<float>(type: "real", nullable: false),
                    MidControl = table.Column<float>(type: "real", nullable: false),
                    CTPlayersInAsite = table.Column<int>(type: "int", nullable: false),
                    TPlayersInAsite = table.Column<int>(type: "int", nullable: false),
                    CTPlayersInBsite = table.Column<int>(type: "int", nullable: false),
                    TPlayersInBsite = table.Column<int>(type: "int", nullable: false),
                    CTPlayersInMid = table.Column<int>(type: "int", nullable: false),
                    TPlayersInMid = table.Column<int>(type: "int", nullable: false),
                    ControlledChokes = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    AdvantageousPositions = table.Column<int>(type: "int", nullable: false),
                    DisadvantageousPositions = table.Column<int>(type: "int", nullable: false),
                    SmokesCoveringAreas = table.Column<int>(type: "int", nullable: false),
                    FlashesBlindingAreas = table.Column<int>(type: "int", nullable: false),
                    HEGrenadesControllingAreas = table.Column<int>(type: "int", nullable: false),
                    MolotovsBlockingAreas = table.Column<int>(type: "int", nullable: false),
                    ControlMomentum = table.Column<float>(type: "real", nullable: false),
                    IsShiftingControl = table.Column<bool>(type: "bit", nullable: false),
                    TimeInControl = table.Column<float>(type: "real", nullable: false),
                    DominantTeamZone = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TerritoryBalance = table.Column<float>(type: "real", nullable: false),
                    CTRotatingToA = table.Column<bool>(type: "bit", nullable: false),
                    CTRotatingToB = table.Column<bool>(type: "bit", nullable: false),
                    TRotatingToA = table.Column<bool>(type: "bit", nullable: false),
                    TRotatingToB = table.Column<bool>(type: "bit", nullable: false),
                    CTStackedOneSite = table.Column<bool>(type: "bit", nullable: false),
                    TCommittedToSite = table.Column<bool>(type: "bit", nullable: false),
                    ExpectedStrategy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ControlNotes = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MapControls", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MapControls_DemoFiles_DemoFileId",
                        column: x => x.DemoFileId,
                        principalTable: "DemoFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MapControls_Rounds_RoundId",
                        column: x => x.RoundId,
                        principalTable: "Rounds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PerformanceMetrics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DemoFileId = table.Column<int>(type: "int", nullable: false),
                    PlayerId = table.Column<int>(type: "int", nullable: false),
                    RoundId = table.Column<int>(type: "int", nullable: true),
                    Tick = table.Column<int>(type: "int", nullable: false),
                    GameTime = table.Column<float>(type: "real", nullable: false),
                    MetricType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MetricName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Value = table.Column<float>(type: "real", nullable: false),
                    NormalizedValue = table.Column<float>(type: "real", nullable: false),
                    Confidence = table.Column<float>(type: "real", nullable: false),
                    Context = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Situation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RoundNumber = table.Column<int>(type: "int", nullable: false),
                    TeamAverage = table.Column<float>(type: "real", nullable: false),
                    MatchAverage = table.Column<float>(type: "real", nullable: false),
                    PercentileRank = table.Column<float>(type: "real", nullable: false),
                    MovingAverage = table.Column<float>(type: "real", nullable: false),
                    Trend = table.Column<float>(type: "real", nullable: false),
                    IsImproving = table.Column<bool>(type: "bit", nullable: false),
                    IsDecreasing = table.Column<bool>(type: "bit", nullable: false),
                    ImpactScore = table.Column<float>(type: "real", nullable: false),
                    PositiveImpact = table.Column<bool>(type: "bit", nullable: false),
                    NegativeImpact = table.Column<bool>(type: "bit", nullable: false),
                    AdditionalData = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerformanceMetrics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PerformanceMetrics_DemoFiles_DemoFileId",
                        column: x => x.DemoFileId,
                        principalTable: "DemoFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PerformanceMetrics_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PerformanceMetrics_Rounds_RoundId",
                        column: x => x.RoundId,
                        principalTable: "Rounds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlayerBehaviorEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DemoFileId = table.Column<int>(type: "int", nullable: false),
                    RoundId = table.Column<int>(type: "int", nullable: true),
                    PlayerId = table.Column<int>(type: "int", nullable: false),
                    Tick = table.Column<int>(type: "int", nullable: false),
                    GameTime = table.Column<float>(type: "real", nullable: false),
                    BehaviorType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BehaviorSubType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PositionX = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    PositionY = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    PositionZ = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    VelocityX = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                    VelocityY = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                    VelocityZ = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                    Speed = table.Column<float>(type: "real", nullable: true),
                    Direction = table.Column<float>(type: "real", nullable: true),
                    ViewAngleX = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                    ViewAngleY = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                    SoundVolume = table.Column<float>(type: "real", nullable: true),
                    SoundRadius = table.Column<float>(type: "real", nullable: true),
                    SoundType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SurfaceMaterial = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    WeaponName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsWeaponInspection = table.Column<bool>(type: "bit", nullable: false),
                    IsWeaponReload = table.Column<bool>(type: "bit", nullable: false),
                    IsWeaponDraw = table.Column<bool>(type: "bit", nullable: false),
                    IsWeaponHolster = table.Column<bool>(type: "bit", nullable: false),
                    JumpHeight = table.Column<float>(type: "real", nullable: true),
                    FallDistance = table.Column<float>(type: "real", nullable: true),
                    FallDamage = table.Column<float>(type: "real", nullable: true),
                    LandingImpact = table.Column<float>(type: "real", nullable: true),
                    IsWalking = table.Column<bool>(type: "bit", nullable: false),
                    IsRunning = table.Column<bool>(type: "bit", nullable: false),
                    IsCrouching = table.Column<bool>(type: "bit", nullable: false),
                    IsInAir = table.Column<bool>(type: "bit", nullable: false),
                    IsOnLadder = table.Column<bool>(type: "bit", nullable: false),
                    IsInWater = table.Column<bool>(type: "bit", nullable: false),
                    IsSilentMovement = table.Column<bool>(type: "bit", nullable: false),
                    IsAudibleToEnemies = table.Column<bool>(type: "bit", nullable: false),
                    StealthScore = table.Column<float>(type: "real", nullable: true),
                    TacticalContext = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsPeeking = table.Column<bool>(type: "bit", nullable: false),
                    IsRetreating = table.Column<bool>(type: "bit", nullable: false),
                    IsAdvancing = table.Column<bool>(type: "bit", nullable: false),
                    IsHoldingAngle = table.Column<bool>(type: "bit", nullable: false),
                    TimeSinceLastAction = table.Column<float>(type: "real", nullable: true),
                    ActionDuration = table.Column<float>(type: "real", nullable: true),
                    WasCompromising = table.Column<bool>(type: "bit", nullable: false),
                    WasTactical = table.Column<bool>(type: "bit", nullable: false),
                    Team = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RoundNumber = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerBehaviorEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerBehaviorEvents_DemoFiles_DemoFileId",
                        column: x => x.DemoFileId,
                        principalTable: "DemoFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlayerBehaviorEvents_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlayerBehaviorEvents_Rounds_RoundId",
                        column: x => x.RoundId,
                        principalTable: "Rounds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlayerMovements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DemoFileId = table.Column<int>(type: "int", nullable: false),
                    RoundId = table.Column<int>(type: "int", nullable: true),
                    PlayerId = table.Column<int>(type: "int", nullable: false),
                    Tick = table.Column<int>(type: "int", nullable: false),
                    GameTime = table.Column<float>(type: "real", nullable: false),
                    MovementType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PositionX = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    PositionY = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    PositionZ = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    VelocityX = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    VelocityY = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    VelocityZ = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    Speed = table.Column<float>(type: "real", nullable: false),
                    SpeedHorizontal = table.Column<float>(type: "real", nullable: false),
                    ViewAngleX = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    ViewAngleY = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    Team = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsOnGround = table.Column<bool>(type: "bit", nullable: false),
                    IsInAir = table.Column<bool>(type: "bit", nullable: false),
                    IsDucking = table.Column<bool>(type: "bit", nullable: false),
                    IsWalking = table.Column<bool>(type: "bit", nullable: false),
                    JumpHeight = table.Column<float>(type: "real", nullable: true),
                    FallDistance = table.Column<float>(type: "real", nullable: true),
                    IsBhopping = table.Column<bool>(type: "bit", nullable: false),
                    IsStrafing = table.Column<bool>(type: "bit", nullable: false),
                    IsSurfing = table.Column<bool>(type: "bit", nullable: false),
                    MovementTechnique = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerMovements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerMovements_DemoFiles_DemoFileId",
                        column: x => x.DemoFileId,
                        principalTable: "DemoFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlayerMovements_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlayerMovements_Rounds_RoundId",
                        column: x => x.RoundId,
                        principalTable: "Rounds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlayerRoundStats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlayerId = table.Column<int>(type: "int", nullable: false),
                    RoundId = table.Column<int>(type: "int", nullable: false),
                    Kills = table.Column<int>(type: "int", nullable: false),
                    Deaths = table.Column<int>(type: "int", nullable: false),
                    Assists = table.Column<int>(type: "int", nullable: false),
                    Damage = table.Column<int>(type: "int", nullable: false),
                    UtilityDamage = table.Column<int>(type: "int", nullable: false),
                    StartMoney = table.Column<int>(type: "int", nullable: false),
                    MoneySpent = table.Column<int>(type: "int", nullable: false),
                    EndMoney = table.Column<int>(type: "int", nullable: false),
                    EquipmentValue = table.Column<int>(type: "int", nullable: false),
                    IsAlive = table.Column<bool>(type: "bit", nullable: false),
                    Health = table.Column<int>(type: "int", nullable: false),
                    Armor = table.Column<int>(type: "int", nullable: false),
                    HasHelmet = table.Column<bool>(type: "bit", nullable: false),
                    HasDefuseKit = table.Column<bool>(type: "bit", nullable: false),
                    HasBomb = table.Column<bool>(type: "bit", nullable: false),
                    Rating = table.Column<float>(type: "real", nullable: false),
                    ShotsFired = table.Column<int>(type: "int", nullable: false),
                    ShotsHit = table.Column<int>(type: "int", nullable: false),
                    Accuracy = table.Column<float>(type: "real", nullable: false),
                    KAST = table.Column<bool>(type: "bit", nullable: false),
                    MVP = table.Column<bool>(type: "bit", nullable: false),
                    FlashAssists = table.Column<int>(type: "int", nullable: false),
                    EnemiesFlashed = table.Column<int>(type: "int", nullable: false),
                    TeammatesFlashed = table.Column<int>(type: "int", nullable: false),
                    FlashDuration = table.Column<float>(type: "real", nullable: false),
                    SurvivalTime = table.Column<float>(type: "real", nullable: false),
                    ObjectiveTime = table.Column<int>(type: "int", nullable: false),
                    IsClutch = table.Column<bool>(type: "bit", nullable: false),
                    ClutchSize = table.Column<int>(type: "int", nullable: false),
                    ClutchWon = table.Column<bool>(type: "bit", nullable: false),
                    PositionX = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PositionY = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PositionZ = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ViewAngleX = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ViewAngleY = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ViewAngleZ = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerRoundStats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerRoundStats_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlayerRoundStats_Rounds_RoundId",
                        column: x => x.RoundId,
                        principalTable: "Rounds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RadioCommands",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DemoFileId = table.Column<int>(type: "int", nullable: false),
                    RoundId = table.Column<int>(type: "int", nullable: true),
                    PlayerId = table.Column<int>(type: "int", nullable: false),
                    Tick = table.Column<int>(type: "int", nullable: false),
                    GameTime = table.Column<float>(type: "real", nullable: false),
                    Command = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CommandCategory = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PositionX = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    PositionY = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    PositionZ = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    Team = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RoundNumber = table.Column<int>(type: "int", nullable: false),
                    Context = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RadioCommands", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RadioCommands_DemoFiles_DemoFileId",
                        column: x => x.DemoFileId,
                        principalTable: "DemoFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RadioCommands_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RadioCommands_Rounds_RoundId",
                        column: x => x.RoundId,
                        principalTable: "Rounds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RoundImpacts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DemoFileId = table.Column<int>(type: "int", nullable: false),
                    PlayerId = table.Column<int>(type: "int", nullable: false),
                    RoundId = table.Column<int>(type: "int", nullable: false),
                    RoundNumber = table.Column<int>(type: "int", nullable: false),
                    OverallImpact = table.Column<float>(type: "real", nullable: false),
                    PositiveImpact = table.Column<float>(type: "real", nullable: false),
                    NegativeImpact = table.Column<float>(type: "real", nullable: false),
                    NetImpact = table.Column<float>(type: "real", nullable: false),
                    FraggingImpact = table.Column<float>(type: "real", nullable: false),
                    UtilityImpact = table.Column<float>(type: "real", nullable: false),
                    PositionalImpact = table.Column<float>(type: "real", nullable: false),
                    EconomicImpact = table.Column<float>(type: "real", nullable: false),
                    TacticalImpact = table.Column<float>(type: "real", nullable: false),
                    HasEntryFrag = table.Column<bool>(type: "bit", nullable: false),
                    HasClutchAttempt = table.Column<bool>(type: "bit", nullable: false),
                    HasClutchWin = table.Column<bool>(type: "bit", nullable: false),
                    HasMultiKill = table.Column<bool>(type: "bit", nullable: false),
                    HasCriticalSave = table.Column<bool>(type: "bit", nullable: false),
                    HasGameChangingPlay = table.Column<bool>(type: "bit", nullable: false),
                    EarlyRoundImpact = table.Column<float>(type: "real", nullable: false),
                    MidRoundImpact = table.Column<float>(type: "real", nullable: false),
                    LateRoundImpact = table.Column<float>(type: "real", nullable: false),
                    WinRoundContribution = table.Column<float>(type: "real", nullable: false),
                    LossRoundImpact = table.Column<float>(type: "real", nullable: false),
                    RoundOutcomePrediction = table.Column<float>(type: "real", nullable: false),
                    DecisionQuality = table.Column<float>(type: "real", nullable: false),
                    GoodDecisions = table.Column<int>(type: "int", nullable: false),
                    BadDecisions = table.Column<int>(type: "int", nullable: false),
                    CriticalDecisions = table.Column<int>(type: "int", nullable: false),
                    TeamSupportImpact = table.Column<float>(type: "real", nullable: false),
                    LeadershipImpact = table.Column<float>(type: "real", nullable: false),
                    FollowupImpact = table.Column<float>(type: "real", nullable: false),
                    RiskTaken = table.Column<float>(type: "real", nullable: false),
                    RewardAchieved = table.Column<float>(type: "real", nullable: false),
                    RiskRewardRatio = table.Column<float>(type: "real", nullable: false),
                    MomentumGenerated = table.Column<float>(type: "real", nullable: false),
                    MomentumLost = table.Column<float>(type: "real", nullable: false),
                    MomentumShift = table.Column<float>(type: "real", nullable: false),
                    RoundType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RoundTypeImpact = table.Column<float>(type: "real", nullable: false),
                    KeyMoment = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    KeyMomentImpact = table.Column<float>(type: "real", nullable: false),
                    WinProbabilityContribution = table.Column<float>(type: "real", nullable: false),
                    ExpectedValue = table.Column<float>(type: "real", nullable: false),
                    PerformanceVsExpected = table.Column<float>(type: "real", nullable: false),
                    ImpactSummary = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoundImpacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoundImpacts_DemoFiles_DemoFileId",
                        column: x => x.DemoFileId,
                        principalTable: "DemoFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RoundImpacts_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RoundImpacts_Rounds_RoundId",
                        column: x => x.RoundId,
                        principalTable: "Rounds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SmokeClouds",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DemoFileId = table.Column<int>(type: "int", nullable: false),
                    RoundId = table.Column<int>(type: "int", nullable: true),
                    ThrowerPlayerId = table.Column<int>(type: "int", nullable: true),
                    GrenadeEntityId = table.Column<int>(type: "int", nullable: false),
                    StartTick = table.Column<int>(type: "int", nullable: false),
                    EndTick = table.Column<int>(type: "int", nullable: true),
                    StartTime = table.Column<float>(type: "real", nullable: false),
                    EndTime = table.Column<float>(type: "real", nullable: true),
                    Duration = table.Column<float>(type: "real", nullable: false),
                    CenterX = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    CenterY = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    CenterZ = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    MaxRadius = table.Column<float>(type: "real", nullable: false),
                    CurrentRadius = table.Column<float>(type: "real", nullable: false),
                    Opacity = table.Column<float>(type: "real", nullable: false),
                    Phase = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ExpansionTime = table.Column<float>(type: "real", nullable: false),
                    FullTime = table.Column<float>(type: "real", nullable: false),
                    DissipationTime = table.Column<float>(type: "real", nullable: false),
                    PlayersObscured = table.Column<int>(type: "int", nullable: false),
                    SightLinesBlocked = table.Column<int>(type: "int", nullable: false),
                    BlocksBombsiteView = table.Column<bool>(type: "bit", nullable: false),
                    BlocksChoke = table.Column<bool>(type: "bit", nullable: false),
                    EnabledPlantDefuse = table.Column<bool>(type: "bit", nullable: false),
                    TacticalPurpose = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Team = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RoundNumber = table.Column<int>(type: "int", nullable: false),
                    AffectedByWind = table.Column<bool>(type: "bit", nullable: false),
                    WindDirection = table.Column<float>(type: "real", nullable: false),
                    WindStrength = table.Column<float>(type: "real", nullable: false),
                    Properties = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SmokeClouds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SmokeClouds_DemoFiles_DemoFileId",
                        column: x => x.DemoFileId,
                        principalTable: "DemoFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SmokeClouds_Players_ThrowerPlayerId",
                        column: x => x.ThrowerPlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SmokeClouds_Rounds_RoundId",
                        column: x => x.RoundId,
                        principalTable: "Rounds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TacticalEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DemoFileId = table.Column<int>(type: "int", nullable: false),
                    RoundId = table.Column<int>(type: "int", nullable: true),
                    InitiatorPlayerId = table.Column<int>(type: "int", nullable: true),
                    Tick = table.Column<int>(type: "int", nullable: false),
                    GameTime = table.Column<float>(type: "real", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Team = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RoundNumber = table.Column<int>(type: "int", nullable: false),
                    TargetArea = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SecondaryArea = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PlayersInvolved = table.Column<int>(type: "int", nullable: false),
                    Coordination = table.Column<float>(type: "real", nullable: false),
                    Timing = table.Column<float>(type: "real", nullable: false),
                    SmokesUsed = table.Column<int>(type: "int", nullable: false),
                    FlashesUsed = table.Column<int>(type: "int", nullable: false),
                    HEGrenadesUsed = table.Column<int>(type: "int", nullable: false),
                    MolotovsUsed = table.Column<int>(type: "int", nullable: false),
                    DecoysUsed = table.Column<int>(type: "int", nullable: false),
                    WasSuccessful = table.Column<bool>(type: "bit", nullable: false),
                    SuccessRate = table.Column<float>(type: "real", nullable: false),
                    ExecutionQuality = table.Column<float>(type: "real", nullable: false),
                    StartTime = table.Column<float>(type: "real", nullable: false),
                    EndTime = table.Column<float>(type: "real", nullable: true),
                    Duration = table.Column<float>(type: "real", nullable: false),
                    WasRushed = table.Column<bool>(type: "bit", nullable: false),
                    WasDelayed = table.Column<bool>(type: "bit", nullable: false),
                    CounterTactic = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    WasCountered = table.Column<bool>(type: "bit", nullable: false),
                    CounterEffectiveness = table.Column<float>(type: "real", nullable: false),
                    KillsGenerated = table.Column<int>(type: "int", nullable: false),
                    DeathsCaused = table.Column<int>(type: "int", nullable: false),
                    DamageDealt = table.Column<float>(type: "real", nullable: false),
                    AchievedObjective = table.Column<bool>(type: "bit", nullable: false),
                    RoundContext = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    StrategicIntent = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsInnovativePlay = table.Column<bool>(type: "bit", nullable: false),
                    IsAdaptation = table.Column<bool>(type: "bit", nullable: false),
                    Unpredictability = table.Column<float>(type: "real", nullable: false),
                    TacticalNotes = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TacticalEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TacticalEvents_DemoFiles_DemoFileId",
                        column: x => x.DemoFileId,
                        principalTable: "DemoFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TacticalEvents_Players_InitiatorPlayerId",
                        column: x => x.InitiatorPlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TacticalEvents_Rounds_RoundId",
                        column: x => x.RoundId,
                        principalTable: "Rounds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TeamStates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DemoFileId = table.Column<int>(type: "int", nullable: false),
                    RoundId = table.Column<int>(type: "int", nullable: true),
                    Tick = table.Column<int>(type: "int", nullable: false),
                    GameTime = table.Column<float>(type: "real", nullable: false),
                    Team = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RoundNumber = table.Column<int>(type: "int", nullable: false),
                    TotalMoney = table.Column<int>(type: "int", nullable: false),
                    AverageMoney = table.Column<int>(type: "int", nullable: false),
                    TotalEquipmentValue = table.Column<int>(type: "int", nullable: false),
                    LivingPlayers = table.Column<int>(type: "int", nullable: false),
                    PlayersWithArmor = table.Column<int>(type: "int", nullable: false),
                    PlayersWithHelmet = table.Column<int>(type: "int", nullable: false),
                    PlayersWithDefuseKit = table.Column<int>(type: "int", nullable: false),
                    RifleCount = table.Column<int>(type: "int", nullable: false),
                    PistolCount = table.Column<int>(type: "int", nullable: false),
                    SniperCount = table.Column<int>(type: "int", nullable: false),
                    SMGCount = table.Column<int>(type: "int", nullable: false),
                    ShotgunCount = table.Column<int>(type: "int", nullable: false),
                    HEGrenadeCount = table.Column<int>(type: "int", nullable: false),
                    FlashbangCount = table.Column<int>(type: "int", nullable: false),
                    SmokegrenadeCount = table.Column<int>(type: "int", nullable: false),
                    MolotovCount = table.Column<int>(type: "int", nullable: false),
                    DecoyCount = table.Column<int>(type: "int", nullable: false),
                    PrimaryArea = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SecondaryArea = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TeamSpread = table.Column<float>(type: "real", nullable: false),
                    IsStacked = table.Column<bool>(type: "bit", nullable: false),
                    IsRotating = table.Column<bool>(type: "bit", nullable: false),
                    IsSaveRound = table.Column<bool>(type: "bit", nullable: false),
                    IsForceRound = table.Column<bool>(type: "bit", nullable: false),
                    IsEcoRound = table.Column<bool>(type: "bit", nullable: false),
                    IsFullBuyRound = table.Column<bool>(type: "bit", nullable: false),
                    IsAntiEcoRound = table.Column<bool>(type: "bit", nullable: false),
                    TeamCohesion = table.Column<float>(type: "real", nullable: false),
                    TradeKillPotential = table.Column<float>(type: "real", nullable: false),
                    SiteControl = table.Column<float>(type: "real", nullable: false),
                    TacticalNotes = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamStates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeamStates_DemoFiles_DemoFileId",
                        column: x => x.DemoFileId,
                        principalTable: "DemoFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TeamStates_Rounds_RoundId",
                        column: x => x.RoundId,
                        principalTable: "Rounds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TemporaryEntities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DemoFileId = table.Column<int>(type: "int", nullable: false),
                    RoundId = table.Column<int>(type: "int", nullable: true),
                    PlayerId = table.Column<int>(type: "int", nullable: true),
                    Tick = table.Column<int>(type: "int", nullable: false),
                    GameTime = table.Column<float>(type: "real", nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SubType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PositionX = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    PositionY = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    PositionZ = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    DirectionX = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                    DirectionY = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                    DirectionZ = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                    Intensity = table.Column<float>(type: "real", nullable: true),
                    Duration = table.Column<float>(type: "real", nullable: true),
                    Scale = table.Column<float>(type: "real", nullable: true),
                    Material = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    WeaponName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EndPositionX = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                    EndPositionY = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                    EndPositionZ = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                    TargetEntityId = table.Column<int>(type: "int", nullable: true),
                    Color = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Alpha = table.Column<float>(type: "real", nullable: true),
                    ImpactForce = table.Column<float>(type: "real", nullable: true),
                    IsWallbang = table.Column<bool>(type: "bit", nullable: false),
                    PenetrationCount = table.Column<int>(type: "int", nullable: true),
                    ExplosionRadius = table.Column<float>(type: "real", nullable: true),
                    DamageRadius = table.Column<float>(type: "real", nullable: true),
                    Team = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RoundNumber = table.Column<int>(type: "int", nullable: false),
                    AdditionalData = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemporaryEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TemporaryEntities_DemoFiles_DemoFileId",
                        column: x => x.DemoFileId,
                        principalTable: "DemoFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TemporaryEntities_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TemporaryEntities_Rounds_RoundId",
                        column: x => x.RoundId,
                        principalTable: "Rounds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VoiceCommunications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DemoFileId = table.Column<int>(type: "int", nullable: false),
                    RoundId = table.Column<int>(type: "int", nullable: true),
                    SpeakerId = table.Column<int>(type: "int", nullable: false),
                    StartTick = table.Column<int>(type: "int", nullable: false),
                    EndTick = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<float>(type: "real", nullable: false),
                    EndTime = table.Column<float>(type: "real", nullable: false),
                    Duration = table.Column<float>(type: "real", nullable: false),
                    CommunicationType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RadioCommand = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CommandCategory = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CommandPurpose = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    VoiceIntensity = table.Column<float>(type: "real", nullable: false),
                    IsUrgent = table.Column<bool>(type: "bit", nullable: false),
                    IsCalm = table.Column<bool>(type: "bit", nullable: false),
                    IsEmotional = table.Column<bool>(type: "bit", nullable: false),
                    DuringAction = table.Column<bool>(type: "bit", nullable: false),
                    PreRound = table.Column<bool>(type: "bit", nullable: false),
                    MidRound = table.Column<bool>(type: "bit", nullable: false),
                    PostRound = table.Column<bool>(type: "bit", nullable: false),
                    SituationalContext = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ToTeam = table.Column<bool>(type: "bit", nullable: false),
                    ToSpecific = table.Column<bool>(type: "bit", nullable: false),
                    TargetPlayerId = table.Column<int>(type: "int", nullable: true),
                    WasFollowed = table.Column<bool>(type: "bit", nullable: false),
                    WasCorrect = table.Column<bool>(type: "bit", nullable: false),
                    EffectivenessScore = table.Column<float>(type: "real", nullable: false),
                    ClarityScore = table.Column<float>(type: "real", nullable: false),
                    SpeakerPositionX = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    SpeakerPositionY = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    SpeakerPositionZ = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    SpeakerArea = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TranscribedContent = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ContentSummary = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TriggeredRotation = table.Column<bool>(type: "bit", nullable: false),
                    TriggeredRegroup = table.Column<bool>(type: "bit", nullable: false),
                    TriggeredExecute = table.Column<bool>(type: "bit", nullable: false),
                    TriggeredSave = table.Column<bool>(type: "bit", nullable: false),
                    Team = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RoundNumber = table.Column<int>(type: "int", nullable: false),
                    IsLeadershipCommunication = table.Column<bool>(type: "bit", nullable: false),
                    IsQuestion = table.Column<bool>(type: "bit", nullable: false),
                    IsOrder = table.Column<bool>(type: "bit", nullable: false),
                    IsResponse = table.Column<bool>(type: "bit", nullable: false),
                    IsCallout = table.Column<bool>(type: "bit", nullable: false),
                    InterruptedOther = table.Column<bool>(type: "bit", nullable: false),
                    WasInterrupted = table.Column<bool>(type: "bit", nullable: false),
                    InterruptedCommunicationId = table.Column<int>(type: "int", nullable: true),
                    AdditionalData = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VoiceCommunications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VoiceCommunications_DemoFiles_DemoFileId",
                        column: x => x.DemoFileId,
                        principalTable: "DemoFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VoiceCommunications_Players_SpeakerId",
                        column: x => x.SpeakerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VoiceCommunications_Players_TargetPlayerId",
                        column: x => x.TargetPlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VoiceCommunications_Rounds_RoundId",
                        column: x => x.RoundId,
                        principalTable: "Rounds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WeaponFires",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DemoFileId = table.Column<int>(type: "int", nullable: false),
                    RoundId = table.Column<int>(type: "int", nullable: false),
                    Tick = table.Column<int>(type: "int", nullable: false),
                    GameTime = table.Column<float>(type: "real", nullable: false),
                    PlayerId = table.Column<int>(type: "int", nullable: false),
                    Weapon = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    WeaponClass = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PositionX = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PositionY = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PositionZ = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ViewAngleX = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ViewAngleY = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ViewAngleZ = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Team = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsScoped = table.Column<bool>(type: "bit", nullable: false),
                    IsSilenced = table.Column<bool>(type: "bit", nullable: false),
                    Ammo = table.Column<int>(type: "int", nullable: false),
                    AmmoReserve = table.Column<int>(type: "int", nullable: false),
                    RecoilIndex = table.Column<float>(type: "real", nullable: false),
                    Accuracy = table.Column<float>(type: "real", nullable: false),
                    Velocity = table.Column<float>(type: "real", nullable: false),
                    ThroughSmoke = table.Column<bool>(type: "bit", nullable: false),
                    IsBlind = table.Column<bool>(type: "bit", nullable: false),
                    FlashDuration = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeaponFires", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WeaponFires_DemoFiles_DemoFileId",
                        column: x => x.DemoFileId,
                        principalTable: "DemoFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WeaponFires_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WeaponFires_Rounds_RoundId",
                        column: x => x.RoundId,
                        principalTable: "Rounds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WeaponStates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DemoFileId = table.Column<int>(type: "int", nullable: false),
                    RoundId = table.Column<int>(type: "int", nullable: true),
                    PlayerId = table.Column<int>(type: "int", nullable: false),
                    Tick = table.Column<int>(type: "int", nullable: false),
                    GameTime = table.Column<float>(type: "real", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    WeaponName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AmmoClip = table.Column<int>(type: "int", nullable: false),
                    AmmoReserve = table.Column<int>(type: "int", nullable: false),
                    IsScoped = table.Column<bool>(type: "bit", nullable: false),
                    ZoomLevel = table.Column<int>(type: "int", nullable: false),
                    IsSilenced = table.Column<bool>(type: "bit", nullable: false),
                    IsReloading = table.Column<bool>(type: "bit", nullable: false),
                    PositionX = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    PositionY = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    PositionZ = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    Team = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RoundNumber = table.Column<int>(type: "int", nullable: false),
                    ReloadDuration = table.Column<float>(type: "real", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeaponStates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WeaponStates_DemoFiles_DemoFileId",
                        column: x => x.DemoFileId,
                        principalTable: "DemoFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WeaponStates_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WeaponStates_Rounds_RoundId",
                        column: x => x.RoundId,
                        principalTable: "Rounds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ZoneEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DemoFileId = table.Column<int>(type: "int", nullable: false),
                    RoundId = table.Column<int>(type: "int", nullable: true),
                    PlayerId = table.Column<int>(type: "int", nullable: false),
                    Tick = table.Column<int>(type: "int", nullable: false),
                    GameTime = table.Column<float>(type: "real", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ZoneType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PositionX = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    PositionY = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    PositionZ = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    Team = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RoundNumber = table.Column<int>(type: "int", nullable: false),
                    TimeInZone = table.Column<float>(type: "real", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZoneEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ZoneEvents_DemoFiles_DemoFileId",
                        column: x => x.DemoFileId,
                        principalTable: "DemoFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ZoneEvents_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ZoneEvents_Rounds_RoundId",
                        column: x => x.RoundId,
                        principalTable: "Rounds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdvancedPlayerStats_DemoFileId_RoundNumber",
                table: "AdvancedPlayerStats",
                columns: new[] { "DemoFileId", "RoundNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_AdvancedPlayerStats_MatchId",
                table: "AdvancedPlayerStats",
                column: "MatchId");

            migrationBuilder.CreateIndex(
                name: "IX_AdvancedPlayerStats_PlayerId_StatsType",
                table: "AdvancedPlayerStats",
                columns: new[] { "PlayerId", "StatsType" });

            migrationBuilder.CreateIndex(
                name: "IX_AdvancedPlayerStats_RoundId",
                table: "AdvancedPlayerStats",
                column: "RoundId");

            migrationBuilder.CreateIndex(
                name: "IX_AdvancedUserMessages_DemoFileId",
                table: "AdvancedUserMessages",
                column: "DemoFileId");

            migrationBuilder.CreateIndex(
                name: "IX_AdvancedUserMessages_PlayerId_Tick",
                table: "AdvancedUserMessages",
                columns: new[] { "PlayerId", "Tick" });

            migrationBuilder.CreateIndex(
                name: "IX_AdvancedUserMessages_RoundId_MessageType",
                table: "AdvancedUserMessages",
                columns: new[] { "RoundId", "MessageType" });

            migrationBuilder.CreateIndex(
                name: "IX_AdvancedUserMessages_TargetPlayerId",
                table: "AdvancedUserMessages",
                column: "TargetPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Bombs_DemoFileId",
                table: "Bombs",
                column: "DemoFileId");

            migrationBuilder.CreateIndex(
                name: "IX_Bombs_PlayerId",
                table: "Bombs",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Bombs_RoundId_Tick",
                table: "Bombs",
                columns: new[] { "RoundId", "Tick" });

            migrationBuilder.CreateIndex(
                name: "IX_BulletImpacts_DemoFileId",
                table: "BulletImpacts",
                column: "DemoFileId");

            migrationBuilder.CreateIndex(
                name: "IX_BulletImpacts_HitPlayerId",
                table: "BulletImpacts",
                column: "HitPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_BulletImpacts_PlayerId_Tick",
                table: "BulletImpacts",
                columns: new[] { "PlayerId", "Tick" });

            migrationBuilder.CreateIndex(
                name: "IX_BulletImpacts_RoundId_Weapon",
                table: "BulletImpacts",
                columns: new[] { "RoundId", "Weapon" });

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_DemoFileId_Tick",
                table: "ChatMessages",
                columns: new[] { "DemoFileId", "Tick" });

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_PlayerId",
                table: "ChatMessages",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_CommunicationPatterns_DemoFileId_Team",
                table: "CommunicationPatterns",
                columns: new[] { "DemoFileId", "Team" });

            migrationBuilder.CreateIndex(
                name: "IX_CommunicationPatterns_PrimaryLeaderId",
                table: "CommunicationPatterns",
                column: "PrimaryLeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_CommunicationPatterns_RoundId_PatternType",
                table: "CommunicationPatterns",
                columns: new[] { "RoundId", "PatternType" });

            migrationBuilder.CreateIndex(
                name: "IX_Damages_AttackerId",
                table: "Damages",
                column: "AttackerId");

            migrationBuilder.CreateIndex(
                name: "IX_Damages_DemoFileId",
                table: "Damages",
                column: "DemoFileId");

            migrationBuilder.CreateIndex(
                name: "IX_Damages_RoundId_Tick",
                table: "Damages",
                columns: new[] { "RoundId", "Tick" });

            migrationBuilder.CreateIndex(
                name: "IX_Damages_VictimId",
                table: "Damages",
                column: "VictimId");

            migrationBuilder.CreateIndex(
                name: "IX_DroppedItems_DemoFileId",
                table: "DroppedItems",
                column: "DemoFileId");

            migrationBuilder.CreateIndex(
                name: "IX_DroppedItems_DropperPlayerId_DropTick",
                table: "DroppedItems",
                columns: new[] { "DropperPlayerId", "DropTick" });

            migrationBuilder.CreateIndex(
                name: "IX_DroppedItems_EntityId",
                table: "DroppedItems",
                column: "EntityId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DroppedItems_PickerPlayerId",
                table: "DroppedItems",
                column: "PickerPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_DroppedItems_RoundId_ItemType",
                table: "DroppedItems",
                columns: new[] { "RoundId", "ItemType" });

            migrationBuilder.CreateIndex(
                name: "IX_EconomyEvents_DemoFileId",
                table: "EconomyEvents",
                column: "DemoFileId");

            migrationBuilder.CreateIndex(
                name: "IX_EconomyEvents_PlayerId_Tick",
                table: "EconomyEvents",
                columns: new[] { "PlayerId", "Tick" });

            migrationBuilder.CreateIndex(
                name: "IX_EconomyEvents_RoundId_EventType",
                table: "EconomyEvents",
                columns: new[] { "RoundId", "EventType" });

            migrationBuilder.CreateIndex(
                name: "IX_EconomyStates_DemoFileId_Phase",
                table: "EconomyStates",
                columns: new[] { "DemoFileId", "Phase" });

            migrationBuilder.CreateIndex(
                name: "IX_EconomyStates_RoundId_Team",
                table: "EconomyStates",
                columns: new[] { "RoundId", "Team" });

            migrationBuilder.CreateIndex(
                name: "IX_EntityEffects_DemoFileId",
                table: "EntityEffects",
                column: "DemoFileId");

            migrationBuilder.CreateIndex(
                name: "IX_EntityEffects_RoundId_EffectType",
                table: "EntityEffects",
                columns: new[] { "RoundId", "EffectType" });

            migrationBuilder.CreateIndex(
                name: "IX_EntityEffects_SourcePlayerId_StartTick",
                table: "EntityEffects",
                columns: new[] { "SourcePlayerId", "StartTick" });

            migrationBuilder.CreateIndex(
                name: "IX_EntityInteractions_DemoFileId",
                table: "EntityInteractions",
                column: "DemoFileId");

            migrationBuilder.CreateIndex(
                name: "IX_EntityInteractions_InitiatorPlayerId_Tick",
                table: "EntityInteractions",
                columns: new[] { "InitiatorPlayerId", "Tick" });

            migrationBuilder.CreateIndex(
                name: "IX_EntityInteractions_RoundId_InteractionType",
                table: "EntityInteractions",
                columns: new[] { "RoundId", "InteractionType" });

            migrationBuilder.CreateIndex(
                name: "IX_EntityInteractions_TargetPlayerId",
                table: "EntityInteractions",
                column: "TargetPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_EntityLifecycles_DemoFileId",
                table: "EntityLifecycles",
                column: "DemoFileId");

            migrationBuilder.CreateIndex(
                name: "IX_EntityLifecycles_EntityType_EntityId",
                table: "EntityLifecycles",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_EntityLifecycles_PlayerId",
                table: "EntityLifecycles",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_EntityLifecycles_RoundId_EventType",
                table: "EntityLifecycles",
                columns: new[] { "RoundId", "EventType" });

            migrationBuilder.CreateIndex(
                name: "IX_EntityPropertyChanges_CausedByPlayerId",
                table: "EntityPropertyChanges",
                column: "CausedByPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_EntityPropertyChanges_DemoFileId",
                table: "EntityPropertyChanges",
                column: "DemoFileId");

            migrationBuilder.CreateIndex(
                name: "IX_EntityPropertyChanges_EntityIndex_PropertyName",
                table: "EntityPropertyChanges",
                columns: new[] { "EntityIndex", "PropertyName" });

            migrationBuilder.CreateIndex(
                name: "IX_EntityPropertyChanges_PlayerId_Tick",
                table: "EntityPropertyChanges",
                columns: new[] { "PlayerId", "Tick" });

            migrationBuilder.CreateIndex(
                name: "IX_EntityPropertyChanges_RoundId_ChangeType",
                table: "EntityPropertyChanges",
                columns: new[] { "RoundId", "ChangeType" });

            migrationBuilder.CreateIndex(
                name: "IX_EntityVisibilities_DemoFileId",
                table: "EntityVisibilities",
                column: "DemoFileId");

            migrationBuilder.CreateIndex(
                name: "IX_EntityVisibilities_ObserverPlayerId_Tick",
                table: "EntityVisibilities",
                columns: new[] { "ObserverPlayerId", "Tick" });

            migrationBuilder.CreateIndex(
                name: "IX_EntityVisibilities_RoundId_EntityType",
                table: "EntityVisibilities",
                columns: new[] { "RoundId", "EntityType" });

            migrationBuilder.CreateIndex(
                name: "IX_EntityVisibilities_TargetPlayerId",
                table: "EntityVisibilities",
                column: "TargetPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Equipment_DemoFileId",
                table: "Equipment",
                column: "DemoFileId");

            migrationBuilder.CreateIndex(
                name: "IX_Equipment_PlayerId_Tick",
                table: "Equipment",
                columns: new[] { "PlayerId", "Tick" });

            migrationBuilder.CreateIndex(
                name: "IX_FireAreas_DemoFileId",
                table: "FireAreas",
                column: "DemoFileId");

            migrationBuilder.CreateIndex(
                name: "IX_FireAreas_RoundId_GrenadeType",
                table: "FireAreas",
                columns: new[] { "RoundId", "GrenadeType" });

            migrationBuilder.CreateIndex(
                name: "IX_FireAreas_ThrowerPlayerId_StartTick",
                table: "FireAreas",
                columns: new[] { "ThrowerPlayerId", "StartTick" });

            migrationBuilder.CreateIndex(
                name: "IX_FlashEvents_DemoFileId",
                table: "FlashEvents",
                column: "DemoFileId");

            migrationBuilder.CreateIndex(
                name: "IX_FlashEvents_FlashedPlayerId_Tick",
                table: "FlashEvents",
                columns: new[] { "FlashedPlayerId", "Tick" });

            migrationBuilder.CreateIndex(
                name: "IX_FlashEvents_FlasherPlayerId",
                table: "FlashEvents",
                column: "FlasherPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_FlashEvents_RoundId_FlashDuration",
                table: "FlashEvents",
                columns: new[] { "RoundId", "FlashDuration" });

            migrationBuilder.CreateIndex(
                name: "IX_GameEvents_DemoFileId_Tick",
                table: "GameEvents",
                columns: new[] { "DemoFileId", "Tick" });

            migrationBuilder.CreateIndex(
                name: "IX_GameEvents_EventName",
                table: "GameEvents",
                column: "EventName");

            migrationBuilder.CreateIndex(
                name: "IX_GameEvents_PlayerId",
                table: "GameEvents",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Grenades_DemoFileId",
                table: "Grenades",
                column: "DemoFileId");

            migrationBuilder.CreateIndex(
                name: "IX_Grenades_PlayerId",
                table: "Grenades",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Grenades_RoundId_ThrowTick",
                table: "Grenades",
                columns: new[] { "RoundId", "ThrowTick" });

            migrationBuilder.CreateIndex(
                name: "IX_GrenadeTrajectories_DemoFileId",
                table: "GrenadeTrajectories",
                column: "DemoFileId");

            migrationBuilder.CreateIndex(
                name: "IX_GrenadeTrajectories_PlayerId_ThrowTick",
                table: "GrenadeTrajectories",
                columns: new[] { "PlayerId", "ThrowTick" });

            migrationBuilder.CreateIndex(
                name: "IX_GrenadeTrajectories_RoundId_GrenadeType",
                table: "GrenadeTrajectories",
                columns: new[] { "RoundId", "GrenadeType" });

            migrationBuilder.CreateIndex(
                name: "IX_HostageEvents_DemoFileId",
                table: "HostageEvents",
                column: "DemoFileId");

            migrationBuilder.CreateIndex(
                name: "IX_HostageEvents_HostageEntityId_EventType",
                table: "HostageEvents",
                columns: new[] { "HostageEntityId", "EventType" });

            migrationBuilder.CreateIndex(
                name: "IX_HostageEvents_PlayerId_Tick",
                table: "HostageEvents",
                columns: new[] { "PlayerId", "Tick" });

            migrationBuilder.CreateIndex(
                name: "IX_HostageEvents_RoundId_EventType",
                table: "HostageEvents",
                columns: new[] { "RoundId", "EventType" });

            migrationBuilder.CreateIndex(
                name: "IX_InfernoEvents_DemoFileId",
                table: "InfernoEvents",
                column: "DemoFileId");

            migrationBuilder.CreateIndex(
                name: "IX_InfernoEvents_ExtinguishedByPlayerId",
                table: "InfernoEvents",
                column: "ExtinguishedByPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_InfernoEvents_InfernoEntityId_EventType",
                table: "InfernoEvents",
                columns: new[] { "InfernoEntityId", "EventType" });

            migrationBuilder.CreateIndex(
                name: "IX_InfernoEvents_RoundId_EventType",
                table: "InfernoEvents",
                columns: new[] { "RoundId", "EventType" });

            migrationBuilder.CreateIndex(
                name: "IX_InfernoEvents_ThrowerPlayerId_StartTick",
                table: "InfernoEvents",
                columns: new[] { "ThrowerPlayerId", "StartTick" });

            migrationBuilder.CreateIndex(
                name: "IX_Kills_AssisterId",
                table: "Kills",
                column: "AssisterId");

            migrationBuilder.CreateIndex(
                name: "IX_Kills_DemoFileId",
                table: "Kills",
                column: "DemoFileId");

            migrationBuilder.CreateIndex(
                name: "IX_Kills_KillerId",
                table: "Kills",
                column: "KillerId");

            migrationBuilder.CreateIndex(
                name: "IX_Kills_RoundId_Tick",
                table: "Kills",
                columns: new[] { "RoundId", "Tick" });

            migrationBuilder.CreateIndex(
                name: "IX_Kills_VictimId",
                table: "Kills",
                column: "VictimId");

            migrationBuilder.CreateIndex(
                name: "IX_MapControls_DemoFileId_MapName",
                table: "MapControls",
                columns: new[] { "DemoFileId", "MapName" });

            migrationBuilder.CreateIndex(
                name: "IX_MapControls_RoundId_Tick",
                table: "MapControls",
                columns: new[] { "RoundId", "Tick" });

            migrationBuilder.CreateIndex(
                name: "IX_Matches_DemoFileId",
                table: "Matches",
                column: "DemoFileId");

            migrationBuilder.CreateIndex(
                name: "IX_PerformanceMetrics_DemoFileId",
                table: "PerformanceMetrics",
                column: "DemoFileId");

            migrationBuilder.CreateIndex(
                name: "IX_PerformanceMetrics_PlayerId_MetricType",
                table: "PerformanceMetrics",
                columns: new[] { "PlayerId", "MetricType" });

            migrationBuilder.CreateIndex(
                name: "IX_PerformanceMetrics_RoundId_MetricName",
                table: "PerformanceMetrics",
                columns: new[] { "RoundId", "MetricName" });

            migrationBuilder.CreateIndex(
                name: "IX_PlayerBehaviorEvents_DemoFileId_BehaviorType",
                table: "PlayerBehaviorEvents",
                columns: new[] { "DemoFileId", "BehaviorType" });

            migrationBuilder.CreateIndex(
                name: "IX_PlayerBehaviorEvents_PlayerId_Tick",
                table: "PlayerBehaviorEvents",
                columns: new[] { "PlayerId", "Tick" });

            migrationBuilder.CreateIndex(
                name: "IX_PlayerBehaviorEvents_RoundId_BehaviorType",
                table: "PlayerBehaviorEvents",
                columns: new[] { "RoundId", "BehaviorType" });

            migrationBuilder.CreateIndex(
                name: "IX_PlayerMatchStats_MatchId",
                table: "PlayerMatchStats",
                column: "MatchId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerMatchStats_PlayerId_MatchId",
                table: "PlayerMatchStats",
                columns: new[] { "PlayerId", "MatchId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlayerMovements_DemoFileId",
                table: "PlayerMovements",
                column: "DemoFileId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerMovements_PlayerId_Tick",
                table: "PlayerMovements",
                columns: new[] { "PlayerId", "Tick" });

            migrationBuilder.CreateIndex(
                name: "IX_PlayerMovements_RoundId_MovementType",
                table: "PlayerMovements",
                columns: new[] { "RoundId", "MovementType" });

            migrationBuilder.CreateIndex(
                name: "IX_PlayerPositions_DemoFileId",
                table: "PlayerPositions",
                column: "DemoFileId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerPositions_PlayerId_Tick",
                table: "PlayerPositions",
                columns: new[] { "PlayerId", "Tick" });

            migrationBuilder.CreateIndex(
                name: "IX_PlayerRoundStats_PlayerId_RoundId",
                table: "PlayerRoundStats",
                columns: new[] { "PlayerId", "RoundId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlayerRoundStats_RoundId",
                table: "PlayerRoundStats",
                column: "RoundId");

            migrationBuilder.CreateIndex(
                name: "IX_Players_DemoFileId_PlayerSlot",
                table: "Players",
                columns: new[] { "DemoFileId", "PlayerSlot" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Players_SteamId",
                table: "Players",
                column: "SteamId");

            migrationBuilder.CreateIndex(
                name: "IX_RadioCommands_DemoFileId",
                table: "RadioCommands",
                column: "DemoFileId");

            migrationBuilder.CreateIndex(
                name: "IX_RadioCommands_PlayerId_Tick",
                table: "RadioCommands",
                columns: new[] { "PlayerId", "Tick" });

            migrationBuilder.CreateIndex(
                name: "IX_RadioCommands_RoundId_Command",
                table: "RadioCommands",
                columns: new[] { "RoundId", "Command" });

            migrationBuilder.CreateIndex(
                name: "IX_RoundImpacts_DemoFileId_OverallImpact",
                table: "RoundImpacts",
                columns: new[] { "DemoFileId", "OverallImpact" });

            migrationBuilder.CreateIndex(
                name: "IX_RoundImpacts_PlayerId_RoundId",
                table: "RoundImpacts",
                columns: new[] { "PlayerId", "RoundId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoundImpacts_RoundId",
                table: "RoundImpacts",
                column: "RoundId");

            migrationBuilder.CreateIndex(
                name: "IX_Rounds_DemoFileId",
                table: "Rounds",
                column: "DemoFileId");

            migrationBuilder.CreateIndex(
                name: "IX_Rounds_MatchId_RoundNumber",
                table: "Rounds",
                columns: new[] { "MatchId", "RoundNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SmokeClouds_DemoFileId",
                table: "SmokeClouds",
                column: "DemoFileId");

            migrationBuilder.CreateIndex(
                name: "IX_SmokeClouds_RoundId_Phase",
                table: "SmokeClouds",
                columns: new[] { "RoundId", "Phase" });

            migrationBuilder.CreateIndex(
                name: "IX_SmokeClouds_ThrowerPlayerId_StartTick",
                table: "SmokeClouds",
                columns: new[] { "ThrowerPlayerId", "StartTick" });

            migrationBuilder.CreateIndex(
                name: "IX_TacticalEvents_DemoFileId",
                table: "TacticalEvents",
                column: "DemoFileId");

            migrationBuilder.CreateIndex(
                name: "IX_TacticalEvents_InitiatorPlayerId_Tick",
                table: "TacticalEvents",
                columns: new[] { "InitiatorPlayerId", "Tick" });

            migrationBuilder.CreateIndex(
                name: "IX_TacticalEvents_RoundId_EventType",
                table: "TacticalEvents",
                columns: new[] { "RoundId", "EventType" });

            migrationBuilder.CreateIndex(
                name: "IX_TeamStates_DemoFileId_Tick",
                table: "TeamStates",
                columns: new[] { "DemoFileId", "Tick" });

            migrationBuilder.CreateIndex(
                name: "IX_TeamStates_RoundId_Team",
                table: "TeamStates",
                columns: new[] { "RoundId", "Team" });

            migrationBuilder.CreateIndex(
                name: "IX_TemporaryEntities_DemoFileId_EntityType",
                table: "TemporaryEntities",
                columns: new[] { "DemoFileId", "EntityType" });

            migrationBuilder.CreateIndex(
                name: "IX_TemporaryEntities_PlayerId_Tick",
                table: "TemporaryEntities",
                columns: new[] { "PlayerId", "Tick" });

            migrationBuilder.CreateIndex(
                name: "IX_TemporaryEntities_RoundId_EntityType",
                table: "TemporaryEntities",
                columns: new[] { "RoundId", "EntityType" });

            migrationBuilder.CreateIndex(
                name: "IX_VoiceCommunications_DemoFileId",
                table: "VoiceCommunications",
                column: "DemoFileId");

            migrationBuilder.CreateIndex(
                name: "IX_VoiceCommunications_RoundId_CommunicationType",
                table: "VoiceCommunications",
                columns: new[] { "RoundId", "CommunicationType" });

            migrationBuilder.CreateIndex(
                name: "IX_VoiceCommunications_SpeakerId_StartTick",
                table: "VoiceCommunications",
                columns: new[] { "SpeakerId", "StartTick" });

            migrationBuilder.CreateIndex(
                name: "IX_VoiceCommunications_TargetPlayerId",
                table: "VoiceCommunications",
                column: "TargetPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_WeaponFires_DemoFileId",
                table: "WeaponFires",
                column: "DemoFileId");

            migrationBuilder.CreateIndex(
                name: "IX_WeaponFires_PlayerId",
                table: "WeaponFires",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_WeaponFires_RoundId_Tick",
                table: "WeaponFires",
                columns: new[] { "RoundId", "Tick" });

            migrationBuilder.CreateIndex(
                name: "IX_WeaponStates_DemoFileId",
                table: "WeaponStates",
                column: "DemoFileId");

            migrationBuilder.CreateIndex(
                name: "IX_WeaponStates_PlayerId_Tick",
                table: "WeaponStates",
                columns: new[] { "PlayerId", "Tick" });

            migrationBuilder.CreateIndex(
                name: "IX_WeaponStates_RoundId_EventType",
                table: "WeaponStates",
                columns: new[] { "RoundId", "EventType" });

            migrationBuilder.CreateIndex(
                name: "IX_ZoneEvents_DemoFileId",
                table: "ZoneEvents",
                column: "DemoFileId");

            migrationBuilder.CreateIndex(
                name: "IX_ZoneEvents_PlayerId_Tick",
                table: "ZoneEvents",
                columns: new[] { "PlayerId", "Tick" });

            migrationBuilder.CreateIndex(
                name: "IX_ZoneEvents_RoundId_ZoneType",
                table: "ZoneEvents",
                columns: new[] { "RoundId", "ZoneType" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdvancedPlayerStats");

            migrationBuilder.DropTable(
                name: "AdvancedUserMessages");

            migrationBuilder.DropTable(
                name: "Bombs");

            migrationBuilder.DropTable(
                name: "BulletImpacts");

            migrationBuilder.DropTable(
                name: "ChatMessages");

            migrationBuilder.DropTable(
                name: "CommunicationPatterns");

            migrationBuilder.DropTable(
                name: "Damages");

            migrationBuilder.DropTable(
                name: "DroppedItems");

            migrationBuilder.DropTable(
                name: "EconomyEvents");

            migrationBuilder.DropTable(
                name: "EconomyStates");

            migrationBuilder.DropTable(
                name: "EntityEffects");

            migrationBuilder.DropTable(
                name: "EntityInteractions");

            migrationBuilder.DropTable(
                name: "EntityLifecycles");

            migrationBuilder.DropTable(
                name: "EntityPropertyChanges");

            migrationBuilder.DropTable(
                name: "EntityVisibilities");

            migrationBuilder.DropTable(
                name: "Equipment");

            migrationBuilder.DropTable(
                name: "FireAreas");

            migrationBuilder.DropTable(
                name: "FlashEvents");

            migrationBuilder.DropTable(
                name: "GameEvents");

            migrationBuilder.DropTable(
                name: "Grenades");

            migrationBuilder.DropTable(
                name: "GrenadeTrajectories");

            migrationBuilder.DropTable(
                name: "HostageEvents");

            migrationBuilder.DropTable(
                name: "InfernoEvents");

            migrationBuilder.DropTable(
                name: "Kills");

            migrationBuilder.DropTable(
                name: "MapControls");

            migrationBuilder.DropTable(
                name: "PerformanceMetrics");

            migrationBuilder.DropTable(
                name: "PlayerBehaviorEvents");

            migrationBuilder.DropTable(
                name: "PlayerMatchStats");

            migrationBuilder.DropTable(
                name: "PlayerMovements");

            migrationBuilder.DropTable(
                name: "PlayerPositions");

            migrationBuilder.DropTable(
                name: "PlayerRoundStats");

            migrationBuilder.DropTable(
                name: "RadioCommands");

            migrationBuilder.DropTable(
                name: "RoundImpacts");

            migrationBuilder.DropTable(
                name: "SmokeClouds");

            migrationBuilder.DropTable(
                name: "TacticalEvents");

            migrationBuilder.DropTable(
                name: "TeamStates");

            migrationBuilder.DropTable(
                name: "TemporaryEntities");

            migrationBuilder.DropTable(
                name: "VoiceCommunications");

            migrationBuilder.DropTable(
                name: "WeaponFires");

            migrationBuilder.DropTable(
                name: "WeaponStates");

            migrationBuilder.DropTable(
                name: "ZoneEvents");

            migrationBuilder.DropTable(
                name: "Players");

            migrationBuilder.DropTable(
                name: "Rounds");

            migrationBuilder.DropTable(
                name: "Matches");

            migrationBuilder.DropTable(
                name: "DemoFiles");
        }
    }
}
