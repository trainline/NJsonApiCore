using NJsonApi.Infrastructure;
using Xunit;

namespace NJsonApi.Test.Infrastructure
{
    public class MetaDataContainerTest
    {
        [Fact]
        public void ObjectMetaDataContainer_using_ctor_ok()
        {
            // Arrange

            // Act
            var sut = new ObjectMetaDataContainer();

            // Assert
            Assert.Empty(sut.GetMetaData());
        }

        [Fact]
        public void ObjectMetaData_add_items_ok()
        {
            // Arrange

            // Act
            var sut = new ObjectMetaDataContainer();
            sut.GetMetaData().Add("key", "value");

            // Assert
            Assert.Equal("value", sut.GetMetaData()["key"]);
        }

        [Fact]
        public void RelationshipMetaDataContainer_using_ctor_ok()
        {
            // Arrange

            // Act
            var sut = new RelationshipMetaDataContainer();

            // Assert
            Assert.Empty(sut.GetMetaData());
        }

        [Fact]
        public void RelationshipMetaData_add_items_ok()
        {
            // Arrange

            // Act
            var sut = new RelationshipMetaDataContainer();
            sut.GetMetaData().Add("key", "value");

            // Assert
            Assert.Equal("value", sut.GetMetaData()["key"]);
        }
    }
}