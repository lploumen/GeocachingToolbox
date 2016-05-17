using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace GeocachingToolbox.GeocachingCom.MapFetch
{
    public class MapFetcher
    {
        private readonly TileCache _tileCache = new TileCache();
        private double MilliTimeStamp()
        {
            DateTime d1 = new DateTime(1970, 1, 1);
            DateTime d2 = DateTime.UtcNow;
            TimeSpan ts = new TimeSpan(d2.Ticks - d1.Ticks);
            return ts.TotalMilliseconds;
        }

        //public Action<byte[]> DebugTile { get; set; }
        public async Task<List<Geocache>> FetchCaches(IGCConnector gcConnector,Location topLeft,Location bottomRight)
        {
            var viewport = new Viewport(topLeft,bottomRight);

            var tiles = Tile.GetTilesForViewport(viewport);
            var caches = new List<Geocache>();
            var tsk = new List<Task>();
            foreach (Tile tile in tiles)
            {
                var t = Task.Run(async () =>
                {
                   
                    if (!_tileCache.Contains(tile))
                    {
                        var parameters = new Dictionary<string, string>()
                        {
                            {"x", tile.TileX + ""},
                            {"y", tile.TileY + ""},
                            {"z", tile.Zoomlevel + ""},
                            {"ep", "1"},
                        };

                        if (tile.Zoomlevel != 14)
                        {
                            parameters.Add("_", MilliTimeStamp() + "");
                        }

                        var currentTile = tile;
                        try
                        {
                            var tilePixels = await Tile.RequestMapTile(parameters, gcConnector);

                            var c =
                                await
                                    currentTile.RequestMapInfo(GCConstants.URL_MAP_INFO, parameters,
                                        GCConstants.URL_LIVE_MAP, tilePixels,currentTile.Zoomlevel);
                            caches.AddRange(c);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.Message);
                        }
                        _tileCache.Add(currentTile);

                    }
                    else
                    {
                        Debug.WriteLine("Reusing tile");
                    }
                });
                tsk.Add(t);
            }
            await Task.WhenAll(tsk);
            return caches;
        }
    }
}
