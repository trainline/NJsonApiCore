using System.Collections.Generic;

namespace NJsonApi.Infrastructure
{
    public interface IMetaData : IDictionary<string, object>
    {
    }

    public interface IObjectMetaData : IMetaData
    {
    }

    public interface IRelationshipMetaData : IMetaData
    {
    }
}