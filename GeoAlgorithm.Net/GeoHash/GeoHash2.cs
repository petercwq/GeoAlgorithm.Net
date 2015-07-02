using System;
using System.Collections.Generic;
using System.Text;

namespace GeoAlgorithm.Net.GeoHash
{
    /**
     * <p>
     * Utility functions for <a
     * href="http://en.wikipedia.org/wiki/Geohash">geohashing</a>.
     * </p>
     * 
     * @author wq
     * 
     */
    public static class GeoHash2
    {
        public enum Direction
        {
            BOTTOM,
            TOP,
            LEFT,
            RIGHT
        }

        public enum Parity
        {
            EVEN, ODD
        }

        public static Direction Opposite(this Direction direction)
        {
            if (direction == Direction.BOTTOM)
                return Direction.TOP;
            else if (direction == Direction.TOP)
                return Direction.BOTTOM;
            else if (direction == Direction.LEFT)
                return Direction.RIGHT;
            else
                return Direction.LEFT;
        }

        private const double PRECISION = 0.000000000001;

        /**
         * The standard practical maximum length for geohashes.
         */
        public const int MAX_HASH_LENGTH = 12;

        /**
         * Default maximum number of hashes for covering a bounding box.
         */
        public const int DEFAULT_MAX_HASHES = 12;

        /**
         * Powers of 2 from 32 down to 1.
         */
        private static readonly int[] BITS = new int[] { 16, 8, 4, 2, 1 };

        /**
         * The characters used in base 32 representations.
         */
        private static readonly string BASE32 = "0123456789bcdefghjkmnpqrstuvwxyz";

        /**
         * Utility lookup for neighbouring hashes.
         */
        private static readonly Dictionary<Direction, Dictionary<Parity, string>> NEIGHBOURS = CreateNeighbours();

        /**
         * Utility lookup for hash borders.
         */
        private static readonly Dictionary<Direction, Dictionary<Parity, string>> BORDERS = CreateBorders();

        /**
         * Returns a map to be used in hash border calculations.
         * 
         * @return map of borders
         */
        private static Dictionary<Direction, Dictionary<Parity, string>> CreateBorders()
        {
            Dictionary<Direction, Dictionary<Parity, string>> m = CreateDirectionParityMap();

            m[Direction.RIGHT].Add(Parity.EVEN, "bcfguvyz");
            m[Direction.LEFT].Add(Parity.EVEN, "0145hjnp");
            m[Direction.TOP].Add(Parity.EVEN, "prxz");
            m[Direction.BOTTOM].Add(Parity.EVEN, "028b");

            AddOddParityEntries(m);
            return m;
        }

        /**
         * Returns a map to be used in adjacent hash calculations.
         * 
         * @return map
         */
        private static Dictionary<Direction, Dictionary<Parity, string>> CreateNeighbours()
        {
            Dictionary<Direction, Dictionary<Parity, string>> m = CreateDirectionParityMap();

            m[Direction.RIGHT].Add(Parity.EVEN, "bc01fg45238967deuvhjyznpkmstqrwx");
            m[Direction.LEFT].Add(Parity.EVEN, "238967debc01fg45kmstqrwxuvhjyznp");
            m[Direction.TOP].Add(Parity.EVEN, "p0r21436x8zb9dcf5h7kjnmqesgutwvy");
            m[Direction.BOTTOM].Add(Parity.EVEN, "14365h7k9dcfesgujnmqp0r2twvyx8zb");
            AddOddParityEntries(m);

            return m;
        }

        /**
         * Create a direction and parity map for use in adjacent hash calculations.
         * 
         * @return map
         */
        private static Dictionary<Direction, Dictionary<Parity, string>> CreateDirectionParityMap()
        {
            Dictionary<Direction, Dictionary<Parity, string>> m = new Dictionary<Direction, Dictionary<Parity, string>>();
            m.Add(Direction.BOTTOM, new Dictionary<Parity, string>());
            m.Add(Direction.TOP, new Dictionary<Parity, string>());
            m.Add(Direction.LEFT, new Dictionary<Parity, string>());
            m.Add(Direction.RIGHT, new Dictionary<Parity, string>());
            return m;
        }

