using CodeBridgeGPT.AI.Models;

namespace CodeBridgeGPT.AI.Interfaces
{
    public interface IGitCommitProcessor
    {
        Task<string> CreateOrUpdateFileAsync(GitHubContentUpdateRequest request, string token);
    }
}
