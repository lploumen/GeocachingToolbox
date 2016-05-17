using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ImageTools;
using ImageTools.IO.Png;
using Newtonsoft.Json;

namespace GeocachingToolbox.GeocachingCom.MapFetch
{
    public class Tile
    {
        private const int ZoomlevelMax = 18;
        private const int ZoomlevelMin = 0;

        public const int TILE_SIZE = 256;
        private static int TileRequestsCounter = 1;
        public const int MAX_CACHES_ON_TILE = 200;

        private static readonly int[] NumberOfTiles = new int[ZoomlevelMax - ZoomlevelMin + 1];
        private static readonly int[] NumberOfPixels = new int[ZoomlevelMax - ZoomlevelMin + 1];

        private readonly TileCache _tileCache = new TileCache();
        public int TileX { get; set; }
        public int TileY { get; set; }
        public int Zoomlevel { get; set; }

        public Viewport Viewport { get; set; }

        static Tile()
        {
            for (var z = ZoomlevelMin; z <= ZoomlevelMax; z++)
            {
                NumberOfTiles[z] = 1 << z;
                NumberOfPixels[z] = TILE_SIZE * 1 << z;
            }
        }

        public Tile(Location origin, int zoomlevel)
        {
            Zoomlevel = Math.Max(Math.Min(zoomlevel, ZoomlevelMax), ZoomlevelMin);

            TileX = CalcX(origin);
            TileY = CalcY(origin);

            Viewport = new Viewport(GetCoord(new UTFGridPosition(0, 0)), GetCoord(new UTFGridPosition(63, 63)));
        }

        /**
         * Calculate latitude/longitude for a given x/y position in this tile.
         * 
         * @see <a
         *      href="http://developers.cloudmade.com/projects/tiles/examples/convert-coordinates-to-tile-numbers">Cloudmade</a>
         */
        public Location GetCoord(UTFGridPosition pos)
        {

            double pixX = TileX * TILE_SIZE + pos.x * 4;
            double pixY = TileY * TILE_SIZE + pos.y * 4;

            decimal lonDeg = (decimal) (((360.0 * pixX) / NumberOfPixels[Zoomlevel]) - 180.0);
            double latRad = Math.Atan(Math.Sinh(Math.PI * (1 - 2 * pixY / NumberOfPixels[Zoomlevel])));
            return new Location(ConvertToDegrees(latRad), lonDeg);
        }

        /**
         * Calculate the tile for a Geopoint based on the Spherical Mercator.
         *
         * @see <a
         *      href="http://developers.cloudmade.com/projects/tiles/examples/convert-coordinates-to-tile-numbers">Cloudmade</a>
         */
        private int CalcX(Location origin)
        {
            return (int)((origin.Longitude + (decimal) 180.0) / (decimal) 360.0 * NumberOfTiles[Zoomlevel]);
        }

        /**
         * Calculate the tile for a Geopoint based on the Spherical Mercator.
         *
         */
        private int CalcY(Location origin)
        {
            // double latRad = Math.toRadians(origin.getLatitude());
            // return (int) ((1 - (Math.log(Math.tan(latRad) + (1 / Math.cos(latRad))) / Math.PI)) / 2 * numberOfTiles);

            // Optimization from Bing
            var sinLatRad = Math.Sin(ConvertToRadians((double) origin.Latitude));
            return (int)((0.5 - Math.Log((1 + sinLatRad) / (1 - sinLatRad)) / (4 * Math.PI)) * NumberOfTiles[Zoomlevel]);
        }

        private double ConvertToRadians(double angle)
        {
            return (Math.PI / 180) * angle;
        }

        public static decimal ConvertToDegrees(double angrad)
        {
            return (decimal) (angrad * 180.0 / Math.PI);
        }


        /**
         * Calculates the inverted hyperbolic sine
         * (after Bronstein, Semendjajew: Taschenbuch der Mathematik
         *
         * @param x
         * @return
         */
        private static double asinh(double x)
        {
            return Math.Log(x + Math.Sqrt(x * x + 1.0));
        }

        private static double tanGrad(double angleGrad)
        {
            return Math.Tan(angleGrad / 180.0 * Math.PI);
        }

        /**
         * Calculates the maximum possible zoom level where the supplied points
         * are covered by adjacent tiles on the east/west axis.
         * The order of the points (left/right) is irrelevant.
         *
         * @param left
         *            First point
         * @param right
         *            Second point
         * @return
         */
        public static int CalcZoomLon(Location left, Location right)
        {

            int zoom = (int)Math.Floor(
                Math.Log(360.0 / (double) Math.Abs(left.Longitude - right.Longitude))
                / Math.Log(2)
                );

            Tile tileLeft = new Tile(left, zoom);
            Tile tileRight = new Tile(right, zoom);

            if (tileLeft.TileX == tileRight.TileX)
            {
                zoom += 1;
            }

            return Math.Min(zoom, ZoomlevelMax);
        }

