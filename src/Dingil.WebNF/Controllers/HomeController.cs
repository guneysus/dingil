using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Web;
using System.Web.Mvc;

namespace Dingil.WebNF.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }


        [HttpPost]
        public ActionResult Index(string body)
        {
            var typeInformations = YamlParser.DingilYamlParser.Parse(body);
            var (assemblyBuilder, moduleBuilder, types) = Builder.DingilBuilder.BuildModule(
                AppDomain.CurrentDomain,
                typeInformations,
                AssemblyBuilderAccess.Run,
                "MyDynamicAssembly",
                true);


            Type type = types.Single(x => x.Name == "Employee");
            var employee = Activator.CreateInstance(type);
            FieldInfo birthdate = type.GetField("Birthdate");
            FieldInfo age = type.GetField("Age");
            FieldInfo firstname = type.GetField("Firstname");
            FieldInfo lastname = type.GetField("Lastname");

            birthdate.SetValue(employee, new DateTime(2000, 1, 31));
            age.SetValue(employee, 30);
            firstname.SetValue(employee, "Ahmed");
            lastname.SetValue(employee, "Güneysu");

            return Json(employee);
        }
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}