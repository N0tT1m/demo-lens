using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text.Json;

namespace CS2DemoParserWeb.Controllers
{
    [ApiController]
    [Route("api/round-viewer")]
    public class RoundViewerController : ControllerBase
    {
        private readonly string _connectionString;
        private readonly ILogger<RoundViewerController> _logger;

        public RoundViewerController(IConfiguration configuration, ILogger<RoundViewerController> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? Environment.GetEnvironmentVariable("CONNECTION_STRING")
                ?? throw new ArgumentNullException("No connection string found");
            _logger = logger;
        }

        private async Task<string?> GetDemoSourceAsync(int demoId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var sql = "SELECT DemoSource FROM DemoFiles WHERE Id = @DemoId";
                using var command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@DemoId", demoId);

                var result = await command.ExecuteScalarAsync();
                return result?.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting demo source for demoId: {DemoId}", demoId);
                return null;
            }
        }

        private int GetDisplayRoundNumber(int databaseRoundNumber, string? demoSource)
        {
            // For Faceit and ESEA demos, subtract 2 from database round to show user-friendly round numbers
            if (demoSource == "faceit" || demoSource == "esea")
            {
                return Math.Max(1, databaseRoundNumber - 2);
            }

            return databaseRoundNumber;
        }

        private int GetDatabaseRoundNumber(int displayRoundNumber, string? demoSource)
        {
            // For Faceit and ESEA demos, add 2 to user's round number to get database round
            if (demoSource == "faceit" || demoSource == "esea")
            {
                return displayRoundNumber + 2;
            }

            return displayRoundNumber;
        }

