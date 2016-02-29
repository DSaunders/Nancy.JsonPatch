namespace Nancy.JsonPatch.OperationProcessor
{
    using System;
    using System.Collections;
    using System.Reflection;
    using Exceptions;
    using Json;
    using Models;

    internal class JsonPatchOperationExecutor
    {
        public void Remove(JsonPatchPath path)
        {
            // Get propery of item
            if (path.IsCollection)
            {
                var listIndex = int.Parse(path.TargetPropertyName);
                ((IList)path.TargetObject).RemoveAt(listIndex);
            }
            else
            {
                path.TargetObject.GetType()
                    .GetProperty(path.TargetPropertyName)
                    .SetValue(path.TargetObject, null);
            }
        }

        public void Replace<T>(JsonPatchPath path, T value)
        {
            if (path.IsCollection)
            {
                var listIndex = int.Parse(path.TargetPropertyName);
                var listType = path.TargetObject.GetType().GetGenericArguments()[0];
                var convertedType = ConvertToType(value, listType);
                ((IList)path.TargetObject)[listIndex] = convertedType;
            }
            else
            {
                var targetProperty = path.TargetObject.GetType()
                    .GetProperty(path.TargetPropertyName);

                var convertedType = ConvertToType(value, targetProperty.PropertyType);
                targetProperty.SetValue(path.TargetObject, convertedType);
            }
        }

        public void Add<T>(JsonPatchPath path, T value)
        {
            if (!path.IsCollection)
            {
                Replace(path, value);
                return;
            }

            var listType = path.TargetObject.GetType().GetGenericArguments()[0];
            var typedObject = ConvertToType(value, listType);

            if (path.TargetPropertyName.Equals("-"))
            {
                // Add to the end of the collection
                ((IList) path.TargetObject).Add(typedObject);
            }
            else
            {
                // Add before the item in the index
                var listIndex = int.Parse(path.TargetPropertyName);
                ((IList)path.TargetObject).Insert(listIndex, typedObject);
            }
        }

        private object ConvertToType(object target, Type type)
        {
            var serializer = new JavaScriptSerializer();
            var method = typeof(JavaScriptSerializer).GetMethod("ConvertToType");
            var generic = method.MakeGenericMethod(type);

            try
            {
                return generic.Invoke(serializer, new[] { target });
            }
            catch (TargetInvocationException)
            {
                throw new JsonPatchValueException("The value could not be converted to type " + type.Name);
            }
        }
    }
}
