namespace GeocachingToolbox.GeocachingCom.MapFetch
{
    class TileCache
    {
        private readonly LruCache<int, Tile> _tileCache = new LruCache<int, Tile>(64);

        /*
                    public static void RemoveFromTileCache(GeoCoordinate point) {
                        if (point != null) {
                            var tiles = tileCache.GetValues();
                            foreach (Tile tile in tiles) {
                                if (tile.ContainsPoint(point)) {
                                    tileCache.remove(tile.GetHashCode());
                                }
                            }
                        }
                    }
        */

        public bool Contains(Tile tile)
        {
            return _tileCache.ContainsKey(tile.GetHashCode());
        }

        public void Add(Tile tile)
        {
            _tileCache.Add(tile.GetHashCode(), tile);
        }
    }
}