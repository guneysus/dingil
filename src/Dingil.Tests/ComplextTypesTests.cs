using functional.net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using Xunit;
using static functional.net.Ext;

namespace Dingil.Tests
{
    public class SimpleTests
    {
        [Fact]
        public void Primitive_Types()
        {
            Assert.Equal(typeof(string), DingilBuilder.MapType("System.String"));
        }

        [Fact]
        public void Simple_Class_Definition()
        {
            var builder = DingilBuilder.New(AppDomain.CurrentDomain)
                .SetAssemblyName(assemblyName: "Models")
                .CreateAssembly()
                .CreateModule(emitSymbolInfo: true)
                ;

            builder.InitializeClass(name: "Student");

            var studentTypeInfo = Ext.dict<string, string>(
                ("Id", "System.Int32"),
                ("Name", "System.String"),
                ("Birthdate", "System.DateTime")
            );

            studentTypeInfo.ToList().ForEach(kv =>
            {
                builder.AddField("Student", kv.Key, kv.Value);
            });

            builder.AddProp("Student", "Age", typeof(int));

            builder.CreateClass("Student");

            Type studentType = builder.GetClass("Student"); // typeBuilder.CreateType();

            Assert.NotNull(studentType.GetField("Id"));
            Assert.NotNull(studentType.GetField("Name"));
            Assert.NotNull(studentType.GetField("Birthdate"));
        }

        [Fact]
        public void Easy_Class_Definition()
        {
            var builder = DingilBuilder.New(AppDomain.CurrentDomain)
                .SetAssemblyName(assemblyName: "Models")
                .CreateAssembly()
                .CreateModule(emitSymbolInfo: true);

            var properties = dict(
                ("Id", "System.Int32"),
                ("Name", "System.String"),
                ("Birthdate", "System.DateTime")
            );

            builder.InitializeAndCreateClass("Student", properties);

            Type studentType = builder.GetClass("Student"); // typeBuilder.CreateType();

            Assert.NotNull(studentType.GetField("Id"));
            Assert.NotNull(studentType.GetField("Name"));
            Assert.NotNull(studentType.GetField("Birthdate"));
        }

        [Fact]
        public void Easy_Class_Definition_With_Types()
        {
            var builder = DingilBuilder.New(AppDomain.CurrentDomain)
                .SetAssemblyName(assemblyName: "Models")
                .CreateAssembly()
                .CreateModule(emitSymbolInfo: true);

            var properties = dict(
                    ("Id", typeof(int)),
                    ("Name", typeof(string)),
                    ("Birthdate", typeof(DateTime)
                )
            );

            builder.InitializeAndCreateClass("Student", properties);

            Type studentType = builder.GetClass("Student"); // typeBuilder.CreateType();

            Assert.NotNull(studentType.GetField("Id"));
            Assert.NotNull(studentType.GetField("Name"));
            Assert.NotNull(studentType.GetField("Birthdate"));
        }

        [Fact]
        public void Bulk_Class_Definition()
        {
            var builder = DingilBuilder.New(AppDomain.CurrentDomain)
                .SetAssemblyName(assemblyName: "Models")
                .CreateAssembly()
                .CreateModule(emitSymbolInfo: true);

            var properties = Ext.dict<string, string>(
                ("Id", "System.Int32"),
                ("Name", "System.String"),
                ("Birthdate", "System.DateTime")
            );

            var classDefinitions = Ext.dict<string, Dictionary<string, string>>(
                ("Student", properties),
                ("Employee", properties)
            );

            builder.InitializeAndCreateClasses(classDefinitions);

            Type studentType = builder.GetClass("Student");

            Assert.NotNull(studentType.GetField("Id"));
            Assert.NotNull(studentType.GetField("Name"));
            Assert.NotNull(studentType.GetField("Birthdate"));

            Type employeeType = builder.GetClass("Employee");

            Assert.NotNull(employeeType.GetField("Id"));
            Assert.NotNull(employeeType.GetField("Name"));
            Assert.NotNull(employeeType.GetField("Birthdate"));
        }

        [Fact]
        public void Bulk_Class_Definition_With_Types()
        {
            var builder = DingilBuilder.New(AppDomain.CurrentDomain)
                .SetAssemblyName(assemblyName: "Models")
                .SetAssemblyAccess(AssemblyBuilderAccess.Save)
                .CreateAssembly()
                .CreateModule(emitSymbolInfo: true);

            var properties = dict(
                    ("Id", typeof(int)),
                    ("Name", typeof(string)),
                    ("Birthdate", typeof(DateTime)
                )
            );

            var classDefinitions = Ext.dict<string, Dictionary<string, Type>>(
                ("Student", properties),
                ("Employee", properties)
            );

            builder.InitializeAndCreateClasses(classDefinitions);

            Type studentType = builder.GetClass("Student");

            Assert.NotNull(studentType.GetField("Id"));
            Assert.NotNull(studentType.GetField("Name"));
            Assert.NotNull(studentType.GetField("Birthdate"));

            Type employeeType = builder.GetClass("Employee");

            Assert.NotNull(employeeType.GetField("Id"));
            Assert.NotNull(employeeType.GetField("Name"));
            Assert.NotNull(employeeType.GetField("Birthdate"));

            builder.SaveAssembly();
        }

        [Fact]
        public void Fluent_API_Tests()
        {
            var builder = DingilBuilder.New(AppDomain.CurrentDomain)
                .SetAssemblyName(assemblyName: "Models")
                .SetAssemblyAccess(access: AssemblyBuilderAccess.RunAndSave)
                .SetAssemblyVersion(version: new Version("1.0.0"))
                .CreateAssembly()
                .CreateModule(emitSymbolInfo: true)

                .InitializeClass(name: "Student")

                .InitializeClass(name: "Class")

                .AddField(typeName: "Student", fieldName: "Id", fieldType: typeof(int))
                .AddField(typeName: "Student", fieldName: "Name", fieldType: "System.String")
                .AddField(typeName: "Student", fieldName: "Birthdate", fieldType: "System.DateTime")

                .AddField(typeName: "Class", fieldName: "Id", fieldType: typeof(int))
                .CreateClass(name: "Class")

                .AddReferenceField(typeName: "Student", fieldName: "Class", fieldType: "Class")

                .CreateClass(name: "Student")
                .SaveAssembly()
                ;

            var studentType = builder.GetClass("Student");

            Assert.NotNull(studentType.GetField("Id"));
            Assert.NotNull(studentType.GetField("Name"));
            Assert.NotNull(studentType.GetField("Birthdate"));
        }
    }

}
