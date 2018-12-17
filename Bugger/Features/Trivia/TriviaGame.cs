using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;

namespace Bugger.Features.Trivia
{
    public class TriviaGame
    {
        // Used to identify which reactions to actually react to
        internal ulong PlayerId;
        // Used to identify if a reaction is ment for this game
        internal ulong GameMessageId;

        internal string Difficulty;
        internal Category Category;
        internal string QuestionType;
        private static string _token;

        private readonly List<Question> previousQuestions;
        private Question _currentQuestion;
        private GameStates _gamestate;
        private EmbedBuilder _emb;
        private int _currentCategoryPage;

        private readonly TriviaGames _triviaGames;

        private const int CategoriesPerPage = 4;

        internal TriviaGame(ulong gameMessageId, ulong playerId, TriviaGames triviaGames, string category = "any",
            string difficulty = "", string questionType = "")
        {
            _triviaGames = triviaGames;
            PlayerId = playerId;
            GameMessageId = gameMessageId;
            Difficulty = difficulty;
            QuestionType = questionType;
            previousQuestions = new List<Question>();
            Category = _triviaGames.Categories.Find(cat => cat.name.ToLower() == category);
            _token = _triviaGames.NewToken().Result;
            _currentCategoryPage = 1;
            _gamestate = GameStates.StartPage;
            _emb = _triviaGames.TrivaStartingEmbed(this);
        }

