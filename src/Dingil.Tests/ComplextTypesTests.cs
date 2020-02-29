using functional.net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using Xunit;

namespace Dingil.Tests
{
    public class SimpleTests
    {
        [Fact]
        public void Primitive_Types()
        {
            Assert.Equal(typeof(string), DingilBuilder.GetType("System.String"));
        }

        [Fact]
        public void Simple_Class()
        {
            var studentTypeInfo = Ext.dict<string, string>(
                ("Id", "System.Int32"),
                ("Name", "System.String"),
                ("Birthdate", "System.DateTime")
            );

            // Type studentType = default(Type); // Dingil.Core.Dingil.GetType("Student", studentTypeInfo);

            AssemblyBuilder studentAssemblyBuilder = DingilBuilder.AssemblyBuilderFactory(
                AppDomain.CurrentDomain,
                "Student",
                new Version(),
                AssemblyBuilderAccess.Save);

            #region student
            ModuleBuilder moduleBuilder = DingilBuilder.ModuleBuilderFactory(studentAssemblyBuilder,
        "Student.dll",
        emitSymbolInfo: true);

            TypeBuilder typeBuilder = DingilBuilder.TypeBuilderFactory(moduleBuilder, "Student");


            foreach (var kv in studentTypeInfo)
            {
                var name = kv.Key;
                var typeName = kv.Value;
                FieldBuilder t = DingilBuilder.AddField(typeBuilder, name, DingilBuilder.GetType(typeName));
            }

            //_ = DingilBuilder.AddProperty(typeBuilder, "Id", Dingil.Core.Dingil.GetType("System.Int32"));

            var studentType = typeBuilder.CreateType();
            #endregion

            studentAssemblyBuilder.Save(@"Student.dll");

            Assert.NotNull(studentType.GetField("Id"));
            Assert.NotNull(studentType.GetField("Name"));
            Assert.NotNull(studentType.GetField("Birthdate"));
        }
    }

}


public class PocoBuilder
{

}