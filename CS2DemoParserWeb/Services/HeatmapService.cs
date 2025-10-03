using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;
using System.Data;

namespace CS2DemoParserWeb.Services
{
    public class HeatmapService : IHeatmapService
    {
        private readonly string _connectionString;
        private readonly IMemoryCache _cache;
        private readonly ILogger<HeatmapService> _logger;
        
        private static readonly Dictionary<string, MapConfig> MapConfigs = new()
        {
            {
                "de_ancient", new MapConfig
                {
                    Name = "ancient",
                    PosX = -2953,
                    PosY = 2164,
                    Scale = 5
                }
            },
            {
                "de_anubis", new MapConfig
                {
                    Name = "anubis",
                    PosX = -2796,
                    PosY = 3328,
                    Scale = 5.22f
                }
            },
            {
               "de_dust2", new MapConfig
               {
                   Name = "dust2",
                   PosX = -2476,
                   PosY = 3239,
                   Scale = 4.4f
               }
            },
            {
                "de_inferno", new MapConfig
                {
                    Name = "inferno",
                    PosX = -2087,
                    PosY = 3870,
                    Scale = 4.9f
                }
            },
            {
                "de_mirage", new MapConfig
                {
                    Name = "mirage",
                    PosX = -3230,
                    PosY = 1713,
                    Scale = 5.00f,
                }
            },
            {
                "de_nuke", new MapConfig
                {
                    Name = "nuke",
                    PosX = -3453,
                    PosY = 2887,
                    Scale = 7f,
                    HasLowerLevel = true,
                    DefaultAltitudeMax = 10000,
                    DefaultAltitudeMin = -495,
                    LowerAltitudeMax = -495,
                    LowerAltitudeMin = -10000
                }
            },
            {
                "de_overpass", new MapConfig
                {
                    Name = "overpass",
                    PosX = -4831,
                    PosY = 1781,
                    Scale = 5.20f,
                }
            },
            {
                "de_train", new MapConfig
                {
                    Name = "train",
                    PosX = -2308,
                    PosY = 2078,
                    Scale = 4.082077f,
                    HasLowerLevel = true,
                    DefaultAltitudeMax = 20000,
                    DefaultAltitudeMin = -50,
                    LowerAltitudeMax = -50,
                    LowerAltitudeMin = -5000
                }
            },
            {
                "de_vertigo", new MapConfig
                {
                    Name = "vertigo",
                    PosX = -3168,
                    PosY = 1762,
                    Scale = 4.0f,
                    HasLowerLevel = true,
                    DefaultAltitudeMax = 20000,
                    DefaultAltitudeMin = 11700,
                    LowerAltitudeMax = 11700,
                    LowerAltitudeMin = -10000
                }
            },
            {
                "de_cache", new MapConfig
                {
                    Name = "cache",
                    PosX = -2000,
                    PosY = 3250,
                    Scale = 5.5f
                }
            }
        };
        
        public HeatmapService(IConfiguration configuration, IMemoryCache cache, ILogger<HeatmapService> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new ArgumentNullException("DefaultConnection");
            _cache = cache;
            _logger = logger;
        }

        public async Task<HeatmapData> GetPlayerPositionHeatmapAsync(HeatmapQuery query)
        {
            var cacheKey = $"heatmap_positions_{query.MapName}_{query.PlayerName}_{query.Team}_{query.DemoName}_{query.DemoId}_{query.RoundNumber}";
            
            if (_cache.TryGetValue(cacheKey, out HeatmapData? cachedData) && cachedData != null)
            {
                _logger.LogInformation("Returning cached heatmap data for key: {CacheKey}", cacheKey);
                return cachedData;
            }

            var sql = @"
                SELECT 
                    pp.PositionX as X, pp.PositionY as Y, pp.PositionZ as Z, 
                    p.PlayerName, p.Team,
                    COUNT(*) as EventCount
                FROM PlayerPositions pp
                INNER JOIN Players p ON pp.PlayerId = p.Id
                INNER JOIN DemoFiles d ON p.DemoFileId = d.Id
                LEFT JOIN Matches m ON d.Id = m.DemoFileId
                LEFT JOIN Rounds r ON m.Id = r.MatchId
                    AND pp.Tick >= r.StartTick
                    AND (r.EndTick IS NULL OR pp.Tick <= r.EndTick)
                WHERE 1=1
                    AND (@DemoName IS NULL OR d.FileName = @DemoName)
                    AND (@DemoId IS NULL OR d.Id = @DemoId)
                    AND (@MapName IS NULL OR d.MapName = @MapName)
                    AND (@PlayerName IS NULL OR p.PlayerName = @PlayerName)
                    AND (@Team IS NULL OR p.Team = @Team)
                    AND (@RoundNumber IS NULL OR r.RoundNumber = @RoundNumber)
                GROUP BY pp.PositionX, pp.PositionY, pp.PositionZ, p.PlayerName, p.Team
                ORDER BY EventCount DESC";

            var points = await ExecutePlayerPositionQuery(sql, query);
            
            var result = new HeatmapData
            {
                MapName = query.MapName ?? "Unknown",
                HeatmapType = "player_positions",
                Points = points,
                Bounds = CalculateBounds(points),
                Metadata = CalculateMetadata(points, "Player position data")
            };

            // Cache for 10 minutes
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
                SlidingExpiration = TimeSpan.FromMinutes(5),
                Priority = CacheItemPriority.Normal
            };
            
