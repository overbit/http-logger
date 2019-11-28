using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace OverApps.Logging.Tests
{
    internal class TestConfiguration : JsonConfigurationProvider
    {
        private Func<string> _json;
        public TestConfiguration(JsonConfigurationSource source, Func<string> json)
            : base(source)
        {
            _json = json;
        }

        public override void Load()
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(_json());
            writer.Flush();
            stream.Seek(0, SeekOrigin.Begin);
            Load(stream);
        }

        public static ConfigurationRoot Create(Func<string> getJson)
        {
            var provider = new TestConfiguration(new JsonConfigurationSource { Optional = true }, getJson);
            return new ConfigurationRoot(new List<IConfigurationProvider> { provider });
        }
    }
}
