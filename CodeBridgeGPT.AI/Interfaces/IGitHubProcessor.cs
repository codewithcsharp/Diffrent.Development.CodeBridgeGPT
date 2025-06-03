using CodeBridgeGPT.AI.Models;

namespace CodeBridgeGPT.AI.Interfaces
{
    public interface IGitHubProcessor
    {
        Task<string> CodeFilesPushGitHubAsync(long installationId, string repository, string loginuser, GitHubProcessModel files);
        Task<string> GenerateInstallationTokenAsync(long installationId);
        Task<long?> GenerateInstallationIdAsync(string owner);
    }
}
