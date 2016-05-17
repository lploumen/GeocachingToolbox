using System;
using System.Diagnostics;

namespace GeocachingToolbox.GeocachingCom.MapFetch
{
    public class IconDecoder
    {
        private const int IconWidth = 256;
        private const int IconHeight = 256;

        private const int CT_TRADITIONAL = 0;
        private const int CT_MULTI = 1;
        private const int CT_MYSTERY = 2;
        private const int CT_EVENT = 3;
        private const int CT_EARTH = 4;
        private const int CT_FOUND = 5;
        private const int CT_OWN = 6;
        private const int CT_MEGAEVENT = 7;
        private const int CT_CITO = 8;
        private const int CT_WEBCAM = 9;
        private const int CT_WHERIGO = 10;
        private const int CT_VIRTUAL = 11;
        private const int CT_LETTERBOX = 12;


        public static bool parseMapPNG(Geocache cache, int[] bitmap, UTFGridPosition xy, int zoomlevel)
        {
            int topX = xy.x * 4;
            int topY = xy.y * 4;
            int bitmapWidth = IconWidth;
            int bitmapHeight = IconHeight;

            if ((topX < 0) || (topY < 0) || (topX + 4 > bitmapWidth) || (topY + 4 > bitmapHeight))
            {
                return false; //out of image position
            }

            int numberOfDetections = 7; //for level 12 and 13
            if (zoomlevel < 12)
            {
                numberOfDetections = 5;
            }
            if (zoomlevel > 13)
            {
                numberOfDetections = 13;
            }

            int[] pngType = new int[numberOfDetections];
            for (int x = topX; x < topX + 4; x++)
            {
                for (int y = topY; y < topY + 4; y++)
                {
                    int color = GetPixelColor(bitmap, x, y);// bitmap.getPixel(x, y);

                    //if ((color >> 24) != 255)
                    //int c = color >> 24;
                    if (color == 0)
                    {
                        continue; //transparent pixels (or semi_transparent) are only shadows of border
                    }

                    int r = (color & 0xFF0000) >> 16;
                    int g = (color & 0xFF00) >> 8;
                    int b = color & 0xFF;

                    if (isPixelDuplicated(r, g, b, zoomlevel))
                    {
                        continue;
                    }

                    int t;
                    if (zoomlevel < 12)
                    {
                        t = getCacheTypeFromPixel11(r, g, b);
                    }
                    else
                    {
                        if (zoomlevel > 13)
                        {
                            t = getCacheTypeFromPixel14(r, g, b);
                        }
                        else
                        {
                            t = getCacheTypeFromPixel13(r, g, b);
                        }
                    }
                    pngType[(int)t]++;
                }
            }

            int type = -1;
            int count = 0;

            for (int x = 0; x < pngType.Length; x++)
            {
                if (pngType[x] > count)
                {
                    count = pngType[x];
                    type = x;
                }
            }

            if (count > 1)
            { // 2 pixels need to detect same type and we say good to go
                switch (type)
                {
                    case CT_TRADITIONAL:
                        cache.Type = GeocacheType.Traditional;
                        //cache.setType(CacheType.TRADITIONAL, zoomlevel);
                        return true;
                    case CT_MULTI:
                        cache.Type = GeocacheType.Multicache;
                        //cache.setType(CacheType.MULTI, zoomlevel);
                        return true;
                    case CT_MYSTERY:
                        //cache.setType(CacheType.MYSTERY, zoomlevel);
                        cache.Type = GeocacheType.Unknown;
                        return true;
                    //case CT_EVENT:
                    //    cache.Type = GeocacheType.;
                        //cache.setType(CacheType.EVENT, zoomlevel);
                        return true;
                    case CT_EARTH:
                        cache.Type = GeocacheType.Earthcache;
                        //cache.setType(CacheType.EARTH, zoomlevel);
                        return true;
                    //case CT_FOUND:
                    //    cache.
                    //    cache.Found = true;
                    //    //cache.setFound(true);
                    //    return true;
                    //case CT_OWN:
                    //    cache.Owner = true;
                        //cache.setOwnerUserId(Settings.getUsername());
                       // return true;
                    //case CT_MEGAEVENT:
                    //    cache.setType(CacheType.MEGA_EVENT, zoomlevel);
                    //    return true;
                    //case CT_CITO:
                    //    cache.setType(CacheType.CITO, zoomlevel);
                    //    return true;
                    //case CT_WEBCAM:
                    //    cache.setType(CacheType.WEBCAM, zoomlevel);
                    //    return true;
                    //case CT_WHERIGO:
                    //    cache.Type = CacheType.Wherigo;
                    //    return true;
                    //case CT_VIRTUAL:
                    //    cache.setType(CacheType.VIRTUAL, zoomlevel);
                    //    return true;
                    case CT_LETTERBOX:
                        cache.Type = GeocacheType.LetterboxHybrid;
                        //cache.setType(CacheType.LETTERBOX, zoomlevel);
                        return true;
                    default:
                        cache.Type = GeocacheType.Unknown;
                        return true;
                }
            }
            return false;
        }

