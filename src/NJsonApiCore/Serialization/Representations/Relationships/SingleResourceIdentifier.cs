using System.Collections.Generic;
using Newtonsoft.Json;
using NJsonApi.Serialization.Representations.Resources;
using NJsonApi.Infrastructure;

namespace NJsonApi.Serialization.Representations.Relationships
{
    public class SingleResourceIdentifier : IResourceLinkage, IResourceIdentifier
    {
        [JsonProperty(PropertyName = "id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "type", NullValueHandling = NullValueHandling.Ignore)]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "meta", NullValueHandling = NullValueHandling.Ignore)]
        public MetaData MetaData { get; set; }
    }
}