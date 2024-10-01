///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

string CliProjectFile = "cli/Kryptor.Cli/Kryptor.Cli.csproj";
string CliAotProjectFile = "cli/Kryptor.Cli.Native/Kryptor.Cli.Native.csproj";

///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////

Task("AOT")
	.Does(() =>
{
	DotNetRestore(CliAotProjectFile);
	DotNetPublish(CliAotProjectFile, new DotNetPublishSettings(){
		NoRestore = true,
		Framework = "net8.0",
		Verbosity = DotNetVerbosity.Normal,
		OutputDirectory = "bin/AOT"
	});
});

Task("Restore-Cli")
	.Does(() => {
		DotNetRestore(CliProjectFile);
	});

Task("Build-Cli")
	.IsDependentOn("Restore-Cli")
	.Does(() => {
		DotNetBuild(CliProjectFile);
	});

Task("Default")
	.IsDependentOn("Build-Cli");

RunTarget(target);