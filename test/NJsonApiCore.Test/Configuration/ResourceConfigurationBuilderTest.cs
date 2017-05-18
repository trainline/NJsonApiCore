using Newtonsoft.Json;
using NJsonApi.Test.TestControllers;
using NJsonApi.Test.TestModel;
using System.Collections.Generic;
using Xunit;

namespace NJsonApi.Test.Configuration
{
    public class ResourceConfigurationBuilderTest
    {
        private ConfigurationBuilder configurationBuilder;

        public void Setup()
        {
            configurationBuilder = new ConfigurationBuilder();
        }

        [Fact]
        public void TestWithResourceType()
        {
            //Arrange
            Setup();

            string resourceType = typeof(Author).Name;
            var classUnderTest = configurationBuilder.Resource<Author, AuthorsController>();

            //Act
            classUnderTest.WithResourceType(resourceType);

            //Assert
            Assert.Equal(classUnderTest.BuiltResourceMapping.ResourceType, resourceType);
        }

        [Fact]
        public void TestWithResourceTypeForMultipleTypes()
        {
            //Arrange
            Setup();

            string resourceTypeAuthor = typeof(Author).Name;
            string resourceTypePost = typeof(Post).Name;
            var resourceConfigurationForAuthor = configurationBuilder.Resource<Author, AuthorsController>();
            var resourceConfigurationForPost = configurationBuilder.Resource<Post, PostsController>();

            //Act
            resourceConfigurationForAuthor.WithResourceType(resourceTypeAuthor);
            resourceConfigurationForPost.WithResourceType(resourceTypePost);

            //Assert
            Assert.Equal(resourceConfigurationForAuthor
                .BuiltResourceMapping
                .ResourceType, resourceTypeAuthor);

            Assert.Equal(resourceConfigurationForPost
                .BuiltResourceMapping
                .ResourceType, resourceTypePost);
        }

        [Fact]
        public void TestWithIdSelector()
        {
            //Arrange
            Setup();

            var resourceConfigurationForAuthor = configurationBuilder.Resource<Author, AuthorsController>();
            const int authorId = 5;
            var author = new Author() { Id = authorId };

            //Act
            resourceConfigurationForAuthor.WithIdSelector(a => a.Id);

            //Assert
            var result = (int)resourceConfigurationForAuthor.BuiltResourceMapping.IdGetter.Invoke(author);
            Assert.Equal(result, authorId);
        }

        [Fact]
        public void TestWithIdSelectorForMultipleTypes()
        {
            //Arrange
            Setup();

            var resourceConfigurationForAuthor = configurationBuilder.Resource<Author, AuthorsController>();
            var resourceConfigurationForPost = configurationBuilder.Resource<Post, PostsController>();
            const int authorId = 5;
            const int postId = 6;
            var author = new Author() { Id = authorId };
            var post = new Post() { Id = postId };

            //Act
            resourceConfigurationForAuthor.WithIdSelector(a => a.Id);
            resourceConfigurationForPost.WithIdSelector(p => p.Id);

            //Assert
            var authorResult = (int)resourceConfigurationForAuthor.BuiltResourceMapping.IdGetter.Invoke(author);
            Assert.Equal(authorResult, authorId);
            var postResult = (int)resourceConfigurationForPost.BuiltResourceMapping.IdGetter.Invoke(post);
            Assert.Equal(postResult, postId);
        }

        [Fact]
        public void TestWithSimpleProperty()
        {
            //Arrange
            Setup();

            const int authorId = 5;
            var author = new Author() { Id = authorId };
            var resourceConfigurationForAuthor = configurationBuilder.Resource<Author, AuthorsController>();

            //Act
            resourceConfigurationForAuthor.WithSimpleProperty(a => a.Name);

            //Assert
            Assert.Equal(resourceConfigurationForAuthor.BuiltResourceMapping.PropertyGetters.Count, 1);
            Assert.Equal(resourceConfigurationForAuthor.BuiltResourceMapping.PropertySetters.Count, 1);
            Assert.Null(resourceConfigurationForAuthor.BuiltResourceMapping.IdGetter);
            Assert.Contains("author", resourceConfigurationForAuthor.BuiltResourceMapping.ResourceType);
        }