        private static bool isPixelDuplicated(int r, int g, int b, int zoomlevel)
        {
            if (zoomlevel < 12)
            {
                if (((r == g) && (g == b)) || ((r == 233) && (g == 233) && (b == 234)))
                {
                    return true;
                }
                return false;
            }
            if (zoomlevel > 13)
            {
                if ((r == g) && (g == b))
                {
                    if ((r == 119) || (r == 231) || (r == 5) || (r == 230) || (r == 244) || (r == 93) || (r == 238) || (r == 73) || (r == 9) || (r == 225) || (r == 162) || (r == 153) || (r == 32) ||
                        (r == 50) || (r == 20) || (r == 232) || (r == 224) || (r == 192) || (r == 248) || (r == 152) || (r == 128) || (r == 176) || (r == 184) || (r == 200))
                    {
                        return false;
                    }
                    return true;
                }
                if ((r == 44) && (b == 44) && (g == 17) ||
                    (r == 228) && (b == 228) && (g == 255) ||
                    (r == 236) && (b == 236) && (g == 255) ||
                    (r == 252) && (b == 225) && (g == 83) ||
                    (r == 252) && (b == 221) && (g == 81) ||
                    (r == 252) && (b == 216) && (g == 79) ||
                    (r == 252) && (b == 211) && (g == 77) ||
                    (r == 251) && (b == 206) && (g == 75) ||
                    (r == 251) && (b == 201) && (g == 73) ||
                    (r == 251) && (b == 196) && (g == 71) ||
                    (r == 251) && (b == 191) && (g == 69) ||
                    (r == 243) && (b == 153) && (g == 36))
                {
                    return true;
                }
                return false;
            }
            //zoom level 12, 13
            if ((r == 95) && (g == 95) && (b == 95))
            {
                return true;
            }
            return false;
        }

        private static int getCacheTypeFromPixel13(int r, int g, int b)
        {
            if (b < 130)
            {
                if (r < 41)
                {
                    return CT_MYSTERY;
                }
                if (g < 74)
                {
                    return CT_EVENT;
                }
                if (r < 130)
                {
                    return CT_TRADITIONAL;
                }
                if (b < 31)
                {
                    return CT_MULTI;
                }
                if (b < 101)
                {
                    if (g < 99)
                    {
                        return r < 178 ? CT_FOUND : CT_EVENT;
                    }
                    if (b < 58)
                    {
                        if (g < 174)
                        {
                            return CT_FOUND;
                        }
                        if (r < 224)
                        {
                            return CT_OWN;
                        }
                        if (b < 49)
                        {
                            return g < 210 ? CT_FOUND : CT_OWN;
                        }
                        if (g < 205)
                        {
                            return g < 202 ? CT_FOUND : CT_OWN;
                        }
                        return CT_FOUND;
                    }
                    if (r < 255)
                    {
                        return CT_FOUND;
                    }
                    return g < 236 ? CT_MULTI : CT_FOUND;
                }
                return g < 182 ? CT_EVENT : CT_MULTI;
            }
            if (r < 136)
            {
                return CT_MYSTERY;
            }
            if (b < 168)
            {
                return g < 174 ? CT_EARTH : CT_TRADITIONAL;
            }
            return CT_EARTH;
        }

