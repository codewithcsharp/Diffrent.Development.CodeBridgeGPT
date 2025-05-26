using CodeBridgeGPT.AI.Models;
using Microsoft.SemanticKernel;

namespace CodeBridgeGPT.AI.Interfaces
{
    public interface IKernelService
    {
        Task<AIResponseModel> GenerateCodeFromPromptAsync(AIRequestModel request);
        Kernel GetKernel();
    }
}
