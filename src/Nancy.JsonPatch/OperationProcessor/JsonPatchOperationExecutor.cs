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
                var targetType = ((IList)path.TargetObject)[listIndex].GetType();
                var convertedType = ConvertToType(value, targetType);
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
                Replace(path, value);
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
