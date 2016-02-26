namespace Nancy.JsonPatch.PathParser
{
    using System.Collections;
    using System.Linq;
    using Exceptions;
    using Models;

    // JSON Pointer parsing: http://tools.ietf.org/html/rfc6901

    internal class JsonPatchPathParser
    {
        // TODO: Refactor this
        public JsonPatchPath GetThing<T>(string path, T target)
        {
            var pathObject = new JsonPatchPath();

            // Remove initial slash
            var trimmedPath = path.TrimStart('/');
            var pathSections = trimmedPath.Split('/').ToArray();

            if (pathSections.Length <= 0)
                return null;

            object parentObject = null;
            object targetObject = target;
            string parentPropertyName = null;
            string targetPropertyName = null;

            foreach (var pathSection in pathSections)
            {
                if (targetPropertyName != null)
                {
                    parentObject = targetObject;

                    var ind = 0;
                    if (int.TryParse(targetPropertyName, out ind))
                    {
                        // This is an array indexer
                        // Get the item in the array and carry on
                        if (!IsCollectionType(parentObject))
                            throw new JsonPatchProcessException(string.Format("Could not access path '{0}' in target object. '{1}' is not a collection", path, parentPropertyName));

                        targetObject = ((IList) parentObject)[ind];
                    }
                    else
                    {
                        targetObject = targetObject.GetType().GetProperty(targetPropertyName).GetValue(targetObject);
                        if (targetObject == null)
                            throw new JsonPatchProcessException(
                                string.Format("Could not access path '{0}' in target object. '{1}' is null", path,
                                    targetPropertyName));
                    }
                }

                parentPropertyName = targetPropertyName;
                targetPropertyName = pathSection;
            }

            // Get propery of item
            var indexer = 0;
            if (int.TryParse(targetPropertyName, out indexer))
            {
                pathObject.IsCollection = true;
                targetObject = parentObject.GetType().GetProperty(parentPropertyName).GetValue(parentObject);

                if (!IsCollectionType(targetObject))
                    throw new JsonPatchProcessException(string.Format("Could not access path '{0}' in target object. '{1}' is not a collection", path, parentPropertyName));

            }
            else
            {
                var targetProperty = targetObject.GetType().GetProperty(targetPropertyName);
                if (targetProperty == null)
                    throw new JsonPatchProcessException(string.Format("Could not find path '{0}' in target object", path));

                if (!targetProperty.CanWrite)
                    throw new JsonPatchProcessException(string.Format("Property '{0}' on target object cannot be set",
                        path));
            }

            pathObject.TargetObject = targetObject;
            pathObject.TargetPropertyName = targetPropertyName;

            return pathObject;
        }

        private bool IsCollectionType(object obj)
        {
            return obj is IEnumerable && !(obj is string);
        }
    }

}