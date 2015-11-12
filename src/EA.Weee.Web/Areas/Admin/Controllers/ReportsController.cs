﻿namespace EA.Weee.Web.Areas.Admin.Controllers
{
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using Api.Client;
    using Base;
    using Core.Scheme;
    using Core.Shared;
    using Infrastructure;
    using Prsd.Core.Web.ApiClient;
    using Prsd.Core.Web.Mvc.Extensions;
    using Services;
    using ViewModels.Reports;
    using Weee.Requests.Admin;
    using Weee.Requests.Scheme;
    using Weee.Requests.Shared;

    public class ReportsController : AdminController
    {
        private readonly Func<IWeeeClient> apiClient;
        private readonly BreadcrumbService breadcrumb;

        public ReportsController(Func<IWeeeClient> apiClient, BreadcrumbService breadcrumb)
        {
            this.apiClient = apiClient;
            this.breadcrumb = breadcrumb;
        }

        // GET: Admin/Reports
        public async Task<ActionResult> Index()
        {
            SetBreadcrumb();

            using (var client = apiClient())
            {
                var userStatus = await client.SendAsync(User.GetAccessToken(), new GetAdminUserStatus(User.GetUserId()));

                switch (userStatus)
                {
                    case UserStatus.Active:
                        return RedirectToAction("ChooseReport", "Reports");
                    case UserStatus.Inactive:
                    case UserStatus.Pending:
                    case UserStatus.Rejected:
                        return RedirectToAction("InternalUserAuthorisationRequired", "Account", new { userStatus });
                    default:
                        throw new NotSupportedException(
                            string.Format("Cannot determine result for user with status '{0}'", userStatus));
                }
            }
        }

        [HttpGet]
        public ActionResult ChooseReport()
        {
            SetBreadcrumb();

            var model = new ChooseReportViewModel();
            return View("ChooseReport", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChooseReport(ChooseReportViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            SetBreadcrumb();

            switch (model.SelectedValue)
            {
                case Reports.ProducerDetails:
                    return RedirectToAction("ProducerDetails", "Reports");

                default:
                    throw new NotSupportedException();
            }
        }

        [HttpGet]
        public async Task<ActionResult> ProducerDetails()
        {
            SetBreadcrumb();

            using (var client = apiClient())
            {
                try
                {
                    ProducerDetailsViewModel model = new ProducerDetailsViewModel();
                    await SetReportsFilterLists(model, client);
                    return View("ProducerDetails", model);
                }
                catch (ApiBadRequestException ex)
                {
                    this.HandleBadRequest(ex);
                    if (ModelState.IsValid)
                    {
                        throw;
                    }
                    return View();
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ProducerDetails(ProducerDetailsViewModel model)
        {
            using (var client = apiClient())
            {
                await SetReportsFilterLists(model, client);
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                //Download the csv based on the filters.
                string approvalnumber = string.Empty;
                string csvFileName = string.Format("{0}_producerdetails_{1}.csv", model.SelectedYear, DateTime.Now.ToString("ddMMyyyy_HHmm"));
                if (model.SelectedScheme.HasValue)
                {
                    SchemeData scheme =
                        await client.SendAsync(User.GetAccessToken(), new GetSchemeById(model.SelectedScheme.Value));
                    approvalnumber = scheme.ApprovalName.Replace("/", string.Empty);
                    csvFileName = string.Format("{0}_{1}_producerdetails_{2}.csv", model.SelectedYear,
                    approvalnumber, DateTime.Now.ToString("ddMMyyyy_HHmm"));
                }
                if (model.SelectedAA.HasValue)
                {
                    UKCompetentAuthorityData authorityData =
                        await
                            client.SendAsync(User.GetAccessToken(),
                                new GetUKCompetentAuthorityById(model.SelectedAA.Value));
                    var authorisedAuthorityName = authorityData.Abbreviation;
                    csvFileName = string.Format("{0}_{1}_{2}_producerdetails_{3}.csv", model.SelectedYear,
                   approvalnumber, authorisedAuthorityName, DateTime.Now.ToString("ddMMyyyy_HHmm"));
                }
             
                var membersDetailsCsvData = await client.SendAsync(User.GetAccessToken(),
                    new GetMemberDetailsCSV(model.SelectedYear, model.SelectedScheme, model.SelectedAA));

                byte[] data = new UTF8Encoding().GetBytes(membersDetailsCsvData.FileContent);
                return File(data, "text/csv", csvFileName);
            }
        }

        private async Task SetReportsFilterLists(ProducerDetailsViewModel model, IWeeeClient client)
        {
            var allYears = await client.SendAsync(User.GetAccessToken(), new GetAllComplianceYears());
            var allSchemes = await client.SendAsync(User.GetAccessToken(), new GetAllApprovedSchemes());
            var appropriateAuthorities = await client.SendAsync(User.GetAccessToken(), new GetUKCompetentAuthorities());

            model.ComplianceYears = new SelectList(allYears);
            model.SchemeNames = new SelectList(allSchemes, "Id", "SchemeName");
            model.AppropriateAuthorities = new SelectList(appropriateAuthorities, "Id", "Abbreviation");
        }

        private void SetBreadcrumb()
        {
            breadcrumb.InternalActivity = "View reports";
        }
    }
}