using CodeBridgeGPT.AI.Interfaces;
using CodeBridgeGPT.AI.Models;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Newtonsoft.Json;
using System.Text;

namespace CodeBridgeGPT.AI.Services
{
    public class KernelService : IKernelService
    {
        private readonly Kernel _kernel;
        private readonly IChatCompletionService _chatService;

        public KernelService(IConfiguration configuration)
        {
            var apiKey = configuration["KernelSettings:ApiKey"];
            var model = configuration["KernelSettings:Model"] ?? "gpt-3.5-turbo";

            if (string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException(nameof(apiKey));
            if (string.IsNullOrWhiteSpace(model))
                throw new ArgumentNullException(nameof(model));

            var builder = Kernel.CreateBuilder();
            builder.AddOpenAIChatCompletion(model, apiKey);
            _kernel = builder.Build();

            _chatService = _kernel.GetRequiredService<IChatCompletionService>();
        }

        public Kernel GetKernel() => _kernel;

        public async Task<AIResponseModel> GenerateCodeFromPromptAsync(AIRequestModel request)
        {
            var chat = new ChatHistory();
            chat.AddUserMessage(BuildPromptFromRequest(request));

            var result = await _chatService.GetChatMessageContentAsync(chat);

            if (result == null || string.IsNullOrWhiteSpace(result.Content))
                throw new InvalidOperationException("AI bot returned an empty response.");

            var response = JsonConvert.DeserializeObject<AIResponseModel>(result.Content!);

            if (response == null)
                throw new InvalidOperationException("Failed to parse AI response as JSON.");

            return response;
        }

        private static string BuildPromptFromRequest(AIRequestModel request)
        {
            var sb = new StringBuilder();
            sb.AppendLine(request.Prompt);
            sb.AppendLine();
            sb.AppendLine("Context:");
            sb.AppendLine(JsonConvert.SerializeObject(request.Context, Formatting.Indented));
            return sb.ToString();
        }
    }
}
