using autoplaysharp.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace autoplaysharp.Game.Tasks
{
    public class AutoFight : GameTask
    {
        private readonly List<Func<bool>> _conditions;
        
        private const string Tier3Skillid = "BATTLE_SKILL_T3";
        
        private readonly int _maxWaitTime;


        public AutoFight(IGame game, IUiRepository repository, params Func<bool>[] conditions) : this(game, repository, 30, conditions)
        {
            
        }

        public AutoFight(IGame game, IUiRepository repository, int maxWaitTime, params Func<bool>[] conditions) : base(game, repository)
        {
            _conditions = conditions.ToList();
            _conditions.Add(() => game.IsVisible("GENERIC_MISSION_MISSION_SUCCESS"));
            _conditions.Add(() => game.IsVisible("BATTLE_END_BATTLE_NOTICE")); // Generic end message? Used in world event... Anywhere else?? TODO: if not move to another json...
            _conditions.Add(() => game.IsVisible("WORLD_BOSS_MISSION_SUCCESS"));
            _conditions.Add(() => game.IsVisible("ALLIANCE_BATTLE_CLEAR_MESSAGE"));
            _conditions.Add(() => game.IsVisible("ALLIANCE_BATTLE_ENDED_MESSAGE"));
            _conditions.Add(() => game.IsVisible("DANGER_ROOM_HIGHEST_EXCLUSIV_SKILL_COUNT"));
            _maxWaitTime = maxWaitTime;
        }

        private bool BattleHasStarted()
        {
            if(Game.IsVisible("BATTLE_TAP_THE_SCREEN"))
            {
                Game.Click("BATTLE_TAP_THE_SCREEN");
            }
            return GetAvailableSkills().Any();
        }

        protected override async Task RunCore(CancellationToken token)
        {
            Console.WriteLine("Running AutoFight");
            Console.WriteLine("Waiting for skills to come available");
            if(!await WaitUntil(BattleHasStarted, _maxWaitTime, 0.5f))
            {
                Console.WriteLine("No skills appeared in time. Ending");
                return;
            }

            while (!token.IsCancellationRequested && !HasFightEnded())
            {
                if (Game.IsVisible("BATTLE_TAP_THE_SCREEN"))
                {
                    Game.Click("BATTLE_TAP_THE_SCREEN");
                }
                
                var skillId = GetBestAvailableSkill();
                if(skillId == string.Empty)
                {
                    await Task.Delay(1000);
                    Console.WriteLine("No skill available. Waiting briefly");
                }
                else
                {
                    await TryCastSkill(skillId);

                    // TODO: find smarter way of deciding how long to wait...
                    Console.WriteLine("Waiting 1s then casting next skill");
                    await Task.Delay(skillId == Tier3Skillid ? 5000 : 1500).ConfigureAwait(false);
                }
            }
        }


        private async Task TryCastSkill(string skillId)
        {
            Console.WriteLine($"Casting skill: {skillId}");
            Game.Click(skillId);

            await Task.Delay(500);

            int attempts = 3;
            int cd;
            while (!int.TryParse(Game.GetText(skillId), out cd) && attempts > 0)
            {
                Console.WriteLine("Failed to cast. Trying again.");
                Game.Click(skillId);
                await Task.Delay(500);
                attempts--;
            }
            Console.WriteLine($"Successfully casted. Skill has {cd}s cooldown");
        }

        private string GetSkillId(int skillNum) => $"BATTLE_SKILL_{skillNum}";

        private string GetBestAvailableSkill()
        {
            if (Game.IsVisible($"BATTLE_SKILL_T3_NUM"))
            {
                var t3SkillId = "BATTLE_SKILL_T3";
                var chargePercentageText = Game.GetText(t3SkillId);
                var t3Locked = Game.IsVisible("BATTLE_SKILL_T3_LOCKED");
                var couldParseChargePercentage = int.TryParse(chargePercentageText, System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.CultureInfo.InvariantCulture, out var chargePercentage);
                if (t3Locked || couldParseChargePercentage || chargePercentageText.Any(char.IsDigit))
                {
                    Console.WriteLine($"T3 not ready yet...");
                }
                else
                {
                    return t3SkillId;
                }
            }

            var availableSkills = GetAvailableSkills();
            foreach(var s in availableSkills.OrderByDescending(x => x))
            {
                var skillId = GetSkillId(s);
                var text = Game.GetText(skillId);
                if(int.TryParse(text, out var cooldown))
                {
                    Console.WriteLine($"Skill {skillId} on cooldown: {cooldown}s");
                }
                else
                {
                    return skillId;
                }
            }
            return string.Empty;
        }

        private IEnumerable<int> GetAvailableSkills()
        {
            for(int i = 1; i < 6;i++)
            {
                if(Game.IsVisible($"{GetSkillId(i)}_NUM"))
                {
                    yield return i;
                }
            }
        }

        private bool HasFightEnded()
        {
            if (_conditions.Any(cond => cond()))
            {
                Console.WriteLine("Detected end condition");
                return true;
            }

            return false;
        }
    }
}