        /**
         * Puts odd parity entries in the map m based purely on the even entries.
         * 
         * @param m
         *            map
         */
        private static void AddOddParityEntries(
                Dictionary<Direction, Dictionary<Parity, string>> m)
        {
            m[Direction.BOTTOM].Add(Parity.ODD, m[Direction.LEFT][Parity.EVEN]);
            m[Direction.TOP].Add(Parity.ODD, m[Direction.RIGHT][Parity.EVEN]);
            m[Direction.LEFT].Add(Parity.ODD, m[Direction.BOTTOM][Parity.EVEN]);
            m[Direction.RIGHT].Add(Parity.ODD, m[Direction.TOP][Parity.EVEN]);
        }

        /**
         * Returns the adjacent hash in given {@link Direction}. Based on
         * https://github.com/davetroy/geohash-js/blob/master/geohash.js. This
         * method is an improvement on the original js method because it works at
         * borders too (at the poles and the -180,180 longitude boundaries).
         * 
         * @param hash
         * @param direction
         * @return hash of adjacent hash
         */
        public static string AdjacentHash(string hash, Direction direction)
        {
            if (string.IsNullOrWhiteSpace(hash))
                throw new ArgumentException("adjacent has no meaning for a zero length hash that covers the whole world");

            string ahat = AdjacentHashAtBorder(hash, direction);
            if (ahat != null)
                return ahat;

            string source = hash.ToLower();
            char lastChar = source[source.Length - 1];
            Parity parity = (source.Length % 2 == 0) ? Parity.EVEN : Parity.ODD;
            string b = source.Substring(0, source.Length - 1);
            if (BORDERS[direction][parity].IndexOf(lastChar) != -1)
                b = AdjacentHash(b, direction);
            return b + BASE32[NEIGHBOURS[direction][parity].IndexOf(lastChar)];
        }

        private static string AdjacentHashAtBorder(string hash, Direction direction)
        {
            // check if hash is on edge and direction would push us over the edge
            // if so, wrap round to the other limit for longitude
            // or if at latitude boundary (a pole) then spin longitude around 180
            // degrees.
            LatLong centre = DecodeHash(hash);

            // if rightmost hash
            if (Direction.RIGHT == direction)
            {
                if (Math.Abs(centre.Lon + WidthDegrees(hash.Length) / 2
                        - 180) < PRECISION)
                {
                    return EncodeHash(centre.Lat, -180, hash.Length);
                }
            }
            // if leftmost hash
            else if (Direction.LEFT == direction)
            {
                if (Math.Abs(centre.Lon - WidthDegrees(hash.Length) / 2
                        + 180) < PRECISION)
                {
                    return EncodeHash(centre.Lat, 180, hash.Length);
                }
            }
            // if topmost hash
            else if (Direction.TOP == direction)
            {
                if (Math.Abs(centre.Lat + WidthDegrees(hash.Length) / 2 - 90) < PRECISION)
                {
                    return EncodeHash(centre.Lat, centre.Lon + 180,
                            hash.Length);
                }
            }
            // if bottommost hash
            else
            {
                if (Math.Abs(centre.Lat - WidthDegrees(hash.Length) / 2 + 90) < PRECISION)
                {
                    return EncodeHash(centre.Lat, centre.Lon + 180,
                            hash.Length);
                }
            }

            return null;
        }

        /**
         * Returns the adjacent hash to the right (east).
         * 
         * @param hash
         *            to check
         * @return hash on right of given hash
         */
        public static string Right(string hash)
        {
            return AdjacentHash(hash, Direction.RIGHT);
        }

