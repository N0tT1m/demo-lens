using DemoFile;
using DemoFile.Game.Cs;
using DemoFile.Sdk;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using CS2DemoParser.Data;
using CS2DemoParser.Models;
using System.Collections.Concurrent;

namespace CS2DemoParser.Services;

public class CorrectedDemoParserService
{
    private readonly CS2DemoContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CorrectedDemoParserService> _logger;
    private readonly ConcurrentDictionary<int, Models.Player> _players = new();
    private readonly ConcurrentBag<Models.Kill> _kills = new();
    private readonly ConcurrentBag<Models.Damage> _damages = new();
    private readonly ConcurrentBag<Models.WeaponFire> _weaponFires = new();
    private readonly ConcurrentBag<Models.Grenade> _grenades = new();
    private readonly ConcurrentBag<Models.Bomb> _bombs = new();
    private readonly ConcurrentBag<Models.PlayerPosition> _playerPositions = new();
    private readonly ConcurrentBag<Models.ChatMessage> _chatMessages = new();
    private readonly ConcurrentBag<Models.Equipment> _equipment = new();
    private readonly ConcurrentBag<Models.GameEvent> _gameEvents = new();
    private readonly ConcurrentBag<Models.Round> _rounds = new();
    private readonly ConcurrentBag<Models.PlayerMatchStats> _playerMatchStats = new();
    private readonly ConcurrentBag<Models.PlayerRoundStats> _playerRoundStats = new();
    
    // New comprehensive collections
    private readonly ConcurrentBag<Models.GrenadeTrajectory> _grenadeTrajectories = new();
    private readonly ConcurrentBag<Models.EconomyEvent> _economyEvents = new();
    private readonly ConcurrentBag<Models.BulletImpact> _bulletImpacts = new();
    private readonly ConcurrentBag<Models.PlayerMovement> _playerMovements = new();
    private readonly ConcurrentBag<Models.ZoneEvent> _zoneEvents = new();
    private readonly ConcurrentBag<Models.RadioCommand> _radioCommands = new();
    private readonly ConcurrentBag<Models.WeaponState> _weaponStates = new();
    private readonly ConcurrentBag<Models.FlashEvent> _flashEvents = new();
    
    // Enhanced analytics collections for advanced gameplay insights
    private readonly ConcurrentBag<Models.PlayerInput> _playerInputs = new();
    private readonly ConcurrentBag<Models.WeaponStateChange> _weaponStateChanges = new();
    private readonly ConcurrentBag<Models.EnhancedPlayerPosition> _enhancedPlayerPositions = new();
    
    // Advanced entity tracking collections
    private readonly ConcurrentBag<Models.EntityLifecycle> _entityLifecycles = new();
    private readonly ConcurrentBag<Models.EntityInteraction> _entityInteractions = new();
    private readonly ConcurrentBag<Models.EntityVisibility> _entityVisibilities = new();
    private readonly ConcurrentBag<Models.EntityEffect> _entityEffects = new();
    private readonly ConcurrentBag<Models.DroppedItem> _droppedItems = new();
    private readonly ConcurrentBag<Models.SmokeCloud> _smokeClouds = new();
    private readonly ConcurrentBag<Models.FireArea> _fireAreas = new();
    
    // Player money tracking for economy events
    private readonly ConcurrentDictionary<int, int> _playerMoneyTracking = new();
    
    // Game state tracking collections
    private readonly ConcurrentBag<Models.TeamState> _teamStates = new();
    private readonly ConcurrentBag<Models.EconomyState> _economyStates = new();
    private readonly ConcurrentBag<Models.MapControl> _mapControls = new();
    private readonly ConcurrentBag<Models.TacticalEvent> _tacticalEvents = new();
    
    // Advanced statistics collections
    private readonly ConcurrentBag<Models.AdvancedPlayerStats> _advancedPlayerStats = new();
    private readonly ConcurrentBag<Models.PerformanceMetric> _performanceMetrics = new();
    private readonly ConcurrentBag<Models.RoundImpact> _roundImpacts = new();
    
    // Voice and communication collections
    private readonly ConcurrentBag<Models.VoiceCommunication> _voiceCommunications = new();
    private readonly ConcurrentBag<Models.CommunicationPattern> _communicationPatterns = new();
    
    // Advanced event tracking collections
    private readonly ConcurrentBag<Models.TemporaryEntity> _temporaryEntities = new();
    private readonly ConcurrentBag<Models.EntityPropertyChange> _entityPropertyChanges = new();
    private readonly ConcurrentBag<Models.HostageEvent> _hostageEvents = new();
    private readonly ConcurrentBag<Models.AdvancedUserMessage> _advancedUserMessages = new();
    private readonly ConcurrentBag<Models.PlayerBehaviorEvent> _playerBehaviorEvents = new();
    private readonly ConcurrentBag<Models.InfernoEvent> _infernoEvents = new();

    private Models.DemoFile? _currentDemoFile;
    private Models.Match? _currentMatch;
    private Models.Round? _currentRound;
    private int _currentRoundNumber = 0;
    private string? _demoSource;
    
    private int GetDisplayRoundNumber()
    {
        return (_demoSource == "esea" || _demoSource == "faceit") ? _currentRoundNumber - 2 : _currentRoundNumber;
    }
    
    private bool ShouldSkipRound()
    {
        return (_demoSource == "esea" || _demoSource == "faceit") && _currentRoundNumber <= 2;
    }
    private readonly Dictionary<int, Models.PlayerRoundStats> _currentRoundStats = new();
    private CsDemoParser? _demo; // Store demo reference

    public CorrectedDemoParserService(CS2DemoContext context, IConfiguration configuration, ILogger<CorrectedDemoParserService> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> ParseDemoAsync(string filePath, string? demoSource = null, string? mapName = null)
    {
        try
        {
            _logger.LogInformation("Starting to parse demo file: {FilePath}", filePath);

            var fileInfo = new FileInfo(filePath);
            if (!fileInfo.Exists)
            {
                _logger.LogError("Demo file not found: {FilePath}", filePath);
                return false;
            }

            await using var fileStream = fileInfo.OpenRead();
            _demo = new CsDemoParser();

            await CreateDemoFileRecord(fileInfo, demoSource, mapName);
            SetupEventHandlers(_demo);

            _logger.LogInformation("Parsing demo file...");
            var reader = DemoFileReader.Create(_demo, fileStream);
            
            await reader.ReadAllAsync();

            await SaveDataToDatabase();

            _logger.LogInformation("Successfully parsed demo file: {FilePath}", filePath);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing demo file: {FilePath}. File size: {FileSize} bytes. Error type: {ErrorType}", 
                filePath, 
                System.IO.File.Exists(filePath) ? new FileInfo(filePath).Length : 0, 
                ex.GetType().Name);
            
            // Log additional context for Azure debugging
            try
            {
                var availableMemory = GC.GetTotalMemory(false);
                var workingSet = Environment.WorkingSet;
                _logger.LogError("Memory diagnostics - GC Memory: {GCMemory} bytes, Working Set: {WorkingSet} bytes", 
                    availableMemory, workingSet);
            }
            catch
            {
                // Ignore memory diagnostic errors
            }
            
            return false;
        }
    }

    private async Task CreateDemoFileRecord(FileInfo fileInfo, string? demoSource = null, string? mapName = null)
    {
        _demoSource = demoSource;
        _currentDemoFile = new Models.DemoFile
        {
            FileName = fileInfo.Name,
            FilePath = fileInfo.FullName,
            FileSize = fileInfo.Length,
            CreatedAt = fileInfo.CreationTime,
            ParsedAt = DateTime.UtcNow,
            MapName = mapName ?? _demo.ServerInfo?.MapName ?? ExtractMapFromFilename(fileInfo.Name) ?? "Unknown",
            DemoSource = demoSource
        };

        _context.DemoFiles.Add(_currentDemoFile);
        await _context.SaveChangesAsync();
    }

