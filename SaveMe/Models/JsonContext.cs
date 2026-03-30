using System.Text.Json.Serialization;
using SaveMe.Models;

[JsonSerializable(typeof(Snapshots))]
[JsonSerializable(typeof(CommitFile))]
[JsonSerializable(typeof(CommitFile[]))]
[JsonSerializable(typeof(Snapshots[]))]
[JsonSerializable(typeof(AppSettings))]
public partial class JsonContext : JsonSerializerContext
{
}
