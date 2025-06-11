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
        public async Task<IActionResult> GenerateCode([FromBody] CodeBridgeGptRequestModel request)
        {
            try
            {
                if(!ModelState.IsValid)
                {
                    List<string> errors = ModelState.Where(ms => ms.Value?.Errors.Count > 0).SelectMany(ms => ms.Value!.Errors).Select(e => e.ErrorMessage).ToList();

                    return BadRequest(new { Message = "Invalid request", Errors = errors });
                }
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

        [HttpPut("commits")]
        public async Task<IActionResult> ProcessGitCommitAsync([FromBody] GitHubContentUpdateRequest request)
        {
            try
            {
                // Check for Authorization header
                if (!Request.Headers.TryGetValue("Authorization", out var authHeader) || string.IsNullOrWhiteSpace(authHeader))
                    return Unauthorized("Authorization header is missing");

                var token = authHeader.ToString().Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase);

                // Optional: Validate token format or use it in downstream service (_gitCommitProcessor)

                // Validate request body
                if (request == null)
                    return BadRequest("Request body is missing.");

                if (string.IsNullOrWhiteSpace(request.Owner) || string.IsNullOrWhiteSpace(request.Repo))
                    return BadRequest("Owner and repository name must be provided.");

                // Process commit
                var result = await _gitCommitProcessor.CreateOrUpdateFileAsync(request, token);

                return Ok(result);
            }
            catch (Exception ex)
            {
                // Optional: Log exception here
                return StatusCode(500, $"Internal server error: {ex.Message}");
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
