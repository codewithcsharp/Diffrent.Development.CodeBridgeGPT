namespace CodeBridgeGPT.AI.Interfaces
{
    public interface IModelInspectorService
    {
        public bool ClassExists(string className);
        public List<string> GetClassProperties(string className);
    }
}
