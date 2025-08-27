using Microsoft.EntityFrameworkCore;
using CS2DemoParser.Models;

namespace CS2DemoParser.Data;

public class CS2DemoContext : DbContext
{
    public CS2DemoContext(DbContextOptions<CS2DemoContext> options) : base(options)
    {
    }

    public DbSet<Models.DemoFile> DemoFiles { get; set; } = null!;
    public DbSet<Match> Matches { get; set; } = null!;
    public DbSet<Player> Players { get; set; } = null!;
    public DbSet<Round> Rounds { get; set; } = null!;
    public DbSet<PlayerMatchStats> PlayerMatchStats { get; set; } = null!;
    public DbSet<PlayerRoundStats> PlayerRoundStats { get; set; } = null!;
    public DbSet<Kill> Kills { get; set; } = null!;
    public DbSet<Damage> Damages { get; set; } = null!;
    public DbSet<WeaponFire> WeaponFires { get; set; } = null!;
    public DbSet<Grenade> Grenades { get; set; } = null!;
    public DbSet<Bomb> Bombs { get; set; } = null!;
    public DbSet<PlayerPosition> PlayerPositions { get; set; } = null!;
    public DbSet<ChatMessage> ChatMessages { get; set; } = null!;
    public DbSet<Equipment> Equipment { get; set; } = null!;
    public DbSet<GameEvent> GameEvents { get; set; } = null!;
    public DbSet<GrenadeTrajectory> GrenadeTrajectories { get; set; } = null!;
    public DbSet<EconomyEvent> EconomyEvents { get; set; } = null!;
    public DbSet<BulletImpact> BulletImpacts { get; set; } = null!;
    public DbSet<PlayerMovement> PlayerMovements { get; set; } = null!;
    public DbSet<ZoneEvent> ZoneEvents { get; set; } = null!;
    public DbSet<RadioCommand> RadioCommands { get; set; } = null!;
    public DbSet<WeaponState> WeaponStates { get; set; } = null!;
    public DbSet<FlashEvent> FlashEvents { get; set; } = null!;
    
    // Enhanced tracking tables for advanced analytics
    public DbSet<PlayerInput> PlayerInputs { get; set; } = null!;
    public DbSet<WeaponStateChange> WeaponStateChanges { get; set; } = null!;
    public DbSet<EnhancedPlayerPosition> EnhancedPlayerPositions { get; set; } = null!;
    
    // Advanced entity tracking tables
    public DbSet<EntityLifecycle> EntityLifecycles { get; set; } = null!;
    public DbSet<EntityInteraction> EntityInteractions { get; set; } = null!;
    public DbSet<EntityVisibility> EntityVisibilities { get; set; } = null!;
    public DbSet<EntityEffect> EntityEffects { get; set; } = null!;
    public DbSet<DroppedItem> DroppedItems { get; set; } = null!;
    public DbSet<SmokeCloud> SmokeClouds { get; set; } = null!;
    public DbSet<FireArea> FireAreas { get; set; } = null!;
    
    // Game state tracking tables
    public DbSet<TeamState> TeamStates { get; set; } = null!;
    public DbSet<EconomyState> EconomyStates { get; set; } = null!;
    public DbSet<MapControl> MapControls { get; set; } = null!;
    public DbSet<TacticalEvent> TacticalEvents { get; set; } = null!;
    
    // Advanced statistics tables
    public DbSet<AdvancedPlayerStats> AdvancedPlayerStats { get; set; } = null!;
    public DbSet<PerformanceMetric> PerformanceMetrics { get; set; } = null!;
    public DbSet<RoundImpact> RoundImpacts { get; set; } = null!;
    
    // Voice and communication tables
    public DbSet<VoiceCommunication> VoiceCommunications { get; set; } = null!;
    public DbSet<CommunicationPattern> CommunicationPatterns { get; set; } = null!;
    
