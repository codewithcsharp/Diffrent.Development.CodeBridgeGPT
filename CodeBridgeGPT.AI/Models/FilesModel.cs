namespace CodeBridgeGPT.AI.Models
{
    public class FilesModel
    {
        public string FilePath { get; set; } = default!;
        public string Content { get; set; } = default!;
        public string Action { get; set; } = default!;
        public string Message { get; set; } = default!;

        //public bool InsertAfter { get; set; } = true;
    }
}
