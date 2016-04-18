using System;

namespace Nancy.JsonPatch.PropertyResolver
{
    public interface IJsonPatchPropertyResolver
    {
        string Resolve(Type type, string propertyName);
    }
}
