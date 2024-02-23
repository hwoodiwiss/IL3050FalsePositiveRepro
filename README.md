This repo provides an example of a use case in which I believe the ILC analyzers are producing false-positive trim warnings.

Uses: https://github.com/hwoodiwiss/dotnet-sdk/tree/hwoodiwiss-kiota-update to ensure that the latest Kiota libraries are used.

When running `.\build\publish-aot.ps1 -RuntimeIdentifier win-x64` (RID is not important, but I'm using `win-x64`), the following warnings are produced:

```
ILC : Trim analysis warning IL2075: Microsoft.Kiota.Serialization.Json.JsonSerializationWriter.WriteNonParsableObjectValue<T>(String,!!0):
 'this' argument does not satisfy 'DynamicallyAccessedMemberTypes.PublicProperties' in call to 'System.Type.GetProperties()'. The return v
alue of method 'System.Object.GetType()' does not have matching annotations. The source value must declare at least the same requirements
as those declared on the target location it is assigned to. [R:\source\repos\IL3050FalsePositiveRepro\IL3050FalsePositiveRepro.csproj]

ILC : AOT analysis warning IL3050: GitHub.Models.PullRequestReview.<GetFieldDeserializers>b__58_0(IParseNode): Using member 'System.Enum.G
etValues(Type)' which has 'RequiresDynamicCodeAttribute' can break functionality when AOT compiling. It might not be possible to create an
 array of the enum type at runtime. Use the GetValues<TEnum> overload or the GetValuesAsUnderlyingType method instead. [R:\source\repos\IL
3050FalsePositiveRepro\IL3050FalsePositiveRepro.csproj]

ILC : AOT analysis warning IL3050: GitHub.Repos.Item.Item.Pulls.Item.Reviews.ReviewsPostRequestBody.<GetFieldDeserializers>b__22_3(IParseN
ode): Using member 'System.Enum.GetValues(Type)' which has 'RequiresDynamicCodeAttribute' can break functionality when AOT compiling. It m
ight not be possible to create an array of the enum type at runtime. Use the GetValues<TEnum> overload or the GetValuesAsUnderlyingType me
thod instead. [R:\source\repos\IL3050FalsePositiveRepro\IL3050FalsePositiveRepro.csproj]

ILC : Trim analysis warning IL2075: Microsoft.Kiota.Abstractions.RequestInformation.GetEnumName<T>(!!0): 'this' argument does not satisfy
'DynamicallyAccessedMemberTypes.PublicFields' in call to 'System.Type.GetField(String)'. The return value of method 'System.Object.GetType
()' does not have matching annotations. The source value must declare at least the same requirements as those declared on the target locat
ion it is assigned to. [R:\source\repos\IL3050FalsePositiveRepro\IL3050FalsePositiveRepro.csproj]
```
However, `System.Enum.GetValues(Type)` is not called from the referenced methods.
An example is `GitHub.Models.PullRequestReview.<GetFieldDeserializers>b__58_0(IParseNode)`, which is:
```csharp
[NullableContext(1)]
[CompilerGenerated]
private void <GetFieldDeserializers>b__58_0(IParseNode n)
{
  this.AuthorAssociation = n.GetEnumValue<GitHub.Models.AuthorAssociation>();
}
```
The implementations of `GetEnumValue<GitHub.Models.AuthorAssociation>()` are in the Kiota serialization libraries,
and I can't find anywhere in any of them that `System.Enum.GetValues(Type)` is called. I've also tried to find
transient calls looking through the source of called reflection methods.

Kiota GetEnumValue<T> implementations:
kiota-serialization-form-dotnet - [FormParseNode.cs](https://github.com/microsoft/kiota-serialization-form-dotnet/blob/2d9b8a3476b0619478e19fa920baebc303158037/src/FormParseNode.cs#L206)
kiota-serialization-json-dotnet - [JsonParseNode.cs](https://github.com/microsoft/kiota-serialization-json-dotnet/blob/7492e10c9f29cd63c4c3a6115698a9db1fc71896/src/JsonParseNode.cs#L156)
kiota-serialization-text-dotnet - [TextParseNode.cs](https://github.com/microsoft/kiota-serialization-text-dotnet/blob/b6109dd61da7c8d3aa6c77b40d64891b0c218ec7/src/TextParseNode.cs#L81)
