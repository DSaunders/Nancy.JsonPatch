# Nancy.JsonPatch [![NuGet Version](https://img.shields.io/nuget/v/Nancy.JsonPatch.svg?style=flat)](https://www.nuget.org/packages/Nancy.JsonPatch/)

Adds JSON Patch functionality to Nancy

```csharp
Patch["/customer/{customerId}"] = _ =>
{
    Customer customer = _repository.Get(_.customerId);

    if (this.JsonPatch(customer))
        _repository.Save(customer);

    return HttpStatusCode.NoContent;
};
```

This is an example of a JSON Patch document:

```json
[
  { "op": "test", "path": "/a/b/c", "value": "foo" },
  { "op": "remove", "path": "/a/b/c" },
]
```

For a good introduction to JSON Patch, checkout [jsonpatch.com](http://jsonpatch.com/) or [RFC6902](http://tools.ietf.org/html/rfc6902).

## Installation

Install via NuGet:

```
PM > Install-Package Nancy.JsonPatch
```

## Getting Started

*Nancy.JsonPatch* adds an extension method to NancyModule, so usage is as simple as calling:
```csharp
this.JsonPatch(myThing);
```
from anywhere inside your route action.

*Nancy.JsonPatch* will then deserialize the request body and apply the patch operations to your object.

PATCH should be idempotent, so you only want to save the changes to you object if all operations in the document succeed. 

The .JsonPatch() extension method returns a `JsonPatchResult` indicating the result of the patch. This can be implicitly converted to a `bool`, allowing you to do this:

```csharp
  if (this.JsonPatch(myThing))
      _repository.Save(myThing);
```

## Advanced Error Handling

[RFC5789](http://tools.ietf.org/html/rfc5789#section-2.2) describes various error codes that should be returned depending on the result of the JSON Patch operation.
If you wish to handle failure in this way, the `JsonPatchResult` object returned from .JsonPatch() should give you the information you need:

```csharp
public class JsonPatchResult
{
    // True if the operation succeeds, otherwise false
    public bool Succeeded { get; set; }

    // In the event that an operation fails, this will contain a message describing the failure
    public string Message { get; set; }

    // Can be either:
    //  TestFailed - A 'test' operation failed
    //  CouldNotParseJson - The JSON Patch document in the request body is invalid
    //  CouldNotParsePath - A 'path' could not be mapped to a property on the object being patched
    //  CouldNotParseFrom - As above, but for the 'from' path used for copy/move operations
    //  OperationFailed - An operation in the document could not be completed (see the Message for details)
    public JsonPatchFailureReason FailureReason { get; set; }
}
```

For example:

```csharp
var patchResult = this.JsonPatch(myThing);
if (!patchResult)
{
    if (patchResult.FailureReason == JsonPatchFailureReason.CouldNotParsePath)
        return HttpStatusCode.Conflict;
    else
        return HttpStatusCode.UnprocessableEntity;
}

_repository.Save(myThing);
return HttpStatusCode.NoContent;

```


## JSON Patch operations in details

Since C# is strongly typed, our implementation has to be slightly different from that described in [RFC6902](http://tools.ietf.org/html/rfc6902).

This is how *Nancy.JsonPatch* implements each JsonPatch operation:

### Add

```json
[
  { "op": "add", "path": "/Name", "value": "Nancy" }
]
```

If the 'path' refers to a specific item in a collection (e.g. '/SomeCollection/2'), the item is added _before_ that item.

If the 'path' uses '-', to refer to the end of the collection (e.g '/SomeCollection/-') the item is added to the _end_ of the existing collection.

We can't add properties to objects in .NET. Therefore if the 'path' refers to a property that is not a collection (e.g. '/FirstName'), *Add* is treated as *Replace*.

### Remove

```json
[
  { "op": "remove", "path": "/Name" }
]
```

For collections, removes the item from the collection.

Since we can't remove properties from strongly-typed objects, if the 'path' does not refer to a collection then the target property will be set to it's default value (e.g. `null` for reference types)

### Replace

```json
[
  { "op": "replace", "path": "/SomeCollection/1", "value": { "FirstName" : "Nancy"  } }
]
```

Replaces the property referred to by the 'path' with the new 'value'.


### Copy

```json
[
  { "op": "copy", "path": "/Customer/1", "from": "/TopCustomer" }
]
```

Copies the value of one property to another.

Performs an *add* operation, taking the value of the item in the 'from' path and adding to the object in referred to by 'path'.

### Move

```json
[
  { "op": "move", "path": "/Customer/1", "from": "/TopCustomer" }
]
```

Moves the value of one property to another.

The same as *copy*, but performs a *remove* operation on the original 'path' first.

### Test

```json
[
  { "op": "test", "path": "/Customer/1/FirstName", "value": "Nancy" }
]
```

Tests that the value of the property referred to by the 'path' matches that specified in 'value'.
If the test fails, execution of the JSON Patch document stops and an error is returned from the .JsonPatch() method.


## So is this fine to use right now?
Yeah! If you do find any issues though, please let me know (or, even better, submit a pull request :smile: ).
