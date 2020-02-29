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
            var typeInformations = Parsers.DingilYamlParser.Parse(body);
            throw new NotImplementedException();
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