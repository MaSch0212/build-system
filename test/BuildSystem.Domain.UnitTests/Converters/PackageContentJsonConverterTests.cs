using BuildSystem.Domain.Configuration;
using BuildSystem.Domain.Converters;
using System.Text.Json;

#pragma warning disable SA1602 // Enumeration items should be documented
#pragma warning disable SA1649 // File name should match first type name

namespace BuildSystem.Domain.UnitTests.Converters.PackageContentJsonConverterTests;

[TestClass]
public class WithNamingPolicy : PackageContentJsonConverterTests
{
    public WithNamingPolicy()
        : base(true)
    {
    }

    protected override string SourceProp => "source";
    protected override string TargetProp => "target";
    protected override string FilterProp => "filter";
}

[TestClass]
public class WithoutNamingPolicy : PackageContentJsonConverterTests
{
    public WithoutNamingPolicy()
        : base(false)
    {
    }

    protected override string SourceProp => "Source";
    protected override string TargetProp => "Target";
    protected override string FilterProp => "Filter";
}

public abstract class PackageContentJsonConverterTests : TestClassBase
{
    private readonly JsonSerializerOptions _serializerOptions;

    public PackageContentJsonConverterTests(bool withNamingPolicy)
    {
        _serializerOptions = new JsonSerializerOptions
        {
            WriteIndented = false,
        };

        if (withNamingPolicy)
            _serializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    }

    public enum EmptyFilterType
    {
        Null,
        EmptyArray,
        DefaultFilterUnixStyle,
        DefaultFilterWindowsStyle,
    }

    protected abstract string SourceProp { get; }
    protected abstract string TargetProp { get; }
    protected abstract string FilterProp { get; }

    [TestMethod]
    public void Write_WithSourceTargetAndTwoFilters()
    {
        var expectedJson = $"{{\"{SourceProp}\":\"MySource\",\"{TargetProp}\":\"MyTarget\",\"{FilterProp}\":[\"Filter1\",\"Filter2\"]}}";
        var input = new PackageContent("MySource", "MyTarget", new[] { "Filter1", "Filter2" });

        var actualJson = JsonSerializer.Serialize(input, _serializerOptions);

        Assert.AreEqual(expectedJson, actualJson);
    }

    [TestMethod]
    public void Write_WithSourceTargetAndOneFilter()
    {
        var expectedJson = $"{{\"{SourceProp}\":\"MySource\",\"{TargetProp}\":\"MyTarget\",\"{FilterProp}\":\"MyFilter\"}}";
        var input = new PackageContent("MySource", "MyTarget", new[] { "MyFilter" });

        var actualJson = JsonSerializer.Serialize(input, _serializerOptions);

        Assert.AreEqual(expectedJson, actualJson);
    }

    [TestMethod]
    [DataRow(EmptyFilterType.Null)]
    [DataRow(EmptyFilterType.EmptyArray)]
    [DataRow(EmptyFilterType.DefaultFilterUnixStyle)]
    [DataRow(EmptyFilterType.DefaultFilterWindowsStyle)]
    public void Write_WithSourceAndTarget_WithoutFilter(EmptyFilterType filterType)
    {
        var expectedJson = $"{{\"{SourceProp}\":\"MySource\",\"{TargetProp}\":\"MyTarget\"}}";
        var filter = filterType switch
        {
            EmptyFilterType.EmptyArray => Array.Empty<string>(),
            EmptyFilterType.DefaultFilterUnixStyle => new[] { "**/*" },
            EmptyFilterType.DefaultFilterWindowsStyle => new[] { "**\\*" },
            _ => null!,
        };
        var input = new PackageContent("MySource", "MyTarget", filter);

        var actualJson = JsonSerializer.Serialize(input, _serializerOptions);

        Assert.AreEqual(expectedJson, actualJson);
    }

    [TestMethod]
    [DataRow(null, DisplayName = "Null Target")]
    [DataRow("", DisplayName = "Empty Target")]
    [DataRow(".", DisplayName = "\".\" Target")]
    [DataRow("./", DisplayName = "\"./\" Target")]
    [DataRow(".\\", DisplayName = "\".\\\" Target")]
    public void Write_WithSource_WithoutTargetAndFilter(string? target)
    {
        const string expectedJson = "\"MySource\"";
        var input = new PackageContent("MySource", target!, Array.Empty<string>());

        var actualJson = JsonSerializer.Serialize(input, _serializerOptions);

        Assert.AreEqual(expectedJson, actualJson);
    }

    [TestMethod]
    public void Read_WithSourceTargetAndTwoFilters()
    {
        var expectedContent = new PackageContent("MySource", "MyTarget", new[] { "Filter1", "Filter2" });
        var input = $"{{\"{SourceProp}\":\"MySource\",\"{TargetProp}\":\"MyTarget\",\"{FilterProp}\":[\"Filter1\",\"Filter2\"]}}";

        var actualContent = JsonSerializer.Deserialize<PackageContent>(input, _serializerOptions);

        AssertContent(expectedContent, actualContent);
    }

    [TestMethod]
    public void Read_WithSourceTargetAndOneFilter()
    {
        var expectedContent = new PackageContent("MySource", "MyTarget", new[] { "MyFilter" });
        var input = $"{{\"{SourceProp}\":\"MySource\",\"{TargetProp}\":\"MyTarget\",\"{FilterProp}\":\"MyFilter\"}}";

        var actualContent = JsonSerializer.Deserialize<PackageContent>(input, _serializerOptions);

        AssertContent(expectedContent, actualContent);
    }

    [TestMethod]
    public void Read_WithSourceAndTarget_WithoutFilter()
    {
        var expectedContent = new PackageContent("MySource", "MyTarget", new[] { "**/*" });
        var input = $"{{\"{SourceProp}\":\"MySource\",\"{TargetProp}\":\"MyTarget\"}}";

        var actualContent = JsonSerializer.Deserialize<PackageContent>(input, _serializerOptions);

        AssertContent(expectedContent, actualContent);
    }

    [TestMethod]
    public void Read_WithSource_WithoutTargetAndFilter()
    {
        const string input = "\"MySource\"";
        var expectedContent = new PackageContent("MySource", ".", new[] { "**/*" });

        var actualContent = JsonSerializer.Deserialize<PackageContent>(input, _serializerOptions);

        AssertContent(expectedContent, actualContent);
    }

    private void AssertContent(PackageContent? expectedContent, PackageContent? actualContent)
    {
        Assert.AreEqual(expectedContent is null, actualContent is null, "Is null");
        if (expectedContent is null || actualContent is null)
            return;

        Assert.AreEqual(expectedContent.Source, actualContent.Source, "Source");
        Assert.AreEqual(expectedContent.Target, actualContent.Target, "Target");
        Assert.AreCollectionsEqual(expectedContent.Filter, actualContent.Filter, "Filter");
    }
}
