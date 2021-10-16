using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using D2RAssist.Types;

namespace D2RAssist.Helpers
{
    public static class PointOfInterestHandler
    {
        private static readonly HashSet<GameObject> _interestingGameObjects = new HashSet<GameObject>
        {
            GameObject.HoradricCubeChest,
            GameObject.HoradricScrollChest,
            GameObject.StaffOfKingsChest,
            GameObject.HoradricOrifice,
            GameObject.YetAnotherTome, // Summoner in Arcane Sanctuary
            GameObject.FrozenAnya,
            GameObject.InifussTree,
            GameObject.CairnStoneAlpha,
            GameObject.WirtCorpse,
            GameObject.MaggotLairGooPile
        };

        private static readonly HashSet<GameObject> _goodChests = new HashSet<GameObject>
        {
            GameObject.GoodChest,
            GameObject.SparklyChest,
            GameObject.ArcaneLargeChestLeft,
            GameObject.ArcaneLargeChestRight,
            GameObject.ArcaneSmallChestLeft,
            GameObject.ArcaneSmallChestRight
        };

        public static List<PointOfInterest> Get(MapApi mapApi, AreaData areaData)
        {
            var pointOfInterest = new List<PointOfInterest>();

            switch (areaData.Area)
            {
                case Area.CanyonOfTheMagi:
                    // Work out which tomb is the right once. 
                    // Load the maps for all of the tombs, and check which one has the Orifice.
                    // Declare that tomb as point of interest.
                    var tombs = new[]
                    {
                        Area.TalRashasTomb1, Area.TalRashasTomb2, Area.TalRashasTomb3, Area.TalRashasTomb4,
                        Area.TalRashasTomb5, Area.TalRashasTomb6, Area.TalRashasTomb7
                    };
                    var realTomb = Area.None;
                    Parallel.ForEach(tombs, tombArea =>
                    {
                        var tombData = mapApi.GetMapData(tombArea);
                        if (tombData.Objects.ContainsKey(GameObject.HoradricOrifice))
                        {
                            realTomb = tombArea;
                        }
                    });

                    if (realTomb != Area.None && areaData.AdjacentLevels[realTomb].Exits.Any())
                    {
                        pointOfInterest.Add(new PointOfInterest
                        {
                            Label = "Tal Rashas Tomb",
                            Position = areaData.AdjacentLevels[realTomb].Exits[0],
                            DrawLabel = true,
                            DrawLine = true,
                            DrawIcon = Icons.DoorNext
                        });
                    }

                    break;
                default:
                    // By default, draw a line to the next highest neighbouring area.
                    // Also draw labels and previous doors for all other areas.
                    if (areaData.AdjacentLevels.Any())
                    {
                        var highestArea = areaData.AdjacentLevels.Keys.Max();
                        if (highestArea > areaData.Area)
                        {
                            if (areaData.AdjacentLevels[highestArea].Exits.Any())
                            {
                                pointOfInterest.Add(new PointOfInterest
                                {
                                    Label = highestArea.Name(),
                                    Position = areaData.AdjacentLevels[highestArea].Exits[0],
                                    DrawLabel = true,
                                    DrawLine = true,
                                    DrawIcon = Icons.DoorNext
                                });
                            }
                        }

                        foreach (var level in areaData.AdjacentLevels.Values)
                        {
                            // Already have something drawn for this.
                            if (level.Area == highestArea)
                            {
                                continue;
                            }

                            foreach (var positin in level.Exits)
                            {
                                pointOfInterest.Add(new PointOfInterest
                                {
                                    Label = level.Area.Name(),
                                    Position = positin,
                                    DrawLabel = true,
                                    DrawLine = false,
                                    DrawIcon = Icons.DoorPrevious
                                });
                            }
                        }
                    }

                    break;
            }

            foreach (var objAndPoints in areaData.Objects)
            {
                var obj = objAndPoints.Key;
                var points = objAndPoints.Value;

                if (!points.Any())
                {
                    continue;
                }

                // Waypoints
                if (obj.IsWaypoint())
                {
                    pointOfInterest.Add(new PointOfInterest
                    {
                        Position = points[0],
                        DrawLabel = false,
                        DrawLine = false,
                        DrawIcon = Icons.Waypoint
                    });
                }
                // Quest objects
                else if (_interestingGameObjects.Contains(obj))
                {
                    pointOfInterest.Add(new PointOfInterest
                    {
                        Position = points[0],
                        DrawLabel = false,
                        DrawLine = true
                    });
                }
                // Chests
                else if (_goodChests.Contains(obj))
                {
                    foreach (var point in points)
                    {
                        pointOfInterest.Add(new PointOfInterest
                        {
                            Position = point,
                            DrawIcon = Icons.SuperChest
                        });
                    }
                }
            }

            return pointOfInterest;
        }
    }
}