namespace Nancy.JsonPatch.Tests.OperationProcessor
{
    using System.Collections.Generic;
    using System.Linq;
    using Fakes;
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
            // Given
            var target = new ExampleTarget { Name = "Bob the Builder" };
            var path = new JsonPatchPath { TargetObject = target, TargetPropertyName = "Name" };

            // When
            var result = _executor.Remove(path);

            // Then
            target.Name.ShouldBeNull();
            result.Succeeded.ShouldBeTrue();
        }

        [Fact]
        public void Remove_Operation_Sets_Default_Value_If_Target_Field_Not_Nullable()
        {
            // Given
            var target = new ExampleTarget { ValueType = 999 };
            var path = new JsonPatchPath { TargetObject = target, TargetPropertyName = "ValueType" };

            // When
            var result = _executor.Remove(path);

            // Then
            target.ValueType.ShouldEqual(default(int));
            result.Succeeded.ShouldBeTrue();
        }

        [Fact]
        public void Remove_Operation_Returns_False_If_Cannot_Find_Item_In_Collection()
        {
            // Given
            var target = new ExampleTarget { ValueType = 999, ChildList = new List<ExampleTargetChild>() };
            var path = new JsonPatchPath
            {
                IsCollection = true,
                TargetObject = target.ChildList, 
                TargetPropertyName = "7"
            };

            // When
            var result = _executor.Remove(path);

            // Then
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldEqual("Could not find item 7 in the collection");
        }


        [Fact]
        public void Replaces_Reference_Types()
        {
            // Given
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

            // When
            var result = _executor.Replace(path, new Dictionary<string, object>
            {
                {"ChildName", "New Child"}
            });

            // Then
            exampleTarget.Child.ChildName.ShouldEqual("New Child");
            result.Succeeded.ShouldBeTrue();
        }

        [Fact]
        public void Replaces_Value_Types()
        {
            // Given
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

            // When
            var result = _executor.Replace(path, 999);

            // Then
            exampleTarget.ValueType.ShouldEqual(999);
            result.Succeeded.ShouldBeTrue();
        }

        [Fact]
        public void Replaces_Reference_Types_In_Collections()
        {
            // Given
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

            // When
            var result = _executor.Replace(path, new Dictionary<string, object>
                {
                    {"ChildName", "New Child"}
                });

            // Then
            exampleTarget.ChildList[0].ChildName.ShouldEqual("Child 1");
            exampleTarget.ChildList[1].ChildName.ShouldEqual("New Child");
            exampleTarget.ChildList[2].ChildName.ShouldEqual("Child 3");
            result.Succeeded.ShouldBeTrue();
        }

        [Fact]
        public void Replaces_Value_Types_In_Collections()
        {
            // Given
            var exampleTarget = new ExampleTarget
            {
                IntList = new List<int> { 3, 4, 5 }
            };

            var path = new JsonPatchPath
            {
                TargetObject = exampleTarget.IntList,
                IsCollection = true,
                TargetPropertyName = "1"
            };

            // When
            var result = _executor.Replace(path, 999);

            // Then
            exampleTarget.IntList[0].ShouldEqual(3);
            exampleTarget.IntList[1].ShouldEqual(999);
            exampleTarget.IntList[2].ShouldEqual(5);
            result.Succeeded.ShouldBeTrue();
        }

        [Fact]
        public void Replace_Returns_False_If_Cannot_Cast_To_Original_Property_Type()
        {
            // Given
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

            // When
            var result = _executor.Replace(path, "This is an invalid value for the Child Type");

            // Then
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldEqual("The value could not be converted to type ExampleTargetChild");
        }

        [Fact]
        public void Replace_Returns_False_If_Cannot_Find_Index_In_Collection()
        {
            // Given
            var exampleTarget = new ExampleTarget
            {
                ChildList = new List<ExampleTargetChild>()
            };

            var path = new JsonPatchPath
            {
                TargetObject = exampleTarget.ChildList,
                IsCollection = true,
                TargetPropertyName = "67"
            };

            // When
            var result = _executor.Replace(path, new ExampleTargetChild());

            // Then
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldEqual("Could not find item 67 in the collection");
        }

        [Fact]
        public void Replace_Returns_False_If_Cannot_Convert_Value_To_Correct_Type_For_Collection()
        {
            // Given
            var exampleTarget = new ExampleTarget
            {
                ChildList = new List<ExampleTargetChild> { new ExampleTargetChild() }
            };

            var path = new JsonPatchPath
            {
                TargetObject = exampleTarget.ChildList,
                IsCollection = true,
                TargetPropertyName = "0"
            };

            // When
            var result = _executor.Replace(path, 78);

            // Then
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldEqual("The value could not be converted to type ExampleTargetChild");
        }


        [Fact]
        public void Add_Replaces_Reference_Types()
        {
            // Given
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

            // When
            var result = _executor.Add(path, new Dictionary<string, object>
                {
                    {"ChildName", "New Child"}
                });

            // Then
            exampleTarget.Child.ChildName.ShouldEqual("New Child");
            result.Succeeded.ShouldBeTrue();
        }

        [Fact]
        public void Add_Replaces_Value_Types()
        {
            // Given
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

            // When
            var result = _executor.Add(path, 999);

            // Then
            exampleTarget.ValueType.ShouldEqual(999);
            result.Succeeded.ShouldBeTrue();
        }

        [Fact]
        public void Adds_Items_Before_Indexer_Of_Collection()
        {
            // Given
            var exampleTarget = new ExampleTarget
            {
                IntList = new List<int> { 3, 4, 5 }
            };

            var path = new JsonPatchPath
            {
                TargetObject = exampleTarget.IntList,
                IsCollection = true,
                TargetPropertyName = "1"
            };

            // When
            var result = _executor.Add(path, 999);

            // Then
            exampleTarget.IntList[0].ShouldEqual(3);
            exampleTarget.IntList[1].ShouldEqual(999);
            exampleTarget.IntList[2].ShouldEqual(4);
            exampleTarget.IntList[3].ShouldEqual(5);
            result.Succeeded.ShouldBeTrue();
        }

        [Fact]
        public void Adds_Items_To_End_Of_Collection_If_Indexer_Is_Minus()
        {
            // Given
            var exampleTarget = new ExampleTarget
            {
                IntList = new List<int> { 3, 4, 5 }
            };

            var path = new JsonPatchPath
            {
                TargetObject = exampleTarget.IntList,
                IsCollection = true,
                TargetPropertyName = "-"
            };

            // When
            var result = _executor.Add(path, 999);

            // Then
            exampleTarget.IntList[0].ShouldEqual(3);
            exampleTarget.IntList[1].ShouldEqual(4);
            exampleTarget.IntList[2].ShouldEqual(5);
            exampleTarget.IntList[3].ShouldEqual(999);
            result.Succeeded.ShouldBeTrue();
        }

        [Fact]
        public void Adds_Items_To_Empty_Collections()
        {
            // Given
            var exampleTarget = new ExampleTarget
            {
                IntList = new List<int>()
            };

            var path = new JsonPatchPath
            {
                TargetObject = exampleTarget.IntList,
                IsCollection = true,
                TargetPropertyName = "-"
            };

            // When
            var result = _executor.Add(path, 999);

            // Then
            exampleTarget.IntList[0].ShouldEqual(999);
            result.Succeeded.ShouldBeTrue();
        }

        [Fact]
        public void Add_Returns_False_If_Cannot_Cast_To_Original_Property_Type()
        {
            // Given
            var exampleTarget = new ExampleTarget
            {
                IntList = new List<int>()
            };

            var path = new JsonPatchPath
            {
                TargetObject = exampleTarget.IntList,
                IsCollection = true,
                TargetPropertyName = "-"
            };

            // When
            var result = _executor.Add(path, "This is an invalid value for the Child Type");

            // Then
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldEqual("The value could not be converted to type Int32");
        }


        [Fact]
        public void Copy_Performs_Add_With_Value_Of_Object_In_From_Path()
        {
            // Given
            var exampleTarget = new ExampleTarget
            {
                Child = new ExampleTargetChild { ChildName = "Original Child" },
                ChildList = new List<ExampleTargetChild>
                {
                    new ExampleTargetChild { ChildName = "Nested Child 1"},
                    new ExampleTargetChild { ChildName = "Nested Child 2"},
                    new ExampleTargetChild { ChildName = "Nested Child 3"},
                }
            };

            var from = new JsonPatchPath
            {
                TargetObject = exampleTarget,
                TargetPropertyName = "Child"
            };

            var path = new JsonPatchPath
            {
                TargetObject = exampleTarget.ChildList,
                IsCollection = true,
                TargetPropertyName = "1"
            };

            // When
            var result = _executor.Copy(from, path);

            // Then
            result.Succeeded.ShouldBeTrue();
            exampleTarget.Child.ChildName.ShouldEqual("Original Child");
            exampleTarget.ChildList[0].ChildName.ShouldEqual("Nested Child 1");
            exampleTarget.ChildList[1].ChildName.ShouldEqual("Original Child");
            exampleTarget.ChildList[2].ChildName.ShouldEqual("Nested Child 2");
            exampleTarget.ChildList[3].ChildName.ShouldEqual("Nested Child 3");
        }

        [Fact]
        public void Copy_Gets_Items_From_Collections()
        {
            // Given
            var exampleTarget = new ExampleTarget
            {
                Child = new ExampleTargetChild { ChildName = "Non-Nested Child" },
                ChildList = new List<ExampleTargetChild>
                {
                    new ExampleTargetChild { ChildName = "Nested Child 1"},
                    new ExampleTargetChild { ChildName = "Nested Child 2"},
                    new ExampleTargetChild { ChildName = "Nested Child 3"},
                }
            };

            var from = new JsonPatchPath
            {
                TargetObject = exampleTarget.ChildList,
                IsCollection = true,
                TargetPropertyName = "0"
            };

            var path = new JsonPatchPath
            {
                TargetObject = exampleTarget,
                TargetPropertyName = "Child"
            };
            
            // When
            var result = _executor.Copy(from, path);

            // Then
            result.Succeeded.ShouldBeTrue();
            exampleTarget.Child.ChildName.ShouldEqual("Nested Child 1");
            exampleTarget.ChildList[0].ChildName.ShouldEqual("Nested Child 1");
            exampleTarget.ChildList[1].ChildName.ShouldEqual("Nested Child 2");
            exampleTarget.ChildList[2].ChildName.ShouldEqual("Nested Child 3");
        }

        [Fact]
        public void Copy_Performs_Add_With_ValueTypes()
        {
            // Given
            var exampleTarget = new ExampleTarget
            {
                IntList = new List<int> {1, 2, 3, 4, 5}
            };

            var from = new JsonPatchPath
            {
                TargetObject = exampleTarget.IntList,
                IsCollection = true,
                TargetPropertyName = "1"
            };

            var path = new JsonPatchPath
            {
                TargetObject = exampleTarget.IntList,
                IsCollection = true,
                TargetPropertyName = "4"
            };

           // When
            var result = _executor.Copy(from, path);

            // Then
            result.Succeeded.ShouldBeTrue();
            exampleTarget.IntList
                .SequenceEqual(new[] {1, 2, 3, 4, 2, 5})
                .ShouldBeTrue();
        }


        [Fact]
        public void Move_Performs_Copy_Of_From_To_Path_Followed_By_Remove_Of_From_Value()
        {
            // Given
            var exampleTarget = new ExampleTarget
            {
                Child = new ExampleTargetChild { ChildName = "Original Child" },
                Name = "Old Name"
            };

            var from = new JsonPatchPath
            {
                TargetObject = exampleTarget.Child,
                TargetPropertyName = "ChildName"
            };

            var path = new JsonPatchPath
            {
                TargetObject = exampleTarget,
                TargetPropertyName = "Name"
            };
            
            // When
            var result = _executor.Move(from, path);

            // Then
            exampleTarget.Name.ShouldEqual("Original Child");
            exampleTarget.Child.ChildName.ShouldBeNull();
            result.Succeeded.ShouldBeTrue();
        }

        [Fact]
        public void Move_Removes_Items_From_Collection()
        {
            // Given
            var exampleTarget = new ExampleTarget
            {
                Child = new ExampleTargetChild { ChildName = "Original Child" },
                ChildList = new List<ExampleTargetChild>
                {
                    new ExampleTargetChild { ChildName = "Nested Child 1"},
                    new ExampleTargetChild { ChildName = "Nested Child 2"},
                    new ExampleTargetChild { ChildName = "Nested Child 3"},
                }
            };

            var from = new JsonPatchPath
            {
                TargetObject = exampleTarget.ChildList,
                IsCollection = true,
                TargetPropertyName = "0"
            };

            var path = new JsonPatchPath
            {
                TargetObject = exampleTarget,
                TargetPropertyName = "Child"
            };

            // When
            var result = _executor.Move(from, path);

            // Then
            exampleTarget.Child.ChildName.ShouldEqual("Nested Child 1");
            exampleTarget.ChildList[0].ChildName.ShouldEqual("Nested Child 2");
            exampleTarget.ChildList[1].ChildName.ShouldEqual("Nested Child 3");
            exampleTarget.ChildList.Count.ShouldEqual(2);
            result.Succeeded.ShouldBeTrue();
        }

        [Fact]
        public void Move_Returns_False_If_Cannot_Get_Value_From_Collection()
        {
            // Given
            var exampleTarget = new ExampleTarget
            {
                Child = new ExampleTargetChild { ChildName = "Original Child" },
                ChildList = new List<ExampleTargetChild>
                {
                    new ExampleTargetChild { ChildName = "Nested Child 1"},
                    new ExampleTargetChild { ChildName = "Nested Child 2"},
                    new ExampleTargetChild { ChildName = "Nested Child 3"},
                }
            };

            var from = new JsonPatchPath
            {
                TargetObject = exampleTarget.ChildList,
                IsCollection = true,
                TargetPropertyName = "9"
            };

            var path = new JsonPatchPath
            {
                TargetObject = exampleTarget,
                TargetPropertyName = "Child"
            };

            // When
            var result = _executor.Move(from, path);

            // Then
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldEqual("Could not find item 9 in the collection");
        }


        [Fact]
        public void Test_Fails_If_Simple_Objects_Do_Not_Match()
        {
            // Given
            var example = new ExampleTarget
            {
                Child = new ExampleTargetChild
                {
                    ChildName = "Child Property"
                }
            };

            var path = new JsonPatchPath
            {
                TargetPropertyName = "ChildName",
                TargetObject = example.Child
            };

            // When
            var result = _executor.Test(path, "This does not match");

            // Then
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldEqual("Test operation failed. 'ChildName' property did not match");
        }

        [Fact]
        public void Test_Returns_True_If_Simple_Objects_Match_When_Serialised()
        {
            // Given
            var example = new ExampleTarget
            {
                Child = new ExampleTargetChild
                {
                    ChildName = "Child Property"
                }
            };

            var path = new JsonPatchPath
            {
                TargetPropertyName = "ChildName",
                TargetObject = example.Child
            };

            // When
            var result = _executor.Test(path, "Child Property");

            // Then
            result.Succeeded.ShouldBeTrue();
        }

        [Fact]
        public void Test_Returns_True_If_Complex_Objects_Match_When_Serialised()
        {
            // Given
            var example = new ExampleTarget
            {
                Child = new ExampleTargetChild
                {
                    ChildName = "Child Property"
                }
            };

            var path = new JsonPatchPath
            {
                TargetPropertyName = "Child",
                TargetObject = example
            };

            // When
            var result = _executor.Test(path, new ExampleTargetChild
            {
                ChildName = "Child Property"
            });

            // Then
            result.Succeeded.ShouldBeTrue();
        }
        
        [Fact]
        public void Test_Throws_If_Complex_Objects_Do_Not_Match_When_Serialised()
        {
            // Given
            var example = new ExampleTarget
            {
                Child = new ExampleTargetChild
                {
                    ChildName = "Child Property"
                }
            };

            var path = new JsonPatchPath
            {
                TargetPropertyName = "Child",
                TargetObject = example
            };

            // When
            var result = _executor.Test(path, new ExampleTargetChild
            {
                ChildName = "No Matching Property"
            });

            // Then
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldEqual("Test operation failed. 'Child' property did not match");
        }

        [Fact]
        public void Test_Returns_True_If_Collections_Match_When_Serialised()
        {
            // Given
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
                TargetPropertyName = "ChildList",
                TargetObject = exampleTarget
            };

            // When
            var result =  _executor.Test(path, new[]
            {
                new ExampleTargetChild {ChildName = "Child 1"},
                new ExampleTargetChild {ChildName = "Child 2"},
                new ExampleTargetChild {ChildName = "Child 3"},
            });

            // Then
            result.Succeeded.ShouldBeTrue();
        }

        [Fact]
        public void Test_Returns_False_If_Collections_No_not_Match_When_Serialised()
        {
            // Given
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
                TargetPropertyName = "ChildList",
                TargetObject = exampleTarget
            };

            // When
            var result = _executor.Test(path, new[]
            {
                new ExampleTargetChild {ChildName = "Child 1"},
                new ExampleTargetChild {ChildName = "Child 3"},
                new ExampleTargetChild {ChildName = "Child 2"},
            });

            // Then
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldEqual("Test operation failed. 'ChildList' property did not match");
        }
    }
}
