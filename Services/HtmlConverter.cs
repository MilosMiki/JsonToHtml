using FlawlessCode.Models;
using System.Collections.Generic;
using System.Linq;

namespace FlawlessCode.Services
{
    public class HtmlConverter
    {
        // recursive function looping through the JsonNode list
        public string ConvertToHtml(Models.JsonNode node, int indent = 0, bool isHead = false)
        {
            if (node == null) return "";

            //special rules for first pass (parent node)
            if (indent == 0)
            {
                // if found doctype, put it on top of the file
                string doctype = node.FindValue("doctype")?.StringValue != null
                    ? $"<!DOCTYPE {node.FindValue("doctype")!.StringValue}>\n"
                    : "";

                // head should appear 1st inside the <html> element
                var headContent = WrapInHtmlElement("head", node.FindValue("head")?.ObjectValue, 2, true);
                // body should appear 2nd inside the <html> element
                // body should be with attributes (this is implemented in the polymorphed method)
                var bodyContent = WrapInHtmlElement("body", node.FindValue("body")?.ObjectValue, 2, false);

                // Get HTML attributes (excluding body, head, doctype)
                var htmlAttributes = node.Properties
                    // Skip keys that we already covered
                    .Where(kvp => !new[] { "body", "head", "doctype" }.Contains(kvp.Key))
                    // specific code when encountering language parameter (for html element)
                    .Select(kvp => kvp.Key.Equals("language")
                        ? new KeyValuePair<string, Models.JsonValue>("lang", kvp.Value)
                        : kvp)
                    .ToList();

                return doctype + WrapInHtmlElement("html", headContent + bodyContent, 1, false, htmlAttributes);
            }

            // if deeper than first pass (structure the document in the order it appears in the JSON file)
            string result = "";
            foreach (var kvp in node.Properties)
            {
                // always skip attributes tag (already made a pass when creating the parent html element)
                if (kvp.Key.Equals("attributes", StringComparison.OrdinalIgnoreCase))
                    continue;

                // if of type string: string, just add to result
                if (kvp.Value.IsString)
                {
                    result += WrapInHtmlElement(kvp.Key, kvp.Value.StringValue!, indent + 1, isHead, null, false);
                }
                else if (kvp.Value.IsObject)
                {
                    if (isHead)
                    {
                        // specific code for meta keyword
                        if (kvp.Key.Equals("meta", StringComparison.OrdinalIgnoreCase))
                        {
                            result += ProcessMetaTags(kvp.Value.ObjectValue!, indent);
                        }
                        //example found in helloWorld.json, when encountering link (i split it into objecttype instead of arraytype)
                        //this is how i will format all items in the head (i don't have enough examples to assume whether this formatting is head specific or link specific)
                        else
                        {
                            // Handle any element in head section with attributes
                            result += indentString(indent) + $"<{kvp.Key}";

                            // Add all attributes from the JsonValue
                            foreach (var attr in kvp.Value.ObjectValue!.Properties)
                            {
                                if (attr.Value.IsString)
                                {
                                    result += $" {attr.Key}=\"{attr.Value.StringValue}\"";
                                }
                            }

                            result += ">\n";
                        }
                    }
                    else
                    {
                        // Normal nested elements
                        result += WrapInHtmlElement(kvp.Key, kvp.Value.ObjectValue!, indent + 1, isHead);
                    }
                }
            }

            return result;
        }

        private string ProcessMetaTags(Models.JsonNode metaNode, int indent)
        {
            string result = "";
            foreach (var metaKvp in metaNode.Properties)
            {
                result += indentString(indent);
                if (metaKvp.Key.Equals("charset", StringComparison.OrdinalIgnoreCase))
                {
                    //assuming the charset will always be a string value
                    result += $"<meta charset=\"{metaKvp.Value.StringValue!}\">\n";
                }
                else
                {
                    string content = metaKvp.Value.IsString
                        ? metaKvp.Value.StringValue!
                        //taking example from specific code found in pageNotFound2.json under viewport
                        : string.Join(", ", metaKvp.Value.ObjectValue!.Properties.Select(p => $"{p.Key}={p.Value.StringValue}"));
                    result += $"<meta name=\"{metaKvp.Key}\" content=\"{content}\">\n";
                }
            }
            return result;
        }

        //use this when dealing with object output (it can find attributes for the html element)
        public string WrapInHtmlElement(
            string element,
            Models.JsonNode? node,
            int indent,
            bool isHead,
            List<KeyValuePair<string, Models.JsonValue>>? parameters = null)
        {
            string content = node != null ? ConvertToHtml(node, indent, isHead) : "";
            return WrapInHtmlElement(element, content, indent, isHead, parameters ?? node?.FindAttributes()?.Properties);
        }

        //use this when dealing with string output
        //(although it does also work with object output, just won't find attributes unless given)
        public string WrapInHtmlElement(
            string element,
            string content,
            int indent,
            bool isHead,
            IEnumerable<KeyValuePair<string, Models.JsonValue>>? parameters = null,
            bool addNewlines = true)
        {
            if (string.IsNullOrEmpty(element)) return content;

            // Build attributes string
            string attributes = (parameters != null && parameters.Any())
                ? " " + string.Join(" ", parameters
                    .Select(p => p.Value.IsString
                        ? $"{p.Key}=\"{p.Value.StringValue}\""
                        : p.Value.IsObject && p.Value.ObjectValue != null
                            ? $"{p.Key}=\"{string.Join(";", p.Value.ObjectValue.Properties.Select(prop => $"{prop.Key}:{prop.Value.StringValue}"))}\""
                            : "")
                    .Where(a => !string.IsNullOrEmpty(a)))
                : "";

            string indentStr = indentString(indent - 1);
            string newline = addNewlines ? "\n" : "";
            string addIndent = addNewlines ? indentStr : "";

            string res = $"{indentStr}<{element}{attributes}>{newline}{content}";
            if (res.EndsWith("\n"))
            {
                return $"{res}{addIndent}</{element}>\n";
            }

            return $"{res}{newline}{addIndent}</{element}>\n";
        }

        private string indentString(int indent)
        {
            return new string('\t', indent);
        }
    }
}