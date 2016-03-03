namespace Nancy.JsonPatch
{
    public class JsonPatchResult
    {
        public bool Succeeded { get; set; }
        public JsonPatchFailureReason FailureReason { get; set; }
        public string Message { get; set; }

        public static implicit operator bool(JsonPatchResult result)
        {
            return result.Succeeded;
        }
    }
}
