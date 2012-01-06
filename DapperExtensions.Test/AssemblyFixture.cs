using System;
using System.Collections.Generic;
using System.Linq;
using DapperExtensions.Test.Helpers;
using NUnit.Framework;

[SetUpFixture]
public class AssemblyFixture
{
    [SetUp]
    public void Setup()
    {
        TestHelpers.DeleteAllDatabase();
    }
}