using System;
using System.Collections.Generic;
using System.IO;
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
            DapperExtensions.ClearCache();
        }

        [TearDown]
        public virtual void Teardown()
        {
        }

        protected string ReadScriptFile(string name)
        {
            using (Stream s = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("DapperExtensions.Test.SqlScripts." + name + ".sql"))
            using (StreamReader sr = new StreamReader(s))
            {
                return sr.ReadToEnd();
            }
        }
    }
}