        /**
         * Returns the adjacent hash to the left (west).
         * 
         * @param hash
         *            origin
         * @return hash on left of origin hash
         */
        public static string Left(string hash)
        {
            return AdjacentHash(hash, Direction.LEFT);
        }

        /**
         * Returns the adjacent hash to the top (north).
         * 
         * @param hash
         *            origin
         * @return hash above origin hash
         */
        public static string Top(string hash)
        {
            return AdjacentHash(hash, Direction.TOP);
        }

        /**
         * Returns the adjacent hash to the bottom (south).
         * 
         * @param hash
         *            origin
         * @return hash below (south) of origin hash
         */
        public static string Bottom(string hash)
        {
            return AdjacentHash(hash, Direction.BOTTOM);
        }

        /**
         * Returns the adjacent hash N steps in the given {@link Direction}. A
         * negative N will use the opposite {@link Direction}.
         * 
         * @param hash
         *            origin hash
         * @param direction
         *            to desired hash
         * @param steps
         *            number of hashes distance to desired hash
         * @return hash at position in direction a number of hashes away (steps)
         */
        public static string AdjacentHash(string hash, Direction direction, int steps)
        {
            if (steps < 0)
                return AdjacentHash(hash, direction.Opposite(), Math.Abs(steps));
            else
            {
                string h = hash;
                for (int i = 0; i < steps; i++)
                    h = AdjacentHash(h, direction);
                return h;
            }
        }

        /**
         * Returns a list of the 8 surrounding hashes for a given hash in order
         * left,right,top,bottom,left-top,left-bottom,right-top,right-bottom.
         * 
         * @param hash
         *            source
         * @return a list of neighbour hashes
         */
        public static List<string> Neighbours(string hash)
        {
            List<string> list = new List<string>();
            string left = AdjacentHash(hash, Direction.LEFT);
            string right = AdjacentHash(hash, Direction.RIGHT);
            list.Add(left);
            list.Add(right);
            list.Add(AdjacentHash(hash, Direction.TOP));
            list.Add(AdjacentHash(hash, Direction.BOTTOM));
            list.Add(AdjacentHash(left, Direction.TOP));
            list.Add(AdjacentHash(left, Direction.BOTTOM));
            list.Add(AdjacentHash(right, Direction.TOP));
            list.Add(AdjacentHash(right, Direction.BOTTOM));
            return list;
        }

        /**
         * Returns a geohash of given length for the given WGS84 point.
         * 
         * @param p
         *            point
         * @param length
         *            length of hash
         * @return hash at point of given length
         */
        public static string EncodeHash(LatLong p, int length)
        {
            return EncodeHash(p.Lat, p.Lon, length);
        }

        /**
         * Returns a geohash of of length {@link GeoHash#MAX_HASH_LENGTH} (12) for
         * the given WGS84 point.
         * 
         * @param p
         *            point
         * @return hash of default length
         */
        public static string EncodeHash(LatLong p)
        {
            return EncodeHash(p.Lat, p.Lon);
        }

        /**
         * Returns a geohash of given length for the given WGS84 point
         * (latitude,longitude). If latitude is not between -90 and 90 throws an
         * {@link IllegalArgumentException}.
         * 
         * @param latitude
         *            in decimal degrees (WGS84)
         * @param longitude
         *            in decimal degrees (WGS84)
         * @param length
         *            length of desired hash
         * @return geohash of given length for the given point
         */
        // Translated to java from:
        // geohash.js
        // Geohash library for Javascript
        // (c) 2008 David Troy
        // Distributed under the MIT License
        public static string EncodeHash(double latitude, double longitude, int length = MAX_HASH_LENGTH)
        {
            if (length <= 0)
                throw new ArgumentException("length must be greater than zero");
            if (!(latitude >= -90 && latitude <= 90))
                throw new ArgumentException("latitude must be between -90 and 90 inclusive");
            To180(ref longitude);
            return FromLongToString(EncodeHashToLong(latitude, longitude, length));
        }

