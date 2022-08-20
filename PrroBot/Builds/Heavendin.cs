using GameOverlay.Drawing;
using MapAssist.Types;
using PrroBot.GameInteraction;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace PrroBot.Builds
{
    internal class Heavendin : Build
    {
        public const Keys HolyBot = Keys.F1;
        public const Keys FistOfHeaven = Keys.F2;
        public const Keys Redemption = Keys.F3;

        private const int MaxAttackerCount = 40;

        public override bool DoPrebuffs()
        {
            _log.Info("Prebuffs");
            var canUseBattleCommand = Movement.CanUseSkill(Skill.BattleCommand);
            var switchedWeapon = false;
            if (!canUseBattleCommand)
            {
                Input.KeyPress(Keys.W);
                switchedWeapon = true;
                Thread.Sleep(500);
                canUseBattleCommand = Movement.CanUseSkill(Skill.BattleCommand);
            }

            if (canUseBattleCommand)
            {
                Input.KeyPress(BotConfig.SkillConfig.BattleCommand);
                Thread.Sleep(500);
            }

            if (switchedWeapon)
            {
                Input.KeyPress(Keys.W);
                Thread.Sleep(500);
            }

            Input.KeyPress(BotConfig.SkillConfig.HolyShield);
            Thread.Sleep(500);
            Input.KeyPress(Redemption);
            Thread.Sleep(500);

            return true;
        }

        public override void KillSingleTarget(UnitMonster monster)
        {
            //TODO limit total runtime to x minutes via a proper timer/timestamp instead of simple counters...
            //TODO try different spots around the monster if monster not hit for x seconds. if no walkable spot around monster, throw exception

            _log.Info("Killing single target " + monster.Npc);

            /*
            if (Pathing.CalculateDistance(gameData.PlayerPosition, monster.Position) > 5)
            {
                Movement.MoveToPoint(new Point(monster.Position.X + 2, monster.Position.Y + 2));
                Movement.WaitForNeutral();
                gameData = Core.GetGameData();
                monster = gameData.Monsters.FirstOrDefault(x => x.UnitId == monster.UnitId && !x.IsCorpse);
                if (monster == null) return;
            }

            Movement.WaitForNeutral();
            

            Point screenCoord;
            (_, screenCoord) = Common.WorldToScreen(gameData, areaData, new Point(monster.Position.X + 2, monster.Position.Y + 2), gameData.PlayerPosition);
            Input.SetCursorPos(screenCoord);
            Thread.Sleep(50);
            Input.KeyPress(BotConfig.SkillConfig.Teleport);
            Movement.WaitForNeutral();
            gameData = Core.GetGameData();
            */
            var attackCount = 0;
            var notHitCounter = 0;
            var gameData = Core.GetGameData();
            var player = gameData.PlayerUnit;
            while (monster != null && monster.HealthPercentage > 0 && attackCount < MaxAttackerCount && notHitCounter < 15 && player.LifePercentage > 0)
            {
                attackCount++;
                var areaData = Core.GetAreaData();

                var attackSkill = FistOfHeaven;

                var monstersInRange = Common.GetMonstersInRange(monster.Position, 10);
                if (monstersInRange.Count < 3)
                {
                    attackSkill = HolyBot;
                }
                /*
                // reposition if we are too far away from the monster of if we have been in a suboptimal position for 2.5 seconds
                if (Pathing.CalculateDistance(gameData.PlayerPosition, monster.Position) > 4
                       || (counter % 5 == 0 && gameData.PlayerPosition != new Point(monster.Position.X + 2, monster.Position.Y + 2)))
                {
                    Input.KeyUp(BotConfig.SkillConfig.BlessedHammer);
                    Movement.WaitForNeutral();
                    (_, screenCoord) = Common.WorldToScreen(gameData, areaData, new Point(monster.Position.X + 2, monster.Position.Y + 2), gameData.PlayerPosition);
                    Input.SetCursorPos(screenCoord);
                    Thread.Sleep(50);
                    Input.KeyPress(BotConfig.SkillConfig.Teleport);
                    Movement.WaitForNeutral();
                    Input.KeyDown(BotConfig.SkillConfig.BlessedHammer);
                    Thread.Sleep(50);
                }
                */
                Point screenCoord;
                (_, screenCoord) = Common.WorldToScreen(gameData, areaData, new Point(monster.Position.X, monster.Position.Y), gameData.PlayerPosition);
                Input.SetCursorPos(screenCoord);
                Thread.Sleep(50);
                Input.KeyDown(attackSkill);
                Thread.Sleep(50);
                Input.KeyUp(attackSkill);
                Thread.Sleep(500);
                
                var oldHealth = monster.HealthPercentage;
                gameData = Core.GetGameData();
                monster = gameData.Monsters.FirstOrDefault(x => x.UnitId == monster.UnitId);
                player = gameData.PlayerUnit;
                if (monster != null && monster.HealthPercentage >= oldHealth)
                {
                    notHitCounter++;
                    if (notHitCounter % 5 == 0)
                        _log.Info("Did not hit monster (" + notHitCounter + ")");
                }
            }
            //Input.KeyUp(BotConfig.SkillConfig.BlessedHammer);
            //Thread.Sleep(50);
        }
    }
}
