using System.Runtime.InteropServices;

///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument("t", "Build-Cli");
var configuration = Argument("c", "Release");
var runtime = Argument("r", "");
var framework  = Argument("f", "");
var output  = Argument("o", "");

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

DotNetRestoreSettings GlobalRestoreSettings => new(){
	Runtime = runtime,
};

DotNetBuildSettings GlobalBuildSettings => new(){
	NoRestore = true,
	Configuration = configuration,
	Framework = framework,
	Runtime = runtime,
	OutputDirectory = output,
};

DotNetTestSettings GlobalTestSettings => new(){
	NoRestore = true,
	NoBuild = true,
	Verbosity = DotNetVerbosity.Normal,
	Configuration = configuration,
	Framework = framework,
	Runtime = runtime,
};

DotNetPublishSettings GlobalPublishSettings => new(){
	NoRestore = true,
	NoBuild = true,
	Configuration = configuration,
	Framework = framework,
	Runtime = runtime,
	OutputDirectory = output,
};

DotNetPackSettings GlobalPackSettings => new(){
	NoRestore = true,
	NoBuild = true,
	Configuration = configuration,
	Runtime = runtime,
	OutputDirectory = output,
};

Task("Restore-Engine")
	.Description("Restore kryptor engine dependencies")
	.Does(() => {
		DotNetRestore(engineProjectFile, GlobalRestoreSettings);
	});
	
Task("Build-Engine")
	.Description("Build kryptor engine")
	.IsDependentOn("Restore-Engine")
	.Does(() => {
		DotNetBuild(engineProjectFile, GlobalBuildSettings);
	});

Task("Pack-Engine")
	.Description("Create a NuGet package for kryptor engine")
	.IsDependentOn("Build-Engine")
	.Does(() => {
		DotNetPack(engineProjectFile, GlobalPackSettings);
	});

Task("Restore-Client")
	.Description("Restore kryptor client utilities dependencies")
	.IsDependentOn("Restore-Engine")
	.Does(() => {
		DotNetRestore(clientProjectFile, GlobalRestoreSettings);
	});
	
Task("Build-Client")
	.Description("Build kryptor client utilities")
	.IsDependentOn("Restore-Client")
	.Does(() => {
		DotNetBuild(clientProjectFile, GlobalBuildSettings);
	});

Task("Pack-Client")
	.Description("Create a NuGet package for kryptor client utilities")
	.IsDependentOn("Build-Client")
	.Does(() => {
		DotNetPack(clientProjectFile, GlobalPackSettings);
	});

Task("Restore-Cli")
	.Description("Restore kryptor command line interface dependencies")
	.IsDependentOn("Restore-Client")
	.Does(() => {
		DotNetRestore(cliProjectFile, GlobalRestoreSettings);
	});

Task("Build-Cli")
	.Description("Build kryptor command line interface")
	.IsDependentOn("Restore-Cli")
	.Does(() => {
		DotNetBuild(cliProjectFile, GlobalBuildSettings);
	});

Task("Pack-Cli")
	.Description("Create a NuGet package for kryptor command line interface")
	.IsDependentOn("Build-Cli")
	.Does(() => {
		DotNetPack(cliProjectFile, GlobalPackSettings);
	});

Task("Restore-Cli.Android")
	.Description("Restore kryptor cli for android dependencies")
	.IsDependentOn("Restore-Client")
	.Does(() => {
		DotNetRestore(cliAndroidProjectFile, GlobalRestoreSettings);
	});

Task("Build-Cli.Android")
	.Description("Build kryptor cli for android")
	.IsDependentOn("Restore-Cli.Android")
	.Does(() => {
		DotNetBuild(cliAndroidProjectFile, GlobalBuildSettings);
	});

Task("Restore-Cli.Legacy")
	.Description("Restore kryptor cli for .NET Framework dependencies")
	.IsDependentOn("Restore-Client")
	.Does(() => {
		DotNetRestore(cliLegacyProjectFile, GlobalRestoreSettings);
	});

Task("Build-Cli.Legacy")
	.Description("Build kryptor cli for .NET Framework")
	.IsDependentOn("Restore-Cli.Legacy")
	.Does(() => {
		DotNetBuild(cliLegacyProjectFile, GlobalBuildSettings);
	});

Task("Restore-Cli.Aot")
	.Description("Restore kryptor cli native AOT dependencies")
	.IsDependentOn("Restore-Client")
	.Does(() => {
		var restoreSettings = GlobalRestoreSettings;

		if (string.IsNullOrEmpty(restoreSettings.Runtime)){
			restoreSettings.Runtime = RuntimeInformation.RuntimeIdentifier;
		}

		DotNetRestore(cliAotProjectFile, restoreSettings);
	});

Task("Build-Cli.Aot")
	.Description("Build kryptor cli native AOT")
	.IsDependentOn("Restore-Cli.Aot")
	.Does(() => {
		var publishSettings = GlobalPublishSettings;

		if (string.IsNullOrEmpty(publishSettings.Runtime)){
			publishSettings.Runtime = RuntimeInformation.RuntimeIdentifier;
		}

		if (string.IsNullOrEmpty(publishSettings.Framework)){
			publishSettings.Framework = "net8.0";
		}

		if (publishSettings.OutputDirectory == null || string.IsNullOrEmpty(publishSettings.OutputDirectory.ToString())){
			publishSettings.OutputDirectory = $"bin/Kryptor.Cli.Native/Aot/{publishSettings.Configuration}/{publishSettings.Framework}";
		}

		publishSettings.NoBuild = false;

		DotNetPublish(cliAotProjectFile, publishSettings);
	});

Task("Restore-Engine.Test")
	.Description("Restore kryptor engine test dependencies")
	.IsDependentOn("Restore-Engine")
	.Does(() => {
		DotNetRestore(engineTestProjectFile, GlobalRestoreSettings);
	});

Task("Build-Engine.Test")
	.Description("Build kryptor engine test")
	.IsDependentOn("Restore-Engine.Test")
	.Does(() => {
		DotNetBuild(engineTestProjectFile, GlobalBuildSettings);
	});

Task("Test-Engine")
	.Description("Run kryptor engine tests")
	.IsDependentOn("Build-Engine.Test")
	.Does(() => {
		DotNetTest(engineTestProjectFile, GlobalTestSettings);
	});

Task("Build-All")
	.Description("Build all projects")
	.IsDependentOn("Build-Engine")
	.IsDependentOn("Build-Client")
	.IsDependentOn("Build-Cli")
	.IsDependentOn("Build-Cli.Android")
	.IsDependentOn("Build-Cli.Legacy")
	.IsDependentOn("Build-Cli.Aot");

Task("Pack-All")
	.Description("Create NuGet package for all projects")
	.IsDependentOn("Pack-Engine")
	.IsDependentOn("Pack-Client")
	.IsDependentOn("Pack-Cli");
	
Task("Test-All")
	.Description("Run all tests")
	.IsDependentOn("Test-Engine");

RunTarget(target);