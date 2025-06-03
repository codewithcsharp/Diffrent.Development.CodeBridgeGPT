using CodeBridgeGPT.AI.Interfaces;
using CodeBridgeGPT.AI.Models;
using Microsoft.AspNetCore.Mvc;

namespace CodeBridgeGPT.AI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GPTController : ControllerBase
    {
        private readonly IKernelService _kernelService;
        private readonly IConfiguration _configuration;
        private readonly IGitHubProcessor _gitHubService;
        private readonly ICreateRepository _gitHubRepositoryService;
        private readonly IGitCommitProcessor _gitCommitProcessor;

        public GPTController(IKernelService kernelService, IConfiguration configuration, IGitHubProcessor gitHubService, ICreateRepository repository, IGitCommitProcessor gitCommitProcessor)
        {
            _kernelService = kernelService;
            _configuration = configuration;
            _gitHubService = gitHubService;
            _gitHubRepositoryService = repository;
            _gitCommitProcessor = gitCommitProcessor;
        }

        [HttpPost("codebridgegpt")]
        public async Task<IActionResult> GenerateCode([FromBody] AIRequestModel request)
        {
            try
            {
                var result = await _kernelService.GenerateCodeFromPromptAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpPost("githubapp-push")]
        public async Task<IActionResult> GitRepositoryPushFilesAsync([FromBody] GitHubProcessModel request)
        {
            try
            {
                var loginuser = _configuration["GitHub:Owner"];
                var repository = _configuration["GitHub:Repo"];
                if (string.IsNullOrEmpty(loginuser) || string.IsNullOrEmpty(repository))
                {
                    return BadRequest("GitHub owner or repository is not configured.");
                }
                long? installationId = await _gitHubService.GenerateInstallationIdAsync(loginuser);
                
                if (installationId == null)
                    return BadRequest("GitHub App is not installed for the specified owner.");

                var result = await _gitHubService.CodeFilesPushGitHubAsync(installationId.Value, repository, loginuser, request);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpPut("commit-changes")]
        public async Task<IActionResult> ProcessGitCommitAsync([FromBody] GitHubContentUpdateRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrEmpty(request.Owner) || string.IsNullOrEmpty(request.Repo))
                {
                    return BadRequest("Invalid request data.");
                }
                var result = await _gitCommitProcessor.CreateOrUpdateFileAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpPost("createrepo")]
        public async Task<IActionResult> CreateRepo(string orgnisation, [FromBody] RepositoryModel request)
        {
            try
            {
                if (!Request.Headers.TryGetValue("Authorization", out var authHeader) || string.IsNullOrWhiteSpace(authHeader))
                    return Unauthorized("Authorization header is missing");

                var token = authHeader.ToString().Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase);
                var result = await _gitHubRepositoryService.CreateNewRepositoryAsync(orgnisation, request, token);
                return Content(result, "application/json");
            }
            catch (HttpRequestException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