    private void SetupEventHandlers(CsDemoParser demo)
    {
        // Basic game events
        demo.Source1GameEvents.RoundStart += OnRoundStart;
        demo.Source1GameEvents.RoundEnd += OnRoundEnd;
        demo.Source1GameEvents.PlayerDeath += OnPlayerDeath;
        demo.Source1GameEvents.PlayerHurt += OnPlayerHurt;
        demo.Source1GameEvents.WeaponFire += OnWeaponFire;
        
        // Grenade events
        demo.Source1GameEvents.HegrenadeDetonate += OnHegrenadeDetonate;
        demo.Source1GameEvents.FlashbangDetonate += OnFlashbangDetonate;
        demo.Source1GameEvents.SmokegrenadeDetonate += OnSmokegrenadeDetonate;
        
        // Core bomb events
        demo.Source1GameEvents.BombPlanted += OnBombPlanted;
        demo.Source1GameEvents.BombDefused += OnBombDefused;
        demo.Source1GameEvents.BombExploded += OnBombExploded;
        
        // Core player events
        demo.Source1GameEvents.PlayerConnect += OnPlayerConnect;
        demo.Source1GameEvents.PlayerDisconnect += OnPlayerDisconnect;
        demo.Source1GameEvents.PlayerTeam += OnPlayerTeam;

        // Additional bomb events
        demo.Source1GameEvents.BombBeginplant += OnBombBeginPlant;
        demo.Source1GameEvents.BombAbortplant += OnBombAbortPlant;
        demo.Source1GameEvents.BombBegindefuse += OnBombBeginDefuse;
        demo.Source1GameEvents.BombAbortdefuse += OnBombAbortDefuse;
        demo.Source1GameEvents.BombDropped += OnBombDropped;
        demo.Source1GameEvents.BombPickup += OnBombPickup;

        // Zone events
        demo.Source1GameEvents.EnterBombzone += OnEnterBombZone;
        demo.Source1GameEvents.ExitBombzone += OnExitBombZone;
        demo.Source1GameEvents.EnterBuyzone += OnEnterBuyZone;
        demo.Source1GameEvents.ExitBuyzone += OnExitBuyZone;

        // Weapon events
        demo.Source1GameEvents.WeaponReload += OnWeaponReload;
        demo.Source1GameEvents.WeaponZoom += OnWeaponZoom;

        // Item events
        demo.Source1GameEvents.ItemPurchase += OnItemPurchase;
        demo.Source1GameEvents.ItemPickup += OnItemPickup;
        demo.Source1GameEvents.ItemEquip += OnItemEquip;

        // Chat events
        demo.Source1GameEvents.PlayerChat += OnPlayerChat;
        
        // Hostage events
        demo.Source1GameEvents.HostageFollows += OnHostageFollows;
        demo.Source1GameEvents.HostageHurt += OnHostageHurt;
        demo.Source1GameEvents.HostageKilled += OnHostageKilled;
        demo.Source1GameEvents.HostageRescued += OnHostageRescued;
        demo.Source1GameEvents.HostageStopsFollowing += OnHostageStopsFollowing;
        demo.Source1GameEvents.HostageRescuedAll += OnHostageRescuedAll;
        demo.Source1GameEvents.HostageCallForHelp += OnHostageCallForHelp;
        
        // Additional player events
        demo.Source1GameEvents.PlayerJump += OnPlayerJump;
        demo.Source1GameEvents.PlayerBlind += OnPlayerBlind;
        demo.Source1GameEvents.PlayerFootstep += OnPlayerFootstep;
        demo.Source1GameEvents.PlayerSpawn += OnPlayerSpawn;
        demo.Source1GameEvents.PlayerFalldamage += OnPlayerFallDamage;
        demo.Source1GameEvents.PlayerChangename += OnPlayerChangeName;
        demo.Source1GameEvents.PlayerScore += OnPlayerScore;
        demo.Source1GameEvents.SwitchTeam += OnSwitchTeam;
        
        // Additional weapon events
        demo.Source1GameEvents.WeaponFireOnEmpty += OnWeaponFireOnEmpty;
        demo.Source1GameEvents.InspectWeapon += OnInspectWeapon;
        demo.Source1GameEvents.SilencerDetach += OnSilencerDetach;
        demo.Source1GameEvents.SilencerOff += OnSilencerOff;
        demo.Source1GameEvents.SilencerOn += OnSilencerOn;
        
        // Grenade throw and bounce events
        demo.Source1GameEvents.GrenadeThrown += OnGrenadeThrown;
        demo.Source1GameEvents.GrenadeBounce += OnGrenadeBounce;
        
        // Decoy events
        demo.Source1GameEvents.DecoyDetonate += OnDecoyDetonate;
        demo.Source1GameEvents.DecoyStarted += OnDecoyStarted;
        demo.Source1GameEvents.DecoyFiring += OnDecoyFiring;
        
        // Inferno/fire events
        demo.Source1GameEvents.InfernoStartburn += OnInfernoStartBurn;
        demo.Source1GameEvents.InfernoExpire += OnInfernoExpire;
        demo.Source1GameEvents.InfernoExtinguish += OnInfernoExtinguish;
        demo.Source1GameEvents.MolotovDetonate += OnMolotovDetonate;

        // Note: TickEnd event doesn't exist in this demo parser library
        // Position tracking is handled via other frequent events
        
        // Smoke events
        demo.Source1GameEvents.SmokegrenadeExpired += OnSmokegrenadeExpired;
        
        // Additional round events
        demo.Source1GameEvents.RoundFreezeEnd += OnRoundFreezeEnd;
        demo.Source1GameEvents.RoundMvp += OnRoundMvp;
        demo.Source1GameEvents.RoundTimeWarning += OnRoundTimeWarning;
        demo.Source1GameEvents.RoundOfficiallyEnded += OnRoundOfficiallyEnded;
        demo.Source1GameEvents.RoundAnnounceMatchPoint += OnRoundAnnounceMatchPoint;
        demo.Source1GameEvents.RoundAnnounceFinal += OnRoundAnnounceFinal;
        demo.Source1GameEvents.RoundAnnounceLastRoundHalf += OnRoundAnnounceLastRoundHalf;
        demo.Source1GameEvents.RoundAnnounceMatchStart += OnRoundAnnounceMatchStart;
        demo.Source1GameEvents.RoundAnnounceWarmup += OnRoundAnnounceWarmup;
        
        // Additional item events
        demo.Source1GameEvents.ItemPickupSlerp += OnItemPickupSlerp;
        demo.Source1GameEvents.ItemPickupFailed += OnItemPickupFailed;
        demo.Source1GameEvents.ItemRemove += OnItemRemove;
        demo.Source1GameEvents.AmmoPickup += OnAmmoPickup;
        demo.Source1GameEvents.DefuserDropped += OnDefuserDropped;
        demo.Source1GameEvents.DefuserPickup += OnDefuserPickup;
        
        // Zone events
        demo.Source1GameEvents.EnterRescueZone += OnEnterRescueZone;
        demo.Source1GameEvents.ExitRescueZone += OnExitRescueZone;
        demo.Source1GameEvents.BuytimeEnded += OnBuyTimeEnded;
        
        // Communication events
        demo.Source1GameEvents.PlayerRadio += OnPlayerRadio;
        demo.Source1GameEvents.TeamplayBroadcastAudio += OnTeamplayBroadcastAudio;
        
        // Vote events
        demo.Source1GameEvents.VoteStarted += OnVoteStarted;
        demo.Source1GameEvents.VoteFailed += OnVoteFailed;
        demo.Source1GameEvents.VotePassed += OnVotePassed;
        demo.Source1GameEvents.VoteChanged += OnVoteChanged;
        demo.Source1GameEvents.VoteCastYes += OnVoteCastYes;
        demo.Source1GameEvents.VoteCastNo += OnVoteCastNo;
        
        // Game state events
        demo.Source1GameEvents.GameStart += OnGameStart;
        demo.Source1GameEvents.GameEnd += OnGameEnd;
        demo.Source1GameEvents.WarmupEnd += OnWarmupEnd;
        demo.Source1GameEvents.BeginNewMatch += OnBeginNewMatch;
        demo.Source1GameEvents.StartHalftime += OnStartHalftime;
        
        // Server events
        demo.Source1GameEvents.ServerSpawn += OnServerSpawn;
        demo.Source1GameEvents.ServerMessage += OnServerMessage;
        demo.Source1GameEvents.ServerCvar += OnServerCvar;
        
        // Environmental events
        demo.Source1GameEvents.DoorOpen += OnDoorOpen;
        demo.Source1GameEvents.DoorClose += OnDoorClose;
        demo.Source1GameEvents.BreakBreakable += OnBreakBreakable;
        demo.Source1GameEvents.BreakProp += OnBreakProp;
        
        // Special events
        demo.Source1GameEvents.PlayerAvengedTeammate += OnPlayerAvengedTeammate;
        demo.Source1GameEvents.BotTakeover += OnBotTakeover;
        demo.Source1GameEvents.PlayerGivenC4 += OnPlayerGivenC4;
        
        // Set up tick-based position tracking
        if (_configuration.GetValue<bool>("ParserSettings:ParsePlayerPositions", true))
        {
            // Position tracking will be handled in the main parsing loop
        }
        
        // Enhanced: Set up entity tracking for comprehensive state monitoring
        // Note: Entity callbacks may need API adjustments based on demofile-net version
        /*
        try
        {
            // Player pawn tracking for real-time state changes
            _demo.EntityEvents.CCSPlayerPawn.AddChangeCallback(
                pawn => new { pawn.Origin, pawn.EyeAngles, pawn.Health, pawn.ArmorValue, pawn.Velocity },
                OnPlayerStateChanged
            );
            
            // Weapon state tracking for reload patterns, ammunition usage
            _demo.EntityEvents.CCSWeaponBase.AddChangeCallback(
                weapon => new { weapon.OwnerEntity, weapon.Clip1 },
                OnWeaponStateChanged
            );
            
            // Grenade trajectory tracking
            _demo.EntityEvents.CBaseCSGrenadeProjectile.Create += OnGrenadeEntityCreated;
            _demo.EntityEvents.CBaseCSGrenadeProjectile.Delete += OnGrenadeEntityDestroyed;
            
            // Economic item tracking
            _demo.EntityEvents.CEconEntity.Create += OnEconomicItemCreated;
            _demo.EntityEvents.CEconEntity.Delete += OnEconomicItemDestroyed;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Some entity tracking features may not be available in this demo version");
        }
        */
        
        // Critical missing Source1 game events
        demo.Source1GameEvents.BulletDamage += OnBulletDamage;
        demo.Source1GameEvents.BulletImpact += OnBulletImpact;
        demo.Source1GameEvents.PlayerShoot += OnPlayerShoot;
        demo.Source1GameEvents.PlayerSound += OnPlayerSound;
        demo.Source1GameEvents.PlayerPing += OnPlayerPing;
        demo.Source1GameEvents.PlayerPingStop += OnPlayerPingStop;
        demo.Source1GameEvents.WeaponZoomRifle += OnWeaponZoomRifle;
        demo.Source1GameEvents.WeaponhudSelection += OnWeaponHudSelection;
        
        // Game state events
        demo.Source1GameEvents.GameInit += OnGameInit;
        demo.Source1GameEvents.GamePhaseChanged += OnGamePhaseChanged;
        demo.Source1GameEvents.CsIntermission += OnCsIntermission;
        demo.Source1GameEvents.CsMatchEndRestart += OnCsMatchEndRestart;
        demo.Source1GameEvents.CsPreRestart += OnCsPreRestart;
        demo.Source1GameEvents.CsRoundFinalBeep += OnCsRoundFinalBeep;
        demo.Source1GameEvents.CsRoundStartBeep += OnCsRoundStartBeep;
        demo.Source1GameEvents.CsWinPanelMatch += OnCsWinPanelMatch;
        demo.Source1GameEvents.CsWinPanelRound += OnCsWinPanelRound;
        
        // Advanced combat events
        demo.Source1GameEvents.TagrenadeDetonate += OnTagrenadeDetonate;
        demo.Source1GameEvents.OtherDeath += OnOtherDeath;
        
        // Economic events
        demo.Source1GameEvents.BuymenuOpen += OnBuymenuOpen;
        demo.Source1GameEvents.BuymenuClose += OnBuymenuClose;
        
        // Additional weapon events
        demo.Source1GameEvents.WeaponFireOnEmpty += OnWeaponFireOnEmpty;
        
        // Map events
        demo.Source1GameEvents.PlayerActivate += OnPlayerActivate;
        
        // TODO: ALL REMAINING Source1 Game Events (100+ additional events) - Temporarily commented out until handlers are implemented
        /*
        // Environmental Events
        demo.Source1GameEvents.DoorMoving += OnDoorMoving;
        demo.Source1GameEvents.DoorClosed += OnDoorClosed;
        demo.Source1GameEvents.BrokenBreakable += OnBrokenBreakable;
        
        // HLTV Events
        demo.Source1GameEvents.HltvCameraman += OnHltvCameraman;
        demo.Source1GameEvents.HltvChase += OnHltvChase;
        demo.Source1GameEvents.HltvRankCamera += OnHltvRankCamera;
        demo.Source1GameEvents.HltvRankEntity += OnHltvRankEntity;
        demo.Source1GameEvents.HltvFixed += OnHltvFixed;
        demo.Source1GameEvents.HltvMessage += OnHltvMessage;
        demo.Source1GameEvents.HltvStatus += OnHltvStatus;
        demo.Source1GameEvents.HltvTitle += OnHltvTitle;
        demo.Source1GameEvents.HltvChat += OnHltvChat;
        demo.Source1GameEvents.HltvVersioninfo += OnHltvVersioninfo;
        demo.Source1GameEvents.HltvReplay += OnHltvReplay;
        demo.Source1GameEvents.HltvReplayStatus += OnHltvReplayStatus;
        demo.Source1GameEvents.HltvChangedMode += OnHltvChangedMode;
        
        // Achievement and XP Events
        demo.Source1GameEvents.AchievementEvent += OnAchievementEvent;
        demo.Source1GameEvents.AchievementEarned += OnAchievementEarned;
        demo.Source1GameEvents.AchievementWriteFailed += OnAchievementWriteFailed;
        demo.Source1GameEvents.EntityKilled += OnEntityKilled;
        
        // Additional Valid Weapon Events
        demo.Source1GameEvents.WeaponReload += OnWeaponReload;
        demo.Source1GameEvents.WeaponZoom += OnWeaponZoom;
        demo.Source1GameEvents.SilencerDetach += OnSilencerDetach;
        demo.Source1GameEvents.InspectWeapon += OnInspectWeapon;
        demo.Source1GameEvents.WeaponZoomRifle += OnWeaponZoomRifle;
        
        // Navigation and Movement
        demo.Source1GameEvents.NavBlocked += OnNavBlocked;
        demo.Source1GameEvents.NavGenerate += OnNavGenerate;
        
        // Physics Events
        demo.Source1GameEvents.PhysgunPickup += OnPhysgunPickup;
        demo.Source1GameEvents.FlareIgniteNpc += OnFlareIgniteNpc;
        demo.Source1GameEvents.HelicopterGrenadePuntMiss += OnHelicopterGrenadePuntMiss;
        
        // Game Instructor Events
        demo.Source1GameEvents.InstructorServerHintCreate += OnInstructorServerHintCreate;
        demo.Source1GameEvents.InstructorServerHintStop += OnInstructorServerHintStop;
        
        // Player Events
        demo.Source1GameEvents.PlayerStatsUpdated += OnPlayerStatsUpdated;
        
        // Additional Game State Events
        demo.Source1GameEvents.TeamInfo += OnTeamInfo;
        demo.Source1GameEvents.TeamScore += OnTeamScore;
        demo.Source1GameEvents.TeamplayRoundStart += OnTeamplayRoundStart;
        
        // Entity Interaction Events
        demo.Source1GameEvents.EntityVisible += OnEntityVisible;
        
        // Survival/Danger Zone Events  
        demo.Source1GameEvents.SurvivalAnnouncePhase += OnSurvivalAnnouncePhase;
        demo.Source1GameEvents.ParachutePickup += OnParachutePickup;
        demo.Source1GameEvents.ParachuteDeploy += OnParachuteDeploy;
        demo.Source1GameEvents.DronegunAttack += OnDronegunAttack;
        demo.Source1GameEvents.DroneDispatched += OnDroneDispatched;
        demo.Source1GameEvents.LootCrateVisible += OnLootCrateVisible;
        demo.Source1GameEvents.LootCrateOpened += OnLootCrateOpened;
        demo.Source1GameEvents.SurvivalTeammateRespawn += OnSurvivalTeammateRespawn;
        */
        
        // Comprehensive User Message Events - Temporarily disabled due to API compatibility
        /* 
        demo.UserMessageEvents.VguiMenu += OnVguiMenuUserMessage;
        demo.UserMessageEvents.Geiger += OnGeigerUserMessage;
        demo.UserMessageEvents.Train += OnTrainUserMessage;
        demo.UserMessageEvents.HudText += OnHudTextUserMessage;
        demo.UserMessageEvents.HudMsg += OnHudMsgUserMessage;
        demo.UserMessageEvents.ResetHud += OnResetHudUserMessage;
        demo.UserMessageEvents.GameTitle += OnGameTitleUserMessage;
        demo.UserMessageEvents.Shake += OnShakeUserMessage;
        demo.UserMessageEvents.Fade += OnFadeUserMessage;
        demo.UserMessageEvents.Rumble += OnRumbleUserMessage;
        demo.UserMessageEvents.CloseCaption += OnCloseCaptionUserMessage;
        demo.UserMessageEvents.CloseCaptionDirect += OnCloseCaptionDirectUserMessage;
        demo.UserMessageEvents.SendAudio += OnSendAudioUserMessage;
        demo.UserMessageEvents.RawAudio += OnRawAudioUserMessage;
        demo.UserMessageEvents.VoiceMask += OnVoiceMaskUserMessage;
        demo.UserMessageEvents.RequestState += OnRequestStateUserMessage;
        demo.UserMessageEvents.Damage += OnDamageUserMessage;
        demo.UserMessageEvents.RadioText += OnRadioTextUserMessage;
        demo.UserMessageEvents.HintText += OnHintTextUserMessage;
        demo.UserMessageEvents.KeyHintText += OnKeyHintTextUserMessage;
        demo.UserMessageEvents.ProcessSpottedEntityUpdate += OnProcessSpottedEntityUpdateUserMessage;
        demo.UserMessageEvents.ReloadEffect += OnReloadEffectUserMessage;
        demo.UserMessageEvents.AdjustMoney += OnAdjustMoneyUserMessage;
        demo.UserMessageEvents.StopSpectatorMode += OnStopSpectatorModeUserMessage;
        demo.UserMessageEvents.KillCam += OnKillCamUserMessage;
        demo.UserMessageEvents.DesiredTimescale += OnDesiredTimescaleUserMessage;
        demo.UserMessageEvents.CurrentTimescale += OnCurrentTimescaleUserMessage;
        demo.UserMessageEvents.AchievementEvent += OnAchievementEventUserMessage;
        demo.UserMessageEvents.MatchEndConditions += OnMatchEndConditionsUserMessage;
        demo.UserMessageEvents.DisconnectToLobby += OnDisconnectToLobbyUserMessage;
        demo.UserMessageEvents.PlayerStatsUpdate += OnPlayerStatsUpdateUserMessage;
        demo.UserMessageEvents.WarmupHasEnded += OnWarmupHasEndedUserMessage;
        demo.UserMessageEvents.ClientInfo += OnClientInfoUserMessage;
        demo.UserMessageEvents.XRankGet += OnXRankGetUserMessage;
        demo.UserMessageEvents.XRankUpd += OnXRankUpdUserMessage;
        demo.UserMessageEvents.CallVoteFailed += OnCallVoteFailedUserMessage;
        demo.UserMessageEvents.VoteStart += OnVoteStartUserMessage;
        demo.UserMessageEvents.VotePass += OnVotePassUserMessage;
        demo.UserMessageEvents.VoteFailed += OnVoteFailedUserMessage;
        demo.UserMessageEvents.VoteSetup += OnVoteSetupUserMessage;
        demo.UserMessageEvents.ServerRankRevealAll += OnServerRankRevealAllUserMessage;
        demo.UserMessageEvents.SendLastKillerDamageToClient += OnSendLastKillerDamageToClientUserMessage;
        demo.UserMessageEvents.ServerRankUpdate += OnServerRankUpdateUserMessage;
        demo.UserMessageEvents.ItemPickup += OnItemPickupUserMessage;
        demo.UserMessageEvents.ShowMenu += OnShowMenuUserMessage;
        demo.UserMessageEvents.BarTime += OnBarTimeUserMessage;
        demo.UserMessageEvents.AmmoDenied += OnAmmoDeniedUserMessage;
        demo.UserMessageEvents.MarkAchievement += OnMarkAchievementUserMessage;
        demo.UserMessageEvents.MatchStatsUpdate += OnMatchStatsUpdateUserMessage;
        demo.UserMessageEvents.ItemDrop += OnItemDropUserMessage;
        demo.UserMessageEvents.GlowPropTurnOff += OnGlowPropTurnOffUserMessage;
        demo.UserMessageEvents.SendPlayerItemDrops += OnSendPlayerItemDropsUserMessage;
        demo.UserMessageEvents.RoundBackupFilenames += OnRoundBackupFilenamesUserMessage;
        demo.UserMessageEvents.SendPlayerItemFound += OnSendPlayerItemFoundUserMessage;
        demo.UserMessageEvents.ReportHit += OnReportHitUserMessage;
        demo.UserMessageEvents.XpUpdate += OnXpUpdateUserMessage;
        demo.UserMessageEvents.QuestProgress += OnQuestProgressUserMessage;
        demo.UserMessageEvents.ScoreLeaderboardData += OnScoreLeaderboardDataUserMessage;
        demo.UserMessageEvents.PlayerDecalDigitalSignature += OnPlayerDecalDigitalSignatureUserMessage;
        demo.UserMessageEvents.WeaponSound += OnWeaponSoundUserMessage;
        demo.UserMessageEvents.UpdateScreenHealthBar += OnUpdateScreenHealthBarUserMessage;
        demo.UserMessageEvents.EntityOutlineHighlight += OnEntityOutlineHighlightUserMessage;
        demo.UserMessageEvents.Ssui += OnSsuiUserMessage;
        demo.UserMessageEvents.SurvivalStats += OnSurvivalStatsUserMessage;
        demo.UserMessageEvents.EndOfMatchAllPlayersData += OnEndOfMatchAllPlayersDataUserMessage;
        demo.UserMessageEvents.PostRoundDamageReport += OnPostRoundDamageReportUserMessage;
        demo.UserMessageEvents.RoundEndReportData += OnRoundEndReportDataUserMessage;
        demo.UserMessageEvents.CurrentRoundOdds += OnCurrentRoundOddsUserMessage;
        demo.UserMessageEvents.DeepStats += OnDeepStatsUserMessage;
        demo.UserMessageEvents.ShootInfo += OnShootInfoUserMessage;
        */
    }

    private void OnRoundStart(Source1RoundStartEvent e)
    {
        if (_demo == null || _currentDemoFile == null) return;

        _currentRoundNumber++;

        // Skip the first two rounds for ESEA/FACEIT demos (warmup and knife round)
        if (ShouldSkipRound())
        {
            return;
        }

        if (_currentMatch == null)
        {
            _currentMatch = new Models.Match
            {
                DemoFileId = _currentDemoFile.Id,
                MapName = _demo.ServerInfo?.MapName ?? ExtractMapFromFilename(_currentDemoFile.FileName) ?? "Unknown",
                StartTime = DateTime.UtcNow,
                IsFinished = false
            };

            _currentDemoFile.TotalTicks = _demo.CurrentDemoTick.Value;
            _currentDemoFile.TickRate = CsDemoParser.TickRate;
        }

        // Calculate display round number (start from 1 for ESEA/FACEIT after skipping first 2 rounds)
        var displayRoundNumber = GetDisplayRoundNumber();
        
        _currentRound = new Models.Round
        {
            DemoFileId = _currentDemoFile.Id,
            MatchId = _currentMatch?.Id ?? 0,
            RoundNumber = displayRoundNumber,
            StartTick = _demo.CurrentDemoTick.Value,
            StartTime = DateTime.UtcNow,
            CTScore = _demo.TeamCounterTerrorist?.Score ?? 0,
            TScore = _demo.TeamTerrorist?.Score ?? 0
        };

        _rounds.Add(_currentRound);
        _currentRoundStats.Clear();

        foreach (var player in _demo.Players)
        {
            if (player.PlayerName != null)
            {
                var playerId = (int)player.EntityIndex.Value;
                var currentMoney = player.InGameMoneyServices?.Account ?? 0;
                
                // Initialize or update player money tracking for economy events
                _playerMoneyTracking[playerId] = currentMoney;
                
                var roundStats = new Models.PlayerRoundStats
                {
                    PlayerId = playerId - 1, // Use PlayerSlot (EntityIndex - 1) temporarily
                    RoundId = displayRoundNumber, // Use display round number
                    StartMoney = currentMoney,
                    Health = player.PlayerPawn?.Health ?? 0,
                    Armor = player.PlayerPawn?.ArmorValue ?? 0,
                    HasHelmet = player.PawnHasHelmet,
                    HasDefuseKit = player.PawnHasDefuser,
                    IsAlive = (player.PlayerPawn?.Health ?? 0) > 0
                };

                _currentRoundStats[playerId] = roundStats;
                _playerRoundStats.Add(roundStats);
            }
        }

        LogGameEvent(_demo, "round_start", "Round started", true);

        // CRITICAL: Track player positions at round start to capture pre-round/spawn positions
        TrackPlayerPositions();
    }

    private void OnRoundEnd(Source1RoundEndEvent e)
    {
        if (_demo == null || _currentRound == null) return;

        // Skip the first two rounds for ESEA/FACEIT demos (warmup and knife round)
        if (ShouldSkipRound())
        {
            return;
        }

        _currentRound.EndTick = _demo.CurrentDemoTick.Value;
        _currentRound.EndTime = DateTime.UtcNow;
        _currentRound.Duration = (float)(DateTime.UtcNow - _currentRound.StartTime).TotalSeconds;
        _currentRound.WinnerTeam = e.Winner.ToString();
        _currentRound.EndReason = e.Reason.ToString();
        _currentRound.CTScore = _demo.TeamCounterTerrorist?.Score ?? 0;
        _currentRound.TScore = _demo.TeamTerrorist?.Score ?? 0;

        foreach (var player in _demo.Players)
        {
            if (player.PlayerName != null && _currentRoundStats.TryGetValue((int)player.EntityIndex.Value, out var roundStats))
            {
                roundStats.EndMoney = player.InGameMoneyServices?.Account ?? 0;
                roundStats.Health = player.PlayerPawn?.Health ?? 0;
                roundStats.Armor = player.PlayerPawn?.ArmorValue ?? 0;
                roundStats.IsAlive = (player.PlayerPawn?.Health ?? 0) > 0;
            }
        }

        LogGameEvent(_demo, "round_end", $"Round ended - Winner: {e.Winner}, Reason: {e.Reason}", true);
    }

