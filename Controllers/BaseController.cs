using System;
using System.Collections.Generic;
using System.Mvc;


namespace SmartSecurity.Controllers
{
    using Models;
    public abstract class BaseController : Controller
    {
        class AsyncMessage
        {
            public string Topic;
            public object Value;
        }

        //public ActionResult Message(string text)
        //{
        //    return View(new Views.Message(), text);
        //}

        public ActionResult Wait()
        {
            return new ActionResult { View = new Waiting() };
        }
    }
}
