using System;
using System.Collections.Generic;
using System.Reflection;

#if NET6_0_OR_GREATER
using System.Text.Json.Serialization;
#endif

using SAPTeam.Kryptor.Client;

namespace SAPTeam.Kryptor.Cli
{
    public class CliHeader : ClientHeader
    {
#if NET6_0_OR_GREATER
        /// <inheritdoc/>
        protected override JsonSerializerContext JsonSerializerContext => SourceGenerationCliHeaderContext.Default;
#endif

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

#if NET6_0_OR_GREATER
    [JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Metadata, WriteIndented = false, DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)]
    [JsonSerializable(typeof(Header))]
    [JsonSerializable(typeof(ClientHeader))]
    [JsonSerializable(typeof(CliHeader))]
    internal partial class SourceGenerationCliHeaderContext : JsonSerializerContext
    {
    }
#endif
}