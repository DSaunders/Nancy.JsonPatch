namespace Nancy.JsonPatch
{
    public enum JsonPatchFailureReason
    {
        TestFailed,
        CouldNotParseJson,
        CouldNotParsePath,
        CouldNotParseFrom,
        OperationFailed
    }
}