using CodeBridgeGPT.AI.Interfaces;
using CodeBridgeGPT.AI.Models;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace CodeBridgeGPT.AI.Services
{
    public class CreateRepositoryService(IHttpClientFactory httpClient) : ICreateRepository
    {
        private readonly HttpClient _httpClient = httpClient.CreateClient();

        public async Task<string> CreateNewRepositoryAsync(string orgnisation, RepositoryModel request, string token)
        {
            string url = $"https://api.github.com/orgs/{orgnisation}/repos";

            var jsonContent = JsonConvert.SerializeObject(request);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("gptbot.ai/1.0");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.PostAsync(url, content);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to create repo: {response.StatusCode} - {error}");
            }

            return await response.Content.ReadAsStringAsync();
        }
    }
}
