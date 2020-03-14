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

                       //db = new LiteDatabase(options.DbPath.FullName);
                       db = new LiteDatabase(new MemoryStream());
                   });

            System.Threading.Tasks.Task<string> task = Options.Url.GetStringAsync();

            IDingilBuilder dingil = default(IDingilBuilder);

            Task.Run(async () =>
            {
                var body = await Options.Url.GetStringAsync();
                var typeInformations = Dingil.Parsers.DingilYamlParser.ParseBasic(body);

                dingil = Dingil.DingilBuilder.New()
                    .SetAssemblyAccess(AssemblyBuilderAccess.RunAndCollect)
                    .SetAssemblyName(Guid.NewGuid().ToString())
                    .SetAssemblyVersion(new Version(0, 0, 0))
                    .CreateAssembly()
                    .CreateModule()
                    //.InitializeAndCreateClass("EmptyClass", new Dictionary<string, string>())
                    .InitializeAndCreateClasses(typeInformations)
                ;

            }).Wait();

            var app = WebApplication.Create(args);

            foreach (var (name, @class) in dingil.GetClasses())
            {

                var type = typeof(ApiController<>).MakeGenericType(@class);

                var instance = Activator.CreateInstance(type, new object[] { db });
                var postDelegate = typeof(Func<,,>).MakeGenericType(new Type[] { typeof(HttpContext), @class, typeof(Task) });
                var requestDelegateType = typeof(Func<,>).MakeGenericType(new Type[] { typeof(HttpContext), typeof(Task) });

                string resourceUrl = $"/api/{@class.Name}";

                app.MapGet(resourceUrl + "/{id}", async (context) => await ((Func<HttpContext, int, Task>)Delegate.CreateDelegate(type: typeof(Func<HttpContext, int, Task>), target: instance, method: "get", ignoreCase: true)).Invoke(context, context.Request.RouteValues.Get<int>("id").Value));

                app.MapGet(resourceUrl, async (context) => await ((Func<HttpContext, Task>)Delegate.CreateDelegate(type: typeof(Func<HttpContext, Task>), target: instance, method: "get", ignoreCase: true)).Invoke(context));

                app.MapPost(resourceUrl, async (context) => await (Task)(Delegate.CreateDelegate(type: requestDelegateType, target: instance, method: "post", ignoreCase: true)).DynamicInvoke(context));

                app.MapPut(resourceUrl + "/{id}", async (context) => await (Task)(Delegate.CreateDelegate(type: requestDelegateType, target: instance, method: "put", ignoreCase: true)).DynamicInvoke(context));
            }

            await app.RunAsync();
        }
    }
}

namespace Dingoz.Service
{
    public class ApiController<T>
    {
        private readonly LiteDatabase db;
        private readonly ILiteCollection<T> collection;

        public ApiController(LiteDatabase db)
        {
            this.db = db;
            this.collection = db.GetCollection<T>();
        }

        public async Task Get(HttpContext context)
        {
            context.Response.StatusCode = (int)HttpStatusCode.OK;
            var items = collection.FindAll();

            await context.Response.WriteJsonAsync(new
            {
                errors = new string[] { },
                items = items
            });
        }

        public async Task Get(HttpContext context, int id)
        {
            var response = collection.FindById(id);
            if (response == null)
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;

                await context.Response.WriteJsonAsync(new
                {
                    items = new object[] { },
                    errors = new string[] { }
                });

                return;
            }

            context.Response.StatusCode = (int)HttpStatusCode.OK;
            await context.Response.WriteJsonAsync(new
            {
                items = new object[] { response },
                errors = new string[] { }
            });

        }

        public async Task Post(HttpContext context)
        {
            var entity = await System.Text.Json.JsonSerializer.DeserializeAsync<T>(context.Request.BodyReader.AsStream());

            var result = collection.Insert(entity);

            await context.Response.WriteJsonAsync(new
            {
                items = new object[] { collection.FindById(result) },
                errors = new string[] { }
            });

        }

        public async Task Put(HttpContext context)
        {
            var entity = await System.Text.Json.JsonSerializer.DeserializeAsync<T>(context.Request.BodyReader.AsStream());
            var result = collection.Update(entity);

            if (!result)
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;

                await context.Response.WriteJsonAsync(new
                {
                    result = result,
                    errors = new string[] { }
                });

                return;
            }

            context.Response.StatusCode = (int)HttpStatusCode.OK;

            await context.Response.WriteJsonAsync(new
            {
                result = result,
                errors = new string[] { }
            });
        }

        public async Task Patch(HttpContext context, T entity) => throw new NotImplementedException();
        public async Task Delete(HttpContext context, int id) => throw new NotImplementedException();
    }

}


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


