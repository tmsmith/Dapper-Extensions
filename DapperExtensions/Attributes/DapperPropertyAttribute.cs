using System;

namespace DapperExtensions.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public abstract class DapperPropertyAttribute : Attribute
    {
    }
}
