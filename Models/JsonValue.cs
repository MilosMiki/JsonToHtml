using System.Text.Json.Nodes;

namespace FlawlessCode.Models
{
    public class JsonValue
    {
        // when value is a string (example: "html", "en")
        public string? StringValue { get; set; }
        // when value is a list (example: "body" : { ... })
        public JsonNode? ObjectValue { get; set; }
        // when value is an array (example: "link" : [...])
        public List<JsonNode>? ArrayValue { get; set; }

        public bool IsString => StringValue != null;
        public bool IsObject => ObjectValue != null;
        public bool IsArray => ArrayValue != null;
        public static Models.JsonValue JsonNodeToValue(Models.JsonNode node)
        {
            return new Models.JsonValue { ObjectValue = node };
        }
    }

}