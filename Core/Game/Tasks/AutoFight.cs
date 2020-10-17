using autoplaysharp.Contracts;
using autoplaysharp.Contracts.Configuration;
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
        private const string DangerRoomOrCoopSkill = "BATTLE_SKILL_7";

        private readonly int _maxWaitTime;
        private bool _dangerRoomSpecialSkillAvailable;

        public AutoFight(IGame game, IUiRepository repository, ISettings settings, params Func<bool>[] conditions) : this(game, repository, settings, 30, conditions)
        {
            
        }

        public AutoFight(IGame game, IUiRepository repository, ISettings settings, int maxWaitTime, params Func<bool>[] conditions) : base(game, repository, settings)
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
            if(Game.IsVisible(UIds.BATTLE_TAP_THE_SCREEN))
            {
                Game.Click(UIds.BATTLE_TAP_THE_SCREEN);
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
                if (Game.IsVisible(UIds.BATTLE_TAP_THE_SCREEN))
                {
                    Game.Click(UIds.BATTLE_TAP_THE_SCREEN);
                }
                
                var skillId = GetBestAvailableSkill();
                if(skillId == string.Empty)
                {
                    await Task.Delay(1000);
                    Logger.LogDebug("No skill available. Waiting briefly");
                }
                else
                {
                    if(!await TryCastSkill(skillId))
                    {
                        await Task.Delay(250);
                        continue;
                    }

                    var waitTime = GetSkillCastWaitTime(skillId);
                    Logger.LogDebug($"Waiting {waitTime}ms then casting next skill");
                    await Task.Delay(waitTime).ConfigureAwait(false);
                }
            }
        }

        private static int GetSkillCastWaitTime(string skillId)
        {
            return skillId == AwakenOrT3SkillId
                   ? 7000 : 1500;
        }

        private async Task<bool> TryCastSkill(string skillId)
        {
            Logger.LogDebug($"Casting skill: {skillId}");
            Game.Click(skillId);

            await Task.Delay(500);

            int attempts = 3;
            if (skillId == DangerRoomOrCoopSkill)
            {
                if (_dangerRoomSpecialSkillAvailable)
                {
                    while (attempts > 0)
                    {
                        if (Game.IsVisible(UIds.BATTLE_DANGER_ROOM_SPECIAL_SKILL_PERCENTAGE))
                        {
                            Logger.LogInformation("Successfully casted danger room special skill");
                            return true;
                        }
                        else
                        {
                            Logger.LogInformation("Failed to cast danger room skill. Waiting briefly");
                            await Task.Delay(500);
                            Game.Click(skillId);
                        }
                        attempts--;
                    }
                    // TODO: theres an issue with the bot being stuck in a loop trying to cast this skill.
                    // issue seems to be with detecting the '%' sign. this is a workaround until its fixed.
                    Logger.LogInformation("Failed to cast danger room skill 3 times... Disabling");
                    _dangerRoomSpecialSkillAvailable = false;
                }
                return false;
            }

            int cd;
            while (!int.TryParse(Game.GetText(skillId), out cd) && attempts > 0)
            {
                Logger.LogDebug("Failed to cast. Trying again.");
                Game.Click(skillId);
                await Task.Delay(500);
                attempts--;
                if(attempts == 0)
                {
                    return false;
                }
            }

            Logger.LogDebug($"Successfully casted. Skill has {cd}s cooldown");
            return true;
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
            for(int i = 1; i < 8;i++)
            {
                var id = $"{GetSkillId(i)}_NUM";
                if(i == 7)
                {
                    if(!_dangerRoomSpecialSkillAvailable && Game.IsVisible(UIds.BATTLE_DANGER_ROOM_SPECIAL_SKILL_PERCENTAGE))
                    {
                        Logger.LogInformation("Found Danger Room special skill.");
                        _dangerRoomSpecialSkillAvailable = true;
                    }
                    if(_dangerRoomSpecialSkillAvailable && !Game.IsVisible(UIds.BATTLE_DANGER_ROOM_SPECIAL_SKILL_PERCENTAGE))
                    {
                        yield return i;
                    }

                    // TODO: handle coop skill here?
                }
                else
                {
                    if (Game.IsVisible(id))
                    {
                        if (i == 6)
                        {
                            var chargePercentageText = Game.GetText(GetSkillId(i));
                            var t3Locked = Game.IsVisible(UIds.BATTLE_SKILL_T3_LOCKED);
                            var t3Charging = Game.IsVisible(UIds.BATTLE_T3_PERCENTAGE_SIGN);
                            var couldParseChargePercentage = int.TryParse(chargePercentageText, System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.CultureInfo.InvariantCulture, out var chargePercentage);
                            if (t3Locked || t3Charging || couldParseChargePercentage || chargePercentageText.Any(char.IsDigit))
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
