namespace Dapper.Extensions.Linq.Test.Helpers
{
    public static class TestHelpers
    {
        public static Protected TestProtected(this object obj)
        {
            return new Protected(obj);
        }
    }
}