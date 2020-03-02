using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Dingil
{
    public class DingilBuilder : IAssemblyBuilder, IModuleBuilder, IClassBuilder, IDingilBuilder
    {

        #region Props

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

        protected DingilBuilder(AppDomain appDomain)
        {
            this.appDomain = appDomain;
        }

        #region IAssemblyBuilder
        public static IAssemblyBuilder New(AppDomain appDomain)
        {
            return new DingilBuilder(appDomain);
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
            this.assemblyBuilder = appDomain.DefineDynamicAssembly(new AssemblyName()
            {
                Name = assemblyName,
                Version = assemblyVersion
            }, assemblyAccess);

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
                    string fieldName = p.Key;
                    var fieldType = p.Value;

                    AddField(typeName: className, fieldName: fieldName, fieldType: fieldType);
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

        public IDingilBuilder SaveAssembly()
        {
            assemblyBuilder.Save(this.assemblyName);
            return this;
        }

        public IDingilBuilder CreateClass(string name)
        {
            types.Add(name, typeBuilders[name].CreateType());
            return this;
        }

        public IDingilBuilder CreateModule(bool emitSymbolInfo = false)
        {
            if (assemblyName.Length > 260) throw new ArgumentOutOfRangeException(nameof(assemblyName));
            this.moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName, emitSymbolInfo);
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
        IDingilBuilder CreateModule(bool emitSymbolInfo = false);
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
        IClassBuilder AddReferenceField(string typeName, string fieldName, string fieldType);
    }

    public interface IDingilBuilder : IClassBuilder
    {
        IDingilBuilder InitializeAndCreateClass(string name, Dictionary<string, string> properties);
        IDingilBuilder InitializeAndCreateClass(string name, Dictionary<string, Type> properties);
        IDingilBuilder InitializeAndCreateClasses(Dictionary<string, Dictionary<string, string>> typeDefinitions);
        IDingilBuilder InitializeAndCreateClasses(Dictionary<string, Dictionary<string, Type>> typeDefinitions);
        IDingilBuilder SaveAssembly();

        Type GetClass(string name);
    }
}
