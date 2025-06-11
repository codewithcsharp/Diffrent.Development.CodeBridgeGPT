namespace CodeBridgeGPT.AI.Interfaces
{
    public interface IPromptValidator
    {
        public List<string> ValidateStringPrompt(string prompt);
    }
}
