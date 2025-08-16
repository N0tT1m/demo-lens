using DemoFile;
using DemoFile.Game.Cs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using CS2DemoParser.Data;
using CS2DemoParser.Models;
using System.Collections.Concurrent;
using Concentus;

namespace CS2DemoParser.Services;

public class DemoParserService
{
    private readonly CS2DemoContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DemoParserService> _logger;
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
    
    // Advanced entity tracking collections
    private readonly ConcurrentBag<Models.EntityLifecycle> _entityLifecycles = new();
    private readonly ConcurrentBag<Models.EntityInteraction> _entityInteractions = new();
    private readonly ConcurrentBag<Models.EntityVisibility> _entityVisibilities = new();
    private readonly ConcurrentBag<Models.EntityEffect> _entityEffects = new();
    private readonly ConcurrentBag<Models.DroppedItem> _droppedItems = new();
    private readonly ConcurrentBag<Models.SmokeCloud> _smokeClouds = new();
    private readonly ConcurrentBag<Models.FireArea> _fireAreas = new();
    
    // Entity tracking state
    private readonly Dictionary<int, Models.DroppedItem> _activeDroppedItems = new();
    private readonly Dictionary<int, Models.SmokeCloud> _activeSmokeClouds = new();
    private readonly Dictionary<int, Models.FireArea> _activeFireAreas = new();
    
    // Game state tracking collections
    private readonly ConcurrentBag<Models.TeamState> _teamStates = new();
    private readonly ConcurrentBag<Models.EconomyState> _economyStates = new();
    private readonly ConcurrentBag<Models.MapControl> _mapControls = new();
    private readonly ConcurrentBag<Models.TacticalEvent> _tacticalEvents = new();
    
    // Game state tracking variables
    private readonly Dictionary<string, int> _teamConsecutiveLosses = new() { { "CT", 0 }, { "T", 0 } };
    private readonly Dictionary<string, int> _teamConsecutiveWins = new() { { "CT", 0 }, { "T", 0 } };
    private readonly Dictionary<string, List<int>> _teamRoundMoney = new() { { "CT", new() }, { "T", new() } };
    
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
    
    // Voice tracking state
    private readonly Dictionary<int, Models.VoiceCommunication> _activeVoiceComms = new();
    private readonly List<Models.VoiceCommunication> _recentComms = new();
    private readonly Dictionary<ulong, List<CMsgVoiceAudio>> _voiceDataPerSteamId = new();
    
    // Advanced tracking state
    private readonly Dictionary<int, Dictionary<string, object?>> _entityPropertyCache = new();
    private readonly Dictionary<int, Models.InfernoEvent> _activeInfernos = new();
    
    // Match Statistics tracking collections
    private readonly ConcurrentBag<Models.MatchStatistic> _matchStatistics = new();
    private readonly ConcurrentBag<Models.EndOfMatchData> _endOfMatchData = new();
    private readonly ConcurrentBag<Models.TeamPerformance> _teamPerformances = new();
    private readonly ConcurrentBag<Models.EconomicAnalysis> _economicAnalyses = new();
    private readonly ConcurrentBag<Models.MapStatistic> _mapStatistics = new();
    private readonly ConcurrentBag<Models.RoundOutcome> _roundOutcomes = new();
    
    // Match statistics state tracking
    private readonly Dictionary<string, Dictionary<string, object>> _teamMatchStats = new()
    {
        { "CT", new Dictionary<string, object>() },
        { "T", new Dictionary<string, object>() }
    };
    private readonly List<Models.RoundOutcome> _roundHistory = new();
    private DateTime _matchStartTime;
    private DateTime _matchEndTime;

    private Models.DemoFile? _currentDemoFile;
    private Models.Match? _currentMatch;
    private Models.Round? _currentRound;
    private int _currentRoundNumber = 0;
    private readonly Dictionary<int, Models.PlayerRoundStats> _currentRoundStats = new();
    private CsDemoParser? _demo;

    public DemoParserService(CS2DemoContext context, IConfiguration configuration, ILogger<DemoParserService> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    #region Real-time Processing

    public async Task<bool> ParseHttpBroadcastAsync(string broadcastUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            // Ensure trailing slash
            if (!broadcastUrl.EndsWith("/"))
                broadcastUrl += "/";

            var demo = new CsDemoParser();
            SetupEventHandlers(demo);

            var httpReader = HttpBroadcastReader.Create(demo, new Uri(broadcastUrl));

            _logger.LogInformation("Starting HTTP broadcast parsing from {Url}", broadcastUrl);
            await httpReader.StartReadingAsync(cancellationToken);

            var tickInterval = TimeSpan.Zero;
            demo.PacketEvents.SvcServerInfo += e =>
            {
                tickInterval = TimeSpan.FromSeconds(e.TickInterval);
                _logger.LogInformation("Server info received. Tick rate: {TickRate}", 1 / e.TickInterval);
            };

            // Process broadcast in real-time
            while (!cancellationToken.IsCancellationRequested)
            {
                if (!await httpReader.MoveNextAsync(cancellationToken))
                {
                    _logger.LogInformation("HTTP broadcast ended");
                    break;
                }

                // Save data in batches to avoid overwhelming the database
                if (_gameEvents.Count > 1000 || _playerPositions.Count > 5000)
                {
                    await SaveBatchDataAsync();
                }
            }

            // Final save
            await SaveBatchDataAsync();
            _logger.LogInformation("HTTP broadcast parsing completed");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing HTTP broadcast from {Url}", broadcastUrl);
            return false;
        }
    }

    public async Task<bool> ParseDemoParallelAsync(string filePath, int maxDegreeOfParallelism = -1)
    {
        try
        {
            var demo = new CsDemoParser();
            SetupEventHandlers(demo);
            _demo = demo;

            await SetupDemoFileAsync(new FileInfo(filePath));

            _logger.LogInformation("Starting demo parsing for file {FilePath}", filePath);

            using var fileStream = File.OpenRead(filePath);
            var reader = DemoFileReader.Create(demo, fileStream);

            // Read the entire demo
            await reader.ReadAllAsync();

            await SaveAllDataAsync();
            _logger.LogInformation("Demo parsing completed");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing demo file {FilePath}", filePath);
            return false;
        }
    }


    private async Task SaveBatchDataAsync()
    {
        try
        {
            // Save collected data in batches
            if (_gameEvents.Any())
            {
                _context.GameEvents.AddRange(_gameEvents.ToArray());
                _gameEvents.Clear();
            }

            if (_playerPositions.Any())
            {
                _context.PlayerPositions.AddRange(_playerPositions.ToArray());
                _playerPositions.Clear();
            }

            if (_entityPropertyChanges.Any())
            {
                _context.EntityPropertyChanges.AddRange(_entityPropertyChanges.ToArray());
                _entityPropertyChanges.Clear();
            }

            if (_advancedUserMessages.Any())
            {
                _context.AdvancedUserMessages.AddRange(_advancedUserMessages.ToArray());
                _advancedUserMessages.Clear();
            }

            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving batch data");
        }
    }

    #endregion

    public async Task<bool> ParseDemoAsync(string filePath)
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

            await CreateDemoFileRecord(fileInfo);

            SetupEventHandlers(_demo);

            _logger.LogInformation("Parsing demo file...");
            var reader = DemoFileReader.Create(_demo, fileStream);
            await reader.ReadAllAsync();

            // Finish any active voice communications
            FinishVoiceCommunications();
            
            // Analyze communication patterns
            AnalyzeCommunicationPatterns();

            await SaveDataToDatabase();

