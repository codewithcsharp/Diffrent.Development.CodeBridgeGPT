using System.ComponentModel.DataAnnotations;

namespace CodeBridgeGPT.AI.Models
{
    public class CodeBridgeGptRequestModel
    {
        [Required]
        [RegularExpression(@"^REQ-\d{6}$", ErrorMessage = "TaskId must be in the format 'REQ-' followed by 6 digits.")]
        public string TaskId { get; set; } = default!;
        
        [Required(ErrorMessage = "Prompt is required.")]
        [RegularExpression(@"^You are an AI-powered Developer Assistant. Based on the following User Story - Generate all necessary files required to scaffold and implement a complete logic-\A")]
        public string TaskExecutionPrompt { get; set; } = default!;

        [Required]
        [RegularExpression(@"^PROJ-\d{6}$", ErrorMessage = "ProjectId must be in the format 'PROJ-' followed by 6 digits.")]
        public string ProjectId { get; set; } = default!;

        [RegularExpression(@"^[A-Z][A-Za-z\.]*$", ErrorMessage = "Repository name must start with a capital letter and may contain letters and dots.")]
        public string RepositoryName { get; set; } = default!;

        [Required]
        public ContextModel Context { get; set; } = default!;
    }
}
