using BuildSystem.Domain.Converters;
using System.Text.Json.Serialization;

namespace BuildSystem.Domain.Configuration;

public record PackageConfiguration(
    string Id,
    string BuildName,
    string[] Dependencies,
    string[] Triggers,
    PackageContent[] Contents);

[JsonConverter(typeof(PackageContentJsonConverter))]
public record PackageContent(
    string Source,
    string Target,
    string[] Filter);