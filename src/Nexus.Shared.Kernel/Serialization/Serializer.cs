using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml;

namespace Nexus.Shared.Kernel.Serialization;

public static class Serializer
{
    /// <summary>
    /// Serializes object to JSON
    /// </summary>
    /// <param name="subject"> object to serialize </param>
    /// <param name="ignoreNullValues"> should null values be ignored in serialization </param>
    /// <param name="ignoreDefaultValues"> should default values be ignored in serialization </param>
    /// <param name="writeIndented"> should the JSON structure be written indented (eases reading, but takes up more space) </param>
    /// <param name="useCamelCase"></param>
    /// <returns></returns>
    public static string ToJson(object subject, bool ignoreNullValues = true, bool ignoreDefaultValues = false, bool writeIndented = false, bool useCamelCase = true, Newtonsoft.Json.JsonConverter[] converters = null)
    {
        var options = new JsonSerializerSettings();

        if (ignoreNullValues)
            options.NullValueHandling = NullValueHandling.Ignore;

        if (ignoreDefaultValues)
            options.DefaultValueHandling = DefaultValueHandling.Ignore;

        if (writeIndented)
            options.Formatting = Newtonsoft.Json.Formatting.Indented;

        if (useCamelCase)
            options.ContractResolver = new CamelCasePropertyNamesContractResolver();

        foreach (var converter in converters ?? [])
        {
            options.Converters.Add(converter);
        }

        return JsonConvert.SerializeObject(subject, options);
    }

    public static T ToModel<T>(string json)
    {
        try
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
        catch (Exception ex)
        {
            throw new SerializerException(json, ex);
        }
    }

    public static JToken ToJsonElement(string json)
    {
        try
        {
            return JToken.Parse(json);
        }
        catch (Exception ex)
        {
            throw new SerializerException(json, ex);
        }
    }
}