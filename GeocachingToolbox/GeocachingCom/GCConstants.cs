using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeocachingToolbox.GeocachingCom
{
    public class GCConstants
    {
        public const string PATTERN_USERSESSION = "UserSession\\('([^']+)'";
        public const string PATTERN_SESSIONTOKEN = "sessionToken:'([^']+)'";
        public const string GC_URL = "http://www.geocaching.com/";
        /** Live Map */
        public const string URL_LIVE_MAP = GC_URL + "map/default.aspx";
        /** Live Map pop-up */
        public const string URL_LIVE_MAP_DETAILS = GC_URL + "map/map.details";
        /** Caches in a tile */
        public const string URL_MAP_INFO = GC_URL + "map/map.info";
        /** Tile itself */
        public const string URL_MAP_TILE = GC_URL + "map/map.png";
    }
}
