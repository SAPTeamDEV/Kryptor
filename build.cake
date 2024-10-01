using System.Runtime.InteropServices;

///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument("target", "Build-All");
var configuration = Argument("configuration", "Release");
var runtime = Argument("r", "");
string framework  = Argument("f", "");

string engineProjectFile = "src/Kryptor/Kryptor.csproj";
string clientProjectFile = "src/Kryptor.Client/Kryptor.Client.csproj";

string cliProjectFile = "cli/Kryptor.Cli/Kryptor.Cli.csproj";
string cliAotProjectFile = "cli/Kryptor.Cli.Native/Kryptor.Cli.Native.csproj";

///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////

Task("Restore-Engine")
	.Does(() => {
		DotNetRestore(engineProjectFile);
	});
	
Task("Build-Engine")
	.IsDependentOn("Restore-Engine")
	.Does(() => {
		DotNetBuild(engineProjectFile);
	});

Task("Restore-Client")
	.IsDependentOn("Restore-Engine")
	.Does(() => {
		DotNetRestore(clientProjectFile);
	});
	
Task("Build-Client")
	.IsDependentOn("Restore-Client")
	.Does(() => {
		DotNetBuild(clientProjectFile);
	});

Task("Restore-Cli")
	.IsDependentOn("Restore-Client")
	.Does(() => {
		DotNetRestore(cliProjectFile);
	});

Task("Build-Cli")
	.IsDependentOn("Restore-Cli")
	.Does(() => {
		DotNetBuild(cliProjectFile);
	});

Task("Restore-Aot")
	.IsDependentOn("Restore-Client")
	.Does(() => {
		if (string.IsNullOrEmpty(framework)){
			framework = "net8.0";
		}
		if (string.IsNullOrEmpty(runtime)){
			runtime = RuntimeInformation.RuntimeIdentifier;
		}

		DotNetRestore(cliAotProjectFile, new DotNetRestoreSettings(){
			Runtime = runtime,
		});
	});

Task("Build-Aot")
	.IsDependentOn("Restore-Aot")
	.Does(() => {
		DotNetPublish(cliAotProjectFile, new DotNetPublishSettings(){
			NoRestore = true,
			Runtime = runtime,
			Framework = framework,
			OutputDirectory = $"bin/Kryptor.Cli.Native/Aot/{configuration}/{framework}"
		});
	});

Task("Build-All")
	.IsDependentOn("Build-Engine")
	.IsDependentOn("Build-Client")
	.IsDependentOn("Build-Cli")
	.IsDependentOn("Build-Aot");

RunTarget(target);