        [HttpGet("rounds")]
        public async Task<IActionResult> GetRounds([FromQuery] int? demoId = null, [FromQuery] string? mapName = null)
        {
            try
            {
                _logger.LogInformation("GetRounds called with demoId: {DemoId}, mapName: {MapName}", demoId, mapName);

                if (demoId == null)
                {
                    _logger.LogWarning("GetRounds called with null demoId");
                    return BadRequest("DemoId is required");
                }

                // Get demo source for round number adjustment
                var demoSource = await GetDemoSourceAsync(demoId.Value);
                _logger.LogInformation("Demo source: {DemoSource}", demoSource);

                var sql = @"
                    SELECT
                        r.Id,
                        r.RoundNumber,
                        r.StartTick,
                        r.EndTick,
                        r.Duration,
                        r.WinnerTeam,
                        r.EndReason,
                        r.CTScore,
                        r.TScore,
                        r.CTLivePlayers,
                        r.TLivePlayers,
                        r.BombPlanted,
                        r.BombDefused,
                        r.BombExploded,
                        d.Id as DemoId,
                        d.FileName as DemoName,
                        d.MapName
                    FROM Rounds r
                    INNER JOIN Matches m ON r.MatchId = m.Id
                    INNER JOIN DemoFiles d ON m.DemoFileId = d.Id
                    WHERE d.Id = @DemoId AND r.RoundNumber IS NOT NULL
                    ORDER BY r.RoundNumber";

                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@DemoId", (object?)demoId ?? DBNull.Value);

                using var reader = await command.ExecuteReaderAsync();
                var rounds = new List<Dictionary<string, object>>();

                while (await reader.ReadAsync())
                {
                    var round = new Dictionary<string, object>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        var fieldName = reader.GetName(i);
                        var value = reader.GetValue(i);

                        // Adjust RoundNumber for display (Faceit/ESEA demos)
                        if (fieldName == "RoundNumber" && value != DBNull.Value)
                        {
                            var databaseRoundNumber = Convert.ToInt32(value);
                            var displayRoundNumber = GetDisplayRoundNumber(databaseRoundNumber, demoSource);
                            round[fieldName] = displayRoundNumber;
                        }
                        else
                        {
                            round[fieldName] = value == DBNull.Value ? null! : value;
                        }
                    }

                    // Only include rounds that have positive display numbers
                    if (round.ContainsKey("RoundNumber") && Convert.ToInt32(round["RoundNumber"]) > 0)
                    {
                        rounds.Add(round);
                    }
                }

                return Ok(new
                {
                    Title = "Available Rounds",
                    Data = rounds,
                    TotalRecords = rounds.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting rounds");
                return StatusCode(500, $"Error getting rounds: {ex.Message}");
            }
        }

        [HttpGet("debug-round-info")]
        public async Task<IActionResult> GetDebugRoundInfo([FromQuery] int demoId, [FromQuery] int roundNumber)
        {
            try
            {
                var demoSource = await GetDemoSourceAsync(demoId);
                var adjustedRoundNumber = GetDatabaseRoundNumber(roundNumber, demoSource);

                var sql = @"
                    SELECT
                        r.StartTick,
                        r.EndTick,
                        r.Duration,
                        COUNT(DISTINCT pp.Tick) as AvailableTickCount,
                        MIN(pp.Tick) as MinDataTick,
                        MAX(pp.Tick) as MaxDataTick,
                        COUNT(DISTINCT pp.PlayerId) as PlayerCount
                    FROM Rounds r
                    INNER JOIN Matches m ON r.MatchId = m.Id
                    INNER JOIN DemoFiles d ON m.DemoFileId = d.Id
                    INNER JOIN Players p ON p.DemoFileId = d.Id
                    INNER JOIN PlayerPositions pp ON pp.PlayerId = p.Id
                    WHERE d.Id = @DemoId
                        AND r.RoundNumber = @RoundNumber
                        AND pp.Tick >= (r.StartTick - 2000)  -- Look 2000 ticks back
                        AND (r.EndTick IS NULL OR pp.Tick <= r.EndTick)
                    GROUP BY r.StartTick, r.EndTick, r.Duration";

                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@DemoId", demoId);
                command.Parameters.AddWithValue("@RoundNumber", adjustedRoundNumber);

                using var reader = await command.ExecuteReaderAsync();
                var debugInfo = new Dictionary<string, object>();

                if (await reader.ReadAsync())
                {
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        var value = reader.GetValue(i);
                        debugInfo[reader.GetName(i)] = value == DBNull.Value ? null! : value;
                    }
                }

                return Ok(new
                {
                    Title = "Debug Round Info",
                    DemoId = demoId,
                    RoundNumber = roundNumber,
                    AdjustedRoundNumber = adjustedRoundNumber,
                    DemoSource = demoSource,
                    DebugInfo = debugInfo
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting debug round info");
                return StatusCode(500, $"Error getting debug info: {ex.Message}");
            }
        }

        [HttpGet("check-spawn-data")]
        public async Task<IActionResult> CheckSpawnData([FromQuery] int demoId, [FromQuery] int roundNumber)
        {
            try
            {
                var demoSource = await GetDemoSourceAsync(demoId);
                var adjustedRoundNumber = GetDatabaseRoundNumber(roundNumber, demoSource);

                // Check if we have ANY position data before the round starts
                var sql = @"
                    SELECT TOP 50
                        pp.Tick,
                        p.PlayerName,
                        p.Team,
                        pp.PositionX,
                        pp.PositionY,
                        pp.PositionZ,
                        r.StartTick,
                        (pp.Tick - r.StartTick) as TicksFromRoundStart
                    FROM PlayerPositions pp
                    INNER JOIN Players p ON pp.PlayerId = p.Id
                    INNER JOIN DemoFiles d ON p.DemoFileId = d.Id
                    INNER JOIN Matches m ON d.Id = m.DemoFileId
                    INNER JOIN Rounds r ON m.Id = r.MatchId
                    WHERE d.Id = @DemoId
                        AND r.RoundNumber = @RoundNumber
                        AND pp.Tick BETWEEN (r.StartTick - 2000) AND (r.StartTick + 100)
                    ORDER BY pp.Tick, p.PlayerName";

                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@DemoId", demoId);
                command.Parameters.AddWithValue("@RoundNumber", adjustedRoundNumber);

                using var reader = await command.ExecuteReaderAsync();
                var spawnData = new List<Dictionary<string, object>>();

                while (await reader.ReadAsync())
                {
                    var row = new Dictionary<string, object>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        var value = reader.GetValue(i);
                        row[reader.GetName(i)] = value == DBNull.Value ? null! : value;
                    }
                    spawnData.Add(row);
                }

                return Ok(new
                {
                    Title = "Spawn Data Check",
                    DemoId = demoId,
                    RoundNumber = roundNumber,
                    AdjustedRoundNumber = adjustedRoundNumber,
                    SpawnData = spawnData,
                    TotalRecords = spawnData.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking spawn data");
                return StatusCode(500, $"Error checking spawn data: {ex.Message}");
            }
        }

        [HttpGet("verify-continuous-tracking")]
        public async Task<IActionResult> VerifyContinuousTracking([FromQuery] int demoId, [FromQuery] int roundNumber)
        {
            try
            {
                var demoSource = await GetDemoSourceAsync(demoId);
                var adjustedRoundNumber = GetDatabaseRoundNumber(roundNumber, demoSource);

                // Check tick intervals to see if positions are captured continuously or just on events
                var sql = @"
                    SELECT
                        pp.Tick,
                        COUNT(DISTINCT pp.PlayerId) as PlayerCount,
                        (pp.Tick - LAG(pp.Tick) OVER (ORDER BY pp.Tick)) as TickGap,
                        STRING_AGG(p.PlayerName, ', ') as Players
                    FROM PlayerPositions pp
                    INNER JOIN Players p ON pp.PlayerId = p.Id
                    INNER JOIN DemoFiles d ON p.DemoFileId = d.Id
                    INNER JOIN Matches m ON d.Id = m.DemoFileId
                    INNER JOIN Rounds r ON m.Id = r.MatchId
                    WHERE d.Id = @DemoId
                        AND r.RoundNumber = @RoundNumber
                        AND pp.Tick BETWEEN (r.StartTick - 500) AND (r.StartTick + 500)
                    GROUP BY pp.Tick
                    ORDER BY pp.Tick";

                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@DemoId", demoId);
                command.Parameters.AddWithValue("@RoundNumber", adjustedRoundNumber);

                using var reader = await command.ExecuteReaderAsync();
                var tickData = new List<Dictionary<string, object>>();

                while (await reader.ReadAsync())
                {
                    var row = new Dictionary<string, object>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        var value = reader.GetValue(i);
                        row[reader.GetName(i)] = value == DBNull.Value ? null! : value;
                    }
                    tickData.Add(row);
                }

                // Analyze the data to determine tracking pattern
                var analysis = new
                {
                    TotalTicks = tickData.Count,
                    TickGaps = tickData.Where(t => t["TickGap"] != null).Select(t => Convert.ToInt32(t["TickGap"])).ToList(),
                    AverageGap = tickData.Where(t => t["TickGap"] != null).Any()
                        ? tickData.Where(t => t["TickGap"] != null).Select(t => Convert.ToInt32(t["TickGap"])).Average()
                        : 0,
                    MaxGap = tickData.Where(t => t["TickGap"] != null).Any()
                        ? tickData.Where(t => t["TickGap"] != null).Select(t => Convert.ToInt32(t["TickGap"])).Max()
                        : 0,
                    IsRegularInterval = false // We'll determine this from gaps
                };

                // Check if tracking is at regular intervals (good) or sporadic (bad)
                var gaps = analysis.TickGaps;
                var isRegular = gaps.Count > 5 && gaps.All(gap => gap >= 8 && gap <= 32); // Should be every 16 ticks roughly

                return Ok(new
                {
                    Title = "Continuous Tracking Verification",
                    DemoId = demoId,
                    RoundNumber = roundNumber,
                    Analysis = new
                    {
                        TotalTicksWithData = analysis.TotalTicks,
                        AverageTickGap = analysis.AverageGap,
                        MaxTickGap = analysis.MaxGap,
                        IsRegularInterval = isRegular,
                        TrackingPattern = isRegular ? "CONTINUOUS (Good)" : "EVENT-BASED (Bad)",
                        Recommendation = isRegular ? "Position tracking is working correctly" : "Position tracking is still event-based - demo needs reparsing"
                    },
                    RawData = tickData.Take(20) // First 20 ticks for inspection
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying continuous tracking");
                return StatusCode(500, $"Error verifying tracking: {ex.Message}");
            }
        }

        [HttpGet("get-round-data")]
        public async Task<IActionResult> GetRoundData([FromQuery] int? roundId = null, [FromQuery] int? demoId = null, [FromQuery] int? roundNumber = null, [FromQuery] int? tick = null)
        {
            try
            {
                // Handle both old (roundId) and new (demoId + roundNumber) approaches
                if (demoId.HasValue && roundNumber.HasValue)
                {
                    // Get demo source for round number adjustment (same as HeatmapService)
                    var demoSource = await GetDemoSourceAsync(demoId.Value);
                    var adjustedRoundNumber = GetDatabaseRoundNumber(roundNumber.Value, demoSource);

                    _logger.LogInformation("Using HeatmapService approach: demoId={DemoId}, displayRound={DisplayRound}, databaseRound={DatabaseRound}",
                        demoId, roundNumber, adjustedRoundNumber);

                    // Use HeatmapService query pattern
                    var sql = @"
                        SELECT
                            -- Round Info (from first record)
                            r.Id as RoundId,
                            r.RoundNumber,
                            r.StartTick,
                            r.EndTick,
                            r.Duration,
                            r.WinnerTeam,
                            r.EndReason,
                            r.BombPlanted,
                            r.BombDefused,
                            r.BombExploded,
                            d.MapName,
                            d.FileName as DemoName,

                            -- Player Positions (same fields as original)
                            pp.PlayerId,
                            p.PlayerName,
                            p.Team,
                            pp.Tick,
                            pp.GameTime,
                            CAST(pp.PositionX AS FLOAT) as PositionX,
                            CAST(pp.PositionY AS FLOAT) as PositionY,
                            CAST(pp.PositionZ AS FLOAT) as PositionZ,
                            CAST(pp.ViewAngleX AS FLOAT) as ViewAngleX,
                            CAST(pp.ViewAngleY AS FLOAT) as ViewAngleY,
                            pp.IsAlive,
                            pp.Health,
                            pp.Armor,
                            pp.ActiveWeapon,
                            pp.Money,
                            pp.IsScoped,
                            pp.IsWalking,
                            pp.IsCrouching,
                            pp.IsDefusing,
                            pp.IsPlanting,
                            pp.InSmoke,
                            pp.IsBlind

                        FROM PlayerPositions pp
                        INNER JOIN Players p ON pp.PlayerId = p.Id
                        INNER JOIN DemoFiles d ON p.DemoFileId = d.Id
                        LEFT JOIN Matches m ON d.Id = m.DemoFileId
                        LEFT JOIN Rounds r ON m.Id = r.MatchId
                        WHERE d.Id = @DemoId
                            AND r.RoundNumber = @RoundNumber
                            AND pp.Tick >= (r.StartTick - 1000)  -- Include 1000 ticks before round start for spawn positions
                            AND (r.EndTick IS NULL OR pp.Tick <= r.EndTick)
                            AND (@Tick IS NULL OR pp.Tick = @Tick)
                        ORDER BY pp.Tick, p.Team, p.PlayerName";

                    using var connection = new SqlConnection(_connectionString);
                    await connection.OpenAsync();

                    using var command = new SqlCommand(sql, connection);
                    command.Parameters.AddWithValue("@DemoId", demoId);
                    command.Parameters.AddWithValue("@RoundNumber", adjustedRoundNumber);
                    command.Parameters.AddWithValue("@Tick", (object?)tick ?? DBNull.Value);

                    using var reader = await command.ExecuteReaderAsync();
                    var data = new List<Dictionary<string, object>>();

                    while (await reader.ReadAsync())
                    {
                        var row = new Dictionary<string, object>();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            var value = reader.GetValue(i);
                            row[reader.GetName(i)] = value == DBNull.Value ? null! : value;
                        }
                        data.Add(row);
                    }

                    return Ok(new
                    {
                        Title = $"Round Data - Tick {tick?.ToString() ?? "All"}",
                        DemoId = demoId ?? 0,
                        RoundNumber = roundNumber ?? 0,
                        RequestedTick = tick,
                        Data = data,
                        TotalRecords = data.Count
                    });
                }
                else if (roundId.HasValue)
                {
                    // Fallback to original approach for compatibility
                    var sql = @"
                        SELECT
                            -- Round Info
                            r.Id as RoundId,
                            r.RoundNumber,
                            r.StartTick,
                            r.EndTick,
                            r.Duration,
                            r.WinnerTeam,
                            r.EndReason,
                            r.BombPlanted,
                            r.BombDefused,
                            r.BombExploded,
                            d.MapName,
                            d.FileName as DemoName,

                            -- Player Positions
                            pp.PlayerId,
                            p.PlayerName,
                            p.Team,
                            pp.Tick,
                            pp.GameTime,
                            CAST(pp.PositionX AS FLOAT) as PositionX,
                            CAST(pp.PositionY AS FLOAT) as PositionY,
                            CAST(pp.PositionZ AS FLOAT) as PositionZ,
                            CAST(pp.ViewAngleX AS FLOAT) as ViewAngleX,
                            CAST(pp.ViewAngleY AS FLOAT) as ViewAngleY,
                            pp.IsAlive,
                            pp.Health,
                            pp.Armor,
                            pp.ActiveWeapon,
                            pp.Money,
                            pp.IsScoped,
                            pp.IsWalking,
                            pp.IsCrouching,
                            pp.IsDefusing,
                            pp.IsPlanting,
                            pp.InSmoke,
                            pp.IsBlind

                        FROM Rounds r
                        INNER JOIN Matches m ON r.MatchId = m.Id
                        INNER JOIN DemoFiles d ON m.DemoFileId = d.Id
                        INNER JOIN Players p ON p.DemoFileId = d.Id
                        INNER JOIN PlayerPositions pp ON pp.PlayerId = p.Id
                        WHERE r.Id = @RoundId
                            AND pp.Tick BETWEEN r.StartTick AND COALESCE(r.EndTick, r.StartTick + 32000)
                            AND (@Tick IS NULL OR pp.Tick = @Tick)
                        ORDER BY pp.Tick, p.Team, p.PlayerName";

                    using var connection = new SqlConnection(_connectionString);
                    await connection.OpenAsync();

                    using var command = new SqlCommand(sql, connection);
                    command.Parameters.AddWithValue("@RoundId", roundId);
                    command.Parameters.AddWithValue("@Tick", (object?)tick ?? DBNull.Value);
                    using var reader = await command.ExecuteReaderAsync();
                    var data = new List<Dictionary<string, object>>();

                    while (await reader.ReadAsync())
                    {
                        var row = new Dictionary<string, object>();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            var value = reader.GetValue(i);
                            row[reader.GetName(i)] = value == DBNull.Value ? null! : value;
                        }
                        data.Add(row);
                    }

                    return Ok(new
                    {
                        Title = $"Round Data - Tick {tick?.ToString() ?? "All"}",
                        RoundId = roundId ?? 0,
                        RequestedTick = tick,
                        Data = data,
                        TotalRecords = data.Count
                    });
                }
                else
                {
                    return BadRequest("Either roundId or (demoId + roundNumber) must be provided");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting round data - roundId: {RoundId}, demoId: {DemoId}, roundNumber: {RoundNumber}, tick: {Tick}",
                    roundId, demoId, roundNumber, tick);
                return StatusCode(500, $"Error getting round data: {ex.Message}");
            }
        }

        [HttpGet("round-events")]
        public async Task<IActionResult> GetRoundEvents([FromQuery] int roundId)
        {
            try
            {
                var sql = @"
                    WITH RoundTicks AS (
                        SELECT StartTick, EndTick
                        FROM Rounds r
                        INNER JOIN Matches m ON r.MatchId = m.Id
                        WHERE r.Id = @RoundId
                    )
                    SELECT
                        'kill' as EventType,
                        k.Tick,
                        k.GameTime,
                        killer.PlayerName as KillerName,
                        killer.Team as KillerTeam,
                        victim.PlayerName as VictimName,
                        victim.Team as VictimTeam,
                        k.Weapon,
                        k.IsHeadshot,
                        CAST(k.KillerPositionX AS FLOAT) as PositionX,
                        CAST(k.KillerPositionY AS FLOAT) as PositionY,
                        CAST(k.KillerPositionZ AS FLOAT) as PositionZ
                    FROM Kills k
                    INNER JOIN Players killer ON k.KillerId = killer.Id
                    INNER JOIN Players victim ON k.VictimId = victim.Id
                    CROSS JOIN RoundTicks rt
                    WHERE k.Tick BETWEEN rt.StartTick AND COALESCE(rt.EndTick, rt.StartTick + 32000)

                    UNION ALL

                    SELECT
                        'bomb' as EventType,
                        b.Tick,
                        b.GameTime,
                        p.PlayerName as PlayerName,
                        p.Team,
                        NULL as VictimName,
                        NULL as VictimTeam,
                        b.EventType as Weapon,
                        0 as IsHeadshot,
                        CAST(b.PositionX AS FLOAT) as PositionX,
                        CAST(b.PositionY AS FLOAT) as PositionY,
                        CAST(b.PositionZ AS FLOAT) as PositionZ
                    FROM Bombs b
                    INNER JOIN Players p ON b.PlayerId = p.Id
                    CROSS JOIN RoundTicks rt
                    WHERE b.Tick BETWEEN rt.StartTick AND COALESCE(rt.EndTick, rt.StartTick + 32000)

                    UNION ALL

                    SELECT
                        'grenade' as EventType,
                        g.ThrowTick as Tick,
                        g.ThrowTime as GameTime,
                        p.PlayerName as PlayerName,
                        p.Team,
                        NULL as VictimName,
                        NULL as VictimTeam,
                        g.GrenadeType as Weapon,
                        0 as IsHeadshot,
                        CAST(g.ThrowPositionX AS FLOAT) as PositionX,
                        CAST(g.ThrowPositionY AS FLOAT) as PositionY,
                        CAST(g.ThrowPositionZ AS FLOAT) as PositionZ
                    FROM Grenades g
                    INNER JOIN Players p ON g.PlayerId = p.Id
                    CROSS JOIN RoundTicks rt
                    WHERE g.ThrowTick BETWEEN rt.StartTick AND COALESCE(rt.EndTick, rt.StartTick + 32000)

                    ORDER BY Tick, EventType";

                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@RoundId", roundId);

                using var reader = await command.ExecuteReaderAsync();
                var events = new List<Dictionary<string, object>>();

                while (await reader.ReadAsync())
                {
                    var evt = new Dictionary<string, object>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        var value = reader.GetValue(i);
                        evt[reader.GetName(i)] = value == DBNull.Value ? null! : value;
                    }
                    events.Add(evt);
                }

                return Ok(new
                {
                    Title = "Round Events",
                    RoundId = roundId,
                    Data = events,
                    TotalRecords = events.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting round events for roundId: {RoundId}", roundId);
                return StatusCode(500, $"Error getting round events: {ex.Message}");
            }
        }
    }
}