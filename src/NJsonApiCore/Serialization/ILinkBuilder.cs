using NJsonApi.Serialization.Representations;

namespace NJsonApi.Serialization
{
    public interface ILinkBuilder
    {
        ISimpleLink FindResourceSelfLink(Context context, string resourceId, IResourceMapping resourceMapping);

        ISimpleLink RelationshipSelfLink(Context context, string resourceId, IResourceMapping resourceMapping, IRelationshipMapping linkMapping);

        ISimpleLink RelationshipRelatedLink(Context context, string resourceId, IResourceMapping resourceMapping, IRelationshipMapping linkMapping);
    }
}