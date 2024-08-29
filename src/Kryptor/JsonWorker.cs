using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SAPTeam.Kryptor
{
    /// <summary>
    /// Represents methods to work with json serialization and deserialization with full support for AOT.
    /// </summary>
    public static class JsonWorker
    {
#if NET6_0_OR_GREATER
        /// <summary>
        /// Converts the provided object to json string.
        /// </summary>
        /// <remarks>
        /// This method is AOT-Compliant and you must prepare your types with System.Text.Json source generation.
        /// </remarks>
        /// <param name="obj">
        /// The object to convert.
        /// </param>
        /// <param name="context">
        /// A metadata provider for serializable types.
        /// </param>
        /// <returns>
        /// The converted json string.
        /// </returns>
        public static string ToJson(object obj, JsonSerializerContext context)
        {
            return JsonSerializer.Serialize(obj, obj.GetType(), context);
        }

        /// <summary>
        /// Converts the provided json string to <typeparamref name="T"/>.
        /// </summary>
        /// <remarks>
        /// This method is AOT-Compliant and you must prepare your types with System.Text.Json source generation.
        /// </remarks>
        /// <param name="json">
        /// The json string to convert.
        /// </param>
        /// <param name="context">
        /// A metadata provider for serializable types.
        /// </param>
        /// <typeparam name="T">
        /// The type of the object to convert to and return.
        /// </typeparam>
        /// <returns>
        /// A new instance of <typeparamref name="T"/> initialized with json data.
        /// </returns>
        public static T ReadJson<T>(string json, JsonSerializerContext context)
            where T : class
        {
            return JsonSerializer.Deserialize(json, typeof(T), context) as T;
        }
#endif

#if !NET7_0_OR_GREATER
        /// <summary>
        /// Converts the provided object to json string.
        /// </summary>
        /// <param name="obj">
        /// The object to convert.
        /// </param>
        /// <param name="jsonSerializerOptions">
        /// Options to control the conversion behavior.
        /// </param>
        /// <returns>
        /// The converted json string.
        /// </returns>
        public static string ToJson(object obj, JsonSerializerOptions jsonSerializerOptions)
        {
            return JsonSerializer.Serialize(obj, obj.GetType(), jsonSerializerOptions);
        }

        /// <summary>
        /// Converts the provided json string to <typeparamref name="T"/>.
        /// </summary>
        /// <param name="json">
        /// The json string to convert.
        /// </param>
        /// <param name="jsonSerializerOptions">
        /// Options to control the conversion behavior.
        /// </param>
        /// <typeparam name="T">
        /// The type of the object to convert to and return.
        /// </typeparam>
        /// <returns>
        /// A new instance of <typeparamref name="T"/> initialized with json data.
        /// </returns>
        public static T ReadJson<T>(string json, JsonSerializerOptions jsonSerializerOptions)
            where T : class
        {
            return JsonSerializer.Deserialize(json, typeof(T), jsonSerializerOptions) as T;
        }
#endif
    }
}
