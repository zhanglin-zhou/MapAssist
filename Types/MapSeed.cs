using System.ComponentModel;
using System.Threading.Tasks;

namespace MapAssist.Types
{
    public class MapSeed
    {
        private static readonly NLog.Logger _log = NLog.LogManager.GetCurrentClassLogger();

        private BackgroundWorker BackgroundCalculator;
        private uint GameSeedXor { get; set; } = 0;

        public bool IsReady = true;

        public uint Get(UnitPlayer player)
        {
            return player.MapSeed;
        }
    }
}
