using Dingil.Core;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Dingil.Builder
{
    public class DingilBuilder
    {
        protected DingilBuilder() { }

        public static Tuple<AssemblyBuilder, ModuleBuilder, IEnumerable<Type>> BuildModule(AppDomain appDomain,
            DingilTypes typeInformations,
            AssemblyBuilderAccess access = AssemblyBuilderAccess.Run,
            string name = null,
            bool emitSymbolInfo = false)
        {
            Type[] types = new Type[typeInformations.Types.Count];

            AssemblyBuilder assemblyBuilder = AssemblyBuilderFactory(appDomain, name, access);
            var assemblyFilename = $"{name}.dll";

            ModuleBuilder moduleBuilder = ModuleBuilderFactory(assemblyBuilder, assemblyFilename, emitSymbolInfo);

            var index = 0;
            foreach (Tuple<string, IEnumerable<Tuple<string, Type>>> typeInformation in typeInformations.GetTypes())
            {
                Type type = null;

                var (typeName, props) = typeInformation;
                type = TypeBuild(moduleBuilder, typeName, props);
                types[index] = type;
                index++;
            }


            switch (access)
            {
                case AssemblyBuilderAccess.Run:
                    break;
                case AssemblyBuilderAccess.Save:
                case AssemblyBuilderAccess.RunAndSave:
                    try
                    {
                        assemblyBuilder.Save(assemblyFilename); 
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                    break;
                case AssemblyBuilderAccess.ReflectionOnly:
                    break;
                case AssemblyBuilderAccess.RunAndCollect:
                    break;
                default:
                    break;
            }

            return new Tuple<AssemblyBuilder, ModuleBuilder, IEnumerable<Type>>(
                assemblyBuilder,
                moduleBuilder,
                types
            );
        }

        public static Type TypeBuild(ModuleBuilder moduleBuilder, string typeName, IEnumerable<Tuple<string, Type>> props)
        {
            Type type = default(Type);

            TypeBuilder myTypeBldr = moduleBuilder.DefineType(typeName, TypeAttributes.Public);

            foreach (var prop in props)
            {
                var (propName, propType) = prop;
                myTypeBldr.DefineField(propName, propType, FieldAttributes.Public);
            }

            type = myTypeBldr.CreateType();
            return type;
        }

        public static AssemblyBuilder AssemblyBuilderFactory(AppDomain appDomain, string name, AssemblyBuilderAccess access)
        {
            return appDomain.DefineDynamicAssembly(new AssemblyName()
            {
                Name = name
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

    }
}
