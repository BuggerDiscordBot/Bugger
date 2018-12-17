using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bugger.Configuration;
using Bugger.Features.Blogs;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace Bugger.Handlers
{
    class BlogHandler
    {
        private static readonly string blogFile = "blogs.json";
        public static Embed SubscribeToBlog(ulong userId, string blogname)
        {
            var dataStorage = InversionOfControl.Container.GetInstance<JsonDataStorage>();
            var blogs = dataStorage.RestoreObject<List<BlogItem>>(blogFile);

            var blog = blogs.FirstOrDefault(k => k.Name == blogname);

            if (blog != null)
            {
                if (!blog.Subscribers.Contains(userId))
                {
                    blog.Subscribers.Add(userId);

                    dataStorage.StoreObject(blogs, blogFile, Formatting.Indented);

                    return EmbedHandler.CreateEmbed("Obserwujesz teraz tego bloga", EmbedHandler.EmbedMessageType.Success);
                }
                else
                {
                    return EmbedHandler.CreateEmbed("Już obserwowałeś tego bloga", EmbedHandler.EmbedMessageType.Info);
                }
            }
            else
            {
                return EmbedHandler.CreateEmbed($"Nie znalazłem bloga z nazwą: {blogname}", EmbedHandler.EmbedMessageType.Error);
            }
        }

        public static Embed UnSubscribeToBlog(ulong userId, string blogname)
        {
            var dataStorage = InversionOfControl.Container.GetInstance<JsonDataStorage>();
            var blogs = dataStorage.RestoreObject<List<BlogItem>>(blogFile);

            var blog = blogs.FirstOrDefault(k => k.Name == blogname);

            if (blog != null)
            {
                if (blog.Subscribers.Contains(userId))
                {
                    blog.Subscribers.Remove(userId);

                    dataStorage.StoreObject(blogs, blogFile, Formatting.Indented);

                    return EmbedHandler.CreateEmbed("Nie obserwujesz już tego bloga", EmbedHandler.EmbedMessageType.Success);
                }
                else
                {
                    return EmbedHandler.CreateEmbed("Nie obserwowałeś tego bloga", EmbedHandler.EmbedMessageType.Info);
                }
            }
            else
            {
                return EmbedHandler.CreateEmbed($"Nie znalazłem bloga z nazwą: {blogname}", EmbedHandler.EmbedMessageType.Error);
            }
        }

        public static async Task ReactionAdded(SocketReaction reaction)
        {
            var msgList = Global.MessagesIdToTrack ?? new Dictionary<ulong, string>();
            if (msgList.ContainsKey(reaction.MessageId))
            {
                if (reaction.Emote.Name == "➕")
                {
                    var item = msgList.FirstOrDefault(k => k.Key == reaction.MessageId);
                    var embed = BlogHandler.SubscribeToBlog(reaction.User.Value.Id, item.Value);
                }
            }
        }
    }
}
