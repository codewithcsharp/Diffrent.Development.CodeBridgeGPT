using CodeBridgeGPT.AI.Models;

namespace CodeBridgeGPT.AI.Interfaces
{
    public interface IGitHubProcessor
    {
        Task<long?> GenerateInstallationIdAsync(string owner);
        Task<string> GenerateInstallationTokenAsync(long installationId);
        Task<string> PushFilesToGitHubAsync(long installationId, string repoName, string owner, GitHubProcessModel files);
    }
}
