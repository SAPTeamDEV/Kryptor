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

DotNetRestoreSettings GlobalRestoreSettings => new(){
	Runtime = runtime,
};

DotNetBuildSettings GlobalBuildSettings => new(){
	NoRestore = true,
	Configuration = configuration,
	Framework = framework,
	Runtime = runtime,
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
};

DotNetPackSettings GlobalPackSettings => new(){
	NoRestore = true,
	NoBuild = true,
	Configuration = configuration,
	Runtime = runtime,
};

Task("Restore-Engine")
	.Does(() => {
		DotNetRestore(engineProjectFile, GlobalRestoreSettings);
	});
	
Task("Build-Engine")
	.IsDependentOn("Restore-Engine")
	.Does(() => {
		DotNetBuild(engineProjectFile, GlobalBuildSettings);
	});

Task("Pack-Engine")
	.IsDependentOn("Build-Engine")
	.Does(() => {
		DotNetPack(engineProjectFile, GlobalPackSettings);
	});

Task("Restore-Client")
	.IsDependentOn("Restore-Engine")
	.Does(() => {
		DotNetRestore(clientProjectFile, GlobalRestoreSettings);
	});
	
Task("Build-Client")
	.IsDependentOn("Restore-Client")
	.Does(() => {
		DotNetBuild(clientProjectFile, GlobalBuildSettings);
	});

Task("Pack-Client")
	.IsDependentOn("Build-Client")
	.Does(() => {
		DotNetPack(clientProjectFile, GlobalPackSettings);
	});

Task("Restore-Cli")
	.IsDependentOn("Restore-Client")
	.Does(() => {
		DotNetRestore(cliProjectFile, GlobalRestoreSettings);
	});

Task("Build-Cli")
	.IsDependentOn("Restore-Cli")
	.Does(() => {
		DotNetBuild(cliProjectFile, GlobalBuildSettings);
	});

Task("Pack-Cli")
	.IsDependentOn("Build-Cli")
	.Does(() => {
		DotNetPack(cliProjectFile, GlobalPackSettings);
	});

Task("Restore-Cli.Android")
	.IsDependentOn("Restore-Client")
	.Does(() => {
		DotNetRestore(cliAndroidProjectFile, GlobalRestoreSettings);
	});

Task("Build-Cli.Android")
	.IsDependentOn("Restore-Cli.Android")
	.Does(() => {
		DotNetBuild(cliAndroidProjectFile, GlobalBuildSettings);
	});

Task("Restore-Cli.Legacy")
	.IsDependentOn("Restore-Client")
	.Does(() => {
		DotNetRestore(cliLegacyProjectFile, GlobalRestoreSettings);
	});

Task("Build-Cli.Legacy")
	.IsDependentOn("Restore-Cli.Legacy")
	.Does(() => {
		DotNetBuild(cliLegacyProjectFile, GlobalBuildSettings);
	});

Task("Restore-Cli.Aot")
	.IsDependentOn("Restore-Client")
	.Does(() => {
		var restoreSettings = GlobalRestoreSettings;

		if (string.IsNullOrEmpty(restoreSettings.Runtime)){
			restoreSettings.Runtime = RuntimeInformation.RuntimeIdentifier;
		}

		DotNetRestore(cliAotProjectFile, restoreSettings);
	});

Task("Build-Cli.Aot")
	.IsDependentOn("Restore-Cli.Aot")
	.Does(() => {
		var publishSettings = GlobalPublishSettings;

		if (string.IsNullOrEmpty(publishSettings.Runtime)){
			publishSettings.Runtime = RuntimeInformation.RuntimeIdentifier;
		}

		if (string.IsNullOrEmpty(publishSettings.Framework)){
			publishSettings.Framework = "net8.0";
		}

		publishSettings.NoBuild = false;
		publishSettings.OutputDirectory = $"bin/Kryptor.Cli.Native/Aot/{publishSettings.Configuration}/{publishSettings.Framework}";

		DotNetPublish(cliAotProjectFile, publishSettings);
	});

Task("Restore-Engine.Test")
	.Description("Restore kryptor engine test project dependencies")
	.IsDependentOn("Restore-Engine")
	.Does(() => {
		DotNetRestore(engineTestProjectFile, GlobalRestoreSettings);
	});

Task("Build-Engine.Test")
	.Description("Compile kryptor engine test project")
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
	.IsDependentOn("Build-Engine")
	.IsDependentOn("Build-Client")
	.IsDependentOn("Build-Cli")
	.IsDependentOn("Build-Cli.Android")
	.IsDependentOn("Build-Cli.Legacy")
	.IsDependentOn("Build-Cli.Aot");

RunTarget(target);