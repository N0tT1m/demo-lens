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
                        var value = reader.GetValue(i);
                        round[reader.GetName(i)] = value == DBNull.Value ? null! : value;
                    }
                    rounds.Add(round);
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

        [HttpGet("round-data")]
        public async Task<IActionResult> GetRoundData([FromQuery] int roundId, [FromQuery] int? tick = null)
        {
            try
            {
                var sql = @"
                    WITH RoundInfo AS (
                        SELECT
                            r.Id,
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
                            d.FileName as DemoName
                        FROM Rounds r
                        INNER JOIN Matches m ON r.MatchId = m.Id
                        INNER JOIN DemoFiles d ON m.DemoFileId = d.Id
                        WHERE r.Id = @RoundId
                    )
                    SELECT
                        -- Round Info
                        ri.Id as RoundId,
                        ri.RoundNumber,
                        ri.StartTick,
                        ri.EndTick,
                        ri.Duration,
                        ri.WinnerTeam,
                        ri.EndReason,
                        ri.BombPlanted,
                        ri.BombDefused,
                        ri.BombExploded,
                        ri.MapName,
                        ri.DemoName,

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

                    FROM RoundInfo ri
                    INNER JOIN PlayerPositions pp ON pp.Tick BETWEEN ri.StartTick AND COALESCE(ri.EndTick, ri.StartTick + 32000)
                    INNER JOIN Players p ON pp.PlayerId = p.Id
                    WHERE (@Tick IS NULL OR pp.Tick = @Tick)
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
                    RoundId = roundId,
                    RequestedTick = tick,
                    Data = data,
                    TotalRecords = data.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting round data for roundId: {RoundId}, tick: {Tick}", roundId, tick);
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