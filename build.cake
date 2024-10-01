using System.Runtime.InteropServices;

///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument("t", "Build-Cli");
var configuration = Argument("c", "Release");
var runtime = Argument("r", "");
string framework  = Argument("f", "");

string engineProjectFile = "src/Kryptor/Kryptor.csproj";
string clientProjectFile = "src/Kryptor.Client/Kryptor.Client.csproj";

string cliProjectFile = "cli/Kryptor.Cli/Kryptor.Cli.csproj";
string cliAotProjectFile = "cli/Kryptor.Cli.Native/Kryptor.Cli.Native.csproj";
string cliAndroidProjectFile = "cli/Kryptor.Cli.Android/Kryptor.Cli.Android.csproj";
string cliLegacyProjectFile = "cli/Kryptor.Cli.Legacy/Kryptor.Cli.Legacy.csproj";

string engineTestProjectFile = "test/Kryptor.Tests/Kryptor.Tests.csproj";

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
		DotNetBuild(engineProjectFile, new DotNetBuildSettings(){
			NoRestore = true,
		});
	});

Task("Restore-Client")
	.IsDependentOn("Restore-Engine")
	.Does(() => {
		DotNetRestore(clientProjectFile);
	});
	
Task("Build-Client")
	.IsDependentOn("Restore-Client")
	.Does(() => {
		DotNetBuild(clientProjectFile, new DotNetBuildSettings(){
			NoRestore = true,
		});
	});

Task("Restore-Cli")
	.IsDependentOn("Restore-Client")
	.Does(() => {
		DotNetRestore(cliProjectFile);
	});

Task("Build-Cli")
	.IsDependentOn("Restore-Cli")
	.Does(() => {
		DotNetBuild(cliProjectFile, new DotNetBuildSettings(){
			NoRestore = true,
		});
	});

Task("Restore-Android")
	.IsDependentOn("Restore-Client")
	.Does(() => {
		DotNetRestore(cliAndroidProjectFile);
	});

Task("Build-Android")
	.IsDependentOn("Restore-Android")
	.Does(() => {
		DotNetBuild(cliAndroidProjectFile, new DotNetBuildSettings(){
			NoRestore = true,
		});
	});

Task("Restore-Legacy")
	.IsDependentOn("Restore-Client")
	.Does(() => {
		DotNetRestore(cliLegacyProjectFile);
	});

Task("Build-Legacy")
	.IsDependentOn("Restore-Legacy")
	.Does(() => {
		DotNetBuild(cliLegacyProjectFile, new DotNetBuildSettings(){
			NoRestore = true,
		});
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

Task("Restore-EngineTest")
	.Description("Restore kryptor engine test project dependencies")
	.IsDependentOn("Restore-Engine")
	.Does(() => {
		DotNetRestore(engineTestProjectFile);
	});

Task("Build-EngineTest")
	.Description("Compile kryptor engine test project")
	.IsDependentOn("Restore-EngineTest")
	.Does(() => {
		DotNetBuild(engineTestProjectFile, new DotNetBuildSettings(){
			NoRestore = true,
		});
	});

Task("Test-Engine")
	.Description("Run kryptor engine tests")
	.IsDependentOn("Build-EngineTest")
	.Does(() => {
		DotNetTest(engineTestProjectFile, new DotNetTestSettings(){
			NoRestore = true,
			NoBuild = true,
			Verbosity = DotNetVerbosity.Normal
		});
	});

Task("Build-All")
	.IsDependentOn("Build-Engine")
	.IsDependentOn("Build-Client")
	.IsDependentOn("Build-Cli")
	.IsDependentOn("Build-Android")
	.IsDependentOn("Build-Legacy")
	.IsDependentOn("Build-Aot");

RunTarget(target);