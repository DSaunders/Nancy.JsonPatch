namespace Nancy.JsonPatch.OperationProcessor
{
    using System;
    using System.Collections;
    using System.Reflection;
    using Json;
    using Models;

    internal class JsonPatchOperationExecutor
    {
        public JsonPatchOperationExecutorResult Remove(JsonPatchPath path)
        {
            if (path.IsCollection)
            {
                var listIndex = int.Parse(path.TargetPropertyName);
                if (((IList) path.TargetObject).Count < (listIndex + 1))
                    return Failure("Could not find item " + listIndex + " in the collection");

                ((IList) path.TargetObject).RemoveAt(listIndex);
            }
            else
            {
                // We can't remove properties in C#, so we null them instead
                path.TargetObject.GetType()
                    .GetProperty(path.TargetPropertyName)
                    .SetValue(path.TargetObject, null);
            }

            return Success();
        }

        public JsonPatchOperationExecutorResult Replace<T>(JsonPatchPath path, T value)
        {
            if (path.IsCollection)
            {
                var listIndex = int.Parse(path.TargetPropertyName);
                var listType = path.TargetObject.GetType().GetGenericArguments()[0];
                
                var convertedType = ConvertToType(value, listType);
                if (convertedType == null)
                    return Failure("The value could not be converted to type " + listType.Name);

                if (((IList)path.TargetObject).Count < (listIndex + 1))
                    return Failure("Could not find item " + listIndex + " in the collection");
                ((IList)path.TargetObject)[listIndex] = convertedType;
            }
            else
            {
                var targetProperty = path.TargetObject.GetType()
                    .GetProperty(path.TargetPropertyName);

                var convertedType = ConvertToType(value, targetProperty.PropertyType);
                if (convertedType == null)
                    return Failure("The value could not be converted to type " + targetProperty.PropertyType.Name);

                targetProperty.SetValue(path.TargetObject, convertedType);
            }

            return Success();
        }

        public JsonPatchOperationExecutorResult Add<T>(JsonPatchPath path, T value)
        {
            if (!path.IsCollection)
            {
                // We can't 'Add' to a typed object in C# when it's not a collection, 
                //  so treat Add as a Replace here
                return Replace(path, value);
            }

            var listType = path.TargetObject.GetType().GetGenericArguments()[0];
            var typedObject = ConvertToType(value, listType);
            if (typedObject == null)
                return Failure("The value could not be converted to type " + listType.Name);

            if (path.TargetPropertyName.Equals("-"))
            {
                // Add to the end of the collection
                ((IList)path.TargetObject).Add(typedObject);
            }
            else
            {
                // Add before the item in the index
                var listIndex = int.Parse(path.TargetPropertyName);
                ((IList)path.TargetObject).Insert(listIndex, typedObject);
            }

            return Success();
        }

        public JsonPatchOperationExecutorResult Test<T>(JsonPatchPath path, T value)
        {
            var serializer = new JavaScriptSerializer();

            var targetValue = path.TargetObject.GetType()
                        .GetProperty(path.TargetPropertyName)
                        .GetValue(path.TargetObject);

            var targetString = serializer.Serialize(targetValue);
            var valueString = serializer.Serialize(value);


            if (!targetString.Equals(valueString))
                return Failure("Test operation failed. '" + path.TargetPropertyName + "' property did not match");

            return Success();
        }

        public JsonPatchOperationExecutorResult Move(JsonPatchPath from, JsonPatchPath path)
        {
            return CopyValue(from, path, removeOriginal: true);
        }

        public JsonPatchOperationExecutorResult Copy(JsonPatchPath from, JsonPatchPath path)
        {
            return CopyValue(from, path, removeOriginal: false);
        }

        private JsonPatchOperationExecutorResult CopyValue(JsonPatchPath from, JsonPatchPath to, bool removeOriginal)
        {
            // Copy/Move is an 'Add' of the value in the 'From' path
            //  (preceded by a 'remove' for the 'move' operation)
            object value;
            if (from.IsCollection)
            {
                var listIndex = int.Parse(from.TargetPropertyName);

                if (((IList) from.TargetObject).Count < (listIndex + 1))
                    return Failure("Could not find item " + listIndex + " in the collection");

                value = ((IList) from.TargetObject)[listIndex];
                
            }
            else
            {
                value = from.TargetObject.GetType()
                        .GetProperty(from.TargetPropertyName)
                        .GetValue(from.TargetObject);
            }

            if (removeOriginal)
                Remove(from);

            return Add(to, value);
        }

        private static object ConvertToType(object target, Type type)
        {
            // Here we use the Nancy JavaScriptSerializer, and have to construct
            // the ConvertToType method using the correct type at runtime. To remove this
            // dependency, consider implementing our own 'ConvertToType' method.
            var serializer = new JavaScriptSerializer();
            var method = typeof(JavaScriptSerializer).GetMethod("ConvertToType");
            var generic = method.MakeGenericMethod(type);

            try
            {
                return generic.Invoke(serializer, new[] { target });
            }
            catch (TargetInvocationException)
            {
                return null;
            }
        }

        private JsonPatchOperationExecutorResult Failure(string message)
        {
            return new JsonPatchOperationExecutorResult
            {
                Succeeded = false,
                Message = message
            };
        }

        private JsonPatchOperationExecutorResult Success()
        {
            return new JsonPatchOperationExecutorResult { Succeeded = true };
        }
    }
}
