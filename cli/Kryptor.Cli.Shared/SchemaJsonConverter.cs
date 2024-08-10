using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System;

namespace SAPTeam.Kryptor.Cli
{
    public class SchemaJsonConverter : JsonConverter
    {
        private readonly string _schemaUrl;

        public SchemaJsonConverter(string schemaUrl)
        {
            _schemaUrl = schemaUrl;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            try
            {
                var settings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    NullValueHandling = NullValueHandling.Ignore,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                };

                JObject obj = JObject.FromObject(value, JsonSerializer.Create(settings));
                obj.AddFirst(new JProperty("$schema", _schemaUrl));
                obj.WriteTo(writer);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during serialization: {ex.Message}");
                throw;
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        public override bool CanRead => false;
    }
}