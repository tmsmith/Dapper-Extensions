using System;
using Dapper.Extensions.Linq.Core;

namespace Dapper.Extensions.Linq.Test.Entities
{
    public class Animal : IEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}
