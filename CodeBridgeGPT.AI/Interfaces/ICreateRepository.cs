using CodeBridgeGPT.AI.Models;

namespace CodeBridgeGPT.AI.Interfaces
{
    public interface ICreateRepository
    {
        Task<string> CreateNewRepositoryAsync(string orgnisation, RepositoryModel repository, string token);
    }
}
