using System;
using GeoAlgorithm.Net.GeoHash;

namespace GeoHashDemoApp
{
    class Program
    {
        static void Main(string[] args)
        {
            const double testLat = 40.7571397;
            const double testLong = -73.9891705;
            const int precision = 13;

            // Calculate hash with full precision
            string hash = Geohash.Encode(testLat, testLong, precision);

            // Print out the hash for a range of precision
            for (int i = 1; i <= precision; i++)
            {
                var code = Geohash.Encode(testLat, testLong, i);
                var ll = Geohash.Decode(code);

                Console.WriteLine("precision {2}: {0}, {1} \t->\t {3} \t->\t {4}, {5}", testLat, testLong, i, code, ll[0], ll[1]);
            }

            // Print neighbours
            Console.WriteLine("{0} \t: {1}", "T", Geohash.CalculateAdjacent(hash, Geohash.Direction.Top));
            Console.WriteLine("{0} \t: {1}", "L", Geohash.CalculateAdjacent(hash, Geohash.Direction.Left));
            Console.WriteLine("{0} \t: {1}", "R", Geohash.CalculateAdjacent(hash, Geohash.Direction.Right));
            Console.WriteLine("{0} \t: {1}", "B", Geohash.CalculateAdjacent(hash, Geohash.Direction.Bottom));
        }
    }
}
