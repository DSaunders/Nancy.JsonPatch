namespace Nancy.JsonPatch.Tests
{
    using System.Collections.Generic;
    using Fakes;
    using Should;
    using Xunit;

    /// <summary>
    /// This class contains end-to-end tests of Nancy.JsonPatch through all of its various components
    /// </summary>
    public class JsonPatchExecutorTests
    {
        private readonly JsonPatchExecutor _jsonPatch;

        public JsonPatchExecutorTests()
        {
            _jsonPatch = new JsonPatchExecutor();
        }

        [Fact]
        public void Performs_Replace_Operations()
        {
            // Given
            var target = new ExampleTarget
            {
                ValueType = 1234,
                Child = new ExampleTargetChild
                {
                    ChildName = "I am a child"
                }
            };
            
            // When
            var result = _jsonPatch.Patch(
                "[" +
                "   { \"op\": \"replace\", \"path\" : \"/ValueType\", \"value\" : 999}" +
                "]",
                target);

            // Then
            result.Succeeded.ShouldBeTrue();
            target.ValueType.ShouldEqual(999);
        }

        [Fact]
        public void Returns_Unsuccessful_When_Replace_Operation_Fails()
        {
            // Given
            var target = new ExampleTarget
            {
                ValueType = 1234,
                Child = new ExampleTargetChild
                {
                    ChildName = "I am a child"
                }
            };

            // When
            var result = _jsonPatch.Patch(
                "[" +
                "   { \"op\": \"replace\", \"path\" : \"/ValueType\", \"value\" : \"Hello\"}" +
                "]",
                target);

            // Then
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldEqual("The value could not be converted to type Int32");
            result.FailureReason.ShouldEqual(JsonPatchFailureReason.OperationFailed);
        }

        [Fact]
        public void Performs_Move_Operations()
        {
            // Given
            var target = new ExampleTarget
            {
                Child = new ExampleTargetChild
                {
                    ChildName = "I am a child"
                }
            };

            // When
            var result = _jsonPatch.Patch(
                "[" +
                "   { \"op\": \"move\", \"path\" : \"/Name\", \"from\" : \"/Child/ChildName\"}" +
                "]",
                target);
            
            // Then
            target.Name.ShouldEqual("I am a child");
            target.Child.ChildName.ShouldBeNull();
            result.Succeeded.ShouldBeTrue();
        }

        [Fact]
        public void Returns_Unsuccessful_When_Move_Operation_Fails()
        {
            // Given
            var target = new ExampleTarget
            {
                Child = new ExampleTargetChild
                {
                    ChildName = "I am a child"
                }
            };

            // When
            var result = _jsonPatch.Patch(
                "[" +
                "   { \"op\": \"move\", \"path\" : \"/Name\", \"from\" : \"/Child\"}" +
                "]",
                target);

            // Then
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldEqual("The value could not be converted to type String");
            result.FailureReason.ShouldEqual(JsonPatchFailureReason.OperationFailed);
        }

        [Fact]
        public void Performs_Copy_Operations()
        {
            // Given
            var target = new ExampleTarget
            {
                Child = new ExampleTargetChild
                {
                    ChildName = "I am a child"
                }
            };

            // When
            var result = _jsonPatch.Patch(
                "[" +
                "   { \"op\": \"copy\", \"path\" : \"/Name\", \"from\" : \"/Child/ChildName\"}" +
                "]",
                target);

            // Then
            target.Name.ShouldEqual("I am a child");
            target.Child.ChildName.ShouldEqual("I am a child");
            result.Succeeded.ShouldBeTrue();
        }

        [Fact]
        public void Returns_Unsuccessful_When_Copy_Operation_Fails()
        {
            // Given
            var target = new ExampleTarget
            {
                Child = new ExampleTargetChild
                {
                    ChildName = "I am a child"
                }
            };

            // When
            var result = _jsonPatch.Patch(
                "[" +
                "   { \"op\": \"copy\", \"path\" : \"/Name\", \"from\" : \"/Child\"}" +
                "]",
                target);

            // Then
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldEqual("The value could not be converted to type String");
            result.FailureReason.ShouldEqual(JsonPatchFailureReason.OperationFailed);
        }

        [Fact]
        public void Performs_Add_Operations()
        {
            // Given
            var target = new ExampleTarget
            {
                ChildList = new List<ExampleTargetChild>
                {
                    new ExampleTargetChild {ChildName = "Child 1"},
                    new ExampleTargetChild {ChildName = "Child 2"},
                }
            };

            // When
            var result = _jsonPatch.Patch(
                "[" +
                "   { \"op\": \"add\", \"path\" : \"/ChildList/1\", \"value\" : { \"ChildName\" : \"New Entry\" }}" +
                "]",
                target);
            
            // Then
            target.ChildList.Count.ShouldEqual(3);
            target.ChildList[0].ChildName.ShouldEqual("Child 1");
            target.ChildList[1].ChildName.ShouldEqual("New Entry");
            target.ChildList[2].ChildName.ShouldEqual("Child 2");
            result.Succeeded.ShouldBeTrue();
        }

        [Fact]
        public void Returns_Unsuccessful_When_Add_Operation_Fails()
        {
            // Given
            var target = new ExampleTarget
            {
                ChildList = new List<ExampleTargetChild>
                {
                    new ExampleTargetChild {ChildName = "Child 1"},
                    new ExampleTargetChild {ChildName = "Child 2"},
                }
            };

            // When
            var result = _jsonPatch.Patch(
                "[" +
                "   { \"op\": \"add\", \"path\" : \"/ChildList/1\", \"value\" : 789 }" +
                "]",
                target);

            // Then
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldEqual("The value could not be converted to type ExampleTargetChild");
            result.FailureReason.ShouldEqual(JsonPatchFailureReason.OperationFailed);
        }

        [Fact]
        public void Performs_Remove_Operations()
        {
            // Given
            var target = new ExampleTarget
            {
                ChildList = new List<ExampleTargetChild>
                {
                    new ExampleTargetChild {ChildName = "Child 1"},
                    new ExampleTargetChild {ChildName = "Child 2"},
                }
            };

            // When
            var result = _jsonPatch.Patch(
                "[" +
                "   { \"op\": \"remove\", \"path\" : \"/ChildList/0\" }" +
                "]",
                target);

            // Then
            target.ChildList.Count.ShouldEqual(1);
            target.ChildList[0].ChildName.ShouldEqual("Child 2");
            result.Succeeded.ShouldBeTrue();
        }

        [Fact]
        public void Returns_Unsuccessful_When_Remove_Operation_Fails()
        {
            // Given
            var target = new ExampleTarget
            {
                ChildList = new List<ExampleTargetChild>
                {
                    new ExampleTargetChild {ChildName = "Child 1"},
                    new ExampleTargetChild {ChildName = "Child 2"},
                }
            };

            // When
            var result = _jsonPatch.Patch(
                "[" +
                "   { \"op\": \"remove\", \"path\" : \"/ChildList/89\" }" +
                "]",
                target);

            // Then
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldEqual("Could not find item 89 in the collection");
            result.FailureReason.ShouldEqual(JsonPatchFailureReason.OperationFailed);
        }

        [Fact]
        public void Returns_Success_When_Test_Passes()
        {
            // Given
            var target = new ExampleTarget
            {
                ValueType = 1234,
                Child = new ExampleTargetChild
                {
                    ChildName = "I am a child"
                }
            };

            // When
            var result = _jsonPatch.Patch(
                "[" +
                "   { \"op\": \"test\", \"path\" : \"/ValueType\", \"value\" : 1234}," +
                "   { \"op\": \"replace\", \"path\" : \"/ValueType\", \"value\" : 999}" +
                "]",
                target);
            // Then
            target.ValueType.ShouldEqual(999);
            result.Succeeded.ShouldBeTrue();
        }

        [Fact]
        public void Returns_Unsuccessful_When_Test_Fails()
        {
            // Given
            var target = new ExampleTarget
            {
                ValueType = 1234,
                Child = new ExampleTargetChild
                {
                    ChildName = "I am a child"
                }
            };

            // When
            var result = _jsonPatch.Patch(
                "[" +
                "   { \"op\": \"test\", \"path\" : \"/ValueType\", \"value\" : 909090}," +
                "   { \"op\": \"replace\", \"path\" : \"/ValueType\", \"value\" : 999}" +
                "]",
                target);

            // Then
            result.Succeeded.ShouldBeFalse();
            result.FailureReason.ShouldEqual(JsonPatchFailureReason.TestFailed);
            result.Message.ShouldContain("Test operation failed. 'ValueType' property did not match");
        }

        [Fact]
        public void Performs_Operations_On_Collections()
        {
            // Given
            var target = new ExampleTarget
            {
                ChildList = new List<ExampleTargetChild>
                {
                    new ExampleTargetChild { ChildName = "Child 1"},
                    new ExampleTargetChild { ChildName = "Child 2"},
                    new ExampleTargetChild { ChildName = "Child 3"},
                }
            };

            // When
            var result = _jsonPatch.Patch(
                "[" +
                "   { \"op\": \"move\", \"path\" : \"/Child\", \"from\" : \"/ChildList/2\"}," +
                "   { \"op\": \"replace\", \"path\" : \"/ChildList/1/ChildName\", \"value\" : \"Replaced\"}," +
                "   { \"op\": \"remove\", \"path\" : \"/ChildList/0\" }" +
                "]",
                target);

            // Then
            target.Child.ChildName.ShouldEqual("Child 3");
            target.ChildList.Count.ShouldEqual(1);
            target.ChildList[0].ChildName.ShouldEqual("Replaced");
            result.Succeeded.ShouldBeTrue();
        }

        [Fact]
        public void Returns_Unsuccessful_When_Cannot_Parse_Json()
        {
            // Given
            var target = new ExampleTarget
            {
                ValueType = 1234,
                Child = new ExampleTargetChild
                {
                    ChildName = "I am a child"
                }
            };

            // When
            var result = _jsonPatch.Patch(
                "[" +
                "   { \"op\"/ValueType\",,,,, \"value\" : 909090}," +
                "]",
                target);

            // Then
            result.Succeeded.ShouldBeFalse();
            result.FailureReason.ShouldEqual(JsonPatchFailureReason.CouldNotParseJson);
        }
        
        [Fact]
        public void Returns_Unsuccessful_When_Cannot_Parse_Path()
        {
            // Given
            var target = new ExampleTarget
            {
                ValueType = 1234,
                Child = new ExampleTargetChild
                {
                    ChildName = "I am a child"
                }
            };

            // When
            var result = _jsonPatch.Patch(
                "[" +
                "   { \"op\": \"remove\", \"path\" : \"/Child/L/i/st/0\" }" +
                "]",
                target);

            // Then
            result.Succeeded.ShouldBeFalse();
            result.FailureReason.ShouldEqual(JsonPatchFailureReason.CouldNotParsePath);
        }

        [Fact]
        public void Returns_Unsuccessful_When_Cannot_Parse_From()
        {
            // Given
            var target = new ExampleTarget
            {
                ValueType = 1234,
                Child = new ExampleTargetChild
                {
                    ChildName = "I am a child"
                }
            };

            // When
            var result = _jsonPatch.Patch(
                "[" +
                "   { \"op\": \"copy\", \"path\" : \"/Name\", \"from\" : \"/ChildName\"}" +
                "]",
                target);

            // Then
            result.Succeeded.ShouldBeFalse();
            result.FailureReason.ShouldEqual(JsonPatchFailureReason.CouldNotParseFrom);
        }

        [Fact]
        public void Performs_Multiple_Operations()
        {
            // Given
            var target = new ExampleTarget
            {
                ValueType = 1234,
                ChildList = new List<ExampleTargetChild>
                {
                    new ExampleTargetChild { ChildName = "Child 1"},
                    new ExampleTargetChild { ChildName = "Child 2"},
                },

                Child = new ExampleTargetChild
                {
                    ChildName = "I am a child"
                }
            };

            // When
            var result = _jsonPatch.Patch(
                "[" +
                "   { \"op\": \"move\", \"path\" : \"/Name\", \"from\" : \"/Child/ChildName\"}," +
                "   { \"op\": \"replace\", \"path\" : \"/ChildList/1/ChildName\", \"value\" : \"Replaced Name\"}," +
                "   { \"op\": \"test\", \"path\" : \"/ValueType\", \"value\" : 1234}," +
                "   { \"op\": \"add\", \"path\" : \"/ValueType\", \"value\" : 789}," +
                "   { \"op\": \"test\", \"path\" : \"/ValueType\", \"value\" : 789}" +
                "]",
                target);

            // Then
            result.Message.ShouldBeNull();
            result.Succeeded.ShouldBeTrue();
            target.Name.ShouldEqual("I am a child");
            target.Child.ChildName.ShouldBeNull();
            target.ChildList[1].ChildName.ShouldEqual("Replaced Name");
            target.ValueType.ShouldEqual(789);
            
        }
    }
}