            _logger.LogInformation("Successfully parsed demo file: {FilePath}", filePath);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing demo file: {FilePath}", filePath);
            return false;
        }
    }

    private async Task CreateDemoFileRecord(FileInfo fileInfo)
    {
        _currentDemoFile = new Models.DemoFile
        {
            FileName = fileInfo.Name,
            FilePath = fileInfo.FullName,
            FileSize = fileInfo.Length,
            CreatedAt = fileInfo.CreationTime,
            ParsedAt = DateTime.UtcNow
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
        
        // Grenade events (only actual existing events)
        demo.Source1GameEvents.HegrenadeDetonate += OnHegrenadeDetonate;
        demo.Source1GameEvents.FlashbangDetonate += OnFlashbangDetonate;
        demo.Source1GameEvents.SmokegrenadeDetonate += OnSmokegrenadeDetonate;
        demo.Source1GameEvents.TagrenadeDetonate += OnDecoyDetonate;
        
        // Core bomb events
        demo.Source1GameEvents.BombPlanted += OnBombPlanted;
        demo.Source1GameEvents.BombDefused += OnBombDefused;
        demo.Source1GameEvents.BombExploded += OnBombExploded;
        
        // Core player events
        demo.Source1GameEvents.PlayerConnect += OnPlayerConnect;
        demo.Source1GameEvents.PlayerDisconnect += OnPlayerDisconnect;
        demo.Source1GameEvents.PlayerTeam += OnPlayerTeam;
        
        // Advanced entity tracking events
        demo.Source1GameEvents.ItemPickup += OnItemPickup;
        demo.Source1GameEvents.ItemRemove += OnItemRemove;
        demo.Source1GameEvents.WeaponFire += OnAdvancedWeaponFire;
        
        // Entity creation/destruction events
        demo.EntityEvents.CBaseEntity.Create += OnEntityCreate;
        demo.EntityEvents.CBaseEntity.Delete += OnEntityDelete;
        
        // Advanced Entity Callbacks for real-time monitoring
        SetupAdvancedEntityCallbacks(demo);
        
        // Tick processing for continuous tracking
        demo.Source1GameEvents.RoundFreezeEnd += OnRoundFreezeEnd;
        demo.Source1GameEvents.RoundStart += OnRoundStartGameState;
        demo.Source1GameEvents.RoundEnd += OnRoundEndGameState;
        
        // Voice and communication event handlers
        demo.PacketEvents.SvcVoiceData += OnVoiceData;
        demo.PacketEvents.SvcVoiceInit += OnVoiceInit;
        demo.Source1GameEvents.PlayerChat += OnPlayerChat;
        demo.Source1GameEvents.PlayerRadio += OnPlayerRadio;
        demo.UserMessageEvents.RadioText += OnRadioText;
        demo.BaseUserMessageEvents.UserMessageSayText2 += OnSayText2;
        demo.Source1GameEvents.TeamplayBroadcastAudio += OnTeamBroadcastAudio;
        
        // Temporary Entity Events
        demo.TempEntityEvents.EffectDispatch += OnEffectDispatch;
        demo.TempEntityEvents.ArmorRicochet += OnArmorRicochet;
        demo.TempEntityEvents.Explosion += OnExplosion;
        demo.TempEntityEvents.Dust += OnDust;
        demo.TempEntityEvents.Smoke += OnSmoke;
        demo.TempEntityEvents.Sparks += OnSparks;
        demo.TempEntityEvents.MuzzleFlash += OnMuzzleFlash;
        demo.TempEntityEvents.Impact += OnImpact;
        demo.TempEntityEvents.BeamEntPoint += OnBeamEntPoint;
        demo.TempEntityEvents.BeamEnts += OnBeamEnts;
        demo.TempEntityEvents.BeamPoints += OnBeamPoints;
        demo.TempEntityEvents.BeamRing += OnBeamRing;
        demo.TempEntityEvents.BSPDecal += OnBSPDecal;
        demo.TempEntityEvents.Decal += OnDecal;
        demo.TempEntityEvents.WorldDecal += OnWorldDecal;
        demo.TempEntityEvents.PlayerDecal += OnPlayerDecal;
        
        // Enhanced Game Events
        demo.Source1GameEvents.HostageFollows += OnHostageFollows;
        demo.Source1GameEvents.HostageHurt += OnHostageHurt;
        demo.Source1GameEvents.HostageKilled += OnHostageKilled;
        demo.Source1GameEvents.HostageRescued += OnHostageRescued;
        demo.Source1GameEvents.HostageStopsFollowing += OnHostageStopsFollowing;
        demo.Source1GameEvents.HostageCallForHelp += OnHostageCallForHelp;
        demo.Source1GameEvents.EnterBuyzone += OnEnterBuyzone;
        demo.Source1GameEvents.ExitBuyzone += OnExitBuyzone;
        demo.Source1GameEvents.EnterBombzone += OnEnterBombzone;
        demo.Source1GameEvents.ExitBombzone += OnExitBombzone;
        demo.Source1GameEvents.InspectWeapon += OnInspectWeapon;
        demo.Source1GameEvents.ItemPickupFailed += OnItemPickupFailed;
        demo.Source1GameEvents.PlayerFootstep += OnPlayerFootstep;
        demo.Source1GameEvents.PlayerJump += OnPlayerJump;
        demo.Source1GameEvents.PlayerSound += OnPlayerSound;
        
        // Advanced User Messages
        demo.UserMessageEvents.Damage += OnDamageReport;
        demo.UserMessageEvents.SendLastKillerDamageToClient += OnLastKillerDamageReport;
        demo.UserMessageEvents.PlayerStatsUpdate += OnPlayerStatsUpdate;
        demo.UserMessageEvents.AdjustMoney += OnMoneyAdjustment;
        demo.UserMessageEvents.VoteStart += OnVoteStart;
        demo.UserMessageEvents.VotePass += OnVotePass;
        demo.UserMessageEvents.VoteFailed += OnVoteFailed;
        demo.UserMessageEvents.CallVoteFailed += OnCallVoteFailed;
        demo.UserMessageEvents.ServerRankUpdate += OnServerRankUpdate;
        demo.UserMessageEvents.XRankGet += OnXRankGet;
        demo.UserMessageEvents.XRankUpd += OnXRankUpdate;
        demo.UserMessageEvents.ItemPickup += OnItemPickupMessage;
        demo.UserMessageEvents.AchievementEvent += OnAchievementEvent;
    }

    private void OnRoundStart(Source1RoundStartEvent e)
    {
        if (_demo == null || _currentDemoFile == null) return;

        _currentRoundNumber++;

        if (_currentMatch == null)
        {
            _currentMatch = new Models.Match
            {
                DemoFileId = _currentDemoFile.Id,
                MapName = _demo.Map,
                StartTime = DateTime.UtcNow,
                IsFinished = false
            };

            _currentDemoFile.MapName = _demo.Map;
            _currentDemoFile.TotalTicks = _demo.CurrentDemoTick.Value;
            _currentDemoFile.TickRate = _demo.TickRate;
            
            // Initialize match statistics tracking
            InitializeMatchStatistics();
        }

        _currentRound = new Models.Round
        {
            DemoFileId = _currentDemoFile.Id,
            MatchId = _currentMatch?.Id ?? 0,
            RoundNumber = _currentRoundNumber,
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
                var roundStats = new Models.PlayerRoundStats
                {
                    PlayerId = GetOrCreatePlayer(player).Id,
                    RoundId = _currentRound.Id,
                    StartMoney = player.InGameMoneyServices?.Account ?? 0,
                    Health = player.PlayerPawn?.Health ?? 0,
                    Armor = player.PlayerPawn?.ArmorValue ?? 0,
                    HasHelmet = player.PlayerPawn?.HasHelmet ?? false,
                    HasDefuseKit = player.PlayerPawn?.HasDefuser ?? false,
                    IsAlive = player.PlayerPawn?.Health > 0
                };

                _currentRoundStats[player.Slot] = roundStats;
                _playerRoundStats.Add(roundStats);
            }
        }

        LogGameEvent(_demo, "round_start", "Round started", true);
    }

    private void OnRoundEnd(Source1RoundEndEvent e)
    {
        if (_demo == null || _currentRound == null) return;

        _currentRound.EndTick = _demo.CurrentDemoTick.Value;
        _currentRound.EndTime = DateTime.UtcNow;
        _currentRound.Duration = (float)(DateTime.UtcNow - _currentRound.StartTime).TotalSeconds;
        _currentRound.WinnerTeam = e.Winner.ToString();
        _currentRound.EndReason = e.Reason.ToString();
        _currentRound.CTScore = _demo.TeamCounterTerrorist?.Score ?? 0;
        _currentRound.TScore = _demo.TeamTerrorist?.Score ?? 0;

        foreach (var player in demo.Players)
        {
            if (player.PlayerName != null && _currentRoundStats.TryGetValue(player.Slot, out var roundStats))
            {
                roundStats.EndMoney = player.InGameMoneyServices?.Account ?? 0;
                roundStats.Health = player.PlayerPawn?.Health ?? 0;
                roundStats.Armor = player.PlayerPawn?.ArmorValue ?? 0;
                roundStats.IsAlive = player.PlayerPawn?.Health > 0;
            }
        }

        LogGameEvent(_demo, "round_end", $"Round ended - Winner: {e.Winner}, Reason: {e.Reason}", true);
    }

    private void OnPlayerDeath(Source1PlayerDeathEvent e)
    {
        if (_demo == null || _currentDemoFile == null || _currentRound == null || e.Player == null) return;

        var victim = GetOrCreatePlayer(e.Player);
        var killer = e.Attacker != null ? GetOrCreatePlayer(e.Attacker) : null;
        var assister = e.Assister != null ? GetOrCreatePlayer(e.Assister) : null;

        var kill = new Models.Kill
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound.Id,
            Tick = _demo.CurrentDemoTick.Value,
            GameTime = (float)_demo.CurrentGameTime.TotalSeconds,
            KillerId = killer?.Id,
            VictimId = victim.Id,
            AssisterId = assister?.Id,
            Weapon = e.Weapon,
            IsHeadshot = e.Headshot,
            IsWallbang = e.Penetrated > 0,
            Penetration = e.Penetrated,
            IsNoScope = e.Noscope,
            ThroughSmoke = e.Thrusmoke,
            AttackerBlind = e.Attackerblind,
            KillerPositionX = (decimal)(e.AttackerPawn?.CBodyComponent?.SceneNode?.AbsOrigin?.X ?? 0),
            KillerPositionY = (decimal)(e.AttackerPawn?.CBodyComponent?.SceneNode?.AbsOrigin?.Y ?? 0),
            KillerPositionZ = (decimal)(e.AttackerPawn?.CBodyComponent?.SceneNode?.AbsOrigin?.Z ?? 0),
            VictimPositionX = (decimal)(e.PlayerPawn?.CBodyComponent?.SceneNode?.AbsOrigin?.X ?? 0),
            VictimPositionY = (decimal)(e.PlayerPawn?.CBodyComponent?.SceneNode?.AbsOrigin?.Y ?? 0),
            VictimPositionZ = (decimal)(e.PlayerPawn?.CBodyComponent?.SceneNode?.AbsOrigin?.Z ?? 0),
            KillerHealth = e.AttackerPawn?.Health ?? 0,
            KillerArmor = e.AttackerPawn?.ArmorValue ?? 0,
            VictimHealth = 0,
            VictimArmor = e.PlayerPawn?.ArmorValue ?? 0,
            KillerTeam = e.Attacker?.TeamNum.ToString(),
            VictimTeam = e.Player.TeamNum.ToString(),
            IsTeamKill = e.Attacker?.TeamNum == e.Player.TeamNum
        };

        if (e.AttackerPawn?.CBodyComponent?.SceneNode?.AbsOrigin != null && 
            e.PlayerPawn?.CBodyComponent?.SceneNode?.AbsOrigin != null)
        {
            var dx = (e.AttackerPawn.CBodyComponent.SceneNode.AbsOrigin.X - e.PlayerPawn.CBodyComponent.SceneNode.AbsOrigin.X);
            var dy = (e.AttackerPawn.CBodyComponent.SceneNode.AbsOrigin.Y - e.PlayerPawn.CBodyComponent.SceneNode.AbsOrigin.Y);
            var dz = (e.AttackerPawn.CBodyComponent.SceneNode.AbsOrigin.Z - e.PlayerPawn.CBodyComponent.SceneNode.AbsOrigin.Z);
            kill.Distance = (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        _kills.Add(kill);

        if (_currentRoundStats.TryGetValue(e.Player.Slot, out var victimStats))
        {
            victimStats.Deaths++;
        }

        if (killer != null && _currentRoundStats.TryGetValue(killer.Slot, out var killerStats))
        {
            killerStats.Kills++;
        }

        if (assister != null && _currentRoundStats.TryGetValue(assister.Slot, out var assisterStats))
        {
            assisterStats.Assists++;
        }

        LogGameEvent(demo, "player_death", $"{e.Player.PlayerName} killed by {e.Attacker?.PlayerName} with {e.Weapon}");
    }

    private void OnPlayerHurt(Source1PlayerHurtEvent e)
    {
        var demo = e.GameEventParser;
        if (demo == null || _currentDemoFile == null || _currentRound == null || e.Player == null) return;

        var victim = GetOrCreatePlayer(e.Player);
        var attacker = e.Attacker != null ? GetOrCreatePlayer(e.Attacker) : null;

        var damage = new Models.Damage
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound.Id,
            Tick = demo.CurrentDemoTick.Value,
            GameTime = (float)(demo.CurrentGameTime?.TotalSeconds ?? 0),
            AttackerId = attacker?.Id,
            VictimId = victim.Id,
            Weapon = e.Weapon,
            DamageAmount = e.DmgHealth,
            DamageArmor = e.DmgArmor,
            Health = e.Health,
            Armor = e.Armor,
            AttackerPositionX = (decimal)(e.AttackerPawn?.CBodyComponent?.SceneNode?.AbsOrigin?.X ?? 0),
            AttackerPositionY = (decimal)(e.AttackerPawn?.CBodyComponent?.SceneNode?.AbsOrigin?.Y ?? 0),
            AttackerPositionZ = (decimal)(e.AttackerPawn?.CBodyComponent?.SceneNode?.AbsOrigin?.Z ?? 0),
            VictimPositionX = (decimal)(e.PlayerPawn?.CBodyComponent?.SceneNode?.AbsOrigin?.X ?? 0),
            VictimPositionY = (decimal)(e.PlayerPawn?.CBodyComponent?.SceneNode?.AbsOrigin?.Y ?? 0),
            VictimPositionZ = (decimal)(e.PlayerPawn?.CBodyComponent?.SceneNode?.AbsOrigin?.Z ?? 0),
            AttackerTeam = e.Attacker?.Team.ToString(),
            VictimTeam = e.Player.Team.ToString(),
            IsTeamDamage = e.Attacker?.Team == e.Player.Team
        };

        if (e.AttackerPawn?.CBodyComponent?.SceneNode?.AbsOrigin != null && 
            e.PlayerPawn?.CBodyComponent?.SceneNode?.AbsOrigin != null)
        {
            var dx = (e.AttackerPawn.CBodyComponent.SceneNode.AbsOrigin.X - e.PlayerPawn.CBodyComponent.SceneNode.AbsOrigin.X);
            var dy = (e.AttackerPawn.CBodyComponent.SceneNode.AbsOrigin.Y - e.PlayerPawn.CBodyComponent.SceneNode.AbsOrigin.Y);
            var dz = (e.AttackerPawn.CBodyComponent.SceneNode.AbsOrigin.Z - e.PlayerPawn.CBodyComponent.SceneNode.AbsOrigin.Z);
            damage.Distance = (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        _damages.Add(damage);

        if (_currentRoundStats.TryGetValue(e.Player.Slot, out var victimStats))
        {
            victimStats.Damage += e.DmgHealth;
        }

        LogGameEvent(demo, "player_hurt", $"{e.Player.PlayerName} hurt by {e.Attacker?.PlayerName} for {e.DmgHealth} damage");
    }

    #region Enhanced Hostage Events

    private void OnHostageFollows(Source1HostageFollowsEvent e)
    {
        var demo = e.GameEventParser;
        if (demo == null || _currentDemoFile == null || _currentRound == null || e.Player == null) return;

        var player = GetOrCreatePlayer(e.Player);
        var hostageEvent = new Models.HostageEvent
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound.Id,
            PlayerId = player.Id,
            Tick = demo.CurrentDemoTick.Value,
            GameTime = (float)(demo.CurrentGameTime?.TotalSeconds ?? 0),
            EventType = "follows",
            HostageEntityId = e.Hostage,
            PositionX = (decimal)(e.PlayerPawn?.CBodyComponent?.SceneNode?.AbsOrigin?.X ?? 0),
            PositionY = (decimal)(e.PlayerPawn?.CBodyComponent?.SceneNode?.AbsOrigin?.Y ?? 0),
            PositionZ = (decimal)(e.PlayerPawn?.CBodyComponent?.SceneNode?.AbsOrigin?.Z ?? 0),
            Team = e.Player.Team.ToString(),
            RoundNumber = _currentRoundNumber,
            WasBeingFollowed = false,
            HostageState = "following",
            Description = $"Hostage {e.Hostage} started following {e.Player.PlayerName}"
        };

        _hostageEvents.Add(hostageEvent);
        LogGameEvent(demo, "hostage_follows", $"Hostage {e.Hostage} follows {e.Player.PlayerName}");
    }

    private void OnHostageHurt(Source1HostageHurtEvent e)
    {
        var demo = e.GameEventParser;
        if (demo == null || _currentDemoFile == null || _currentRound == null || e.Player == null) return;

        var player = GetOrCreatePlayer(e.Player);
        var hostageEvent = new Models.HostageEvent
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound.Id,
            PlayerId = player.Id,
            Tick = demo.CurrentDemoTick.Value,
            GameTime = (float)(demo.CurrentGameTime?.TotalSeconds ?? 0),
            EventType = "hurt",
            HostageEntityId = e.Hostage,
            PositionX = (decimal)(e.PlayerPawn?.CBodyComponent?.SceneNode?.AbsOrigin?.X ?? 0),
            PositionY = (decimal)(e.PlayerPawn?.CBodyComponent?.SceneNode?.AbsOrigin?.Y ?? 0),
            PositionZ = (decimal)(e.PlayerPawn?.CBodyComponent?.SceneNode?.AbsOrigin?.Z ?? 0),
            Team = e.Player.Team.ToString(),
            RoundNumber = _currentRoundNumber,
            HostageState = "hurt",
            Description = $"Hostage {e.Hostage} hurt by {e.Player.PlayerName}"
        };

        _hostageEvents.Add(hostageEvent);
        LogGameEvent(demo, "hostage_hurt", $"Hostage {e.Hostage} hurt by {e.Player.PlayerName}");
    }

    private void OnHostageKilled(Source1HostageKilledEvent e)
    {
        var demo = e.GameEventParser;
        if (demo == null || _currentDemoFile == null || _currentRound == null || e.Player == null) return;

        var player = GetOrCreatePlayer(e.Player);
        var hostageEvent = new Models.HostageEvent
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound.Id,
            PlayerId = player.Id,
            Tick = demo.CurrentDemoTick.Value,
            GameTime = (float)(demo.CurrentGameTime?.TotalSeconds ?? 0),
            EventType = "killed",
            HostageEntityId = e.Hostage,
            PositionX = (decimal)(e.PlayerPawn?.CBodyComponent?.SceneNode?.AbsOrigin?.X ?? 0),
            PositionY = (decimal)(e.PlayerPawn?.CBodyComponent?.SceneNode?.AbsOrigin?.Y ?? 0),
            PositionZ = (decimal)(e.PlayerPawn?.CBodyComponent?.SceneNode?.AbsOrigin?.Z ?? 0),
            Team = e.Player.Team.ToString(),
            RoundNumber = _currentRoundNumber,
            HostageState = "dead",
            Description = $"Hostage {e.Hostage} killed by {e.Player.PlayerName}"
        };

        _hostageEvents.Add(hostageEvent);
        LogGameEvent(demo, "hostage_killed", $"Hostage {e.Hostage} killed by {e.Player.PlayerName}");
    }

    private void OnHostageRescued(Source1HostageRescuedEvent e)
    {
        var demo = e.GameEventParser;
        if (demo == null || _currentDemoFile == null || _currentRound == null || e.Player == null) return;

        var player = GetOrCreatePlayer(e.Player);
        var hostageEvent = new Models.HostageEvent
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound.Id,
            PlayerId = player.Id,
            Tick = demo.CurrentDemoTick.Value,
            GameTime = (float)(demo.CurrentGameTime?.TotalSeconds ?? 0),
            EventType = "rescued",
            HostageEntityId = e.Hostage,
            PositionX = (decimal)(e.PlayerPawn?.CBodyComponent?.SceneNode?.AbsOrigin?.X ?? 0),
            PositionY = (decimal)(e.PlayerPawn?.CBodyComponent?.SceneNode?.AbsOrigin?.Y ?? 0),
            PositionZ = (decimal)(e.PlayerPawn?.CBodyComponent?.SceneNode?.AbsOrigin?.Z ?? 0),
            Team = e.Player.Team.ToString(),
            RoundNumber = _currentRoundNumber,
            WasBeingRescued = true,
            HostageState = "rescued",
            Description = $"Hostage {e.Hostage} rescued by {e.Player.PlayerName} at site {e.Site}"
        };

        _hostageEvents.Add(hostageEvent);
        LogGameEvent(demo, "hostage_rescued", $"Hostage {e.Hostage} rescued by {e.Player.PlayerName}");
    }

    private void OnHostageStopsFollowing(Source1HostageStopsFollowingEvent e)
    {
        var demo = e.GameEventParser;
        if (demo == null || _currentDemoFile == null || _currentRound == null || e.Player == null) return;

        var player = GetOrCreatePlayer(e.Player);
        var hostageEvent = new Models.HostageEvent
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound.Id,
            PlayerId = player.Id,
            Tick = demo.CurrentDemoTick.Value,
            GameTime = (float)(demo.CurrentGameTime?.TotalSeconds ?? 0),
            EventType = "stops_following",
            HostageEntityId = e.Hostage,
            PositionX = (decimal)(e.PlayerPawn?.CBodyComponent?.SceneNode?.AbsOrigin?.X ?? 0),
            PositionY = (decimal)(e.PlayerPawn?.CBodyComponent?.SceneNode?.AbsOrigin?.Y ?? 0),
            PositionZ = (decimal)(e.PlayerPawn?.CBodyComponent?.SceneNode?.AbsOrigin?.Z ?? 0),
            Team = e.Player.Team.ToString(),
            RoundNumber = _currentRoundNumber,
            WasBeingFollowed = true,
            HostageState = "idle",
            Description = $"Hostage {e.Hostage} stopped following {e.Player.PlayerName}"
        };

        _hostageEvents.Add(hostageEvent);
        LogGameEvent(demo, "hostage_stops_following", $"Hostage {e.Hostage} stopped following {e.Player.PlayerName}");
    }

    private void OnHostageCallForHelp(Source1HostageCallForHelpEvent e)
    {
        var demo = e.GameEventParser;
        if (demo == null || _currentDemoFile == null || _currentRound == null) return;

        var hostageEvent = new Models.HostageEvent
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound.Id,
            Tick = demo.CurrentDemoTick.Value,
            GameTime = (float)(demo.CurrentGameTime?.TotalSeconds ?? 0),
            EventType = "call_for_help",
            HostageEntityId = e.Hostage,
            RoundNumber = _currentRoundNumber,
            HostageState = "calling_for_help",
            Description = $"Hostage {e.Hostage} called for help"
        };

        _hostageEvents.Add(hostageEvent);
        LogGameEvent(demo, "hostage_call_for_help", $"Hostage {e.Hostage} called for help");
    }

    #endregion

    #region Zone Events

    private void OnEnterBuyzone(Source1EnterBuyzoneEvent e)
    {
        var demo = e.GameEventParser;
        if (demo == null || _currentDemoFile == null || _currentRound == null || e.Player == null) return;

        var player = GetOrCreatePlayer(e.Player);
        var zoneEvent = new Models.ZoneEvent
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound.Id,
            PlayerId = player.Id,
            Tick = demo.CurrentDemoTick.Value,
            GameTime = (float)(demo.CurrentGameTime?.TotalSeconds ?? 0),
            ZoneType = "buyzone",
            EventType = "enter",
            PositionX = (decimal)(e.PlayerPawn?.CBodyComponent?.SceneNode?.AbsOrigin?.X ?? 0),
            PositionY = (decimal)(e.PlayerPawn?.CBodyComponent?.SceneNode?.AbsOrigin?.Y ?? 0),
            PositionZ = (decimal)(e.PlayerPawn?.CBodyComponent?.SceneNode?.AbsOrigin?.Z ?? 0),
            Team = e.Player.Team.ToString(),
            RoundNumber = _currentRoundNumber,
            Description = $"{e.Player.PlayerName} entered buy zone"
        };

        _zoneEvents.Add(zoneEvent);
        LogGameEvent(demo, "enter_buyzone", $"{e.Player.PlayerName} entered buy zone");
    }

    private void OnExitBuyzone(Source1ExitBuyzoneEvent e)
    {
        var demo = e.GameEventParser;
        if (demo == null || _currentDemoFile == null || _currentRound == null || e.Player == null) return;

        var player = GetOrCreatePlayer(e.Player);
        var zoneEvent = new Models.ZoneEvent
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound.Id,
            PlayerId = player.Id,
            Tick = demo.CurrentDemoTick.Value,
            GameTime = (float)(demo.CurrentGameTime?.TotalSeconds ?? 0),
            ZoneType = "buyzone",
            EventType = "exit",
            PositionX = (decimal)(e.PlayerPawn?.CBodyComponent?.SceneNode?.AbsOrigin?.X ?? 0),
            PositionY = (decimal)(e.PlayerPawn?.CBodyComponent?.SceneNode?.AbsOrigin?.Y ?? 0),
            PositionZ = (decimal)(e.PlayerPawn?.CBodyComponent?.SceneNode?.AbsOrigin?.Z ?? 0),
            Team = e.Player.Team.ToString(),
            RoundNumber = _currentRoundNumber,
            Description = $"{e.Player.PlayerName} exited buy zone"
        };

        _zoneEvents.Add(zoneEvent);
        LogGameEvent(demo, "exit_buyzone", $"{e.Player.PlayerName} exited buy zone");
    }

    private void OnEnterBombzone(Source1EnterBombzoneEvent e)
    {
        var demo = e.GameEventParser;
        if (demo == null || _currentDemoFile == null || _currentRound == null || e.Player == null) return;

        var player = GetOrCreatePlayer(e.Player);
        var zoneEvent = new Models.ZoneEvent
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound.Id,
            PlayerId = player.Id,
            Tick = demo.CurrentDemoTick.Value,
            GameTime = (float)(demo.CurrentGameTime?.TotalSeconds ?? 0),
            ZoneType = "bombzone",
            EventType = "enter",
            PositionX = (decimal)(e.PlayerPawn?.CBodyComponent?.SceneNode?.AbsOrigin?.X ?? 0),
            PositionY = (decimal)(e.PlayerPawn?.CBodyComponent?.SceneNode?.AbsOrigin?.Y ?? 0),
            PositionZ = (decimal)(e.PlayerPawn?.CBodyComponent?.SceneNode?.AbsOrigin?.Z ?? 0),
            Team = e.Player.Team.ToString(),
            RoundNumber = _currentRoundNumber,
            IsCarryingBomb = e.Player.PlayerPawn?.WeaponServices?.Weapons?.Any(w => 
                demo.GetEntityByHandle(w)?.DesignerName?.Contains("c4") == true) ?? false,
            Description = $"{e.Player.PlayerName} entered bomb zone"
        };

        _zoneEvents.Add(zoneEvent);
        LogGameEvent(demo, "enter_bombzone", $"{e.Player.PlayerName} entered bomb zone");
    }

    private void OnExitBombzone(Source1ExitBombzoneEvent e)
    {
        var demo = e.GameEventParser;
        if (demo == null || _currentDemoFile == null || _currentRound == null || e.Player == null) return;

        var player = GetOrCreatePlayer(e.Player);
        var zoneEvent = new Models.ZoneEvent
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound.Id,
            PlayerId = player.Id,
            Tick = demo.CurrentDemoTick.Value,
            GameTime = (float)(demo.CurrentGameTime?.TotalSeconds ?? 0),
            ZoneType = "bombzone",
            EventType = "exit",
            PositionX = (decimal)(e.PlayerPawn?.CBodyComponent?.SceneNode?.AbsOrigin?.X ?? 0),
            PositionY = (decimal)(e.PlayerPawn?.CBodyComponent?.SceneNode?.AbsOrigin?.Y ?? 0),
            PositionZ = (decimal)(e.PlayerPawn?.CBodyComponent?.SceneNode?.AbsOrigin?.Z ?? 0),
            Team = e.Player.Team.ToString(),
            RoundNumber = _currentRoundNumber,
            Description = $"{e.Player.PlayerName} exited bomb zone"
        };

        _zoneEvents.Add(zoneEvent);
        LogGameEvent(demo, "exit_bombzone", $"{e.Player.PlayerName} exited bomb zone");
    }

    #endregion

    #region Weapon and Item Events

    private void OnInspectWeapon(Source1InspectWeaponEvent e)
    {
        var demo = e.GameEventParser;
        if (demo == null || _currentDemoFile == null || _currentRound == null || e.Player == null) return;

        var player = GetOrCreatePlayer(e.Player);
        var gameEvent = new Models.GameEvent
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound.Id,
            PlayerId = player.Id,
            Tick = demo.CurrentDemoTick.Value,
            GameTime = (float)(demo.CurrentGameTime?.TotalSeconds ?? 0),
            EventType = "weapon_inspect",
            EventData = $"{{\"weapon\":\"{e.Player.PlayerPawn?.WeaponServices?.ActiveWeapon?.Value?.DesignerName ?? "unknown"}\"}}",
            PositionX = (decimal)(e.PlayerPawn?.CBodyComponent?.SceneNode?.AbsOrigin?.X ?? 0),
            PositionY = (decimal)(e.PlayerPawn?.CBodyComponent?.SceneNode?.AbsOrigin?.Y ?? 0),
            PositionZ = (decimal)(e.PlayerPawn?.CBodyComponent?.SceneNode?.AbsOrigin?.Z ?? 0),
            Team = e.Player.Team.ToString(),
            RoundNumber = _currentRoundNumber,
            Description = $"{e.Player.PlayerName} inspected weapon"
        };

        _gameEvents.Add(gameEvent);
        LogGameEvent(demo, "inspect_weapon", $"{e.Player.PlayerName} inspected weapon");
    }

    private void OnItemPickupFailed(Source1ItemPickupFailedEvent e)
    {
        var demo = e.GameEventParser;
        if (demo == null || _currentDemoFile == null || _currentRound == null || e.Player == null) return;

        var player = GetOrCreatePlayer(e.Player);
        var gameEvent = new Models.GameEvent
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound.Id,
            PlayerId = player.Id,
            Tick = demo.CurrentDemoTick.Value,
            GameTime = (float)(demo.CurrentGameTime?.TotalSeconds ?? 0),
            EventType = "item_pickup_failed",
            EventData = $"{{\"item\":\"{e.Item}\",\"reason\":\"{e.Reason}\"}}",
            PositionX = (decimal)(e.PlayerPawn?.CBodyComponent?.SceneNode?.AbsOrigin?.X ?? 0),
            PositionY = (decimal)(e.PlayerPawn?.CBodyComponent?.SceneNode?.AbsOrigin?.Y ?? 0),
            PositionZ = (decimal)(e.PlayerPawn?.CBodyComponent?.SceneNode?.AbsOrigin?.Z ?? 0),
            Team = e.Player.Team.ToString(),
            RoundNumber = _currentRoundNumber,
            Description = $"{e.Player.PlayerName} failed to pickup {e.Item}: {e.Reason}"
        };

        _gameEvents.Add(gameEvent);
        LogGameEvent(demo, "item_pickup_failed", $"{e.Player.PlayerName} failed to pickup {e.Item}: {e.Reason}");
    }

    #endregion

    #region Advanced Entity Callbacks

    private void SetupAdvancedEntityCallbacks(CsDemoParser demo)
    {
        // Player position tracking with change detection
        demo.EntityEvents.CCSPlayerPawn.AddChangeCallback(
            player => player.CBodyComponent?.SceneNode?.AbsOrigin,
            OnPlayerPositionChanged);

        // Player health tracking
        demo.EntityEvents.CCSPlayerPawn.AddChangeCallback(
            player => player.Health,
            OnPlayerHealthChanged);

        // Player armor tracking  
        demo.EntityEvents.CCSPlayerPawn.AddChangeCallback(
            player => player.ArmorValue,
            OnPlayerArmorChanged);

        // Weapon state tracking
        demo.EntityEvents.CCSPlayerPawn.AddChangeCallback(
            player => player.WeaponServices?.ActiveWeapon?.Value,
            OnActiveWeaponChanged);

        // Player money tracking
        demo.EntityEvents.CCSPlayerController.AddChangeCallback(
            player => player.InGameMoneyServices?.Account,
            OnPlayerMoneyChanged);

        // Player state tracking (alive/dead)
        demo.EntityEvents.CCSPlayerPawn.AddChangeCallback(
            player => player.LifeState,
            OnPlayerLifeStateChanged);

        // Grenade tracking
        demo.EntityEvents.CBaseCSGrenade.AddChangeCallback(
            grenade => grenade.CBodyComponent?.SceneNode?.AbsOrigin,
            OnGrenadePositionChanged);

        // Smoke grenade tracking
        demo.EntityEvents.CSmokeGrenade.AddChangeCallback(
            smoke => smoke.SmokeEffectTickBegin,
            OnSmokeEffectChanged);

        // Flash effect tracking
        demo.EntityEvents.CCSPlayerPawn.AddChangeCallback(
            player => player.FlashDuration,
            OnFlashEffectChanged);

        // Breakable prop tracking
        demo.EntityEvents.CBreakableProp.AddChangeCallback(
            prop => prop.Health,
            OnBreakablePropHealthChanged);

        // Door state tracking
        demo.EntityEvents.CBasePropDoor.AddChangeCallback(
            door => door.DoorState,
            OnDoorStateChanged);

        // Weapon ammo tracking
        demo.EntityEvents.CBasePlayerWeapon.AddChangeCallback(
            weapon => weapon.Clip1,
            OnWeaponAmmoChanged);

        // Player team tracking
        demo.EntityEvents.CCSPlayerController.AddChangeCallback(
            player => player.TeamNum,
            OnPlayerTeamChanged);

        // Visibility tracking for entities
        demo.EntityEvents.CBaseEntity.AddChangeCallback(
            entity => entity.RenderMode,
            OnEntityVisibilityChanged);

        // Inferno tracking
        demo.EntityEvents.CInferno.AddChangeCallback(
            inferno => inferno.FireCount,
            OnInfernoFireCountChanged);

        demo.EntityEvents.CInferno.AddChangeCallback(
            inferno => inferno.FireLifetime,
            OnInfernoLifetimeChanged);

        demo.EntityEvents.CInferno.AddChangeCallback(
            inferno => inferno.InPostEffectTime,
            OnInfernoPostEffectChanged);

        // Molotov grenade tracking
        demo.EntityEvents.CMolotovGrenade.AddChangeCallback(
            molotov => molotov.CBodyComponent?.SceneNode?.AbsOrigin,
            OnMolotovPositionChanged);

        // Inferno entity creation/deletion
        demo.EntityEvents.CInferno.Create += OnInfernoCreate;
        demo.EntityEvents.CInferno.Delete += OnInfernoDelete;

        // Advanced weapon state tracking
        demo.EntityEvents.CCSWeaponBase.AddChangeCallback(
            weapon => weapon.State,
            OnWeaponStateChanged);

        demo.EntityEvents.CCSWeaponBase.AddChangeCallback(
            weapon => weapon.WeaponMode,
            OnWeaponModeChanged);

        demo.EntityEvents.CCSWeaponBase.AddChangeCallback(
            weapon => weapon.InReload,
            OnWeaponReloadStateChanged);

        demo.EntityEvents.CCSWeaponBase.AddChangeCallback(
            weapon => weapon.SilencerOn,
            OnWeaponSilencerChanged);

        demo.EntityEvents.CCSWeaponBaseGun.AddChangeCallback(
            weapon => weapon.ZoomLevel,
            OnWeaponZoomChanged);

        demo.EntityEvents.CCSWeaponBaseGun.AddChangeCallback(
            weapon => weapon.BurstShotsRemaining,
            OnWeaponBurstChanged);

        // Decoy simulation tracking
        demo.EntityEvents.CDecoyProjectile.AddChangeCallback(
            decoy => decoy.DecoyShotTick,
            OnDecoyShotTickChanged);

        demo.EntityEvents.CDecoyProjectile.Create += OnDecoyCreate;
        demo.EntityEvents.CDecoyProjectile.Delete += OnDecoyDelete;

        demo.EntityEvents.CDecoyGrenade.Create += OnDecoyGrenadeCreate;
        demo.EntityEvents.CDecoyGrenade.Delete += OnDecoyGrenadeDelete;
    }

    private void OnPlayerPositionChanged(CCSPlayerPawn player, Vector? oldPos, Vector? newPos)
    {
        if (_currentDemoFile == null || _currentRound == null || newPos == null || oldPos == null) return;
        if (_demo == null) return;

        var controller = _demo.Players.FirstOrDefault(p => p.PlayerPawn?.EntityIndex == player.EntityIndex);
        if (controller == null) return;

        var distance = oldPos.Value.DistanceTo(newPos.Value);
        if (distance > 0.1f) // Only track significant movement
        {
            var propertyChange = new Models.EntityPropertyChange
            {
                DemoFileId = _currentDemoFile.Id,
                RoundId = _currentRound.Id,
                Tick = _demo.CurrentDemoTick.Value,
                GameTime = (float)_demo.CurrentGameTime.Value,
                EntityId = player.EntityIndex.Value,
                EntityType = "CCSPlayerPawn",
                PropertyName = "Position",
                OldValue = $"{oldPos.Value.X:F2},{oldPos.Value.Y:F2},{oldPos.Value.Z:F2}",
                NewValue = $"{newPos.Value.X:F2},{newPos.Value.Y:F2},{newPos.Value.Z:F2}",
                PlayerId = GetOrCreatePlayer(controller).Id,
                ChangeType = "movement",
                RoundNumber = _currentRoundNumber
            };

            _entityPropertyChanges.Add(propertyChange);

            // Analyze movement patterns
            AnalyzeMovementPatterns(controller, newPos.Value);
        }
    }

    private void OnPlayerHealthChanged(CCSPlayerPawn player, int oldHealth, int newHealth)
    {
        if (_currentDemoFile == null || _currentRound == null) return;
        if (_demo == null) return;

        var controller = _demo.Players.FirstOrDefault(p => p.PlayerPawn?.EntityIndex == player.EntityIndex);
        if (controller == null) return;

        var propertyChange = new Models.EntityPropertyChange
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound.Id,
            Tick = _demo.CurrentDemoTick.Value,
            GameTime = (float)(_demo.CurrentGameTime?.TotalSeconds ?? 0),
            EntityId = player.EntityIndex.Value,
            EntityType = "CCSPlayerPawn",
            PropertyName = "Health",
            OldValue = oldHealth.ToString(),
            NewValue = newHealth.ToString(),
            PlayerId = GetOrCreatePlayer(controller).Id,
            ChangeType = newHealth < oldHealth ? "damage" : "heal",
            RoundNumber = _currentRoundNumber
        };

        _entityPropertyChanges.Add(propertyChange);
    }

    private void OnPlayerArmorChanged(CCSPlayerPawn player, int oldArmor, int newArmor)
    {
        if (_currentDemoFile == null || _currentRound == null) return;
        if (_demo == null) return;

        var controller = _demo.Players.FirstOrDefault(p => p.PlayerPawn?.EntityIndex == player.EntityIndex);
        if (controller == null) return;

        var propertyChange = new Models.EntityPropertyChange
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound.Id,
            Tick = _demo.CurrentDemoTick.Value,
            GameTime = (float)(_demo.CurrentGameTime?.TotalSeconds ?? 0),
            EntityId = player.EntityIndex.Value,
            EntityType = "CCSPlayerPawn",
            PropertyName = "Armor",
            OldValue = oldArmor.ToString(),
            NewValue = newArmor.ToString(),
            PlayerId = GetOrCreatePlayer(controller).Id,
            ChangeType = newArmor < oldArmor ? "armor_damage" : "armor_acquired",
            RoundNumber = _currentRoundNumber
        };

        _entityPropertyChanges.Add(propertyChange);
    }

    private void OnActiveWeaponChanged(CCSPlayerPawn player, CHandle<CBasePlayerWeapon, CsDemoParser>? oldWeapon, CHandle<CBasePlayerWeapon, CsDemoParser>? newWeapon)
    {
        if (_currentDemoFile == null || _currentRound == null) return;
        if (_demo == null) return;

        var controller = _demo.Players.FirstOrDefault(p => p.PlayerPawn?.EntityIndex == player.EntityIndex);
        if (controller == null) return;

        var oldWeaponEntity = oldWeapon?.Value;
        var newWeaponEntity = newWeapon?.Value;

        var propertyChange = new Models.EntityPropertyChange
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound.Id,
            Tick = _demo.CurrentDemoTick.Value,
            GameTime = (float)(_demo.CurrentGameTime?.TotalSeconds ?? 0),
            EntityId = player.EntityIndex.Value,
            EntityType = "CCSPlayerPawn",
            PropertyName = "ActiveWeapon",
            OldValue = oldWeaponEntity?.DesignerName ?? "none",
            NewValue = newWeaponEntity?.DesignerName ?? "none",
            PlayerId = GetOrCreatePlayer(controller).Id,
            ChangeType = "weapon_switch",
            RoundNumber = _currentRoundNumber
        };

        _entityPropertyChanges.Add(propertyChange);
    }

    private void OnPlayerMoneyChanged(CCSPlayerController player, int oldMoney, int newMoney)
    {
        if (_currentDemoFile == null || _currentRound == null) return;
        if (_demo == null) return;

        var propertyChange = new Models.EntityPropertyChange
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound.Id,
            Tick = _demo.CurrentDemoTick.Value,
            GameTime = (float)(_demo.CurrentGameTime?.TotalSeconds ?? 0),
            EntityId = player.EntityIndex.Value,
            EntityType = "CCSPlayerController",
            PropertyName = "Money",
            OldValue = oldMoney.ToString(),
            NewValue = newMoney.ToString(),
            PlayerId = GetOrCreatePlayer(player).Id,
            ChangeType = newMoney > oldMoney ? "money_earned" : "money_spent",
            RoundNumber = _currentRoundNumber
        };

        _entityPropertyChanges.Add(propertyChange);
    }

    private void OnPlayerLifeStateChanged(CCSPlayerPawn player, byte oldState, byte newState)
    {
        if (_currentDemoFile == null || _currentRound == null) return;
        if (_demo == null) return;

        var controller = _demo.Players.FirstOrDefault(p => p.PlayerPawn?.EntityIndex == player.EntityIndex);
        if (controller == null) return;

        var propertyChange = new Models.EntityPropertyChange
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound.Id,
            Tick = _demo.CurrentDemoTick.Value,
            GameTime = (float)(_demo.CurrentGameTime?.TotalSeconds ?? 0),
            EntityId = player.EntityIndex.Value,
            EntityType = "CCSPlayerPawn",
            PropertyName = "LifeState",
            OldValue = oldState.ToString(),
            NewValue = newState.ToString(),
            PlayerId = GetOrCreatePlayer(controller).Id,
            ChangeType = newState == 0 ? "respawn" : "death",
            RoundNumber = _currentRoundNumber
        };

        _entityPropertyChanges.Add(propertyChange);
    }

    private void OnGrenadePositionChanged(CBaseCSGrenade grenade, Vector? oldPos, Vector? newPos)
    {
        if (_currentDemoFile == null || _currentRound == null || newPos == null || oldPos == null) return;
        if (_demo == null) return;

        var distance = oldPos.Value.DistanceTo(newPos.Value);
        if (distance > 0.5f) // Only track significant grenade movement
        {
            var propertyChange = new Models.EntityPropertyChange
            {
                DemoFileId = _currentDemoFile.Id,
                RoundId = _currentRound.Id,
                Tick = _demo.CurrentDemoTick.Value,
                GameTime = (float)(_demo.CurrentGameTime?.TotalSeconds ?? 0),
                EntityId = grenade.EntityIndex.Value,
                EntityType = "CBaseCSGrenade",
                PropertyName = "Position",
                OldValue = $"{oldPos.Value.X:F2},{oldPos.Value.Y:F2},{oldPos.Value.Z:F2}",
                NewValue = $"{newPos.Value.X:F2},{newPos.Value.Y:F2},{newPos.Value.Z:F2}",
                ChangeType = "grenade_movement",
                RoundNumber = _currentRoundNumber
            };

            _entityPropertyChanges.Add(propertyChange);
        }
    }

    private void OnSmokeEffectChanged(CSmokeGrenade smoke, GameTick oldTick, GameTick newTick)
    {
        if (_currentDemoFile == null || _currentRound == null) return;
        if (_demo == null) return;

        var propertyChange = new Models.EntityPropertyChange
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound.Id,
            Tick = _demo.CurrentDemoTick.Value,
            GameTime = (float)(_demo.CurrentGameTime?.TotalSeconds ?? 0),
            EntityId = smoke.EntityIndex.Value,
            EntityType = "CSmokeGrenade",
            PropertyName = "SmokeEffectTickBegin",
            OldValue = oldTick.Value.ToString(),
            NewValue = newTick.Value.ToString(),
            ChangeType = "smoke_effect_start",
            RoundNumber = _currentRoundNumber
        };

        _entityPropertyChanges.Add(propertyChange);
    }

    private void OnFlashEffectChanged(CCSPlayerPawn player, float oldDuration, float newDuration)
    {
        if (_currentDemoFile == null || _currentRound == null) return;
        if (_demo == null) return;

        var controller = _demo.Players.FirstOrDefault(p => p.PlayerPawn?.EntityIndex == player.EntityIndex);
        if (controller == null) return;

        var propertyChange = new Models.EntityPropertyChange
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound.Id,
            Tick = _demo.CurrentDemoTick.Value,
            GameTime = (float)(_demo.CurrentGameTime?.TotalSeconds ?? 0),
            EntityId = player.EntityIndex.Value,
            EntityType = "CCSPlayerPawn",
            PropertyName = "FlashDuration",
            OldValue = oldDuration.ToString("F2"),
            NewValue = newDuration.ToString("F2"),
            PlayerId = GetOrCreatePlayer(controller).Id,
            ChangeType = newDuration > oldDuration ? "flash_start" : "flash_fade",
            RoundNumber = _currentRoundNumber
        };

        _entityPropertyChanges.Add(propertyChange);
    }

    private void OnBreakablePropHealthChanged(CBreakableProp prop, int oldHealth, int newHealth)
    {
        if (_currentDemoFile == null || _currentRound == null) return;
        if (_demo == null) return;

        var propertyChange = new Models.EntityPropertyChange
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound.Id,
            Tick = _demo.CurrentDemoTick.Value,
            GameTime = (float)(_demo.CurrentGameTime?.TotalSeconds ?? 0),
            EntityId = prop.EntityIndex.Value,
            EntityType = "CBreakableProp",
            PropertyName = "Health",
            OldValue = oldHealth.ToString(),
            NewValue = newHealth.ToString(),
            ChangeType = newHealth <= 0 ? "prop_broken" : "prop_damaged",
            RoundNumber = _currentRoundNumber
        };

        _entityPropertyChanges.Add(propertyChange);
    }

    private void OnDoorStateChanged(CBasePropDoor door, uint oldState, uint newState)
    {
        if (_currentDemoFile == null || _currentRound == null) return;
        if (_demo == null) return;

        var propertyChange = new Models.EntityPropertyChange
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound.Id,
            Tick = _demo.CurrentDemoTick.Value,
            GameTime = (float)(_demo.CurrentGameTime?.TotalSeconds ?? 0),
            EntityId = door.EntityIndex.Value,
            EntityType = "CBasePropDoor",
            PropertyName = "DoorState",
            OldValue = oldState.ToString(),
            NewValue = newState.ToString(),
            ChangeType = "door_state_change",
            RoundNumber = _currentRoundNumber
        };

        _entityPropertyChanges.Add(propertyChange);
    }

    private void OnWeaponAmmoChanged(CBasePlayerWeapon weapon, int oldAmmo, int newAmmo)
    {
        if (_currentDemoFile == null || _currentRound == null) return;
        if (_demo == null) return;

        var propertyChange = new Models.EntityPropertyChange
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound.Id,
            Tick = _demo.CurrentDemoTick.Value,
            GameTime = (float)(_demo.CurrentGameTime?.TotalSeconds ?? 0),
            EntityId = weapon.EntityIndex.Value,
            EntityType = "CBasePlayerWeapon",
            PropertyName = "Clip1",
            OldValue = oldAmmo.ToString(),
            NewValue = newAmmo.ToString(),
            ChangeType = newAmmo < oldAmmo ? "ammo_used" : "ammo_reload",
            RoundNumber = _currentRoundNumber
        };

        _entityPropertyChanges.Add(propertyChange);
    }

    private void OnPlayerTeamChanged(CCSPlayerController player, byte oldTeam, byte newTeam)
    {
        if (_currentDemoFile == null || _currentRound == null) return;
        if (_demo == null) return;

        var propertyChange = new Models.EntityPropertyChange
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound.Id,
            Tick = _demo.CurrentDemoTick.Value,
            GameTime = (float)(_demo.CurrentGameTime?.TotalSeconds ?? 0),
            EntityId = player.EntityIndex.Value,
            EntityType = "CCSPlayerController",
            PropertyName = "TeamNum",
            OldValue = oldTeam.ToString(),
            NewValue = newTeam.ToString(),
            PlayerId = GetOrCreatePlayer(player).Id,
            ChangeType = "team_change",
            RoundNumber = _currentRoundNumber
        };

        _entityPropertyChanges.Add(propertyChange);
    }

    private void OnEntityVisibilityChanged(CBaseEntity entity, byte oldMode, byte newMode)
    {
        if (_currentDemoFile == null || _currentRound == null) return;
        if (_demo == null) return;

        var visibilityEvent = new Models.EntityVisibility
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound.Id,
            Tick = _demo.CurrentDemoTick.Value,
            GameTime = (float)(_demo.CurrentGameTime?.TotalSeconds ?? 0),
            EntityId = entity.EntityIndex.Value,
            EntityType = entity.DesignerName ?? entity.GetType().Name,
            OldRenderMode = oldMode,
            NewRenderMode = newMode,
            IsVisible = newMode == 0, // 0 = normal visibility
            PositionX = (decimal)(entity.CBodyComponent?.SceneNode?.AbsOrigin?.X ?? 0),
            PositionY = (decimal)(entity.CBodyComponent?.SceneNode?.AbsOrigin?.Y ?? 0),
            PositionZ = (decimal)(entity.CBodyComponent?.SceneNode?.AbsOrigin?.Z ?? 0),
            RoundNumber = _currentRoundNumber
        };

        _entityVisibilities.Add(visibilityEvent);
    }

    #endregion

    #region Advanced User Messages

    private void OnDamageReport(CCSUsrMsg_Damage damage)
    {
        if (_currentDemoFile == null || _currentRound == null || _demo == null) return;

        var userMessage = new Models.AdvancedUserMessage
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound.Id,
            Tick = _demo.CurrentDemoTick.Value,
            GameTime = (float)(_demo.CurrentGameTime?.TotalSeconds ?? 0),
            MessageType = "damage_report",
            Data = System.Text.Json.JsonSerializer.Serialize(new
            {
                damage_amount = damage.Amount,
                damage_type = damage.DmgType,
                direction = new { x = damage.Dir?.X, y = damage.Dir?.Y, z = damage.Dir?.Z }
            }),
            RoundNumber = _currentRoundNumber
        };

        _advancedUserMessages.Add(userMessage);
    }

    private void OnLastKillerDamageReport(CCSUsrMsg_SendLastKillerDamageToClient damageReport)
    {
        if (_currentDemoFile == null || _currentRound == null || _demo == null) return;

        var userMessage = new Models.AdvancedUserMessage
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound.Id,
            Tick = _demo.CurrentDemoTick.Value,
            GameTime = (float)(_demo.CurrentGameTime?.TotalSeconds ?? 0),
            MessageType = "last_killer_damage",
            Data = System.Text.Json.JsonSerializer.Serialize(new
            {
                num_hits = damageReport.NumHits,
                damage_taken = damageReport.Damage,
                actual_damage = damageReport.ActualDamage,
                entity_index = damageReport.EntityIndex
            }),
            RoundNumber = _currentRoundNumber
        };

        _advancedUserMessages.Add(userMessage);
    }

    private void OnPlayerStatsUpdate(CCSUsrMsg_PlayerStatsUpdate statsUpdate)
    {
        if (_currentDemoFile == null || _currentRound == null || _demo == null) return;

        var userMessage = new Models.AdvancedUserMessage
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound.Id,
            Tick = _demo.CurrentDemoTick.Value,
            GameTime = (float)(_demo.CurrentGameTime?.TotalSeconds ?? 0),
            MessageType = "player_stats_update",
            Data = System.Text.Json.JsonSerializer.Serialize(new
            {
                version = statsUpdate.Version,
                stats_count = statsUpdate.Stats?.Count ?? 0,
                stats_data = statsUpdate.Stats?.Select(s => new { stat_id = s.StatId, stat_delta = s.StatDelta }).ToList()
            }),
            RoundNumber = _currentRoundNumber
        };

        _advancedUserMessages.Add(userMessage);
    }

    private void OnMoneyAdjustment(CCSUsrMsg_AdjustMoney moneyMsg)
    {
        if (_currentDemoFile == null || _currentRound == null || _demo == null) return;

        var userMessage = new Models.AdvancedUserMessage
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound.Id,
            Tick = _demo.CurrentDemoTick.Value,
            GameTime = (float)(_demo.CurrentGameTime?.TotalSeconds ?? 0),
            MessageType = "money_adjustment",
            Data = System.Text.Json.JsonSerializer.Serialize(new
            {
                amount = moneyMsg.Amount,
                reason = moneyMsg.AwardType?.ToString()
            }),
            RoundNumber = _currentRoundNumber
        };

        _advancedUserMessages.Add(userMessage);
    }

    private void OnVoteStart(CCSUsrMsg_VoteStart voteStart)
    {
        if (_currentDemoFile == null || _currentRound == null || _demo == null) return;

        var userMessage = new Models.AdvancedUserMessage
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound.Id,
            Tick = _demo.CurrentDemoTick.Value,
            GameTime = (float)(_demo.CurrentGameTime?.TotalSeconds ?? 0),
            MessageType = "vote_start",
            Data = System.Text.Json.JsonSerializer.Serialize(new
            {
                team = voteStart.Team,
                entity_index = voteStart.EntityIndex,
                vote_type = voteStart.VoteType,
                display_string = voteStart.DisplayString,
                details_string = voteStart.DetailsString,
                other_team_string = voteStart.OtherTeamString,
                is_yes_no_vote = voteStart.IsYesNoVote
            }),
            RoundNumber = _currentRoundNumber
        };

        _advancedUserMessages.Add(userMessage);
    }

    private void OnVotePass(CCSUsrMsg_VotePass votePass)
    {
        if (_currentDemoFile == null || _currentRound == null || _demo == null) return;

        var userMessage = new Models.AdvancedUserMessage
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound.Id,
            Tick = _demo.CurrentDemoTick.Value,
            GameTime = (float)(_demo.CurrentGameTime?.TotalSeconds ?? 0),
            MessageType = "vote_pass",
            Data = System.Text.Json.JsonSerializer.Serialize(new
            {
                team = votePass.Team,
                vote_type = votePass.VoteType,
                display_string = votePass.DisplayString,
                details_string = votePass.DetailsString
            }),
            RoundNumber = _currentRoundNumber
        };

        _advancedUserMessages.Add(userMessage);
    }

    private void OnVoteFailed(CCSUsrMsg_VoteFailed voteFailed)
    {
        if (_currentDemoFile == null || _currentRound == null || _demo == null) return;

        var userMessage = new Models.AdvancedUserMessage
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound.Id,
            Tick = _demo.CurrentDemoTick.Value,
            GameTime = (float)(_demo.CurrentGameTime?.TotalSeconds ?? 0),
            MessageType = "vote_failed",
            Data = System.Text.Json.JsonSerializer.Serialize(new
            {
                team = voteFailed.Team,
                reason = voteFailed.Reason
            }),
            RoundNumber = _currentRoundNumber
        };

        _advancedUserMessages.Add(userMessage);
    }

    private void OnCallVoteFailed(CCSUsrMsg_CallVoteFailed callVoteFailed)
    {
        if (_currentDemoFile == null || _currentRound == null || _demo == null) return;

        var userMessage = new Models.AdvancedUserMessage
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound.Id,
            Tick = _demo.CurrentDemoTick.Value,
            GameTime = (float)(_demo.CurrentGameTime?.TotalSeconds ?? 0),
            MessageType = "call_vote_failed",
            Data = System.Text.Json.JsonSerializer.Serialize(new
            {
                reason = callVoteFailed.Reason,
                time = callVoteFailed.Time
            }),
            RoundNumber = _currentRoundNumber
        };

        _advancedUserMessages.Add(userMessage);
    }

    private void OnServerRankUpdate(CCSUsrMsg_ServerRankUpdate rankUpdate)
    {
        if (_currentDemoFile == null || _currentRound == null || _demo == null) return;

        var userMessage = new Models.AdvancedUserMessage
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound.Id,
            Tick = _demo.CurrentDemoTick.Value,
            GameTime = (float)(_demo.CurrentGameTime?.TotalSeconds ?? 0),
            MessageType = "server_rank_update",
            Data = System.Text.Json.JsonSerializer.Serialize(new
            {
                rank_updates = rankUpdate.RankUpdate?.Select(r => new
                {
                    account_id = r.AccountId,
                    rank_old = r.RankOld,
                    rank_new = r.RankNew,
                    num_wins = r.NumWins,
                    rank_change = r.RankChange
                }).ToList()
            }),
            RoundNumber = _currentRoundNumber
        };

        _advancedUserMessages.Add(userMessage);
    }

    private void OnXRankGet(CCSUsrMsg_XRankGet xRankGet)
    {
        if (_currentDemoFile == null || _currentRound == null || _demo == null) return;

        var userMessage = new Models.AdvancedUserMessage
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound.Id,
            Tick = _demo.CurrentDemoTick.Value,
            GameTime = (float)(_demo.CurrentGameTime?.TotalSeconds ?? 0),
            MessageType = "xrank_get",
            Data = System.Text.Json.JsonSerializer.Serialize(new
            {
                mode_idx = xRankGet.ModeIdx,
                controller = xRankGet.Controller
            }),
            RoundNumber = _currentRoundNumber
        };

        _advancedUserMessages.Add(userMessage);
    }

    private void OnXRankUpdate(CCSUsrMsg_XRankUpd xRankUpdate)
    {
        if (_currentDemoFile == null || _currentRound == null || _demo == null) return;

        var userMessage = new Models.AdvancedUserMessage
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound.Id,
            Tick = _demo.CurrentDemoTick.Value,
            GameTime = (float)(_demo.CurrentGameTime?.TotalSeconds ?? 0),
            MessageType = "xrank_update",
            Data = System.Text.Json.JsonSerializer.Serialize(new
            {
                mode_idx = xRankUpdate.ModeIdx,
                controller = xRankUpdate.Controller,
                ranking = xRankUpdate.Ranking
            }),
            RoundNumber = _currentRoundNumber
        };

        _advancedUserMessages.Add(userMessage);
    }

    private void OnItemPickupMessage(CCSUsrMsg_ItemPickup itemPickup)
    {
        if (_currentDemoFile == null || _currentRound == null || _demo == null) return;

        var userMessage = new Models.AdvancedUserMessage
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound.Id,
            Tick = _demo.CurrentDemoTick.Value,
            GameTime = (float)(_demo.CurrentGameTime?.TotalSeconds ?? 0),
            MessageType = "item_pickup",
            Data = System.Text.Json.JsonSerializer.Serialize(new
            {
                item = itemPickup.Item
            }),
            RoundNumber = _currentRoundNumber
        };

        _advancedUserMessages.Add(userMessage);
    }

    private void OnAchievementEvent(CCSUsrMsg_AchievementEvent achievement)
    {
        if (_currentDemoFile == null || _currentRound == null || _demo == null) return;

        var userMessage = new Models.AdvancedUserMessage
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound.Id,
            Tick = _demo.CurrentDemoTick.Value,
            GameTime = (float)(_demo.CurrentGameTime?.TotalSeconds ?? 0),
            MessageType = "achievement",
            Data = System.Text.Json.JsonSerializer.Serialize(new
            {
                achievement = achievement.Achievement
            }),
            RoundNumber = _currentRoundNumber
        };

        _advancedUserMessages.Add(userMessage);
    }

    #endregion

    #region Inferno Tracking

    private void OnInfernoCreate(CInferno inferno)
    {
        if (_currentDemoFile == null || _currentRound == null || _demo == null) return;

        var infernoEvent = new Models.InfernoEvent
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound.Id,
            Tick = _demo.CurrentDemoTick.Value,
            GameTime = (float)(_demo.CurrentGameTime?.TotalSeconds ?? 0),
            InfernoEntityId = inferno.EntityIndex.Value,
            EventType = "ignited",
            InfernoType = inferno.InfernoType,
            FireCount = inferno.FireCount,
            FireLifetime = inferno.FireLifetime,
            PositionX = (decimal)(inferno.CBodyComponent?.SceneNode?.AbsOrigin?.X ?? 0),
            PositionY = (decimal)(inferno.CBodyComponent?.SceneNode?.AbsOrigin?.Y ?? 0),
            PositionZ = (decimal)(inferno.CBodyComponent?.SceneNode?.AbsOrigin?.Z ?? 0),
            FirePositions = string.Join(";", inferno.FirePositions?.Select(p => $"{p.X:F2},{p.Y:F2},{p.Z:F2}") ?? new string[0]),
            FireStates = string.Join(";", inferno.FireIsBurning?.Select(b => b.ToString()) ?? new string[0]),
            RoundNumber = _currentRoundNumber,
            IsActive = true,
            Description = $"Inferno {inferno.EntityIndex.Value} ignited with {inferno.FireCount} fire points"
        };

        _infernoEvents.Add(infernoEvent);
        _activeInfernos[inferno.EntityIndex.Value] = infernoEvent;
    }

    private void OnInfernoDelete(CInferno inferno)
    {
        if (_currentDemoFile == null || _currentRound == null || _demo == null) return;

        var infernoEvent = new Models.InfernoEvent
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound.Id,
            Tick = _demo.CurrentDemoTick.Value,
            GameTime = (float)(_demo.CurrentGameTime?.TotalSeconds ?? 0),
            InfernoEntityId = inferno.EntityIndex.Value,
            EventType = "extinguished",
            InfernoType = inferno.InfernoType,
            FireCount = 0,
            FireLifetime = inferno.FireLifetime,
            PositionX = (decimal)(inferno.CBodyComponent?.SceneNode?.AbsOrigin?.X ?? 0),
            PositionY = (decimal)(inferno.CBodyComponent?.SceneNode?.AbsOrigin?.Y ?? 0),
            PositionZ = (decimal)(inferno.CBodyComponent?.SceneNode?.AbsOrigin?.Z ?? 0),
            RoundNumber = _currentRoundNumber,
            IsActive = false,
            Description = $"Inferno {inferno.EntityIndex.Value} extinguished after {inferno.FireLifetime:F2}s"
        };

        _infernoEvents.Add(infernoEvent);
        _activeInfernos.Remove(inferno.EntityIndex.Value);
    }

    private void OnInfernoFireCountChanged(CInferno inferno, int oldCount, int newCount)
    {
        if (_currentDemoFile == null || _currentRound == null || _demo == null) return;

        var eventType = newCount > oldCount ? "spread" : "diminish";
        var infernoEvent = new Models.InfernoEvent
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound.Id,
            Tick = _demo.CurrentDemoTick.Value,
            GameTime = (float)(_demo.CurrentGameTime?.TotalSeconds ?? 0),
            InfernoEntityId = inferno.EntityIndex.Value,
            EventType = eventType,
            InfernoType = inferno.InfernoType,
            FireCount = newCount,
            PreviousFireCount = oldCount,
            FireLifetime = inferno.FireLifetime,
            PositionX = (decimal)(inferno.CBodyComponent?.SceneNode?.AbsOrigin?.X ?? 0),
            PositionY = (decimal)(inferno.CBodyComponent?.SceneNode?.AbsOrigin?.Y ?? 0),
            PositionZ = (decimal)(inferno.CBodyComponent?.SceneNode?.AbsOrigin?.Z ?? 0),
            FirePositions = string.Join(";", inferno.FirePositions?.Select(p => $"{p.X:F2},{p.Y:F2},{p.Z:F2}") ?? new string[0]),
            FireStates = string.Join(";", inferno.FireIsBurning?.Select(b => b.ToString()) ?? new string[0]),
            BurnNormals = string.Join(";", inferno.BurnNormal?.Select(n => $"{n.X:F2},{n.Y:F2},{n.Z:F2}") ?? new string[0]),
            RoundNumber = _currentRoundNumber,
            IsActive = newCount > 0,
            Description = $"Inferno {inferno.EntityIndex.Value} fire count changed from {oldCount} to {newCount}"
        };

        _infernoEvents.Add(infernoEvent);

        // Update active inferno tracking
        if (_activeInfernos.ContainsKey(inferno.EntityIndex.Value))
        {
            _activeInfernos[inferno.EntityIndex.Value] = infernoEvent;
        }
    }

    private void OnInfernoLifetimeChanged(CInferno inferno, float oldLifetime, float newLifetime)
    {
        if (_currentDemoFile == null || _currentRound == null || _demo == null) return;

        // Only track significant lifetime changes to avoid spam
        if (Math.Abs(newLifetime - oldLifetime) < 0.1f) return;

        var infernoEvent = new Models.InfernoEvent
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound.Id,
            Tick = _demo.CurrentDemoTick.Value,
            GameTime = (float)(_demo.CurrentGameTime?.TotalSeconds ?? 0),
            InfernoEntityId = inferno.EntityIndex.Value,
            EventType = "lifetime_update",
            InfernoType = inferno.InfernoType,
            FireCount = inferno.FireCount,
            FireLifetime = newLifetime,
            PreviousFireLifetime = oldLifetime,
            PositionX = (decimal)(inferno.CBodyComponent?.SceneNode?.AbsOrigin?.X ?? 0),
            PositionY = (decimal)(inferno.CBodyComponent?.SceneNode?.AbsOrigin?.Y ?? 0),
            PositionZ = (decimal)(inferno.CBodyComponent?.SceneNode?.AbsOrigin?.Z ?? 0),
            RoundNumber = _currentRoundNumber,
            IsActive = inferno.FireCount > 0,
            Description = $"Inferno {inferno.EntityIndex.Value} lifetime: {newLifetime:F2}s"
        };

        _infernoEvents.Add(infernoEvent);
    }

    private void OnInfernoPostEffectChanged(CInferno inferno, bool oldPostEffect, bool newPostEffect)
    {
        if (_currentDemoFile == null || _currentRound == null || _demo == null) return;

        var eventType = newPostEffect ? "entering_post_effect" : "exiting_post_effect";
        var infernoEvent = new Models.InfernoEvent
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound.Id,
            Tick = _demo.CurrentDemoTick.Value,
            GameTime = (float)(_demo.CurrentGameTime?.TotalSeconds ?? 0),
            InfernoEntityId = inferno.EntityIndex.Value,
            EventType = eventType,
            InfernoType = inferno.InfernoType,
            FireCount = inferno.FireCount,
            FireLifetime = inferno.FireLifetime,
            PositionX = (decimal)(inferno.CBodyComponent?.SceneNode?.AbsOrigin?.X ?? 0),
            PositionY = (decimal)(inferno.CBodyComponent?.SceneNode?.AbsOrigin?.Y ?? 0),
            PositionZ = (decimal)(inferno.CBodyComponent?.SceneNode?.AbsOrigin?.Z ?? 0),
            InPostEffectTime = newPostEffect,
            RoundNumber = _currentRoundNumber,
            IsActive = inferno.FireCount > 0,
            Description = $"Inferno {inferno.EntityIndex.Value} {eventType}"
        };

        _infernoEvents.Add(infernoEvent);
    }

    private void OnMolotovPositionChanged(CMolotovGrenade molotov, Vector? oldPos, Vector? newPos)
    {
        if (_currentDemoFile == null || _currentRound == null || newPos == null || oldPos == null) return;
        if (_demo == null) return;

        var distance = oldPos.Value.DistanceTo(newPos.Value);
        if (distance > 0.5f) // Only track significant movement
        {
            var infernoEvent = new Models.InfernoEvent
            {
                DemoFileId = _currentDemoFile.Id,
                RoundId = _currentRound.Id,
                Tick = _demo.CurrentDemoTick.Value,
                GameTime = (float)(_demo.CurrentGameTime?.TotalSeconds ?? 0),
                InfernoEntityId = molotov.EntityIndex.Value,
                EventType = "molotov_trajectory",
                InfernoType = 1, // Molotov type
                PositionX = (decimal)newPos.Value.X,
                PositionY = (decimal)newPos.Value.Y,
                PositionZ = (decimal)newPos.Value.Z,
                PreviousPositionX = (decimal)oldPos.Value.X,
                PreviousPositionY = (decimal)oldPos.Value.Y,
                PreviousPositionZ = (decimal)oldPos.Value.Z,
                RoundNumber = _currentRoundNumber,
                IsActive = true,
                Description = $"Molotov {molotov.EntityIndex.Value} trajectory update"
            };

            _infernoEvents.Add(infernoEvent);
        }
    }

    #endregion

    #region Decoy Simulation and Advanced Weapon States

    private void OnWeaponStateChanged(CCSWeaponBase weapon, CSWeaponState oldState, CSWeaponState newState)
    {
        if (_currentDemoFile == null || _currentRound == null || _demo == null) return;

        var weaponState = new Models.WeaponState
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound.Id,
            Tick = _demo.CurrentDemoTick.Value,
            GameTime = (float)(_demo.CurrentGameTime?.TotalSeconds ?? 0),
            WeaponEntityId = weapon.EntityIndex.Value,
            WeaponType = weapon.DesignerName ?? "unknown",
            StateType = "weapon_state",
            OldValue = oldState.ToString(),
            NewValue = newState.ToString(),
            PositionX = (decimal)(weapon.CBodyComponent?.SceneNode?.AbsOrigin?.X ?? 0),
            PositionY = (decimal)(weapon.CBodyComponent?.SceneNode?.AbsOrigin?.Y ?? 0),
            PositionZ = (decimal)(weapon.CBodyComponent?.SceneNode?.AbsOrigin?.Z ?? 0),
            RoundNumber = _currentRoundNumber,
            Description = $"Weapon {weapon.EntityIndex.Value} state changed from {oldState} to {newState}"
        };

        _weaponStates.Add(weaponState);
    }

    private void OnWeaponModeChanged(CCSWeaponBase weapon, CSWeaponMode oldMode, CSWeaponMode newMode)
    {
        if (_currentDemoFile == null || _currentRound == null || _demo == null) return;

        var weaponState = new Models.WeaponState
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound.Id,
            Tick = _demo.CurrentDemoTick.Value,
            GameTime = (float)(_demo.CurrentGameTime?.TotalSeconds ?? 0),
            WeaponEntityId = weapon.EntityIndex.Value,
            WeaponType = weapon.DesignerName ?? "unknown",
            StateType = "weapon_mode",
            OldValue = oldMode.ToString(),
            NewValue = newMode.ToString(),
            PositionX = (decimal)(weapon.CBodyComponent?.SceneNode?.AbsOrigin?.X ?? 0),
            PositionY = (decimal)(weapon.CBodyComponent?.SceneNode?.AbsOrigin?.Y ?? 0),
            PositionZ = (decimal)(weapon.CBodyComponent?.SceneNode?.AbsOrigin?.Z ?? 0),
            RoundNumber = _currentRoundNumber,
            Description = $"Weapon {weapon.EntityIndex.Value} mode changed from {oldMode} to {newMode}"
        };

        _weaponStates.Add(weaponState);
    }

    private void OnWeaponReloadStateChanged(CCSWeaponBase weapon, bool oldReloading, bool newReloading)
    {
        if (_currentDemoFile == null || _currentRound == null || _demo == null) return;

        var weaponState = new Models.WeaponState
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound.Id,
            Tick = _demo.CurrentDemoTick.Value,
            GameTime = (float)(_demo.CurrentGameTime?.TotalSeconds ?? 0),
            WeaponEntityId = weapon.EntityIndex.Value,
            WeaponType = weapon.DesignerName ?? "unknown",
            StateType = "reload_state",
            OldValue = oldReloading.ToString(),
            NewValue = newReloading.ToString(),
            AmmoClip = weapon.Clip1,
            AmmoReserve = weapon.ReserveAmmo[0],
            PositionX = (decimal)(weapon.CBodyComponent?.SceneNode?.AbsOrigin?.X ?? 0),
            PositionY = (decimal)(weapon.CBodyComponent?.SceneNode?.AbsOrigin?.Y ?? 0),
            PositionZ = (decimal)(weapon.CBodyComponent?.SceneNode?.AbsOrigin?.Z ?? 0),
            RoundNumber = _currentRoundNumber,
            Description = $"Weapon {weapon.EntityIndex.Value} reload state: {(newReloading ? "started" : "ended")}"
        };

        _weaponStates.Add(weaponState);
    }

    private void OnWeaponSilencerChanged(CCSWeaponBase weapon, bool oldSilenced, bool newSilenced)
    {
        if (_currentDemoFile == null || _currentRound == null || _demo == null) return;

        var weaponState = new Models.WeaponState
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound.Id,
            Tick = _demo.CurrentDemoTick.Value,
            GameTime = (float)(_demo.CurrentGameTime?.TotalSeconds ?? 0),
            WeaponEntityId = weapon.EntityIndex.Value,
            WeaponType = weapon.DesignerName ?? "unknown",
            StateType = "silencer_state",
            OldValue = oldSilenced.ToString(),
            NewValue = newSilenced.ToString(),
            PositionX = (decimal)(weapon.CBodyComponent?.SceneNode?.AbsOrigin?.X ?? 0),
            PositionY = (decimal)(weapon.CBodyComponent?.SceneNode?.AbsOrigin?.Y ?? 0),
            PositionZ = (decimal)(weapon.CBodyComponent?.SceneNode?.AbsOrigin?.Z ?? 0),
            RoundNumber = _currentRoundNumber,
            Description = $"Weapon {weapon.EntityIndex.Value} silencer {(newSilenced ? "attached" : "detached")}"
        };

        _weaponStates.Add(weaponState);
    }

    private void OnWeaponZoomChanged(CCSWeaponBaseGun weapon, int oldZoom, int newZoom)
    {
        if (_currentDemoFile == null || _currentRound == null || _demo == null) return;

        var weaponState = new Models.WeaponState
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound.Id,
            Tick = _demo.CurrentDemoTick.Value,
            GameTime = (float)(_demo.CurrentGameTime?.TotalSeconds ?? 0),
            WeaponEntityId = weapon.EntityIndex.Value,
            WeaponType = weapon.DesignerName ?? "unknown",
            StateType = "zoom_level",
            OldValue = oldZoom.ToString(),
            NewValue = newZoom.ToString(),
            ZoomLevel = newZoom,
            PositionX = (decimal)(weapon.CBodyComponent?.SceneNode?.AbsOrigin?.X ?? 0),
            PositionY = (decimal)(weapon.CBodyComponent?.SceneNode?.AbsOrigin?.Y ?? 0),
            PositionZ = (decimal)(weapon.CBodyComponent?.SceneNode?.AbsOrigin?.Z ?? 0),
            RoundNumber = _currentRoundNumber,
            Description = $"Weapon {weapon.EntityIndex.Value} zoom level changed from {oldZoom} to {newZoom}"
        };

        _weaponStates.Add(weaponState);
    }

    private void OnWeaponBurstChanged(CCSWeaponBaseGun weapon, int oldBurst, int newBurst)
    {
        if (_currentDemoFile == null || _currentRound == null || _demo == null) return;

        var weaponState = new Models.WeaponState
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound.Id,
            Tick = _demo.CurrentDemoTick.Value,
            GameTime = (float)(_demo.CurrentGameTime?.TotalSeconds ?? 0),
            WeaponEntityId = weapon.EntityIndex.Value,
            WeaponType = weapon.DesignerName ?? "unknown",
            StateType = "burst_shots",
            OldValue = oldBurst.ToString(),
            NewValue = newBurst.ToString(),
            BurstShotsRemaining = newBurst,
            PositionX = (decimal)(weapon.CBodyComponent?.SceneNode?.AbsOrigin?.X ?? 0),
            PositionY = (decimal)(weapon.CBodyComponent?.SceneNode?.AbsOrigin?.Y ?? 0),
            PositionZ = (decimal)(weapon.CBodyComponent?.SceneNode?.AbsOrigin?.Z ?? 0),
            RoundNumber = _currentRoundNumber,
            Description = $"Weapon {weapon.EntityIndex.Value} burst shots remaining: {newBurst}"
        };

        _weaponStates.Add(weaponState);
    }

    private void OnDecoyCreate(CDecoyProjectile decoy)
    {
        if (_currentDemoFile == null || _currentRound == null || _demo == null) return;

        var gameEvent = new Models.GameEvent
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound.Id,
            Tick = _demo.CurrentDemoTick.Value,
            GameTime = (float)(_demo.CurrentGameTime?.TotalSeconds ?? 0),
            EventType = "decoy_projectile_spawn",
            EventData = System.Text.Json.JsonSerializer.Serialize(new
            {
                decoy_entity_id = decoy.EntityIndex.Value,
                initial_shot_tick = decoy.DecoyShotTick,
                bounce_count = decoy.BounceCount,
                explode_time = decoy.ExplodeTime?.TotalSeconds
            }),
            PositionX = (decimal)(decoy.CBodyComponent?.SceneNode?.AbsOrigin?.X ?? 0),
            PositionY = (decimal)(decoy.CBodyComponent?.SceneNode?.AbsOrigin?.Y ?? 0),
            PositionZ = (decimal)(decoy.CBodyComponent?.SceneNode?.AbsOrigin?.Z ?? 0),
            RoundNumber = _currentRoundNumber,
            Description = $"Decoy projectile {decoy.EntityIndex.Value} spawned"
        };

        _gameEvents.Add(gameEvent);
    }

    private void OnDecoyDelete(CDecoyProjectile decoy)
    {
        if (_currentDemoFile == null || _currentRound == null || _demo == null) return;

        var gameEvent = new Models.GameEvent
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound.Id,
            Tick = _demo.CurrentDemoTick.Value,
            GameTime = (float)(_demo.CurrentGameTime?.TotalSeconds ?? 0),
            EventType = "decoy_projectile_explode",
            EventData = System.Text.Json.JsonSerializer.Serialize(new
            {
                decoy_entity_id = decoy.EntityIndex.Value,
                final_shot_tick = decoy.DecoyShotTick,
                total_bounces = decoy.BounceCount
            }),
            PositionX = (decimal)(decoy.CBodyComponent?.SceneNode?.AbsOrigin?.X ?? 0),
            PositionY = (decimal)(decoy.CBodyComponent?.SceneNode?.AbsOrigin?.Y ?? 0),
            PositionZ = (decimal)(decoy.CBodyComponent?.SceneNode?.AbsOrigin?.Z ?? 0),
            RoundNumber = _currentRoundNumber,
            Description = $"Decoy projectile {decoy.EntityIndex.Value} exploded"
        };

        _gameEvents.Add(gameEvent);
    }

    private void OnDecoyShotTickChanged(CDecoyProjectile decoy, int oldTick, int newTick)
    {
        if (_currentDemoFile == null || _currentRound == null || _demo == null) return;
        if (oldTick == newTick) return; // Avoid unnecessary events

        var gameEvent = new Models.GameEvent
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound.Id,
            Tick = _demo.CurrentDemoTick.Value,
            GameTime = (float)(_demo.CurrentGameTime?.TotalSeconds ?? 0),
            EventType = "decoy_shot_simulation",
            EventData = System.Text.Json.JsonSerializer.Serialize(new
            {
                decoy_entity_id = decoy.EntityIndex.Value,
                shot_tick = newTick,
                previous_shot_tick = oldTick,
                shots_simulated = newTick - oldTick
            }),
            PositionX = (decimal)(decoy.CBodyComponent?.SceneNode?.AbsOrigin?.X ?? 0),
            PositionY = (decimal)(decoy.CBodyComponent?.SceneNode?.AbsOrigin?.Y ?? 0),
            PositionZ = (decimal)(decoy.CBodyComponent?.SceneNode?.AbsOrigin?.Z ?? 0),
            RoundNumber = _currentRoundNumber,
            Description = $"Decoy {decoy.EntityIndex.Value} simulated shot at tick {newTick}"
        };

        _gameEvents.Add(gameEvent);
    }

    private void OnDecoyGrenadeCreate(CDecoyGrenade decoyGrenade)
    {
        if (_currentDemoFile == null || _currentRound == null || _demo == null) return;

        var gameEvent = new Models.GameEvent
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound.Id,
            Tick = _demo.CurrentDemoTick.Value,
            GameTime = (float)(_demo.CurrentGameTime?.TotalSeconds ?? 0),
            EventType = "decoy_grenade_spawn",
            EventData = System.Text.Json.JsonSerializer.Serialize(new
            {
                decoy_grenade_id = decoyGrenade.EntityIndex.Value,
                pin_pulled = decoyGrenade.PinPulled,
                throw_time = decoyGrenade.ThrowTime?.TotalSeconds,
                throw_strength = decoyGrenade.ThrowStrength
            }),
            PositionX = (decimal)(decoyGrenade.CBodyComponent?.SceneNode?.AbsOrigin?.X ?? 0),
            PositionY = (decimal)(decoyGrenade.CBodyComponent?.SceneNode?.AbsOrigin?.Y ?? 0),
            PositionZ = (decimal)(decoyGrenade.CBodyComponent?.SceneNode?.AbsOrigin?.Z ?? 0),
            RoundNumber = _currentRoundNumber,
            Description = $"Decoy grenade {decoyGrenade.EntityIndex.Value} equipped"
        };

        _gameEvents.Add(gameEvent);
    }

    private void OnDecoyGrenadeDelete(CDecoyGrenade decoyGrenade)
    {
        if (_currentDemoFile == null || _currentRound == null || _demo == null) return;

        var gameEvent = new Models.GameEvent
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound.Id,
            Tick = _demo.CurrentDemoTick.Value,
            GameTime = (float)(_demo.CurrentGameTime?.TotalSeconds ?? 0),
            EventType = "decoy_grenade_thrown",
            EventData = System.Text.Json.JsonSerializer.Serialize(new
            {
                decoy_grenade_id = decoyGrenade.EntityIndex.Value,
                final_throw_strength = decoyGrenade.ThrowStrength,
                was_jump_throw = decoyGrenade.JumpThrow
            }),
            PositionX = (decimal)(decoyGrenade.CBodyComponent?.SceneNode?.AbsOrigin?.X ?? 0),
            PositionY = (decimal)(decoyGrenade.CBodyComponent?.SceneNode?.AbsOrigin?.Y ?? 0),
            PositionZ = (decimal)(decoyGrenade.CBodyComponent?.SceneNode?.AbsOrigin?.Z ?? 0),
            RoundNumber = _currentRoundNumber,
            Description = $"Decoy grenade {decoyGrenade.EntityIndex.Value} thrown"
        };

        _gameEvents.Add(gameEvent);
    }

    #endregion

    #region Player Behavior Events

    private void OnPlayerFootstep(Source1PlayerFootstepEvent e)
    {
        if (_currentDemoFile == null || _currentRound == null || _demo == null || e.Player == null) return;

        var player = GetOrCreatePlayer(e.Player);
        var playerPawn = e.PlayerPawn;
        
        var behaviorEvent = new Models.PlayerBehaviorEvent
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound.Id,
            PlayerId = player.Id,
            Tick = _demo.CurrentDemoTick.Value,
            GameTime = (float)(_demo.CurrentGameTime?.TotalSeconds ?? 0),
            EventType = "footstep",
            PositionX = (decimal)(playerPawn?.CBodyComponent?.SceneNode?.AbsOrigin?.X ?? 0),
            PositionY = (decimal)(playerPawn?.CBodyComponent?.SceneNode?.AbsOrigin?.Y ?? 0),
            PositionZ = (decimal)(playerPawn?.CBodyComponent?.SceneNode?.AbsOrigin?.Z ?? 0),
            Velocity = CalculatePlayerVelocity(playerPawn),
            MovementType = DetermineMovementType(playerPawn),
            IsRunning = IsPlayerRunning(playerPawn),
            IsCrouching = playerPawn?.Flags.HasFlag(PlayerFlags.FL_DUCKING) ?? false,
            IsOnGround = playerPawn?.Flags.HasFlag(PlayerFlags.FL_ONGROUND) ?? false,
            Surface = DetermineSurface(playerPawn),
            Team = e.Player.Team.ToString(),
            RoundNumber = _currentRoundNumber,
            Description = $"{e.Player.PlayerName} footstep at {playerPawn?.CBodyComponent?.SceneNode?.AbsOrigin}"
        };

        _playerBehaviorEvents.Add(behaviorEvent);
    }

    private void OnPlayerJump(Source1PlayerJumpEvent e)
    {
        if (_currentDemoFile == null || _currentRound == null || _demo == null || e.Player == null) return;

        var player = GetOrCreatePlayer(e.Player);
        var playerPawn = e.Player.PlayerPawn;
        
        var behaviorEvent = new Models.PlayerBehaviorEvent
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound.Id,
            PlayerId = player.Id,
            Tick = _demo.CurrentDemoTick.Value,
            GameTime = (float)(_demo.CurrentGameTime?.TotalSeconds ?? 0),
            EventType = "jump",
            PositionX = (decimal)(playerPawn?.CBodyComponent?.SceneNode?.AbsOrigin?.X ?? 0),
            PositionY = (decimal)(playerPawn?.CBodyComponent?.SceneNode?.AbsOrigin?.Y ?? 0),
            PositionZ = (decimal)(playerPawn?.CBodyComponent?.SceneNode?.AbsOrigin?.Z ?? 0),
            Velocity = CalculatePlayerVelocity(playerPawn),
            VerticalVelocity = (float)(playerPawn?.Velocity?.Z ?? 0),
            MovementType = DetermineMovementType(playerPawn),
            IsRunning = IsPlayerRunning(playerPawn),
            IsCrouching = playerPawn?.Flags.HasFlag(PlayerFlags.FL_DUCKING) ?? false,
            IsOnGround = false, // Player is jumping
            JumpType = DetermineJumpType(playerPawn),
            Team = e.Player.Team.ToString(),
            RoundNumber = _currentRoundNumber,
            Description = $"{e.Player.PlayerName} jumped"
        };

        _playerBehaviorEvents.Add(behaviorEvent);
    }

    private void OnPlayerSound(Source1PlayerSoundEvent e)
    {
        if (_currentDemoFile == null || _currentRound == null || _demo == null || e.Player == null) return;

        var player = GetOrCreatePlayer(e.Player);
        var playerPawn = e.PlayerPawn;
        
        var behaviorEvent = new Models.PlayerBehaviorEvent
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound.Id,
            PlayerId = player.Id,
            Tick = _demo.CurrentDemoTick.Value,
            GameTime = (float)(_demo.CurrentGameTime?.TotalSeconds ?? 0),
            EventType = "sound",
            PositionX = (decimal)(playerPawn?.CBodyComponent?.SceneNode?.AbsOrigin?.X ?? 0),
            PositionY = (decimal)(playerPawn?.CBodyComponent?.SceneNode?.AbsOrigin?.Y ?? 0),
            PositionZ = (decimal)(playerPawn?.CBodyComponent?.SceneNode?.AbsOrigin?.Z ?? 0),
            SoundRadius = e.Radius,
            SoundDuration = e.Duration,
            SoundStep = e.Step,
            Team = e.Player.Team.ToString(),
            RoundNumber = _currentRoundNumber,
            Description = $"{e.Player.PlayerName} made sound (radius: {e.Radius}, duration: {e.Duration:F2}s)"
        };

        _playerBehaviorEvents.Add(behaviorEvent);
    }

    private float CalculatePlayerVelocity(CCSPlayerPawn? playerPawn)
    {
        if (playerPawn?.Velocity == null) return 0f;
        
        var velocity = playerPawn.Velocity;
        return (float)Math.Sqrt(velocity.X * velocity.X + velocity.Y * velocity.Y + velocity.Z * velocity.Z);
    }

    private string DetermineMovementType(CCSPlayerPawn? playerPawn)
    {
        if (playerPawn?.Velocity == null) return "stationary";
        
        var horizontalSpeed = Math.Sqrt(playerPawn.Velocity.X * playerPawn.Velocity.X + playerPawn.Velocity.Y * playerPawn.Velocity.Y);
        
        if (horizontalSpeed < 5) return "stationary";
        if (horizontalSpeed < 100) return "walking";
        if (horizontalSpeed < 200) return "running";
        if (horizontalSpeed < 300) return "sprinting";
        return "fast_movement";
    }

    private bool IsPlayerRunning(CCSPlayerPawn? playerPawn)
    {
        if (playerPawn?.Velocity == null) return false;
        
        var horizontalSpeed = Math.Sqrt(playerPawn.Velocity.X * playerPawn.Velocity.X + playerPawn.Velocity.Y * playerPawn.Velocity.Y);
        return horizontalSpeed > 100;
    }

    private string DetermineSurface(CCSPlayerPawn? playerPawn)
    {
        // This would need more sophisticated surface detection
        // For now, we'll use basic logic based on position and context
        if (playerPawn?.CBodyComponent?.SceneNode?.AbsOrigin == null) return "unknown";
        
        // Simple surface detection - this could be enhanced with map data
        var z = playerPawn.CBodyComponent.SceneNode.AbsOrigin.Z;
        if (z > 200) return "elevated";
        if (z < -100) return "underground";
        return "ground";
    }

    private string DetermineJumpType(CCSPlayerPawn? playerPawn)
    {
        if (playerPawn?.Velocity == null) return "normal";
        
        var horizontalSpeed = Math.Sqrt(playerPawn.Velocity.X * playerPawn.Velocity.X + playerPawn.Velocity.Y * playerPawn.Velocity.Y);
        var verticalSpeed = Math.Abs(playerPawn.Velocity.Z);
        
        if (horizontalSpeed > 200 && verticalSpeed > 300) return "long_jump";
        if (verticalSpeed > 400) return "high_jump";
        if (horizontalSpeed < 50) return "static_jump";
        return "normal";
    }

    #endregion

    #region Movement Pattern Analysis

    private readonly Dictionary<int, List<Models.PlayerBehaviorEvent>> _recentMovements = new();
    private readonly Dictionary<int, Vector> _lastKnownPositions = new();
    private readonly Dictionary<int, DateTime> _lastMovementTimes = new();

    private void AnalyzeMovementPatterns(CCSPlayerController player, Vector currentPosition)
    {
        var playerId = GetOrCreatePlayer(player).Id;
        var currentTime = DateTime.UtcNow;

        // Initialize tracking for new player
        if (!_recentMovements.ContainsKey(playerId))
        {
            _recentMovements[playerId] = new List<Models.PlayerBehaviorEvent>();
            _lastKnownPositions[playerId] = currentPosition;
            _lastMovementTimes[playerId] = currentTime;
            return;
        }

        var lastPosition = _lastKnownPositions[playerId];
        var lastTime = _lastMovementTimes[playerId];
        var timeDelta = (currentTime - lastTime).TotalSeconds;

        if (timeDelta > 0.1) // Only analyze if enough time has passed
        {
            var distance = lastPosition.DistanceTo(currentPosition);
            var speed = distance / (float)timeDelta;

            // Detect movement patterns
            var pattern = DetectMovementPattern(playerId, currentPosition, speed);
            
            if (!string.IsNullOrEmpty(pattern))
            {
                var behaviorEvent = new Models.PlayerBehaviorEvent
                {
                    DemoFileId = _currentDemoFile?.Id ?? 0,
                    RoundId = _currentRound?.Id,
                    PlayerId = playerId,
                    Tick = _demo?.CurrentDemoTick.Value ?? 0,
                    GameTime = (float)(_demo?.CurrentGameTime?.TotalSeconds ?? 0),
                    EventType = "movement_pattern",
                    PositionX = (decimal)currentPosition.X,
                    PositionY = (decimal)currentPosition.Y,
                    PositionZ = (decimal)currentPosition.Z,
                    Velocity = speed,
                    MovementPattern = pattern,
                    Team = player.Team.ToString(),
                    RoundNumber = _currentRoundNumber,
                    Description = $"{player.PlayerName} movement pattern: {pattern}"
                };

                _playerBehaviorEvents.Add(behaviorEvent);
            }

            _lastKnownPositions[playerId] = currentPosition;
            _lastMovementTimes[playerId] = currentTime;
        }
    }

    private string DetectMovementPattern(int playerId, Vector currentPosition, float speed)
    {
        var recentMovements = _recentMovements[playerId];
        
        // Keep only recent movements (last 5 seconds)
        var cutoffTime = DateTime.UtcNow.AddSeconds(-5);
        recentMovements.RemoveAll(m => m.CreatedAt < cutoffTime);

        if (recentMovements.Count < 3) return string.Empty;

        // Analyze patterns
        if (IsCircularMovement(recentMovements)) return "circular";
        if (IsBackAndForthMovement(recentMovements)) return "back_and_forth";
        if (IsStraightLineMovement(recentMovements)) return "straight_line";
        if (IsErraticMovement(recentMovements)) return "erratic";
        if (IsCampingBehavior(recentMovements)) return "camping";
        if (IsPeekingBehavior(recentMovements)) return "peeking";

        return string.Empty;
    }

    private bool IsCircularMovement(List<Models.PlayerBehaviorEvent> movements)
    {
        if (movements.Count < 8) return false;

        // Check if player is moving in a roughly circular pattern
        var centerX = movements.Average(m => (double)m.PositionX);
        var centerY = movements.Average(m => (double)m.PositionY);
        
        var distances = movements.Select(m => 
            Math.Sqrt(Math.Pow((double)m.PositionX - centerX, 2) + Math.Pow((double)m.PositionY - centerY, 2))).ToList();
        
        var avgDistance = distances.Average();
        var distanceVariance = distances.Sum(d => Math.Pow(d - avgDistance, 2)) / distances.Count;
        
        return distanceVariance < avgDistance * 0.2; // Low variance indicates circular movement
    }

    private bool IsBackAndForthMovement(List<Models.PlayerBehaviorEvent> movements)
    {
        if (movements.Count < 6) return false;

        // Check for alternating direction changes
        var directionChanges = 0;
        for (int i = 2; i < movements.Count; i++)
        {
            var prev = movements[i - 2];
            var curr = movements[i - 1];
            var next = movements[i];

            var dir1X = (double)curr.PositionX - (double)prev.PositionX;
            var dir1Y = (double)curr.PositionY - (double)prev.PositionY;
            var dir2X = (double)next.PositionX - (double)curr.PositionX;
            var dir2Y = (double)next.PositionY - (double)curr.PositionY;

            // Check if directions are roughly opposite
            var dotProduct = dir1X * dir2X + dir1Y * dir2Y;
            if (dotProduct < 0) directionChanges++;
        }

        return directionChanges >= movements.Count * 0.4; // 40% direction changes
    }

    private bool IsStraightLineMovement(List<Models.PlayerBehaviorEvent> movements)
    {
        if (movements.Count < 4) return false;

        var first = movements.First();
        var last = movements.Last();
        
        var expectedDirX = (double)last.PositionX - (double)first.PositionX;
        var expectedDirY = (double)last.PositionY - (double)first.PositionY;
        var expectedLength = Math.Sqrt(expectedDirX * expectedDirX + expectedDirY * expectedDirY);
        
        if (expectedLength < 50) return false; // Too short to be considered straight line
        
        expectedDirX /= expectedLength;
        expectedDirY /= expectedLength;

        var deviations = 0;
        for (int i = 1; i < movements.Count - 1; i++)
        {
            var curr = movements[i];
            var expectedX = (double)first.PositionX + expectedDirX * i * (expectedLength / movements.Count);
            var expectedY = (double)first.PositionY + expectedDirY * i * (expectedLength / movements.Count);
            
            var deviation = Math.Sqrt(Math.Pow((double)curr.PositionX - expectedX, 2) + Math.Pow((double)curr.PositionY - expectedY, 2));
            if (deviation > 20) deviations++;
        }

        return deviations < movements.Count * 0.2; // Less than 20% deviation
    }

    private bool IsErraticMovement(List<Models.PlayerBehaviorEvent> movements)
    {
        if (movements.Count < 5) return false;

        var velocityChanges = 0;
        for (int i = 1; i < movements.Count; i++)
        {
            var prevVel = movements[i - 1].Velocity ?? 0;
            var currVel = movements[i].Velocity ?? 0;
            
            if (Math.Abs(currVel - prevVel) > 50) velocityChanges++;
        }

        return velocityChanges > movements.Count * 0.6; // High velocity changes
    }

    private bool IsCampingBehavior(List<Models.PlayerBehaviorEvent> movements)
    {
        if (movements.Count < 10) return false;

        var avgX = movements.Average(m => (double)m.PositionX);
        var avgY = movements.Average(m => (double)m.PositionY);
        
        var maxDistance = movements.Max(m => 
            Math.Sqrt(Math.Pow((double)m.PositionX - avgX, 2) + Math.Pow((double)m.PositionY - avgY, 2)));
        
        return maxDistance < 30; // Player stayed within 30 units
    }

    private bool IsPeekingBehavior(List<Models.PlayerBehaviorEvent> movements)
    {
        if (movements.Count < 6) return false;

        // Look for quick forward-back movements (peek pattern)
        var peekCount = 0;
        for (int i = 2; i < movements.Count - 2; i++)
        {
            var start = movements[i - 2];
            var peak = movements[i];
            var end = movements[i + 2];
            
            var distToStart = Math.Sqrt(Math.Pow((double)peak.PositionX - (double)start.PositionX, 2) + 
                                      Math.Pow((double)peak.PositionY - (double)start.PositionY, 2));
            var distToEnd = Math.Sqrt(Math.Pow((double)end.PositionX - (double)start.PositionX, 2) + 
                                    Math.Pow((double)end.PositionY - (double)start.PositionY, 2));
            
            if (distToStart > 20 && distToEnd < 10) peekCount++;
        }

        return peekCount >= 2;
    }

    #endregion

    private void OnWeaponFire(Source1WeaponFireEvent e)
    {
        var demo = e.GameEventParser;
        if (demo == null || _currentDemoFile == null || _currentRound == null || e.Player == null) return;

        var player = GetOrCreatePlayer(e.Player);

        var weaponFire = new Models.WeaponFire
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound.Id,
            Tick = demo.CurrentDemoTick.Value,
            GameTime = (float)(demo.CurrentGameTime?.TotalSeconds ?? 0),
            PlayerId = player.Id,
            Weapon = e.Weapon,
            PositionX = (decimal)(e.PlayerPawn?.CBodyComponent?.SceneNode?.AbsOrigin?.X ?? 0),
            PositionY = (decimal)(e.PlayerPawn?.CBodyComponent?.SceneNode?.AbsOrigin?.Y ?? 0),
            PositionZ = (decimal)(e.PlayerPawn?.CBodyComponent?.SceneNode?.AbsOrigin?.Z ?? 0),
            Team = e.Player.Team.ToString(),
            IsScoped = e.PlayerPawn?.IsScoped ?? false,
            IsSilenced = e.Silenced
        };

        _weaponFires.Add(weaponFire);

        if (_currentRoundStats.TryGetValue(e.Player.Slot, out var roundStats))
        {
            roundStats.ShotsFired++;
        }
    }

    private void OnHegrenadeDetonate(Source1HegrenadeDetonateEvent e)
    {
        ProcessGrenadeDetonate(e.GameEventParser, "hegrenade", e.X, e.Y, e.Z, e.Player);
        ProcessAdvancedGrenadeDetonate(e.GameEventParser, "hegrenade", e.X, e.Y, e.Z, e.Player);
    }

    private void OnFlashbangDetonate(Source1FlashbangDetonateEvent e)
    {
        ProcessGrenadeDetonate(e.GameEventParser, "flashbang", e.X, e.Y, e.Z, e.Player);
        ProcessAdvancedGrenadeDetonate(e.GameEventParser, "flashbang", e.X, e.Y, e.Z, e.Player);
    }

    private void OnSmokegrenadeDetonate(Source1SmokegrenadeDetonateEvent e)
    {
        ProcessGrenadeDetonate(e.GameEventParser, "smokegrenade", e.X, e.Y, e.Z, e.Player);
        ProcessAdvancedGrenadeDetonate(e.GameEventParser, "smokegrenade", e.X, e.Y, e.Z, e.Player);
    }

    private void OnDecoyDetonate(Source1TagrenadeDetonateEvent e)
    {
        ProcessGrenadeDetonate(e.GameEventParser, "decoy", e.X, e.Y, e.Z, e.Player);
        ProcessAdvancedGrenadeDetonate(e.GameEventParser, "decoy", e.X, e.Y, e.Z, e.Player);
    }

    private void ProcessGrenadeDetonate(CsDemoParser? demo, string grenadeType, float x, float y, float z, DemoFile.Game.Cs.CCSPlayerController? player)
    {
        if (demo == null || _currentDemoFile == null || _currentRound == null || player == null) return;

        var playerModel = GetOrCreatePlayer(player);

        var grenade = new Models.Grenade
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound.Id,
            PlayerId = playerModel.Id,
            DetonateTick = demo.CurrentDemoTick.Value,
            DetonateTime = (float)(demo.CurrentGameTime?.TotalSeconds ?? 0),
            GrenadeType = grenadeType,
            DetonatePositionX = (decimal)x,
            DetonatePositionY = (decimal)y,
            DetonatePositionZ = (decimal)z,
            Team = player.Team.ToString()
        };

        _grenades.Add(grenade);
        LogGameEvent(demo, $"{grenadeType}_detonate", $"{player.PlayerName} {grenadeType} detonated");
    }

    private void OnBombPlanted(Source1BombPlantedEvent e)
    {
        ProcessBombEvent(e.GameEventParser, "planted", e.Player, e.Site);
    }

    private void OnBombDefused(Source1BombDefusedEvent e)
    {
        ProcessBombEvent(e.GameEventParser, "defused", e.Player, e.Site);
    }

    private void OnBombExploded(Source1BombExplodedEvent e)
    {
        ProcessBombEvent(e.GameEventParser, "exploded", e.Player, e.Site);
    }

    private void ProcessBombEvent(CsDemoParser? demo, string eventType, DemoFile.Game.Cs.CCSPlayerController? player, int? site)
    {
        if (demo == null || _currentDemoFile == null || _currentRound == null || player == null) return;

        var playerModel = GetOrCreatePlayer(player);

        var bomb = new Models.Bomb
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound.Id,
            EventType = eventType,
            Tick = demo.CurrentDemoTick.Value,
            GameTime = (float)(demo.CurrentGameTime?.TotalSeconds ?? 0),
            PlayerId = playerModel.Id,
            Site = site?.ToString(),
            PositionX = (decimal)(player.PlayerPawn?.CBodyComponent?.SceneNode?.AbsOrigin?.X ?? 0),
            PositionY = (decimal)(player.PlayerPawn?.CBodyComponent?.SceneNode?.AbsOrigin?.Y ?? 0),
            PositionZ = (decimal)(player.PlayerPawn?.CBodyComponent?.SceneNode?.AbsOrigin?.Z ?? 0),
            Team = player.Team.ToString(),
            HasKit = player.PlayerPawn?.HasDefuser ?? false
        };

        _bombs.Add(bomb);
        LogGameEvent(demo, $"bomb_{eventType}", $"{player.PlayerName} {eventType} bomb", true);
    }

    private void OnPlayerConnect(Source1PlayerConnectEvent e)
    {
        LogGameEvent(e.GameEventParser, "player_connect", $"{e.Name} connected");
    }

    private void OnPlayerDisconnect(Source1PlayerDisconnectEvent e)
    {
        LogGameEvent(e.GameEventParser, "player_disconnect", $"{e.Name} disconnected: {e.Reason}");
    }

    private void OnPlayerTeam(Source1PlayerTeamEvent e)
    {
        LogGameEvent(e.GameEventParser, "player_team", $"Player {e.Player?.PlayerName} changed team");
    }

    private void OnPlayerConnectedServices(DemoFile.Game.Cs.CCSPlayerController entity)
    {
        if (entity.PlayerName != null)
        {
            GetOrCreatePlayer(entity);
        }
    }

    private void OnTickEnd(object? sender, EventArgs e)
    {
        var demo = sender as CsDemoParser;
        if (demo == null || _currentDemoFile == null) return;

        var parsePositions = _configuration.GetValue<bool>("ParserSettings:ParsePlayerPositions", true);
        var positionInterval = _configuration.GetValue<int>("ParserSettings:PlayerPositionInterval", 16);

        if (parsePositions && demo.CurrentDemoTick.Value % positionInterval == 0)
        {
            foreach (var player in demo.Players)
            {
                if (player.PlayerName != null && player.PlayerPawn != null)
                {
                    var playerModel = GetOrCreatePlayer(player);
                    var position = new Models.PlayerPosition
                    {
                        DemoFileId = _currentDemoFile.Id,
                        PlayerId = playerModel.Id,
                        Tick = demo.CurrentDemoTick.Value,
                        GameTime = (float)(demo.CurrentGameTime?.TotalSeconds ?? 0),
                        PositionX = (decimal)(player.PlayerPawn.CBodyComponent?.SceneNode?.AbsOrigin?.X ?? 0),
                        PositionY = (decimal)(player.PlayerPawn.CBodyComponent?.SceneNode?.AbsOrigin?.Y ?? 0),
                        PositionZ = (decimal)(player.PlayerPawn.CBodyComponent?.SceneNode?.AbsOrigin?.Z ?? 0),
                        ViewAngleX = (decimal)(player.PlayerPawn.EyeAngles?.X ?? 0),
                        ViewAngleY = (decimal)(player.PlayerPawn.EyeAngles?.Y ?? 0),
                        ViewAngleZ = (decimal)(player.PlayerPawn.EyeAngles?.Z ?? 0),
                        VelocityX = (decimal)(player.PlayerPawn.CBodyComponent?.SceneNode?.AbsVelocity?.X ?? 0),
                        VelocityY = (decimal)(player.PlayerPawn.CBodyComponent?.SceneNode?.AbsVelocity?.Y ?? 0),
                        VelocityZ = (decimal)(player.PlayerPawn.CBodyComponent?.SceneNode?.AbsVelocity?.Z ?? 0),
                        Speed = player.PlayerPawn.CBodyComponent?.SceneNode?.AbsVelocity?.Length2D() ?? 0,
                        IsAlive = player.PlayerPawn.Health > 0,
                        Health = player.PlayerPawn.Health,
                        Armor = player.PlayerPawn.ArmorValue,
                        HasHelmet = player.PlayerPawn.HasHelmet,
                        HasDefuseKit = player.PlayerPawn.HasDefuser,
                        IsScoped = player.PlayerPawn.IsScoped,
                        IsCrouching = player.PlayerPawn.Flags.HasFlag(PlayerFlags.Ducking),
                        ActiveWeapon = player.PlayerPawn.WeaponServices?.ActiveWeapon?.Value?.DesignerName,
                        Team = player.Team.ToString(),
                        Money = player.MoneyServices?.Account ?? 0,
                        IsBlind = player.PlayerPawn.FlashDuration > 0,
                        FlashDuration = (int)player.PlayerPawn.FlashDuration
                    };

                    _playerPositions.Add(position);
                }
            }
        }
    }

    private Models.Player GetOrCreatePlayer(DemoFile.Game.Cs.CCSPlayerController playerController)
    {
        if (_players.TryGetValue(playerController.Slot, out var existingPlayer))
        {
            return existingPlayer;
        }

        var player = new Models.Player
        {
            DemoFileId = _currentDemoFile!.Id,
            PlayerSlot = playerController.Slot,
            UserId = playerController.UserId ?? 0,
            SteamId = playerController.SteamId,
            PlayerName = playerController.PlayerName,
            Team = playerController.Team.ToString(),
            IsBot = playerController.IsBot,
            IsHltv = playerController.IsHltv,
            IsConnected = playerController.Connected,
            ConnectedAt = DateTime.UtcNow
        };

        _players.TryAdd(playerController.Slot, player);
        return player;
    }

    private void LogGameEvent(CsDemoParser demo, string eventName, string description, bool isImportant = false)
    {
        if (_currentDemoFile == null) return;

        var gameEvent = new Models.GameEvent
        {
            DemoFileId = _currentDemoFile.Id,
            Tick = demo.CurrentDemoTick.Value,
            GameTime = (float)(demo.CurrentGameTime?.TotalSeconds ?? 0),
            EventName = eventName,
            Description = description,
            IsImportant = isImportant,
            RoundNumber = _currentRoundNumber,
            CreatedAt = DateTime.UtcNow
        };

        _gameEvents.Add(gameEvent);
    }

    // ==== CORRECTED EVENT HANDLERS FOR EXISTING EVENTS ====

    #region Advanced Bomb Events

    private void OnBombBeginPlant(Source1BombBeginplantEvent e)
    {
        ProcessBombEvent(e.GameEventParser, "begin_plant", e.Player, e.Site);
    }

    private void OnBombAbortPlant(Source1BombAbortplantEvent e)
    {
        ProcessBombEvent(e.GameEventParser, "abort_plant", e.Player, e.Site);
    }

    private void OnBombBeginDefuse(Source1BombBegindefuseEvent e)
    {
        ProcessBombEvent(e.GameEventParser, "begin_defuse", e.Player, null);
    }

    private void OnBombAbortDefuse(Source1BombAbortdefuseEvent e)
    {
        ProcessBombEvent(e.GameEventParser, "abort_defuse", e.Player, null);
    }

    private void OnBombDropped(Source1BombDroppedEvent e)
    {
        ProcessBombEvent(e.GameEventParser, "dropped", e.Player, null);
    }

    private void OnBombPickup(Source1BombPickupEvent e)
    {
        ProcessBombEvent(e.GameEventParser, "pickup", e.Player, null);
    }

    #endregion

    #region Zone Events

    private void OnEnterBombZone(Source1EnterBombzoneEvent e)
    {
        var zoneName = e.ZoneId == 0 ? "bombzone_a" : "bombzone_b";
        ProcessZoneEvent(e.GameEventParser, "enter_bombzone", zoneName, e.Player);
    }

    private void OnExitBombZone(Source1ExitBombzoneEvent e)
    {
        var zoneName = e.ZoneId == 0 ? "bombzone_a" : "bombzone_b";
        ProcessZoneEvent(e.GameEventParser, "exit_bombzone", zoneName, e.Player);
    }

    private void ProcessZoneEvent(CsDemoParser? demo, string eventType, string zoneType, DemoFile.Game.Cs.CCSPlayerController? player)
    {
        if (demo == null || _currentDemoFile == null || player == null) return;

        var playerModel = GetOrCreatePlayer(player);

        var zoneEvent = new Models.ZoneEvent
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound?.Id,
            PlayerId = playerModel.Id,
            Tick = demo.CurrentDemoTick.Value,
            GameTime = (float)(demo.CurrentGameTime?.TotalSeconds ?? 0),
            EventType = eventType,
            ZoneType = zoneType,
            PositionX = (decimal)(player.PlayerPawn?.CBodyComponent?.SceneNode?.AbsOrigin?.X ?? 0),
            PositionY = (decimal)(player.PlayerPawn?.CBodyComponent?.SceneNode?.AbsOrigin?.Y ?? 0),
            PositionZ = (decimal)(player.PlayerPawn?.CBodyComponent?.SceneNode?.AbsOrigin?.Z ?? 0),
            Team = player.Team.ToString(),
            RoundNumber = _currentRoundNumber,
            Description = $"{player.PlayerName} {eventType.Replace("_", " ")}"
        };

        _zoneEvents.Add(zoneEvent);
        LogGameEvent(demo, eventType, $"{player.PlayerName} {eventType.Replace("_", " ")} {zoneType}");
    }

    #endregion

    #region Player Behavior Events

    private void OnPlayerFallDamage(Source1PlayerFalldamageEvent e)
    {
        ProcessMovementEvent(e.GameEventParser, "fall_damage", e.Player);
        LogGameEvent(e.GameEventParser, "player_falldamage", $"{e.Player?.PlayerName} took {e.Damage} fall damage");
    }

    private void ProcessMovementEvent(CsDemoParser? demo, string movementType, DemoFile.Game.Cs.CCSPlayerController? player)
    {
        if (demo == null || _currentDemoFile == null || player?.PlayerPawn == null) return;

        var playerModel = GetOrCreatePlayer(player);

        var movement = new Models.PlayerMovement
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound?.Id,
            PlayerId = playerModel.Id,
            Tick = demo.CurrentDemoTick.Value,
            GameTime = (float)(demo.CurrentGameTime?.TotalSeconds ?? 0),
            MovementType = movementType,
            PositionX = (decimal)(player.PlayerPawn.CBodyComponent?.SceneNode?.AbsOrigin?.X ?? 0),
            PositionY = (decimal)(player.PlayerPawn.CBodyComponent?.SceneNode?.AbsOrigin?.Y ?? 0),
            PositionZ = (decimal)(player.PlayerPawn.CBodyComponent?.SceneNode?.AbsOrigin?.Z ?? 0),
            VelocityX = (decimal)(player.PlayerPawn.CBodyComponent?.SceneNode?.AbsVelocity?.X ?? 0),
            VelocityY = (decimal)(player.PlayerPawn.CBodyComponent?.SceneNode?.AbsVelocity?.Y ?? 0),
            VelocityZ = (decimal)(player.PlayerPawn.CBodyComponent?.SceneNode?.AbsVelocity?.Z ?? 0),
            Speed = player.PlayerPawn.CBodyComponent?.SceneNode?.AbsVelocity?.Length2D() ?? 0,
            SpeedHorizontal = player.PlayerPawn.CBodyComponent?.SceneNode?.AbsVelocity?.Length2D() ?? 0,
            ViewAngleX = (decimal)(player.PlayerPawn.EyeAngles?.X ?? 0),
            ViewAngleY = (decimal)(player.PlayerPawn.EyeAngles?.Y ?? 0),
            Team = player.Team.ToString(),
            IsOnGround = !player.PlayerPawn.Flags.HasFlag(PlayerFlags.OnGround),
            IsInAir = player.PlayerPawn.Flags.HasFlag(PlayerFlags.OnGround),
            IsDucking = player.PlayerPawn.Flags.HasFlag(PlayerFlags.Ducking),
            IsWalking = player.PlayerPawn.Flags.HasFlag(PlayerFlags.Walking)
        };

        _playerMovements.Add(movement);
    }

    #endregion

    #region Match Statistics and End-of-Match Data Tracking

    private void InitializeMatchStatistics()
    {
        if (_demo == null || _currentDemoFile == null || _currentMatch == null) return;

        _matchStartTime = DateTime.UtcNow;
        _teamMatchStats["CT"].Clear();
        _teamMatchStats["T"].Clear();
        _roundHistory.Clear();

        // Initialize team performance tracking
        InitializeTeamStatistics("CT");
        InitializeTeamStatistics("T");

        // Track match start
        TrackMatchStatistic("match_start", "Match started", 1);
        
        // Setup advanced user message handlers for match end conditions
        if (_demo.UserMessageEvents.MatchEndConditions != null)
        {
            _demo.UserMessageEvents.MatchEndConditions += OnMatchEndConditions;
        }
        if (_demo.UserMessageEvents.EndOfMatchAllPlayersData != null)
        {
            _demo.UserMessageEvents.EndOfMatchAllPlayersData += OnEndOfMatchAllPlayersData;
        }
        if (_demo.UserMessageEvents.RoundEndReportData != null)
        {
            _demo.UserMessageEvents.RoundEndReportData += OnRoundEndReportData;
        }

        _logger.LogInformation("Match statistics tracking initialized");
    }

    private void InitializeTeamStatistics(string team)
    {
        _teamMatchStats[team]["total_rounds"] = 0;
        _teamMatchStats[team]["rounds_won"] = 0;
        _teamMatchStats[team]["rounds_lost"] = 0;
        _teamMatchStats[team]["total_kills"] = 0;
        _teamMatchStats[team]["total_deaths"] = 0;
        _teamMatchStats[team]["total_assists"] = 0;
        _teamMatchStats[team]["total_damage"] = 0;
        _teamMatchStats[team]["total_money_spent"] = 0;
        _teamMatchStats[team]["eco_rounds"] = 0;
        _teamMatchStats[team]["force_buy_rounds"] = 0;
        _teamMatchStats[team]["full_buy_rounds"] = 0;
        _teamMatchStats[team]["pistol_rounds_won"] = 0;
        _teamMatchStats[team]["anti_eco_rounds_won"] = 0;
        _teamMatchStats[team]["clutch_rounds_won"] = 0;
        _teamMatchStats[team]["consecutive_wins"] = 0;
        _teamMatchStats[team]["max_consecutive_wins"] = 0;
        _teamMatchStats[team]["first_kills"] = 0;
        _teamMatchStats[team]["multikills"] = 0;
        _teamMatchStats[team]["headshot_percentage"] = 0.0f;
        _teamMatchStats[team]["utility_damage"] = 0;
        _teamMatchStats[team]["weapon_accuracy"] = 0.0f;
    }

    private void OnMatchEndConditions(CCSUsrMsg_MatchEndConditions msg)
    {
        if (_demo == null || _currentDemoFile == null || _currentMatch == null) return;

        _matchEndTime = DateTime.UtcNow;

        // Track match end conditions
        TrackMatchStatistic("match_end_conditions", "Match end conditions received", 1);

        // Create comprehensive match end data
        CreateEndOfMatchData();
        
        _logger.LogInformation("Match end conditions processed");
    }

    private void OnEndOfMatchAllPlayersData(CCSUsrMsg_EndOfMatchAllPlayersData msg)
    {
        if (_demo == null || _currentDemoFile == null || _currentMatch == null) return;

        try
        {
            // Process end-of-match player data
            foreach (var playerData in msg.AllPlayersData)
            {
                ProcessEndOfMatchPlayerData(playerData);
            }

            // Calculate final team performances
            CalculateFinalTeamPerformances();

            // Generate comprehensive match statistics
            GenerateComprehensiveMatchStatistics();

            TrackMatchStatistic("end_of_match_data", "End of match data processed", msg.AllPlayersData.Count);
            
            _logger.LogInformation("End of match data processed for {Count} players", msg.AllPlayersData.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing end of match data");
        }
    }

    private void OnRoundEndReportData(CCSUsrMsg_RoundEndReportData msg)
    {
        if (_demo == null || _currentDemoFile == null || _currentMatch == null) return;

        try
        {
            // Track round end statistics
            var roundOutcome = new Models.RoundOutcome
            {
                DemoFileId = _currentDemoFile.Id,
                MatchId = _currentMatch.Id,
                RoundNumber = _currentRoundNumber,
                Tick = _demo.CurrentDemoTick.Value,
                GameTime = (float)(_demo.CurrentGameTime?.TotalSeconds ?? 0),
                WinnerTeam = DetermineRoundWinner(),
                RoundEndReason = msg.ReportData?.FirstOrDefault()?.ToString() ?? "unknown",
                CTScore = _demo.TeamCounterTerrorist?.Score ?? 0,
                TScore = _demo.TeamTerrorist?.Score ?? 0,
                RoundDuration = _currentRound != null ? (DateTime.UtcNow - _currentRound.StartTime).TotalSeconds : 0,
                PlantedBomb = _bombs.Any(b => b.RoundId == _currentRound?.Id && b.EventType == "plant"),
                DefusedBomb = _bombs.Any(b => b.RoundId == _currentRound?.Id && b.EventType == "defuse"),
                ExplodedBomb = _bombs.Any(b => b.RoundId == _currentRound?.Id && b.EventType == "explode"),
                Description = $"Round {_currentRoundNumber} ended"
            };

            _roundOutcomes.Add(roundOutcome);
            _roundHistory.Add(roundOutcome);

            // Update team statistics
            UpdateTeamStatisticsForRound(roundOutcome);

            TrackMatchStatistic("round_end_report", "Round end report processed", 1);
            
            _logger.LogInformation("Round end report processed for round {Round}", _currentRoundNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing round end report data");
        }
    }

    private void CreateEndOfMatchData()
    {
        if (_currentMatch == null || _currentDemoFile == null) return;

        var matchDuration = (_matchEndTime - _matchStartTime).TotalSeconds;
        var totalRounds = _roundHistory.Count;
        var ctWins = _roundHistory.Count(r => r.WinnerTeam == "CT");
        var tWins = _roundHistory.Count(r => r.WinnerTeam == "T");

        var endOfMatchData = new Models.EndOfMatchData
        {
            DemoFileId = _currentDemoFile.Id,
            MatchId = _currentMatch.Id,
            MatchDuration = matchDuration,
            TotalRounds = totalRounds,
            CTWins = ctWins,
            TWins = tWins,
            FinalScore = $"{Math.Max(ctWins, tWins)}-{Math.Min(ctWins, tWins)}",
            WinnerTeam = ctWins > tWins ? "CT" : "T",
            WinnerScore = Math.Max(ctWins, tWins),
            LoserScore = Math.Min(ctWins, tWins),
            MatchEndReason = DetermineMatchEndReason(),
            OvertimeRounds = Math.Max(0, totalRounds - 30),
            PistolRoundsWon = CalculatePistolRoundsWon(),
            AntiEcoRoundsWon = CalculateAntiEcoRoundsWon(),
            ClutchRoundsTotal = CalculateClutchRoundsTotal(),
            TotalKills = _kills.Count,
            TotalDeaths = _kills.Count,
            TotalAssists = _kills.Count(k => k.AssisterId.HasValue),
            TotalDamage = _damages.Sum(d => d.Damage),
            TotalHeadshots = _kills.Count(k => k.IsHeadshot),
            TotalWallbangs = _kills.Count(k => k.IsWallbang),
            AveragePlayersPerRound = CalculateAveragePlayersPerRound(),
            MapName = _demo.Map,
            ServerName = _demo.ServerName ?? "Unknown",
            Description = $"Match completed: {_demo.Map} - {ctWins}:{tWins}"
        };

        _endOfMatchData.Add(endOfMatchData);
    }

    private void ProcessEndOfMatchPlayerData(CCSUsrMsg_EndOfMatchAllPlayersData.Types.PlayerData playerData)
    {
        // Find the corresponding player in our system
        var player = _players.Values.FirstOrDefault(p => p.SteamId == playerData.AccountId);
        if (player == null) return;

        // Create detailed end-of-match player statistics
        var playerEndMatchStats = new Models.EndOfMatchData
        {
            DemoFileId = _currentDemoFile.Id,
            MatchId = _currentMatch.Id,
            PlayerId = player.Id,
            PlayerName = player.Name,
            SteamId = player.SteamId,
            Team = player.Team,
            Kills = playerData.Kills,
            Deaths = playerData.Deaths,
            Assists = playerData.Assists,
            Score = playerData.Score,
            MVP = playerData.Mvps,
            TotalDamage = (int)playerData.HealthPointsDealtTotal,
            UtilityDamage = (int)playerData.UtilityDamageTotal,
            EnemiesFlashed = playerData.EnemiesFlashedTotal,
            EquipmentValue = (int)playerData.EquipmentValue,
            MoneySpent = (int)playerData.MoneySpentTotal,
            LiveTime = (int)playerData.LiveTimeTotal,
            HeadshotKills = playerData.HeadshotKills,
            AceRounds = playerData.AceRounds,
            QuadKills = playerData.QuadKills,
            TripleKills = playerData.TripleKills,
            DoubleKills = playerData.DoubleKills,
            ClutchKills = playerData.ClutchKills,
            Description = $"End of match stats for {player.Name}"
        };

        _endOfMatchData.Add(playerEndMatchStats);
    }

    private void CalculateFinalTeamPerformances()
    {
        foreach (var team in new[] { "CT", "T" })
        {
            var teamPlayers = _players.Values.Where(p => p.Team == team).ToList();
            if (!teamPlayers.Any()) continue;

            var teamPerformance = new Models.TeamPerformance
            {
                DemoFileId = _currentDemoFile.Id,
                MatchId = _currentMatch.Id,
                Team = team,
                RoundsWon = (int)_teamMatchStats[team]["rounds_won"],
                RoundsLost = (int)_teamMatchStats[team]["rounds_lost"],
                TotalKills = (int)_teamMatchStats[team]["total_kills"],
                TotalDeaths = (int)_teamMatchStats[team]["total_deaths"],
                TotalAssists = (int)_teamMatchStats[team]["total_assists"],
                TotalDamage = (int)_teamMatchStats[team]["total_damage"],
                MoneySpent = (int)_teamMatchStats[team]["total_money_spent"],
                EcoRounds = (int)_teamMatchStats[team]["eco_rounds"],
                ForceBuyRounds = (int)_teamMatchStats[team]["force_buy_rounds"],
                FullBuyRounds = (int)_teamMatchStats[team]["full_buy_rounds"],
                PistolRoundsWon = (int)_teamMatchStats[team]["pistol_rounds_won"],
                AntiEcoRoundsWon = (int)_teamMatchStats[team]["anti_eco_rounds_won"],
                ClutchRoundsWon = (int)_teamMatchStats[team]["clutch_rounds_won"],
                MaxConsecutiveWins = (int)_teamMatchStats[team]["max_consecutive_wins"],
                FirstKills = (int)_teamMatchStats[team]["first_kills"],
                Multikills = (int)_teamMatchStats[team]["multikills"],
                HeadshotPercentage = (float)_teamMatchStats[team]["headshot_percentage"],
                UtilityDamage = (int)_teamMatchStats[team]["utility_damage"],
                WeaponAccuracy = (float)_teamMatchStats[team]["weapon_accuracy"],
                AveragePlayersAlive = CalculateAveragePlayersAlive(team),
                WinPercentage = CalculateWinPercentage(team),
                Description = $"Final performance for team {team}"
            };

            _teamPerformances.Add(teamPerformance);
        }
    }

    private void GenerateComprehensiveMatchStatistics()
    {
        var mapStats = new Models.MapStatistic
        {
            DemoFileId = _currentDemoFile.Id,
            MatchId = _currentMatch.Id,
            MapName = _demo.Map,
            TotalRounds = _roundHistory.Count,
            CTWins = _roundHistory.Count(r => r.WinnerTeam == "CT"),
            TWins = _roundHistory.Count(r => r.WinnerTeam == "T"),
            BombPlants = _bombs.Count(b => b.EventType == "plant"),
            BombDefuses = _bombs.Count(b => b.EventType == "defuse"),
            BombExplosions = _bombs.Count(b => b.EventType == "explode"),
            TotalKills = _kills.Count,
            TotalHeadshots = _kills.Count(k => k.IsHeadshot),
            TotalWallbangs = _kills.Count(k => k.IsWallbang),
            TotalDamage = _damages.Sum(d => d.Damage),
            UtilityDamage = _grenades.Sum(g => g.Damage ?? 0),
            FlashbangsThrown = _grenades.Count(g => g.Type == "flashbang"),
            SmokesThrown = _grenades.Count(g => g.Type == "smoke"),
            HEGrenadesThrown = _grenades.Count(g => g.Type == "hegrenade"),
            MolotovsThrown = _grenades.Count(g => g.Type == "molotov" || g.Type == "incgrenade"),
            AverageRoundDuration = _roundHistory.Any() ? _roundHistory.Average(r => r.RoundDuration) : 0,
            LongestRound = _roundHistory.Any() ? _roundHistory.Max(r => r.RoundDuration) : 0,
            ShortestRound = _roundHistory.Any() ? _roundHistory.Min(r => r.RoundDuration) : 0,
            OvertimeRounds = Math.Max(0, _roundHistory.Count - 30),
            Description = $"Comprehensive statistics for {_demo.Map}"
        };

        _mapStatistics.Add(mapStats);
    }

    private void UpdateTeamStatisticsForRound(Models.RoundOutcome roundOutcome)
    {
        var winnerTeam = roundOutcome.WinnerTeam;
        var loserTeam = winnerTeam == "CT" ? "T" : "CT";

        // Update winner stats
        _teamMatchStats[winnerTeam]["total_rounds"] = (int)_teamMatchStats[winnerTeam]["total_rounds"] + 1;
        _teamMatchStats[winnerTeam]["rounds_won"] = (int)_teamMatchStats[winnerTeam]["rounds_won"] + 1;
        _teamMatchStats[winnerTeam]["consecutive_wins"] = (int)_teamMatchStats[winnerTeam]["consecutive_wins"] + 1;
        
        var consecutiveWins = (int)_teamMatchStats[winnerTeam]["consecutive_wins"];
        if (consecutiveWins > (int)_teamMatchStats[winnerTeam]["max_consecutive_wins"])
        {
            _teamMatchStats[winnerTeam]["max_consecutive_wins"] = consecutiveWins;
        }

        // Update loser stats
        _teamMatchStats[loserTeam]["total_rounds"] = (int)_teamMatchStats[loserTeam]["total_rounds"] + 1;
        _teamMatchStats[loserTeam]["rounds_lost"] = (int)_teamMatchStats[loserTeam]["rounds_lost"] + 1;
        _teamMatchStats[loserTeam]["consecutive_wins"] = 0; // Reset consecutive wins

        // Update round-specific statistics
        var roundKills = _kills.Where(k => k.RoundId == _currentRound?.Id).ToList();
        var roundDamages = _damages.Where(d => d.RoundId == _currentRound?.Id).ToList();

        foreach (var team in new[] { "CT", "T" })
        {
            var teamPlayers = _players.Values.Where(p => p.Team == team).ToList();
            var teamKills = roundKills.Where(k => teamPlayers.Any(p => p.Id == k.KillerId)).ToList();
            var teamDeaths = roundKills.Where(k => teamPlayers.Any(p => p.Id == k.VictimId)).ToList();
            var teamAssists = roundKills.Where(k => teamPlayers.Any(p => p.Id == k.AssisterId)).ToList();
            var teamDamage = roundDamages.Where(d => teamPlayers.Any(p => p.Id == d.AttackerId)).ToList();

            _teamMatchStats[team]["total_kills"] = (int)_teamMatchStats[team]["total_kills"] + teamKills.Count;
            _teamMatchStats[team]["total_deaths"] = (int)_teamMatchStats[team]["total_deaths"] + teamDeaths.Count;
            _teamMatchStats[team]["total_assists"] = (int)_teamMatchStats[team]["total_assists"] + teamAssists.Count;
            _teamMatchStats[team]["total_damage"] = (int)_teamMatchStats[team]["total_damage"] + teamDamage.Sum(d => d.Damage);
            _teamMatchStats[team]["first_kills"] = (int)_teamMatchStats[team]["first_kills"] + teamKills.Count(k => k.IsFirstKill);
            
            // Calculate headshot percentage
            var totalKills = (int)_teamMatchStats[team]["total_kills"];
            if (totalKills > 0)
            {
                var headshots = teamKills.Count(k => k.IsHeadshot);
                _teamMatchStats[team]["headshot_percentage"] = (float)headshots / totalKills * 100;
            }
        }
    }

    private void TrackMatchStatistic(string statType, string description, int value)
    {
        if (_demo == null || _currentDemoFile == null || _currentMatch == null) return;

        var matchStat = new Models.MatchStatistic
        {
            DemoFileId = _currentDemoFile.Id,
            MatchId = _currentMatch.Id,
            Tick = _demo.CurrentDemoTick.Value,
            GameTime = (float)(_demo.CurrentGameTime?.TotalSeconds ?? 0),
            StatisticType = statType,
            StatisticName = description,
            Value = value,
            RoundNumber = _currentRoundNumber,
            Team = null, // Match-level statistic
            Description = description
        };

        _matchStatistics.Add(matchStat);
    }

    // Helper methods for match statistics calculations
    private string DetermineRoundWinner()
    {
        if (_demo?.TeamCounterTerrorist?.Score > _demo?.TeamTerrorist?.Score)
            return "CT";
        else if (_demo?.TeamTerrorist?.Score > _demo?.TeamCounterTerrorist?.Score)
            return "T";
        return "Unknown";
    }

    private string DetermineMatchEndReason()
    {
        var ctScore = _demo?.TeamCounterTerrorist?.Score ?? 0;
        var tScore = _demo?.TeamTerrorist?.Score ?? 0;
        var totalRounds = _roundHistory.Count;

        if (totalRounds == 30) return "Regular Time";
        if (totalRounds > 30) return "Overtime";
        if (Math.Abs(ctScore - tScore) >= 16) return "Early Finish";
        return "Unknown";
    }

    private int CalculatePistolRoundsWon()
    {
        return _roundHistory.Count(r => (r.RoundNumber == 1 || r.RoundNumber == 16) && r.WinnerTeam != "Unknown");
    }

    private int CalculateAntiEcoRoundsWon()
    {
        // Simplified calculation - would need more sophisticated economy tracking
        return _roundHistory.Count(r => r.RoundEndReason?.Contains("eco") == true);
    }

    private int CalculateClutchRoundsTotal()
    {
        // Count rounds where a clutch situation occurred
        return _roundHistory.Count(r => r.Description?.Contains("clutch") == true);
    }

    private float CalculateAveragePlayersPerRound()
    {
        if (_roundHistory.Count == 0) return 0;
        return (float)_players.Count / _roundHistory.Count * 10; // Assuming max 10 players
    }

    private float CalculateAveragePlayersAlive(string team)
    {
        var teamPlayers = _players.Values.Where(p => p.Team == team).Count();
        return teamPlayers > 0 ? (float)teamPlayers / 2 : 0; // Simplified
    }

    private float CalculateWinPercentage(string team)
    {
        var totalRounds = (int)_teamMatchStats[team]["total_rounds"];
        var roundsWon = (int)_teamMatchStats[team]["rounds_won"];
        return totalRounds > 0 ? (float)roundsWon / totalRounds * 100 : 0;
    }

    #endregion

    #region Weapon Events

    private void OnWeaponReload(Source1WeaponReloadEvent e)
    {
        ProcessWeaponStateEvent(e.GameEventParser, "reload", e.Player, e.Player?.PlayerPawn?.WeaponServices?.ActiveWeapon?.Value?.DesignerName);
    }

    private void OnWeaponZoom(Source1WeaponZoomEvent e)
    {
        ProcessWeaponStateEvent(e.GameEventParser, "zoom", e.Player, e.Player?.PlayerPawn?.WeaponServices?.ActiveWeapon?.Value?.DesignerName);
    }

    private void ProcessWeaponStateEvent(CsDemoParser? demo, string eventType, DemoFile.Game.Cs.CCSPlayerController? player, string? weaponName)
    {
        if (demo == null || _currentDemoFile == null || player == null || weaponName == null) return;

        var playerModel = GetOrCreatePlayer(player);

        var weaponState = new Models.WeaponState
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound?.Id,
            PlayerId = playerModel.Id,
            Tick = demo.CurrentDemoTick.Value,
            GameTime = (float)(demo.CurrentGameTime?.TotalSeconds ?? 0),
            EventType = eventType,
            WeaponName = weaponName,
            AmmoClip = player.PlayerPawn?.WeaponServices?.ActiveWeapon?.Value?.Clip1 ?? 0,
            AmmoReserve = player.PlayerPawn?.WeaponServices?.ActiveWeapon?.Value?.ReserveAmmo?[0] ?? 0,
            IsScoped = player.PlayerPawn?.IsScoped ?? false,
            PositionX = (decimal)(player.PlayerPawn?.CBodyComponent?.SceneNode?.AbsOrigin?.X ?? 0),
            PositionY = (decimal)(player.PlayerPawn?.CBodyComponent?.SceneNode?.AbsOrigin?.Y ?? 0),
            PositionZ = (decimal)(player.PlayerPawn?.CBodyComponent?.SceneNode?.AbsOrigin?.Z ?? 0),
            Team = player.Team.ToString(),
            RoundNumber = _currentRoundNumber,
            Description = $"{player.PlayerName} {eventType.Replace("_", " ")} {weaponName}"
        };

        _weaponStates.Add(weaponState);
        LogGameEvent(demo, $"weapon_{eventType}", weaponState.Description);
    }

    #endregion

    #region Advanced Entity Tracking

    private void OnItemPickup(Source1ItemPickupEvent e)
    {
        var demo = e.GameEventParser;
        if (demo == null || _currentDemoFile == null || e.Player == null) return;

        var player = GetOrCreatePlayer(e.Player);
        
        // Track entity interaction
        var interaction = new Models.EntityInteraction
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound?.Id,
            InitiatorPlayerId = player.Id,
            InteractionType = "pickup",
            SourceEntityType = "item",
            SourceEntityName = e.Item,
            Tick = demo.CurrentDemoTick.Value,
            GameTime = (float)(demo.CurrentGameTime?.TotalSeconds ?? 0),
            PositionX = (decimal)(e.PlayerPawn?.CBodyComponent?.SceneNode?.AbsOrigin?.X ?? 0),
            PositionY = (decimal)(e.PlayerPawn?.CBodyComponent?.SceneNode?.AbsOrigin?.Y ?? 0),
            PositionZ = (decimal)(e.PlayerPawn?.CBodyComponent?.SceneNode?.AbsOrigin?.Z ?? 0),
            IsSuccessful = true,
            Description = $"{player.PlayerName} picked up {e.Item}"
        };
        
        _entityInteractions.Add(interaction);
        
        // Update dropped item if it exists
        if (_activeDroppedItems.Values.FirstOrDefault(di => di.ItemName == e.Item && !di.WasPickedUp) is var droppedItem && droppedItem != null)
        {
            droppedItem.PickupTick = demo.CurrentDemoTick.Value;
            droppedItem.PickupTime = (float)(demo.CurrentGameTime?.TotalSeconds ?? 0);
            droppedItem.PickerPlayerId = player.Id;
            droppedItem.PickerTeam = e.Player.Team.ToString();
            droppedItem.WasPickedUp = true;
            droppedItem.TimeOnGround = droppedItem.PickupTime - droppedItem.DropTime;
            droppedItem.PickupPositionX = interaction.PositionX;
            droppedItem.PickupPositionY = interaction.PositionY;
            droppedItem.PickupPositionZ = interaction.PositionZ;
        }

        LogEntityLifecycle(demo, "item", e.Item, "pickup", player, interaction.PositionX, interaction.PositionY, interaction.PositionZ);
    }

    private void OnItemRemove(Source1ItemRemoveEvent e)
    {
        var demo = e.GameEventParser;
        if (demo == null || _currentDemoFile == null || e.Player == null) return;

        var player = GetOrCreatePlayer(e.Player);
        
        // Track entity interaction
        var interaction = new Models.EntityInteraction
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound?.Id,
            InitiatorPlayerId = player.Id,
            InteractionType = "remove",
            SourceEntityType = "item",
            SourceEntityName = e.Item,
            Tick = demo.CurrentDemoTick.Value,
            GameTime = (float)(demo.CurrentGameTime?.TotalSeconds ?? 0),
            PositionX = (decimal)(e.PlayerPawn?.CBodyComponent?.SceneNode?.AbsOrigin?.X ?? 0),
            PositionY = (decimal)(e.PlayerPawn?.CBodyComponent?.SceneNode?.AbsOrigin?.Y ?? 0),
            PositionZ = (decimal)(e.PlayerPawn?.CBodyComponent?.SceneNode?.AbsOrigin?.Z ?? 0),
            IsSuccessful = true,
            Description = $"{player.PlayerName} removed {e.Item}"
        };
        
        _entityInteractions.Add(interaction);
        LogEntityLifecycle(demo, "item", e.Item, "remove", player, interaction.PositionX, interaction.PositionY, interaction.PositionZ);
    }

    private void OnAdvancedWeaponFire(Source1WeaponFireEvent e)
    {
        var demo = e.GameEventParser;
        if (demo == null || _currentDemoFile == null || e.Player == null) return;

        var player = GetOrCreatePlayer(e.Player);
        
        // Track weapon interaction
        var interaction = new Models.EntityInteraction
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound?.Id,
            InitiatorPlayerId = player.Id,
            InteractionType = "fire",
            SourceEntityType = "weapon",
            SourceEntityName = e.Weapon,
            Tick = demo.CurrentDemoTick.Value,
            GameTime = (float)(demo.CurrentGameTime?.TotalSeconds ?? 0),
            PositionX = (decimal)(e.PlayerPawn?.CBodyComponent?.SceneNode?.AbsOrigin?.X ?? 0),
            PositionY = (decimal)(e.PlayerPawn?.CBodyComponent?.SceneNode?.AbsOrigin?.Y ?? 0),
            PositionZ = (decimal)(e.PlayerPawn?.CBodyComponent?.SceneNode?.AbsOrigin?.Z ?? 0),
            IsSuccessful = true,
            Properties = $"{{\"scoped\":{(e.PlayerPawn?.IsScoped ?? false).ToString().ToLower()},\"silenced\":{e.Silenced.ToString().ToLower()}}}",
            Description = $"{player.PlayerName} fired {e.Weapon}"
        };
        
        _entityInteractions.Add(interaction);
    }

    private void OnEntityCreate(DemoFile.Game.Cs.CBaseEntity entity)
    {
        if (_demo == null || _currentDemoFile == null) return;

        // Track entity creation
        LogEntityLifecycle(_demo, GetEntityType(entity), GetEntityName(entity), "spawn", null, 
            (decimal)(entity.CBodyComponent?.SceneNode?.AbsOrigin?.X ?? 0),
            (decimal)(entity.CBodyComponent?.SceneNode?.AbsOrigin?.Y ?? 0), 
            (decimal)(entity.CBodyComponent?.SceneNode?.AbsOrigin?.Z ?? 0), entity.Index);
    }

    private void OnEntityDelete(DemoFile.Game.Cs.CBaseEntity entity)
    {
        if (_demo == null || _currentDemoFile == null) return;

        // Track entity destruction
        LogEntityLifecycle(_demo, GetEntityType(entity), GetEntityName(entity), "destroy", null,
            (decimal)(entity.CBodyComponent?.SceneNode?.AbsOrigin?.X ?? 0),
            (decimal)(entity.CBodyComponent?.SceneNode?.AbsOrigin?.Y ?? 0),
            (decimal)(entity.CBodyComponent?.SceneNode?.AbsOrigin?.Z ?? 0), entity.Index);
            
        // Clean up active entity tracking
        _activeDroppedItems.Remove(entity.Index);
        _activeSmokeClouds.Remove(entity.Index);
        _activeFireAreas.Remove(entity.Index);
    }

    private void OnRoundFreezeEnd(Source1RoundFreezeEndEvent e)
    {
        // Clear active entity states for new round
        _activeDroppedItems.Clear();
        _activeSmokeClouds.Clear();
        _activeFireAreas.Clear();
    }

    private void ProcessAdvancedGrenadeDetonate(CsDemoParser demo, string grenadeType, float x, float y, float z, DemoFile.Game.Cs.CCSPlayerController? player)
    {
        if (demo == null || _currentDemoFile == null || _currentRound == null || player == null) return;

        var playerModel = GetOrCreatePlayer(player);

        // Create entity effect for the grenade
        var effect = new Models.EntityEffect
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound.Id,
            SourcePlayerId = playerModel.Id,
            SourceEntityType = "grenade",
            SourceEntityName = grenadeType,
            EffectType = GetGrenadeEffectType(grenadeType),
            StartTick = demo.CurrentDemoTick.Value,
            StartTime = (float)(demo.CurrentGameTime?.TotalSeconds ?? 0),
            CenterX = (decimal)x,
            CenterY = (decimal)y,
            CenterZ = (decimal)z,
            Radius = GetGrenadeRadius(grenadeType),
            MaxIntensity = GetGrenadeIntensity(grenadeType),
            CurrentIntensity = GetGrenadeIntensity(grenadeType),
            Team = player.Team.ToString(),
            RoundNumber = _currentRoundNumber,
            Description = $"{player.PlayerName} {grenadeType} effect at {x:F1}, {y:F1}, {z:F1}"
        };

        // Handle special effects
        if (grenadeType == "smokegrenade")
        {
            var smokeCloud = new Models.SmokeCloud
            {
                DemoFileId = _currentDemoFile.Id,
                RoundId = _currentRound.Id,
                ThrowerPlayerId = playerModel.Id,
                GrenadeEntityId = 0, // Would need to track from entity creation
                StartTick = demo.CurrentDemoTick.Value,
                StartTime = (float)(demo.CurrentGameTime?.TotalSeconds ?? 0),
                CenterX = (decimal)x,
                CenterY = (decimal)y,
                CenterZ = (decimal)z,
                MaxRadius = 144f, // Standard CS2 smoke radius
                CurrentRadius = 0f,
                Opacity = 0f,
                Phase = "expanding",
                ExpansionTime = 3f, // Standard expansion time
                FullTime = 15f, // Standard full duration
                DissipationTime = 3f, // Standard dissipation time
                Duration = 18f, // Total duration
                Team = player.Team.ToString(),
                RoundNumber = _currentRoundNumber,
                Description = $"{player.PlayerName} smoke cloud"
            };

            _smokeClouds.Add(smokeCloud);
            effect.BlocksVision = true;
        }
        else if (grenadeType == "molotov" || grenadeType == "incgrenade")
        {
            var fireArea = new Models.FireArea
            {
                DemoFileId = _currentDemoFile.Id,
                RoundId = _currentRound.Id,
                ThrowerPlayerId = playerModel.Id,
                GrenadeEntityId = 0, // Would need to track from entity creation
                GrenadeType = grenadeType,
                StartTick = demo.CurrentDemoTick.Value,
                StartTime = (float)(demo.CurrentGameTime?.TotalSeconds ?? 0),
                CenterX = (decimal)x,
                CenterY = (decimal)y,
                CenterZ = (decimal)z,
                MaxRadius = 128f, // Standard fire radius
                CurrentRadius = 0f,
                Intensity = 100f,
                SpreadTime = 2f,
                PeakTime = 5f,
                BurnoutTime = 2f,
                Duration = 7f,
                DamagePerSecond = 40f, // Approximate damage
                Team = player.Team.ToString(),
                RoundNumber = _currentRoundNumber,
                CausesDamage = true,
                BlocksPath = true,
                Description = $"{player.PlayerName} fire area"
            };

            _fireAreas.Add(fireArea);
            effect.CausesDamage = true;
        }

        _entityEffects.Add(effect);
    }

    private void LogEntityLifecycle(CsDemoParser demo, string entityType, string entityName, string eventType, Models.Player? player, decimal x, decimal y, decimal z, int entityId = 0)
    {
        if (_currentDemoFile == null) return;

        var lifecycle = new Models.EntityLifecycle
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound?.Id,
            PlayerId = player?.Id,
            EntityType = entityType,
            EntityName = entityName,
            EventType = eventType,
            Tick = demo.CurrentDemoTick.Value,
            GameTime = (float)(demo.CurrentGameTime?.TotalSeconds ?? 0),
            PositionX = x,
            PositionY = y,
            PositionZ = z,
            Team = player?.Team,
            RoundNumber = _currentRoundNumber,
            EntityId = entityId,
            IsActive = eventType == "spawn" || eventType == "pickup",
            Description = $"{entityType} {entityName} {eventType}"
        };

        _entityLifecycles.Add(lifecycle);
    }

    private string GetEntityType(DemoFile.Game.Cs.CBaseEntity entity)
    {
        var entityName = entity.GetType().Name;
        
        if (entityName.Contains("Weapon") || entityName.Contains("Gun"))
            return "weapon";
        else if (entityName.Contains("Grenade"))
            return "grenade";
        else if (entityName.Contains("Bomb"))
            return "bomb";
        else if (entityName.Contains("Hostage"))
            return "hostage";
        else if (entityName.Contains("Item"))
            return "item";
        else
            return "entity";
    }

    private string GetEntityName(DemoFile.Game.Cs.CBaseEntity entity)
    {
        // Try to get designer name if it's a weapon
        if (entity is DemoFile.Game.Cs.CBasePlayerWeapon weapon)
            return weapon.DesignerName ?? entity.GetType().Name;
        
        return entity.GetType().Name;
    }

    private string GetGrenadeEffectType(string grenadeType)
    {
        return grenadeType switch
        {
            "hegrenade" => "damage",
            "flashbang" => "blind",
            "smokegrenade" => "obscure",
            "molotov" or "incgrenade" => "burn",
            "decoy" => "distract",
            _ => "unknown"
        };
    }

    private float GetGrenadeRadius(string grenadeType)
    {
        return grenadeType switch
        {
            "hegrenade" => 350f,
            "flashbang" => 400f,
            "smokegrenade" => 144f,
            "molotov" or "incgrenade" => 128f,
            "decoy" => 200f,
            _ => 100f
        };
    }

    private float GetGrenadeIntensity(string grenadeType)
    {
        return grenadeType switch
        {
            "hegrenade" => 100f,
            "flashbang" => 100f,
            "smokegrenade" => 100f,
            "molotov" or "incgrenade" => 100f,
            "decoy" => 50f,
            _ => 50f
        };
    }

    #endregion

    #region Game State Tracking

    private void OnRoundStartGameState(Source1RoundStartEvent e)
    {
        var demo = e.GameEventParser;
        if (demo == null || _currentDemoFile == null || _currentRound == null) return;

        // Track initial economy states
        AnalyzeAndTrackEconomyState(demo, "freeze_time");
        AnalyzeAndTrackTeamStates(demo);
        AnalyzeAndTrackMapControl(demo);
    }

    private void OnRoundEndGameState(Source1RoundEndEvent e)
    {
        var demo = e.GameEventParser;
        if (demo == null || _currentDemoFile == null || _currentRound == null) return;

        // Update team win/loss streaks
        var winnerTeam = e.Winner.ToString();
        var loserTeam = winnerTeam == "CT" ? "T" : "CT";

        _teamConsecutiveWins[winnerTeam]++;
        _teamConsecutiveLosses[winnerTeam] = 0;
        _teamConsecutiveLosses[loserTeam]++;
        _teamConsecutiveWins[loserTeam] = 0;

        // Final economy and team state analysis
        AnalyzeAndTrackEconomyState(demo, "round_end");
        AnalyzeAndTrackTeamStates(demo);
        AnalyzeAndTrackMapControl(demo);

        // Analyze tactical events that occurred during the round
        AnalyzeTacticalEvents(demo);
        
        // Calculate advanced statistics
        CalculateRoundImpactStats();
        TrackPerformanceMetrics();
    }

    private void AnalyzeAndTrackEconomyState(CsDemoParser demo, string phase)
    {
        foreach (var team in new[] { "CT", "T" })
        {
            var teamPlayers = demo.Players.Where(p => p.Team.ToString() == team && p.PlayerName != null).ToList();
            if (!teamPlayers.Any()) continue;

            var totalMoney = teamPlayers.Sum(p => p.MoneyServices?.Account ?? 0);
            var avgMoney = totalMoney / teamPlayers.Count;
            var maxMoney = teamPlayers.Max(p => p.MoneyServices?.Account ?? 0);
            var minMoney = teamPlayers.Min(p => p.MoneyServices?.Account ?? 0);

            // Calculate equipment values
            var totalWeaponValue = CalculateTeamWeaponValue(teamPlayers);
            var totalUtilityValue = CalculateTeamUtilityValue(teamPlayers);
            var totalArmorValue = CalculateTeamArmorValue(teamPlayers);

            // Determine round type
            var roundType = DetermineRoundType(teamPlayers, avgMoney, totalWeaponValue);
            var buyPercentage = totalMoney > 0 ? (float)(totalWeaponValue + totalUtilityValue + totalArmorValue) / totalMoney * 100 : 0;

            var economyState = new Models.EconomyState
            {
                DemoFileId = _currentDemoFile.Id,
                RoundId = _currentRound.Id,
                Tick = demo.CurrentDemoTick.Value,
                GameTime = (float)(demo.CurrentGameTime?.TotalSeconds ?? 0),
                Team = team,
                RoundNumber = _currentRoundNumber,
                Phase = phase,
                TotalMoney = totalMoney,
                AverageMoney = avgMoney,
                MaxMoney = maxMoney,
                MinMoney = minMoney,
                RoundType = roundType,
                BuyPercentage = buyPercentage,
                PlayersCanFullBuy = teamPlayers.Count(p => (p.MoneyServices?.Account ?? 0) >= 4000),
                PlayersOnEco = teamPlayers.Count(p => (p.MoneyServices?.Account ?? 0) < 2000),
                TotalWeaponValue = totalWeaponValue,
                TotalUtilityValue = totalUtilityValue,
                TotalArmorValue = totalArmorValue,
                ConsecutiveLosses = _teamConsecutiveLosses[team],
                ConsecutiveWins = _teamConsecutiveWins[team],
                LossBonus = CalculateLossBonus(_teamConsecutiveLosses[team]),
                CanFullBuyNextRound = PredictNextRoundMoney(teamPlayers, team) >= 4000 * teamPlayers.Count,
                Description = $"{team} {roundType} economy - ${avgMoney:F0} avg"
            };

            _economyStates.Add(economyState);
        }
    }

    private void AnalyzeAndTrackTeamStates(CsDemoParser demo)
    {
        foreach (var team in new[] { "CT", "T" })
        {
            var teamPlayers = demo.Players.Where(p => p.Team.ToString() == team && p.PlayerName != null && p.PlayerPawn != null).ToList();
            if (!teamPlayers.Any()) continue;

            var livingPlayers = teamPlayers.Count(p => (p.PlayerPawn?.Health ?? 0) > 0);
            var totalMoney = teamPlayers.Sum(p => p.MoneyServices?.Account ?? 0);
            var avgMoney = totalMoney / teamPlayers.Count;

            // Count equipment
            var weaponCounts = CountTeamWeapons(teamPlayers);
            var utilityCounts = CountTeamUtility(teamPlayers);

            // Calculate tactical metrics
            var teamSpread = CalculateTeamSpread(teamPlayers);
            var teamCohesion = CalculateTeamCohesion(teamPlayers);
            var siteControl = CalculateSiteControl(teamPlayers, team);

            // Determine tactical state
            var isStacked = DetermineIfStacked(teamPlayers);
            var isRotating = DetermineIfRotating(teamPlayers);
            var primaryArea = DeterminePrimaryArea(teamPlayers);

            var teamState = new Models.TeamState
            {
                DemoFileId = _currentDemoFile.Id,
                RoundId = _currentRound.Id,
                Tick = demo.CurrentDemoTick.Value,
                GameTime = (float)(demo.CurrentGameTime?.TotalSeconds ?? 0),
                Team = team,
                RoundNumber = _currentRoundNumber,
                TotalMoney = totalMoney,
                AverageMoney = avgMoney,
                LivingPlayers = livingPlayers,
                PlayersWithArmor = teamPlayers.Count(p => (p.PlayerPawn?.ArmorValue ?? 0) > 0),
                PlayersWithHelmet = teamPlayers.Count(p => p.PlayerPawn?.HasHelmet ?? false),
                PlayersWithDefuseKit = teamPlayers.Count(p => p.PlayerPawn?.HasDefuser ?? false),
                RifleCount = weaponCounts["rifles"],
                PistolCount = weaponCounts["pistols"],
                SniperCount = weaponCounts["snipers"],
                SMGCount = weaponCounts["smgs"],
                ShotgunCount = weaponCounts["shotguns"],
                HEGrenadeCount = utilityCounts["he"],
                FlashbangCount = utilityCounts["flash"],
                SmokegrenadeCount = utilityCounts["smoke"],
                MolotovCount = utilityCounts["molotov"],
                DecoyCount = utilityCounts["decoy"],
                PrimaryArea = primaryArea,
                TeamSpread = teamSpread,
                IsStacked = isStacked,
                IsRotating = isRotating,
                TeamCohesion = teamCohesion,
                SiteControl = siteControl,
                IsSaveRound = DetermineIfSaveRound(teamPlayers, avgMoney),
                IsForceRound = DetermineIfForceRound(teamPlayers, avgMoney),
                IsEcoRound = DetermineIfEcoRound(teamPlayers, avgMoney),
                IsFullBuyRound = DetermineIfFullBuyRound(teamPlayers, avgMoney),
                Description = $"{team} team state - {livingPlayers} alive, {primaryArea} focus"
            };

            _teamStates.Add(teamState);
        }
    }

    private void AnalyzeAndTrackMapControl(CsDemoParser demo)
    {
        if (_currentDemoFile == null || _currentRound == null) return;

        var ctPlayers = demo.Players.Where(p => p.Team.ToString() == "CT" && p.PlayerPawn != null && (p.PlayerPawn.Health ?? 0) > 0).ToList();
        var tPlayers = demo.Players.Where(p => p.Team.ToString() == "T" && p.PlayerPawn != null && (p.PlayerPawn.Health ?? 0) > 0).ToList();

        // Calculate area control based on player positions
        var areaControl = CalculateAreaControl(ctPlayers, tPlayers, demo.Map);
        var siteControl = CalculateSpecificSiteControl(ctPlayers, tPlayers, demo.Map);

        var mapControl = new Models.MapControl
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound.Id,
            Tick = demo.CurrentDemoTick.Value,
            GameTime = (float)(demo.CurrentGameTime?.TotalSeconds ?? 0),
            RoundNumber = _currentRoundNumber,
            MapName = demo.Map,
            CTAreaControl = areaControl["CT"],
            TAreaControl = areaControl["T"],
            NeutralAreaControl = areaControl["Neutral"],
            ASiteControl = siteControl["A"],
            BSiteControl = siteControl["B"],
            MidControl = siteControl["Mid"],
            CTPlayersInAsite = CountPlayersInArea(ctPlayers, "A", demo.Map),
            TPlayersInAsite = CountPlayersInArea(tPlayers, "A", demo.Map),
            CTPlayersInBsite = CountPlayersInArea(ctPlayers, "B", demo.Map),
            TPlayersInBsite = CountPlayersInArea(tPlayers, "B", demo.Map),
            CTPlayersInMid = CountPlayersInArea(ctPlayers, "Mid", demo.Map),
            TPlayersInMid = CountPlayersInArea(tPlayers, "Mid", demo.Map),
            CTStackedOneSite = CheckIfStackedOneSite(ctPlayers, demo.Map),
            TCommittedToSite = CheckIfCommittedToSite(tPlayers, demo.Map),
            DominantTeamZone = DetermineDominantTeam(areaControl),
            TerritoryBalance = CalculateTerritoryBalance(areaControl),
            Description = $"Map control - CT: {areaControl["CT"]:F1}%, T: {areaControl["T"]:F1}%"
        };

        _mapControls.Add(mapControl);
    }

    private void AnalyzeTacticalEvents(CsDemoParser demo)
    {
        // This would analyze the round for tactical events like executes, retakes, etc.
        // For now, implementing basic event detection based on player movements and utility usage
        
        // Detect utility-based tactics (smokes + flashes = execute)
        var recentGrenades = _grenades.Where(g => g.RoundId == _currentRound?.Id)
                                     .Where(g => g.DetonateTick > (demo.CurrentDemoTick.Value - 320)) // Last 10 seconds
                                     .GroupBy(g => g.Team)
                                     .ToList();

        foreach (var teamGrenades in recentGrenades)
        {
            var smokes = teamGrenades.Count(g => g.GrenadeType == "smokegrenade");
            var flashes = teamGrenades.Count(g => g.GrenadeType == "flashbang");
            var hes = teamGrenades.Count(g => g.GrenadeType == "hegrenade");
            var molotovs = teamGrenades.Count(g => g.GrenadeType == "molotov" || g.GrenadeType == "incgrenade");

            // Detect coordinated execute
            if (smokes >= 2 && flashes >= 1)
            {
                var tacticalEvent = new Models.TacticalEvent
                {
                    DemoFileId = _currentDemoFile.Id,
                    RoundId = _currentRound.Id,
                    Tick = demo.CurrentDemoTick.Value,
                    GameTime = (float)(demo.CurrentGameTime?.TotalSeconds ?? 0),
                    EventType = "execute",
                    Team = teamGrenades.Key,
                    RoundNumber = _currentRoundNumber,
                    PlayersInvolved = teamGrenades.Select(g => g.PlayerId).Distinct().Count(),
                    SmokesUsed = smokes,
                    FlashesUsed = flashes,
                    HEGrenadesUsed = hes,
                    MolotovsUsed = molotovs,
                    Coordination = CalculateUtilityCoordination(teamGrenades.ToList()),
                    ExecutionQuality = 75f, // Base quality, would be refined
                    Description = $"{teamGrenades.Key} coordinated execute with {smokes} smokes, {flashes} flashes"
                };

                _tacticalEvents.Add(tacticalEvent);
            }
        }
    }

    // Helper methods for game state analysis
    private Dictionary<string, int> CountTeamWeapons(List<DemoFile.Game.Cs.CCSPlayerController> players)
    {
        var counts = new Dictionary<string, int>
        {
            ["rifles"] = 0, ["pistols"] = 0, ["snipers"] = 0, ["smgs"] = 0, ["shotguns"] = 0
        };

        foreach (var player in players)
        {
            var weapon = player.PlayerPawn?.WeaponServices?.ActiveWeapon?.Value?.DesignerName ?? "";
            if (weapon.Contains("ak47") || weapon.Contains("m4a") || weapon.Contains("galil") || weapon.Contains("famas"))
                counts["rifles"]++;
            else if (weapon.Contains("awp") || weapon.Contains("ssg08"))
                counts["snipers"]++;
            else if (weapon.Contains("mp") || weapon.Contains("p90") || weapon.Contains("ump"))
                counts["smgs"]++;
            else if (weapon.Contains("nova") || weapon.Contains("xm1014") || weapon.Contains("mag7"))
                counts["shotguns"]++;
            else if (weapon.Contains("glock") || weapon.Contains("usp") || weapon.Contains("p250") || weapon.Contains("deagle"))
                counts["pistols"]++;
        }

        return counts;
    }

    private Dictionary<string, int> CountTeamUtility(List<DemoFile.Game.Cs.CCSPlayerController> players)
    {
        // This would require parsing inventory/equipment data
        // For now returning placeholder values
        return new Dictionary<string, int>
        {
            ["he"] = 0, ["flash"] = 0, ["smoke"] = 0, ["molotov"] = 0, ["decoy"] = 0
        };
    }

    private int CalculateTeamWeaponValue(List<DemoFile.Game.Cs.CCSPlayerController> players) => 0; // Placeholder
    private int CalculateTeamUtilityValue(List<DemoFile.Game.Cs.CCSPlayerController> players) => 0; // Placeholder
    private int CalculateTeamArmorValue(List<DemoFile.Game.Cs.CCSPlayerController> players) => 0; // Placeholder
    private string DetermineRoundType(List<DemoFile.Game.Cs.CCSPlayerController> players, int avgMoney, int weaponValue)
    {
        if (avgMoney < 2000) return "eco";
        if (avgMoney < 3500) return "force";
        return "full_buy";
    }
    private int CalculateLossBonus(int consecutiveLosses) => Math.Min(consecutiveLosses * 500 + 1400, 3400);
    private int PredictNextRoundMoney(List<DemoFile.Game.Cs.CCSPlayerController> players, string team) => 16000; // Placeholder
    private float CalculateTeamSpread(List<DemoFile.Game.Cs.CCSPlayerController> players) => 0f; // Placeholder
    private float CalculateTeamCohesion(List<DemoFile.Game.Cs.CCSPlayerController> players) => 0f; // Placeholder
    private float CalculateSiteControl(List<DemoFile.Game.Cs.CCSPlayerController> players, string team) => 0f; // Placeholder
    private bool DetermineIfStacked(List<DemoFile.Game.Cs.CCSPlayerController> players) => false; // Placeholder
    private bool DetermineIfRotating(List<DemoFile.Game.Cs.CCSPlayerController> players) => false; // Placeholder
    private string DeterminePrimaryArea(List<DemoFile.Game.Cs.CCSPlayerController> players) => "unknown"; // Placeholder
    private bool DetermineIfSaveRound(List<DemoFile.Game.Cs.CCSPlayerController> players, int avgMoney) => avgMoney < 1500;
    private bool DetermineIfForceRound(List<DemoFile.Game.Cs.CCSPlayerController> players, int avgMoney) => avgMoney >= 1500 && avgMoney < 3500;
    private bool DetermineIfEcoRound(List<DemoFile.Game.Cs.CCSPlayerController> players, int avgMoney) => avgMoney < 2000;
    private bool DetermineIfFullBuyRound(List<DemoFile.Game.Cs.CCSPlayerController> players, int avgMoney) => avgMoney >= 4000;
    private Dictionary<string, float> CalculateAreaControl(List<DemoFile.Game.Cs.CCSPlayerController> ct, List<DemoFile.Game.Cs.CCSPlayerController> t, string map)
    {
        return new() { ["CT"] = 40f, ["T"] = 40f, ["Neutral"] = 20f }; // Placeholder
    }
    private Dictionary<string, float> CalculateSpecificSiteControl(List<DemoFile.Game.Cs.CCSPlayerController> ct, List<DemoFile.Game.Cs.CCSPlayerController> t, string map)
    {
        return new() { ["A"] = 0f, ["B"] = 0f, ["Mid"] = 0f }; // Placeholder
    }
    private int CountPlayersInArea(List<DemoFile.Game.Cs.CCSPlayerController> players, string area, string map) => 0; // Placeholder
    private bool CheckIfStackedOneSite(List<DemoFile.Game.Cs.CCSPlayerController> players, string map) => false; // Placeholder
    private bool CheckIfCommittedToSite(List<DemoFile.Game.Cs.CCSPlayerController> players, string map) => false; // Placeholder
    private string DetermineDominantTeam(Dictionary<string, float> areaControl) => areaControl["CT"] > areaControl["T"] ? "CT" : "T";
    private float CalculateTerritoryBalance(Dictionary<string, float> areaControl) => Math.Abs(areaControl["CT"] - areaControl["T"]);
    private float CalculateUtilityCoordination(List<Models.Grenade> grenades) => 75f; // Placeholder

    #endregion

    #region Advanced Statistics Calculations

    private void CalculateRoundImpactStats()
    {
        if (_currentRound == null || _currentDemoFile == null) return;

        foreach (var player in _players.Values)
        {
            var roundImpact = CalculatePlayerRoundImpact(player);
            _roundImpacts.Add(roundImpact);
        }
    }

    private Models.RoundImpact CalculatePlayerRoundImpact(Models.Player player)
    {
        // Get player's actions in this round
        var kills = _kills.Where(k => k.KillerId == player.Id && k.RoundId == _currentRound!.Id).ToList();
        var deaths = _kills.Where(k => k.VictimId == player.Id && k.RoundId == _currentRound!.Id).ToList();
        var damage = _damages.Where(d => d.AttackerId == player.Id && d.RoundId == _currentRound!.Id).ToList();

        // Calculate impact scores
        var fraggingImpact = CalculateFraggingImpact(kills, deaths, damage);
        var utilityImpact = CalculateUtilityImpact(player);
        var positionalImpact = CalculatePositionalImpact(player);
        var economicImpact = CalculateEconomicImpact(player);

        var overallImpact = (fraggingImpact + utilityImpact + positionalImpact) * 0.8f + economicImpact * 0.2f;

        return new Models.RoundImpact
        {
            DemoFileId = _currentDemoFile!.Id,
            PlayerId = player.Id,
            RoundId = _currentRound!.Id,
            RoundNumber = _currentRoundNumber,
            OverallImpact = overallImpact,
            FraggingImpact = fraggingImpact,
            UtilityImpact = utilityImpact,
            PositionalImpact = positionalImpact,
            EconomicImpact = economicImpact,
            HasEntryFrag = kills.Any(k => k.IsFirstKill),
            HasMultiKill = kills.Count >= 2,
            HasClutchAttempt = DetermineClutchAttempt(player),
            HasClutchWin = DetermineClutchWin(player),
            RoundType = DeterminePlayerRoundType(player),
            DecisionQuality = CalculateDecisionQuality(player),
            ImpactSummary = GenerateImpactSummary(overallImpact, kills.Count, deaths.Count, damage.Sum(d => d.DamageAmount))
        };
    }

    private void CalculateAdvancedPlayerStats(string statsType)
    {
        foreach (var player in _players.Values)
        {
            var stats = CalculatePlayerAdvancedStats(player, statsType);
            _advancedPlayerStats.Add(stats);
        }
    }

    private Models.AdvancedPlayerStats CalculatePlayerAdvancedStats(Models.Player player, string statsType)
    {
        // Get all player data for calculations
        var playerKills = _kills.Where(k => k.KillerId == player.Id).ToList();
        var playerDeaths = _kills.Where(k => k.VictimId == player.Id).ToList();
        var playerDamage = _damages.Where(d => d.AttackerId == player.Id).ToList();
        var playerAssists = _kills.Where(k => k.AssisterId == player.Id).ToList();

        var totalRounds = _currentRoundNumber;
        if (totalRounds == 0) totalRounds = 1; // Avoid division by zero

        // Core metrics
        var kills = playerKills.Count;
        var deaths = playerDeaths.Count;
        var assists = playerAssists.Count;
        var totalDamage = playerDamage.Sum(d => d.DamageAmount);

        // Calculate HLTV Rating 2.0
        var hltv2Rating = CalculateHLTV2Rating(kills, deaths, assists, totalDamage, totalRounds);
        var hltv1Rating = CalculateHLTV1Rating(kills, deaths, totalRounds);

        // Calculate KAST
        var kastPercentage = CalculateKASTPercentage(player, totalRounds);

        // Calculate other advanced metrics
        var clutchStats = CalculateClutchStats(player);
        var multiKillStats = CalculateMultiKillStats(playerKills);
        var weaponEfficiency = CalculateWeaponEfficiency(player);

        return new Models.AdvancedPlayerStats
        {
            DemoFileId = _currentDemoFile!.Id,
            PlayerId = player.Id,
            MatchId = _currentMatch?.Id,
            RoundId = statsType == "round" ? _currentRound?.Id : null,
            RoundNumber = _currentRoundNumber,
            StatsType = statsType,
            HLTVRating = hltv2Rating,
            HLTVRating1 = hltv1Rating,
            KASTPercentage = kastPercentage,
            KillsPerRound = (float)kills / totalRounds,
            DeathsPerRound = (float)deaths / totalRounds,
            AssistsPerRound = (float)assists / totalRounds,
            KillDeathRatio = deaths > 0 ? (float)kills / deaths : kills,
            AverageDamagePerRound = (float)totalDamage / totalRounds,
            DamagePerRound = (float)totalDamage / totalRounds,
            FirstKillsPerRound = (float)playerKills.Count(k => k.IsFirstKill) / totalRounds,
            FirstDeathsPerRound = (float)playerDeaths.Count(d => d.IsFirstKill) / totalRounds,
            HeadshotPercentage = kills > 0 ? (float)playerKills.Count(k => k.IsHeadshot) / kills * 100 : 0,
            SurvivalRate = (float)(totalRounds - deaths) / totalRounds * 100,
            Clutch1v1Attempts = clutchStats["1v1Attempts"],
            Clutch1v1Wins = clutchStats["1v1Wins"],
            Clutch1v2Attempts = clutchStats["1v2Attempts"],
            Clutch1v2Wins = clutchStats["1v2Wins"],
            Clutch1v3Attempts = clutchStats["1v3Attempts"],
            Clutch1v3Wins = clutchStats["1v3Wins"],
            OverallClutchSuccessRate = CalculateOverallClutchRate(clutchStats),
            DoubleKills = multiKillStats["doubles"],
            TripleKills = multiKillStats["triples"],
            QuadKills = multiKillStats["quads"],
            PentaKills = multiKillStats["pentas"],
            AwpKillsPerRound = (float)playerKills.Count(k => k.Weapon?.Contains("awp") ?? false) / totalRounds,
            SampleSize = totalRounds,
            Notes = $"Calculated {statsType} stats for {totalRounds} rounds"
        };
    }

    private void TrackPerformanceMetrics()
    {
        if (_demo == null || _currentDemoFile == null) return;

        foreach (var player in _demo.Players.Where(p => p.PlayerName != null))
        {
            var playerModel = GetOrCreatePlayer(player);
            
            // Track aim accuracy metric
            var aimAccuracy = CalculateAimAccuracy(player);
            if (aimAccuracy >= 0)
            {
                _performanceMetrics.Add(new Models.PerformanceMetric
                {
                    DemoFileId = _currentDemoFile.Id,
                    PlayerId = playerModel.Id,
                    RoundId = _currentRound?.Id,
                    Tick = _demo.CurrentDemoTick.Value,
                    GameTime = (float)(_demo.CurrentGameTime?.TotalSeconds ?? 0),
                    MetricType = "aim_accuracy",
                    MetricName = "shot_accuracy",
                    Value = aimAccuracy,
                    NormalizedValue = Math.Min(aimAccuracy * 100, 100),
                    Confidence = 75f,
                    RoundNumber = _currentRoundNumber,
                    Description = $"Shot accuracy: {aimAccuracy:P2}"
                });
            }

            // Track movement efficiency
            var movementEfficiency = CalculateMovementEfficiency(player);
            if (movementEfficiency >= 0)
            {
                _performanceMetrics.Add(new Models.PerformanceMetric
                {
                    DemoFileId = _currentDemoFile.Id,
                    PlayerId = playerModel.Id,
                    RoundId = _currentRound?.Id,
                    Tick = _demo.CurrentDemoTick.Value,
                    GameTime = (float)(_demo.CurrentGameTime?.TotalSeconds ?? 0),
                    MetricType = "movement_efficiency",
                    MetricName = "optimal_pathing",
                    Value = movementEfficiency,
                    NormalizedValue = movementEfficiency * 100,
                    Confidence = 60f,
                    RoundNumber = _currentRoundNumber,
                    Description = $"Movement efficiency: {movementEfficiency:P2}"
                });
            }

            // Track positioning quality
            var positioningQuality = CalculatePositioningQuality(player);
            if (positioningQuality >= 0)
            {
                _performanceMetrics.Add(new Models.PerformanceMetric
                {
                    DemoFileId = _currentDemoFile.Id,
                    PlayerId = playerModel.Id,
                    RoundId = _currentRound?.Id,
                    Tick = _demo.CurrentDemoTick.Value,
                    GameTime = (float)(_demo.CurrentGameTime?.TotalSeconds ?? 0),
                    MetricType = "positioning_quality",
                    MetricName = "tactical_positioning",
                    Value = positioningQuality,
                    NormalizedValue = positioningQuality * 100,
                    Confidence = 50f,
                    RoundNumber = _currentRoundNumber,
                    Description = $"Positioning quality: {positioningQuality:P2}"
                });
            }
        }
    }

    // HLTV Rating calculations
    private float CalculateHLTV2Rating(int kills, int deaths, int assists, int damage, int rounds)
    {
        if (rounds == 0) return 0f;

        var kpr = (float)kills / rounds;
        var spr = Math.Max(0, (float)(rounds - deaths) / rounds);
        var rmk = (float)assists / rounds;

        // Simplified HLTV 2.0 formula
        var rating = (kpr + 0.3f * rmk + spr - 0.5f) / 2.3f;
        
        // Apply damage factor
        var adr = (float)damage / rounds;
        var damageRating = adr / 76f; // 76 ADR = 1.0 rating contribution
        
        return Math.Max(0, (rating + damageRating) / 2f);
    }

    private float CalculateHLTV1Rating(int kills, int deaths, int rounds)
    {
        if (rounds == 0) return 0f;
        
        var killRatio = deaths > 0 ? (float)kills / deaths : kills;
        var killsPerRound = (float)kills / rounds;
        
        return (killRatio + killsPerRound) / 2f;
    }

    private float CalculateKASTPercentage(Models.Player player, int totalRounds)
    {
        // KAST = rounds with Kill, Assist, Survive, or Trade
        // For now, simplified version focusing on kills, assists, and survival
        var kills = _kills.Count(k => k.KillerId == player.Id);
        var assists = _kills.Count(k => k.AssisterId == player.Id);
        var deaths = _kills.Count(k => k.VictimId == player.Id);
        var survivedRounds = totalRounds - deaths;
        
        var kastRounds = Math.Min(totalRounds, kills + assists + survivedRounds);
        return totalRounds > 0 ? (float)kastRounds / totalRounds * 100 : 0f;
    }

    // Helper calculation methods (simplified implementations)
    private float CalculateFraggingImpact(List<Models.Kill> kills, List<Models.Kill> deaths, List<Models.Damage> damage)
    {
        var killImpact = kills.Count * 20f;
        var deathImpact = deaths.Count * -15f;
        var damageImpact = damage.Sum(d => d.DamageAmount) * 0.1f;
        var headshotBonus = kills.Count(k => k.IsHeadshot) * 5f;
        var firstKillBonus = kills.Count(k => k.IsFirstKill) * 10f;
        
        return Math.Max(-50, Math.Min(50, killImpact + deathImpact + damageImpact + headshotBonus + firstKillBonus));
    }

    private float CalculateUtilityImpact(Models.Player player) => 0f; // Placeholder
    private float CalculatePositionalImpact(Models.Player player) => 0f; // Placeholder
    private float CalculateEconomicImpact(Models.Player player) => 0f; // Placeholder
    private bool DetermineClutchAttempt(Models.Player player) => false; // Placeholder
    private bool DetermineClutchWin(Models.Player player) => false; // Placeholder
    private string DeterminePlayerRoundType(Models.Player player) => "standard"; // Placeholder
    private float CalculateDecisionQuality(Models.Player player) => 75f; // Placeholder
    private string GenerateImpactSummary(float impact, int kills, int deaths, int damage)
    {
        var impactLevel = impact switch
        {
            > 30 => "Excellent",
            > 15 => "Good",
            > 0 => "Positive",
            > -15 => "Neutral",
            _ => "Negative"
        };
        return $"{impactLevel} impact: {kills}K/{deaths}D, {damage} damage";
    }

    private Dictionary<string, int> CalculateClutchStats(Models.Player player)
    {
        return new Dictionary<string, int>
        {
            ["1v1Attempts"] = 0, ["1v1Wins"] = 0,
            ["1v2Attempts"] = 0, ["1v2Wins"] = 0,
            ["1v3Attempts"] = 0, ["1v3Wins"] = 0
        };
    }

    private Dictionary<string, int> CalculateMultiKillStats(List<Models.Kill> kills)
    {
        return new Dictionary<string, int>
        {
            ["doubles"] = 0, ["triples"] = 0, ["quads"] = 0, ["pentas"] = 0
        };
    }

    private float CalculateWeaponEfficiency(Models.Player player) => 0.75f; // Placeholder
    private float CalculateOverallClutchRate(Dictionary<string, int> clutchStats) => 0f; // Placeholder
    private float CalculateAimAccuracy(DemoFile.Game.Cs.CCSPlayerController player) => -1f; // Placeholder
    private float CalculateMovementEfficiency(DemoFile.Game.Cs.CCSPlayerController player) => -1f; // Placeholder
    private float CalculatePositioningQuality(DemoFile.Game.Cs.CCSPlayerController player) => -1f; // Placeholder

    #endregion

    #region Temporary Entity Events Processing

    private void OnEffectDispatch(CMsgTEEffectDispatch e)
    {
        if (_demo == null || _currentDemoFile == null) return;

        var temporaryEntity = new Models.TemporaryEntity
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound?.Id,
            Tick = _demo.CurrentDemoTick.Value,
            GameTime = (float)(_demo.CurrentGameTime?.TotalSeconds ?? 0),
            EntityType = "effect_dispatch",
            SubType = e.EffectName,
            PositionX = (decimal)e.Origin.X,
            PositionY = (decimal)e.Origin.Y,
            PositionZ = (decimal)e.Origin.Z,
            DirectionX = e.Normal.HasValue ? (decimal)e.Normal.Value.X : null,
            DirectionY = e.Normal.HasValue ? (decimal)e.Normal.Value.Y : null,
            DirectionZ = e.Normal.HasValue ? (decimal)e.Normal.Value.Z : null,
            Intensity = e.Magnitude,
            Scale = e.Scale,
            Team = GetPlayerTeamFromEntityId(e.EntityIndex) ?? "Unknown",
            RoundNumber = _currentRoundNumber,
            Description = $"Effect dispatch: {e.EffectName}"
        };

        _temporaryEntities.Add(temporaryEntity);
    }

    private void OnMuzzleFlash(CMsgTEMuzzleFlash e)
    {
        if (_demo == null || _currentDemoFile == null) return;

        var player = _demo.Players.FirstOrDefault(p => p.EntityIndex == e.EntityIndex);
        var playerModel = player != null ? GetOrCreatePlayer(player) : null;

        var temporaryEntity = new Models.TemporaryEntity
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound?.Id,
            PlayerId = playerModel?.Id,
            Tick = _demo.CurrentDemoTick.Value,
            GameTime = (float)(_demo.CurrentGameTime?.TotalSeconds ?? 0),
            EntityType = "muzzle_flash",
            PositionX = (decimal)e.Origin.X,
            PositionY = (decimal)e.Origin.Y,
            PositionZ = (decimal)e.Origin.Z,
            DirectionX = (decimal)e.Angles.X,
            DirectionY = (decimal)e.Angles.Y,
            DirectionZ = (decimal)e.Angles.Z,
            Scale = e.Scale,
            WeaponName = player?.PlayerPawn?.WeaponServices?.ActiveWeapon?.Value?.DesignerName,
            Team = player?.Team.ToString() ?? "Unknown",
            RoundNumber = _currentRoundNumber,
            Description = $"Muzzle flash from {player?.PlayerName ?? "Unknown"}"
        };

        _temporaryEntities.Add(temporaryEntity);
    }

    private void OnImpact(CMsgTEImpact e)
    {
        if (_demo == null || _currentDemoFile == null) return;

        var temporaryEntity = new Models.TemporaryEntity
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound?.Id,
            Tick = _demo.CurrentDemoTick.Value,
            GameTime = (float)(_demo.CurrentGameTime?.TotalSeconds ?? 0),
            EntityType = "impact",
            SubType = "bullet_impact",
            PositionX = (decimal)e.Origin.X,
            PositionY = (decimal)e.Origin.Y,
            PositionZ = (decimal)e.Origin.Z,
            DirectionX = (decimal)e.Normal.X,
            DirectionY = (decimal)e.Normal.Y,
            DirectionZ = (decimal)e.Normal.Z,
            Material = GetSurfaceMaterial(e.SurfaceProp),
            Team = "Unknown", // Impact events don't directly link to players
            RoundNumber = _currentRoundNumber,
            Description = $"Bullet impact on {GetSurfaceMaterial(e.SurfaceProp)}"
        };

        _temporaryEntities.Add(temporaryEntity);
    }

    private void OnExplosion(CMsgTEExplosion e)
    {
        if (_demo == null || _currentDemoFile == null) return;

        var temporaryEntity = new Models.TemporaryEntity
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound?.Id,
            Tick = _demo.CurrentDemoTick.Value,
            GameTime = (float)(_demo.CurrentGameTime?.TotalSeconds ?? 0),
            EntityType = "explosion",
            SubType = "grenade_explosion",
            PositionX = (decimal)e.Origin.X,
            PositionY = (decimal)e.Origin.Y,
            PositionZ = (decimal)e.Origin.Z,
            Scale = e.Scale,
            Intensity = e.Magnitude,
            ExplosionRadius = e.Radius,
            Team = "Unknown", // Will be determined by context
            RoundNumber = _currentRoundNumber,
            Description = $"Explosion at {e.Origin.X:F1}, {e.Origin.Y:F1}, {e.Origin.Z:F1}"
        };

        _temporaryEntities.Add(temporaryEntity);
    }

    private void OnArmorRicochet(CMsgTEArmorRicochet e)
    {
        if (_demo == null || _currentDemoFile == null) return;

        var temporaryEntity = new Models.TemporaryEntity
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound?.Id,
            Tick = _demo.CurrentDemoTick.Value,
            GameTime = (float)(_demo.CurrentGameTime?.TotalSeconds ?? 0),
            EntityType = "armor_ricochet",
            PositionX = (decimal)e.Pos.X,
            PositionY = (decimal)e.Pos.Y,
            PositionZ = (decimal)e.Pos.Z,
            DirectionX = (decimal)e.Dir.X,
            DirectionY = (decimal)e.Dir.Y,
            DirectionZ = (decimal)e.Dir.Z,
            Team = "Unknown",
            RoundNumber = _currentRoundNumber,
            Description = "Armor ricochet effect"
        };

        _temporaryEntities.Add(temporaryEntity);
    }

    private void OnDust(CMsgTEDust e)
    {
        CreateSimpleTemporaryEntity("dust", e.Pos, e.Dir, null);
    }

    private void OnSmoke(CMsgTESmoke e)
    {
        CreateSimpleTemporaryEntity("smoke", e.Pos, null, e.Scale);
    }

    private void OnSparks(CMsgTESparks e)
    {
        CreateSimpleTemporaryEntity("sparks", e.Pos, e.Dir, e.Magnitude);
    }

    private void OnBeamEntPoint(CMsgTEBeamEntPoint e)
    {
        if (_demo == null || _currentDemoFile == null) return;

        var temporaryEntity = new Models.TemporaryEntity
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound?.Id,
            Tick = _demo.CurrentDemoTick.Value,
            GameTime = (float)(_demo.CurrentGameTime?.TotalSeconds ?? 0),
            EntityType = "beam",
            SubType = "beam_ent_point",
            PositionX = (decimal)e.StartPoint.X,
            PositionY = (decimal)e.StartPoint.Y,
            PositionZ = (decimal)e.StartPoint.Z,
            EndPositionX = (decimal)e.EndPoint.X,
            EndPositionY = (decimal)e.EndPoint.Y,
            EndPositionZ = (decimal)e.EndPoint.Z,
            Duration = e.Life,
            Alpha = e.Amplitude / 255f,
            Color = $"{e.Red},{e.Green},{e.Blue}",
            Team = "Unknown",
            RoundNumber = _currentRoundNumber,
            Description = $"Beam from entity to point"
        };

        _temporaryEntities.Add(temporaryEntity);
    }

    private void OnBeamEnts(CMsgTEBeamEnts e)
    {
        CreateBeamEntity("beam_ents", e.StartPoint, e.EndPoint, e.Life, e.Amplitude, e.Red, e.Green, e.Blue);
    }

    private void OnBeamPoints(CMsgTEBeamPoints e)
    {
        CreateBeamEntity("beam_points", e.StartPoint, e.EndPoint, e.Life, e.Amplitude, e.Red, e.Green, e.Blue);
    }

    private void OnBeamRing(CMsgTEBeamRing e)
    {
        CreateBeamEntity("beam_ring", e.Center, e.Center, e.Life, e.Amplitude, e.Red, e.Green, e.Blue);
    }

    private void OnBSPDecal(CMsgTEBSPDecal e)
    {
        CreateDecalEntity("bsp_decal", e.Pos, e.Entity);
    }

    private void OnDecal(CMsgTEDecal e)
    {
        CreateDecalEntity("decal", e.Pos, e.Entity);
    }

    private void OnWorldDecal(CMsgTEWorldDecal e)
    {
        CreateDecalEntity("world_decal", e.Pos, null);
    }

    private void OnPlayerDecal(CMsgTEPlayerDecal e)
    {
        CreateDecalEntity("player_decal", e.Pos, null);
    }

    // Helper methods for temporary entities
    private void CreateSimpleTemporaryEntity(string entityType, Vector pos, Vector? dir, float? scale)
    {
        if (_demo == null || _currentDemoFile == null) return;

        var temporaryEntity = new Models.TemporaryEntity
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound?.Id,
            Tick = _demo.CurrentDemoTick.Value,
            GameTime = (float)(_demo.CurrentGameTime?.TotalSeconds ?? 0),
            EntityType = entityType,
            PositionX = (decimal)pos.X,
            PositionY = (decimal)pos.Y,
            PositionZ = (decimal)pos.Z,
            DirectionX = dir.HasValue ? (decimal)dir.Value.X : null,
            DirectionY = dir.HasValue ? (decimal)dir.Value.Y : null,
            DirectionZ = dir.HasValue ? (decimal)dir.Value.Z : null,
            Scale = scale,
            Team = "Unknown",
            RoundNumber = _currentRoundNumber,
            Description = $"{entityType.Replace('_', ' ')} effect"
        };

        _temporaryEntities.Add(temporaryEntity);
    }

    private void CreateBeamEntity(string subType, Vector startPoint, Vector endPoint, float life, int amplitude, int red, int green, int blue)
    {
        if (_demo == null || _currentDemoFile == null) return;

        var temporaryEntity = new Models.TemporaryEntity
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound?.Id,
            Tick = _demo.CurrentDemoTick.Value,
            GameTime = (float)(_demo.CurrentGameTime?.TotalSeconds ?? 0),
            EntityType = "beam",
            SubType = subType,
            PositionX = (decimal)startPoint.X,
            PositionY = (decimal)startPoint.Y,
            PositionZ = (decimal)startPoint.Z,
            EndPositionX = (decimal)endPoint.X,
            EndPositionY = (decimal)endPoint.Y,
            EndPositionZ = (decimal)endPoint.Z,
            Duration = life,
            Alpha = amplitude / 255f,
            Color = $"{red},{green},{blue}",
            Team = "Unknown",
            RoundNumber = _currentRoundNumber,
            Description = $"Beam effect: {subType.Replace('_', ' ')}"
        };

        _temporaryEntities.Add(temporaryEntity);
    }

    private void CreateDecalEntity(string subType, Vector pos, int? entityIndex)
    {
        if (_demo == null || _currentDemoFile == null) return;

        var temporaryEntity = new Models.TemporaryEntity
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound?.Id,
            Tick = _demo.CurrentDemoTick.Value,
            GameTime = (float)(_demo.CurrentGameTime?.TotalSeconds ?? 0),
            EntityType = "decal",
            SubType = subType,
            PositionX = (decimal)pos.X,
            PositionY = (decimal)pos.Y,
            PositionZ = (decimal)pos.Z,
            TargetEntityId = entityIndex,
            Team = "Unknown",
            RoundNumber = _currentRoundNumber,
            Description = $"Decal: {subType.Replace('_', ' ')}"
        };

        _temporaryEntities.Add(temporaryEntity);
    }

    private string? GetPlayerTeamFromEntityId(int entityIndex)
    {
        var player = _demo?.Players.FirstOrDefault(p => p.EntityIndex == entityIndex);
        return player?.Team.ToString();
    }

    private string GetSurfaceMaterial(string surfaceProp)
    {
        // Map surface properties to readable materials
        return surfaceProp.ToLower() switch
        {
            "concrete" or "concrete_block" => "concrete",
            "metal" or "metalbox" => "metal",
            "wood" or "plywood" => "wood",
            "dirt" or "grass" => "dirt",
            "sand" => "sand",
            "water" => "water",
            "glass" => "glass",
            "flesh" => "flesh",
            _ => surfaceProp
        };
    }

    #endregion

    #region Voice and Communication Processing

    private void OnVoiceData(CSVCMsg_VoiceData e)
    {
        if (_demo == null || _currentDemoFile == null) return;

        // Find player by client index or Steam ID
        var player = FindPlayerByClientIndex(e.Client) ?? FindPlayerBySteamId(e.Xuid);
        if (player == null) return;

        // Store voice audio data for potential extraction
        if (e.Audio != null && e.Xuid > 0)
        {
            _voiceDataPerSteamId.TryGetValue(e.Xuid, out var voiceData);
            voiceData ??= new();
            voiceData.Add(e.Audio);
            _voiceDataPerSteamId[e.Xuid] = voiceData;
        }

        var playerModel = GetOrCreatePlayer(player);
        var currentTick = _demo.CurrentDemoTick.Value;

        // Check if this is a continuation of existing voice communication
        if (_activeVoiceComms.TryGetValue(playerModel.Id, out var activeComm))
        {
            // Update existing communication
            activeComm.EndTick = currentTick;
            activeComm.EndTime = (float)(_demo.CurrentGameTime?.TotalSeconds ?? 0);
            activeComm.Duration = activeComm.EndTime - activeComm.StartTime;
            
            // Update voice intensity based on voice level
            if (e.Audio?.VoiceLevel != null)
            {
                activeComm.VoiceIntensity = Math.Max(activeComm.VoiceIntensity, e.Audio.VoiceLevel * 100);
            }
        }
        else
        {
            // Start new voice communication
            var voiceComm = new Models.VoiceCommunication
            {
                DemoFileId = _currentDemoFile.Id,
                RoundId = _currentRound?.Id,
                SpeakerId = playerModel.Id,
                StartTick = currentTick,
                StartTime = (float)(_demo.CurrentGameTime?.TotalSeconds ?? 0),
                CommunicationType = "voice",
                VoiceIntensity = e.Audio?.VoiceLevel * 100 ?? 50f,
                ToTeam = !e.Proximity, // Proximity voice is not team-only
                SpeakerPositionX = (decimal)(player.PlayerPawn?.CBodyComponent?.SceneNode?.AbsOrigin?.X ?? 0),
                SpeakerPositionY = (decimal)(player.PlayerPawn?.CBodyComponent?.SceneNode?.AbsOrigin?.Y ?? 0),
                SpeakerPositionZ = (decimal)(player.PlayerPawn?.CBodyComponent?.SceneNode?.AbsOrigin?.Z ?? 0),
                SpeakerArea = DetermineMapArea(player),
                Team = player.Team.ToString(),
                RoundNumber = _currentRoundNumber,
                DuringAction = IsDuringAction(),
                PreRound = IsPreRound(),
                MidRound = IsMidRound(),
                PostRound = IsPostRound(),
                SituationalContext = DetermineSituationalContext(player),
                Description = $"{player.PlayerName} voice communication"
            };

            _activeVoiceComms[playerModel.Id] = voiceComm;
        }
    }

    private void OnVoiceInit(CSVCMsg_VoiceInit e)
    {
        // Log voice initialization - this tells us about voice codec and quality
        _logger.LogInformation("Voice initialized - Codec: {Codec}, Quality: {Quality}", e.Codec, e.Quality);
    }

    private void OnPlayerChat(Source1PlayerChatEvent e)
    {
        if (_demo == null || _currentDemoFile == null || e.Player == null) return;

        var player = _demo.Players.FirstOrDefault(p => p.UserId == e.Player);
        if (player == null || player.PlayerName == null) return;

        var playerModel = GetOrCreatePlayer(player);

        var chatComm = new Models.VoiceCommunication
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound?.Id,
            SpeakerId = playerModel.Id,
            StartTick = _demo.CurrentDemoTick.Value,
            EndTick = _demo.CurrentDemoTick.Value,
            StartTime = (float)(_demo.CurrentGameTime?.TotalSeconds ?? 0),
            EndTime = (float)(_demo.CurrentGameTime?.TotalSeconds ?? 0),
            Duration = 0f,
            CommunicationType = "text",
            ToTeam = e.Teamonly,
            TranscribedContent = e.Text,
            ContentSummary = SummarizeTextContent(e.Text),
            CommandCategory = CategorizeTextContent(e.Text),
            CommandPurpose = DetermineTextPurpose(e.Text),
            SpeakerPositionX = (decimal)(player.PlayerPawn?.CBodyComponent?.SceneNode?.AbsOrigin?.X ?? 0),
            SpeakerPositionY = (decimal)(player.PlayerPawn?.CBodyComponent?.SceneNode?.AbsOrigin?.Y ?? 0),
            SpeakerPositionZ = (decimal)(player.PlayerPawn?.CBodyComponent?.SceneNode?.AbsOrigin?.Z ?? 0),
            SpeakerArea = DetermineMapArea(player),
            Team = player.Team.ToString(),
            RoundNumber = _currentRoundNumber,
            DuringAction = IsDuringAction(),
            PreRound = IsPreRound(),
            MidRound = IsMidRound(),
            PostRound = IsPostRound(),
            SituationalContext = DetermineSituationalContext(player),
            IsCallout = IsCalloutMessage(e.Text),
            IsOrder = IsOrderMessage(e.Text),
            IsQuestion = IsQuestionMessage(e.Text),
            Description = $"{player.PlayerName} chat: {e.Text}"
        };

        _voiceCommunications.Add(chatComm);
        _recentComms.Add(chatComm);

        // Clean up old recent communications (keep last 10 seconds)
        var cutoffTime = chatComm.StartTime - 10f;
        _recentComms.RemoveAll(c => c.StartTime < cutoffTime);
    }

    private void OnPlayerRadio(Source1PlayerRadioEvent e)
    {
        if (_demo == null || _currentDemoFile == null || e.Player == null) return;

        var playerModel = GetOrCreatePlayer(e.Player);

        // Process radio command
        var radioComm = new Models.VoiceCommunication
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound?.Id,
            SpeakerId = playerModel.Id,
            StartTick = _demo.CurrentDemoTick.Value,
            EndTick = _demo.CurrentDemoTick.Value + 64, // Approximate radio duration
            StartTime = (float)(_demo.CurrentGameTime?.TotalSeconds ?? 0),
            EndTime = (float)(_demo.CurrentGameTime?.TotalSeconds ?? 0) + 2f,
            Duration = 2f,
            CommunicationType = "radio",
            RadioCommand = e.Text,
            CommandCategory = CategorizeRadioCommand(e.Text),
            CommandPurpose = DetermineRadioPurpose(e.Text),
            ToTeam = true, // Radio commands are always team-only
            SpeakerPositionX = (decimal)(e.Player.PlayerPawn?.CBodyComponent?.SceneNode?.AbsOrigin?.X ?? 0),
            SpeakerPositionY = (decimal)(e.Player.PlayerPawn?.CBodyComponent?.SceneNode?.AbsOrigin?.Y ?? 0),
            SpeakerPositionZ = (decimal)(e.Player.PlayerPawn?.CBodyComponent?.SceneNode?.AbsOrigin?.Z ?? 0),
            SpeakerArea = DetermineMapArea(e.Player),
            Team = e.Player.Team.ToString(),
            RoundNumber = _currentRoundNumber,
            DuringAction = IsDuringAction(),
            PreRound = IsPreRound(),
            MidRound = IsMidRound(),
            PostRound = IsPostRound(),
            SituationalContext = DetermineSituationalContext(e.Player),
            IsCallout = IsRadioCallout(e.Text),
            IsOrder = IsRadioOrder(e.Text),
            EffectivenessScore = CalculateRadioEffectiveness(e.Text, e.Player),
            Description = $"{e.Player.PlayerName} radio: {e.Text}"
        };

        _voiceCommunications.Add(radioComm);
        _recentComms.Add(radioComm);

        // Also add to radio commands collection for backwards compatibility
        var radioCommand = new Models.RadioCommand
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound?.Id,
            PlayerId = playerModel.Id,
            Tick = _demo.CurrentDemoTick.Value,
            GameTime = (float)(_demo.CurrentGameTime?.TotalSeconds ?? 0),
            Command = e.Text,
            CommandCategory = radioComm.CommandCategory ?? "unknown",
            PositionX = radioComm.SpeakerPositionX,
            PositionY = radioComm.SpeakerPositionY,
            PositionZ = radioComm.SpeakerPositionZ,
            Team = radioComm.Team,
            RoundNumber = _currentRoundNumber,
            Context = radioComm.SituationalContext
        };

        _radioCommands.Add(radioCommand);
    }

    private void OnRadioText(CCSUsrMsg_RadioText e)
    {
        if (_demo == null || _currentDemoFile == null) return;

        var player = FindPlayerByClientIndex(e.Client);
        if (player == null) return;

        var playerModel = GetOrCreatePlayer(player);

        // Process radio text message
        var radioTextComm = new Models.VoiceCommunication
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound?.Id,
            SpeakerId = playerModel.Id,
            StartTick = _demo.CurrentDemoTick.Value,
            EndTick = _demo.CurrentDemoTick.Value,
            StartTime = (float)(_demo.CurrentGameTime?.TotalSeconds ?? 0),
            EndTime = (float)(_demo.CurrentGameTime?.TotalSeconds ?? 0),
            Duration = 0f,
            CommunicationType = "radio_text",
            RadioCommand = e.MsgName,
            TranscribedContent = string.Join(" ", e.Params),
            CommandCategory = CategorizeRadioText(e.MsgName),
            CommandPurpose = DetermineRadioTextPurpose(e.MsgName),
            ToTeam = true,
            SpeakerPositionX = (decimal)(player.PlayerPawn?.CBodyComponent?.SceneNode?.AbsOrigin?.X ?? 0),
            SpeakerPositionY = (decimal)(player.PlayerPawn?.CBodyComponent?.SceneNode?.AbsOrigin?.Y ?? 0),
            SpeakerPositionZ = (decimal)(player.PlayerPawn?.CBodyComponent?.SceneNode?.AbsOrigin?.Z ?? 0),
            Team = player.Team.ToString(),
            RoundNumber = _currentRoundNumber,
            Description = $"{player.PlayerName} radio text: {e.MsgName}"
        };

        _voiceCommunications.Add(radioTextComm);
    }

    private void OnSayText2(CUserMessageSayText2 e)
    {
        if (_demo == null || _currentDemoFile == null) return;

        var player = _demo.Players.FirstOrDefault(p => p.Index == e.Entityindex);
        if (player?.PlayerName == null) return;

        var playerModel = GetOrCreatePlayer(player);

        var textComm = new Models.VoiceCommunication
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound?.Id,
            SpeakerId = playerModel.Id,
            StartTick = _demo.CurrentDemoTick.Value,
            EndTick = _demo.CurrentDemoTick.Value,
            StartTime = (float)(_demo.CurrentGameTime?.TotalSeconds ?? 0),
            EndTime = (float)(_demo.CurrentGameTime?.TotalSeconds ?? 0),
            Duration = 0f,
            CommunicationType = "formatted_text",
            TranscribedContent = $"{e.Messagename}: {string.Join(" ", e.Param1, e.Param2, e.Param3, e.Param4)}",
            CommandCategory = CategorizeFormattedText(e.Messagename),
            Team = player.Team.ToString(),
            RoundNumber = _currentRoundNumber,
            Description = $"{player.PlayerName} formatted text: {e.Messagename}"
        };

        _voiceCommunications.Add(textComm);
    }

    private void OnTeamBroadcastAudio(Source1TeamplayBroadcastAudioEvent e)
    {
        if (_demo == null || _currentDemoFile == null) return;

        // This represents team-wide audio broadcasts (like round win sounds, etc.)
        var teamAudioComm = new Models.VoiceCommunication
        {
            DemoFileId = _currentDemoFile.Id,
            RoundId = _currentRound?.Id,
            SpeakerId = -1, // System/team broadcast
            StartTick = _demo.CurrentDemoTick.Value,
            EndTick = _demo.CurrentDemoTick.Value + 160, // Approximate duration
            StartTime = (float)(_demo.CurrentGameTime?.TotalSeconds ?? 0),
            EndTime = (float)(_demo.CurrentGameTime?.TotalSeconds ?? 0) + 5f,
            Duration = 5f,
            CommunicationType = "team_audio",
            RadioCommand = e.Sound,
            CommandCategory = "team_broadcast",
            ToTeam = true,
            Team = e.Team == 2 ? "T" : "CT",
            RoundNumber = _currentRoundNumber,
            Description = $"Team {e.Team} audio broadcast: {e.Sound}"
        };

        _voiceCommunications.Add(teamAudioComm);
    }

    private void FinishVoiceCommunications()
    {
        // Finish any active voice communications
        foreach (var activeComm in _activeVoiceComms.Values)
        {
            if (_demo != null)
            {
                activeComm.EndTick = _demo.CurrentDemoTick.Value;
                activeComm.EndTime = (float)(_demo.CurrentGameTime?.TotalSeconds ?? 0);
                activeComm.Duration = activeComm.EndTime - activeComm.StartTime;
            }
            
            _voiceCommunications.Add(activeComm);
        }
        
        _activeVoiceComms.Clear();
        
        // Optional: Extract voice audio files if enabled
        if (_configuration.GetValue<bool>("ParserSettings:ExtractVoiceAudio", false))
        {
            ExtractVoiceAudioFiles();
        }
    }

    private void ExtractVoiceAudioFiles()
    {
        try
        {
            if (!_voiceDataPerSteamId.Any()) return;

            var outputDirectory = _configuration.GetValue<string>("ParserSettings:VoiceOutputDirectory", "voice_output");
            Directory.CreateDirectory(outputDirectory);

            const int sampleRate = 48000;
            const int numChannels = 1;

            OpusCodecFactory.AttemptToUseNativeLibrary = false;
            using var decoder = OpusCodecFactory.CreateDecoder(sampleRate, numChannels);

            foreach (var item in _voiceDataPerSteamId)
            {
                ulong steamId = item.Key;
                var audioMessages = item.Value;
                
                if (!audioMessages.Any()) continue;

                var player = _demo?.GetPlayerBySteamId(steamId);
                var playerName = player?.PlayerName ?? steamId.ToString();

                int compressedSize = audioMessages.Sum(a => a.VoiceData.Length);
                float[] pcmSamples = new float[compressedSize * 64];
                int numDecodedSamples = 0;

                foreach (var audioMessage in audioMessages)
                {
                    if (audioMessage.VoiceData.Length == 0) continue;
                    
                    try
                    {
                        int numSamplesDecodedInMessage = decoder.Decode(
                            audioMessage.VoiceData.Span, 
                            pcmSamples.AsSpan(numDecodedSamples), 
                            pcmSamples.Length - numDecodedSamples);
                        numDecodedSamples += numSamplesDecodedInMessage;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to decode voice data for player {PlayerName}", playerName);
                    }
                }

                if (numDecodedSamples > 0)
                {
                    var fileName = Path.Combine(outputDirectory, $"{steamId}_{playerName}_voice.wav");
                    WriteWavFile(fileName, sampleRate, numChannels, pcmSamples.AsSpan(0, numDecodedSamples));
                    
                    _logger.LogInformation("Extracted voice audio for {PlayerName}: {Messages} messages, {Size} KB", 
                        playerName, audioMessages.Count, compressedSize / 1024);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting voice audio files");
        }
    }

    private static void WriteWavFile(string filePath, int sampleRate, int numChannels, ReadOnlySpan<float> samplesFloat32)
    {
        int numSamples = samplesFloat32.Length;
        int sampleSize = sizeof(int);

        int[] samplesInt32 = new int[numSamples];
        const int conversionScale = int.MaxValue - 1;
        for (int i = 0; i < numSamples; i++)
            samplesInt32[i] = (int)(samplesFloat32[i] * conversionScale);

        WriteWavFile(filePath, numSamples, sampleRate, numChannels, sampleSize, 
            System.Runtime.InteropServices.MemoryMarshal.AsBytes(samplesInt32.AsSpan()));
    }

    private static void WriteWavFile(string filePath, int numSamples, int sampleRate, int numChannels, int sampleSize, ReadOnlySpan<byte> audioData)
    {
        var stream = new MemoryStream(44 + numSamples * sampleSize * numChannels);
        var wr = new BinaryWriter(stream);

        wr.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
        wr.Write(36 + numSamples * numChannels * sampleSize);
        wr.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));
        wr.Write((int)16);
        wr.Write((short)1); // Encoding
        wr.Write((short)numChannels); // Channels
        wr.Write((int)(sampleRate)); // Sample rate
        wr.Write((int)(sampleRate * sampleSize * numChannels)); // Average bytes per second
        wr.Write((short)(sampleSize * numChannels)); // block align
        wr.Write((short)(8 * sampleSize)); // bits per sample
        wr.Write(System.Text.Encoding.ASCII.GetBytes("data")); // data chunk id
        wr.Write((int)(numSamples * sampleSize * numChannels)); // data size

        stream.Write(audioData);
        File.WriteAllBytes(filePath, stream.GetBuffer());
    }

    private void AnalyzeCommunicationPatterns()
    {
        if (_currentRound == null || _currentDemoFile == null) return;

        // Analyze communication patterns for each team
        foreach (var team in new[] { "CT", "T" })
        {
            var teamComms = _recentComms.Where(c => c.Team == team && c.RoundId == _currentRound.Id)
                                       .OrderBy(c => c.StartTime)
                                       .ToList();

            if (teamComms.Count < 2) continue;

            // Detect coordination patterns
            var coordinationPattern = DetectCoordinationPattern(teamComms, team);
            if (coordinationPattern != null)
            {
                _communicationPatterns.Add(coordinationPattern);
            }

            // Detect leadership patterns
            var leadershipPattern = DetectLeadershipPattern(teamComms, team);
            if (leadershipPattern != null)
            {
                _communicationPatterns.Add(leadershipPattern);
            }
        }
    }

    // Helper methods for communication analysis
    private DemoFile.Game.Cs.CCSPlayerController? FindPlayerByClientIndex(int clientIndex)
    {
        if (clientIndex < 0) return null;
        return _demo?.Players.FirstOrDefault(p => p.Index == clientIndex);
    }

    private DemoFile.Game.Cs.CCSPlayerController? FindPlayerBySteamId(ulong steamId)
    {
        if (steamId == 0) return null;
        return _demo?.Players.FirstOrDefault(p => p.SteamId == steamId);
    }

    private string DetermineMapArea(DemoFile.Game.Cs.CCSPlayerController player)
    {
        // Simplified map area detection based on position
        // This would be enhanced with actual map coordinate analysis
        var pos = player.PlayerPawn?.CBodyComponent?.SceneNode?.AbsOrigin;
        if (pos == null) return "unknown";
        
        // Placeholder logic - would be map-specific
        return "mid"; // This would be replaced with actual area detection
    }

    private bool IsDuringAction() => _demo?.GamePhase?.ToString() == "Playing"; // Simplified
    private bool IsPreRound() => _demo?.GamePhase?.ToString() == "WarmupPeriod"; // Simplified  
    private bool IsMidRound() => IsDuringAction(); // Simplified
    private bool IsPostRound() => _demo?.GamePhase?.ToString() == "GameHalftime"; // Simplified

    private string DetermineSituationalContext(DemoFile.Game.Cs.CCSPlayerController player)
    {
        // Analyze current situation to determine context
        var livingTeammates = _demo?.Players.Count(p => p.Team == player.Team && (p.PlayerPawn?.Health ?? 0) > 0) ?? 1;
        var livingEnemies = _demo?.Players.Count(p => p.Team != player.Team && (p.PlayerPawn?.Health ?? 0) > 0) ?? 1;

        if (livingTeammates == 1 && livingEnemies > 1) return "clutch";
        if (livingTeammates > livingEnemies + 1) return "advantage";
        if (livingEnemies > livingTeammates + 1) return "disadvantage";
        return "balanced";
    }

    // Content analysis methods (simplified implementations)
    private string SummarizeTextContent(string text) => text.Length > 50 ? text[..47] + "..." : text;
    private string CategorizeTextContent(string text) => text.Contains("!") ? "urgent" : "informational";
    private string DetermineTextPurpose(string text) => 
        text.Contains("?") ? "question" : 
        text.ToLower().Contains("go") || text.ToLower().Contains("rush") ? "order" : "information";
    private bool IsCalloutMessage(string text) => text.ToLower().Contains("enemy") || text.ToLower().Contains("spotted");
    private bool IsOrderMessage(string text) => text.ToLower().Contains("go") || text.ToLower().Contains("rush") || text.ToLower().Contains("rotate");
    private bool IsQuestionMessage(string text) => text.Contains("?");

    private string CategorizeRadioCommand(string command) => 
        command.ToLower().Contains("enemy") ? "callout" :
        command.ToLower().Contains("go") ? "tactical" : "communication";
    private string DetermineRadioPurpose(string command) => 
        command.ToLower().Contains("spotted") ? "information" : "coordination";
    private bool IsRadioCallout(string command) => command.ToLower().Contains("enemy") || command.ToLower().Contains("spotted");
    private bool IsRadioOrder(string command) => command.ToLower().Contains("go") || command.ToLower().Contains("move");
    private float CalculateRadioEffectiveness(string command, DemoFile.Game.Cs.CCSPlayerController player) => 75f; // Placeholder

    private string CategorizeRadioText(string msgName) => "radio";
    private string DetermineRadioTextPurpose(string msgName) => "communication";
    private string CategorizeFormattedText(string messageName) => "system";

    private Models.CommunicationPattern? DetectCoordinationPattern(List<Models.VoiceCommunication> comms, string team)
    {
        if (comms.Count < 3) return null;

        // Detect if there's a coordination pattern (multiple players communicating within short time)
        var timeWindow = 5f; // 5 second window
        var firstComm = comms.First();
        var coordComms = comms.Where(c => c.StartTime - firstComm.StartTime <= timeWindow).ToList();

        if (coordComms.Count >= 3 && coordComms.Select(c => c.SpeakerId).Distinct().Count() >= 2)
        {
            return new Models.CommunicationPattern
            {
                DemoFileId = _currentDemoFile!.Id,
                RoundId = _currentRound?.Id,
                PatternType = "coordination",
                Team = team,
                StartTick = coordComms.Min(c => c.StartTick),
                EndTick = coordComms.Max(c => c.EndTick),
                StartTime = coordComms.Min(c => c.StartTime),
                EndTime = coordComms.Max(c => c.EndTime),
                Duration = coordComms.Max(c => c.EndTime) - coordComms.Min(c => c.StartTime),
                RoundNumber = _currentRoundNumber,
                ParticipantCount = coordComms.Select(c => c.SpeakerId).Distinct().Count(),
                ParticipantIds = $"[{string.Join(",", coordComms.Select(c => c.SpeakerId).Distinct())}]",
                CommunicationDensity = coordComms.Count / Math.Max(0.1f, coordComms.Max(c => c.EndTime) - coordComms.Min(c => c.StartTime)),
                CoordinationQuality = 75f, // Placeholder
                EffectivenessScore = 70f,
                IsExecutePattern = coordComms.Any(c => c.CommandCategory == "tactical"),
                PatternDescription = $"Team coordination with {coordComms.Count} communications"
            };
        }

        return null;
    }

    private Models.CommunicationPattern? DetectLeadershipPattern(List<Models.VoiceCommunication> comms, string team)
    {
        // Detect leadership patterns based on communication frequency and order-giving
        var leaders = comms.Where(c => c.IsOrder)
                          .GroupBy(c => c.SpeakerId)
                          .OrderByDescending(g => g.Count())
                          .FirstOrDefault();

        if (leaders != null && leaders.Count() >= 2)
        {
            return new Models.CommunicationPattern
            {
                DemoFileId = _currentDemoFile!.Id,
                RoundId = _currentRound?.Id,
                PatternType = "leadership",
                Team = team,
                StartTick = comms.Min(c => c.StartTick),
                EndTick = comms.Max(c => c.EndTick),
                StartTime = comms.Min(c => c.StartTime),
                EndTime = comms.Max(c => c.EndTime),
                Duration = comms.Max(c => c.EndTime) - comms.Min(c => c.StartTime),
                RoundNumber = _currentRoundNumber,
                PrimaryLeaderId = leaders.Key != -1 ? leaders.Key : null,
                ParticipantCount = comms.Select(c => c.SpeakerId).Distinct().Count(),
                LeadershipClarity = 80f,
                IsLeadershipSequence = true,
                PatternDescription = $"Leadership pattern with {leaders.Count()} orders"
            };
        }

        return null;
    }

    #endregion

    private async Task SaveDataToDatabase()
    {
        try
        {
            _logger.LogInformation("Saving parsed data to database...");

            var batchSize = _configuration.GetValue<int>("ParserSettings:DatabaseBatchSize", 1000);

            await SavePlayersAsync();
            await SaveMatchAsync();
            await SaveRoundsAsync();
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
            await SaveInBatchesAsync(_grenadeTrajectories, batchSize, "Grenade Trajectories");
            await SaveInBatchesAsync(_economyEvents, batchSize, "Economy Events");
            await SaveInBatchesAsync(_bulletImpacts, batchSize, "Bullet Impacts");
            await SaveInBatchesAsync(_playerMovements, batchSize, "Player Movements");
            await SaveInBatchesAsync(_zoneEvents, batchSize, "Zone Events");
            await SaveInBatchesAsync(_radioCommands, batchSize, "Radio Commands");
            await SaveInBatchesAsync(_weaponStates, batchSize, "Weapon States");
            await SaveInBatchesAsync(_flashEvents, batchSize, "Flash Events");
            
            // Save advanced entity tracking data
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
            
            // Save match statistics and end-of-match data
            await SaveInBatchesAsync(_matchStatistics, batchSize, "Match Statistics");
            await SaveInBatchesAsync(_endOfMatchData, batchSize, "End Of Match Data");
            await SaveInBatchesAsync(_teamPerformances, batchSize, "Team Performances");
            await SaveInBatchesAsync(_economicAnalyses, batchSize, "Economic Analyses");
            await SaveInBatchesAsync(_mapStatistics, batchSize, "Map Statistics");
            await SaveInBatchesAsync(_roundOutcomes, batchSize, "Round Outcomes");

            await CalculateAndSavePlayerMatchStats();
            
            // Calculate final match-level advanced statistics
            CalculateAdvancedPlayerStats("match");

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

    private async Task SaveInBatchesAsync<T>(ConcurrentBag<T> items, int batchSize, string itemType) where T : class
    {
        var itemsList = items.ToList();
        if (!itemsList.Any()) return;

        for (int i = 0; i < itemsList.Count; i += batchSize)
        {
            var batch = itemsList.Skip(i).Take(batchSize).ToList();
            _context.Set<T>().AddRange(batch);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Saved batch of {Count} {ItemType} (Total: {Total})", batch.Count, itemType, Math.Min(i + batchSize, itemsList.Count));
        }

        _logger.LogInformation("Saved {Count} {ItemType}", itemsList.Count, itemType);
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
                TotalDamageDealt = playerDamages.Sum(d => d.Damage),
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

    private async Task SetupDemoFileAsync(FileInfo fileInfo)
    {
        _logger.LogInformation("Setting up demo file: {FileName}", fileInfo.Name);
        // Demo file setup is handled by the DemoFileReader
        await Task.CompletedTask;
    }

    private async Task SaveAllDataAsync()
    {
        _logger.LogInformation("Starting to save all parsed data to database");

        try
        {
            // Save all collected data to database
            if (_players.Any())
            {
                await SaveBatch(_players.Values.ToList(), "Players");
            }

            if (_kills.Any())
            {
                await SaveBatch(_kills.ToList(), "Kills");
            }

            if (_damages.Any())
            {
                await SaveBatch(_damages.ToList(), "Damages");
            }

            if (_weaponFires.Any())
            {
                await SaveBatch(_weaponFires.ToList(), "WeaponFires");
            }

            if (_grenades.Any())
            {
                await SaveBatch(_grenades.ToList(), "Grenades");
            }

            if (_bombs.Any())
            {
                await SaveBatch(_bombs.ToList(), "Bombs");
            }

            if (_playerPositions.Any())
            {
                await SaveBatch(_playerPositions.ToList(), "PlayerPositions");
            }

            if (_chatMessages.Any())
            {
                await SaveBatch(_chatMessages.ToList(), "ChatMessages");
            }

            if (_equipment.Any())
            {
                await SaveBatch(_equipment.ToList(), "Equipment");
            }

            if (_gameEvents.Any())
            {
                await SaveBatch(_gameEvents.ToList(), "GameEvents");
            }

            if (_rounds.Any())
            {
                await SaveBatch(_rounds.ToList(), "Rounds");
            }

            // Calculate and save match statistics
            await CalculateAndSavePlayerMatchStats();

            _logger.LogInformation("Successfully saved all data to database");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving data to database");
            throw;
        }
    }
}