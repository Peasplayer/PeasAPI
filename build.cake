var target = Argument("target", "Build");

var buildId = EnvironmentVariable("GITHUB_RUN_NUMBER");

var @ref = EnvironmentVariable("GITHUB_REF");
const string prefix = "refs/tags/";
var tag = !string.IsNullOrEmpty(@ref) && @ref.StartsWith(prefix) ? @ref.Substring(prefix.Length) : null;

Task("Build")
    .Does(() =>
{
    var settings = new DotNetCoreBuildSettings
    {
        Configuration = "Release",
        MSBuildSettings = new DotNetCoreMSBuildSettings()
    };
	
    if (buildId != null)
    {
        settings.VersionSuffix = "ci." + buildId.Split("+")[0];
    }
	
	//settings.VersionSuffix = buildId.Split("+")[0] + "--helo--" + buildId.Split("+")[1];
	
    foreach (var gamePlatform in new[] { "Steam", "Itch" })
    {
        settings.MSBuildSettings.Properties["GamePlatform"] = new[] { gamePlatform };
        DotNetCoreBuild(".", settings);
    }
});

RunTarget(target);
