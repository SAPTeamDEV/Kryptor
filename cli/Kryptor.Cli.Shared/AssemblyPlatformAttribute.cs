namespace SAPTeam.Kryptor.Cli
{
    public class AssemblyPlatformAttribute : Attribute
    {
        public string TargetFrameworkMoniker { get; set; }

        public string RuntimeIdentifier { get; set; }

        public AssemblyPlatformAttribute(string tfm, string rid)
        {
            TargetFrameworkMoniker = tfm;
            RuntimeIdentifier = rid;
        }
    }
}