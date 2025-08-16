namespace CS2DemoParserWeb.Services
{
    public interface IHeatmapService
    {
        Task<HeatmapData> GetPlayerPositionHeatmapAsync(HeatmapQuery query);
        Task<HeatmapData> GetDeathHeatmapAsync(HeatmapQuery query);
        Task<HeatmapData> GetUtilityHeatmapAsync(HeatmapQuery query);
        Task<HeatmapData> GetBombSiteHeatmapAsync(HeatmapQuery query);
        Task<HeatmapData> GetWeaponFireHeatmapAsync(HeatmapQuery query);
        Task<List<string>> GetAvailableMapsAsync();
        Task<List<string>> GetAvailablePlayersAsync(string? demoName = null);
        Task<List<string>> GetAvailablePlayersByDemoIdAsync(int demoId);
        Task<List<int>> GetAvailableRoundsByDemoIdAsync(int demoId);
        Task<string?> GetDemoSourceAsync(int demoId);
    }

    public class HeatmapQuery
    {
        public string? DemoName { get; set; }
        public int? DemoId { get; set; } // Filter by specific demo file ID
        public string? MapName { get; set; }
        public string? PlayerName { get; set; }
        public string? Team { get; set; } // "T", "CT", or null for both
        public int? RoundNumber { get; set; } // Filter by specific round number
        public List<int>? RoundNumbers { get; set; }
        public string? UtilityType { get; set; } // "hegrenade", "flashbang", "smokegrenade", "molotov"
        public string? DemoSource { get; set; } // "faceit", "esea", "matchmaking", etc.
        public int GridSize { get; set; } = 64; // Grid resolution for heatmap
        public bool NormalizeIntensity { get; set; } = true;
    }

    public class HeatmapData
    {
        public required string MapName { get; set; }
        public required string HeatmapType { get; set; }
        public List<HeatmapPoint> Points { get; set; } = new();
        public HeatmapBounds Bounds { get; set; } = new();
        public HeatmapMetadata Metadata { get; set; } = new();
    }

    public class HeatmapPoint
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float Intensity { get; set; } // Normalized 0-1 or raw count
        public int Count { get; set; } // Raw event count at this position
        public string? PlayerName { get; set; }
        public string? Team { get; set; }
        public string? EventType { get; set; }
    }

    public class HeatmapBounds
    {
        public float MinX { get; set; }
        public float MaxX { get; set; }
        public float MinY { get; set; }
        public float MaxY { get; set; }
        public float MinZ { get; set; }
        public float MaxZ { get; set; }
    }

    public class HeatmapMetadata
    {
        public int TotalEvents { get; set; }
        public int UniquePositions { get; set; }
        public float MaxIntensity { get; set; }
        public List<string> PlayersIncluded { get; set; } = new();
        public List<int> RoundsIncluded { get; set; } = new();
        public string? FilterDescription { get; set; }
    }
}