            _cache.Set(cacheKey, result, cacheOptions);
            _logger.LogInformation("Cached heatmap data for key: {CacheKey}", cacheKey);
            
            return result;
        }

        public async Task<HeatmapData> GetDeathHeatmapAsync(HeatmapQuery query)
        {
            var cacheKey = $"heatmap_deaths_{query.MapName}_{query.PlayerName}_{query.Team}_{query.DemoName}_{query.DemoId}_{query.RoundNumber}";
            
            _logger.LogInformation("Death heatmap request - MapName: {MapName}, PlayerName: {PlayerName}, Team: {Team}, DemoId: {DemoId}, RoundNumber: {RoundNumber}, DemoSource: {DemoSource}", 
                query.MapName, query.PlayerName, query.Team, query.DemoId, query.RoundNumber, query.DemoSource);
            
            if (_cache.TryGetValue(cacheKey, out HeatmapData? cachedData) && cachedData != null)
            {
                _logger.LogInformation("Returning cached death heatmap data for key: {CacheKey}", cacheKey);
                return cachedData;
            }

            var sql = @"
                SELECT 
                    k.VictimPositionX as X, k.VictimPositionY as Y, k.VictimPositionZ as Z,
                    victim.PlayerName, victim.Team, r.RoundNumber,
                    COUNT(*) as EventCount
                FROM Kills k
                INNER JOIN Players victim ON k.VictimId = victim.Id
                INNER JOIN Rounds r ON k.RoundId = r.Id
                INNER JOIN Matches m ON r.MatchId = m.Id  
                INNER JOIN DemoFiles d ON m.DemoFileId = d.Id
                WHERE k.VictimPositionX IS NOT NULL AND k.VictimPositionY IS NOT NULL AND k.VictimPositionZ IS NOT NULL
                    AND (@DemoName IS NULL OR d.FileName = @DemoName)
                    AND (@DemoId IS NULL OR d.Id = @DemoId)
                    AND (@MapName IS NULL OR d.MapName = @MapName)
                    AND (@PlayerName IS NULL OR victim.PlayerName = @PlayerName)
                    AND (@Team IS NULL OR victim.Team = @Team)
                    AND (@RoundNumber IS NULL OR r.RoundNumber = @RoundNumber)
                GROUP BY k.VictimPositionX, k.VictimPositionY, k.VictimPositionZ, victim.PlayerName, victim.Team, r.RoundNumber
                ORDER BY EventCount DESC";

            var points = await ExecuteHeatmapQuery(sql, query, "deaths");
            
            _logger.LogInformation("Death heatmap query returned {Count} points", points.Count);
            
            var result = new HeatmapData
            {
                MapName = query.MapName ?? "Unknown",
                HeatmapType = "deaths",
                Points = points,
                Bounds = CalculateBounds(points),
                Metadata = CalculateMetadata(points, "Death location data")
            };

            // Cache for 10 minutes
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
                SlidingExpiration = TimeSpan.FromMinutes(5),
                Priority = CacheItemPriority.Normal
            };
            
            _cache.Set(cacheKey, result, cacheOptions);
            _logger.LogInformation("Cached death heatmap data for key: {CacheKey}", cacheKey);
            
