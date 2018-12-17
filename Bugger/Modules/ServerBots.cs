using Discord.Commands;
using Discord.WebSocket;
using Discord;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bugger.Extensions;
using Bugger.Features.GlobalAccounts;

namespace Bugger.Modules
{
    [Group("Bot"), Alias("Boty", "Bots")]
    [Summary("Ustawienia mojego menadżerowania zaproszeniami dla botów, aby w wolnej chwili administratorzy otrzymali mogli otrzymać listę życzeń od np. użytkowników. [Narazie tylko dla współtwórców]\n")]
    public class ServerBots : ModuleBase<MiunieCommandContext>
    {
        [Serializable]
        public class AllGuildsData
        {
            public List<GuildData> guilds;

            public GuildData GetGuild(ulong id)
            {
                foreach (GuildData guild in guilds)
                    if (guild.guildId == id)
                        return guild;

                return null;
            }

            public AllGuildsData()
            {
                guilds = new List<GuildData>();
            }
        }

        [Serializable]
        public class GuildData
        {
            public ulong guildId;
            public List<Submission> queue;
            public List<Submission> archive;

            public GuildData(ulong _guildId)
            {
                guildId = _guildId;
                queue = new List<Submission>();
                archive = new List<Submission>();
            }

            public Submission GetSubmissionFromQueue(ulong id)
            {
                foreach (Submission submission in queue)
                    if (submission.botId == id)
                        return submission;

                return null;
            }

            public Submission GetSubmissionFromArchives(ulong id)
            {
                foreach (Submission submission in archive)
                    if (submission.botId == id)
                        return submission;

                return null;
            }
        }

        [Serializable]
        public class Submission
        {
            public ulong userId;
            public ulong botId;
            public string name;
            public string description;
            public DateTime timeSent;

            public Submission(ulong _botId, ulong _userId, string _name, string _description, DateTime _timeSent)
            {
                botId = _botId;
                name = _name;
                userId = _userId;
                description = _description;
                timeSent = _timeSent;
            }
        }

        public static AllGuildsData data;

        const string LINK_TEMPLATE_FIRST = "https://discordapp.com/api/oauth2/authorize?client_id=";
        const string LINK_TEMPLATE_LAST = "&scope=bot&permissions=1";
        const int SUBMISSIONS_PER_PAGE = 4;

        public static Task Init()
        {
            data = new AllGuildsData();

            foreach (SocketGuild guild in Global.Client.Guilds)
            {
                GuildData savedData = GlobalGuildAccounts.GetGuildAccount(guild.Id).BotData;
                if (savedData == null)
                {
                    AddGuild(guild.Id);
                }
                else
                {
                    data.guilds.Add(savedData);
                }
                StoreData(guild.Id);
            }

            return Task.CompletedTask;
        }

        public static Task JoinedGuild(SocketGuild guild)
        {
            AddGuild(guild.Id);
            return Task.CompletedTask;
        }

        async Task ArchiveSubmission(ulong id)
        {
            GuildData guildData = data.GetGuild(Context.Guild.Id);
            if (guildData != null)
            {
                Submission submission = guildData.GetSubmissionFromQueue(id);
                if (submission != null)
                {
                    guildData.archive.Add(submission);
                    guildData.queue.Remove(submission);
                    StoreData(id);
                    await Context.Channel.SendMessageAsync("Zrobione!");
                }
                else
                {
                    await Context.Channel.SendMessageAsync("Nie znalazłem tego ID w mojej liście.");
                }
            }
            else
            {
                await ReplyAsync("BŁĄD: Nie znalazłem danych serwera.");
            }
        }

        static void AddGuild(ulong guildId)
        {
            data.guilds.Add(new GuildData(guildId));
        }

        async Task AddSubmission(Submission submission, ulong guildId)
        {
            GuildData guild = data.GetGuild(guildId);
            if (guild != null)
            {
                guild.queue.Add(submission);
                StoreData(guildId);
                await Context.Channel.SendMessageAsync("Podsumowanie wysłane!");
            }
            else
            {
                await Context.Channel.SendMessageAsync("Nie udało mi się podsumować...");
            }
        }

        static void StoreData(ulong id)
        {
            var guildAccount = GlobalGuildAccounts.GetGuildAccount(id);
            guildAccount.Modify(g => g.SetBotData(data.GetGuild(id)));
        }

        [Command("add")]
        [Alias("add")]
        [Remarks("Dodam link zaproszeniowy z botem do kolejki botów z której administratorzy będą mogli zweryfikować Twoją proźbę. Skł: `<p>bot dodaj <IdClientaBota> <NazwaBota> \"|\" <Opis>\n")]
        [RequireOwner]
        public async Task AddBot(params string[] argumenty)
        {
            if (ulong.TryParse(argumenty[0], out ulong id))
            {
                if (data.GetGuild(Context.Guild.Id).GetSubmissionFromQueue(id) != null || data.GetGuild(Context.Guild.Id).GetSubmissionFromArchives(id) != null)
                {
                    await ReplyAsync("This bot has already been submitted.");
                    return;
                }

                if (argumenty.Length > 2)
                {
                    string botName = "";
                    string description = "";
                    bool syntax = false;
                    for (int i = 1; i < argumenty.Length; i++)
                    {
                        if (argumenty[i] == "|")
                            syntax = true;
                        else if (syntax)
                        {
                            description += argumenty[i];
                            if (argumenty.Length - 1 > i)
                                description += " ";
                        }
                        else
                        {
                            botName += argumenty[i];
                            if (argumenty.Length - 1 > i)
                                botName += " ";
                        }
                    }

                    if (syntax)
                    {
                        await AddSubmission(new Submission(id, Context.User.Id, botName, description, DateTime.Now), Context.Guild.Id);
                    }
                    else
                        await ReplyAsync("Proszę dołącz jeszcze opis twojego bota.");

                }
                else
                {
                    await ReplyAsync("Proszę dołącz jeszcze nazwę swojego bota.");
                }
            }
            else
            {
                await ReplyAsync("Dopisz prawidłowe ID bota.");
            }
        }

