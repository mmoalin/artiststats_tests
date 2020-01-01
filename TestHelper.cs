using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Xunit.Sdk;

namespace ArtistStats.Test
{
    public static class TestHelper
    {
        /// <inheritDoc />
        public static string GetFileData(string filePath)
        {
            // Get the absolute path to the JSON file
            var path = Path.IsPathRooted(filePath)
                ? filePath
                : Path.GetRelativePath(Directory.GetCurrentDirectory(), filePath);

            if (!File.Exists(path))
            {
                throw new ArgumentException($"Could not find file at path: {path}");
            }

            // Load the file
            return File.ReadAllText(filePath);
            //return fileData
        }
    }
}

