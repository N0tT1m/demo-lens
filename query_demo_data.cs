using Microsoft.EntityFrameworkCore;
using CS2DemoParser.Data;
using CS2DemoParser.Models;

// Simple C# program to query demo data
var connectionString = "Data Source=CS2DemoParser/cs2demo_test.db";
var optionsBuilder = new DbContextOptionsBuilder<CS2DemoContext>();
optionsBuilder.UseSqlite(connectionString);

using var context = new CS2DemoContext(optionsBuilder.Options);

// Get demo info
var demo = await context.DemoFiles.FirstOrDefaultAsync();
if (demo != null)
{
    Console.WriteLine($"Demo: {demo.FileName}");
    Console.WriteLine($"Map: {demo.MapName}");
    Console.WriteLine($"Duration: {demo.Duration:F2} seconds");
}

// Get all players
var players = await context.Players
    .Where(p => p.SteamId != 0)
    .ToListAsync();

Console.WriteLine("\nPlayers:");
foreach (var player in players)
{
    Console.WriteLine($"{player.PlayerName} (Steam: {player.SteamId})");
}

// Get kills (if any)
var killCount = await context.Kills.CountAsync();
Console.WriteLine($"\nTotal Kills: {killCount}");

// Get damages
var damageCount = await context.Damages.CountAsync();
Console.WriteLine($"Total Damage Events: {damageCount}");