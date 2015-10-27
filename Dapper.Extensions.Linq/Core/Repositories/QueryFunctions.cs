using System;

namespace Dapper.Extensions.Linq.Core.Repositories
{
    public static class QueryFunctions
    {
		/// <summary>
		/// For reflection only.
		/// </summary>
		/// <param name="pattern"></param>
		/// <param name="member"></param>
		/// <returns></returns>
        public static bool Like(string pattern, object member)
        {
            throw new InvalidOperationException("For reflection only!");
        }
    }
}
