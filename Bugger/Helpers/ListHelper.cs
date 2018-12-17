using Bugger.Extensions;
using Bugger.Features.Lists;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using static Bugger.Features.Lists.ListException;

namespace Bugger.Helpers
{
    public static class ListHelper
    {
        public enum ListPermission
        {
            PRIVATE,
            LIST,
            READ,
            PUBLIC
        };

        public static IReadOnlyList<String> PermissionStrings { get; } = new List<String>
        {
            "private",
            "view only",
            "read only",
            "public"
        };

        public static IReadOnlyDictionary<string, ListPermission> ValidPermissions { get; } = new Dictionary<string, ListPermission>
        {
            { "-p", ListPermission.PRIVATE },
            { "-l", ListPermission.LIST },
            { "-r", ListPermission.READ },
            { "-pu", ListPermission.PUBLIC }
        };

        public static IReadOnlyDictionary<string, Discord.Emoji> ControlEmojis { get; } = new Dictionary<string, Discord.Emoji>
        {
            {"up", new Discord.Emoji("⬆") },
            {"down", new Discord.Emoji("⬇") },
            {"check", new Discord.Emoji("✅") }
        };

        public struct UserInfo : IEquatable<object>
        {
            public ulong Id { get; }
            public ulong[] RoleIds { get; }

            public UserInfo(ulong id, ulong[] roleIds)
            {
                this.Id = id;
                this.RoleIds = roleIds;
            }
        }

        public enum ManagerMethodId
        {
            MODIFY,
            GETPRIVATE,
            GETPUBLIC,
            CREATEPRIVATE,
            CREATEPUBLIC,
            ADD,
            INSERT,
            OUTPUTPRIVATE,
            OUTPUTPUBLIC,
            REMOVE,
            REMOVELIST,
            CLEAR
        }

        public static IReadOnlyList<ManagerMethod> GetValidOperations(ListManager manager)
        {
            var validOperations = new List<ManagerMethod>
            {
                new ManagerMethod("-m",     ManagerMethodId.MODIFY,         (userInfo, availableRoles, args) => manager.ModifyPermission(userInfo, availableRoles, args)),
                new ManagerMethod("-g",     ManagerMethodId.GETPRIVATE,     (userInfo, availableRoles, args) => manager.GetAllPrivate(userInfo)),
                new ManagerMethod("-gp",    ManagerMethodId.GETPUBLIC,      (userInfo, availableRoles, args) => manager.GetAllPublic(userInfo)),
                new ManagerMethod("-c",     ManagerMethodId.CREATEPRIVATE,  (userInfo, availableRoles, args) => manager.CreateListPrivate(userInfo, args)),
                new ManagerMethod("-cp",    ManagerMethodId.CREATEPUBLIC,   (userInfo, availableRoles, args) => manager.CreateListPublic(userInfo, args)),
                new ManagerMethod("-a",     ManagerMethodId.ADD,            (userInfo, availableRoles, args) => manager.Add(userInfo, args)),
                new ManagerMethod("-i",     ManagerMethodId.INSERT,         (userInfo, availableRoles, args) => manager.Insert(userInfo, args)),
                new ManagerMethod("-l",     ManagerMethodId.OUTPUTPRIVATE,  (userInfo, availableRoles, args) => manager.OutputListPrivate(userInfo, args)),
                new ManagerMethod("-lp",    ManagerMethodId.OUTPUTPUBLIC,   (userInfo, availableRoles, args) => manager.OutputListPublic(userInfo, args)),
                new ManagerMethod("-r",     ManagerMethodId.REMOVE,         (userInfo, availableRoles, args) => manager.Remove(userInfo, args)),
                new ManagerMethod("-rl",    ManagerMethodId.REMOVELIST,     (userInfo, availableRoles, args) => manager.RemoveList(userInfo, args)),
                new ManagerMethod("-cl",    ManagerMethodId.CLEAR,          (userInfo, availableRoles, args) => manager.Clear(userInfo, args))
            };
            return validOperations;
        }

        public struct ManagerMethod : IEquatable<object>
        {
            public string Shortcut { get; set; }
            public ManagerMethodId MethodId { get; set; }
            public Func<UserInfo, Dictionary<string, ulong>, string[], ListOutput> Reference { get; set; }

            public ManagerMethod(string shortcut, ManagerMethodId methodId, Func<UserInfo, Dictionary<string, ulong>, string[], ListOutput> reference)
            {
                Shortcut = shortcut;
                MethodId = methodId;
                Reference = reference;
            }
        }

        public struct ListOutput : IEquatable<object>
        {
            public string outputString { get; set; }
            public Discord.Embed outputEmbed { get; set; }
            public bool listenForReactions { get; set; }
            public ListPermission permission { get; set; }
        }

        public static ListOutput GetListOutput(string s)
        {
            return new ListOutput { outputString = s, permission = ListPermission.PUBLIC };
        }

        public static ListOutput GetListOutput(string s, ListPermission p)
        {
            return new ListOutput { outputString = s, permission = p };
        }

        public static ListOutput GetListOutput(Discord.Embed e)
        {
            return new ListOutput { outputEmbed = e, permission = ListPermission.PUBLIC };
        }

        public static ListOutput GetListOutput(Discord.Embed e, ListPermission p)
        {
            return new ListOutput { outputEmbed = e, permission = p };
        }

        public static string GetNounPlural(string s, int count)
        {
            return $"{s}{(count < 2 ? "" : "s")}";
        }

        public static SeperatedArray SeperateArray(string[] input)
        {
            return SeperateArray(input, input.Length - 1);
        }

        public static SeperatedArray SeperateArray(string[] input, params int[] indices)
        {
            var sa = new SeperatedArray();
            sa.seperated = new string[indices.Length];
            for (int i = 0; i < indices.Length; i++)
            {
                sa.seperated[i] = input[indices[i]];
            }
            sa.array = new string[input.Length - indices.Length];
            var currentIndex = 0;
            for (int i = 0; i < input.Length; i++)
            {
                if (!indices.Contains(i))
                {
                    sa.array[currentIndex++] = input[i];
                }
            }
            return sa;
        }

        public struct SeperatedArray : IEquatable<object>
        {
            public string[] seperated { get; set; }
            public string[] array { get; set; }
        }
    }
}
