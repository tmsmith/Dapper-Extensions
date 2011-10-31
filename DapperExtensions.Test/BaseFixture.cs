using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace DapperExtensions.Test
{
    [TestFixture]
    public class BaseFixture
    {
        [SetUp]
        public virtual void Setup()
        {
            DapperExtensions.ClearMapCache();
        }

        [TearDown]
        public virtual void Teardown()
        {
        }
    }
}