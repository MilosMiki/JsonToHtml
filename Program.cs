using FlawlessCode.Models;
using FlawlessCode.Services;
using System;
using System.IO;

namespace FlawlessCode
{
    internal class Program
    {
        private readonly JsonParser _jsonParser;
        private readonly HtmlConverter _htmlConverter;
        private readonly FileHandler _fileHandler;

        public Program()
        {
            _jsonParser = new JsonParser();
            _htmlConverter = new HtmlConverter();
            _fileHandler = new FileHandler();
        }

        static void Main(string[] args)
        {
            new Program().Run();
            Console.ReadKey();
        }

        private void Run()
        {
            string rootDirectory = Directory.GetCurrentDirectory();
            string inputDirectory = Path.Combine(rootDirectory, "input");
            string outputDirectory = Path.Combine(rootDirectory, "output");

            _fileHandler.EnsureDirectoryExists(inputDirectory);
            _fileHandler.EnsureDirectoryExists(outputDirectory);

            var jsonFiles = _fileHandler.GetJsonFiles(inputDirectory);

            if (jsonFiles.Length == 0)
            {
                Console.WriteLine("No JSON files found in the input directory.");
                return;
            }

            ProcessJsonFiles(jsonFiles, outputDirectory);
        }

        private void ProcessJsonFiles(string[] jsonFiles, string outputDirectory)
        {
            Console.WriteLine("Found JSON files:");
            foreach (var file in jsonFiles)
            {
                ProcessSingleJsonFile(file, outputDirectory);
            }
        }

        private void ProcessSingleJsonFile(string filePath, string outputDirectory)
        {
            // find the json file
            string fileName = Path.GetFileName(filePath);
            Console.WriteLine(fileName);

            // read the json file
            string json = _fileHandler.ReadFileContent(filePath);
            JsonNode parsedJson = _jsonParser.ParseJson(json);

            //Console.WriteLine("Parsed JSON structure:");
            _jsonParser.PrintJsonStructure(parsedJson, 0);

            // Create an empty .html file (overwrite if exists) in the output directory
            string convertedHtml = _htmlConverter.ConvertToHtml(parsedJson);
            _fileHandler.WriteHtmlFile(outputDirectory, fileName, convertedHtml);
        }
    }
}