        private static void To180(ref double longitude)
        {
            while (longitude > 180)
                longitude -= 360;
            while (longitude < -180)
                longitude += 360;
        }

        /** Takes a hash represented as a long and returns it as a string.
         *
         * @param hash the hash, with the length encoded in the 4 least significant bits
         * @return the string encoded geohash
         */
        public static string FromLongToString(long hash)
        {
            int length = (int)(hash & 0xf);
            if (length > 12 || length < 1)
                throw new ArgumentException("invalid long geohash " + hash);
            char[] geohash = new char[length];
            for (int pos = 0; pos < length; pos++)
            {
                geohash[pos] = BASE32[(int)(hash >> 59)];
                hash <<= 5;
            }
            return new string(geohash);
        }

        public static long EncodeHashToLong(double latitude, double longitude, int length)
        {
            bool isEven = true;
            double minLat = -90.0, maxLat = 90;
            double minLon = -180.0, maxLon = 180.0;
            var bit = 0x8000000000000000L;
            ulong g = 0x0L;

            var target = 0x8000000000000000L >> (5 * length);
            while (bit != target)
            {
                if (isEven)
                {
                    double mid = (minLon + maxLon) / 2;
                    if (longitude >= mid)
                    {
                        g |= bit;
                        minLon = mid;
                    }
                    else
                        maxLon = mid;
                }
                else
                {
                    double mid = (minLat + maxLat) / 2;
                    if (latitude >= mid)
                    {
                        g |= bit;
                        minLat = mid;
                    }
                    else
                        maxLat = mid;
                }

                isEven = !isEven;
                bit >>= 1;
            }

            long gl = (long)g;
            return gl |= length;
        }

        /**
         * Returns a latitude,longitude pair as the centre of the given geohash.
         * Latitude will be between -90 and 90 and longitude between -180 and 180.
         * 
         * @param geohash
         * @return lat long point
         */
        // Translated to java from:
        // geohash.js
        // Geohash library for Javascript
        // (c) 2008 David Troy
        // Distributed under the MIT License
        public static LatLong DecodeHash(string geohash)
        {
            if (string.IsNullOrWhiteSpace(geohash))
                throw new ArgumentException("geohash cannot be null");
            bool isEven = true;
            double[] lat = new double[2];
            double[] lon = new double[2];
            lat[0] = -90.0;
            lat[1] = 90.0;
            lon[0] = -180.0;
            lon[1] = 180.0;

            for (int i = 0; i < geohash.Length; i++)
            {
                char c = geohash[i];
                int cd = BASE32.IndexOf(c);
                for (int j = 0; j < 5; j++)
                {
                    int mask = BITS[j];
                    if (isEven)
                    {
                        RefineInterval(lon, cd, mask);
                    }
                    else
                    {
                        RefineInterval(lat, cd, mask);
                    }
                    isEven = !isEven;
                }
            }
            double resultLat = (lat[0] + lat[1]) / 2;
            double resultLon = (lon[0] + lon[1]) / 2;

            return new LatLong(resultLat, resultLon);
        }

        /**
         * Refines interval by a factor or 2 in either the 0 or 1 ordinate.
         * 
         * @param interval
         *            two entry array of double values
         * @param cd
         *            used with mask
         * @param mask
         *            used with cd
         */
        private static void RefineInterval(double[] interval, int cd, int mask)
        {
            if ((cd & mask) != 0)
                interval[0] = (interval[0] + interval[1]) / 2;
            else
                interval[1] = (interval[0] + interval[1]) / 2;
        }

