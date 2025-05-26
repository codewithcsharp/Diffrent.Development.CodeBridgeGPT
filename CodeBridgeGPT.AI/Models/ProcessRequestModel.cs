using System.ComponentModel.DataAnnotations;

namespace CodeBridgeGPT.AI.Models
{
    public class ProcessRequestModel
    {


        public string FilePath { get; set; } = default!;
        public string FileType { get; set; } = default!;
        public string Action { get; set; } = default!;
        public string Content { get; set; } = default!;
        public string CommitMessage { get; set; } = default!;
        public string? Branch { get; set; } = "master";
    }
}
