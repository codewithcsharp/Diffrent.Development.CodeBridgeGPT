using CodeBridgeGPT.AI.Interfaces;
using CodeBridgeGPT.AI.Models;
using Microsoft.IdentityModel.Tokens;
using Octokit;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
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

        public async Task<string> CodeFilesPushGitHubAsync(long installationId, string repository, string loginuser, GitHubProcessModel files)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("CodebridgeGPT", "1.0"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GenerateInstallationTokenAsync(installationId));

            // Use the first file's branch (assuming all files are for the same branch)
            var branch = files.Files.FirstOrDefault()?.Branch ?? "master";

            // Step 1: Get the reference for the branch
            var refResponse = await client.GetAsync($"https://api.github.com/repos/{loginuser}/{repository}/git/refs/heads/{branch}");
            if (!refResponse.IsSuccessStatusCode)
            {
                var error = await refResponse.Content.ReadAsStringAsync();
                throw new Exception($"Failed to get ref: {error}");
            }

            var refContent = await refResponse.Content.ReadAsStringAsync();
            var latestCommitSha = JsonDocument.Parse(refContent).RootElement.GetProperty("object").GetProperty("sha").GetString();

            // Step 2: Get the base tree SHA from the latest commit
            var commitResponse = await client.GetAsync($"https://api.github.com/repos/{loginuser}/{repository}/git/commits/{latestCommitSha}");
            var commitContent = await commitResponse.Content.ReadAsStringAsync();
            var baseTreeSha = JsonDocument.Parse(commitContent).RootElement.GetProperty("tree").GetProperty("sha").GetString();

            // Step 3: Create blobs and collect tree entries
            var treeEntries = new List<object>();
            foreach (var file in files.Files)
            {
                var contentBytes = Encoding.UTF8.GetBytes(file.Content);
                var base64Content = Convert.ToBase64String(contentBytes);
                var blobPayload = new
                {
                    content = file.Content,
                    encoding = "utf-8"
                };

                var blobResponse = await client.PostAsync(
                    $"https://api.github.com/repos/{loginuser}/{repository}/git/blobs",
                    new StringContent(JsonSerializer.Serialize(blobPayload), Encoding.UTF8, "application/json")
                );

                blobResponse.EnsureSuccessStatusCode();
                var blobResult = await blobResponse.Content.ReadAsStringAsync();
                var blobSha = JsonDocument.Parse(blobResult).RootElement.GetProperty("sha").GetString();

                treeEntries.Add(new
                {
                    path = file.FilePath,
                    mode = "100644",
                    type = "blob",
                    sha = blobSha
                });
            }

            // Step 4: Create a new tree from collected blobs
            var treePayload = new
            {
                base_tree = baseTreeSha,
                tree = treeEntries
            };

            var treeCreateResponse = await client.PostAsync(
                $"https://api.github.com/repos/{loginuser}/{repository}/git/trees",
                new StringContent(JsonSerializer.Serialize(treePayload), Encoding.UTF8, "application/json")
            );

            var treeCreateContent = await treeCreateResponse.Content.ReadAsStringAsync();
            var newTreeSha = JsonDocument.Parse(treeCreateContent).RootElement.GetProperty("sha").GetString();

            // Step 5: Create a commit with this new tree
            var commitPayload = new
            {
                message = files.Files.First().CommitMessage ?? "Updated files via API",
                tree = newTreeSha,
                parents = new[] { latestCommitSha }
            };

            var newCommitResponse = await client.PostAsync(
                $"https://api.github.com/repos/{loginuser}/{repository}/git/commits",
                new StringContent(JsonSerializer.Serialize(commitPayload), Encoding.UTF8, "application/json")
            );

            var commitResult = await newCommitResponse.Content.ReadAsStringAsync();
            var newCommitSha = JsonDocument.Parse(commitResult).RootElement.GetProperty("sha").GetString();

            // Step 6: Update the branch reference to point to new commit
            var updateRefPayload = new
            {
                sha = newCommitSha,
                force = true
            };

            var updateRefResponse = await client.PatchAsync(
                $"https://api.github.com/repos/{loginuser}/{repository}/git/refs/heads/{branch}",
                new StringContent(JsonSerializer.Serialize(updateRefPayload), Encoding.UTF8, "application/json")
            );

            updateRefResponse.EnsureSuccessStatusCode();

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