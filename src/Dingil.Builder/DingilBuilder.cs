using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Dingil
{
    public class DingilBuilder
    {
        private readonly AppDomain appDomain;
        private string assemblyName;
        private Version assemblyVersion = new Version();
        private AssemblyBuilderAccess assemblyAccess = AssemblyBuilderAccess.Run;
        private AssemblyBuilder assemblyBuilder;
        private ModuleBuilder moduleBuilder;
        private Dictionary<string, TypeBuilder> typeBuilders = new Dictionary<string, TypeBuilder>();
        private Dictionary<string, Type> types = new Dictionary<string, Type>();

        #region Fluent API Region
        protected DingilBuilder(AppDomain appDomain)
        {
            this.appDomain = appDomain;
        }

        public static DingilBuilder New(AppDomain appDomain)
        {
            return new DingilBuilder(appDomain);
        }

        public DingilBuilder SetAssemblyName(string assemblyName)
        {
            this.assemblyName = $"{assemblyName}.dll";
            return this;
        }

        public DingilBuilder SetAssemblyVersion(Version version)
        {
            this.assemblyVersion = version;
            return this;
        }

        public DingilBuilder SetAssemblyAccess(AssemblyBuilderAccess access)
        {
            this.assemblyAccess = access;
            return this;
        }

        public DingilBuilder CreateAssembly()
        {
            this.assemblyBuilder = appDomain.DefineDynamicAssembly(new AssemblyName()
            {
                Name = assemblyName,
                Version = assemblyVersion
            }, assemblyAccess);

            return this;
        }


        public DingilBuilder CreateModule(bool emitSymbolInfo = false)
        {
            if (assemblyName.Length > 260) throw new ArgumentOutOfRangeException(nameof(assemblyName));
            this.moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName, emitSymbolInfo);
            return this;
        }

        public DingilBuilder InitializeClass(string name)
        {
            TypeBuilder typeBuilder = moduleBuilder.DefineType(name, TypeAttributes.Public);
            typeBuilders.Add(name, typeBuilder);
            return this;
        }

        public DingilBuilder AddField(string typeName, string fieldName, Type fieldType)
        {
            typeBuilders[typeName].DefineField(fieldName, fieldType, FieldAttributes.Public);
            return this;
        }

        public DingilBuilder AddField(string typeName, string fieldName, string fieldType)
        {
            Type type = MapType(fieldType);
            typeBuilders[typeName].DefineField(fieldName, type, FieldAttributes.Public);
            return this;
        }

        public DingilBuilder AddReferenceField(string typeName, string fieldName, string fieldType)
        {
            Type type = this.assemblyBuilder.GetType(fieldType, throwOnError: true);
            typeBuilders[typeName].DefineField(fieldName, type, FieldAttributes.Public);
            return this;
        }

        public DingilBuilder SaveAssembly()
        {
            assemblyBuilder.Save(this.assemblyName);
            return this;
        }

        public DingilBuilder CreateClass(string name)
        {
            types.Add(name, typeBuilders[name].CreateType());
            return this;
        }

        public DingilBuilder InitializeAndCreateClass(string name, Dictionary<string, string> properties)
        {
            this.InitializeClass(name);

            properties.ToList().ForEach(kv =>
             {
                 AddField(typeName: name, fieldName: kv.Key, fieldType: kv.Value);
             });

            types.Add(name, typeBuilders[name].CreateType());
            return this;
        }

        public DingilBuilder InitializeAndCreateClass(string name, Dictionary<string, Type> properties)
        {
            this.InitializeClass(name);

            properties.ToList().ForEach(kv =>
            {
                AddField(typeName: name, fieldName: kv.Key, fieldType: kv.Value);
            });

            types.Add(name, typeBuilders[name].CreateType());
            return this;
        }

        public DingilBuilder InitializeAndCreateClasses(Dictionary<string, Dictionary<string, string>> typeDefinitions)
        {
            typeDefinitions.ToList().ForEach(kv =>
            {
                string className = kv.Key;
                Dictionary<string, string> props = kv.Value;

                this.InitializeClass(className);

                props.ToList().ForEach(p =>
                {
                    string fieldName = p.Key;
                    var fieldType = p.Value;

                    AddField(typeName: className, fieldName: fieldName, fieldType: fieldType);
                });

                Type type = typeBuilders[className].CreateType();
                types.Add(className, type);
            });

            return this;
        }

        public DingilBuilder InitializeAndCreateClasses(Dictionary<string, Dictionary<string, Type>> typeDefinitions)
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

        public Type GetClass(string name)
        {
            return types[name];
        }

        #endregion

        /// <summary>
        /// TODO Text, Number, URL, Email, Money, etc.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Type MapType(string name)
        {
            return Type.GetType(name);
        }
    }
}
