using System;

namespace Mongodb.Fuzzy.Net.Console.Models
{
    public class Movie
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Fullplot { get; set; }

        public Movie()
        {
            Id = Guid.NewGuid().ToString("N");
        }
    }
}
