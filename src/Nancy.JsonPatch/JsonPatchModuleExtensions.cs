namespace Nancy.JsonPatch
{
    using Extensions;
    using PropertyResolver;

    public static class JsonPatchModuleExtensions
    {
        public static JsonPatchResult JsonPatch<T>(this NancyModule module, T target)
        {
            return JsonPatch(module, target, new JsonPatchPropertyResolver());
        }

        public static JsonPatchResult JsonPatch<T>(this NancyModule module, T target,
            IJsonPatchPropertyResolver propertyResolver)
        {
            return new JsonPatchExecutor().Patch(module.Request.Body.AsString(), target, propertyResolver);
        }
    }
}
