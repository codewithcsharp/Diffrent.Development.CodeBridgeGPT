namespace CodeBridgeGPT.AI.Models
{
    public class GithubFileContentModel
    {
        public string FilePath { get; set; } = default!;
        public string Content { get; set; } = default!;
        public string Message { get; set; } = default!;
        public string Sha { get; set; } = default!;

    }
}
