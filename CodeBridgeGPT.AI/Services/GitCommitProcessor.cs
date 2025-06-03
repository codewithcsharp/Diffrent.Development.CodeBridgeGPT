using CodeBridgeGPT.AI.Interfaces;
using CodeBridgeGPT.AI.Models;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;

namespace CodeBridgeGPT.AI.Services
{
    public class GitCommitProcessor : IGitCommitProcessor
    {
        private readonly HttpClient _httpClient;
        private readonly string _token = "ghp_your_personal_access_token_here";

        public GitCommitProcessor(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("HttpClient", "1.0"));
        }

        public async Task<string> CreateOrUpdateFileAsync(GitHubContentUpdateRequest updateRequest)
        {
            string url = $"https://api.github.com/repos/{updateRequest.Owner}/{updateRequest.Repo}/contents/{updateRequest.FilePath}";

            var payload = new 
            {   
                message = updateRequest.Message,
                committer = new { name = updateRequest.Committer.Name, email = updateRequest.Committer.Email },
                content = Convert.ToBase64String(Encoding.UTF8.GetBytes(updateRequest.Content)),
                sha = string.IsNullOrEmpty(updateRequest.Sha) ? null : updateRequest.Sha
            };

            HttpRequestMessage httpRequest = new (HttpMethod.Put, url);
            httpRequest.Headers.UserAgent.ParseAdd("CSharp-App");
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "<your-github-token>");
            httpRequest.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(httpRequest);
            var responseBody = await response.Content.ReadAsStringAsync();
            response.EnsureSuccessStatusCode();

            return responseBody;
        }
    }
}