        [Fact]
        public void TestWithSimplePropertyWithIdentity()
        {
            //Arrange & Act
            Setup();

            var resourceConfigurationForAuthor = configurationBuilder
                .Resource<Author, AuthorsController>()
                .WithSimpleProperty(a => a.Name)
                .WithIdSelector(a => a.Id);
            //Assert
            AssertResourceConfigurationHasValuesForWithSimpleProperty(resourceConfigurationForAuthor);
            Assert.Contains("author", resourceConfigurationForAuthor.BuiltResourceMapping.ResourceType);
        }

        [Fact]
        public void GIVEN_ModelWithSingleProperty_WHEN_BuildConfiguration_THEN_PropertyIsMapped()
        {
            //Arrange
            Setup();

            const int authorId = 5;
            const string authorName = "Valentin";
            var author = new Author() { Id = authorId, Name = authorName };

            //Act
            var resourceConfigurationForAuthor = configurationBuilder
                .Resource<Author, AuthorsController>()
                .WithSimpleProperty(a => a.Name)
                .WithIdSelector(a => a.Id);

            var resultForName = resourceConfigurationForAuthor.BuiltResourceMapping.PropertyGetters["name"].Invoke(author);
            var resultForId = resourceConfigurationForAuthor.BuiltResourceMapping.IdGetter.Invoke(author);

            //Assert
            Assert.Equal(resultForName, authorName);
            Assert.Equal(resultForId, authorId);
        }

        [Fact]
        public void GIVEN_ModelWithComplexObjectProperty_WHEN_BuildConfiguration_THEN_PropertyIsMapped_Getter()
        {
            //Arrange
            Setup();

            const int authorId = 5;
            const string authorName = "Valentin";
            var article = new Article() { Id = 1 };
            var author = new Author { Id = authorId, Name = authorName };
            article.Author = author;

            //Act
            var resourceConfigurationForArticle = configurationBuilder
                .Resource<Article, ArticlesController>()
                .WithSimpleProperty(a => a.Author)
                .WithIdSelector(a => a.Id);

            var resultForAuthor = resourceConfigurationForArticle.BuiltResourceMapping.PropertyGetters["author"].Invoke(article);

            //Assert
            Assert.Equal(authorId, ((Author)resultForAuthor).Id);
            Assert.Equal(authorName, ((Author)resultForAuthor).Name);
        }

        [Fact]
        public void GIVEN_ModelWithComplexObjectProperty_WHEN_BuildConfiguration_THEN_PropertyIsMapped_SetterAsObject()
        {
            //Arrange
            Setup();

            const int authorId = 5;
            const string authorName = "Valentin";
            var article = new Article() { Id = 1 };
            var author = new Author { Id = authorId, Name = authorName };

            //Act
            var resourceConfigurationForArticle = new ConfigurationBuilder()
                .Resource<Article, ArticlesController>()
                .WithSimpleProperty(a => a.Author)
                .WithIdSelector(a => a.Id);

            resourceConfigurationForArticle.BuiltResourceMapping.PropertySetters["author"].Invoke(article, author);

            //Assert
            Assert.Equal(authorId, article.Author.Id);
            Assert.Equal(authorName, article.Author.Name);
        }