        /**
         * Returns the maximum length of hash that covers the bounding box. If no
         * hash can enclose the bounding box then 0 is returned.
         * 
         * @param topLeftLat
         * @param topLeftLon
         * @param bottomRightLat
         * @param bottomRightLon
         * @return
         */
        public static int HashLengthToCoverBoundingBox(double topLeftLat, double topLeftLon, double bottomRightLat, double bottomRightLon)
        {
            bool isEven = true;
            double minLat = -90.0, maxLat = 90;
            double minLon = -180.0, maxLon = 180.0;

            for (int bits = 0; bits < MAX_HASH_LENGTH * 5; bits++)
            {
                if (isEven)
                {
                    double mid = (minLon + maxLon) / 2;
                    if (topLeftLon >= mid)
                    {
                        if (bottomRightLon < mid)
                            return bits / 5;
                        minLon = mid;
                    }
                    else
                    {
                        if (bottomRightLon >= mid)
                            return bits / 5;
                        maxLon = mid;
                    }
                }
                else
                {
                    double mid = (minLat + maxLat) / 2;
                    if (topLeftLat >= mid)
                    {
                        if (bottomRightLat < mid)
                            return bits / 5;
                        minLat = mid;
                    }
                    else
                    {
                        if (bottomRightLat >= mid)
                            return bits / 5;
                        maxLat = mid;
                    }
                }

                isEven = !isEven;
            }
            return MAX_HASH_LENGTH;
        }

        /**
         * Returns true if and only if the bounding box corresponding to the hash
         * contains the given lat and long.
         * 
         * @param hash
         * @param lat
         * @param lon
         * @return
         */
        public static bool HashContains(string hash, double lat, double lon)
        {
            LatLong centre = DecodeHash(hash);
            var nlon = centre.Lon - lon;
            To180(ref nlon);
            return Math.Abs(centre.Lat - lat) <= HeightDegrees(hash.Length) / 2
                    && Math.Abs(nlon) <= WidthDegrees(hash.Length) / 2;
        }

        /**
         * Returns the result of coverBoundingBoxMaxHashes with a maxHashes value of
         * {@link GeoHash}.DEFAULT_MAX_HASHES.
         * 
         * @param topLeftLat
         * @param topLeftLon
         * @param bottomRightLat
         * @param bottomRightLon
         * @return
         */
        public static Coverage CoverBoundingBox(double topLeftLat, double topLeftLon, double bottomRightLat, double bottomRightLon)
        {

            return CoverBoundingBoxMaxHashes(topLeftLat, topLeftLon,
                    bottomRightLat, bottomRightLon, DEFAULT_MAX_HASHES);
        }

        /**
         * Returns the hashes that are required to cover the given bounding box. The
         * maximum length of hash is selected that satisfies the number of hashes
         * returned is less than <code>maxHashes</code>. Returns null if hashes
         * cannot be found satisfying that condition. Maximum hash length returned
         * will be {@link GeoHash}.MAX_HASH_LENGTH.
         * 
         * @param topLeftLat
         * @param topLeftLon
         * @param bottomRightLat
         * @param bottomRightLon
         * @param maxHashes
         * @return
         */
        public static Coverage CoverBoundingBoxMaxHashes(double topLeftLat, double topLeftLon, double bottomRightLat, double bottomRightLon, int maxHashes)
        {
            CoverageLongs coverage = null;
            int startLength = HashLengthToCoverBoundingBox(topLeftLat, topLeftLon,
                    bottomRightLat, bottomRightLon);
            if (startLength == 0)
                startLength = 1;
            for (int length = startLength; length <= MAX_HASH_LENGTH; length++)
            {
                CoverageLongs c = CoverBoundingBoxLongs(topLeftLat, topLeftLon,
                        bottomRightLat, bottomRightLon, length);
                if (c.Count > maxHashes)
                    return coverage == null ? null : new Coverage(coverage);
                else
                    coverage = c;
            }
            //note coverage can never be null
            return new Coverage(coverage);
        }

