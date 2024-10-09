using System.Runtime.InteropServices;

///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument("t", "Build-Cli");
var configuration = Argument("c", "Release");
var runtime = Argument("r", "");
var framework  = Argument("f", "");
var output = Argument<string>("o", null);

var noRestore = Argument("no-restore", false);
var noBuild = Argument("no-build", false);

string engineProjectFile = "src/Kryptor/Kryptor.csproj";
string clientProjectFile = "src/Kryptor.Client/Kryptor.Client.csproj";

string cliProjectFile = "src/Kryptor.Cli/Kryptor.Cli.csproj";

string engineTestProjectFile = "test/Kryptor.Tests/Kryptor.Tests.csproj";

///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////

bool ignoreLockFile;
bool CiBuild;

DotNetMSBuildSettings GlobalMSBuildSettings => new(){
	ContinuousIntegrationBuild = CiBuild,
};

DotNetRestoreSettings GlobalRestoreSettings => new(){
	Runtime = runtime,
	UseLockFile = !ignoreLockFile,
	MSBuildSettings = GlobalMSBuildSettings,
};

DotNetBuildSettings GlobalBuildSettings => new(){
	NoRestore = true,
	Configuration = configuration,
	Framework = framework,
	Runtime = runtime,
	OutputDirectory = output,
	MSBuildSettings = GlobalMSBuildSettings,
};

DotNetTestSettings GlobalTestSettings => new(){
	NoRestore = true,
	NoBuild = true,
	Verbosity = DotNetVerbosity.Normal,
	Collectors = {"XPlat Code Coverage"},
	Loggers = {"trx"},
	Configuration = configuration,
	Framework = framework,
	Runtime = runtime,
	MSBuildSettings = GlobalMSBuildSettings,
};

DotNetPublishSettings GlobalPublishSettings => new(){
	NoRestore = true,
	NoBuild = true,
	Configuration = configuration,
	Framework = framework,
	Runtime = runtime,
	OutputDirectory = output,
	MSBuildSettings = GlobalMSBuildSettings,
};

DotNetPackSettings GlobalPackSettings => new(){
	NoRestore = true,
	NoBuild = true,
	Configuration = configuration,
	Runtime = runtime,
	OutputDirectory = output,
	MSBuildSettings = GlobalMSBuildSettings,
};

string ResolveRuntimeIdentifier(){
	var rawId = RuntimeInformation.RuntimeIdentifier;
	string resId;

	if (IsRunningOnLinux()){
		resId = $"linux-{rawId.Split('-')[1]}";
	}
	else{
		resId = rawId;
	}

	return resId;
}

Setup(context => {
	CiBuild = GitHubActions.IsRunningOnGitHubActions;

	if (CiBuild){
		Information("Running build in Github Actions");
	}

	if (context.TasksToExecute.Where(task => task.Name.StartsWith("Publish-")).Count() > 0){
		ignoreLockFile = true;
	}
	else{
		ignoreLockFile = false;
	}
});

Task("Restore-Engine")
	.Description("Restore kryptor engine dependencies")
	.WithCriteria(!noRestore)
	.Does(() => {
		DotNetRestore(engineProjectFile, GlobalRestoreSettings);
	});
	
Task("Build-Engine")
	.Description("Build kryptor engine")
	.IsDependentOn("Restore-Engine")
	.WithCriteria(!noBuild)
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
	.WithCriteria(!noRestore)
	.Does(() => {
		DotNetRestore(clientProjectFile, GlobalRestoreSettings);
	});
	
Task("Build-Client")
	.Description("Build kryptor client utilities")
	.IsDependentOn("Restore-Client")
	.WithCriteria(!noBuild)
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
	.WithCriteria(!noRestore)
	.Does(() => {
		DotNetRestore(cliProjectFile, GlobalRestoreSettings);
	});

Task("Build-Cli")
	.Description("Build kryptor command line interface")
	.IsDependentOn("Restore-Cli")
	.WithCriteria(!noBuild)
	.Does(() => {
		DotNetBuild(cliProjectFile, GlobalBuildSettings);
	});

