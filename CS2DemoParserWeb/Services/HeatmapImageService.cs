using SkiaSharp;
using CS2DemoParserWeb.Services;

namespace CS2DemoParserWeb.Services
{
    public interface IHeatmapImageService
    {
        Task<byte[]> GenerateHeatmapImageAsync(HeatmapData heatmapData);
    }

    public class LegendItem
    {
        public string Name { get; set; } = string.Empty;
        public SKColor Color { get; set; }
    }

    public class HeatmapImageService : IHeatmapImageService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<HeatmapImageService> _logger;

        public HeatmapImageService(IWebHostEnvironment environment, ILogger<HeatmapImageService> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        public Task<byte[]> GenerateHeatmapImageAsync(HeatmapData heatmapData)
        {
            try
            {
                var mapImagePath = GetMapImagePath(heatmapData.MapName);
                
                // Load the base map image
                var mapImage = LoadMapImage(mapImagePath);
                if (mapImage == null)
                {
                    _logger.LogWarning("Map image not found for {MapName}, creating blank image", heatmapData.MapName);
                    mapImage = CreateBlankMapImage();
                }

                // Create a surface for drawing
                using var surface = SKSurface.Create(new SKImageInfo(mapImage.Width, mapImage.Height));
                using var canvas = surface.Canvas;

                // Draw the base map
                canvas.DrawBitmap(mapImage, 0, 0);

                // Draw heatmap points
                DrawHeatmapPoints(canvas, heatmapData, mapImage.Width, mapImage.Height);

                // Generate the final image
                using var finalImage = surface.Snapshot();
                using var data = finalImage.Encode(SKEncodedImageFormat.Png, 100);
                
                // Dispose of the mapImage
                mapImage?.Dispose();
                
                return Task.FromResult(data.ToArray());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating heatmap image for {MapName}", heatmapData.MapName);
                throw;
            }
        }

        private string GetMapImagePath(string mapName)
        {
            var mapFileName = $"{mapName}.png";
            return Path.Combine(_environment.WebRootPath, "maps", mapFileName);
        }

        private SKBitmap? LoadMapImage(string imagePath)
        {
            if (!File.Exists(imagePath))
            {
                _logger.LogWarning("Map image file not found: {ImagePath}", imagePath);
                return null;
            }

            try
            {
                using var stream = File.OpenRead(imagePath);
                return SKBitmap.Decode(stream);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading map image: {ImagePath}", imagePath);
                return null;
            }
        }

        private SKBitmap CreateBlankMapImage()
        {
            var bitmap = new SKBitmap(1024, 1024);
            using var canvas = new SKCanvas(bitmap);
            canvas.Clear(SKColors.DarkGray);
            return bitmap;
        }

        private void DrawHeatmapPoints(SKCanvas canvas, HeatmapData heatmapData, int imageWidth, int imageHeight)
        {
            if (!heatmapData.Points.Any())
                return;

            var mapConfig = GetMapConfig(heatmapData.MapName);
            if (mapConfig == null)
            {
                _logger.LogWarning("No map configuration found for {MapName}", heatmapData.MapName);
                return;
            }

            // Draw coordinate grid
            DrawCoordinateGrid(canvas, imageWidth, imageHeight);

            // Determine what to group by based on heatmap type
            var isUtilityHeatmap = heatmapData.HeatmapType == "utility" || 
                                   heatmapData.Points.Any(p => IsGrenadeType(p.EventType));
            var isBombHeatmap = heatmapData.HeatmapType == "bomb_events" ||
                               heatmapData.Points.Any(p => IsBombEventType(p.EventType));

            if (isUtilityHeatmap)
            {
                DrawUtilityHeatmap(canvas, heatmapData, imageWidth, imageHeight, mapConfig);
            }
            else if (isBombHeatmap)
            {
                DrawBombHeatmap(canvas, heatmapData, imageWidth, imageHeight, mapConfig);
            }
            else
            {
                DrawPlayerHeatmap(canvas, heatmapData, imageWidth, imageHeight, mapConfig);
            }
        }

        private void DrawPlayerHeatmap(SKCanvas canvas, HeatmapData heatmapData, int imageWidth, int imageHeight, MapConfig mapConfig)
        {
            // Group points by player to assign unique colors
            var playerGroups = heatmapData.Points
                .Where(p => !string.IsNullOrEmpty(p.PlayerName))
                .GroupBy(p => p.PlayerName)
                .ToList();

            var playerColors = GetPlayerColors(playerGroups.Count);

            // Draw player points
            for (int i = 0; i < playerGroups.Count; i++)
            {
                var playerGroup = playerGroups[i];
                var playerColor = playerColors[i];
                
                foreach (var point in playerGroup)
                {
                    var screenX = ConvertToImageX(point.X, imageWidth, mapConfig);
                    var screenY = ConvertToImageY(point.Y, imageHeight, mapConfig);

                    // Skip points that are outside the image bounds
                    if (screenX < 0 || screenX >= imageWidth || screenY < 0 || screenY >= imageHeight)
                        continue;

                    DrawPlayerPoint(canvas, screenX, screenY, playerColor, 3);
                }
            }

            // Draw legend for players
            DrawLegend(canvas, playerGroups.Select(g => g.Key).Where(name => !string.IsNullOrEmpty(name)).ToList(), 
                      playerColors, imageWidth, imageHeight, "Players");
        }

        private void DrawUtilityHeatmap(SKCanvas canvas, HeatmapData heatmapData, int imageWidth, int imageHeight, MapConfig mapConfig)
        {
            // Group points by grenade type
            var grenadeGroups = heatmapData.Points
                .Where(p => !string.IsNullOrEmpty(p.EventType))
                .GroupBy(p => p.EventType)
                .ToList();

            // Draw grenade points with type-specific colors
            foreach (var grenadeGroup in grenadeGroups)
            {
                var grenadeColor = GetGrenadeTypeColor(grenadeGroup.Key);
                
                foreach (var point in grenadeGroup)
                {
                    var screenX = ConvertToImageX(point.X, imageWidth, mapConfig);
                    var screenY = ConvertToImageY(point.Y, imageHeight, mapConfig);

                    // Skip points that are outside the image bounds
                    if (screenX < 0 || screenX >= imageWidth || screenY < 0 || screenY >= imageHeight)
                        continue;

                    DrawGrenadePoint(canvas, screenX, screenY, grenadeColor, point.EventType, 5); // Larger radius for better visibility
                }
            }

            // Draw legend for grenades with type-specific colors
            var legendItems = grenadeGroups.Select(g => new LegendItem
            {
                Name = GetFriendlyGrenadeName(g.Key),
                Color = GetGrenadeTypeColor(g.Key)
            }).Where(item => !string.IsNullOrEmpty(item.Name)).ToList();

            DrawGrenadeTypeLegend(canvas, legendItems, imageWidth, imageHeight);
        }

        private void DrawBombHeatmap(SKCanvas canvas, HeatmapData heatmapData, int imageWidth, int imageHeight, MapConfig mapConfig)
        {
            // Group points by bomb event type
            var bombGroups = heatmapData.Points
                .Where(p => !string.IsNullOrEmpty(p.EventType))
                .GroupBy(p => p.EventType)
                .ToList();

            // Draw bomb event points with type-specific colors and icons
            foreach (var bombGroup in bombGroups)
            {
                var bombColor = GetBombEventTypeColor(bombGroup.Key);
                
                foreach (var point in bombGroup)
                {
                    var screenX = ConvertToImageX(point.X, imageWidth, mapConfig);
                    var screenY = ConvertToImageY(point.Y, imageHeight, mapConfig);

                    // Skip points that are outside the image bounds
                    if (screenX < 0 || screenX >= imageWidth || screenY < 0 || screenY >= imageHeight)
                        continue;

                    DrawBombEventPoint(canvas, screenX, screenY, bombColor, point.EventType, 6); // Large for visibility
                }
            }

            // Draw legend for bomb events with type-specific colors
            var legendItems = bombGroups.Select(g => new LegendItem
            {
                Name = GetFriendlyBombEventName(g.Key),
                Color = GetBombEventTypeColor(g.Key)
            }).Where(item => !string.IsNullOrEmpty(item.Name)).ToList();

            DrawBombEventLegend(canvas, legendItems, imageWidth, imageHeight);
        }

        private bool IsGrenadeType(string? eventType)
        {
            if (string.IsNullOrEmpty(eventType)) return false;
            
            var grenadeTypes = new[] { "smokegrenade", "flashbang", "hegrenade", "molotov", "incgrenade", "decoy" };
            return grenadeTypes.Contains(eventType.ToLower());
        }

        private bool IsBombEventType(string? eventType)
        {
            if (string.IsNullOrEmpty(eventType)) return false;
            
            var bombEventTypes = new[] { "begin_plant", "planted", "begin_defuse", "defused", "exploded", "dropped", "pickup" };
            return bombEventTypes.Contains(eventType.ToLower());
        }

        private string GetFriendlyGrenadeName(string? grenadeType)
        {
            if (string.IsNullOrEmpty(grenadeType)) return "Unknown";
            
            return grenadeType.ToLower() switch
            {
                "smokegrenade" => "Smoke",
                "flashbang" => "Flash",
                "hegrenade" => "HE Grenade",
                "molotov" => "Molotov",
                "incgrenade" => "Incendiary",
                "decoy" => "Decoy",
                _ => grenadeType
            };
        }

        private void DrawGrenadePoint(SKCanvas canvas, float x, float y, SKColor color, string? grenadeType, float radius)
        {
            // Different shapes for different grenade types
            if (string.IsNullOrEmpty(grenadeType))
            {
                DrawPlayerPoint(canvas, x, y, color, radius);
                return;
            }

            using var fillPaint = new SKPaint
            {
                Color = color,
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };

            using var strokePaint = new SKPaint
            {
                Color = SKColors.Black,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1,
                IsAntialias = true
            };

            switch (grenadeType.ToLower())
            {
                case "smokegrenade":
                    // Draw circle for smoke
                    canvas.DrawCircle(x, y, radius, fillPaint);
                    canvas.DrawCircle(x, y, radius, strokePaint);
                    break;
                    
                case "flashbang":
                    // Draw star shape for flash
                    DrawStar(canvas, x, y, radius, fillPaint, strokePaint);
                    break;
                    
                case "hegrenade":
                    // Draw square for HE
                    canvas.DrawRect(x - radius, y - radius, radius * 2, radius * 2, fillPaint);
                    canvas.DrawRect(x - radius, y - radius, radius * 2, radius * 2, strokePaint);
                    break;
                    
                case "molotov":
                case "incgrenade":
                    // Draw triangle for molotov/incendiary
                    DrawTriangle(canvas, x, y, radius, fillPaint, strokePaint);
                    break;
                    
                default:
                    // Default to circle
                    canvas.DrawCircle(x, y, radius, fillPaint);
                    canvas.DrawCircle(x, y, radius, strokePaint);
                    break;
            }
        }

        private void DrawStar(SKCanvas canvas, float centerX, float centerY, float radius, SKPaint fillPaint, SKPaint strokePaint)
        {
            // Simple 4-point star (diamond shape)
            using var path = new SKPath();
            path.MoveTo(centerX, centerY - radius);
            path.LineTo(centerX + radius, centerY);
            path.LineTo(centerX, centerY + radius);
            path.LineTo(centerX - radius, centerY);
            path.Close();
            
            canvas.DrawPath(path, fillPaint);
            canvas.DrawPath(path, strokePaint);
        }

        private void DrawTriangle(SKCanvas canvas, float centerX, float centerY, float radius, SKPaint fillPaint, SKPaint strokePaint)
        {
            using var path = new SKPath();
            path.MoveTo(centerX, centerY - radius);
            path.LineTo(centerX - radius, centerY + radius);
            path.LineTo(centerX + radius, centerY + radius);
            path.Close();
            
            canvas.DrawPath(path, fillPaint);
            canvas.DrawPath(path, strokePaint);
        }

        private void DrawBombIcon(SKCanvas canvas, float centerX, float centerY, float radius, SKPaint fillPaint, SKPaint strokePaint)
        {
            // Draw round bomb body
            canvas.DrawCircle(centerX, centerY, radius * 0.8f, fillPaint);
            canvas.DrawCircle(centerX, centerY, radius * 0.8f, strokePaint);
            
            // Draw fuse sticking out of top
            using var fusePaint = new SKPaint
            {
                Color = SKColors.Black,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = radius * 0.3f,
                IsAntialias = true,
                StrokeCap = SKStrokeCap.Round
            };
            
            // Curvy fuse line
            using var fusePath = new SKPath();
            fusePath.MoveTo(centerX, centerY - radius * 0.8f);
            fusePath.LineTo(centerX - radius * 0.3f, centerY - radius * 1.3f);
            fusePath.LineTo(centerX + radius * 0.2f, centerY - radius * 1.5f);
            canvas.DrawPath(fusePath, fusePaint);
            
            // Small spark at end of fuse
            using var sparkPaint = new SKPaint
            {
                Color = new SKColor(255, 215, 0), // Gold
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };
            canvas.DrawCircle(centerX + radius * 0.2f, centerY - radius * 1.5f, radius * 0.2f, sparkPaint);
        }

        private void DrawDefuseKitIcon(SKCanvas canvas, float centerX, float centerY, float radius, SKPaint fillPaint, SKPaint strokePaint)
        {
            // Draw rectangular defuse kit body
            var kitWidth = radius * 1.4f;
            var kitHeight = radius * 1.0f;
            var kitRect = new SKRect(centerX - kitWidth/2, centerY - kitHeight/2, centerX + kitWidth/2, centerY + kitHeight/2);
            canvas.DrawRect(kitRect, fillPaint);
            canvas.DrawRect(kitRect, strokePaint);
            
            // Draw tools sticking out (screwdriver and wire cutters)
            using var toolPaint = new SKPaint
            {
                Color = SKColors.Silver,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = radius * 0.25f,
                IsAntialias = true,
                StrokeCap = SKStrokeCap.Round
            };
            
            // Screwdriver
            canvas.DrawLine(centerX - radius * 0.3f, centerY - kitHeight/2, 
                           centerX - radius * 0.3f, centerY - radius * 1.3f, toolPaint);
            
            // Wire cutters (V shape)
            using var cutterPath = new SKPath();
            cutterPath.MoveTo(centerX + radius * 0.3f, centerY - radius * 1.2f);
            cutterPath.LineTo(centerX + radius * 0.1f, centerY - kitHeight/2);
            cutterPath.LineTo(centerX + radius * 0.5f, centerY - kitHeight/2);
            canvas.DrawPath(cutterPath, toolPaint);
            
            // Small LED indicator
            using var ledPaint = new SKPaint
            {
                Color = SKColors.Lime,
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };
            canvas.DrawCircle(centerX, centerY + radius * 0.2f, radius * 0.15f, ledPaint);
        }

        private void DrawExplosionIcon(SKCanvas canvas, float centerX, float centerY, float radius, SKPaint fillPaint, SKPaint strokePaint)
        {
            // Draw multiple irregular explosion clouds
            var cloudCount = 6;
            var angleStep = 360f / cloudCount;
            
            for (int i = 0; i < cloudCount; i++)
            {
                var angle = (i * angleStep) * Math.PI / 180;
                var distance = radius * (0.6f + (i % 2) * 0.4f); // Vary distance
                var cloudX = centerX + distance * (float)Math.Cos(angle);
                var cloudY = centerY + distance * (float)Math.Sin(angle);
                var cloudRadius = radius * (0.3f + (i % 3) * 0.15f); // Vary size
                
                // Draw jagged cloud shape
                using var cloudPath = new SKPath();
                var points = 8;
                var cloudAngleStep = 360f / points;
                
                for (int j = 0; j < points; j++)
                {
                    var cloudAngle = (j * cloudAngleStep) * Math.PI / 180;
                    var r = cloudRadius * (0.7f + (j % 2) * 0.6f); // Jagged edges
                    var x = cloudX + r * (float)Math.Cos(cloudAngle);
                    var y = cloudY + r * (float)Math.Sin(cloudAngle);
                    
                    if (j == 0)
                        cloudPath.MoveTo(x, y);
                    else
                        cloudPath.LineTo(x, y);
                }
                cloudPath.Close();
                
                canvas.DrawPath(cloudPath, fillPaint);
                canvas.DrawPath(cloudPath, strokePaint);
            }
        }

        private void DrawDropIcon(SKCanvas canvas, float centerX, float centerY, float radius, SKPaint fillPaint, SKPaint strokePaint)
        {
            // Draw box/package shape
            var boxSize = radius * 1.2f;
            using var boxPath = new SKPath();
            
            // 3D box effect - front face
            boxPath.MoveTo(centerX - boxSize/2, centerY - boxSize/2);
            boxPath.LineTo(centerX + boxSize/2, centerY - boxSize/2);
            boxPath.LineTo(centerX + boxSize/2, centerY + boxSize/2);
            boxPath.LineTo(centerX - boxSize/2, centerY + boxSize/2);
            boxPath.Close();
            
            canvas.DrawPath(boxPath, fillPaint);
            canvas.DrawPath(boxPath, strokePaint);
            
            // Top face (3D effect)
            using var topPath = new SKPath();
            var offset = radius * 0.3f;
            topPath.MoveTo(centerX - boxSize/2, centerY - boxSize/2);
            topPath.LineTo(centerX - boxSize/2 + offset, centerY - boxSize/2 - offset);
            topPath.LineTo(centerX + boxSize/2 + offset, centerY - boxSize/2 - offset);
            topPath.LineTo(centerX + boxSize/2, centerY - boxSize/2);
            topPath.Close();
            
            using var lighterFill = new SKPaint
            {
                Color = new SKColor((byte)Math.Min(255, fillPaint.Color.Red + 40), 
                                  (byte)Math.Min(255, fillPaint.Color.Green + 40), 
                                  (byte)Math.Min(255, fillPaint.Color.Blue + 40)),
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };
            canvas.DrawPath(topPath, lighterFill);
            canvas.DrawPath(topPath, strokePaint);
            
            // Side face
            using var sidePath = new SKPath();
            sidePath.MoveTo(centerX + boxSize/2, centerY - boxSize/2);
            sidePath.LineTo(centerX + boxSize/2 + offset, centerY - boxSize/2 - offset);
            sidePath.LineTo(centerX + boxSize/2 + offset, centerY + boxSize/2 - offset);
            sidePath.LineTo(centerX + boxSize/2, centerY + boxSize/2);
            sidePath.Close();
            
            using var darkerFill = new SKPaint
            {
                Color = new SKColor((byte)Math.Max(0, fillPaint.Color.Red - 40), 
                                  (byte)Math.Max(0, fillPaint.Color.Green - 40), 
                                  (byte)Math.Max(0, fillPaint.Color.Blue - 40)),
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };
            canvas.DrawPath(sidePath, darkerFill);
            canvas.DrawPath(sidePath, strokePaint);
            
            // Fragile symbol (wine glass)
            using var symbolPaint = new SKPaint
            {
                Color = SKColors.Black,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = radius * 0.15f,
                IsAntialias = true
            };
            
            var symbolSize = radius * 0.4f;
            // Glass bowl
            canvas.DrawArc(new SKRect(centerX - symbolSize/2, centerY - symbolSize/3, 
                          centerX + symbolSize/2, centerY + symbolSize/3), 0, 180, false, symbolPaint);
            // Stem
            canvas.DrawLine(centerX, centerY, centerX, centerY + symbolSize/2, symbolPaint);
            // Base
            canvas.DrawLine(centerX - symbolSize/3, centerY + symbolSize/2, 
                           centerX + symbolSize/3, centerY + symbolSize/2, symbolPaint);
        }

        private void DrawPickupIcon(SKCanvas canvas, float centerX, float centerY, float radius, SKPaint fillPaint, SKPaint strokePaint)
        {
            // Draw hand/glove shape
            using var handPath = new SKPath();
            var handSize = radius * 1.1f;
            
            // Palm base
            handPath.MoveTo(centerX - handSize * 0.4f, centerY + handSize * 0.5f);
            handPath.LineTo(centerX + handSize * 0.4f, centerY + handSize * 0.5f);
            handPath.LineTo(centerX + handSize * 0.3f, centerY - handSize * 0.2f);
            handPath.LineTo(centerX - handSize * 0.3f, centerY - handSize * 0.2f);
            handPath.Close();
            
            canvas.DrawPath(handPath, fillPaint);
            canvas.DrawPath(handPath, strokePaint);
            
            // Fingers
            using var fingerPaint = new SKPaint
            {
                Color = fillPaint.Color,
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };
            
            // Thumb
            var thumbPath = new SKPath();
            thumbPath.MoveTo(centerX - handSize * 0.3f, centerY);
            thumbPath.LineTo(centerX - handSize * 0.6f, centerY - handSize * 0.1f);
            thumbPath.LineTo(centerX - handSize * 0.7f, centerY + handSize * 0.1f);
            thumbPath.LineTo(centerX - handSize * 0.4f, centerY + handSize * 0.2f);
            thumbPath.Close();
            
            canvas.DrawPath(thumbPath, fingerPaint);
            canvas.DrawPath(thumbPath, strokePaint);
            
            // Four fingers (simplified as rectangles)
            for (int i = 0; i < 4; i++)
            {
                var fingerX = centerX - handSize * 0.2f + (i * handSize * 0.13f);
                var fingerRect = new SKRect(fingerX, centerY - handSize * 0.6f, 
                                          fingerX + handSize * 0.1f, centerY - handSize * 0.2f);
                canvas.DrawRect(fingerRect, fingerPaint);
                canvas.DrawRect(fingerRect, strokePaint);
            }
            
            // Object being grabbed (small cube)
            using var objectPaint = new SKPaint
            {
                Color = new SKColor(255, 215, 0), // Gold
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };
            
            var objectSize = radius * 0.3f;
            var objectRect = new SKRect(centerX - objectSize/2, centerY - handSize * 0.9f,
                                       centerX + objectSize/2, centerY - handSize * 0.6f);
            canvas.DrawRect(objectRect, objectPaint);
            canvas.DrawRect(objectRect, strokePaint);
        }

        private SKColor GetGrenadeTypeColor(string? grenadeType)
        {
            if (string.IsNullOrEmpty(grenadeType)) return SKColors.Gray;

            return grenadeType.ToLower() switch
            {
                "smokegrenade" => new SKColor(200, 200, 200), // Light gray for smoke
                "flashbang" => new SKColor(255, 255, 0),      // Bright yellow for flash
                "hegrenade" => new SKColor(255, 0, 0),        // Bright red for HE
                "molotov" => new SKColor(255, 69, 0),         // Bright orange-red for molotov (highly visible)
                "incgrenade" => new SKColor(255, 140, 0),     // Dark orange for incendiary (highly visible)
                "decoy" => new SKColor(128, 0, 128),          // Purple for decoy
                _ => new SKColor(100, 100, 100)               // Default gray
            };
        }

        private List<SKColor> GetGrenadeColors(int grenadeCount)
        {
            var colors = new List<SKColor>
            {
                new SKColor(128, 128, 128), // Gray for smoke
                new SKColor(255, 255, 0),   // Yellow for flash
                new SKColor(255, 0, 0),     // Red for HE
                new SKColor(255, 165, 0),   // Orange for molotov
                new SKColor(0, 255, 0),     // Green for incendiary
                new SKColor(128, 0, 128),   // Purple for decoy
                new SKColor(0, 255, 255),   // Cyan
                new SKColor(255, 192, 203), // Pink
            };

            var result = new List<SKColor>();
            for (int i = 0; i < grenadeCount; i++)
            {
                result.Add(colors[i % colors.Count]);
            }
            return result;
        }

        private void DrawPlayerPoint(SKCanvas canvas, float x, float y, SKColor color, float radius)
        {
            // Draw filled circle
            using var fillPaint = new SKPaint
            {
                Color = color,
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };
            canvas.DrawCircle(x, y, radius, fillPaint);

            // Draw black outline for visibility
            using var strokePaint = new SKPaint
            {
                Color = SKColors.Black,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1,
                IsAntialias = true
            };
            canvas.DrawCircle(x, y, radius, strokePaint);
        }

        private void DrawCoordinateGrid(SKCanvas canvas, int imageWidth, int imageHeight)
        {
            using var textPaint = new SKPaint
            {
                Color = SKColors.LightGray,
                IsAntialias = true,
                TextSize = 20,
                Typeface = SKTypeface.FromFamilyName("Arial")
            };

            // Draw vertical grid labels (A-P)
            var letters = "ABCDEFGHIJKLMNOP".ToCharArray();
            var gridSpacingX = imageWidth / 16f;
            for (int i = 0; i < letters.Length; i++)
            {
                var x = i * gridSpacingX + gridSpacingX / 2;
                canvas.DrawText(letters[i].ToString(), x - 8, 25, textPaint); // Top
                canvas.DrawText(letters[i].ToString(), x - 8, imageHeight - 5, textPaint); // Bottom
            }

            // Draw horizontal grid labels (1-17)
            var gridSpacingY = imageHeight / 17f;
            for (int i = 1; i <= 17; i++)
            {
                var y = (i - 1) * gridSpacingY + gridSpacingY / 2;
                canvas.DrawText(i.ToString(), 5, y + 8, textPaint); // Left
                canvas.DrawText(i.ToString(), imageWidth - 25, y + 8, textPaint); // Right
            }
        }

        private void DrawLegend(SKCanvas canvas, List<string> playerNames, List<SKColor> colors, int imageWidth, int imageHeight, string title = "Players")
        {
            if (!playerNames.Any()) return;

            var legendX = imageWidth - 200;
            var legendY = 50;
            var legendItemHeight = 25;

            // Draw legend background
            using var backgroundPaint = new SKPaint
            {
                Color = new SKColor(0, 0, 0, 180), // Semi-transparent black
                Style = SKPaintStyle.Fill
            };
            
            var legendHeight = playerNames.Count * legendItemHeight + 20;
            canvas.DrawRect(legendX - 10, legendY - 10, 180, legendHeight, backgroundPaint);

            // Draw legend border
            using var borderPaint = new SKPaint
            {
                Color = SKColors.White,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1
            };
            canvas.DrawRect(legendX - 10, legendY - 10, 180, legendHeight, borderPaint);

            // Draw legend title
            using var titlePaint = new SKPaint
            {
                Color = SKColors.White,
                IsAntialias = true,
                TextSize = 16,
                Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)
            };
            canvas.DrawText(title, legendX, legendY, titlePaint);

            // Draw legend items
            using var textPaint = new SKPaint
            {
                Color = SKColors.White,
                IsAntialias = true,
                TextSize = 14,
                Typeface = SKTypeface.FromFamilyName("Arial")
            };

            for (int i = 0; i < playerNames.Count; i++)
            {
                var itemY = legendY + 20 + (i * legendItemHeight);
                
                // Draw color circle
                DrawPlayerPoint(canvas, legendX + 10, itemY, colors[i], 5);
                
                // Draw player name (truncate if too long)
                var playerName = playerNames[i];
                if (playerName.Length > 15)
                    playerName = playerName.Substring(0, 12) + "...";
                
                canvas.DrawText(playerName, legendX + 25, itemY + 5, textPaint);
            }
        }

