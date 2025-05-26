namespace CodeBridgeGPT.AI.Models
{
    public class ContextModel
    {
        public string Language { get; set; } = default!;
        public string Framework { get; set; } = default!;
        public string ResponseFormat { get; set; } = default!;
        public string RepoPath { get; set; } = default!;
        public List<string> Tags { get; set; } = [];
    }
}
