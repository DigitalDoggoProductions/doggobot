using System.IO;
using System.Text;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace DoggoBot.Core.Common.Local
{
    public class LocalString
    {
        private readonly string LocalStringPath = @"Data/Local/StringList.json";
        private Dictionary<string, string> currentStrings = new Dictionary<string, string>();

        /// <summary>
        /// Retrieve the value of a key from the String List
        /// </summary>
        /// <param name="key">Key for Value Pair</param>
        /// <returns>String Key value</returns>
        public string GetLocalString(string key)
        {
            if (currentStrings.Count == 0)
                LoadStrings();

            return currentStrings[key];
        }

        /// <summary>
        /// Modify the value of a key from the String List
        /// </summary>
        /// <param name="key">Key for Value Pair</param>
        /// <param name="changedValue">New Key Value</param>
        /// <returns>Bool determining the result [True = Successful, False = Error]</returns>
        public bool ModifyLocalString(string key, string changedValue)
        {
            if (currentStrings.Count == 0)
                LoadStrings();

            string oldValue = currentStrings[key];

            dynamic ourJson = JsonConvert.DeserializeObject(File.ReadAllText(LocalStringPath));
            ourJson[key] = changedValue;
            File.WriteAllText(LocalStringPath, JsonConvert.SerializeObject(ourJson, Formatting.Indented));

            currentStrings.Clear();
            LoadStrings();

            if (currentStrings[key] != changedValue)
                return false;
            else
                return true;
        }

        /// <summary>
        /// Add a key/value pair to the String List
        /// </summary>
        /// <param name="newKey">Key to add</param>
        /// <param name="newValue">Value for the key</param>
        /// <returns>Bool determining the result [True = Successful, False = Error]</returns>
        public bool AddLocalString(string newKey, string newValue)
        {
            if (currentStrings.Count == 0)
                LoadStrings();

            currentStrings.Add(newKey, newValue);
            File.WriteAllText(LocalStringPath, JsonConvert.SerializeObject(currentStrings, Formatting.Indented));

            currentStrings.Clear();
            LoadStrings();

            if (!currentStrings.ContainsKey(newKey))
                return false;
            else
                return true;
        }

        /// <summary>
        /// Remove a key/value pair from the String List
        /// </summary>
        /// <param name="key">Key to remove</param>
        /// <returns>Bool determining the result [True = Successful, False = Error]</returns>
        public bool RemoveLocalString(string key)
        {
            if (currentStrings.Count == 0)
                LoadStrings();

            currentStrings.Remove(key);
            File.WriteAllText(LocalStringPath, JsonConvert.SerializeObject(currentStrings, Formatting.Indented));

            currentStrings.Clear();
            LoadStrings();

            if (currentStrings.ContainsKey(key))
                return false;
            else
                return true;
        }

        private void LoadStrings()
        {
            var loadFile = File.ReadAllText(LocalStringPath, new UTF8Encoding(false));
            currentStrings = JsonConvert.DeserializeObject<Dictionary<string, string>>(loadFile);
        }
    }
}