        [Fact]
        public void GIVEN_ModelWithComplexObjectProperty_WHEN_BuildConfiguration_THEN_PropertyIsMapped_SetterAsJObject()
        {
            //Arrange
            Setup();

            const int authorId = 5;
            const string authorName = "Valentin";
            var article = new Article() { Id = 1 };
            var author = new Author { Id = authorId, Name = authorName };

            //Act
            var resourceConfigurationForArticle = new ConfigurationBuilder()
                .Resource<Article, ArticlesController>()
                .WithSimpleProperty(a => a.Author)
                .WithIdSelector(a => a.Id);

            resourceConfigurationForArticle.BuiltResourceMapping.PropertySetters["author"].Invoke(article, JsonConvert.DeserializeObject(JsonConvert.SerializeObject(author)));

            //Assert
            Assert.Equal(authorId, article.Author.Id);
            Assert.Equal(authorName, article.Author.Name);
        }

        [Fact]
        public void GIVEN_ModelWithComplexObjectCollection_WHEN_BuildConfiguration_THEN_PropertyIsMapped_Getter()
        {
            //Arrange
            Setup();

            var article = new Article() { Id = 1 };
            var comments = new List<Comment> { new Comment { Body = "body 1" }, new Comment { Body = "body 2" } };
            article.Comments = comments;

            //Act
            var resourceConfigurationForArticle = configurationBuilder
                .Resource<Article, ArticlesController>()
                .WithSimpleProperty(a => a.Comments)
                .WithIdSelector(a => a.Id);

            var resultForComments = (List<Comment>) resourceConfigurationForArticle.BuiltResourceMapping.PropertyGetters["comments"].Invoke(article);

            //Assert
            Assert.Equal(comments.Count, resultForComments.Count);
            Assert.Equal(comments[0].Body, resultForComments[0].Body);
            Assert.Equal(comments[1].Body, resultForComments[1].Body);
        }


        [Fact]
        public void GIVEN_ModelWithComplexObjectCollection_WHEN_BuildConfiguration_THEN_PropertyIsMapped_SetterAsList()
        {
            //Arrange
            Setup();

            var article = new Article() { Id = 1 };
            var comments = new List<Comment> { new Comment { Body = "body 1" }, new Comment { Body = "body 2" } };
            
            //Act
            var resourceConfigurationForArticle = configurationBuilder
                .Resource<Article, ArticlesController>()
                .WithSimpleProperty(a => a.Comments)
                .WithIdSelector(a => a.Id);

            resourceConfigurationForArticle.BuiltResourceMapping.PropertySetters["comments"].Invoke(article, comments);

            //Assert
            Assert.Equal(comments.Count, article.Comments.Count);
            Assert.Equal(comments[0].Body, article.Comments[0].Body);
            Assert.Equal(comments[1].Body, article.Comments[1].Body);
        }

        [Fact]
        public void GIVEN_ModelWithComplexObjectCollection_WHEN_BuildConfiguration_THEN_PropertyIsMapped_SetterAsJArray()
        {
            //Arrange
            Setup();

            var article = new Article() { Id = 1 };
            var comments = new List<Comment> { new Comment { Body = "body 1" }, new Comment { Body = "body 2" } };

            //Act
            var resourceConfigurationForArticle = configurationBuilder
                .Resource<Article, ArticlesController>()
                .WithSimpleProperty(a => a.Comments)
                .WithIdSelector(a => a.Id);

            resourceConfigurationForArticle.BuiltResourceMapping.PropertySetters["comments"].Invoke(article, JsonConvert.DeserializeObject(JsonConvert.SerializeObject(comments)));

            //Assert
            Assert.Equal(comments.Count, article.Comments.Count);
            Assert.Equal(comments[0].Body, article.Comments[0].Body);
            Assert.Equal(comments[1].Body, article.Comments[1].Body);
        }

