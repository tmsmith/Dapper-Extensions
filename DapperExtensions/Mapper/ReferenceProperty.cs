using DapperExtensions.Enums;
using DapperExtensions.Extensions;
using System;
using System.Reflection;
namespace DapperExtensions.Mapper
{
    public interface IReferenceProperty
    {
        string Name { get; }
        Guid ParentIdentity { get; }
        Guid Identity { get; }
        PropertyInfo PropertyInfo { get; }
        Type EntityType { get; }
        Comparator Comparator { get; }
        PropertyKey LeftProperty { get; }
        PropertyKey RightProperty { get; }
        string ComparatorSignal { get; }

        void SetIdentity(Guid identity);
        void SetParentIdentity(Guid identity);
    }

    public interface IReferenceProperty<T> : IReferenceProperty
    {
    }

    public class ReferenceProperty<T> : IReferenceProperty<T>
    {
        public Guid Identity { get; set; }
        public Guid ParentIdentity { get; set; }
        public string Name { get; }
        public PropertyInfo PropertyInfo { get; }
        public Type EntityType { get; }
        public Comparator Comparator { get; private set; }
        public PropertyKey LeftProperty { get; private set; }
        public PropertyKey RightProperty { get; private set; }

        public string ComparatorSignal { get => Comparator.Description(); }

        public ReferenceProperty(PropertyInfo propertyInfo, Guid parentIdentity, Guid identity)
        {
            ParentIdentity = parentIdentity;
            PropertyInfo = propertyInfo;
            Name = propertyInfo.Name;
            EntityType = typeof(T);
            Identity = identity;
        }

        public void SetIdentity(Guid identity)
        {
            Identity = identity;
        }
        public void SetParentIdentity(Guid identity)
        {
            ParentIdentity = identity;
        }

        internal void Compare(PropertyKey leftProperty, PropertyKey rightProperty, Comparator comparator)
        {
            LeftProperty = leftProperty;
            RightProperty = rightProperty;
            Comparator = comparator;
        }
    }
}

