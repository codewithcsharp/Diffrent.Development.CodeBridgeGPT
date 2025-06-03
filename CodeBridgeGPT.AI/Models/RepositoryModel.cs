using Newtonsoft.Json;

namespace CodeBridgeGPT.AI.Models
{
    public class RepositoryModel
    {
        [JsonProperty("name")]
        public string Name { get; set; } = default!;
        
        [JsonProperty("description")]
        public string Description { get; set; } = default!;

        [JsonProperty("homepage")]
        public string Homepage { get; set; } = default!;
        
        [JsonProperty("private")]
        public bool Private { get; set; } = false;

        [JsonProperty("has_issues")]
        public bool HasIssues { get; set; } = true;

        [JsonProperty("has_projects")]
        public bool HasProjects { get; set; } = true;

        [JsonProperty("has_wiki")]
        public bool HasWiki { get; set; } = true;
        
        [JsonProperty("autoInit")]
        public bool AutoInit { get; set; } = true;
    }
}