        // Main function that gets triggered when a player reacts to their gamemessage
        internal async Task HandleReaction(IUserMessage socketMsg, IReaction reaction)
        {
            switch (_gamestate)
            {
                case (GameStates.StartPage):
                    await HandleStartPageInput(socketMsg, reaction);
                    break;
                case (GameStates.ChangingCategory):
                    HandleSelectCategoryInput(socketMsg, reaction);
                    break;
                case (GameStates.Playing):
                    await HandlePlayingInput(socketMsg, reaction);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // Actually update the message with all the information we set 
            // in the cases depending on the gamestate
            await UpdateMessage(socketMsg);
        }

        /// <summary>
        /// Updates a message with the games embed
        /// </summary>
        private async Task UpdateMessage(IUserMessage socketMsg)
        {
            await socketMsg.ModifyAsync(message =>
            {
                message.Embed = _emb.Build();
                // This somehow can't be empty or it won't update the 
                // embed propperly sometimes... I don't know why
               
            });
        }

        /// <summary>
        /// Sets all the neccessary information of the games embedbuilder to send out the startmenu 
        /// including the possibly available stats
        /// </summary>
        private void PrepareStartMenue(IUserMessage socketMsg)
        {
            // Get the startmenu embedbuilder that has all the settings of this game set up
            _emb = _triviaGames.TrivaStartingEmbed(this);
            // String to build up the stats
            var stats = "";
            // Group the questions by difficulty for stats
            var orderd = previousQuestions.GroupBy(question => question.difficulty, question => question).ToList();
            // Generate the stats by saving global wins/losses and difficulty dependend wins/losses
            double overallCorrect = 0;
            double overallWrong = 0;
            foreach (var difficulty in orderd)
            {
                if (difficulty.Key == null) continue;
                double correct = 0;
                double wrong = 0;
                foreach (var question in difficulty)
                {
                    if (question.correct) correct++;
                    else wrong++;
                }

                overallCorrect += correct;
                overallWrong += wrong;
                stats += $"{_triviaGames.Difficulties[difficulty.Key]}: " +
                         $"**{(int)(correct / (correct + wrong) * 100)}%** correct ({correct}/{correct + wrong})\n";
            }
            // Only show the overall stat if the player had questions with different difficulties
            if (orderd.Count > 1)
                stats += $"Overall: " +
                         $"**{(int)(overallCorrect / (Math.Max(overallCorrect + overallWrong, 1)) * 100)}%**" +
                         $" correct ({overallCorrect}/{overallCorrect + overallWrong})\n";
            // Only add a field if there is actually something to show
            if (!string.IsNullOrEmpty(stats)) _emb.AddField("Your Stats: ", stats);
            _gamestate = GameStates.StartPage;
        }

        /// <summary>
        /// Handles behaviour on reactions while in the selectCategory gamestate
        /// </summary>
        /// <param name="socketMsg"></param>
        /// <param name="reaction"></param>
        private void HandleSelectCategoryInput(IUserMessage socketMsg, IReaction reaction)
        {
            if (reaction.Emote.Equals(_triviaGames.ReactOptions["ok"]))
            {
                _gamestate = GameStates.StartPage;
                PrepareStartMenue(socketMsg);
                return;
            }
            if (reaction.Emote.Equals(_triviaGames.ReactOptions["left"]))
            {
                _currentCategoryPage = Math.Max(_currentCategoryPage - 1, 1);
                PrepareCategoryEmb();
                return;
            }
            if (reaction.Emote.Equals(_triviaGames.ReactOptions["right"]))
            {
                _currentCategoryPage = Math.Min(_currentCategoryPage + 1, 1 + _triviaGames.Categories.Count / CategoriesPerPage);
                PrepareCategoryEmb();
                return;
            }
            Category = PickCategory(socketMsg.Embeds.FirstOrDefault(), reaction);
            _gamestate = GameStates.StartPage;
            PrepareStartMenue(socketMsg);
        }

        /// <summary>
        /// Takes a specifc formated embed and returns the correct category according to the given reaction
        /// </summary>
        /// <param name="emb">Embed which has categories in the values of its fields 
        /// with an reaction Emoji on front</param>
        /// <param name="reaction">The users reaction that decides which category to pick form the embed</param>
        private Category PickCategory(IEmbed emb, IReaction reaction)
        {
            // Search which of the embed fields starts with the given reaction
            var categoryString = emb.Fields
                .Select(fi => fi.Name)
                .Where(field => field.StartsWith(reaction.Emote.Name));
            var enumerable = categoryString as string[] ?? categoryString.ToArray();
            return !enumerable.Any()
                // If no reaction fits use the "Any" category
                ? new Category{name = "Any", id = "" }
                // Else slice out the reaction and trailing / leading whitespaces off the fields value to get the category
                : _triviaGames.Categories.FirstOrDefault(value => 
                        value.name == enumerable.ToArray()[0].Replace(reaction.Emote.Name, ""
                    ).Trim());
        }

        /// <summary>
        /// Handles behaviour on reactions while in the startpage gamestate
        /// </summary>
        private async Task HandleStartPageInput(IUserMessage socketMsg, IReaction reaction)
        {
            var reactionName = reaction.Emote.Name;
            // If the player wants to change the category
            if (reactionName == _triviaGames.ReactOptions["1"].Name)
            { 
                socketMsg.AddReactionAsync(_triviaGames.ReactOptions["left"]);
                socketMsg.AddReactionAsync(_triviaGames.ReactOptions["right"]);
                PrepareCategoryEmb();
                _gamestate = GameStates.ChangingCategory;
                return;
            }
            // If the player wants to change the questiontype
            if (reactionName == _triviaGames.ReactOptions["2"].Name)
            {
                // Take the current type and use set it to the next one
                var index = _triviaGames.QuestionTypes.ToList().FindIndex(q => QuestionType == q.Key);
                QuestionType = _triviaGames.QuestionTypes.ToList()[(index + 1) % _triviaGames.QuestionTypes.Count].Key;
                PrepareStartMenue(socketMsg);
            }
            // If the player wants to change the difficulty
            else if (reactionName == _triviaGames.ReactOptions["3"].Name)
            {
                // Take the current difficulty and use set it to the next one
                var index = _triviaGames.Difficulties.ToList().FindIndex(q => Difficulty == q.Key);
                Difficulty = _triviaGames.Difficulties.ToList()[(index + 1) % _triviaGames.Difficulties.Count].Key;
                PrepareStartMenue(socketMsg);
            }
            // If the player wants to start the game
            if (reactionName == _triviaGames.ReactOptions["ok"].Name)
                await PreparePlayEmb(socketMsg, reaction);
        }

        /// <summary>
        /// Set the game embedbuilder up to be send out to show the paged categories
        /// </summary>
        private void PrepareCategoryEmb()
        {
            _emb.Fields.Clear();
            _emb.WithDescription(
                "Choose the category the questions should be in with the corresponding reaction.\n" +
                $"You can navigate the pages with {_triviaGames.ReactOptions["left"]} and {_triviaGames.ReactOptions["right"]} "+
                $"or {_triviaGames.ReactOptions["ok"]} to go back without changing the category.");
            var categories = _triviaGames.CategoriesPaged(_currentCategoryPage, CategoriesPerPage);
            for (var i = 1; i <= categories.Count; i++)
            {
                _emb.AddField(_triviaGames.ReactOptions[i.ToString()].Name + "  " + categories[i-1].name, Constants.InvisibleString);
            }
        }

        /// <summary>
        /// Handles behaviour on reactions while in the playing gamestate
        /// </summary>
        private async Task HandlePlayingInput(IUserMessage socketMsg, IReaction reaction)
        {
            if (reaction.Emote.Equals(_triviaGames.ReactOptions["ok"]) && _gamestate == GameStates.Playing)
            {
                PrepareStartMenue(socketMsg);
                _gamestate = GameStates.StartPage;
                return;
            }

            await PreparePlayEmb(socketMsg, reaction);
        }

        /// <summary>
        /// Set the game embedbuilder up to be send out to show a question + eventually the result of the previous one
        /// </summary>
        private async Task PreparePlayEmb(IUserMessage socketMsg, IReaction reaction)
        {
            // If the game is already in the playing state we want to show the result of the last guess
            var wrongRightMessage = 
                _gamestate == GameStates.Playing 
                ? GuessingResponse(socketMsg.Embeds.FirstOrDefault(), reaction) 
                : null;
            await NewQuestion();
            // Set the gamestate in case we are came from the main menue
            _gamestate = GameStates.Playing;
            _emb = _triviaGames.QuestionToEmbed(_currentQuestion, _emb);
            // Add empty field for cosmetics and one that tells how to get back to the main menue
            _emb.AddField(Constants.InvisibleString, $"{wrongRightMessage}{Constants.InvisibleString}");
            _emb.AddField(Constants.InvisibleString, $"{_triviaGames.ReactOptions["ok"]} to get back to the main menue");
        }

        /// <summary>
        /// Remembers the current question (for stats) and queries a new question according to the current game settings
        /// </summary>
        private async Task NewQuestion()
        {
            if (_gamestate == GameStates.Playing)
                previousQuestions.Add(_currentQuestion);
            _currentQuestion = 
               (await _triviaGames.GetQuestions(
                    categoryId: Category.id, difficulty: Difficulty, token: _token, type: QuestionType))
                .FirstOrDefault();
        }

        /// <summary>
        /// Determins if a player answered correctly and accordingly colors the embedbuilder of the game and returns a string
        /// </summary>
        private string GuessingResponse(IEmbed emb, IReaction reaction)
        {
            // Get the embed field that matches the reaction (or null)
            var answered = emb.Fields.FirstOrDefault(field => field.Name == reaction.Emote.Name);
            if (_currentQuestion.correct_answer == answered.Value)
            {
                _emb.WithColor(Color.Green);
                _currentQuestion.correct = true;
                return $":white_check_mark: You guessed correct! The answer was \"{_currentQuestion.correct_answer}\"";
            }

            _emb.WithColor(Color.Red);
            _currentQuestion.correct = false;
            return $":x: **WRONG!** The correct answer would have been \"{_currentQuestion.correct_answer}\"";
        }    
    }
}
