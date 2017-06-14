namespace NJsonApi.Infrastructure
{
    public interface IObjectMetaDataContainer
    {
        MetaData GetMetaData();
    }

    public interface IRelationshipMetaDataContainer
    {
        MetaData GetMetaData();
    }

    public class ObjectMetaDataContainer : IObjectMetaDataContainer
    {
        private MetaData _metaData = new MetaData();

        public MetaData GetMetaData() { return _metaData; }
    }

    public class RelationshipMetaDataContainer : IRelationshipMetaDataContainer
    {
        private MetaData _metaData = new MetaData();

        public MetaData GetMetaData() { return _metaData; }
    }
}