        /**
         * Returns the hashes of given length that are required to cover the given
         * bounding box.
         * 
         * @param topLeftLat
         * @param topLeftLon
         * @param bottomRightLat
         * @param bottomRightLon
         * @param length
         * @return
         */
        public static Coverage CoverBoundingBox(double topLeftLat, double topLeftLon, double bottomRightLat, double bottomRightLon, int length)
        {
            return new Coverage(CoverBoundingBoxLongs(topLeftLat, topLeftLon, bottomRightLat, bottomRightLon, length));
        }

        private class LongSet
        {
            public int count = 0;
            int cap = 16;
            public long[] array = new long[16];

            public void add(long l)
            {
                for (int i = 0; i < count; i++)
                    if (array[i] == l)
                        return;
                if (count == cap)
                {
                    long[] newArray = new long[cap *= 2];
                    Array.Copy(array, 0, newArray, 0, count);
                    array = newArray;
                }
                array[count++] = l;
            }
        }

        private static CoverageLongs CoverBoundingBoxLongs(double topLeftLat, double topLeftLon, double bottomRightLat, double bottomRightLon, int length)
        {
            if (topLeftLat < bottomRightLat)
                throw new ArgumentException("topLeftLat must be >= bottomRighLat");
            if (topLeftLon > bottomRightLon)
                throw new ArgumentException("topLeftLon must be <= bottomRighLon");
            if (length <= 0)
                throw new ArgumentException("length must be greater than zero");
            double actualWidthDegreesPerHash = WidthDegrees(length);
            double actualHeightDegreesPerHash = HeightDegrees(length);

            LongSet hashes = new LongSet();
            To180(ref bottomRightLon);
            To180(ref topLeftLon);
            double diff = bottomRightLon - topLeftLon;
            double maxLon = topLeftLon + diff;

            for (double lat = bottomRightLat; lat <= topLeftLat; lat += actualHeightDegreesPerHash)
            {
                for (double lon = topLeftLon; lon <= maxLon; lon += actualWidthDegreesPerHash)
                {
                    hashes.add(EncodeHashToLong(lat, lon, length));
                }
            }
            // ensure have the borders covered
            for (double lat = bottomRightLat; lat <= topLeftLat; lat += actualHeightDegreesPerHash)
            {
                hashes.add(EncodeHashToLong(lat, maxLon, length));
            }
            for (double lon = topLeftLon; lon <= maxLon; lon += actualWidthDegreesPerHash)
            {
                hashes.add(EncodeHashToLong(topLeftLat, lon, length));
            }
            // ensure that the topRight corner is covered
            hashes.add(EncodeHashToLong(topLeftLat, maxLon, length));

            double areaDegrees = diff * (topLeftLat - bottomRightLat);
            double coverageAreaDegrees = hashes.count * WidthDegrees(length)
                    * HeightDegrees(length);
            double ratio = coverageAreaDegrees / areaDegrees;
            return new CoverageLongs(hashes.array, hashes.count, ratio);
        }

        /**
         * Array to cache hash height calculations.
         */
        private static Double[] hashHeightCache = new Double[MAX_HASH_LENGTH];

        /**
         * Returns height in degrees of all geohashes of length n. Results are
         * deterministic and cached to increase performance.
         * 
         * @param n
         * @return
         */
        public static double HeightDegrees(int n)
        {
            if (n > 0 && n <= MAX_HASH_LENGTH)
            {
                if (hashHeightCache[n - 1] == 0d)
                    hashHeightCache[n - 1] = CalculateHeightDegrees(n);
                return hashHeightCache[n - 1];
            }
            else
                return CalculateHeightDegrees(n);
        }

        /**
         * Returns the height in degrees of the region represented by a geohash of
         * length n.
         * 
         * @param n
         * @return
         */
        private static double CalculateHeightDegrees(int n)
        {
            double a;
            if (n % 2 == 0)
                a = 0;
            else
                a = -0.5;
            double result = 180 / Math.Pow(2, 2.5 * n + a);
            return result;
        }

        /**
         * Array to cache hash width calculations.
         */
        private static Double[] hashWidthCache = new Double[MAX_HASH_LENGTH];

