using System;
using System.Globalization;
using System.Linq;
using System.Reflection;

using SAPTeam.Kryptor.Client;

namespace SAPTeam.Kryptor.Cli
{
    internal static partial class BuildInformation
    {
#if DEBUG
        public const BuildBranch Branch = BuildBranch.Debug;
#elif NUGET
        public const BuildBranch Branch = BuildBranch.Nuget;
#elif INDEXER
        public const BuildBranch Branch = BuildBranch.Indexer;
#elif RELEASE
        public const BuildBranch Branch = BuildBranch.Release;
#else
        public const BuildBranch Branch = BuildBranch.None;
#endif

#if AOT
        public const bool IsAot = true;
#else
        public const bool IsAot = false;
#endif

        public static DateTime BuildTime { get; }

        public static string TargetPlatform { get; }

        public static string TargetFramework { get; }

        public static Version ApplicationVersion { get; }

        public static Version ClientVersion { get; }

        public static Version EngineVersion { get; }

        static BuildInformation()
        {
            string format = "MM/dd/yyyy HH:mm:ss";
            string dateTimeString = Assembly.GetAssembly(typeof(Program)).GetCustomAttributes<AssemblyMetadataAttribute>().Where(x => x.Key == "BuildTime").First().Value;

            // Parse the string into a DateTime object
            DateTime parsedDateTime = DateTime.ParseExact(dateTimeString, format, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);

            BuildTime = parsedDateTime;

            AssemblyPlatformAttribute platformInfo = Assembly.GetAssembly(typeof(Program)).GetCustomAttribute<AssemblyPlatformAttribute>();
            TargetPlatform = platformInfo.RuntimeIdentifier;
            TargetFramework = platformInfo.TargetFrameworkMoniker;

            ApplicationVersion = new Version(Assembly.GetAssembly(typeof(Program)).GetCustomAttribute<AssemblyFileVersionAttribute>().Version);
            ClientVersion = new Version(Assembly.GetAssembly(typeof(Utilities)).GetCustomAttribute<AssemblyFileVersionAttribute>().Version);
            EngineVersion = new Version(Assembly.GetAssembly(typeof(Kes)).GetCustomAttribute<AssemblyFileVersionAttribute>().Version);
        }
    }
}