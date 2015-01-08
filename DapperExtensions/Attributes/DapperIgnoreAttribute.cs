using System;

namespace DapperExtensions.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class DapperIgnoreAttribute : Attribute
    {
    }
}
