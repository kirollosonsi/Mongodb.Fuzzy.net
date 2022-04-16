using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace Mongodb.Fuzzy.Net.Console.Models
{
    public class SearchableKeyword
    {
        public ObjectId Id { get; set; }
        public List<string> MovieIds { get; set; }
        public string PhoneticKeyword { get; set; }
        public List<string> OriginalKeywords { get; set; }

        [BsonIgnore]
        public double Rank { get; set; }

        public SearchableKeyword()
        {
            Rank = 0;
            MovieIds = new List<string>();
            OriginalKeywords = new List<string>();
        }
    }
}
