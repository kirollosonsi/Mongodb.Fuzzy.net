using Mongodb.Fuzzy.Net.Console.DB;

namespace Mongodb.Fuzzy.Net.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            Database.Initialize();

            //actual string: Phil and Ted Stoneman
            var example1 = Database.FuzzySearch("Td Phl Stonman");

            //actual string: Buster and Sybil exit
            var example2 = Database.FuzzySearch("Sbil ext Bustr");

            //actual string: family will endure great suffering in the conflict
            var example3 = Database.FuzzySearch("famly sffring cnflct");
        }
    }
}
