namespace Nett.Util
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Nett.Mapping;

    internal static class ReflectionUtil
    {
        public static PropertyInfo GetPropertyInfo<TSource, TProperty>(Expression<Func<TSource, TProperty>> propertyLambda)
        {
            Type type = typeof(TSource);

            MemberExpression member = propertyLambda.Body as MemberExpression;
            if (member == null)
            {
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a method, not a property.",
                    propertyLambda.ToString()));
            }

            PropertyInfo propInfo = member.Member as PropertyInfo;
            if (propInfo == null)
            {
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a field, not a property.",
                    propertyLambda.ToString()));
            }

            if (type != propInfo.ReflectedType && !type.IsSubclassOf(propInfo.ReflectedType))
            {
                throw new ArgumentException(string.Format(
                    "Expresion '{0}' refers to a property that is not from type {1}.",
                    propertyLambda.ToString(),
                    type));
            }

            return propInfo;
        }

        public static SerializationMember GetSerMemberInfo<TSource, TProperty>(Expression<Func<TSource, TProperty>> propertyLambda)
        {
            MemberExpression member = propertyLambda.Body as MemberExpression;
            if (member == null)
            {
                throw new ArgumentException(string.Format(
                    "Expression '{0}' does not refer to a field or a property.",
                    propertyLambda.ToString()));
            }

            if (member.Member is PropertyInfo pi) { return new SerializationMember(pi); }
            else if (member.Member is FieldInfo fi) { return new SerializationMember(fi); }
            else
            {
                throw new ArgumentException(string.Format(
                    "Expression '{0}' does not refer to a field or a property.",
                    propertyLambda.ToString()));
            }
        }

        public static SerializationMember GetSerMemberInfo(Type ownerType, string memberName, BindingFlags bindFlags)
        {
            var fi = ownerType.GetField(memberName, bindFlags);
            if (fi != null) { return new SerializationMember(fi); }

            var pi = ownerType.GetProperty(memberName, bindFlags);
            if (pi != null) { return new SerializationMember(pi); }

            var mi = ownerType.GetMembers(bindFlags);
            if (mi.Any())
            {
                throw new Exception($"Member '{memberName}' of type '{ownerType}' is not a field or property.");
            }

            throw new Exception($"Member '{memberName}' was not found on type '{ownerType}'.");
        }

        public static T GetCustomAttribute<T>(MemberInfo member, bool inherit = false)
            where T : Attribute
        {
            return (T)member.GetCustomAttributes(typeof(T), inherit).SingleOrDefault();
        }

        public static IEnumerable<T> GetCustomAttributes<T>(MemberInfo member, bool inherit = false)
        {
            return member.GetCustomAttributes(typeof(T), inherit).Cast<T>();
        }

        public static IEnumerable<PropertyInfo> GetPropertiesWithAttribute<TAttribute>(Type owner)
            where TAttribute : Attribute
            => owner.GetProperties().Where(a => a.IsDefined(typeof(TAttribute), false));
    }
}
