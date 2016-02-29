namespace Nancy.JsonPatch.PathParser
{
    using System.Collections;
    using System.Linq;
    using Exceptions;
    using Models;

    // JSON Pointer parsing: http://tools.ietf.org/html/rfc6901

    internal class JsonPatchPathParser
    {
        public JsonPatchPath ParsePath<T>(string path, T target)
        {
            var pathObject = new JsonPatchPath();

            object targetObject = target;

            // Iterate the path finding the target object and property
            var pathSections = SplitPath(path);
            for (var i = 0; i < pathSections.Length-1; i++)
            {
                int ind;
                if (int.TryParse(pathSections[i], out ind))
                {
                    // This propery refers to an item in the collection. Check we are acting on a collection
                    if (!IsCollectionType(targetObject))
                        throw new JsonPatchPathException(string.Format("Could not access path '{0}' in target object. Parent object for '{1}' is not a collection", path, pathSections[i]));

                    targetObject = ((IList)targetObject)[ind];
                }
                else
                {
                    // This is a normal property, rather than a collection
                    targetObject = GetValueOfProperty(targetObject, pathSections[i]);
                    if (targetObject == null)
                        throw new JsonPatchPathException(string.Format("Could not access path '{0}' in target object. '{1}' is null", path, pathSections[i]));
                }
            }

            var lastProperty = pathSections.Last();
            
            int indexer;
            if (int.TryParse(lastProperty, out indexer) || lastProperty.Equals("-"))
            {
                // Last section of path is an indexer, return the parent object as the target
                pathObject.IsCollection = true;
                
                if (!IsCollectionType(targetObject))
                    throw new JsonPatchPathException(string.Format("Could not access path '{0}' in target object. Parent object for '{1}' is not a collection", path, lastProperty));

            }
            else
            {
                // Property is not a collection, return 
                var targetProperty = targetObject.GetType().GetProperty(lastProperty);
                if (targetProperty == null)
                    throw new JsonPatchPathException(string.Format("Could not find path '{0}' in target object", path));

                if (!targetProperty.CanWrite)
                    throw new JsonPatchPathException(string.Format("Property '{0}' on target object cannot be set",
                        path));
            }

            pathObject.TargetObject = targetObject;
            pathObject.TargetPropertyName = lastProperty;

            return pathObject;
        }

        private object GetValueOfProperty<T>(T target, string propertyName)
        {
            return target.GetType().GetProperty(propertyName).GetValue(target);
        }

        private bool IsCollectionType(object obj)
        {
            return obj is IEnumerable && !(obj is string);
        }

        private static string[] SplitPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new JsonPatchPathException("Could not parse the path \"\". Nancy.Json cannot modify the root of the object");

            if (!path.StartsWith("/"))
                throw new JsonPatchPathException("Could not parse the path \"" + path + "\". Path must start with a '/'");

            if (path.Equals("/"))
                throw new JsonPatchPathException("Could not parse the path \"/\". This path is not valid in Nancy.Json");

            // Split and remove the initial slash
            return path.Split('/').Skip(1).ToArray();
        }
    }

}