using NJsonApi.Infrastructure;

namespace NJsonApi.Serialization.Representations
{
    public interface ILink
    {
    }

    public interface ISimpleLink : ILink
    {
        string Href { get; set; }
    }

    public interface ILinkObject : ILink
    {
        ISimpleLink Link { get; set; }
        MetaData Meta { get; set; }
    }
}