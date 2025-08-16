using Microsoft.EntityFrameworkCore;
using CS2DemoParser.Data;
using CS2DemoParser.Models;
using System.Reflection;

// Simple C# program to query demo data
var connectionString = "Data Source=/Users/timmy/workspace/public-projects/cs-vault/CS2DemoParser/cs2demo_test.db";
var optionsBuilder = new DbContextOptionsBuilder<CS2DemoContext>();
optionsBuilder.UseSqlite(connectionString);

using var context = new CS2DemoContext(optionsBuilder.Options);

Console.WriteLine("=== CS2 Demo Parser Database Analysis ===\n");

// Get demo info
var demo = await context.DemoFiles.FirstOrDefaultAsync();
if (demo != null)
{
    Console.WriteLine($"Demo: {demo.FileName}");
    Console.WriteLine($"Map: {demo.MapName}");
    Console.WriteLine($"Duration: {demo.Duration:F2} seconds");
    Console.WriteLine($"Tick Rate: {demo.TickRate}");
    Console.WriteLine($"Server Name: {demo.ServerName}");
    Console.WriteLine();
}
else
{
    Console.WriteLine("No demo file found in database!\n");
}

// Check all table counts
var allDbSets = context.GetType().GetProperties()
    .Where(p => p.PropertyType.IsGenericType && 
                p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
    .ToList();

Console.WriteLine("=== Table Row Counts ===");
var emptyCounts = 0;
var totalTables = allDbSets.Count;

foreach (var dbSetProperty in allDbSets)
{
    var dbSet = dbSetProperty.GetValue(context);
    if (dbSet != null)
    {
        // Use reflection to call Count() on the DbSet
        var countMethod = typeof(Queryable).GetMethods()
            .First(m => m.Name == "Count" && m.GetParameters().Length == 1)
            .MakeGenericMethod(dbSetProperty.PropertyType.GetGenericArguments()[0]);
        
        var count = (int)countMethod.Invoke(null, new[] { dbSet })!;
        
        if (count == 0)
        {
            emptyCounts++;
            Console.WriteLine($"{dbSetProperty.Name}: {count} ⚠️  EMPTY");
        }
        else
        {
            Console.WriteLine($"{dbSetProperty.Name}: {count} ✓");
        }
    }
}

Console.WriteLine($"\n=== Summary ===");
Console.WriteLine($"Total tables: {totalTables}");
Console.WriteLine($"Empty tables: {emptyCounts}");
Console.WriteLine($"Populated tables: {totalTables - emptyCounts}");
Console.WriteLine($"Population percentage: {((double)(totalTables - emptyCounts) / totalTables * 100):F1}%");

// Get basic populated data details
if (demo != null)
{
    Console.WriteLine("\n=== Basic Data Analysis ===");

    // Players
    var players = await context.Players
        .Where(p => p.SteamId != 0)
        .ToListAsync();
    
    Console.WriteLine($"\nPlayers ({players.Count}):");
    foreach (var player in players.Take(10)) // Show only first 10
    {
        Console.WriteLine($"  - {player.PlayerName} (Steam: {player.SteamId})");
    }
    if (players.Count > 10)
    {
        Console.WriteLine($"  ... and {players.Count - 10} more");
    }

    // Kills
    var killCount = await context.Kills.CountAsync();
    Console.WriteLine($"\nKills: {killCount}");
    if (killCount > 0)
    {
        var sampleKills = await context.Kills
            .Include(k => k.Killer)
            .Include(k => k.Victim)
            .Take(5)
            .ToListAsync();
        foreach (var kill in sampleKills)
        {
            Console.WriteLine($"  - {kill.Killer?.PlayerName} killed {kill.Victim?.PlayerName} with {kill.Weapon} at tick {kill.Tick}");
        }
    }

    // Damages
    var damageCount = await context.Damages.CountAsync();
    Console.WriteLine($"\nDamages: {damageCount}");
    if (damageCount > 0)
    {
        var sampleDamages = await context.Damages
            .Include(d => d.Attacker)
            .Include(d => d.Victim)
            .Take(5)
            .ToListAsync();
        foreach (var damage in sampleDamages)
        {
            Console.WriteLine($"  - {damage.Attacker?.PlayerName} damaged {damage.Victim?.PlayerName} for {damage.DamageAmount} HP with {damage.Weapon}");
        }
    }
}