﻿namespace EA.Weee.Web.Areas.AatfReturn.Controllers
{
    using Api.Client;
    using Constant;
    using EA.Weee.Requests.AatfReturn;
    using EA.Weee.Web.Areas.AatfReturn.ViewModels;
    using EA.Weee.Web.Infrastructure;
    using Prsd.Core.Mapper;
    using Services;
    using Services.Caching;
    using System;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using Web.Controllers.Base;

    public class ReceivedPCSListController : ExternalSiteController
    {
        private readonly Func<IWeeeClient> apiClient;
        private readonly IWeeeCache cache;
        private readonly BreadcrumbService breadcrumb;
        private readonly IMapper mapper;

        public ReceivedPCSListController(Func<IWeeeClient> apiClient,
            IWeeeCache cache,
            BreadcrumbService breadcrumb,
            IMapper mapper)
        {
            this.apiClient = apiClient;
            this.cache = cache;
            this.breadcrumb = breadcrumb;
            this.mapper = mapper;
        }

        // GET: AatfReturn/ReceivedPCSList
        [HttpGet]
        public async Task<ActionResult> Index(Guid organisationId, Guid returnId)
        {
            var viewModel = new ReceivedPCSListViewModel();

            using (var client = apiClient())
            {
                var @return = await client.SendAsync(User.GetAccessToken(), new GetReturnScheme(returnId));

                viewModel.OrganisationName = @return[0].Name;
                viewModel.OrganisationId = organisationId;
                viewModel.ReturnId = returnId;

                var schemeIDList = await client.SendAsync(User.GetAccessToken(), new GetReturnScheme(returnId));
                
                viewModel.SchemeList = schemeIDList;
            }
            await SetBreadcrumb(organisationId, BreadCrumbConstant.AatfReturn);

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<ActionResult> Index(ReceivedPCSListViewModel viewModel)
        {
            return await Task.Run(() => RedirectToAction("Index", "AatfTaskList", new { returnid = viewModel.ReturnId, organisationid = viewModel.OrganisationId }));
        }

        private async Task SetBreadcrumb(Guid organisationId, string activity)
        {
            breadcrumb.ExternalOrganisation = await cache.FetchOrganisationName(organisationId);
            breadcrumb.ExternalActivity = activity;
            breadcrumb.SchemeInfo = await cache.FetchSchemePublicInfo(organisationId);
        }
    }
}