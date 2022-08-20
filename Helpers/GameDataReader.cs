using MapAssist.Settings;
using MapAssist.Types;
using System;
using System.Linq;
using System.Text;

namespace MapAssist.Helpers
{
    public class GameDataReader
    {
        private static readonly NLog.Logger _log = NLog.LogManager.GetCurrentClassLogger();
        private volatile GameData _gameData;
        private AreaData _areaData;
        private MapApi _mapApi;

        public (GameData, AreaData, bool) Get()
        {
            var gameData = GameMemory.GetGameData();
            var changed = false;

            if (gameData != null)
            {
                if (gameData.HasGameChanged(_gameData))
                {
                    if (gameData.MapSeed == 0 && !gameData.MapSeedReady)
                    {
                        _log.Info($"Brute forcing first map seed");

                        changed = true;
                        _areaData = null;
                    }
                    else
                    {
                        _log.Info($"Game changed to {gameData.Difficulty} with {gameData.MapSeed} seed");
                    }

                    _mapApi = new MapApi(gameData);
                }

                if (gameData.HasMapChanged(_gameData) && gameData.MapSeed > 0 && gameData.Area != Area.None)
                {
                    _log.Info($"Area changed to {gameData.Area}");
                    _areaData = _mapApi.GetMapData(gameData.Area);

                    if (_areaData == null)
                    {
                        _log.Info($"Area data not loaded");
                    }
                    else
                    {
                        _areaData.CalcViewAreas();

                        foreach (var adjacentArea in _areaData.AdjacentAreas.Values)
                        {
                            adjacentArea.CalcViewAreas();
                        }
                        if (_areaData.PointsOfInterest == null)
                        {
                            _areaData.PointsOfInterest = PointOfInterestHandler.Get(_mapApi, _areaData, gameData);
                            _log.Info($"Found {_areaData.PointsOfInterest.Count} points of interest");
                        }
                    }
                    changed = true;
                }
            }

            _gameData = gameData;

            /*
            if (_gameData != null)
            {
                foreach (var gameObject in _gameData.Objects)
                {
                    if (gameObject.IsPortal)
                    {
                        var playerNameUnicode = Encoding.UTF8.GetString(gameObject.ObjectData.Owner).TrimEnd((char)0);
                        var playerName = !string.IsNullOrWhiteSpace(playerNameUnicode) ? playerNameUnicode : null;
                        var destinationArea = (Area)Enum.ToObject(typeof(Area), gameObject.ObjectData.InteractType);
                        var label = destinationArea.PortalLabel(_gameData.Difficulty, playerName);
                        _log.Info($"Found portal {label}");
                        if (_areaData != null)
                        {
                            _log.Info($"area data include point {_areaData.IncludesPoint(gameObject.Position)}");
                        }
                    }
                }
            }
            */
            ImportFromGameData();

            return (_gameData, _areaData, changed);
        }

        private void ImportFromGameData()
        {
            if (_gameData == null || _areaData == null) return;

            foreach (var gameObject in _gameData.Objects)
            {
                if (!_areaData.IncludesPoint(gameObject.Position)) continue;

                if (gameObject.IsShrine || gameObject.IsWell)
                {
                    var existingPoint = _areaData.PointsOfInterest.FirstOrDefault(x => x.Position == gameObject.Position);

                    if (existingPoint != null)
                    {
                        existingPoint.Label = Shrine.ShrineDisplayName(gameObject);
                    }
                    else
                    {
                        _areaData.PointsOfInterest.Add(new PointOfInterest()
                        {
                            Area = _areaData.Area,
                            Label = Shrine.ShrineDisplayName(gameObject),
                            Position = gameObject.Position,
                            RenderingSettings = MapAssistConfiguration.Loaded.MapConfiguration.Shrine,
                            Type = PoiType.Shrine
                        });
                    }
                } else if (gameObject.IsPortal)
                {
                    var destinationArea = (Area)Enum.ToObject(typeof(Area), gameObject.ObjectData.InteractType);
                    var playerNameUnicode = gameObject.ObjectData.Owner != null ? Encoding.UTF8.GetString(gameObject.ObjectData.Owner).TrimEnd((char)0) : null;
                    var playerName = !string.IsNullOrWhiteSpace(playerNameUnicode) ? playerNameUnicode : null;
                    var label = destinationArea.PortalLabel(_gameData.Difficulty, playerName);
                    _areaData.PointsOfInterest.Add(new PointOfInterest
                    {
                        Area = _areaData.Area,
                        NextArea = destinationArea,
                        Label = label,
                        Position = gameObject.Position,
                        RenderingSettings = MapAssistConfiguration.Loaded.MapConfiguration.GamePortal,
                        Type = PoiType.AreaPortal
                    });
                    //_log.Info($"Add portal {label} into PointsOfInterest");
                }
            }
        }
    }
}
