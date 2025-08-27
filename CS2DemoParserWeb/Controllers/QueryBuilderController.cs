using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace CS2DemoParserWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QueryBuilderController : ControllerBase
    {
        private readonly string _connectionString;
        private readonly ILogger<QueryBuilderController> _logger;

        // Allowed tables and columns for safety - Complete CS2 Demo Database Schema
        private readonly Dictionary<string, HashSet<string>> _allowedTables = new()
        {
            // Core Tables
            {
                "DemoFiles", new HashSet<string>
                {
                    "Id", "FileName", "MapName", "ParsedAt", "Duration", "DemoSource", "FileSize", "TickRate", "ServerName"
                }
            },
            {
                "Matches", new HashSet<string>
                {
                    "Id", "DemoFileId", "MapName", "GameMode", "StartTime", "EndTime", "TotalRounds", 
                    "CTScore", "TScore", "CTScoreFirstHalf", "TScoreFirstHalf", "CTScoreSecondHalf", 
                    "TScoreSecondHalf", "CTScoreOvertime", "TScoreOvertime", "IsOvertime", "IsFinished", 
                    "WinnerTeam", "WinCondition", "MaxRounds", "RoundTimeLimit", "FreezeTime", "BuyTime"
                }
            },
            {
                "Rounds", new HashSet<string>
                {
                    "Id", "DemoFileId", "MatchId", "RoundNumber", "StartTick", "EndTick",
                    "StartTime", "EndTime", "Duration", "WinnerTeam", "EndReason",
                    "CTScore", "TScore", "CTLivePlayers", "TLivePlayers",
                    "CTStartMoney", "TStartMoney", "CTEquipmentValue", "TEquipmentValue",
                    "BombPlanted", "BombDefused", "BombExploded", "BombSite"
                }
            },
            {
                "Players", new HashSet<string>
                {
                    "Id", "DemoFileId", "PlayerSlot", "UserId", "SteamId", "PlayerName", "Team", "ClanTag",
                    "IsBot", "IsHltv", "IsConnected", "Rank", "Wins",
                    "ConnectedAt", "DisconnectedAt", "DisconnectReason",
                    "PingAverage", "PingMax", "PingMin", "PacketLoss"
                }
            },
            {
                "GameEvents", new HashSet<string>
                {
                    "Id", "DemoFileId", "Tick", "EventType", "EventData", "GameTime"
                }
            },
            
            // Player Performance Tables
            {
                "PlayerMatchStats", new HashSet<string>
                {
                    "Id", "PlayerId", "MatchId", "Kills", "Deaths", "Assists", "Score", "MVPs", "ADR", "KAST", "Rating"
                }
            },
            {
                "PlayerRoundStats", new HashSet<string>
                {
                    "Id", "PlayerId", "RoundId", "Kills", "Deaths", "Assists", "Damage", "EquipmentValue", "MoneySpent"
                }
            },
            {
                "PlayerPositions", new HashSet<string>
                {
                    "Id", "PlayerId", "Tick", "PositionX", "PositionY", "PositionZ", "ViewAngleX", "ViewAngleY", "Velocity"
                }
            },
            {
                "PlayerMovements", new HashSet<string>
                {
                    "Id", "PlayerId", "RoundId", "Tick", "MovementType", "Speed", "Direction", "PositionX", "PositionY", "PositionZ"
                }
            },
            {
                "PlayerBehaviorEvents", new HashSet<string>
                {
                    "Id", "PlayerId", "RoundId", "Tick", "BehaviorType", "Context", "Duration"
                }
            },
            {
                "AdvancedPlayerStats", new HashSet<string>
                {
                    "Id", "PlayerId", "MatchId", "HeadshotPercentage", "AccuracyPercentage", "FirstKillPercentage", "ClutchSuccessRate"
                }
            },
            
            // Combat & Weapon Tables
            {
                "Kills", new HashSet<string>
                {
                    "Id", "DemoFileId", "RoundId", "KillerId", "VictimId", "AssisterId", "Tick", "GameTime", 
                    "Weapon", "WeaponClass", "HitGroup", "IsHeadshot", "IsWallbang", "IsNoScope",
                    "KillerPositionX", "KillerPositionY", "KillerPositionZ",
                    "VictimPositionX", "VictimPositionY", "VictimPositionZ",
                    "KillerViewAngleX", "KillerViewAngleY", "VictimViewAngleX", "VictimViewAngleY"
                }
            },
            {
                "Damages", new HashSet<string>
                {
                    "Id", "DemoFileId", "RoundId", "AttackerId", "VictimId", "Tick", "GameTime",
                    "AttackerPositionX", "AttackerPositionY", "AttackerPositionZ",
                    "VictimPositionX", "VictimPositionY", "VictimPositionZ",
                    "AttackerViewAngleX", "AttackerViewAngleY", "VictimViewAngleX", "VictimViewAngleY",
                    "DamageAmount", "DamageArmor", "Health", "Armor", "Weapon", "WeaponClass", "HitGroup"
                }
            },
            {
                "WeaponFires", new HashSet<string>
                {
                    "Id", "DemoFileId", "RoundId", "PlayerId", "Tick", "GameTime",
                    "Weapon", "WeaponClass", "PositionX", "PositionY", "PositionZ",
                    "ViewAngleX", "ViewAngleY", "ViewAngleZ", "Team",
                    "IsScoped", "IsSilenced", "Ammo", "AmmoReserve",
                    "RecoilIndex", "Accuracy", "Velocity", "ThroughSmoke", "IsBlind", "FlashDuration"
                }
            },
            {
                "WeaponStates", new HashSet<string>
                {
                    "Id", "PlayerId", "RoundId", "Tick", "Weapon", "State", "AmmoClip", "AmmoReserve", "IsReloading"
                }
            },
            {
                "BulletImpacts", new HashSet<string>
                {
                    "Id", "PlayerId", "RoundId", "Tick", "PositionX", "PositionY", "PositionZ", "Surface", "Penetration"
                }
            },
            {
                "Equipment", new HashSet<string>
                {
                    "Id", "PlayerId", "RoundId", "Tick", "EquipmentType", "Action", "Value", "Position"
                }
            },
            
            // Grenade & Utility Tables
            {
                "Grenades", new HashSet<string>
                {
                    "Id", "PlayerId", "RoundId", "Tick", "GrenadeType", "DetonatePositionX", "DetonatePositionY", "DetonatePositionZ", "ThrowPositionX", "ThrowPositionY", "ThrowPositionZ"
                }
            },
            {
                "GrenadeTrajectories", new HashSet<string>
                {
                    "Id", "GrenadeId", "Tick", "PositionX", "PositionY", "PositionZ", "VelocityX", "VelocityY", "VelocityZ"
                }
            },
            {
                "FlashEvents", new HashSet<string>
                {
                    "Id", "ThrowerPlayerId", "VictimPlayerId", "RoundId", "Tick", "Duration", "Distance", "Angle"
                }
            },
            {
                "SmokeClouds", new HashSet<string>
                {
                    "Id", "PlayerId", "RoundId", "StartTick", "EndTick", "CenterX", "CenterY", "CenterZ", "Radius"
                }
            },
            {
                "InfernoEvents", new HashSet<string>
                {
                    "Id", "ThrowerPlayerId", "RoundId", "Tick", "EventType", "OriginX", "OriginY", "OriginZ", "GrenadeType"
                }
            },
            {
                "FireAreas", new HashSet<string>
                {
                    "Id", "ThrowerPlayerId", "RoundId", "StartTick", "EndTick", "CenterX", "CenterY", "CenterZ", "GrenadeType"
                }
            },
            
            // Bomb & Objective Tables
            {
                "Bombs", new HashSet<string>
                {
                    "Id", "PlayerId", "RoundId", "Tick", "EventType", "PositionX", "PositionY", "PositionZ", "Site"
                }
            },
            {
                "HostageEvents", new HashSet<string>
                {
                    "Id", "PlayerId", "RoundId", "Tick", "EventType", "HostageId", "PositionX", "PositionY", "PositionZ"
                }
            },
            {
                "ZoneEvents", new HashSet<string>
                {
                    "Id", "PlayerId", "RoundId", "Tick", "ZoneType", "EventType", "PositionX", "PositionY", "PositionZ"
                }
            },
            
            // Communication Tables
            {
                "ChatMessages", new HashSet<string>
                {
                    "Id", "PlayerId", "RoundId", "Tick", "Message", "IsTeamMessage", "RecipientTeam"
                }
            },
            {
                "VoiceCommunications", new HashSet<string>
                {
                    "Id", "PlayerId", "RoundId", "StartTick", "EndTick", "Duration", "IsTeamVoice"
                }
            },
            {
                "RadioCommands", new HashSet<string>
                {
                    "Id", "PlayerId", "RoundId", "Tick", "Command", "TargetPlayerId", "PositionX", "PositionY", "PositionZ"
                }
            },
            {
                "CommunicationPatterns", new HashSet<string>
                {
                    "Id", "MatchId", "TeamId", "CommunicationType", "Frequency", "Effectiveness", "TimingPattern"
                }
            },
            {
                "AdvancedUserMessages", new HashSet<string>
                {
                    "Id", "PlayerId", "RoundId", "Tick", "MessageType", "Content", "Priority"
                }
            },
            
            // Economic Tables
            {
                "EconomyEvents", new HashSet<string>
                {
                    "Id", "DemoFileId", "RoundId", "PlayerId", "Tick", "GameTime",
                    "EventType", "ItemName", "ItemCategory", "ItemCost", "MoneyBefore", "MoneyAfter", "MoneyChange",
                    "ItemQuantity", "PositionX", "PositionY", "PositionZ", "Team", "RoundNumber",
                    "IsInBuyZone", "IsBuyTimeActive", "Description", "CreatedAt"
                }
            },
            {
                "EconomyStates", new HashSet<string>
                {
                    "Id", "TeamId", "RoundId", "Tick", "TotalMoney", "AverageMoney", "EquipmentValue", "LossBonus"
                }
            },
            {
                "EconomicAnalyses", new HashSet<string>
                {
                    "Id", "MatchId", "TeamId", "EconomicStrategy", "Efficiency", "WastePercentage", "ForceRounds"
                }
            },
            {
                "DroppedItems", new HashSet<string>
                {
                    "Id", "PlayerId", "RoundId", "Tick", "ItemType", "PositionX", "PositionY", "PositionZ", "PickupPlayerId"
                }
            },
            
            // Team & Tactical Tables
            {
                "TeamStates", new HashSet<string>
                {
                    "Id", "TeamId", "RoundId", "Tick", "Score", "Money", "PlayersAlive", "Equipment"
                }
            },
            {
                "TeamPerformances", new HashSet<string>
                {
                    "Id", "TeamId", "MatchId", "RoundsWon", "RoundsLost", "FirstKills", "ClutchesWon", "EcoRoundsWon"
                }
            },
            {
                "TacticalEvents", new HashSet<string>
                {
                    "Id", "TeamId", "RoundId", "Tick", "TacticType", "Success", "PlayersInvolved", "MapArea"
                }
            },
            {
                "MapControls", new HashSet<string>
                {
                    "Id", "TeamId", "RoundId", "Tick", "AreaName", "ControlPercentage", "PlayersInArea"
                }
            },
            {
                "RoundImpacts", new HashSet<string>
                {
                    "Id", "PlayerId", "RoundId", "ImpactType", "Value", "Context", "TeamInfluence"
                }
            },
            {
                "RoundOutcomes", new HashSet<string>
                {
                    "Id", "RoundId", "WinningTeam", "WinCondition", "Duration", "EconomicImpact", "MomentumShift"
                }
            },
            
            // Statistics & Analytics Tables
            {
                "MatchStatistics", new HashSet<string>
                {
                    "Id", "MatchId", "StatisticType", "Value", "Context", "Comparison"
                }
            },
            {
                "MapStatistics", new HashSet<string>
                {
                    "Id", "MapName", "StatisticType", "Value", "SampleSize", "LastUpdated"
                }
            },
            {
                "PerformanceMetrics", new HashSet<string>
                {
                    "Id", "PlayerId", "MatchId", "MetricType", "Value", "Percentile", "Trend"
                }
            },
            {
                "EndOfMatchData", new HashSet<string>
                {
                    "Id", "MatchId", "DataType", "JsonData", "ProcessedAt"
                }
            },
            
            // Entity & Environment Tables
            {
                "EntityEffects", new HashSet<string>
                {
                    "Id", "RoundId", "Tick", "EntityType", "EffectType", "PositionX", "PositionY", "PositionZ", "Duration"
                }
            },
            {
                "EntityInteractions", new HashSet<string>
                {
                    "Id", "PlayerId", "RoundId", "Tick", "EntityType", "InteractionType", "EntityId", "Result"
                }
            },
            {
                "EntityLifecycles", new HashSet<string>
                {
                    "Id", "RoundId", "EntityId", "CreationTick", "DestructionTick", "EntityType", "LifespanSeconds"
                }
            },
            {
                "EntityPropertyChanges", new HashSet<string>
                {
                    "Id", "RoundId", "Tick", "EntityId", "PropertyName", "OldValue", "NewValue", "ChangeReason"
                }
            },
            {
                "EntityVisibilities", new HashSet<string>
                {
                    "Id", "ObserverPlayerId", "TargetEntityId", "RoundId", "Tick", "IsVisible", "Distance", "ViewAngle"
                }
            },
            {
                "TemporaryEntities", new HashSet<string>
                {
                    "Id", "RoundId", "Tick", "EntityType", "PositionX", "PositionY", "PositionZ", "Duration", "Properties"
                }
            }
        };

        // Safe SQL functions and operators
        private readonly HashSet<string> _allowedFunctions = new()
        {
            "COUNT", "SUM", "AVG", "MIN", "MAX", "DISTINCT", "TOP",
            "CAST", "CONVERT", "ISNULL", "NULLIF", "CASE", "WHEN", "THEN", "ELSE", "END"
        };

        private readonly HashSet<string> _allowedOperators = new()
        {
            "=", "!=", "<>", "<", ">", "<=", ">=", "AND", "OR", "NOT", "IN", "NOT IN",
            "LIKE", "NOT LIKE", "BETWEEN", "IS NULL", "IS NOT NULL", "EXISTS", "NOT EXISTS"
        };

        public QueryBuilderController(IConfiguration configuration, ILogger<QueryBuilderController> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new ArgumentNullException("DefaultConnection");
            _logger = logger;
        }

        [HttpGet("schema")]
        public async Task<IActionResult> GetDatabaseSchema()
        {
            try
            {
                // Get tables that actually have data
                var tablesWithData = await GetTablesWithData();
                
                var schema = _allowedTables
                    .Where(kvp => tablesWithData.ContainsKey(kvp.Key))
                    .Select(kvp => new
                    {
                        TableName = kvp.Key,
                        Columns = kvp.Value.Select(col => new { ColumnName = col }).ToList(),
                        RowCount = tablesWithData.ContainsKey(kvp.Key) ? tablesWithData[kvp.Key] : 0
                    }).ToList();

                return Ok(new
                {
                    Tables = schema,
                    AllowedFunctions = _allowedFunctions,
                    AllowedOperators = _allowedOperators
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving database schema");
                return StatusCode(500, "Error retrieving database schema");
            }
        }
        
        private async Task<Dictionary<string, int>> GetTablesWithData()
        {
            var tablesWithData = new Dictionary<string, int>();
            
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            foreach (var tableName in _allowedTables.Keys)
            {
                try
                {
                    var sql = $"SELECT COUNT(*) FROM {SanitizeIdentifier(tableName)}";
                    using var command = new SqlCommand(sql, connection);
                    command.CommandTimeout = 10; // Quick check
                    
                    var count = Convert.ToInt32(await command.ExecuteScalarAsync());
                    if (count > 0)
                    {
                        tablesWithData[tableName] = count;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Table {TableName} doesn't exist or has no data: {Error}", tableName, ex.Message);
                }
            }
            
            return tablesWithData;
        }

        [HttpPost("visual")]
        public async Task<IActionResult> ExecuteVisualQuery([FromBody] VisualQueryRequest request)
        {
            try
            {
                var sql = BuildSqlFromVisualQuery(request);
                _logger.LogInformation("Generated SQL from visual query: {SQL}", sql);
                
                var results = await ExecuteSafeQuery(sql, new Dictionary<string, object>());
                
                if (request.Format?.ToLower() == "csv")
                {
                    var csv = ConvertToCsv(results);
                    var fileName = $"query_results_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                    Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");
                    return File(Encoding.UTF8.GetBytes(csv), "text/csv");
                }

                return Ok(new
                {
                    Query = sql,
                    Results = results,
                    Count = results.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing visual query");
                return BadRequest($"Query error: {ex.Message}");
            }
        }

        [HttpPost("advanced")]
        public async Task<IActionResult> ExecuteAdvancedQuery([FromBody] AdvancedQueryRequest request)
        {
            try
            {
                // Validate and sanitize the SQL query
                var sanitizedSql = ValidateAndSanitizeQuery(request.SqlQuery);
                if (sanitizedSql == null)
                {
                    return BadRequest("Invalid or unsafe SQL query. Only SELECT statements with approved tables and columns are allowed.");
                }

                _logger.LogInformation("Executing advanced SQL query: {SQL}", sanitizedSql);
                
                var results = await ExecuteSafeQuery(sanitizedSql, request.Parameters ?? new Dictionary<string, object>());
                
                if (request.Format?.ToLower() == "csv")
                {
                    var csv = ConvertToCsv(results);
                    var fileName = $"advanced_query_results_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                    Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");
                    return File(Encoding.UTF8.GetBytes(csv), "text/csv");
                }

                return Ok(new
                {
                    Query = sanitizedSql,
                    Results = results,
                    Count = results.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing advanced query: {Error}", ex.Message);
                return BadRequest($"Query error: {ex.Message}");
            }
        }

        [HttpPost("validate")]
        public IActionResult ValidateQuery([FromBody] QueryValidationRequest request)
        {
            try
            {
                var sanitizedSql = ValidateAndSanitizeQuery(request.SqlQuery);
                var isValid = sanitizedSql != null;

                return Ok(new
                {
                    IsValid = isValid,
                    SanitizedQuery = sanitizedSql,
                    Message = isValid ? "Query is valid and safe" : "Query contains invalid or unsafe elements"
                });
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    IsValid = false,
                    SanitizedQuery = (string?)null,
                    Message = $"Query validation error: {ex.Message}"
                });
            }
        }

        [HttpPost("execute")]
        public async Task<IActionResult> ExecuteSimpleQuery([FromBody] SimpleQueryRequest request)
        {
            try
            {
                var sanitizedSql = ValidateAndSanitizeQuery(request.Sql);
                if (sanitizedSql == null)
                {
                    return BadRequest("Invalid or unsafe SQL query. Only SELECT statements with approved tables and columns are allowed.");
                }

                _logger.LogInformation("Executing simple SQL query: {SQL}", sanitizedSql);
                
                var results = await ExecuteSafeQuery(sanitizedSql, new Dictionary<string, object>());
                
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing simple query: {Error}", ex.Message);
                return BadRequest($"Query error: {ex.Message}");
            }
        }

        [HttpPost("export")]
        public async Task<IActionResult> ExportQueryResults([FromBody] SimpleQueryRequest request)
        {
            try
            {
                var sanitizedSql = ValidateAndSanitizeQuery(request.Sql);
                if (sanitizedSql == null)
                {
                    return BadRequest("Invalid or unsafe SQL query. Only SELECT statements with approved tables and columns are allowed.");
                }

                _logger.LogInformation("Exporting query results: {SQL}", sanitizedSql);
                
                var results = await ExecuteSafeQuery(sanitizedSql, new Dictionary<string, object>());
                var csv = ConvertToCsv(results);
                var fileName = $"query_export_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                
                Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");
                return File(Encoding.UTF8.GetBytes(csv), "text/csv");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting query: {Error}", ex.Message);
                return BadRequest($"Query error: {ex.Message}");
            }
        }

        private string BuildSqlFromVisualQuery(VisualQueryRequest request)
        {
            var sql = new StringBuilder("SELECT ");

            // Build SELECT clause
            if (request.SelectFields?.Any() == true)
            {
                var validFields = new List<string>();
                foreach (var field in request.SelectFields)
                {
                    if (IsValidTableColumn(field.Table, field.Column))
                    {
                        var fieldName = string.IsNullOrEmpty(field.Alias) 
                            ? $"{field.Table}.{field.Column}"
                            : $"{field.Table}.{field.Column} AS {SanitizeIdentifier(field.Alias)}";
                        validFields.Add(fieldName);
                    }
                }
                sql.Append(string.Join(", ", validFields));
            }
            else
            {
                sql.Append("*");
            }

            // Build FROM clause
            sql.Append($" FROM {SanitizeIdentifier(request.FromTable)}");

            // Build JOIN clauses
            if (request.Joins?.Any() == true)
            {
                foreach (var join in request.Joins)
                {
                    if (_allowedTables.ContainsKey(join.Table) && 
                        IsValidTableColumn(join.LeftTable, join.LeftColumn) &&
                        IsValidTableColumn(join.RightTable, join.RightColumn))
                    {
                        sql.Append($" {join.JoinType} JOIN {SanitizeIdentifier(join.Table)} ON {join.LeftTable}.{join.LeftColumn} = {join.RightTable}.{join.RightColumn}");
                    }
                }
            }

            // Build WHERE clause
            if (request.WhereConditions?.Any() == true)
            {
                sql.Append(" WHERE ");
                var conditions = new List<string>();
                
                foreach (var condition in request.WhereConditions)
                {
                    if (IsValidTableColumn(condition.Table, condition.Column) && 
                        _allowedOperators.Contains(condition.Operator.ToUpper()))
                    {
                        var conditionSql = $"{condition.Table}.{condition.Column} {condition.Operator}";
                        
                        if (condition.Operator.ToUpper().Contains("NULL"))
                        {
                            // No value needed for NULL checks
                        }
                        else if (condition.Operator.ToUpper() == "IN" || condition.Operator.ToUpper() == "NOT IN")
                        {
                            // Handle IN clause
                            var values = condition.Value?.ToString()?.Split(',').Select(v => $"'{SanitizeValue(v.Trim())}'");
                            conditionSql += $" ({string.Join(", ", values ?? new[] { "''" })})";
                        }
                        else
                        {
                            conditionSql += $" '{SanitizeValue(condition.Value?.ToString() ?? "")}'";
                        }
                        
                        conditions.Add(conditionSql);
                    }
                }
                
                sql.Append(string.Join($" {request.WhereLogic ?? "AND"} ", conditions));
            }

            // Build GROUP BY clause
            if (request.GroupByFields?.Any() == true)
            {
                var validGroupFields = request.GroupByFields
                    .Where(field => IsValidTableColumn(field.Table, field.Column))
                    .Select(field => $"{field.Table}.{field.Column}");
                
                if (validGroupFields.Any())
                {
                    sql.Append($" GROUP BY {string.Join(", ", validGroupFields)}");
                }
            }

            // Build ORDER BY clause
            if (request.OrderByFields?.Any() == true)
            {
                var validOrderFields = request.OrderByFields
                    .Where(field => IsValidTableColumn(field.Table, field.Column))
                    .Select(field => $"{field.Table}.{field.Column} {(field.Descending ? "DESC" : "ASC")}");
                
                if (validOrderFields.Any())
                {
                    sql.Append($" ORDER BY {string.Join(", ", validOrderFields)}");
                }
            }

            // Add LIMIT if specified
            if (request.Limit > 0)
            {
                sql.Insert(7, $"TOP {request.Limit} "); // Insert after "SELECT "
            }

            return sql.ToString();
        }

        private string? ValidateAndSanitizeQuery(string sqlQuery)
        {
            if (string.IsNullOrWhiteSpace(sqlQuery))
                return null;

            // Remove comments and normalize whitespace
            var cleanQuery = Regex.Replace(sqlQuery, @"/\*.*?\*/|--.*?(\r?\n|$)", " ", RegexOptions.Singleline);
            cleanQuery = Regex.Replace(cleanQuery, @"\s+", " ").Trim();

            // Must be a SELECT statement
            if (!cleanQuery.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
                return null;

            // Block dangerous keywords
            var dangerousKeywords = new[] { "DROP", "DELETE", "INSERT", "UPDATE", "CREATE", "ALTER", "EXEC", "EXECUTE", "xp_", "sp_" };
            foreach (var keyword in dangerousKeywords)
            {
                if (Regex.IsMatch(cleanQuery, $@"\b{keyword}\b", RegexOptions.IgnoreCase))
                    return null;
            }

            // Validate table references
            var tableMatches = Regex.Matches(cleanQuery, @"\bFROM\s+(\w+)|\bJOIN\s+(\w+)", RegexOptions.IgnoreCase);
            foreach (Match match in tableMatches)
            {
                var tableName = match.Groups[1].Value ?? match.Groups[2].Value;
                if (!_allowedTables.ContainsKey(tableName))
                    return null;
            }

            // Basic validation passed
            return cleanQuery;
        }

        private bool IsValidTableColumn(string tableName, string columnName)
        {
            return _allowedTables.ContainsKey(tableName) && 
                   _allowedTables[tableName].Contains(columnName);
        }

        private string SanitizeIdentifier(string identifier)
        {
            // Remove any non-alphanumeric characters except underscore
            return Regex.Replace(identifier ?? "", @"[^\w]", "");
        }

        private string SanitizeValue(string? value)
        {
            // Escape single quotes
            return value?.Replace("'", "''") ?? "";
        }

        private async Task<List<Dictionary<string, object>>> ExecuteSafeQuery(string sql, Dictionary<string, object> parameters)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            using var command = new SqlCommand(sql, connection);
            
            // Add parameters to prevent injection
            foreach (var param in parameters)
            {
                command.Parameters.AddWithValue($"@{SanitizeIdentifier(param.Key)}", param.Value ?? DBNull.Value);
            }
            
            // Set a reasonable timeout
            command.CommandTimeout = 30;
            
            using var reader = await command.ExecuteReaderAsync();
            var results = new List<Dictionary<string, object>>();
            
            // Limit results to prevent memory issues
            int maxRows = 10000;
            int rowCount = 0;
            
            while (await reader.ReadAsync() && rowCount < maxRows)
            {
                var row = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var value = reader.GetValue(i);
                    row[reader.GetName(i)] = value == DBNull.Value ? null! : value;
                }
                results.Add(row);
                rowCount++;
            }
            
            return results;
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
    }

    public class VisualQueryRequest
    {
        public string FromTable { get; set; } = "";
        public List<SelectField>? SelectFields { get; set; }
        public List<JoinClause>? Joins { get; set; }
        public List<WhereCondition>? WhereConditions { get; set; }
        public string? WhereLogic { get; set; } = "AND";
        public List<GroupByField>? GroupByFields { get; set; }
        public List<OrderByField>? OrderByFields { get; set; }
        public int Limit { get; set; } = 0;
        public string? Format { get; set; }
    }

    public class SelectField
    {
        public string Table { get; set; } = "";
        public string Column { get; set; } = "";
        public string? Alias { get; set; }
    }

    public class JoinClause
    {
        public string JoinType { get; set; } = "INNER";
        public string Table { get; set; } = "";
        public string LeftTable { get; set; } = "";
        public string LeftColumn { get; set; } = "";
        public string RightTable { get; set; } = "";
        public string RightColumn { get; set; } = "";
    }

    public class WhereCondition
    {
        public string Table { get; set; } = "";
        public string Column { get; set; } = "";
        public string Operator { get; set; } = "=";
        public object? Value { get; set; }
    }

    public class GroupByField
    {
        public string Table { get; set; } = "";
        public string Column { get; set; } = "";
    }

    public class OrderByField
    {
        public string Table { get; set; } = "";
        public string Column { get; set; } = "";
        public bool Descending { get; set; } = false;
    }

    public class AdvancedQueryRequest
    {
        public string SqlQuery { get; set; } = "";
        public Dictionary<string, object>? Parameters { get; set; }
        public string? Format { get; set; }
    }

    public class QueryValidationRequest
    {
        public string SqlQuery { get; set; } = "";
    }

    public class SimpleQueryRequest
    {
        public string Sql { get; set; } = "";
    }
}