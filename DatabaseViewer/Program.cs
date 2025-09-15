using Microsoft.EntityFrameworkCore;
using CS2DemoParser.Data;
using CS2DemoParser.Models;

// Comprehensive C# app to analyze demo database structure and investigate timing issues
var connectionString = "Server=192.168.1.74,1433;Database=demos;User Id=sa;Password=StrongPassword123!;TrustServerCertificate=true;";
var optionsBuilder = new DbContextOptionsBuilder<CS2DemoContext>();
optionsBuilder.UseSqlServer(connectionString);

using var context = new CS2DemoContext(optionsBuilder.Options);

Console.WriteLine("=== CS2 Demo Parser Database Inspector ===\n");

// Get demo info
var demo = await context.DemoFiles.FirstOrDefaultAsync();
if (demo == null)
{
    Console.WriteLine("❌ No demo file found in database!");
    return;
}

Console.WriteLine($"📁 Demo: {demo.FileName}");
Console.WriteLine($"🗺️  Map: {demo.MapName}");
Console.WriteLine($"⏱️  Duration: {demo.Duration:F2} seconds");
Console.WriteLine($"🎯 Tick Rate: {demo.TickRate}");
Console.WriteLine($"📊 Total Ticks: {demo.TotalTicks}");
Console.WriteLine($"🖥️  Server: {demo.ServerName}");
Console.WriteLine($"🗓️  Parsed: {demo.ParsedAt}");
Console.WriteLine();

// Check rounds data specifically for timing issues
var rounds = await context.Rounds
    .OrderBy(r => r.RoundNumber)
    .ToListAsync();

Console.WriteLine($"=== ROUND TIMING ANALYSIS ({rounds.Count} rounds) ===");
Console.WriteLine();

if (!rounds.Any())
{
    Console.WriteLine("❌ No rounds found in database!");
    return;
}

// Calculate demo start tick (minimum tick from all data)
var minTickFromPositions = await context.PlayerPositions.MinAsync(pp => (int?)pp.Tick) ?? 0;
var minTickFromEvents = 0;

// Try to get minimum tick from various event tables
var killsMinTick = await context.Kills.AnyAsync() ? await context.Kills.MinAsync(k => (int?)k.Tick) ?? 0 : int.MaxValue;
var damagesMinTick = await context.Damages.AnyAsync() ? await context.Damages.MinAsync(d => (int?)d.Tick) ?? 0 : int.MaxValue;
var grenadesMinTick = await context.Grenades.AnyAsync() ? await context.Grenades.MinAsync(g => (int?)g.ThrowTick) ?? 0 : int.MaxValue;

var overallMinTick = new[] { minTickFromPositions, killsMinTick, damagesMinTick, grenadesMinTick }
    .Where(t => t != int.MaxValue)
    .DefaultIfEmpty(0)
    .Min();

Console.WriteLine($"🔍 Demo Analysis:");
Console.WriteLine($"   Overall minimum tick: {overallMinTick}");
Console.WriteLine($"   Demo total ticks: {demo.TotalTicks}");
Console.WriteLine($"   Calculated demo duration: {(double)demo.TotalTicks / demo.TickRate:F2}s");
Console.WriteLine();

Console.WriteLine("📋 DETAILED ROUND BREAKDOWN:");
Console.WriteLine("Round | StartTick | EndTick   | Duration | Start->End Calc | Game Time Start | Winner");
Console.WriteLine("------|-----------|-----------|----------|-----------------|------------------|--------");

foreach (var round in rounds)
{
    var tickDuration = round.EndTick.HasValue ? round.EndTick.Value - round.StartTick : 0;
    var calculatedDuration = round.EndTick.HasValue ? (double)tickDuration / demo.TickRate : 0;

    // Calculate game time from demo start
    var gameTimeStart = (double)(round.StartTick - overallMinTick) / demo.TickRate;

    Console.WriteLine($"{round.RoundNumber,5} | {round.StartTick,9} | {round.EndTick?.ToString() ?? "NULL",9} | {round.Duration,8:F1}s | {calculatedDuration,15:F1}s | {gameTimeStart,16:F1}s | {round.WinnerTeam ?? "TBD"}");
}

Console.WriteLine();

// Analyze potential timing issues
Console.WriteLine("🚨 TIMING ISSUE ANALYSIS:");

