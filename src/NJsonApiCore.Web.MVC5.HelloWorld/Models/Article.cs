using NJsonApi.Infrastructure;
using System.Collections.Generic;

namespace NJsonApi.Web.MVC5.HelloWorld.Models
{
    public class Article : MetaDataContainer
    {
        public Article()
        {
        }

        public Article(string title)
        {
            Comments = new List<Comment>();
            MoreComments = new List<Comment>();
            MoreTags = new List<Tag>();
            PublishedInYears = new List<int>();
            Id = StaticPersistentStore.GetNextId();
            Title = title;

        }

        public int Id { get; set; }

        // simple property type
        public string Title { get; set; }

        // a 1:1 related resource
        public Person Author { get; set; }

        // a 1:1 related resource of the same type as Author
        public Person Reviewer { get; set; }

        // a 1:n related resource
        public List<Comment> Comments { get; set; }

        // a 1:n related resource with the same type as Comments
        public List<Comment> MoreComments { get; set; }

        // complex type that is not a resource and serializes as an attribute
        public Tag SingleTag { get; set; }
        // an array of complex types that are not a resource and serialize as an attribute
        public List<Tag> MoreTags { get; set; }

        // an array of simple types that serializes as an attribute
        public List<int> PublishedInYears { get; set; }
    }
}