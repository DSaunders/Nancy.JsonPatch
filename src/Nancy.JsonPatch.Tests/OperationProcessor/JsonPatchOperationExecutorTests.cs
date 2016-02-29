namespace Nancy.JsonPatch.Tests.OperationProcessor
{
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
        public void Add_Adds_Item_To_Collection_Before_Index()
        {
            // Arrange

            // Act

            // Assert
        }

        [Fact]
        public void Add_Adds_Item_To_Collection_To_End_If_Minus_Passed_As_Path()
        {
            // Arrange

            // Act

            // Assert
        }
    }
}
