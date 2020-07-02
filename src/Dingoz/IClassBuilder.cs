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

    public interface IClassBuilder : IFieldBuilder
    {
        IClassBuilder InitializeClass(string name);
        IDingilBuilder CreateClass(string name);
    }
}