            return result;
        }

        public async Task<HeatmapData> GetUtilityHeatmapAsync(HeatmapQuery query)
        {
            // Query for NON-MOLOTOV grenades only (smokes, flashes, HE, decoys)
            var grenadesSQL = @"
                SELECT 
                    g.DetonatePositionX as X, g.DetonatePositionY as Y, g.DetonatePositionZ as Z,
                    p.PlayerName, p.Team, r.RoundNumber, g.GrenadeType,
                    COUNT(*) as EventCount
                FROM Grenades g
                INNER JOIN Players p ON g.PlayerId = p.Id
                INNER JOIN Rounds r ON g.RoundId = r.Id
                INNER JOIN Matches m ON r.MatchId = m.Id
                INNER JOIN DemoFiles d ON m.DemoFileId = d.Id
                WHERE 1=1
                    AND g.GrenadeType NOT IN ('molotov', 'incgrenade', 'incendiary')
                    AND (@DemoName IS NULL OR d.FileName = @DemoName)
                    AND (@DemoId IS NULL OR d.Id = @DemoId)
                    AND (@MapName IS NULL OR d.MapName = @MapName)
                    AND (@PlayerName IS NULL OR p.PlayerName = @PlayerName)
                    AND (@UtilityType IS NULL OR g.GrenadeType = @UtilityType)
                    AND (@RoundNumber IS NULL OR r.RoundNumber = @RoundNumber)
                GROUP BY g.DetonatePositionX, g.DetonatePositionY, g.DetonatePositionZ, p.PlayerName, p.Team, r.RoundNumber, g.GrenadeType";

            // Query for molotov/incendiary from InfernoEvents (primary molotov data source)
            var infernoSQL = @"
                SELECT 
                    ie.OriginX as X, ie.OriginY as Y, ie.OriginZ as Z,
                    p.PlayerName, p.Team, r.RoundNumber, 
                    CASE 
                        WHEN ie.GrenadeType = 'incgrenade' THEN 'incgrenade'
                        WHEN ie.GrenadeType = 'incendiary' THEN 'incgrenade'
                        ELSE 'molotov' 
                    END as GrenadeType,
                    COUNT(*) as EventCount
                FROM InfernoEvents ie
                LEFT JOIN Players p ON ie.ThrowerPlayerId = p.Id
                INNER JOIN Rounds r ON ie.RoundId = r.Id
                INNER JOIN Matches m ON r.MatchId = m.Id
                INNER JOIN DemoFiles d ON m.DemoFileId = d.Id
                WHERE ie.EventType = 'start'
                    AND (@DemoName IS NULL OR d.FileName = @DemoName)
                    AND (@DemoId IS NULL OR d.Id = @DemoId)
                    AND (@MapName IS NULL OR d.MapName = @MapName)
                    AND (@PlayerName IS NULL OR p.PlayerName IS NULL OR p.PlayerName = @PlayerName)
                    AND (@UtilityType IS NULL OR 
                         (@UtilityType = 'molotov' AND ie.GrenadeType IN ('molotov', 'incgrenade', 'incendiary')) OR
                         (@UtilityType = 'incgrenade' AND ie.GrenadeType IN ('incgrenade', 'incendiary')) OR
                         (@UtilityType NOT IN ('molotov', 'incgrenade')))
                    AND (@RoundNumber IS NULL OR r.RoundNumber = @RoundNumber)
                GROUP BY ie.OriginX, ie.OriginY, ie.OriginZ, p.PlayerName, p.Team, r.RoundNumber, ie.GrenadeType";

            // Secondary query for molotov fire areas (backup data source)
            var fireAreasSQL = @"
                SELECT 
                    fa.CenterX as X, fa.CenterY as Y, fa.CenterZ as Z,
                    p.PlayerName, p.Team, r.RoundNumber,
                    CASE 
                        WHEN fa.GrenadeType = 'incgrenade' THEN 'incgrenade'
                        WHEN fa.GrenadeType = 'incendiary' THEN 'incgrenade'
                        ELSE 'molotov' 
                    END as GrenadeType,
                    COUNT(*) as EventCount
                FROM FireAreas fa
                INNER JOIN Players p ON fa.ThrowerPlayerId = p.Id
                INNER JOIN Rounds r ON fa.RoundId = r.Id
                INNER JOIN Matches m ON r.MatchId = m.Id
                INNER JOIN DemoFiles d ON m.DemoFileId = d.Id
                WHERE 1=1
                    AND (@DemoName IS NULL OR d.FileName = @DemoName)
                    AND (@DemoId IS NULL OR d.Id = @DemoId)
                    AND (@MapName IS NULL OR d.MapName = @MapName)
                    AND (@PlayerName IS NULL OR p.PlayerName = @PlayerName)
                    AND (@UtilityType IS NULL OR 
                         (@UtilityType = 'molotov' AND fa.GrenadeType IN ('molotov', 'incgrenade', 'incendiary')) OR
                         (@UtilityType = 'incgrenade' AND fa.GrenadeType IN ('incgrenade', 'incendiary')) OR
                         (@UtilityType NOT IN ('molotov', 'incgrenade')))
                    AND (@RoundNumber IS NULL OR r.RoundNumber = @RoundNumber)
                GROUP BY fa.CenterX, fa.CenterY, fa.CenterZ, p.PlayerName, p.Team, r.RoundNumber, fa.GrenadeType";

            var points = await ExecuteUtilityQueryCombined(grenadesSQL, fireAreasSQL, infernoSQL, query);
            
            return new HeatmapData
            {
                MapName = query.MapName ?? "Unknown",
                HeatmapType = "utility",
                Points = points,
                Bounds = CalculateBounds(points),
                Metadata = CalculateMetadata(points, $"Utility usage data ({query.UtilityType ?? "all types"})")
            };
        }

        public async Task<HeatmapData> GetBombSiteHeatmapAsync(HeatmapQuery query)
        {
            var sql = @"
                SELECT
                    b.PositionX as X, b.PositionY as Y, b.PositionZ as Z,
                    p.PlayerName, b.Team, r.RoundNumber, b.EventType,
                    COUNT(*) as EventCount
                FROM Bombs b
                INNER JOIN Players p ON b.PlayerId = p.Id
                INNER JOIN Rounds r ON b.RoundId = r.Id
                INNER JOIN Matches m ON r.MatchId = m.Id
                INNER JOIN DemoFiles d ON m.DemoFileId = d.Id
                WHERE b.PositionX IS NOT NULL AND b.PositionY IS NOT NULL AND b.PositionZ IS NOT NULL
                    AND (@DemoName IS NULL OR d.FileName = @DemoName)
                    AND (@DemoId IS NULL OR d.Id = @DemoId)
                    AND (@MapName IS NULL OR d.MapName = @MapName)
                    AND (@PlayerName IS NULL OR p.PlayerName = @PlayerName)
                    AND (@Team IS NULL OR b.Team = @Team OR
                        (@Team = '2' AND b.Team = 'Terrorist') OR
                        (@Team = '3' AND b.Team = 'CounterTerrorist'))
                    AND (@RoundNumber IS NULL OR r.RoundNumber = @RoundNumber)
                GROUP BY b.PositionX, b.PositionY, b.PositionZ, p.PlayerName, b.Team, r.RoundNumber, b.EventType
                ORDER BY EventCount DESC";

            _logger.LogInformation("Bomb heatmap query - Team filter: {Team}, DemoId: {DemoId}, MapName: {MapName}",
                query.Team, query.DemoId, query.MapName);

            var points = await ExecuteHeatmapQuery(sql, query, "bomb_events");

            _logger.LogInformation("Bomb heatmap returned {Count} points", points.Count);

            return new HeatmapData
            {
                MapName = query.MapName ?? "Unknown",
                HeatmapType = "bomb_events",
                Points = points,
                Bounds = CalculateBounds(points),
                Metadata = CalculateMetadata(points, "Bomb event data (plants/defuses)")
            };
        }

        public async Task<HeatmapData> GetWeaponFireHeatmapAsync(HeatmapQuery query)
        {
            var sql = @"
                SELECT 
                    w.PositionX as X, w.PositionY as Y, w.PositionZ as Z,
                    p.PlayerName, p.Team, r.RoundNumber,
                    COUNT(*) as EventCount
                FROM WeaponFires w
                INNER JOIN Players p ON w.PlayerId = p.Id
                INNER JOIN Rounds r ON w.RoundId = r.Id
                INNER JOIN Matches m ON r.MatchId = m.Id
                INNER JOIN DemoFiles d ON m.DemoFileId = d.Id
                WHERE w.PositionX IS NOT NULL AND w.PositionY IS NOT NULL AND w.PositionZ IS NOT NULL
                    AND (@DemoName IS NULL OR d.FileName = @DemoName)
                    AND (@DemoId IS NULL OR d.Id = @DemoId)
                    AND (@MapName IS NULL OR d.MapName = @MapName)
                    AND (@PlayerName IS NULL OR p.PlayerName = @PlayerName)
                    AND (@Team IS NULL OR p.Team = @Team)
                    AND (@RoundNumber IS NULL OR r.RoundNumber = @RoundNumber)
                GROUP BY w.PositionX, w.PositionY, w.PositionZ, p.PlayerName, p.Team, r.RoundNumber
                ORDER BY EventCount DESC";

            var points = await ExecuteHeatmapQuery(sql, query, "weapon_fire");
            
            return new HeatmapData
            {
                MapName = query.MapName ?? "Unknown",
                HeatmapType = "weapon_fire",
                Points = points,
                Bounds = CalculateBounds(points),
                Metadata = CalculateMetadata(points, "Weapon fire position data")
            };
        }

        public async Task<List<string>> GetAvailableMapsAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            // First try to get maps from MapName column
            var sql = "SELECT DISTINCT MapName FROM DemoFiles WHERE MapName IS NOT NULL ORDER BY MapName";
            using var command = new SqlCommand(sql, connection);
            
            var maps = new List<string>();
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var mapName = reader.GetString("MapName");
                maps.Add(mapName);
            }
            
            // If no maps found in MapName column, try to extract from filenames
            if (!maps.Any())
            {
                reader.Close();
                var filenameSql = "SELECT DISTINCT FileName FROM DemoFiles WHERE FileName IS NOT NULL";
                using var filenameCommand = new SqlCommand(filenameSql, connection);
                using var filenameReader = await filenameCommand.ExecuteReaderAsync();
                
                var extractedMaps = new HashSet<string>();
                while (await filenameReader.ReadAsync())
                {
                    var fileName = filenameReader.GetString("FileName");
                    var mapName = ExtractMapFromFilename(fileName);
                    if (!string.IsNullOrEmpty(mapName))
                    {
                        extractedMaps.Add(mapName);
                    }
                }
                
                maps.AddRange(extractedMaps.OrderBy(m => m));
            }
            
            return maps;
        }

        public async Task<List<string>> GetAvailablePlayersAsync(string? demoName = null)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var sql = @"
                SELECT DISTINCT p.PlayerName 
                FROM Players p
                INNER JOIN DemoFiles d ON p.DemoFileId = d.Id 
                WHERE (@DemoName IS NULL OR d.FileName = @DemoName)
                ORDER BY p.PlayerName";
                
            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@DemoName", (object?)demoName ?? DBNull.Value);
            
            var players = new List<string>();
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                players.Add(reader.GetString("PlayerName"));
            }
            
            return players;
        }

        public async Task<List<string>> GetAvailablePlayersByDemoIdAsync(int demoId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var sql = @"
                SELECT DISTINCT p.PlayerName 
                FROM Players p
                INNER JOIN DemoFiles d ON p.DemoFileId = d.Id 
                WHERE d.Id = @DemoId
                ORDER BY p.PlayerName";
                
            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@DemoId", demoId);
            
            var players = new List<string>();
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                players.Add(reader.GetString("PlayerName"));
            }
            
            return players;
        }

        public async Task<List<int>> GetAvailableRoundsByDemoIdAsync(int demoId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var sql = @"
                SELECT DISTINCT r.RoundNumber 
                FROM Rounds r
                INNER JOIN Matches m ON r.MatchId = m.Id
                INNER JOIN DemoFiles d ON m.DemoFileId = d.Id 
                WHERE d.Id = @DemoId AND r.RoundNumber IS NOT NULL
                ORDER BY r.RoundNumber";
                
            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@DemoId", demoId);
            
            var rounds = new List<int>();
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                rounds.Add(reader.GetInt32("RoundNumber"));
            }
            
            return rounds;
        }

        public async Task<string?> GetDemoSourceAsync(int demoId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var sql = "SELECT DemoSource FROM DemoFiles WHERE Id = @DemoId";
            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@DemoId", demoId);
            
            var result = await command.ExecuteScalarAsync();
            return result?.ToString();
        }

        private async Task<List<HeatmapPoint>> ExecuteHeatmapQuery(string sql, HeatmapQuery query, string eventType)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            using var command = new SqlCommand(sql, connection);
            AddCommonParameters(command, query);
            
            var points = new List<HeatmapPoint>();
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                // Try to get specific event type from database, fallback to parameter
                string specificEventType = eventType;
                if (reader.GetColumnSchema().Any(c => c.ColumnName == "EventType") && !reader.IsDBNull("EventType"))
                {
                    specificEventType = reader.GetString("EventType");
                }
                
                points.Add(new HeatmapPoint
                {
                    X = (float)reader.GetDecimal("X"),
                    Y = (float)reader.GetDecimal("Y"),
                    Z = (float)reader.GetDecimal("Z"),
                    Count = reader.GetInt32("EventCount"),
                    PlayerName = reader.IsDBNull("PlayerName") ? null : reader.GetString("PlayerName"),
                    Team = reader.IsDBNull("Team") ? null : reader.GetString("Team"),
                    EventType = specificEventType
                });
            }
            
            return NormalizeIntensity(points, query.NormalizeIntensity);
        }

        private async Task<List<HeatmapPoint>> ExecuteUtilityQuery(string sql, HeatmapQuery query)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            using var command = new SqlCommand(sql, connection);
            AddCommonParameters(command, query);
            command.Parameters.AddWithValue("@UtilityType", (object?)query.UtilityType ?? DBNull.Value);
            
            var points = new List<HeatmapPoint>();
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                points.Add(new HeatmapPoint
                {
                    X = (float)reader.GetDecimal("X"),
                    Y = (float)reader.GetDecimal("Y"),
                    Z = (float)reader.GetDecimal("Z"),
                    Count = reader.GetInt32("EventCount"),
                    PlayerName = reader.IsDBNull("PlayerName") ? null : reader.GetString("PlayerName"),
                    EventType = reader.IsDBNull("GrenadeType") ? "utility" : reader.GetString("GrenadeType")
                });
            }
            
            return NormalizeIntensity(points, query.NormalizeIntensity);
        }

        private async Task<List<HeatmapPoint>> ExecuteUtilityQueryCombined(string grenadesSQL, string fireAreasSQL, string infernoSQL, HeatmapQuery query)
        {
            var allPoints = new List<HeatmapPoint>();
            
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            // Execute non-molotov grenades query (smokes, flashes, HE, decoys)
            using (var command = new SqlCommand(grenadesSQL, connection))
            {
                AddCommonParameters(command, query);
                command.Parameters.AddWithValue("@UtilityType", (object?)query.UtilityType ?? DBNull.Value);
                
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    allPoints.Add(new HeatmapPoint
                    {
                        X = (float)reader.GetDecimal("X"),
                        Y = (float)reader.GetDecimal("Y"),
                        Z = (float)reader.GetDecimal("Z"),
                        Count = reader.GetInt32("EventCount"),
                        PlayerName = reader.IsDBNull("PlayerName") ? null : reader.GetString("PlayerName"),
                        Team = reader.IsDBNull("Team") ? null : reader.GetString("Team"),
                        EventType = reader.IsDBNull("GrenadeType") ? "utility" : reader.GetString("GrenadeType")
                    });
                }
            }
            
            // Execute molotov/incendiary from InfernoEvents query (PRIMARY molotov source)
            using (var command = new SqlCommand(infernoSQL, connection))
            {
                AddCommonParameters(command, query);
                command.Parameters.AddWithValue("@UtilityType", (object?)query.UtilityType ?? DBNull.Value);
                
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    allPoints.Add(new HeatmapPoint
                    {
                        X = (float)reader.GetDecimal("X"),
                        Y = (float)reader.GetDecimal("Y"),
                        Z = (float)reader.GetDecimal("Z"),
                        Count = reader.GetInt32("EventCount"),
                        PlayerName = reader.IsDBNull("PlayerName") ? null : reader.GetString("PlayerName"),
                        Team = reader.IsDBNull("Team") ? null : reader.GetString("Team"),
                        EventType = reader.IsDBNull("GrenadeType") ? "molotov" : reader.GetString("GrenadeType")
                    });
                }
            }
            
            // Only execute FireAreas query if no molotov data found in InfernoEvents (BACKUP source)
            if (!allPoints.Any(p => p.EventType?.ToLower() == "molotov" || p.EventType?.ToLower() == "incgrenade"))
            {
                using var command = new SqlCommand(fireAreasSQL, connection);
                AddCommonParameters(command, query);
                command.Parameters.AddWithValue("@UtilityType", (object?)query.UtilityType ?? DBNull.Value);
                
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    allPoints.Add(new HeatmapPoint
                    {
                        X = (float)reader.GetDecimal("X"),
                        Y = (float)reader.GetDecimal("Y"),
                        Z = (float)reader.GetDecimal("Z"),
                        Count = reader.GetInt32("EventCount"),
                        PlayerName = reader.IsDBNull("PlayerName") ? null : reader.GetString("PlayerName"),
                        Team = reader.IsDBNull("Team") ? null : reader.GetString("Team"),
                        EventType = reader.IsDBNull("GrenadeType") ? "molotov" : reader.GetString("GrenadeType")
                    });
                }
            }
            
            return NormalizeIntensity(allPoints, query.NormalizeIntensity);
        }

        private async Task<List<HeatmapPoint>> ExecutePlayerPositionQuery(string sql, HeatmapQuery query)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            using var command = new SqlCommand(sql, connection);
            
            // Use the adjusted round number for Faceit/ESEA demos
            var adjustedRoundNumber = GetAdjustedRoundNumber(query.RoundNumber, query.DemoSource);
            
            command.Parameters.AddWithValue("@DemoName", (object?)query.DemoName ?? DBNull.Value);
            command.Parameters.AddWithValue("@DemoId", (object?)query.DemoId ?? DBNull.Value);
            command.Parameters.AddWithValue("@MapName", (object?)query.MapName ?? DBNull.Value);
            command.Parameters.AddWithValue("@PlayerName", (object?)query.PlayerName ?? DBNull.Value);
            command.Parameters.AddWithValue("@Team", (object?)query.Team ?? DBNull.Value);
            command.Parameters.AddWithValue("@RoundNumber", (object?)adjustedRoundNumber ?? DBNull.Value);
            
            var points = new List<HeatmapPoint>();
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                points.Add(new HeatmapPoint
                {
                    X = (float)reader.GetDecimal("X"),
                    Y = (float)reader.GetDecimal("Y"),
                    Z = (float)reader.GetDecimal("Z"),
                    Count = reader.GetInt32("EventCount"),
                    PlayerName = reader.IsDBNull("PlayerName") ? null : reader.GetString("PlayerName"),
                    Team = reader.IsDBNull("Team") ? null : reader.GetString("Team"),
                    EventType = "player_positions"
                });
            }
            
            return NormalizeIntensity(points, query.NormalizeIntensity);
        }

        private void AddCommonParameters(SqlCommand command, HeatmapQuery query)
        {
            // Adjust round number based on demo source for Faceit/ESEA demos
            var adjustedRoundNumber = GetAdjustedRoundNumber(query.RoundNumber, query.DemoSource);

            _logger.LogInformation("Adding SQL parameters - DemoId: {DemoId}, MapName: {MapName}, PlayerName: {PlayerName}, Team: {Team}, RoundNumber: {RoundNumber} (Original: {OriginalRound}, Adjusted: {AdjustedRound})",
                query.DemoId, query.MapName, query.PlayerName, query.Team, adjustedRoundNumber, query.RoundNumber, adjustedRoundNumber);

            command.Parameters.AddWithValue("@DemoName", (object?)query.DemoName ?? DBNull.Value);
            command.Parameters.AddWithValue("@DemoId", (object?)query.DemoId ?? DBNull.Value);
            command.Parameters.AddWithValue("@MapName", (object?)query.MapName ?? DBNull.Value);
            command.Parameters.AddWithValue("@PlayerName", string.IsNullOrEmpty(query.PlayerName) ? DBNull.Value : query.PlayerName);
            command.Parameters.AddWithValue("@Team", string.IsNullOrEmpty(query.Team) ? DBNull.Value : query.Team);
            command.Parameters.AddWithValue("@RoundNumber", (object?)adjustedRoundNumber ?? DBNull.Value);
        }

        private int? GetAdjustedRoundNumber(int? userRoundNumber, string? demoSource)
        {
            if (userRoundNumber == null) return null;
            
            // For Faceit and ESEA demos, add 2 to the user's round number to skip warmup/knife rounds
            if (demoSource == "faceit" || demoSource == "esea")
            {
                return userRoundNumber + 2;
            }
            
            // For other demo sources (matchmaking, etc.), use the round number as-is
            return userRoundNumber;
        }

        private List<HeatmapPoint> NormalizeIntensity(List<HeatmapPoint> points, bool normalize)
        {
            if (!points.Any())
                return points;

            var maxCount = points.Max(p => p.Count);
            
            foreach (var point in points)
            {
                if (normalize)
                {
                    point.Intensity = maxCount > 0 ? (float)point.Count / maxCount : 0;
                }
                else
                {
                    point.Intensity = point.Count;
                }
            }
            
            return points;
        }

        private HeatmapBounds CalculateBounds(List<HeatmapPoint> points)
        {
            if (!points.Any())
            {
                return new HeatmapBounds();
            }

            return new HeatmapBounds
            {
                MinX = points.Min(p => p.X),
                MaxX = points.Max(p => p.X),
                MinY = points.Min(p => p.Y),
                MaxY = points.Max(p => p.Y),
                MinZ = points.Min(p => p.Z),
                MaxZ = points.Max(p => p.Z)
            };
        }

        private HeatmapMetadata CalculateMetadata(List<HeatmapPoint> points, string description)
        {
            return new HeatmapMetadata
            {
                TotalEvents = points.Sum(p => p.Count),
                UniquePositions = points.Count,
                MaxIntensity = points.Any() ? points.Max(p => p.Intensity) : 0,
                PlayersIncluded = points.Where(p => !string.IsNullOrEmpty(p.PlayerName))
                                       .Select(p => p.PlayerName!)
                                       .Distinct()
                                       .ToList(),
                FilterDescription = description
            };
        }

        private MapConfig? GetMapConfig(string mapName)
        {
            if (MapConfigs.TryGetValue(mapName.ToLower(), out var config))
            {
                return config;
            }
            return null;
        }

        public float ConvertToImageX(float gameX, int imageWidth, MapConfig mapConfig)
        {
            // Convert from game coordinates to radar coordinates
            float normalized = (gameX - mapConfig.PosX) / mapConfig.Scale;
            // Convert to image coordinates (1024x1024 base)
            return normalized * imageWidth / 1024f;
        }

        public float ConvertToImageY(float gameY, int imageHeight, MapConfig mapConfig)
        {
            // Convert from game coordinates to radar coordinates, flip Y axis
            float normalized = (mapConfig.PosY - gameY) / mapConfig.Scale;
            // Convert to image coordinates (1024x1024 base)
            return normalized * imageHeight / 1024f;
        }

        public bool IsLowerLevel(float z, MapConfig mapConfig)
        {
            if (!mapConfig.HasLowerLevel)
                return false;
                
            return z <= mapConfig.LowerAltitudeMax;
        }

        public string GetMapImagePath(string mapName, bool lowerLevel = false)
        {
            var mapConfig = GetMapConfig(mapName);
            if (mapConfig == null) return $"/maps/{mapName}.png";
            
            var suffix = lowerLevel && mapConfig.HasLowerLevel ? "_lower" : "";
            return $"/maps/{mapConfig.Name}{suffix}.png";
        }

        private string ExtractMapFromFilename(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return string.Empty;
            
            var lowerFileName = fileName.ToLower();
            
            // List of known CS2 maps
            var knownMaps = new[]
            {
                "de_ancient", "de_anubis", "de_dust2", "de_inferno", "de_mirage", 
                "de_nuke", "de_overpass", "de_train", "de_vertigo", "de_cache",
                "ancient", "anubis", "dust2", "inferno", "mirage", 
                "nuke", "overpass", "train", "vertigo", "cache"
            };
            
            foreach (var map in knownMaps)
            {
                if (lowerFileName.Contains(map))
                {
                    // Return the de_ version if it's just the map name
                    if (map.StartsWith("de_"))
                        return map;
                    else
                        return $"de_{map}";
                }
            }
            
            return string.Empty;
        }
    }

    public class MapConfig
    {
        public string Name { get; set; } = string.Empty;
        public float PosX { get; set; }
        public float PosY { get; set; }
        public float Scale { get; set; }
        public bool HasLowerLevel { get; set; }
        public float DefaultAltitudeMax { get; set; }
        public float DefaultAltitudeMin { get; set; }
        public float LowerAltitudeMax { get; set; }
        public float LowerAltitudeMin { get; set; }
    }
}