using Newtonsoft.Json;
using System;
using System.IO;

namespace Bugger.Configuration
{
    public class JsonDataStorage : IDataStorage
    {
        private readonly string resourcesFolder = Constants.ResourceFolder;

        public JsonDataStorage()
        {
            if (!Directory.Exists(resourcesFolder))
            {
                Directory.CreateDirectory(resourcesFolder);
            }
        }

        public void StoreObject(object obj, string file)
        {
            StoreObject(obj, file, true);
        }

        public void StoreObject(object obj, string file, Formatting formatting)
        {
            string json = JsonConvert.SerializeObject(obj, formatting);
            string filePath = String.Concat(resourcesFolder, "/", file);
            File.WriteAllText(filePath, json);
        }

        public void StoreObject(object obj, string file, bool useIndentations)
        {
            var formatting = (useIndentations) ? Formatting.Indented : Formatting.None;
            StoreObject(obj, file, formatting);
        }

        public T RestoreObject<T>(string file)
        {
            string json = GetOrCreateFileContents(file);
            return JsonConvert.DeserializeObject<T>(json);
        }

        public bool KeyExists(string key)
        {
            return LocalFileExists(key);
        }

        public bool LocalFileExists(string file)
        {
            string filePath = String.Concat(resourcesFolder, "/", file);
            return File.Exists(filePath);
        }

        private string GetOrCreateFileContents(string file)
        {
            string filePath = String.Concat(resourcesFolder, "/", file);
            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, "");
                return "";
            }
            return File.ReadAllText(filePath);
        }
    }
}
