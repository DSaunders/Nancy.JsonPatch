namespace Nancy.JsonPatch.Tests.DocumentParser
{
    using System.Collections;
    using System.Linq;
    using Exceptions;
    using JsonPatch.DocumentParser;
    using Models;
    using Should;
    using Xunit;

    public class JsonPatchDocumentParserTests
    {
        private readonly JsonPatchDocumentParser _documentParser;

        public JsonPatchDocumentParserTests()
        {
            _documentParser = new JsonPatchDocumentParser();
        }

        [Fact]
        public void Deserializes_JsonPatch_Document()
        {
            // Given
            var patchDocument =
                "[" +
                "   { \"op\": \"add\", \"path\": \"/baz\", \"value\": \"boo\" }," +
                "   { \"op\": \"remove\", \"path\": \"/hello\" }," +
                "   { \"op\": \"replace\", \"path\": \"/hello\", \"value\": \"world\" }," +
                "   { \"op\": \"move\", \"path\": \"/hello\", \"from\": \"/world\" }," +
                "   { \"op\": \"copy\", \"path\": \"/hello\", \"from\": \"/world\" }," +
                "   { \"op\": \"test\", \"path\": \"/hello\", \"value\": \"world\" }" +
                "]";

            // When
            var result = _documentParser.DeserializeJsonPatchRequest(patchDocument);

            // Then
            result[0].Op.ShouldEqual(JsonPatchOpCode.add);
            result[0].Path.ShouldEqual("/baz");
            result[0].Value.ShouldEqual("boo");
            result[1].Op.ShouldEqual(JsonPatchOpCode.remove);
            result[1].Path.ShouldEqual("/hello");
            result[2].Op.ShouldEqual(JsonPatchOpCode.replace);
            result[2].Path.ShouldEqual("/hello");
            result[2].Value.ShouldEqual("world");
            result[3].Op.ShouldEqual(JsonPatchOpCode.move);
            result[3].Path.ShouldEqual("/hello");
            result[3].From.ShouldEqual("/world");
            result[4].Op.ShouldEqual(JsonPatchOpCode.copy);
            result[4].Path.ShouldEqual("/hello");
            result[4].From.ShouldEqual("/world");
            result[5].Op.ShouldEqual(JsonPatchOpCode.test);
            result[5].Path.ShouldEqual("/hello");
            result[5].Value.ShouldEqual("world");
        }

        [Fact]
        public void Deserializes_Operations_With_Collection_Values()
        {
            // Arrange
            var patchDocument =
               "[" +
               "   { \"op\": \"add\", \"path\": \"/hello\", \"value\": [\"world\",\"foo\"] }" +
               "]";

            // When
            var result = _documentParser.DeserializeJsonPatchRequest(patchDocument);

            // Then
            var list = ((IEnumerable) result[0].Value).Cast<string>().ToList();
            list.Count.ShouldEqual(2);
            list[0].ShouldEqual("world");
            list[1].ShouldEqual("foo");
        }

        [Fact]
        public void Throws_Exception_If_Op_Is_Not_Present()
        {
            // Arrange
            var patchDocument =
               "[" +
               "   { \"path\": \"/hello\", \"value\": [\"world\",\"foo\"] }" +
               "]";

            // When
            var ex = Record.Exception(() => _documentParser.DeserializeJsonPatchRequest(patchDocument));

            // Then
            ex.ShouldNotBeNull();
            ex.ShouldBeType<JsonPatchParseException>();
            ex.Message.ShouldEqual("Cannot deserialize JSON patch operation. The 'op' property must be present");
        }

        [Fact]
        public void Throws_Exception_If_Op_Is_Not_String()
        {
            // Arrange
            var patchDocument =
               "[" +
               "   { \"op\": 42, \"path\": \"/hello\", \"value\": [\"world\",\"foo\"] }" +
               "]";

            // When
            var ex = Record.Exception(() => _documentParser.DeserializeJsonPatchRequest(patchDocument));

            // Then
            ex.ShouldNotBeNull();
            ex.ShouldBeType<JsonPatchParseException>();
            ex.Message.ShouldEqual("Cannot deserialize JSON patch operation. The 'op' property must be a string");
        }

        [Fact]
        public void Throws_Exception_If_Op_Is_Not_Valid()
        {
            // Arrange
            var patchDocument =
               "[" +
               "   { \"op\": \"thing\", \"path\": \"/hello\", \"value\": [\"world\",\"foo\"] }" +
               "]";

            // When
            var ex = Record.Exception(() => _documentParser.DeserializeJsonPatchRequest(patchDocument));

            // Then
            ex.ShouldNotBeNull();
            ex.ShouldBeType<JsonPatchParseException>();
            ex.Message.ShouldEqual("Cannot deserialize JSON patch operation. The 'op' property is not valid");
        }

        [Fact]
        public void Throws_Exception_If_Path_Is_Not_Present()
        {
            // Arrange
            var patchDocument =
               "[" +
               "   { \"op\": \"add\", \"value\": [\"world\",\"foo\"] }" +
               "]";

            // When
            var ex = Record.Exception(() => _documentParser.DeserializeJsonPatchRequest(patchDocument));

            // Then
            ex.ShouldNotBeNull();
            ex.ShouldBeType<JsonPatchParseException>();
            ex.Message.ShouldEqual("Cannot deserialize JSON patch operation. The 'path' property must be present");
        }

        [Fact]
        public void Throws_Exception_If_Path_Is_Not_String()
        {
            // Arrange
            var patchDocument =
               "[" +
               "   { \"op\": \"add\", \"path\": [32,43], \"value\": [\"world\",\"foo\"] }" +
               "]";

            // When
            var ex = Record.Exception(() => _documentParser.DeserializeJsonPatchRequest(patchDocument));

            // Then
            ex.ShouldNotBeNull();
            ex.ShouldBeType<JsonPatchParseException>();
            ex.Message.ShouldEqual("Cannot deserialize JSON patch operation. The 'path' property must be a string");
        }
    }
}
