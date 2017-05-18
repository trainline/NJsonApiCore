using System;
using System.Collections.Generic;
using System.Linq;

namespace NJsonApi.Web.MVC5.HelloWorld.Models
{
    /// <summary>
    /// Primitive backing store for persistence.
    /// </summary>
    public static class StaticPersistentStore
    {
        private static int currentId { get; set; }

        public static List<Article> Articles { get; set; }

        public static List<Person> People { get; set; }

        public static List<Comment> Comments { get; set; }

        static StaticPersistentStore()
        {
            currentId = 1;

            Articles = new List<Article>();
            People = new List<Person>();
            Comments = new List<Comment>();

            var article1 = new Article("JSON API paints my bikeshed!");
            var article2 = new Article("JSON API makes the tea!");

            var person1 = new Person("Dan", "Gebhardt", "dgeb");
            person1.GetMetaData().Add("person created", DateTime.Now);
            var person2 = new Person("Rob", "Lang", "brainwipe");

            var comment1 = new Comment("First!");
            var comment2 = new Comment("I like XML Better");
            var comment3 = new Comment("First! More");
            var comment4 = new Comment("I like XML Better More");

            article1.Author = person1;
            article1.Comments.Add(comment1);
            article1.Comments.Add(comment2);
            article1.MoreComments.Add(comment3);
            article1.MoreComments.Add(comment4);
            article1.GetMetaData().Add("article created", DateTime.Now);
            article1.SingleTag = new Tag { Key = "tag key", Value = "single tag value" };
            article1.MoreTags.Add(new Tag { Key = "tag key (more)", Value = "tag value 1" });
            article1.MoreTags.Add(new Tag { Key = "tag key (more)", Value = "tag value 2" });
            article1.PublishedInYears.Add(1987);
            article1.PublishedInYears.Add(2001);
            article1.PublishedInYears.Add(2005);
            Articles.Add(article1);

            article2.Author = person2;
            Articles.Add(article2);

            People.Add(person1);
            People.Add(person2);

            comment1.Author = person1;
            comment2.Author = person2;

            Comments.Add(comment1);
            Comments.Add(comment2);
            Comments.Add(comment3);
            Comments.Add(comment4);
        }

        public static int GetNextId()
        {
            return currentId++;
        }
    }
}