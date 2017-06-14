using NJsonApi.Serialization;
using System;
using NJsonApi.Serialization.Representations;

namespace NJsonApi.Test.Fakes
{
    internal class FakeLinkBuilder : ILinkBuilder
    {
        public ISimpleLink FindResourceSelfLink(Context context, string id, IResourceMapping resourceMapping)
        {
            return new SimpleLink(new Uri("http://example.com"));
        }

        public ISimpleLink RelationshipRelatedLink(Context context, string parentId, IResourceMapping resourceMapping, IRelationshipMapping linkMapping)
        {
            return new SimpleLink(new Uri("http://example.com"));
        }

        public ISimpleLink RelationshipSelfLink(Context context, string resourceId, IResourceMapping resourceMapping, IRelationshipMapping linkMapping)
        {
            return new SimpleLink(new Uri("http://example.com"));
        }
    }
}
