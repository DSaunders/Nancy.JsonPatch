namespace Nancy.JsonPatch.DocumentParser
{
    using System.Collections.Generic;
    using Json;
    using Models;

    internal class JsonPatchDocumentParser
    {
        public List<JsonPatchOperation> DeserializeJsonPatchRequest(string request)
        {
            var deserializer = new JavaScriptSerializer();
            deserializer.RegisterConverters(new[] { new JsonPatchRequestConverter() });
            var result =  deserializer.Deserialize<List<JsonPatchOperation>>(request);

            return result;
        } 
    }
}
