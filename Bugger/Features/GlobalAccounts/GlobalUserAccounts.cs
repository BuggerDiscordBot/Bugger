using Bugger.Configuration;
using System;
using Bugger.Entities;
using Discord;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Bugger.Features.GlobalAccounts
{
    internal static class GlobalUserAccounts
    {
        private static readonly ConcurrentDictionary<ulong, GlobalUserAccount> userAccounts = new ConcurrentDictionary<ulong, GlobalUserAccount>();
        private static readonly DirectoryInfo _directoryInfo;

        private static readonly string _directoryPath =
            Path.Combine(Constants.ResourceFolder, Constants.UserAccountsFolder);

        static GlobalUserAccounts()
        {
            _directoryInfo = Directory.CreateDirectory(_directoryPath);
            var files = _directoryInfo.GetFiles("*.json");
            if (files.Length > 0)
            {
                foreach (var file in files)
                {
                    var user = InversionOfControl.Container.GetInstance<JsonDataStorage>().RestoreObject<GlobalUserAccount>(Path.Combine(file.Directory.Name, file.Name));
                    userAccounts.TryAdd(user.Id, user);
                }
            }
            else
            {
                userAccounts = new ConcurrentDictionary<ulong, GlobalUserAccount>();
            }
        }

        
        internal static string GetAccountFilePath(ulong id)
        {
            var filePath = Path.Combine(Path.Combine(_directoryPath, $"{id}.json"));
            return File.Exists(filePath) ? filePath : String.Empty;
        }

        internal static bool DeleteAccountFile(ulong accountId)
        {
            if (!userAccounts.TryRemove(accountId, out var account)) return false;
            var file = GetAccountFilePath(accountId);
            if (String.IsNullOrEmpty(file)) return false;
            File.Delete(file);
            return true;
        }
        
        internal static GlobalUserAccount GetUserAccount(ulong id)
        {
            return userAccounts.GetOrAdd(id, (key) =>
            {
                var newAccount = new GlobalUserAccount(id);
                InversionOfControl.Container.GetInstance<JsonDataStorage>().StoreObject(newAccount, Path.Combine(Constants.UserAccountsFolder, $"{id}.json"), useIndentations: true);
                return newAccount;
            });
        }

        internal static GlobalUserAccount GetUserAccount(IUser user)
        {
            return GetUserAccount(user.Id);
        }

        internal static List<GlobalUserAccount> GetAllAccounts()
        {
            return userAccounts.Values.ToList();
        }

        internal static List<GlobalUserAccount> GetFilteredAccounts(Func<GlobalUserAccount, bool> filter)
        {
            return userAccounts.Values.Where(filter).ToList();
        }

        /// <summary>
        /// This rewrites ALL UserAccounts to the harddrive... Strongly recommend to use SaveAccounts(id1, id2, id3...) where possible instead
        /// </summary>
        internal static void SaveAccounts()
        {
            foreach (var id in userAccounts.Keys)
            {
                SaveAccounts(id);
            }
        }

        /// <summary>
        /// Saves one or multiple Accounts by provided Ids
        /// </summary>
        internal static void SaveAccounts(params ulong[] ids)
        {
            var dataStorage = InversionOfControl.Container.GetInstance<JsonDataStorage>();
            foreach (var id in ids)
            {
                dataStorage.StoreObject(GetUserAccount(id), Path.Combine(Constants.UserAccountsFolder, $"{id}.json"), useIndentations: true);
            }
        }
    }
}
