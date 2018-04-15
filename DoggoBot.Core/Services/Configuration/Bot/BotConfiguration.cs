using System;
using System.IO;
using System.Collections.Generic;

using Newtonsoft.Json;

using DoggoBot.Core.Models.Secrets;
using DoggoBot.Core.Models.BlockedUser;

namespace DoggoBot.Core.Services.Configuration.Bot
{
    public class BotConfiguration
    {
        public SecretsModel LoadedSecrets;
        public Dictionary<ulong, BlockedUserModel> BlockedUsers;

        private readonly string SecretsFile = @"Data/Configuration/Bot/Secrets.json";
        private readonly string BlockedUsersFile = @"Data/Configuration/Bot/BlockedUsers.json";

        public SecretsModel LoadSecrets()
        {
            if (File.Exists(SecretsFile))
                return LoadedSecrets = JsonConvert.DeserializeObject<SecretsModel>(File.ReadAllText(SecretsFile));
            else
                throw new FileNotFoundException("Unable to locate the Secrtes File!");
        }

        public Dictionary<ulong, BlockedUserModel> LoadBlockedUsers()
        {
            if (File.Exists(BlockedUsersFile))
                return BlockedUsers = JsonConvert.DeserializeObject<Dictionary<ulong, BlockedUserModel>>(File.ReadAllText(BlockedUsersFile)) ?? new Dictionary<ulong, BlockedUserModel>();
            else
                throw new FileNotFoundException("Unable to locate the Blocked Users File!");
        }

        public void AddUserBlock(ulong Id, bool isPermanent, DateTime blockedOn, string reason)
        {
            BlockedUsers = LoadBlockedUsers();
            BlockedUsers.Add(Id, new BlockedUserModel { Permanent = isPermanent, BlockedTime = blockedOn, Reason = reason });

            File.WriteAllText(BlockedUsersFile, JsonConvert.SerializeObject(BlockedUsers, Formatting.Indented));
        }

        public void RemoveUserBlock(ulong Id)
        {
            BlockedUsers = LoadBlockedUsers();
            BlockedUsers.Remove(Id);

            File.WriteAllText(BlockedUsersFile, JsonConvert.SerializeObject(BlockedUsers, Formatting.Indented));
        }
    }
}
