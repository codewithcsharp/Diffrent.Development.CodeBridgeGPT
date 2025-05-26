namespace CodeBridgeGPT.AI.Models
{
    public class AIResponseModel
    {
        public string ResponseId { get; set; } = default!;
        public string Timestamp { get; set; } = default!;
        public List<AIFileModel> Files { get; set; } = [];
    }
}
