using CodeBridgeGPT.AI.Models;
using Microsoft.SemanticKernel;

namespace CodeBridgeGPT.AI.Interfaces
{
    public interface IKernelService
    {
        Task<CodeBridgeGptResponseModel> GenerateCodeFromPromptAsync(CodeBridgeGptRequestModel request);
        Kernel GetKernel();
    }
}
