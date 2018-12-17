using Discord;

namespace Bugger.Handlers
{
    public static class EmbedHandler
    {
        /// <summary>
        /// Create a new embed
        /// </summary>
        /// <param name="title">Tytuł</param>
        /// <param name="footer">Stopka</param>
        /// <param name="type">Type of the Embed (Error, Info, Exception, Success) -> Sets the color</param>
        /// <param name="withTimeStamp">Adds the current Timestamp to the embed</param>
        /// <returns></returns>
        public static Embed CreateEmbed(string title, EmbedMessageType type)
        {
            var embed = new EmbedBuilder();
            embed.WithTitle(title);
            embed.WithFooter(Global.GetRandomDidYouKnow());
            embed.Footer.WithIconUrl("https://www.leedsfestival.com/wp-content/uploads/2018/02/info-cat-icon-essentials-update.png");

            switch (type)
            {
                case EmbedMessageType.Info:
                    embed.WithColor(new Color(0, 175, 255));
                    break;
                case EmbedMessageType.Success:
                    embed.WithColor(new Color(255, 240, 0));
                    break;
                case EmbedMessageType.Error:
                    embed.WithColor(new Color(255, 0, 0));
                    break;
                case EmbedMessageType.Exception:
                    embed.WithColor(new Color(255, 136, 0));
                    break;
                default:
                    embed.WithColor(new Color(0, 255, 0));
                    break;
            }

            return embed.Build();
        }

        public static Embed CreateBlogEmbed(string title, string body, string subscribers, EmbedMessageType type, bool withTimeStamp = false)
        {
            var embed = new EmbedBuilder();
            embed.WithTitle($"Blog: {title}");
            embed.WithDescription(body);
            embed.AddField("Subscribers", subscribers);

            embed.WithFooter("+ to subscribe");

            switch (type)
            {
                case EmbedMessageType.Info:
                    embed.WithColor(new Color(0, 175, 255));
                    break;
                case EmbedMessageType.Success:
                    embed.WithColor(new Color(255, 240, 0));
                    break;
                case EmbedMessageType.Error:
                    embed.WithColor(new Color(255, 0, 0));
                    break;
                case EmbedMessageType.Exception:
                    embed.WithColor(new Color(255, 136, 0));
                    break;
                default:
                    embed.WithColor(new Color(0, 255, 0));
                    break;
            }

            if (withTimeStamp)
            {
                embed.WithCurrentTimestamp();
            }

            return embed.Build();
        }

        public enum EmbedMessageType
        {
            Success = 0,
            Info = 10,
            Error = 20,
            Exception = 30
        }
    }
}
