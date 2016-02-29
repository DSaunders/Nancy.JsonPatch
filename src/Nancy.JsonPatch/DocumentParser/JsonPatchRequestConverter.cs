namespace Nancy.JsonPatch.DocumentParser
{
    using System;
    using System.Collections.Generic;
    using Exceptions;
    using Json;
    using Models;

    internal class JsonPatchRequestConverter : JavaScriptConverter
    {
        public override IEnumerable<Type> SupportedTypes
        {
            get { yield return typeof(JsonPatchOperation); }
        }

        public override object Deserialize(IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer)
        {
            var operation = new JsonPatchOperation();

            if (!dictionary.ContainsKey("Op"))
                throw new JsonPatchParseException("Cannot deserialize JSON patch operation. The 'op' property must be present");

            var opCodeString = dictionary["Op"] as string;
            if (opCodeString == null)
                throw new JsonPatchParseException("Cannot deserialize JSON patch operation. The 'op' property must be a string");

            JsonPatchOpCode opCode;
            if (!Enum.TryParse(opCodeString, out opCode))
                throw new JsonPatchParseException("Cannot deserialize JSON patch operation. The 'op' property is not valid");

            operation.Op = opCode;

            if (!dictionary.ContainsKey("Path"))
                throw new JsonPatchParseException("Cannot deserialize JSON patch operation. The 'path' property must be present");

            operation.Path = dictionary["Path"] as string;
            if (operation.Path == null)
                throw new JsonPatchParseException("Cannot deserialize JSON patch operation. The 'path' property must be a string");

            if (dictionary.ContainsKey("Value"))
                operation.Value = dictionary["Value"];

            if (dictionary.ContainsKey("From"))
                operation.From = dictionary["From"] as string;

            return operation;
        }

        public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
        {
            return null;
        }
    }
}