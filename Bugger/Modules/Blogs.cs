using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bugger.Configuration;
using Bugger.Extensions;
using Bugger.Features.Blogs;
using Bugger.Handlers;
using Discord;
using Discord.Commands;
using Newtonsoft.Json;

namespace Bugger.Modules
{
    [Group("Blog"), Summary("Ustawienia tworzenia blogów na tym serwerze, które użytkownicy będą mogli subskrybować, aby nie przegapili, jeśli opublikujesz nowego.\n")]
    public class Blogs : ModuleBase<MiunieCommandContext>
    {
        private static readonly string blogFile = "blogs.json";
        [Command("Stwórz"), Alias("Dodaj", "Create", "Add"), Remarks("Stworzę Ci bloga!")]
        public async Task Create(string name)
        {
            await Context.Message.DeleteAsync();

            var dataStorage = InversionOfControl.Container.GetInstance<JsonDataStorage>();
            var blogs = dataStorage.RestoreObject<List<BlogItem>>(blogFile) ?? new List<BlogItem>();

            if (blogs.FirstOrDefault(k=>k.Name == name) == null)
            {
                var newBlog = new BlogItem
                {
                    BlogId = Guid.NewGuid(),
                    Author = Context.User.Id,
                    Name = name,
                    Subscribers = new List<ulong>()
                };

                blogs.Add(newBlog);

                dataStorage.StoreObject(blogs, blogFile, Formatting.Indented);
                
                var embed = EmbedHandler.CreateEmbed($"Stworzyłem Twojego bloga o nazwie {name}!", EmbedHandler.EmbedMessageType.Success);
                await Context.Channel.SendMessageAsync("", false, embed);
            }
            else
            {
                var embed = EmbedHandler.CreateEmbed($"Już istnieje blog z tą nazwą: {name}", EmbedHandler.EmbedMessageType.Error);
                await Context.Channel.SendMessageAsync("", false, embed);
            }
        }

        [Command("Publikuj"), Alias("Postuj", "Opublikuj", "Wyślij"), Remarks("Opublikuję wybranego przez Ciebie bloga!")]
        public async Task Post(string name, [Remainder]string post)
        {
            await Context.Message.DeleteAsync();

            var blogs = InversionOfControl.Container.GetInstance<JsonDataStorage>().RestoreObject<List<BlogItem>>(blogFile);

            var blog = blogs.FirstOrDefault(k => k.Name == name && k.Author == Context.User.Id);

            if (blog != null)
            {
                var subs = string.Empty;
                foreach (var subId in blog.Subscribers)
                {
                    var sub = Context.Guild.GetUser(subId);
                    
                    subs += $"{sub.Mention},";
                }

                if (string.IsNullOrEmpty(subs))
                {
                    subs = "Brak subskrybentów";
                }

                var embed = EmbedHandler.CreateBlogEmbed(blog.Name, post, subs, EmbedHandler.EmbedMessageType.Info, true);
                var msg = Context.Channel.SendMessageAsync("", false, embed);
                
                if (Global.MessagesIdToTrack == null)
                {
                    Global.MessagesIdToTrack = new Dictionary<ulong, string>();
                }

                Global.MessagesIdToTrack.Add(msg.Result.Id, blog.Name);

                await msg.Result.AddReactionAsync(new Emoji("➕"));
            }
        }

        [Command("Subskrybuj"), Alias("Sub", "Subuj", "Obserwuj", "Subscribe", "Observe", "Follow")]
        [Remarks("Dam komuś Twojego suba!")]
        public async Task Subscribe(string name)
        {
            await Context.Message.DeleteAsync();

            var embed = BlogHandler.SubscribeToBlog(Context.User.Id, name);

            await Context.Channel.SendMessageAsync("", false, embed);
        }

        [Command("NieSubskrybuj"), Alias("UnSub", "NieSubuj", "UnSubskrybuj", "NieSubskrybuj", "NieObserwuj", "UnSubscribe", "UnObserve", "UnFollow")] 
        [Remarks("Zabiorę komuś Twojego suba!")]
        public async Task UnSubscribe(string name)
        {
            await Context.Message.DeleteAsync();

            var embed = BlogHandler.UnSubscribeToBlog(Context.User.Id, name);

            await Context.Channel.SendMessageAsync("", false, embed);
        }
    }
}
