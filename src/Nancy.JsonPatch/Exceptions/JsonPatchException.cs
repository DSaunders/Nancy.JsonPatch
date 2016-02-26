namespace Nancy.JsonPatch.Exceptions
{
    using System;

    public abstract class JsonPatchException : Exception
    {
        protected JsonPatchException(string message)
            : base(message)
        {
        }
    }
}