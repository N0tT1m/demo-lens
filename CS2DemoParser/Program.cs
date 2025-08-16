using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using CS2DemoParser.Data;
using CS2DemoParser.Services;
using Microsoft.Data.SqlClient;

namespace CS2DemoParser;

class Program
{
    static async Task Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();
        
        using var scope = host.Services.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();
        
        try
        {
            logger.LogInformation("CS2 Demo Parser starting...");
            
            var context = services.GetRequiredService<CS2DemoContext>();
            await EnsureDatabaseCreated(context, logger);
            
            var parserService = services.GetRequiredService<CorrectedDemoParserService>();
            
            if (args.Length == 0)
            {
                logger.LogError("Please provide a demo file path or --examine-db flag as an argument");
                logger.LogInformation("Usage: CS2DemoParser.exe <demo_file_path>");
                logger.LogInformation("       CS2DemoParser.exe --examine-db");
                logger.LogInformation("Example: CS2DemoParser.exe \"C:\\path\\to\\demo.dem\"");
                return;
            }
            
            if (args[0] == "--examine-db")
            {
                await ExamineDatabase(context, logger);
                return;
            }
            
            var demoFilePath = args[0];
            logger.LogInformation("Parsing demo file: {FilePath}", demoFilePath);
            
            var parseStartTime = DateTime.UtcNow;
            var success = await parserService.ParseDemoAsync(demoFilePath);
            var parseEndTime = DateTime.UtcNow;
            var parseDuration = parseEndTime - parseStartTime;
            
            if (success)
            {
                logger.LogInformation("Demo parsing completed successfully in {Duration}", parseDuration);
                await PrintStatistics(context, logger);
            }
            else
            {
                logger.LogError("Demo parsing failed");
                Environment.Exit(1);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred during demo parsing");
            Environment.Exit(1);
        }
        
        logger.LogInformation("CS2 Demo Parser finished");
    }
    
