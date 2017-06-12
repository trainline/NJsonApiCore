using NJsonApi.Infrastructure;
using System.Collections.Generic;
using System;

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
        ISimpleLink Href { get; set; }
        MetaData Meta { get; set; }
    }

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