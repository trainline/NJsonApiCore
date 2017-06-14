using NJsonApi.Serialization.Representations;
using System.Collections.Generic;

namespace NJsonApi.Infrastructure
{
    public interface ILinkData : IDictionary<string, ILink>
    {
    }

    public class LinkData : Dictionary<string, ILink>, ILinkData
    {

    }
}