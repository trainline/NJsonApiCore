using NJsonApi;
using NJsonApi.Serialization.Documents;
using NJsonApi.Serialization.Representations;
using NJsonApi.Serialization.Representations.Resources;
using NJsonApi.Test.Builders;
using NJsonApi.Test.TestControllers;
using System;
using Xunit;
using NJsonApi.Infrastructure;

namespace NJsonApi.Test.Serialization.JsonApiTransformerTest
{
    public class TestLinkData
    {
        [Fact]
        public void Creates_CompoundDocument_for_single_class_with_no_links_and_properly_map_no_links()
        {
            // Arrange
            var context = CreateContext();
            SampleClass objectToTransform = CreateObjectToTransform();
            var transformer = new JsonApiTransformerBuilder()
                .With(CreateConfiguration())
                .Build();

            // Act
            CompoundDocument result = transformer.Transform(objectToTransform, context);

            // Assert
            Assert.NotNull(result.Data);
            var transformedObject = result.Data as SingleResource;
            Assert.NotNull(transformedObject);
            Assert.Equal(1, transformedObject.Links.Count);
            Assert.Equal("http://example.com/", ((ISimpleLink)((LinkData)transformedObject.Links)["self"]).Href);
        }

        [Fact]
        public void Creates_CompoundDocument_for_single_class_with_links_and_properly_map_links()
        {
            // Arrange
            var context = CreateContext();
            SampleClassWithObjectLinks objectToTransform = CreateObjectWithLinkDataToTransform();
            var transformer = new JsonApiTransformerBuilder()
                .With(CreateConfiguration())
                .Build();

            // Act
            CompoundDocument result = transformer.Transform(objectToTransform, context);

            // Assert
            var transformedObject = result.Data as SingleResource;
            Assert.Equal("url1", ((ISimpleLink)transformedObject.Links["link1"]).Href);
            Assert.Equal("url2", ((ISimpleLink)transformedObject.Links["link2"]).Href);
        }

        [Fact]
        public void Creates_CompoundDocument_for_single_class_with_link_objects_and_properly_map_links()
        {
            // Arrange
            var context = CreateContext();
            SampleClassWithObjectLinks objectToTransform = CreateObjectWithLinkObjectsToTransform();
            var transformer = new JsonApiTransformerBuilder()
                .With(CreateConfiguration())
                .Build();

            // Act
            CompoundDocument result = transformer.Transform(objectToTransform, context);

            // Assert
            var transformedObject = result.Data as SingleResource;
            Assert.Equal("url1", ((ILinkObject)transformedObject.Links["link1"]).Link.Href);
            Assert.Equal("url2", ((ILinkObject)transformedObject.Links["link2"]).Link.Href);

            Assert.Equal("data1", ((ILinkObject)transformedObject.Links["link1"]).Meta["meta1"]);
            Assert.Equal("data2", ((ILinkObject)transformedObject.Links["link2"]).Meta["meta2"]);
        }

        private static SampleClass CreateObjectToTransform()
        {
            return new SampleClass
            {
                Id = 1,
                SomeValue = "Somevalue text test string",
                DateTime = DateTime.UtcNow,
                NotMappedValue = "Should be not mapped"
            };
        }

        private static SampleClassWithObjectLinks CreateObjectWithLinkDataToTransform()
        {
            var o = new SampleClassWithObjectLinks
            {
                Id = 1,
                SomeValue = "Somevalue text test string"
            };
            o.GetLinks().Add("link1", new SimpleLink { Href = "url1" });
            o.GetLinks().Add("link2", new SimpleLink { Href = "url2" });
            return o;
        }

        private static SampleClassWithObjectLinks CreateObjectWithLinkObjectsToTransform()
        {
            var o = new SampleClassWithObjectLinks
            {
                Id = 1,
                SomeValue = "Somevalue text test string"
            };
            var meta1 = new MetaData();
            meta1.Add("meta1", "data1");
            var meta2 = new MetaData();
            meta2.Add("meta2", "data2");

            o.GetLinks().Add("link1", new LinkObject { Link = new SimpleLink { Href = "url1" }, Meta = meta1 });
            o.GetLinks().Add("link2", new LinkObject { Link = new SimpleLink { Href = "url2" }, Meta = meta2 });
            return o;
        }

        private Context CreateContext()
        {
            return new Context(new Uri("http://fakehost:1234/", UriKind.Absolute));
        }

        private IConfiguration CreateConfiguration()
        {
            var mapping = new ResourceMapping<SampleClass, DummyController>(c => c.Id);
            mapping.ResourceType = "sampleClasses";
            mapping.AddPropertyGetter("someValue", c => c.SomeValue);
            mapping.AddPropertyGetter("date", c => c.DateTime);

            var mappingWithLinks = new ResourceMapping<SampleClassWithObjectLinks, DummyController>(c => c.Id);
            mappingWithLinks.ResourceType = "sampleClassesWithLinks";
            mappingWithLinks.AddPropertyGetter("someValue", c => c.SomeValue);

            var config = new NJsonApi.Configuration();
            config.AddMapping(mapping);
            config.AddMapping(mappingWithLinks);
            return config;
        }

        private class SampleClass
        {
            public int Id { get; set; }
            public string SomeValue { get; set; }
            public DateTime DateTime { get; set; }
            public string NotMappedValue { get; set; }
        }

        private class SampleClassWithObjectLinks : IObjectLinkContainer
        {
            private LinkData _linkData = new LinkData();

            public int Id { get; set; }
            public string SomeValue { get; set; }

            public ILinkData GetLinks()
            {
                return _linkData;
            }
        }
    }
}
