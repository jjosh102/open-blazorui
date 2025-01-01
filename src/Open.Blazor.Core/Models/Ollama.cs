using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Open.Blazor.Core.Models;

/// <summary>
///     https://github.com/jmorganca/ollama/blob/main/docs/api.md#list-local-models
/// </summary>
public class Ollama
{
    [JsonPropertyName("models")]
	public OllamaModel[] Models { get; set; } = null!;
}

[DebuggerDisplay("{Name}")]
public class OllamaModel
{
    [JsonPropertyName("name")]
	public string Name { get; set; } = null!;
	
	[JsonPropertyName("modified_at")]
	public DateTime ModifiedAt { get; set; }

	[JsonPropertyName("size")]
	public long Size { get; set; }

	[JsonPropertyName("digest")]
	public string Digest { get; set; } = null!;

	[JsonPropertyName("details")]
	public OllamaModelDetails Details { get; set; } = null!;
}

public class OllamaModelDetails
{
	
	[JsonPropertyName("parent_model")]
	public string? ParentModel { get; set; }

	[JsonPropertyName("format")]
	public string Format { get; set; } = null!;

	[JsonPropertyName("family")]
	public string Family { get; set; } = null!;

	[JsonPropertyName("families")]
	public string[]? Families { get; set; }

	[JsonPropertyName("parameter_size")]
	public string ParameterSize { get; set; } = null!;

	[JsonPropertyName("quantization_level")]
	public string QuantizationLevel { get; set; } = null!;
}