        /**
         * This method returns detected type from specific pixel from geocaching.com live map level 14 or higher.
         * It was constructed based on classification tree made by Orange (http://orange.biolab.si/)
         * Input file was made from every non-transparent pixel of every possible "full" cache icon from GC map
         *
         * @param r
         *            Red component of pixel (from 0 - 255)
         * @param g
         *            Green component of pixel (from 0 - 255)
         * @param b
         *            Blue component of pixel (from 0 - 255)
         * @return Value from 0 to 6 representing detected type or state of the cache.
         */
        private static int getCacheTypeFromPixel14(int r, int g, int b)
        {
            if (b < 128)
            {
                if (r < 214)
                {
                    if (b < 37)
                    {
                        if (g < 50)
                        {
                            if (b < 19)
                            {
                                if (g < 16)
                                {
                                    if (b < 4)
                                    {
                                        return CT_FOUND;
                                    }
                                    return r < 8 ? CT_VIRTUAL : CT_WEBCAM;
                                }
                                return CT_FOUND;
                            }
                            return CT_WEBCAM;
                        }
                        if (b < 24)
                        {
                            if (b < 18)
                            {
                                return CT_EARTH;
                            }
                            return r < 127 ? CT_TRADITIONAL : CT_EARTH;
                        }
                        return CT_FOUND;
                    }
                    if (r < 142)
                    {
                        if (r < 63)
                        {
                            if (r < 26)
                            {
                                return CT_CITO;
                            }
                            return r < 51 ? CT_WEBCAM : CT_CITO;
                        }
                        return g < 107 ? CT_WEBCAM : CT_MULTI;
                    }
                    if (g < 138)
                    {
                        return r < 178 ? CT_MEGAEVENT : CT_EVENT;
                    }
                    return b < 71 ? CT_FOUND : CT_EARTH;
                }
                if (b < 77)
                {
                    if (g < 166)
                    {
                        if (r < 238)
                        {
                            return g < 120 ? CT_MULTI : CT_OWN;
                        }
                        if (b < 57)
                        {
                            if (r < 254)
                            {
                                if (b < 39)
                                {
                                    if (r < 239)
                                    {
                                        return CT_OWN;
                                    }
                                    if (b < 36)
                                    {
                                        if (g < 150)
                                        {
                                            if (b < 24)
                                            {
                                                return b < 22 ? CT_FOUND : CT_OWN;
                                            }
                                            if (g < 138)
                                            {
                                                return b < 25 ? CT_FOUND : CT_OWN;
                                            }
                                            return CT_FOUND;
                                        }
                                        return CT_OWN;
                                    }
                                    if (b < 38)
                                    {
                                        if (b < 37)
                                        {
                                            if (g < 153)
                                            {
                                                return r < 242 ? CT_OWN : CT_FOUND;
                                            }
                                            return CT_OWN;
                                        }
                                        return CT_FOUND;
                                    }
                                    return CT_OWN;
                                }
                                if (g < 148)
                                {
                                    return CT_OWN;
                                }
                                if (r < 244)
                                {
                                    return CT_FOUND;
                                }
                                if (b < 45)
                                {
                                    if (b < 42)
                                    {
                                        return CT_FOUND;
                                    }
                                    if (g < 162)
                                    {
                                        return r < 245 ? CT_OWN : CT_FOUND;
                                    }
                                    return CT_OWN;
                                }
                                return CT_FOUND;
                            }
                            return g < 3 ? CT_FOUND : CT_VIRTUAL;
                        }
                        return CT_OWN;
                    }
                    if (b < 51)
                    {
                        if (r < 251)
                        {
                            return CT_OWN;
                        }
                        return g < 208 ? CT_EARTH : CT_MULTI;
                    }
                    if (b < 63)
                    {
                        if (r < 247)
                        {
                            return CT_FOUND;
                        }
                        if (r < 250)
                        {
                            if (g < 169)
                            {
                                return CT_FOUND;
                            }
                            if (g < 192)
                            {
                                if (b < 54)
                                {
                                    return CT_OWN;
                                }
                                if (r < 248)
                                {
                                    return g < 180 ? CT_FOUND : CT_OWN;
                                }
                                return CT_OWN;
                            }
                            return g < 193 ? CT_FOUND : CT_OWN;
                        }
                        return CT_FOUND;
                    }
                    return CT_FOUND;
                }
                if (g < 177)
                {
                    return CT_OWN;
                }
                if (r < 239)
                {
                    return CT_FOUND;
                }
                if (g < 207)
                {
                    return CT_OWN;
                }
                return r < 254 ? CT_FOUND : CT_EARTH;
            }
            if (r < 203)
            {
                if (b < 218)
                {
                    if (g < 158)
                    {
                        if (g < 71)
                        {
                            return CT_MYSTERY;
                        }
                        return r < 153 ? CT_WHERIGO : CT_WEBCAM;
                    }
                    if (b < 167)
                    {
                        return r < 157 ? CT_TRADITIONAL : CT_WEBCAM;
                    }
                    return CT_WHERIGO;
                }
                if (g < 199)
                {
                    if (r < 142)
                    {
                        return CT_LETTERBOX;
                    }
                    return r < 175 ? CT_CITO : CT_LETTERBOX;
                }
                if (g < 207)
                {
                    return r < 167 ? CT_MEGAEVENT : CT_CITO;
                }
                return CT_EARTH;
            }
            if (b < 224)
            {
                if (g < 235)
                {
                    if (b < 163)
                    {
                        if (r < 249)
                        {
                            return b < 133 ? CT_FOUND : CT_OWN;
                        }
                        return CT_FOUND;
                    }
                    if (r < 235)
                    {
                        if (r < 213)
                        {
                            if (r < 207)
                            {
                                return CT_FOUND;
                            }
                            if (g < 206)
                            {
                                return CT_OWN;
                            }
                            return g < 207 ? CT_FOUND : CT_OWN;
                        }
                        return g < 194 ? CT_OWN : CT_FOUND;
                    }
                    if (g < 230)
                    {
                        return CT_OWN;
                    }
                    return b < 205 ? CT_FOUND : CT_OWN;
                }
                if (r < 238)
                {
                    return CT_CITO;
                }
                return b < 170 ? CT_EVENT : CT_FOUND;
            }
            if (r < 251)
            {
                if (r < 210)
                {
                    return CT_MYSTERY;
                }
                if (b < 252)
                {
                    if (r < 243)
                    {
                        if (r < 225)
                        {
                            return CT_WHERIGO;
                        }
                        if (b < 232)
                        {
                            if (g < 228)
                            {
                                return CT_WEBCAM;
                            }
                            return r < 231 ? CT_VIRTUAL : CT_TRADITIONAL;
                        }
                        if (r < 236)
                        {
                            return CT_WHERIGO;
                        }
                        return r < 240 ? CT_WEBCAM : CT_WHERIGO;
                    }
                    if (g < 247)
                    {
                        return r < 245 ? CT_WEBCAM : CT_FOUND;
                    }
                    return CT_WHERIGO;
                }
                return CT_LETTERBOX;
            }
            if (r < 255)
            {
                return CT_OWN;
            }
            return g < 254 ? CT_FOUND : CT_OWN;
        }

