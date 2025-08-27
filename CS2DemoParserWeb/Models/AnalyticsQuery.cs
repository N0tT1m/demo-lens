namespace CS2DemoParserWeb.Models
{
    public class AnalyticsQuery
    {
        public int? DemoId { get; set; }
        public string? MapName { get; set; }
        public string? PlayerName { get; set; }
        public string? Team { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Format { get; set; }
        public int? RoundNumber { get; set; }
    }
}