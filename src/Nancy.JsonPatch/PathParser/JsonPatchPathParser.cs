namespace Nancy.JsonPatch.PathParser
{
    using System;
    using System.Collections;
    using System.Linq;
    using System.Reflection;
    using Models;
    using PropertyResolver;

    // JSON Pointer parsing: http://tools.ietf.org/html/rfc6901

    internal class JsonPatchPathParser
    {
        private readonly IJsonPatchPropertyResolver _propertyResolver;

        public JsonPatchPathParser(IJsonPatchPropertyResolver propertyResolver)
        {
            _propertyResolver = propertyResolver;
        }

        public JsonPatchPathParserResult ParsePath<T>(string path, T target)
        {
            var pathObject = new JsonPatchPath();

            object targetObject = target;

            // Iterate the path finding the target object and property
            if (string.IsNullOrEmpty(path))
                return Failure("Could not parse the path \"\". Nancy.Json cannot modify the root of the object");

            if (!path.StartsWith("/"))
                return Failure("Could not parse the path \"" + path + "\". Path must start with a '/'");

            if (path.Equals("/"))
                return Failure("Could not parse the path \"/\". This path is not valid in Nancy.Json");

            // Split and remove the initial slash
            var pathSections = path.Split('/').Skip(1).ToArray();

            for (var i = 0; i < pathSections.Length-1; i++)
            {
                int ind;
                if (int.TryParse(pathSections[i], out ind))
                {
                    // This property refers to an item in the collection. Check we are acting on a collection
                    if (!IsCollectionType(targetObject))
                        return Failure("Could not access path '{0}' in target object. Parent object for '{1}' is not a collection", path, pathSections[i]);
                    

                    targetObject = ((IList)targetObject)[ind];
                }
                else
                {
                    // This is a normal property, rather than a collection
                    targetObject = GetValueOfProperty(targetObject, pathSections[i]);
                    if (targetObject == null)
                        return Failure("Could not access path '{0}' in target object. '{1}' is null", path, pathSections[i]);
                }
            }

            var lastProperty = pathSections.Last();
            
            int indexer;
            if (int.TryParse(lastProperty, out indexer) || lastProperty.Equals("-"))
            {
                // Last section of path is an indexer, return the parent object as the target
                pathObject.IsCollection = true;

                if (!IsCollectionType(targetObject))
                    return Failure( "Could not access path '{0}' in target object. Parent object for '{1}' is not a collection", path, lastProperty);

            }
            else
            {
                lastProperty = _propertyResolver.Resolve(targetObject.GetType(), lastProperty);

                // Property is not a collection, return 
                var targetProperty = targetObject.GetType().GetProperty(lastProperty);
                if (targetProperty == null)
                    return Failure("Could not find path '{0}' in target object", path);

                if (!targetProperty.CanWrite)
                    return Failure("Property '{0}' on target object cannot be set", path);
            }

            pathObject.TargetObject = targetObject;
            pathObject.TargetPropertyName = lastProperty;

            return new JsonPatchPathParserResult
            {
                Path = pathObject
            };
        }

        private PropertyInfo GetProperty(Type type, string propertyName)
        {
            var resolvedPropertyName = _propertyResolver.Resolve(type, propertyName);

            return type.GetProperty(resolvedPropertyName);
        }

        private object GetValueOfProperty<T>(T target, string propertyName)
        {
            var property = GetProperty(target.GetType(), propertyName);
            return property == null 
                ? null 
                : property.GetValue(target);
        }

        private bool IsCollectionType(object obj)
        {
            return obj is IEnumerable && !(obj is string);
        }

        private static JsonPatchPathParserResult Failure(string message, params object[] parameters)
        {
            return new JsonPatchPathParserResult
            {
                Error = string.Format(message, parameters)
            };
        }
    }

}