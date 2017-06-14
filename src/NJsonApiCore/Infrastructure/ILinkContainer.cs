using NJsonApi.Infrastructure;

namespace NJsonApi.Serialization.Representations
{
    public interface IObjectLinkContainer
    {
        ILinkData GetLinks();
    }

    public class ObjectLinkContainer : IObjectLinkContainer
    {
        private ILinkData _linkData = new LinkData();

        public ILinkData GetLinks()
        {
            return _linkData;
        }
    }
}
