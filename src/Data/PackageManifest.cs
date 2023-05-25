using AntDesign;
using System.Xml.Serialization;
using Semver;
[XmlRoot("PackageManifest", Namespace = "http://schemas.microsoft.com/developer/vsx-schema/2011")]
public class ExtensionManifest
{
    [XmlElement("Metadata")]
    public Metadata? Metadata { get; set; }
    [XmlArray("Assets")]
    [XmlArrayItem("Asset")]
    public List<Asset>? Assets { get; set; }
    public string Identifier => $"{Metadata?.Identity?.Publisher}.{Metadata?.Identity?.Id}";
    public bool IsPreRelease
    {
        get
        {
            bool isPreRelease = Convert.ToBoolean(Metadata?.Properties?.FirstOrDefault(p => p.Id == "Microsoft.VisualStudio.Code.PreRelease")?.Value);
            var success = SemVersion.TryParse(Version, SemVersionStyles.Strict, out var semVersion);

            return isPreRelease || (success && semVersion.IsPrerelease);
        }
    }
    public string Version => Metadata?.Identity?.Version ?? "0.0.0";
    public string Target => Metadata?.Identity?.TargetPlatform ?? "any";
    public string Location { get; set; } = string.Empty;
    public string[]? Categories => Metadata?.CategoryString?.Split(',');
    public string DisplayName => Metadata?.DisplayName ?? Identifier;
    public string? Description => Metadata?.Description ?? string.Empty;
    public string? RelativeIconPath => Assets?.FirstOrDefault(a => a.AssetType == "Microsoft.VisualStudio.Services.Icons.Default")?.Path;

    public string? PackageIconPath => (RelativeIconPath is null) ? "/box.png" : Path.Combine("output", Location, RelativeIconPath);
    public string? RelativeReadmePath => Assets?.FirstOrDefault(a => a.AssetType == "Microsoft.VisualStudio.Services.Content.Details")?.Path;

    public string? ReadmePath => RelativeReadmePath is null ? null : Path.Combine("output", Location, RelativeReadmePath);

    public string GalleryFlags => Metadata?.GalleryFlags ?? "Public";

    public bool IsPreview
    {
        get
        {
            return (GalleryFlags.ToLowerInvariant().Contains("preview"));
        }
    }
}

public class Metadata
{
    [XmlElement("Identity")]
    public Identity? Identity { get; set; }
    [XmlElement("DisplayName")]
    public string? DisplayName { get; set; }
    [XmlElement("Categories")]
    public string? CategoryString { get; set; }
    [XmlElement("Description")]
    public string? Description { get; set; } = string.Empty;
    public string[]? Categories => CategoryString?.Split(',');
    [XmlElement("GalleryFlags")]
    public string? GalleryFlags { get; set; }
    [XmlArray("Properties")]
    [XmlArrayItem("Property")]
    public List<Property>? Properties { get; set; }
}

public class Property
{
    [XmlAttribute("Id")]
    public string? Id { get; set; }

    [XmlAttribute("Value")]
    public string? Value { get; set; }
}

public class Identity
{
    [XmlAttribute("Language")]
    public string? Language { get; set; }

    [XmlAttribute("Id")]
    public string? Id { get; set; }

    [XmlAttribute("Version")]
    public string? Version { get; set; }

    [XmlAttribute("Publisher")]
    public string? Publisher { get; set; }

    [XmlAttribute("TargetPlatform")]
    public string? TargetPlatform { get; set; }
}

public class Asset
{
    [XmlAttribute("Type")]
    public string? AssetType { get; set; }

    [XmlAttribute("Path")]
    public string? Path { get; set; }
}