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

namespace Dingil
{

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;

    public interface IDingilBuilder : IClassBuilder
    {
        IDingilBuilder InitializeAndCreateClass(string name, Dictionary<string, string> properties);
        IDingilBuilder InitializeAndCreateClass(string name, Dictionary<string, Type> properties);
        IDingilBuilder InitializeAndCreateClasses(Dictionary<string, Dictionary<string, string>> typeDefinitions);
        IDingilBuilder InitializeAndCreateClasses(Dictionary<string, Dictionary<string, Type>> typeDefinitions);

        [Obsolete("Deprecated", true)]
        IDingilBuilder SaveAssembly();

        Type GetClass(string name);
        Dictionary<string, Type> GetClasses();
    }
}
