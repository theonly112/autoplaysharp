using autoplaysharp.Contracts;
using autoplaysharp.Contracts.Configuration;
using F23.StringSimilarity;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using autoplaysharp.Contracts.Errors;

namespace autoplaysharp.Core.Game.Tasks.Missions
{
    public class DailyTrivia : GameTask
    {
        private record QuestionAnswerPair(string Question, string Answer);
        private readonly List<QuestionAnswerPair> _questionsAndAnswers;

        public DailyTrivia(IGame game, IUiRepository repository, ISettings settings) : base(game, repository, settings)
        {
            var resourceName = GetType().Namespace + ".DailyTriviaAnswers.json";
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
            using var reader = new StreamReader(stream);
            var json = reader.ReadToEnd();
            _questionsAndAnswers = JsonConvert.DeserializeObject<List<QuestionAnswerPair>>(json);
        }

        protected override async Task RunCore(CancellationToken token)
        {
            if(!await GoToMainScreen(token))
            {
                Logger.LogError("Could not go to main menu");
                return;
            }

            if(!await OpenMenu().ConfigureAwait(false))
            {
                Logger.LogError("Could not open main menu");
                return;
            }

            Game.Click(UIds.MAIN_MENU_CHALLENGES_BUTTON);

            await Task.Delay(1000, token);

            if (!await ClickWhenVisible(UIds.CHALLENGES_DAILY_TRIVIA_TAB))
            {
                Game.OnError(new ElementNotFoundError(Repository[UIds.CHALLENGES_DAILY_TRIVIA_TAB]));
            }

            await Task.Delay(1000, token);

            if(!Game.IsVisible(UIds.CHALLENGES_DAILY_START_BUTTON) && !Game.IsVisible(UIds.CHALLENGES_DAILY_TRIVIA_BASIC_REWARD))
            {
                Logger.LogInformation("Daily trivia already completed");
                return;
            }

            if(Game.IsVisible(UIds.CHALLENGES_DAILY_START_BUTTON))
            {
                Game.Click(UIds.CHALLENGES_DAILY_START_BUTTON);
            }

            await Task.Delay(2000, token);

            var questionStatus = Game.GetText(UIds.CHALLENGES_DAILY_TRIVIA_QUESTION_STATUS);
            if (questionStatus.Contains("/5"))
            {
                Logger.LogInformation("Daily Trivia already started");
            }
            else
            {
                Logger.LogError("Start button did not appear");
                return;
            }

            var nl = new NormalizedLevenshtein();

            while(Game.IsVisible(UIds.CHALLENGES_DAILY_TRIVIA_BASIC_REWARD) && !token.IsCancellationRequested)
            {
                var question = Game.GetText(UIds.CHALLENGES_DAILY_TRIVIA_QUESTION);
                Logger.LogDebug($"Question is: {question}");
                string answer = null;
                double highestSimilarityQuestion = 0;
                foreach (var item in _questionsAndAnswers)
                {
                    var similarity = nl.Similarity(question, item.Question);
                    if (similarity > highestSimilarityQuestion)
                    {
                        highestSimilarityQuestion = similarity;
                        Logger.LogDebug($"Found question that matches better: {item.Question}");
                        answer = item.Answer;
                    }
                }

                Logger.LogDebug($"Expected answer is: {answer}");

                double highestSimilarityAnswer = 0;
                UIElement bestAnswerElement = null;
                for (int i = 0; i < 4; i++)
                {
                    var answerId = Repository[UIds.CHALLENGES_DAILY_TRIVIA_ANSWER_DYN, 0, i];
                    var potentialAnswer = Game.GetText(answerId);
                    var similarity = nl.Similarity(potentialAnswer, answer);
                    Logger.LogDebug($"Found potential answer '{potentialAnswer}' that has a similarity of: {similarity * 100f}%");
                    if(similarity > highestSimilarityAnswer)
                    {
                        highestSimilarityAnswer = similarity;
                        bestAnswerElement = answerId;
                    }
                }

                if(bestAnswerElement == null)
                {
                    Logger.LogError("Could not find answer for question.");
                    return;
                }

                Game.Click(bestAnswerElement);
                if (!await WaitUntilVisible(UIds.CHALLENGES_DAILY_TRIVIA_CLOSE_BUTTON))
                {
                    Logger.LogError($"Close button did not appear. Wrong answer? Q: {question} A: {answer}");
                    return;
                }
                await Task.Delay(500, token);
                Game.Click(UIds.CHALLENGES_DAILY_TRIVIA_CLOSE_BUTTON);

                await Task.Delay(2000, token);
            }

            Logger.LogInformation("Daily trivia completed");
        }


    }
}
