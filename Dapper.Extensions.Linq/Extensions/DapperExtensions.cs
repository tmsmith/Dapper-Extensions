using System;
using Dapper.Extensions.Linq.Core.Configuration;
using Dapper.Extensions.Linq.Core.Mapper;

namespace Dapper.Extensions.Linq.Extensions
{
    public static class DapperExtensions
    {
        /// <summary>
        /// Extension method to assign a class mapper during configuration
        /// 
        /// <see cref="IClassMapper"/>  implemention is required
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="typeOfMapper"></param>
        /// <returns></returns>
        public static IDapperConfigurationContainer UseClassMapper(this IDapperConfigurationContainer configuration, Type typeOfMapper)
        {
            if (typeof(IClassMapper).IsAssignableFrom(typeOfMapper) == false)
                throw new NullReferenceException("Mapping type of not type of IClassMapper");

            Linq.DapperExtensions.DefaultMapper = typeOfMapper;
            return configuration;
        }
    }
}
