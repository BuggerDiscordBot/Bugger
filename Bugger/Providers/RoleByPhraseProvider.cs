using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bugger.Features.GlobalAccounts;
using Bugger.Features.RoleAssignment;
using Discord;

namespace Bugger.Providers
{
    public static class RoleByPhraseProvider
    {
        public enum RoleByPhraseOperationResult { Success, AlreadyExists, Failed }

        public static RoleByPhraseOperationResult AddPhrase(IGuild guild, string phrase)
        {
            var guildSettings = GlobalGuildAccounts.GetGuildAccount(guild);

            try
            {
                guildSettings.RoleByPhraseSettings.AddPhrase(phrase);
                GlobalGuildAccounts.SaveAccounts();
                return RoleByPhraseOperationResult.Success;
            }
            catch (PhraseAlreadyAddedException)
            {
                return RoleByPhraseOperationResult.AlreadyExists;
            }
            catch (Exception)
            {
                return RoleByPhraseOperationResult.Failed;
            }
        }

        public static RoleByPhraseOperationResult AddRole(IGuild guild, IRole role)
        {
            var guildSettings = GlobalGuildAccounts.GetGuildAccount(guild);

            if (guild.GetRole(role.Id) is null) return RoleByPhraseOperationResult.Failed;

            try
            {
                guildSettings.RoleByPhraseSettings.AddRole(role.Id);
                GlobalGuildAccounts.SaveAccounts();
                return RoleByPhraseOperationResult.Success;
            }
            catch (RoleIdAlreadyAddedException)
            {
                return RoleByPhraseOperationResult.AlreadyExists;
            }
            catch (Exception)
            {
                return RoleByPhraseOperationResult.Failed;
            }
        }

        public static RoleByPhraseOperationResult ForceRelation(IGuild guild, string phrase, IRole role)
        {
            var guildSettings = GlobalGuildAccounts.GetGuildAccount(guild);

            if (guild.GetRole(role.Id) is null) return RoleByPhraseOperationResult.Failed;

            try
            {
                guildSettings.RoleByPhraseSettings.ForceCreateRelation(phrase, role.Id);
                GlobalGuildAccounts.SaveAccounts();
                return RoleByPhraseOperationResult.Success;
            }
            catch (RelationAlreadyExistsException)
            {
                return RoleByPhraseOperationResult.AlreadyExists;
            }
            catch (Exception)
            {
                return RoleByPhraseOperationResult.Failed;
            }
        }

        public enum RelationCreationResult { Success, InvalidIndex, AlreadyExists, Failed }

        public static RelationCreationResult CreateRelation(IGuild guild, int phraseIndex, int roleIdIndex)
        {
            var guildSettings = GlobalGuildAccounts.GetGuildAccount(guild);

            try
            {
                guildSettings.RoleByPhraseSettings.CreateRelation(phraseIndex, roleIdIndex);
                GlobalGuildAccounts.SaveAccounts();
                return RelationCreationResult.Success;
            }
            catch (ArgumentException)
            {
                return RelationCreationResult.InvalidIndex;
            }
            catch (RelationAlreadyExistsException)
            {
                return RelationCreationResult.AlreadyExists;
            }
            catch (Exception)
            {
                return RelationCreationResult.Failed;
            }
        }

        public static void RemovePhrase(IGuild guild, int phraseIndex)
        {
            var guildSettings = GlobalGuildAccounts.GetGuildAccount(guild);

            guildSettings.RoleByPhraseSettings.RemovePhraseByIndex(phraseIndex);
            GlobalGuildAccounts.SaveAccounts();
        }

        public static void RemoveRole(IGuild guild, int roleIdIndex)
        {
            var guildSettings = GlobalGuildAccounts.GetGuildAccount(guild);

            guildSettings.RoleByPhraseSettings.RemoveRoleIdByIndex(roleIdIndex);
            GlobalGuildAccounts.SaveAccounts();
        }

        public static void RemoveRelation(IGuild guild, int phraseIndex, int roleIdIndex)
        {
            var guildSettings = GlobalGuildAccounts.GetGuildAccount(guild);

            guildSettings.RoleByPhraseSettings.RemoveRelation(phraseIndex, roleIdIndex);
            GlobalGuildAccounts.SaveAccounts();
        }

        public static async Task EvaluateMessage(IGuild guild, string message, IGuildUser sender)
        {
            var guildSettings = GlobalGuildAccounts.GetGuildAccount(guild);

            var triggeredPhrases = guildSettings.RoleByPhraseSettings.Phrases.Where(message.Contains).ToList();

            if (!triggeredPhrases.Any()) return;

            var roleIdsToGet = new List<ulong>();

            foreach (var phrase in triggeredPhrases)
            {
                var phraseIndex = guildSettings.RoleByPhraseSettings.Phrases.IndexOf(phrase);
                var roleIds = guildSettings.RoleByPhraseSettings.Relations
                    .Where(r => r.PhraseIndex == phraseIndex)
                    .Select(r => guildSettings.RoleByPhraseSettings.RolesIds.ElementAt(r.RoleIdIndex))
                    .ToList();

                foreach (var roleId in roleIds)
                {
                    if (roleIdsToGet.Contains(roleId)) continue;
                    roleIdsToGet.Add(roleId);
                }
            }

            foreach (var roleId in roleIdsToGet)
            {
                if (sender.RoleIds.Contains(roleId)) continue;
                var role = guild.GetRole(roleId);
                if(role is null) continue;
                await sender.AddRoleAsync(role);
            }
        }
    }
}