namespace Dingil
{

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;

    public class DingilBuilder : IAssemblyBuilder, IModuleBuilder, IClassBuilder, IDingilBuilder
    {

        #region Props

        [Obsolete("", true)]
        private readonly AppDomain appDomain;

        private string assemblyName;
        private Version assemblyVersion = new Version();
        private AssemblyBuilderAccess assemblyAccess = AssemblyBuilderAccess.Run;
        private AssemblyBuilder assemblyBuilder;
        private ModuleBuilder moduleBuilder;
        private Dictionary<string, TypeBuilder> typeBuilders = new Dictionary<string, TypeBuilder>();
        private Dictionary<string, Type> types = new Dictionary<string, Type>();

        #endregion

        #region Fluent API Region

        [Obsolete("Deprecated", true)]
        protected DingilBuilder(AppDomain appDomain)
        {
            this.appDomain = appDomain;
        }

        protected DingilBuilder()
        {
        }

        #region IAssemblyBuilder
        [Obsolete("Deprecated", true)]
        public static IAssemblyBuilder New(AppDomain appDomain)
        {
            return new DingilBuilder(appDomain);
        }

        public static IAssemblyBuilder New()
        {
            return new DingilBuilder();
        }

        public IAssemblyBuilder SetAssemblyName(string assemblyName)
        {
            this.assemblyName = $"{assemblyName}.dll";
            return this;
        }

        public IAssemblyBuilder SetAssemblyVersion(Version version)
        {
            this.assemblyVersion = version;
            return this;
        }

        public IAssemblyBuilder SetAssemblyAccess(AssemblyBuilderAccess access)
        {
            this.assemblyAccess = access;
            return this;
        }
        #endregion

        #region IModuleBuilder
        public IModuleBuilder CreateAssembly()
        {
            var assemblyName = new AssemblyName()
            {
                Name = this.assemblyName,
                Version = assemblyVersion
            };

            this.assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, assemblyAccess);
            return this;
        }
        #endregion

        #region IClassBuilder
        public IClassBuilder InitializeClass(string name)
        {
            TypeBuilder typeBuilder = moduleBuilder.DefineType(name, TypeAttributes.Public);
            typeBuilders.Add(name, typeBuilder);
            return this;
        }

        public IClassBuilder AddField(string typeName, string fieldName, Type fieldType)
        {
            typeBuilders[typeName].DefineField(fieldName, fieldType, FieldAttributes.Public);
            return this;
        }

        public IClassBuilder AddField(string typeName, string fieldName, string fieldType)
        {
            Type type = MapType(fieldType);
            typeBuilders[typeName].DefineField(fieldName, type, FieldAttributes.Public);
            return this;
        }

        public IClassBuilder AddProp(string typeName, string propName, string propType)
        {
            Type type = MapType(propType);
            this.AddProp(typeName, propName, type);
            return this;
        }

        public IClassBuilder AddProp(string typeName, string propName, Type propType)
        {
            TypeBuilder typeBuilder = typeBuilders[typeName];

            FieldBuilder fieldBuilder = typeBuilder.DefineField($"_{propName}", // TODO lowercase (camelCase)
                                                            typeof(string),
                                                            FieldAttributes.Private);

            MethodAttributes getSetAttr =
                 MethodAttributes.Public | MethodAttributes.SpecialName |
                     MethodAttributes.HideBySig;

            var builder = typeBuilder.DefineProperty(
                propName,
                PropertyAttributes.None,
                propType, null);


            // Define the "get" accessor method for Prop.
            MethodBuilder getMethodBuilder = typeBuilder.DefineMethod($"get_{propName}",
                                           getSetAttr,
                                           propType,
                                           Type.EmptyTypes);

            ILGenerator ilGet = getMethodBuilder.GetILGenerator();

            ilGet.Emit(OpCodes.Ldarg_0);
            ilGet.Emit(OpCodes.Ldfld, fieldBuilder);
            ilGet.Emit(OpCodes.Ret);


            // Define the "set" accessor method for Prop.
            MethodBuilder setMethodBuilder = typeBuilder.DefineMethod($"set_{propName}",
                                           getSetAttr,
                                           null,
                                           new Type[] { propType });

            ILGenerator ilSet = setMethodBuilder.GetILGenerator();

            ilSet.Emit(OpCodes.Ldarg_0);
            ilSet.Emit(OpCodes.Ldarg_1);
            ilSet.Emit(OpCodes.Stfld, fieldBuilder);
            ilSet.Emit(OpCodes.Ret);


            builder.SetGetMethod(getMethodBuilder);
            builder.SetSetMethod(setMethodBuilder);
            return this;
        }

