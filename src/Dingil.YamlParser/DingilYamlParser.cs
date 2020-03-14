namespace Dingil.Parsers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using YamlDotNet.Serialization;
    using YamlDotNet.Serialization.NamingConventions;


    public static class DingilYamlParser
    {
        public static object ParseRaw(string content)
        {
            var deserializer = new DeserializerBuilder()
                .Build();
            var result = deserializer.Deserialize<object>(content);
            return result;

        }

        public static T ParseRaw<T>(string content)
        {
            var deserializer = new DeserializerBuilder()
                .Build();
            var result = deserializer.Deserialize<T>(content);
            return result;

        }
    }

}