    // Advanced event tracking tables
    public DbSet<TemporaryEntity> TemporaryEntities { get; set; } = null!;
    public DbSet<EntityPropertyChange> EntityPropertyChanges { get; set; } = null!;
    public DbSet<HostageEvent> HostageEvents { get; set; } = null!;
    public DbSet<AdvancedUserMessage> AdvancedUserMessages { get; set; } = null!;
    public DbSet<PlayerBehaviorEvent> PlayerBehaviorEvents { get; set; } = null!;
    public DbSet<InfernoEvent> InfernoEvents { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // SQL Server cascade delete fix: Only cascade from DemoFile
        modelBuilder.Entity<Match>()
            .HasOne(m => m.DemoFile)
            .WithMany(d => d.Matches)
            .HasForeignKey(m => m.DemoFileId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Player>()
            .HasOne(p => p.DemoFile)
            .WithMany(d => d.Players)
            .HasForeignKey(p => p.DemoFileId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Kill>()
            .HasOne(k => k.Killer)
            .WithMany(p => p.KillsAsKiller)
            .HasForeignKey(k => k.KillerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Kill>()
            .HasOne(k => k.Victim)
            .WithMany(p => p.KillsAsVictim)
            .HasForeignKey(k => k.VictimId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Kill>()
            .HasOne(k => k.Assister)
            .WithMany(p => p.KillsAsAssister)
            .HasForeignKey(k => k.AssisterId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Damage>()
            .HasOne(d => d.Attacker)
            .WithMany(p => p.DamagesAsAttacker)
            .HasForeignKey(d => d.AttackerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Damage>()
            .HasOne(d => d.Victim)
            .WithMany(p => p.DamagesAsVictim)
            .HasForeignKey(d => d.VictimId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Player>()
            .HasIndex(p => p.SteamId);

        modelBuilder.Entity<Player>()
            .HasIndex(p => new { p.DemoFileId, p.PlayerSlot })
            .IsUnique();

        modelBuilder.Entity<PlayerPosition>()
            .HasIndex(pp => new { pp.PlayerId, pp.Tick });

        modelBuilder.Entity<Kill>()
            .HasIndex(k => new { k.RoundId, k.Tick });

        modelBuilder.Entity<Damage>()
            .HasIndex(d => new { d.RoundId, d.Tick });

        modelBuilder.Entity<WeaponFire>()
            .HasIndex(wf => new { wf.RoundId, wf.Tick });

        modelBuilder.Entity<GameEvent>()
            .HasIndex(ge => new { ge.DemoFileId, ge.Tick });

        modelBuilder.Entity<GameEvent>()
            .HasIndex(ge => ge.EventName);

        modelBuilder.Entity<Round>()
            .HasIndex(r => new { r.MatchId, r.RoundNumber })
            .IsUnique();

        modelBuilder.Entity<PlayerMatchStats>()
            .HasIndex(pms => new { pms.PlayerId, pms.MatchId })
            .IsUnique();

        modelBuilder.Entity<PlayerRoundStats>()
            .HasIndex(prs => new { prs.PlayerId, prs.RoundId })
            .IsUnique();

        modelBuilder.Entity<ChatMessage>()
            .HasIndex(cm => new { cm.DemoFileId, cm.Tick });

        modelBuilder.Entity<Grenade>()
            .HasIndex(g => new { g.RoundId, g.ThrowTick });

        modelBuilder.Entity<Bomb>()
            .HasIndex(b => new { b.RoundId, b.Tick });

        modelBuilder.Entity<Equipment>()
            .HasIndex(e => new { e.PlayerId, e.Tick });

        // Additional indices for new models
        modelBuilder.Entity<GrenadeTrajectory>()
            .HasIndex(gt => new { gt.PlayerId, gt.ThrowTick });

        modelBuilder.Entity<GrenadeTrajectory>()
            .HasIndex(gt => new { gt.RoundId, gt.GrenadeType });

        modelBuilder.Entity<EconomyEvent>()
            .HasIndex(ee => new { ee.PlayerId, ee.Tick });

        modelBuilder.Entity<EconomyEvent>()
            .HasIndex(ee => new { ee.RoundId, ee.EventType });

        modelBuilder.Entity<BulletImpact>()
            .HasIndex(bi => new { bi.PlayerId, bi.Tick });

        modelBuilder.Entity<BulletImpact>()
            .HasIndex(bi => new { bi.RoundId, bi.Weapon });

        modelBuilder.Entity<PlayerMovement>()
            .HasIndex(pm => new { pm.PlayerId, pm.Tick });

        modelBuilder.Entity<PlayerMovement>()
            .HasIndex(pm => new { pm.RoundId, pm.MovementType });

        modelBuilder.Entity<ZoneEvent>()
            .HasIndex(ze => new { ze.PlayerId, ze.Tick });

        modelBuilder.Entity<ZoneEvent>()
            .HasIndex(ze => new { ze.RoundId, ze.ZoneType });

        modelBuilder.Entity<RadioCommand>()
            .HasIndex(rc => new { rc.PlayerId, rc.Tick });

        modelBuilder.Entity<RadioCommand>()
            .HasIndex(rc => new { rc.RoundId, rc.Command });

        modelBuilder.Entity<WeaponState>()
            .HasIndex(ws => new { ws.PlayerId, ws.Tick });

        modelBuilder.Entity<WeaponState>()
            .HasIndex(ws => new { ws.RoundId, ws.EventType });

        modelBuilder.Entity<FlashEvent>()
            .HasIndex(fe => new { fe.FlashedPlayerId, fe.Tick });

        modelBuilder.Entity<FlashEvent>()
            .HasIndex(fe => new { fe.RoundId, fe.FlashDuration });

        // Advanced entity tracking indices
        modelBuilder.Entity<EntityLifecycle>()
            .HasIndex(el => new { el.EntityType, el.EntityId });

        modelBuilder.Entity<EntityLifecycle>()
            .HasIndex(el => new { el.RoundId, el.EventType });

        modelBuilder.Entity<EntityInteraction>()
            .HasIndex(ei => new { ei.InitiatorPlayerId, ei.Tick });

        modelBuilder.Entity<EntityInteraction>()
            .HasIndex(ei => new { ei.RoundId, ei.InteractionType });

        modelBuilder.Entity<EntityVisibility>()
            .HasIndex(ev => new { ev.ObserverPlayerId, ev.Tick });

        modelBuilder.Entity<EntityVisibility>()
            .HasIndex(ev => new { ev.RoundId, ev.EntityType });

        modelBuilder.Entity<EntityEffect>()
            .HasIndex(ee => new { ee.SourcePlayerId, ee.StartTick });

        modelBuilder.Entity<EntityEffect>()
            .HasIndex(ee => new { ee.RoundId, ee.EffectType });

        modelBuilder.Entity<DroppedItem>()
            .HasIndex(di => new { di.DropperPlayerId, di.DropTick });

        modelBuilder.Entity<DroppedItem>()
            .HasIndex(di => new { di.RoundId, di.ItemType });

        modelBuilder.Entity<DroppedItem>()
            .HasIndex(di => di.EntityId)
            .IsUnique();

        modelBuilder.Entity<SmokeCloud>()
            .HasIndex(sc => new { sc.ThrowerPlayerId, sc.StartTick });

        modelBuilder.Entity<SmokeCloud>()
            .HasIndex(sc => new { sc.RoundId, sc.Phase });

        modelBuilder.Entity<FireArea>()
            .HasIndex(fa => new { fa.ThrowerPlayerId, fa.StartTick });

        modelBuilder.Entity<FireArea>()
            .HasIndex(fa => new { fa.RoundId, fa.GrenadeType });

        // Additional relationship configurations for new models
        modelBuilder.Entity<EntityInteraction>()
            .HasOne(ei => ei.InitiatorPlayer)
            .WithMany()
            .HasForeignKey(ei => ei.InitiatorPlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<EntityInteraction>()
            .HasOne(ei => ei.TargetPlayer)
            .WithMany()
            .HasForeignKey(ei => ei.TargetPlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<EntityVisibility>()
            .HasOne(ev => ev.ObserverPlayer)
            .WithMany()
            .HasForeignKey(ev => ev.ObserverPlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<EntityVisibility>()
            .HasOne(ev => ev.TargetPlayer)
            .WithMany()
            .HasForeignKey(ev => ev.TargetPlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DroppedItem>()
            .HasOne(di => di.DropperPlayer)
            .WithMany()
            .HasForeignKey(di => di.DropperPlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DroppedItem>()
            .HasOne(di => di.PickerPlayer)
            .WithMany()
            .HasForeignKey(di => di.PickerPlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Game state tracking indices
        modelBuilder.Entity<TeamState>()
            .HasIndex(ts => new { ts.RoundId, ts.Team });

        modelBuilder.Entity<TeamState>()
            .HasIndex(ts => new { ts.DemoFileId, ts.Tick });

        modelBuilder.Entity<EconomyState>()
            .HasIndex(es => new { es.RoundId, es.Team });

        modelBuilder.Entity<EconomyState>()
            .HasIndex(es => new { es.DemoFileId, es.Phase });

        modelBuilder.Entity<MapControl>()
            .HasIndex(mc => new { mc.RoundId, mc.Tick });

        modelBuilder.Entity<MapControl>()
            .HasIndex(mc => new { mc.DemoFileId, mc.MapName });

        modelBuilder.Entity<TacticalEvent>()
            .HasIndex(te => new { te.RoundId, te.EventType });

        modelBuilder.Entity<TacticalEvent>()
            .HasIndex(te => new { te.InitiatorPlayerId, te.Tick });

        modelBuilder.Entity<TacticalEvent>()
            .HasOne(te => te.InitiatorPlayer)
            .WithMany()
            .HasForeignKey(te => te.InitiatorPlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Advanced statistics indices
        modelBuilder.Entity<AdvancedPlayerStats>()
            .HasIndex(aps => new { aps.PlayerId, aps.StatsType });

        modelBuilder.Entity<AdvancedPlayerStats>()
            .HasIndex(aps => new { aps.DemoFileId, aps.RoundNumber });

        modelBuilder.Entity<PerformanceMetric>()
            .HasIndex(pm => new { pm.PlayerId, pm.MetricType });

        modelBuilder.Entity<PerformanceMetric>()
            .HasIndex(pm => new { pm.RoundId, pm.MetricName });

        modelBuilder.Entity<RoundImpact>()
            .HasIndex(ri => new { ri.PlayerId, ri.RoundId })
            .IsUnique();

        modelBuilder.Entity<RoundImpact>()
            .HasIndex(ri => new { ri.DemoFileId, ri.OverallImpact });

        // Relationships for advanced stats
        modelBuilder.Entity<AdvancedPlayerStats>()
            .HasOne(aps => aps.Player)
            .WithMany()
            .HasForeignKey(aps => aps.PlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PerformanceMetric>()
            .HasOne(pm => pm.Player)
            .WithMany()
            .HasForeignKey(pm => pm.PlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RoundImpact>()
            .HasOne(ri => ri.Player)
            .WithMany()
            .HasForeignKey(ri => ri.PlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RoundImpact>()
            .HasOne(ri => ri.Round)
            .WithMany()
            .HasForeignKey(ri => ri.RoundId)
            .OnDelete(DeleteBehavior.Restrict);

        // Voice and communication indices
        modelBuilder.Entity<VoiceCommunication>()
            .HasIndex(vc => new { vc.SpeakerId, vc.StartTick });

        modelBuilder.Entity<VoiceCommunication>()
            .HasIndex(vc => new { vc.RoundId, vc.CommunicationType });

        modelBuilder.Entity<CommunicationPattern>()
            .HasIndex(cp => new { cp.RoundId, cp.PatternType });

        modelBuilder.Entity<CommunicationPattern>()
            .HasIndex(cp => new { cp.DemoFileId, cp.Team });

        // Voice and communication relationships
        modelBuilder.Entity<VoiceCommunication>()
            .HasOne(vc => vc.Speaker)
            .WithMany()
            .HasForeignKey(vc => vc.SpeakerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<VoiceCommunication>()
            .HasOne(vc => vc.TargetPlayer)
            .WithMany()
            .HasForeignKey(vc => vc.TargetPlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CommunicationPattern>()
            .HasOne(cp => cp.PrimaryLeader)
            .WithMany()
            .HasForeignKey(cp => cp.PrimaryLeaderId)
            .OnDelete(DeleteBehavior.Restrict);

        // Advanced event tracking indices and relationships
        modelBuilder.Entity<TemporaryEntity>()
            .HasIndex(te => new { te.PlayerId, te.Tick });

        modelBuilder.Entity<TemporaryEntity>()
            .HasIndex(te => new { te.RoundId, te.EntityType });

        modelBuilder.Entity<TemporaryEntity>()
            .HasIndex(te => new { te.DemoFileId, te.EntityType });

        modelBuilder.Entity<EntityPropertyChange>()
            .HasIndex(epc => new { epc.PlayerId, epc.Tick });

        modelBuilder.Entity<EntityPropertyChange>()
            .HasIndex(epc => new { epc.EntityIndex, epc.PropertyName });

        modelBuilder.Entity<EntityPropertyChange>()
            .HasIndex(epc => new { epc.RoundId, epc.ChangeType });

        modelBuilder.Entity<EntityPropertyChange>()
            .HasOne(epc => epc.CausedByPlayer)
            .WithMany()
            .HasForeignKey(epc => epc.CausedByPlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<HostageEvent>()
            .HasIndex(he => new { he.PlayerId, he.Tick });

        modelBuilder.Entity<HostageEvent>()
            .HasIndex(he => new { he.RoundId, he.EventType });

        modelBuilder.Entity<HostageEvent>()
            .HasIndex(he => new { he.HostageEntityId, he.EventType });

        modelBuilder.Entity<AdvancedUserMessage>()
            .HasIndex(aum => new { aum.PlayerId, aum.Tick });

        modelBuilder.Entity<AdvancedUserMessage>()
            .HasIndex(aum => new { aum.RoundId, aum.MessageType });

        modelBuilder.Entity<AdvancedUserMessage>()
            .HasOne(aum => aum.TargetPlayer)
            .WithMany()
            .HasForeignKey(aum => aum.TargetPlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PlayerBehaviorEvent>()
            .HasIndex(pbe => new { pbe.PlayerId, pbe.Tick });

        modelBuilder.Entity<PlayerBehaviorEvent>()
            .HasIndex(pbe => new { pbe.RoundId, pbe.BehaviorType });

        modelBuilder.Entity<PlayerBehaviorEvent>()
            .HasIndex(pbe => new { pbe.DemoFileId, pbe.BehaviorType });

        modelBuilder.Entity<InfernoEvent>()
            .HasIndex(ie => new { ie.ThrowerPlayerId, ie.StartTick });

        modelBuilder.Entity<InfernoEvent>()
            .HasIndex(ie => new { ie.RoundId, ie.EventType });

        modelBuilder.Entity<InfernoEvent>()
            .HasIndex(ie => new { ie.InfernoEntityId, ie.EventType });

        modelBuilder.Entity<InfernoEvent>()
            .HasOne(ie => ie.ExtinguishedByPlayer)
            .WithMany()
            .HasForeignKey(ie => ie.ExtinguishedByPlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Apply restrict to all foreign keys except the ones we specifically want to cascade
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var foreignKey in entityType.GetForeignKeys())
            {
                // Keep cascade for DemoFile -> Match and DemoFile -> Player only
                if ((foreignKey.DeclaringEntityType.ClrType == typeof(Match) && 
                     foreignKey.PrincipalEntityType.ClrType == typeof(Models.DemoFile)) ||
                    (foreignKey.DeclaringEntityType.ClrType == typeof(Player) && 
                     foreignKey.PrincipalEntityType.ClrType == typeof(Models.DemoFile)))
                {
                    continue; // Keep cascade
                }
                
                // Set all others to restrict
                foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }
    }
}