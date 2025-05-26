using System.ComponentModel.DataAnnotations;

namespace CodeBridgeGPT.AI.Models
{
    public class GitHubProcessModel
    {
        public string ResponseId { get; set; } = default!;
        public DateTime Timestamp { get; set; } = DateTime.MinValue;
        public List<ProcessRequestModel> Files { get; set; } = [];
        //public string FilePath { get; set; } = default!;
        //public string FileType { get; set; } = default!;
        //public string Action { get; set; } = default!;
        //public string Content { get; set; } = default!;
    }
}
