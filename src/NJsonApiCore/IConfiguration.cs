﻿using Newtonsoft.Json;
using System;

namespace NJsonApi
{
    public interface IConfiguration
    {
        string DefaultJsonApiMediaType { get; }

        void AddMapping(IResourceMapping resourceMapping);

        IResourceMapping GetMapping(Type type);

        IResourceMapping GetMapping(string resourceType);

        IResourceMapping GetMapping(object objectGraph);

        JsonSerializer GetJsonSerializer();

        bool IsResourceRegistered(Type resource);

        bool IsControllerRegistered(Type controller);

        bool ValidateIncludedRelationshipPaths(string[] includedPaths, object objectGraph);

        bool ValidateAcceptHeader(string acceptsHeaders);

        string[] FindRelationshipPathsToInclude(string includeQueryParameter);

        JsonSerializerSettings GetJsonSerializerSettings();
    }
}