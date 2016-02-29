namespace Nancy.JsonPatch.Exceptions
{
    public class JsonPatchPathException : JsonPatchException
    {
        public JsonPatchPathException(string message)
            : base(message)
        {
        }
    }
}