    private void OnPlayerDeath(Source1PlayerDeathEvent e)
    {
        if (_demo == null || _currentDemoFile == null || _currentRound == null || e.Player == null) return;
        
        // Skip the first two rounds for ESEA/FACEIT demos (warmup and knife round)
        if (ShouldSkipRound()) return;

        var victim = GetOrCreatePlayer(e.Player);
        var killer = e.Attacker != null ? GetOrCreatePlayer(e.Attacker) : null;
        var assister = e.Assister != null ? GetOrCreatePlayer(e.Assister) : null;

        var kill = new Models.Kill
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = GetDisplayRoundNumber(), // Use display round number
            Tick = _demo.CurrentDemoTick.Value,
            GameTime = (float)_demo.CurrentGameTime.Value,
            Killer = killer,
            Victim = victim,
            Assister = assister,
            Weapon = e.Weapon,
            IsHeadshot = e.Headshot,
            IsWallbang = e.Penetrated > 0,
            Penetration = e.Penetrated,
            IsNoScope = e.Noscope,
            ThroughSmoke = e.Thrusmoke,
            AttackerBlind = e.Attackerblind,
            KillerHealth = e.AttackerPawn?.Health ?? 0,
            KillerArmor = e.AttackerPawn?.ArmorValue ?? 0,
            VictimHealth = 0,
            VictimArmor = e.PlayerPawn?.ArmorValue ?? 0,
            KillerTeam = e.Attacker?.CSTeamNum.ToString(),
            VictimTeam = e.Player.CSTeamNum.ToString(),
            IsTeamKill = e.Attacker?.CSTeamNum == e.Player.CSTeamNum
        };

        // Get positions using correct API
        var attackerPos = e.AttackerPawn?.Origin;
        var victimPos = e.PlayerPawn?.Origin;
        
        // Get view angles using correct API
        var attackerAngles = e.AttackerPawn?.EyeAngles;
        var victimAngles = e.PlayerPawn?.EyeAngles;

        if (attackerPos != null)
        {
            kill.KillerPositionX = (decimal)attackerPos.Value.X;
            kill.KillerPositionY = (decimal)attackerPos.Value.Y;
            kill.KillerPositionZ = (decimal)attackerPos.Value.Z;
        }

        if (victimPos != null)
        {
            kill.VictimPositionX = (decimal)victimPos.Value.X;
            kill.VictimPositionY = (decimal)victimPos.Value.Y;
            kill.VictimPositionZ = (decimal)victimPos.Value.Z;
        }
        
        // Capture view angles for crosshair placement analysis
        if (attackerAngles.HasValue)
        {
            kill.KillerViewAngleX = (decimal)attackerAngles.Value.Pitch;
            kill.KillerViewAngleY = (decimal)attackerAngles.Value.Yaw;
        }
        
        if (victimAngles.HasValue)
        {
            kill.VictimViewAngleX = (decimal)victimAngles.Value.Pitch;
            kill.VictimViewAngleY = (decimal)victimAngles.Value.Yaw;
        }

        if (attackerPos != null && victimPos != null)
        {
            var dx = attackerPos.Value.X - victimPos.Value.X;
            var dy = attackerPos.Value.Y - victimPos.Value.Y;
            var dz = attackerPos.Value.Z - victimPos.Value.Z;
            kill.Distance = (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }
        
        // Enhanced data from demofile-net Source1PlayerDeathEvent
        kill.IsRevengeKill = e.Revenge > 0;
        kill.IsDominating = e.Dominated > 0;
        kill.IsRevenge = e.Revenge > 0;
        kill.VictimBlind = false; // Property not available in current version
        kill.AttackerBlind = e.Attackerblind;
        kill.FlashDuration = 0; // Flash duration tracking - simplified for now
        
        // Enhanced assist tracking
        if (e.Assistedflash && e.Assister != null)
        {
            kill.AssistType = "Flash Assist";
            // Calculate assist distance if both positions are available
            var assisterPos = e.AssisterPawn?.Origin;
            if (assisterPos != null && victimPos != null)
            {
                var adx = assisterPos.Value.X - victimPos.Value.X;
                var ady = assisterPos.Value.Y - victimPos.Value.Y;
                var adz = assisterPos.Value.Z - victimPos.Value.Z;
                kill.AssistDistance = (float)Math.Sqrt(adx * adx + ady * ady + adz * adz);
            }
        }
        else if (e.Assister != null)
        {
            kill.AssistType = "Damage Assist";
        }

        _kills.Add(kill);

        if (_currentRoundStats.TryGetValue((int)e.Player.EntityIndex.Value, out var victimStats))
        {
            victimStats.Deaths++;
        }

        if (killer != null && e.Attacker != null && _currentRoundStats.TryGetValue((int)e.Attacker.EntityIndex.Value, out var killerStats))
        {
            killerStats.Kills++;
        }

        if (assister != null && e.Assister != null && _currentRoundStats.TryGetValue((int)e.Assister.EntityIndex.Value, out var assisterStats))
        {
            assisterStats.Assists++;
        }

        LogGameEvent(_demo, "player_death", $"{e.Player.PlayerName} killed by {e.Attacker?.PlayerName} with {e.Weapon}");
    }


    private void OnPlayerHurt(Source1PlayerHurtEvent e)
    {
        if (_demo == null || _currentDemoFile == null || _currentRound == null || e.Player == null) return;
        
        // Skip the first two rounds for ESEA/FACEIT demos (warmup and knife round)
        if (ShouldSkipRound()) return;

        var victim = GetOrCreatePlayer(e.Player);
        var attacker = e.Attacker != null ? GetOrCreatePlayer(e.Attacker) : null;

        var damage = new Models.Damage
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = GetDisplayRoundNumber(), // Use display round number
            Tick = _demo.CurrentDemoTick.Value,
            GameTime = (float)_demo.CurrentGameTime.Value,
            Attacker = attacker,
            Victim = victim,
            Weapon = e.Weapon,
            DamageAmount = e.DmgHealth,
            DamageArmor = e.DmgArmor,
            Health = e.Health,
            Armor = e.Armor,
            AttackerTeam = e.Attacker?.CSTeamNum.ToString(),
            VictimTeam = e.Player.CSTeamNum.ToString(),
            IsTeamDamage = e.Attacker?.CSTeamNum == e.Player.CSTeamNum
        };

        // Get positions using correct API
        var attackerPos = e.AttackerPawn?.Origin;
        var victimPos = e.PlayerPawn?.Origin;

        if (attackerPos != null)
        {
            damage.AttackerPositionX = (decimal)attackerPos.Value.X;
            damage.AttackerPositionY = (decimal)attackerPos.Value.Y;
            damage.AttackerPositionZ = (decimal)attackerPos.Value.Z;
        }

        if (victimPos != null)
        {
            damage.VictimPositionX = (decimal)victimPos.Value.X;
            damage.VictimPositionY = (decimal)victimPos.Value.Y;
            damage.VictimPositionZ = (decimal)victimPos.Value.Z;
        }

        if (attackerPos != null && victimPos != null)
        {
            var dx = attackerPos.Value.X - victimPos.Value.X;
            var dy = attackerPos.Value.Y - victimPos.Value.Y;
            var dz = attackerPos.Value.Z - victimPos.Value.Z;
            damage.Distance = (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        _damages.Add(damage);

        if (_currentRoundStats.TryGetValue((int)e.Player.EntityIndex.Value, out var victimStats))
        {
            victimStats.Damage += e.DmgHealth;
        }

        LogGameEvent(_demo, "player_hurt", $"{e.Player.PlayerName} hurt by {e.Attacker?.PlayerName} for {e.DmgHealth} damage");

        // Track player positions during damage events for continuous tracking
        TrackPlayerPositions();
    }

    private void OnWeaponFire(Source1WeaponFireEvent e)
    {
        if (_demo == null || _currentDemoFile == null || _currentRound == null || e.Player == null) return;
        
        // Skip the first two rounds for ESEA/FACEIT demos (warmup and knife round)
        if (ShouldSkipRound()) return;

        var player = GetOrCreatePlayer(e.Player);

        var weaponFire = new Models.WeaponFire
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = GetDisplayRoundNumber(), // Use display round number
            Tick = _demo.CurrentDemoTick.Value,
            GameTime = (float)_demo.CurrentGameTime.Value,
            Player = player,
            Weapon = e.Weapon,
            Team = ((int)e.Player.CSTeamNum).ToString(),
            IsScoped = e.PlayerPawn?.IsScoped ?? false,
            IsSilenced = e.Silenced
        };

        var playerPos = e.PlayerPawn?.Origin;
        if (playerPos != null)
        {
            weaponFire.PositionX = (decimal)playerPos.Value.X;
            weaponFire.PositionY = (decimal)playerPos.Value.Y;
            weaponFire.PositionZ = (decimal)playerPos.Value.Z;
        }

        _weaponFires.Add(weaponFire);

        if (_currentRoundStats.TryGetValue((int)e.Player.EntityIndex.Value, out var roundStats))
        {
            roundStats.ShotsFired++;
        }

        // Enhanced weapon state change tracking - simplified for current API
        var weaponStateChange = new Models.WeaponStateChange
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = GetDisplayRoundNumber(),
            Player = player,
            Tick = _demo.CurrentDemoTick.Value,
            GameTime = (float)_demo.CurrentGameTime.Value,
            WeaponName = e.Weapon,
            WeaponClass = "Unknown", // WeaponClass not easily available in fire event
            EventType = "Fire",
            AmmoClip = 0, // Ammo info not available in fire event
            AmmoReserve = 0,
            IsReloading = false,
            IsZoomed = e.PlayerPawn?.IsScoped ?? false,
            PositionX = weaponFire.PositionX,
            PositionY = weaponFire.PositionY,
            PositionZ = weaponFire.PositionZ
        };

        // Capture view angles for weapon fire analysis
        var eyeAngles = e.PlayerPawn?.EyeAngles;
        if (eyeAngles.HasValue)
        {
            weaponStateChange.ViewAngleX = (decimal)eyeAngles.Value.Pitch;
            weaponStateChange.ViewAngleY = (decimal)eyeAngles.Value.Yaw;
        }

        _weaponStateChanges.Add(weaponStateChange);