        [Command("list")]
        [Alias("list")]
        [RequireOwner]
        public async Task ViewBots(params string[] argumenty)
        {
            if (!(argumenty.Length == 2))
            {
                await ReplyAsync("Nie użyłeś prawidłowej liczby argumentów.");
            }
            if (int.TryParse(argumenty[0], out int page))
            {
                if (page <= 0)
                {
                    await ReplyAsync("Nice try!!!");
                    return;
                }

                List<Submission> list;
                if (argumenty[1] == "pending")
                    list = data.GetGuild(Context.Guild.Id).queue;
                else if (argumenty[1] == "archives")
                    list = data.GetGuild(Context.Guild.Id).archive;
                else
                {
                    await ReplyAsync("Sprecyzuj które!");
                    return;
                }

                if (list.Count == 0)
                {
                    await ReplyAsync("Nie ma tylu podsumowań na tej liście!");
                    return;
                }

                decimal pages = Math.Ceiling((decimal)(list.Count) / SUBMISSIONS_PER_PAGE);

                EmbedBuilder builder = new EmbedBuilder
                {
                    Title = $"**Linki botów z listy {argumenty[1]}**",
                    Description = "",
                    Color = new Color(119, 165, 239),
                    Footer = new EmbedFooterBuilder
                    {
                        Text = $"Strona {page}/{pages}"
                    }
                };

                if (pages >= page)
                {
                    for (int i = 0; i < SUBMISSIONS_PER_PAGE; i++)
                    {
                        try
                        {
                            int index = i + (SUBMISSIONS_PER_PAGE * (page - 1));

                            if (index < list.Count)
                            builder.Description += $"{index + 1}. [{list[index].name}]({LINK_TEMPLATE_FIRST + list[index].botId + LINK_TEMPLATE_LAST})" +
                                $"przez **{Context.Client.GetUser(list[index].userId).Username}**:\n{list[index].description}\n" +
                                $"*Client ID: {list[index].botId}*\n{list[index].timeSent} {TimeZone.CurrentTimeZone.StandardName}\n\n";
                        }
                        catch (IndexOutOfRangeException)
                        {
                            break;
                        }
                    }

                    await ReplyAsync("", false, builder.Build());
                }
                else
                {
                    await ReplyAsync("Nie ma tyle stron!");
                }
            }
            else
            {
                await ReplyAsync("Pierwszym argumentem musi być liczba całkowita!");
            }
        }

        [Command("usuń"), RequireUserPermission(GuildPermission.ManageGuild)]
        [Alias("Remove")]
        [RequireOwner]
        public async Task RemoveBot(params string[] argumenty)
        {
            if (!(argumenty.Length == 2))
            {
                await ReplyAsync("Please use the right number of arguments.");
                return;
            }

            if (ulong.TryParse(argumenty[0], out ulong id))
            {
                Submission toRemove;
                GuildData guildData = data.GetGuild(Context.Guild.Id);
                if (guildData != null)
                {
                    if (argumenty[1] == "archives")
                    {
                        toRemove = guildData.GetSubmissionFromArchives(id);
                        if (toRemove != null)
                        {
                            guildData.archive.Remove(toRemove);
                            StoreData(id);
                            await ReplyAsync("Successfully removed submission from the archives list.");
                        }
                        else
                            await ReplyAsync("Could not find bot in archives list.");
                    }
                    else if (argumenty[1] == "pending")
                    {
                        toRemove = guildData.GetSubmissionFromQueue(id);
                        if (toRemove != null)
                        {
                            guildData.queue.Remove(toRemove);
                            StoreData(id);
                            await ReplyAsync("Successfully removed submission from the pending list.");
                        }
                        else
                            await ReplyAsync("Could not find bot in pending list.");
                    }
                    else
                        await ReplyAsync("Please specify either archives or pending lists.");
                }
                else
                    await ReplyAsync("BŁĄD: podczas wczytywania danych serwera");
            }
            else
            {
                await ReplyAsync("Please type a valid bot id.");
            }
        }

        [Command("Archiwizuj"), RequireUserPermission(GuildPermission.ManageGuild)]
        [Alias("archive")]
        [Remarks("Użycie: bots archive <BotId>")]
        [RequireOwner]
        public async Task ArchiveBot(params string[] args)
        {
            if (args.Length != 1)
            {
                await ReplyAsync("Zła ilość argumentów.");
                return;
            }

            if (ulong.TryParse(args[0], out ulong id))
            {
                await ArchiveSubmission(id);
            }
            else
            {
                await ReplyAsync("Złe ID Clienta.");
            }
        }
    }
} 
