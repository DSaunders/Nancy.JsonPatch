namespace Nancy.JsonPatch
{
    using System;
    using System.Collections.Generic;
    using Models;
    
    internal class JsonPatchExecutor
    {
        public JsonPatchResult Patch<T>(string requestBody, T target)
        {
            var documentParser = new DocumentParser.JsonPatchDocumentParser();
            var pathParser = new PathParser.JsonPatchPathParser();
            var operationExecutor = new OperationProcessor.JsonPatchOperationExecutor();
            
            List<JsonPatchOperation> operations;
            try
            {
                operations = documentParser.DeserializeJsonPatchRequest(requestBody);
            }
            catch (Exception ex)
            {
                return Failure(JsonPatchFailureReason.CouldNotParseJson, ex.Message);
            }
            
            foreach (var operation in operations)
            {
                var pathResult = pathParser.ParsePath(operation.Path, target);
                if (pathResult.Path == null)
                    return Failure(JsonPatchFailureReason.CouldNotParsePath, pathResult.Error);

                switch (operation.Op)
                {
                    case JsonPatchOpCode.replace:
                        var replaceResult = operationExecutor.Replace(pathResult.Path, operation.Value);
                        if (!replaceResult.Succeeded)
                            return Failure(JsonPatchFailureReason.OperationFailed, replaceResult.Message);
                        break;

                    case JsonPatchOpCode.move:
                        var moveFrom = pathParser.ParsePath(operation.From, target);
                        if (moveFrom.Path == null)
                            return Failure(JsonPatchFailureReason.CouldNotParseFrom, moveFrom.Error);

                        var moveResult = operationExecutor.Move(moveFrom.Path, pathResult.Path);
                        if (!moveResult.Succeeded)
                            return Failure(JsonPatchFailureReason.OperationFailed, moveResult.Message);
                        break;

                    case JsonPatchOpCode.copy:
                        var copyFrom = pathParser.ParsePath(operation.From, target);
                        if (copyFrom.Path == null)
                            return Failure(JsonPatchFailureReason.CouldNotParseFrom, copyFrom.Error);

                        var copyResult = operationExecutor.Copy(copyFrom.Path, pathResult.Path);
                        if (!copyResult.Succeeded)
                            return Failure(JsonPatchFailureReason.OperationFailed, copyResult.Message);
                        break;

                    case JsonPatchOpCode.add:
                        var addResult = operationExecutor.Add(pathResult.Path, operation.Value);
                        if (!addResult.Succeeded)
                            return Failure(JsonPatchFailureReason.OperationFailed, addResult.Message);
                        break;

                    case JsonPatchOpCode.remove:
                        var removeResult = operationExecutor.Remove(pathResult.Path);
                        if (!removeResult.Succeeded)
                            return Failure(JsonPatchFailureReason.OperationFailed, removeResult.Message);
                        break;

                    case JsonPatchOpCode.test:
                        var result = operationExecutor.Test(pathResult.Path, operation.Value);
                        if (!result.Succeeded)
                            return Failure(JsonPatchFailureReason.TestFailed, result.Message);
                        break;
                }
            }

            return new JsonPatchResult  { Succeeded = true };
        }

        private static JsonPatchResult Failure(JsonPatchFailureReason reason, string error)
        {
            return new JsonPatchResult
            {
                FailureReason = reason,
                Succeeded = false,
                Message = error
            };
        }
    }
}
