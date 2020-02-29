using System;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Dingil.Parsers
{
    public static class DingilYamlParser
    {
        public static object Parse(string content)
        {
            var deserializer = new DeserializerBuilder()
                .Build();
            var result = deserializer.Deserialize<object>(content);
            return result;

        }
    }

}