        /**
         * Calculates the maximum possible zoom level where the supplied points
         * are covered by adjacent tiles on the north/south axis.
         * The order of the points (bottom/top) is irrelevant.
         *
         * @param bottom
         *            First point
         * @param top
         *            Second point
         * @return
         */
        public static int CalcZoomLat(Location bottom, Location top)
        {

            int zoom = (int)Math.Ceiling(
                Math.Log(2.0 * Math.PI /
                         Math.Abs(
                             asinh(tanGrad((double) bottom.Latitude))
                             - asinh(tanGrad((double) top.Latitude))
                             )
                    ) / Math.Log(2)
                );

            Tile tileBottom = new Tile(bottom, zoom);
            Tile tileTop = new Tile(top, zoom);

            if (Math.Abs(tileBottom.TileY - tileTop.TileY) > 1)
            {
                zoom -= 1;
            }

            return Math.Min(zoom, ZoomlevelMax);
        }

        public static HashSet<Tile> GetTilesForViewport(Location bottomLeft, Location topRight)
        {
            var tiles = new HashSet<Tile>();
            int zoom = Math.Min(Tile.CalcZoomLon(bottomLeft, topRight), Tile.CalcZoomLat(bottomLeft, topRight));
            tiles.Add(new Tile(bottomLeft, zoom));
            tiles.Add(new Tile(new Location(bottomLeft.Latitude, topRight.Longitude), zoom));
            tiles.Add(new Tile(new Location(topRight.Latitude, bottomLeft.Longitude), zoom));
            tiles.Add(new Tile(topRight, zoom));
            return tiles;
        }

        public static HashSet<Tile> GetTilesForViewport(Viewport viewport)
        {
            return GetTilesForViewport(viewport.bottomLeft, viewport.topRight);
        }

        public bool ContainsPoint(Location point)
        {
            return Viewport.Contains(point);
        }

