using Dingil.Builder;
using Dingil.Core;
using Dingil.YamlParser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using Xunit;

namespace Dingil.Tests
{
    public class DingilTests
    {
        readonly AssemblyBuilder assemblyBuilder;
        readonly ModuleBuilder moduleBuilder;
        readonly IEnumerable<Type> types;
        private readonly Func<Type, bool> predicate = x => x.Name == "Employee";

        public DingilTests()
        {
            DingilTypes typeInformations = DingilYamlParser.Parse(File.ReadAllText("Types.yml"));
            AppDomain appDomain = AppDomain.CurrentDomain; // AppDomain.CurrentDomain | Thread.GetDomain()

            string assemblyName = "MyDynamicAssembly";
            AssemblyBuilderAccess access = AssemblyBuilderAccess.Run;

            (assemblyBuilder, moduleBuilder, types) = DingilBuilder.BuildModule(
                appDomain,
                typeInformations,
                access: access,
                emitSymbolInfo: true,
                name: assemblyName);

        }

        [Fact(DisplayName = "Types Has Items")]
        public void Type_Has_Items()
        {
            Assert.True(types.Any());
        }


        [Fact(DisplayName = "Employee and Company Types Is Not Null")]
        public void Employee_and_Company_Types_Is_Not_Null()
        {
            Assert.Contains("Employee", types.Select(x => x.Name));
            Assert.Contains("Company", types.Select(x => x.Name));
        }

        [Fact(DisplayName = "Instance is not null")]
        public void Employee_Instance_Is_Not_Null()
        {
            object instanceFromConstructor = types
                .Single(predicate)
                .GetConstructor(new Type[] { })
                .Invoke(new object[] { });

            Assert.NotNull(instanceFromConstructor);
        }


        [Fact(DisplayName = "Employee Instance Json Serialize Deserialieze And Set Field")]
        public void Employee_Instance_Set_Field_And_Json_Serialize_Deserialize()
        {
            Type type = types.Single(predicate);

            object instance = type
                .GetConstructor(new Type[] { })
                .Invoke(new object[] { });

            FieldInfo birthDateFieldInfo = type.GetField("Birthdate");

            // set birthdate
            DateTime birthdate = new DateTime(1990, 1, 31);
            birthDateFieldInfo.SetValue(instance, birthdate);

            Assert.Equal(birthdate, birthDateFieldInfo.GetValue(instance));

            string json = Newtonsoft.Json.JsonConvert.SerializeObject(instance);
            object fromJson = Newtonsoft.Json.JsonConvert.DeserializeObject(json, type);

            var newValue = birthDateFieldInfo.GetValue(fromJson);

            Assert.Equal(birthdate, newValue);
        }

        [Fact(DisplayName = "Dirty tests")]
        public void Plain_Type_Build_Test()
        {

            /*
             * getting types
             * 
             */

            Func<Type, bool> employeeTypePredicate = x => x.Name == "Employee";

            Type typeFromResult = types.Single(employeeTypePredicate);
            Type typeFromFromAssembly = moduleBuilder.Assembly.GetType("Employee");
            System.Collections.Generic.IEnumerable<Type> typeFromGetTypesMethod = moduleBuilder.Assembly.GetTypes().Where(employeeTypePredicate);
            TypeInfo typeFromAssemblyBuilderDefinedTypes = assemblyBuilder.DefinedTypes.Single(x => x.Name == "Employee");


            Type typeByGetType = Type.GetType(moduleBuilder.Assembly.GetType("Employee").AssemblyQualifiedName);

            // var typeFromExportedTypes = moduleBuilder.Assembly.ExportedTypes.Single(x => x.Name == "Company"); // System.NotSupportedException: 'The invoked member is not supported in a dynamic assembly.'
            // var typeFromExportedTypesMethod = moduleBuilder.Assembly.GetExportedTypes().Single(employeeTypePredicate); // System.NotSupportedException: 'The invoked member is not supported in a dynamic assembly.'

            object instanceFromConstructor = typeFromResult.GetConstructor(new Type[] { }).Invoke(new object[] { });
            object instanceFromActivatorWithType = Activator.CreateInstance(typeFromResult);

            object instanceFromAssembly = typeFromResult.Assembly.CreateInstance("Employee");

            //ObjectHandle instanceFromAppDomainObjectHandle = AppDomain.CurrentDomain.CreateInstance(moduleBuilder.Assembly.GetName().Name, "Employee");
            //ObjectHandle instanceByAssemblyAndTypeNameObjectHandle = Activator.CreateInstance(typeFromResult.Assembly.GetName().Name, "Employee");
            //object instanceFromAppDomain = instanceFromAppDomainObjectHandle.Unwrap();
            //object instanceByAssemblyAndTypeName = instanceByAssemblyAndTypeNameObjectHandle.Unwrap();

            //var employeeType2 = Type.GetType(moduleBuilder.Assembly.GetType("Employee").AssemblyQualifiedName);

            FieldInfo birthDateFieldInfo = instanceFromConstructor.GetType().GetField("Birthdate");

            // set birthdate
            DateTime birthdate = new DateTime(1990, 1, 31);
            birthDateFieldInfo.SetValue(instanceFromConstructor, birthdate);

            string json = Newtonsoft.Json.JsonConvert.SerializeObject(instanceFromConstructor);
            object instanceFromJson = Newtonsoft.Json.JsonConvert.DeserializeObject(json, typeFromResult);


            var newValue = birthDateFieldInfo.GetValue(instanceFromJson);
            var oldValue = birthDateFieldInfo.GetValue(instanceFromConstructor);

            Assert.Equal(oldValue, newValue);

        }
    }
}
