using NJsonApi.Infrastructure;
using NJsonApi.Test.TestModel;
using System;
using System.Collections.Generic;
using Xunit;
using Moq;
using System.Collections;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace NJsonApi.Test.Infrastructure
{
    public class DeltaTest
    {
        Mock<IConfiguration> _configuration;
        Mock<IResourceMapping> _mapping;

        public void Setup()
        {
            _configuration = new Mock<IConfiguration>(MockBehavior.Strict);
            _mapping = new Mock<IResourceMapping>(MockBehavior.Strict);

            _configuration.Setup(c => c.GetMapping(It.IsAny<Type>())).Returns(_mapping.Object);
            Dictionary<string, Action<object, object>> propertySetters = new Dictionary<string, Action<object, object>>();
            _mapping.SetupGet(m => m.PropertySetters).Returns(propertySetters);
            List<IRelationshipMapping> relationships = new List<IRelationshipMapping>();
            _mapping.SetupGet(m => m.Relationships).Returns(relationships);
        }

        [Fact]
        public void GIVEN_DeltaObject_WHEN_SetValue_THEN_ObjectPropertyValuesIsUpdated()
        {
            //Arrange
            Setup();

            var author = new Author();
            var classUnderTest = new Delta<Author>(_configuration.Object);

            classUnderTest.ObjectPropertyValues =
                new Dictionary<string, object>()
                {
                };

            //Act
            classUnderTest.SetValue(o => o.Name, "author name");

            //Assert
            Assert.Equal("author name", classUnderTest.ObjectPropertyValues["name"]);
        }

        [Fact]
        public void GIVEN_DeltaObject_WHEN_GetValue_THEN_ObjectPropertyValuesIsRead()
        {
            //Arrange
            Setup();

            var author = new Author();
            var classUnderTest = new Delta<Author>(_configuration.Object);

            classUnderTest.ObjectPropertyValues =
                new Dictionary<string, object>()
                {
                    { "name", "author name" }
                };

            //Act
            var result = classUnderTest.GetValue(o => o.Name);

            //Assert
            Assert.Equal("author name", result);
        }

        [Fact]
        public void GIVEN_IncompleteProperties_WHEN_DeltaApply_THEN_OnlyThoseSpecifiedApplied()
        {
            //Arrange
            Setup();
            _mapping.Object.PropertySetters.Add("id", (o, p) => { ((Author)o).Id = (int)p; });
            _mapping.Object.PropertySetters.Add("name", (o, p) => { ((Author)o).Name = (string)p; });
            _mapping.Object.PropertySetters.Add("dateTimeCreated", (o, p) => { ((Author)o).DateTimeCreated = (DateTime)p; });


            var author = new Author();
            var classUnderTest = new Delta<Author>(_configuration.Object);
            
            classUnderTest.ObjectPropertyValues =
                new Dictionary<string, object>()
                {
                    {"id", 1},
                    {"dateTimeCreated", new DateTime(2016,1,1)},
                };

            classUnderTest.Scan();

            //Act
            classUnderTest.ApplySimpleProperties(author);

            //Assert
            Assert.Equal(author.Id, 1);
            Assert.Equal(author.DateTimeCreated, new DateTime(2016, 1, 1));
            Assert.Null(author.Name);
        }

        [Fact]
        public void GIVEN_PropertiesAndFilteredOutSetters_WHEN_DeltaApply_THEN_OnlyThoseSpecifiedApplied()
        {
            //Arrange
            Setup();
            _mapping.Object.PropertySetters.Add("id", (o, p) => { ((Author)o).Id = (int)p; });
            _mapping.Object.PropertySetters.Add("name", (o, p) => { ((Author)o).Name = (string)p; });
            _mapping.Object.PropertySetters.Add("dateTimeCreated", (o, p) => { ((Author)o).DateTimeCreated = (DateTime)p; });


            var author = new Author();
            var classUnderTest = new Delta<Author>(_configuration.Object);

            classUnderTest.ObjectPropertyValues =
                new Dictionary<string, object>()
                {
                    {"id", 1},
                    {"dateTimeCreated", new DateTime(2016,1,1)},
                    {"name", "author name to be filtered out" }
                };

            classUnderTest.Scan();
            classUnderTest.FilterOut(t => t.Name);

            //Act
            classUnderTest.ApplySimpleProperties(author);

            //Assert
            Assert.Equal(author.Id, 1);
            Assert.Equal(author.DateTimeCreated, new DateTime(2016, 1, 1));
            Assert.Null(author.Name);
        }

        [Fact]
        public void GIVEN_SimpleProperty_WHEN_DeltaApply_THEN_ValuesApplied()
        {
            //Arrange
            Setup();

            _mapping.Object.PropertySetters.Add("tittle", (o, p) => { ((Article)o).Tittle = (string)p; });

            var article = new Article();
            var classUnderTest = new Delta<Article>(_configuration.Object);

            classUnderTest.ObjectPropertyValues =
                new Dictionary<string, object>()
                {
                    {"tittle", "tittle value"}
                };

            classUnderTest.Scan();

            //Act
            classUnderTest.ApplySimpleProperties(article);

            //Assert
            Assert.Equal(article.Tittle, "tittle value");
        }

        [Fact]
        public void GIVEN_RelatedResource_WHEN_DeltaApply_THEN_ValuesApplied()
        {
            //Arrange
            Setup();

            var articleAuthorRelatedProperty = new Mock<IPropertyHandle>(MockBehavior.Strict);
            Action<object, object> actionSetter = (o, p) => { ((Article)o).Author = (Author)p; };
            articleAuthorRelatedProperty.SetupGet(o => o.SetterDelegate).Returns((Delegate)actionSetter);

            var articleAuthorRelationship = new Mock<IRelationshipMapping>(MockBehavior.Strict);
            articleAuthorRelationship.SetupGet(o => o.IsCollection).Returns(false);
            articleAuthorRelationship.SetupGet(o => o.RelationshipName).Returns("author");
            articleAuthorRelationship.SetupGet(o => o.RelatedProperty).Returns(articleAuthorRelatedProperty.Object);

            _mapping.Object.Relationships.Add(articleAuthorRelationship.Object);

            var article = new Article();
            var classUnderTest = new Delta<Article>(_configuration.Object);

            classUnderTest.ObjectPropertyValues =
                new Dictionary<string, object>()
                {
                    {"author", new Author {Id = 1 } }
                };

            classUnderTest.Scan();

            //Act
            classUnderTest.ApplySimpleProperties(article);

            //Assert
            Assert.NotNull(article.Author);
            Assert.Equal(article.Author.Id, 1);
        }

        [Fact]
        public void GIVEN_RelatedResourceCollection_WHEN_DeltaApply_THEN_ValuesApplied()
        {
            //Arrange
            Setup();

            var articleAuthorRelatedProperty = new Mock<IPropertyHandle>(MockBehavior.Strict);
            Action<Article, ICollection> actionSetter = (o, p) => { ((Article)o).Comments = (IList<Comment>)p; };
            Func<Article, ICollection> actionGetter = (o) => null;
            articleAuthorRelatedProperty.SetupGet(o => o.SetterDelegate).Returns((Delegate)actionSetter);
            articleAuthorRelatedProperty.SetupGet(o => o.GetterDelegate).Returns((Delegate)actionGetter);
            articleAuthorRelatedProperty.SetupGet(o => o.Type).Returns(typeof(List<Comment>));

            var articleAuthorRelationship = new Mock<IRelationshipMapping>(MockBehavior.Strict);
            articleAuthorRelationship.SetupGet(o => o.IsCollection).Returns(true);
            articleAuthorRelationship.SetupGet(o => o.RelationshipName).Returns("comments");
            articleAuthorRelationship.SetupGet(o => o.RelatedProperty).Returns(articleAuthorRelatedProperty.Object);

            _mapping.Object.Relationships.Add(articleAuthorRelationship.Object);

            var article = new Article();
            var classUnderTest = new Delta<Article>(_configuration.Object);

            classUnderTest.CollectionDeltas =
                new Dictionary<string, ICollectionDelta>()
                {
                    {"comments", new CollectionDelta<Comment>(c => c.Id)
                        {
                            Elements = new List<Comment> {
                                new Comment { Id = 1 },
                                new Comment { Id = 2 }
                            }
                        }
                    }
                };

            classUnderTest.Scan();

            //Act
            classUnderTest.ApplyCollections(article);

            //Assert
            Assert.NotNull(article.Comments);
            Assert.Equal(2, article.Comments.Count);
            Assert.Equal(1, article.Comments[0].Id);
            Assert.Equal(2, article.Comments[1].Id);
        }

        [Fact]
        public void GIVEN_ComplexObjectProperty_WHEN_DeltaApply_THEN_ValuesApplied()
        {
            //Arrange
            Setup();

            _mapping.Object.PropertySetters.Add("author", (o, p) => { ((Article)o).Author = ((JObject)p).ToObject<Author>(); });

            var article = new Article();
            var classUnderTest = new Delta<Article>(_configuration.Object);

            var authorJObject = JsonConvert.DeserializeObject("{ 'name': 'author name' }");
            classUnderTest.ObjectPropertyValues =
                new Dictionary<string, object>()
                {
                    {"author",  authorJObject}
                };

            classUnderTest.Scan();

            //Act
            classUnderTest.ApplySimpleProperties(article);

            //Assert
            Assert.NotNull(article.Author);
            Assert.Equal("author name", article.Author.Name);
        }

        [Fact]
        public void GIVEN_ComplexObjectCollection_WHEN_DeltaApply_THEN_ValuesApplied()
        {
            //Arrange
            Setup();

            _mapping.Object.PropertySetters.Add("comments", (o, p) => { ((Article)o).Comments = ((JArray)p).ToObject<List<Comment>>(); });

            var article = new Article();
            var classUnderTest = new Delta<Article>(_configuration.Object);

            var commentsJArray = JsonConvert.DeserializeObject("[ { 'body': 'comment 1 body' }, { 'body': 'comment 2 body' } ]");
            classUnderTest.ObjectPropertyValues =
                new Dictionary<string, object>()
                {
                    {"comments",  commentsJArray}
                };

            classUnderTest.Scan();

            //Act
            classUnderTest.ApplySimpleProperties(article);

            //Assert
            Assert.NotNull(article.Comments);
            Assert.Equal(2, article.Comments.Count);
            Assert.Equal("comment 1 body", article.Comments[0].Body);
            Assert.Equal("comment 2 body", article.Comments[1].Body);
        }

        [Fact]
        public void GIVEN_SimpleTypeCollection_WHEN_DeltaApply_THEN_ValuesApplied()
        {
            //Arrange
            Setup();

            _mapping.Object.PropertySetters.Add("yearsPublished", (o, p) => { ((Article)o).YearsPublished = ((JArray)p).ToObject<List<int>>(); });

            var article = new Article();
            var classUnderTest = new Delta<Article>(_configuration.Object);

            var commentsJArray = JsonConvert.DeserializeObject("[ 1234, 5678 ]");
            classUnderTest.ObjectPropertyValues =
                new Dictionary<string, object>()
                {
                    {"yearsPublished",  commentsJArray}
                };

            classUnderTest.Scan();

            //Act
            classUnderTest.ApplySimpleProperties(article);

            //Assert
            Assert.NotNull(article.YearsPublished);
            Assert.Equal(2, article.YearsPublished.Count);
            Assert.Equal(1234, article.YearsPublished[0]);
            Assert.Equal(5678, article.YearsPublished[1]);
        }

        [Fact]
        public void GIVEN_NoProperties_WHEN_DeltaApply_THEN_OutputsAreDefault()
        {
            //Arrange
            Setup();

            var simpleObject = new Author();
            var objectUnderTest = new Delta<Author>(_configuration.Object);
            objectUnderTest.Scan();

            //Act
            objectUnderTest.ApplySimpleProperties(simpleObject);

            //Assert
            Assert.Equal(simpleObject.Id, 0);
            Assert.Null(simpleObject.Name);
            Assert.Equal(simpleObject.DateTimeCreated, new DateTime());
        }

        [Fact]
        public void GIVEN_ScanNotCalled_WHEN_DeltaFilterOut_THEN_ExceptionThrown()
        {
            //Arrange
            Setup();

            var classUnderTest = new Delta<Author>(_configuration.Object);

            classUnderTest.ObjectPropertyValues =
                new Dictionary<string, object>()
                {
                    {"id", 1},
                    {"dateTimeCreated", new DateTime(2016,1,1)}
                };

            //Act/Assert
            var ex = Assert.Throws<Exception>(()=> classUnderTest.FilterOut(t => t.Name));
            Assert.Equal("Scan must be called before this method", ex.Message);
        }

        [Fact]
        public void GIVEN_ScanNotCalled_WHEN_DeltaApplySimpleProperties_THEN_ExceptionThrown()
        {
            //Arrange
            Setup();

            var author = new Author();
            var classUnderTest = new Delta<Author>(_configuration.Object);

            //Act/Assert
            var ex = Assert.Throws<Exception>(() => classUnderTest.ApplySimpleProperties(author));
            Assert.Equal("Scan must be called before this method", ex.Message);
        }

        [Fact]
        public void GIVEN_ScanNotCalled_WHEN_DeltaApplyCollections_THEN_ExceptionThrown()
        {
            //Arrange
            Setup();

            var author = new Author();
            var classUnderTest = new Delta<Author>(_configuration.Object);

            //Act/Assert
            var ex = Assert.Throws<Exception>(() => classUnderTest.ApplyCollections(author));
            Assert.Equal("Scan must be called before this method", ex.Message);
        }
    }
}