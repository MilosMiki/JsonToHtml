using FlawlessCode.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace FlawlessCode.Services
{
    public class JsonParser
    {
        // initially the json data goes to this function (creates the head node)
        public Models.JsonNode ParseJson(string json)
        {
            var jsonObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            return ParseDictionary(jsonObject!);
        }

        // creates child nodes
        private Models.JsonNode ParseDictionary(Dictionary<string, object> dict)
        {
            var node = new Models.JsonNode();
            if (dict == null)
            {
                return node;
            }
            foreach (var kvp in dict)
            {
                Models.JsonValue res = ParseValue(kvp.Value);
                if (res.IsArray && res.ArrayValue != null)
                {
                    foreach (Models.JsonNode item in res.ArrayValue)
                    {
                        node.Properties.Add(new KeyValuePair<string, Models.JsonValue>(kvp.Key, Models.JsonValue.JsonNodeToValue(item))); 
                    }
                }
                node.Properties.Add(new KeyValuePair<string, Models.JsonValue>(kvp.Key, res));
            }
            return node;
        }

        // parses the value type of the node (string, list, array)
        private Models.JsonValue ParseValue(object value)
        {
            if (value == null) return new Models.JsonValue();

            switch (value)
            {
                // if json element is of type string: string
                case string str:
                    return new Models.JsonValue { StringValue = str };
                // if json element is of type string: list (with curly brackets)
                case JObject obj:
                    return new Models.JsonValue { ObjectValue = ParseDictionary(obj.ToObject<Dictionary<string, object>>()!) };
                // if json element is of type string: array (with square brackets)
                case JArray arr:
                    return ParseArray(arr);
                // for other uses not featured in example files (likely to cause errors/exceptions later)
                default:
                    return new Models.JsonValue { StringValue = value.ToString() };
            }
        }

        private Models.JsonValue ParseArray(JArray arr)
        {
            var list = new List<Models.JsonNode>();
            foreach (var item in arr)
            {
                if (item is JObject itemObj)
                {
                    list.Add(ParseDictionary(itemObj.ToObject<Dictionary<string, object>>()!));
                }
            }
            return new Models.JsonValue { ArrayValue = list };
        }

        public void PrintJsonStructure(JsonNode? node, int indent)
        {
            if (node == null) return;
            foreach (var kvp in node.Properties)
            {
                Console.Write(new string(' ', indent * 2) + kvp.Key + ": ");
                if (kvp.Value.IsString)
                {
                    Console.WriteLine(kvp.Value.StringValue);
                }
                else if (kvp.Value.IsObject)
                {
                    Console.WriteLine("{");
                    PrintJsonStructure(kvp.Value.ObjectValue, indent + 1);
                    Console.WriteLine(new string(' ', indent * 2) + "}");
                }
                else if (kvp.Value.IsArray && kvp.Value.ArrayValue != null)
                {
                    Console.WriteLine("[");
                    foreach (var item in kvp.Value.ArrayValue)
                    {
                        Console.WriteLine(new string(' ', (indent + 1) * 2) + "{");
                        PrintJsonStructure(item, indent + 2);
                        Console.WriteLine(new string(' ', (indent + 1) * 2) + "}");
                    }
                    Console.WriteLine(new string(' ', indent * 2) + "]");
                }
            }
        }
    }
}