using autoplaysharp.Contracts;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace autoplaysharp.Game.Tasks
{
    /// <summary>
    /// TODO: there is a problem, where it initially think it casted the t3 awaken skill sucessfully and then waits x seconds. event though it didnt work.
    /// </summary>
    public class AutoFight : GameTask
    {
        private readonly List<Func<bool>> _conditions;
        
        private const string AwakenOrT3SkillId = "BATTLE_SKILL_6";

        private readonly int _maxWaitTime;


        public AutoFight(IGame game, IUiRepository repository, params Func<bool>[] conditions) : this(game, repository, 30, conditions)
        {
            
        }

        public AutoFight(IGame game, IUiRepository repository, int maxWaitTime, params Func<bool>[] conditions) : base(game, repository)
        {
            _conditions = conditions.ToList();
            _conditions.Add(() => game.IsVisible(UIds.GENERIC_MISSION_MISSION_SUCCESS));
            
            // TODO: these conditions should be moved to specific missions.
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
            Logger.LogInformation("Running AutoFight");
            Logger.LogDebug("Waiting for skills to come available");
            if(!await WaitUntil(BattleHasStarted, token, _maxWaitTime, 0.5f))
            {
                Logger.LogError("No skills appeared in time. Ending");
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
                    Logger.LogDebug("No skill available. Waiting briefly");
                }
                else
                {
                    await TryCastSkill(skillId);

                    // TODO: find smarter way of deciding how long to wait...
                    Logger.LogDebug("Waiting 1s then casting next skill");
                    await Task.Delay(GetSkillCastWaitTime(skillId)).ConfigureAwait(false);
                }
            }
        }

        private static int GetSkillCastWaitTime(string skillId)
        {
            return skillId == AwakenOrT3SkillId
                   ? 7000 : 1500;
        }

        private async Task TryCastSkill(string skillId)
        {
            Logger.LogDebug($"Casting skill: {skillId}");
            Game.Click(skillId);

            await Task.Delay(500);

            int attempts = 3;
            int cd;
            while (!int.TryParse(Game.GetText(skillId), out cd) && attempts > 0)
            {
                Logger.LogDebug("Failed to cast. Trying again.");
                Game.Click(skillId);
                await Task.Delay(500);
                attempts--;
            }
            Logger.LogDebug($"Successfully casted. Skill has {cd}s cooldown");
        }

        private string GetSkillId(int skillNum) => $"BATTLE_SKILL_{skillNum}";

        private string GetBestAvailableSkill()
        {
            var availableSkills = GetAvailableSkills();
            foreach(var s in availableSkills.OrderByDescending(x => x))
            {
                var skillId = GetSkillId(s);
                var text = Game.GetText(skillId);
                if(int.TryParse(text, out var cooldown))
                {
                    Logger.LogDebug($"Skill {skillId} on cooldown: {cooldown}s");
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
            for(int i = 1; i < 7;i++)
            {
                var id = $"{GetSkillId(i)}_NUM";
                if (Game.IsVisible(id))
                {
                    if (i == 6)
                    {
                        var chargePercentageText = Game.GetText(GetSkillId(i));
                        var t3Locked = Game.IsVisible("BATTLE_SKILL_T3_LOCKED");
                        var couldParseChargePercentage = int.TryParse(chargePercentageText, System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.CultureInfo.InvariantCulture, out var chargePercentage);
                        if (t3Locked || couldParseChargePercentage || chargePercentageText.Any(char.IsDigit))
                        {
                            Logger.LogDebug($"T3 not ready yet...");
                        }
                        else
                        {
                            yield return i;
                        }
                    }
                    else
                    {
                        yield return i;
                    }
                }
            }
        }

        private bool HasFightEnded()
        {
            if (_conditions.Any(cond => cond()))
            {
                Logger.LogInformation("Detected end condition");
                return true;
            }

            return false;
        }
    }
}
