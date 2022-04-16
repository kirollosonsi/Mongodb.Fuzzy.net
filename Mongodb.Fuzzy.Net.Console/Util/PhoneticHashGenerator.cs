using Phonix;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Mongodb.Fuzzy.Net.Console.Util
{
    public class PhoneticHashGenerator
    {
        private static DoubleMetaphone doubleMetaphone = new DoubleMetaphone();
        private const string WordsRegExpression = @"\p{L}{2,}";

        public static string GeneratePhoneticHash(string keyword)
        {
            if (string.IsNullOrEmpty(keyword))
            {
                return null;
            }

            return doubleMetaphone.BuildKeys(keyword)[0];
        }

        public static IEnumerable<KeyValuePair<string, string>> ExtractPhoneticKeywordsPair(string value)
        {
            var keywords = Regex.Matches(value, WordsRegExpression)
                .Cast<Match>()
                .Select(m => m.Value)
                .Where(x => !string.IsNullOrEmpty(x))
                .Distinct();

            foreach (var keyword in keywords)
            {
                yield return new KeyValuePair<string, string>(keyword, GeneratePhoneticHash(keyword));
            }
        }
    }
}
