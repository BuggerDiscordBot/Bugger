using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace Bugger.Features.Trivia
{
    public class TriviaGames
    {
        private const string UBase = "https://opentdb.com/";
        private const string UApi = "api.php";
        private const string UGetToken = "api_token.php?command=request";
        private const string UGetCategories = "api_category.php";
        private const string UCount = "?amount=";
        private const string UCategory = "&category=";
        private const string UType = "&type=";
        private const string UDiff = "&difficulty=";

        
        private const string UToken = "&token=";
        /// Maps the question types the API understands with their values to be displayed
        internal readonly Dictionary<string, string> QuestionTypes;
        /// Maps the difficulties the API understands with the values to be displayed
        internal readonly Dictionary<string, string> Difficulties;
        /// Maps simple strings to Discord Emojis for easier usage
        internal readonly Dictionary<string, Emoji> ReactOptions;
        internal readonly List<Category> Categories;
        private readonly List<TriviaGame> _activeTriviaGames;

        private readonly Logger _logger;

        private DiscordSocketClient _client;

        public TriviaGames(Logger logger, DiscordSocketClient client)
        {
            _logger = logger;
            _client = client;
            _activeTriviaGames = new List<TriviaGame>();

            Difficulties =  new Dictionary<string, string>
            {
                {"easy", "Easy"}, {"medium", "Medium"}, {"hard", "Hard"}, {"", "Any"}
            };
            ReactOptions = new Dictionary<string, Emoji>
            {
                { "1", new Emoji("1⃣")}, { "2", new Emoji("2⃣")}, {"3", new Emoji("3⃣")}, {"4", new Emoji("4⃣")},
                {"ok", new Emoji("🆗")}, { "right", new Emoji("➡") }, {"left", new Emoji("⬅")}
            };
            QuestionTypes = new Dictionary<string, string>
            {
                {"", "Any"}, {"multiple", "Multiple Choice"}, {"boolean", "True / False"}
            };
            Categories = GetCategories();
        }

        /// <summary>
        /// Adds a new TriviaGame to the active Trivia Games
        /// </summary>
        internal async Task NewTrivia(IUserMessage msg, IUser user)
        {
            _activeTriviaGames.Add(new TriviaGame(msg.Id, user.Id, this));
            await msg.AddReactionAsync(ReactOptions["1"]);
            await msg.AddReactionAsync(ReactOptions["2"]);
            await msg.AddReactionAsync(ReactOptions["3"]);
            await msg.AddReactionAsync(ReactOptions["4"]);
            await msg.AddReactionAsync(ReactOptions["ok"]);
        }


        /// <summary>
        /// Checks if given reaction associated to a running game and if the person who game 
        /// the reaction is the same who created this game - if so initiates handling the game mechanics
        /// </summary>
        internal async Task HandleReactionAdded(Cacheable<IUserMessage, ulong> cache, SocketReaction reaction)
        {
            // Only trigger if we listed this messsage and only act on 
            // reactions from the user who triggerd the command
            var triviaGame = _activeTriviaGames.FirstOrDefault(game =>
                game.GameMessageId == reaction.MessageId &&
                game.PlayerId == reaction.UserId);
            if (triviaGame != null)
            {
                var msg = await cache.GetOrDownloadAsync();
                // Immediatly remove reaction so user is able to use it as input 
                // Check permissions first
                if (reaction.UserId != msg.Author.Id)
                {
                    var user = reaction.User.GetValueOrDefault(null) ?? _client.GetUser(reaction.UserId);
                    try
                    {
                        await msg.RemoveReactionAsync(reaction.Emote, user);
                    }
                    catch (Exception e)

                    {
                        await _logger.Log(new LogMessage(LogSeverity.Warning, $"Discord | Missing Permissions to remove reaction in {msg.Channel}", e.Message, e.InnerException));
                    }
                }
                await triviaGame.HandleReaction(msg, reaction);
            }
        }
    
        /// <summary>
        /// Receives all the available categories (and adds the "Any" category)
        /// </summary>
        private List<Category> GetCategories()
        {
            var responseString = Global.SendWebRequest(UBase + UGetCategories).Result;
            var response = JsonConvert.DeserializeObject<Response>(responseString);
            var cats = new List<Category>();
            if (response.response_code == 0) cats = response.trivia_categories;
            cats.Add(new Category{ name = "Any", id = ""});
            return cats;
        }

        /// <summary>
        /// Creates a new token for use of GetQuestions so questions won't get repeated
        /// in one game session.
        /// </summary>
        internal async Task<string> NewToken()
        {
            var response = JsonConvert.DeserializeObject<Response>(
                await Global.SendWebRequest(UBase + UGetToken));
            return response.response_code == 0 ? response.token : "";
        }

        /// <summary>
        /// Sends a query to https://opentdb.com/api.php with the parameters (all parameters are optional)
        /// </summary>
        /// <param name="count">Amount of questions to get | defaults to 1</param>
        /// <param name="categoryId">Either the string or the id_string of the category | defaults to any category</param>
        /// <param name="difficulty">Can be easy, medium, hard or any | defaults to any difficulty</param>
        /// <param name="type">The type the questions should be any, multiple or boolean | defaults to any</param>
        /// <param name="token">The token - used to not get the same questions when repeating this fuction | defaults to an empty string which means no token</param>
        /// <returns>The awaitable List of questions or an empty list if the request was not successfull</returns>
        internal async Task<List<Question>> GetQuestions(int count = 1, string categoryId = "", string difficulty = "", string type = "", string token = "")
        {
            // Creating the actual request link
            var request = UBase + UApi + UCount + count + UCategory + categoryId + UDiff + difficulty + UType + type + UToken + token;
            // Getting the json string respond from the request link
            var responseJson = await Global.SendWebRequest(request);
            // Converting the string into a Response object
            var response =  JsonConvert.DeserializeObject<Response>(responseJson);
            if (response.response_code == 4)
            {
                return new List<Question>()
                {
                    new Question
                    {
                        question =
                            "**OHH!**\nIt seems like you have answered all available questions that match these settings...\n\n" +
                            "Try to change some of them in the main menu!\n\n" +
                            "~~Answering this question with anything but the emoji to go back to the menu " +
                            "will tell you that it you answered it correctly but won't count it to your stats...~~",
                        category = categoryId,
                        type = "exhausted"
                    }
                };
            }

            if (response.response_code != 0) return new List<Question>();
            // Convert all text in the results to utf-8 characters (replace eg. '&#x3C;' with '<')
            response.results = response.results.ConvertAll<Question>(question =>
            {
                question.correct_answer = WebUtility.HtmlDecode(question.correct_answer);
                question.incorrect_answers = question.incorrect_answers.ConvertAll<string>(WebUtility.HtmlDecode);
                return question;
            });
            return response.results;
        }

        /// <summary>
        /// Returns an EmbedBuilder with all the information set to send out the start menu embed
        /// </summary>
        /// <param name="game">Optional | If provied takes the settings of that game (Difficulty, QuestionType, Category)</param>
        internal EmbedBuilder TrivaStartingEmbed(TriviaGame game = null)
        {
            var difficulty = game == null ? "Any" : Difficulties[game.Difficulty];
            var questionType = game == null ? "Any" : QuestionTypes[game.QuestionType];
            var category = game == null ? "Any" : game.Category.name;
            return new EmbedBuilder()
                .WithAuthor("Welcome to Trivia!")
                .WithDescription("What do you Want to do?")
                .WithColor(Color.Blue)
                .WithFooter("Use the reactions down here to take a choice. \n(Only the one who called the command can use them)")
                .AddField(ReactOptions["1"] + " Change Category", category, true)
                .AddField(ReactOptions["2"] + " Change question type", questionType, true)
                .AddField(ReactOptions["3"] + " Change difficulty", difficulty, true)
                .AddField(Constants.InvisibleString, ReactOptions["ok"] + " **Start**");
        }

        /// <summary>
        /// Sets all the requried information to display the question in an embed
        /// </summary>
        /// <param name="q">The question to display</param>
        /// <param name="emb">The embedbuilder from which to inherit some properties (Title, Author, Footer)</param>
        internal EmbedBuilder QuestionToEmbed(Question q, EmbedBuilder emb)
        {
            // Inherit information of the given embed
            var embB = new EmbedBuilder()
                .WithTitle(emb.Title)
                .WithAuthor(emb.Author)
                .WithFooter(emb.Footer);
            // If there are no more questions whith these settings tell the player so
            if (q.type == "exhausted")
            {
                embB.WithColor(Color.Orange)
                    .WithDescription(q.question);
                return embB;
            }
            // Set Color and Description
            embB.WithColor(emb.Color.GetValueOrDefault(Color.Blue))
                .WithDescription($"[{Difficulties[q.difficulty]} | {q.category}]\n" +
                                 $"**{WebUtility.HtmlDecode(q.question)}**");

            // Merge correct with incorrect answers and shuffle them
            var answers = q.incorrect_answers.Concat(new List<string>{q.correct_answer});
            var answersShuffled = answers.OrderBy(t => Global.Rng.Next()).ToList();
            
            for (var i = 1; i <= answersShuffled.Count; i++)
            {
                // Add empty fields in so the answers are visible in a 2 x 2 grid instead of
                // 3 in one row and then 1 lonely below (if it is 4 answers) - purely cosmetic
                if (embB.Fields.Count % 3 == 2) embB.AddField(Constants.InvisibleString, Constants.InvisibleString);
                // Get the correct emoji for i
                ReactOptions.TryGetValue(i.ToString(), out var reactWith);
                // Adds the correct reaction for i and the question to the embedbuilder
                embB.AddField(reactWith.Name, WebUtility.HtmlDecode(answersShuffled[i-1]), true);
            }
            return embB;
        }

        /// <summary>
        /// Get a specified amount of categories out of all available ones
        /// </summary>
        /// <param name="page">Page to acces (one-based!)</param>
        /// <param name="pagesize">Amount of categories per page</param>
        /// <returns>A list of Categories with size of pagesize (or lower if it is the last page)</returns>
        internal List<Category> CategoriesPaged(int page, int pagesize)
        {
            // one-based input so for easier internal handling make it zero-based again
            page--;
            var startIndex = page * pagesize;
            return Categories.GetRange(startIndex, Math.Min(pagesize, Categories.Count - (startIndex)));
        }
    }

    /// <summary>
    /// States a TriviaGame can be in
    /// </summary>
    internal enum GameStates
    {
        StartPage, ChangingCategory, Playing
    }

    /// <summary>
    /// Struct for parsing different json string responses of the API. 
    /// Combines multiple possible results - not all fields are actually available at the same time 
    /// if you parse an API response into this! 
    /// Only response_code will always be available. 
    /// </summary>
    internal struct Response
    {
        public int response_code;
        public string response_Message;
        public List<Question> results;
        public List<Category> trivia_categories;
        public string token;
    }

    /// <summary>
    /// Struct for parsing API responds into Questions - see Response struct
    /// </summary>
    internal struct Question
    {
        public string category;
        public string type;
        public string difficulty;
        public string question;
        public string correct_answer;
        public List<string> incorrect_answers;
        public bool correct;
    }

    /// <summary>
    /// Struct for parsing AIP respons into Categories - see Response struct
    /// </summary>
    public struct Category
    {
        public string name;
        public string id;
    }
}
