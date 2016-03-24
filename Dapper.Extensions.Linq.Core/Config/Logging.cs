using System;
using System.Configuration;
using System.Xml;

namespace Dapper.Extensions.Linq.Core.Config
{
    public class Logging : IConfigurationSectionHandler
    {
        public string Threshold { get; private set; }

        /// <summary>
        /// Creates a <see cref="Dapper.Extensions.Linq.Core.Logging"/> object from the configuration file.
        /// </summary>
        /// <seealso cref="Dapper.Extensions.Linq.Core.Logging"/>
        public object Create(object parent, object configContext, XmlNode section)
        {
            if(section.Attributes == null)
                throw new NullReferenceException("Logging has no attributes, check your web/app.config");

            XmlAttribute attribute = section.Attributes["Threshold"];

            if (attribute == null)
                throw new NullReferenceException("Threshold was not found, check your web/app.config");

            return new Logging { Threshold = attribute.Value };
        }
    }
}