using Newtonsoft.Json;
using NJsonApi.Infrastructure;
using NJsonApi.Serialization.Converters;
using System;
using System.Collections.Generic;

namespace NJsonApi.Serialization.Representations
{
    [JsonConverter(typeof(SerializationAwareConverter))]
    public class SimpleLink : ISimpleLink, ISerializationAware
    {
        public SimpleLink()
        {
        }

        public SimpleLink(Uri href)
        {
            this.Href = href.AbsoluteUri;
        }

        public string Href { get; set; }

        public void Serialize(JsonWriter writer) => writer.WriteValue(Href);

        public override string ToString() => Href;
    }

    public class LinkObject : ILinkObject, IObjectMetaDataContainer
    {
        private MetaData _meta = new MetaData();

        [JsonProperty(PropertyName = "href", NullValueHandling = NullValueHandling.Ignore)]
        public ISimpleLink Link { get; set; }

        [JsonProperty(PropertyName = "meta", NullValueHandling = NullValueHandling.Ignore)]
        public MetaData Meta { get { return _meta; } set { _meta = value; } }

        public LinkObject()
        {
        }

        public LinkObject(Uri href)
        {
            this.Link = new SimpleLink(href);
        }

        public MetaData GetMetaData()
        {
            return _meta;
        }
    }
}