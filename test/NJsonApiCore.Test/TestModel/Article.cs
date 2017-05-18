using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace NJsonApi.Test.TestModel
{
    internal class Article
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public List<int> YearsPublished { get; set; } = new List<int>();
        public Author Author { get; set; } = new Author();
        public IList<Comment> Comments { get; set; } = new List<Comment>();
    }
}
