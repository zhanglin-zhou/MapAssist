using System.ComponentModel;
using System.Threading.Tasks;

namespace MapAssist.Types
{
    public class MapSeed
    {
        private static readonly NLog.Logger _log = NLog.LogManager.GetCurrentClassLogger();

        public bool IsReady = true;

        public uint Get(UnitPlayer player)
        {
            return player.MapSeed;
        }
    }
}
