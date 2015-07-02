
namespace GeoAlgorithm.Net.GeoHash
{
    public class LatLong
    {
        /**
         * Constructor.
         * 
         * @param lat
         * @param lon
         */
        public LatLong(double lat, double lon)
        {
            Lat = lat;
            Lon = lon;
        }

        /**
         * Returns the latitude in decimal degrees.
         * 
         * @return
         */
        public double Lat { get; private set; }

        /**
         * Returns the longitude in decimal degrees.
         * 
         * @return
         */
        public double Lon { get; private set; }

        /**
         * Returns a new {@link LatLong} object with lat, lon increased by deltaLat,
         * deltaLon.
         * 
         * @param deltaLat
         * @param deltaLon
         * @return
         */
        public LatLong Add(double deltaLat, double deltaLon)
        {
            return new LatLong(Lat + deltaLat, Lon + deltaLon);
        }
    }

}
