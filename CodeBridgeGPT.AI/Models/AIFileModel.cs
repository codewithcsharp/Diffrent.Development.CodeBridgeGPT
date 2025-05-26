namespace CodeBridgeGPT.AI.Models
{
    public class AIFileModel
    {
        public string FilePath { get; set; } = default!;
        public string FileType { get; set; } = default!;
        public string Action { get; set; } = default!;
        public string Content { get; set; } = default!;
    }
}