        public override String ToString()
        {
            return String.Format("({0}/{1}), zoom={2}", TileX, TileY, Zoomlevel);
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        private static string FormUrl(string baseUrl, Dictionary<string, string> parameters)
        {
            var urlString = baseUrl;
            var firstParameter = true;
            foreach (var k in parameters.Keys)
            {
                if (firstParameter)
                {
                    urlString += "?" + k + "=" + parameters[k];
                    firstParameter = false;
                }
                else
                {
                    urlString += "&" + k + "=" + parameters[k];
                }
            }
            return urlString;
        }



        public static async Task<int[]> RequestMapTile(/*MapToken token*/Dictionary<string, string> parameters, IGCConnector gcConnector)
        {
            //m_Token = token;
            //var parameters = new Dictionary<string, string>
            //{
            //        {"x", TileX + ""},
            //        {"y", TileY + ""},
            //        {"z", Zoomlevel + ""},
            //        {"ep", "1"},
            //};

            //if (token != null)
            //{
            //    parameters.Add("k", token.UserSession);
            //    parameters.Add("st", token.SessionToken);
            //}


            //if (Zoomlevel != 14)
            //    parameters.Add("_", Environment.TickCount.ToString());

            int tileServerNb = ++TileRequestsCounter;
            tileServerNb = tileServerNb % 4 + 1;

            var urlString = FormUrl("http://tiles01.geocaching.com/map/map.png", parameters);
            Debug.WriteLine("Tile download url :" + urlString);
            //var urlString = FormUrl("http://tiles0" + tileServerNb + ".geocaching.com/map/map.png", parameters);

            HttpContent content = await gcConnector.GetContent(urlString,null);//, HttpMethod.Get);
            byte[] tileBytes = await content.ReadAsByteArrayAsync();

            ExtendedImage img = new ExtendedImage();
            PngDecoder pngDecoder = new PngDecoder();
            using (MemoryStream ms = new MemoryStream(tileBytes))
            {
                pngDecoder.Decode(img, ms);
            }
            if (img.PixelWidth == 1)
                throw new Exception("RequestMapTile - Invalid tile image");
            int[] imgPixelsInt = new int[256 * 256];
            for (int i = 0; i < 256 * 256; i++)
            {
                int R = img.Pixels[i * 4];
                int G = img.Pixels[i * 4 + 1];
                int B = img.Pixels[i * 4 + 2];
                imgPixelsInt[i] = (R << 16) + (G << 8) + B;
            }

            return imgPixelsInt;
        }

        /** Request .png image for a tile. */
        //public static void RequestMapTile(Action<WriteableBitmap> processImage, Dictionary<string, string> parameters)
        //{
        //    var urlString = FormUrl(GCConstants.URL_MAP_TILE, parameters);

        //    new PhotoDownloader().DownloadPng(processImage, urlString);
        //}
        private int m_UrlMapInfoServerNumber = 1;
        public async Task<List<Geocache>> RequestMapInfo(string url, Dictionary<string, string> parameters, string referer, int[] tilePixels, int zoomLevel)
        {
            //var urlString = FormUrl(url, parameters);
            WebBrowserSimulator webBrowserSimulator = new WebBrowserSimulator();
            //string data = await webBrowserSimulator.GetRequestAsString(urlString);

            String urlString = string.Format("http://tiles01.geocaching.com/map/map.info", m_UrlMapInfoServerNumber);
            var urlString2 = FormUrl(urlString, parameters);
            Debug.WriteLine("Tile info url :" + urlString2);
            m_UrlMapInfoServerNumber = (m_UrlMapInfoServerNumber + 1) % 4 + 1;

            String data = await webBrowserSimulator.GetRequestAsString(urlString2);

            return ParseMapInfos(data, tilePixels, new CancellationToken(), zoomLevel);
        }

        protected List<Geocache> ParseMapInfos(String jsonResult, int[] TilePixels, CancellationToken ct, int zoomLevel)
        {
            if (!String.IsNullOrWhiteSpace(jsonResult))
            {
                var nameCache = new Dictionary<string, string>(); // JSON id, cache name

                var parsedData =
                    (GeocachingComApiCaches)
                        JsonConvert.DeserializeObject(jsonResult, typeof(GeocachingComApiCaches));

                var keys = parsedData.keys;

                var positions = new Dictionary<string, List<UTFGridPosition>>();

                int startTimer = Environment.TickCount;


                // JSON id as key
                for (var i = 1; i < keys.Length; i++)
                {
                    if (ct.IsCancellationRequested)
                        return null;
                    // index 0 is empty
                    var key = keys[i];
                    if (!String.IsNullOrWhiteSpace(key))
                    {
                        var pos = UTFGridPosition.FromString(key);

                        var dataForKey = parsedData.data[key];
                        foreach (var c in dataForKey)
                        {
                            var id = c.i;
                            if (!nameCache.ContainsKey(id))
                            {
                                nameCache.Add(id, c.n);
                            }

                            if (!positions.ContainsKey(id))
                            {
                                positions.Add(id, new List<UTFGridPosition>());
                            }
                            if (positions.Count > MAX_CACHES_ON_TILE)
                            {
                                Debug.WriteLine("Tile contains " + positions.Keys.Count + " caches, too much!");
                                //ContainsTooManyCaches = true;
                                //throw new TooManyCachesOnTileException();
                            }
                            positions[id].Add(pos);
                        }
                    }
                }
                Debug.WriteLine("Parsing positions : " + (Environment.TickCount - startTimer) + " ms");


                //if (positions.Keys.Count > MAX_CACHES_ON_TILE)
                //{
                //    Debug.WriteLine("Tile contains " + positions.Keys.Count + " caches, too much!");
                //    ContainsTooManyCaches = true;
                //    throw new TooManyCachesOnTileException();
                //}

                startTimer = Environment.TickCount;
                var caches = new List<Geocache>();

                foreach (var id in positions.Keys)
                {
                    if (ct.IsCancellationRequested)
                        return null;
                    var pos = positions[id];
                    var xy = UTFGrid.GetPositionInGrid(pos);
                    var cache = new GCGeocache()
                    {
                        Name = nameCache[id],
                        //HasFullCacheInfo = false,
                    };
                    cache.SetWaypoint(GetCoord(xy), zoomLevel);
                    //cache.SetCoordinates(GetCoord(xy), zoomLevel);
                    //caches.Add(cache);
                    if (IconDecoder.parseMapPNG(cache, TilePixels, xy, Zoomlevel))
                    {
                        caches.Add(cache);
                    }
                }
                Debug.WriteLine("Parsing images : " + (Environment.TickCount - startTimer) + " ms");

                return caches;
            }
            return null;
        }

        ///** Request JSON informations for a tile */
        //public void RequestMapInfo(Action<DownloadStringCompletedEventArgs> callback, string url, Dictionary<string, string> parameters, string referer)
        //{
        //    var urlString = FormUrl(url, parameters);

        //    var client = new WebClient();
        //    client.Headers["Referer"] = referer;

        //    client.DownloadStringCompleted += (sender, e) => callback(e);

        //    client.DownloadStringAsync(new Uri(urlString));
        //}
    }
}