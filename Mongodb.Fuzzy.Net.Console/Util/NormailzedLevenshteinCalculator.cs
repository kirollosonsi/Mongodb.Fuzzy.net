using FuzzySharp;

namespace Mongodb.Fuzzy.Net.Console.Util
{
    public static class NormailzedLevenshteinCalculator
    {
        public static double Score(string value1, string value2)
        {
            var weightedRatio = Fuzz.WeightedRatio(value1, value2);
            var tokenSetRatio = Fuzz.TokenSetRatio(value1, value2);

            return (weightedRatio + tokenSetRatio) / 2D;
        }
    }
}
