using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text;
using System.Text.Json;
using System.Globalization;

namespace CS2DemoParserWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly string _connectionString;
        private readonly ILogger<ReportsController> _logger;

        public ReportsController(IConfiguration configuration, ILogger<ReportsController> logger)
        {
            _logger = logger;
            
            var configString = configuration.GetConnectionString("DefaultConnection");
            var envString = Environment.GetEnvironmentVariable("CONNECTION_STRING");
            
            _logger.LogInformation("Connection string from config: {ConfigString}", string.IsNullOrEmpty(configString) ? "NULL/EMPTY" : "SET");
            _logger.LogInformation("Connection string from environment: {EnvString}", string.IsNullOrEmpty(envString) ? "NULL/EMPTY" : "SET");
            
            _connectionString = configString ?? envString;
            
            if (string.IsNullOrEmpty(_connectionString))
            {
                _logger.LogError("No connection string found in configuration or environment variables");
                throw new InvalidOperationException("No connection string configured. Set CONNECTION_STRING environment variable or configure DefaultConnection in appsettings.json");
            }
            
            _logger.LogInformation("Successfully initialized ReportsController with connection string");
        }

        [HttpGet("kills")]
        public async Task<IActionResult> GetKillsReport([FromQuery] ReportQuery query)
        {
            try
            {
                var sql = @"
                    SELECT 
                        d.FileName as DemoFile,
                        d.MapName,
                        r.RoundNumber,
                        killer.PlayerName as KillerName,
                        killer.Team as KillerTeam,
                        victim.PlayerName as VictimName,
                        victim.Team as VictimTeam,
                        k.Weapon,
                        k.IsHeadshot,
                        k.KillerPositionX,
                        k.KillerPositionY,
                        k.VictimPositionX,
                        k.VictimPositionY
                    FROM Kills k
                    LEFT JOIN Players killer ON k.KillerId = killer.Id
                    INNER JOIN Players victim ON k.VictimId = victim.Id
                    INNER JOIN Rounds r ON k.RoundId = r.Id
                    INNER JOIN Matches m ON r.MatchId = m.Id
                    INNER JOIN DemoFiles d ON m.DemoFileId = d.Id
                    WHERE 1=1
                        AND (@DemoId IS NULL OR d.Id = @DemoId)
                        AND (@MapName IS NULL OR d.MapName = @MapName)
                        AND (@PlayerName IS NULL OR killer.PlayerName = @PlayerName OR victim.PlayerName = @PlayerName)
                        AND (@Team IS NULL OR killer.Team = @Team OR victim.Team = @Team)
                        AND (@StartDate IS NULL OR d.ParsedAt >= @StartDate)
                        AND (@EndDate IS NULL OR d.ParsedAt <= @EndDate)
                    ORDER BY d.FileName, r.RoundNumber";

                var data = await ExecuteReportQuery(sql, query);
                
                if (query.Format?.ToLower() == "csv")
                {
                    var csv = ConvertToCsv(data);
                    var fileName = $"kills_report_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                    Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");
                    return File(Encoding.UTF8.GetBytes(csv), "text/csv");
                }

                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating kills report");
                return StatusCode(500, "Error generating kills report");
            }
        }

        [HttpGet("economy")]
        public async Task<IActionResult> GetEconomyReport([FromQuery] ReportQuery query)
        {
            try
            {
                var sql = @"
                    SELECT 
                        d.FileName as DemoFile,
                        d.MapName,
                        e.RoundNumber,
                        p.PlayerName,
                        p.Team,
                        e.EventType,
                        e.ItemName,
                        e.ItemCost,
                        e.MoneyBefore,
                        e.MoneyAfter
                    FROM EconomyEvents e
                    INNER JOIN Players p ON e.PlayerId = p.Id
                    INNER JOIN DemoFiles d ON e.DemoFileId = d.Id
                    WHERE 1=1
                        AND (@DemoId IS NULL OR d.Id = @DemoId)
                        AND (@MapName IS NULL OR d.MapName = @MapName)
                        AND (@PlayerName IS NULL OR p.PlayerName = @PlayerName)
                        AND (@Team IS NULL OR p.Team = @Team)
                        AND (@StartDate IS NULL OR d.ParsedAt >= @StartDate)
                        AND (@EndDate IS NULL OR d.ParsedAt <= @EndDate)
                    ORDER BY d.FileName, e.RoundNumber, p.PlayerName";

                var data = await ExecuteReportQuery(sql, query);
                
                if (query.Format?.ToLower() == "csv")
                {
                    var csv = ConvertToCsv(data);
                    var fileName = $"economy_report_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                    Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");
                    return File(Encoding.UTF8.GetBytes(csv), "text/csv");
                }

                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating economy report");
                return StatusCode(500, "Error generating economy report");
            }
        }

        [HttpGet("player-stats")]
        public async Task<IActionResult> GetPlayerStatsReport([FromQuery] ReportQuery query)
        {
            try
            {
                _logger.LogInformation("Player stats report requested with query: DemoId={DemoId}, MapName={MapName}, PlayerName={PlayerName}", 
                    query.DemoId, query.MapName, query.PlayerName);

                // Test database connectivity first
                using (var testConnection = new SqlConnection(_connectionString))
                {
                    await testConnection.OpenAsync();
                    _logger.LogInformation("Database connection test successful");
                }
                var sql = @"
                    SELECT TOP 1000
                        p.PlayerName,
                        p.Team,
                        d.MapName,
                        d.FileName as DemoFile,
                        COUNT(DISTINCT k.Id) as Kills,
                        COUNT(DISTINCT kv.Id) as Deaths,
                        CASE 
                            WHEN COUNT(DISTINCT kv.Id) = 0 THEN CAST(COUNT(DISTINCT k.Id) AS FLOAT)
                            ELSE CAST(COUNT(DISTINCT k.Id) AS FLOAT) / CAST(COUNT(DISTINCT kv.Id) AS FLOAT)
                        END as KDRatio,
                        SUM(CASE WHEN k.IsHeadshot = 1 THEN 1 ELSE 0 END) as Headshots,
                        COUNT(DISTINCT wf.Id) as ShotsFired,
                        COALESCE(SUM(dm.DamageAmount), 0) as TotalDamage,
                        COALESCE(AVG(CAST(dm.DamageAmount AS FLOAT)), 0.0) as AvgDamagePerHit
                    FROM Players p
                    INNER JOIN DemoFiles d ON p.DemoFileId = d.Id
                    LEFT JOIN Kills k ON p.Id = k.KillerId
                    LEFT JOIN Kills kv ON p.Id = kv.VictimId
                    LEFT JOIN WeaponFires wf ON p.Id = wf.PlayerId
                    LEFT JOIN Damages dm ON p.Id = dm.AttackerId
                    WHERE 1=1
                        AND (@DemoId IS NULL OR d.Id = @DemoId)
                        AND (@MapName IS NULL OR d.MapName = @MapName)
                        AND (@PlayerName IS NULL OR p.PlayerName = @PlayerName)
                        AND (@Team IS NULL OR p.Team = @Team)
                        AND (@StartDate IS NULL OR d.ParsedAt >= @StartDate)
                        AND (@EndDate IS NULL OR d.ParsedAt <= @EndDate)
                    GROUP BY p.Id, p.PlayerName, p.Team, d.MapName, d.FileName
                    ORDER BY COUNT(DISTINCT k.Id) DESC, 
                        CASE 
                            WHEN COUNT(DISTINCT kv.Id) = 0 THEN CAST(COUNT(DISTINCT k.Id) AS FLOAT)
                            ELSE CAST(COUNT(DISTINCT k.Id) AS FLOAT) / CAST(COUNT(DISTINCT kv.Id) AS FLOAT)
                        END DESC";

                var data = await ExecuteReportQuery(sql, query);
                
                if (query.Format?.ToLower() == "csv")
                {
                    var csv = ConvertToCsv(data);
                    var fileName = $"player_stats_report_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                    Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");
                    return File(Encoding.UTF8.GetBytes(csv), "text/csv");
                }

                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating player stats report. Connection string configured: {HasConnection}", 
                    !string.IsNullOrEmpty(_connectionString));
                return StatusCode(500, $"Error generating player stats report: {ex.Message}");
            }
        }

        [HttpGet("rounds")]
        public async Task<IActionResult> GetRoundsReport([FromQuery] ReportQuery query)
        {
            try
            {
                var sql = @"
                    SELECT 
                        d.FileName as DemoFile,
                        d.MapName,
                        r.RoundNumber,
                        r.WinnerTeam,
                        r.EndReason,
                        r.StartTick,
                        r.EndTick,
                        r.Duration as DurationSeconds,
                        COUNT(DISTINCT k.Id) as TotalKills,
                        COUNT(DISTINCT b.Id) as BombEvents,
                        COUNT(DISTINCT g.Id) as GrenadeEvents
                    FROM Rounds r
                    INNER JOIN Matches m ON r.MatchId = m.Id
                    INNER JOIN DemoFiles d ON m.DemoFileId = d.Id
                    LEFT JOIN Kills k ON r.Id = k.RoundId
                    LEFT JOIN Bombs b ON r.Id = b.RoundId
                    LEFT JOIN Grenades g ON r.Id = g.RoundId
                    WHERE 1=1
                        AND (@DemoId IS NULL OR d.Id = @DemoId)
                        AND (@MapName IS NULL OR d.MapName = @MapName)
                        AND (@StartDate IS NULL OR d.ParsedAt >= @StartDate)
                        AND (@EndDate IS NULL OR d.ParsedAt <= @EndDate)
                    GROUP BY d.FileName, d.MapName, r.RoundNumber, r.WinnerTeam, r.EndReason, r.StartTick, r.EndTick, r.Duration, d.Id
                    ORDER BY d.FileName, r.RoundNumber";

                var data = await ExecuteReportQuery(sql, query);
                
                if (query.Format?.ToLower() == "csv")
                {
                    var csv = ConvertToCsv(data);
                    var fileName = $"rounds_report_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                    Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");
                    return File(Encoding.UTF8.GetBytes(csv), "text/csv");
                }

                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating rounds report");
                return StatusCode(500, "Error generating rounds report");
            }
        }

        [HttpGet("weapons")]
        public async Task<IActionResult> GetWeaponsReport([FromQuery] ReportQuery query)
        {
            try
            {
                var sql = @"
                    SELECT 
                        k.Weapon,
                        COUNT(*) as TotalKills,
                        SUM(CASE WHEN k.IsHeadshot = 1 THEN 1 ELSE 0 END) as HeadshotKills,
                        CAST(SUM(CASE WHEN k.IsHeadshot = 1 THEN 1 ELSE 0 END) AS FLOAT) / COUNT(*) * 100 as HeadshotPercentage,
                        COUNT(DISTINCT k.KillerId) as UniqueUsers,
                        d.MapName
                    FROM Kills k
                    INNER JOIN DemoFiles d ON k.DemoFileId = d.Id
                    WHERE 1=1
                        AND k.Weapon IS NOT NULL
                        AND (@DemoId IS NULL OR d.Id = @DemoId)
                        AND (@MapName IS NULL OR d.MapName = @MapName)
                        AND (@StartDate IS NULL OR d.ParsedAt >= @StartDate)
                        AND (@EndDate IS NULL OR d.ParsedAt <= @EndDate)
                    GROUP BY k.Weapon, d.MapName
                    ORDER BY TotalKills DESC";

                var data = await ExecuteReportQuery(sql, query);
                
                if (query.Format?.ToLower() == "csv")
                {
                    var csv = ConvertToCsv(data);
                    var fileName = $"weapons_report_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                    Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");
                    return File(Encoding.UTF8.GetBytes(csv), "text/csv");
                }

                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating weapons report");
                return StatusCode(500, "Error generating weapons report");
            }
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummaryReport([FromQuery] ReportQuery query)
        {
            try
            {
                var summaryData = new
                {
                    DemoFiles = await GetScalarResult("SELECT COUNT(*) FROM DemoFiles WHERE (@StartDate IS NULL OR ParsedAt >= @StartDate) AND (@EndDate IS NULL OR ParsedAt <= @EndDate)", query),
                    TotalMatches = await GetScalarResult("SELECT COUNT(*) FROM Matches m INNER JOIN DemoFiles d ON m.DemoFileId = d.Id WHERE (@StartDate IS NULL OR d.ParsedAt >= @StartDate) AND (@EndDate IS NULL OR d.ParsedAt <= @EndDate)", query),
                    TotalRounds = await GetScalarResult("SELECT COUNT(*) FROM Rounds r INNER JOIN Matches m ON r.MatchId = m.Id INNER JOIN DemoFiles d ON m.DemoFileId = d.Id WHERE (@StartDate IS NULL OR d.ParsedAt >= @StartDate) AND (@EndDate IS NULL OR d.ParsedAt <= @EndDate)", query),
                    TotalKills = await GetScalarResult("SELECT COUNT(*) FROM Kills k INNER JOIN Rounds r ON k.RoundId = r.Id INNER JOIN Matches m ON r.MatchId = m.Id INNER JOIN DemoFiles d ON m.DemoFileId = d.Id WHERE (@StartDate IS NULL OR d.ParsedAt >= @StartDate) AND (@EndDate IS NULL OR d.ParsedAt <= @EndDate)", query),
                    TotalPlayers = await GetScalarResult("SELECT COUNT(DISTINCT PlayerName) FROM Players p INNER JOIN DemoFiles d ON p.DemoFileId = d.Id WHERE (@StartDate IS NULL OR d.ParsedAt >= @StartDate) AND (@EndDate IS NULL OR d.ParsedAt <= @EndDate)", query),
                    MostPlayedMaps = await GetTopMaps(query),
                    TopPlayers = await GetTopPlayersByKills(query)
                };

                if (query.Format?.ToLower() == "json")
                {
                    var json = JsonSerializer.Serialize(summaryData, new JsonSerializerOptions { WriteIndented = true });
                    var fileName = $"summary_report_{DateTime.Now:yyyyMMdd_HHmmss}.json";
                    Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");
                    return File(Encoding.UTF8.GetBytes(json), "application/json");
                }

                return Ok(summaryData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating summary report");
                return StatusCode(500, "Error generating summary report");
            }
        }

        private async Task<List<Dictionary<string, object>>> ExecuteReportQuery(string sql, ReportQuery query)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            using var command = new SqlCommand(sql, connection);
            command.CommandTimeout = 120; // Reduce to 2 minutes - should be enough with optimization
            AddReportParameters(command, query);
            
            using var reader = await command.ExecuteReaderAsync();
            var results = new List<Dictionary<string, object>>();
            
            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var value = reader.GetValue(i);
                    row[reader.GetName(i)] = value == DBNull.Value ? null! : value;
                }
                results.Add(row);
            }
            
            return results;
        }

        private void AddReportParameters(SqlCommand command, ReportQuery query)
        {
            command.Parameters.AddWithValue("@DemoId", query.DemoId.HasValue ? (object)query.DemoId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@MapName", !string.IsNullOrEmpty(query.MapName) ? (object)query.MapName : DBNull.Value);
            command.Parameters.AddWithValue("@PlayerName", !string.IsNullOrEmpty(query.PlayerName) ? (object)query.PlayerName : DBNull.Value);
            command.Parameters.AddWithValue("@Team", !string.IsNullOrEmpty(query.Team) ? (object)query.Team : DBNull.Value);
            command.Parameters.AddWithValue("@StartDate", query.StartDate.HasValue ? (object)query.StartDate.Value : DBNull.Value);
            command.Parameters.AddWithValue("@EndDate", query.EndDate.HasValue ? (object)query.EndDate.Value : DBNull.Value);
        }

        private async Task<int> GetScalarResult(string sql, ReportQuery query)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            using var command = new SqlCommand(sql, connection);
            command.CommandTimeout = 120; // Reduce to 2 minutes
            AddReportParameters(command, query);
            
            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result ?? 0);
        }

        private async Task<List<object>> GetTopMaps(ReportQuery query)
        {
            var sql = @"
                SELECT TOP 10 
                    d.MapName, 
                    COUNT(*) as PlayCount 
                FROM DemoFiles d 
                WHERE (@StartDate IS NULL OR d.ParsedAt >= @StartDate) 
                    AND (@EndDate IS NULL OR d.ParsedAt <= @EndDate)
                    AND d.MapName IS NOT NULL
                GROUP BY d.MapName 
                ORDER BY PlayCount DESC";
            
            var data = await ExecuteReportQuery(sql, query);
            return data.Cast<object>().ToList();
        }

        private async Task<List<object>> GetTopPlayersByKills(ReportQuery query)
        {
            var sql = @"
                SELECT TOP 10 
                    p.PlayerName, 
                    COUNT(k.Id) as Kills,
                    COUNT(kv.Id) as Deaths,
                    CAST(COUNT(k.Id) AS FLOAT) / NULLIF(COUNT(kv.Id), 0) as KDRatio
                FROM Players p
                INNER JOIN DemoFiles d ON p.DemoFileId = d.Id
                LEFT JOIN Kills k ON p.Id = k.KillerId
                LEFT JOIN Kills kv ON p.Id = kv.VictimId
                WHERE (@StartDate IS NULL OR d.ParsedAt >= @StartDate) 
                    AND (@EndDate IS NULL OR d.ParsedAt <= @EndDate)
                GROUP BY p.PlayerName 
                HAVING COUNT(k.Id) > 0
                ORDER BY Kills DESC";
            
            var data = await ExecuteReportQuery(sql, query);
            return data.Cast<object>().ToList();
        }

        private string ConvertToCsv(List<Dictionary<string, object>> data)
        {
            if (!data.Any()) return "";
            
            var csv = new StringBuilder();
            
            // Headers
            var headers = data.First().Keys;
            csv.AppendLine(string.Join(",", headers.Select(h => $"\"{h}\"")));
            
            // Data rows
            foreach (var row in data)
            {
                var values = headers.Select(h => 
                {
                    var value = row[h]?.ToString() ?? "";
                    // Escape quotes and wrap in quotes if contains comma or quote
                    if (value.Contains(",") || value.Contains("\""))
                    {
                        value = "\"" + value.Replace("\"", "\"\"") + "\"";
                    }
                    return value;
                });
                csv.AppendLine(string.Join(",", values));
            }
            
            return csv.ToString();
        }

        // CSV Export Endpoints
        [HttpGet("kills/csv")]
        public async Task<IActionResult> GetKillsReportCsv([FromQuery] ReportQuery query)
        {
            try
            {
                var sql = @"
                    SELECT 
                        d.FileName as DemoFile,
                        d.MapName,
                        r.RoundNumber,
                        killer.PlayerName as KillerName,
                        killer.Team as KillerTeam,
                        victim.PlayerName as VictimName,
                        victim.Team as VictimTeam,
                        k.Weapon,
                        k.IsHeadshot,
                        k.KillerPositionX,
                        k.KillerPositionY,
                        k.VictimPositionX,
                        k.VictimPositionY
                    FROM Kills k
                    LEFT JOIN Players killer ON k.KillerId = killer.Id
                    INNER JOIN Players victim ON k.VictimId = victim.Id
                    INNER JOIN Rounds r ON k.RoundId = r.Id
                    INNER JOIN Matches m ON r.MatchId = m.Id
                    INNER JOIN DemoFiles d ON m.DemoFileId = d.Id
                    WHERE 1=1
                        AND (@DemoId IS NULL OR d.Id = @DemoId)
                        AND (@MapName IS NULL OR d.MapName = @MapName)
                        AND (@PlayerName IS NULL OR (killer.PlayerName = @PlayerName OR victim.PlayerName = @PlayerName))
                        AND (@Team IS NULL OR (killer.Team = @Team OR victim.Team = @Team))
                        AND (@StartDate IS NULL OR d.ParsedAt >= @StartDate)
                        AND (@EndDate IS NULL OR d.ParsedAt <= @EndDate)
                    ORDER BY d.FileName, r.RoundNumber";

                var data = await ExecuteReportQuery(sql, query);
                var csv = ConvertToCsv(data);
                
                var fileName = $"kills_report_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");
                
                return File(Encoding.UTF8.GetBytes(csv), "text/csv", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating kills CSV report");
                return StatusCode(500, "Error generating kills CSV report");
            }
        }

        [HttpGet("economy/csv")]
        public async Task<IActionResult> GetEconomyReportCsv([FromQuery] ReportQuery query)
        {
            try
            {
                var sql = @"
                    SELECT 
                        d.FileName as DemoFile,
                        d.MapName,
                        r.RoundNumber,
                        p.PlayerName,
                        p.Team,
                        e.Money,
                        e.Equipment,
                        e.TotalValue
                    FROM EconomyStates e
                    INNER JOIN Players p ON e.PlayerId = p.Id
                    INNER JOIN Rounds r ON e.RoundId = r.Id
                    INNER JOIN Matches m ON r.MatchId = m.Id
                    INNER JOIN DemoFiles d ON m.DemoFileId = d.Id
                    WHERE 1=1
                        AND (@DemoId IS NULL OR d.Id = @DemoId)
                        AND (@MapName IS NULL OR d.MapName = @MapName)
                        AND (@PlayerName IS NULL OR p.PlayerName = @PlayerName)
                        AND (@Team IS NULL OR p.Team = @Team)
                        AND (@StartDate IS NULL OR d.ParsedAt >= @StartDate)
                        AND (@EndDate IS NULL OR d.ParsedAt <= @EndDate)
                    ORDER BY d.FileName, r.RoundNumber, p.PlayerName";

                var data = await ExecuteReportQuery(sql, query);
                var csv = ConvertToCsv(data);
                
                var fileName = $"economy_report_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");
                
                return File(Encoding.UTF8.GetBytes(csv), "text/csv", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating economy CSV report");
                return StatusCode(500, "Error generating economy CSV report");
            }
        }

        [HttpGet("player-stats/csv")]
        public async Task<IActionResult> GetPlayerStatsReportCsv([FromQuery] ReportQuery query)
        {
            try
            {
                var sql = @"
                    SELECT 
                        p.PlayerName,
                        p.Team,
                        d.MapName,
                        COUNT(DISTINCT d.Id) as DemosPlayed,
                        COUNT(k.Id) as Kills,
                        COUNT(kv.Id) as Deaths,
                        CAST(COUNT(k.Id) AS FLOAT) / NULLIF(COUNT(kv.Id), 0) as KDRatio,
                        SUM(CASE WHEN k.IsHeadshot = 1 THEN 1 ELSE 0 END) as Headshots,
                        COUNT(DISTINCT k.Weapon) as UniqueWeapons
                    FROM Players p
                    INNER JOIN DemoFiles d ON p.DemoFileId = d.Id
                    LEFT JOIN Kills k ON p.Id = k.KillerId
                    LEFT JOIN Kills kv ON p.Id = kv.VictimId
                    WHERE 1=1
                        AND (@DemoId IS NULL OR d.Id = @DemoId)
                        AND (@MapName IS NULL OR d.MapName = @MapName)
                        AND (@PlayerName IS NULL OR p.PlayerName = @PlayerName)
                        AND (@Team IS NULL OR p.Team = @Team)
                        AND (@StartDate IS NULL OR d.ParsedAt >= @StartDate)
                        AND (@EndDate IS NULL OR d.ParsedAt <= @EndDate)
                    GROUP BY p.PlayerName, p.Team, d.MapName
                    ORDER BY KDRatio DESC, Kills DESC";

                var data = await ExecuteReportQuery(sql, query);
                var csv = ConvertToCsv(data);
                
                var fileName = $"player_stats_report_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");
                
                return File(Encoding.UTF8.GetBytes(csv), "text/csv", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating player stats CSV report");
                return StatusCode(500, "Error generating player stats CSV report");
            }
        }

        [HttpGet("rounds/csv")]
        public async Task<IActionResult> GetRoundsReportCsv([FromQuery] ReportQuery query)
        {
            try
            {
                var sql = @"
                    SELECT 
                        d.FileName as DemoFile,
                        d.MapName,
                        r.RoundNumber,
                        r.WinnerTeam,
                        r.RoundEndReason,
                        r.Duration,
                        r.CTScore,
                        r.TScore
                    FROM Rounds r
                    INNER JOIN Matches m ON r.MatchId = m.Id
                    INNER JOIN DemoFiles d ON m.DemoFileId = d.Id
                    WHERE 1=1
                        AND (@DemoId IS NULL OR d.Id = @DemoId)
                        AND (@MapName IS NULL OR d.MapName = @MapName)
                        AND (@StartDate IS NULL OR d.ParsedAt >= @StartDate)
                        AND (@EndDate IS NULL OR d.ParsedAt <= @EndDate)
                    ORDER BY d.FileName, r.RoundNumber";

                var data = await ExecuteReportQuery(sql, query);
                var csv = ConvertToCsv(data);
                
                var fileName = $"rounds_report_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");
                
                return File(Encoding.UTF8.GetBytes(csv), "text/csv", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating rounds CSV report");
                return StatusCode(500, "Error generating rounds CSV report");
            }
        }

        [HttpGet("weapons/csv")]
        public async Task<IActionResult> GetWeaponsReportCsv([FromQuery] ReportQuery query)
        {
            try
            {
                var sql = @"
                    SELECT 
                        d.FileName as DemoFile,
                        d.MapName,
                        k.Weapon,
                        COUNT(*) as KillCount,
                        SUM(CASE WHEN k.IsHeadshot = 1 THEN 1 ELSE 0 END) as HeadshotCount,
                        CAST(SUM(CASE WHEN k.IsHeadshot = 1 THEN 1 ELSE 0 END) AS FLOAT) / COUNT(*) * 100 as HeadshotPercentage
                    FROM Kills k
                    INNER JOIN Rounds r ON k.RoundId = r.Id
                    INNER JOIN Matches m ON r.MatchId = m.Id
                    INNER JOIN DemoFiles d ON m.DemoFileId = d.Id
                    WHERE 1=1
                        AND (@DemoId IS NULL OR d.Id = @DemoId)
                        AND (@MapName IS NULL OR d.MapName = @MapName)
                        AND (@StartDate IS NULL OR d.ParsedAt >= @StartDate)
                        AND (@EndDate IS NULL OR d.ParsedAt <= @EndDate)
                    GROUP BY d.FileName, d.MapName, k.Weapon
                    ORDER BY KillCount DESC";

                var data = await ExecuteReportQuery(sql, query);
                var csv = ConvertToCsv(data);
                
                var fileName = $"weapons_report_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");
                
                return File(Encoding.UTF8.GetBytes(csv), "text/csv", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating weapons CSV report");
                return StatusCode(500, "Error generating weapons CSV report");
            }
        }
    }

    public class ReportQuery
    {
        public int? DemoId { get; set; }
        public string? MapName { get; set; }
        public string? PlayerName { get; set; }
        public string? Team { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Format { get; set; } // "json", "csv"
    }
}