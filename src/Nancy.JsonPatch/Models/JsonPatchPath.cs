namespace Nancy.JsonPatch.Models
{
    internal class JsonPatchPath
    {
        public bool IsCollection { get; set; }
        public object TargetObject { get; set; }
        public string TargetPropertyName { get; set; }
    }
}