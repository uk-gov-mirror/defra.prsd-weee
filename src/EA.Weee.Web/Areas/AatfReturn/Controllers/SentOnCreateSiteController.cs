﻿namespace EA.Weee.Web.Areas.AatfReturn.Controllers
{
    using System;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using Attributes;
    using EA.Prsd.Core.Mapper;
    using EA.Weee.Api.Client;
    using EA.Weee.Core.AatfReturn;
    using EA.Weee.Core.DataReturns;
    using EA.Weee.Requests.AatfReturn;
    using EA.Weee.Requests.AatfReturn.Obligated;
    using EA.Weee.Requests.Shared;
    using EA.Weee.Web.Areas.AatfReturn.Mappings.ToViewModel;
    using EA.Weee.Web.Areas.AatfReturn.Requests;
    using EA.Weee.Web.Areas.AatfReturn.ViewModels;
    using EA.Weee.Web.Constant;
    using EA.Weee.Web.Controllers.Base;
    using EA.Weee.Web.Infrastructure;
    using EA.Weee.Web.Services;
    using EA.Weee.Web.Services.Caching;

    [ValidateReturnCreatedActionFilter]
    public class SentOnCreateSiteController : ExternalSiteController
    {
        private readonly Func<IWeeeClient> apiClient;
        private readonly BreadcrumbService breadcrumb;
        private readonly IWeeeCache cache;
        private readonly IAddSentOnAatfSiteRequestCreator requestCreator;
        private readonly IMap<ReturnAndAatfToSentOnCreateSiteViewModelMapTransfer, SentOnCreateSiteViewModel> mapper;

        public SentOnCreateSiteController(Func<IWeeeClient> apiClient, BreadcrumbService breadcrumb, IWeeeCache cache, IAddSentOnAatfSiteRequestCreator requestCreator, IMap<ReturnAndAatfToSentOnCreateSiteViewModelMapTransfer, SentOnCreateSiteViewModel> mapper)
        {
            this.apiClient = apiClient;
            this.breadcrumb = breadcrumb;
            this.cache = cache;
            this.requestCreator = requestCreator;
            this.mapper = mapper;
        }

        [HttpGet]
        public virtual async Task<ActionResult> Index(Guid returnId, Guid aatfId, Guid? weeeSentOnId)
        {
            using (var client = apiClient())
            {
                Guid? siteAddressId = null;
                var siteAddress = new AatfAddressData();
                var @return = await client.SendAsync(User.GetAccessToken(), new GetReturn(returnId, false));

                if (weeeSentOnId != null)
                {
                    var weeeSentOnList = await client.SendAsync(User.GetAccessToken(), new GetWeeeSentOn(aatfId, returnId, weeeSentOnId));
                    var weeeSentOn = weeeSentOnList[0];
                    siteAddress = weeeSentOn.SiteAddress;
                    siteAddressId = weeeSentOn.SiteAddressId;
                }

                var countryData = await client.SendAsync(User.GetAccessToken(), new GetCountries(false));
                var viewModel = mapper.Map(new ReturnAndAatfToSentOnCreateSiteViewModelMapTransfer() { CountryData = countryData, WeeeSentOnId = weeeSentOnId, SiteAddressId = siteAddressId, ReturnId = returnId, AatfId = aatfId, OrganisationId = @return.OrganisationData.Id, SiteAddressData = siteAddress });
               
                await SetBreadcrumb(@return.OrganisationData.Id, BreadCrumbConstant.AatfReturn, aatfId, DisplayHelper.FormatQuarter(@return.Quarter, @return.QuarterWindow));
                TempData["currentQuarter"] = @return.Quarter;
                TempData["currentQuarterWindow"] = @return.QuarterWindow;
                return View(viewModel);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<ActionResult> Index(SentOnCreateSiteViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                using (var client = apiClient())
                {
                    var request = requestCreator.ViewModelToRequest(viewModel);

                    var result = await client.SendAsync(User.GetAccessToken(), request);
                    return AatfRedirect.SentOnCreateSiteOperator(viewModel.OrganisationId, viewModel.AatfId, viewModel.ReturnId, result);
                }
            }

            using (var client = apiClient())
            {
                viewModel.SiteAddressData.Countries = await client.SendAsync(User.GetAccessToken(), new GetCountries(false));
            }

            await SetBreadcrumb(viewModel.OrganisationId, BreadCrumbConstant.AatfReturn, viewModel.AatfId, DisplayHelper.FormatQuarter(TempData["currentQuarter"] as Quarter, TempData["currentQuarterWindow"] as QuarterWindow));
            return View(viewModel);
        }

        private async Task SetBreadcrumb(Guid organisationId, string activity, Guid aatfId, string quarter)
        {
            breadcrumb.ExternalOrganisation = await cache.FetchOrganisationName(organisationId);
            breadcrumb.ExternalActivity = activity;
            breadcrumb.OrganisationId = organisationId;
            var aatfInfo = await cache.FetchAatfData(organisationId, aatfId);
            breadcrumb.QuarterDisplayInfo = quarter;
            breadcrumb.AatfDisplayInfo = DisplayHelper.ReportingOnValue(aatfInfo.Name, aatfInfo.ApprovalNumber);
        }
    }
}