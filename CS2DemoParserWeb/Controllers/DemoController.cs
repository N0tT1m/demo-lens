using Microsoft.AspNetCore.Mvc;
using CS2DemoParser.Services;
using CS2DemoParser.Data;
using Microsoft.EntityFrameworkCore;

namespace CS2DemoParserWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DemoController : ControllerBase
    {
        private readonly CorrectedDemoParserService _demoParserService;
        private readonly CS2DemoContext _context;
        private readonly ILogger<DemoController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public DemoController(
            CorrectedDemoParserService demoParserService,
            CS2DemoContext context,
            ILogger<DemoController> logger,
            IConfiguration configuration,
            IServiceScopeFactory serviceScopeFactory)
        {
            _demoParserService = demoParserService;
            _context = context;
            _logger = logger;
            _configuration = configuration;
            _serviceScopeFactory = serviceScopeFactory;
        }

        [HttpPost("upload")]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> UploadDemo([FromForm] IFormFile file, [FromForm] string demoSource, [FromForm] string mapName)
        {
            _logger.LogInformation("Upload endpoint hit - Request content type: {ContentType}", Request.ContentType);
            _logger.LogInformation("Form files count: {Count}", Request.Form.Files.Count);
            _logger.LogInformation("Demo source: {DemoSource}, Map name: {MapName}", demoSource, mapName);
            
            if (Request.Form.Files.Count > 0)
            {
                var formFile = Request.Form.Files[0];
                _logger.LogInformation("First form file - Name: {Name}, Length: {Length}", formFile.FileName, formFile.Length);
            }
            
            return await UploadDemoInternal(file, demoSource, mapName);
        }

        private async Task<IActionResult> UploadDemoInternal(IFormFile file, string demoSource, string mapName)
        {
            try
            {
                _logger.LogInformation("Upload attempt - File: {File}, Length: {Length}, Source: {DemoSource}, Map: {MapName}", file?.FileName ?? "null", file?.Length ?? 0, demoSource, mapName);
                
                if (file == null || file.Length == 0)
                {
                    _logger.LogWarning("Upload failed - No file or empty file");
                    return BadRequest(new { message = "No file uploaded or file is empty" });
                }

                if (string.IsNullOrEmpty(demoSource))
                {
                    return BadRequest(new { message = "Demo source is required" });
                }

                if (string.IsNullOrEmpty(mapName))
                {
                    return BadRequest(new { message = "Map name is required" });
                }

                if (!file.FileName.EndsWith(".dem", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest("Only .dem files are allowed");
                }

                var maxSizeBytes = _configuration.GetValue<long>("DemoParser:MaxFileSizeBytes", 1_000_000_000);
                if (file.Length > maxSizeBytes)
                {
                    return BadRequest($"File size exceeds maximum allowed size of {maxSizeBytes / 1024 / 1024}MB");
                }

                // Create uploads directory if it doesn't exist
                var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
                if (!Directory.Exists(uploadDir))
                {
                    Directory.CreateDirectory(uploadDir);
                }

                // Save file with unique name
                var fileName = $"{Guid.NewGuid()}_{file.FileName}";
                var filePath = Path.Combine(uploadDir, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                _logger.LogInformation("File uploaded: {FileName} (Size: {Size} bytes, Source: {DemoSource}, Map: {MapName})", fileName, file.Length, demoSource, mapName);

                // Start parsing in background task with new scope
                _logger.LogInformation("Starting background parse for uploaded file: {FileName} with source: {DemoSource} and map: {MapName}", fileName, demoSource, mapName);
                _ = Task.Run(async () =>
                {
                    // Create a new scope for the background task
                    using var scope = _serviceScopeFactory.CreateScope();
                    var scopedParserService = scope.ServiceProvider.GetRequiredService<CorrectedDemoParserService>();

                    try
                    {
                        var parseResult = await scopedParserService.ParseDemoAsync(filePath, demoSource, mapName);
                        if (parseResult)
                        {
                            _logger.LogInformation("Background parse completed successfully for: {FileName}", fileName);
                        }
                        else
                        {
                            _logger.LogWarning("Background parse failed for: {FileName}", fileName);
                        }

                        // Clean up uploaded file after parsing
                        try
                        {
                            System.IO.File.Delete(filePath);
                            _logger.LogInformation("Cleaned up uploaded file: {FileName}", fileName);
                        }
                        catch (Exception cleanupEx)
                        {
                            _logger.LogWarning(cleanupEx, "Failed to clean up uploaded file: {FileName}", fileName);
                        }
                    }
                    catch (Exception parseEx)
                    {
                        _logger.LogError(parseEx, "Error during background parse for: {FileName}", fileName);

                        // Clean up uploaded file after parsing error
                        try
                        {
                            System.IO.File.Delete(filePath);
                            _logger.LogInformation("Cleaned up uploaded file after error: {FileName}", fileName);
                        }
                        catch (Exception cleanupEx)
                        {
                            _logger.LogWarning(cleanupEx, "Failed to clean up uploaded file: {FileName}", fileName);
                        }
                    }
                });

                // Return immediately - parsing happens in background
                return Ok(new
                {
                    message = "File uploaded, parsing started",
                    fileName = fileName,
                    originalName = file.FileName,
                    size = file.Length,
                    demoSource = demoSource,
                    mapName = mapName,
                    parsing = true // Indicates parsing is in progress
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file");
                return StatusCode(500, "Error uploading file");
            }
        }

        [HttpPost("parse/{fileName}")]
        public async Task<IActionResult> ParseDemo(string fileName)
        {
            try
            {
                var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
                var filePath = Path.Combine(uploadDir, fileName);

                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound("Demo file not found");
                }

                _logger.LogInformation("Starting to parse demo: {FileName}", fileName);
                
                var parseStartTime = DateTime.UtcNow;
                var success = await _demoParserService.ParseDemoAsync(filePath);
                var parseEndTime = DateTime.UtcNow;
                var parseDuration = parseEndTime - parseStartTime;

                if (success)
                {
                    _logger.LogInformation("Demo parsing completed successfully in {Duration}", parseDuration);
                    
                    // Get parsing statistics
                    var statsResponse = await GetStatistics();
                    var stats = (statsResponse as OkObjectResult)?.Value;
                    
                    // Clean up uploaded file after successful parsing
                    try
                    {
                        System.IO.File.Delete(filePath);
                        _logger.LogInformation("Cleaned up uploaded file: {FileName}", fileName);
                    }
                    catch (Exception cleanupEx)
                    {
                        _logger.LogWarning(cleanupEx, "Failed to clean up uploaded file: {FileName}", fileName);
                    }

                    return Ok(new
                    {
                        message = "Demo parsed successfully",
                        duration = parseDuration.ToString(),
                        statistics = stats
                    });
                }
                else
                {
                    _logger.LogError("Demo parsing failed for file: {FileName}", fileName);
                    return StatusCode(500, "Demo parsing failed");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing demo: {FileName}", fileName);
                return StatusCode(500, $"Error parsing demo: {ex.Message}");
            }
        }

        [HttpGet("progress")]
        public IActionResult GetParseProgress()
        {
            try
            {
                return Ok(new
                {
                    isParsing = _demoParserService.IsParsing,
                    progress = Math.Round(_demoParserService.ParseProgress, 2),
                    fileName = _demoParserService.CurrentFileName
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving parse progress");
                return StatusCode(500, "Error retrieving parse progress");
            }
        }

        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics()
        {
            try
            {
                // Check if database is accessible
                if (!await _context.Database.CanConnectAsync())
                {
                    _logger.LogWarning("Database connection failed");
                    return Ok(new { message = "Database not available", connected = false });
                }

                var stats = new
                {
                    demoFiles = await _context.DemoFiles.CountAsync(),
                    players = await _context.Players.CountAsync(),
                    matches = await _context.Matches.CountAsync(),
                    rounds = await _context.Rounds.CountAsync(),
                    kills = await _context.Kills.CountAsync(),
                    damages = await _context.Damages.CountAsync(),
                    weaponFires = await _context.WeaponFires.CountAsync(),
                    grenades = await _context.Grenades.CountAsync(),
                    bombEvents = await _context.Bombs.CountAsync(),
                    playerPositions = await _context.PlayerPositions.CountAsync(),
                    equipment = await _context.Equipment.CountAsync(),
                    economyEvents = await _context.EconomyEvents.CountAsync(),
                    gameEvents = await _context.GameEvents.CountAsync(),
                    flashEvents = await _context.FlashEvents.CountAsync(),
                    smokeClouds = await _context.SmokeClouds.CountAsync(),
                    playerMatchStats = await _context.PlayerMatchStats.CountAsync(),
                    playerRoundStats = await _context.PlayerRoundStats.CountAsync(),
                    connected = true
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving statistics");
                return Ok(new { 
                    message = $"Database error: {ex.Message}", 
                    connected = false,
                    demoFiles = 0,
                    players = 0,
                    matches = 0,
                    rounds = 0,
                    kills = 0,
                    damages = 0,
                    weaponFires = 0,
                    grenades = 0,
                    bombEvents = 0,
                    playerPositions = 0,
                    equipment = 0,
                    economyEvents = 0,
                    gameEvents = 0,
                    flashEvents = 0,
                    smokeClouds = 0,
                    playerMatchStats = 0,
                    playerRoundStats = 0
                });
            }
        }

        [HttpGet("demos")]
        public async Task<IActionResult> GetParsedDemos([FromQuery] string? mapName = null)
        {
            try
            {
                var query = _context.DemoFiles.AsQueryable();
                
                // Filter by map name if provided
                if (!string.IsNullOrEmpty(mapName))
                {
                    query = query.Where(d => d.MapName == mapName);
                }
                
                var demos = await query
                    .OrderByDescending(d => d.ParsedAt)
                    .Select(d => new
                    {
                        d.Id,
                        d.FileName,
                        d.MapName,
                        d.Duration,
                        d.ParsedAt,
                        d.TotalTicks,
                        d.TickRate,
                        d.GameMode,
                        PlayerCount = _context.Players.Count(p => p.DemoFileId == d.Id)
                    })
                    .Take(50)
                    .ToListAsync();

                return Ok(demos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving parsed demos");
                return StatusCode(500, "Error retrieving parsed demos");
            }
        }

        [HttpGet("demo/{id}/details")]
        public async Task<IActionResult> GetDemoDetails(int id)
        {
            try
            {
                var demo = await _context.DemoFiles
                    .Where(d => d.Id == id)
                    .Select(d => new
                    {
                        d.Id,
                        d.FileName,
                        d.MapName,
                        d.Duration,
                        d.ParsedAt,
                        d.TotalTicks,
                        d.TickRate,
                        d.GameMode,
                        Players = _context.Players
                            .Where(p => p.DemoFileId == d.Id)
                            .Select(p => new
                            {
                                p.PlayerName,
                                p.SteamId,
                                p.Team,
                                p.PlayerSlot
                            }).ToList(),
                        Matches = _context.Matches
                            .Where(m => m.DemoFileId == d.Id)
                            .Select(m => new
                            {
                                m.Id,
                                m.StartTime,
                                m.EndTime,
                                m.MapName,
                                m.GameMode,
                                RoundCount = _context.Rounds.Count(r => r.MatchId == m.Id)
                            }).ToList()
                    })
                    .FirstOrDefaultAsync();

                if (demo == null)
                {
                    return NotFound("Demo not found");
                }

                return Ok(demo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving demo details for ID: {Id}", id);
                return StatusCode(500, "Error retrieving demo details");
            }
        }

        [HttpGet("rounds")]
        public async Task<IActionResult> GetRoundsForDemo([FromQuery] int demoId)
        {
            try
            {
                // Get the demo source for round number adjustment
                var demo = await _context.DemoFiles
                    .Where(d => d.Id == demoId)
                    .Select(d => new { d.DemoSource })
                    .FirstOrDefaultAsync();

                var rounds = await _context.Rounds
                    .Where(r => r.Match.DemoFileId == demoId)
                    .OrderBy(r => r.RoundNumber)
                    .Select(r => new
                    {
                        r.Id,
                        DatabaseRoundNumber = r.RoundNumber,
                        r.StartTime,
                        r.EndTime,
                        r.WinnerTeam,
                        r.EndReason
                    })
                    .ToListAsync();

                // Adjust round numbers for Faceit/ESEA demos
                var adjustedRounds = rounds.Select(r => new
                {
                    r.Id,
                    RoundNumber = GetDisplayRoundNumber(r.DatabaseRoundNumber, demo?.DemoSource),
                    r.StartTime,
                    r.EndTime,
                    r.WinnerTeam,
                    r.EndReason
                }).Where(r => r.RoundNumber > 0) // Filter out warmup/knife rounds
                .ToList();

                return Ok(adjustedRounds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving rounds for demo ID: {DemoId}", demoId);
                return StatusCode(500, "Error retrieving rounds");
            }
        }

        private int GetDisplayRoundNumber(int databaseRoundNumber, string? demoSource)
        {
            // For Faceit and ESEA demos, subtract 2 to convert database round to display round
            // This makes round 3 (first competitive) display as round 1
            if (demoSource == "faceit" || demoSource == "esea")
            {
                return databaseRoundNumber - 2;
            }
            
            // For other demo sources, use the round number as-is
            return databaseRoundNumber;
        }

        [HttpGet("players")]
        public async Task<IActionResult> GetPlayersForDemo([FromQuery] int demoId)
        {
            try
            {
                var players = await _context.Players
                    .Where(p => p.DemoFileId == demoId)
                    .OrderBy(p => p.PlayerName)
                    .Select(p => new
                    {
                        p.Id,
                        p.PlayerName,
                        p.Team,
                        p.SteamId
                    })
                    .ToListAsync();

                return Ok(players);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving players for demo ID: {DemoId}", demoId);
                return StatusCode(500, "Error retrieving players");
            }
        }

        [HttpPost("fix-maps")]
        public async Task<IActionResult> FixMapNames()
        {
            try
            {
                var unknownMaps = await _context.DemoFiles
                    .Where(d => d.MapName == "Unknown" || d.MapName == null)
                    .ToListAsync();

                foreach (var demo in unknownMaps)
                {
                    demo.MapName = "de_dust2";
                }

                await _context.SaveChangesAsync();

                return Ok(new { message = $"Fixed {unknownMaps.Count} demo files with unknown map names", count = unknownMaps.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fixing map names");
                return StatusCode(500, "Error fixing map names");
            }
        }

        [HttpPost("fix-bomb-teams")]
        public async Task<IActionResult> FixBombTeams()
        {
            try
            {
                // Update Terrorist to 2
                var terroristBombs = await _context.Bombs
                    .Where(b => b.Team == "Terrorist")
                    .ToListAsync();

                foreach (var bomb in terroristBombs)
                {
                    bomb.Team = "2";
                }

                // Update CounterTerrorist to 3
                var ctBombs = await _context.Bombs
                    .Where(b => b.Team == "CounterTerrorist")
                    .ToListAsync();

                foreach (var bomb in ctBombs)
                {
                    bomb.Team = "3";
                }

                await _context.SaveChangesAsync();

                return Ok(new {
                    message = $"Fixed bomb team values",
                    terroristCount = terroristBombs.Count,
                    ctCount = ctBombs.Count,
                    total = terroristBombs.Count + ctBombs.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fixing bomb team values");
                return StatusCode(500, "Error fixing bomb team values");
            }
        }

        [HttpGet("recent-logs")]
        public IActionResult GetRecentLogs([FromQuery] int count = 50)
        {
            // This would require a logging provider that stores logs
            // For now, return a message about where to find logs
            return Ok(new
            {
                message = "Logs are available in your application hosting environment",
                suggestions = new[]
                {
                    "Check console output if running locally",
                    "Check docker logs if running in container: docker logs <container-name>",
                    "Check Application Insights if configured in Azure",
                    "The logs you're looking for contain: 'ShouldSkipRound check', 'Round start event fired', 'Starting to parse demo file'"
                }
            });
        }

        [HttpGet("check-spawn-positions")]
        public async Task<IActionResult> CheckSpawnPositions()
        {
            try
            {
                var latestDemo = await _context.DemoFiles
                    .OrderByDescending(d => d.Id)
                    .FirstOrDefaultAsync();

                if (latestDemo == null)
                {
                    return Ok(new { message = "No demos found" });
                }

                // Get position counts per round
                var roundPositions = await _context.Rounds
                    .Where(r => r.DemoFileId == latestDemo.Id)
                    .Select(r => new
                    {
                        RoundNumber = r.RoundNumber,
                        PositionCount = _context.PlayerPositions.Count(pp => pp.DemoFileId == latestDemo.Id && pp.Tick >= r.StartTick && pp.Tick <= r.EndTick)
                    })
                    .ToListAsync();

                // Get sample spawn positions from round 1
                var round1 = await _context.Rounds
                    .Where(r => r.DemoFileId == latestDemo.Id && r.RoundNumber == 1)
                    .FirstOrDefaultAsync();

                var spawnPositions = new List<object>();
                if (round1 != null)
                {
                    var endTick = round1.StartTick + (5 * 64); // First 5 seconds at 64 tick rate
                    spawnPositions = await _context.PlayerPositions
                        .Where(pp => pp.DemoFileId == latestDemo.Id
                            && pp.Tick >= round1.StartTick
                            && pp.Tick <= endTick)
                        .OrderBy(pp => pp.Tick)
                        .Take(20)
                        .Select(pp => new
                        {
                            PlayerName = pp.Player.PlayerName,
                            Tick = pp.Tick,
                            X = pp.PositionX,
                            Y = pp.PositionY,
                            Z = pp.PositionZ
                        })
                        .ToListAsync<object>();
                }

                return Ok(new
                {
                    demoId = latestDemo.Id,
                    demoName = latestDemo.FileName,
                    totalRounds = roundPositions.Count,
                    roundPositions = roundPositions,
                    round1SpawnSample = spawnPositions,
                    hasPositions = spawnPositions.Count > 0
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking spawn positions");
                return StatusCode(500, $"Error checking spawn positions: {ex.Message}");
            }
        }
    }
}