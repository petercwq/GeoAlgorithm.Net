using System;
using System.Collections.Generic;

namespace GeoAlgorithm.Net.GeoHash
{
    /**
     * Conversion methods between long values and geohash-style base 32 encoding.
     * 
     * @author wq
     * 
     */
    public static class Base32
    {
        /**
         * The characters used for encoding base 32 strings.
         */
        private readonly static char[] characters = { '0', '1', '2', '3', '4', '5',
            '6', '7', '8', '9', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'j', 'k',
            'm', 'n', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' };

        /**
         * Used for lookup of index of characters in the above array.
         */
        private readonly static Dictionary<char, int> characterIndexes = new Dictionary<char, int>();

        static Base32()
        {
            for (int i = 0; i < characters.Length; i++)
                characterIndexes.Add(characters[i], i);
        }


        /**
         * Returns the base 32 encoding of the given length from a {@link Long}
         * geohash.
         * 
         * @param i
         *            the geohash
         * @param length
         *            the length of the returned hash
         * @return the string geohash
         */
        public static String EncodeBase32(long i, int length)
        {
            char[] buf = new char[65];
            int charPos = 64;
            bool negative = (i < 0);
            if (!negative)
                i = -i;
            while (i <= -32)
            {
                buf[charPos--] = characters[(int)(-(i % 32))];
                i /= 32;
            }
            buf[charPos] = characters[(int)(-i)];
            String result = new String(buf, charPos, (65 - charPos)).PadLeft(length, '0');
            if (negative)
                return "-" + result;
            else
                return result;
        }

        /**
         * Returns the base 32 encoding of length {@link GeoHash#MAX_HASH_LENGTH}
         * from a {@link Long} geohash.
         * 
         * @param i
         *            the geohash
         * @return the base32 geohash
         */
        public static String EncodeBase32(long i)
        {
            return EncodeBase32(i, GeoHash2.MAX_HASH_LENGTH);
        }

        /**
         * Returns the conversion of a base32 geohash to a long.
         * 
         * @param hash
         *            geohash as a string
         * @return long representation of hash
         */
        public static long DecodeBase32(String hash)
        {
            bool isNegative = hash.StartsWith("-");
            int startIndex = isNegative ? 1 : 0;
            long b = 1;
            long result = 0;
            for (int i = hash.Length - 1; i >= startIndex; i--)
            {
                int j = GetCharIndex(hash[i]);
                result = result + b * j;
                b = b * 32;
            }
            if (isNegative)
                result *= -1;
            return result;
        }

        /**
         * Returns the index in the digits array of the character ch. Throws an
         * {@link IllegalArgumentException} if the character is not found in the
         * array.
         * 
         * @param ch
         *            character to obtain index for
         * @return index of ch character in characterIndexes map.
         */
        // @VisibleForTesting
        static int GetCharIndex(char ch)
        {
            if (!characterIndexes.ContainsKey(ch))
                throw new ArgumentException("not a base32 character: " + ch);
            else
                return characterIndexes[ch];
        }

        ///**
        // * Pad left with zeros to desired string length.
        // * 
        // * @param s
        // *            string to pad
        // * @param length
        // * @return padded string with left zeros.
        // */
        //// @VisibleForTesting
        //static String PadLeftWithZerosToLength(String s, int length)
        //{
        //    if (s.Length < length)
        //    {
        //        int count = length - s.Length;
        //        StringBuilder b = new StringBuilder();
        //        for (int i = 0; i < count; i++)
        //            b.Append('0');
        //        b.Append(s);
        //        return b.ToString();
        //    }
        //    else
        //        return s;
        //}
    }
}
