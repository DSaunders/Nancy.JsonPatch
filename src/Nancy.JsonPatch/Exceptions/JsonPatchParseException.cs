namespace Nancy.JsonPatch.Exceptions
{
    public class JsonPatchParseException : JsonPatchException
    {
        public JsonPatchParseException(string message) : base(message)
        {
        }
    }

    public class JsonPatchValueException : JsonPatchException
    {
        public JsonPatchValueException(string message)
            : base(message)
        {
        }
    }
}