        public IClassBuilder AddReferenceField(string typeName, string fieldName, string fieldType)
        {
            Type type = this.assemblyBuilder.GetType(fieldType, throwOnError: true);
            typeBuilders[typeName].DefineField(fieldName, type, FieldAttributes.Public);
            return this;
        }
        #endregion

        #region IDingilBuilder

        public IDingilBuilder InitializeAndCreateClass(string name, Dictionary<string, string> properties)
        {
            this.InitializeClass(name);

            properties.ToList().ForEach(kv =>
            {
                AddField(typeName: name, fieldName: kv.Key, fieldType: kv.Value);
            });

            types.Add(name, typeBuilders[name].CreateType());
            return this;
        }

        public IDingilBuilder InitializeAndCreateClass(string name, Dictionary<string, Type> properties)
        {
            this.InitializeClass(name);

            properties.ToList().ForEach(kv =>
            {
                AddField(typeName: name, fieldName: kv.Key, fieldType: kv.Value);
            });

            types.Add(name, typeBuilders[name].CreateType());
            return this;
        }

        public IDingilBuilder InitializeAndCreateClasses(Dictionary<string, Dictionary<string, string>> typeDefinitions)
        {
            typeDefinitions.ToList().ForEach(kv =>
            {
                string className = kv.Key;
                Dictionary<string, string> props = kv.Value;

                this.InitializeClass(className);

                props.ToList().ForEach(p =>
                {
                    string propName = p.Key;
                    var propType = p.Value;

                    AddProp(typeName: className, propName: propName, propType: propType);
                });

                Type type = typeBuilders[className].CreateType();
                types.Add(className, type);
            });

            return this;
        }

        public IDingilBuilder InitializeAndCreateClasses(Dictionary<string, Dictionary<string, Type>> typeDefinitions)
        {
            typeDefinitions.ToList().ForEach(kv =>
            {
                string className = kv.Key;
                Dictionary<string, Type> props = kv.Value;

                this.InitializeClass(className);

                props.ToList().ForEach(p =>
                {
                    string fieldName = p.Key;
                    Type fieldType = p.Value;
                    AddField(typeName: className, fieldName: fieldName, fieldType: fieldType);
                });

                Type type = typeBuilders[className].CreateType();
                types.Add(className, type);
            });

            return this;
        }

        [Obsolete("Deprecated", true)]
        public IDingilBuilder SaveAssembly()
        {
            // assemblyBuilder.Save(this.assemblyName);
            return this;
        }

        public IDingilBuilder CreateClass(string name)
        {
            types.Add(name, typeBuilders[name].CreateType());
            return this;
        }

        [Obsolete("", true)]
        public IDingilBuilder CreateModule(bool emitSymbolInfo)
        {
            if (assemblyName.Length > 260) throw new ArgumentOutOfRangeException(nameof(assemblyName));
            this.moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName);
            return this;
        }

        public IDingilBuilder CreateModule()
        {
            if (assemblyName.Length > 260) throw new ArgumentOutOfRangeException(nameof(assemblyName));
            this.moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName);
            return this;
        }

        #endregion

        public Type GetClass(string name)
        {
            return types[name];
        }

        #endregion

        #region Static Methods
        /// <summary>
        /// TODO Text, Number, URL, Email, Money, etc.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Type MapType(string name)
        {
            return Type.GetType(name);
        }

        public Dictionary<string, Type> GetClasses()
        {
            return types;
        }

        #endregion
    }

    public interface IAssemblyBuilder
    {
        IAssemblyBuilder SetAssemblyName(string assemblyName);
        IAssemblyBuilder SetAssemblyVersion(Version version);
        IAssemblyBuilder SetAssemblyAccess(AssemblyBuilderAccess access);

        IModuleBuilder CreateAssembly();
    }

    public interface IModuleBuilder
    {
        [Obsolete("Deprecated", true)]
        IDingilBuilder CreateModule(bool emitSymbolInfo);
        IDingilBuilder CreateModule();
    }

    public interface IClassBuilder : IFieldBuilder
    {
        IClassBuilder InitializeClass(string name);
        IDingilBuilder CreateClass(string name);
    }

    public interface IFieldBuilder
    {
        IClassBuilder AddField(string typeName, string fieldName, string fieldType);
        IClassBuilder AddField(string typeName, string fieldName, Type fieldType);

        IClassBuilder AddProp(string typeName, string propName, string propType);
        IClassBuilder AddProp(string typeName, string propName, Type propType);

        IClassBuilder AddReferenceField(string typeName, string fieldName, string fieldType);
    }

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
