using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using SAPTeam.Kryptor.Client.Security;

namespace SAPTeam.Kryptor.Client
{
    public class ClientTypesJsonWorker
    {
        private static readonly JsonSerializerOptions jOptions = new JsonSerializerOptions()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
            WriteIndented = true,
        };

#if NET6_0_OR_GREATER
        public static string ToJson(object obj)
        {
            return JsonWorker.ToJson(obj, ClientTypesJsonSerializerContext.Default);
        }

        public static T ReadJson<T>(string json)
            where T : class
        {
            return JsonWorker.ReadJson<T>(json, ClientTypesJsonSerializerContext.Default);
        }
#else
        public static string ToJson(object obj)
        {
            return JsonWorker.ToJson(obj, jOptions);
        }

        public static T ReadJson<T>(string json)
            where T : class
        {
            return JsonWorker.ReadJson<T>(json, jOptions);
        }
#endif
    }

#if NET6_0_OR_GREATER
    [JsonSourceGenerationOptions(WriteIndented = true, DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)]
    [JsonSerializable(typeof(WordlistVerificationMetadata))]
    [JsonSerializable(typeof(List<WordlistVerificationMetadata>))]
    [JsonSerializable(typeof(KeyChain))]
    [JsonSerializable(typeof(List<KeyChain>))]
    [JsonSerializable(typeof(List<WordlistIndexEntry>))]
    [JsonSerializable(typeof(WordlistIndexEntry))]
    [JsonSerializable(typeof(WordlistIndex))]
    internal partial class ClientTypesJsonSerializerContext : JsonSerializerContext { }
#endif
}
