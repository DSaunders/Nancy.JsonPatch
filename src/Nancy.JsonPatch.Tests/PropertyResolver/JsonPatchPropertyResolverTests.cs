using Nancy.JsonPatch.PropertyResolver;

namespace Nancy.JsonPatch.Tests.PropertyResolver
{
    using Should;
    using Xunit;

    public class JsonPatchPropertyResolverTests
    {
        private readonly IJsonPatchPropertyResolver _propertyResolver;

        public JsonPatchPropertyResolverTests()
        {
            _propertyResolver = new JsonPatchPropertyResolver();
        }

        [Fact]
        public void Returns_Parameter()
        {
            const string propertyName = "TestProperty";

            var result = _propertyResolver.Resolve(null, propertyName);

            result.ShouldBeSameAs(propertyName);
        }
    }
}
