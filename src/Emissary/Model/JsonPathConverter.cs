using System;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EmissaryCore
{
    public class JsonPathConverter : JsonConverter
    {

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jObject = JObject.Load(reader);
            object targetObject = Activator.CreateInstance(objectType);
            foreach (PropertyInfo property in objectType.GetProperties().Where(p => p.CanRead && p.CanWrite)) {
                JsonPropertyAttribute attribute = property.GetCustomAttributes(true).OfType<JsonPropertyAttribute>().FirstOrDefault();
                string jsonPath = (attribute != null ? attribute.PropertyName : property.Name);
                JToken jToken = jObject.SelectToken(jsonPath);
                if (jToken != null && jToken.Type != JTokenType.Null) {
                    object value = jToken.ToObject(property.PropertyType, serializer);
                    property.SetValue(targetObject, value, null);
                }
            }
            return targetObject;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return false;
        }

        public override bool CanWrite
        {
            get { return false; }
        }

    }
}