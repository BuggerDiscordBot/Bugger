using System;
using System.Collections.Generic;
using System.Linq;

namespace Bugger.Features.RoleAssignment
{
    public static class RoleByPhrase
    {
        /// <summary>
        /// Adds a phrase to the list for Phrase to Role relations.
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="phrase">String phrase to trigger a relation action.</param>
        /// <exception cref="PhraseAlreadyAddedException"></exception>
        /// <exception cref="InvalidPhraseException"></exception>
        public static void AddPhrase(this RoleByPhraseSettings settings, string phrase)
        {
            if (settings.Phrases.Contains(phrase)) 
            {
                throw new PhraseAlreadyAddedException($"Phrase '{phrase}' is already added.");
            }

            if (phrase == string.Empty || phrase.Length > Constants.MaxMessageLength - 50)
            {
                throw new InvalidPhraseException();
            }

            settings.Phrases.Add(phrase);
        }

        /// <summary>
        /// Adds a role ID to the list for Phrase to Role relations.
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="roleId">Discord ID of the target role.</param>
        /// <exception cref="RoleIdAlreadyAddedException"></exception>
        public static void AddRole(this RoleByPhraseSettings settings, ulong roleId)
        {
            if(settings.RolesIds.Contains(roleId))
            {
                throw new RoleIdAlreadyAddedException($"The role with ID '{roleId}' is already added.");
            }

            settings.RolesIds.Add(roleId);
        }

        /// <summary>
        /// Forces a relation to be added. If either the phrase or roleId does not exist, it is added.
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="phrase">Phrase for the relation</param>
        /// <param name="roleId">Role ID for the relation</param>
        /// <exception cref="RelationAlreadyExistsException"></exception>
        public static void ForceCreateRelation(this RoleByPhraseSettings settings, string phrase, ulong roleId)
        {
            if(!settings.Phrases.Contains(phrase))
            {
                settings.Phrases.Add(phrase);
            }

            if(!settings.RolesIds.Contains(roleId))
            {
                settings.RolesIds.Add(roleId);
            }

            var phraseIndex = settings.Phrases.IndexOf(phrase);
            var roleIdIndex = settings.RolesIds.IndexOf(roleId);

            if(settings.Relations.Any(r => r.PhraseIndex == phraseIndex && r.RoleIdIndex == roleIdIndex))
            {
                throw new RelationAlreadyExistsException();
            }

            settings.Relations.Add(new RoleByPhraseRelation{PhraseIndex = phraseIndex, RoleIdIndex = roleIdIndex});
        }

        /// <summary>
        /// Creates a relation based on the index of a phrase and an index of a roleID.
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="phraseIndex">Index of the phrase in the list.</param>
        /// <param name="roleIdIndex">Index of the role ID in the list.</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="RelationAlreadyExistsException"></exception>
        public static void CreateRelation(this RoleByPhraseSettings settings, int phraseIndex, int roleIdIndex)
        {
            settings.Phrases.ValidateIndex(phraseIndex);
            settings.RolesIds.ValidateIndex(roleIdIndex);

            if (settings.Relations.Any(r => r.PhraseIndex == phraseIndex && r.RoleIdIndex == roleIdIndex))
            {
                throw new RelationAlreadyExistsException();
            }

            settings.Relations.Add(new RoleByPhraseRelation { PhraseIndex = phraseIndex, RoleIdIndex = roleIdIndex });
        }

        /// <summary>
        /// Safe removes Phrase by its index. Removes all relations with that phrase. Updates all other indexes in relations.
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="phraseIndex">Index of the phrase to remove</param>
        public static void RemovePhraseByIndex(this RoleByPhraseSettings settings, int phraseIndex)
        {
            settings.Phrases.ValidateIndex(phraseIndex);

            var affectedElementsOldIds = new List<int>();

            for(var i = phraseIndex; i < settings.Phrases.Count; i++)
            {
                affectedElementsOldIds.Add(i);
            }

            settings.Phrases.RemoveAt(phraseIndex);

            settings.Relations = settings.Relations.Where(r => r.PhraseIndex != phraseIndex).ToList();

            if (affectedElementsOldIds.Count == 1)
            {
                return;
            }

            foreach (var oldIndex in affectedElementsOldIds)
            {
                foreach (var rel in settings.Relations.Where(r => r.PhraseIndex == oldIndex))
                {
                    rel.PhraseIndex = oldIndex - 1;
                }
            }
        }

        /// <summary>
        /// Safe removes RoleID by its index. Removes all relations with that RoleID. Updates all other indexes in relations.
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="roleIdIndex">Index of the RoleID to remove</param>
        public static void RemoveRoleIdByIndex(this RoleByPhraseSettings settings, int roleIdIndex)
        {
            settings.RolesIds.ValidateIndex(roleIdIndex);

            var affectedElementsOldIds = new List<int>();

            for (var i = roleIdIndex; i < settings.RolesIds.Count; i++)
            {
                affectedElementsOldIds.Add(i);
            }

            settings.RolesIds.RemoveAt(roleIdIndex);

            settings.Relations = settings.Relations.Where(r => r.RoleIdIndex != roleIdIndex).ToList();

            if (affectedElementsOldIds.Count == 1)
            {
                return;
            }

            foreach (var oldIndex in affectedElementsOldIds)
            {
                foreach (var rel in settings.Relations.Where(r => r.RoleIdIndex == oldIndex))
                {
                    rel.RoleIdIndex = oldIndex - 1;
                }
            }
        }

        /// <summary>
        /// Removes a relation based on its phraseIndex and roleIdIndex.
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="phraseIndex">index of the target Phrase</param>
        /// <param name="roleIdIndex">index of the target RoleID</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="RelationNotFoundException"></exception>
        public static void RemoveRelation(this RoleByPhraseSettings settings, int phraseIndex, int roleIdIndex)
        {
            settings.Phrases.ValidateIndex(phraseIndex);
            settings.RolesIds.ValidateIndex(roleIdIndex);

            if (!settings.Relations.Any(r => r.PhraseIndex == phraseIndex && r.RoleIdIndex == roleIdIndex))
            {
                throw new RelationNotFoundException();
            }

            settings.Relations = settings.Relations
                .Where(r => r.PhraseIndex != phraseIndex || r.RoleIdIndex != roleIdIndex)
                .ToList();
        }

        private static void ValidateIndex<T>(this IEnumerable<T> collection, int index)
        {
            if (index < 0 || index > collection.Count() - 1)
            {
                throw new ArgumentException($"Index {index} is outside of the scope of {nameof(collection)}.");
            }
        }
    }
}
