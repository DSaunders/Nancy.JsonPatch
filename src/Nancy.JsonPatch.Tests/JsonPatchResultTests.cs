namespace Nancy.JsonPatch.Tests
{
    using Xunit;

    public class JsonPatchResultTests
    {
        [Fact]
        public void Acts_As_True_When_Successful()
        {
            // Given
            var result = new JsonPatchResult { Succeeded = true };

            // Then
            Assert.True(result);
        }

        [Fact]
        public void Acts_As_False_When_Unsuccessful()
        {
            // Given
            var result = new JsonPatchResult { Succeeded = false };

            // Then
            Assert.False(result);
        }
    }
}