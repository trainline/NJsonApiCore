using System;
using NJsonApi.Infrastructure;

namespace NJsonApi.Web.MVC5.HelloWorld.Models
{
    public class Person : IObjectMetaDataContainer, IRelationshipMetaDataContainer
    {
        private ObjectMetaDataContainer _objectMetaData = new ObjectMetaDataContainer();
        private RelationshipMetaDataContainer _relationshipMetaData = new RelationshipMetaDataContainer();

        public Person()
        {
            StaticPersistentStore.GetNextId();
        }

        public Person(string firstname, string lastname, string twitter) : this()
        {
            Id = StaticPersistentStore.GetNextId();
            FirstName = firstname;
            LastName = lastname;
            Twitter = twitter;
        }
            
        public int Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Twitter { get; set; }

        MetaData IObjectMetaDataContainer.GetMetaData()
        {
            return ((IObjectMetaDataContainer)_objectMetaData).GetMetaData();
        }

        MetaData IRelationshipMetaDataContainer.GetMetaData()
        {
            return ((IRelationshipMetaDataContainer)_relationshipMetaData).GetMetaData();
        }
    }
}