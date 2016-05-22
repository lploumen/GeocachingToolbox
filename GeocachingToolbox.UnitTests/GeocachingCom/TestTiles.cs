using System;
using GeocachingToolbox.GeocachingCom.MapFetch;
using Machine.Specifications;

namespace GeocachingToolbox.UnitTests.GeocachingCom
{
    [Subject("Test Zoom")]
    public class TestTiles
    {
        static readonly Location bottomLeft = new Location(49.3, 8.3);
        static readonly Location topRight = new Location(49.4, 8.4);
        static int zoomLat;
        static int zoomLon;
        Establish context = () =>
        {

        };
        Because of = () =>
        {
            zoomLat = Tile.CalcZoomLat(bottomLeft, topRight);
            zoomLon = Tile.CalcZoomLon(bottomLeft, topRight);
        };

        It should_return_tiles = () =>
        {
            //zoomLat
            Math.Abs(new Tile(bottomLeft, zoomLat).TileY - new Tile(topRight, zoomLat).TileY).ShouldEqual(1);
            Math.Abs(new Tile(bottomLeft, zoomLat + 1).TileY - new Tile(topRight, zoomLat + 1).TileY).ShouldBeGreaterThan(1);

            // zoomLon
            (new Tile(bottomLeft, zoomLon).TileX + 1).ShouldEqual(new Tile(topRight, zoomLon).TileX);
            (new Tile(bottomLeft, zoomLon+1).TileX + 1).ShouldBeLessThan(new Tile(topRight, zoomLon +1).TileX);
        };
    }
}