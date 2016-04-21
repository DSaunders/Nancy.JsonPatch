using System;

namespace Nancy.JsonPatch.PropertyResolver
{
    public class JsonPatchPropertyResolver : IJsonPatchPropertyResolver
    {
        public string Resolve(Type type, string propertyName)
        {
            return propertyName;
        }
    }
}
