namespace Nancy.JsonPatch.Tests.PathParser
{
    using System;
    using System.Collections.Generic;
    using Fakes;
    using JsonPatch.PathParser;
    using JsonPatch.PropertyResolver;
    using Should;
    using Xunit;

    public class JsonPatchPathParserTests
    {
        private readonly JsonPatchPathParser _pathParser;

        public JsonPatchPathParserTests()
        {
            _pathParser = new JsonPatchPathParser(new JsonPatchPropertyResolver());
        }

        [Fact]
        public void Returns_Null_With_Error_If_Path_Is_Empty()
        {
            // When
            var result = _pathParser.ParsePath(string.Empty, new Object());

            // Then
            result.Path.ShouldBeNull();
            result.Error.ShouldEqual("Could not parse the path \"\". Nancy.Json cannot modify the root of the object");
        }

        [Fact]
        public void Returns_Null_With_Error_If_Path_Refers_To_DoubleQuote_Property_On_Object()
        {
            // When
            var result = _pathParser.ParsePath("/", new Object());

            // Then
            result.Path.ShouldBeNull();
            result.Error.ShouldEqual("Could not parse the path \"/\". This path is not valid in Nancy.Json");
        }

        [Fact]
        public void Returns_Null_With_Error_If_Path_Does_Not_Start_With_Slash()
        {
            // When
            var result = _pathParser.ParsePath("SomeProperty", new Object());

            // Then
            result.Path.ShouldBeNull();
            result.Error.ShouldEqual("Could not parse the path \"SomeProperty\". Path must start with a '/'");
        }

        [Fact]
        public void Find_Simple_Paths()
        {
            // Given
            var target = new ExampleTarget();

            // When
            var result = _pathParser.ParsePath("/Name", target);

            // Then
            result.Path.IsCollection.ShouldBeFalse();
            result.Path.TargetObject.ShouldEqual(target);
            result.Path.TargetPropertyName.ShouldEqual("Name");
        }


        [Fact]
        public void Returns_Null_With_Error_If_Target_Field_Does_Not_Exist()
        {
            // Given
            var target = new ExampleTarget();

            // When
            var result = _pathParser.ParsePath("/DoesntExist", target);

            // Then
            result.Path.ShouldBeNull();
            result.Error.ShouldEqual("Could not find path '/DoesntExist' in target object");
        }

        [Fact]
        public void Returns_Null_With_Error_If_Target_Field_Has_No_Setter()
        {
            // Given
            var target = new ExampleTarget();

            // When
            var result = _pathParser.ParsePath("/CantSetMe", target);

            // Then
            result.Path.ShouldBeNull();
            result.Error.ShouldEqual("Property '/CantSetMe' on target object cannot be set");
        }


        [Fact]
        public void Finds_Nested_Fields()
        {
            // Given
            var target = new ExampleTarget
            {
                Child = new ExampleTargetChild { ChildName = "Peter Pan " }
            };

            // When
            var result =_pathParser.ParsePath("/Child/ChildName", target);

            // Then
            result.Path.IsCollection.ShouldBeFalse();
            result.Path.TargetObject.ShouldEqual(target.Child);
            result.Path.TargetPropertyName.ShouldEqual("ChildName");
        }

        [Fact]
        public void Returns_Null_With_Error_If_Path_To_Nested_Child_Is_Null()
        {
            // Given
            var target = new ExampleTarget();

            // When
            var result = _pathParser.ParsePath("/Child/ChildName", target);

            // Then
            result.Path.ShouldBeNull();
            result.Error.ShouldEqual("Could not access path '/Child/ChildName' in target object. 'Child' is null");
        }

        [Fact]
        public void Returns_Null_With_Error_If_Nested_Target_Field_Has_No_Setter()
        {
            // Given
            var target = new ExampleTarget { Child = new ExampleTargetChild() };

            // When
            var result = _pathParser.ParsePath("/Child/ChildCantSetMe", target);

            // Then
            result.Path.ShouldBeNull();
            result.Error.ShouldEqual("Property '/Child/ChildCantSetMe' on target object cannot be set");
        }

        [Fact]
        public void Refers_To_Items_In_Collections()
        {
            // Given
            var target = new ExampleTarget
            {
                StringList = new List<string> { "foo", "bar", "baz" }
            };

            // When
            var result = _pathParser.ParsePath("/StringList/1", target);

            // Then
            result.Path.IsCollection.ShouldBeTrue();
            result.Path.TargetObject.ShouldEqual(target.StringList);
            result.Path.TargetPropertyName.ShouldEqual("1");
        }

        [Fact]
        public void Returns_Collection_And_Indexer_If_Minus_Passed()
        {
            // Given
            var target = new ExampleTarget
            {
                StringList = new List<string> { "foo", "bar", "baz" }
            };

            // When
            var result = _pathParser.ParsePath("/StringList/-", target);

            // Then
            result.Path.IsCollection.ShouldBeTrue();
            result.Path.TargetObject.ShouldEqual(target.StringList);
            result.Path.TargetPropertyName.ShouldEqual("-");
        }

        [Fact]
        public void Finds_Objects_With_Collection_Indexer_In_Path()
        {
            // Given
            var target = new ExampleTarget
            {
                ChildList = new List<ExampleTargetChild>
                {
                    new ExampleTargetChild {ChildName = "Item 1"},
                    new ExampleTargetChild {ChildName = "Item 2"},
                    new ExampleTargetChild {ChildName = "Item 3"},
                }
            };

            // When
            var result = _pathParser.ParsePath("/ChildList/1/ChildName", target);

            // Then
            result.Path.IsCollection.ShouldBeFalse();
            result.Path.TargetObject.ShouldEqual(target.ChildList[1]);
            result.Path.TargetPropertyName.ShouldEqual("ChildName");
        }

        [Fact]
        public void Returns_Null_With_Error_If_Indexer_In_Path_But_Target_Is_Not_Collection()
        {
            // Given
            var target = new ExampleTarget { Name = "Hello" };

            // When
            var result = _pathParser.ParsePath("/Name/1/Something", target);

            // Then
            result.Path.ShouldBeNull();
            result.Error.ShouldEqual("Could not access path '/Name/1/Something' in target object. Parent object for '1' is not a collection");
        }

        [Fact]
        public void Returns_Null_With_Error_If_Target_Is_Not_Collection_But_Last_Part_Of_Path_Is_Indexer()
        {
            // Given
            var target = new ExampleTarget
            {
                ChildList = new List<ExampleTargetChild>
                {
                    new ExampleTargetChild {ChildName = "Item 1"},
                    new ExampleTargetChild {ChildName = "Item 2"},
                    new ExampleTargetChild {ChildName = "Item 3"},
                }
            };

            // When
            var result = _pathParser.ParsePath("/ChildList/1/ChildName/2", target);

            // Then
            result.Path.ShouldBeNull();
            result.Error.ShouldEqual("Could not access path '/ChildList/1/ChildName/2' in target object. Parent object for '2' is not a collection");
        }
    }
}