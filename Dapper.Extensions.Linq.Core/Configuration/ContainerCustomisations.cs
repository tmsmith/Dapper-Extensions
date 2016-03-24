using System.Collections.Generic;

namespace Dapper.Extensions.Linq.Core.Configuration
{
    public class ContainerCustomisations : IContainerCustomisations
    {
        private readonly Dictionary<string, object> _settings = new Dictionary<string, object>();

        public Dictionary<string, object> Settings()
        {
            return _settings;
        }
    }
}