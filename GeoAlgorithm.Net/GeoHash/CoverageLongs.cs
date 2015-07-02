using System;

namespace GeoAlgorithm.Net.GeoHash
{
    /**
     * A set of hashes repesented by longs and a measure of how well those hashes cover a region.
     * Immutable.
     * 
     * @author wq
     * 
     */
    public class CoverageLongs
    {

        /**
         * The hashes providing the coverage.
         */
        private readonly long[] hashes;

        private readonly int count;

        /**
         * How well the coverage is covered by the hashes. Will be >=1. Closer to 1
         * the close the coverage is to the region in question.
         */
        private readonly double ratio;

        /**
         * Constructor.
         * 
         * @param hashes
         *            set of hashes comprising the coverage
         * @param ratio
         *            ratio of area of hashes to the area of target region
         */
        public CoverageLongs(long[] hashes, int count, double ratio)
        {
            this.hashes = hashes;
            this.count = count;
            this.ratio = ratio;
        }

        /**
         * Returns the hashes which are expected to be all of the same length.
         * 
         * @return set of hashes
         */
        public long[] Hashes
        {
            get
            {
                long[] res = new long[count];
                Array.Copy(hashes, 0, res, 0, count);
                return res;
            }
        }

        /**
         * Returns the measure of how well the hashes cover a region. The ratio is
         * the total area of hashes divided by the area of the bounding box in
         * degrees squared. The closer the ratio is to 1 the better the more closely
         * the hashes approximate the bounding box.
         * 
         * @return ratio of area of hashes to area of target region.
         */
        public double Ratio
        {
            get
            {
                return ratio;
            }
        }

        /**
         * Returns the length in characters of the first hash returned by an
         * iterator on the hash set. All hashes should be of the same length in this
         * coverage.
         * 
         * @return length of the hash
         */
        public int HashLength
        {
            get
            {
                if (count == 0)
                    return 0;
                else
                    return (int)(hashes[0] & 0x0f);
            }
        }

        public int Count
        {
            get { return count; }
        }

        public override String ToString()
        {
            return "Coverage [hashes=" + Hashes + ", ratio=" + ratio + "]";
        }
    }
}
