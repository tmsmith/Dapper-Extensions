using DapperExtensions.Predicate;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace DapperExtensions.Mapper
{
    public enum JoinType
    {
        Left,
        Right,
        Inner
    }

    public interface IReferenceMap
    {
        Guid Identity { get; }
        Guid ParentIdentity { get; }
        string Name { get; }
        PropertyInfo PropertyInfo { get; }
        Type EntityType { get; }
        Type ParentEntityType { get; }
        IList<IReferenceProperty> ReferenceProperties { get; }
        JoinType JoinType { get; }
        IPredicateGroup JoinPredicate { get; }
        void SetParentIdentity(Guid parentIdentity);
        void SetIdentity(Guid identity);
        void SetJoinType(JoinType join);
        void SetJoinPredicate(IPredicateGroup predicate);
    }

    public interface IReferenceMap<T> : IReferenceMap
    {
        void Reference<TMany>(Expression<Func<TMany, T, object>> expression);
        void Reference(Expression<Func<T, T, object>> expression);
    }

    public class ReferenceMap<T> : IReferenceMap<T>
    {
        public Guid Identity { get; private set; }
        public Guid ParentIdentity { get; private set; }
        public string Name { get; private set; }
        public PropertyInfo PropertyInfo { get; private set; }
        public Type EntityType { get; private set; }
        public Type ParentEntityType { get; private set; }
        public IList<IReferenceProperty> ReferenceProperties { get; private set; }

        public JoinType JoinType { get; private set; }
        public IPredicateGroup JoinPredicate { get; private set; }

        public ReferenceMap(PropertyInfo propertyInfo, Guid parentIdentity)
        {
            Name = propertyInfo.Name;
            PropertyInfo = propertyInfo;
            ReferenceProperties = new List<IReferenceProperty>();
            Identity = Guid.NewGuid();
            ParentIdentity = parentIdentity;
            JoinType = JoinType.Left;
            JoinPredicate = null;
        }

        public void SetParentIdentity(Guid parentIdentity)
        {
            ParentIdentity = parentIdentity;
        }

        public void SetIdentity(Guid identity)
        {
            Identity = identity;
        }

        public void SetJoinType(JoinType join)
        {
            JoinType = join;
        }

        private MemberInfo GetMemberInfo(Expression expression)
        {
            if (expression is MemberExpression memberExpression)
                return memberExpression.Member;

            if (expression is UnaryExpression unaryExpression)
                return GetMemberInfo(unaryExpression.Operand);

            return ((MemberExpression)((UnaryExpression)expression).Operand).Member;
        }

        protected void SetReferenceProperties<TEntity>(UnaryExpression expression)
        {
            var binaries = ReflectionHelper.GetBinaryExpressionsFromUnary(((UnaryExpression)expression));
            foreach (BinaryExpression binary in binaries)
            {
                var leftMember = GetMemberInfo(binary.Left);
                var rightMember = GetMemberInfo(binary.Right);

                var leftKey = new PropertyKey((PropertyInfo)leftMember, leftMember.DeclaringType, leftMember.Name);
                var rightKey = new PropertyKey((PropertyInfo)rightMember, rightMember.DeclaringType, rightMember.Name);
                var comparator = ReflectionHelper.GetRelacionalComparator(binary.NodeType, Name);

                var referenceProperty = new ReferenceProperty<TEntity>(PropertyInfo, ParentIdentity, Identity);
                referenceProperty.Compare(leftKey, rightKey, comparator);

                ReferenceProperties.Add(referenceProperty);
            }
        }

        public void SetJoinPredicate(IPredicateGroup predicate)
        {
            JoinPredicate = predicate;
        }

        public void Reference(Expression<Func<T, T, object>> expression)
        {
            Reference<T>(expression);
        }

        public void Reference<TMany>(Expression<Func<TMany, T, object>> expression)
        {
            var resultExpression = ReflectionHelper.GetProperty(expression, true);
            SetReferenceProperties<TMany>((UnaryExpression)resultExpression);
            EntityType = typeof(TMany);
            ParentEntityType = typeof(T);
        }
    }
}
