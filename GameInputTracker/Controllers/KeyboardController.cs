using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GameInputTracker.Controllers
{
    public class KeyboardController : Controller
    {
        // GET: Keyboard
        public ActionResult Index()
        {
            return View();
        }
    }
}