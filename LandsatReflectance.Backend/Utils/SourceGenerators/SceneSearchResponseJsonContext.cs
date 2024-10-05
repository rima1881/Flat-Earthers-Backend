using System.Text.Json.Serialization;
using LandsatReflectance.Backend.Models.UsgsApi.Endpoints;
using LandsatReflectance.Backend.Models.UsgsApi.Types;

namespace LandsatReflectance.Backend.Utils.SourceGenerators;

[JsonSerializable(typeof(SceneSearchResponse))]
public partial class SceneSearchResponseJsonContext : JsonSerializerContext
{ }