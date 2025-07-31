using System;
using System.IO;

namespace FlawlessCode.Services
{
    public class FileHandler
    {
        public string[] GetJsonFiles(string inputDirectory)
        {
            return Directory.GetFiles(inputDirectory, "*.json", SearchOption.TopDirectoryOnly);
        }

        public void WriteHtmlFile(string outputDirectory, string fileName, string content)
        {
            string htmlFileName = Path.ChangeExtension(fileName, ".html");
            string htmlFilePath = Path.Combine(outputDirectory, htmlFileName);
            File.WriteAllText(htmlFilePath, content);
        }

        public string ReadFileContent(string filePath)
        {
            return File.ReadAllText(filePath);
        }

        public void EnsureDirectoryExists(string directory)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }
    }
}