Task("Pack-Cli")
	.Description("Create a NuGet package for kryptor command line interface")
	.IsDependentOn("Build-Cli")
	.Does(() => {
		DotNetPack(cliProjectFile, GlobalPackSettings);
	});

Task("Publish-Cli")
	.Description("Publish kryptor cli")
	.IsDependentOn("Build-Cli")
	.Does(() => {
		DotNetPublish(cliProjectFile, GlobalPublishSettings);
	});

Task("Publish-Cli.bundle")
	.Description("Publish kryptor cli bundled (self contained)")
	.IsDependentOn("Build-Cli")
	.Does(() => {
		var publishSettings = GlobalPublishSettings;
		publishSettings.SelfContained = true;
		publishSettings.PublishTrimmed = true;
		publishSettings.MSBuildSettings = new DotNetMSBuildSettings()
			.WithProperty("CompileBundled", "True");

		DotNetPublish(cliProjectFile, publishSettings);
	});

Task("Restore-Cli.Aot")
	.Description("Restore kryptor cli native AOT dependencies")
	.IsDependentOn("Restore-Client")
	.WithCriteria(!noRestore)
	.Does(() => {
		var restoreSettings = GlobalRestoreSettings;

		if (string.IsNullOrEmpty(restoreSettings.Runtime)){
			restoreSettings.Runtime = ResolveRuntimeIdentifier();
		}

		restoreSettings.MSBuildSettings = new DotNetMSBuildSettings()
			.WithProperty("CompileAot", "True");

		DotNetRestore(cliProjectFile, restoreSettings);
	});

Task("Publish-Cli.Aot")
	.Description("Publish kryptor cli native AOT")
	.IsDependentOn("Restore-Cli.Aot")
	.Does(() => {
		var publishSettings = GlobalPublishSettings;

		if (string.IsNullOrEmpty(publishSettings.Runtime)){
			publishSettings.Runtime = ResolveRuntimeIdentifier();
		}

		if (string.IsNullOrEmpty(publishSettings.Framework)){
			publishSettings.Framework = "net8.0";
		}

		publishSettings.NoBuild = false;

		publishSettings.MSBuildSettings = new DotNetMSBuildSettings()
			.WithProperty("CompileAot", "True");

		DotNetPublish(cliProjectFile, publishSettings);
	});

Task("Restore-Engine.Test")
	.Description("Restore kryptor engine test dependencies")
	.IsDependentOn("Restore-Engine")
	.WithCriteria(!noRestore)
	.Does(() => {
		DotNetRestore(engineTestProjectFile, GlobalRestoreSettings);
	});

Task("Build-Engine.Test")
	.Description("Build kryptor engine test")
	.IsDependentOn("Restore-Engine.Test")
	.WithCriteria(!noBuild)
	.Does(() => {
		DotNetBuild(engineTestProjectFile, GlobalBuildSettings);
	});

Task("Test-Engine")
	.Description("Run kryptor engine tests")
	.IsDependentOn("Build-Engine.Test")
	.Does(() => {
		DotNetTest(engineTestProjectFile, GlobalTestSettings);
	});

Task("Restore-All")
	.Description("Restore all projects")
	.IsDependentOn("Restore-Engine")
	.IsDependentOn("Restore-Client")
	.IsDependentOn("Restore-Cli");

Task("Build-All")
	.Description("Build all projects")
	.IsDependentOn("Build-Engine")
	.IsDependentOn("Build-Client")
	.IsDependentOn("Build-Cli");

Task("Pack-All")
	.Description("Create NuGet package for all projects")
	.IsDependentOn("Pack-Engine")
	.IsDependentOn("Pack-Client")
	.IsDependentOn("Pack-Cli");
	
Task("Test-All")
	.Description("Run all tests")
	.IsDependentOn("Test-Engine");

RunTarget(target);