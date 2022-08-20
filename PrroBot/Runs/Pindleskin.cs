using MapAssist.Types;
using PrroBot.Builds;
using PrroBot.GameInteraction;
using System.Threading;

namespace PrroBot.Runs
{
    internal class Pindleskin : Run
    {
        public override void Execute(Build build)
        {
            Town.DoTownChores();

            //Movement.TakeWaypoint(4, 0);
            Movement.MoveToNpc(Npc.Drehya);

            Movement.MoveToPortal(MapAssist.Types.Area.NihlathaksTemple);

            build.DoPrebuffs();

            Movement.MoveToPoint(10058, 13234);

            build.KillSingleTarget("Pindleskin");

            Thread.Sleep(1000);

            //build.ClearArea();

            build.LootItemsOnGround();

            //Movement.ToTownViaPortal(build);

            Thread.Sleep(1000);
        }

        public override string GetName()
        {
            return "Pindleskin";
        }
    }
}