        [Fact]
        public void GIVEN_ModelWithRelatedResourceProperty_WHEN_BuildConfiguration_THEN_PropertyIsMapped_Getter()
        {
            //Arrange
            Setup();

            const int authorId = 5;
            const string authorName = "Valentin";
            var article = new Article() { Id = 1 };
            var author = new Author { Id = authorId, Name = authorName };
            article.Author = author;

            //Act
            var resourceConfigurationForArticle = configurationBuilder
                .Resource<Article, ArticlesController>()
                .WithLinkedResource<Author>(a => a.Author)
                .WithIdSelector(a => a.Id);

            var resultForAuthor = (Author)resourceConfigurationForArticle
                .BuiltResourceMapping
                .Relationships
                .Find(r => r.RelationshipName == "author")
                .RelatedProperty
                .GetterDelegate
                .DynamicInvoke(article);

            //Assert
            Assert.Equal(authorId, ((Author)resultForAuthor).Id);
            Assert.Equal(authorName, ((Author)resultForAuthor).Name);
        }

        [Fact]
        public void GIVEN_ModelWithRelatedResourceProperty_WHEN_BuildConfiguration_THEN_PropertyIsMapped_Setter()
        {
            //Arrange
            Setup();

            const int authorId = 5;
            const string authorName = "Valentin";
            var article = new Article() { Id = 1 };
            var author = new Author { Id = authorId, Name = authorName };

            //Act
            var resourceConfigurationForArticle = new ConfigurationBuilder()
                .Resource<Article, ArticlesController>()
                .WithLinkedResource<Author>(a => a.Author)
                .WithIdSelector(a => a.Id);

            resourceConfigurationForArticle
                .BuiltResourceMapping
                .Relationships
                .Find(r => r.RelationshipName == "author")
                .RelatedProperty
                .SetterDelegate
                .DynamicInvoke(article, author);

            //Assert
            Assert.Equal(authorId, article.Author.Id);
            Assert.Equal(authorName, article.Author.Name);
        }

        [Fact]
        public void GIVEN_ModelWithRelatedResourceCollection_WHEN_BuildConfiguration_THEN_PropertyIsMapped_Getter()
        {
            //Arrange
            Setup();

            var article = new Article() { Id = 1 };
            var comments = new List<Comment> { new Comment { Body = "body 1" }, new Comment { Body = "body 2" } };
            article.Comments = comments;

            //Act
            var resourceConfigurationForArticle = configurationBuilder
                .Resource<Article, ArticlesController>()
                .WithLinkedResource<IList<Comment>>(a => a.Comments)
                .WithIdSelector(a => a.Id);

            var resultForComments = (List<Comment>)resourceConfigurationForArticle
                .BuiltResourceMapping
                .Relationships
                .Find(r => r.RelationshipName == "comments")
                .RelatedProperty
                .GetterDelegate
                .DynamicInvoke(article);

            //Assert
            Assert.Equal(comments.Count, resultForComments.Count);
            Assert.Equal(comments[0].Body, resultForComments[0].Body);
            Assert.Equal(comments[1].Body, resultForComments[1].Body);
        }


        [Fact]
        public void GIVEN_ModelWithRelatedResourceCollection_WHEN_BuildConfiguration_THEN_PropertyIsMapped_Setter()
        {
            //Arrange
            Setup();

            var article = new Article() { Id = 1 };
            var comments = new List<Comment> { new Comment { Body = "body 1" }, new Comment { Body = "body 2" } };

            //Act
            var resourceConfigurationForArticle = configurationBuilder
                .Resource<Article, ArticlesController>()
                .WithLinkedResource<IList<Comment>>(a => a.Comments)
                .WithIdSelector(a => a.Id);

            resourceConfigurationForArticle
                .BuiltResourceMapping
                .Relationships
                .Find(r => r.RelationshipName == "comments")
                .RelatedProperty
                .SetterDelegate
                .DynamicInvoke(article, comments);

            //Assert
            Assert.Equal(comments.Count, article.Comments.Count);
            Assert.Equal(comments[0].Body, article.Comments[0].Body);
            Assert.Equal(comments[1].Body, article.Comments[1].Body);
        }

