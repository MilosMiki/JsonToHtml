using System.Collections.Generic;

namespace FlawlessCode.Models
{
    public class JsonNode
    {
        public List<KeyValuePair<string, JsonValue>> Properties { get; set; } = new List<KeyValuePair<string, JsonValue>>();

        public JsonValue? FindValue(string key)
        {
            return Properties.Find(kvp => kvp.Key.Equals(key, StringComparison.OrdinalIgnoreCase)).Value;
        }

        public JsonNode? FindAttributes()
        {
            var attributesValue = FindValue("attributes");
            return attributesValue?.ObjectValue;
        }
    }
}