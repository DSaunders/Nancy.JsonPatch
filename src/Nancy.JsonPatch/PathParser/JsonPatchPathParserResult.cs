namespace Nancy.JsonPatch.PathParser
{
    using Models;

    internal class JsonPatchPathParserResult
    {
        public JsonPatchPath Path { get; set; }
        public string Error { get; set; }
    }
}