using CodeBridgeGPT.AI.Interfaces;
using CodeBridgeGPT.AI.Models;
using Microsoft.IdentityModel.Tokens;
using Octokit;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ProductHeaderValue = Octokit.ProductHeaderValue;

namespace CodeBridgeGPT.AI.Services
{
    public class GitHubProcessorService : IGitHubProcessor
    {
        private readonly IConfiguration _configuration;

        public GitHubProcessorService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<long?> GenerateInstallationIdAsync(string owner)
        {
            var jwtToken = GenerateJwtToken();
            var appClient = new GitHubClient(new ProductHeaderValue("CodeBridgeApp"))
            {
                Credentials = new Credentials(jwtToken, AuthenticationType.Bearer)
            };

            var installations = await appClient.GitHubApps.GetAllInstallationsForCurrent();
            var installation = installations.FirstOrDefault(i =>
                i.Account?.Login?.Equals(owner, StringComparison.OrdinalIgnoreCase) == true);

            return installation?.Id;
        }

        public async Task<string> GenerateInstallationTokenAsync(long installationId)
        {
            var jwtToken = GenerateJwtToken();
            var appClient = new GitHubClient(new ProductHeaderValue("CodeBridgeApp"))
            {
                Credentials = new Credentials(jwtToken, AuthenticationType.Bearer)
            };

            var token = await appClient.GitHubApps.CreateInstallationToken(installationId);
            return token.Token;
        }

        public async Task<string> PushFilesToGitHubAsync(long installationId, string repoName, string owner, GitHubProcessModel files)
        {
            var token = await GenerateInstallationTokenAsync(installationId);
            var github = new GitHubClient(new ProductHeaderValue("CodeBridgeApp"))
            {
                Credentials = new Credentials(token)
            };

            foreach (var file in files.Files)
            {
                var branch = file.Branch ?? "master";
                var encodedContent = Convert.ToBase64String(Encoding.UTF8.GetBytes(file.Content));

                try
                {
                    // Try to get existing file content to update
                    var existingContent = await github.Repository.Content.GetAllContentsByRef(owner, repoName, file.FilePath, branch);
                    var updateRequest = new UpdateFileRequest(file.CommitMessage, file.Content, existingContent.First().Sha)
                    {
                        Branch = branch
                    };

                    await github.Repository.Content.UpdateFile(owner, repoName, file.FilePath, updateRequest);
                }
                catch (NotFoundException)
                {
                    // File does not exist, create it
                    var createRequest = new CreateFileRequest(file.CommitMessage, file.Content)
                    {
                        Branch = branch
                    };

                    await github.Repository.Content.CreateFile(owner, repoName, file.FilePath, createRequest);
                }
            }

            return "Files pushed successfully.";
        }

        private string GenerateJwtToken()
        {
            var rsa = RSA.Create();
            var pemFilePath = _configuration["GitHub:PemFilePath"] ?? throw new Exception("Pem file not found");
            rsa.ImportFromPem(File.ReadAllText(pemFilePath));

            var securityKey = new RsaSecurityKey(rsa);
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.RsaSha256);
            var now = DateTime.UtcNow;

            var token = new JwtSecurityToken(
                issuer: _configuration["GitHub:AppId"],
                claims: [new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)],
                notBefore: now,
                expires: now.AddMinutes(10),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}