var issuesFound = 0;

foreach (var round in rounds.Take(5)) // Check first 5 rounds in detail
{
    Console.WriteLine($"\n--- Round {round.RoundNumber} Analysis ---");
    Console.WriteLine($"Start Tick: {round.StartTick}");
    Console.WriteLine($"End Tick: {round.EndTick?.ToString() ?? "NULL"}");
    Console.WriteLine($"Stored Duration: {round.Duration:F1}s");
    Console.WriteLine($"Start Time: {round.StartTime}");
    Console.WriteLine($"End Time: {round.EndTime?.ToString() ?? "NULL"}");

    if (round.EndTick.HasValue)
    {
        var tickDuration = round.EndTick.Value - round.StartTick;
        var calculatedDuration = (double)tickDuration / demo.TickRate;
        var durationDiff = Math.Abs(calculatedDuration - round.Duration);

        Console.WriteLine($"Tick Duration: {tickDuration} ticks");
        Console.WriteLine($"Calculated Duration: {calculatedDuration:F1}s");
        Console.WriteLine($"Duration Difference: {durationDiff:F1}s");

        if (durationDiff > 0.5) // More than 0.5s difference is significant
        {
            Console.WriteLine($"⚠️  ISSUE: Duration mismatch > 0.5s ({durationDiff:F1}s)");
            issuesFound++;
        }
    }

    // Calculate game time start
    var gameTimeStart = (double)(round.StartTick - overallMinTick) / demo.TickRate;
    Console.WriteLine($"Game Time at Start: {gameTimeStart:F1}s");

    if (round.RoundNumber == 1 && gameTimeStart > 30.0) // First round shouldn't start way into the game
    {
        Console.WriteLine($"⚠️  ISSUE: Round 1 starts at {gameTimeStart:F1}s - expected closer to 0s");
        issuesFound++;
    }

    // Check if we have events in this round
    var eventsInRound = await context.Kills
        .Where(k => k.Tick >= round.StartTick && (!round.EndTick.HasValue || k.Tick <= round.EndTick))
        .CountAsync();

    var positionsInRound = await context.PlayerPositions
        .Where(pp => pp.Tick >= round.StartTick && (!round.EndTick.HasValue || pp.Tick <= round.EndTick.Value))
        .CountAsync();

    Console.WriteLine($"Events in round: {eventsInRound}");
    Console.WriteLine($"Position records in round: {positionsInRound}");
}

Console.WriteLine($"\n=== SUMMARY ===");
Console.WriteLine($"Total rounds analyzed: {rounds.Count}");
Console.WriteLine($"Issues found: {issuesFound}");

if (issuesFound > 0)
{
    Console.WriteLine("\n🔧 RECOMMENDED FIXES:");
    Console.WriteLine("1. Review round start tick calculation in CorrectedDemoParserService.OnRoundStart()");
    Console.WriteLine("2. Check if round_start event is firing at the correct game time");
    Console.WriteLine("3. Consider using demo.CurrentGameTime instead of DateTime.UtcNow for timing");
    Console.WriteLine("4. Verify tick rate calculation and demo parsing sequence");
}

// Show tick range for each round vs actual events
Console.WriteLine("\n=== TICK RANGE VALIDATION ===");
foreach (var round in rounds.Take(3))
{
    Console.WriteLine($"\nRound {round.RoundNumber}:");
    Console.WriteLine($"  Expected tick range: {round.StartTick} - {round.EndTick?.ToString() ?? "ongoing"}");

    var minEventTick = await context.Kills
        .Where(k => k.RoundId == round.Id)
        .MinAsync(k => (int?)k.Tick);
    var maxEventTick = await context.Kills
        .Where(k => k.RoundId == round.Id)
        .MaxAsync(k => (int?)k.Tick);

    if (minEventTick.HasValue && maxEventTick.HasValue)
    {
        Console.WriteLine($"  Actual event range: {minEventTick} - {maxEventTick}");
        if (minEventTick < round.StartTick || (round.EndTick.HasValue && maxEventTick > round.EndTick))
        {
            Console.WriteLine($"  ⚠️  Events outside expected round bounds!");
        }
    }
    else
    {
        Console.WriteLine($"  No events found in round");
    }
}

Console.WriteLine("\n✅ Database inspection complete!");