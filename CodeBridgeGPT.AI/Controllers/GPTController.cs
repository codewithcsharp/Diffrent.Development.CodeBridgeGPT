using CodeBridgeGPT.AI.Interfaces;
using CodeBridgeGPT.AI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.SemanticKernel;
using Octokit;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;

namespace CodeBridgeGPT.AI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GPTController : ControllerBase
    {
        private readonly IKernelService _kernelService;
        private readonly IConfiguration _configuration;
        private readonly IGitHubProcessor _gitHubService;

        public GPTController(IKernelService kernelService, IConfiguration configuration, IGitHubProcessor gitHubService)
        {
            _kernelService = kernelService;
            _configuration = configuration;
            _gitHubService = gitHubService;
        }

        [HttpPost("codebridgegpt-generate-code")]
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
        public async Task<IActionResult> PushMultipleFilesAsync([FromBody] GitHubProcessModel request)
        {
            try
            {
                var owner = _configuration["GitHub:Owner"];
                var repo = _configuration["GitHub:Repo"];

                var installationId = await _gitHubService.GenerateInstallationIdAsync(owner!);
                if (installationId == null)
                    return BadRequest("GitHub App is not installed for the specified owner.");

                var result = await _gitHubService.PushFilesToGitHubAsync(installationId.Value, repo!, owner!, request);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }
    }
}
