namespace Nancy.JsonPatch.OperationProcessor
{
    using System.Collections;
    using Models;

    internal class JsonPatchOperationExecutor
    {
        public void Remove(JsonPatchPath jsonPatchPath)
        {
            // Get propery of item
            if (jsonPatchPath.IsCollection)
            {
                var listIndex = int.Parse(jsonPatchPath.TargetPropertyName);
                ((IList)jsonPatchPath.TargetObject).RemoveAt(listIndex);
            }
            else
            {
                jsonPatchPath.TargetObject.GetType()
                    .GetProperty(jsonPatchPath.TargetPropertyName)
                    .SetValue(jsonPatchPath.TargetObject, null);
            }
        }

        
    }
}
