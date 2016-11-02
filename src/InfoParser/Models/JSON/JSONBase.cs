using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace InfoParser.Models.JSON
{
    public class JsonBase
    {
        static JsonBase()
        {
            JsonConvert.DefaultSettings = (() =>
            {
                var settings = new JsonSerializerSettings();
                settings.Converters.Add(new StringEnumConverter { CamelCaseText = true });
                return settings;
            });
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public override string ToString()
        {
            return ToJson();
        }
    }
}