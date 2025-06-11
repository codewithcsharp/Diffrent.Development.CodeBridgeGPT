namespace CodeBridgeGPT.AI.Models
{
    public class GitHubContentUpdateRequest
    {
        public string ResponseId { get; set; } = default!;
        public string Owner { get; set; } = default!;
        public string Repo { get; set; } = default!;
        public GitHubCommitter Committer { get; set; } = default!;
        public List<GithubFileContentModel> Files { get; set; } = [];
    }
}
