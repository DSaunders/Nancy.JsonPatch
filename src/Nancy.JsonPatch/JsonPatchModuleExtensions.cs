namespace Nancy.JsonPatch
{
    using Extensions;

    public static class JsonPatchModuleExtensions
    {
        public static JsonPatchResult JsonPatch<T>(this NancyModule module, T target)
        {
            return new JsonPatchExecutor().Patch(module.Request.Body.AsString(),target);
        }
    }
}
