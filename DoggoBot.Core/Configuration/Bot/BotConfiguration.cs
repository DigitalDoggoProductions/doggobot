using System.IO;

using Newtonsoft.Json;

using DoggoBot.Core.Models.Credentials;

namespace DoggoBot.Core.Configuration.Bot
{
    public class BotConfiguration
    {
        private readonly string BasePath = @"Data/Configuration/Bot/BotCredentials.json";

        /// <summary>
        /// Load bot credentials from file
        /// </summary>
        /// <returns>Deserialized Credentials</returns>
        public CredentialsModel Load()
        {
            if (File.Exists(BasePath))
                return JsonConvert.DeserializeObject<CredentialsModel>(File.ReadAllText(BasePath));
            else
                throw new FileNotFoundException("Configuration File not found!");
        }
    }
}
