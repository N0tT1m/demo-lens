using Microsoft.AspNetCore.Mvc;
using CS2DemoParserWeb.Services;

namespace CS2DemoParserWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HeatmapController : ControllerBase
    {
        private readonly IHeatmapService _heatmapService;
        private readonly IHeatmapImageService _heatmapImageService;
        private readonly ILogger<HeatmapController> _logger;

        public HeatmapController(IHeatmapService heatmapService, IHeatmapImageService heatmapImageService, ILogger<HeatmapController> logger)
        {
            _heatmapService = heatmapService;
            _heatmapImageService = heatmapImageService;
            _logger = logger;
        }

        private async Task<HeatmapQuery> PopulateDemoSourceAsync(HeatmapQuery query)
        {
            // If DemoId is provided but DemoSource is not, fetch it from the database
            if (query.DemoId.HasValue && string.IsNullOrEmpty(query.DemoSource))
            {
                query.DemoSource = await _heatmapService.GetDemoSourceAsync(query.DemoId.Value);
            }
            return query;
        }

        [HttpGet("maps")]
        public async Task<ActionResult<List<string>>> GetAvailableMaps()
        {
            try
            {
                var maps = await _heatmapService.GetAvailableMapsAsync();
                return Ok(maps);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving available maps");
                // Return empty list instead of error to allow UI to function
                return Ok(new List<string>());
            }
        }

        [HttpGet("players")]
        public async Task<ActionResult<List<string>>> GetAvailablePlayers([FromQuery] string? demoName = null, [FromQuery] int? demoId = null)
        {
            try
            {
                List<string> players;
                if (demoId.HasValue)
                {
                    players = await _heatmapService.GetAvailablePlayersByDemoIdAsync(demoId.Value);
                }
                else
                {
                    players = await _heatmapService.GetAvailablePlayersAsync(demoName);
                }
                return Ok(players);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving available players");
                // Return empty list instead of error to allow UI to function
                return Ok(new List<string>());
            }
        }

        [HttpGet("rounds")]
        public async Task<ActionResult<List<int>>> GetAvailableRounds([FromQuery] int demoId)
        {
            try
            {
                var rounds = await _heatmapService.GetAvailableRoundsByDemoIdAsync(demoId);
                return Ok(rounds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving available rounds for demo {DemoId}", demoId);
                // Return empty list instead of error to allow UI to function
                return Ok(new List<int>());
            }
        }

        [HttpGet("player-positions")]
        public async Task<ActionResult<HeatmapData>> GetPlayerPositionHeatmap([FromQuery] HeatmapQuery query)
        {
            try
            {
                query = await PopulateDemoSourceAsync(query);
                
                _logger.LogInformation("Player position heatmap request - MapName: {MapName}, PlayerName: {PlayerName}, Team: {Team}, DemoId: {DemoId}, RoundNumber: {RoundNumber}, DemoSource: {DemoSource}", 
                    query.MapName, query.PlayerName, query.Team, query.DemoId, query.RoundNumber, query.DemoSource);
                
                var heatmapData = await _heatmapService.GetPlayerPositionHeatmapAsync(query);
                return Ok(heatmapData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating player position heatmap");
                return StatusCode(500, "Error generating player position heatmap");
            }
        }

        [HttpGet("deaths")]
        public async Task<ActionResult<HeatmapData>> GetDeathHeatmap([FromQuery] HeatmapQuery query)
        {
            try
            {
                query = await PopulateDemoSourceAsync(query);
                var heatmapData = await _heatmapService.GetDeathHeatmapAsync(query);
                return Ok(heatmapData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating death heatmap");
                return StatusCode(500, "Error generating death heatmap");
            }
        }

        [HttpGet("utility")]
        public async Task<ActionResult<HeatmapData>> GetUtilityHeatmap([FromQuery] HeatmapQuery query)
        {
            try
            {
                query = await PopulateDemoSourceAsync(query);
                var heatmapData = await _heatmapService.GetUtilityHeatmapAsync(query);
                return Ok(heatmapData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating utility heatmap");
                return StatusCode(500, "Error generating utility heatmap");
            }
        }

        [HttpGet("bomb-sites")]
        public async Task<ActionResult<HeatmapData>> GetBombSiteHeatmap([FromQuery] HeatmapQuery query)
        {
            try
            {
                query = await PopulateDemoSourceAsync(query);
                var heatmapData = await _heatmapService.GetBombSiteHeatmapAsync(query);
                return Ok(heatmapData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating bomb site heatmap");
                return StatusCode(500, "Error generating bomb site heatmap");
            }
        }

        [HttpGet("bomb-events")]
        public async Task<ActionResult<HeatmapData>> GetBombEventsHeatmap([FromQuery] HeatmapQuery query)
        {
            try
            {
                query = await PopulateDemoSourceAsync(query);
                var heatmapData = await _heatmapService.GetBombEventsHeatmapAsync(query);
                return Ok(heatmapData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating bomb events heatmap");
                return StatusCode(500, "Error generating bomb events heatmap");
            }
        }

        [HttpGet("weapon-fire")]
        public async Task<ActionResult<HeatmapData>> GetWeaponFireHeatmap([FromQuery] HeatmapQuery query)
        {
            try
            {
                query = await PopulateDemoSourceAsync(query);
                var heatmapData = await _heatmapService.GetWeaponFireHeatmapAsync(query);
                return Ok(heatmapData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating weapon fire heatmap");
                return StatusCode(500, "Error generating weapon fire heatmap");
            }
        }

        [HttpGet("generate")]
        public async Task<IActionResult> GenerateHeatmap([FromQuery] HeatmapQuery query, [FromQuery] string eventType)
        {
            try
            {
                query = await PopulateDemoSourceAsync(query);
                
                _logger.LogInformation("Generate heatmap request - EventType: {EventType}, MapName: {MapName}, Team: {Team}, DemoId: {DemoId}", 
                    eventType, query.MapName, query.Team, query.DemoId);

                HeatmapData heatmapData = eventType.ToLower() switch
                {
                    "kills" => await _heatmapService.GetDeathHeatmapAsync(query),
                    "deaths" => await _heatmapService.GetDeathHeatmapAsync(query),
                    "positions" => await _heatmapService.GetPlayerPositionHeatmapAsync(query),
                    "grenades" => await _heatmapService.GetUtilityHeatmapAsync(query),
                    "weapon_fires" => await _heatmapService.GetWeaponFireHeatmapAsync(query),
                    "bomb_plants" => await _heatmapService.GetBombSiteHeatmapAsync(query),
                    _ => throw new ArgumentException($"Unknown event type: {eventType}")
                };

                // Generate the heatmap image
                var imageBytes = await _heatmapImageService.GenerateHeatmapImageAsync(heatmapData);
                
                return File(imageBytes, "image/png", $"heatmap_{query.MapName}_{eventType}.png");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating heatmap for event type: {EventType}", eventType);
                return StatusCode(500, $"Error generating heatmap: {ex.Message}");
            }
        }

    }
}