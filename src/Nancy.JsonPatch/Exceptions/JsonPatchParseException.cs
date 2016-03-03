namespace Nancy.JsonPatch.Exceptions
{
    using System;

    public class JsonPatchParseException : Exception
    {
        public JsonPatchParseException(string message) : base(message)
        {
        }
    }
}