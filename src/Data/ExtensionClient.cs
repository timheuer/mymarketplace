using Semver;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

public class ExtensionClient
{
    public ExtensionClient(
    [NotNull] IDatabaseService databaseService,
    IWebHostEnvironment environment,
    IPackageReader manifestReader,
    ILogger<ExtensionController> logger,
    IConfiguration config)
    {
        _databaseService = databaseService;
        _environment = environment;
        _manifestReader = manifestReader;
        _logger = logger;
        _config = config;
    }
    readonly IDatabaseService _databaseService;
    readonly IPackageReader _manifestReader;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<ExtensionController> _logger;
    private readonly IConfiguration _config;

    public async Task<IEnumerable<ExtensionPackage>> GetPreReleaseExtensionPackagesAsync(bool prerelease)
    {
        var packagesList = prerelease ? _databaseService.Query()
        : _databaseService.Find(p => !p.IsPreRelease);

        var extensionPackages = PackageExtensions(packagesList);

        return extensionPackages;
    }

    public async Task<IEnumerable<ExtensionPackage>> GetExtensionPackagesAsync(string[] identifiers)
    {
        var packagesList = _databaseService.Find(p => identifiers.Contains(p.Identifier)).DistinctBy(p => p.Identifier);
        var extensionPackages = PackageExtensions(packagesList);
        return extensionPackages;
    }

    IOrderedEnumerable<ExtensionPackage> PackageExtensions(IEnumerable<ExtensionManifest> packagesList)
    {
        return packagesList.GroupBy(p => new { p.Identifier, p.Version })
        .Select(x =>
            new ExtensionPackage(x.Key.Identifier, x.Key.Version,
                x.Where(r => r.Identifier == x.Key.Identifier).ToList()
            )).OrderByDescending(r => GetVersion(r.Version));
    }



    public async Task<IEnumerable<ExtensionPackage>> GetExtensionsAsync(bool prerelease = false)
    {
        var packagesList = await GetPreReleaseExtensionPackagesAsync(prerelease);

        return packagesList;
    }

    public async Task<IEnumerable<ExtensionPackage>> GetExtensionDetailsAsync(string identifier)
    {
        var packagesList = _databaseService.Find(p => p.Identifier == identifier);

        var extensionPackages = packagesList.GroupBy(p => new { p.Identifier, p.Version })
        .Select(x =>
            new ExtensionPackage(x.Key.Identifier, x.Key.Version,
                x.Where(r => r.Identifier == x.Key.Identifier).ToList()
            )).OrderByDescending(r => GetVersion(r.Version));

        return extensionPackages;
    }

    public async Task<bool> DeletePackage(string identifier)
    {
        var package = await GetExtensionDetailsAsync(identifier);
        bool deletedFromDb = _databaseService.Delete(identifier);

        foreach (var pkg in package)
        {
            foreach (var extension in pkg.Extensions)
            {
                // delete the folder from the server
                string directoryOnServer = Path.Combine(Utilities.OutputDirectory(_environment), extension.Location);
                if (Directory.Exists(directoryOnServer))
                    Directory.Delete(directoryOnServer, true);

                // delete the vsix from the server
                string fileOnServer = $"{Path.Combine(Utilities.OutputDirectory(_environment), extension.Location)}.vsix";
                if (File.Exists(fileOnServer))
                    File.Delete(fileOnServer);
            }
        }

        return deletedFromDb;
    }

    SemVersion GetVersion(string version)
    {
        var ver = SemVersion.Parse("0.0.0", SemVersionStyles.Any);
        if (string.IsNullOrWhiteSpace(version))
            return ver;

        SemVersion.TryParse(version, SemVersionStyles.Any, out ver);
        return ver;
    }
}
