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

        var extensionPackages = packagesList.GroupBy(p => new { p.Identifier, p.Version })
        .Select(x =>
            new ExtensionPackage(x.Key.Identifier, x.Key.Version,
                x.Where(r => r.Identifier == x.Key.Identifier).ToList()
            )).OrderByDescending(r => GetVersion(r.Version));

        return extensionPackages;
    }

    public async Task<IEnumerable<ExtensionPackage>> GetExtensionsAsync(bool prerelease=false)
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
                string fileOnServer = Path.Combine(Utilities.OutputDirectory(_environment), extension.Location);
                Directory.Delete(fileOnServer, true);
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
