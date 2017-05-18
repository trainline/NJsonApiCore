using NJsonApi;
using NJsonApi.Web.MVC5.HelloWorld.Controllers;
using NJsonApi.Web.MVC5.HelloWorld.Models;
using NJsonApiCore.Web.MVC5.HelloWorld.Controllers;
using NJsonApiCore.Web.MVC5.HelloWorld.Models;
using NJsonApiCore.Web.MVC5.HelloWorld.Models.Test;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NJsonApiCore.Web.MVC5.HelloWorld
{
    public static class MyNJsonApiConfigurationBuilder
    {
        public static IConfiguration BuildConfiguration()
        {
            var configBuilder = new ConfigurationBuilder();

            configBuilder
                // see the Article class for clarification what each of the properties or relations illustrates
                .Resource<Article, ArticlesController>()
                .WithAllSimpleProperties()
                .WithSimpleProperty(o => o.SingleTag)
                .WithSimpleProperty(o => o.MoreTags)
                .WithSimpleProperty(o => o.PublishedInYears)
                .WithLinkedResource(o => o.Author)
                .WithLinkedResource(o => o.Reviewer)
                .WithLinkedResource(o => o.Comments)
                .WithLinkedResource(o => o.MoreComments);

            configBuilder
                .Resource<Person, PeopleController>()
                .WithAllProperties();

            configBuilder
                .Resource<Comment, CommentsController>()
                .WithAllProperties();

            configBuilder
                .Resource<SimplestPossibleModel, SimplestPossibleController>()
                .WithAllProperties();

            configBuilder
                .Resource<ModelWithPascalCase, TestExamplesController>()
                .WithAllProperties();

            var nJsonApiConfig = configBuilder.Build();
            return nJsonApiConfig;
        }
    }
}