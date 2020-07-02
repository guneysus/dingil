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

namespace Dingoz
{
    internal static class Program
    {
        static Options Options;
        static LiteDatabase db;

        public static async Task Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                   .WithParsed(options =>
                   {
                       Options = options;

                       if (options.InMemory)
                       {
                           db = new LiteDatabase(new MemoryStream());
                       }
                       else
                       {
                           db = new LiteDatabase(options.DbPath.FullName);
                       }

                   });


            if (Options?.Url == null)
                return;

            System.Threading.Tasks.Task<string> task = Options.Url.GetStringAsync();

            IDingilBuilder dingil = default;

            Task.Run(async () =>
            {
                var body = await Options.Url.GetStringAsync().ConfigureAwait(false);
                var typeInformations = Dingil.Parsers.DingilYamlParser.ParseBasic(body);

                dingil = Dingil.DingilBuilder.New()
                    .SetAssemblyAccess(AssemblyBuilderAccess.RunAndCollect)
                    .SetAssemblyName(Guid.NewGuid().ToString())
                    .SetAssemblyVersion(new Version(0, 0, 0))
                    .CreateAssembly()
                    .CreateModule()
                    .InitializeAndCreateClasses(typeInformations)
                ;

            }).Wait();

            var app = WebApplication.Create(args);

            Type requestDelegateType = typeof(RequestDelegate);

            foreach (var (name, @class) in dingil.GetClasses())
            {
                var instance = Activator.CreateInstance(typeof(ApiController<>).MakeGenericType(@class), new object[] { db });

                string uri = $"/api/{@class.Name}";

                app.MapGet(uri + "/{id}", (RequestDelegate)Delegate.CreateDelegate(requestDelegateType, instance, "GetById"));

                app.MapGet(uri, (RequestDelegate)Delegate.CreateDelegate(requestDelegateType, instance, "GetAll"));

                app.MapPost(uri, (RequestDelegate)Delegate.CreateDelegate(requestDelegateType, instance, "Post"));

                app.MapPut(uri + "/{id}", (RequestDelegate)Delegate.CreateDelegate(requestDelegateType, instance, "Put"));
            }

            await app.RunAsync().ConfigureAwait(false);
        }
    }
}
