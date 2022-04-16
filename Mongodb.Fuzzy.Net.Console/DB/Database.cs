using Mongodb.Fuzzy.Net.Console.Models;
using Mongodb.Fuzzy.Net.Console.Util;
using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Mongodb.Fuzzy.Net.Console.DB
{
    public static class Database
    {
        private static IMongoCollection<Movie> movieCollection;
        private static IMongoCollection<SearchableKeyword> searchableCollection;

        public static void Initialize()
        {
            var mongoClient = new MongoClient("mongodb://localhost:27017/");
            var db = mongoClient.GetDatabase("FuzzyMovies");
            movieCollection = db.GetCollection<Movie>(nameof(Movie));
            searchableCollection = db.GetCollection<SearchableKeyword>(nameof(SearchableKeyword));
            CreateSearchableIndex();
            SeedDatabase();
        }

        private static void CreateSearchableIndex()
        {
            var indexModel = new CreateIndexModel<SearchableKeyword>(
                    Builders<SearchableKeyword>.IndexKeys.Ascending(x => x.PhoneticKeyword),
                    new CreateIndexOptions<SearchableKeyword> { Name = nameof(SearchableKeyword.PhoneticKeyword), Unique = true });

            searchableCollection.Indexes.CreateOne(indexModel);
        }

        private static void SeedDatabase()
        {
            if (DatabaseSeeded())
            {
                return;
            }

            var moviesData = File.ReadAllText("./moviesData.json");
            var movies = JsonConvert.DeserializeObject<Movie[]>(moviesData).Where(x => !string.IsNullOrEmpty(x.Fullplot));
            var searchableKeywords = GenerateSearchableKeywords(movies);

            movieCollection.InsertMany(movies);
            SaveSearchableKeywords(searchableKeywords);
        }

        private static IEnumerable<SearchableKeyword> GenerateSearchableKeywords(IEnumerable<Movie> movies)
        {
            foreach (var movie in movies)
            {
                var phoneticKeywordPairs = PhoneticHashGenerator.ExtractPhoneticKeywordsPair(movie.Fullplot.ToLower());

                foreach (var phoneticKeywordPair in phoneticKeywordPairs)
                {
                    yield return new SearchableKeyword
                    {
                        MovieIds = new List<string> { movie.Id },
                        OriginalKeywords = new List<string> { phoneticKeywordPair.Key },
                        PhoneticKeyword = phoneticKeywordPair.Value
                    };
                }
            }
        }

        private static void SaveSearchableKeywords(IEnumerable<SearchableKeyword> searchableKeywords)
        {
            foreach (var searchableKeyword in searchableKeywords)
            {
                var updateDefinition = Builders<SearchableKeyword>.Update
                    .AddToSet(x => x.OriginalKeywords, searchableKeyword.OriginalKeywords[0])
                    .AddToSet(x => x.MovieIds, searchableKeyword.MovieIds[0]);
                searchableCollection.UpdateOne(x => x.PhoneticKeyword == searchableKeyword.PhoneticKeyword, updateDefinition, new UpdateOptions { IsUpsert = true });
            }
        }

        public static IEnumerable<Movie> FuzzySearch(string term)
        {
            var phoneticKeywordPairs = PhoneticHashGenerator.ExtractPhoneticKeywordsPair(term.ToLower());
            var keywords = phoneticKeywordPairs.Select(x => x.Value);
            var results = searchableCollection
                .Find(x => keywords.Contains(x.PhoneticKeyword))
                .ToList();

            foreach (var result in results)
            {
                var phoneticKeyPair = phoneticKeywordPairs.FirstOrDefault(x => x.Value == result.PhoneticKeyword);
                var scores = result.OriginalKeywords.Select(x => NormailzedLevenshteinCalculator.Score(x, phoneticKeyPair.Key));
                result.Rank = scores.Max();
            }

            var accepetedResults = results.Where(x => x.Rank > 70).ToList();
            var movieIds = accepetedResults
                .SelectMany(x => x.MovieIds)
                .GroupBy(x => x)
                .OrderByDescending(x => x.Count())
                .Take(3)
                .Select(x => x.Key)
                .ToList();

            return movieCollection
                .Find(x => movieIds.Contains(x.Id))
                .ToList()
                .OrderBy(x => movieIds.IndexOf(x.Id));
        }

        private static bool DatabaseSeeded()
        {
            return movieCollection.Find(_ => true).FirstOrDefault() != null && searchableCollection.Find(_ => true).FirstOrDefault() != null;
        }
    }
}
