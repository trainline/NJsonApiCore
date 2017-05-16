using NJsonApi.Infrastructure;
using NJsonApi.Test.TestModel;
using System;
using System.Collections.Generic;
using Xunit;
using Moq;

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
        public void GIVEN_IncompleteProperties_WHEN_DeltaApply_THEN_OnlyThoseSpecifiedApplied()
        {
            //Arrange
            Setup();
            _mapping.Object.PropertySetters.Add("id", (o, p) => { ((Author)o).Id = (int)p; });
            _mapping.Object.PropertySetters.Add("dateTimeCreated", (o, p) => { ((Author)o).DateTimeCreated = (DateTime)p; });

            var author = new Author();
            var classUnderTest = new Delta<Author>(_configuration.Object);
            
            classUnderTest.ObjectPropertyValues =
                new Dictionary<string, object>()
                {
                    {"id", 1},
                    {"dateTimeCreated", new DateTime(2016,1,1)}
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