        /**
         * This method returns detected type from specific pixel from geocaching.com live map level 11 or lower.
         * It was constructed based on classification tree made by Orange (http://orange.biolab.si/)
         * Input file was made from every non-transparent pixel of every possible "full" cache icon from GC map
         *
         * @param r
         *            Red component of pixel (from 0 - 255)
         * @param g
         *            Green component of pixel (from 0 - 255)
         * @param b
         *            Blue component of pixel (from 0 - 255)
         * @return Value from 0 to 4 representing detected type or state of the cache.
         */
        private static int getCacheTypeFromPixel11(int r, int g, int b)
        {
            if (g < 136)
            {
                if (r < 90)
                {
                    return g < 111 ? CT_MYSTERY : CT_TRADITIONAL;
                }
                return b < 176 ? CT_EVENT : CT_MYSTERY;
            }
            if (r < 197)
            {
                return CT_TRADITIONAL;
            }
            return b < 155 ? CT_MULTI : CT_EARTH;
        }



        //-------------------------------------------------------------------------------------------------------


        //public static void parseMapPNG(Cache cache, int[] bitmap, UTFGridPosition xy, int zoomlevel)
        //{
        //    if (zoomlevel >= 14)
        //    {
        //        parseMapPNG14(cache, bitmap, xy);
        //    }
        //    else
        //    {
        //        parseMapPNG13(cache, bitmap, xy);
        //    }
        //}

        private static readonly int[] OFFSET_X = { 0, -1, -1, 0, 1, 1, 1, 0, -1, -2, -2, -2, -2, -1, 0, 1, 2, 2, 2, 2, 2, 1, 0, -1, -2 };
        private static readonly int[] OFFSET_Y = { 0, 0, 1, 1, 1, 0, -1, -1, -1, -1, 0, 1, 2, 2, 2, 2, 2, 1, 0, -1, -2, -2, -2, -2, -2 };

        private static int GetPixelColor(int[] bitmap, int x, int y)
        {
            return  bitmap[y * IconWidth + x];
            //int R = bitmap[y * 4 * IconWidth + x * 4]; // lpl 20141113 & 0x00FF FF FF;
            //int G = bitmap[y * IconWidth * 4 + x * 4 + 1];
            //int B = bitmap[y * IconWidth * 4 + x * 4 + 2];
            //int color = (R << 16) + (G << 8) + B;
            //return color;
        }

