using System.Reflection;

using System.Text.Json.Serialization;

using SAPTeam.Kryptor.Client;

namespace SAPTeam.Kryptor.Cli
{
    public class CliHeader : ClientHeader
    {
        /// <inheritdoc/>
        protected override JsonSerializerContext JsonSerializerContext => SourceGenerationCliHeaderContext.Default;

        public static ClientHeader Create()
        {
            Dictionary<string, string> extra = new Dictionary<string, string>
            {
                ["client"] = "kryptor-cli"
            };

            return new CliHeader()
            {
                ClientName = "Kryptor Cli",
                ClientVersion = new Version(Assembly.GetAssembly(typeof(Program)).GetCustomAttribute<AssemblyFileVersionAttribute>().Version),
                Extra = extra
            };
        }
    }

    [JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Metadata, WriteIndented = false, DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)]
    [JsonSerializable(typeof(Header))]
    [JsonSerializable(typeof(ClientHeader))]
    [JsonSerializable(typeof(CliHeader))]
    internal partial class SourceGenerationCliHeaderContext : JsonSerializerContext
    {
    }
}