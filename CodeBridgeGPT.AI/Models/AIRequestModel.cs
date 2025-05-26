namespace CodeBridgeGPT.AI.Models
{
    public class AIRequestModel
    {
        public string TaskId { get; set; } = default!;
        public string Prompt { get; set; } = default!;
        public ContextModel Context { get; set; } = default!;
    }
}
