namespace Nancy.JsonPatch.Tests.OperationProcessor
{
    using System.Collections.Generic;
    using Exceptions;
    using JsonPatch.OperationProcessor;
    using Models;
    using Should;
    using Xunit;

    public class JsonPatchOperationExecutorTests
    {
        private readonly JsonPatchOperationExecutor _executor;

        public JsonPatchOperationExecutorTests()
        {
            _executor = new JsonPatchOperationExecutor();
        }

        [Fact]
        public void Remove_Operation_Nulls_Field()
        {
            // Arrange
            var target = new ExampleTarget { Name = "Bob the Builder" };
            var path = new JsonPatchPath {TargetObject = target, TargetPropertyName = "Name"};

            // Act
            _executor.Remove(path);

            // Assert
            target.Name.ShouldBeNull();
        }

        [Fact]
        public void Remove_Operation_Sets_Default_Value_If_Target_Field_Not_Nullable()
        {
            // Arrange
            var target = new ExampleTarget { ValueType = 999 };
            var path = new JsonPatchPath { TargetObject = target, TargetPropertyName = "ValueType" };

            // Act
            _executor.Remove(path);

            // Assert
            target.ValueType.ShouldEqual(default(int));
        }

        [Fact]
        public void Replaces_Reference_Types()
        {
            // Arrange
            var exampleTarget = new ExampleTarget
            {
                Child = new ExampleTargetChild
                {
                    ChildName = "Old Child"
                }
            };

            var path = new JsonPatchPath
            {
                TargetObject = exampleTarget,
                IsCollection = false,
                TargetPropertyName = "Child"
            };

            // Act
            _executor.Replace(path, new JsonPatchOperation
            {
                Value = new Dictionary<string, object>
                {
                    {"ChildName", "New Child"}
                }
            });

            // Assert
            exampleTarget.Child.ChildName.ShouldEqual("New Child");
        }

        [Fact]
        public void Replaces_Value_Types()
        {
            // Arrange
            var exampleTarget = new ExampleTarget
            {
                ValueType = 1234
            };
            var path = new JsonPatchPath
            {
                TargetObject = exampleTarget,
                IsCollection = false,
                TargetPropertyName = "ValueType"
            };

            // Act
            _executor.Replace(path, new JsonPatchOperation
            {
                Value = 999
            });

            // Assert
            exampleTarget.ValueType.ShouldEqual(999);
        }

        [Fact]
        public void Replaces_Reference_Types_In_Collections()
        {
            // Arrange
            var exampleTarget = new ExampleTarget
            {
                ChildList = new List<ExampleTargetChild>
                {
                    new ExampleTargetChild { ChildName = "Child 1"},
                    new ExampleTargetChild { ChildName = "Child 2"},
                    new ExampleTargetChild { ChildName = "Child 3"},
                }
            };

            var path = new JsonPatchPath
            {
                TargetObject = exampleTarget.ChildList,
                IsCollection = true,
                TargetPropertyName = "1"
            };

            // Act
            _executor.Replace(path, new JsonPatchOperation
            {
                Value = new Dictionary<string, object>
                {
                    {"ChildName", "New Child"}
                }
            });

            // Assert
            exampleTarget.ChildList[0].ChildName.ShouldEqual("Child 1");
            exampleTarget.ChildList[1].ChildName.ShouldEqual("New Child");
            exampleTarget.ChildList[2].ChildName.ShouldEqual("Child 3");
        }

        [Fact]
        public void Replaces_Value_Types_In_Collections()
        {
            // Arrange
            var exampleTarget = new ExampleTarget
            {
                IntList = new List<int> {3, 4, 5}
            };

            var path = new JsonPatchPath
            {
                TargetObject = exampleTarget.IntList,
                IsCollection = true,
                TargetPropertyName = "1"
            };

            // Act
            _executor.Replace(path, new JsonPatchOperation
            {
                Value = 999
            });

            // AssertS
            exampleTarget.IntList[0].ShouldEqual(3);
            exampleTarget.IntList[1].ShouldEqual(999);
            exampleTarget.IntList[2].ShouldEqual(5);
        }

        [Fact]
        public void Replace_Throws_If_Cannot_Cast_To_Original_Property_Type()
        {
            // Arrange
            var exampleTarget = new ExampleTarget
            {
                Child = new ExampleTargetChild
                {
                    ChildName = "Old Child"
                }
            };

            var path = new JsonPatchPath
            {
                TargetObject = exampleTarget,
                IsCollection = false,
                TargetPropertyName = "Child"
            };

            // Act
           var ex = Record.Exception(() => _executor.Replace(path, new JsonPatchOperation
            {
                Value = "This is an invalid value for the Child Type"
            }));

            // Assert
            ex.ShouldNotBeNull();
            ex.ShouldBeType<JsonPatchValueException>();
            ex.Message.ShouldEqual("The value could not be converted to type ExampleTargetChild");
        }
    }
}
