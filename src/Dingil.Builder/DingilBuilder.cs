using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Dingil
{
    public class DingilBuilder
    {

        public static FieldBuilder AddField(TypeBuilder builder, string name, Type type)
        {
            return builder.DefineField(name, type, FieldAttributes.Public);
        }

        public static TypeBuilder TypeBuilderFactory(ModuleBuilder moduleBuilder, string name)
        {
            TypeBuilder builder = moduleBuilder.DefineType(name, TypeAttributes.Public);
            return builder;
        }

        public static AssemblyBuilder AssemblyBuilderFactory(AppDomain appDomain, 
            string name, 
            Version version, 
            AssemblyBuilderAccess access)
        {
            return appDomain.DefineDynamicAssembly(new AssemblyName()
            {
                Name = name,
                Version = version
            }, access);
        }

        public static ModuleBuilder ModuleBuilderFactory(
            AssemblyBuilder assemblyBuilder,
            string filename,
            bool emitSymbolInfo = false)
        {
            if (filename.Length > 260) throw new ArgumentOutOfRangeException(nameof(filename));

            return assemblyBuilder.DefineDynamicModule(filename, emitSymbolInfo);
        }

        /// <summary>
        /// TODO Text, Number, URL, Email, Money, etc.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Type GetType(string name)
        {
            return Type.GetType(name);
        }
    }
}
