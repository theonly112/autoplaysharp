using autoplaysharp.Contracts;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace autoplaysharp.Game.Tasks
{
    internal class AutoFight : GameTask
    {
        public AutoFight(IGame game) : base(game)
        {
        }

        protected override async Task RunCore(CancellationToken token)
        {
            Console.WriteLine("Running AutoFight");
            while (!token.IsCancellationRequested && !HasFightEnded())
            {
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
                    await Task.Delay(1000).ConfigureAwait(false);
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
            //var t3SkillId = "BATTLE_SKILL_T3";
            
            //var chargePercentageText = Game.GetText(t3SkillId);
            //if (Game.IsVisible("BATTLE_SKILL_T3_LOCKED") || int.TryParse(chargePercentageText, out var chargePercentage))
            //{
            //    Console.WriteLine($"T3 not ready yet...");
            //}
            //else
            //{
            //    return t3SkillId;
            //}

            for (int i = 5; i > 0;i--)
            {
                var skillId = GetSkillId(i);
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


        private bool HasFightEnded()
        {
            var missionSuccess = Game.IsVisible("WORLD_BOSS_MISSION_SUCCESS");
            if(missionSuccess)
            {
                Console.WriteLine("Detected World Boss 'Mission Success' screen. Ending AutoFight...");
                return true;
            }

            var allianceBattleEnded = Game.IsVisible("ALLIANCE_BATTLE_CLEAR_MESSAGE") || Game.IsVisible("ALLIANCE_BATTLE_ENDED_MESSAGE");
            if(allianceBattleEnded)
            {
                Console.WriteLine("Detected ALLIANCE_BATTLE_CLEAR_MESSAGE. Ending AutoFight...");
                return true;
            }

            // TODO: add other end indicators.

            return false;
        }
    }
}
