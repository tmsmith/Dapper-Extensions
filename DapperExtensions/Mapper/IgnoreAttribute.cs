using System;

namespace DapperExtensions.Mapper
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class IgnoreAttribute : Attribute
    {
        
    }
}