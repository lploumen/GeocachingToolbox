using System;

namespace GeocachingToolbox.GeocachingCom.MapFetch
{
    public class Viewport
    {
        public Location center;
        public Location bottomLeft;
        public Location topRight;

        public Viewport(Location point1, Location point2)
        {
            //GeoCoordinate gp1 = point1.GetCoords();
            //GeoCoordinate gp2 = point2.GetCoords();
            bottomLeft = new Location(Math.Min(point1.Latitude, point2.Latitude), Math.Min(point1.Longitude, point2.Longitude));
            topRight = new Location(Math.Max(point1.Latitude, point2.Latitude), Math.Max(point1.Longitude, point2.Longitude));
            center = new Location((point1.Latitude + point2.Latitude) / 2, (point1.Longitude + point2.Longitude) / 2);
        }

        public Viewport(double gp1Longitude, double gp2Longitude, double gp1Latitude, double gp2Latitude)
        {
            bottomLeft = new Location(Math.Min(gp1Latitude, gp2Latitude), Math.Min(gp1Longitude, gp2Longitude));
            topRight = new Location(Math.Max(gp1Latitude, gp2Latitude), Math.Max(gp1Longitude, gp2Longitude));
            center = new Location((gp1Latitude + gp2Latitude) / 2, (gp1Longitude + gp2Longitude) / 2);
        }

        //public Viewport(GeoCoordinate gp1, GeoCoordinate gp2)
        //{
        //    this.bottomLeft = new GeoCoordinate(Math.Min(gp1.Latitude, gp2.Latitude), Math.Min(gp1.Longitude, gp2.Longitude));
        //    this.topRight = new GeoCoordinate(Math.Max(gp1.Latitude, gp2.Latitude), Math.Max(gp1.Longitude, gp2.Longitude));
        //    this.center = new GeoCoordinate((gp1.Latitude + gp2.Latitude) / 2, (gp1.Longitude + gp2.Longitude) / 2);
        //}

        /**
         * Check whether a point is contained in this viewport.
         *
         * @param point
         *            the coordinates to check
         * @return true if the point is contained in this viewport, false otherwise or if the point contains no coordinates
         */
        public bool Contains(Location coords)
        {
            return coords != null
                   && coords.Longitude >= bottomLeft.Longitude
                   && coords.Longitude <= topRight.Longitude
                   && coords.Latitude >= bottomLeft.Latitude
                   && coords.Latitude <= topRight.Latitude;
        }

    }
}