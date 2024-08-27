using System;

namespace SAPTeam.Kryptor.Cli
{
    public class AssemblyPlatformAttribute : Attribute
    {
        public string CompileTime { get; set; }

        public string TargetFrameworkMoniker { get; set; }

        public string RuntimeIdentifier { get; set; }

        public AssemblyPlatformAttribute(string tfm, string rid)
        {
            CompileTime = DateTime.UtcNow.ToString("MM/dd/yyyy HH:mm:ss");
            TargetFrameworkMoniker = tfm;
            RuntimeIdentifier = rid;
        }
    }
}