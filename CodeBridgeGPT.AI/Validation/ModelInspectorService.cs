using CodeBridgeGPT.AI.Interfaces;

namespace CodeBridgeGPT.AI.Validation
{
    public class ModelInspectorService : IModelInspectorService
    {
        public bool ClassExists(string className)
        {
            return AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes()).Any(re => re.Name.Equals(className, StringComparison.OrdinalIgnoreCase));
        }

        public List<string> GetClassProperties(string className)
        {
            var properties = new List<string>();
            var type = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes())
                .FirstOrDefault(t => t.Name.Equals(className, StringComparison.OrdinalIgnoreCase));
            if (type != null)
            {
                properties = type.GetProperties().Select(p => p.Name).ToList();
            }
            return properties ?? [];
        }
    }
}
