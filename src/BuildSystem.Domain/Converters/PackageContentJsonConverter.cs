using BuildSystem.Domain.Configuration;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BuildSystem.Domain.Converters;

public class PackageContentJsonConverter : JsonConverter<PackageContent>
{
    private static readonly Regex DefaultTargetRegex = new(@"^(\.[\\\/]?)?$", RegexOptions.Compiled);
    private static readonly Regex DefaultFilterRegex = new(@"^\*\*[\\\/]\*$", RegexOptions.Compiled);

    public override PackageContent? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string? source = null, target = null;
        List<string> filters = new();

        if (reader.TokenType == JsonTokenType.StartObject)
        {
            var comparison = options.PropertyNameCaseInsensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            var sourcePropertyName = GetPropertyName(nameof(PackageContent.Source), options);
            var targetPropertyName = GetPropertyName(nameof(PackageContent.Target), options);
            var filterPropertyName = GetPropertyName(nameof(PackageContent.Filter), options);

            while (reader.Read())
            {
                if (reader.TokenType != JsonTokenType.PropertyName)
                    continue;

                var propertyName = reader.GetString();
                reader.Read();
                if (string.Equals(propertyName, sourcePropertyName, comparison))
                {
                    source = reader.GetString();
                }
                else if (string.Equals(propertyName, targetPropertyName, comparison))
                {
                    target = reader.GetString();
                }
                else if (string.Equals(propertyName, filterPropertyName, comparison))
                {
                    if (reader.TokenType == JsonTokenType.StartArray)
                    {
                        reader.Read();
                        while (reader.TokenType != JsonTokenType.EndArray)
                        {
                            if (reader.TokenType == JsonTokenType.String)
                                filters.Add(reader.GetString()!);
                            else if (reader.TokenType != JsonTokenType.Null)
                                throw new JsonException("Invalid token in array for property \"filter\" of package content.");
                            reader.Read();
                        }
                    }
                    else if (reader.TokenType == JsonTokenType.String || reader.TokenType == JsonTokenType.Null)
                    {
                        var filter = reader.GetString();
                        if (filter is not null)
                            filters.Add(filter);
                    }
                    else
                    {
                        throw new JsonException("Invalid token for property \"filter\" of package content.");
                    }
                }
                else if (reader.TokenType == JsonTokenType.StartObject)
                {
                    reader.Skip();
                }
            }

        }
        else if (reader.TokenType == JsonTokenType.String)
        {
            source = reader.GetString();
        }
        else if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }
        else
        {
            throw new JsonException("Invalid token for package content.");
        }

        return new PackageContent(
            source ?? throw new InvalidOperationException("The \"source\" property needs to be provided and cannot be null in package content."),
            target ?? ".",
            filters.Count == 0 ? new[] { "**/*" } : filters.ToArray());
    }

    public override void Write(Utf8JsonWriter writer, PackageContent value, JsonSerializerOptions options)
    {
        bool isEmptyTarget = value.Target is null || DefaultTargetRegex.IsMatch(value.Target);
        bool isEmptyFilter =
            value.Filter is null ||
            value.Filter.Length == 0 ||
            (value.Filter.Length == 1 && DefaultFilterRegex.IsMatch(value.Filter[0]));

        if (isEmptyTarget && isEmptyFilter)
        {
            writer.WriteStringValue(value.Source);
            return;
        }

        writer.WriteStartObject();
        writer.WriteString(GetPropertyName(nameof(PackageContent.Source), options), value.Source);

        if (!isEmptyTarget)
            writer.WriteString(GetPropertyName(nameof(PackageContent.Target), options), value.Target);

        if (!isEmptyFilter)
        {
            if (value.Filter!.Length == 1)
            {
                writer.WritePropertyName(GetPropertyName(nameof(PackageContent.Filter), options));
                writer.WriteStringValue(value.Filter[0]);
            }
            else
            {
                writer.WritePropertyName(GetPropertyName(nameof(PackageContent.Filter), options));
                writer.WriteStartArray();
                foreach (var filter in value.Filter)
                    writer.WriteStringValue(filter);
                writer.WriteEndArray();
            }
        }

        writer.WriteEndObject();
    }

    private static string GetPropertyName(string originalName, JsonSerializerOptions options)
    {
        return options.PropertyNamingPolicy is null
            ? originalName
            : options.PropertyNamingPolicy.ConvertName(originalName);
    }
}
