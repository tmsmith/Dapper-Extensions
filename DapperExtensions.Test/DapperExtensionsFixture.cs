using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Moq;
using NUnit.Framework;

namespace DapperExtensions.Test
{
    [TestFixture]
    public class DapperExtensionsFixture
    {
        private Mock<IDbConnection> _connection;

        [SetUp]
        public void Setup()
        {
            _connection = new Mock<IDbConnection>();
        }

        [Test]
        public void GetMap_Returns_DefaultMapper()
        {
            var result = DapperExtensions.GetMap<TestEntity>();
            Assert.AreEqual(typeof(AutoClassMapper<TestEntity>), result.GetType());
        }

        [Test]
        public void GetMap_Returns_Mapper_If_Defined()
        {
            var result = DapperExtensions.GetMap<TestEntityWithMap>();
            Assert.AreEqual(typeof(TestMapper), result.GetType());
        }

        private class TestEntity
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
        }

        private class TestEntityWithMap
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
        }

        private class TestMapper : ClassMapper<TestEntityWithMap>
        {
        }
    }
}