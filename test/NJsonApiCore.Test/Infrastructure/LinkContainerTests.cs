using NJsonApi.Serialization.Representations;
using Xunit;

namespace NJsonApi.Test.Infrastructure
{
    public class LinkContainerTests
    {
        [Fact]
        public void ObjectLinkContainer_using_ctor_ok()
        {
            // Arrange

            // Act
            var sut = new ObjectLinkContainer();

            // Assert
            Assert.Empty(sut.GetLinks());
        }

        [Fact]
        public void ObjectLinkContainer_add_items_ok()
        {
            // Arrange

            // Act
            var sut = new ObjectLinkContainer();
            sut.GetLinks().Add("linkname", new SimpleLink { Href = "url" });

            // Assert
            Assert.Equal("url", ((ISimpleLink)sut.GetLinks()["linkname"]).Href);
        }
    }
}
