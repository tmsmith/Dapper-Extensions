using System.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DapperExtensions.Test.Helpers
{
    public class Protected
    {
        private readonly object _obj;

        public Protected(object obj)
        {
            if (obj == null)
            {
                throw new ArgumentException("object cannot be null.", "obj");
            }

            _obj = obj;
        }

        public static Expression IsNull<T>()
        {
            Expression<Func<Type>> expr = () => typeof(T);
            return expr.Body;
        }

        public void RunMethod(string name, params object[] parameters)
        {
            InvokeMethod(name, null, parameters);
        }

        public T RunMethod<T>(string name, params object[] parameters)
        {
            return (T)InvokeMethod(name, null, parameters);
        }

        public void RunGenericMethod(string name, Type[] genericTypes, params object[] parameters)
        {
            InvokeMethod(name, genericTypes, parameters);
        }

        public TResult RunGenericMethod<TResult>(string name, Type[] genericTypes, params object[] parameters)
        {
            return (TResult)InvokeMethod(name, genericTypes, parameters);
        }

        public object InvokeMethod(string name, Type[] genericTypes, object[] parameters)
        {
            object[] pa = parameters.Select(p =>
            {
                if (p is ConstantExpression)
                {
                    return null;
                }

                return p;
            }).ToArray();
            MethodInfo method = GetMethod(name, parameters);
            try
            {
                if (genericTypes != null && genericTypes.Any())
                {
                    method = method.MakeGenericMethod(genericTypes);
                }

                return method.Invoke(_obj, pa);
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException;
            }
        }

        public MethodInfo GetMethod(string name, object[] parameters)
        {
            Type[] types = parameters.Select(p =>
            {
                if (p is ConstantExpression)
                {
                    return (Type)((ConstantExpression)p).Value;
                }

                return p.GetType();
            }).ToArray();

            var methods = _obj.GetType().GetTypeInfo().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            var method = methods.Where(m => m.Name.Equals(name) && m.GetParameters().Select(p => p.ParameterType).SequenceEqual(types)).SingleOrDefault();
            if(method == null)
                method = _obj.GetType().GetTypeInfo().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            //MethodInfo method = _obj.GetType().GetTypeInfo().GetMethod(name, types);
            //MethodInfo method = _obj.GetType().GetTypeInfo().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (method == null)
            {
                throw new ArgumentException(string.Format("{0} was not found in {1}.", name, _obj.GetType()), name);
            }

            return method;
        }
    }
}