        /**
         * The icon decoder walks a spiral around the center pixel position of the cache
         * and searches for characteristic colors.
         *
         * @param cache
         * @param bitmap
         * @param xy
         */
        private static void parseMapPNG13(Geocache cache, int[] bitmap, UTFGridPosition xy)
        {
            int xCenter = xy.x * 4 + 2;
            int yCenter = xy.y * 4 + 2;
            const int bitmapWidth = IconWidth;
            const int bitmapHeight = IconHeight;

            int countMulti = 0;
            int countFound = 0;

            for (int i = 0; i < OFFSET_X.Length; i++)
            {

                // assert that we are still in the tile
                int x = xCenter + OFFSET_X[i];
                if (x < 0 || x >= bitmapWidth)
                {
                    continue;
                }

                int y = yCenter + OFFSET_Y[i];
                if (y < 0 || y >= bitmapHeight)
                {
                    continue;
                }

                int color = GetPixelColor(bitmap, x, y);

                // transparent pixels are not interesting
                if (color == 0)
                {
                    continue;
                }

                int red = (color & 0xFF0000) >> 16;
                int green = (color & 0xFF00) >> 8;
                int blue = color & 0xFF;

                // these are quite sure, so one pixel is enough for matching
                if (green > 0x80 && green > red && green > blue)
                {
                    cache.Type = GeocacheType.Traditional;// Type = GeocachingComCache.Types.TRADITIONAL;

                }
                if (blue > 0x80 && blue > red && blue > green)
                {
                    // cache.Type = GeocachingComCache.Types.MYSTERY;
                    cache.Type = GeocacheType.Unknown;
                }
                //if (red > 0x90 && blue < 0x10 && green < 0x10)
                //{
                //    // cache.Type = GeocachingComCache.Types.EVENT;
                //    cache.Type = CacheType.Event;
                //}

                // next two are hard to distinguish, therefore we sample all pixels of the spiral
                if (red > 0xFA && green > 0xD0)
                {
                    countMulti++;
                }
                if (red < 0xF3 && red > 0xa0 && green > 0x20 && blue < 0x80)
                {
                    countFound++;
                }
            }

            // now check whether we are sure about found/multi
            //if (countFound > countMulti && countFound >= 2)
            //{
            //    cache.Found = true;
            //}
            if (countMulti > countFound && countMulti >= 5)
            {
                cache.Type = GeocacheType.Multicache;
            }
        }

        // Pixel colors in tile
        private const int COLOR_BORDER_GRAY = 0x5F5F5F;
        private const int COLOR_TRADITIONAL = 0x316013;
        private const int COLOR_MYSTERY = 0x243C97;
        private const int COLOR_MULTI = 0xFFDE19;
        private const int COLOR_FOUND = 0xFBEA5D;

        // Offset inside cache icon
        private const int POSX_TRADI = 7;
        private const int POSY_TRADI = -12;
        private const int POSX_MULTI = 5; // for orange 8
        private const int POSY_MULTI = -9; // for orange 10
        private const int POSX_MYSTERY = 5;
        private const int POSY_MYSTERY = -13;
        private const int POSX_FOUND = 10;
        private const int POSY_FOUND = -8;

        /**
         * For level 14 find the borders of the icons and then use a single pixel and color to match.
         *
         * @param cache
         * @param bitmap
         * @param xy
         */
        private static void parseMapPNG14(Geocache cache, int[] bitmap, UTFGridPosition xy)
        {
            int x = xy.x * 4 + 2;
            int y = xy.y * 4 + 2;

            // search for left border
            int countX = 0;
            while ((GetPixelColor(bitmap, x, y)) != COLOR_BORDER_GRAY)
            {
                if (--x < 0 || ++countX > 20)
                {
                    return;
                }
            }
            // search for bottom border
            int countY = 0;
            while ((GetPixelColor(bitmap, x, y)) != 0x000000)
            {
                if (++y >= IconHeight || ++countY > 20)
                {
                    return;
                }
            }

            try
            {

                if ((GetPixelColor(bitmap, x + POSX_TRADI, y + POSY_TRADI)) == COLOR_TRADITIONAL)
                {
                    cache.Type = GeocacheType.Traditional;
                }
                if ((GetPixelColor(bitmap, x + POSX_MYSTERY, y + POSY_MYSTERY)) == COLOR_MYSTERY)
                {
                    cache.Type = GeocacheType.Unknown;
                }
                if ((GetPixelColor(bitmap, x + POSX_MULTI, y + POSY_MULTI)) == COLOR_MULTI)
                {
                    cache.Type = GeocacheType.Multicache;
                }
                //if ((GetPixelColor(bitmap, x + POSX_FOUND, y + POSY_FOUND)) == COLOR_FOUND)
                //{
                //    cache.Found = true;
                //}
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception when trying to get cache type for cache : " + cache.Name + " ==>" + e.Message);
                // intentionally left blank
            }

        }

    }
}