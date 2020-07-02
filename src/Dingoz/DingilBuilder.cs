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

        [Obsolete("do not use", true)]
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
}
