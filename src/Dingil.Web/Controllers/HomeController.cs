using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Dingil.Web.Models;

namespace Dingil.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(string body)
        {
            var types = YamlParser.DingilYamlParser.Parse(body);
            //var (_, _, _) = Builder.DingilBuilder.BuildModule(AppDomain.CurrentDomain, types, System.Reflection.Emit.AssemblyBuilderAccess.Run, "MyDynamicAssembly", true);
            
            return Ok(types);
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