        // Track player positions when weapons are fired
        TrackPlayerPositions();
    }

    private void OnHegrenadeDetonate(Source1HegrenadeDetonateEvent e)
    {
        ProcessGrenadeDetonate("hegrenade", e.X, e.Y, e.Z, e.Player);
    }

    private void OnFlashbangDetonate(Source1FlashbangDetonateEvent e)
    {
        ProcessGrenadeDetonate("flashbang", e.X, e.Y, e.Z, e.Player);
    }

    private void OnSmokegrenadeDetonate(Source1SmokegrenadeDetonateEvent e)
    {
        ProcessGrenadeDetonate("smokegrenade", e.X, e.Y, e.Z, e.Player);
    }

    private void ProcessGrenadeDetonate(string grenadeType, float x, float y, float z, CCSPlayerController? player)
    {
        if (_demo == null || _currentDemoFile == null || _currentRound == null || player == null) return;

        var playerModel = GetOrCreatePlayer(player);

        var grenade = new Models.Grenade
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = GetDisplayRoundNumber(), // Use display round number
            Player = playerModel,
            DetonateTick = _demo.CurrentDemoTick.Value,
            DetonateTime = _demo.CurrentGameTime.Value,
            GrenadeType = grenadeType,
            DetonatePositionX = (decimal)x,
            DetonatePositionY = (decimal)y,
            DetonatePositionZ = (decimal)z,
            Team = player.CSTeamNum.ToString()
        };

        _grenades.Add(grenade);
        LogGameEvent(_demo, $"{grenadeType}_detonate", $"{player.PlayerName} {grenadeType} detonated");
    }

    private void OnBombPlanted(Source1BombPlantedEvent e)
    {
        ProcessBombEvent("planted", e.Player, e.Site);
    }

    private void OnBombDefused(Source1BombDefusedEvent e)
    {
        ProcessBombEvent("defused", e.Player, e.Site);
    }

    private void OnBombExploded(Source1BombExplodedEvent e)
    {
        ProcessBombEvent("exploded", e.Player, e.Site);
    }

    private void OnBombBeginPlant(Source1BombBeginplantEvent e)
    {
        ProcessBombEvent("begin_plant", e.Player, e.Site);
    }

    private void OnBombAbortPlant(Source1BombAbortplantEvent e)
    {
        ProcessBombEvent("abort_plant", e.Player, e.Site);
    }

    private void OnBombBeginDefuse(Source1BombBegindefuseEvent e)
    {
        ProcessBombEvent("begin_defuse", e.Player, null);
    }

    private void OnBombAbortDefuse(Source1BombAbortdefuseEvent e)
    {
        ProcessBombEvent("abort_defuse", e.Player, null);
    }

    private void OnBombDropped(Source1BombDroppedEvent e)
    {
        ProcessBombEvent("dropped", e.Player, null);
    }

    private void OnBombPickup(Source1BombPickupEvent e)
    {
        ProcessBombEvent("pickup", e.PlayerPawn?.Controller, null);
    }

    private void ProcessBombEvent(string eventType, CCSPlayerController? player, int? site)
    {
        if (_demo == null || _currentDemoFile == null || _currentRound == null || player == null) return;

        var playerModel = GetOrCreatePlayer(player);

        var bomb = new Models.Bomb
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = GetDisplayRoundNumber(), // Use display round number
            EventType = eventType,
            Tick = _demo.CurrentDemoTick.Value,
            GameTime = (float)_demo.CurrentGameTime.Value,
            Player = playerModel,
            Site = site?.ToString(),
            Team = player.CSTeamNum.ToString(),
            HasKit = player.PawnHasDefuser
        };

        var playerPos = player.PlayerPawn?.Origin;
        if (playerPos != null)
        {
            bomb.PositionX = (decimal)playerPos.Value.X;
            bomb.PositionY = (decimal)playerPos.Value.Y;
            bomb.PositionZ = (decimal)playerPos.Value.Z;
        }

        _bombs.Add(bomb);
        LogGameEvent(_demo, $"bomb_{eventType}", $"{player.PlayerName} {eventType} bomb", true);
    }

    private void OnPlayerConnect(Source1PlayerConnectEvent e)
    {
        LogGameEvent(_demo, "player_connect", $"{e.Name} connected");
    }

    private void OnPlayerDisconnect(Source1PlayerDisconnectEvent e)
    {
        LogGameEvent(_demo, "player_disconnect", $"{e.Name} disconnected: {e.Reason}");
    }

    private void OnPlayerTeam(Source1PlayerTeamEvent e)
    {
        LogGameEvent(_demo, "player_team", $"Player {e.Player?.PlayerName} changed team");
    }

    private void OnEnterBombZone(Source1EnterBombzoneEvent e)
    {
        ProcessZoneEvent("enter_bombzone", "bombzone", e.Player);
    }

    private void OnExitBombZone(Source1ExitBombzoneEvent e)
    {
        ProcessZoneEvent("exit_bombzone", "bombzone", e.Player);
    }

    private void OnEnterBuyZone(Source1EnterBuyzoneEvent e)
    {
        ProcessZoneEvent("enter_buyzone", "buyzone", e.Player);
    }

    private void OnExitBuyZone(Source1ExitBuyzoneEvent e)
    {
        ProcessZoneEvent("exit_buyzone", "buyzone", e.Player);
    }

    private void ProcessZoneEvent(string eventType, string zoneType, CCSPlayerController? player)
    {
        if (_demo == null || _currentDemoFile == null || player == null) return;

        var playerModel = GetOrCreatePlayer(player);

        var zoneEvent = new Models.ZoneEvent
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound?.Id,
            PlayerId = playerModel.Id,
            Tick = _demo.CurrentDemoTick.Value,
            GameTime = (float)_demo.CurrentGameTime.Value,
            EventType = eventType,
            ZoneType = zoneType,
            Team = player.CSTeamNum.ToString(),
            RoundNumber = GetDisplayRoundNumber(),
            Description = $"{player.PlayerName} {eventType.Replace("_", " ")}"
        };

        var playerPos = player.PlayerPawn?.Origin;
        if (playerPos != null)
        {
            zoneEvent.PositionX = (decimal)playerPos.Value.X;
            zoneEvent.PositionY = (decimal)playerPos.Value.Y;
            zoneEvent.PositionZ = (decimal)playerPos.Value.Z;
        }

        _zoneEvents.Add(zoneEvent);
        LogGameEvent(_demo, eventType, $"{player.PlayerName} {eventType.Replace("_", " ")} {zoneType}");
    }

    private void OnWeaponReload(Source1WeaponReloadEvent e)
    {
        ProcessWeaponStateEvent("reload", e.Player, e.Player?.PlayerPawn?.ActiveWeapon?.GetType().Name);
    }

    private void OnWeaponZoom(Source1WeaponZoomEvent e)
    {
        ProcessWeaponStateEvent("zoom", e.Player, e.Player?.PlayerPawn?.ActiveWeapon?.GetType().Name);
    }

    private void ProcessWeaponStateEvent(string eventType, CCSPlayerController? player, string? weaponName)
    {
        if (_demo == null || _currentDemoFile == null || player == null || weaponName == null) return;

        var playerModel = GetOrCreatePlayer(player);
        var activeWeapon = player.PlayerPawn?.ActiveWeapon;

        var weaponState = new Models.WeaponState
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound?.Id,
            PlayerId = playerModel.Id,
            Tick = _demo.CurrentDemoTick.Value,
            GameTime = (float)_demo.CurrentGameTime.Value,
            EventType = eventType,
            WeaponName = weaponName,
            AmmoClip = activeWeapon?.Clip1 ?? 0,
            AmmoReserve = activeWeapon?.ReserveAmmo?[0] ?? 0,
            IsScoped = player.PlayerPawn?.IsScoped ?? false,
            Team = player.CSTeamNum.ToString(),
            RoundNumber = GetDisplayRoundNumber(),
            Description = $"{player.PlayerName} {eventType.Replace("_", " ")} {weaponName}"
        };

        var playerPos = player.PlayerPawn?.Origin;
        if (playerPos != null)
        {
            weaponState.PositionX = (decimal)playerPos.Value.X;
            weaponState.PositionY = (decimal)playerPos.Value.Y;
            weaponState.PositionZ = (decimal)playerPos.Value.Z;
        }

        _weaponStates.Add(weaponState);
        LogGameEvent(_demo, $"weapon_{eventType}", weaponState.Description);
    }

    private void OnItemPurchase(Source1ItemPurchaseEvent e)
    {
        ProcessEconomyEvent("purchase", e.Player, e.Weapon);
    }

    private void OnItemPickup(Source1ItemPickupEvent e)
    {
        ProcessEconomyEvent("pickup", e.Player, e.Item);
        ProcessEquipmentEvent("pickup", e.Player, e.Item);
    }

    private void OnItemEquip(Source1ItemEquipEvent e)
    {
        ProcessEconomyEvent("equip", e.Player, e.Item);
        ProcessEquipmentEvent("equip", e.Player, e.Item);
    }

    private void ProcessEconomyEvent(string eventType, CCSPlayerController? player, string? item)
    {
        if (_demo == null || _currentDemoFile == null || player == null || item == null) return;

        var playerModel = GetOrCreatePlayer(player);
        
        // Skip if we can't get a valid player model
        if (playerModel == null) return;

        var playerId = (int)player.EntityIndex.Value;
        var currentMoney = player.InGameMoneyServices?.Account ?? 0;
        
        // Get previous money amount for this player
        var previousMoney = _playerMoneyTracking.GetValueOrDefault(playerId, currentMoney);
        
        // Calculate money change and item cost
        var moneyChange = currentMoney - previousMoney;
        var itemCost = eventType == "purchase" ? Math.Abs(moneyChange) : (int?)null;
        
        // Update money tracking
        _playerMoneyTracking[playerId] = currentMoney;

        var economyEvent = new Models.EconomyEvent
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRoundNumber, // Store round number temporarily, will be resolved during save
            PlayerId = playerId, // Store entity index temporarily, will be resolved during save
            Tick = _demo.CurrentDemoTick.Value,
            GameTime = (float)_demo.CurrentGameTime.Value,
            EventType = eventType,
            ItemName = item,
            ItemCost = itemCost,
            MoneyBefore = previousMoney,
            MoneyAfter = currentMoney,
            MoneyChange = moneyChange,
            Team = player.CSTeamNum.ToString(),
            RoundNumber = GetDisplayRoundNumber(),
            Description = $"{player.PlayerName} {eventType} {item} (${previousMoney} -> ${currentMoney})"
        };

        _economyEvents.Add(economyEvent);
        LogGameEvent(_demo, $"item_{eventType}", economyEvent.Description);
    }

    private void ProcessEquipmentEvent(string eventType, CCSPlayerController? player, string? item)
    {
        if (_demo == null || _currentDemoFile == null || player == null || item == null) return;

        var playerModel = GetOrCreatePlayer(player);

        var equipment = new Models.Equipment
        {
            DemoFileId = _currentDemoFile.Id,
            Player = playerModel,
            Tick = _demo.CurrentDemoTick.Value,
            GameTime = (float)_demo.CurrentGameTime.Value,
            Action = eventType,
            ItemName = item,
            Team = player.CSTeamNum.ToString(),
            RoundNumber = GetDisplayRoundNumber(),
            IsActive = eventType == "equip" || eventType == "pickup"
        };

        var playerPos = player.PlayerPawn?.Origin;
        if (playerPos != null)
        {
            equipment.PositionX = (decimal)playerPos.Value.X;
            equipment.PositionY = (decimal)playerPos.Value.Y;
            equipment.PositionZ = (decimal)playerPos.Value.Z;
        }

        _equipment.Add(equipment);
    }

    private void OnPlayerChat(Source1PlayerChatEvent e)
    {
        if (_demo == null || _currentDemoFile == null) return;

        // Find player controller by user ID
        var playerController = _demo.PlayersIncludingDisconnected.FirstOrDefault(p => p.EntityIndex.Value == e.Player);
        if (playerController == null) return;

        var playerModel = GetOrCreatePlayer(playerController);

        var chatMessage = new Models.ChatMessage
        {
            DemoFileId = _currentDemoFile.Id,
            Player = playerModel,
            Tick = _demo.CurrentDemoTick.Value,
            GameTime = (float)_demo.CurrentGameTime.Value,
            Message = e.Text,
            SenderName = playerController.PlayerName,
            IsTeamMessage = e.Teamonly,
            Team = playerController.CSTeamNum.ToString(),
            Timestamp = DateTime.UtcNow
        };

        _chatMessages.Add(chatMessage);
        LogGameEvent(_demo, "player_chat", $"{playerController.PlayerName}: {e.Text}");
    }

    // Method to track player positions on regular intervals
    public void TrackPlayerPositions()
    {
        if (_demo == null || _currentDemoFile == null) return;

        var parsePositions = _configuration.GetValue<bool>("ParserSettings:ParsePlayerPositions", true);
        var positionInterval = _configuration.GetValue<int>("ParserSettings:PlayerPositionInterval", 16);

        if (parsePositions && _demo.CurrentDemoTick.Value % positionInterval == 0)
        {
            foreach (var player in _demo.Players)
            {
                if (player.PlayerName != null && player.PlayerPawn != null)
                {
                    var playerModel = GetOrCreatePlayer(player);
                    var playerPos = player.PlayerPawn.Origin;
                    var eyeAngles = player.PlayerPawn.EyeAngles;
                    var velocity = player.PlayerPawn.Velocity;

                    var position = new Models.PlayerPosition
                    {
                        DemoFileId = _currentDemoFile.Id,
                        Player = playerModel,
                        Tick = _demo.CurrentDemoTick.Value,
                        GameTime = (float)_demo.CurrentGameTime.Value,
                        PositionX = (decimal)playerPos.X,
                        PositionY = (decimal)playerPos.Y,
                        PositionZ = (decimal)playerPos.Z,
                        ViewAngleX = (decimal)eyeAngles.Pitch,
                        ViewAngleY = (decimal)eyeAngles.Yaw,
                        ViewAngleZ = (decimal)eyeAngles.Roll,
                        VelocityX = (decimal)velocity.X,
                        VelocityY = (decimal)velocity.Y,
                        VelocityZ = (decimal)velocity.Z,
                        Speed = (float)Math.Sqrt(velocity.X * velocity.X + velocity.Y * velocity.Y),
                        IsAlive = player.PlayerPawn.Health > 0,
                        Health = player.PlayerPawn.Health,
                        Armor = player.PlayerPawn.ArmorValue,
                        HasHelmet = player.PlayerPawn.ItemServices?.HasHelmet ?? false,
                        HasDefuseKit = player.PlayerPawn.ItemServices?.HasDefuser ?? false,
                        IsScoped = player.PlayerPawn.IsScoped,
                        IsCrouching = (player.PlayerPawn.MovementServices as CCSPlayer_MovementServices)?.Ducking ?? false,
                        ActiveWeapon = player.PlayerPawn.ActiveWeapon?.GetType().Name,
                        Team = player.CSTeamNum.ToString(),
                        Money = player.InGameMoneyServices?.Account ?? 0,
                        IsBlind = player.PlayerPawn.FlashDuration > 0,
                        FlashDuration = (int)player.PlayerPawn.FlashDuration
                    };

                    _playerPositions.Add(position);
                }
            }
        }
    }

    private Models.Player GetOrCreatePlayer(CCSPlayerController playerController)
    {
        var playerSlot = (int)playerController.EntityIndex.Value - 1;
        if (_players.TryGetValue(playerSlot, out var existingPlayer))
        {
            return existingPlayer;
        }

        var player = new Models.Player
        {
            DemoFileId = _currentDemoFile!.Id,
            PlayerSlot = playerSlot,
            UserId = playerController.PlayerInfo?.Userid ?? 0,
            SteamId = playerController.SteamID,
            PlayerName = playerController.PlayerName,
            Team = playerController.TeamNum.ToString(),
            IsBot = playerController.PlayerInfo?.Fakeplayer ?? false,
            IsHltv = false, // Remove HLTV check for now
            IsConnected = playerController.Connected == PlayerConnectedState.PlayerConnected,
            ConnectedAt = DateTime.UtcNow
        };

        _players.TryAdd(playerSlot, player);
        
        // Initialize money tracking for this player
        var playerId = (int)playerController.EntityIndex.Value;
        var currentMoney = playerController.InGameMoneyServices?.Account ?? 0;
        _playerMoneyTracking.TryAdd(playerId, currentMoney);
        
        return player;
    }

    private void LogGameEvent(CsDemoParser? demo, string eventName, string description, bool isImportant = false)
    {
        if (demo == null || _currentDemoFile == null) return;

        var gameEvent = new Models.GameEvent
        {
            DemoFileId = _currentDemoFile.Id,
            Tick = demo.CurrentDemoTick.Value,
            GameTime = (float)demo.CurrentGameTime.Value,
            EventName = eventName,
            Description = description,
            IsImportant = isImportant,
            RoundNumber = GetDisplayRoundNumber(),
            CreatedAt = DateTime.UtcNow
        };

        _gameEvents.Add(gameEvent);
    }

    private async Task SaveDataToDatabase()
    {
        try
        {
            _logger.LogInformation("Saving parsed data to database...");

            var batchSize = _configuration.GetValue<int>("ParserSettings:DatabaseBatchSize", 1000);

            await SavePlayersAsync();
            await SaveMatchAsync();
            // Update all rounds with the correct MatchId after the match has been saved
            await UpdateRoundsWithMatchIdAsync();
            await SaveRoundsAsync();
            // Update all entities with correct Round IDs after rounds have been saved
            await UpdateEntityRoundIdsAsync();
            await SaveInBatchesAsync(_kills, batchSize, "Kills");
            await SaveInBatchesAsync(_damages, batchSize, "Damages");
            await SaveInBatchesAsync(_weaponFires, batchSize, "Weapon Fires");
            await SaveInBatchesAsync(_grenades, batchSize, "Grenades");
            await SaveInBatchesAsync(_bombs, batchSize, "Bomb Events");
            await SaveInBatchesAsync(_playerPositions, batchSize, "Player Positions");
            await SaveInBatchesAsync(_chatMessages, batchSize, "Chat Messages");
            await SaveInBatchesAsync(_equipment, batchSize, "Equipment");
            await SaveInBatchesAsync(_gameEvents, batchSize, "Game Events");
            await SaveInBatchesAsync(_playerRoundStats, batchSize, "Player Round Stats");
            
            // Save new comprehensive data collections  
            // Re-enabling collections that have working event handlers
            await SaveInBatchesAsync(_grenadeTrajectories, batchSize, "Grenade Trajectories");
            await SaveInBatchesAsync(_economyEvents, batchSize, "Economy Events");
            await SaveInBatchesAsync(_bulletImpacts, batchSize, "Bullet Impacts");
            await SaveInBatchesAsync(_playerMovements, batchSize, "Player Movements");
            await SaveInBatchesAsync(_zoneEvents, batchSize, "Zone Events");
            await SaveInBatchesAsync(_radioCommands, batchSize, "Radio Commands");
            await SaveInBatchesAsync(_weaponStates, batchSize, "Weapon States");
            await SaveInBatchesAsync(_flashEvents, batchSize, "Flash Events");
            
            // Save advanced entity tracking data
            // Re-enabling entity tracking collections
            await SaveInBatchesAsync(_entityLifecycles, batchSize, "Entity Lifecycles");
            await SaveInBatchesAsync(_entityInteractions, batchSize, "Entity Interactions");
            await SaveInBatchesAsync(_entityVisibilities, batchSize, "Entity Visibilities");
            await SaveInBatchesAsync(_entityEffects, batchSize, "Entity Effects");
            await SaveInBatchesAsync(_droppedItems, batchSize, "Dropped Items");
            await SaveInBatchesAsync(_smokeClouds, batchSize, "Smoke Clouds");
            await SaveInBatchesAsync(_fireAreas, batchSize, "Fire Areas");
            
            // Save game state tracking data
            await SaveInBatchesAsync(_teamStates, batchSize, "Team States");
            await SaveInBatchesAsync(_economyStates, batchSize, "Economy States");
            await SaveInBatchesAsync(_mapControls, batchSize, "Map Controls");
            await SaveInBatchesAsync(_tacticalEvents, batchSize, "Tactical Events");
            
            // Save advanced statistics data
            await SaveInBatchesAsync(_advancedPlayerStats, batchSize, "Advanced Player Stats");
            await SaveInBatchesAsync(_performanceMetrics, batchSize, "Performance Metrics");
            await SaveInBatchesAsync(_roundImpacts, batchSize, "Round Impacts");
            
            // Save voice and communication data
            await SaveInBatchesAsync(_voiceCommunications, batchSize, "Voice Communications");
            await SaveInBatchesAsync(_communicationPatterns, batchSize, "Communication Patterns");
            
            // Save advanced event tracking data
            await SaveInBatchesAsync(_temporaryEntities, batchSize, "Temporary Entities");
            await SaveInBatchesAsync(_entityPropertyChanges, batchSize, "Entity Property Changes");
            await SaveInBatchesAsync(_hostageEvents, batchSize, "Hostage Events");
            await SaveInBatchesAsync(_advancedUserMessages, batchSize, "Advanced User Messages");
            await SaveInBatchesAsync(_playerBehaviorEvents, batchSize, "Player Behavior Events");
            await SaveInBatchesAsync(_infernoEvents, batchSize, "Inferno Events");
            
            // Save enhanced analytics collections for advanced gameplay insights
            await SaveInBatchesAsync(_playerInputs, batchSize, "Player Inputs");
            await SaveInBatchesAsync(_weaponStateChanges, batchSize, "Weapon State Changes");
            await SaveInBatchesAsync(_enhancedPlayerPositions, batchSize, "Enhanced Player Positions");

            await CalculateAndSavePlayerMatchStats();

            _logger.LogInformation("Successfully saved all data to database");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving data to database");
            throw;
        }
    }

    private async Task SavePlayersAsync()
    {
        var playersToSave = _players.Values.ToList();
        if (playersToSave.Any())
        {
            _context.Players.AddRange(playersToSave);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Saved {Count} players", playersToSave.Count);
        }
    }

    private async Task SaveMatchAsync()
    {
        if (_currentMatch != null)
        {
            _currentMatch.EndTime = DateTime.UtcNow;
            _currentMatch.IsFinished = true;
            _currentMatch.TotalRounds = _currentRoundNumber;

            _context.Matches.Add(_currentMatch);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Saved match data");
        }
    }

    private async Task SaveRoundsAsync()
    {
        var roundsList = _rounds.ToList();
        if (roundsList.Any())
        {
            _context.Rounds.AddRange(roundsList);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Saved {Count} rounds", roundsList.Count);
        }
    }

    private async Task UpdateRoundsWithMatchIdAsync()
    {
        if (_currentMatch?.Id > 0)
        {
            var roundsList = _rounds.ToList();
            foreach (var round in roundsList)
            {
                round.MatchId = _currentMatch.Id;
            }
            _logger.LogInformation("Updated {Count} rounds with MatchId {MatchId}", roundsList.Count, _currentMatch.Id);
        }
    }

    private async Task UpdateEntityRoundIdsAsync()
    {
        if (_currentMatch?.Id > 0)
        {
            // Get the saved rounds from the database
            var savedRounds = await _context.Rounds
                .Where(r => r.MatchId == _currentMatch.Id)
                .ToListAsync();
            
            if (!savedRounds.Any())
            {
                _logger.LogWarning("No saved rounds found for MatchId {MatchId}", _currentMatch.Id);
                return;
            }
            
            var roundLookup = savedRounds.ToDictionary(r => r.RoundNumber, r => r.Id);
            
            // Update all entities that reference Round IDs
            var killsList = _kills.ToList();
            var validKills = new List<Models.Kill>();
            foreach (var kill in killsList)
            {
                // kill.RoundId currently contains the round number, need to convert to actual database ID
                if (roundLookup.ContainsKey(kill.RoundId))
                {
                    kill.RoundId = roundLookup[kill.RoundId]; // Update with actual database ID
                    validKills.Add(kill);
                }
                else
                {
                    _logger.LogWarning("Skipping Kill with invalid Round: RoundNumber = {RoundNumber}", kill.RoundId);
                }
            }
            
            // Replace kills collection with valid kills only
            _kills.Clear();
            foreach (var validKill in validKills)
            {
                _kills.Add(validKill);
            }
            
            var damagesList = _damages.ToList();
            int damagesValidated = 0;
            foreach (var damage in damagesList)
            {
                if (roundLookup.ContainsKey(damage.RoundId))
                {
                    damage.RoundId = roundLookup[damage.RoundId];
                    damagesValidated++;
                }
            }
            
            var weaponFiresList = _weaponFires.ToList();
            int weaponFiresValidated = 0;
            foreach (var weaponFire in weaponFiresList)
            {
                if (roundLookup.ContainsKey(weaponFire.RoundId))
                {
                    weaponFire.RoundId = roundLookup[weaponFire.RoundId];
                    weaponFiresValidated++;
                }
            }
            
            var grenadesList = _grenades.ToList();
            int grenadesValidated = 0;
            foreach (var grenade in grenadesList)
            {
                if (roundLookup.ContainsKey(grenade.RoundId))
                {
                    grenade.RoundId = roundLookup[grenade.RoundId];
                    grenadesValidated++;
                }
            }
            
            var bombsList = _bombs.ToList();
            int bombsValidated = 0;
            foreach (var bomb in bombsList)
            {
                if (roundLookup.ContainsKey(bomb.RoundId))
                {
                    bomb.RoundId = roundLookup[bomb.RoundId];
                    bombsValidated++;
                }
            }
            
            // Update InfernoEvents RoundIds
            var infernoEventsList = _infernoEvents.ToList();
            int infernoEventsValidated = 0;
            foreach (var infernoEvent in infernoEventsList)
            {
                // infernoEvent.RoundId currently contains the round number, need to convert to actual database ID
                if (infernoEvent.RoundId.HasValue && roundLookup.ContainsKey(infernoEvent.RoundId.Value))
                {
                    infernoEvent.RoundId = roundLookup[infernoEvent.RoundId.Value];
                    infernoEventsValidated++;
                }
                else
                {
                    _logger.LogWarning("Skipping InfernoEvent with invalid Round: RoundNumber = {RoundNumber}", infernoEvent.RoundId);
                }
            }
            
            // Update PlayerRoundStats RoundIds and PlayerIds
            var playerRoundStatsList = _playerRoundStats.ToList();
            var validPlayerRoundStats = new List<Models.PlayerRoundStats>();
            
            // Create player lookup from PlayerSlot to database ID
            var playerLookup = new Dictionary<int, int>();
            foreach (var player in _players.Values)
            {
                // PlayerSlot is the key, player.Id (database ID) is the value
                if (player.Id > 0)
                {
                    playerLookup[player.PlayerSlot] = player.Id;
                }
            }
            
            _logger.LogInformation("Created player lookup with {Count} mappings. PlayerRoundStats to validate: {StatsCount}", 
                playerLookup.Count, playerRoundStatsList.Count);
            
            foreach (var playerRoundStats in playerRoundStatsList)
            {
                bool isValid = true;
                
                // Update RoundId
                if (roundLookup.ContainsKey(playerRoundStats.RoundId))
                {
                    playerRoundStats.RoundId = roundLookup[playerRoundStats.RoundId];
                }
                else
                {
                    isValid = false;
                }
                
                // Update PlayerId from EntityIndex to database ID
                if (playerLookup.ContainsKey(playerRoundStats.PlayerId))
                {
                    playerRoundStats.PlayerId = playerLookup[playerRoundStats.PlayerId];
                }
                else
                {
                    isValid = false;
                }
                
                if (isValid)
                {
                    validPlayerRoundStats.Add(playerRoundStats);
                }
            }
            
            // Replace PlayerRoundStats collection with valid ones only
            _playerRoundStats.Clear();
            foreach (var validStats in validPlayerRoundStats)
            {
                _playerRoundStats.Add(validStats);
            }
            
            _logger.LogInformation("Validated and updated Round IDs - Kills: {KillsCount}, Damages: {DamagesCount}, WeaponFires: {WeaponFiresCount}, Grenades: {GrenadesCount}, Bombs: {BombsCount}, PlayerRoundStats: {PlayerRoundStatsCount}", 
                validKills.Count, damagesValidated, weaponFiresValidated, grenadesValidated, bombsValidated, validPlayerRoundStats.Count);
        }
    }

    private async Task SaveInBatchesAsync<T>(ConcurrentBag<T> items, int batchSize, string itemType) where T : class
    {
        var itemsList = items.ToList();
        if (!itemsList.Any()) return;

        // Prepare mapping dictionaries for entity ID resolution
        var entityIndexToPlayerIdMap = _context.Players
            .GroupBy(p => p.PlayerSlot + 1) // PlayerSlot is EntityIndex - 1
            .ToDictionary(g => g.Key, g => g.First().Id); // Take first player with that slot
            
        var roundNumberToRoundIdMap = _context.Rounds
            .GroupBy(r => r.RoundNumber)
            .ToDictionary(g => g.Key, g => g.First().Id); // Take first round with that number

        // Special handling for models that need entity ID to player ID resolution
        if (typeof(T) == typeof(Models.EconomyEvent))
        {
            var validEconomyEvents = new List<Models.EconomyEvent>();
            foreach (var item in itemsList.Cast<Models.EconomyEvent>())
            {
                bool isValid = true;
                
                // Resolve player ID
                if (entityIndexToPlayerIdMap.TryGetValue(item.PlayerId, out var realPlayerId))
                {
                    item.PlayerId = realPlayerId;
                }
                else
                {
                    isValid = false;
                }
                
                // Resolve round ID
                if (item.RoundId.HasValue && roundNumberToRoundIdMap.TryGetValue(item.RoundId.Value, out var realRoundId))
                {
                    item.RoundId = realRoundId;
                }
                else if (item.RoundId.HasValue)
                {
                    isValid = false;
                }
                
                if (isValid)
                {
                    validEconomyEvents.Add(item);
                }
            }
            
            // Replace the original list with valid items
            itemsList = validEconomyEvents.Cast<T>().ToList();
        }
        
        // Handle WeaponStates and other models that need foreign key resolution
        else if (typeof(T) == typeof(Models.WeaponState) || 
                 typeof(T) == typeof(Models.BulletImpact) ||
                 typeof(T) == typeof(Models.GrenadeTrajectory) ||
                 typeof(T) == typeof(Models.PlayerMovement) ||
                 typeof(T) == typeof(Models.ZoneEvent) ||
                 typeof(T) == typeof(Models.RadioCommand) ||
                 typeof(T) == typeof(Models.FlashEvent) ||
                 typeof(T) == typeof(Models.SmokeCloud) ||
                 typeof(T) == typeof(Models.FireArea) ||
                 typeof(T) == typeof(Models.InfernoEvent) ||
                 typeof(T) == typeof(Models.HostageEvent) ||
                 typeof(T) == typeof(Models.PlayerBehaviorEvent) ||
                 typeof(T) == typeof(Models.AdvancedUserMessage) ||
                 typeof(T) == typeof(Models.EntityLifecycle) ||
                 typeof(T) == typeof(Models.EntityPropertyChange))
        {
            var validResolutionItems = new List<T>();
            foreach (var item in itemsList)
            {
                bool isValid = true;
                
                // Check if item has PlayerId property and resolve it
                var playerIdProp = item.GetType().GetProperty("PlayerId");
                if (playerIdProp != null)
                {
                    var playerIdValue = playerIdProp.GetValue(item);
                    if (playerIdValue == null) continue;
                    var currentPlayerId = (int)playerIdValue;
                    
                    // If PlayerId looks like an entity index, try to resolve it
                    if (currentPlayerId > 64 && entityIndexToPlayerIdMap.TryGetValue(currentPlayerId, out var realPlayerId))
                    {
                        playerIdProp.SetValue(item, realPlayerId);
                    }
                    // If PlayerId is 0 or negative, skip this item
                    else if (currentPlayerId <= 0)
                    {
                        isValid = false;
                    }
                }
                
                // Check if item has RoundId property and resolve it
                var roundIdProp = item.GetType().GetProperty("RoundId");
                if (roundIdProp != null && isValid)
                {
                    var currentRoundId = roundIdProp.GetValue(item);
                    if (currentRoundId != null)
                    {
                        if (roundIdProp.PropertyType == typeof(int))
                        {
                            var roundId = (int)currentRoundId;
                            if (roundNumberToRoundIdMap.TryGetValue(roundId, out var realRoundId))
                            {
                                roundIdProp.SetValue(item, realRoundId);
                            }
                        }
                        else if (roundIdProp.PropertyType == typeof(int?))
                        {
                            var roundId = (int?)currentRoundId;
                            if (roundId.HasValue)
                            {
                                if (roundNumberToRoundIdMap.TryGetValue(roundId.Value, out var realRoundId))
                                {
                                    roundIdProp.SetValue(item, realRoundId);
                                }
                                else
                                {
                                    // Round number doesn't exist, set to null
                                    roundIdProp.SetValue(item, null);
                                }
                            }
                        }
                    }
                }
                
                if (isValid)
                {
                    validResolutionItems.Add(item);
                }
            }
            
            // Replace the original list with valid items
            itemsList = validResolutionItems;
        }

        // Filter out items with invalid Round or Player references
        var validItems = new List<T>();
        var savedPlayerIds = _context.Players.Select(p => p.Id).ToHashSet();
        var savedRoundIds = _context.Rounds.Select(r => r.Id).ToHashSet();
        
        _logger.LogInformation("Validating {Count} {ItemType} items. Available Player IDs: [{PlayerIds}], Available Round IDs: [{RoundIds}]", 
            itemsList.Count, itemType, string.Join(",", savedPlayerIds), string.Join(",", savedRoundIds));

        foreach (var item in itemsList)
        {
            bool isValid = true;

            // Check RoundId if the item has one
            var roundIdProp = item.GetType().GetProperty("RoundId");
            if (roundIdProp != null)
            {
                var roundIdValue = roundIdProp.GetValue(item);
                if (roundIdValue != null)
                {
                    if (roundIdProp.PropertyType == typeof(int))
                    {
                        var roundId = (int)roundIdValue;
                        // Allow RoundId = 0 (will be updated later) or valid saved round IDs
                        if (roundId < 0 || (roundId > 0 && !savedRoundIds.Contains(roundId)))
                        {
                            isValid = false;
                        }
                    }
                    else if (roundIdProp.PropertyType == typeof(int?))
                    {
                        var roundId = (int?)roundIdValue;
                        // Allow null, 0, or valid saved round IDs
                        if (roundId.HasValue && roundId.Value < 0 || (roundId.HasValue && roundId.Value > 0 && !savedRoundIds.Contains(roundId.Value)))
                        {
                            isValid = false;
                        }
                    }
                }
                else if (roundIdProp.PropertyType == typeof(int))
                {
                    // Required RoundId is null/0
                    isValid = false;
                }
            }

            // Check player-related foreign keys
            foreach (var prop in item.GetType().GetProperties())
            {
                if (prop.Name.EndsWith("PlayerId") && prop.PropertyType == typeof(int))
                {
                    var playerId = (int)prop.GetValue(item);
                    // Allow PlayerId = 0 (will be updated later) or valid saved player IDs
                    if (playerId < 0 || (playerId > 0 && !savedPlayerIds.Contains(playerId)))
                    {
                        isValid = false;
                        break;
                    }
                }
                else if (prop.Name.EndsWith("Id") && prop.Name.Contains("Player") && prop.PropertyType == typeof(int?))
                {
                    var playerId = (int?)prop.GetValue(item);
                    // Allow null, 0, or valid saved player IDs
                    if (playerId.HasValue && playerId.Value < 0 || (playerId.HasValue && playerId.Value > 0 && !savedPlayerIds.Contains(playerId.Value)))
                    {
                        isValid = false;
                        break;
                    }
                }
            }

            if (isValid)
            {
                validItems.Add(item);
            }
        }

        if (!validItems.Any()) 
        {
            _logger.LogWarning("No valid {ItemType} items to save after filtering", itemType);
            return;
        }

        for (int i = 0; i < validItems.Count; i += batchSize)
        {
            var batch = validItems.Skip(i).Take(batchSize).ToList();
            _context.Set<T>().AddRange(batch);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Saved batch of {Count} {ItemType} (Total: {Total})", batch.Count, itemType, Math.Min(i + batchSize, validItems.Count));
        }

        _logger.LogInformation("Saved {Count} {ItemType}", validItems.Count, itemType);
    }

    private async Task CalculateAndSavePlayerMatchStats()
    {
        if (_currentMatch == null) return;

        foreach (var player in _players.Values)
        {
            var playerKills = _kills.Where(k => k.KillerId == player.Id).ToList();
            var playerDeaths = _kills.Where(k => k.VictimId == player.Id).ToList();
            var playerAssists = _kills.Where(k => k.AssisterId == player.Id).ToList();
            var playerDamages = _damages.Where(d => d.AttackerId == player.Id).ToList();

            var matchStats = new Models.PlayerMatchStats
            {
                PlayerId = player.Id,
                MatchId = _currentMatch.Id,
                Kills = playerKills.Count,
                Deaths = playerDeaths.Count,
                Assists = playerAssists.Count,
                HeadshotKills = playerKills.Count(k => k.IsHeadshot),
                TotalDamageDealt = playerDamages.Sum(d => d.DamageAmount),
                FirstKills = playerKills.Count(k => k.IsFirstKill),
                WallbangKills = playerKills.Count(k => k.IsWallbang),
                RoundsPlayed = _currentRoundNumber
            };

            matchStats.HeadshotPercentage = matchStats.Kills > 0 ? (float)matchStats.HeadshotKills / matchStats.Kills * 100 : 0;
            matchStats.KDRatio = matchStats.Deaths > 0 ? (float)matchStats.Kills / matchStats.Deaths : matchStats.Kills;
            matchStats.ADR = _currentRoundNumber > 0 ? (float)matchStats.TotalDamageDealt / _currentRoundNumber : 0;

            _playerMatchStats.Add(matchStats);
        }

        var matchStatsList = _playerMatchStats.ToList();
        if (matchStatsList.Any())
        {
            _context.PlayerMatchStats.AddRange(matchStatsList);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Saved {Count} player match stats", matchStatsList.Count);
        }
    }

    #region Hostage Event Handlers
    
    private void OnHostageFollows(Source1HostageFollowsEvent e)
    {
        ProcessHostageEvent("follows", e.Player, e.Hostage);
    }
    
    private void OnHostageHurt(Source1HostageHurtEvent e)
    {
        ProcessHostageEvent("hurt", e.Player, e.Hostage);
    }
    
    private void OnHostageKilled(Source1HostageKilledEvent e)
    {
        ProcessHostageEvent("killed", e.Player, e.Hostage);
    }
    
    private void OnHostageRescued(Source1HostageRescuedEvent e)
    {
        ProcessHostageEvent("rescued", e.Player, e.Hostage);
    }
    
    private void OnHostageStopsFollowing(Source1HostageStopsFollowingEvent e)
    {
        ProcessHostageEvent("stops_following", e.Player, e.Hostage);
    }
    
    private void OnHostageRescuedAll(Source1HostageRescuedAllEvent e)
    {
        LogGameEvent(_demo, "hostage_rescued_all", "All hostages rescued", true);
    }
    
    private void OnHostageCallForHelp(Source1HostageCallForHelpEvent e)
    {
        ProcessHostageEvent("call_for_help", null, e.Hostage);
    }
    
    private void ProcessHostageEvent(string eventType, CCSPlayerController? player, int? hostageId)
    {
        if (_demo == null || _currentDemoFile == null) return;
        
        var playerModel = player != null ? GetOrCreatePlayer(player) : null;
        
        var hostageEvent = new Models.HostageEvent
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound?.Id,
            PlayerId = playerModel?.Id,
            Tick = _demo.CurrentDemoTick.Value,
            GameTime = (float)_demo.CurrentGameTime.Value,
            EventType = eventType,
            HostageEntityId = hostageId ?? 0,
            Team = player?.CSTeamNum.ToString(),
            RoundNumber = GetDisplayRoundNumber(),
            Description = $"{(player?.PlayerName ?? "Unknown")} {eventType.Replace("_", " ")} hostage {hostageId}"
        };
        
        var playerPos = player?.PlayerPawn?.Origin;
        if (playerPos != null)
        {
            hostageEvent.PositionX = (decimal)playerPos.Value.X;
            hostageEvent.PositionY = (decimal)playerPos.Value.Y;
            hostageEvent.PositionZ = (decimal)playerPos.Value.Z;
        }
        
        _hostageEvents.Add(hostageEvent);
        LogGameEvent(_demo, $"hostage_{eventType}", hostageEvent.Description);
    }
    
    #endregion
    
    #region Additional Player Event Handlers
    
    private void OnPlayerJump(Source1PlayerJumpEvent e)
    {
        ProcessPlayerBehaviorEvent("jump", e.Player);
    }
    
    private void OnPlayerBlind(Source1PlayerBlindEvent e)
    {
        if (_demo == null || _currentDemoFile == null || _currentRound == null || e.Player == null) return;
        
        var playerModel = GetOrCreatePlayer(e.Player);
        var attackerModel = e.Attacker != null ? GetOrCreatePlayer(e.Attacker) : null;
        
        var flashEvent = new Models.FlashEvent
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = GetDisplayRoundNumber(), // Use display round number
            FlashedPlayer = playerModel,
            FlasherPlayer = attackerModel,
            Tick = _demo.CurrentDemoTick.Value,
            GameTime = (float)_demo.CurrentGameTime.Value,
            FlashDuration = e.BlindDuration,
            FlashedPlayerTeam = e.Player.CSTeamNum.ToString(),
            FlasherPlayerTeam = e.Attacker?.CSTeamNum.ToString(),
            IsTeamFlash = e.Attacker?.CSTeamNum == e.Player.CSTeamNum,
            RoundNumber = GetDisplayRoundNumber(),
            Description = $"{e.Player.PlayerName} flashed by {e.Attacker?.PlayerName ?? "unknown"} for {e.BlindDuration:F2}s"
        };
        
        var playerPos = e.Player.PlayerPawn?.Origin;
        if (playerPos != null)
        {
            flashEvent.FlashedPlayerPositionX = (decimal)playerPos.Value.X;
            flashEvent.FlashedPlayerPositionY = (decimal)playerPos.Value.Y;
            flashEvent.FlashedPlayerPositionZ = (decimal)playerPos.Value.Z;
        }
        
        // Note: Flasher position would need to be tracked via grenade trajectory
        // For now, we're focusing on flashed player position
        
        _flashEvents.Add(flashEvent);
        LogGameEvent(_demo, "player_blind", flashEvent.Description);
    }
    
    private void OnPlayerFootstep(Source1PlayerFootstepEvent e)
    {
        ProcessPlayerBehaviorEvent("footstep", e.Player);

        // Track player positions on footstep for continuous tracking
        TrackPlayerPositions();
    }
    
    private void OnPlayerSpawn(Source1PlayerSpawnEvent e)
    {
        ProcessPlayerBehaviorEvent("spawn", e.Player);
        LogGameEvent(_demo, "player_spawn", $"{e.Player?.PlayerName} spawned", true);

        // CRITICAL: Track player positions immediately on spawn to capture spawn locations
        TrackPlayerPositions();
    }
    
    private void OnPlayerFallDamage(Source1PlayerFalldamageEvent e)
    {
        ProcessPlayerBehaviorEvent("fall_damage", e.Player);
    }
    
    private void OnPlayerChangeName(Source1PlayerChangenameEvent e)
    {
        LogGameEvent(_demo, "player_change_name", $"Player {e.Oldname} changed name to {e.Newname}");
    }
    
    private void OnPlayerScore(Source1PlayerScoreEvent e)
    {
        LogGameEvent(_demo, "player_score", $"Player scored");
    }
    
    private void OnSwitchTeam(Source1SwitchTeamEvent e)
    {
        LogGameEvent(_demo, "switch_team", "Player switched team");
    }
    
    private void ProcessPlayerBehaviorEvent(string behaviorType, CCSPlayerController? player)
    {
        if (_demo == null || _currentDemoFile == null || player == null) return;
        
        var playerModel = GetOrCreatePlayer(player);
        
        var behaviorEvent = new Models.PlayerBehaviorEvent
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound?.Id,
            PlayerId = playerModel.Id,
            Tick = _demo.CurrentDemoTick.Value,
            GameTime = (float)_demo.CurrentGameTime.Value,
            BehaviorType = behaviorType,
            Team = player.CSTeamNum.ToString(),
            RoundNumber = GetDisplayRoundNumber(),
            Description = $"{player.PlayerName} {behaviorType.Replace("_", " ")}"
        };
        
        var playerPos = player.PlayerPawn?.Origin;
        if (playerPos != null)
        {
            behaviorEvent.PositionX = (decimal)playerPos.Value.X;
            behaviorEvent.PositionY = (decimal)playerPos.Value.Y;
            behaviorEvent.PositionZ = (decimal)playerPos.Value.Z;
        }
        
        if (player.PlayerPawn != null)
        {
            // Calculate velocity from the player's velocity vector
            var velocity = player.PlayerPawn.Velocity;
            behaviorEvent.VelocityX = (decimal)velocity.X;
            behaviorEvent.VelocityY = (decimal)velocity.Y;
            behaviorEvent.VelocityZ = (decimal)velocity.Z;
            behaviorEvent.Speed = (float)Math.Sqrt(velocity.X * velocity.X + velocity.Y * velocity.Y);
            behaviorEvent.WeaponName = player.PlayerPawn.ActiveWeapon?.GetType().Name;
        }
        
        _playerBehaviorEvents.Add(behaviorEvent);
    }
    
    #endregion
    
    #region Additional Weapon Event Handlers
    
    private void OnWeaponFireOnEmpty(Source1WeaponFireOnEmptyEvent e)
    {
        ProcessWeaponStateEvent("fire_on_empty", e.Player, e.Player?.PlayerPawn?.ActiveWeapon?.GetType().Name);
    }
    
    private void OnInspectWeapon(Source1InspectWeaponEvent e)
    {
        ProcessWeaponStateEvent("inspect", e.Player, e.Player?.PlayerPawn?.ActiveWeapon?.GetType().Name);
    }
    
    private void OnSilencerDetach(Source1SilencerDetachEvent e)
    {
        ProcessWeaponStateEvent("silencer_detach", e.Player, e.Player?.PlayerPawn?.ActiveWeapon?.GetType().Name);
    }
    
    private void OnSilencerOff(Source1SilencerOffEvent e)
    {
        ProcessWeaponStateEvent("silencer_off", e.Player, e.Player?.PlayerPawn?.ActiveWeapon?.GetType().Name);
    }
    
    private void OnSilencerOn(Source1SilencerOnEvent e)
    {
        ProcessWeaponStateEvent("silencer_on", e.Player, e.Player?.PlayerPawn?.ActiveWeapon?.GetType().Name);
    }
    
    #endregion
    
    #region Grenade Event Handlers
    
    private void OnGrenadeThrown(Source1GrenadeThrownEvent e)
    {
        if (_demo == null || _currentDemoFile == null || _currentRound == null || e.Player == null) return;
        
        var playerModel = GetOrCreatePlayer(e.Player);
        
        var grenadeTrajectory = new Models.GrenadeTrajectory
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = GetDisplayRoundNumber(), // Use display round number
            PlayerId = playerModel.Id,
            ThrowTick = _demo.CurrentDemoTick.Value,
            ThrowTime = _demo.CurrentGameTime.Value,
            GrenadeType = e.Weapon,
            Team = ((int)e.Player.CSTeamNum).ToString(),
            Description = $"{e.Player.PlayerName} threw {e.Weapon}"
        };
        
        var playerPos = e.Player.PlayerPawn?.Origin;
        if (playerPos != null)
        {
            grenadeTrajectory.ThrowPositionX = (decimal)playerPos.Value.X;
            grenadeTrajectory.ThrowPositionY = (decimal)playerPos.Value.Y;
            grenadeTrajectory.ThrowPositionZ = (decimal)playerPos.Value.Z;
        }
        
        var eyeAngles = e.Player.PlayerPawn?.EyeAngles;
        if (eyeAngles != null)
        {
            grenadeTrajectory.ThrowAngleX = (decimal)eyeAngles.Value.Pitch;
            grenadeTrajectory.ThrowAngleY = (decimal)eyeAngles.Value.Yaw;
        }
        
        var velocity = e.Player.PlayerPawn?.Velocity;
        if (velocity != null)
        {
            grenadeTrajectory.ThrowVelocityX = (decimal)velocity.X;
            grenadeTrajectory.ThrowVelocityY = (decimal)velocity.Y;
            grenadeTrajectory.ThrowVelocityZ = (decimal)velocity.Z;
        }
        
        _grenadeTrajectories.Add(grenadeTrajectory);
        LogGameEvent(_demo, "grenade_thrown", grenadeTrajectory.Description);
    }
    
    private void OnGrenadeBounce(Source1GrenadeBounceEvent e)
    {
        if (_demo == null) return;
        LogGameEvent(_demo, "grenade_bounce", "Grenade bounced");
    }
    
    #endregion
    
    #region Decoy Event Handlers
    
    private void OnDecoyDetonate(Source1DecoyDetonateEvent e)
    {
        ProcessGrenadeDetonate("decoy", e.X, e.Y, e.Z, e.Player);
    }
    
    private void OnDecoyStarted(Source1DecoyStartedEvent e)
    {
        LogGameEvent(_demo, "decoy_started", $"Decoy started at ({e.X:F2}, {e.Y:F2}, {e.Z:F2})");
    }
    
    private void OnDecoyFiring(Source1DecoyFiringEvent e)
    {
        LogGameEvent(_demo, "decoy_firing", $"Decoy firing at ({e.X:F2}, {e.Y:F2}, {e.Z:F2})");
    }
    
    #endregion
    
    #region Inferno Event Handlers
    
    private void OnInfernoStartBurn(Source1InfernoStartburnEvent e)
    {
        if (_demo == null || _currentDemoFile == null) return;
        
        // Skip the first two rounds for ESEA/FACEIT demos (warmup and knife round)
        if (ShouldSkipRound()) return;
        
        ProcessInfernoEvent("start", null, e.Entityid, e.X, e.Y, e.Z);
        LogGameEvent(_demo, "inferno_startburn", "Fire started burning");
    }
    
    private void OnInfernoExpire(Source1InfernoExpireEvent e)
    {
        if (_demo == null || _currentDemoFile == null) return;
        
        // Skip the first two rounds for ESEA/FACEIT demos (warmup and knife round)
        if (ShouldSkipRound()) return;
        
        ProcessInfernoEvent("expire", null, e.Entityid, e.X, e.Y, e.Z);
        LogGameEvent(_demo, "inferno_expire", "Fire expired");
    }
    
    private void OnInfernoExtinguish(Source1InfernoExtinguishEvent e)
    {
        if (_demo == null || _currentDemoFile == null) return;
        
        // Skip the first two rounds for ESEA/FACEIT demos (warmup and knife round)
        if (ShouldSkipRound()) return;
        
        ProcessInfernoEvent("extinguish", null, e.Entityid, e.X, e.Y, e.Z);
        LogGameEvent(_demo, "inferno_extinguish", "Fire extinguished");
    }
    
    private void OnMolotovDetonate(Source1MolotovDetonateEvent e)
    {
        if (_demo == null || _currentDemoFile == null) return;
        
        // Skip the first two rounds for ESEA/FACEIT demos (warmup and knife round)
        if (ShouldSkipRound()) return;
        
        ProcessGrenadeDetonate("molotov", e.X, e.Y, e.Z, e.Player);
        ProcessInfernoEvent("molotov_detonate", e.Player, 0, e.X, e.Y, e.Z, "molotov");
    }
    
    private void ProcessInfernoEvent(string eventType, CCSPlayerController? player, int entityId, float x, float y, float z, string grenadeType = "molotov")
    {
        if (_demo == null || _currentDemoFile == null) return;
        
        var playerModel = player != null ? GetOrCreatePlayer(player) : null;
        
        var roundNumber = GetDisplayRoundNumber();
        
        var infernoEvent = new Models.InfernoEvent
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = roundNumber,  // Store round number initially, will be resolved to database ID later
            ThrowerPlayerId = playerModel?.Id,
            StartTick = _demo.CurrentDemoTick.Value,
            StartTime = (float)_demo.CurrentGameTime.Value,
            EventType = eventType,
            GrenadeType = grenadeType,
            InfernoEntityId = entityId,
            OriginX = (decimal)x,
            OriginY = (decimal)y,
            OriginZ = (decimal)z,
            Team = player?.CSTeamNum.ToString() ?? "Unknown",
            RoundNumber = roundNumber,
            Description = $"{(player?.PlayerName ?? "Unknown")} {grenadeType} {eventType.Replace("_", " ")} at ({x:F2}, {y:F2}, {z:F2})"
        };
        
        if (eventType == "extinguish" && player != null)
        {
            infernoEvent.ExtinguishedByPlayerId = playerModel?.Id;
        }
        
        _infernoEvents.Add(infernoEvent);
        LogGameEvent(_demo, $"inferno_{eventType}", infernoEvent.Description);
    }
    
    #endregion
    
    #region Smoke Event Handlers
    
    private void OnSmokegrenadeExpired(Source1SmokegrenadeExpiredEvent e)
    {
        if (_demo == null || _currentDemoFile == null) return;
        
        var smokeCloud = new Models.SmokeCloud
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRoundNumber > 0 ? _currentRoundNumber : null, // Use round number for foreign key resolution, null if invalid
            EndTick = _demo.CurrentDemoTick.Value,
            EndTime = _demo.CurrentGameTime.Value,
            CenterX = (decimal)e.X,
            CenterY = (decimal)e.Y,
            CenterZ = (decimal)e.Z,
            Phase = "expired",
            Team = "Unknown", // Default team value since smoke expired events don't include player info
            RoundNumber = GetDisplayRoundNumber(),
            Description = $"Smoke expired at ({e.X:F2}, {e.Y:F2}, {e.Z:F2})"
        };
        
        _smokeClouds.Add(smokeCloud);
        LogGameEvent(_demo, "smoke_expired", smokeCloud.Description);
    }
    
    #endregion
    
    #region Round Event Handlers
    
    private void OnRoundFreezeEnd(Source1RoundFreezeEndEvent e)
    {
        LogGameEvent(_demo, "round_freeze_end", "Round freeze time ended", true);
    }
    
    private void OnRoundMvp(Source1RoundMvpEvent e)
    {
        LogGameEvent(_demo, "round_mvp", $"MVP: {e.Player?.PlayerName} (Reason: {e.Reason})", true);
    }
    
    private void OnRoundTimeWarning(Source1RoundTimeWarningEvent e)
    {
        LogGameEvent(_demo, "round_time_warning", "Round time warning", true);
    }
    
    private void OnRoundOfficiallyEnded(Source1RoundOfficiallyEndedEvent e)
    {
        LogGameEvent(_demo, "round_officially_ended", "Round officially ended", true);
    }
    
    private void OnRoundAnnounceMatchPoint(Source1RoundAnnounceMatchPointEvent e)
    {
        LogGameEvent(_demo, "round_announce_match_point", "Match point announced", true);
    }
    
    private void OnRoundAnnounceFinal(Source1RoundAnnounceFinalEvent e)
    {
        LogGameEvent(_demo, "round_announce_final", "Final round announced", true);
    }
    
    private void OnRoundAnnounceLastRoundHalf(Source1RoundAnnounceLastRoundHalfEvent e)
    {
        LogGameEvent(_demo, "round_announce_last_round_half", "Last round of half announced", true);
    }
    
    private void OnRoundAnnounceMatchStart(Source1RoundAnnounceMatchStartEvent e)
    {
        LogGameEvent(_demo, "round_announce_match_start", "Match start announced", true);
    }
    
    private void OnRoundAnnounceWarmup(Source1RoundAnnounceWarmupEvent e)
    {
        LogGameEvent(_demo, "round_announce_warmup", "Warmup announced", true);
    }
    
    #endregion
    
    #region Item Event Handlers
    
    private void OnItemPickupSlerp(Source1ItemPickupSlerpEvent e)
    {
        ProcessEconomyEvent("pickup_slerp", e.Player, "item");
    }
    
    private void OnItemPickupFailed(Source1ItemPickupFailedEvent e)
    {
        ProcessEconomyEvent("pickup_failed", e.Player, e.Item);
    }
    
    private void OnItemRemove(Source1ItemRemoveEvent e)
    {
        ProcessEconomyEvent("remove", e.Player, e.Item);
    }
    
    private void OnAmmoPickup(Source1AmmoPickupEvent e)
    {
        ProcessEconomyEvent("ammo_pickup", e.Player, "ammo");
    }
    
    private void OnDefuserDropped(Source1DefuserDroppedEvent e)
    {
        ProcessEconomyEvent("defuser_dropped", GetPlayerFromEntityId(e.Entityid), "defuse_kit");
    }
    
    private void OnDefuserPickup(Source1DefuserPickupEvent e)
    {
        ProcessEconomyEvent("defuser_pickup", e.Player, "defuse_kit");
    }
    
    #endregion
    
    #region Additional Zone Event Handlers
    
    private void OnEnterRescueZone(Source1EnterRescueZoneEvent e)
    {
        ProcessZoneEvent("enter_rescuezone", "rescuezone", e.Player);
    }
    
    private void OnExitRescueZone(Source1ExitRescueZoneEvent e)
    {
        ProcessZoneEvent("exit_rescuezone", "rescuezone", e.Player);
    }
    
    private void OnBuyTimeEnded(Source1BuytimeEndedEvent e)
    {
        LogGameEvent(_demo, "buytime_ended", "Buy time ended", true);
    }
    
    #endregion
    
    #region Communication Event Handlers
    
    private void OnPlayerRadio(Source1PlayerRadioEvent e)
    {
        if (_demo == null || _currentDemoFile == null || e.Player == null) return;
        
        var playerModel = GetOrCreatePlayer(e.Player);
        
        var radioCommand = new Models.RadioCommand
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound?.Id,
            PlayerId = playerModel.Id,
            Tick = _demo.CurrentDemoTick.Value,
            GameTime = (float)_demo.CurrentGameTime.Value,
            Command = e.Slot.ToString(),
            Team = ((int)e.Player.CSTeamNum).ToString(),
            RoundNumber = _currentRoundNumber
        };
        
        var playerPos = e.Player.PlayerPawn?.Origin;
        if (playerPos != null)
        {
            radioCommand.PositionX = (decimal)playerPos.Value.X;
            radioCommand.PositionY = (decimal)playerPos.Value.Y;
            radioCommand.PositionZ = (decimal)playerPos.Value.Z;
        }
        
        _radioCommands.Add(radioCommand);
        LogGameEvent(_demo, "player_radio", $"{e.Player.PlayerName} used radio command {e.Slot}");
    }
    
    private void OnTeamplayBroadcastAudio(Source1TeamplayBroadcastAudioEvent e)
    {
        LogGameEvent(_demo, "teamplay_broadcast_audio", $"Team audio broadcast: {e.Sound}");
    }
    
    #endregion
    
    #region Vote Event Handlers
    
    private void OnVoteStarted(Source1VoteStartedEvent e)
    {
        LogGameEvent(_demo, "vote_started", $"Vote started by player {e.Initiator}: {e.Issue}", true);
    }
    
    private void OnVoteFailed(Source1VoteFailedEvent e)
    {
        LogGameEvent(_demo, "vote_failed", "Vote failed", true);
    }
    
    private void OnVotePassed(Source1VotePassedEvent e)
    {
        LogGameEvent(_demo, "vote_passed", $"Vote passed: {e.Details}", true);
    }
    
    private void OnVoteChanged(Source1VoteChangedEvent e)
    {
        LogGameEvent(_demo, "vote_changed", $"Vote changed: {e.PotentialVotes}");
    }
    
    private void OnVoteCastYes(Source1VoteCastYesEvent e)
    {
        LogGameEvent(_demo, "vote_cast_yes", $"Player {e.Entityid} (Team {e.Team}) voted yes");
    }
    
    private void OnVoteCastNo(Source1VoteCastNoEvent e)
    {
        LogGameEvent(_demo, "vote_cast_no", $"Player {e.Entityid} (Team {e.Team}) voted no");
    }
    
    #endregion
    
    #region Game State Event Handlers
    
    private void OnGameStart(Source1GameStartEvent e)
    {
        LogGameEvent(_demo, "game_start", "Game started", true);
    }
    
    private void OnGameEnd(Source1GameEndEvent e)
    {
        LogGameEvent(_demo, "game_end", $"Game ended - Winner: {e.Winner}", true);
    }
    
    private void OnWarmupEnd(Source1WarmupEndEvent e)
    {
        LogGameEvent(_demo, "warmup_end", "Warmup ended", true);
    }
    
    private void OnBeginNewMatch(Source1BeginNewMatchEvent e)
    {
        LogGameEvent(_demo, "begin_new_match", "New match beginning", true);
    }
    
    private void OnStartHalftime(Source1StartHalftimeEvent e)
    {
        LogGameEvent(_demo, "start_halftime", "Halftime started", true);
    }
    
    #endregion
    
    #region Server Event Handlers
    
    private void OnServerSpawn(Source1ServerSpawnEvent e)
    {
        LogGameEvent(_demo, "server_spawn", $"Server spawned: {e.Hostname}");
    }
    
    private void OnServerMessage(Source1ServerMessageEvent e)
    {
        LogGameEvent(_demo, "server_message", $"Server message: {e.Text}");
    }
    
    private void OnServerCvar(Source1ServerCvarEvent e)
    {
        LogGameEvent(_demo, "server_cvar", $"Server cvar: {e.Cvarname} = {e.Cvarvalue}");
    }
    
    #endregion
    
    #region Environmental Event Handlers
    
    private void OnDoorOpen(Source1DoorOpenEvent e)
    {
        LogGameEvent(_demo, "door_open", $"Door opened by {e.PlayerPawn?.Controller?.PlayerName ?? "Unknown Player"}");
    }
    
    private void OnDoorClose(Source1DoorCloseEvent e)
    {
        LogGameEvent(_demo, "door_close", $"Door closed by {e.PlayerPawn?.Controller?.PlayerName ?? "Unknown Player"}");
    }
    
    private void OnBreakBreakable(Source1BreakBreakableEvent e)
    {
        LogGameEvent(_demo, "break_breakable", $"Breakable broken by {e.PlayerPawn?.Controller?.PlayerName ?? "Unknown Player"}");
    }
    
    private void OnBreakProp(Source1BreakPropEvent e)
    {
        LogGameEvent(_demo, "break_prop", $"Prop broken by {e.PlayerPawn?.Controller?.PlayerName ?? "Unknown Player"}");
    }
    
    #endregion
    
    #region Special Event Handlers
    
    private void OnPlayerAvengedTeammate(Source1PlayerAvengedTeammateEvent e)
    {
        LogGameEvent(_demo, "player_avenged_teammate", $"{e.Avenger?.PlayerName} avenged {e.AvengedPlayer?.PlayerName}");
    }
    
    private void OnBotTakeover(Source1BotTakeoverEvent e)
    {
        LogGameEvent(_demo, "bot_takeover", $"Bot takeover by {e.Player?.PlayerName}");
    }
    
    private void OnPlayerGivenC4(Source1PlayerGivenC4Event e)
    {
        LogGameEvent(_demo, "player_given_c4", $"{e.Player?.PlayerName} given C4", true);
    }
    
    #endregion
    
    #region Entity Event Handlers
    
    // Entity lifecycle tracking will be added when correct API is identified
    
    #endregion
    
    #region Critical Missing Source1 Game Events
    
    private void OnBulletDamage(Source1BulletDamageEvent e)
    {
        if (_demo == null || _currentDemoFile == null || _currentRound == null) return;
        
        var bulletImpact = new Models.BulletImpact
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = GetDisplayRoundNumber(), // Use display round number
            Tick = _demo.CurrentDemoTick.Value,
            GameTime = (float)_demo.CurrentGameTime.Value,
            Weapon = e.AttackerPawn?.ActiveWeapon?.EconItem?.Name ?? "unknown"
        };
        
        // Try to get player information if available
        var shooterController = e.Attacker;
        if (shooterController != null)
        {
            bulletImpact.PlayerId = GetOrCreatePlayer(shooterController).Id;
        }
        
        _bulletImpacts.Add(bulletImpact);
        LogGameEvent(_demo, "bullet_damage", $"Bullet damage with {e.AttackerPawn?.ActiveWeapon?.EconItem?.Name ?? "unknown weapon"}");
    }
    
    private void OnBulletImpact(Source1BulletImpactEvent e)
    {
        if (_demo == null || _currentDemoFile == null || _currentRound == null || e.Player == null) return;
        
        var playerModel = GetOrCreatePlayer(e.Player);
        
        var bulletImpact = new Models.BulletImpact
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = GetDisplayRoundNumber(), // Use display round number
            PlayerId = playerModel.Id,
            Tick = _demo.CurrentDemoTick.Value,
            GameTime = (float)_demo.CurrentGameTime.Value,
            ImpactPositionX = (decimal)e.X,
            ImpactPositionY = (decimal)e.Y,
            ImpactPositionZ = (decimal)e.Z
        };
        
        _bulletImpacts.Add(bulletImpact);
        LogGameEvent(_demo, "bullet_impact", $"{e.Player.PlayerName} bullet impact at ({e.X:F2}, {e.Y:F2}, {e.Z:F2})");
    }
    
    private void OnPlayerShoot(Source1PlayerShootEvent e)
    {
        if (_demo == null || _currentDemoFile == null || _currentRound == null || e.Player == null) return;
        
        var playerModel = GetOrCreatePlayer(e.Player);
        
        var weaponFire = new Models.WeaponFire
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = GetDisplayRoundNumber(), // Use display round number
            PlayerId = playerModel.Id,
            Tick = _demo.CurrentDemoTick.Value,
            GameTime = (float)_demo.CurrentGameTime.Value,
            Weapon = e.Weapon.ToString(),
            Team = ((int)e.Player.CSTeamNum).ToString()
        };
        
        var playerPos = e.Player.PlayerPawn?.Origin;
        if (playerPos != null)
        {
            weaponFire.PositionX = (decimal)playerPos.Value.X;
            weaponFire.PositionY = (decimal)playerPos.Value.Y;
            weaponFire.PositionZ = (decimal)playerPos.Value.Z;
        }
        
        _weaponFires.Add(weaponFire);
        LogGameEvent(_demo, "player_shoot", $"{e.Player.PlayerName} shot {e.Weapon}");
    }
    
    private void OnPlayerSound(Source1PlayerSoundEvent e)
    {
        LogGameEvent(_demo, "player_sound", $"Player sound event");
    }
    
    private void OnPlayerPing(Source1PlayerPingEvent e)
    {
        if (_demo == null || _currentDemoFile == null || e.Player == null) return;
        
        var playerModel = GetOrCreatePlayer(e.Player);
        
        var tacticalEvent = new Models.TacticalEvent
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound?.Id,
            InitiatorPlayerId = playerModel.Id,
            Tick = _demo.CurrentDemoTick.Value,
            GameTime = (float)_demo.CurrentGameTime.Value,
            EventType = "ping",
            TargetArea = $"Position ({e.X:F2}, {e.Y:F2}, {e.Z:F2})",
            RoundNumber = GetDisplayRoundNumber(),
            Description = $"{e.Player.PlayerName} pinged location ({e.X:F2}, {e.Y:F2}, {e.Z:F2})"
        };
        
        _tacticalEvents.Add(tacticalEvent);
        LogGameEvent(_demo, "player_ping", tacticalEvent.Description);
    }
    
    private void OnPlayerPingStop(Source1PlayerPingStopEvent e)
    {
        LogGameEvent(_demo, "player_ping_stop", $"Player ping stopped");
    }
    
    private void OnWeaponZoomRifle(Source1WeaponZoomRifleEvent e)
    {
        ProcessWeaponStateEvent("zoom_rifle", e.Player, e.Player?.PlayerPawn?.ActiveWeapon?.GetType().Name);
    }
    
    private void OnWeaponHudSelection(Source1WeaponhudSelectionEvent e)
    {
        ProcessWeaponStateEvent("hud_selection", e.Player, e.Player?.PlayerPawn?.ActiveWeapon?.GetType().Name);
    }
    
    private void OnGameInit(Source1GameInitEvent e)
    {
        LogGameEvent(_demo, "game_init", "Game initialized", true);
    }
    
    private void OnGamePhaseChanged(Source1GamePhaseChangedEvent e)
    {
        LogGameEvent(_demo, "game_phase_changed", "Game phase changed", true);
    }
    
    private void OnCsIntermission(Source1CsIntermissionEvent e)
    {
        LogGameEvent(_demo, "cs_intermission", "CS intermission started", true);
    }
    
    private void OnCsMatchEndRestart(Source1CsMatchEndRestartEvent e)
    {
        LogGameEvent(_demo, "cs_match_end_restart", "Match end restart", true);
    }
    
    private void OnCsPreRestart(Source1CsPreRestartEvent e)
    {
        LogGameEvent(_demo, "cs_pre_restart", "Pre-restart event", true);
    }
    
    private void OnCsRoundFinalBeep(Source1CsRoundFinalBeepEvent e)
    {
        LogGameEvent(_demo, "cs_round_final_beep", "Round final beep", true);
    }
    
    private void OnCsRoundStartBeep(Source1CsRoundStartBeepEvent e)
    {
        LogGameEvent(_demo, "cs_round_start_beep", "Round start beep", true);
    }
    
    private void OnCsWinPanelMatch(Source1CsWinPanelMatchEvent e)
    {
        LogGameEvent(_demo, "cs_win_panel_match", "Match win panel displayed", true);
    }
    
    private void OnCsWinPanelRound(Source1CsWinPanelRoundEvent e)
    {
        LogGameEvent(_demo, "cs_win_panel_round", "Round win panel displayed", true);
    }
    
    private void OnTagrenadeDetonate(Source1TagrenadeDetonateEvent e)
    {
        ProcessGrenadeDetonate("tagrenade", e.X, e.Y, e.Z, e.Player);
    }
    
    private void OnOtherDeath(Source1OtherDeathEvent e)
    {
        LogGameEvent(_demo, "other_death", $"Non-player entity death");
    }
    
    private void OnBuymenuOpen(Source1BuymenuOpenEvent e)
    {
        LogGameEvent(_demo, "buymenu_open", "Buy menu opened");
    }
    
    private void OnBuymenuClose(Source1BuymenuCloseEvent e)
    {
        ProcessEconomyEvent("buymenu_close", e.Player, "buymenu");
    }
    
    private void OnPlayerActivate(Source1PlayerActivateEvent e)
    {
        LogGameEvent(_demo, "player_activate", $"Player {e.Player?.PlayerName} activated");
    }
    
    #endregion
    
    #region User Message Handlers - ALL 77 Events
    
    private void OnVguiMenuUserMessage(CCSUsrMsg_VGUIMenu e)
    {
        ProcessAdvancedUserMessage("vgui_menu", null, $"VGUI Menu: {e.Name}");
    }
    
    private void OnGeigerUserMessage(CCSUsrMsg_Geiger e)
    {
        ProcessAdvancedUserMessage("geiger", null, $"Geiger: {e.Range}");
    }
    
    private void OnTrainUserMessage(CCSUsrMsg_Train e)
    {
        ProcessAdvancedUserMessage("train", null, $"Train: {e.Train}");
    }
    
    private void OnHudTextUserMessage(CCSUsrMsg_HudText e)
    {
        ProcessAdvancedUserMessage("hud_text", null, $"HUD Text: {e.Text}");
    }
    
    private void OnHudMsgUserMessage(CCSUsrMsg_HudMsg e)
    {
        ProcessAdvancedUserMessage("hud_msg", null, $"HUD Message: {e.Channel}");
    }
    
    private void OnResetHudUserMessage(CCSUsrMsg_ResetHud e)
    {
        ProcessAdvancedUserMessage("reset_hud", null, "Reset HUD");
    }
    
    private void OnGameTitleUserMessage(CCSUsrMsg_GameTitle e)
    {
        ProcessAdvancedUserMessage("game_title", null, "Game title");
    }
    
    private void OnShakeUserMessage(CCSUsrMsg_Shake e)
    {
        ProcessAdvancedUserMessage("shake", null, $"Screen shake: {e.LocalAmplitude}");
    }
    
    private void OnFadeUserMessage(CCSUsrMsg_Fade e)
    {
        ProcessAdvancedUserMessage("fade", null, $"Screen fade: {e.Duration}");
    }
    
    private void OnRumbleUserMessage(CCSUsrMsg_Rumble e)
    {
        ProcessAdvancedUserMessage("rumble", null, $"Controller rumble: {e.Index}");
    }
    
    private void OnCloseCaptionUserMessage(CCSUsrMsg_CloseCaption e)
    {
        ProcessAdvancedUserMessage("close_caption", null, $"Caption: {e.Hash}");
    }
    
    private void OnCloseCaptionDirectUserMessage(CCSUsrMsg_CloseCaptionDirect e)
    {
        ProcessAdvancedUserMessage("close_caption_direct", null, $"Direct caption: {e.Hash}");
    }
    
    private void OnSendAudioUserMessage(CCSUsrMsg_SendAudio e)
    {
        ProcessAdvancedUserMessage("send_audio", null, $"Audio: {e.RadioSound}");
    }
    
    private void OnRawAudioUserMessage(CCSUsrMsg_RawAudio e)
    {
        ProcessAdvancedUserMessage("raw_audio", null, $"Raw audio: {e.Pitch}");
    }
    
    private void OnVoiceMaskUserMessage(CCSUsrMsg_VoiceMask e)
    {
        ProcessAdvancedUserMessage("voice_mask", null, $"Voice mask: {e.PlayerMasks?.Count ?? 0} players");
    }
    
    private void OnRequestStateUserMessage(CCSUsrMsg_RequestState e)
    {
        ProcessAdvancedUserMessage("request_state", null, "State request");
    }
    
    private void OnDamageUserMessage(CCSUsrMsg_Damage e)
    {
        ProcessAdvancedUserMessage("damage", null, $"Damage: {e.Amount} at victim {e.VictimEntindex}");
    }
    
    private void OnRadioTextUserMessage(CCSUsrMsg_RadioText e)
    {
        ProcessAdvancedUserMessage("radio_text", null, $"Radio: {e.MsgName} from client {e.Client}");
    }
    
    private void OnHintTextUserMessage(CCSUsrMsg_HintText e)
    {
        ProcessAdvancedUserMessage("hint_text", null, $"Hint: {e.Message}");
    }
    
    private void OnKeyHintTextUserMessage(CCSUsrMsg_KeyHintText e)
    {
        ProcessAdvancedUserMessage("key_hint_text", null, $"Key hint: {e.Messages.Count} hints");
    }
    
    private void OnProcessSpottedEntityUpdateUserMessage(CCSUsrMsg_ProcessSpottedEntityUpdate e)
    {
        ProcessAdvancedUserMessage("process_spotted_entity_update", null, $"Spotted: {e.NewUpdate}");
    }
    
    private void OnReloadEffectUserMessage(CCSUsrMsg_ReloadEffect e)
    {
        ProcessAdvancedUserMessage("reload_effect", null, $"Reload effect: {e.Entidx}");
    }
    
    private void OnAdjustMoneyUserMessage(CCSUsrMsg_AdjustMoney e)
    {
        ProcessAdvancedUserMessage("adjust_money", null, $"Money adjusted by {e.Amount}");
    }
    
    private void OnStopSpectatorModeUserMessage(CCSUsrMsg_StopSpectatorMode e)
    {
        ProcessAdvancedUserMessage("stop_spectator_mode", null, "Stop spectator mode");
    }
    
    private void OnKillCamUserMessage(CCSUsrMsg_KillCam e)
    {
        ProcessAdvancedUserMessage("kill_cam", null, $"Kill cam: Mode {e.ObsMode}, Target1 {e.FirstTarget}, Target2 {e.SecondTarget}");
    }
    
    private void OnDesiredTimescaleUserMessage(CCSUsrMsg_DesiredTimescale e)
    {
        ProcessAdvancedUserMessage("desired_timescale", null, $"Desired timescale: {e.DesiredTimescale}");
    }
    
    private void OnCurrentTimescaleUserMessage(CCSUsrMsg_CurrentTimescale e)
    {
        ProcessAdvancedUserMessage("current_timescale", null, $"Current timescale: {e.CurTimescale}");
    }
    
    private void OnAchievementEventUserMessage(CCSUsrMsg_AchievementEvent e)
    {
        ProcessAdvancedUserMessage("achievement_event", null, $"Achievement: {e.Achievement}");
    }
    
    private void OnMatchEndConditionsUserMessage(CCSUsrMsg_MatchEndConditions e)
    {
        ProcessAdvancedUserMessage("match_end_conditions", null, $"Match end: {e.Fraglimit} frags, {e.MpMaxrounds} rounds");
    }
    
    private void OnDisconnectToLobbyUserMessage(CCSUsrMsg_DisconnectToLobby e)
    {
        ProcessAdvancedUserMessage("disconnect_to_lobby", null, "Disconnect to lobby");
    }
    
    private void OnPlayerStatsUpdateUserMessage(CCSUsrMsg_PlayerStatsUpdate e)
    {
        ProcessAdvancedUserMessage("player_stats_update", null, $"Player stats: {e.Stats.Count} entries");
    }
    
    private void OnWarmupHasEndedUserMessage(CCSUsrMsg_WarmupHasEnded e)
    {
        ProcessAdvancedUserMessage("warmup_has_ended", null, "Warmup ended");
    }
    
    private void OnClientInfoUserMessage(CCSUsrMsg_ClientInfo e)
    {
        ProcessAdvancedUserMessage("client_info", null, "Client info");
    }
    
    private void OnXRankGetUserMessage(CCSUsrMsg_XRankGet e)
    {
        ProcessAdvancedUserMessage("xrank_get", null, $"XRank get: {e.ModeIdx}");
    }
    
    private void OnXRankUpdUserMessage(CCSUsrMsg_XRankUpd e)
    {
        ProcessAdvancedUserMessage("xrank_upd", null, $"XRank update: {e.ModeIdx}");
    }
    
    private void OnCallVoteFailedUserMessage(CCSUsrMsg_CallVoteFailed e)
    {
        ProcessAdvancedUserMessage("call_vote_failed", null, $"Vote failed: {e.Reason}");
    }
    
    private void OnVoteStartUserMessage(CCSUsrMsg_VoteStart e)
    {
        ProcessAdvancedUserMessage("vote_start", null, $"Vote started: Team {e.Team}, Type {e.VoteType}");
    }
    
    private void OnVotePassUserMessage(CCSUsrMsg_VotePass e)
    {
        ProcessAdvancedUserMessage("vote_pass", null, $"Vote passed: Team {e.Team}, Details {e.DetailsStr}");
    }
    
    private void OnVoteFailedUserMessage(CCSUsrMsg_VoteFailed e)
    {
        ProcessAdvancedUserMessage("vote_failed", null, $"Vote failed: Team {e.Team}, Reason {e.Reason}");
    }
    
    private void OnVoteSetupUserMessage(CCSUsrMsg_VoteSetup e)
    {
        ProcessAdvancedUserMessage("vote_setup", null, $"Vote setup: {e.PotentialIssues.Count} issues");
    }
    
    private void OnServerRankRevealAllUserMessage(CCSUsrMsg_ServerRankRevealAll e)
    {
        ProcessAdvancedUserMessage("server_rank_reveal_all", null, $"Server rank reveal: {e.SecondsTillShutdown}s");
    }
    
    private void OnSendLastKillerDamageToClientUserMessage(CCSUsrMsg_SendLastKillerDamageToClient e)
    {
        ProcessAdvancedUserMessage("send_last_killer_damage", null, $"Last killer: {e.NumHitsGiven} hits given, {e.NumHitsTaken} taken, {e.DamageGiven} damage given, {e.DamageTaken} taken");
    }
    
    private void OnServerRankUpdateUserMessage(CCSUsrMsg_ServerRankUpdate e)
    {
        ProcessAdvancedUserMessage("server_rank_update", null, $"Server rank update: {e.RankUpdate.Count} updates");
    }
    
    private void OnItemPickupUserMessage(CCSUsrMsg_ItemPickup e)
    {
        ProcessAdvancedUserMessage("item_pickup_msg", null, $"Item pickup: {e.Item}");
    }
    
    private void OnShowMenuUserMessage(CCSUsrMsg_ShowMenu e)
    {
        ProcessAdvancedUserMessage("show_menu", null, $"Show menu: {e.BitsValidSlots}, Duration {e.DisplayTime}");
    }
    
    private void OnBarTimeUserMessage(CCSUsrMsg_BarTime e)
    {
        ProcessAdvancedUserMessage("bar_time", null, $"Bar time: {e.Time}");
    }
    
    private void OnAmmoDeniedUserMessage(CCSUsrMsg_AmmoDenied e)
    {
        ProcessAdvancedUserMessage("ammo_denied", null, $"Ammo denied: idx {e.Ammoidx}");
    }
    
    private void OnMarkAchievementUserMessage(CCSUsrMsg_MarkAchievement e)
    {
        ProcessAdvancedUserMessage("mark_achievement", null, $"Mark achievement: {e.Achievement}");
    }
    
    private void OnMatchStatsUpdateUserMessage(CCSUsrMsg_MatchStatsUpdate e)
    {
        ProcessAdvancedUserMessage("match_stats_update", null, "Match stats updated");
    }
    
    private void OnItemDropUserMessage(CCSUsrMsg_ItemDrop e)
    {
        ProcessAdvancedUserMessage("item_drop_msg", null, $"Item drop: {e.Itemid}");
    }
    
    private void OnGlowPropTurnOffUserMessage(CCSUsrMsg_GlowPropTurnOff e)
    {
        ProcessAdvancedUserMessage("glow_prop_turn_off", null, $"Glow prop turn off: {e.Entidx}");
    }
    
    private void OnSendPlayerItemDropsUserMessage(CCSUsrMsg_SendPlayerItemDrops e)
    {
        ProcessAdvancedUserMessage("send_player_item_drops", null, $"Player item drops: {e.EntityUpdates.Count} updates");
    }
    
    private void OnRoundBackupFilenamesUserMessage(CCSUsrMsg_RoundBackupFilenames e)
    {
        ProcessAdvancedUserMessage("round_backup_filenames", null, $"Round backup: {e.Filename}");
    }
    
    private void OnSendPlayerItemFoundUserMessage(CCSUsrMsg_SendPlayerItemFound e)
    {
        ProcessAdvancedUserMessage("send_player_item_found", null, $"Player item found: {e.Iteminfo}");
    }
    
    private void OnReportHitUserMessage(CCSUsrMsg_ReportHit e)
    {
        ProcessAdvancedUserMessage("report_hit", null, $"Hit reported at ({e.PosX}, {e.PosY}, {e.PosZ})");
    }
    
    private void OnXpUpdateUserMessage(CCSUsrMsg_XpUpdate e)
    {
        ProcessAdvancedUserMessage("xp_update", null, $"XP update: {e.Data}");
    }
    
    private void OnQuestProgressUserMessage(CCSUsrMsg_QuestProgress e)
    {
        ProcessAdvancedUserMessage("quest_progress", null, $"Quest progress: {e.QuestId}");
    }
    
    private void OnScoreLeaderboardDataUserMessage(CCSUsrMsg_ScoreLeaderboardData e)
    {
        ProcessAdvancedUserMessage("score_leaderboard_data", null, $"Leaderboard: {e.Data}");
    }
    
    private void OnPlayerDecalDigitalSignatureUserMessage(CCSUsrMsg_PlayerDecalDigitalSignature e)
    {
        ProcessAdvancedUserMessage("player_decal_digital_signature", null, $"Player decal signature: {e.Data}");
    }
    
    private void OnWeaponSoundUserMessage(CCSUsrMsg_WeaponSound e)
    {
        ProcessAdvancedUserMessage("weapon_sound", null, $"Weapon sound: {e.Sound} from entity {e.Entidx} at ({e.OriginX}, {e.OriginY}, {e.OriginZ})");
    }
    
    private void OnUpdateScreenHealthBarUserMessage(CCSUsrMsg_UpdateScreenHealthBar e)
    {
        ProcessAdvancedUserMessage("update_screen_health_bar", null, $"Health bar: {e.Entidx}");
    }
    
    private void OnEntityOutlineHighlightUserMessage(CCSUsrMsg_EntityOutlineHighlight e)
    {
        ProcessAdvancedUserMessage("entity_outline_highlight", null, $"Entity outlined: {e.Entidx}");
    }
    
    private void OnSsuiUserMessage(CCSUsrMsg_SSUI e)
    {
        ProcessAdvancedUserMessage("ssui", null, $"SSUI: {e.Show}");
    }
    
    private void OnSurvivalStatsUserMessage(CCSUsrMsg_SurvivalStats e)
    {
        ProcessAdvancedUserMessage("survival_stats", null, $"Survival stats for XUID: {e.Xuid}");
    }
    
    private void OnEndOfMatchAllPlayersDataUserMessage(CCSUsrMsg_EndOfMatchAllPlayersData e)
    {
        ProcessAdvancedUserMessage("end_of_match_all_players_data", null, $"End match data: {e.Allplayerdata.Count} players");
    }
    
    private void OnPostRoundDamageReportUserMessage(CCSUsrMsg_PostRoundDamageReport e)
    {
        ProcessAdvancedUserMessage("post_round_damage_report", null, $"Post round damage: Given {e.GivenHealthRemoved}, Taken {e.TakenHealthRemoved}");
    }
    
    private void OnRoundEndReportDataUserMessage(CCSUsrMsg_RoundEndReportData e)
    {
        ProcessAdvancedUserMessage("round_end_report_data", null, $"Round end data: {e.AllRerEventData.Count} events");
    }
    
    private void OnCurrentRoundOddsUserMessage(CCSUsrMsg_CurrentRoundOdds e)
    {
        ProcessAdvancedUserMessage("current_round_odds", null, $"Round odds: {e.Odds}%");
    }
    
    private void OnDeepStatsUserMessage(CCSUsrMsg_DeepStats e)
    {
        ProcessAdvancedUserMessage("deep_stats", null, $"Deep stats: {e.Stats}");
    }
    
    private void OnShootInfoUserMessage(CCSUsrMsg_ShootInfo e)
    {
        ProcessAdvancedUserMessage("shoot_info", null, $"Shoot info at frame {e.FrameNumber}");
    }
    
    private void ProcessAdvancedUserMessage(string messageType, CCSPlayerController? targetPlayer, string content)
    {
        if (_demo == null || _currentDemoFile == null) return;
        
        var targetPlayerModel = targetPlayer != null ? GetOrCreatePlayer(targetPlayer) : null;
        
        var advancedUserMessage = new Models.AdvancedUserMessage
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound?.Id,
            TargetPlayerId = targetPlayerModel?.Id,
            Tick = _demo.CurrentDemoTick.Value,
            GameTime = (float)_demo.CurrentGameTime.Value,
            MessageType = messageType,
            MessageContent = content,
            Team = targetPlayerModel?.Team ?? "Unknown",
            RoundNumber = GetDisplayRoundNumber(),
            Description = $"{messageType}: {content}"
        };
        
        _advancedUserMessages.Add(advancedUserMessage);
    }
    
    #endregion
    
    /* Temporarily disabled - Entity Event Handlers
    #region Entity Event Handlers - Temporarily Disabled
    
    // Enhanced: Real-time player state tracking via entity events
    private void OnPlayerStateChanged(CCSPlayerPawn pawn, dynamic oldState, dynamic newState)
    {
        if (_demo == null || _currentDemoFile == null || pawn.Controller == null) return;

        var player = GetOrCreatePlayer(pawn.Controller);
        
        // Track state changes for advanced analytics
        var stateChange = new Models.EntityPropertyChange
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound?.Id,
            PlayerId = player.Id,
            Tick = _demo.CurrentDemoTick.Value,
            GameTime = (float)_demo.CurrentGameTime.Value,
            EntityType = "CCSPlayerPawn",
            PropertyName = "PlayerState",
            OldValue = $"Health:{oldState.Health} Armor:{oldState.ArmorValue}",
            NewValue = $"Health:{newState.Health} Armor:{newState.ArmorValue}",
            Team = pawn.Controller.CSTeamNum.ToString(),
            RoundNumber = GetDisplayRoundNumber(),
            Description = $"{pawn.Controller.PlayerName} state changed"
        };
        
        _entityPropertyChanges.Add(stateChange);
    }
    
    private void OnWeaponStateChanged(CCSWeaponBase weapon, dynamic oldState, dynamic newState)
    {
        if (_demo == null || _currentDemoFile == null || weapon.OwnerEntity?.Value == null) return;

        // Get the owner player from the weapon's OwnerEntity
        var ownerEntityIndex = weapon.OwnerEntity.Value;
        var ownerPlayer = _demo.Players.FirstOrDefault(p => p.EntityIndex.Value == ownerEntityIndex);
        
        if (ownerPlayer == null) return;
        
        var playerModel = GetOrCreatePlayer(ownerPlayer);

        var weaponState = new Models.WeaponState
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound?.Id,
            PlayerId = playerModel.Id,
            Tick = _demo.CurrentDemoTick.Value,
            GameTime = (float)_demo.CurrentGameTime.Value,
            WeaponName = weapon.GetType().Name,
            AmmoClip = newState.Clip1,
            AmmoReserve = 0, // Clip2 might not be available
            EventType = "state_change",
            Team = ownerPlayer.CSTeamNum.ToString(),
            RoundNumber = GetDisplayRoundNumber(),
            Description = $"Weapon state changed: {weapon.GetType().Name} clip: {oldState.Clip1} -> {newState.Clip1}"
        };

        _weaponStates.Add(weaponState);
    }
    
    private void OnGrenadeEntityCreated(CBaseCSGrenadeProjectile grenade)
    {
        if (_demo == null || _currentDemoFile == null) return;

        var lifecycle = new Models.EntityLifecycle
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound?.Id,
            Tick = _demo.CurrentDemoTick.Value,
            GameTime = (float)_demo.CurrentGameTime.Value,
            EntityType = grenade.GetType().Name,
            EventType = "created",
            Team = "Unknown",
            RoundNumber = GetDisplayRoundNumber(),
            Description = $"Grenade entity created: {grenade.GetType().Name}"
        };

        _entityLifecycles.Add(lifecycle);
    }
    
    private void OnGrenadeEntityDestroyed(CBaseCSGrenadeProjectile grenade)
    {
        if (_demo == null || _currentDemoFile == null) return;

        var lifecycle = new Models.EntityLifecycle
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound?.Id,
            Tick = _demo.CurrentDemoTick.Value,
            GameTime = (float)_demo.CurrentGameTime.Value,
            EntityType = grenade.GetType().Name,
            EventType = "destroyed",
            Team = "Unknown",
            RoundNumber = GetDisplayRoundNumber(),
            Description = $"Grenade entity destroyed: {grenade.GetType().Name}"
        };

        _entityLifecycles.Add(lifecycle);
    }
    
    private void OnEconomicItemCreated(CEconEntity item)
    {
        if (_demo == null || _currentDemoFile == null) return;

        var lifecycle = new Models.EntityLifecycle
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound?.Id,
            Tick = _demo.CurrentDemoTick.Value,
            GameTime = (float)_demo.CurrentGameTime.Value,
            EntityType = item.GetType().Name,
            Event = "created",
            Team = "Unknown",
            RoundNumber = GetDisplayRoundNumber(),
            Description = $"Economic item created: {item.GetType().Name}"
        };

        _entityLifecycles.Add(lifecycle);
    }
    
    private void OnEconomicItemDestroyed(CEconEntity item)
    {
        if (_demo == null || _currentDemoFile == null) return;

        var lifecycle = new Models.EntityLifecycle
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound?.Id,
            Tick = _demo.CurrentDemoTick.Value,
            GameTime = (float)_demo.CurrentGameTime.Value,
            EntityType = item.GetType().Name,
            Event = "destroyed",
            Team = "Unknown",
            RoundNumber = GetDisplayRoundNumber(),
            Description = $"Economic item destroyed: {item.GetType().Name}"
        };

        _entityLifecycles.Add(lifecycle);
    }
    
    #endregion
    End of temporarily disabled Entity Event Handlers */
    
    private CCSPlayerController? GetPlayerFromEntityId(int entityId)
    {
        return _demo?.Players.FirstOrDefault(p => p.EntityIndex.Value == entityId);
    }

    private string? ExtractMapFromFilename(string fileName)
    {
        if (string.IsNullOrEmpty(fileName)) return null;
        
        var lowerFileName = fileName.ToLower();
        
        // List of known CS2 maps
        var knownMaps = new[]
        {
            "de_ancient", "de_anubis", "de_dust2", "de_inferno", "de_mirage", 
            "de_nuke", "de_overpass", "de_train", "de_vertigo", "de_cache",
            "ancient", "anubis", "dust2", "inferno", "mirage", 
            "nuke", "overpass", "train", "vertigo", "cache"
        };
        
        foreach (var map in knownMaps)
        {
            if (lowerFileName.Contains(map))
            {
                // Return the de_ version if it's just the map name
                if (map.StartsWith("de_"))
                    return map;
                else
                    return $"de_{map}";
            }
        }
        
        return null;
    }

    // CRITICAL: Tick-based event handler for continuous player position tracking
    private void OnTickEnd(object? sender, EventArgs e)
    {
        // Call our position tracking method on every tick
        // This ensures we capture positions continuously, including spawns
        TrackPlayerPositions();
    }
}

// Helper class for tracking player game state  
public class PlayerGameState
{
    public Vector? LastPosition { get; set; }
    public QAngle? LastViewAngle { get; set; }
    public Vector? LastVelocity { get; set; }
    public int Health { get; set; }
    public int Armor { get; set; }
    public string? CurrentWeapon { get; set; }
    public List<string> Inventory { get; set; } = new();
    public int Money { get; set; }
    public bool IsAlive { get; set; }
    public bool IsBlind { get; set; }
    public float FlashDuration { get; set; }
    public bool IsScoped { get; set; }
    public bool IsCrouching { get; set; }
    public bool HasDefuseKit { get; set; }
    public bool HasHelmet { get; set; }
    public DateTime LastUpdate { get; set; }
}