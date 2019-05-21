using NJsonApi.Exceptions;
using NJsonApi.Infrastructure;
using NJsonApi.Serialization.Representations;
using NJsonApi.Serialization.Representations.Relationships;
using NJsonApi.Serialization.Representations.Resources;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NJsonApi.Serialization
{
    public class TransformationHelper
    {
        private const string MetaCountAttribute = "count";
        private readonly IConfiguration configuration;
        private readonly ILinkBuilder linkBuilder;

        public TransformationHelper(IConfiguration configuration, ILinkBuilder linkBuilder)
        {
            this.configuration = configuration;
            this.linkBuilder = linkBuilder;
        }

        public IResourceRepresentation ChooseProperResourceRepresentation(object resource, IEnumerable<SingleResource> representationList)
        {
            return resource is IEnumerable ?
                (IResourceRepresentation)new ResourceCollection(representationList) :
                representationList.Single();
        }

        public List<SingleResource> CreateIncludedRepresentations(List<object> primaryResourceList, IResourceMapping resourceMapping, Context context)
        {
            var includedList = new List<SingleResource>();

            var primaryResourceIdentifiers = primaryResourceList.Select(x =>
            {
                var id = new SingleResourceIdentifier
                {
                    Id = resourceMapping.IdGetter(x).ToString(),
                    Type = resourceMapping.ResourceType,
                    MetaData = GetRelationshipMetadata(x)
                };

                return id;
            });

            var alreadyVisitedObjects = new HashSet<SingleResourceIdentifier>(primaryResourceIdentifiers, new SingleResourceIdentifierComparer());

            foreach (var resource in primaryResourceList)
            {
                includedList.AddRange(
                    AppendIncludedRepresentationRecursive(
                        resource,
                        resourceMapping,
                        alreadyVisitedObjects,
                        context));
            }

            if (includedList.Any())
            {
                return includedList;
            }
            return null;
        }

        private List<SingleResource> AppendIncludedRepresentationRecursive(
            object resource,
            IResourceMapping resourceMapping,
            HashSet<SingleResourceIdentifier> alreadyVisitedObjects,
            Context context,
            string parentRelationshipPath = "")
        {
            var includedResources = new List<SingleResource>();

            foreach (var relationship in resourceMapping.Relationships)
            {
                if (relationship.InclusionRule == ResourceInclusionRules.ForceOmit)
                {
                    continue;
                }

                var relatedResources = UnifyObjectsToList(relationship.RelatedResource(resource));
                string relationshipPath = BuildRelationshipPath(parentRelationshipPath, relationship);

                if (!context.IncludedResources.Any(x => x.Contains(relationshipPath)))
                {
                    continue;
                }

                foreach (var relatedResource in relatedResources)
                {
                    var relatedResourceId = new SingleResourceIdentifier
                    {
                        Id = relationship.ResourceMapping.IdGetter(relatedResource).ToString(),
                        Type = relationship.ResourceMapping.ResourceType,
                        MetaData = GetRelationshipMetadata(relatedResource)
                    };

                    if (alreadyVisitedObjects.Contains(relatedResourceId))
                    {
                        continue;
                    }

                    alreadyVisitedObjects.Add(relatedResourceId);
                    includedResources.Add(
                        CreateResourceRepresentation(relatedResource, relationship.ResourceMapping, context));

                    includedResources.AddRange(
                        AppendIncludedRepresentationRecursive(relatedResource, relationship.ResourceMapping, alreadyVisitedObjects, context, relationshipPath));
                }
            }

            return includedResources;
        }

        private string BuildRelationshipPath(string parentRelationshipPath, IRelationshipMapping relationship)
        {
            if (string.IsNullOrEmpty(parentRelationshipPath))
            {
                return relationship.RelationshipName;
            }
            else
            {
                return $"{parentRelationshipPath}.{relationship.RelationshipName}";
            }
        }

        public List<object> UnifyObjectsToList(object nestedObject)
        {
            var list = new List<object>();
            if (nestedObject != null)
            {
                if (nestedObject is IEnumerable<object>)
                    list.AddRange((IEnumerable<object>)nestedObject);
                else
                    list.Add(nestedObject);
            }

            return list;
        }

        public void VerifyTypeSupport(Type innerObjectType)
        {
            if (typeof(ITopLevelDocument).IsAssignableFrom(innerObjectType))
            {
                throw new NotSupportedException(string.Format("Error while serializing type {0}. IEnumerable<ITopLevelDocument<>> is not supported.", innerObjectType));
            }

            if (typeof(IEnumerable).IsAssignableFrom(innerObjectType) && !innerObjectType.GetTypeInfo().IsGenericType)
            {
                throw new NotSupportedException(string.Format("Error while serializing type {0}. Non generic IEnumerable are not supported.", innerObjectType));
            }
        }

        public object UnwrapResourceObject(object objectGraph)
        {
            if (objectGraph is ITopLevelDocument)
            {
                var topLevelDocument = objectGraph as ITopLevelDocument;
                return topLevelDocument.Value;
            }
            return objectGraph;
        }

        public IMetaData GetTopLevelMetadata(object objectGraph)
        {
            if (objectGraph is ITopLevelDocument)
            {
                var metaDataContainer = objectGraph as ITopLevelDocument;
                return metaDataContainer.GetMetaData();
            }
            return null;
        }

        public SingleResource CreateResourceRepresentation(
            object objectGraph,
            IResourceMapping resourceMapping,
            Context context)
        {
            var result = new SingleResource();

            result.Id = resourceMapping.IdGetter(objectGraph).ToString();
            result.Type = resourceMapping.ResourceType;
            result.Attributes = resourceMapping.GetAttributes(objectGraph, configuration.GetJsonSerializerSettings());
            result.Links = GetObjectLinkData(objectGraph);
            result.Links.Add("self", linkBuilder.FindResourceSelfLink(context, result.Id, resourceMapping));
            result.MetaData = GetObjectMetadata(objectGraph);

            if (resourceMapping.Relationships.Any())
            {
                result.Relationships = CreateRelationships(objectGraph, result.Id, resourceMapping, context);
            }

            return result;
        }

        public Dictionary<string, Relationship> CreateRelationships(object objectGraph, string parentId, IResourceMapping resourceMapping, Context context)
        {
            var relationships = new Dictionary<string, Relationship>();
            foreach (var linkMapping in resourceMapping.Relationships)
            {
                var relationshipName = linkMapping.RelationshipName;
                var rel = new Relationship();
                var relLinks = new RelationshipLinks();

                relLinks.Self = linkBuilder.RelationshipSelfLink(context, parentId, resourceMapping, linkMapping);
                relLinks.Related = linkBuilder.RelationshipRelatedLink(context, parentId, resourceMapping, linkMapping);

                if (!linkMapping.IsCollection)
                {
                    string relatedId = null;
                    object relatedInstance = null;
                    if (linkMapping.RelatedResource != null)
                    {
                        relatedInstance = linkMapping.RelatedResource(objectGraph);
                        if (relatedInstance != null)
                            relatedId = linkMapping.ResourceMapping.IdGetter(relatedInstance).ToString();
                    }
                    if (linkMapping.RelatedResourceId != null && relatedId == null)
                    {
                        var id = linkMapping.RelatedResourceId(objectGraph);
                        if (id != null)
                            relatedId = id.ToString();
                    }

                    if (linkMapping.InclusionRule != ResourceInclusionRules.ForceOmit)
                    {
                        // Generating resource linkage for to-one relationships
                        if (relatedInstance != null)
                            rel.Data = new SingleResourceIdentifier
                            {
                                Id = relatedId,
                                Type = configuration.GetMapping(relatedInstance.GetType()).ResourceType, // This allows polymorphic (subtyped) resources to be fully represented
                                MetaData = GetRelationshipMetadata(relatedInstance)
                            };
                        else if (relatedId == null || linkMapping.InclusionRule == ResourceInclusionRules.ForceInclude)
                            rel.Data = new NullResourceIdentifier(); // two-state null case, see NullResourceIdentifier summary
                    }
                }
                else
                {
                    IEnumerable relatedInstance = null;
                    if (linkMapping.RelatedResource != null)
                        relatedInstance = (IEnumerable)linkMapping.RelatedResource(objectGraph);

                    // Generating resource linkage for to-many relationships
                    if (linkMapping.InclusionRule == ResourceInclusionRules.ForceInclude && relatedInstance == null)
                        rel.Data = new MultipleResourceIdentifiers();
                    if (linkMapping.InclusionRule != ResourceInclusionRules.ForceOmit && relatedInstance != null)
                    {
                        var idGetter = linkMapping.ResourceMapping.IdGetter;
                        var identifiers = relatedInstance
                            .Cast<object>()
                            .Select(o => new SingleResourceIdentifier
                            {
                                Id = idGetter(o).ToString(),
                                Type = configuration.GetMapping(o.GetType()).ResourceType, // This allows polymorphic (subtyped) resources to be fully represented
                                MetaData = GetRelationshipMetadata(o)
                            });
                        rel.Data = new MultipleResourceIdentifiers(identifiers);
                    }

                    // If data is present, count meta attribute is added for convenience
                    if (rel.Data != null)
                        rel.Meta = new Dictionary<string, string> { { MetaCountAttribute, ((MultipleResourceIdentifiers)rel.Data).Count.ToString() } };
                }

                if (relLinks.Self != null || relLinks.Related != null)
                    rel.Links = relLinks;

                if (rel.Data != null || rel.Links != null)
                    relationships.Add(relationshipName, rel);
            }
            return relationships.Any() ? relationships : null;
        }

        public void AssureAllMappingsRegistered(Type type, IConfiguration config)
        {
            if (!config.IsResourceRegistered(type))
            {
                throw new MissingMappingException(type);
            }
        }

        public Dictionary<string, ILink> GetTopLevelLinks(object objectGraph, Uri requestUri)
        {
            var links = (objectGraph as ITopLevelDocument)?.Links;
            var topLevel = links == null ? new Dictionary<string, ILink>() : new Dictionary<string, ILink>(links);
            topLevel.Add("self", new SimpleLink(requestUri));
            return topLevel;
        }

        private MetaData GetObjectMetadata(object objectGraph)
        {
            var metadata = (objectGraph as IObjectMetaDataContainer);
            return metadata?.GetMetaData().Count > 0 ? metadata.GetMetaData() : null;
        }

        private MetaData GetRelationshipMetadata(object objectGraph)
        {
            var metadata = (objectGraph as IRelationshipMetaDataContainer);
            return metadata?.GetMetaData().Count > 0 ? metadata.GetMetaData() : null;
        }

        private ILinkData GetObjectLinkData(object objectGraph)
        {
            var linkContainer = (objectGraph as IObjectLinkContainer);
            var linkData = linkContainer?.GetLinks().Count > 0 ? linkContainer.GetLinks() : new LinkData();
            return linkData;
        }

    }
}