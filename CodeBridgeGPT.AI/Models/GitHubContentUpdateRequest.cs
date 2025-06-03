namespace CodeBridgeGPT.AI.Models
{
    public class GitHubContentUpdateRequest
    {
        public string FilePath { get; set; } = default!;
        public string Owner { get; set; } = default!;
        public string Repo { get; set; } = default!;
        public string Message { get; set; } = default!;
        public GitHubCommitter Committer { get; set; } = default!;
        public string Content { get; set; } = default!;
        public string Sha { get; set; } = default!;
    }
}
