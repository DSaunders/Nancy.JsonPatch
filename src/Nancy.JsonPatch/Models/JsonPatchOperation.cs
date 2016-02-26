namespace Nancy.JsonPatch.Models
{
    internal class JsonPatchOperation
    {
        public JsonPatchOpCode Op { get; set; }
        public string Path { get; set; }
        public object Value { get; set; }
    }
}