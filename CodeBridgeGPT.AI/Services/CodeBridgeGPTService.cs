using CodeBridgeGPT.AI.Interfaces;
using CodeBridgeGPT.AI.Models;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Newtonsoft.Json;
using System.Text;

namespace CodeBridgeGPT.AI.Services
{
    public class CodeBridgeGPTService : IKernelService
    {
        private readonly Kernel _kernel;
        private readonly IChatCompletionService _chatService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IGitCommitProcessor _gitCommitProcessor;
        private readonly IPromptValidator _inspectorService;

        public CodeBridgeGPTService(IConfiguration configuration, IHttpClientFactory httpClientFactory, IGitCommitProcessor gitCommitProcessor, IPromptValidator inspectorService)
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
            _httpClientFactory = httpClientFactory;
            _gitCommitProcessor = gitCommitProcessor;
            _inspectorService = inspectorService;
        }

        public Kernel GetKernel() => _kernel;

        public async Task<CodeBridgeGptResponseModel> GenerateCodeFromPromptAsync(CodeBridgeGptRequestModel request)
        {
            var promptError = _inspectorService.ValidateStringPrompt(request.TaskExecutionPrompt);
            if (promptError.Count != 0) throw new Exception($"TaskExecutionPrompt is invaild in validation test");
            var chat = new ChatHistory();
            chat.AddUserMessage(BuildPromptFromRequest(request));

            var result = await _chatService.GetChatMessageContentAsync(chat);

            if (result == null || string.IsNullOrWhiteSpace(result.Content))
                throw new InvalidOperationException("AI bot returned an empty response.");

            var response = JsonConvert.DeserializeObject<CodeBridgeGptResponseModel>(result.Content);

            if(string.IsNullOrWhiteSpace(response!.TaskResponseId)) throw new InvalidOperationException("Not a valid response, must contain a TaskResponseId.");

            // if response is success - call commit changes api call to GitHub
            if (response != null)
            {
                GitHubCommitter gitHubCommitter = new()
                {
                    Name = "Abhitosh Kumar",
                    Email = "kumarabhitosh678@gmail.com"
                };

                // For example, you could call a service that handles GitHub commits
                var req = MapGptResponseToGitHubRequest(response, "codebridge-gpt", request.RepositoryName, "master", gitHubCommitter);
                await _gitCommitProcessor.CreateOrUpdateFileAsync(req, "");

            }

            return response ?? throw new InvalidOperationException("Failed to parse AI response as JSON.");
        }

        private static string BuildPromptFromRequest(CodeBridgeGptRequestModel request)
        {
            var sb = new StringBuilder();
            sb.AppendLine(request.TaskExecutionPrompt);
            sb.AppendLine();
            sb.AppendLine("Context:");
            sb.AppendLine(JsonConvert.SerializeObject(request.Context, Formatting.Indented));
            return sb.ToString();
        }

        private static GitHubContentUpdateRequest MapGptResponseToGitHubRequest(CodeBridgeGptResponseModel gptresponse, string owner, string repo, string branch, GitHubCommitter committer)
        {
            //string url = $"https://api.github.com/repos/{owner}/{repo}/contents";
            var commitPayload = new GitHubContentUpdateRequest
            {
                //ResponseId = gptresponse.TaskResponseId,
                Owner = owner,
                Repo = repo,
                Committer = committer,
                Files = []
            };

            foreach (var file in gptresponse.Files)
            {
                commitPayload.Files.Add(new GithubFileContentModel
                {
                    FilePath = file.FilePath,
                    Message = $"Auto commit message: { gptresponse.TaskResponseId }",
                    Content = file.Content,
                    Sha = "" // to be filled after fetching from GitHub if needed
                });
            }

            return commitPayload;
        }

    }
}
