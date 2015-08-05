﻿namespace EA.Weee.Web.Areas.Test.Controllers
{
    using EA.Weee.Web.Areas.Test.ViewModels;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;
    
    public class HomeController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            HomeViewModel viewModel = new HomeViewModel();
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(HomeViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                switch (viewModel.Options.SelectedValue)
                {
                    case HomeViewModel.OptionGeneratePcsXmlFile:
                        return RedirectToAction("SelectOrganisation", "GeneratePcsXml");

                    default:
                        throw new NotSupportedException();
                }
            }

            return View(viewModel);
        }
    }
}