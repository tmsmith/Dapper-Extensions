using System.Collections.Generic;

namespace Dapper.Extensions.Linq.Core.Configuration
{
    public interface IContainerCustomisations
    {
        Dictionary<string, object> Settings();
    }
}
