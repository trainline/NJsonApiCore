using NJsonApi.Infrastructure;
using NJsonApi.Serialization.Documents;
using NJsonApi.Serialization.Representations.Resources;
using NJsonApi.Test.Builders;
using NJsonApi.Test.TestControllers;
using System;
using System.Collections.Generic;
using Xunit;
using Newtonsoft.Json;
using NJsonApi.Serialization.Representations.Relationships;
using NJsonApi.Serialization.Representations;

namespace NJsonApi.Test.Serialization.JsonApiTransformerTest
{
    public class TestMetaData
    {
        [Fact]
        public void Creates_CompondDocument_for_single_class_with_nometadata_and_propertly_map_nometadata()
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
            var transformedObject = result.Data as SingleResource;
            Assert.Null(transformedObject.MetaData);
        }

        [Fact]
        public void Creates_CompondDocument_for_single_class_with_metadata_and_propertly_map_metadata()
        {
            // Arrange
            var context = CreateContext();
            SampleClassWithObjectMetadata objectToTransform = CreateObjectWithMetadataToTransform();
            var transformer = new JsonApiTransformerBuilder()
                .With(CreateConfiguration())
                .Build();

            // Act
            CompoundDocument result = transformer.Transform(objectToTransform, context);

            // Assert
            var transformedObject = result.Data as SingleResource;
            Assert.Equal("value1", transformedObject.MetaData["meta1"]);
            Assert.Equal("value2", transformedObject.MetaData["meta2"]);
        }

        [Fact]
        public void Creates_CompondDocument_for_single_class_with_relationship_metadata_and_propertly_map_metadata()
        {
            // Arrange
            var context = CreateContext();
            SampleClassWithRelatedClassesWithRelationshipMetadata objectToTransform = CreateObjectWithRelationshipMetadataToTransform();
            var transformer = new JsonApiTransformerBuilder()
                .With(CreateConfiguration())
                .Build();

            // Act
            CompoundDocument result = transformer.Transform(objectToTransform, context);

            // Assert
            var transformedObject = result.Data as SingleResource;
            Assert.Equal("value100", ((SingleResourceIdentifier)(transformedObject.Relationships["relatedObject"]).Data).MetaData["meta100"]);
            Assert.Equal("value1", ((MultipleResourceIdentifiers)(transformedObject.Relationships["relatedObjects"]).Data)[0].MetaData["meta1"]);
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

        private static SampleClassWithObjectMetadata CreateObjectWithMetadataToTransform()
        {
            var o = new SampleClassWithObjectMetadata
            {
                Id = 1,
                SomeValue = "Somevalue text test string"
            };
            o.GetMetaData().Add("meta1", "value1");
            o.GetMetaData().Add("meta2", "value2");
            return o;
        }

        private static SampleClassWithRelatedClassesWithRelationshipMetadata CreateObjectWithRelationshipMetadataToTransform()
        {
            var o = new SampleClassWithRelatedClassesWithRelationshipMetadata
            {
                Id = 1,
                RelatedObject = new SampleClassWithRelationshipMetadata
                {
                    Id = 100,
                    SomeValue = "related object (single)"
                },
                RelatedObjects = new List<SampleClassWithRelationshipMetadata>
                {
                    new SampleClassWithRelationshipMetadata { Id = 1, SomeValue = "related object in list (1)"},
                    new SampleClassWithRelationshipMetadata { Id = 2, SomeValue = "related object in list (2)"},
                }
            };
            o.RelatedObject.GetMetaData().Add("meta100", "value100");
            o.RelatedObjects[0].GetMetaData().Add("meta1", "value1");
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

            var mappingWithMeta = new ResourceMapping<SampleClassWithObjectMetadata, DummyController>(c => c.Id);
            mappingWithMeta.ResourceType = "sampleClassesWithMeta";
            mappingWithMeta.AddPropertyGetter("someValue", c => c.SomeValue);

            var mappingWithRelationshipMeta = new ResourceMapping<SampleClassWithRelationshipMetadata, DummyController>(c => c.Id);
            mappingWithRelationshipMeta.ResourceType = "sampleClassWithRelationshipMetadata";
            mappingWithRelationshipMeta.AddPropertyGetter("someValue", c => c.SomeValue);

            var mappingWithRelatedClassesWithRelationshipMeta = new ResourceMapping<SampleClassWithRelatedClassesWithRelationshipMetadata, DummyController>(c => c.Id);
            mappingWithRelatedClassesWithRelationshipMeta.ResourceType = "sampleClassWithRelatedClassesWithRelationshipMetadata";

            var relMapSingle = new RelationshipMapping<SampleClassWithRelatedClassesWithRelationshipMetadata, SampleClassWithRelationshipMetadata>();
            relMapSingle.RelationshipName = "relatedObject";
            relMapSingle.IsCollection = false;
            relMapSingle.RelatedProperty = new PropertyHandle<SampleClassWithRelatedClassesWithRelationshipMetadata, SampleClassWithRelationshipMetadata>(o => o.RelatedObject);
            relMapSingle.ResourceGetter = o => o.RelatedObject;
            relMapSingle.ResourceMapping = mappingWithRelationshipMeta;
            mappingWithRelatedClassesWithRelationshipMeta.Relationships.Add(relMapSingle);

            var relMapCollection = new RelationshipMapping<SampleClassWithRelatedClassesWithRelationshipMetadata, IList<SampleClassWithRelationshipMetadata>>();
            relMapCollection.RelationshipName = "relatedObjects";
            relMapCollection.IsCollection = true;
            relMapCollection.RelatedProperty = new PropertyHandle<SampleClassWithRelatedClassesWithRelationshipMetadata, IList<SampleClassWithRelationshipMetadata>>(o => o.RelatedObjects);
            relMapCollection.ResourceGetter = o => o.RelatedObjects;
            relMapCollection.ResourceMapping = mappingWithRelationshipMeta;
            mappingWithRelatedClassesWithRelationshipMeta.Relationships.Add(relMapCollection);

            var config = new NJsonApi.Configuration();
            config.AddMapping(mapping);
            config.AddMapping(mappingWithMeta);
            config.AddMapping(mappingWithRelationshipMeta);
            config.AddMapping(mappingWithRelatedClassesWithRelationshipMeta);
            return config;
        }

        private class SampleClass
        {
            public int Id { get; set; }
            public string SomeValue { get; set; }
            public DateTime DateTime { get; set; }
            public string NotMappedValue { get; set; }
        }

        private class SampleClassWithObjectMetadata : IObjectMetaDataContainer
        {
            private MetaData _metaData = new MetaData();

            public int Id { get; set; }
            public string SomeValue { get; set; }

            public MetaData GetMetaData()
            {
                return _metaData;
            }
        }

        private class SampleClassWithRelationshipMetadata : IRelationshipMetaDataContainer
        {

            private MetaData _metaData = new MetaData();

            public int Id { get; set; }
            public string SomeValue { get; set; }

            public MetaData GetMetaData()
            {
                return _metaData;
            }
        }

        private class SampleClassWithRelatedClassesWithRelationshipMetadata
        {
            public int Id { get; set; }
            public SampleClassWithRelationshipMetadata RelatedObject { get; set; }
            public IList<SampleClassWithRelationshipMetadata> RelatedObjects { get; set; }
        }
    }
}