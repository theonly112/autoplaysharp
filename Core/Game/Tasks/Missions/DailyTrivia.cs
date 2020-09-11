using autoplaysharp.Contracts;
using autoplaysharp.Game.Tasks;
using F23.StringSimilarity;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace autoplaysharp.Core.Game.Tasks.Missions
{
    public class DailyTrivia : GameTask
    {
        private static string QA = "\r\n{\r\n  \"What is the name of the metal in Wolverine's skeleton?\": \"Adamantium\",\r\n  \"Where was Loki born?\": \"Jotunheim\",\r\n  \"Which Canadian character with superhuman abilities is called the \\\"Merc with a Mouth\\\"?\": \"Deadpool\",\r\n  \"What is the name of the material used to Recruit or Rank Up characters?\": \"Biometrics\",\r\n  \"What is the name of the function that uses a character\u2019s Biometrics to increase a character\u2019s rank?\": \"Rank Up\",\r\n  \"What Character Type is typically strong against Speed Type Characters?\": \"Combat\",\r\n  \"What materials are needed when increasing Mastery of a Character?\": \"Norn Stone\",\r\n  \"What is the name of the Effect gained when a team is assembled with specific characters?\": \"Team Bonus\",\r\n  \"What is the function that allows Tier-1 characters to become Tier-2?\": \"Advancement\",\r\n  \"What is the name of the basic material needed for Gear Upgrades?\": \"Gear Up Kit\",\r\n  \"What is the name of the advanced material needed to upgrade Gear that is at least +10?\": \"Dimension Debris\",\r\n  \"What is the name of an item that can be equipped up to 8 times to increase various stats and receive special set effects?\": \"ISO-8\",\r\n  \"What is the name of the function that imbues a special effect when ISO-8s are equipped onto characters in a specific formation?\": \"Iso-8 Set Bonus\",\r\n  \"What is the name of the item that increases all characters\u2019 stats and is usually obtainable from Dimension Rift?\": \"Comic Cards\",\r\n  \"What is the name of the gear often obtained from Special Missions that gives a special stat to each character when equipped?\": \"Custom Gear\",\r\n  \"What is the name of the material that can further enhance character stats when equipped to Grade 20 Gear?\": \"Enchanted Uru\",\r\n  \"What is the function that uses 2 Uru as material to create a different type of Uru?\": \"Combine Uru\",\r\n  \"What is the function that can further enhance the stats of the equipped Uru?\": \"Uru Amplification\",\r\n  \"What is the name of the material that can raise X-Men mastery?\": \"M'kraan Shard\",\r\n  \"What is the name of the material used to Rank Up X-Men?\": \"X-Gene\",\r\n  \"What is the name of the material used to Advance X-Men to Tier-2?\": \"Phoenix Feather\",\r\n  \"What is the name of the Challenge that gives rewards by solving special challenges daily?\": \"Challenges\",\r\n  \"What is the name of the content that has Anti-Matter Collection, the Item Shop, and the ability to use Deploy?\": \"S.H.I.E.L.D. Lab\",\r\n  \"What is the name of the ability that cancels an opponent\u2019s skill by forcing them to make a basic attack?\": \"Guard Break\",\r\n  \"What is the name of the stat that raises Dodge Rate by a guaranteed fixed amount, regardless of the difference in opponent level?\": \"Guaranteed Dodge Rate Increase\",\r\n  \"What is the name of the stat that raises Critical Rate by a guaranteed fixed amount, regardless of the opponent\u2019s level?\": \"Guaranteed Critical Rate Increase\",\r\n  \"What is the term for the amount of damage received, regardless of a character\u2019s defense and Type Resistance?\": \"True Damage\",\r\n  \"What is the name of the stat that gives a passive-like effect to all characters just by being on the team?\": \"Team Passive\",\r\n  \"What is the name of the ability that grants immunity to all damage (basic attacks, Guard Break, Damage, Debuffs)?\": \"Invincible\",\r\n  \"From the available character skill effects, what is the ability that causes the character to receive damage but stops effects like Guard Break?\": \"Super Armor\",\r\n  \"From the available character skill effects, what is the ability that blocks damage but causes the character to receive effects like Guard Break and debuffs?\": \"Shield\",\r\n  \"From the available character skill effects, what is the ability that blocks damage and debuffs but causes the character to receive effects like Guard Break?\": \"Immune To All Damage\",\r\n  \"What is the ability that puts enemies in fear for a duration of time, rendering them unable to attack or use skills?\": \"Fear\",\r\n  \"What is the ability that groups enemies together and prevents them from moving for a duration of time, rendering them unable to use basic attacks or use skills?\": \"Bind\",\r\n  \"What is the ability that locks opponents in time for a set duration, preventing them from moving, attacking, or dodging?\": \"Time Freezing\",\r\n  \"What is the name of the ability that charms opponents for a set duration, rendering them unable to move, attack, even preventing automatically activated attacks from activating?\": \"Charm\",\r\n  \"Where can you obtain items like Dimension Debris, Comic Cards, and Uniform Kits?\": \"Dimension Rift\",\r\n  \"Where can you obtain EXP Chips and Biometrics and also claim additional rewards by clearing a Hidden Route in certain stages?\": \"Special Mission\",\r\n  \"Where can you obtain Chaos Tokens and various rewards by battling a powerful boss in varying difficulties of Easy, Medium, and Hard?\": \"Villain Siege\",\r\n  \"Where can you battle other users using a set team of 3 and get Honor Tokens as rewards?\": \"Timeline Battle\",\r\n  \"What is a weapon NOT associated with Thor?\": \"Axe of Angarruumus\",\r\n  \"What is the name of Odin's horse?\": \"Sleipnir\",\r\n  \"What is the name of the chapter-based quests that can be cleared in order for rewards?\": \"Heroic Quest\",\r\n  \"What material is Captain America's shield made of?\": \"Vibranium\",\r\n  \"What is the name of the character who is called the Sorcerer Supreme?\": \"Doctor Strange\",\r\n  \"What is the name of the tunnel that connects Yggdrasil's nine realms?\": \"Bifrost\",\r\n  \"What content opens during specific hours of the day and allows Agents to battle with the same Character conditions?\": \"World Event\",\r\n  \"Where was Loki Born?\": \"Jotunheim\",\r\n  \"What is Iron Man's Real name?\": \"Tony Stark\",\r\n  \"Which of the following characters is not a Guardian of the Galaxy?\": \"Quicksilver\",\r\n  \"What is the name of the story-based quests that explore a certain character's journey?\": \"Epic Quest\",\r\n  \"What is the name of the skill that reduces all enemy attack?\": \"Fracture\",\r\n  \"What is the substance that caused Dr. Bruce Banner to turn into Hulk?\": \"Gamma Radiation\",\r\n  \"Which Character is NOT a member of the Black Order?\": \"Hypergiant\",\r\n  \"Which of these isn't a name for Hawkeye?\": \"Magic Eye\",\r\n  \"Which of the following nicknames are not Hawkeye?\": \"Magic Eye\",\r\n  \"What is the name of the character who is a member of the Air Force and one of Tony Stark's good Friends?\": \"James Rupert Rhodes\",\r\n  \"What Character has never actually served as Captain America?\": \"T'Challa\",\r\n  \"What materials are NOT required to upgrade a character to Tier-3?\": \"Type Enhancer\",\r\n  \"Which of following is not an Infinity Stone?\": \"Eternal Stone\",\r\n  \"Which is not one of Ant-man's abilities?\": \"Thermokinesis\",\r\n  \"What is not one of Spider-man's abilities?\": \"Communicating with Spiders\",\r\n  \"What is the name of the skill that reduces all defence and removes all buffs from the enemy?\": \"Incapacitation\",\r\n  \"What is the name of the skill effect that removes targeting and prevents a character from taking damage?\": \"Ignore Targeting\",\r\n  \"What institution trained Black Widow?\": \"Red Room\",\r\n  \"What is the name of the real-time co-op content that pits players against a powerful boss?\": \"Giant Boss Raid\",\r\n  \"What is the name of the character wielding superhuman strength who also happens to be the king of Wakanda?\": \"Black Panther\",\r\n  \"What is the name of Wolverine's quick regenerative ability?\": \"Healing Factor\",\r\n  \"What is the name of the skill effect that decreases Physical Damage received?\": \"Elasticity\",\r\n  \"Which of the following characters is not a Guardian of the Black Order?\": \"Hypergiant\",\r\n  \"What materials are not required to upgrade a character to Tier-3?\": \"Norn Stone of Chaos\",\r\n  \"What is the name of the item that can be equipped (up to a max of 5) on character gear, and can be strengthened using Ampli\uFB01cation?\": \"Enchanted Uru\",\r\n  \"What is the name of the shop where you can acquire rewards using Rift Tokens in Dimension Mission?\": \"Support Shop\",\r\n  \"Which character type is strong against Blast type?\": \"Speed\",\r\n  \"Which character type is strong against Combat type?\": \"Blast\",\r\n  \"What is the name of the mode for which you can earn additional rewards using hidden tickets?\": \"Dimension Mission\",\r\n  \"Which mode grows your heroes while exploring their journey and backstory?\": \"Epic Quest\",\r\n  \"What is the name of the special item that can be equipped to Heroes to gain stats and special abilities?\": \"Custom Gear\",\r\n  \"Which mode is open every day and allows Agents to battle with the same character conditions?\": \"World Event\",\r\n  \"What is the name of items made with Marvel comic covers that can be equipped with up to 5 items per account to increase the stats of all heroes?\": \"Comic Cards\"\r\n}";
        private List<(string Question, string Answer)> _questionsAndAnswers;

        public DailyTrivia(IGame game, IUiRepository repository) : base(game, repository)
        {
            _questionsAndAnswers = JsonConvert.DeserializeObject<Dictionary<string, string>>(QA).Select(x => (x.Key, x.Value)).ToList();
        }

        protected override async Task RunCore(CancellationToken token)
        {
            if(!await GoToMainScreen())
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

            await Task.Delay(2000);

            Game.Click(UIds.CHALLENGES_DAILY_TRIVIA_TAB);

            if(!await WaitUntilVisible(UIds.CHALLENGES_DAILY_START_BUTTON, 3))
            {
                var questionStatus = Game.GetText(UIds.CHALLENGES_DAILY_TRIVIA_QUESTION_STATUS);
                if(questionStatus.Contains("/5"))
                {
                    Logger.LogInformation("Daily Trivia already started");
                }
                else
                {
                    Logger.LogError("Start button did not appear");
                    return;
                }
            }
            else
            {
                Game.Click(UIds.CHALLENGES_DAILY_START_BUTTON);
            }

            var nl = new NormalizedLevenshtein();

            var status = Game.GetText(UIds.CHALLENGES_DAILY_TRIVIA_QUESTION_STATUS);
            while(status.Contains("/5"))
            {
                var question = Game.GetText(UIds.CHALLENGES_DAILY_TRIVIA_QUESTION);
                string answer = null;
                foreach (var item in _questionsAndAnswers)
                {
                    var similarity = nl.Similarity(question, item.Question);
                    if (similarity >= 0.9)
                    {
                        answer = item.Answer;
                    }
                }

                if(answer == null)
                {
                    Logger.LogError("Could not find answer");
                }

                for (int i = 0; i < 4; i++)
                {
                    var answerId = Repository[UIds.CHALLENGES_DAILY_TRIVIA_ANSWER_DYN, 0, i];
                    if (nl.Distance(Game.GetText(answerId), answer) > 0.9) // 90% should be fine.
                    {
                        Game.Click(answerId);
                        if(!await WaitUntilVisible(UIds.CHALLENGES_DAILY_TRIVIA_CLOSE_BUTTON))
                        {
                            Logger.LogError($"Close button did not appear. Wrong answer? Q: {question} A: {answer}");
                            return;
                        }
                        await Task.Delay(500);
                        Game.Click(UIds.CHALLENGES_DAILY_TRIVIA_CLOSE_BUTTON);

                        await Task.Delay(2000);
                    }
                }

                status = Game.GetText(UIds.CHALLENGES_DAILY_TRIVIA_QUESTION_STATUS);
            }
        }


    }
}
