using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Newtonsoft.Json;

namespace Bugger.Helpers
{
    public class GitHub
    {
        public static async Task<List<Contribution>> Contributions(string user, string repo)
        {
            var jsonResponse = await Global.SendWebRequest(
                    $"https://api.github.com/repos/{user}/{repo}/stats/contributors"
                );

            var contributions = new List<Contribution>();

            try
            {
                contributions = JsonConvert.DeserializeObject<List<Contribution>>(jsonResponse);
            }
            catch (JsonException e)
            {
                var msg = new LogMessage(LogSeverity.Error, $"HTTP / JSON Error  | Deserialization of response failed",
                    e.Message, e.InnerException);
            }
            return contributions;
        }

        public static EmbedFieldBuilder ContributionStat(Contribution contribution, Tuple<int, int> edits)
        {
            return new EmbedFieldBuilder
            {
                Name = Constants.InvisibleString,
                Value = $"__**[{contribution.Author.login}]({contribution.Author.html_url})**__\n" +
                        $"Commits  : {contribution.total}\n" +
                        $"Additions: {edits.Item1}\n" +
                        $"Deletions: {edits.Item2}",
                IsInline = true
            };
        }

        public struct Author
        {
            public string login;
            public string name;
            public int id;
            public string avatar_url;
            public string url;
            public string html_url;
            public int total;
        }

        public struct Contribution
        {
            public Author Author;
            public List<Week> weeks;
            public int total;
        }

        public struct Week
        {
            public string w;
            public int a;
            public int d;
            public int c;
        }
    }
}
