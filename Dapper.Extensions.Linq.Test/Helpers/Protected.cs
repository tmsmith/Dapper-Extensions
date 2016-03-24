﻿using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Dapper.Extensions.Linq.Test.Helpers
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
            MethodInfo method = _obj.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, types, null);
            if (method == null)
            {
                throw new ArgumentException(string.Format("{0} was not found in {1}.", name, _obj.GetType()), name);
            }

            return method;
        }
    }
}