        [Fact]
        public void GIVEN_MultipleTypes_WHEN_BuildConfiguration_THEN_TypesAreStoredInConfiguration()
        {
            //Arrange
            Setup();

            var authorId = 5;
            var authorName = "Valentin";
            var postId = 6;
            var postTitle = "The measure of a man";
            var postTitleModifed = "Modified";

            var author = new Author() { Id = authorId, Name = authorName };
            var post = new Post() { Id = postId, Title = postTitle };

            //Act
            var resourceConfigurationForPost = configurationBuilder
                .Resource<Post, PostsController>()
                .WithSimpleProperty(p => p.Title)
                .WithIdSelector(p => p.Id);

            var resourceConfigurationForAuthor = configurationBuilder
                .Resource<Author, AuthorsController>()
                .WithSimpleProperty(a => a.Name)
                .WithIdSelector(a => a.Id);

            //Assert
            var result = resourceConfigurationForAuthor.BuiltResourceMapping;
            AssertResourceConfigurationHasValuesForWithSimpleProperty(resourceConfigurationForAuthor);

            Assert.Contains("author", result.ResourceType);
            Assert.Equal(result.PropertyGetters["name"].Invoke(author), authorName);
            Assert.Equal(result.IdGetter.Invoke(author), authorId);

            AssertResourceConfigurationHasValuesForWithSimpleProperty(resourceConfigurationForPost);

            result = resourceConfigurationForPost.BuiltResourceMapping;
            Assert.Contains("post", result.ResourceType);
            Assert.Equal(result.PropertyGetters["title"].Invoke(post), postTitle);

            resourceConfigurationForPost.BuiltResourceMapping.PropertySetters["title"].Invoke(post, postTitleModifed);
            Assert.Equal(post.Title, postTitleModifed);
            Assert.Equal(result.PropertyGetters["title"].Invoke(post), postTitleModifed);
        }

        [Fact]
        public void IgnorePropertyTest()
        {
            //Arrange
            Setup();

            const int authorId = 5;
            var author = new Author() { Id = authorId };
            var resourceConfigurationForAuthor = configurationBuilder.Resource<Author, AuthorsController>();
            resourceConfigurationForAuthor.WithSimpleProperty(a => a.Name);

            var result = resourceConfigurationForAuthor.BuiltResourceMapping;

            // Assert initial
            Assert.Equal(result.PropertyGetters.Count, 1);
            Assert.Equal(result.PropertySetters.Count, 1);
            Assert.Null(result.IdGetter);
            Assert.Contains("author", resourceConfigurationForAuthor.BuiltResourceMapping.ResourceType);

            //Act
            resourceConfigurationForAuthor.IgnoreProperty(a => a.Name);

            //Assert
            Assert.Equal(result.PropertyGetters.Count, 0);
            Assert.Equal(result.PropertySetters.Count, 0);
            Assert.Null(result.IdGetter);
            Assert.Contains("author", resourceConfigurationForAuthor.BuiltResourceMapping.ResourceType);
        }

        [Fact]
        public void WithLinkedResourceTest()
        {
            //Arrange
            Setup();

            //Act
            var resourceConfigurationForPost = configurationBuilder
                .Resource<Post, PostsController>()
                .WithIdSelector(p => p.Id)
                .WithSimpleProperty(p => p.Title)
                .WithLinkedResource(p => p.Author);

            var result = resourceConfigurationForPost.BuiltResourceMapping;

            //Assert
            Assert.Equal(result.Relationships.Count, 1);
            Assert.Contains("author", result.Relationships[0].RelationshipName);
            Assert.Equal(result.Relationships[0].RelatedBaseType, typeof(Author));
            Assert.Null(result.Relationships[0].ResourceMapping);
        }

        private void AssertResourceConfigurationHasValuesForWithSimpleProperty(IResourceConfigurationBuilder resourceConfiguration)
        {
            var result = resourceConfiguration.BuiltResourceMapping;

            Assert.Equal(result.PropertyGetters.Count, 1);
            Assert.Equal(result.PropertySetters.Count, 1);
            Assert.NotNull(result.IdGetter);
        }
    }
}