using System.Globalization;
using System.Reflection;

using SAPTeam.Kryptor.Client;

namespace SAPTeam.Kryptor.Cli
{
    internal static partial class BuildInformation
    {
        public static BuildVariant Variant { get; private set; }

        public static BuildBranch Branch { get; private set; }

        public static bool IsAot { get; private set; }

        public static DateTime BuildTime { get; }

        public static string TargetPlatform { get; }

        public static string TargetFramework { get; }

        public static Version ApplicationVersion { get; }

        public static Version ClientVersion { get; }

        public static Version EngineVersion { get; }

        static BuildInformation()
        {
            DefineVariant();
            DefineConstants();

            if (Variant == BuildVariant.Android)
            {
                TargetPlatform = "android";
            }

            string format = "MM/dd/yyyy HH:mm:ss";
            string dateTimeString = Assembly.GetAssembly(typeof(Program)).GetCustomAttributes<AssemblyMetadataAttribute>().Where(x => x.Key == "BuildTime").First().Value;

            // Parse the string into a DateTime object
            DateTime parsedDateTime = DateTime.ParseExact(dateTimeString, format, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);

            BuildTime = parsedDateTime;

            AssemblyPlatformAttribute platformInfo = Assembly.GetAssembly(typeof(Program)).GetCustomAttribute<AssemblyPlatformAttribute>();
            if (!string.IsNullOrEmpty(platformInfo.RuntimeIdentifier))
            {
                TargetPlatform = platformInfo.RuntimeIdentifier;
            }
            TargetFramework = platformInfo.TargetFrameworkMoniker;

            ApplicationVersion = new Version(Assembly.GetAssembly(typeof(Program)).GetCustomAttribute<AssemblyFileVersionAttribute>().Version);
            ClientVersion = new Version(Assembly.GetAssembly(typeof(Utilities)).GetCustomAttribute<AssemblyFileVersionAttribute>().Version);
            EngineVersion = new Version(Assembly.GetAssembly(typeof(Kes)).GetCustomAttribute<AssemblyFileVersionAttribute>().Version);
        }

        private static void DefineConstants()
        {
#if DEBUG
            Branch = BuildBranch.Debug;
#elif NUGET
            Branch = BuildBranch.Nuget;
#elif INDEXER
            Branch = BuildBranch.Indexer;
#elif LIGHT
            Branch = BuildBranch.Light;
#elif RELEASE
            Branch = BuildBranch.Release;
#else
            Branch = BuildBranch.None;
#endif

#if AOT
            IsAot = true;
#else
            IsAot = false;
#endif
        }
    }
}