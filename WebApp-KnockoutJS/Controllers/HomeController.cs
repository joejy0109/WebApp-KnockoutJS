using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebApp_KnockoutJS.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
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

        [Route("knockout")]
        public ActionResult knockout()
        {
            return View("SampleWithKnockout");
        }

        [Route("angular")]
        public ActionResult angular()
        {
            return View("Sample2WithAngular");
        }

        [Route("angular2")]
        public ActionResult angular2()
        {
            return View("Sample2WithAngular2");
        }
    }
}