using Newtonsoft.Json;

namespace CodeBridgeGPT.AI.Models
{
    public class CodeBridgeGptResponseModel
    {
        [JsonProperty("responseId")]
        public string TaskResponseId { get; set; } = default!;
        public string Timestamp { get; set; } = default!;
        public List<FilesModel> Files { get; set; } = [];
    }
}
