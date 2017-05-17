using NJsonApi.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace NJsonApi.Infrastructure
{
    public class Delta<T> : IDelta<T> where T : new()
    {
        private readonly IResourceMapping _mapping;

        private Dictionary<string, Action<object, object>> _currentTypeSetters;
        private Dictionary<string, Action<object, object>> _typeSettersTemplates;

        private Dictionary<string, CollectionInfo<T>> _currentCollectionInfos;
        private Dictionary<string, CollectionInfo<T>> _collectionInfoTemplates;

        public Dictionary<string, object> ObjectPropertyValues { get; set; }
        public Dictionary<string, ICollectionDelta> CollectionDeltas { get; set; }
        public IMetaData TopLevelMetaData { get; set; }
        public IMetaData ObjectMetaData { get; set; }
        private bool _scanned;

        public Delta(IConfiguration configuration)
        {
            _mapping = configuration.GetMapping(typeof(T));
            ObjectPropertyValues = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            CollectionDeltas = new Dictionary<string, ICollectionDelta>();
            TopLevelMetaData = null;
            ObjectMetaData = null; 
        }

        public void Scan()
        {
            if (_typeSettersTemplates == null)
            {
                _typeSettersTemplates = ScanForProperties();
            }
            if (_collectionInfoTemplates == null)
            {
                _collectionInfoTemplates = ScanForCollections();
            }

            _currentTypeSetters = _typeSettersTemplates.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            _currentCollectionInfos = _collectionInfoTemplates.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            _scanned = true;
        }

        public void FilterOut<TProperty>(params Expression<Func<T, TProperty>>[] filter)
        {
            ThrowExceptionIfNotScanned();
            foreach (var f in filter)
            {
                var propertyName = CamelCaseUtil.ToCamelCase(f.GetPropertyInfo().Name);
                if (_currentTypeSetters.ContainsKey(propertyName))
                    _currentTypeSetters.Remove(propertyName);
                if (_currentCollectionInfos.ContainsKey(propertyName))
                    _currentCollectionInfos.Remove(propertyName);
            }
        }

        public void SetValue<TProperty>(Expression<Func<T, TProperty>> property, object value)
        {
            var propertyInfo = property.GetPropertyInfo();
            ObjectPropertyValues[CamelCaseUtil.ToCamelCase(propertyInfo.Name)] = value;
        }

        public TProperty GetValue<TProperty>(Expression<Func<T, TProperty>> property)
        {
            var propertyInfo = property.GetPropertyInfo();
            object val;
            ObjectPropertyValues.TryGetValue(CamelCaseUtil.ToCamelCase(propertyInfo.Name), out val);
            return (TProperty)val;
        }

        public void ApplySimpleProperties(T inputObject)
        {
            ThrowExceptionIfNotScanned();
            if (ObjectPropertyValues == null) return;
            foreach (var objectPropertyNameValue in ObjectPropertyValues)
            {
                Action<object, object> setter;

                _currentTypeSetters.TryGetValue(objectPropertyNameValue.Key, out setter);
                if (setter != null)
                    setter(inputObject, objectPropertyNameValue.Value);
            }
        }

        public void ApplyCollections(T inputObject)
        {
            ThrowExceptionIfNotScanned();
            if (ObjectPropertyValues == null) return;
            foreach (var colDelta in CollectionDeltas)
            {
                CollectionInfo<T> info;
                _currentCollectionInfos.TryGetValue(colDelta.Key, out info);
                if (info != null)
                {
                    var existingCollection = info.Getter(inputObject);
                    if (existingCollection == null)
                    {
                        existingCollection = Activator.CreateInstance(info.CollectionType) as ICollection;
                        info.Setter(inputObject, existingCollection);
                    }

                    colDelta.Value.Apply(existingCollection);
                }
            }
        }

        public ICollectionDelta<TElement> Collection<TElement>(Expression<Func<T, ICollection<TElement>>> collectionProperty)
        {
            ICollectionDelta delta;
            CollectionDeltas.TryGetValue(collectionProperty.GetPropertyInfo().Name, out delta);
            return delta as ICollectionDelta<TElement>;
        }

        public T ToObject()
        {
            var t = new T();
            ApplySimpleProperties(t);
            ApplyCollections(t);
            return t;
        }

        private Dictionary<string, Action<object, object>> ScanForProperties()
        {
            // set1 contains simple properties setters that are not related resources
            var set1 = ObjectPropertyValues
                .Where(opv => _mapping.PropertySetters.ContainsKey(opv.Key))
                .ToDictionary(opv => opv.Key, opv => _mapping.PropertySetters[opv.Key]);
            // set2 contains simple properteis setters for 1:1 related resources
            var set2 = ObjectPropertyValues
                .Join(
                    _mapping.Relationships.Where(r => !r.IsCollection),
                    opv => opv.Key,
                    r => r.RelationshipName,
                    (opv, r) => new KeyValuePair<string, Action<object, object>>(opv.Key, (Action<object,object>)r.RelatedProperty.SetterDelegate))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            return set1.Concat(set2).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        private Dictionary<string, CollectionInfo<T>> ScanForCollections()
        {
            // 1:n related resources setters
            var set = CollectionDeltas
                .Join(
                    _mapping.Relationships.Where(r => r.IsCollection),
                    cd => cd.Key,
                    r => r.RelationshipName,
                    (cd, r) =>
                        new KeyValuePair<string, CollectionInfo<T>>(
                            cd.Key,
                            new CollectionInfo<T>
                            {
                                Getter = (Func<T, ICollection>)(r.RelatedProperty.GetterDelegate),
                                Setter = (Action<T, ICollection>)(r.RelatedProperty.SetterDelegate),
                                CollectionType = r.RelatedProperty.Type
                            }))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            return set;
        }

        private class CollectionInfo<TOwner>
        {
            public Type CollectionType { get; set; }
            public Func<TOwner, ICollection> Getter { get; set; }
            public Action<TOwner, ICollection> Setter { get; set; }
        }

        private void ThrowExceptionIfNotScanned()
        {
            if (!_scanned) throw new Exception("Scan must be called before this method");
        }
    }
}