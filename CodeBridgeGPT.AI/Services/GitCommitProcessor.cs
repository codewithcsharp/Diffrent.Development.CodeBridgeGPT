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
        //private readonly string _token = "ghp_your_personal_access_token_here";

        public GitCommitProcessor(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
            //_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("HttpClient", "1.0"));
        }

        public async Task<string> CreateOrUpdateFileAsync(GitHubContentUpdateRequest updateRequest, string token)
        {
            string responseBody = string.Empty;

            foreach (var file in updateRequest.Files)
            {
                if (string.IsNullOrWhiteSpace(file.Message))
                    throw new ArgumentException("Commit message is required for each file.");

                string cleanFilePath = file.FilePath.Replace("\\", "/").TrimStart('/');
                string url = $"https://api.github.com/repos/{updateRequest.Owner}/{updateRequest.Repo}/contents/{cleanFilePath}";

                // Step 1: Try to get existing file details (to get sha)
                string sha = file.Sha;
                if (string.IsNullOrWhiteSpace(sha))
                {
                    var getRequest = new HttpRequestMessage(HttpMethod.Get, url);
                    getRequest.Headers.UserAgent.ParseAdd("gpt.ai/1.0");
                    getRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    var getResponse = await _httpClient.SendAsync(getRequest);

                    if (getResponse.IsSuccessStatusCode)
                    {
                        var getContent = await getResponse.Content.ReadAsStringAsync();
                        using var doc = JsonDocument.Parse(getContent);
                        if (doc.RootElement.TryGetProperty("sha", out var shaElement))
                        {
                            sha = shaElement.GetString();
                        }
                    }
                    // If 404 (file doesn't exist), then it's a new file — sha remains null
                }

                // Step 2: Prepare payload
                var payload = new Dictionary<string, object?>
                {
                    ["message"] = file.Message,
                    ["committer"] = new { name = updateRequest.Committer.Name, email = updateRequest.Committer.Email },
                    ["content"] = Convert.ToBase64String(Encoding.UTF8.GetBytes(file.Content))
                };

                if (!string.IsNullOrWhiteSpace(sha))
                    payload["sha"] = sha;

                // Step 3: Send PUT request to create or update the file
                var putRequest = new HttpRequestMessage(HttpMethod.Put, url);
                putRequest.Headers.UserAgent.ParseAdd("gpt.ai/1.0");
                putRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                putRequest.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

                var putResponse = await _httpClient.SendAsync(putRequest);
                responseBody = await putResponse.Content.ReadAsStringAsync();

                if (!putResponse.IsSuccessStatusCode)
                {
                    // Optional: throw detailed error
                    throw new HttpRequestException($"GitHub API error: {putResponse.StatusCode}\n{responseBody}");
                }
            }

            return responseBody;
        }

    }
}
