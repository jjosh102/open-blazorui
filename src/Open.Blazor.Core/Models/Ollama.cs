using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Open.Blazor.Core.Models;

/// <summary>
///     https://github.com/jmorganca/ollama/blob/main/docs/api.md#list-local-models
/// </summary>
public class Ollama
{
    [JsonPropertyName("models")] public IReadOnlyList<OllamaModel> Models { get; set; } = [];
}

[DebuggerDisplay("{Name}")]
public class OllamaModel
{
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;

    [JsonPropertyName("modified_at")] public DateTime ModifiedAt { get; set; }

    [JsonPropertyName("size")] public long Size { get; set; }

    [JsonPropertyName("digest")] public string Digest { get; set; } = string.Empty;

    [JsonPropertyName("details")] public OllamaModelDetails? Details { get; set; }
}

public class OllamaModelDetails
{
    [JsonPropertyName("parent_model")] public string ParentModel { get; set; } = string.Empty;

    [JsonPropertyName("format")] public string Format { get; set; } = string.Empty;

    [JsonPropertyName("family")] public string Family { get; set; } = string.Empty;

    [JsonPropertyName("families")] public IReadOnlyList<string>? Families { get; set; }

    [JsonPropertyName("parameter_size")] public string ParameterSize { get; set; } = string.Empty;

    [JsonPropertyName("quantization_level")]
    public string QuantizationLevel { get; set; } = string.Empty;
}