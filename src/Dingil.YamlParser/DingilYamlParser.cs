using Dingil.Core;
using System;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Dingil.YamlParser
{
    public class DingilYamlParser
    {
        public static DingilTypes Parse(string content)
        {
            var deserializer = new DeserializerBuilder()
                .Build();
            var result = deserializer.Deserialize<Dictionary<string, Dictionary<string, Type>>>(content);
            return DingilTypes.New(result);

        }
    }

}


