using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text;
using System.Text.Json;
using CS2DemoParserWeb.Models;

namespace CS2DemoParserWeb.Controllers
{
    [ApiController]
    [Route("api/advanced-analytics")]
    public class AdvancedAnalyticsController : ControllerBase
    {
        private readonly string _connectionString;
        private readonly ILogger<AdvancedAnalyticsController> _logger;

        public AdvancedAnalyticsController(IConfiguration configuration, ILogger<AdvancedAnalyticsController> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? Environment.GetEnvironmentVariable("CONNECTION_STRING")
                ?? throw new ArgumentNullException("No connection string found");
            _logger = logger;
        }

        [HttpGet("clutch-analysis")]
        public async Task<IActionResult> GetClutchAnalysis([FromQuery] AnalyticsQuery query)
        {
            try
            {
                var sql = @"
                    WITH LastKillsPerRound AS (
                        SELECT 
                            k.RoundId,
                            k.KillerId,
                            k.VictimId,
                            killer.PlayerName as KillerName,
                            killer.Team as KillerTeam,
                            victim.PlayerName as VictimName,
                            victim.Team as VictimTeam,
                            d.MapName,
                            r.WinnerTeam,
                            ROW_NUMBER() OVER (PARTITION BY k.RoundId ORDER BY k.GameTime DESC) as KillOrder
                        FROM Kills k
                        INNER JOIN Players killer ON k.KillerId = killer.Id
                        INNER JOIN Players victim ON k.VictimId = victim.Id
                        INNER JOIN Rounds r ON k.RoundId = r.Id
                        INNER JOIN DemoFiles d ON r.DemoFileId = d.Id
                        WHERE (@DemoId IS NULL OR d.Id = @DemoId)
                            AND (@MapName IS NULL OR d.MapName = @MapName)
                    ),
                    PotentialClutches AS (
                        SELECT 
                            RoundId,
                            KillerName as ClutchPlayer,
                            KillerTeam as ClutchTeam,
                            MapName,
                            WinnerTeam,
                            COUNT(*) as FinalKills,
                            CASE 
                                WHEN COUNT(*) = 2 THEN '1v2'
                                WHEN COUNT(*) = 3 THEN '1v3' 
                                WHEN COUNT(*) = 4 THEN '1v4'
                                WHEN COUNT(*) >= 5 THEN '1v5'
                                ELSE '1v1'
                            END as ClutchType
                        FROM LastKillsPerRound
                        WHERE KillOrder <= 4 -- Last few kills of the round
                        GROUP BY RoundId, KillerName, KillerTeam, MapName, WinnerTeam
                        HAVING COUNT(*) >= 2 -- At least 2 kills in final moments
                    )
                    SELECT 
                        ClutchPlayer,
                        ClutchTeam,
                        MapName,
                        ClutchType,
                        COUNT(*) as ClutchAttempts,
                        SUM(CASE WHEN ClutchTeam = WinnerTeam THEN 1 ELSE 0 END) as ClutchWins,
                        CAST(SUM(CASE WHEN ClutchTeam = WinnerTeam THEN 1 ELSE 0 END) AS FLOAT) / COUNT(*) * 100 as ClutchSuccessRate,
                        COUNT(DISTINCT RoundId) as DemosPlayed
                    FROM PotentialClutches
                    WHERE (@PlayerName IS NULL OR ClutchPlayer = @PlayerName)
                        AND (@Team IS NULL OR ClutchTeam = @Team)
                    GROUP BY ClutchPlayer, ClutchTeam, MapName, ClutchType
                    ORDER BY ClutchSuccessRate DESC, ClutchAttempts DESC"; 
                
                /*
                    WITH RoundPlayerCounts AS (
                        SELECT 
                            r.Id as RoundId,
                            r.DemoFileId,
                            r.RoundNumber,
                            r.WinnerTeam,
                            d.FileName,
                            d.MapName,
                            -- Count alive players when clutch situation starts (before final kills)
                            r.CTLivePlayers,
                            r.TLivePlayers
                        FROM Rounds r
                        INNER JOIN DemoFiles d ON r.DemoFileId = d.Id
                        WHERE (@DemoId IS NULL OR d.Id = @DemoId)
                            AND (@MapName IS NULL OR d.MapName = @MapName)
                            AND (@StartDate IS NULL OR d.ParsedAt >= @StartDate)
                            AND (@EndDate IS NULL OR d.ParsedAt <= @EndDate)
                    ),
                    ClutchSituations AS (
                        SELECT 
                            k.*,
                            rpc.RoundNumber,
                            rpc.FileName,
                            rpc.MapName,
                            rpc.WinnerTeam,
                            killer.PlayerName as ClutchPlayer,
                            killer.Team as ClutchTeam,
                            victim.PlayerName as VictimName,
                            victim.Team as VictimTeam,
                            rpc.CTLivePlayers,
                            rpc.TLivePlayers,
                            -- Determine clutch scenario
                            CASE 
                                WHEN killer.Team = 'TERRORIST' AND rpc.TLivePlayers = 1 AND rpc.CTLivePlayers > 1 
                                THEN CONCAT('1v', rpc.CTLivePlayers)
                                WHEN killer.Team = 'CT' AND rpc.CTLivePlayers = 1 AND rpc.TLivePlayers > 1 
                                THEN CONCAT('1v', rpc.TLivePlayers)
                                ELSE NULL
                            END as ClutchType,
                            -- Determine if clutch was successful (team won the round)
                            CASE 
                                WHEN killer.Team = rpc.WinnerTeam THEN 1
                                ELSE 0
                            END as ClutchSuccess
                        FROM Kills k
                        INNER JOIN RoundPlayerCounts rpc ON k.RoundId = rpc.RoundId
                        INNER JOIN Players killer ON k.KillerId = killer.Id
                        INNER JOIN Players victim ON k.VictimId = victim.Id
                        WHERE k.IsClutch = 1 OR (
                            -- Identify clutch situations from player counts
                            (killer.Team = 'TERRORIST' AND rpc.TLivePlayers = 1 AND rpc.CTLivePlayers > 1) OR
                            (killer.Team = 'CT' AND rpc.CTLivePlayers = 1 AND rpc.TLivePlayers > 1)
                        )
                    )
                    SELECT 
                        ClutchPlayer,
                        ClutchTeam,
                        MapName,
                        ClutchType,
                        COUNT(*) as ClutchAttempts,
                        SUM(ClutchSuccess) as ClutchWins,
                        CAST(SUM(ClutchSuccess) AS FLOAT) / COUNT(*) * 100 as ClutchSuccessRate,
                        AVG(CAST(ClutchSuccess AS FLOAT)) * 100 as AvgSuccessRate,
                        -- Additional context
                        COUNT(DISTINCT FileName) as DemosPlayed,
                        MIN(RoundNumber) as FirstClutchRound,
                        MAX(RoundNumber) as LastClutchRound
                    FROM ClutchSituations
                    WHERE ClutchType IS NOT NULL
                        AND (@PlayerName IS NULL OR ClutchPlayer = @PlayerName)
                        AND (@Team IS NULL OR ClutchTeam = @Team)
                    GROUP BY ClutchPlayer, ClutchTeam, MapName, ClutchType
                    ORDER BY ClutchSuccessRate DESC, ClutchAttempts DESC";
                */

                var data = await ExecuteAnalyticsQuery(sql, query);
                
                if (query.Format?.ToLower() == "csv")
                {
                    var csv = ConvertToCsv(data);
                    var fileName = $"clutch_analysis_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                    Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");
                    return File(Encoding.UTF8.GetBytes(csv), "text/csv");
                }

                return Ok(new
                {
                    Title = "Clutch Situation Analysis",
                    Description = "Analysis of 1vX clutch situations and success rates",
                    Data = data,
                    TotalRecords = data.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating clutch analysis");
                return StatusCode(500, $"Error generating clutch analysis: {ex.Message}");
            }
        }

        [HttpGet("trade-kill-analysis")]
        public async Task<IActionResult> GetTradeKillAnalysis([FromQuery] AnalyticsQuery query)
        {
            try
            {
                var sql = @"
                    WITH TradeKills AS (
                        SELECT 
                            k1.Id as InitialKillId,
                            k1.GameTime as InitialKillTime,
                            k1.Weapon as InitialWeapon,
                            initial_killer.PlayerName as InitialKiller,
                            initial_killer.Team as InitialKillerTeam,
                            initial_victim.PlayerName as InitialVictim,
                            initial_victim.Team as InitialVictimTeam,
                            k2.Id as TradeKillId,
                            k2.GameTime as TradeKillTime,
                            k2.Weapon as TradeWeapon,
                            trade_killer.PlayerName as TradeKiller,
                            trade_killer.Team as TradeKillerTeam,
                            (k2.GameTime - k1.GameTime) as TradeTimeSeconds,
                            d.FileName,
                            d.MapName,
                            r.RoundNumber,
                            -- Calculate distance between kills
                            SQRT(
                                POWER(CAST(k2.KillerPositionX - k1.VictimPositionX AS FLOAT), 2) +
                                POWER(CAST(k2.KillerPositionY - k1.VictimPositionY AS FLOAT), 2)
                            ) as TradeDistance
                        FROM Kills k1
                        INNER JOIN Kills k2 ON k1.RoundId = k2.RoundId 
                            AND k2.GameTime > k1.GameTime 
                            AND k2.GameTime <= k1.GameTime + 5.0 -- Trades within 5 seconds
                        INNER JOIN Players initial_killer ON k1.KillerId = initial_killer.Id
                        INNER JOIN Players initial_victim ON k1.VictimId = initial_victim.Id
                        INNER JOIN Players trade_killer ON k2.KillerId = trade_killer.Id
                        INNER JOIN Players trade_victim ON k2.VictimId = trade_victim.Id
                        INNER JOIN Rounds r ON k1.RoundId = r.Id
                        INNER JOIN DemoFiles d ON r.DemoFileId = d.Id
                        WHERE 
                            -- Trade kill is revenge (same teams involved, opposite roles)
                            initial_killer.Team != trade_killer.Team
                            AND initial_victim.Team = trade_killer.Team
                            AND trade_victim.Id = initial_killer.Id
                            AND (@DemoId IS NULL OR d.Id = @DemoId)
                            AND (@MapName IS NULL OR d.MapName = @MapName)
                            AND (@PlayerName IS NULL OR initial_victim.PlayerName = @PlayerName OR trade_killer.PlayerName = @PlayerName)
                            AND (@StartDate IS NULL OR d.ParsedAt >= @StartDate)
                            AND (@EndDate IS NULL OR d.ParsedAt <= @EndDate)
                    )
                    SELECT 
                        TradeKiller as Player,
                        TradeKillerTeam as Team,
                        MapName,
                        COUNT(*) as TotalTrades,
                        AVG(TradeTimeSeconds) as AvgTradeTimeSeconds,
                        MIN(TradeTimeSeconds) as FastestTradeSeconds,
                        MAX(TradeTimeSeconds) as SlowestTradeSeconds,
                        AVG(TradeDistance) as AvgTradeDistance,
                        COUNT(DISTINCT FileName) as DemosPlayed,
                        -- Trade efficiency metrics
                        COUNT(CASE WHEN TradeTimeSeconds <= 2.0 THEN 1 END) as FastTrades,
                        COUNT(CASE WHEN TradeTimeSeconds <= 2.0 THEN 1 END) * 100.0 / COUNT(*) as FastTradePercentage
                    FROM TradeKills
                    WHERE (@Team IS NULL OR TradeKillerTeam = @Team)
                    GROUP BY TradeKiller, TradeKillerTeam, MapName
                    ORDER BY TotalTrades DESC, AvgTradeTimeSeconds ASC";

                var data = await ExecuteAnalyticsQuery(sql, query);
                
                if (query.Format?.ToLower() == "csv")
                {
                    var csv = ConvertToCsv(data);
                    var fileName = $"trade_kill_analysis_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                    Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");
                    return File(Encoding.UTF8.GetBytes(csv), "text/csv");
                }

                return Ok(new
                {
                    Title = "Trade Kill Analysis",
                    Description = "Analysis of how quickly teams trade frags after losing a teammate",
                    Data = data,
                    TotalRecords = data.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating trade kill analysis");
                return StatusCode(500, $"Error generating trade kill analysis: {ex.Message}");
            }
        }

        [HttpGet("first-kill-impact")]
        public async Task<IActionResult> GetFirstKillImpact([FromQuery] AnalyticsQuery query)
        {
            try
            {
                var sql = @"
                    WITH RoundFirstKills AS (
                        SELECT 
                            k.RoundId,
                            k.GameTime,
                            killer.PlayerName as FirstKiller,
                            killer.Team as FirstKillerTeam,
                            k.Weapon,
                            k.IsHeadshot,
                            r.WinnerTeam,
                            d.FileName,
                            d.MapName,
                            -- Determine if first kill team won the round
                            CASE WHEN killer.Team = r.WinnerTeam THEN 1 ELSE 0 END as FirstKillTeamWon,
                            -- Categorize the first kill
                            CASE 
                                WHEN k.IsHeadshot = 1 THEN 'Headshot'
                                WHEN k.Weapon LIKE '%awp%' THEN 'AWP Pick'
                                WHEN k.Distance > 1500 THEN 'Long Range'
                                ELSE 'Standard'
                            END as FirstKillType,
                            ROW_NUMBER() OVER (PARTITION BY k.RoundId ORDER BY k.GameTime ASC) as KillOrder
                        FROM Kills k
                        INNER JOIN Players killer ON k.KillerId = killer.Id
                        INNER JOIN Rounds r ON k.RoundId = r.Id
                        INNER JOIN DemoFiles d ON r.DemoFileId = d.Id
                        WHERE killer.PlayerName IS NOT NULL
                            AND (@DemoId IS NULL OR d.Id = @DemoId)
                            AND (@MapName IS NULL OR d.MapName = @MapName)
                            AND (@PlayerName IS NULL OR killer.PlayerName = @PlayerName)
                    ),
                    FirstKills AS (
                        SELECT * FROM RoundFirstKills WHERE KillOrder = 1
                    )
                    SELECT 
                        FirstKiller,
                        FirstKillerTeam,
                        MapName,
                        FirstKillType,
                        COUNT(*) as TotalFirstKills,
                        SUM(FirstKillTeamWon) as RoundsWonAfterFirstKill,
                        CAST(SUM(FirstKillTeamWon) AS FLOAT) / COUNT(*) * 100 as FirstKillWinPercentage,
                        COUNT(CASE WHEN IsHeadshot = 1 THEN 1 END) as HeadshotFirstKills,
                        COUNT(CASE WHEN IsHeadshot = 1 THEN 1 END) * 100.0 / COUNT(*) as HeadshotPercentage,
                        COUNT(DISTINCT FileName) as DemosPlayed,
                        AVG(GameTime) as AvgFirstKillTime
                    FROM FirstKills
                    WHERE (@Team IS NULL OR FirstKillerTeam = @Team)
                    GROUP BY FirstKiller, FirstKillerTeam, MapName, FirstKillType
                    HAVING COUNT(*) >= 1 -- Show all first kills
                    ORDER BY FirstKillWinPercentage DESC, TotalFirstKills DESC";

                var data = await ExecuteAnalyticsQuery(sql, query);
                
                if (query.Format?.ToLower() == "csv")
                {
                    var csv = ConvertToCsv(data);
                    var fileName = $"first_kill_impact_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                    Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");
                    return File(Encoding.UTF8.GetBytes(csv), "text/csv");
                }

                return Ok(new
                {
                    Title = "First Kill Impact Analysis",
                    Description = "Analysis of how first kills in rounds correlate with round wins",
                    Data = data,
                    TotalRecords = data.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating first kill impact analysis");
                return StatusCode(500, $"Error generating first kill impact analysis: {ex.Message}");
            }
        }

        [HttpGet("economic-intelligence")]
        public async Task<IActionResult> GetEconomicIntelligence([FromQuery] AnalyticsQuery query)
        {
            try
            {
                var sql = @"
                    WITH EconomicData AS (
                        SELECT 
                            p.PlayerName,
                            p.Team,
                            d.MapName,
                            r.RoundNumber,
                            r.WinnerTeam,
                            
                            -- WEAPON ECONOMY ANALYSIS BASED ON KILLS
                            k.Weapon,
                            CASE 
                                WHEN k.Weapon LIKE '%ak47%' OR k.Weapon LIKE '%m4a%' THEN 'High_Value_Rifle'
                                WHEN k.Weapon LIKE '%awp%' OR k.Weapon LIKE '%ssg08%' THEN 'Sniper_Investment'
                                WHEN k.Weapon LIKE '%glock%' OR k.Weapon LIKE '%usp%' OR k.Weapon LIKE '%p250%' THEN 'Pistol_Economy'
                                WHEN k.Weapon LIKE '%mp%' OR k.Weapon LIKE '%mac10%' OR k.Weapon LIKE '%ump%' THEN 'SMG_Force'
                                WHEN k.Weapon LIKE '%deagle%' OR k.Weapon LIKE '%r8%' THEN 'Pistol_Force'
                                ELSE 'Other_Equipment'
                            END as EconomyType,
                            
                            -- ROUND TYPE CLASSIFICATION
                            CASE 
                                WHEN r.RoundNumber <= 3 OR r.RoundNumber = 16 OR r.RoundNumber = 19 THEN 'Pistol_Round'
                                WHEN r.RoundNumber BETWEEN 4 AND 6 OR r.RoundNumber BETWEEN 20 AND 22 THEN 'Anti_Eco_Round'
                                WHEN r.RoundNumber BETWEEN 13 AND 15 OR r.RoundNumber BETWEEN 28 AND 30 THEN 'Late_Round'
                                ELSE 'Buy_Round'
                            END as RoundType,
                            
                            -- PERFORMANCE METRICS
                            prs.Damage as RoundDamage,
                            prs.Kills as RoundKills,
                            prs.Deaths as RoundDeaths,
                            CASE WHEN p.Team = r.WinnerTeam THEN 1 ELSE 0 END as RoundWon,
                            
                            -- ECONOMY EFFECTIVENESS
                            CASE WHEN k.Id IS NOT NULL THEN 1 ELSE 0 END as KillMade,
                            k.Distance as KillDistance
                            
                        FROM Players p
                        INNER JOIN PlayerRoundStats prs ON p.Id = prs.PlayerId
                        INNER JOIN Rounds r ON prs.RoundId = r.Id
                        INNER JOIN DemoFiles d ON r.DemoFileId = d.Id
                        LEFT JOIN Kills k ON p.Id = k.KillerId AND k.RoundId = r.Id";

                // Apply filters
                if (!string.IsNullOrEmpty(query.MapName))
                    sql += " AND d.MapName = @MapName";
                if (!string.IsNullOrEmpty(query.PlayerName))
                    sql += " AND p.PlayerName = @PlayerName";
                if (!string.IsNullOrEmpty(query.Team))
                    sql += " AND p.Team = @Team";
                if (query.RoundNumber.HasValue)
                    sql += " AND r.RoundNumber = @RoundNumber";

                sql += @"
                    )
                    SELECT 
                        PlayerName,
                        Team,
                        MapName,
                        EconomyType,
                        RoundType,
                        
                        -- ECONOMIC PERFORMANCE METRICS
                        COUNT(*) as RoundsWithEconomyType,
                        SUM(KillMade) as KillsWithEconomyType,
                        ROUND(SUM(KillMade) * 100.0 / COUNT(*), 2) as KillRateWithEconomyType,
                        ROUND(AVG(RoundDamage), 2) as AvgDamageWithEconomyType,
                        
                        -- ECONOMIC SUCCESS METRICS
                        SUM(RoundWon) as RoundsWonWithEconomyType,
                        ROUND(SUM(RoundWon) * 100.0 / COUNT(*), 2) as WinRateWithEconomyType,
                        
                        -- WEAPON EFFECTIVENESS
                        COUNT(DISTINCT Weapon) as WeaponVariety,
                        ROUND(AVG(KillDistance), 2) as AvgKillDistanceWithEconomy,
                        
                        -- ECONOMIC INTELLIGENCE SCORE
                        CASE 
                            WHEN EconomyType = 'High_Value_Rifle' AND ROUND(SUM(RoundWon) * 100.0 / COUNT(*), 2) >= 70 THEN 95
                            WHEN EconomyType = 'Pistol_Economy' AND ROUND(SUM(KillMade) * 100.0 / COUNT(*), 2) >= 40 THEN 85
                            WHEN EconomyType = 'SMG_Force' AND ROUND(SUM(RoundWon) * 100.0 / COUNT(*), 2) >= 60 THEN 80
                            WHEN ROUND(SUM(RoundWon) * 100.0 / COUNT(*), 2) >= 60 THEN 75
                            WHEN ROUND(SUM(KillMade) * 100.0 / COUNT(*), 2) >= 30 THEN 60
                            ELSE 40
                        END as EconomicIntelligenceScore
                        
                    FROM EconomicData
                    GROUP BY PlayerName, Team, MapName, EconomyType, RoundType
                    HAVING COUNT(*) >= 2
                    ORDER BY EconomicIntelligenceScore DESC, WinRateWithEconomyType DESC";

                var data = await ExecuteAnalyticsQuery(sql, query);
                
                if (query.Format?.ToLower() == "csv")
                {
                    var csv = ConvertToCsv(data);
                    var fileName = $"economic_intelligence_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                    Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");
                    return File(Encoding.UTF8.GetBytes(csv), "text/csv");
                }

                return Ok(new
                {
                    Title = "Economic Intelligence Analysis",
                    Description = "Analysis of round outcomes based on economic situations and spending patterns",
                    Data = data,
                    TotalRecords = data.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating economic intelligence");
                return StatusCode(500, $"Error generating economic intelligence: {ex.Message}");
            }
        }

        [HttpGet("position-analysis")]
        public async Task<IActionResult> GetPositionAnalysis([FromQuery] AnalyticsQuery query)
        {
            try
            {
                var sql = @"
                    WITH DeathHotspots AS (
                        SELECT 
                            d.MapName,
                            k.VictimPositionX as DeathX,
                            k.VictimPositionY as DeathY,
                            k.VictimPositionZ as DeathZ,
                            victim.PlayerName as VictimName,
                            victim.Team as VictimTeam,
                            k.Weapon as KillerWeapon,
                            k.IsHeadshot,
                            k.Distance,
                            -- Categorize map areas (simplified - you could expand this)
                            CASE 
                                WHEN d.MapName = 'de_dust2' THEN
                                    CASE 
                                        WHEN k.VictimPositionY > 1000 THEN 'A Site'
                                        WHEN k.VictimPositionY < -1000 THEN 'B Site'
                                        WHEN k.VictimPositionX > 0 THEN 'Upper Tunnels'
                                        ELSE 'Mid/Lower'
                                    END
                                WHEN d.MapName = 'de_mirage' THEN
                                    CASE 
                                        WHEN k.VictimPositionX > 1000 THEN 'A Site'
                                        WHEN k.VictimPositionX < -1000 THEN 'B Site'
                                        WHEN ABS(k.VictimPositionX) < 500 THEN 'Mid'
                                        ELSE 'Connector'
                                    END
                                ELSE 'Unknown Area'
                            END as MapArea
                        FROM Kills k
                        INNER JOIN Players victim ON k.VictimId = victim.Id
                        INNER JOIN DemoFiles d ON k.DemoFileId = d.Id
                        WHERE k.VictimPositionX != 0 AND k.VictimPositionY != 0 -- Valid positions only
                            AND (@DemoId IS NULL OR d.Id = @DemoId)
                            AND (@MapName IS NULL OR d.MapName = @MapName)
                            AND (@PlayerName IS NULL OR victim.PlayerName = @PlayerName)
                            AND (@StartDate IS NULL OR d.ParsedAt >= @StartDate)
                            AND (@EndDate IS NULL OR d.ParsedAt <= @EndDate)
                    )
                    SELECT 
                        MapName,
                        MapArea,
                        VictimTeam as Team,
                        COUNT(*) as TotalDeaths,
                        AVG(Distance) as AvgKillDistance,
                        COUNT(CASE WHEN IsHeadshot = 1 THEN 1 END) as HeadshotDeaths,
                        COUNT(CASE WHEN IsHeadshot = 1 THEN 1 END) * 100.0 / COUNT(*) as HeadshotPercentage,
                        -- Most common weapons used for kills in this area
                        STRING_AGG(KillerWeapon, ',') WITHIN GROUP (ORDER BY KillerWeapon) as CommonWeapons,
                        -- Position clustering (simplified)
                        AVG(DeathX) as AvgDeathX,
                        AVG(DeathY) as AvgDeathY,
                        AVG(DeathZ) as AvgDeathZ,
                        STDEV(DeathX) as DeathSpreadX,
                        STDEV(DeathY) as DeathSpreadY
                    FROM DeathHotspots
                    WHERE MapArea != 'Unknown Area'
                        AND (@Team IS NULL OR VictimTeam = @Team)
                    GROUP BY MapName, MapArea, VictimTeam
                    HAVING COUNT(*) >= 5 -- Only areas with at least 5 deaths
                    ORDER BY MapName, MapArea, TotalDeaths DESC";

                var data = await ExecuteAnalyticsQuery(sql, query);
                
                if (query.Format?.ToLower() == "csv")
                {
                    var csv = ConvertToCsv(data);
                    var fileName = $"position_analysis_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                    Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");
                    return File(Encoding.UTF8.GetBytes(csv), "text/csv");
                }

                return Ok(new
                {
                    Title = "Position Analysis",
                    Description = "Analysis of death hotspots and dangerous areas by map location",
                    Data = data,
                    TotalRecords = data.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating position analysis");
                return StatusCode(500, $"Error generating position analysis: {ex.Message}");
            }
        }

        [HttpGet("enhanced-trade-kill-optimization")]
        public async Task<IActionResult> GetEnhancedTradeKillOptimization([FromQuery] AnalyticsQuery query)
        {
            try
            {
                var sql = @"
                    WITH TradeKillAnalysis AS (
                        SELECT 
                            k1.Id as InitialKillId,
                            k1.GameTime as InitialKillTime,
                            k1.Weapon as InitialWeapon,
                            k1.KillerPositionX as InitialKillerX,
                            k1.KillerPositionY as InitialKillerY,
                            k1.VictimPositionX as InitialVictimX,
                            k1.VictimPositionY as InitialVictimY,
                            initial_killer.PlayerName as InitialKiller,
                            initial_killer.Team as InitialKillerTeam,
                            initial_victim.PlayerName as InitialVictim,
                            initial_victim.Team as InitialVictimTeam,
                            k2.Id as TradeKillId,
                            k2.GameTime as TradeKillTime,
                            k2.Weapon as TradeWeapon,
                            k2.KillerPositionX as TradeKillerX,
                            k2.KillerPositionY as TradeKillerY,
                            trade_killer.PlayerName as TradeKiller,
                            trade_killer.Team as TradeKillerTeam,
                            trade_victim.PlayerName as TradeVictim,
                            (k2.GameTime - k1.GameTime) as TradeTimeSeconds,
                            d.FileName,
                            d.MapName,
                            r.RoundNumber,
                            r.Id as RoundId,
                            -- Calculate distances
                            SQRT(
                                POWER(CAST(k2.KillerPositionX - k1.VictimPositionX AS FLOAT), 2) +
                                POWER(CAST(k2.KillerPositionY - k1.VictimPositionY AS FLOAT), 2)
                            ) as TradeDistance,
                            -- Calculate how far trade killer moved
                            SQRT(
                                POWER(CAST(k2.KillerPositionX - k1.KillerPositionX AS FLOAT), 2) +
                                POWER(CAST(k2.KillerPositionY - k1.KillerPositionY AS FLOAT), 2)
                            ) as TraderMovementDistance,
                            -- Categorize map areas for initial kill
                            CASE 
                                WHEN d.MapName = 'de_dust2' THEN
                                    CASE 
                                        WHEN k1.VictimPositionY > 1000 THEN 'A Site'
                                        WHEN k1.VictimPositionY < -1000 THEN 'B Site'
                                        WHEN k1.VictimPositionX > 0 THEN 'Upper Tunnels'
                                        ELSE 'Mid/Lower'
                                    END
                                WHEN d.MapName = 'de_mirage' THEN
                                    CASE 
                                        WHEN k1.VictimPositionX > 1000 THEN 'A Site'
                                        WHEN k1.VictimPositionX < -1000 THEN 'B Site'
                                        WHEN ABS(k1.VictimPositionX) < 500 THEN 'Mid'
                                        ELSE 'Connector'
                                    END
                                ELSE 'Unknown Area'
                            END as TradeArea,
                            -- Trade quality assessment
                            CASE 
                                WHEN (k2.GameTime - k1.GameTime) <= 1.0 THEN 'Instant'
                                WHEN (k2.GameTime - k1.GameTime) <= 3.0 THEN 'Fast'
                                WHEN (k2.GameTime - k1.GameTime) <= 5.0 THEN 'Slow'
                                ELSE 'Too Late'
                            END as TradeQuality
                        FROM Kills k1
                        INNER JOIN Kills k2 ON k1.RoundId = k2.RoundId 
                            AND k2.GameTime > k1.GameTime 
                            AND k2.GameTime <= k1.GameTime + 8.0 -- Extended trade window
                        INNER JOIN Players initial_killer ON k1.KillerId = initial_killer.Id
                        INNER JOIN Players initial_victim ON k1.VictimId = initial_victim.Id
                        INNER JOIN Players trade_killer ON k2.KillerId = trade_killer.Id
                        INNER JOIN Players trade_victim ON k2.VictimId = trade_victim.Id
                        INNER JOIN Rounds r ON k1.RoundId = r.Id
                        INNER JOIN DemoFiles d ON r.DemoFileId = d.Id
                        WHERE 
                            -- Trade kill is revenge (opposite teams, killer becomes victim)
                            initial_killer.Team != trade_killer.Team
                            AND initial_victim.Team = trade_killer.Team
                            AND trade_victim.Id = initial_killer.Id
                            AND (@DemoId IS NULL OR d.Id = @DemoId)
                            AND (@MapName IS NULL OR d.MapName = @MapName)
                            AND (@PlayerName IS NULL OR trade_killer.PlayerName = @PlayerName)
                            AND (@StartDate IS NULL OR d.ParsedAt >= @StartDate)
                            AND (@EndDate IS NULL OR d.ParsedAt <= @EndDate)
                    ),
                    TradeChainAnalysis AS (
                        -- Identify trade chains (multiple trades in sequence)
                        SELECT 
                            t1.*,
                            COUNT(t2.TradeKillId) as ChainLength
                        FROM TradeKillAnalysis t1
                        LEFT JOIN TradeKillAnalysis t2 ON t1.RoundId = t2.RoundId 
                            AND t2.TradeKillTime > t1.TradeKillTime 
                            AND t2.TradeKillTime <= t1.TradeKillTime + 3.0
                        GROUP BY t1.TradeKillId, t1.InitialKillId, t1.InitialKillTime, t1.InitialWeapon, t1.InitialKillerX, t1.InitialKillerY, 
                                 t1.InitialVictimX, t1.InitialVictimY, t1.InitialKiller, t1.InitialKillerTeam, t1.InitialVictim, t1.InitialVictimTeam, 
                                 t1.TradeKiller, t1.TradeKillerTeam, t1.TradeVictim, t1.TradeKillTime, t1.TradeWeapon, t1.TradeKillerX, t1.TradeKillerY,
                                 t1.TradeTimeSeconds, t1.FileName, t1.MapName, t1.RoundNumber, t1.RoundId, t1.TradeDistance, t1.TraderMovementDistance, 
                                 t1.TradeArea, t1.TradeQuality
                    ),
                    FailedTradeOpportunities AS (
                        -- Identify situations where trades could have happened but didn't
                        SELECT 
                            k1.RoundId,
                            k1.GameTime as MissedTradeTime,
                            initial_killer.PlayerName as UntragedKiller,
                            initial_killer.Team as UntragedKillerTeam,
                            initial_victim.PlayerName as UntragedVictim,
                            initial_victim.Team as UntragedVictimTeam,
                            d.MapName,
                            r.RoundNumber,
                            CASE 
                                WHEN d.MapName = 'de_dust2' THEN
                                    CASE 
                                        WHEN k1.VictimPositionY > 1000 THEN 'A Site'
                                        WHEN k1.VictimPositionY < -1000 THEN 'B Site'
                                        WHEN k1.VictimPositionX > 0 THEN 'Upper Tunnels'
                                        ELSE 'Mid/Lower'
                                    END
                                WHEN d.MapName = 'de_mirage' THEN
                                    CASE 
                                        WHEN k1.VictimPositionX > 1000 THEN 'A Site'
                                        WHEN k1.VictimPositionX < -1000 THEN 'B Site'
                                        WHEN ABS(k1.VictimPositionX) < 500 THEN 'Mid'
                                        ELSE 'Connector'
                                    END
                                ELSE 'Unknown Area'
                            END as MissedTradeArea
                        FROM Kills k1
                        INNER JOIN Players initial_killer ON k1.KillerId = initial_killer.Id
                        INNER JOIN Players initial_victim ON k1.VictimId = initial_victim.Id
                        INNER JOIN Rounds r ON k1.RoundId = r.Id
                        INNER JOIN DemoFiles d ON r.DemoFileId = d.Id
                        WHERE NOT EXISTS (
                            -- No trade kill happened within 5 seconds
                            SELECT 1 FROM Kills k2
                            INNER JOIN Players revenge_killer ON k2.KillerId = revenge_killer.Id
                            WHERE k2.RoundId = k1.RoundId
                                AND k2.GameTime > k1.GameTime 
                                AND k2.GameTime <= k1.GameTime + 5.0
                                AND k2.VictimId = k1.KillerId
                                AND revenge_killer.Team = initial_victim.Team
                        )
                        AND (@DemoId IS NULL OR d.Id = @DemoId)
                        AND (@MapName IS NULL OR d.MapName = @MapName)
                        AND (@PlayerName IS NULL OR initial_victim.PlayerName = @PlayerName)
                        AND (@StartDate IS NULL OR d.ParsedAt >= @StartDate)
                        AND (@EndDate IS NULL OR d.ParsedAt <= @EndDate)
                    )
                    SELECT 
                        -- Main trade performance metrics
                        TradeKiller as Player,
                        TradeKillerTeam as Team,
                        MapName,
                        TradeArea,
                        COUNT(*) as TotalTrades,
                        AVG(TradeTimeSeconds) as AvgTradeTimeSeconds,
                        MIN(TradeTimeSeconds) as FastestTradeSeconds,
                        MAX(TradeTimeSeconds) as SlowestTradeSeconds,
                        AVG(TradeDistance) as AvgTradeDistance,
                        AVG(TraderMovementDistance) as AvgTraderMovementDistance,
                        COUNT(DISTINCT MapName) as MapsTraded,
                        -- Trade quality breakdown
                        COUNT(CASE WHEN TradeQuality = 'Instant' THEN 1 END) as InstantTrades,
                        COUNT(CASE WHEN TradeQuality = 'Fast' THEN 1 END) as FastTrades,
                        COUNT(CASE WHEN TradeQuality = 'Slow' THEN 1 END) as SlowTrades,
                        COUNT(CASE WHEN TradeQuality = 'Too Late' THEN 1 END) as TooLateTrades,
                        -- Trade quality percentages
                        COUNT(CASE WHEN TradeQuality IN ('Instant', 'Fast') THEN 1 END) * 100.0 / COUNT(*) as EffectiveTradePercentage,
                        -- Trade chain analysis
                        AVG(CAST(ChainLength AS FLOAT)) as AvgTradeChainLength,
                        COUNT(CASE WHEN ChainLength >= 2 THEN 1 END) as TradeChains,
                        -- Positioning analysis
                        COUNT(CASE WHEN TradeDistance <= 500 THEN 1 END) as CloseRangeTrades,
                        COUNT(CASE WHEN TradeDistance > 1500 THEN 1 END) as LongRangeTrades,
                        COUNT(CASE WHEN TraderMovementDistance <= 100 THEN 1 END) as StaticTrades,
                        COUNT(CASE WHEN TraderMovementDistance > 500 THEN 1 END) as MobileTrades
                    FROM TradeChainAnalysis
                    WHERE (@Team IS NULL OR TradeKillerTeam = @Team)
                        AND TradeArea != 'Unknown Area'
                    GROUP BY TradeKiller, TradeKillerTeam, MapName, TradeArea
                    HAVING COUNT(*) >= 3 -- Only show players with at least 3 trades per area
                    ORDER BY EffectiveTradePercentage DESC, TotalTrades DESC";

                var data = await ExecuteAnalyticsQuery(sql, query);
                
                if (query.Format?.ToLower() == "csv")
                {
                    var csv = ConvertToCsv(data);
                    var fileName = $"enhanced_trade_kill_optimization_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                    Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");
                    return File(Encoding.UTF8.GetBytes(csv), "text/csv");
                }

                return Ok(new
                {
                    Title = "Enhanced Trade Kill Optimization",
                    Description = "Advanced analysis of trade kill efficiency including positioning, timing, chains, and missed opportunities",
                    Data = data,
                    TotalRecords = data.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating enhanced trade kill optimization");
                return StatusCode(500, $"Error generating enhanced trade kill optimization: {ex.Message}");
            }
        }

        [HttpGet("weapon-mastery-analytics")]
        public async Task<IActionResult> GetWeaponMasteryAnalytics([FromQuery] AnalyticsQuery query)
        {
            try
            {
                var sql = @"
                    WITH WeaponPerformanceData AS (
                        SELECT 
                            p.PlayerName,
                            p.Team,
                            d.MapName,
                            k.Weapon,
                            k.WeaponClass,
                            k.Distance,
                            k.IsHeadshot,
                            k.IsWallbang,
                            k.IsNoScope,
                            k.Damage,
                            wf.Id as ShotId,
                            -- Categorize weapon types more specifically
                            CASE 
                                WHEN k.Weapon LIKE '%ak47%' THEN 'AK-47'
                                WHEN k.Weapon LIKE '%m4a4%' OR k.Weapon LIKE '%m4a1%' THEN 'M4'
                                WHEN k.Weapon LIKE '%awp%' THEN 'AWP'
                                WHEN k.Weapon LIKE '%deagle%' THEN 'Deagle'
                                WHEN k.Weapon LIKE '%glock%' THEN 'Glock'
                                WHEN k.Weapon LIKE '%usp%' THEN 'USP'
                                WHEN k.Weapon LIKE '%p250%' THEN 'P250'
                                WHEN k.WeaponClass = 'Rifle' THEN 'Other Rifle'
                                WHEN k.WeaponClass = 'Pistol' THEN 'Other Pistol'
                                WHEN k.WeaponClass = 'SMG' THEN 'SMG'
                                WHEN k.WeaponClass = 'Shotgun' THEN 'Shotgun'
                                WHEN k.WeaponClass = 'Sniper' THEN 'Sniper'
                                ELSE 'Other'
                            END as WeaponCategory,
                            -- Distance categorization
                            CASE 
                                WHEN k.Distance <= 300 THEN 'Close'
                                WHEN k.Distance <= 800 THEN 'Medium'
                                WHEN k.Distance <= 1500 THEN 'Long'
                                ELSE 'Very Long'
                            END as EngagementRange,
                            r.RoundNumber
                        FROM Kills k
                        INNER JOIN Players p ON k.KillerId = p.Id
                        INNER JOIN Rounds r ON k.RoundId = r.Id
                        INNER JOIN DemoFiles d ON r.DemoFileId = d.Id
                        LEFT JOIN WeaponFires wf ON wf.PlayerId = p.Id AND wf.RoundId = r.Id 
                            AND ABS(wf.GameTime - k.GameTime) <= 0.5 -- Match shots to kills within 0.5 seconds
                        WHERE k.Weapon IS NOT NULL 
                            AND k.WeaponClass IS NOT NULL
                            AND (@DemoId IS NULL OR d.Id = @DemoId)
                            AND (@MapName IS NULL OR d.MapName = @MapName)
                            AND (@PlayerName IS NULL OR p.PlayerName = @PlayerName)
                            AND (@StartDate IS NULL OR d.ParsedAt >= @StartDate)
                            AND (@EndDate IS NULL OR d.ParsedAt <= @EndDate)
                    ),
                    WeaponFireData AS (
                        SELECT 
                            p.PlayerName,
                            p.Team,
                            d.MapName,
                            wf.Weapon,
                            COUNT(*) as TotalShots,
                            -- Categorize weapon types
                            CASE 
                                WHEN wf.Weapon LIKE '%ak47%' THEN 'AK-47'
                                WHEN wf.Weapon LIKE '%m4a4%' OR wf.Weapon LIKE '%m4a1%' THEN 'M4'
                                WHEN wf.Weapon LIKE '%awp%' THEN 'AWP'
                                WHEN wf.Weapon LIKE '%deagle%' THEN 'Deagle'
                                WHEN wf.Weapon LIKE '%glock%' THEN 'Glock'
                                WHEN wf.Weapon LIKE '%usp%' THEN 'USP'
                                WHEN wf.Weapon LIKE '%p250%' THEN 'P250'
                                ELSE 'Other'
                            END as WeaponCategory
                        FROM WeaponFires wf
                        INNER JOIN Players p ON wf.PlayerId = p.Id
                        INNER JOIN Rounds r ON wf.RoundId = r.Id
                        INNER JOIN DemoFiles d ON r.DemoFileId = d.Id
                        WHERE wf.Weapon IS NOT NULL
                            AND (@DemoId IS NULL OR d.Id = @DemoId)
                            AND (@MapName IS NULL OR d.MapName = @MapName)
                            AND (@PlayerName IS NULL OR p.PlayerName = @PlayerName)
                            AND (@StartDate IS NULL OR d.ParsedAt >= @StartDate)
                            AND (@EndDate IS NULL OR d.ParsedAt <= @EndDate)
                        GROUP BY p.PlayerName, p.Team, d.MapName, wf.Weapon
                    ),
                    WeaponSwitchingPatterns AS (
                        SELECT 
                            p.PlayerName,
                            p.Team,
                            d.MapName,
                            r.RoundNumber,
                            e1.ItemName as PrimaryWeapon,
                            e2.ItemName as SecondaryWeapon,
                            ABS(e2.Tick - e1.Tick) as SwitchSpeed,
                            CASE 
                                WHEN e1.ItemName LIKE '%rifle%' AND e2.ItemName LIKE '%pistol%' THEN 'Rifle to Pistol'
                                WHEN e1.ItemName LIKE '%awp%' AND e2.ItemName LIKE '%knife%' THEN 'AWP Quick Switch'
                                WHEN e1.ItemName LIKE '%pistol%' AND e2.ItemName LIKE '%rifle%' THEN 'Pistol to Rifle'
                                ELSE 'Other Switch'
                            END as SwitchType
                        FROM Equipment e1
                        INNER JOIN Equipment e2 ON e1.PlayerId = e2.PlayerId 
                            AND e1.RoundNumber = e2.RoundNumber 
                            AND e2.Tick > e1.Tick 
                            AND e2.Tick <= e1.Tick + 128 -- Within ~1 second (assuming 128 tick)
                        INNER JOIN Players p ON e1.PlayerId = p.Id
                        INNER JOIN Rounds r ON r.RoundNumber = e1.RoundNumber AND r.DemoFileId = e1.DemoFileId
                        INNER JOIN DemoFiles d ON r.DemoFileId = d.Id
                        WHERE e1.Action = 'Pickup' AND e2.Action = 'Pickup'
                            AND e1.ItemName != e2.ItemName
                            AND (@DemoId IS NULL OR d.Id = @DemoId)
                            AND (@MapName IS NULL OR d.MapName = @MapName)
                            AND (@PlayerName IS NULL OR p.PlayerName = @PlayerName)
                            AND (@StartDate IS NULL OR d.ParsedAt >= @StartDate)
                            AND (@EndDate IS NULL OR d.ParsedAt <= @EndDate)
                    )
                    SELECT 
                        wpd.PlayerName as Player,
                        wpd.Team,
                        wpd.MapName,
                        wpd.WeaponCategory as Weapon,
                        -- Kill statistics
                        COUNT(*) as TotalKills,
                        COUNT(CASE WHEN wpd.IsHeadshot = 1 THEN 1 END) as HeadshotKills,
                        CAST(COUNT(CASE WHEN wpd.IsHeadshot = 1 THEN 1 END) AS FLOAT) / COUNT(*) * 100 as HeadshotPercentage,
                        -- Range effectiveness
                        AVG(wpd.Distance) as AvgKillDistance,
                        COUNT(CASE WHEN wpd.EngagementRange = 'Close' THEN 1 END) as CloseRangeKills,
                        COUNT(CASE WHEN wpd.EngagementRange = 'Medium' THEN 1 END) as MediumRangeKills,
                        COUNT(CASE WHEN wpd.EngagementRange = 'Long' THEN 1 END) as LongRangeKills,
                        COUNT(CASE WHEN wpd.EngagementRange = 'Very Long' THEN 1 END) as VeryLongRangeKills,
                        -- Special kills
                        COUNT(CASE WHEN wpd.IsWallbang = 1 THEN 1 END) as WallbangKills,
                        COUNT(CASE WHEN wpd.IsNoScope = 1 THEN 1 END) as NoScopeKills,
                        -- Accuracy estimation (when shot data is available)
                        ISNULL(AVG(CAST(wfd.TotalShots AS FLOAT)), 0) as AvgShotsPerKill,
                        CASE 
                            WHEN AVG(CAST(wfd.TotalShots AS FLOAT)) > 0 
                            THEN COUNT(*) * 100.0 / AVG(CAST(wfd.TotalShots AS FLOAT))
                            ELSE NULL 
                        END as EstimatedAccuracyPercentage,
                        -- Damage efficiency
                        AVG(wpd.Damage) as AvgDamagePerKill,
                        -- Consistency metrics
                        STDEV(wpd.Distance) as DistanceConsistency,
                        COUNT(DISTINCT wpd.RoundNumber) as RoundsWithKills,
                        -- Weapon switching proficiency
                        COUNT(wsp.SwitchType) as WeaponSwitches,
                        AVG(CAST(wsp.SwitchSpeed AS FLOAT)) as AvgSwitchSpeed
                    FROM WeaponPerformanceData wpd
                    LEFT JOIN WeaponFireData wfd ON wpd.PlayerName = wfd.PlayerName 
                        AND wpd.Team = wfd.Team 
                        AND wpd.MapName = wfd.MapName 
                        AND wpd.WeaponCategory = wfd.WeaponCategory
                    LEFT JOIN WeaponSwitchingPatterns wsp ON wpd.PlayerName = wsp.PlayerName 
                        AND wpd.Team = wsp.Team 
                        AND wpd.MapName = wsp.MapName
                    WHERE (@Team IS NULL OR wpd.Team = @Team)
                        AND wpd.WeaponCategory != 'Other'
                    GROUP BY wpd.PlayerName, wpd.Team, wpd.MapName, wpd.WeaponCategory
                    HAVING COUNT(*) >= 5 -- Only weapons with at least 5 kills
                    ORDER BY COUNT(*) DESC, HeadshotPercentage DESC";

                var data = await ExecuteAnalyticsQuery(sql, query);
                
                if (query.Format?.ToLower() == "csv")
                {
                    var csv = ConvertToCsv(data);
                    var fileName = $"weapon_mastery_analytics_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                    Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");
                    return File(Encoding.UTF8.GetBytes(csv), "text/csv");
                }

                return Ok(new
                {
                    Title = "Weapon Mastery Analytics",
                    Description = "Detailed analysis of weapon-specific performance including accuracy, range effectiveness, and switching patterns",
                    Data = data,
                    TotalRecords = data.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating weapon mastery analytics");
                return StatusCode(500, $"Error generating weapon mastery analytics: {ex.Message}");
            }
        }

        [HttpGet("grenade-impact-quantification")]
        public async Task<IActionResult> GetGrenadeImpactQuantification([FromQuery] AnalyticsQuery query)
        {
            try
            {
                var sql = @"
                    WITH GrenadeEffectivenessData AS (
                        SELECT 
                            p.PlayerName,
                            p.Team,
                            d.MapName,
                            g.GrenadeType,
                            g.ThrowPositionX,
                            g.ThrowPositionY,
                            g.ThrowPositionZ,
                            g.DetonatePositionX,
                            g.DetonatePositionY,
                            g.DetonatePositionZ,
                            g.ThrowTick,
                            g.DetonateTick,
                            r.RoundNumber,
                            r.WinnerTeam,
                            CASE WHEN p.Team = r.WinnerTeam THEN 1 ELSE 0 END as RoundWon,
                            -- Calculate throw distance
                            SQRT(
                                POWER(CAST(g.DetonatePositionX - g.ThrowPositionX AS FLOAT), 2) +
                                POWER(CAST(g.DetonatePositionY - g.ThrowPositionY AS FLOAT), 2)
                            ) as ThrowDistance,
                            -- Categorize map areas
                            CASE 
                                WHEN d.MapName = 'de_dust2' THEN
                                    CASE 
                                        WHEN g.DetonatePositionY > 1000 THEN 'A Site'
                                        WHEN g.DetonatePositionY < -1000 THEN 'B Site'
                                        WHEN g.DetonatePositionX > 0 THEN 'Upper Tunnels'
                                        ELSE 'Mid/Lower'
                                    END
                                WHEN d.MapName = 'de_mirage' THEN
                                    CASE 
                                        WHEN g.DetonatePositionX > 1000 THEN 'A Site'
                                        WHEN g.DetonatePositionX < -1000 THEN 'B Site'
                                        WHEN ABS(g.DetonatePositionX) < 500 THEN 'Mid'
                                        ELSE 'Connector'
                                    END
                                ELSE 'Unknown Area'
                            END as GrenadeArea,
                            -- Timing categorization (airtime)
                            (g.DetonateTick - g.ThrowTick) as AirTime,
                            CASE 
                                WHEN (g.DetonateTick - g.ThrowTick) <= 64 THEN 'Quick'
                                WHEN (g.DetonateTick - g.ThrowTick) <= 192 THEN 'Medium'
                                ELSE 'Long'
                            END as AirTimeCategory
                        FROM Grenades g
                        INNER JOIN Players p ON g.PlayerId = p.Id
                        INNER JOIN Rounds r ON g.RoundId = r.Id
                        INNER JOIN DemoFiles d ON r.DemoFileId = d.Id
                        WHERE g.GrenadeType IS NOT NULL
                            AND (@DemoId IS NULL OR d.Id = @DemoId)
                            AND (@MapName IS NULL OR d.MapName = @MapName)
                            AND (@PlayerName IS NULL OR p.PlayerName = @PlayerName)
                            AND (@StartDate IS NULL OR d.ParsedAt >= @StartDate)
                            AND (@EndDate IS NULL OR d.ParsedAt <= @EndDate)
                    ),
                    FlashEffectiveness AS (
                        SELECT 
                            p.PlayerName,
                            p.Team,
                            d.MapName,
                            fe.FlashDuration,
                            fe.Tick,
                            r.RoundNumber,
                            r.WinnerTeam,
                            CASE WHEN p.Team = r.WinnerTeam THEN 1 ELSE 0 END as RoundWon,
                            fp.PlayerName as FlashedPlayer,
                            fp.Team as FlashedTeam,
                            CASE 
                                WHEN fp.Team != p.Team THEN 'Enemy Flash'
                                ELSE 'Team Flash'
                            END as FlashType,
                            -- Categorize flash effectiveness
                            CASE 
                                WHEN fe.FlashDuration >= 2.0 THEN 'Full Blind'
                                WHEN fe.FlashDuration >= 1.0 THEN 'Partial Blind'
                                WHEN fe.FlashDuration >= 0.5 THEN 'Light Blind'
                                ELSE 'Minimal'
                            END as BlindLevel
                        FROM FlashEvents fe
                        INNER JOIN Players p ON fe.FlasherPlayerId = p.Id
                        INNER JOIN Players fp ON fe.FlashedPlayerId = fp.Id
                        INNER JOIN Rounds r ON fe.RoundId = r.Id
                        INNER JOIN DemoFiles d ON r.DemoFileId = d.Id
                        WHERE fe.FlashDuration > 0
                            AND (@DemoId IS NULL OR d.Id = @DemoId)
                            AND (@MapName IS NULL OR d.MapName = @MapName)
                            AND (@PlayerName IS NULL OR p.PlayerName = @PlayerName)
                            AND (@StartDate IS NULL OR d.ParsedAt >= @StartDate)
                            AND (@EndDate IS NULL OR d.ParsedAt <= @EndDate)
                    ),
                    UtilityComboDetection AS (
                        -- Detect coordinated utility usage (grenades thrown within 3 seconds)
                        SELECT 
                            g1.PlayerId as Player1Id,
                            g2.PlayerId as Player2Id,
                            p1.PlayerName as Player1Name,
                            p2.PlayerName as Player2Name,
                            p1.Team,
                            d.MapName,
                            g1.GrenadeType as Grenade1Type,
                            g2.GrenadeType as Grenade2Type,
                            ABS(g1.ThrowTick - g2.ThrowTick) as TimingDiff,
                            r.RoundNumber,
                            r.WinnerTeam,
                            CASE WHEN p1.Team = r.WinnerTeam THEN 1 ELSE 0 END as RoundWon,
                            CASE 
                                WHEN g1.GrenadeType = 'smoke' AND g2.GrenadeType = 'flashbang' THEN 'Smoke-Flash'
                                WHEN g1.GrenadeType = 'flashbang' AND g2.GrenadeType = 'hegrenade' THEN 'Flash-HE'
                                WHEN g1.GrenadeType = 'smoke' AND g2.GrenadeType = 'hegrenade' THEN 'Smoke-HE'
                                ELSE 'Other Combo'
                            END as ComboType
                        FROM Grenades g1
                        INNER JOIN Grenades g2 ON g1.RoundId = g2.RoundId 
                            AND g1.Id != g2.Id
                            AND ABS(g1.ThrowTick - g2.ThrowTick) <= 384 -- Within ~3 seconds
                        INNER JOIN Players p1 ON g1.PlayerId = p1.Id
                        INNER JOIN Players p2 ON g2.PlayerId = p2.Id
                        INNER JOIN Rounds r ON g1.RoundId = r.Id
                        INNER JOIN DemoFiles d ON r.DemoFileId = d.Id
                        WHERE p1.Team = p2.Team -- Same team coordination
                            AND (@DemoId IS NULL OR d.Id = @DemoId)
                            AND (@MapName IS NULL OR d.MapName = @MapName)
                            AND (@PlayerName IS NULL OR p1.PlayerName = @PlayerName OR p2.PlayerName = @PlayerName)
                            AND (@StartDate IS NULL OR d.ParsedAt >= @StartDate)
                            AND (@EndDate IS NULL OR d.ParsedAt <= @EndDate)
                    )
                    SELECT 
                        ged.PlayerName as Player,
                        ged.Team,
                        ged.MapName,
                        ged.GrenadeType,
                        ged.GrenadeArea,
                        -- Basic grenade statistics
                        COUNT(*) as TotalGrenades,
                        AVG(ged.ThrowDistance) as AvgThrowDistance,
                        COUNT(CASE WHEN ged.RoundWon = 1 THEN 1 END) as GrenadesInWonRounds,
                        CAST(COUNT(CASE WHEN ged.RoundWon = 1 THEN 1 END) AS FLOAT) / COUNT(*) * 100 as WinRateWithGrenades,
                        -- Timing analysis
                        AVG(CAST(ged.AirTime AS FLOAT)) as AvgAirTime,
                        COUNT(CASE WHEN ged.AirTimeCategory = 'Quick' THEN 1 END) as QuickThrows,
                        COUNT(CASE WHEN ged.AirTimeCategory = 'Medium' THEN 1 END) as MediumThrows,
                        COUNT(CASE WHEN ged.AirTimeCategory = 'Long' THEN 1 END) as LongThrows,
                        -- Flash-specific metrics (when available)
                        ISNULL(AVG(fe.FlashDuration), 0) as AvgFlashDuration,
                        ISNULL(COUNT(CASE WHEN fe.FlashType = 'Enemy Flash' THEN 1 END), 0) as EnemyFlashes,
                        ISNULL(COUNT(CASE WHEN fe.FlashType = 'Team Flash' THEN 1 END), 0) as TeamFlashes,
                        ISNULL(COUNT(CASE WHEN fe.BlindLevel = 'Full Blind' THEN 1 END), 0) as FullBlinds,
                        -- Team coordination metrics
                        ISNULL(COUNT(ucd.ComboType), 0) as UtilityCombos,
                        ISNULL(AVG(CAST(ucd.TimingDiff AS FLOAT)), 0) as AvgComboTiming,
                        -- Area-specific effectiveness
                        COUNT(DISTINCT ged.RoundNumber) as RoundsWithGrenades,
                        STDEV(ged.ThrowDistance) as ThrowConsistency
                    FROM GrenadeEffectivenessData ged
                    LEFT JOIN FlashEffectiveness fe ON ged.PlayerName = fe.PlayerName 
                        AND ged.Team = fe.Team 
                        AND ged.MapName = fe.MapName
                        AND ged.RoundNumber = fe.RoundNumber
                    LEFT JOIN UtilityComboDetection ucd ON (ged.PlayerName = ucd.Player1Name OR ged.PlayerName = ucd.Player2Name)
                        AND ged.Team = ucd.Team 
                        AND ged.MapName = ucd.MapName
                        AND ged.RoundNumber = ucd.RoundNumber
                    WHERE (@Team IS NULL OR ged.Team = @Team)
                        AND ged.GrenadeArea != 'Unknown Area'
                    GROUP BY ged.PlayerName, ged.Team, ged.MapName, ged.GrenadeType, ged.GrenadeArea
                    HAVING COUNT(*) >= 3 -- Only areas with at least 3 grenades
                    ORDER BY WinRateWithGrenades DESC, TotalGrenades DESC";

                var data = await ExecuteAnalyticsQuery(sql, query);
                
                if (query.Format?.ToLower() == "csv")
                {
                    var csv = ConvertToCsv(data);
                    var fileName = $"grenade_impact_quantification_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                    Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");
                    return File(Encoding.UTF8.GetBytes(csv), "text/csv");
                }

                return Ok(new
                {
                    Title = "Grenade Impact Quantification",
                    Description = "Analysis of utility effectiveness including flash duration, smoke timing, HE damage, and team coordination",
                    Data = data,
                    TotalRecords = data.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating grenade impact quantification");
                return StatusCode(500, $"Error generating grenade impact quantification: {ex.Message}");
            }
        }

        [HttpGet("first-blood-impact-analysis")]
        public async Task<IActionResult> GetFirstBloodImpactAnalysis([FromQuery] AnalyticsQuery query)
        {
            try
            {
                var sql = @"
                    WITH FirstBloodAnalysis AS (
                        SELECT 
                            k.Id as KillId,
                            k.GameTime,
                            k.Weapon,
                            k.WeaponClass,
                            k.Distance,
                            k.IsHeadshot,
                            k.IsWallbang,
                            k.ThroughSmoke,
                            k.KillerPositionX,
                            k.KillerPositionY,
                            k.KillerPositionZ,
                            k.VictimPositionX,
                            k.VictimPositionY,
                            k.VictimPositionZ,
                            k.KillerViewAngleX,
                            k.KillerViewAngleY,
                            k.VictimViewAngleX,
                            k.VictimViewAngleY,
                            killer.PlayerName as KillerName,
                            killer.Team as KillerTeam,
                            victim.PlayerName as VictimName,
                            victim.Team as VictimTeam,
                            r.RoundNumber,
                            r.WinnerTeam,
                            d.MapName,
                            d.FileName,
                            CASE WHEN killer.Team = r.WinnerTeam THEN 1 ELSE 0 END as FirstKillTeamWon,
                            -- Categorize map areas for first kill location analysis
                            CASE 
                                WHEN d.MapName = 'de_dust2' THEN
                                    CASE 
                                        WHEN k.VictimPositionY > 1000 THEN 'A Site'
                                        WHEN k.VictimPositionY < -1000 THEN 'B Site'
                                        WHEN k.VictimPositionX > 0 THEN 'Upper Tunnels'
                                        ELSE 'Mid/Lower'
                                    END
                                WHEN d.MapName = 'de_mirage' THEN
                                    CASE 
                                        WHEN k.VictimPositionX > 1000 THEN 'A Site'
                                        WHEN k.VictimPositionX < -1000 THEN 'B Site'
                                        WHEN ABS(k.VictimPositionX) < 500 THEN 'Mid'
                                        ELSE 'Connector'
                                    END
                                WHEN d.MapName = 'de_inferno' THEN
                                    CASE 
                                        WHEN k.VictimPositionY > 2000 THEN 'A Site'
                                        WHEN k.VictimPositionY < -500 THEN 'B Site'
                                        WHEN ABS(k.VictimPositionX) < 800 THEN 'Mid'
                                        ELSE 'Banana/Apps'
                                    END
                                ELSE 'Unknown Area'
                            END as FirstKillArea,
                            -- Timing categorization
                            CASE 
                                WHEN k.GameTime <= 15.0 THEN 'Early (0-15s)'
                                WHEN k.GameTime <= 45.0 THEN 'Mid (15-45s)'
                                WHEN k.GameTime <= 75.0 THEN 'Late (45-75s)'
                                ELSE 'Very Late (75s+)'
                            END as FirstKillTiming,
                            -- Calculate crosshair placement accuracy (simplified)
                            ABS(CAST(k.KillerViewAngleX AS FLOAT)) + ABS(CAST(k.KillerViewAngleY AS FLOAT)) as ViewAngleMagnitude
                        FROM Kills k
                        INNER JOIN Players killer ON k.KillerId = killer.Id
                        INNER JOIN Players victim ON k.VictimId = victim.Id
                        INNER JOIN Rounds r ON k.RoundId = r.Id
                        INNER JOIN DemoFiles d ON k.DemoFileId = d.Id
                        WHERE k.IsFirstKill = 1
                            AND (@DemoId IS NULL OR d.Id = @DemoId)
                            AND (@MapName IS NULL OR d.MapName = @MapName)
                            AND (@PlayerName IS NULL OR killer.PlayerName = @PlayerName)
                            AND (@StartDate IS NULL OR d.ParsedAt >= @StartDate)
                            AND (@EndDate IS NULL OR d.ParsedAt <= @EndDate)
                    )
                    SELECT 
                        KillerName as Player,
                        KillerTeam as Team,
                        MapName,
                        FirstKillArea,
                        FirstKillTiming,
                        -- First kill performance metrics
                        COUNT(*) as TotalFirstKills,
                        SUM(FirstKillTeamWon) as FirstKillRoundsWon,
                        CAST(SUM(FirstKillTeamWon) AS FLOAT) / COUNT(*) * 100 as FirstKillWinPercentage,
                        -- Weapon analysis
                        COUNT(CASE WHEN WeaponClass LIKE '%rifle%' OR Weapon LIKE '%ak47%' OR Weapon LIKE '%m4%' THEN 1 END) as RifleFirstKills,
                        COUNT(CASE WHEN WeaponClass LIKE '%sniper%' OR Weapon LIKE '%awp%' THEN 1 END) as SniperFirstKills,
                        COUNT(CASE WHEN WeaponClass LIKE '%pistol%' THEN 1 END) as PistolFirstKills,
                        -- Technical analysis
                        AVG(Distance) as AvgFirstKillDistance,
                        COUNT(CASE WHEN IsHeadshot = 1 THEN 1 END) as HeadshotFirstKills,
                        CAST(COUNT(CASE WHEN IsHeadshot = 1 THEN 1 END) AS FLOAT) / COUNT(*) * 100 as HeadshotPercentage,
                        COUNT(CASE WHEN IsWallbang = 1 THEN 1 END) as WallbangFirstKills,
                        COUNT(CASE WHEN ThroughSmoke = 1 THEN 1 END) as SmokeFirstKills,
                        -- Positioning analysis
                        AVG(CAST(KillerPositionX AS FLOAT)) as AvgKillerPosX,
                        AVG(CAST(KillerPositionY AS FLOAT)) as AvgKillerPosY,
                        AVG(CAST(VictimPositionX AS FLOAT)) as AvgVictimPosX,
                        AVG(CAST(VictimPositionY AS FLOAT)) as AvgVictimPosY,
                        -- Crosshair placement (simplified analysis)
                        AVG(ViewAngleMagnitude) as AvgViewAngleMagnitude,
                        STDEV(ViewAngleMagnitude) as ViewAngleConsistency,
                        -- Timing analysis
                        AVG(GameTime) as AvgFirstKillTime,
                        MIN(GameTime) as FastestFirstKill,
                        MAX(GameTime) as LatestFirstKill,
                        -- Impact analysis
                        COUNT(DISTINCT MapName) as MapsWithFirstKills,
                        COUNT(DISTINCT RoundNumber) as RoundsWithFirstKills
                    FROM FirstBloodAnalysis
                    WHERE (@Team IS NULL OR KillerTeam = @Team)
                        AND FirstKillArea != 'Unknown Area'
                    GROUP BY KillerName, KillerTeam, MapName, FirstKillArea, FirstKillTiming
                    HAVING COUNT(*) >= 2 -- Only show areas/timings with at least 2 first kills
                    ORDER BY FirstKillWinPercentage DESC, TotalFirstKills DESC";

                var data = await ExecuteAnalyticsQuery(sql, query);
                
                if (query.Format?.ToLower() == "csv")
                {
                    var csv = ConvertToCsv(data);
                    var fileName = $"first_blood_impact_analysis_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                    Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");
                    return File(Encoding.UTF8.GetBytes(csv), "text/csv");
                }

                return Ok(new
                {
                    Title = "First Blood Impact Analysis",
                    Description = "Analysis of first kill impact including location heatmaps, weapon effectiveness, timing correlation, and round win impact",
                    Data = data,
                    TotalRecords = data.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating first blood impact analysis");
                return StatusCode(500, $"Error generating first blood impact analysis: {ex.Message}");
            }
        }

        [HttpGet("economic-efficiency-analysis")]
        public async Task<IActionResult> GetEconomicEfficiencyAnalysis([FromQuery] AnalyticsQuery query)
        {
            try
            {
                var sql = @"
                    WITH EconomicRoundData AS (
                        SELECT 
                            prs.PlayerId,
                            p.PlayerName,
                            p.Team,
                            prs.RoundId,
                            r.RoundNumber,
                            r.WinnerTeam,
                            d.MapName,
                            d.FileName,
                            prs.EquipmentValue,
                            prs.MoneySpent,
                            -- Calculate estimated kill reward (standard CS2 kill rewards)
                            (prs.Kills * 300) as KillReward,
                            prs.Kills,
                            prs.Deaths,
                            prs.Assists,
                            prs.Damage,
                            CASE WHEN p.Team = r.WinnerTeam THEN 1 ELSE 0 END as RoundWon,
                            -- Categorize economic states
                            CASE 
                                WHEN prs.EquipmentValue >= 4000 THEN 'Full Buy'
                                WHEN prs.EquipmentValue >= 2500 THEN 'Half Buy'
                                WHEN prs.EquipmentValue >= 1000 THEN 'Force Buy'
                                ELSE 'Eco'
                            END as EconomicState,
                            -- Calculate round momentum (previous round context)
                            LAG(CASE WHEN p.Team = r.WinnerTeam THEN 1 ELSE 0 END) OVER 
                                (PARTITION BY p.Id ORDER BY r.RoundNumber) as PreviousRoundWon,
                            LAG(prs.EquipmentValue) OVER 
                                (PARTITION BY p.Id ORDER BY r.RoundNumber) as PreviousEquipmentValue,
                            -- Team economic context
                            SUM(prs.EquipmentValue) OVER 
                                (PARTITION BY p.Team, r.Id) as TeamEquipmentValue,
                            AVG(prs.EquipmentValue) OVER 
                                (PARTITION BY p.Team, r.Id) as TeamAvgEquipmentValue
                        FROM PlayerRoundStats prs
                        INNER JOIN Players p ON prs.PlayerId = p.Id
                        INNER JOIN Rounds r ON prs.RoundId = r.Id
                        INNER JOIN DemoFiles d ON r.DemoFileId = d.Id
                        WHERE (@DemoId IS NULL OR d.Id = @DemoId)
                            AND (@MapName IS NULL OR d.MapName = @MapName)
                            AND (@PlayerName IS NULL OR p.PlayerName = @PlayerName)
                            AND (@StartDate IS NULL OR d.ParsedAt >= @StartDate)
                            AND (@EndDate IS NULL OR d.ParsedAt <= @EndDate)
                    ),
                    EconomicEfficiency AS (
                        SELECT 
                            *,
                            -- Calculate ROI (Return on Investment)
                            CASE 
                                WHEN EquipmentValue > 0 THEN 
                                    (CAST(Kills AS FLOAT) * 1000 + CAST(Assists AS FLOAT) * 300 + CAST(Damage AS FLOAT)) / EquipmentValue
                                ELSE 0 
                            END as EquipmentROI,
                            -- Force buy analysis
                            CASE 
                                WHEN EconomicState = 'Force Buy' AND PreviousRoundWon = 0 THEN 'Anti-Eco Force'
                                WHEN EconomicState = 'Force Buy' AND PreviousRoundWon = 1 THEN 'Momentum Force'
                                WHEN EconomicState IN ('Eco', 'Force Buy') THEN 'Low Economy'
                                ELSE 'Standard Buy'
                            END as BuyStrategy,
                            -- Save vs Force decision analysis
                            CASE 
                                WHEN EconomicState = 'Eco' AND PreviousEquipmentValue >= 2500 THEN 'Strategic Save'
                                WHEN EconomicState = 'Force Buy' AND PreviousEquipmentValue <= 1500 THEN 'Forced Buy'
                                ELSE 'Standard'
                            END as EconomicDecision
                        FROM EconomicRoundData
                    )
                    SELECT 
                        PlayerName as Player,
                        Team,
                        MapName,
                        EconomicState,
                        BuyStrategy,
                        -- Performance metrics
                        COUNT(*) as TotalRounds,
                        SUM(RoundWon) as RoundsWon,
                        CAST(SUM(RoundWon) AS FLOAT) / COUNT(*) * 100 as WinPercentage,
                        -- Economic efficiency metrics
                        AVG(EquipmentValue) as AvgEquipmentValue,
                        AVG(MoneySpent) as AvgMoneySpent,
                        AVG(EquipmentROI) as AvgEquipmentROI,
                        MAX(EquipmentROI) as MaxEquipmentROI,
                        -- Performance analysis
                        SUM(Kills) as TotalKills,
                        SUM(Deaths) as TotalDeaths,
                        SUM(Assists) as TotalAssists,
                        SUM(Damage) as TotalDamage,
                        AVG(CAST(Kills AS FLOAT)) as AvgKills,
                        AVG(CAST(Deaths AS FLOAT)) as AvgDeaths,
                        AVG(CAST(Damage AS FLOAT)) as AvgDamage,
                        -- Equipment efficiency
                        CASE 
                            WHEN SUM(CASE WHEN EquipmentValue > 0 THEN 1 ELSE 0 END) > 0 THEN
                                SUM(Kills) * 100.0 / SUM(CASE WHEN EquipmentValue > 0 THEN 1 ELSE 0 END)
                            ELSE 0 
                        END as KillsPerEquippedRound,
                        CASE 
                            WHEN AVG(EquipmentValue) > 0 THEN
                                SUM(Damage) / AVG(EquipmentValue)
                            ELSE 0 
                        END as DamagePerDollar,
                        -- Force buy effectiveness
                        COUNT(CASE WHEN BuyStrategy = 'Anti-Eco Force' THEN 1 END) as AntiEcoForces,
                        SUM(CASE WHEN BuyStrategy = 'Anti-Eco Force' THEN RoundWon ELSE 0 END) as AntiEcoForceWins,
                        CASE 
                            WHEN COUNT(CASE WHEN BuyStrategy = 'Anti-Eco Force' THEN 1 END) > 0 THEN
                                CAST(SUM(CASE WHEN BuyStrategy = 'Anti-Eco Force' THEN RoundWon ELSE 0 END) AS FLOAT) / 
                                COUNT(CASE WHEN BuyStrategy = 'Anti-Eco Force' THEN 1 END) * 100
                            ELSE 0 
                        END as AntiEcoForceWinRate,
                        -- Save round effectiveness
                        COUNT(CASE WHEN EconomicState = 'Eco' THEN 1 END) as EcoRounds,
                        SUM(CASE WHEN EconomicState = 'Eco' THEN RoundWon ELSE 0 END) as EcoRoundWins,
                        CASE 
                            WHEN COUNT(CASE WHEN EconomicState = 'Eco' THEN 1 END) > 0 THEN
                                CAST(SUM(CASE WHEN EconomicState = 'Eco' THEN RoundWon ELSE 0 END) AS FLOAT) / 
                                COUNT(CASE WHEN EconomicState = 'Eco' THEN 1 END) * 100
                            ELSE 0 
                        END as EcoRoundWinRate,
                        -- Economic momentum tracking
                        COUNT(CASE WHEN PreviousRoundWon = 1 THEN 1 END) as RoundsAfterWin,
                        COUNT(CASE WHEN PreviousRoundWon = 0 THEN 1 END) as RoundsAfterLoss,
                        CASE 
                            WHEN COUNT(CASE WHEN PreviousRoundWon = 1 THEN 1 END) > 0 THEN
                                CAST(SUM(CASE WHEN PreviousRoundWon = 1 THEN RoundWon ELSE 0 END) AS FLOAT) / 
                                COUNT(CASE WHEN PreviousRoundWon = 1 THEN 1 END) * 100
                            ELSE 0 
                        END as WinRateAfterWin,
                        CASE 
                            WHEN COUNT(CASE WHEN PreviousRoundWon = 0 THEN 1 END) > 0 THEN
                                CAST(SUM(CASE WHEN PreviousRoundWon = 0 THEN RoundWon ELSE 0 END) AS FLOAT) / 
                                COUNT(CASE WHEN PreviousRoundWon = 0 THEN 1 END) * 100
                            ELSE 0 
                        END as WinRateAfterLoss
                    FROM EconomicEfficiency
                    WHERE (@Team IS NULL OR Team = @Team)
                    GROUP BY PlayerName, Team, MapName, EconomicState, BuyStrategy
                    HAVING COUNT(*) >= 3 -- Only show economic states with at least 3 rounds
                    ORDER BY AvgEquipmentROI DESC, WinPercentage DESC";

                var data = await ExecuteAnalyticsQuery(sql, query);
                
                if (query.Format?.ToLower() == "csv")
                {
                    var csv = ConvertToCsv(data);
                    var fileName = $"economic_efficiency_analysis_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                    Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");
                    return File(Encoding.UTF8.GetBytes(csv), "text/csv");
                }

                return Ok(new
                {
                    Title = "Economic Efficiency Analysis",
                    Description = "Deep analysis of equipment ROI, force buy patterns, save optimization, and economic momentum tracking",
                    Data = data,
                    TotalRecords = data.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating economic efficiency analysis");
                return StatusCode(500, $"Error generating economic efficiency analysis: {ex.Message}");
            }
        }

        [HttpGet("round-momentum-tracking")]
        public async Task<IActionResult> GetRoundMomentumTracking([FromQuery] AnalyticsQuery query)
        {
            try
            {
                var sql = @"
                    WITH RoundSequenceData AS (
                        SELECT 
                            r.Id as RoundId,
                            r.RoundNumber,
                            r.WinnerTeam,
                            r.EndReason,
                            d.MapName,
                            d.FileName,
                            -- Calculate round momentum indicators
                            LAG(r.WinnerTeam) OVER (PARTITION BY d.Id ORDER BY r.RoundNumber) as PreviousWinner,
                            LAG(r.WinnerTeam, 2) OVER (PARTITION BY d.Id ORDER BY r.RoundNumber) as TwoRoundsAgoWinner,
                            LAG(r.WinnerTeam, 3) OVER (PARTITION BY d.Id ORDER BY r.RoundNumber) as ThreeRoundsAgoWinner,
                            -- Count consecutive wins
                            CASE WHEN LAG(r.WinnerTeam) OVER (PARTITION BY d.Id ORDER BY r.RoundNumber) = r.WinnerTeam THEN 1 ELSE 0 END as ConsecutiveWin,
                            -- Determine momentum shifts
                            CASE 
                                WHEN LAG(r.WinnerTeam) OVER (PARTITION BY d.Id ORDER BY r.RoundNumber) != r.WinnerTeam THEN 1
                                ELSE 0 
                            END as MomentumShift,
                            -- First kill timing and impact
                            (SELECT TOP 1 k.GameTime 
                             FROM Kills k 
                             WHERE k.RoundId = r.Id AND k.IsFirstKill = 1 
                             ORDER BY k.GameTime) as FirstKillTime,
                            (SELECT TOP 1 killer.Team
                             FROM Kills k 
                             INNER JOIN Players killer ON k.KillerId = killer.Id
                             WHERE k.RoundId = r.Id AND k.IsFirstKill = 1 
                             ORDER BY k.GameTime) as FirstKillTeam,
                            -- Kill sequence analysis
                            (SELECT COUNT(*) FROM Kills k WHERE k.RoundId = r.Id) as TotalKills,
                            (SELECT COUNT(*) FROM Kills k 
                             INNER JOIN Players killer ON k.KillerId = killer.Id
                             WHERE k.RoundId = r.Id AND killer.Team = r.WinnerTeam) as WinnerTeamKills
                        FROM Rounds r
                        INNER JOIN DemoFiles d ON r.DemoFileId = d.Id
                        WHERE (@DemoId IS NULL OR d.Id = @DemoId)
                            AND (@MapName IS NULL OR d.MapName = @MapName)
                            AND (@StartDate IS NULL OR d.ParsedAt >= @StartDate)
                            AND (@EndDate IS NULL OR d.ParsedAt <= @EndDate)
                    ),
                    MomentumStreaks AS (
                        SELECT 
                            *,
                            -- Calculate streak lengths (simplified)
                            ROW_NUMBER() OVER (PARTITION BY MapName, WinnerTeam ORDER BY RoundNumber) as StreakLength,
                            -- Comeback potential scoring
                            CASE 
                                WHEN FirstKillTeam = WinnerTeam THEN 'First Kill Advantage'
                                WHEN FirstKillTeam IS NOT NULL AND FirstKillTeam != WinnerTeam THEN 'Comeback Round'
                                ELSE 'No First Kill Data'
                            END as RoundType,
                            -- Force reset identification
                            CASE 
                                WHEN PreviousWinner != WinnerTeam AND 
                                     TwoRoundsAgoWinner != WinnerTeam AND 
                                     ThreeRoundsAgoWinner != WinnerTeam THEN 'Force Reset'
                                WHEN PreviousWinner = WinnerTeam AND 
                                     TwoRoundsAgoWinner = WinnerTeam THEN 'Momentum Build'
                                ELSE 'Standard'
                            END as MomentumPattern
                        FROM RoundSequenceData
                    )
                    SELECT 
                        MapName,
                        WinnerTeam as Team,
                        RoundType,
                        MomentumPattern,
                        -- Round performance metrics
                        COUNT(*) as TotalRounds,
                        AVG(CAST(StreakLength AS FLOAT)) as AvgStreakLength,
                        MAX(StreakLength) as MaxStreak,
                        -- Momentum analysis
                        SUM(MomentumShift) as TotalMomentumShifts,
                        CAST(SUM(MomentumShift) AS FLOAT) / COUNT(*) * 100 as MomentumShiftPercentage,
                        -- Comeback analysis
                        COUNT(CASE WHEN RoundType = 'Comeback Round' THEN 1 END) as ComebackRounds,
                        CAST(COUNT(CASE WHEN RoundType = 'Comeback Round' THEN 1 END) AS FLOAT) / 
                            COUNT(*) * 100 as ComebackPercentage,
                        -- First kill correlation
                        COUNT(CASE WHEN RoundType = 'First Kill Advantage' THEN 1 END) as FirstKillAdvantageRounds,
                        CAST(COUNT(CASE WHEN RoundType = 'First Kill Advantage' THEN 1 END) AS FLOAT) / 
                            COUNT(*) * 100 as FirstKillAdvantagePercentage,
                        -- Force reset effectiveness
                        COUNT(CASE WHEN MomentumPattern = 'Force Reset' THEN 1 END) as ForceResetRounds,
                        CAST(COUNT(CASE WHEN MomentumPattern = 'Force Reset' THEN 1 END) AS FLOAT) / 
                            COUNT(*) * 100 as ForceResetPercentage,
                        -- Momentum building
                        COUNT(CASE WHEN MomentumPattern = 'Momentum Build' THEN 1 END) as MomentumBuildRounds,
                        CAST(COUNT(CASE WHEN MomentumPattern = 'Momentum Build' THEN 1 END) AS FLOAT) / 
                            COUNT(*) * 100 as MomentumBuildPercentage,
                        -- Kill dominance
                        AVG(CAST(WinnerTeamKills AS FLOAT)) as AvgWinnerTeamKills,
                        AVG(CAST(TotalKills AS FLOAT)) as AvgTotalKills,
                        AVG(CASE WHEN TotalKills > 0 THEN CAST(WinnerTeamKills AS FLOAT) / TotalKills * 100 ELSE 0 END) as WinnerKillDominance,
                        -- Timing analysis
                        AVG(FirstKillTime) as AvgFirstKillTime,
                        COUNT(CASE WHEN FirstKillTime <= 15.0 THEN 1 END) as EarlyFirstKills,
                        COUNT(CASE WHEN FirstKillTime > 75.0 THEN 1 END) as LateFirstKills,
                        -- Round ending analysis
                        COUNT(CASE WHEN EndReason LIKE '%Elimination%' THEN 1 END) as EliminationWins,
                        COUNT(CASE WHEN EndReason LIKE '%Defuse%' OR EndReason LIKE '%Explode%' THEN 1 END) as BombWins,
                        COUNT(CASE WHEN EndReason LIKE '%Time%' THEN 1 END) as TimeWins
                    FROM MomentumStreaks
                    WHERE (@Team IS NULL OR WinnerTeam = @Team)
                    GROUP BY MapName, WinnerTeam, RoundType, MomentumPattern
                    HAVING COUNT(*) >= 2 -- Only show patterns with at least 2 occurrences
                    ORDER BY MomentumBuildPercentage DESC, AvgStreakLength DESC";

                var data = await ExecuteAnalyticsQuery(sql, query);
                
                if (query.Format?.ToLower() == "csv")
                {
                    var csv = ConvertToCsv(data);
                    var fileName = $"round_momentum_tracking_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                    Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");
                    return File(Encoding.UTF8.GetBytes(csv), "text/csv");
                }

                return Ok(new
                {
                    Title = "Round Momentum Tracking",
                    Description = "Analysis of streak patterns, comeback potential, momentum shifts, and force reset identification",
                    Data = data,
                    TotalRecords = data.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating round momentum tracking");
                return StatusCode(500, $"Error generating round momentum tracking: {ex.Message}");
            }
        }

        [HttpGet("positioning-intelligence")]
        public async Task<IActionResult> GetPositioningIntelligence([FromQuery] AnalyticsQuery query)
        {
            try
            {
                var sql = @"
                    WITH DeathPositionAnalysis AS (
                        SELECT 
                            k.Id as KillId,
                            victim.PlayerName as VictimPlayer,
                            victim.Team as VictimTeam,
                            killer.PlayerName as KillerPlayer,
                            killer.Team as KillerTeam,
                            k.VictimPositionX,
                            k.VictimPositionY,
                            k.VictimPositionZ,
                            k.KillerPositionX,
                            k.KillerPositionY,
                            k.KillerPositionZ,
                            k.Distance,
                            k.Weapon,
                            k.GameTime,
                            d.MapName,
                            r.RoundNumber,
                            r.WinnerTeam,
                            -- Position categorization by map
                            CASE 
                                WHEN d.MapName = 'de_dust2' THEN
                                    CASE 
                                        WHEN k.VictimPositionY > 1200 THEN 'A Site'
                                        WHEN k.VictimPositionY < -800 AND k.VictimPositionX < 500 THEN 'B Site'
                                        WHEN k.VictimPositionY < -800 AND k.VictimPositionX > 500 THEN 'Upper Tunnels'
                                        WHEN ABS(k.VictimPositionY) < 400 THEN 'Mid'
                                        WHEN k.VictimPositionY < -400 AND k.VictimPositionY > -800 THEN 'Lower Tunnels'
                                        ELSE 'Other'
                                    END
                                WHEN d.MapName = 'de_mirage' THEN
                                    CASE 
                                        WHEN k.VictimPositionX > 800 THEN 'A Site'
                                        WHEN k.VictimPositionX < -800 THEN 'B Site' 
                                        WHEN ABS(k.VictimPositionX) < 300 AND k.VictimPositionY > 0 THEN 'Mid'
                                        WHEN ABS(k.VictimPositionX) < 600 AND k.VictimPositionY < 0 THEN 'Connector'
                                        ELSE 'Other'
                                    END
                                WHEN d.MapName = 'de_inferno' THEN
                                    CASE 
                                        WHEN k.VictimPositionY > 1000 THEN 'A Site'
                                        WHEN k.VictimPositionY < -500 AND k.VictimPositionX < 0 THEN 'B Site'
                                        WHEN ABS(k.VictimPositionY) < 500 THEN 'Mid'
                                        ELSE 'Other'
                                    END
                                ELSE 'Unknown Map'
                            END as VictimPositionArea,
                            -- Death type categorization
                            CASE 
                                WHEN k.Distance <= 500 THEN 'Close Range'
                                WHEN k.Distance <= 1500 THEN 'Medium Range'
                                WHEN k.Distance <= 3000 THEN 'Long Range'
                                ELSE 'Very Long Range'
                            END as EngagementRange,
                            -- Time period analysis
                            CASE 
                                WHEN k.GameTime <= 15.0 THEN 'Early Round'
                                WHEN k.GameTime <= 60.0 THEN 'Mid Round'
                                WHEN k.GameTime <= 100.0 THEN 'Late Round'
                                ELSE 'Overtime'
                            END as RoundPhase,
                            -- Angle holding analysis
                            CASE 
                                WHEN ABS(k.KillerPositionX - k.VictimPositionX) < 200 AND 
                                     ABS(k.KillerPositionY - k.VictimPositionY) > 800 THEN 'Long Angle Hold'
                                WHEN k.Distance < 800 AND 
                                     ABS(k.KillerPositionX - k.VictimPositionX) < 400 THEN 'Close Angle Hold'
                                WHEN k.Distance > 1200 THEN 'AWP Position'
                                ELSE 'Standard Position'
                            END as PositionType
                        FROM Kills k
                        INNER JOIN Players victim ON k.VictimId = victim.Id
                        INNER JOIN Players killer ON k.KillerId = killer.Id
                        INNER JOIN Rounds r ON k.RoundId = r.Id
                        INNER JOIN DemoFiles d ON r.DemoFileId = d.Id
                        WHERE (@DemoId IS NULL OR d.Id = @DemoId)
                            AND (@MapName IS NULL OR d.MapName = @MapName)
                            AND (@Team IS NULL OR victim.Team = @Team OR killer.Team = @Team)
                            AND (@PlayerName IS NULL OR victim.PlayerName = @PlayerName OR killer.PlayerName = @PlayerName)
                            AND (@StartDate IS NULL OR d.ParsedAt >= @StartDate)
                            AND (@EndDate IS NULL OR d.ParsedAt <= @EndDate)
                    ),
                    MovementPatternAnalysis AS (
                        SELECT 
                            pp.PlayerId,
                            p.PlayerName,
                            p.Team,
                            d.MapName,
                            r.RoundNumber,
                            r.Id as RoundId,
                            -- Calculate movement metrics
                            COUNT(*) as TotalPositionSamples,
                            AVG(SQRT(POWER(pp.VelocityX, 2) + POWER(pp.VelocityY, 2))) as AvgSpeed,
                            MAX(SQRT(POWER(pp.VelocityX, 2) + POWER(pp.VelocityY, 2))) as MaxSpeed,
                            STDEV(SQRT(POWER(pp.VelocityX, 2) + POWER(pp.VelocityY, 2))) as SpeedVariation,
                            -- Position coverage analysis
                            MAX(pp.PositionX) - MIN(pp.PositionX) as XAxisCoverage,
                            MAX(pp.PositionY) - MIN(pp.PositionY) as YAxisCoverage,
                            -- Movement patterns
                            AVG(ABS(pp.VelocityX)) as AvgXMovement,
                            AVG(ABS(pp.VelocityY)) as AvgYMovement,
                            -- Stationary periods
                            COUNT(CASE WHEN SQRT(POWER(pp.VelocityX, 2) + POWER(pp.VelocityY, 2)) < 10 THEN 1 END) as StationaryTicks,
                            CAST(COUNT(CASE WHEN SQRT(POWER(pp.VelocityX, 2) + POWER(pp.VelocityY, 2)) < 10 THEN 1 END) AS FLOAT) / 
                                COUNT(*) * 100 as StationaryPercentage
                        FROM EnhancedPlayerPositions pp
                        INNER JOIN Players p ON pp.PlayerId = p.Id
                        INNER JOIN Rounds r ON pp.RoundId = r.Id
                        INNER JOIN DemoFiles d ON r.DemoFileId = d.Id
                        WHERE (@DemoId IS NULL OR d.Id = @DemoId)
                            AND (@MapName IS NULL OR d.MapName = @MapName)
                            AND (@Team IS NULL OR p.Team = @Team)
                            AND (@PlayerName IS NULL OR p.PlayerName = @PlayerName)
                            AND (@StartDate IS NULL OR d.ParsedAt >= @StartDate)
                            AND (@EndDate IS NULL OR d.ParsedAt <= @EndDate)
                        GROUP BY pp.PlayerId, p.PlayerName, p.Team, d.MapName, r.RoundNumber, r.Id
                        HAVING COUNT(*) >= 10 -- Only rounds with sufficient position data
                    )
                    SELECT 
                        dpa.MapName,
                        dpa.VictimPositionArea as PositionArea,
                        dpa.PositionType,
                        dpa.EngagementRange,
                        dpa.RoundPhase,
                        -- Death probability heatmap data
                        COUNT(*) as TotalDeaths,
                        COUNT(DISTINCT dpa.VictimPlayer) as UniqueVictims,
                        COUNT(DISTINCT dpa.KillerPlayer) as UniqueKillers,
                        -- Death probability by area
                        CAST(COUNT(*) AS FLOAT) / 
                            (SELECT COUNT(*) FROM DeathPositionAnalysis sub WHERE sub.MapName = dpa.MapName) * 100 as DeathProbabilityPercentage,
                        -- Average distance for deaths in this area
                        AVG(dpa.Distance) as AvgDeathDistance,
                        STDEV(dpa.Distance) as DeathDistanceVariation,
                        -- Most common weapons causing deaths here
                        COUNT(CASE WHEN dpa.Weapon LIKE '%ak47%' THEN 1 END) as AK47Deaths,
                        COUNT(CASE WHEN dpa.Weapon LIKE '%m4%' THEN 1 END) as M4Deaths,
                        COUNT(CASE WHEN dpa.Weapon LIKE '%awp%' THEN 1 END) as AWPDeaths,
                        COUNT(CASE WHEN dpa.Weapon LIKE '%knife%' THEN 1 END) as KnifeDeaths,
                        -- Movement pattern correlation
                        AVG(mpa.AvgSpeed) as AvgMovementSpeedAtDeath,
                        AVG(mpa.StationaryPercentage) as AvgStationaryPercentage,
                        AVG(mpa.XAxisCoverage) as AvgXAxisCoverage,
                        AVG(mpa.YAxisCoverage) as AvgYAxisCoverage,
                        -- Site control analysis
                        COUNT(CASE WHEN dpa.VictimTeam = 'Terrorist' THEN 1 END) as TerroristDeaths,
                        COUNT(CASE WHEN dpa.VictimTeam = 'CounterTerrorist' THEN 1 END) as CTDeaths,
                        CAST(COUNT(CASE WHEN dpa.VictimTeam = 'Terrorist' THEN 1 END) AS FLOAT) / 
                            COUNT(*) * 100 as TerroristDeathPercentage,
                        -- Round phase correlation
                        COUNT(CASE WHEN dpa.RoundPhase = 'Early Round' THEN 1 END) as EarlyRoundDeaths,
                        COUNT(CASE WHEN dpa.RoundPhase = 'Mid Round' THEN 1 END) as MidRoundDeaths,
                        COUNT(CASE WHEN dpa.RoundPhase = 'Late Round' THEN 1 END) as LateRoundDeaths,
                        -- Angle holding effectiveness
                        COUNT(CASE WHEN dpa.PositionType = 'Long Angle Hold' THEN 1 END) as LongAngleDeaths,
                        COUNT(CASE WHEN dpa.PositionType = 'Close Angle Hold' THEN 1 END) as CloseAngleDeaths,
                        COUNT(CASE WHEN dpa.PositionType = 'AWP Position' THEN 1 END) as AWPPositionDeaths,
                        CAST(COUNT(CASE WHEN dpa.PositionType = 'Long Angle Hold' THEN 1 END) AS FLOAT) / 
                            COUNT(*) * 100 as LongAngleDeathPercentage
                    FROM DeathPositionAnalysis dpa
                    LEFT JOIN MovementPatternAnalysis mpa ON dpa.VictimPlayer = mpa.PlayerName 
                        AND dpa.VictimTeam = mpa.Team 
                        AND dpa.MapName = mpa.MapName 
                        AND dpa.RoundNumber = mpa.RoundNumber
                    WHERE dpa.VictimPositionArea != 'Other' 
                        AND dpa.VictimPositionArea != 'Unknown Map'
                    GROUP BY dpa.MapName, dpa.VictimPositionArea, dpa.PositionType, dpa.EngagementRange, dpa.RoundPhase
                    HAVING COUNT(*) >= 3 -- Only show positions with at least 3 deaths
                    ORDER BY DeathProbabilityPercentage DESC, TotalDeaths DESC";

                var data = await ExecuteAnalyticsQuery(sql, query);
                
                if (query.Format?.ToLower() == "csv")
                {
                    var csv = ConvertToCsv(data);
                    var fileName = $"positioning_intelligence_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                    Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");
                    return File(Encoding.UTF8.GetBytes(csv), "text/csv");
                }

                return Ok(new
                {
                    Title = "Positioning Intelligence",
                    Description = "Death probability heatmaps, movement patterns, angle holding effectiveness, and site control analysis",
                    Data = data,
                    TotalRecords = data.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating positioning intelligence");
                return StatusCode(500, $"Error generating positioning intelligence: {ex.Message}");
            }
        }

        [HttpGet("team-coordination-metrics")]
        public async Task<IActionResult> GetTeamCoordinationMetrics([FromQuery] AnalyticsQuery query)
        {
            try
            {
                // Updated query using EnhancedPlayerPositions
                var sql = @"
                    WITH RotationAnalysis AS (
                        SELECT 
                            pp.PlayerId,
                            p.PlayerName,
                            p.Team,
                            d.MapName,
                            r.RoundNumber,
                            r.Id as RoundId,
                            pp.Tick,
                            pp.GameTime,
                            pp.PositionX,
                            pp.PositionY,
                            pp.PositionZ,
                            -- Calculate position area for rotation tracking
                            CASE 
                                WHEN d.MapName = 'de_dust2' THEN
                                    CASE 
                                        WHEN pp.PositionY > 1000 THEN 'A Site'
                                        WHEN pp.PositionY < -600 AND pp.PositionX < 500 THEN 'B Site'
                                        WHEN pp.PositionY < -600 AND pp.PositionX > 500 THEN 'Tunnels'
                                        WHEN ABS(pp.PositionY) < 400 THEN 'Mid'
                                        ELSE 'Rotate'
                                    END
                                WHEN d.MapName = 'de_mirage' THEN
                                    CASE 
                                        WHEN pp.PositionX > 600 THEN 'A Site'
                                        WHEN pp.PositionX < -600 THEN 'B Site' 
                                        WHEN ABS(pp.PositionX) < 300 THEN 'Mid'
                                        ELSE 'Rotate'
                                    END
                                ELSE 'Unknown'
                            END as CurrentArea,
                            -- Calculate if player is moving (speed > threshold)
                            CASE WHEN SQRT(POWER(pp.VelocityX, 2) + POWER(pp.VelocityY, 2)) > 100 THEN 1 ELSE 0 END as IsRotating,
                            -- Previous position for rotation detection
                            LAG(CASE 
                                WHEN d.MapName = 'de_dust2' THEN
                                    CASE 
                                        WHEN pp.PositionY > 1000 THEN 'A Site'
                                        WHEN pp.PositionY < -600 AND pp.PositionX < 500 THEN 'B Site'
                                        WHEN pp.PositionY < -600 AND pp.PositionX > 500 THEN 'Tunnels'
                                        WHEN ABS(pp.PositionY) < 400 THEN 'Mid'
                                        ELSE 'Rotate'
                                    END
                                WHEN d.MapName = 'de_mirage' THEN
                                    CASE 
                                        WHEN pp.PositionX > 600 THEN 'A Site'
                                        WHEN pp.PositionX < -600 THEN 'B Site' 
                                        WHEN ABS(pp.PositionX) < 300 THEN 'Mid'
                                        ELSE 'Rotate'
                                    END
                                ELSE 'Unknown'
                            END, 10) OVER (PARTITION BY pp.PlayerId, r.Id ORDER BY pp.Tick) as PreviousArea
                        FROM EnhancedPlayerPositions pp
                        INNER JOIN Players p ON pp.PlayerId = p.Id
                        INNER JOIN Rounds r ON pp.RoundId = r.Id
                        INNER JOIN DemoFiles d ON r.DemoFileId = d.Id
                        WHERE (@DemoId IS NULL OR d.Id = @DemoId)
                            AND (@MapName IS NULL OR d.MapName = @MapName)
                            AND (@Team IS NULL OR p.Team = @Team)
                            AND (@PlayerName IS NULL OR p.PlayerName = @PlayerName)
                            AND (@StartDate IS NULL OR d.ParsedAt >= @StartDate)
                            AND (@EndDate IS NULL OR d.ParsedAt <= @EndDate)
                    ),
                    StackFormationAnalysis AS (
                        SELECT 
                            ra.RoundId,
                            ra.Team,
                            ra.MapName,
                            ra.GameTime,
                            ra.CurrentArea,
                            COUNT(*) as PlayersInArea,
                            STRING_AGG(ra.PlayerName, ', ') as PlayersInStack,
                            AVG(ra.PositionX) as AvgStackX,
                            AVG(ra.PositionY) as AvgStackY,
                            -- Calculate stack spread (how close players are to each other)
                            CASE 
                                WHEN COUNT(*) >= 3 THEN 
                                    SQRT(POWER(MAX(ra.PositionX) - MIN(ra.PositionX), 2) + 
                                         POWER(MAX(ra.PositionY) - MIN(ra.PositionY), 2))
                                ELSE NULL
                            END as StackSpread,
                            -- Determine stack type
                            CASE 
                                WHEN COUNT(*) >= 4 THEN 'Full Stack'
                                WHEN COUNT(*) = 3 THEN 'Triple Stack'
                                WHEN COUNT(*) = 2 THEN 'Double Stack'
                                ELSE 'Solo'
                            END as StackType
                        FROM RotationAnalysis ra
                        WHERE ra.CurrentArea IN ('A Site', 'B Site', 'Mid')
                            AND ra.IsRotating = 0 -- Only stationary players
                        GROUP BY ra.RoundId, ra.Team, ra.MapName, ra.GameTime, ra.CurrentArea
                        HAVING COUNT(*) >= 2 -- Only consider actual stacks
                    ),
                    UtilityCoordinationAnalysis AS (
                        SELECT 
                            g.RoundId,
                            p.Team,
                            d.MapName,
                            g.ThrowTime as GameTime,
                            g.GrenadeType,
                            g.ThrowPositionX,
                            g.ThrowPositionY,
                            COUNT(*) OVER (PARTITION BY g.RoundId, p.Team, g.GrenadeType 
                                          ORDER BY g.ThrowTime 
                                          ROWS BETWEEN CURRENT ROW AND 3 FOLLOWING) as UtilityCombos,
                            -- Detect synchronized utility (multiple grenades within 3 seconds)
                            CASE WHEN COUNT(*) OVER (PARTITION BY g.RoundId, p.Team 
                                                    ORDER BY g.ThrowTime 
                                                    ROWS BETWEEN CURRENT ROW AND 3 FOLLOWING) > 1 
                                 THEN 1 ELSE 0 END as IsSynchronizedUtility
                        FROM Grenades g
                        INNER JOIN Players p ON g.PlayerId = p.Id
                        INNER JOIN Rounds r ON g.RoundId = r.Id
                        INNER JOIN DemoFiles d ON r.DemoFileId = d.Id
                        WHERE (@DemoId IS NULL OR d.Id = @DemoId)
                            AND (@MapName IS NULL OR d.MapName = @MapName)
                            AND (@Team IS NULL OR p.Team = @Team)
                            AND (@StartDate IS NULL OR d.ParsedAt >= @StartDate)
                            AND (@EndDate IS NULL OR d.ParsedAt <= @EndDate)
                    )
                    SELECT 
                        COALESCE(sfa.MapName, uca.MapName) as MapName,
                        COALESCE(sfa.Team, uca.Team) as Team,
                        -- Rotation timing analysis
                        COUNT(CASE WHEN ra.CurrentArea != ra.PreviousArea AND ra.PreviousArea IS NOT NULL THEN 1 END) as TotalRotations,
                        AVG(CASE WHEN ra.CurrentArea != ra.PreviousArea AND ra.PreviousArea IS NOT NULL THEN ra.GameTime END) as AvgRotationTime,
                        COUNT(CASE WHEN ra.CurrentArea != ra.PreviousArea AND ra.GameTime <= 30.0 THEN 1 END) as EarlyRotations,
                        COUNT(CASE WHEN ra.CurrentArea != ra.PreviousArea AND ra.GameTime > 60.0 THEN 1 END) as LateRotations,
                        CAST(COUNT(CASE WHEN ra.CurrentArea != ra.PreviousArea AND ra.GameTime <= 30.0 THEN 1 END) AS FLOAT) / 
                            NULLIF(COUNT(CASE WHEN ra.CurrentArea != ra.PreviousArea AND ra.PreviousArea IS NOT NULL THEN 1 END), 0) * 100 as EarlyRotationPercentage,
                        -- Stack effectiveness metrics
                        COUNT(DISTINCT sfa.RoundId) as RoundsWithStacks,
                        AVG(CAST(sfa.PlayersInArea AS FLOAT)) as AvgStackSize,
                        COUNT(CASE WHEN sfa.StackType = 'Full Stack' THEN 1 END) as FullStackCount,
                        COUNT(CASE WHEN sfa.StackType = 'Triple Stack' THEN 1 END) as TripleStackCount,
                        COUNT(CASE WHEN sfa.StackType = 'Double Stack' THEN 1 END) as DoubleStackCount,
                        AVG(sfa.StackSpread) as AvgStackSpread,
                        -- Stack area distribution
                        COUNT(CASE WHEN sfa.CurrentArea = 'A Site' THEN 1 END) as AStackCount,
                        COUNT(CASE WHEN sfa.CurrentArea = 'B Site' THEN 1 END) as BStackCount,
                        COUNT(CASE WHEN sfa.CurrentArea = 'Mid' THEN 1 END) as MidStackCount,
                        CAST(COUNT(CASE WHEN sfa.CurrentArea = 'A Site' THEN 1 END) AS FLOAT) / 
                            NULLIF(COUNT(*), 0) * 100 as AStackPercentage,
                        -- Spread formation analysis
                        COUNT(CASE WHEN sfa.StackSpread > 1000 THEN 1 END) as WideSpreadStacks,
                        COUNT(CASE WHEN sfa.StackSpread <= 500 THEN 1 END) as TightStacks,
                        CAST(COUNT(CASE WHEN sfa.StackSpread <= 500 THEN 1 END) AS FLOAT) / 
                            NULLIF(COUNT(CASE WHEN sfa.StackSpread IS NOT NULL THEN 1 END), 0) * 100 as TightStackPercentage,
                        -- Synchronized utility metrics
                        COUNT(CASE WHEN uca.IsSynchronizedUtility = 1 THEN 1 END) as SynchronizedUtilityCount,
                        COUNT(DISTINCT CASE WHEN uca.IsSynchronizedUtility = 1 THEN uca.RoundId END) as RoundsWithSyncUtility,
                        COUNT(CASE WHEN uca.GrenadeType = 'Flashbang' AND uca.UtilityCombos > 1 THEN 1 END) as FlashCombos,
                        COUNT(CASE WHEN uca.GrenadeType = 'Smoke Grenade' AND uca.UtilityCombos > 1 THEN 1 END) as SmokeCombos,
                        COUNT(CASE WHEN uca.GrenadeType = 'HE Grenade' AND uca.UtilityCombos > 1 THEN 1 END) as HECombos,
                        -- Utility timing analysis
                        AVG(CASE WHEN uca.IsSynchronizedUtility = 1 THEN uca.GameTime END) as AvgSyncUtilityTime,
                        COUNT(CASE WHEN uca.IsSynchronizedUtility = 1 AND uca.GameTime <= 15.0 THEN 1 END) as EarlySyncUtility,
                        COUNT(CASE WHEN uca.IsSynchronizedUtility = 1 AND uca.GameTime > 60.0 THEN 1 END) as LateSyncUtility,
                        -- Team coordination effectiveness
                        CAST(COUNT(DISTINCT CASE WHEN uca.IsSynchronizedUtility = 1 THEN uca.RoundId END) AS FLOAT) / 
                            NULLIF(COUNT(DISTINCT COALESCE(sfa.RoundId, uca.RoundId)), 0) * 100 as UtilityCoordinationRate,
                        -- Overall coordination score (weighted combination of metrics)
                        (CAST(COUNT(CASE WHEN sfa.StackSpread <= 500 THEN 1 END) AS FLOAT) / 
                         NULLIF(COUNT(CASE WHEN sfa.StackSpread IS NOT NULL THEN 1 END), 0) * 30 +
                         CAST(COUNT(CASE WHEN ra.CurrentArea != ra.PreviousArea AND ra.GameTime <= 30.0 THEN 1 END) AS FLOAT) / 
                         NULLIF(COUNT(CASE WHEN ra.CurrentArea != ra.PreviousArea AND ra.PreviousArea IS NOT NULL THEN 1 END), 0) * 25 +
                         CAST(COUNT(DISTINCT CASE WHEN uca.IsSynchronizedUtility = 1 THEN uca.RoundId END) AS FLOAT) / 
                         NULLIF(COUNT(DISTINCT COALESCE(sfa.RoundId, uca.RoundId)), 0) * 45) as CoordinationScore
                    FROM StackFormationAnalysis sfa
                    FULL OUTER JOIN UtilityCoordinationAnalysis uca ON sfa.RoundId = uca.RoundId 
                        AND sfa.Team = uca.Team 
                        AND sfa.MapName = uca.MapName
                    LEFT JOIN RotationAnalysis ra ON COALESCE(sfa.RoundId, uca.RoundId) = ra.RoundId 
                        AND COALESCE(sfa.Team, uca.Team) = ra.Team
                    WHERE (@Team IS NULL OR COALESCE(sfa.Team, uca.Team) = @Team)
                    GROUP BY COALESCE(sfa.MapName, uca.MapName), COALESCE(sfa.Team, uca.Team)
                    HAVING COUNT(DISTINCT COALESCE(sfa.RoundId, uca.RoundId)) >= 5 -- Minimum rounds for analysis
                    ORDER BY CoordinationScore DESC";

                var data = await ExecuteAnalyticsQuery(sql, query);
                
                if (query.Format?.ToLower() == "csv")
                {
                    var csv = ConvertToCsv(data);
                    var fileName = $"team_coordination_metrics_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                    Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");
                    return File(Encoding.UTF8.GetBytes(csv), "text/csv");
                }

                return Ok(new
                {
                    Title = "Team Coordination Metrics",
                    Description = "Analysis of rotation timing, stack effectiveness, spread formation, and synchronized utility usage",
                    Data = data,
                    TotalRecords = data.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating team coordination metrics");
                return StatusCode(500, $"Error generating team coordination metrics: {ex.Message}");
            }
        }

        [HttpGet("performance-consistency-profiling")]
        public async Task<IActionResult> GetPerformanceConsistencyProfiling([FromQuery] AnalyticsQuery query)
        {
            try
            {
                var sql = @"
                    WITH PlayerStats AS (
                        SELECT 
                            p.PlayerName,
                            p.Team,
                            d.MapName,
                            COUNT(*) as TotalRounds,
                            AVG(CAST(prs.Kills AS FLOAT)) as AverageKills,
                            AVG(CAST(prs.Deaths AS FLOAT)) as AverageDeaths,
                            AVG(CAST(prs.Assists AS FLOAT)) as AverageAssists,
                            AVG(CAST(prs.Damage AS FLOAT)) as AverageDamage,
                            AVG(prs.Kills * 0.6 + prs.Assists * 0.2 + (prs.Damage / 100.0) * 0.1) as AverageRating,
                            STDEV(CAST(prs.Kills AS FLOAT)) as KillVariance,
                            STDEV(CAST(prs.Damage AS FLOAT)) as DamageVariance,
                            MAX(prs.Kills) as BestKillRound,
                            MIN(prs.Kills) as WorstKillRound,
                            MAX(prs.Damage) as HighestDamageRound,
                            MIN(prs.Damage) as LowestDamageRound,
                            AVG(CASE WHEN prs.EquipmentValue >= 3000 THEN CAST(prs.Kills AS FLOAT) END) as FullBuyKills,
                            AVG(CASE WHEN prs.EquipmentValue < 1500 THEN CAST(prs.Kills AS FLOAT) END) as EcoKills
                        FROM PlayerRoundStats prs
                        INNER JOIN Players p ON prs.PlayerId = p.Id
                        INNER JOIN Rounds r ON prs.RoundId = r.Id
                        INNER JOIN DemoFiles d ON r.DemoFileId = d.Id
                        WHERE (@DemoId IS NULL OR d.Id = @DemoId)
                            AND (@MapName IS NULL OR d.MapName = @MapName)
                            AND (@Team IS NULL OR p.Team = @Team)
                            AND (@PlayerName IS NULL OR p.PlayerName = @PlayerName)
                            AND (@StartDate IS NULL OR d.ParsedAt >= @StartDate)
                            AND (@EndDate IS NULL OR d.ParsedAt <= @EndDate)
                        GROUP BY p.PlayerName, p.Team, d.MapName
                        HAVING COUNT(*) >= 5
                    )
                    SELECT 
                        PlayerName,
                        Team,
                        MapName,
                        TotalRounds,
                        AverageKills,
                        AverageDeaths,
                        AverageAssists,
                        AverageDamage,
                        AverageRating,
                        KillVariance,
                        DamageVariance,
                        BestKillRound,
                        WorstKillRound,
                        HighestDamageRound,
                        LowestDamageRound,
                        FullBuyKills,
                        EcoKills,
                        CASE 
                            WHEN KillVariance <= 0.8 THEN 'Very Consistent'
                            WHEN KillVariance <= 1.2 THEN 'Consistent'
                            WHEN KillVariance <= 1.8 THEN 'Moderate'
                            WHEN KillVariance <= 2.5 THEN 'Inconsistent'
                            ELSE 'Very Inconsistent'
                        END as ConsistencyLevel,
                        CASE 
                            WHEN KillVariance <= 0.8 THEN 90
                            WHEN KillVariance <= 1.2 THEN 75
                            WHEN KillVariance <= 1.8 THEN 60
                            WHEN KillVariance <= 2.5 THEN 40
                            ELSE 20
                        END as ConsistencyScore
                    FROM PlayerStats
                    ORDER BY ConsistencyScore DESC, AverageRating DESC";

                var data = await ExecuteAnalyticsQuery(sql, query);
                
                if (query.Format?.ToLower() == "csv")
                {
                    var csv = ConvertToCsv(data);
                    var fileName = $"performance_consistency_profiling_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                    Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");
                    return File(Encoding.UTF8.GetBytes(csv), "text/csv");
                }

                return Ok(new
                {
                    Title = "Performance Consistency Profiling",
                    Description = "Tilt detection, variance tracking, and adaptation analysis for player performance consistency",
                    Data = data,
                    TotalRecords = data.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating performance consistency profiling");
                return StatusCode(500, $"Error generating performance consistency profiling: {ex.Message}");
            }
        }

        [HttpGet("damage-efficiency-analytics")]
        public async Task<IActionResult> GetDamageEfficiencyAnalytics([FromQuery] AnalyticsQuery query)
        {
            try
            {
                var sql = @"
                    WITH DamageBreakdownAnalysis AS (
                        SELECT 
                            attacker.PlayerName as AttackerName,
                            attacker.Team as AttackerTeam,
                            victim.PlayerName as VictimName,
                            victim.Team as VictimTeam,
                            k.Weapon,
                            k.Damage as DamageDealt,
                            k.VictimHealth,
                            k.VictimArmor,
                            k.Distance,
                            k.IsHeadshot,
                            k.GameTime,
                            d.MapName,
                            r.RoundNumber,
                            r.Id as RoundId,
                            -- Calculate damage efficiency metrics
                            CASE WHEN k.VictimHealth <= k.Damage THEN k.VictimHealth ELSE k.Damage END as EffectiveDamage,
                            CASE 
                                WHEN k.VictimHealth <= k.Damage THEN 0 
                                ELSE k.Damage - k.VictimHealth 
                            END as WastedDamage,
                            -- Armor penetration analysis
                            CASE 
                                WHEN k.VictimArmor > 0 AND k.Weapon LIKE '%ak47%' THEN k.Damage * 0.77  -- AK armor pen
                                WHEN k.VictimArmor > 0 AND k.Weapon LIKE '%m4%' THEN k.Damage * 0.70   -- M4 armor pen
                                WHEN k.VictimArmor > 0 AND k.Weapon LIKE '%awp%' THEN k.Damage         -- AWP ignores armor
                                WHEN k.VictimArmor > 0 THEN k.Damage * 0.50                            -- Other weapons
                                ELSE k.Damage
                            END as ArmorPenetratedDamage,
                            -- Weapon class categorization
                            CASE 
                                WHEN k.Weapon LIKE '%ak47%' OR k.Weapon LIKE '%m4%' THEN 'Rifle'
                                WHEN k.Weapon LIKE '%awp%' OR k.Weapon LIKE '%ssg08%' THEN 'Sniper'
                                WHEN k.Weapon LIKE '%glock%' OR k.Weapon LIKE '%usp%' OR k.Weapon LIKE '%p250%' THEN 'Pistol'
                                WHEN k.Weapon LIKE '%mp5%' OR k.Weapon LIKE '%ump%' OR k.Weapon LIKE '%p90%' THEN 'SMG'
                                WHEN k.Weapon LIKE '%nova%' OR k.Weapon LIKE '%xm1014%' THEN 'Shotgun'
                                ELSE 'Other'
                            END as WeaponClass,
                            -- Range categorization for damage falloff analysis
                            CASE 
                                WHEN k.Distance <= 500 THEN 'Close'
                                WHEN k.Distance <= 1500 THEN 'Medium'
                                WHEN k.Distance <= 3000 THEN 'Long'
                                ELSE 'Very Long'
                            END as EngagementRange,
                            -- Finishing ability (kill vs damage)
                            CASE WHEN k.VictimHealth <= k.Damage THEN 1 ELSE 0 END as WasKill
                        FROM Kills k
                        INNER JOIN Players attacker ON k.KillerId = attacker.Id
                        INNER JOIN Players victim ON k.VictimId = victim.Id
                        INNER JOIN Rounds r ON k.RoundId = r.Id
                        INNER JOIN DemoFiles d ON r.DemoFileId = d.Id
                        WHERE (@DemoId IS NULL OR d.Id = @DemoId)
                            AND (@MapName IS NULL OR d.MapName = @MapName)
                            AND (@Team IS NULL OR attacker.Team = @Team OR victim.Team = @Team)
                            AND (@PlayerName IS NULL OR attacker.PlayerName = @PlayerName OR victim.PlayerName = @PlayerName)
                            AND (@StartDate IS NULL OR d.ParsedAt >= @StartDate)
                            AND (@EndDate IS NULL OR d.ParsedAt <= @EndDate)
                            AND k.Damage > 0  -- Only meaningful damage events
                    ),
                    MultiTargetAnalysis AS (
                        SELECT 
                            dba.AttackerName,
                            dba.AttackerTeam,
                            dba.MapName,
                            dba.RoundId,
                            dba.GameTime,
                            COUNT(DISTINCT dba.VictimName) as TargetsHit,
                            SUM(dba.DamageDealt) as TotalDamageInWindow,
                            COUNT(*) as ShotsInWindow,
                            AVG(dba.Distance) as AvgEngagementDistance,
                            -- Multi-target engagement detection (within 5 seconds)
                            COUNT(*) OVER (PARTITION BY dba.AttackerName, dba.RoundId 
                                          ORDER BY dba.GameTime 
                                          ROWS BETWEEN CURRENT ROW AND 5 FOLLOWING) as MultiTargetWindow
                        FROM DamageBreakdownAnalysis dba
                        GROUP BY dba.AttackerName, dba.AttackerTeam, dba.MapName, dba.RoundId, dba.GameTime
                    ),
                    WasteIdentificationAnalysis AS (
                        SELECT 
                            dba.AttackerName,
                            dba.AttackerTeam,
                            dba.MapName,
                            dba.WeaponClass,
                            dba.EngagementRange,
                            -- Damage waste metrics
                            COUNT(*) as TotalDamageEvents,
                            SUM(dba.DamageDealt) as TotalDamageOutput,
                            SUM(dba.EffectiveDamage) as TotalEffectiveDamage,
                            SUM(dba.WastedDamage) as TotalWastedDamage,
                            CAST(SUM(dba.WastedDamage) AS FLOAT) / SUM(dba.DamageDealt) * 100 as DamageWastePercentage,
                            -- Finishing ability metrics
                            SUM(dba.WasKill) as SuccessfulKills,
                            CAST(SUM(dba.WasKill) AS FLOAT) / COUNT(*) * 100 as FinishingPercentage,
                            -- Armor penetration effectiveness
                            COUNT(CASE WHEN dba.VictimArmor > 0 THEN 1 END) as ArmoredTargets,
                            AVG(CASE WHEN dba.VictimArmor > 0 THEN dba.ArmorPenetratedDamage END) as AvgArmorPenDamage,
                            AVG(CASE WHEN dba.VictimArmor = 0 THEN dba.DamageDealt END) as AvgUnarmoredDamage,
                            -- Headshot efficiency
                            COUNT(CASE WHEN dba.IsHeadshot = 1 THEN 1 END) as HeadshotCount,
                            CAST(COUNT(CASE WHEN dba.IsHeadshot = 1 THEN 1 END) AS FLOAT) / COUNT(*) * 100 as HeadshotPercentage,
                            AVG(CASE WHEN dba.IsHeadshot = 1 THEN dba.DamageDealt END) as AvgHeadshotDamage,
                            -- Distance effectiveness
                            AVG(dba.Distance) as AvgEngagementDistance,
                            STDEV(dba.Distance) as DistanceConsistency
                        FROM DamageBreakdownAnalysis dba
                        GROUP BY dba.AttackerName, dba.AttackerTeam, dba.MapName, dba.WeaponClass, dba.EngagementRange
                        HAVING COUNT(*) >= 5 -- Minimum damage events for analysis
                    )
                    SELECT 
                        wia.AttackerName as PlayerName,
                        wia.AttackerTeam as Team,
                        wia.MapName,
                        wia.WeaponClass,
                        wia.EngagementRange,
                        -- Core damage metrics
                        wia.TotalDamageEvents as DamageEvents,
                        wia.TotalDamageOutput as TotalDamage,
                        wia.TotalEffectiveDamage as EffectiveDamage,
                        wia.TotalWastedDamage as WastedDamage,
                        ROUND(wia.DamageWastePercentage, 2) as DamageWastePercentage,
                        -- Finishing ability
                        wia.SuccessfulKills as Kills,
                        ROUND(wia.FinishingPercentage, 2) as FinishingPercentage,
                        ROUND(wia.TotalDamageOutput / CAST(wia.SuccessfulKills AS FLOAT), 1) as DamagePerKill,
                        -- Armor penetration analysis
                        wia.ArmoredTargets,
                        ROUND(wia.AvgArmorPenDamage, 1) as AvgArmorPenDamage,
                        ROUND(wia.AvgUnarmoredDamage, 1) as AvgUnarmoredDamage,
                        ROUND(wia.AvgArmorPenDamage - wia.AvgUnarmoredDamage, 1) as ArmorDamageReduction,
                        -- Headshot efficiency
                        wia.HeadshotCount as Headshots,
                        ROUND(wia.HeadshotPercentage, 2) as HeadshotPercentage,
                        ROUND(wia.AvgHeadshotDamage, 1) as AvgHeadshotDamage,
                        -- Multi-target capability
                        COUNT(CASE WHEN mta.TargetsHit >= 2 THEN 1 END) as MultiTargetRounds,
                        AVG(CASE WHEN mta.TargetsHit >= 2 THEN mta.TargetsHit END) as AvgTargetsPerMultiKill,
                        MAX(mta.TargetsHit) as MaxTargetsInRound,
                        SUM(CASE WHEN mta.TargetsHit >= 2 THEN mta.TotalDamageInWindow END) as MultiTargetDamage,
                        -- Distance and positioning
                        ROUND(wia.AvgEngagementDistance, 1) as AvgDistance,
                        ROUND(wia.DistanceConsistency, 1) as DistanceVariation,
                        -- Overall efficiency scores
                        ROUND(100 - wia.DamageWastePercentage, 2) as DamageEfficiencyScore,
                        ROUND((wia.FinishingPercentage * 0.4) + 
                              ((100 - wia.DamageWastePercentage) * 0.3) + 
                              (wia.HeadshotPercentage * 0.2) + 
                              (CASE WHEN COUNT(CASE WHEN mta.TargetsHit >= 2 THEN 1 END) > 0 THEN 10 ELSE 0 END), 2) as OverallEfficiencyScore,
                        -- Weapon-specific analysis
                        CASE 
                            WHEN wia.WeaponClass = 'Rifle' AND wia.DamageWastePercentage < 15 THEN 'Excellent Rifle Control'
                            WHEN wia.WeaponClass = 'Sniper' AND wia.FinishingPercentage > 80 THEN 'Elite Sniper'
                            WHEN wia.WeaponClass = 'Pistol' AND wia.HeadshotPercentage > 30 THEN 'Pistol Expert'
                            WHEN wia.WeaponClass = 'SMG' AND COUNT(CASE WHEN mta.TargetsHit >= 2 THEN 1 END) > 0 THEN 'SMG Spray Master'
                            WHEN wia.DamageWastePercentage > 40 THEN 'Needs Improvement'
                            ELSE 'Standard Performance'
                        END as PerformanceCategory
                    FROM WasteIdentificationAnalysis wia
                    LEFT JOIN MultiTargetAnalysis mta ON wia.AttackerName = mta.AttackerName 
                        AND wia.AttackerTeam = mta.AttackerTeam 
                        AND wia.MapName = mta.MapName
                    WHERE (@Team IS NULL OR wia.AttackerTeam = @Team)
                    GROUP BY wia.AttackerName, wia.AttackerTeam, wia.MapName, wia.WeaponClass, wia.EngagementRange,
                             wia.TotalDamageEvents, wia.TotalDamageOutput, wia.TotalEffectiveDamage, wia.TotalWastedDamage,
                             wia.DamageWastePercentage, wia.SuccessfulKills, wia.FinishingPercentage,
                             wia.ArmoredTargets, wia.AvgArmorPenDamage, wia.AvgUnarmoredDamage,
                             wia.HeadshotCount, wia.HeadshotPercentage, wia.AvgHeadshotDamage,
                             wia.AvgEngagementDistance, wia.DistanceConsistency
                    HAVING COUNT(DISTINCT mta.RoundId) >= 3 -- Minimum rounds for comprehensive analysis
                    ORDER BY OverallEfficiencyScore DESC, DamageEfficiencyScore DESC";

                var data = await ExecuteAnalyticsQuery(sql, query);
                
                if (query.Format?.ToLower() == "csv")
                {
                    var csv = ConvertToCsv(data);
                    var fileName = $"damage_efficiency_analytics_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                    Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");
                    return File(Encoding.UTF8.GetBytes(csv), "text/csv");
                }

                return Ok(new
                {
                    Title = "Damage Efficiency Analytics",
                    Description = "Comprehensive analysis of damage waste, armor penetration, finishing ability, and multi-target tracking",
                    Data = data,
                    TotalRecords = data.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating damage efficiency analytics");
                return StatusCode(500, $"Error generating damage efficiency analytics: {ex.Message}");
            }
        }

        [HttpGet("comprehensive-clutch-analysis")]
        public async Task<IActionResult> GetComprehensiveClutchAnalysis([FromQuery] AnalyticsQuery query)
        {
            try
            {
                var sql = @"
                    WITH ClutchSetupAnalysis AS (
                        SELECT 
                            k.RoundId,
                            k.Id as KillId,
                            k.GameTime,
                            clutch_player.PlayerName as ClutchPlayer,
                            clutch_player.Team as ClutchTeam,
                            prs.Kills as ClutchSize,  -- Use kills from PlayerRoundStats as proxy for clutch size
                            100 as KillerHealth,  -- Default health values since not available
                            0 as KillerArmor,
                            k.Weapon,
                            SQRT(POWER(CAST(k.KillerPositionX - k.VictimPositionX AS FLOAT), 2) + POWER(CAST(k.KillerPositionY - k.VictimPositionY AS FLOAT), 2)) as Distance,
                            k.KillerPositionX,
                            k.KillerPositionY,
                            0.0 as KillerPositionZ,  -- Z position not available
                            d.MapName,
                            d.FileName,
                            r.RoundNumber,
                            r.WinnerTeam,
                            -- Economic context for clutch
                            prs.EquipmentValue as ClutchPlayerEquipment,
                            prs.MoneySpent as ClutchPlayerMoneySpent,
                            -- Determine clutch outcome
                            CASE WHEN clutch_player.Team = r.WinnerTeam THEN 1 ELSE 0 END as ClutchWon,
                            -- Categorize clutch setup positioning
                            CASE 
                                WHEN d.MapName = 'de_dust2' THEN
                                    CASE 
                                        WHEN k.KillerPositionY > 1000 THEN 'A Site'
                                        WHEN k.KillerPositionY < -1000 THEN 'B Site'
                                        WHEN k.KillerPositionX > 0 THEN 'Upper Tunnels'
                                        ELSE 'Mid/Lower'
                                    END
                                WHEN d.MapName = 'de_mirage' THEN
                                    CASE 
                                        WHEN k.KillerPositionX > 1000 THEN 'A Site'
                                        WHEN k.KillerPositionX < -1000 THEN 'B Site'
                                        WHEN ABS(k.KillerPositionX) < 500 THEN 'Mid'
                                        ELSE 'Connector'
                                    END
                                ELSE 'Unknown Area'
                            END as ClutchArea,
                            -- Health/Armor condition categories
                            CASE 
                                WHEN k.KillerHealth >= 80 THEN 'Healthy'
                                WHEN k.KillerHealth >= 50 THEN 'Moderate'
                                WHEN k.KillerHealth >= 20 THEN 'Low'
                                ELSE 'Critical'
                            END as HealthCondition,
                            -- Equipment investment level
                            CASE 
                                WHEN prs.EquipmentValue >= 4000 THEN 'Full Buy'
                                WHEN prs.EquipmentValue >= 2000 THEN 'Partial Buy'
                                WHEN prs.EquipmentValue >= 1000 THEN 'Light Buy'
                                ELSE 'Eco'
                            END as EquipmentLevel
                        FROM Kills k
                        INNER JOIN Players clutch_player ON k.KillerId = clutch_player.Id
                        INNER JOIN Rounds r ON k.RoundId = r.Id
                        INNER JOIN DemoFiles d ON r.DemoFileId = d.Id
                        LEFT JOIN PlayerRoundStats prs ON prs.PlayerId = clutch_player.Id AND prs.RoundId = r.Id
                        WHERE EXISTS (
                            SELECT 1 FROM PlayerRoundStats prs2 
                            WHERE prs2.RoundId = r.Id 
                            AND prs2.PlayerId = clutch_player.Id 
                            AND prs2.Kills >= 2  -- Focus on multi-kill scenarios as proxy for clutch situations
                        )
                            AND (@DemoId IS NULL OR d.Id = @DemoId)
                            AND (@MapName IS NULL OR d.MapName = @MapName)
                            AND (@PlayerName IS NULL OR clutch_player.PlayerName = @PlayerName)
                            AND (@StartDate IS NULL OR d.ParsedAt >= @StartDate)
                            AND (@EndDate IS NULL OR d.ParsedAt <= @EndDate)
                    ),
                    MultiClutchPlayers AS (
                        SELECT 
                            ClutchPlayer,
                            ClutchTeam,
                            MapName,
                            COUNT(*) as TotalClutchAttempts,
                            SUM(ClutchWon) as TotalClutchWins,
                            CAST(SUM(ClutchWon) AS FLOAT) / COUNT(*) * 100 as OverallClutchSuccessRate,
                            -- Breakdown by clutch size
                            COUNT(CASE WHEN ClutchSize = 2 THEN 1 END) as Clutch1v2Attempts,
                            SUM(CASE WHEN ClutchSize = 2 AND ClutchWon = 1 THEN 1 ELSE 0 END) as Clutch1v2Wins,
                            COUNT(CASE WHEN ClutchSize = 3 THEN 1 END) as Clutch1v3Attempts,
                            SUM(CASE WHEN ClutchSize = 3 AND ClutchWon = 1 THEN 1 ELSE 0 END) as Clutch1v3Wins,
                            COUNT(CASE WHEN ClutchSize >= 4 THEN 1 END) as Clutch1v4PlusAttempts,
                            SUM(CASE WHEN ClutchSize >= 4 AND ClutchWon = 1 THEN 1 ELSE 0 END) as Clutch1v4PlusWins,
                            -- Positioning analysis
                            COUNT(DISTINCT ClutchArea) as UniqueClutchAreas,
                            -- Health condition success rates
                            AVG(CASE WHEN HealthCondition = 'Healthy' THEN CAST(ClutchWon AS FLOAT) ELSE NULL END) * 100 as HealthyClutchSuccessRate,
                            AVG(CASE WHEN HealthCondition = 'Low' THEN CAST(ClutchWon AS FLOAT) ELSE NULL END) * 100 as LowHealthClutchSuccessRate,
                            -- Equipment correlation
                            AVG(CASE WHEN EquipmentLevel = 'Full Buy' THEN CAST(ClutchWon AS FLOAT) ELSE NULL END) * 100 as FullBuyClutchSuccessRate,
                            AVG(CASE WHEN EquipmentLevel = 'Eco' THEN CAST(ClutchWon AS FLOAT) ELSE NULL END) * 100 as EcoClutchSuccessRate,
                            -- Weapon preferences
                            COUNT(CASE WHEN Weapon LIKE '%ak47%' OR Weapon LIKE '%m4%' THEN 1 END) as RifleClutches,
                            COUNT(CASE WHEN Weapon LIKE '%awp%' THEN 1 END) as AwpClutches,
                            COUNT(CASE WHEN Weapon LIKE '%deagle%' OR Weapon LIKE '%glock%' OR Weapon LIKE '%usp%' THEN 1 END) as PistolClutches,
                            -- Economic efficiency
                            AVG(ClutchPlayerEquipment) as AvgClutchEquipmentValue,
                            AVG(Distance) as AvgClutchKillDistance,
                            -- Multi-round clutch streaks detection
                            COUNT(DISTINCT FileName) as DemosWithClutches
                        FROM ClutchSetupAnalysis
                        WHERE (@Team IS NULL OR ClutchTeam = @Team)
                        GROUP BY ClutchPlayer, ClutchTeam, MapName
                        HAVING COUNT(*) >= 3  -- Only players with at least 3 clutch attempts
                    )
                    SELECT 
                        ClutchPlayer,
                        ClutchTeam,
                        MapName,
                        TotalClutchAttempts,
                        TotalClutchWins,
                        OverallClutchSuccessRate,
                        -- Clutch size breakdown
                        Clutch1v2Attempts,
                        Clutch1v2Wins,
                        CASE WHEN Clutch1v2Attempts > 0 THEN CAST(Clutch1v2Wins AS FLOAT) / Clutch1v2Attempts * 100 ELSE 0 END as Clutch1v2SuccessRate,
                        Clutch1v3Attempts,
                        Clutch1v3Wins,
                        CASE WHEN Clutch1v3Attempts > 0 THEN CAST(Clutch1v3Wins AS FLOAT) / Clutch1v3Attempts * 100 ELSE 0 END as Clutch1v3SuccessRate,
                        Clutch1v4PlusAttempts,
                        Clutch1v4PlusWins,
                        CASE WHEN Clutch1v4PlusAttempts > 0 THEN CAST(Clutch1v4PlusWins AS FLOAT) / Clutch1v4PlusAttempts * 100 ELSE 0 END as Clutch1v4PlusSuccessRate,
                        -- Performance factors
                        UniqueClutchAreas,
                        ISNULL(HealthyClutchSuccessRate, 0) as HealthyClutchSuccessRate,
                        ISNULL(LowHealthClutchSuccessRate, 0) as LowHealthClutchSuccessRate,
                        ISNULL(FullBuyClutchSuccessRate, 0) as FullBuyClutchSuccessRate,
                        ISNULL(EcoClutchSuccessRate, 0) as EcoClutchSuccessRate,
                        -- Weapon analysis
                        RifleClutches,
                        AwpClutches,
                        PistolClutches,
                        -- Efficiency metrics
                        AvgClutchEquipmentValue,
                        AvgClutchKillDistance,
                        DemosWithClutches
                    FROM MultiClutchPlayers
                    ORDER BY OverallClutchSuccessRate DESC, TotalClutchAttempts DESC";

                var data = await ExecuteAnalyticsQuery(sql, query);
                
                if (query.Format?.ToLower() == "csv")
                {
                    var csv = ConvertToCsv(data);
                    var fileName = $"comprehensive_clutch_analysis_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                    Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");
                    return File(Encoding.UTF8.GetBytes(csv), "text/csv");
                }

                return Ok(new
                {
                    Title = "Comprehensive Clutch Analysis",
                    Description = "Deep analysis of clutch situations including positioning, health/armor states, economic context, and multi-clutch performance",
                    Data = data,
                    TotalRecords = data.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating comprehensive clutch analysis");
                return StatusCode(500, $"Error generating comprehensive clutch analysis: {ex.Message}");
            }
        }

        [HttpGet("positioning-analysis")]
        public async Task<IActionResult> GetPositioningAnalysis([FromQuery] AnalyticsQuery query)
        {
            try
            {
                var sql = @"
                    WITH PositionalData AS (
                        SELECT 
                            p.PlayerName,
                            p.Team,
                            d.MapName,
                            r.RoundNumber,
                            
                            -- SIMPLIFIED POSITIONING - Use round half for positioning
                            CASE 
                                WHEN r.RoundNumber <= 15 THEN 'First_Half'
                                ELSE 'Second_Half'
                            END as PositionContext,
                            
                            -- ROUND OUTCOME
                            CASE WHEN p.Team = r.WinnerTeam THEN 1 ELSE 0 END as RoundWon,
                            
                            -- PLAYER PERFORMANCE
                            prs.Kills,
                            prs.Deaths,
                            prs.Assists,
                            prs.Damage,
                            prs.IsAlive
                            
                        FROM Players p
                        INNER JOIN PlayerRoundStats prs ON p.Id = prs.PlayerId
                        INNER JOIN Rounds r ON prs.RoundId = r.Id
                        INNER JOIN DemoFiles d ON r.DemoFileId = d.Id
                        WHERE 1=1";

                // Apply filters
                if (!string.IsNullOrEmpty(query.MapName))
                    sql += " AND d.MapName = @MapName";
                if (!string.IsNullOrEmpty(query.PlayerName))
                    sql += " AND p.PlayerName = @PlayerName";
                if (!string.IsNullOrEmpty(query.Team))
                    sql += " AND p.Team = @Team";
                if (query.RoundNumber.HasValue)
                    sql += " AND r.RoundNumber = @RoundNumber";

                sql += @"
                    )
                    SELECT 
                        PlayerName,
                        Team,
                        MapName,
                        PositionContext,
                        
                        -- PERFORMANCE METRICS
                        COUNT(*) as RoundsPlayed,
                        SUM(RoundWon) as RoundsWon,
                        ROUND((SUM(RoundWon) * 100.0 / COUNT(*)), 2) as WinPercentage,
                        
                        -- COMBAT EFFECTIVENESS
                        SUM(Kills) as TotalKills,
                        SUM(Deaths) as TotalDeaths,
                        SUM(Assists) as TotalAssists,
                        SUM(Damage) as TotalDamage,
                        
                        -- RATIOS
                        ROUND(CASE WHEN SUM(Deaths) > 0 THEN SUM(Kills) * 1.0 / SUM(Deaths) ELSE SUM(Kills) END, 2) as KDRatio,
                        ROUND(SUM(Damage) * 1.0 / COUNT(*), 2) as AvgDamagePerRound,
                        
                        -- SURVIVAL METRICS
                        SUM(CASE WHEN IsAlive = 1 THEN 1 ELSE 0 END) as RoundsSurvived,
                        ROUND((SUM(CASE WHEN IsAlive = 1 THEN 1 ELSE 0 END) * 100.0 / COUNT(*)), 2) as SurvivalRate
                        
                    FROM PositionalData
                    GROUP BY PlayerName, Team, MapName, PositionContext
                    HAVING COUNT(*) >= 3
                    ORDER BY WinPercentage DESC, KDRatio DESC";

                var data = await ExecuteAnalyticsQuery(sql, query);
                
                if (query.Format?.ToLower() == "csv")
                {
                    var csv = ConvertToCsv(data);
                    var fileName = $"positioning_analysis_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                    Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");
                    return File(Encoding.UTF8.GetBytes(csv), "text/csv");
                }

                return Ok(new
                {
                    Title = "Positioning Analysis",
                    Description = "Performance analysis by game position (first half vs second half) showing combat effectiveness and survival rates",
                    Data = data,
                    TotalRecords = data.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating positioning analysis");
                return StatusCode(500, $"Error generating positioning analysis: {ex.Message}");
            }
        }

        [HttpGet("weapon-intelligence")]
        public async Task<IActionResult> GetWeaponIntelligence([FromQuery] AnalyticsQuery query)
        {
            try
            {
                var sql = @"
                    WITH WeaponData AS (
                        SELECT 
                            p.PlayerName,
                            p.Team,
                            d.MapName,
                            r.RoundNumber,
                            
                            -- WEAPON ANALYSIS BASED ON KILLS
                            k.Weapon,
                            CASE 
                                WHEN k.Weapon LIKE '%ak47%' OR k.Weapon LIKE '%m4a%' THEN 'Rifle'
                                WHEN k.Weapon LIKE '%awp%' OR k.Weapon LIKE '%ssg08%' THEN 'Sniper'
                                WHEN k.Weapon LIKE '%glock%' OR k.Weapon LIKE '%usp%' OR k.Weapon LIKE '%p250%' THEN 'Pistol'
                                WHEN k.Weapon LIKE '%mp%' OR k.Weapon LIKE '%mac10%' OR k.Weapon LIKE '%ump%' THEN 'SMG'
                                ELSE 'Other'
                            END as WeaponClass,
                            
                            -- KILL METRICS
                            CASE WHEN k.IsHeadshot = 1 THEN 1 ELSE 0 END as IsHeadshot,
                            k.Distance,
                            
                            -- ROUND CONTEXT
                            CASE WHEN p.Team = r.WinnerTeam THEN 1 ELSE 0 END as RoundWon,
                            prs.Damage as RoundDamage
                            
                        FROM Players p
                        INNER JOIN PlayerRoundStats prs ON p.Id = prs.PlayerId
                        INNER JOIN Rounds r ON prs.RoundId = r.Id
                        INNER JOIN DemoFiles d ON r.DemoFileId = d.Id
                        LEFT JOIN Kills k ON p.Id = k.KillerId AND k.RoundId = r.Id
                        WHERE k.Weapon IS NOT NULL";

                // Apply filters
                if (!string.IsNullOrEmpty(query.MapName))
                    sql += " AND d.MapName = @MapName";
                if (!string.IsNullOrEmpty(query.PlayerName))
                    sql += " AND p.PlayerName = @PlayerName";
                if (!string.IsNullOrEmpty(query.Team))
                    sql += " AND p.Team = @Team";
                if (query.RoundNumber.HasValue)
                    sql += " AND r.RoundNumber = @RoundNumber";

                sql += @"
                    )
                    SELECT 
                        PlayerName,
                        Team,
                        MapName,
                        Weapon,
                        WeaponClass,
                        
                        -- WEAPON USAGE METRICS
                        COUNT(*) as KillsWithWeapon,
                        SUM(CAST(IsHeadshot AS INT)) as HeadshotKills,
                        ROUND((SUM(CAST(IsHeadshot AS INT)) * 100.0 / COUNT(*)), 2) as HeadshotRate,
                        
                        -- DISTANCE ANALYSIS
                        ROUND(AVG(Distance), 2) as AvgKillDistance,
                        ROUND(MIN(Distance), 2) as MinKillDistance,
                        ROUND(MAX(Distance), 2) as MaxKillDistance,
                        
                        -- ROUND IMPACT
                        SUM(RoundWon) as RoundsWonWithWeapon,
                        ROUND((SUM(RoundWon) * 100.0 / COUNT(*)), 2) as WinRateWithWeapon,
                        ROUND(AVG(RoundDamage), 2) as AvgRoundDamage,
                        
                        -- WEAPON PROFICIENCY SCORE (0-100)
                        CASE 
                            WHEN ROUND((SUM(CAST(IsHeadshot AS INT)) * 100.0 / COUNT(*)), 2) >= 60 AND COUNT(*) >= 10 THEN 95
                            WHEN ROUND((SUM(CAST(IsHeadshot AS INT)) * 100.0 / COUNT(*)), 2) >= 40 AND COUNT(*) >= 5 THEN 80
                            WHEN ROUND((SUM(RoundWon) * 100.0 / COUNT(*)), 2) >= 70 THEN 75
                            WHEN COUNT(*) >= 10 THEN 60
                            ELSE 40
                        END as WeaponProficiencyScore
                        
                    FROM WeaponData  
                    GROUP BY PlayerName, Team, MapName, Weapon, WeaponClass
                    HAVING COUNT(*) >= 2
                    ORDER BY WeaponProficiencyScore DESC, HeadshotRate DESC";

                var data = await ExecuteAnalyticsQuery(sql, query);
                
                if (query.Format?.ToLower() == "csv")
                {
                    var csv = ConvertToCsv(data);
                    var fileName = $"weapon_intelligence_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                    Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");
                    return File(Encoding.UTF8.GetBytes(csv), "text/csv");
                }

                return Ok(new
                {
                    Title = "Advanced Weapon Intelligence",
                    Description = "Ammo management, spray control, movement shooting, weapon configuration, and handicapped performance analysis",
                    Data = data,
                    TotalRecords = data.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating weapon intelligence");
                return StatusCode(500, $"Error generating weapon intelligence: {ex.Message}");
            }
        }

        [HttpGet("circumstantial-combat")]
        public async Task<IActionResult> GetCircumstantialCombat([FromQuery] AnalyticsQuery query)
        {
            try
            {
                var sql = @"
                    WITH CircumstantialData AS (
                        SELECT 
                            p.PlayerName,
                            p.Team,
                            d.MapName,
                            r.RoundNumber,
                            
                            -- COMBAT CONDITIONS BASED ON AVAILABLE DATA
                            CASE 
                                WHEN k.IsHeadshot = 1 AND k.Distance > 800 THEN 'Precision_LongRange'
                                WHEN k.Distance > 1200 THEN 'Long_Range'
                                WHEN k.Distance > 800 THEN 'Medium_Range'
                                WHEN k.IsHeadshot = 1 THEN 'Precision'
                                WHEN k.Distance < 300 THEN 'Close_Range'
                                WHEN k.Weapon LIKE '%awp%' THEN 'Sniper_Combat'
                                WHEN k.Weapon LIKE '%ak47%' OR k.Weapon LIKE '%m4a%' THEN 'Rifle_Combat'
                                ELSE 'Standard'
                            END as CombatCondition,
                            
                            -- KILL METRICS
                            CASE WHEN k.Id IS NOT NULL THEN 1 ELSE 0 END as KillSuccess,
                            k.Distance as KillDistance,
                            k.IsHeadshot,
                            k.Weapon,
                            
                            -- ROUND CONTEXT
                            CASE WHEN p.Team = r.WinnerTeam THEN 1 ELSE 0 END as RoundWon,
                            prs.Kills as RoundKills,
                            prs.Deaths as RoundDeaths,
                            prs.Damage as RoundDamage
                            
                        FROM Players p
                        INNER JOIN PlayerRoundStats prs ON p.Id = prs.PlayerId
                        INNER JOIN Rounds r ON prs.RoundId = r.Id
                        INNER JOIN DemoFiles d ON r.DemoFileId = d.Id
                        LEFT JOIN Kills k ON p.Id = k.KillerId AND k.RoundId = r.Id";

                // Apply filters
                if (!string.IsNullOrEmpty(query.MapName))
                    sql += " AND d.MapName = @MapName";
                if (!string.IsNullOrEmpty(query.PlayerName))
                    sql += " AND p.PlayerName = @PlayerName";
                if (!string.IsNullOrEmpty(query.Team))
                    sql += " AND p.Team = @Team";
                if (query.RoundNumber.HasValue)
                    sql += " AND r.RoundNumber = @RoundNumber";

                sql += @"
                    )
                    SELECT 
                        PlayerName,
                        Team,
                        MapName,
                        CombatCondition,
                        
                        -- CIRCUMSTANTIAL PERFORMANCE
                        COUNT(*) as EngagementsInCondition,
                        SUM(KillSuccess) as KillsInCondition,
                        ROUND(CAST(SUM(KillSuccess) AS FLOAT) / COUNT(*) * 100, 2) as ConditionKillPercentage,
                        ROUND(AVG(KillDistance), 2) as AvgKillDistanceInCondition,
                        
                        -- WEAPON EFFECTIVENESS IN CONDITIONS
                        COUNT(DISTINCT Weapon) as WeaponsUsedInCondition,
                        SUM(CAST(IsHeadshot AS INT)) as HeadshotKillsInCondition,
                        ROUND(SUM(CAST(IsHeadshot AS INT)) * 100.0 / NULLIF(SUM(KillSuccess), 0), 2) as HeadshotRateInCondition,
                        
                        -- DISTANCE EXPERTISE  
                        COUNT(CASE WHEN KillDistance > 1000 THEN 1 END) as LongRangeKills,
                        COUNT(CASE WHEN KillDistance < 300 THEN 1 END) as CloseRangeKills,
                        ROUND(COUNT(CASE WHEN KillDistance > 1000 THEN 1 END) * 100.0 / NULLIF(COUNT(*), 0), 2) as LongRangeKillRate,
                        
                        -- ROUND IMPACT IN CONDITIONS
                        SUM(RoundWon) as RoundsWonInCondition,
                        ROUND(SUM(RoundWon) * 100.0 / COUNT(*), 2) as WinRateInCondition,
                        ROUND(AVG(RoundDamage), 2) as AvgDamageInCondition,
                        
                        -- CIRCUMSTANTIAL MASTERY SCORE
                        CASE 
                            WHEN CombatCondition = 'Precision_LongRange' THEN 
                                ROUND((CAST(SUM(KillSuccess) AS FLOAT) / COUNT(*) * 100) * 2.0, 2)
                            WHEN CombatCondition LIKE '%Precision%' OR CombatCondition = 'Long_Range' THEN 
                                ROUND((CAST(SUM(KillSuccess) AS FLOAT) / COUNT(*) * 100) * 1.5, 2)
                            WHEN CombatCondition != 'Standard' THEN 
                                ROUND((CAST(SUM(KillSuccess) AS FLOAT) / COUNT(*) * 100) * 1.3, 2)
                            ELSE ROUND(CAST(SUM(KillSuccess) AS FLOAT) / COUNT(*) * 100, 2)
                        END as CircumstantialMasteryScore
                        
                    FROM CircumstantialData
                    GROUP BY PlayerName, Team, MapName, CombatCondition
                    HAVING COUNT(*) >= 2
                    ORDER BY CircumstantialMasteryScore DESC, ConditionKillPercentage DESC";

                var data = await ExecuteAnalyticsQuery(sql, query);
                
                if (query.Format?.ToLower() == "csv")
                {
                    var csv = ConvertToCsv(data);
                    var fileName = $"circumstantial_combat_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                    Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");
                    return File(Encoding.UTF8.GetBytes(csv), "text/csv");
                }

                return Ok(new
                {
                    Title = "Advanced Circumstantial Combat",
                    Description = "Multi-handicap performance, flash coordination, wallbang mastery, and environmental advantage analysis",
                    Data = data,
                    TotalRecords = data.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating circumstantial combat analysis");
                return StatusCode(500, $"Error generating circumstantial combat analysis: {ex.Message}");
            }
        }

        [HttpGet("team-coordination")]
        public async Task<IActionResult> GetTeamCoordination([FromQuery] AnalyticsQuery query)
        {
            try
            {
                var sql = @"
                    WITH TeamData AS (
                        SELECT 
                            p.PlayerName,
                            p.Team,
                            d.MapName,
                            r.RoundNumber,
                            
                            -- TEAM KILL COORDINATION
                            k.GameTime as PlayerKillTime,
                            k.Weapon as PlayerWeapon,
                            teammate_k.GameTime as TeammateKillTime,
                            teammate_k.Weapon as TeammateWeapon,
                            teammate.PlayerName as TeammateName,
                            ABS(k.GameTime - teammate_k.GameTime) as KillTimeDiff,
                            
                            -- TEAM PERFORMANCE METRICS  
                            prs.Kills as PlayerKills,
                            prs.Deaths as PlayerDeaths,
                            prs.Assists as PlayerAssists,
                            prs.Damage as PlayerDamage,
                            teammate_prs.Kills as TeammateKills,
                            teammate_prs.Deaths as TeammateDeaths,
                            teammate_prs.Damage as TeammateDamage,
                            
                            -- ROUND SUCCESS
                            CASE WHEN p.Team = r.WinnerTeam THEN 1 ELSE 0 END as RoundWon
                            
                        FROM Players p
                        INNER JOIN PlayerRoundStats prs ON p.Id = prs.PlayerId
                        INNER JOIN Rounds r ON prs.RoundId = r.Id
                        INNER JOIN DemoFiles d ON r.DemoFileId = d.Id
                        LEFT JOIN Kills k ON p.Id = k.KillerId AND k.RoundId = r.Id
                        
                        -- Join with teammates in same round
                        LEFT JOIN PlayerRoundStats teammate_prs ON r.Id = teammate_prs.RoundId 
                            AND teammate_prs.PlayerId != prs.PlayerId
                        LEFT JOIN Players teammate ON teammate_prs.PlayerId = teammate.Id
                            AND teammate.Team = p.Team
                        LEFT JOIN Kills teammate_k ON teammate.Id = teammate_k.KillerId 
                            AND teammate_k.RoundId = r.Id
                            AND k.Id IS NOT NULL
                            AND ABS(k.GameTime - teammate_k.GameTime) <= 5.0";

                // Apply filters
                if (!string.IsNullOrEmpty(query.MapName))
                    sql += " AND d.MapName = @MapName";
                if (!string.IsNullOrEmpty(query.PlayerName))
                    sql += " AND p.PlayerName = @PlayerName";
                if (!string.IsNullOrEmpty(query.Team))
                    sql += " AND p.Team = @Team";
                if (query.RoundNumber.HasValue)
                    sql += " AND r.RoundNumber = @RoundNumber";

                sql += @"
                    )
                    SELECT 
                        PlayerName,
                        Team,
                        MapName,
                        
                        -- TEAM COORDINATION METRICS
                        COUNT(DISTINCT TeammateName) as TeammatesPlayedWith,
                        COUNT(DISTINCT RoundNumber) as RoundsPlayed,
                        COUNT(CASE WHEN PlayerKillTime IS NOT NULL THEN 1 END) as TotalKills,
                        
                        -- KILL COORDINATION ANALYSIS
                        COUNT(CASE WHEN KillTimeDiff IS NOT NULL AND KillTimeDiff <= 2.0 THEN 1 END) as CoordinatedKills,
                        COUNT(CASE WHEN KillTimeDiff IS NOT NULL AND KillTimeDiff <= 5.0 THEN 1 END) as SupportKills,
                        ROUND(AVG(KillTimeDiff), 2) as AvgKillCoordinationTime,
                        
                        -- WEAPON COORDINATION
                        COUNT(DISTINCT PlayerWeapon) as WeaponVariety,
                        COUNT(CASE WHEN PlayerWeapon != TeammateWeapon AND KillTimeDiff <= 3.0 THEN 1 END) as ComplementaryWeaponKills,
                        
                        -- TEAM PERFORMANCE METRICS
                        ROUND(AVG(CAST(PlayerKills AS FLOAT)), 2) as AvgKillsPerRound,
                        ROUND(AVG(CAST(PlayerAssists AS FLOAT)), 2) as AvgAssistsPerRound,
                        ROUND(AVG(CAST(PlayerDamage AS FLOAT)), 2) as AvgDamagePerRound,
                        ROUND(AVG(CAST(TeammateKills AS FLOAT)), 2) as AvgTeammateKillsPerRound,
                        ROUND(AVG(CAST(TeammateDamage AS FLOAT)), 2) as AvgTeammateDamagePerRound,
                        
                        -- TEAM SUCCESS METRICS
                        SUM(RoundWon) as RoundsWon,
                        ROUND(SUM(RoundWon) * 100.0 / COUNT(DISTINCT RoundNumber), 2) as TeamWinRate,
                        
                        -- COORDINATION EFFECTIVENESS SCORE
                        CASE 
                            WHEN COUNT(CASE WHEN KillTimeDiff IS NOT NULL AND KillTimeDiff <= 2.0 THEN 1 END) > 0 
                            THEN ROUND(
                                (COUNT(CASE WHEN KillTimeDiff IS NOT NULL AND KillTimeDiff <= 2.0 THEN 1 END) * 100.0 / 
                                 NULLIF(COUNT(CASE WHEN PlayerKillTime IS NOT NULL THEN 1 END), 0)) * 
                                (SUM(RoundWon) * 100.0 / COUNT(DISTINCT RoundNumber)) / 100.0, 2)
                            ELSE 0
                        END as CoordinationEffectivenessScore
                        
                    FROM TeamData
                    GROUP BY PlayerName, Team, MapName
                    HAVING COUNT(DISTINCT RoundNumber) >= 3
                    ORDER BY CoordinationEffectivenessScore DESC, TeamWinRate DESC";

                var data = await ExecuteAnalyticsQuery(sql, query);
                
                if (query.Format?.ToLower() == "csv")
                {
                    var csv = ConvertToCsv(data);
                    var fileName = $"team_coordination_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                    Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");
                    return File(Encoding.UTF8.GetBytes(csv), "text/csv");
                }

                return Ok(new
                {
                    Title = "Team Coordination Intelligence",
                    Description = "Flash coordination, utility teamwork, site support, and coordinated execution analysis",
                    Data = data,
                    TotalRecords = data.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating team coordination analysis");
                return StatusCode(500, $"Error generating team coordination analysis: {ex.Message}");
            }
        }


        [HttpGet("pressure-metrics")]
        public async Task<IActionResult> GetPressureMetrics([FromQuery] AnalyticsQuery query)
        {
            try
            {
                var sql = @"
                    WITH PressureData AS (
                        SELECT 
                            p.PlayerName,
                            p.Team,
                            d.MapName,
                            r.RoundNumber,
                            
                            -- NUMERICAL DISADVANTAGE CONTEXT
                            CASE 
                                WHEN prs.IsAlive = 1 AND p.Team = 'CT' AND r.CTLivePlayers = 1 AND r.TLivePlayers >= 2 THEN '1vX_CT'
                                WHEN prs.IsAlive = 1 AND p.Team = 'T' AND r.TLivePlayers = 1 AND r.CTLivePlayers >= 2 THEN '1vX_T'
                                WHEN prs.IsAlive = 1 AND p.Team = 'CT' AND r.CTLivePlayers = 2 AND r.TLivePlayers >= 3 THEN '2vX_CT'
                                WHEN prs.IsAlive = 1 AND p.Team = 'T' AND r.TLivePlayers = 2 AND r.CTLivePlayers >= 3 THEN '2vX_T'
                                ELSE 'Neutral'
                            END as NumericalSituation,
                            
                            -- SIMPLIFIED PRESSURE LEVEL CALCULATION
                            CASE 
                                WHEN prs.IsAlive = 1 AND prs.Deaths > 0 AND prs.Kills >= 2 THEN 9  -- High performance under pressure
                                WHEN prs.IsAlive = 1 AND prs.Deaths > 0 AND prs.Kills >= 1 THEN 7  -- Good performance under pressure
                                WHEN prs.Deaths > 0 AND prs.Kills = 0 THEN 3  -- Failed under pressure
                                WHEN prs.IsAlive = 1 AND prs.Deaths = 0 THEN 5  -- Safe round
                                ELSE 4  -- Standard pressure
                            END as PressureLevel,
                            
                            -- PERFORMANCE METRICS
                            prs.Kills,
                            prs.Deaths,
                            prs.Assists,
                            prs.Damage,
                            prs.IsAlive,
                            CASE WHEN p.Team = r.WinnerTeam THEN 1 ELSE 0 END as RoundWon
                            
                        FROM Players p
                        INNER JOIN PlayerRoundStats prs ON p.Id = prs.PlayerId
                        INNER JOIN Rounds r ON prs.RoundId = r.Id
                        INNER JOIN DemoFiles d ON r.DemoFileId = d.Id
                        WHERE 1=1";

                // Apply filters
                if (!string.IsNullOrEmpty(query.MapName))
                    sql += " AND d.MapName = @MapName";
                if (!string.IsNullOrEmpty(query.PlayerName))
                    sql += " AND p.PlayerName = @PlayerName";
                if (!string.IsNullOrEmpty(query.Team))
                    sql += " AND p.Team = @Team";
                if (query.RoundNumber.HasValue)
                    sql += " AND r.RoundNumber = @RoundNumber";

                sql += @"
                    )
                    SELECT 
                        PlayerName,
                        Team,
                        MapName,
                        NumericalSituation,
                        
                        -- PRESSURE SITUATION ANALYSIS
                        COUNT(*) as TotalSituations,
                        ROUND(AVG(PressureLevel), 2) as AvgPressureLevel,
                        COUNT(CASE WHEN PressureLevel >= 7 THEN 1 END) as HighPressureSituations,
                        COUNT(CASE WHEN PressureLevel <= 4 THEN 1 END) as LowPressureSituations,
                        
                        -- PERFORMANCE UNDER PRESSURE
                        SUM(Kills) as TotalKills,
                        SUM(Deaths) as TotalDeaths,
                        SUM(Assists) as TotalAssists,
                        SUM(Damage) as TotalDamage,
                        SUM(RoundWon) as RoundsWon,
                        
                        -- COMBAT RATIOS
                        ROUND(CASE WHEN SUM(Deaths) > 0 THEN SUM(Kills) * 1.0 / SUM(Deaths) ELSE SUM(Kills) END, 2) as KDRatio,
                        ROUND(SUM(Damage) * 1.0 / COUNT(*), 2) as AvgDamagePerRound,
                        ROUND((SUM(RoundWon) * 100.0 / COUNT(*)), 2) as WinRate,
                        
                        -- CLUTCH PERFORMANCE
                        COUNT(CASE WHEN NumericalSituation LIKE '1vX_%' THEN 1 END) as ClutchAttempts,
                        SUM(CASE WHEN NumericalSituation LIKE '1vX_%' AND RoundWon = 1 THEN 1 ELSE 0 END) as ClutchWins,
                        ROUND(CASE WHEN COUNT(CASE WHEN NumericalSituation LIKE '1vX_%' THEN 1 END) > 0 
                              THEN (SUM(CASE WHEN NumericalSituation LIKE '1vX_%' AND RoundWon = 1 THEN 1 ELSE 0 END) * 100.0 / 
                                   COUNT(CASE WHEN NumericalSituation LIKE '1vX_%' THEN 1 END))
                              ELSE 0 END, 2) as ClutchSuccessRate,
                        
                        -- SURVIVAL UNDER PRESSURE
                        SUM(CASE WHEN IsAlive = 1 THEN 1 ELSE 0 END) as SurvivedRounds,
                        ROUND((SUM(CASE WHEN IsAlive = 1 THEN 1 ELSE 0 END) * 100.0 / COUNT(*)), 2) as SurvivalRate,
                        
                        -- PRESSURE PERFORMANCE SCORE (0-100)
                        CASE 
                            WHEN ROUND((SUM(RoundWon) * 100.0 / COUNT(*)), 2) >= 70 AND 
                                 ROUND(CASE WHEN SUM(Deaths) > 0 THEN SUM(Kills) * 1.0 / SUM(Deaths) ELSE SUM(Kills) END, 2) >= 1.5 THEN 95
                            WHEN ROUND((SUM(RoundWon) * 100.0 / COUNT(*)), 2) >= 60 AND 
                                 ROUND(CASE WHEN SUM(Deaths) > 0 THEN SUM(Kills) * 1.0 / SUM(Deaths) ELSE SUM(Kills) END, 2) >= 1.2 THEN 80
                            WHEN ROUND((SUM(RoundWon) * 100.0 / COUNT(*)), 2) >= 50 THEN 65
                            WHEN ROUND(CASE WHEN SUM(Deaths) > 0 THEN SUM(Kills) * 1.0 / SUM(Deaths) ELSE SUM(Kills) END, 2) >= 1.0 THEN 50
                            ELSE 30
                        END as PressurePerformanceScore
                        
                    FROM PressureData
                    WHERE NumericalSituation != 'Neutral'
                    GROUP BY PlayerName, Team, MapName, NumericalSituation
                    HAVING COUNT(*) >= 3
                    ORDER BY PressurePerformanceScore DESC, WinRate DESC";

                var data = await ExecuteAnalyticsQuery(sql, query);
                
                if (query.Format?.ToLower() == "csv")
                {
                    var csv = ConvertToCsv(data);
                    var fileName = $"pressure_metrics_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                    Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");
                    return File(Encoding.UTF8.GetBytes(csv), "text/csv");
                }

                return Ok(new
                {
                    Title = "Psychological Pressure Metrics",
                    Description = "High-pressure performance, clutch scaling, objective completion under fire, mental resilience, and comeback potential analysis",
                    Data = data,
                    TotalRecords = data.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating pressure metrics analysis");
                return StatusCode(500, $"Error generating pressure metrics analysis: {ex.Message}");
            }
        }

        private async Task<List<Dictionary<string, object>>> ExecuteAnalyticsQuery(string sql, AnalyticsQuery query)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            using var command = new SqlCommand(sql, connection);
            command.CommandTimeout = 120;
            AddAnalyticsParameters(command, query);
            
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

        [HttpGet("database-diagnostic")]
        public async Task<IActionResult> GetDatabaseDiagnostic()
        {
            try
            {
                var sql = @"
                    SELECT 
                        'DemoFiles' as TableName,
                        COUNT(*) as RecordCount,
                        MAX(Id) as MaxId,
                        MIN(Id) as MinId
                    FROM DemoFiles
                    
                    UNION ALL
                    
                    SELECT 
                        'Rounds' as TableName,
                        COUNT(*) as RecordCount,
                        MAX(Id) as MaxId,
                        MIN(Id) as MinId
                    FROM Rounds
                    
                    UNION ALL
                    
                    SELECT 
                        'Players' as TableName,
                        COUNT(*) as RecordCount,
                        MAX(Id) as MaxId,
                        MIN(Id) as MinId
                    FROM Players
                    
                    UNION ALL
                    
                    SELECT 
                        'Kills' as TableName,
                        COUNT(*) as RecordCount,
                        MAX(Id) as MaxId,
                        MIN(Id) as MinId
                    FROM Kills
                    
                    UNION ALL
                    
                    SELECT 
                        'PlayerRoundStats' as TableName,
                        COUNT(*) as RecordCount,
                        MAX(Id) as MaxId,
                        MIN(Id) as MinId
                    FROM PlayerRoundStats";

                var data = await ExecuteAnalyticsQuery(sql, new AnalyticsQuery());

                return Ok(new
                {
                    Title = "Database Diagnostic",
                    Description = "Basic table counts and ranges",
                    Data = data,
                    TotalRecords = data.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating database diagnostic");
                return StatusCode(500, new { Error = "Database diagnostic failed", Details = ex.Message });
            }
        }

        private void AddAnalyticsParameters(SqlCommand command, AnalyticsQuery query)
        {
            command.Parameters.AddWithValue("@DemoId", query.DemoId.HasValue ? (object)query.DemoId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@MapName", !string.IsNullOrEmpty(query.MapName) ? (object)query.MapName : DBNull.Value);
            command.Parameters.AddWithValue("@PlayerName", !string.IsNullOrEmpty(query.PlayerName) ? (object)query.PlayerName : DBNull.Value);
            command.Parameters.AddWithValue("@Team", !string.IsNullOrEmpty(query.Team) ? (object)query.Team : DBNull.Value);
            command.Parameters.AddWithValue("@StartDate", query.StartDate.HasValue ? (object)query.StartDate.Value : DBNull.Value);
            command.Parameters.AddWithValue("@EndDate", query.EndDate.HasValue ? (object)query.EndDate.Value : DBNull.Value);
            command.Parameters.AddWithValue("@RoundNumber", query.RoundNumber.HasValue ? (object)query.RoundNumber.Value : DBNull.Value);
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

        [HttpGet("economy-intelligence-dashboard")]
        public async Task<IActionResult> GetEconomyIntelligenceDashboard([FromQuery] AnalyticsQuery query)
        {
            try
            {
                var sql = @"
                    WITH PlayerOverallStats AS (
                        -- Get overall player performance stats
                        SELECT 
                            p.PlayerName,
                            p.Team,
                            d.MapName,
                            AVG(CAST(prs.Kills AS FLOAT)) as OverallAvgKills,
                            AVG(CAST(prs.Deaths AS FLOAT)) as OverallAvgDeaths,
                            AVG(CAST(prs.Damage AS FLOAT)) as OverallAvgDamage,
                            AVG(prs.Rating) as OverallAvgRating
                        FROM PlayerRoundStats prs
                        INNER JOIN Players p ON prs.PlayerId = p.Id
                        INNER JOIN Rounds r ON prs.RoundId = r.Id
                        INNER JOIN DemoFiles d ON r.DemoFileId = d.Id
                        WHERE (@DemoId IS NULL OR d.Id = @DemoId)
                            AND (@MapName IS NULL OR d.MapName = @MapName)
                            AND (@PlayerName IS NULL OR p.PlayerName = @PlayerName)
                            AND (@Team IS NULL OR p.Team = @Team)
                        GROUP BY p.PlayerName, p.Team, d.MapName
                    ),
                    EconomyPerformance AS (
                        SELECT 
                            p.PlayerName,
                            p.Team,
                            d.MapName,
                            e.RoundNumber,
                            e.MoneyBefore,
                            e.MoneyAfter,
                            e.ItemName,
                            e.EventType,
                            r.WinnerTeam,
                            CASE WHEN p.Team = r.WinnerTeam THEN 1 ELSE 0 END as RoundWon,
                            -- Enhanced money state categorization using actual money data
                            CASE 
                                WHEN e.MoneyBefore >= 5000 THEN 'Rich ($5000+)'
                                WHEN e.MoneyBefore >= 3000 THEN 'Full Buy ($3000-5000)'
                                WHEN e.MoneyBefore >= 1900 THEN 'Force Buy ($1900-3000)'
                                WHEN e.MoneyBefore >= 800 THEN 'Eco ($800-1900)'
                                ELSE 'Broke (<$800)'
                            END as MoneyState,
                            -- Money efficiency (money difference)
                            (e.MoneyAfter - e.MoneyBefore) as MoneyDelta,
                            -- Equipment tracking
                            CASE 
                                WHEN e.ItemName IN ('ak47', 'aug', 'awp', 'm4a1_silencer', 'm4a4', 'krieg') THEN 'Rifle'
                                WHEN e.ItemName IN ('deagle', 'glock', 'usp_silencer', 'p250', 'tec9', 'fiveseven', 'cz75a', 'dualberettas') THEN 'Pistol'
                                WHEN e.ItemName IN ('mp9', 'mac10', 'mp7', 'ump45', 'bizon', 'p90') THEN 'SMG'
                                WHEN e.ItemName IN ('nova', 'xm1014', 'mag7', 'sawedoff') THEN 'Shotgun'
                                WHEN e.ItemName IN ('vesthelm', 'vest') THEN 'Armor'
                                WHEN e.ItemName IN ('flashbang', 'hegrenade', 'smokegrenade', 'molotov', 'incgrenade', 'decoy') THEN 'Grenade'
                                WHEN e.ItemName IN ('defusekit') THEN 'Utility'
                                ELSE 'Other'
                            END as EquipmentCategory
                        FROM EconomyEvents e
                        INNER JOIN Players p ON e.PlayerId = p.Id
                        INNER JOIN DemoFiles d ON e.DemoFileId = d.Id
                        LEFT JOIN Rounds r ON e.RoundNumber = r.RoundNumber AND r.DemoFileId = d.Id
                        WHERE (@DemoId IS NULL OR d.Id = @DemoId)
                            AND (@MapName IS NULL OR d.MapName = @MapName)  
                            AND (@PlayerName IS NULL OR p.PlayerName = @PlayerName)
                            AND (@Team IS NULL OR p.Team = @Team)
                            AND e.RoundNumber >= 0 -- Valid rounds only
                    ),
                    RoundEconomyStats AS (
                        SELECT 
                            PlayerName,
                            Team,
                            MapName,
                            RoundNumber,
                            MoneyState,
                            MAX(MoneyBefore) as RoundStartMoney,
                            MIN(MoneyAfter) as RoundEndMoney,
                            COUNT(CASE WHEN EventType = 'equip' THEN 1 END) as ItemsEquipped,
                            COUNT(CASE WHEN EventType = 'pickup' THEN 1 END) as ItemsPickedUp,
                            COUNT(CASE WHEN EquipmentCategory = 'Rifle' THEN 1 END) as HasRifle,
                            COUNT(CASE WHEN EquipmentCategory = 'Armor' THEN 1 END) as HasArmor,
                            COUNT(CASE WHEN EquipmentCategory = 'Grenade' THEN 1 END) as GrenadeCount,
                            MAX(RoundWon) as RoundWon
                        FROM EconomyPerformance 
                        GROUP BY PlayerName, Team, MapName, RoundNumber, MoneyState
                    )
                    SELECT 
                        res.PlayerName,
                        res.Team,
                        res.MapName,
                        res.MoneyState as EconomyState,
                        COUNT(*) as TotalRounds,
                        SUM(res.RoundWon) as RoundsWon,
                        CAST(SUM(res.RoundWon) AS FLOAT) / COUNT(*) * 100 as WinPercentage,
                        AVG(CAST(res.RoundStartMoney AS FLOAT)) as AvgStartMoney,
                        AVG(CAST(res.RoundEndMoney AS FLOAT)) as AvgEndMoney,
                        AVG(CAST(res.RoundStartMoney - res.RoundEndMoney AS FLOAT)) as AvgMoneySpent,
                        AVG(CAST(res.ItemsEquipped AS FLOAT)) as AvgItemsEquipped,
                        AVG(CAST(res.ItemsPickedUp AS FLOAT)) as AvgItemsPickedUp,
                        SUM(res.HasRifle) as RoundsWithRifle,
                        CAST(SUM(res.HasRifle) AS FLOAT) / COUNT(*) * 100 as RifleUsageRate,
                        SUM(res.HasArmor) as RoundsWithArmor,
                        CAST(SUM(res.HasArmor) AS FLOAT) / COUNT(*) * 100 as ArmorUsageRate,
                        AVG(CAST(res.GrenadeCount AS FLOAT)) as AvgGrenadesPerRound,
                        -- Economic efficiency metrics
                        CASE 
                            WHEN COUNT(*) > 0 THEN 
                                CAST(SUM(res.RoundWon) AS FLOAT) / AVG(CAST(res.RoundStartMoney AS FLOAT)) * 1000 
                            ELSE 0 
                        END as EconomicEfficiency, -- Wins per $1000 invested
                        -- Money management rating
                        CASE 
                            WHEN AVG(CAST(res.RoundStartMoney - res.RoundEndMoney AS FLOAT)) > 0 AND COUNT(*) > 0 THEN
                                CAST(SUM(res.RoundWon) AS FLOAT) / COUNT(*) / (AVG(CAST(res.RoundStartMoney - res.RoundEndMoney AS FLOAT)) / 1000)
                            ELSE 0
                        END as MoneyManagementScore,
                        -- Performance metrics from PlayerOverallStats
                        pos.OverallAvgDamage as AvgDamage,
                        pos.OverallAvgKills as AvgKills, 
                        pos.OverallAvgRating as AvgRating,
                        -- Equipment value approximation (rifle=2700, armor=1000, etc.)
                        CASE 
                            WHEN COUNT(*) > 0 THEN 
                                (SUM(res.HasRifle) * 2700.0 + SUM(res.HasArmor) * 1000.0 + AVG(CAST(res.GrenadeCount AS FLOAT)) * 300.0) / COUNT(*)
                            ELSE 0 
                        END as AvgEquipmentValue,
                        -- Damage per dollar efficiency
                        CASE 
                            WHEN AVG(CAST(res.RoundStartMoney AS FLOAT)) > 0 AND pos.OverallAvgDamage > 0 THEN
                                pos.OverallAvgDamage / AVG(CAST(res.RoundStartMoney AS FLOAT)) * 100
                            ELSE 0
                        END as AvgDamagePerDollar,
                        -- Equipment ROI (wins per equipment investment ratio)
                        CASE 
                            WHEN (SUM(res.HasRifle) * 2700.0 + SUM(res.HasArmor) * 1000.0) > 0 THEN
                                CAST(SUM(res.RoundWon) AS FLOAT) / (SUM(res.HasRifle) * 2700.0 + SUM(res.HasArmor) * 1000.0) * 100000
                            ELSE 0
                        END as AvgEquipmentROI,
                        -- Low money analysis
                        SUM(CASE WHEN res.RoundStartMoney < 1500 THEN 1 ELSE 0 END) as LowMoneyRounds,
                        CASE 
                            WHEN COUNT(*) > 0 THEN 
                                CAST(SUM(CASE WHEN res.RoundStartMoney < 1500 THEN 1 ELSE 0 END) AS FLOAT) / COUNT(*) * 100
                            ELSE 0 
                        END as LowMoneyRate
                    FROM RoundEconomyStats res
                    LEFT JOIN PlayerOverallStats pos ON res.PlayerName = pos.PlayerName 
                        AND res.Team = pos.Team 
                        AND res.MapName = pos.MapName
                    GROUP BY res.PlayerName, res.Team, res.MapName, res.MoneyState, pos.OverallAvgKills, pos.OverallAvgDamage, pos.OverallAvgRating
                    HAVING COUNT(*) >= 3 -- At least 3 rounds in this money state for meaningful analysis
                    ORDER BY EconomicEfficiency DESC, WinPercentage DESC";

                var data = await ExecuteAnalyticsQuery(sql, query);
                
                if (query.Format?.ToLower() == "csv")
                {
                    var csv = ConvertToCsv(data);
                    var fileName = $"economy_intelligence_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                    Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");
                    return File(Encoding.UTF8.GetBytes(csv), "text/csv");
                }

                return Ok(new
                {
                    Title = "Economy Intelligence Dashboard",
                    Description = "Advanced economy analysis including money efficiency, equipment ROI, and economic state performance correlation",
                    Data = data,
                    TotalRecords = data.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating economy intelligence dashboard");
                return StatusCode(500, $"Error generating economy intelligence dashboard: {ex.Message}");
            }
        }

        [HttpGet("advanced-player-performance")]
        public async Task<IActionResult> GetAdvancedPlayerPerformance([FromQuery] AnalyticsQuery query)
        {
            try
            {
                var sql = @"
                    WITH PlayerPerformance AS (
                        SELECT 
                            prs.PlayerId,
                            p.PlayerName,
                            p.Team,
                            d.MapName,
                            r.RoundNumber,
                            prs.Kills,
                            prs.Deaths,
                            prs.Assists,
                            prs.Damage,
                            prs.Rating,
                            prs.IsAlive,
                            CASE WHEN p.Team = r.WinnerTeam THEN 1 ELSE 0 END as RoundWon,
                            -- Multi-kill detection
                            CASE 
                                WHEN prs.Kills >= 4 THEN 'Quad Kill+'
                                WHEN prs.Kills = 3 THEN 'Triple Kill'
                                WHEN prs.Kills = 2 THEN 'Double Kill'
                                WHEN prs.Kills = 1 THEN 'Single Kill'
                                ELSE 'No Kills'
                            END as KillType,
                            -- Performance consistency
                            prs.Rating - AVG(prs.Rating) OVER (PARTITION BY prs.PlayerId) as RatingDeviation,
                            -- Impact rating (custom HLTV-style calculation)
                            (prs.Kills * 0.679 + prs.Assists * 0.154 + prs.Damage * 0.0021 + 
                             CASE WHEN prs.IsAlive = 1 THEN 0.15 ELSE 0 END) as ImpactRating
                        FROM PlayerRoundStats prs
                        INNER JOIN Players p ON prs.PlayerId = p.Id
                        INNER JOIN Rounds r ON prs.RoundId = r.Id
                        INNER JOIN DemoFiles d ON r.DemoFileId = d.Id
                        WHERE (@DemoId IS NULL OR d.Id = @DemoId)
                            AND (@MapName IS NULL OR d.MapName = @MapName)
                            AND (@PlayerName IS NULL OR p.PlayerName = @PlayerName)
                            AND (@Team IS NULL OR p.Team = @Team)
                    )
                    SELECT 
                        PlayerName,
                        Team,
                        MapName,
                        COUNT(*) as TotalRounds,
                        SUM(RoundWon) as RoundsWon,
                        CAST(SUM(RoundWon) AS FLOAT) / COUNT(*) * 100 as WinRate,
                        -- Performance metrics
                        AVG(CAST(Kills AS FLOAT)) as AvgKills,
                        AVG(CAST(Deaths AS FLOAT)) as AvgDeaths,
                        AVG(CAST(Assists AS FLOAT)) as AvgAssists,
                        AVG(CAST(Damage AS FLOAT)) as AvgDamage,
                        AVG(Rating) as AvgRating,
                        AVG(ImpactRating) as AvgImpactRating,
                        -- Multi-kill performance
                        COUNT(CASE WHEN KillType = 'Quad Kill+' THEN 1 END) as QuadKills,
                        COUNT(CASE WHEN KillType = 'Triple Kill' THEN 1 END) as TripleKills,
                        COUNT(CASE WHEN KillType = 'Double Kill' THEN 1 END) as DoubleKills,
                        -- Consistency metrics
                        STDEV(Rating) as RatingConsistency,
                        MIN(Rating) as WorstRating,
                        MAX(Rating) as BestRating,
                        -- Survival and clutch potential
                        SUM(CASE WHEN IsAlive = 1 THEN 1 ELSE 0 END) as RoundsSurvived,
                        CAST(SUM(CASE WHEN IsAlive = 1 THEN 1 ELSE 0 END) AS FLOAT) / COUNT(*) * 100 as SurvivalRate,
                        -- Efficiency score
                        (AVG(ImpactRating) * (1 - (STDEV(Rating) / NULLIF(AVG(Rating), 0)))) as EfficiencyScore
                    FROM PlayerPerformance
                    GROUP BY PlayerName, Team, MapName
                    HAVING COUNT(*) >= 5 -- At least 5 rounds played
                    ORDER BY EfficiencyScore DESC, AvgImpactRating DESC";

                var data = await ExecuteAnalyticsQuery(sql, query);
                
                if (query.Format?.ToLower() == "csv")
                {
                    var csv = ConvertToCsv(data);
                    var fileName = $"advanced_player_performance_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                    Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");
                    return File(Encoding.UTF8.GetBytes(csv), "text/csv");
                }

                return Ok(new
                {
                    Title = "Advanced Player Performance Matrix",
                    Description = "Comprehensive player analysis including HLTV-style ratings, consistency scoring, and multi-kill performance",
                    Data = data,
                    TotalRecords = data.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating advanced player performance");
                return StatusCode(500, $"Error generating advanced player performance: {ex.Message}");
            }
        }

        [HttpGet("player-inventory")]
        public async Task<IActionResult> GetPlayerInventory([FromQuery] AnalyticsQuery query)
        {
            try
            {
                var sql = @"
                    WITH PlayerInventory AS (
                        SELECT 
                            p.PlayerName,
                            p.Team,
                            d.MapName,
                            e.RoundNumber,
                            e.EventType,
                            e.ItemName,
                            e.MoneyBefore,
                            e.MoneyAfter,
                            -- Categorize equipment types
                            CASE 
                                WHEN e.ItemName IN ('ak47', 'aug', 'awp', 'm4a1_silencer', 'm4a4', 'krieg', 'ssg08', 'g3sg1', 'scar20') THEN 'Primary'
                                WHEN e.ItemName IN ('deagle', 'glock', 'usp_silencer', 'hkp2000', 'p250', 'tec9', 'fiveseven', 'cz75a', 'dualberettas', 'revolver') THEN 'Pistol'
                                WHEN e.ItemName IN ('mp9', 'mac10', 'mp7', 'ump45', 'bizon', 'p90', 'mp5sd') THEN 'SMG'
                                WHEN e.ItemName IN ('nova', 'xm1014', 'mag7', 'sawedoff') THEN 'Shotgun'
                                WHEN e.ItemName IN ('vesthelm', 'vest') THEN 'Armor'
                                WHEN e.ItemName IN ('flashbang', 'hegrenade', 'smokegrenade', 'molotov', 'incgrenade', 'decoy') THEN 'Grenade'
                                WHEN e.ItemName IN ('defusekit', 'c4') THEN 'Utility'
                                WHEN e.ItemName = 'knife' THEN 'Melee'
                                ELSE 'Other'
                            END as EquipmentType,
                            -- Calculate estimated item value
                            CASE e.ItemName
                                WHEN 'ak47' THEN 2700
                                WHEN 'm4a1_silencer' THEN 3100
                                WHEN 'm4a4' THEN 3100
                                WHEN 'awp' THEN 4750
                                WHEN 'aug' THEN 3300
                                WHEN 'krieg' THEN 2750
                                WHEN 'deagle' THEN 700
                                WHEN 'vesthelm' THEN 1000
                                WHEN 'vest' THEN 650
                                WHEN 'flashbang' THEN 200
                                WHEN 'hegrenade' THEN 300
                                WHEN 'smokegrenade' THEN 300
                                WHEN 'molotov' THEN 400
                                WHEN 'incgrenade' THEN 600
                                WHEN 'defusekit' THEN 400
                                ELSE 0
                            END as EstimatedValue,
                            ROW_NUMBER() OVER (PARTITION BY p.PlayerName, e.RoundNumber, e.ItemName ORDER BY e.Id DESC) as ItemRank
                        FROM EconomyEvents e
                        INNER JOIN Players p ON e.PlayerId = p.Id
                        INNER JOIN DemoFiles d ON e.DemoFileId = d.Id
                        WHERE (@DemoId IS NULL OR d.Id = @DemoId)
                            AND (@MapName IS NULL OR d.MapName = @MapName)
                            AND (@PlayerName IS NULL OR p.PlayerName = @PlayerName)
                            AND (@Team IS NULL OR p.Team = @Team)
                            AND e.RoundNumber >= 0 -- Valid rounds only
                            AND e.EventType IN ('equip', 'pickup') -- Focus on equipment acquisition
                    ),
                    LatestInventory AS (
                        SELECT 
                            PlayerName,
                            Team,
                            MapName,
                            RoundNumber,
                            ItemName,
                            EquipmentType,
                            EstimatedValue,
                            MoneyBefore,
                            MoneyAfter,
                            CASE WHEN EventType = 'pickup' THEN 1 ELSE 0 END as WasPurchased
                        FROM PlayerInventory 
                        WHERE ItemRank = 1 -- Latest event for each item per round
                    ),
                    RoundInventorySummary AS (
                        SELECT 
                            PlayerName,
                            Team,
                            MapName,
                            RoundNumber,
                            MAX(MoneyBefore) as RoundStartMoney,
                            MIN(MoneyAfter) as RoundEndMoney,
                            SUM(EstimatedValue) as TotalEquipmentValue,
                            SUM(WasPurchased * EstimatedValue) as MoneySpentOnEquipment,
                            COUNT(DISTINCT ItemName) as UniqueItemsCarried,
                            COUNT(CASE WHEN EquipmentType = 'Primary' THEN 1 END) as HasPrimaryWeapon,
                            COUNT(CASE WHEN EquipmentType = 'Armor' THEN 1 END) as HasArmor,
                            COUNT(CASE WHEN EquipmentType = 'Grenade' THEN 1 END) as GrenadeCount,
                            STRING_AGG(
                                CASE WHEN EquipmentType IN ('Primary', 'Pistol', 'SMG', 'Shotgun') THEN ItemName END, 
                                ', '
                            ) as WeaponsCarried,
                            STRING_AGG(
                                CASE WHEN EquipmentType = 'Grenade' THEN ItemName END, 
                                ', '
                            ) as GrenadesCarried
                        FROM LatestInventory
                        GROUP BY PlayerName, Team, MapName, RoundNumber
                    )
                    SELECT 
                        PlayerName,
                        Team,
                        MapName,
                        RoundNumber,
                        RoundStartMoney,
                        RoundEndMoney,
                        TotalEquipmentValue,
                        MoneySpentOnEquipment,
                        UniqueItemsCarried,
                        CASE WHEN HasPrimaryWeapon > 0 THEN 'Yes' ELSE 'No' END as HasPrimary,
                        CASE WHEN HasArmor > 0 THEN 'Yes' ELSE 'No' END as HasArmor,
                        GrenadeCount,
                        WeaponsCarried,
                        GrenadesCarried,
                        -- Economic efficiency for this round
                        CASE 
                            WHEN TotalEquipmentValue > 0 THEN 
                                CAST(RoundStartMoney AS FLOAT) / TotalEquipmentValue 
                            ELSE 0 
                        END as MoneyToEquipmentRatio,
                        -- Loadout categorization
                        CASE 
                            WHEN TotalEquipmentValue >= 6000 THEN 'Full Buy'
                            WHEN TotalEquipmentValue >= 3000 THEN 'Buy Round'
                            WHEN TotalEquipmentValue >= 1500 THEN 'Force Buy'
                            WHEN TotalEquipmentValue >= 500 THEN 'Eco Round'
                            ELSE 'Save Round'
                        END as LoadoutCategory
                    FROM RoundInventorySummary
                    WHERE (@RoundNumber IS NULL OR RoundNumber = @RoundNumber)
                    ORDER BY PlayerName, RoundNumber DESC";

                var data = await ExecuteAnalyticsQuery(sql, query);
                
                if (query.Format?.ToLower() == "csv")
                {
                    var csv = ConvertToCsv(data);
                    var fileName = $"player_inventory_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                    Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");
                    return File(Encoding.UTF8.GetBytes(csv), "text/csv");
                }

                return Ok(new
                {
                    Title = "Player Inventory Analysis",
                    Description = "Detailed round-by-round player equipment, economy, and loadout analysis",
                    Data = data,
                    TotalRecords = data.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating player inventory analysis");
                return StatusCode(500, $"Error generating player inventory analysis: {ex.Message}");
            }
        }

        [HttpGet("master-analytics-dashboard")]
        public async Task<IActionResult> GetMasterAnalyticsDashboard([FromQuery] AnalyticsQuery query)
        {
            try
            {
                var sql = @"
                    WITH PlayerMasterStats AS (
                        SELECT 
                            p.PlayerName,
                            p.Team,
                            d.MapName,
                            COUNT(DISTINCT r.Id) as TotalRounds,
                            
                            -- CORE COMBAT ANALYTICS
                            COALESCE(SUM(prs.Kills), 0) as TotalKills,
                            COALESCE(SUM(prs.Deaths), 0) as TotalDeaths,
                            COALESCE(SUM(prs.Assists), 0) as TotalAssists,
                            COALESCE(SUM(prs.Damage), 0) as TotalDamageDealt,
                            COALESCE(AVG(prs.Rating), 0) as AvgRating,
                            
                            -- SURVIVAL & PERFORMANCE
                            COUNT(CASE WHEN prs.IsAlive = 1 THEN 1 END) as RoundsSurvived,
                            COUNT(CASE WHEN prs.KAST = 1 THEN 1 END) as KASTRounds,
                            COUNT(CASE WHEN prs.MVP = 1 THEN 1 END) as MVPRounds,
                            COUNT(CASE WHEN prs.IsClutch = 1 THEN 1 END) as ClutchRounds,
                            
                            -- ADVANCED KILLS DATA
                            COUNT(k.Id) as KillEvents,
                            COUNT(CASE WHEN k.IsHeadshot = 1 THEN 1 END) as Headshots,
                            COUNT(CASE WHEN k.IsWallbang = 1 THEN 1 END) as WallbangKills,
                            COUNT(CASE WHEN k.ThroughSmoke = 1 THEN 1 END) as SmokeKills,
                            COALESCE(AVG(CAST(k.Distance AS FLOAT)), 0) as AvgKillDistance,
                            
                            -- WEAPON PERFORMANCE
                            COUNT(wf.Id) as ShotsFired,
                            COALESCE(AVG(wf.Accuracy), 0) as AvgShotAccuracy,
                            
                            -- UTILITY USAGE
                            COUNT(g.Id) as GrenadesThrown,
                            COALESCE(SUM(g.TotalDamage), 0) as GrenadeDamageDealt,
                            COUNT(fe.Id) as TimesFlashed,
                            COALESCE(AVG(fe.FlashDuration), 0) as AvgFlashDuration
                            
                        FROM Players p
                        INNER JOIN DemoFiles d ON p.DemoFileId = d.Id
                        LEFT JOIN Matches m ON d.Id = m.DemoFileId
                        LEFT JOIN Rounds r ON m.Id = r.MatchId
                        LEFT JOIN PlayerRoundStats prs ON p.Id = prs.PlayerId AND r.Id = prs.RoundId
                        LEFT JOIN Kills k ON p.Id = k.KillerId
                        LEFT JOIN WeaponFires wf ON p.Id = wf.PlayerId
                        LEFT JOIN Grenades g ON p.Id = g.PlayerId
                        LEFT JOIN FlashEvents fe ON p.Id = fe.FlashedPlayerId
                        
                        WHERE (@DemoId IS NULL OR d.Id = @DemoId)
                            AND (@MapName IS NULL OR d.MapName = @MapName)
                            AND (@PlayerName IS NULL OR p.PlayerName = @PlayerName)
                            AND (@Team IS NULL OR p.Team = @Team)
                        GROUP BY p.PlayerName, p.Team, d.MapName
                    )
                    SELECT 
                        PlayerName,
                        Team,
                        MapName,
                        TotalRounds,
                        
                        -- Core Combat Performance
                        TotalKills,
                        TotalDeaths,
                        TotalAssists,
                        TotalDamageDealt,
                        CASE 
                            WHEN TotalDeaths > 0 THEN CAST(TotalKills AS FLOAT) / TotalDeaths
                            ELSE CAST(TotalKills AS FLOAT)
                        END as KDRatio,
                        
                        -- Advanced Combat
                        KillEvents,
                        Headshots,
                        CASE WHEN TotalKills > 0 THEN CAST(Headshots AS FLOAT) / TotalKills * 100 ELSE 0 END as HeadshotPercentage,
                        WallbangKills,
                        SmokeKills,
                        AvgKillDistance,
                        
                        -- Weapon Performance  
                        ShotsFired,
                        CASE WHEN ShotsFired > 0 THEN CAST(TotalKills AS FLOAT) / ShotsFired * 100 ELSE 0 END as KillsPerShotPercentage,
                        AvgShotAccuracy,
                        
                        -- Utility Usage
                        GrenadesThrown,
                        GrenadeDamageDealt,
                        CASE WHEN GrenadesThrown > 0 THEN CAST(GrenadeDamageDealt AS FLOAT) / GrenadesThrown ELSE 0 END as UtilityEfficiency,
                        
                        -- Flash Performance
                        TimesFlashed,
                        AvgFlashDuration,
                        
                        -- Survival & Performance
                        RoundsSurvived,
                        CASE WHEN TotalRounds > 0 THEN CAST(RoundsSurvived AS FLOAT) / TotalRounds * 100 ELSE 0 END as SurvivalRate,
                        KASTRounds,
                        CASE WHEN TotalRounds > 0 THEN CAST(KASTRounds AS FLOAT) / TotalRounds * 100 ELSE 0 END as KASTPercentage,
                        MVPRounds,
                        ClutchRounds,
                        AvgRating,
                        
                        -- Overall Performance Score
                        CASE WHEN TotalRounds >= 3 THEN
                            (AvgRating * 25 + 
                             LEAST(CASE WHEN TotalDeaths > 0 THEN CAST(TotalKills AS FLOAT) / TotalDeaths ELSE CAST(TotalKills AS FLOAT) END * 10, 25) + 
                             LEAST(CASE WHEN TotalRounds > 0 THEN CAST(RoundsSurvived AS FLOAT) / TotalRounds * 100 ELSE 0 END * 0.25, 25) + 
                             LEAST(CASE WHEN TotalKills > 0 THEN CAST(Headshots AS FLOAT) / TotalKills * 100 ELSE 0 END * 0.25, 25))
                        ELSE 0 END as OverallPerformanceScore
                        
                    FROM PlayerMasterStats
                    WHERE TotalRounds >= 1
                    ORDER BY OverallPerformanceScore DESC, KDRatio DESC";

                var data = await ExecuteAnalyticsQuery(sql, query);
                
                if (query.Format?.ToLower() == "csv")
                {
                    var csv = ConvertToCsv(data);
                    var fileName = $"master_analytics_dashboard_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                    Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");
                    return File(Encoding.UTF8.GetBytes(csv), "text/csv");
                }

                return Ok(new
                {
                    Title = "Master Analytics Dashboard",
                    Description = "Comprehensive multi-dimensional analysis integrating all game data: combat, utility, economy, objectives, and performance intelligence",
                    Data = data,
                    TotalRecords = data.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating master analytics dashboard");
                return StatusCode(500, $"Error generating master analytics dashboard: {ex.Message}");
            }
        }

        [HttpGet("situation-analysis")]
        public async Task<IActionResult> GetSituationAnalysis([FromQuery] AnalyticsQuery query)
        {
            try
            {
                var sql = @"
                    WITH SituationalPerformance AS (
                        SELECT 
                            p.PlayerName,
                            p.Team,
                            d.MapName,
                            r.RoundNumber,
                            
                            -- SITUATIONAL CONTEXT
                            CASE 
                                WHEN prs.IsAlive = 1 AND r.CTLivePlayers = 1 AND r.TLivePlayers > 1 AND p.Team = 'CT' THEN '1vX_CT'
                                WHEN prs.IsAlive = 1 AND r.TLivePlayers = 1 AND r.CTLivePlayers > 1 AND p.Team = 'T' THEN '1vX_T'
                                WHEN prs.IsAlive = 1 AND r.CTLivePlayers = 2 AND r.TLivePlayers > 2 AND p.Team = 'CT' THEN '2vX_CT'
                                WHEN prs.IsAlive = 1 AND r.TLivePlayers = 2 AND r.CTLivePlayers > 2 AND p.Team = 'T' THEN '2vX_T'
                                WHEN r.BombPlanted = 1 AND prs.IsAlive = 1 THEN 'PostPlant_Alive'
                                WHEN r.BombPlanted = 1 AND prs.IsAlive = 0 THEN 'PostPlant_Dead'
                                ELSE 'Standard'
                            END as SituationType,
                            
                            -- ECONOMIC SITUATION
                            CASE 
                                WHEN COALESCE(ee.MoneyBefore, 0) >= 5000 THEN 'Rich'
                                WHEN COALESCE(ee.MoneyBefore, 0) >= 3000 THEN 'Full_Buy'
                                WHEN COALESCE(ee.MoneyBefore, 0) >= 1500 THEN 'Force'
                                WHEN COALESCE(ee.MoneyBefore, 0) >= 500 THEN 'Eco'
                                ELSE 'Save'
                            END as EconomyState,
                            
                            -- UTILITY SITUATION
                            CASE 
                                WHEN COALESCE(fe.FlashDuration, 0) > 1.0 THEN 'Flashed'
                                ELSE 'Clear'
                            END as UtilityState,
                            
                            -- PERFORMANCE METRICS PER SITUATION
                            COALESCE(prs.Kills, 0) as Kills,
                            COALESCE(prs.Deaths, 0) as Deaths,
                            COALESCE(prs.Assists, 0) as Assists,
                            COALESCE(prs.Damage, 0) as Damage,
                            COALESCE(prs.Rating, 0.0) as Rating,
                            COALESCE(prs.IsAlive, 0) as IsAlive,
                            CASE WHEN p.Team = r.WinnerTeam THEN 1 ELSE 0 END as RoundWon
                            
                        FROM Players p
                        INNER JOIN DemoFiles d ON p.DemoFileId = d.Id
                        INNER JOIN PlayerRoundStats prs ON p.Id = prs.PlayerId
                        INNER JOIN Rounds r ON prs.RoundId = r.Id AND r.DemoFileId = d.Id
                        
                        LEFT JOIN EconomyEvents ee ON p.Id = ee.PlayerId AND ee.RoundNumber = r.RoundNumber
                        LEFT JOIN FlashEvents fe ON p.Id = fe.FlashedPlayerId AND fe.RoundId = r.Id
                        
                        WHERE (@DemoId IS NULL OR d.Id = @DemoId)
                            AND (@MapName IS NULL OR d.MapName = @MapName)
                            AND (@PlayerName IS NULL OR p.PlayerName = @PlayerName)
                            AND (@Team IS NULL OR p.Team = @Team)
                    )
                    SELECT 
                        PlayerName,
                        Team,
                        MapName,
                        SituationType,
                        EconomyState,
                        UtilityState,
                        
                        -- SITUATION FREQUENCY
                        COUNT(*) as TimesInSituation,
                        
                        -- PERFORMANCE IN SITUATION
                        AVG(CAST(Kills AS FLOAT)) as AvgKillsInSituation,
                        AVG(CAST(Deaths AS FLOAT)) as AvgDeathsInSituation,
                        AVG(CAST(Assists AS FLOAT)) as AvgAssistsInSituation,
                        AVG(CAST(Damage AS FLOAT)) as AvgDamageInSituation,
                        AVG(Rating) as AvgRatingInSituation,
                        
                        -- SUCCESS METRICS
                        SUM(RoundWon) as RoundsWonInSituation,
                        CAST(SUM(RoundWon) AS FLOAT) / COUNT(*) * 100 as WinPercentageInSituation,
                        SUM(IsAlive) as SurvivedInSituation,
                        CAST(SUM(IsAlive) AS FLOAT) / COUNT(*) * 100 as SurvivalRateInSituation,
                        
                        -- COMBAT EFFECTIVENESS IN SITUATION
                        CASE 
                            WHEN AVG(CAST(Deaths AS FLOAT)) > 0 THEN AVG(CAST(Kills AS FLOAT)) / AVG(CAST(Deaths AS FLOAT))
                            ELSE AVG(CAST(Kills AS FLOAT))
                        END as KDRatioInSituation,
                        
                        -- SITUATIONAL DIFFICULTY SCORE (0-10)
                        CASE 
                            WHEN SituationType LIKE '1vX_%' THEN 9.0
                            WHEN SituationType LIKE '2vX_%' THEN 7.0
                            WHEN SituationType = 'PostPlant_Alive' THEN 6.0
                            WHEN EconomyState = 'Save' THEN 8.0
                            WHEN EconomyState = 'Eco' THEN 6.0
                            WHEN UtilityState = 'Flashed' THEN 7.0
                            ELSE 3.0
                        END as DifficultyScore,
                        
                        -- PERFORMANCE VS DIFFICULTY RATIO (higher = better under pressure)
                        CASE 
                            WHEN SituationType LIKE '1vX_%' AND AVG(Rating) > 0 THEN AVG(Rating) * 3.0  -- 3x multiplier for 1vX situations
                            WHEN SituationType LIKE '2vX_%' AND AVG(Rating) > 0 THEN AVG(Rating) * 2.0  -- 2x multiplier for 2vX situations
                            WHEN EconomyState IN ('Save', 'Eco') AND AVG(Rating) > 0 THEN AVG(Rating) * 1.5  -- 1.5x multiplier for eco situations
                            WHEN UtilityState = 'Flashed' AND AVG(Rating) > 0 THEN AVG(Rating) * 1.8  -- 1.8x for flashed performance
                            ELSE AVG(Rating)
                        END as PressurePerformanceScore
                        
                    FROM SituationalPerformance
                    GROUP BY PlayerName, Team, MapName, SituationType, EconomyState, UtilityState
                    HAVING COUNT(*) >= 2  -- At least 2 occurrences of this situation
                    ORDER BY PressurePerformanceScore DESC, WinPercentageInSituation DESC";

                var data = await ExecuteAnalyticsQuery(sql, query);
                
                if (query.Format?.ToLower() == "csv")
                {
                    var csv = ConvertToCsv(data);
                    var fileName = $"situation_analysis_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                    Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");
                    return File(Encoding.UTF8.GetBytes(csv), "text/csv");
                }

                return Ok(new
                {
                    Title = "Situational Performance Analysis",
                    Description = "Deep dive into player performance across different game situations: clutches, economy states, utility conditions, and time phases with pressure performance scoring",
                    Data = data,
                    TotalRecords = data.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating situation analysis");
                return StatusCode(500, $"Error generating situation analysis: {ex.Message}");
            }
        }

        [HttpGet("economy-intelligence-enhanced")]
        public async Task<IActionResult> GetEconomyIntelligenceEnhanced([FromQuery] AnalyticsQuery query)
        {
            try
            {
                var sql = @"
                    WITH EconomyAnalysis AS (
                        SELECT 
                            p.PlayerName,
                            p.Team,
                            d.MapName,
                            r.RoundNumber,
                            r.WinnerTeam,
                            
                            -- ECONOMY EVENTS DATA
                            e.EventType,
                            e.ItemName,
                            e.ItemCost,
                            e.MoneyBefore,
                            e.MoneyAfter,
                            e.MoneyBefore - e.MoneyAfter as MoneySpent,
                            
                            -- ROUND CONTEXT
                            CASE WHEN p.Team = r.WinnerTeam THEN 1 ELSE 0 END as RoundWon,
                            prs.Kills as RoundKills,
                            prs.Deaths as RoundDeaths,
                            prs.Damage as RoundDamage,
                            
                            -- EQUIPMENT CLASSIFICATION
                            CASE 
                                WHEN e.ItemName LIKE '%ak47%' OR e.ItemName LIKE '%m4a%' THEN 'Rifle'
                                WHEN e.ItemName LIKE '%awp%' OR e.ItemName LIKE '%ssg08%' THEN 'Sniper'
                                WHEN e.ItemName LIKE '%glock%' OR e.ItemName LIKE '%usp%' OR e.ItemName LIKE '%p250%' THEN 'Pistol'
                                WHEN e.ItemName LIKE '%he%' OR e.ItemName LIKE '%smoke%' OR e.ItemName LIKE '%flash%' THEN 'Utility'
                                WHEN e.ItemName LIKE '%vest%' OR e.ItemName LIKE '%helmet%' THEN 'Armor'
                                ELSE 'Other'
                            END as ItemCategory,
                            
                            -- MONEY MANAGEMENT
                            CASE 
                                WHEN e.MoneyBefore >= 10000 THEN 'Full_Buy'
                                WHEN e.MoneyBefore >= 5000 THEN 'Force_Buy'
                                WHEN e.MoneyBefore >= 2000 THEN 'Eco_Buy'
                                ELSE 'Save_Round'
                            END as EconomyState
                            
                        FROM EconomyEvents e
                        INNER JOIN Players p ON e.PlayerId = p.Id
                        INNER JOIN DemoFiles d ON e.DemoFileId = d.Id
                        INNER JOIN Rounds r ON e.RoundNumber = r.RoundNumber AND r.DemoFileId = d.Id
                        LEFT JOIN PlayerRoundStats prs ON p.Id = prs.PlayerId AND r.Id = prs.RoundId";

                // Apply filters
                if (!string.IsNullOrEmpty(query.MapName))
                    sql += " AND d.MapName = @MapName";
                if (!string.IsNullOrEmpty(query.PlayerName))
                    sql += " AND p.PlayerName = @PlayerName";
                if (!string.IsNullOrEmpty(query.Team))
                    sql += " AND p.Team = @Team";
                if (query.RoundNumber.HasValue)
                    sql += " AND r.RoundNumber = @RoundNumber";

                sql += @"
                    )
                    SELECT 
                        PlayerName,
                        Team,
                        MapName,
                        ItemCategory,
                        EconomyState,
                        
                        -- SPENDING ANALYSIS
                        COUNT(*) as PurchaseCount,
                        SUM(ItemCost) as TotalSpent,
                        ROUND(AVG(CAST(ItemCost AS FLOAT)), 2) as AvgItemCost,
                        ROUND(AVG(CAST(MoneyBefore AS FLOAT)), 2) as AvgMoneyBefore,
                        ROUND(AVG(CAST(MoneyAfter AS FLOAT)), 2) as AvgMoneyAfter,
                        
                        -- ROI ANALYSIS
                        SUM(RoundWon) as RoundsWon,
                        ROUND(SUM(RoundWon) * 100.0 / COUNT(*), 2) as WinRateWithPurchase,
                        SUM(RoundKills) as TotalKills,
                        SUM(RoundDamage) as TotalDamage,
                        ROUND(SUM(RoundKills) * 1.0 / NULLIF(SUM(ItemCost), 0) * 1000, 2) as KillsPerThousandSpent,
                        ROUND(SUM(RoundDamage) * 1.0 / NULLIF(SUM(ItemCost), 0), 2) as DamagePerDollarSpent,
                        
                        -- MONEY MANAGEMENT SCORE
                        CASE 
                            WHEN ROUND(SUM(RoundWon) * 100.0 / COUNT(*), 2) >= 70 AND ItemCategory = 'Rifle' THEN 95
                            WHEN ROUND(SUM(RoundWon) * 100.0 / COUNT(*), 2) >= 60 AND EconomyState = 'Force_Buy' THEN 85
                            WHEN ROUND(SUM(RoundKills) * 1.0 / NULLIF(SUM(ItemCost), 0) * 1000, 2) >= 0.5 THEN 80
                            WHEN ROUND(SUM(RoundWon) * 100.0 / COUNT(*), 2) >= 50 THEN 70
                            ELSE 50
                        END as MoneyManagementScore
                        
                    FROM EconomyAnalysis
                    GROUP BY PlayerName, Team, MapName, ItemCategory, EconomyState
                    HAVING COUNT(*) >= 2
                    ORDER BY MoneyManagementScore DESC, WinRateWithPurchase DESC";

                var data = await ExecuteAnalyticsQuery(sql, query);
                
                if (query.Format?.ToLower() == "csv")
                {
                    var csv = ConvertToCsv(data);
                    var fileName = $"economy_intelligence_enhanced_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                    Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");
                    return File(Encoding.UTF8.GetBytes(csv), "text/csv");
                }

                return Ok(new
                {
                    Title = "Enhanced Economy Intelligence",
                    Description = "Advanced economic analysis including ROI, money management, and purchase effectiveness",
                    Data = data,
                    TotalRecords = data.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating enhanced economy intelligence");
                return StatusCode(500, $"Error generating enhanced economy intelligence: {ex.Message}");
            }
        }

        [HttpGet("movement-positioning")]
        public async Task<IActionResult> GetMovementPositioning([FromQuery] AnalyticsQuery query)
        {
            try
            {
                var sql = @"
                    WITH PositionData AS (
                        SELECT 
                            d.MapName,
                            victim.PlayerName as PlayerName,
                            victim.Team,
                            r.RoundNumber,
                            
                            -- DEATH POSITIONS
                            k.VictimPositionX as DeathX,
                            k.VictimPositionY as DeathY,
                            k.VictimPositionZ as DeathZ,
                            
                            -- KILLER POSITIONS  
                            k.KillerPositionX as KillerX,
                            k.KillerPositionY as KillerY,
                            k.KillerPositionZ as KillerZ,
                            
                            -- DISTANCE AND ANGLE
                            k.Distance,
                            k.Weapon,
                            k.IsHeadshot,
                            
                            -- MAP AREA CLASSIFICATION
                            CASE 
                                WHEN d.MapName = 'de_dust2' THEN
                                    CASE 
                                        WHEN k.VictimPositionY > 1000 THEN 'A_Site'
                                        WHEN k.VictimPositionY < -1000 THEN 'B_Site'
                                        WHEN k.VictimPositionX > 0 AND ABS(k.VictimPositionY) < 1000 THEN 'Mid'
                                        WHEN k.VictimPositionX < 0 THEN 'Tunnels'
                                        ELSE 'Other'
                                    END
                                WHEN d.MapName = 'de_mirage' THEN
                                    CASE 
                                        WHEN k.VictimPositionX > 1000 THEN 'A_Site'
                                        WHEN k.VictimPositionX < -1000 THEN 'B_Site'
                                        WHEN ABS(k.VictimPositionX) < 500 THEN 'Mid'
                                        WHEN k.VictimPositionY > 500 THEN 'Connector'
                                        ELSE 'Other'
                                    END
                                WHEN d.MapName = 'de_inferno' THEN
                                    CASE 
                                        WHEN k.VictimPositionY > 1000 THEN 'A_Site'
                                        WHEN k.VictimPositionY < -1000 THEN 'B_Site'
                                        WHEN ABS(k.VictimPositionY) < 500 THEN 'Mid'
                                        ELSE 'Other'
                                    END
                                ELSE 'Unknown_Map'
                            END as MapArea,
                            
                            -- POSITIONING ADVANTAGE
                            CASE 
                                WHEN k.Distance > 1000 THEN 'Long_Range_Position'
                                WHEN k.Distance < 300 THEN 'Close_Range_Position'
                                WHEN ABS(k.KillerPositionZ - k.VictimPositionZ) > 100 THEN 'Elevation_Advantage'
                                ELSE 'Standard_Position'
                            END as PositionType
                            
                        FROM Kills k
                        INNER JOIN Players victim ON k.VictimId = victim.Id
                        INNER JOIN Rounds r ON k.RoundId = r.Id
                        INNER JOIN DemoFiles d ON r.DemoFileId = d.Id
                        WHERE k.VictimPositionX != 0 AND k.VictimPositionY != 0";

                // Apply filters
                if (!string.IsNullOrEmpty(query.MapName))
                    sql += " AND d.MapName = @MapName";
                if (!string.IsNullOrEmpty(query.PlayerName))
                    sql += " AND victim.PlayerName = @PlayerName";
                if (!string.IsNullOrEmpty(query.Team))
                    sql += " AND victim.Team = @Team";
                if (query.RoundNumber.HasValue)
                    sql += " AND r.RoundNumber = @RoundNumber";

                sql += @"
                    )
                    SELECT 
                        MapName,
                        MapArea,
                        PositionType,
                        
                        -- DEATH ZONE ANALYSIS
                        COUNT(*) as DeathCount,
                        COUNT(DISTINCT PlayerName) as PlayersAffected,
                        ROUND(AVG(DeathX), 2) as AvgDeathX,
                        ROUND(AVG(DeathY), 2) as AvgDeathY,
                        ROUND(AVG(DeathZ), 2) as AvgDeathZ,
                        
                        -- POSITION DANGER METRICS
                        ROUND(AVG(Distance), 2) as AvgKillDistance,
                        COUNT(CASE WHEN IsHeadshot = 1 THEN 1 END) as HeadshotDeaths,
                        ROUND(COUNT(CASE WHEN IsHeadshot = 1 THEN 1 END) * 100.0 / COUNT(*), 2) as HeadshotDeathRate,
                        
                        -- WEAPON EFFECTIVENESS BY POSITION
                        COUNT(DISTINCT Weapon) as WeaponsUsed,
                        
                        -- POSITION DANGER SCORE
                        CASE 
                            WHEN COUNT(*) >= 20 AND ROUND(COUNT(CASE WHEN IsHeadshot = 1 THEN 1 END) * 100.0 / COUNT(*), 2) >= 60 THEN 95
                            WHEN COUNT(*) >= 15 AND ROUND(AVG(Distance), 2) < 500 THEN 85
                            WHEN COUNT(*) >= 10 THEN 70
                            WHEN COUNT(*) >= 5 THEN 55
                            ELSE 40
                        END as PositionDangerScore
                        
                    FROM PositionData
                    GROUP BY MapName, MapArea, PositionType
                    HAVING COUNT(*) >= 3
                    ORDER BY PositionDangerScore DESC, DeathCount DESC";

                var data = await ExecuteAnalyticsQuery(sql, query);
                
                if (query.Format?.ToLower() == "csv")
                {
                    var csv = ConvertToCsv(data);
                    var fileName = $"movement_positioning_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                    Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");
                    return File(Encoding.UTF8.GetBytes(csv), "text/csv");
                }

                return Ok(new
                {
                    Title = "Movement & Positioning Analytics",
                    Description = "Analysis of death zones, positioning advantages, and map control patterns",
                    Data = data,
                    TotalRecords = data.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating movement positioning analysis");
                return StatusCode(500, $"Error generating movement positioning analysis: {ex.Message}");
            }
        }

        [HttpGet("timing-tempo")]
        public async Task<IActionResult> GetTimingTempo([FromQuery] AnalyticsQuery query)
        {
            try
            {
                var sql = @"
                    WITH TimingData AS (
                        SELECT 
                            d.MapName,
                            p.PlayerName,
                            p.Team,
                            r.RoundNumber,
                            r.WinnerTeam,
                            
                            -- ROUND TIMING
                            k.GameTime,
                            LAG(k.GameTime) OVER (PARTITION BY r.Id ORDER BY k.GameTime) as PrevKillTime,
                            k.GameTime - LAG(k.GameTime) OVER (PARTITION BY r.Id ORDER BY k.GameTime) as TimeBetweenKills,
                            
                            -- KILL SEQUENCE
                            ROW_NUMBER() OVER (PARTITION BY r.Id ORDER BY k.GameTime) as KillSequence,
                            COUNT(*) OVER (PARTITION BY r.Id) as TotalRoundKills,
                            
                            -- ROUND PHASE
                            CASE 
                                WHEN k.GameTime <= 30 THEN 'Opening'
                                WHEN k.GameTime <= 60 THEN 'Mid_Round'
                                WHEN k.GameTime <= 90 THEN 'Late_Round'
                                ELSE 'Overtime'
                            END as RoundPhase,
                            
                            -- TEMPO CLASSIFICATION
                            CASE 
                                WHEN k.GameTime <= 15 THEN 'Aggressive_Rush'
                                WHEN k.GameTime <= 45 THEN 'Standard_Execute'
                                WHEN k.GameTime <= 75 THEN 'Slow_Default'
                                ELSE 'Late_Round_Play'
                            END as TempoStyle,
                            
                            k.Weapon,
                            k.IsHeadshot,
                            CASE WHEN p.Team = r.WinnerTeam THEN 1 ELSE 0 END as RoundWon
                            
                        FROM Kills k
                        INNER JOIN Players p ON k.KillerId = p.Id
                        INNER JOIN Rounds r ON k.RoundId = r.Id
                        INNER JOIN DemoFiles d ON r.DemoFileId = d.Id";

                // Apply filters
                if (!string.IsNullOrEmpty(query.MapName))
                    sql += " WHERE d.MapName = @MapName";
                if (!string.IsNullOrEmpty(query.PlayerName))
                    sql += (sql.Contains("WHERE") ? " AND" : " WHERE") + " p.PlayerName = @PlayerName";
                if (!string.IsNullOrEmpty(query.Team))
                    sql += (sql.Contains("WHERE") ? " AND" : " WHERE") + " p.Team = @Team";
                if (query.RoundNumber.HasValue)
                    sql += (sql.Contains("WHERE") ? " AND" : " WHERE") + " r.RoundNumber = @RoundNumber";

                sql += @"
                    )
                    SELECT 
                        PlayerName,
                        Team,
                        MapName,
                        RoundPhase,
                        TempoStyle,
                        
                        -- TIMING METRICS
                        COUNT(*) as KillsInPhase,
                        ROUND(AVG(GameTime), 2) as AvgKillTiming,
                        ROUND(AVG(TimeBetweenKills), 2) as AvgTimeBetweenKills,
                        
                        -- SEQUENCE ANALYSIS
                        COUNT(CASE WHEN KillSequence = 1 THEN 1 END) as FirstKills,
                        COUNT(CASE WHEN KillSequence = TotalRoundKills THEN 1 END) as LastKills,
                        ROUND(AVG(CAST(KillSequence AS FLOAT)), 2) as AvgKillSequence,
                        
                        -- TEMPO SUCCESS
                        SUM(RoundWon) as RoundsWon,
                        ROUND(SUM(RoundWon) * 100.0 / COUNT(DISTINCT RoundNumber), 2) as TempoWinRate,
                        
                        -- EXECUTION EFFICIENCY
                        COUNT(CASE WHEN IsHeadshot = 1 THEN 1 END) as HeadshotKills,
                        ROUND(COUNT(CASE WHEN IsHeadshot = 1 THEN 1 END) * 100.0 / COUNT(*), 2) as HeadshotRate,
                        COUNT(DISTINCT Weapon) as WeaponsUsed,
                        
                        -- TEMPO MASTERY SCORE
                        CASE 
                            WHEN TempoStyle = 'Aggressive_Rush' AND ROUND(SUM(RoundWon) * 100.0 / COUNT(DISTINCT RoundNumber), 2) >= 70 THEN 95
                            WHEN TempoStyle = 'Late_Round_Play' AND COUNT(CASE WHEN KillSequence = TotalRoundKills THEN 1 END) >= 5 THEN 90
                            WHEN ROUND(SUM(RoundWon) * 100.0 / COUNT(DISTINCT RoundNumber), 2) >= 60 THEN 80
                            WHEN COUNT(CASE WHEN KillSequence = 1 THEN 1 END) >= 3 THEN 70
                            ELSE 55
                        END as TempoMasteryScore
                        
                    FROM TimingData
                    GROUP BY PlayerName, Team, MapName, RoundPhase, TempoStyle
                    HAVING COUNT(*) >= 3
                    ORDER BY TempoMasteryScore DESC, TempoWinRate DESC";

                var data = await ExecuteAnalyticsQuery(sql, query);
                
                if (query.Format?.ToLower() == "csv")
                {
                    var csv = ConvertToCsv(data);
                    var fileName = $"timing_tempo_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                    Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");
                    return File(Encoding.UTF8.GetBytes(csv), "text/csv");
                }

                return Ok(new
                {
                    Title = "Timing & Tempo Analytics",
                    Description = "Analysis of round timing patterns, execution speed, and tempo effectiveness",
                    Data = data,
                    TotalRecords = data.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating timing tempo analysis");
                return StatusCode(500, $"Error generating timing tempo analysis: {ex.Message}");
            }
        }

        [HttpGet("weapon-mastery")]
        public async Task<IActionResult> GetWeaponMastery([FromQuery] AnalyticsQuery query)
        {
            try
            {
                var sql = @"
                    WITH WeaponMasteryData AS (
                        SELECT 
                            p.PlayerName,
                            p.Team,
                            d.MapName,
                            k.Weapon,
                            
                            -- WEAPON CLASSIFICATION
                            CASE 
                                WHEN k.Weapon LIKE '%ak47%' OR k.Weapon LIKE '%m4a%' THEN 'Assault_Rifle'
                                WHEN k.Weapon LIKE '%awp%' OR k.Weapon LIKE '%ssg08%' THEN 'Sniper_Rifle'
                                WHEN k.Weapon LIKE '%glock%' OR k.Weapon LIKE '%usp%' OR k.Weapon LIKE '%p250%' OR k.Weapon LIKE '%deagle%' THEN 'Pistol'
                                WHEN k.Weapon LIKE '%mp%' OR k.Weapon LIKE '%mac10%' OR k.Weapon LIKE '%ump%' THEN 'SMG'
                                WHEN k.Weapon LIKE '%knife%' THEN 'Knife'
                                ELSE 'Other'
                            END as WeaponClass,
                            
                            -- PERFORMANCE METRICS
                            k.Distance,
                            k.IsHeadshot,
                            k.GameTime,
                            r.RoundNumber,
                            
                            -- SITUATIONAL CONTEXT
                            CASE 
                                WHEN k.Distance > 1200 THEN 'Long_Range'
                                WHEN k.Distance > 600 THEN 'Medium_Range'
                                WHEN k.Distance > 300 THEN 'Short_Range'
                                ELSE 'Close_Range'
                            END as EngagementRange,
                            
                            CASE WHEN p.Team = r.WinnerTeam THEN 1 ELSE 0 END as RoundWon
                            
                        FROM Kills k
                        INNER JOIN Players p ON k.KillerId = p.Id
                        INNER JOIN Rounds r ON k.RoundId = r.Id
                        INNER JOIN DemoFiles d ON r.DemoFileId = d.Id";

                // Apply filters
                if (!string.IsNullOrEmpty(query.MapName))
                    sql += " WHERE d.MapName = @MapName";
                if (!string.IsNullOrEmpty(query.PlayerName))
                    sql += (sql.Contains("WHERE") ? " AND" : " WHERE") + " p.PlayerName = @PlayerName";
                if (!string.IsNullOrEmpty(query.Team))
                    sql += (sql.Contains("WHERE") ? " AND" : " WHERE") + " p.Team = @Team";
                if (query.RoundNumber.HasValue)
                    sql += (sql.Contains("WHERE") ? " AND" : " WHERE") + " r.RoundNumber = @RoundNumber";

                sql += @"
                    )
                    SELECT 
                        PlayerName,
                        Team,
                        MapName,
                        WeaponClass,
                        Weapon,
                        EngagementRange,
                        
                        -- USAGE STATISTICS
                        COUNT(*) as KillsWithWeapon,
                        ROUND(AVG(Distance), 2) as AvgKillDistance,
                        MIN(Distance) as MinKillDistance,
                        MAX(Distance) as MaxKillDistance,
                        
                        -- ACCURACY METRICS
                        SUM(CAST(IsHeadshot AS INT)) as HeadshotKills,
                        ROUND(SUM(CAST(IsHeadshot AS INT)) * 100.0 / COUNT(*), 2) as HeadshotPercentage,
                        
                        -- EFFECTIVENESS METRICS
                        SUM(RoundWon) as RoundsWonWithWeapon,
                        ROUND(SUM(RoundWon) * 100.0 / COUNT(*), 2) as WinRateWithWeapon,
                        
                        -- PROGRESSION ANALYSIS
                        ROUND(AVG(GameTime), 2) as AvgKillTiming,
                        COUNT(DISTINCT RoundNumber) as RoundsUsed,
                        
                        -- WEAPON MASTERY SCORE
                        CASE 
                            WHEN WeaponClass = 'Sniper_Rifle' AND ROUND(SUM(CAST(IsHeadshot AS INT)) * 100.0 / COUNT(*), 2) >= 80 THEN 100
                            WHEN WeaponClass = 'Assault_Rifle' AND ROUND(SUM(CAST(IsHeadshot AS INT)) * 100.0 / COUNT(*), 2) >= 60 AND COUNT(*) >= 20 THEN 95
                            WHEN WeaponClass = 'Pistol' AND ROUND(SUM(CAST(IsHeadshot AS INT)) * 100.0 / COUNT(*), 2) >= 70 THEN 90
                            WHEN ROUND(SUM(RoundWon) * 100.0 / COUNT(*), 2) >= 70 AND COUNT(*) >= 10 THEN 85
                            WHEN ROUND(SUM(CAST(IsHeadshot AS INT)) * 100.0 / COUNT(*), 2) >= 40 AND COUNT(*) >= 5 THEN 75
                            WHEN COUNT(*) >= 10 THEN 65
                            ELSE 50
                        END as WeaponMasteryScore,
                        
                        -- VERSATILITY BONUS
                        COUNT(DISTINCT EngagementRange) as RangeVersatility
                        
                    FROM WeaponMasteryData
                    GROUP BY PlayerName, Team, MapName, WeaponClass, Weapon, EngagementRange
                    HAVING COUNT(*) >= 2
                    ORDER BY WeaponMasteryScore DESC, HeadshotPercentage DESC, KillsWithWeapon DESC";

                var data = await ExecuteAnalyticsQuery(sql, query);
                
                if (query.Format?.ToLower() == "csv")
                {
                    var csv = ConvertToCsv(data);
                    var fileName = $"weapon_mastery_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                    Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");
                    return File(Encoding.UTF8.GetBytes(csv), "text/csv");
                }

                return Ok(new
                {
                    Title = "Weapon Mastery Analytics",
                    Description = "Detailed weapon performance analysis including accuracy, versatility, and progression",
                    Data = data,
                    TotalRecords = data.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating weapon mastery analysis");
                return StatusCode(500, $"Error generating weapon mastery analysis: {ex.Message}");
            }
        }

        [HttpGet("match-flow")]
        public async Task<IActionResult> GetMatchFlow([FromQuery] AnalyticsQuery query)
        {
            try
            {
                var sql = @"
                    WITH MatchFlowData AS (
                        SELECT 
                            d.MapName,
                            d.Id as DemoFileId,
                            r.RoundNumber,
                            r.WinnerTeam,
                            p.PlayerName,
                            p.Team,
                            
                            -- ROUND OUTCOME
                            CASE WHEN p.Team = r.WinnerTeam THEN 1 ELSE 0 END as RoundWon,
                            
                            -- MOMENTUM CALCULATION
                            LAG(CASE WHEN p.Team = r.WinnerTeam THEN 1 ELSE 0 END, 1) OVER 
                                (PARTITION BY d.Id, p.Team ORDER BY r.RoundNumber) as PrevRoundWon,
                            LAG(CASE WHEN p.Team = r.WinnerTeam THEN 1 ELSE 0 END, 2) OVER 
                                (PARTITION BY d.Id, p.Team ORDER BY r.RoundNumber) as TwoRoundsAgo,
                            
                            -- STREAK DETECTION
                            CASE WHEN p.Team = r.WinnerTeam THEN 1 ELSE -1 END as StreakValue,
                            
                            -- PLAYER PERFORMANCE
                            prs.Kills,
                            prs.Deaths,
                            prs.Damage,
                            
                            -- SCORE CONTEXT  
                            SUM(CASE WHEN p.Team = r.WinnerTeam THEN 1 ELSE 0 END) OVER 
                                (PARTITION BY d.Id, p.Team ORDER BY r.RoundNumber ROWS UNBOUNDED PRECEDING) as TeamScore,
                            SUM(CASE WHEN p.Team != r.WinnerTeam THEN 1 ELSE 0 END) OVER 
                                (PARTITION BY d.Id, p.Team ORDER BY r.RoundNumber ROWS UNBOUNDED PRECEDING) as OpponentScore
                            
                        FROM Rounds r
                        INNER JOIN DemoFiles d ON r.DemoFileId = d.Id
                        INNER JOIN PlayerRoundStats prs ON r.Id = prs.RoundId
                        INNER JOIN Players p ON prs.PlayerId = p.Id";

                // Apply filters
                if (!string.IsNullOrEmpty(query.MapName))
                    sql += " WHERE d.MapName = @MapName";
                if (!string.IsNullOrEmpty(query.PlayerName))
                    sql += (sql.Contains("WHERE") ? " AND" : " WHERE") + " p.PlayerName = @PlayerName";
                if (!string.IsNullOrEmpty(query.Team))
                    sql += (sql.Contains("WHERE") ? " AND" : " WHERE") + " p.Team = @Team";
                if (query.RoundNumber.HasValue)
                    sql += (sql.Contains("WHERE") ? " AND" : " WHERE") + " r.RoundNumber = @RoundNumber";

                sql += @"
                    ),
                    MomentumAnalysis AS (
                        SELECT *,
                            -- MOMENTUM CLASSIFICATION
                            CASE 
                                WHEN RoundWon = 1 AND PrevRoundWon = 1 AND TwoRoundsAgo = 1 THEN 'Hot_Streak'
                                WHEN RoundWon = 1 AND PrevRoundWon = 1 THEN 'Building_Momentum'
                                WHEN RoundWon = 0 AND PrevRoundWon = 0 AND TwoRoundsAgo = 0 THEN 'Cold_Streak'
                                WHEN RoundWon = 0 AND PrevRoundWon = 0 THEN 'Losing_Momentum'
                                WHEN RoundWon = 1 AND PrevRoundWon = 0 THEN 'Comeback_Round'
                                WHEN RoundWon = 0 AND PrevRoundWon = 1 THEN 'Momentum_Lost'
                                ELSE 'Neutral'
                            END as MomentumState,
                            
                            -- PRESSURE SITUATION
                            CASE 
                                WHEN ABS(TeamScore - OpponentScore) <= 1 THEN 'Close_Game'
                                WHEN TeamScore - OpponentScore >= 3 THEN 'Leading'
                                WHEN OpponentScore - TeamScore >= 3 THEN 'Trailing'
                                ELSE 'Balanced'
                            END as GameState
                            
                        FROM MatchFlowData
                    )
                    SELECT 
                        PlayerName,
                        Team,
                        MapName,
                        MomentumState,
                        GameState,
                        
                        -- MOMENTUM METRICS
                        COUNT(*) as RoundsInState,
                        SUM(RoundWon) as RoundsWonInState,
                        ROUND(SUM(RoundWon) * 100.0 / COUNT(*), 2) as WinRateInState,
                        
                        -- PERFORMANCE UNDER PRESSURE
                        ROUND(AVG(CAST(Kills AS FLOAT)), 2) as AvgKillsInState,
                        ROUND(AVG(CAST(Deaths AS FLOAT)), 2) as AvgDeathsInState,
                        ROUND(AVG(CAST(Damage AS FLOAT)), 2) as AvgDamageInState,
                        
                        -- CLUTCH FACTOR
                        COUNT(CASE WHEN Kills > Deaths AND RoundWon = 1 THEN 1 END) as ClutchRounds,
                        COUNT(CASE WHEN GameState = 'Close_Game' AND RoundWon = 1 THEN 1 END) as CloseGameWins,
                        
                        -- MOMENTUM MASTERY SCORE
                        CASE 
                            WHEN MomentumState = 'Comeback_Round' AND ROUND(SUM(RoundWon) * 100.0 / COUNT(*), 2) >= 70 THEN 100
                            WHEN MomentumState = 'Hot_Streak' AND ROUND(AVG(CAST(Kills AS FLOAT)), 2) >= 2.0 THEN 95
                            WHEN GameState = 'Close_Game' AND ROUND(SUM(RoundWon) * 100.0 / COUNT(*), 2) >= 60 THEN 90
                            WHEN MomentumState = 'Building_Momentum' AND COUNT(*) >= 5 THEN 85
                            WHEN ROUND(SUM(RoundWon) * 100.0 / COUNT(*), 2) >= 50 THEN 70
                            ELSE 50
                        END as MomentumMasteryScore
                        
                    FROM MomentumAnalysis
                    GROUP BY PlayerName, Team, MapName, MomentumState, GameState
                    HAVING COUNT(*) >= 3
                    ORDER BY MomentumMasteryScore DESC, WinRateInState DESC";

                var data = await ExecuteAnalyticsQuery(sql, query);
                
                if (query.Format?.ToLower() == "csv")
                {
                    var csv = ConvertToCsv(data);
                    var fileName = $"match_flow_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                    Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");
                    return File(Encoding.UTF8.GetBytes(csv), "text/csv");
                }

                return Ok(new
                {
                    Title = "Match Flow Analytics",
                    Description = "Analysis of momentum patterns, comeback potential, and performance under pressure",
                    Data = data,
                    TotalRecords = data.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating match flow analysis");
                return StatusCode(500, $"Error generating match flow analysis: {ex.Message}");
            }
        }

        [HttpGet("performance-trends")]
        public async Task<IActionResult> GetPerformanceTrends([FromQuery] AnalyticsQuery query)
        {
            try
            {
                var sql = @"
                    WITH PerformanceData AS (
                        SELECT 
                            p.PlayerName,
                            p.Team,
                            d.MapName,
                            d.ParsedAt,
                            r.RoundNumber,
                            
                            -- PERFORMANCE METRICS
                            prs.Kills,
                            prs.Deaths,
                            prs.Damage,
                            prs.Assists,
                            CASE WHEN prs.Deaths = 0 THEN prs.Kills ELSE CAST(prs.Kills AS FLOAT) / prs.Deaths END as KDRatio,
                            
                            -- TIME-BASED GROUPING
                            ROW_NUMBER() OVER (PARTITION BY p.Id ORDER BY d.ParsedAt, r.RoundNumber) as ChronologicalOrder,
                            NTILE(5) OVER (PARTITION BY p.Id ORDER BY d.ParsedAt, r.RoundNumber) as TimeQuintile,
                            
                            -- MAP EXPERIENCE
                            ROW_NUMBER() OVER (PARTITION BY p.Id, d.MapName ORDER BY d.ParsedAt, r.RoundNumber) as MapExperience,
                            
                            -- ROUND OUTCOME
                            CASE WHEN p.Team = r.WinnerTeam THEN 1 ELSE 0 END as RoundWon
                            
                        FROM PlayerRoundStats prs
                        INNER JOIN Players p ON prs.PlayerId = p.Id
                        INNER JOIN Rounds r ON prs.RoundId = r.Id
                        INNER JOIN DemoFiles d ON r.DemoFileId = d.Id";

                // Apply filters
                if (!string.IsNullOrEmpty(query.MapName))
                    sql += " WHERE d.MapName = @MapName";
                if (!string.IsNullOrEmpty(query.PlayerName))
                    sql += (sql.Contains("WHERE") ? " AND" : " WHERE") + " p.PlayerName = @PlayerName";
                if (!string.IsNullOrEmpty(query.Team))
                    sql += (sql.Contains("WHERE") ? " AND" : " WHERE") + " p.Team = @Team";

                sql += @"
                    )
                    SELECT 
                        PlayerName,
                        Team,
                        MapName,
                        TimeQuintile,
                        
                        -- PERFORMANCE PROGRESSION
                        COUNT(*) as RoundsInPeriod,
                        ROUND(AVG(CAST(Kills AS FLOAT)), 2) as AvgKills,
                        ROUND(AVG(CAST(Deaths AS FLOAT)), 2) as AvgDeaths,
                        ROUND(AVG(CAST(Damage AS FLOAT)), 2) as AvgDamage,
                        ROUND(AVG(KDRatio), 2) as AvgKDRatio,
                        
                        -- WIN RATE TRENDS
                        SUM(RoundWon) as RoundsWon,
                        ROUND(SUM(RoundWon) * 100.0 / COUNT(*), 2) as WinRate,
                        
                        -- CONSISTENCY METRICS
                        ROUND(STDEV(CAST(Kills AS FLOAT)), 2) as KillsStdDev,
                        ROUND(STDEV(CAST(Damage AS FLOAT)), 2) as DamageStdDev,
                        
                        -- MAP LEARNING CURVE
                        MAX(MapExperience) as MaxMapExperience,
                        
                        -- IMPROVEMENT SCORE
                        CASE 
                            WHEN TimeQuintile = 5 THEN
                                CASE 
                                    WHEN ROUND(AVG(CAST(Kills AS FLOAT)), 2) >= 1.5 AND ROUND(AVG(KDRatio), 2) >= 1.2 THEN 95
                                    WHEN ROUND(SUM(RoundWon) * 100.0 / COUNT(*), 2) >= 60 THEN 85
                                    WHEN ROUND(AVG(CAST(Damage AS FLOAT)), 2) >= 80 THEN 75
                                    ELSE 60
                                END
                            WHEN TimeQuintile >= 3 THEN 70
                            ELSE 50
                        END as TrendScore
                        
                    FROM PerformanceData
                    GROUP BY PlayerName, Team, MapName, TimeQuintile
                    HAVING COUNT(*) >= 5
                    ORDER BY TrendScore DESC, TimeQuintile DESC, AvgKDRatio DESC";

                var data = await ExecuteAnalyticsQuery(sql, query);
                
                if (query.Format?.ToLower() == "csv")
                {
                    var csv = ConvertToCsv(data);
                    var fileName = $"performance_trends_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                    Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");
                    return File(Encoding.UTF8.GetBytes(csv), "text/csv");
                }

                return Ok(new
                {
                    Title = "Performance Trends Analytics",
                    Description = "Analysis of player improvement over time, consistency metrics, and learning curves",
                    Data = data,
                    TotalRecords = data.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating performance trends analysis");
                return StatusCode(500, $"Error generating performance trends analysis: {ex.Message}");
            }
        }

        [HttpGet("team-dynamics")]
        public async Task<IActionResult> GetTeamDynamics([FromQuery] AnalyticsQuery query)
        {
            try
            {
                var sql = @"
                    WITH TeamDynamicsData AS (
                        SELECT 
                            p1.PlayerName as PlayerName,
                            p1.Team,
                            d.MapName,
                            r.RoundNumber,
                            
                            -- PLAYER PERFORMANCE
                            prs1.Kills as PlayerKills,
                            prs1.Deaths as PlayerDeaths,
                            prs1.Damage as PlayerDamage,
                            prs1.Assists as PlayerAssists,
                            
                            -- TEAMMATE PERFORMANCE
                            p2.PlayerName as TeammateName,
                            prs2.Kills as TeammateKills,
                            prs2.Deaths as TeammateDeaths,
                            prs2.Damage as TeammateDamage,
                            prs2.Assists as TeammateAssists,
                            
                            -- ROUND OUTCOME
                            CASE WHEN p1.Team = r.WinnerTeam THEN 1 ELSE 0 END as RoundWon,
                            
                            -- KILL TIMING ANALYSIS
                            k1.GameTime as PlayerKillTime,
                            k2.GameTime as TeammateKillTime,
                            ABS(k1.GameTime - k2.GameTime) as KillTimeDifference,
                            
                            -- SUPPORT DETECTION
                            CASE 
                                WHEN ABS(k1.GameTime - k2.GameTime) <= 3.0 AND k1.GameTime IS NOT NULL AND k2.GameTime IS NOT NULL THEN 1
                                ELSE 0
                            END as TradeKill,
                            
                            CASE 
                                WHEN prs1.Assists > 0 AND prs2.Kills > prs2.Deaths THEN 1
                                ELSE 0
                            END as SupportPlay
                            
                        FROM PlayerRoundStats prs1
                        INNER JOIN Players p1 ON prs1.PlayerId = p1.Id
                        INNER JOIN Rounds r ON prs1.RoundId = r.Id
                        INNER JOIN DemoFiles d ON r.DemoFileId = d.Id
                        
                        -- Join with teammates in same round
                        INNER JOIN PlayerRoundStats prs2 ON r.Id = prs2.RoundId AND prs2.PlayerId != prs1.PlayerId
                        INNER JOIN Players p2 ON prs2.PlayerId = p2.Id AND p2.Team = p1.Team
                        
                        -- Optional kill timing data
                        LEFT JOIN Kills k1 ON p1.Id = k1.KillerId AND k1.RoundId = r.Id
                        LEFT JOIN Kills k2 ON p2.Id = k2.KillerId AND k2.RoundId = r.Id";

                // Apply filters
                if (!string.IsNullOrEmpty(query.MapName))
                    sql += " WHERE d.MapName = @MapName";
                if (!string.IsNullOrEmpty(query.PlayerName))
                    sql += (sql.Contains("WHERE") ? " AND" : " WHERE") + " p1.PlayerName = @PlayerName";
                if (!string.IsNullOrEmpty(query.Team))
                    sql += (sql.Contains("WHERE") ? " AND" : " WHERE") + " p1.Team = @Team";
                if (query.RoundNumber.HasValue)
                    sql += (sql.Contains("WHERE") ? " AND" : " WHERE") + " r.RoundNumber = @RoundNumber";

                sql += @"
                    )
                    SELECT 
                        PlayerName,
                        Team,
                        MapName,
                        TeammateName,
                        
                        -- COLLABORATION METRICS
                        COUNT(*) as RoundsPlayedTogether,
                        SUM(RoundWon) as RoundsWonTogether,
                        ROUND(SUM(RoundWon) * 100.0 / COUNT(*), 2) as TeamWinRate,
                        
                        -- INDIVIDUAL PERFORMANCE WITH TEAMMATE
                        ROUND(AVG(CAST(PlayerKills AS FLOAT)), 2) as AvgKillsWithTeammate,
                        ROUND(AVG(CAST(PlayerDeaths AS FLOAT)), 2) as AvgDeathsWithTeammate,
                        ROUND(AVG(CAST(PlayerDamage AS FLOAT)), 2) as AvgDamageWithTeammate,
                        ROUND(AVG(CAST(PlayerAssists AS FLOAT)), 2) as AvgAssistsWithTeammate,
                        
                        -- TEAMMATE SYNERGY
                        ROUND(AVG(CAST(TeammateKills AS FLOAT)), 2) as TeammateAvgKills,
                        ROUND(AVG(CAST(TeammateDamage AS FLOAT)), 2) as TeammateAvgDamage,
                        
                        -- COORDINATION METRICS
                        SUM(TradeKill) as TradeKillsExecuted,
                        ROUND(SUM(TradeKill) * 100.0 / NULLIF(COUNT(CASE WHEN PlayerKillTime IS NOT NULL THEN 1 END), 0), 2) as TradeKillRate,
                        SUM(SupportPlay) as SupportPlays,
                        ROUND(AVG(KillTimeDifference), 2) as AvgKillCoordination,
                        
                        -- TEAM CHEMISTRY SCORE
                        CASE 
                            WHEN ROUND(SUM(RoundWon) * 100.0 / COUNT(*), 2) >= 70 AND SUM(TradeKill) >= 5 THEN 100
                            WHEN ROUND(SUM(RoundWon) * 100.0 / COUNT(*), 2) >= 60 AND SUM(SupportPlay) >= 10 THEN 90
                            WHEN SUM(TradeKill) >= 3 AND ROUND(AVG(KillTimeDifference), 2) <= 5.0 THEN 85
                            WHEN ROUND(SUM(RoundWon) * 100.0 / COUNT(*), 2) >= 50 THEN 75
                            WHEN COUNT(*) >= 20 THEN 65
                            ELSE 50
                        END as TeamChemistryScore
                        
                    FROM TeamDynamicsData
                    GROUP BY PlayerName, Team, MapName, TeammateName
                    HAVING COUNT(*) >= 10
                    ORDER BY TeamChemistryScore DESC, TeamWinRate DESC, TradeKillsExecuted DESC";

                var data = await ExecuteAnalyticsQuery(sql, query);
                
                if (query.Format?.ToLower() == "csv")
                {
                    var csv = ConvertToCsv(data);
                    var fileName = $"team_dynamics_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                    Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");
                    return File(Encoding.UTF8.GetBytes(csv), "text/csv");
                }

                return Ok(new
                {
                    Title = "Advanced Team Dynamics",
                    Description = "Analysis of player partnerships, trade kills, support plays, and team chemistry",
                    Data = data,
                    TotalRecords = data.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating team dynamics analysis");
                return StatusCode(500, $"Error generating team dynamics analysis: {ex.Message}");
            }
        }
    }
}
