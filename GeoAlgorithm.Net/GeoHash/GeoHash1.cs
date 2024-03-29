﻿//  Weiqing Chen <kevincwq@gmail.com>
//
//  Copyright (c) 2015 Weiqing Chen

using System;

namespace GeoAlgorithm.Net.GeoHash
{
    /// <Surmmary>
    /// Geohash is a latitude/longitude geocode system invented by Gustavo Niemeyer when writing
    /// the web service at geohash.org, and put into the public domain. It is a hierarchical spatial
    /// data structure which subdivides space into buckets of grid shape.
    /// http://en.wikipedia.org/wiki/Geohash
    /// http://geohash.org/
    /// </Summary>
    /// <example>
    /// In rectangular area (except "faultlines"), points can be requested by single parameter: 
    /// SELECT * FROM myPoints WHERE geohash >= u85 AND geohash <= u8x
    /// </example>
    public static class Geohash1
    {
        #region Direction enum

        public enum Direction
        {
            Top = 0,
            Right = 1,
            Bottom = 2,
            Left = 3
        }

        #endregion

        private const string Base32 = "0123456789bcdefghjkmnpqrstuvwxyz";
        private static readonly int[] Bits = new[] { 16, 8, 4, 2, 1 };

        private static readonly string[][] Neighbors = {
                                                           new[]{
                                                               "p0r21436x8zb9dcf5h7kjnmqesgutwvy", // Top
                                                               "bc01fg45238967deuvhjyznpkmstqrwx", // Right
                                                               "14365h7k9dcfesgujnmqp0r2twvyx8zb", // Bottom
                                                               "238967debc01fg45kmstqrwxuvhjyznp", // Left
                                                               },
                                                            new[]{
                                                                "bc01fg45238967deuvhjyznpkmstqrwx", // Top
                                                                "p0r21436x8zb9dcf5h7kjnmqesgutwvy", // Right
                                                                "238967debc01fg45kmstqrwxuvhjyznp", // Bottom
                                                                "14365h7k9dcfesgujnmqp0r2twvyx8zb", // Left
                                                                }
                                                       };

        private static readonly string[][] Borders = {
                                                         new[] {"prxz", "bcfguvyz", "028b", "0145hjnp"},
                                                         new[] {"bcfguvyz", "prxz", "0145hjnp", "028b"}
                                                     };

        public static String CalculateAdjacent(String hash, Direction direction)
        {
            hash = hash.ToLower();

            char lastChr = hash[hash.Length - 1];
            int type = hash.Length % 2;
            var dir = (int)direction;
            string nHash = hash.Substring(0, hash.Length - 1);

            if (Borders[type][dir].IndexOf(lastChr) != -1)
            {
                nHash = CalculateAdjacent(nHash, (Direction)dir);
            }
            return nHash + Base32[Neighbors[type][dir].IndexOf(lastChr)];
        }

        private static void RefineInterval(ref double[] interval, int cd, int mask)
        {
            if ((cd & mask) != 0)
            {
                interval[0] = (interval[0] + interval[1]) / 2;
            }
            else
            {
                interval[1] = (interval[0] + interval[1]) / 2;
            }
        }

        public static double[] Decode(String geohash)
        {
            bool even = true;
            double[] lat = { -90.0, 90.0 };
            double[] lon = { -180.0, 180.0 };

            foreach (char c in geohash)
            {
                int cd = Base32.IndexOf(c);
                for (int j = 0; j < 5; j++)
                {
                    int mask = Bits[j];
                    if (even)
                    {
                        RefineInterval(ref lon, cd, mask);
                    }
                    else
                    {
                        RefineInterval(ref lat, cd, mask);
                    }
                    even = !even;
                }
            }

            return new[] { (lat[0] + lat[1]) / 2, (lon[0] + lon[1]) / 2 };
        }

        public static String Encode(double latitude, double longitude, int precision = 12)
        {
            bool even = true;
            int bit = 0;
            int ch = 0;
            string geohash = "";

            double[] lat = { -90.0, 90.0 };
            double[] lon = { -180.0, 180.0 };

            if (precision < 1 || precision > 20)
                precision = 12;

            while (geohash.Length < precision)
            {
                double mid;

                if (even)
                {
                    mid = (lon[0] + lon[1]) / 2;
                    if (longitude > mid)
                    {
                        ch |= Bits[bit];
                        lon[0] = mid;
                    }
                    else
                        lon[1] = mid;
                }
                else
                {
                    mid = (lat[0] + lat[1]) / 2;
                    if (latitude > mid)
                    {
                        ch |= Bits[bit];
                        lat[0] = mid;
                    }
                    else
                        lat[1] = mid;
                }

                even = !even;
                if (bit < 4)
                    bit++;
                else
                {
                    geohash += Base32[ch];
                    bit = 0;
                    ch = 0;
                }
            }
            return geohash;
        }
    }
}
