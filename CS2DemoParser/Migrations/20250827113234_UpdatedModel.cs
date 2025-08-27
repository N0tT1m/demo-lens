using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CS2DemoParser.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EnhancedPlayerPositions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DemoFileId = table.Column<int>(type: "int", nullable: false),
                    RoundId = table.Column<int>(type: "int", nullable: false),
                    PlayerId = table.Column<int>(type: "int", nullable: false),
                    Tick = table.Column<int>(type: "int", nullable: false),
                    GameTime = table.Column<float>(type: "real", nullable: false),
                    PositionX = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PositionY = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PositionZ = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ViewAngleX = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ViewAngleY = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    VelocityX = table.Column<float>(type: "real", nullable: false),
                    VelocityY = table.Column<float>(type: "real", nullable: false),
                    VelocityZ = table.Column<float>(type: "real", nullable: false),
                    Speed = table.Column<float>(type: "real", nullable: false),
                    Health = table.Column<int>(type: "int", nullable: false),
                    Armor = table.Column<int>(type: "int", nullable: false),
                    HasHelmet = table.Column<bool>(type: "bit", nullable: false),
                    IsAlive = table.Column<bool>(type: "bit", nullable: false),
                    IsDefusing = table.Column<bool>(type: "bit", nullable: false),
                    IsPlanting = table.Column<bool>(type: "bit", nullable: false),
                    IsReloading = table.Column<bool>(type: "bit", nullable: false),
                    IsScoped = table.Column<bool>(type: "bit", nullable: false),
                    IsWalking = table.Column<bool>(type: "bit", nullable: false),
                    IsDucking = table.Column<bool>(type: "bit", nullable: false),
                    IsBlinded = table.Column<bool>(type: "bit", nullable: false),
                    ActiveWeapon = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ActiveWeaponClass = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AmmoClip = table.Column<int>(type: "int", nullable: false),
                    AmmoReserve = table.Column<int>(type: "int", nullable: false),
                    Money = table.Column<int>(type: "int", nullable: false),
                    EquipmentValue = table.Column<int>(type: "int", nullable: false),
                    MapArea = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PositionType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DistanceToNearestEnemy = table.Column<float>(type: "real", nullable: false),
                    DistanceToNearestTeammate = table.Column<float>(type: "real", nullable: false),
                    VisibleEnemies = table.Column<int>(type: "int", nullable: false),
                    VisibleTeammates = table.Column<int>(type: "int", nullable: false),
                    IsInSmokeArea = table.Column<bool>(type: "bit", nullable: false),
                    IsInFlashArea = table.Column<bool>(type: "bit", nullable: false),
                    IsInFireArea = table.Column<bool>(type: "bit", nullable: false),
                    HasLineOfSightToBomb = table.Column<bool>(type: "bit", nullable: false),
                    MovementAcceleration = table.Column<float>(type: "real", nullable: false),
                    ViewAngleChangeRate = table.Column<float>(type: "real", nullable: false),
                    IsCounterStrafing = table.Column<bool>(type: "bit", nullable: false),
                    IsPeeking = table.Column<bool>(type: "bit", nullable: false),
                    IsWithTeammates = table.Column<bool>(type: "bit", nullable: false),
                    TeammatesNearby = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnhancedPlayerPositions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EnhancedPlayerPositions_DemoFiles_DemoFileId",
                        column: x => x.DemoFileId,
                        principalTable: "DemoFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EnhancedPlayerPositions_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EnhancedPlayerPositions_Rounds_RoundId",
                        column: x => x.RoundId,
                        principalTable: "Rounds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlayerInputs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DemoFileId = table.Column<int>(type: "int", nullable: false),
                    RoundId = table.Column<int>(type: "int", nullable: false),
                    PlayerId = table.Column<int>(type: "int", nullable: false),
                    Tick = table.Column<int>(type: "int", nullable: false),
                    GameTime = table.Column<float>(type: "real", nullable: false),
                    Forward = table.Column<bool>(type: "bit", nullable: false),
                    Backward = table.Column<bool>(type: "bit", nullable: false),
                    Left = table.Column<bool>(type: "bit", nullable: false),
                    Right = table.Column<bool>(type: "bit", nullable: false),
                    Jump = table.Column<bool>(type: "bit", nullable: false),
                    Duck = table.Column<bool>(type: "bit", nullable: false),
                    Attack = table.Column<bool>(type: "bit", nullable: false),
                    Attack2 = table.Column<bool>(type: "bit", nullable: false),
                    Reload = table.Column<bool>(type: "bit", nullable: false),
                    Use = table.Column<bool>(type: "bit", nullable: false),
                    Walk = table.Column<bool>(type: "bit", nullable: false),
                    Speed = table.Column<bool>(type: "bit", nullable: false),
                    PositionX = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PositionY = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PositionZ = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ViewAngleX = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ViewAngleY = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Velocity = table.Column<float>(type: "real", nullable: false),
                    VelocityX = table.Column<float>(type: "real", nullable: false),
                    VelocityY = table.Column<float>(type: "real", nullable: false),
                    VelocityZ = table.Column<float>(type: "real", nullable: false),
                    IsCounterStrafing = table.Column<bool>(type: "bit", nullable: false),
                    IsPeeking = table.Column<bool>(type: "bit", nullable: false),
                    IsJigglePeeking = table.Column<bool>(type: "bit", nullable: false),
                    IsBhopping = table.Column<bool>(type: "bit", nullable: false),
                    MovementType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerInputs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerInputs_DemoFiles_DemoFileId",
                        column: x => x.DemoFileId,
                        principalTable: "DemoFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlayerInputs_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlayerInputs_Rounds_RoundId",
                        column: x => x.RoundId,
                        principalTable: "Rounds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WeaponStateChanges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DemoFileId = table.Column<int>(type: "int", nullable: false),
                    RoundId = table.Column<int>(type: "int", nullable: false),
                    PlayerId = table.Column<int>(type: "int", nullable: false),
                    Tick = table.Column<int>(type: "int", nullable: false),
                    GameTime = table.Column<float>(type: "real", nullable: false),
                    WeaponName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    WeaponClass = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EventType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AmmoClip = table.Column<int>(type: "int", nullable: false),
                    AmmoReserve = table.Column<int>(type: "int", nullable: false),
                    IsReloading = table.Column<bool>(type: "bit", nullable: false),
                    IsZoomed = table.Column<bool>(type: "bit", nullable: false),
                    ZoomLevel = table.Column<float>(type: "real", nullable: false),
                    PositionX = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PositionY = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PositionZ = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ViewAngleX = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ViewAngleY = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PreviousWeapon = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SwitchTime = table.Column<float>(type: "real", nullable: false),
                    WeaponItemId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    OriginalOwnerSteamId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsDropped = table.Column<bool>(type: "bit", nullable: false),
                    IsThrown = table.Column<bool>(type: "bit", nullable: false),
                    ShotsFiredSinceLastEvent = table.Column<int>(type: "int", nullable: false),
                    AccuracySinceLastEvent = table.Column<float>(type: "real", nullable: false),
                    WasKillShot = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeaponStateChanges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WeaponStateChanges_DemoFiles_DemoFileId",
                        column: x => x.DemoFileId,
                        principalTable: "DemoFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WeaponStateChanges_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WeaponStateChanges_Rounds_RoundId",
                        column: x => x.RoundId,
                        principalTable: "Rounds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EnhancedPlayerPositions_DemoFileId",
                table: "EnhancedPlayerPositions",
                column: "DemoFileId");

            migrationBuilder.CreateIndex(
                name: "IX_EnhancedPlayerPositions_PlayerId",
                table: "EnhancedPlayerPositions",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_EnhancedPlayerPositions_RoundId",
                table: "EnhancedPlayerPositions",
                column: "RoundId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerInputs_DemoFileId",
                table: "PlayerInputs",
                column: "DemoFileId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerInputs_PlayerId",
                table: "PlayerInputs",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerInputs_RoundId",
                table: "PlayerInputs",
                column: "RoundId");

            migrationBuilder.CreateIndex(
                name: "IX_WeaponStateChanges_DemoFileId",
                table: "WeaponStateChanges",
                column: "DemoFileId");

            migrationBuilder.CreateIndex(
                name: "IX_WeaponStateChanges_PlayerId",
                table: "WeaponStateChanges",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_WeaponStateChanges_RoundId",
                table: "WeaponStateChanges",
                column: "RoundId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EnhancedPlayerPositions");

            migrationBuilder.DropTable(
                name: "PlayerInputs");

            migrationBuilder.DropTable(
                name: "WeaponStateChanges");
        }
    }
}
