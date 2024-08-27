using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

using SAPTeam.Kryptor.Client;

namespace SAPTeam.Kryptor.Cli
{
    internal static partial class BuildInformation
    {
        public static DateTime BuildTime { get; }

        public static BuildBranch Branch { get; }

        public static string TargetPlatform { get; }

        public static string TargetFramework { get; }

        public static Version ApplicationVersion { get; }

        public static Version ClientVersion { get; }

        public static Version EngineVersion { get; }

        static BuildInformation()
        {
#if DEBUG
            Branch = BuildBranch.Debug;
#elif NUGET
            Branch = BuildBranch.Nuget;
#elif INDEXER
            Branch = BuildBranch.Indexer;
#elif RELEASE
            Branch = BuildBranch.Release;
#else
            Branch = BuildBranch.None;
#endif

            string format = "M/d/yyyy h:mm:ss tt";
            string dateTimeString = Assembly.GetAssembly(typeof(Program)).GetCustomAttributes<AssemblyMetadataAttribute>().Where(x => x.Key == "BuildTime").First().Value;

            // Parse the string into a DateTime object
            DateTime parsedDateTime = DateTime.ParseExact(dateTimeString, format, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);

            BuildTime = parsedDateTime;

            var platformInfo = Assembly.GetAssembly(typeof(Program)).GetCustomAttribute<AssemblyPlatformAttribute>();
            TargetPlatform = platformInfo.RuntimeIdentifier;
            TargetFramework = platformInfo.TargetFrameworkMoniker;

            ApplicationVersion = new Version(Assembly.GetAssembly(typeof(Program)).GetCustomAttribute<AssemblyFileVersionAttribute>().Version);
            ClientVersion = new Version(Assembly.GetAssembly(typeof(Utilities)).GetCustomAttribute<AssemblyFileVersionAttribute>().Version);
            EngineVersion = new Version(Assembly.GetAssembly(typeof(Kes)).GetCustomAttribute<AssemblyFileVersionAttribute>().Version);
        }
    }
}