using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace NJsonApi.Test.TestModel
{
    internal class Article
    {
        public int Id { get; set; }
        public string Tittle { get; set; }
        public List<int> YearsPublished = new List<int>();
        public Author Author = new Author();
        public IList<Comment> Comments = new List<Comment>();
    }
}