        /**
         * Returns width in degrees of all geohashes of length n. Results are
         * deterministic and cached to increase performance (might be unnecessary,
         * have not benchmarked).
         * 
         * @param n
         * @return
         */
        public static double WidthDegrees(int n)
        {
            if (n > 0 && n <= MAX_HASH_LENGTH)
            {
                if (hashWidthCache[n - 1] == 0d)
                {
                    hashWidthCache[n - 1] = CalculateWidthDegrees(n);
                }
                return hashWidthCache[n - 1];
            }
            else
                return CalculateWidthDegrees(n);
        }

        /**
         * Returns the width in degrees of the region represented by a geohash of
         * length n.
         * 
         * @param n
         * @return
         */
        private static double CalculateWidthDegrees(int n)
        {
            double a;
            if (n % 2 == 0)
                a = -1;
            else
                a = -0.5;
            double result = 180 / Math.Pow(2, 2.5 * n + a);
            return result;
        }

        /**
         * <p>
         * Returns a string of lines of hashes to represent the relative positions
         * of hashes on a map. The grid is of height and width 2*size centred around
         * the given hash. Highlighted hashes are displayed in upper case. For
         * example, gridToString("dr",1,Collections.<string>emptySet()) returns:
         * </p>
         * 
         * <pre>
         * f0 f2 f8 
         * dp dr dx 
         * dn dq dw
         * </pre>
         * 
         * @param hash
         * @param size
         * @param highlightThese
         * @return
         */
        public static string GridAsString(string hash, int size, HashSet<string> highlightThese)
        {
            return GridAsString(hash, -size, -size, size, size, highlightThese);
        }

        /**
         * Returns a string of lines of hashes to represent the relative positions
         * of hashes on a map.
         * 
         * @param hash
         * @param fromRight
         *            top left of the grid in hashes to the right (can be negative).
         * @param fromBottom
         *            top left of the grid in hashes to the bottom (can be
         *            negative).
         * @param toRight
         *            bottom righth of the grid in hashes to the bottom (can be
         *            negative).
         * @param toBottom
         *            bottom right of the grid in hashes to the bottom (can be
         *            negative).
         * @return
         */
        public static string GridAsString(string hash, int fromRight, int fromBottom, int toRight, int toBottom)
        {
            return GridAsString(hash, fromRight, fromBottom, toRight, toBottom, new HashSet<string>());
        }

        /**
         * Returns a string of lines of hashes to represent the relative positions
         * of hashes on a map. Highlighted hashes are displayed in upper case. For
         * example, gridToString("dr",-1,-1,1,1,Sets.newHashSet("f2","f8")) returns:
         * </p>
         * 
         * <pre>
         * f0 F2 F8 
         * dp dr dx 
         * dn dq dw
         * </pre>
         * 
         * @param hash
         * @param fromRight
         *            top left of the grid in hashes to the right (can be negative).
         * @param fromBottom
         *            top left of the grid in hashes to the bottom (can be
         *            negative).
         * @param toRight
         *            bottom righth of the grid in hashes to the bottom (can be
         *            negative).
         * @param toBottom
         *            bottom right of the grid in hashes to the bottom (can be
         *            negative).
         * @param highlightThese
         * @return
         */
        public static string GridAsString(string hash, int fromRight,
                int fromBottom, int toRight, int toBottom,
                HashSet<string> highlightThese)
        {
            StringBuilder s = new StringBuilder();
            for (int bottom = fromBottom; bottom <= toBottom; bottom++)
            {
                for (int right = fromRight; right <= toRight; right++)
                {
                    string h = AdjacentHash(hash, Direction.RIGHT, right);
                    h = AdjacentHash(h, Direction.BOTTOM, bottom);
                    if (highlightThese.Contains(h))
                        h = h.ToUpper();
                    s.Append(h).Append(" ");
                }
                s.Append("\n");
            }
            return s.ToString();
        }
    }
}