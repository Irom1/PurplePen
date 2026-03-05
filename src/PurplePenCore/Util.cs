using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PurplePen
{
    public class Util
    {
        // Get a bit from a uint. The bit number is interpreted mod 32.
        public static bool GetBit(uint u, int bitNumber)
        {
            return (u & (1 << (bitNumber & 0x1F))) != 0;
        }

        // Set a bit from a uint. The bit number is interpreted mod 32.
        public static uint SetBit(uint u, int bitNumber, bool newValue)
        {
            if (newValue)
                return u | (1U << (bitNumber & 0x1F));
            else
                return u & ~(1U << (bitNumber & 0x1F));
        }

        // Are two arrays equal
        public static bool ArrayEquals<T>(T[] a1, T[] a2)
        {
            if (a1 == null)
                return a2 == null;
            if (a2 == null)
                return a1 == null;

            if (a1.Length != a2.Length)
                return false;

            for (int i = 0; i < a1.Length; ++i)
                if (!object.Equals(a1[i], a2[i]))
                    return false;

            return true;
        }

        public static bool EqualArrays<T>(T[] a1, T[] a2)
        {
            if (a1 == null)
                return (a2 == null);
            else if (a2 == null)
                return (a1 == null);
            else {
                if (a1.Length != a2.Length)
                    return false;

                for (int i = 0; i < a1.Length; ++i) {
                    if (!a1[i].Equals(a2[i]))
                        return false;
                }
            }

            return true;
        }


        // Get hash code of array.
        public static int ArrayHashCode<T>(T[] a)
        {
            if (a == null)
                return 98112;
            else {
                int hash = 991137;
                for (int i = 0; i < a.Length; ++i)
                    hash = hash * 327 + a[i].GetHashCode();
                return hash;
            }
        }

        // Clone an array and its elemenets.
        public static T[] CloneArrayAndElements<T>(T[] a)
            where T : ICloneable
        {
            if (a == null)
                return null;

            T[] newArray = new T[a.Length];
            for (int i = 0; i < a.Length; ++i) {
                newArray[i] = (T)a[i].Clone();
            }

            return newArray;
        }

        // Clone a dictionary and its elements.
        public static Dictionary<K, V> CloneDictionary<K, V>(Dictionary<K, V> dict)
        {
            Dictionary<K, V> newDict = new Dictionary<K, V>(dict.Count);

            foreach (KeyValuePair<K, V> pair in dict) {
                K key = pair.Key;
                V value = pair.Value;

                ICloneable cloneableKey = key as ICloneable;
                if (cloneableKey != null) {
                    key = (K)cloneableKey.Clone();
                }

                ICloneable cloneableValue = value as ICloneable;
                if (cloneableValue != null) {
                    value = (V)cloneableValue.Clone();
                }

                newDict.Add(key, value);
            }

            return newDict;
        }

        // Copy a dictionary, so changes to the source no longer affect the result.
        public static Dictionary<K, V> CopyDictionary<K, V>(Dictionary<K, V> source)
        {
            if (source == null)
                return null;

            Dictionary<K, V> result = new Dictionary<K, V>();

            foreach (KeyValuePair<K, V> pair in source) {
                result.Add(pair.Key, pair.Value);
            }

            return result;
        }


        public static long Factorial(int n)
        {
            long result = 1;
            for (int i = 2; i <= n; ++i) {
                result *= i;
            }
            return result;
        }

        public static void Swap<T>(ref T a, ref T b)
        {
            T temp = a;
            a = b;
            b = temp;
        }

        public static bool IsInteger(string s)
        {
            int result;
            return int.TryParse(s, NumberStyles.None, null, out result);
        }

        // Compare two codes and sort them. Integers sort in integer
        // order before strings in string order.
        public static int CompareCodes(string code1, string code2)
        {
            // Null sorts first.
            if (code1 == null && code2 == null)
                return 0;
            else if (code1 == null)
                return -1;
            else if (code2 == null)
                return 1;

            bool isInt1, isInt2;

            isInt1 = IsInteger(code1);
            isInt2 = IsInteger(code2);

            if (isInt1 && !isInt2)
                return -1;
            else if (!isInt1 && isInt2)
                return 1;
            else if (isInt1 && isInt2)
                return int.Parse(code1).CompareTo(int.Parse(code2));
            else if (!isInt1 && !isInt2)
                return string.Compare(code1, code2, StringComparison.CurrentCulture);

            return 0;  // can't get here.
        }

        // Determine if the current culture uses metric. This is based on the CultureInfo.CurrentCulture, 
        // NOT the RegionInfo.CurrentRegion, for compatibility for what we always did (and I think it's the 
        // right thing).
        public static bool IsCurrentCultureMetric()
        {
            RegionInfo regionInfo = new RegionInfo(Thread.CurrentThread.CurrentCulture.Name);
            return regionInfo.IsMetric;
        }

        // Get text describing a distance. The input is in hundreths of an inch.
        public static string GetDistanceText(int distance, bool addUnits = true)
        {
            string result;
            if (Util.IsCurrentCultureMetric()) {
                result = (distance * 25.4 / 100.0).ToString("0");
                if (addUnits)
                    result += "mm";
            }
            else {
                result = (distance / 100.0).ToString("0.##");
                if (addUnits)
                    result += "\"";
            }

            return result;
        }

        // Get decimal for a distance.
        public static decimal GetDistanceValue(int distance)
        {
            if (Util.IsCurrentCultureMetric()) {
                return ((decimal)distance * 25.4M / 100.0M);
            }
            else {
                return ((decimal)distance / 100.0M);
            }
        }

        // Get distance in hundredth of an inch from a decimal.
        public static int GetDistanceFromValue(decimal value)
        {
            if (Util.IsCurrentCultureMetric()) {
                return (int)Math.Round(value * 100.0M / 25.4M);
            }
            else {
                return (int)Math.Round(value * 100.0M);
            }
        }

        // Get length in km, handling possible range and rounding.
        public static string GetLengthInKm(float minLenInMeters, float maxLenInMeters, int decimalPlaces, bool addKmSuffix = true)
        {
            double min = Math.Round(minLenInMeters / 1000.0, decimalPlaces, MidpointRounding.AwayFromZero);
            double max = Math.Round(maxLenInMeters / 1000.0, decimalPlaces, MidpointRounding.AwayFromZero);
            string formatStr = "{0:0." + new string('0', decimalPlaces) + "}";
            string minStr = String.Format(formatStr, min);
            string maxStr = String.Format(formatStr, max);

            string suffix = addKmSuffix ? " km" : "";
            if (minStr == maxStr)
                return minStr + suffix;
            else
                return minStr + "\u2013" + maxStr + suffix;
        }

        public static string RangeIfNeeded(int n1, int n2)
        {
            if (n1 == n2)
                return n1.ToString();
            else
                return n1.ToString() + "\u2013" + n2.ToString();
        }

    }
}