        private void DrawGrenadeTypeLegend(SKCanvas canvas, List<LegendItem> legendItems, int imageWidth, int imageHeight)
        {
            if (!legendItems.Any()) return;

            var legendX = imageWidth - 220; // Slightly wider for grenade names
            var legendY = 50;
            var legendItemHeight = 30; // Taller for grenade shapes

            // Draw legend background
            using var backgroundPaint = new SKPaint
            {
                Color = new SKColor(0, 0, 0, 200), // More opaque for better visibility
                Style = SKPaintStyle.Fill
            };
            
            var legendHeight = legendItems.Count * legendItemHeight + 30;
            canvas.DrawRect(legendX - 10, legendY - 10, 200, legendHeight, backgroundPaint);

            // Draw legend border
            using var borderPaint = new SKPaint
            {
                Color = SKColors.White,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 2 // Thicker border
            };
            canvas.DrawRect(legendX - 10, legendY - 10, 200, legendHeight, borderPaint);

            // Draw legend title
            using var titlePaint = new SKPaint
            {
                Color = SKColors.White,
                IsAntialias = true,
                TextSize = 18, // Larger title
                Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)
            };
            canvas.DrawText("Grenades", legendX, legendY, titlePaint);

            // Draw legend items
            using var textPaint = new SKPaint
            {
                Color = SKColors.White,
                IsAntialias = true,
                TextSize = 16, // Larger text
                Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)
            };

            for (int i = 0; i < legendItems.Count; i++)
            {
                var item = legendItems[i];
                var itemY = legendY + 25 + (i * legendItemHeight);
                
                // Draw grenade shape based on type (determine from name)
                var grenadeType = GetGrenadeTypeFromFriendlyName(item.Name);
                DrawGrenadePoint(canvas, legendX + 15, itemY, item.Color, grenadeType, 6); // Larger radius for legend
                
                // Draw grenade name with bold text
                canvas.DrawText(item.Name, legendX + 35, itemY + 5, textPaint);
            }
        }

        private string? GetGrenadeTypeFromFriendlyName(string friendlyName)
        {
            return friendlyName switch
            {
                "Smoke" => "smokegrenade",
                "Flash" => "flashbang", 
                "HE Grenade" => "hegrenade",
                "Molotov" => "molotov",
                "Incendiary" => "incgrenade",
                "Decoy" => "decoy",
                _ => null
            };
        }

        private SKColor GetBombEventTypeColor(string? eventType)
        {
            if (string.IsNullOrEmpty(eventType)) return SKColors.Gray;

            return eventType.ToLower() switch
            {
                "begin_plant" => new SKColor(220, 20, 60),     // Crimson red for bomb plants
                "planted" => new SKColor(220, 20, 60),         // Crimson red for bomb plants
                "begin_defuse" => new SKColor(50, 205, 50),    // Lime green for defuses
                "defused" => new SKColor(50, 205, 50),         // Lime green for defuses
                "exploded" => new SKColor(255, 140, 0),        // Dark orange for explosions
                "dropped" => new SKColor(255, 215, 0),         // Gold for drops
                "pickup" => new SKColor(30, 144, 255),         // Dodger blue for pickups
                _ => new SKColor(128, 128, 128)                // Default gray
            };
        }

        private string GetFriendlyBombEventName(string? eventType)
        {
            if (string.IsNullOrEmpty(eventType)) return "Unknown";
            
            return eventType.ToLower() switch
            {
                "begin_plant" => "Begin Plant",
                "planted" => "Planted",
                "begin_defuse" => "Begin Defuse", 
                "defused" => "Defused",
                "exploded" => "Exploded",
                "dropped" => "Dropped",
                "pickup" => "Pickup",
                _ => eventType
            };
        }

        private void DrawBombEventPoint(SKCanvas canvas, float x, float y, SKColor color, string? eventType, float radius)
        {
            // Different unique shapes for different bomb events
            if (string.IsNullOrEmpty(eventType))
            {
                DrawPlayerPoint(canvas, x, y, color, radius);
                return;
            }

            using var fillPaint = new SKPaint
            {
                Color = color,
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };

            using var strokePaint = new SKPaint
            {
                Color = SKColors.Black,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 2,
                IsAntialias = true
            };

            switch (eventType.ToLower())
            {
                case "plant":
                    // Draw bomb icon (circle with fuse line)
                    DrawBombIcon(canvas, x, y, radius, fillPaint, strokePaint);
                    break;
                    
                case "defuse":
                    // Draw defuse kit icon (rectangle with tools)
                    DrawDefuseKitIcon(canvas, x, y, radius, fillPaint, strokePaint);
                    break;
                    
                case "explode":
                    // Draw explosion burst
                    DrawExplosionIcon(canvas, x, y, radius, fillPaint, strokePaint);
                    break;
                    
                case "drop":
                    // Draw package/box dropping icon
                    DrawDropIcon(canvas, x, y, radius, fillPaint, strokePaint);
                    break;
                    
                case "pickup":
                    // Draw hand grabbing icon
                    DrawPickupIcon(canvas, x, y, radius, fillPaint, strokePaint);
                    break;
                    
                default:
                    // Default to circle
                    canvas.DrawCircle(x, y, radius, fillPaint);
                    canvas.DrawCircle(x, y, radius, strokePaint);
                    break;
            }
        }

        private void DrawBombEventLegend(SKCanvas canvas, List<LegendItem> legendItems, int imageWidth, int imageHeight)
        {
            if (!legendItems.Any()) return;

            var legendX = imageWidth - 240; // Even wider for bomb event names
            var legendY = 50;
            var legendItemHeight = 35; // Taller for bomb shapes

            // Draw legend background
            using var backgroundPaint = new SKPaint
            {
                Color = new SKColor(0, 0, 0, 220), // Very opaque for bomb events
                Style = SKPaintStyle.Fill
            };
            
            var legendHeight = legendItems.Count * legendItemHeight + 35;
            canvas.DrawRect(legendX - 10, legendY - 10, 220, legendHeight, backgroundPaint);

            // Draw legend border
            using var borderPaint = new SKPaint
            {
                Color = SKColors.White,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 3 // Very thick border for visibility
            };
            canvas.DrawRect(legendX - 10, legendY - 10, 220, legendHeight, borderPaint);

            // Draw legend title
            using var titlePaint = new SKPaint
            {
                Color = SKColors.White,
                IsAntialias = true,
                TextSize = 20, // Large title
                Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)
            };
            canvas.DrawText("Bomb Events", legendX, legendY, titlePaint);

            // Draw legend items
            using var textPaint = new SKPaint
            {
                Color = SKColors.White,
                IsAntialias = true,
                TextSize = 18, // Large text
                Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)
            };

            for (int i = 0; i < legendItems.Count; i++)
            {
                var item = legendItems[i];
                var itemY = legendY + 30 + (i * legendItemHeight);
                
                // Draw bomb event shape
                var bombEventType = GetBombEventTypeFromFriendlyName(item.Name);
                DrawBombEventPoint(canvas, legendX + 20, itemY, item.Color, bombEventType, 8); // Large radius for legend
                
                // Draw bomb event name with bold text
                canvas.DrawText(item.Name, legendX + 45, itemY + 6, textPaint);
            }
        }

        private string? GetBombEventTypeFromFriendlyName(string friendlyName)
        {
            return friendlyName switch
            {
                "Begin Plant" => "begin_plant",
                "Planted" => "planted",
                "Begin Defuse" => "begin_defuse",
                "Defused" => "defused",
                "Exploded" => "exploded",
                "Dropped" => "dropped",
                "Pickup" => "pickup",
                _ => null
            };
        }

        private List<SKColor> GetPlayerColors(int playerCount)
        {
            var colors = new List<SKColor>
            {
                new SKColor(255, 0, 0),     // Red
                new SKColor(0, 255, 0),     // Green  
                new SKColor(0, 0, 255),     // Blue
                new SKColor(255, 255, 0),   // Yellow
                new SKColor(255, 0, 255),   // Magenta
                new SKColor(0, 255, 255),   // Cyan
                new SKColor(255, 165, 0),   // Orange
                new SKColor(128, 0, 128),   // Purple
                new SKColor(255, 192, 203), // Pink
                new SKColor(0, 128, 0),     // Dark Green
                new SKColor(128, 128, 128), // Gray
                new SKColor(165, 42, 42),   // Brown
                new SKColor(255, 20, 147),  // Deep Pink
                new SKColor(0, 191, 255),   // Deep Sky Blue
                new SKColor(50, 205, 50),   // Lime Green
                new SKColor(220, 20, 60),   // Crimson
            };

            // Return enough colors for all players, cycling if needed
            var result = new List<SKColor>();
            for (int i = 0; i < playerCount; i++)
            {
                result.Add(colors[i % colors.Count]);
            }
            return result;
        }


        private MapConfig? GetMapConfig(string mapName)
        {
            var mapConfigs = new Dictionary<string, MapConfig>
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

            if (mapConfigs.TryGetValue(mapName.ToLower(), out var config))
            {
                return config;
            }
            return null;
        }

        private float ConvertToImageX(float gameX, int imageWidth, MapConfig mapConfig)
        {
            // Convert from game coordinates to radar coordinates
            float normalized = (gameX - mapConfig.PosX) / mapConfig.Scale;
            // Convert to image coordinates (1024x1024 base)
            return normalized * imageWidth / 1024f;
        }

        private float ConvertToImageY(float gameY, int imageHeight, MapConfig mapConfig)
        {
            // Convert from game coordinates to radar coordinates, flip Y axis
            float normalized = (mapConfig.PosY - gameY) / mapConfig.Scale;
            // Convert to image coordinates (1024x1024 base)
            return normalized * imageHeight / 1024f;
        }
    }
}