    static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                config.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true);
                config.AddEnvironmentVariables();
                config.AddCommandLine(args);
            })
            .ConfigureServices((context, services) =>
            {
                var connectionString = context.Configuration.GetConnectionString("DefaultConnection");
                
                services.AddDbContext<CS2DemoContext>(options =>
                    options.UseSqlServer(connectionString));
                
                services.AddScoped<CorrectedDemoParserService>();
                
                services.AddLogging(builder =>
                {
                    builder.AddConsole();
                    builder.AddDebug();
                });
            });
    
    static async Task EnsureDatabaseCreated(CS2DemoContext context, ILogger logger)
    {
        try
        {
            logger.LogInformation("Ensuring database is created...");
            
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
            if (pendingMigrations.Any())
            {
                logger.LogInformation("Applying database migrations...");
                await context.Database.MigrateAsync();
                logger.LogInformation("Database migrations applied successfully");
            }
            else
            {
                await context.Database.EnsureCreatedAsync();
                logger.LogInformation("Database is up to date");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to ensure database is created");
            throw;
        }
    }
    
    static async Task PrintStatistics(CS2DemoContext context, ILogger logger)
    {
        try
        {
            logger.LogInformation("=== PARSING STATISTICS ===");
            
            var demoFilesCount = await context.DemoFiles.CountAsync();
            var playersCount = await context.Players.CountAsync();
            var roundsCount = await context.Rounds.CountAsync();
            var killsCount = await context.Kills.CountAsync();
            var damagesCount = await context.Damages.CountAsync();
            var weaponFiresCount = await context.WeaponFires.CountAsync();
            var grenadesCount = await context.Grenades.CountAsync();
            var bombEventsCount = await context.Bombs.CountAsync();
            var playerPositionsCount = await context.PlayerPositions.CountAsync();
            var chatMessagesCount = await context.ChatMessages.CountAsync();
            var equipmentChangesCount = await context.Equipment.CountAsync();
            var gameEventsCount = await context.GameEvents.CountAsync();
            
            // Count new comprehensive data
            var grenadeTrajectoryCount = await context.GrenadeTrajectories.CountAsync();
            var economyEventsCount = await context.EconomyEvents.CountAsync();
            var bulletImpactsCount = await context.BulletImpacts.CountAsync();
            var playerMovementsCount = await context.PlayerMovements.CountAsync();
            var zoneEventsCount = await context.ZoneEvents.CountAsync();
            var radioCommandsCount = await context.RadioCommands.CountAsync();
            var weaponStatesCount = await context.WeaponStates.CountAsync();
            var flashEventsCount = await context.FlashEvents.CountAsync();
            var voiceCommunicationsCount = await context.VoiceCommunications.CountAsync();
            var communicationPatternsCount = await context.CommunicationPatterns.CountAsync();
            var temporaryEntitiesCount = await context.TemporaryEntities.CountAsync();
            var entityPropertyChangesCount = await context.EntityPropertyChanges.CountAsync();
            var hostageEventsCount = await context.HostageEvents.CountAsync();
            var advancedUserMessagesCount = await context.AdvancedUserMessages.CountAsync();
            var playerBehaviorEventsCount = await context.PlayerBehaviorEvents.CountAsync();
            var infernoEventsCount = await context.InfernoEvents.CountAsync();
            
            logger.LogInformation("Demo Files: {Count}", demoFilesCount);
            logger.LogInformation("Players: {Count}", playersCount);
            logger.LogInformation("Rounds: {Count}", roundsCount);
            logger.LogInformation("Kills: {Count}", killsCount);
            logger.LogInformation("Damage Events: {Count}", damagesCount);
            logger.LogInformation("Weapon Fires: {Count}", weaponFiresCount);
            logger.LogInformation("Grenade Events: {Count}", grenadesCount);
            logger.LogInformation("Bomb Events: {Count}", bombEventsCount);
            logger.LogInformation("Player Positions: {Count}", playerPositionsCount);
            logger.LogInformation("Chat Messages: {Count}", chatMessagesCount);
            logger.LogInformation("Equipment Changes: {Count}", equipmentChangesCount);
            logger.LogInformation("Game Events: {Count}", gameEventsCount);
            
            // Log new comprehensive data counts
            logger.LogInformation("Grenade Trajectories: {Count}", grenadeTrajectoryCount);
            logger.LogInformation("Economy Events: {Count}", economyEventsCount);
            logger.LogInformation("Bullet Impacts: {Count}", bulletImpactsCount);
            logger.LogInformation("Player Movements: {Count}", playerMovementsCount);
            logger.LogInformation("Zone Events: {Count}", zoneEventsCount);
            logger.LogInformation("Radio Commands: {Count}", radioCommandsCount);
            logger.LogInformation("Weapon States: {Count}", weaponStatesCount);
            logger.LogInformation("Flash Events: {Count}", flashEventsCount);
            logger.LogInformation("Voice Communications: {Count}", voiceCommunicationsCount);
            logger.LogInformation("Communication Patterns: {Count}", communicationPatternsCount);
            logger.LogInformation("Temporary Entities: {Count}", temporaryEntitiesCount);
            logger.LogInformation("Entity Property Changes: {Count}", entityPropertyChangesCount);
            logger.LogInformation("Hostage Events: {Count}", hostageEventsCount);
            logger.LogInformation("Advanced User Messages: {Count}", advancedUserMessagesCount);
            logger.LogInformation("Player Behavior Events: {Count}", playerBehaviorEventsCount);
            logger.LogInformation("Inferno Events: {Count}", infernoEventsCount);
            
            var latestDemo = await context.DemoFiles
                .OrderByDescending(d => d.ParsedAt)
                .FirstOrDefaultAsync();
                
            if (latestDemo != null)
            {
                logger.LogInformation("=== LATEST DEMO INFO ===");
                logger.LogInformation("File: {FileName}", latestDemo.FileName);
                logger.LogInformation("Map: {MapName}", latestDemo.MapName);
                logger.LogInformation("Duration: {Duration:F2} seconds", latestDemo.Duration);
                logger.LogInformation("Total Ticks: {TotalTicks}", latestDemo.TotalTicks);
                logger.LogInformation("Tick Rate: {TickRate}", latestDemo.TickRate);
                
                var topFragger = await context.PlayerMatchStats
                    .Include(pms => pms.Player)
                    .Where(pms => pms.Player.DemoFileId == latestDemo.Id)
                    .OrderByDescending(pms => pms.Kills)
                    .FirstOrDefaultAsync();
                    
                if (topFragger != null)
                {
                    logger.LogInformation("Top Fragger: {PlayerName} ({Kills}/{Deaths}/{Assists})", 
                        topFragger.Player.PlayerName, topFragger.Kills, topFragger.Deaths, topFragger.Assists);
                }
            }
            
            logger.LogInformation("=========================");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error printing statistics");
        }
    }
    
    static async Task ExamineDatabase(CS2DemoContext context, ILogger logger)
    {
        try
        {
            logger.LogInformation("=== CS2 DEMO PARSER DATABASE EXAMINATION ===");
            
            var connectionString = context.Database.GetConnectionString();
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
            
            // Get all tables and their row counts
            var tablesQuery = @"
                SELECT TABLE_NAME 
                FROM INFORMATION_SCHEMA.TABLES 
                WHERE TABLE_TYPE = 'BASE TABLE' 
                AND TABLE_SCHEMA = 'dbo'
                ORDER BY TABLE_NAME";
            
            var tables = new List<string>();
            using (var command = new SqlCommand(tablesQuery, connection))
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    tables.Add(reader.GetString(0));
                }
            }
            
            var emptyTables = new List<string>();
            var tablesWithData = new List<(string name, long count)>();
            
            logger.LogInformation("TABLE ANALYSIS:");
            logger.LogInformation(new string('=', 60));
            
            foreach (var table in tables)
            {
                var countQuery = $"SELECT COUNT(*) FROM [{table}]";
                using var command = new SqlCommand(countQuery, connection);
                var count = Convert.ToInt64(await command.ExecuteScalarAsync());
                
                if (count == 0)
                {
                    logger.LogInformation("❌ {Table,-30} EMPTY", table);
                    emptyTables.Add(table);
                }
                else
                {
                    logger.LogInformation("✅ {Table,-30} {Count:N0} rows", table, count);
                    tablesWithData.Add((table, count));
                }
            }
            
            logger.LogInformation(new string('=', 60));
            logger.LogInformation("📊 SUMMARY:");
            logger.LogInformation("   Total Tables: {Total}", tables.Count);
            logger.LogInformation("   Tables with Data: {WithData}", tablesWithData.Count);
            logger.LogInformation("   Empty Tables: {Empty}", emptyTables.Count);
            logger.LogInformation("   Data Coverage: {Coverage:F1}%", (tablesWithData.Count * 100.0 / tables.Count));
            
            if (emptyTables.Any())
            {
                logger.LogInformation("🚨 EMPTY TABLES ANALYSIS:");
                var criticalTables = new[] { "Kills", "Damages", "WeaponFires", "Grenades", "Bombs", "PlayerPositions", "ChatMessages", "Equipment", "PlayerRoundStats", "PlayerMatchStats" };
                
                foreach (var table in emptyTables)
                {
                    if (criticalTables.Contains(table))
                    {
                        logger.LogInformation("   🔴 CRITICAL: {Table} - Should have data from demo parsing", table);
                    }
                    else
                    {
                        logger.LogInformation("   🟡 {Table} - May be optional/advanced feature", table);
                    }
                }
            }
            
            // Check data relationships
            logger.LogInformation(new string('=', 60));
            logger.LogInformation("🔗 FOREIGN KEY CONSTRAINT ANALYSIS:");
            
            // Check for common orphaned records
            var fkChecks = new Dictionary<string, string>
            {
                ["Players without DemoFile"] = "SELECT COUNT(*) FROM Players p LEFT JOIN DemoFiles d ON p.DemoFileId = d.Id WHERE d.Id IS NULL",
                ["Matches without DemoFile"] = "SELECT COUNT(*) FROM Matches m LEFT JOIN DemoFiles d ON m.DemoFileId = d.Id WHERE d.Id IS NULL",
                ["Rounds without Match"] = "SELECT COUNT(*) FROM Rounds r LEFT JOIN Matches m ON r.MatchId = m.Id WHERE m.Id IS NULL",
                ["Kills with invalid VictimId"] = "SELECT COUNT(*) FROM Kills k LEFT JOIN Players p ON k.VictimId = p.Id WHERE p.Id IS NULL AND k.VictimId IS NOT NULL",
                ["Kills with invalid RoundId"] = "SELECT COUNT(*) FROM Kills k LEFT JOIN Rounds r ON k.RoundId = r.Id WHERE r.Id IS NULL",
                ["Damages with invalid RoundId"] = "SELECT COUNT(*) FROM Damages d LEFT JOIN Rounds r ON d.RoundId = r.Id WHERE r.Id IS NULL"
            };
            
            foreach (var check in fkChecks)
            {
                try
                {
                    using var command = new SqlCommand(check.Value, connection);
                    var count = Convert.ToInt64(await command.ExecuteScalarAsync());
                    
                    if (count > 0)
                    {
                        logger.LogInformation("   🚨 {Check}: {Count:N0} orphaned records", check.Key, count);
                    }
                    else
                    {
                        logger.LogInformation("   ✅ {Check}: No issues", check.Key);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogInformation("   ❌ {Check}: Error - {Message}", check.Key, ex.Message);
                }
            }
            
            // Show some sample data from populated tables
            if (tablesWithData.Any())
            {
                logger.LogInformation(new string('=', 60));
                logger.LogInformation("📋 SAMPLE DATA FROM POPULATED TABLES:");
                
                var sampleTables = tablesWithData.Take(3).ToList();
                foreach (var (tableName, rowCount) in sampleTables)
                {
                    logger.LogInformation("--- {TableName} (showing first 2 rows) ---", tableName);
                    try
                    {
                        var sampleQuery = $"SELECT TOP 2 * FROM [{tableName}]";
                        using var command = new SqlCommand(sampleQuery, connection);
                        using var reader = await command.ExecuteReaderAsync();
                        
                        if (reader.HasRows)
                        {
                            var fieldCount = reader.FieldCount;
                            var columnNames = new string[fieldCount];
                            for (int i = 0; i < fieldCount; i++)
                            {
                                columnNames[i] = reader.GetName(i);
                            }
                            
                            logger.LogInformation("Columns: {Columns}...", string.Join(", ", columnNames.Take(5)));
                            
                            int rowNum = 1;
                            while (await reader.ReadAsync() && rowNum <= 2)
                            {
                                var values = new List<string>();
                                for (int i = 0; i < Math.Min(5, fieldCount); i++)
                                {
                                    var value = reader.IsDBNull(i) ? "NULL" : reader.GetValue(i).ToString();
                                    values.Add(value?.Length > 20 ? value.Substring(0, 17) + "..." : value ?? "NULL");
                                }
                                logger.LogInformation("Row {RowNum}: {Values}...", rowNum, string.Join(" | ", values));
                                rowNum++;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogInformation("Error reading sample data: {Message}", ex.Message);
                    }
                }
            }
            
            logger.LogInformation(new string('=', 60));
            logger.LogInformation("✨ Database examination complete!");
            
            if (emptyTables.Count > 5)
            {
                logger.LogInformation("💡 RECOMMENDATION: {Count} tables are empty. This suggests:", emptyTables.Count);
                logger.LogInformation("   1. Demo parsing may not be populating all expected entities");
                logger.LogInformation("   2. Some features may be disabled or not implemented");
                logger.LogInformation("   3. Foreign key constraints may be preventing data insertion");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error examining database");
        }
    }
}
