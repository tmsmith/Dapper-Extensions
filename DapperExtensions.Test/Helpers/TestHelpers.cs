using System;
using System.Collections.Generic;
using System.Linq;

namespace DapperExtensions.Test.Helpers
{
    public static class TestHelpers
    {
        public static Protected Protected(this object obj)
        {
            return new Protected(obj);
        }
    }
}