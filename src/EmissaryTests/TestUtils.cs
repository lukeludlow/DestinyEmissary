using System;
using System.IO;

namespace EmissaryTests
{
    public class TestUtils
    {

        public static string ReadFile(string testFileName)
        {
            // string testsDirectory;
            // if (System.Diagnostics.Debugger.IsAttached) {
                // we're running in debug mode (unit tests)
            // } else {
                // we're running in production (running from the console)
            // }
            string testsDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;
            string jsonResponsesPath = Path.Combine(testsDirectory, "json-responses");
            // string filePath = Path.Combine(jsonResponsesPath, testFileName);
            string[] files = Directory.GetFiles(testsDirectory, testFileName, SearchOption.AllDirectories);
            string fileContents = "";
            if (files.Length == 1) {
                fileContents = File.ReadAllText(files[0]);
            } 
            return fileContents;
        }

    }
}