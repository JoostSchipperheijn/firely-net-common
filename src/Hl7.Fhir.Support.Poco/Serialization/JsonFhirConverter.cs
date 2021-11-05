/* 
 * Copyright (c) 2021, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/firely-net-sdk/master/LICENSE
 */

#nullable enable

#if NETSTANDARD2_0_OR_GREATER || NET5_0_OR_GREATER

using Hl7.Fhir.Introspection;
using Hl7.Fhir.Model;
using System;
using System.Buffers;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Hl7.Fhir.Serialization
{
    /// <summary>
    /// Common resource converter to support polymorphic deserialization.
    /// </summary>
    public class JsonFhirConverter : JsonConverter<Base>
    {
        /// <summary>
        /// Constructs a <see cref="JsonConverter{T}"/> that (de)serializes FHIR json for the 
        /// POCOs in a given assembly.
        /// </summary>
        /// <param name="assembly">The assembly containing classes to be used for deserialization.</param>
        /// <param name="filter">A filter that determines which elements to include when serializing a summary.</param>
        public JsonFhirConverter(Assembly assembly, SerializationFilter? filter = default)
        {
            ModelInspector inspector = ModelInspector.ForAssembly(assembly);

             _deserializer = new JsonDynamicDeserializer(assembly);
            _serializer = new JsonFhirDictionarySerializer(inspector.FhirRelease);
            SerializationFilter = filter;
        }

        /// <summary>
        /// Constructs a <see cref="JsonConverter{T}"/> that (de)serializes FHIR json for the 
        /// POCOs in a given assembly.
        /// </summary>
        /// <param name="deserializer">A custom deserializer to be used by the json converter.</param>
        /// <param name="serializer">A customer serializer to be used by the json converter.</param>
        /// <param name="filter">A filter that determines which elements to include when serializing a summary.</param>
        /// <remarks>Since the standard serializer/deserializer will allow you to override its behaviour to produce
        /// custom behaviour, this constructor will allow the developer to use such custom serializers/deserializers instead
        /// of the defaults.</remarks>
        public JsonFhirConverter(JsonDynamicDeserializer deserializer, JsonFhirDictionarySerializer serializer, SerializationFilter? filter = default)
        {
            _deserializer = deserializer;
            _serializer = serializer;
            SerializationFilter = filter;
        }

        /// <summary>
        /// Determines whether the specified type can be converted.
        /// </summary>
        public override bool CanConvert(Type objectType) => typeof(Base).IsAssignableFrom(objectType);

        private readonly JsonDynamicDeserializer _deserializer;
        private readonly JsonFhirDictionarySerializer _serializer;

        /// <summary>
        /// The filter used to serialize a summary of the resource.
        /// </summary>
        public SerializationFilter? SerializationFilter { get; }

        /// <summary>
        /// Writes a specified value as JSON.
        /// </summary>
        public override void Write(Utf8JsonWriter writer, Base poco, JsonSerializerOptions options)
        {
            _serializer.Serialize(poco, writer, SerializationFilter);
        }

        /// <summary>
        /// Reads and converts the JSON to a typed object.
        /// </summary>
        public override Base Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return typeof(Resource).IsAssignableFrom(typeToConvert)
                ? _deserializer.DeserializeResource(ref reader)
                : _deserializer.DeserializeObject(typeToConvert, ref reader);
        }
    }
}

#endif
#nullable restore