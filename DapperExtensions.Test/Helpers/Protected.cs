using System;
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
            _obj = obj ?? throw new ArgumentException("object cannot be null.", nameof(obj));
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
            var pa = parameters.Select(p =>
            {
                if (p is ConstantExpression)
                {
                    return null;
                }

                return p;
            }).ToArray();
            var method = GetMethod(name, parameters);
            try
            {
                if (genericTypes?.Any() == true)
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

        public MethodInfo GetGenericMethod(string name, Type[] arguments, object[] parameters)
        {
            return _obj.GetType()
                         .GetMethods()
                         .Where(m => m.Name == name)
                         .Select(m => new
                         {
                             Method = m,
                             Params = m.GetParameters(),
                             Args = m.GetGenericArguments()
                         })
                         .Where(x => x.Params.Length == parameters.Length
                                     && x.Args.Length == arguments.Length
                                     //&& x.Params[0].ParameterType == x.Args[0]
                                     )
                         .Select(x => x.Method)
                         .First();
        }

        public object ExectueGenericMethod(string name, Type[] arguments, object[] parameters)
        {
            var method = GetGenericMethod(name, arguments, parameters);
            return method.MakeGenericMethod(arguments).Invoke(_obj, parameters);
        }

        public MethodInfo GetMethod(string name, object[] parameters)
        {
            var types = parameters.Select(p =>
            {
                if (p is ConstantExpression constantExpression)
                {
                    return (Type)(constantExpression).Value;
                }

                return p.GetType();
            }).ToArray();
            MethodInfo method = _obj.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, null, types, null);
            if (method == null)
            {
                throw new ArgumentException(string.Format("{0} was not found in {1}.", name, _obj.GetType()), name);
            }

            return method;
        }
    }
}