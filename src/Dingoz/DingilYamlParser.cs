using CommandLine;
using Flurl.Http;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Dingil;
using LiteDB;
using System.IO;
using Dingoz.Service;
using System.Net;
using System.Linq;
using Newtonsoft.Json;


namespace Dingil.Parsers
{
    using System.Collections.Generic;
    using YamlDotNet.Serialization;


    public static class DingilYamlParser
    {
        static IDeserializer Deserializer = new DeserializerBuilder().Build();
        public static Dictionary<string, Dictionary<string, string>> ParseBasic(string content)
        {
            var result = Deserializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(content);
            return result;
        }

        public static T ParseRaw<T>(string content)
        {
            var result = Deserializer.Deserialize<T>(content);
            return result;
        }
    }

}
