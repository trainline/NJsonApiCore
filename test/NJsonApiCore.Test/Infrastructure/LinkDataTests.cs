using NJsonApi.Infrastructure;
using NJsonApi.Serialization.Representations;
using Xunit;

namespace NJsonApi.Test.Infrastructure
{
    public class LinkDataTests
    {
        [Fact]
        public void LinkData_using_ctor_ok()
        {
            // Arrange

            // Act
            var sut = new LinkData();

            // Assert
            Assert.Empty(sut);
        }

        [Fact]
        public void LinkData_add_simple_link_items_ok()
        {
            // Arrange

            // Act
            var sut = new LinkData();
            sut.Add("linkname", new SimpleLink { Href = "url" });

            // Assert
            Assert.Equal("url", ((ISimpleLink)sut["linkname"]).Href);
        }

        [Fact]
        public void LinkData_add_link_object_items_ok()
        {
            // Arrange
            var itemMeta = new MetaData();
            itemMeta.Add("about", "this");

            // Act
            var sut = new LinkData();
            sut.Add("linkname", new LinkObject { Link = new SimpleLink { Href = "url" }, Meta = itemMeta });

            // Arrange
            var matchingItem = (ILinkObject) sut["linkname"];
            Assert.Equal("url", matchingItem.Link.Href);
            Assert.Equal("this", matchingItem.Meta["about"]);
        }
    }
}
