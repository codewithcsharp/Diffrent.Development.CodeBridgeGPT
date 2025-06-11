using System.ComponentModel.DataAnnotations;

namespace CodeBridgeGPT.AI.Models
{
    public class ContextModel
    {
        [Required]
        public string Language { get; set; } = default!;

        [Required]
        public string Framework { get; set; } = default!;
        
        public List<string?> Tags { get; set; } = [];
        
        //public List<FilesToModify> FilesToModify { get; set; } = [];
        //public string BranchName { get; set; } = default!;
        //public string CommitMessage { get; set; } = default!;
    }
}
