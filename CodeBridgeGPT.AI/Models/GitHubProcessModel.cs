namespace CodeBridgeGPT.AI.Models
{
    public class GitHubProcessModel
    {
        public string ResponseId { get; set; } = default!;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public List<ProcessRequestModel> Files { get; set; } = [];
    }
}
