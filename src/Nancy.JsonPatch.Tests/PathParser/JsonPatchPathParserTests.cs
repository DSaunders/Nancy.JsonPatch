namespace Nancy.JsonPatch.Tests.PathParser
{
    using System;
    using System.Collections.Generic;
    using Exceptions;
    using JsonPatch.PathParser;
    using Should;
    using Xunit;

    public class JsonPatchPathParserTests
    {
        private readonly JsonPatchPathParser _pathParser;

        public JsonPatchPathParserTests()
        {
            _pathParser = new JsonPatchPathParser();
        }

        [Fact]
        public void Throws_If_Path_Is_Empty()
        {
            // Act
            var ex = Record.Exception(() => _pathParser.ParsePath(string.Empty, new Object()));

            // Assert
            ex.ShouldNotBeNull();
            ex.ShouldBeType<JsonPatchPathException>();
            ex.Message.ShouldEqual("Could not parse the path \"\". Nancy.Json cannot modify the root of the object");
        }

        [Fact]
        public void Throws_If_Path_Refers_To_DoubleQuote_Property_On_Object()
        {
            // Act
            var ex = Record.Exception(() => _pathParser.ParsePath("/", new Object()));

            // Assert
            ex.ShouldNotBeNull();
            ex.ShouldBeType<JsonPatchPathException>();
            ex.Message.ShouldEqual("Could not parse the path \"/\". This path is not valid in Nancy.Json");
        }

        [Fact]
        public void Throws_If_Path_Does_Not_Start_With_Slash()
        {
            // Act
            var ex = Record.Exception(() => _pathParser.ParsePath("SomeProperty", new Object()));

            // Assert
            ex.ShouldNotBeNull();
            ex.ShouldBeType<JsonPatchPathException>();
            ex.Message.ShouldEqual("Could not parse the path \"SomeProperty\". Path must start with a '/'");
        }

        [Fact]
        public void Find_Simple_Paths()
        {
            // Arrange
            var target = new ExampleTarget();

            // Act
            var result = _pathParser.ParsePath("/Name", target);

            // Assert
            result.IsCollection.ShouldBeFalse();
            result.TargetObject.ShouldEqual(target);
            result.TargetPropertyName.ShouldEqual("Name");
        }


        [Fact]
        public void Throws_If_Target_Field_Does_Not_Exist()
        {
            // Arrange
            var target = new ExampleTarget();

            // Act
            var ex = Record.Exception(() => _pathParser.ParsePath("/DoesntExist", target));

            // Assert
            ex.ShouldNotBeNull();
            ex.ShouldBeType<JsonPatchPathException>();
            ex.Message.ShouldEqual("Could not find path '/DoesntExist' in target object");
        }

        [Fact]
        public void Throws_If_Target_Field_Has_No_Setter()
        {
            // Arrange
            var target = new ExampleTarget();

            // Act
            var ex = Record.Exception(() => _pathParser.ParsePath("/CantSetMe", target));

            // Assert
            ex.ShouldNotBeNull();
            ex.ShouldBeType<JsonPatchPathException>();
            ex.Message.ShouldEqual("Property '/CantSetMe' on target object cannot be set");
        }


        [Fact]
        public void Finds_Nested_Fields()
        {
            // Arrange
            var target = new ExampleTarget
            {
                Child = new ExampleTargetChild { ChildName = "Peter Pan " }
            };

            // Act
            var path =_pathParser.ParsePath("/Child/ChildName", target);

            // Assert
            path.IsCollection.ShouldBeFalse();
            path.TargetObject.ShouldEqual(target.Child);
            path.TargetPropertyName.ShouldEqual("ChildName");
        }

        [Fact]
        public void Throws_If_Path_To_Nested_Child_Is_Null()
        {
            // Arrange
            var target = new ExampleTarget();

            // Act
            var ex = Record.Exception(() => _pathParser.ParsePath("/Child/ChildName", target));

            // Assert
            ex.ShouldNotBeNull();
            ex.ShouldBeType<JsonPatchPathException>();
            ex.Message.ShouldEqual("Could not access path '/Child/ChildName' in target object. 'Child' is null");
        }

        [Fact]
        public void Throws_If_Nested_Target_Field_Has_No_Setter()
        {
            // Arrange
            var target = new ExampleTarget { Child = new ExampleTargetChild() };

            // Act
            var ex = Record.Exception(() => _pathParser.ParsePath("/Child/ChildCantSetMe", target));

            // Assert
            ex.ShouldNotBeNull();
            ex.ShouldBeType<JsonPatchPathException>();
            ex.Message.ShouldEqual("Property '/Child/ChildCantSetMe' on target object cannot be set");
        }

        [Fact]
        public void Refers_To_Items_In_Collections()
        {
            // Arrange
            var target = new ExampleTarget
            {
                StringList = new List<string> { "foo", "bar", "baz" }
            };

            // Act
            var path = _pathParser.ParsePath("/StringList/1", target);

            // Assert
            path.IsCollection.ShouldBeTrue();
            path.TargetObject.ShouldEqual(target.StringList);
            path.TargetPropertyName.ShouldEqual("1");
        }

        [Fact]
        public void Returns_Collection_And_Indexer_If_Minus_Passed()
        {
            // Arrange
            var target = new ExampleTarget
            {
                StringList = new List<string> { "foo", "bar", "baz" }
            };

            // Act
            var path = _pathParser.ParsePath("/StringList/-", target);

            // Assert
            path.IsCollection.ShouldBeTrue();
            path.TargetObject.ShouldEqual(target.StringList);
            path.TargetPropertyName.ShouldEqual("-");
        }

        [Fact]
        public void Finds_Objects_With_Collection_Indexer_In_Path()
        {
            // Arrange
            var target = new ExampleTarget
            {
                ChildList = new List<ExampleTargetChild>
                {
                    new ExampleTargetChild {ChildName = "Item 1"},
                    new ExampleTargetChild {ChildName = "Item 2"},
                    new ExampleTargetChild {ChildName = "Item 3"},
                }
            };

            // Act
            var path = _pathParser.ParsePath("/ChildList/1/ChildName", target);

            // Assert
            path.IsCollection.ShouldBeFalse();
            path.TargetObject.ShouldEqual(target.ChildList[1]);
            path.TargetPropertyName.ShouldEqual("ChildName");
        }

        [Fact]
        public void Throws_If_Indexer_In_Path_But_Target_Is_Not_Collection()
        {
            // Arrange
            var target = new ExampleTarget { Name = "Hello" };

            // Act
            var ex = Record.Exception(() => _pathParser.ParsePath("/Name/1/Something", target));

            // Assert
            ex.ShouldNotBeNull();
            ex.ShouldBeType<JsonPatchPathException>();
            ex.Message.ShouldEqual("Could not access path '/Name/1/Something' in target object. Parent object for '1' is not a collection");
        }

        [Fact]
        public void Throws_If_Target_Is_Not_Collection_But_Last_Part_Of_Path_Is_Indexer()
        {
            // Arrange
            var target = new ExampleTarget
            {
                ChildList = new List<ExampleTargetChild>
                {
                    new ExampleTargetChild {ChildName = "Item 1"},
                    new ExampleTargetChild {ChildName = "Item 2"},
                    new ExampleTargetChild {ChildName = "Item 3"},
                }
            };

            // Act
            var ex = Record.Exception(() => _pathParser.ParsePath("/ChildList/1/ChildName/2", target));

            // Assert
            ex.ShouldNotBeNull();
            ex.ShouldBeType<JsonPatchPathException>();
            ex.Message.ShouldEqual("Could not access path '/ChildList/1/ChildName/2' in target object. Parent object for '2' is not a collection");
        }
    }
}