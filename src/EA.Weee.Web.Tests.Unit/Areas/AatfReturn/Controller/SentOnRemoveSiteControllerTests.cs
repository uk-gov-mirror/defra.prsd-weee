﻿namespace EA.Weee.Web.Tests.Unit.Areas.AatfReturn.Controller
{
    using EA.Prsd.Core.Mapper;
    using EA.Weee.Api.Client;
    using EA.Weee.Core.AatfReturn;
    using EA.Weee.Core.Scheme;
    using EA.Weee.Requests.AatfReturn.Obligated;
    using EA.Weee.Web.Areas.AatfReturn.Controllers;
    using EA.Weee.Web.Areas.AatfReturn.Mappings.ToViewModel;
    using EA.Weee.Web.Areas.AatfReturn.ViewModels;
    using EA.Weee.Web.Constant;
    using EA.Weee.Web.Controllers.Base;
    using EA.Weee.Web.Services;
    using EA.Weee.Web.Services.Caching;
    using FakeItEasy;
    using FluentAssertions;
    using System;
    using System.Web.Mvc;
    using Xunit;

    public class SentOnRemoveSiteControllerTests
    {
        private readonly IWeeeClient apiClient;
        private readonly BreadcrumbService breadcrumb;
        private readonly IWeeeCache cache;
        private readonly SentOnRemoveSiteController controller;
        private readonly IMap<ReturnAndAatfToSentOnRemoveSiteViewModelMapTransfer, SentOnRemoveSiteViewModel> mapper;

        public SentOnRemoveSiteControllerTests()
        {
            this.apiClient = A.Fake<IWeeeClient>();
            this.breadcrumb = A.Fake<BreadcrumbService>();
            this.cache = A.Fake<IWeeeCache>();
            this.mapper = A.Fake<IMap<ReturnAndAatfToSentOnRemoveSiteViewModelMapTransfer, SentOnRemoveSiteViewModel>>();

            controller = new SentOnRemoveSiteController(() => apiClient, breadcrumb, cache, mapper);
        }

        [Fact]
        public void CheckSentOnCreateSiteOperatorControllerInheritsExternalSiteController()
        {
            typeof(SentOnRemoveSiteController).BaseType.Name.Should().Be(typeof(ExternalSiteController).Name);
        }

        [Fact]
        public async void IndexGet_GivenValidViewModel_BreadcrumbShouldBeSet()
        {
            var organisationId = Guid.NewGuid();
            var schemeInfo = A.Fake<SchemePublicInfo>();
            const string orgName = "orgName";

            A.CallTo(() => cache.FetchOrganisationName(organisationId)).Returns(orgName);
            A.CallTo(() => cache.FetchSchemePublicInfo(organisationId)).Returns(schemeInfo);

            await controller.Index(organisationId, A.Dummy<Guid>(), A.Dummy<Guid>(), A.Dummy<Guid>());

            breadcrumb.ExternalActivity.Should().Be(BreadCrumbConstant.AatfReturn);
            breadcrumb.ExternalOrganisation.Should().Be(orgName);
            breadcrumb.SchemeInfo.Should().Be(schemeInfo);
        }

        [Fact]
        public async void IndexPost_GivenSelectedValueIsYes_RemoveWeeeSentOnIsCalled()
        {
            var viewModel = new SentOnRemoveSiteViewModel()
            {
                SelectedValue = "Yes",
                WeeeSentOnId = Guid.NewGuid()
            };

            await controller.Index(viewModel);

            A.CallTo(() => apiClient.SendAsync(A<string>._, A<RemoveWeeeSentOn>.That.Matches(r => r.WeeeSentOnId == viewModel.WeeeSentOnId))).MustHaveHappened(Repeated.Exactly.Once);
        }
        
        [Fact]
        public async void IndexPost_GivenSelectedValueIsNo_RedirectToActionIsCalled()
        {
            var returnId = new Guid();
            var organisationId = new Guid();
            var aatfId = new Guid();
            var model = new SentOnRemoveSiteViewModel()
            {
                ReturnId = returnId,
                OrganisationId = organisationId,
                AatfId = aatfId
            };
            var result = await controller.Index(model) as RedirectToRouteResult;

            result.RouteValues["action"].Should().Be("Index");
            result.RouteValues["controller"].Should().Be("SentOnSiteSummaryList");
            result.RouteValues["area"].Should().Be("AatfReturn");
            result.RouteValues["returnId"].Should().Be(returnId);
            result.RouteValues["organisationId"].Should().Be(organisationId);
            result.RouteValues["aatfId"].Should().Be(aatfId);
        }

        [Fact]
        public void GenerateAddress_GivenAddressData_LongAddressNameShouldBeCreatedCorrectly()
        {            
            var siteAddress = new AatfAddressData("Site name", "Site address 1", "Site address 2", "Site town", "Site county", "GU22 7UY", Guid.NewGuid(), "Site country");
            var siteAddressLong = "Site name<br/>Site address 1<br/>Site address 2<br/>Site town<br/>Site county<br/>Site country<br/>GU22 7UY";

            var siteAddressWithoutAddress2 = new AatfAddressData("Site name", "Site address 1", null, "Site town", "Site county", "GU22 7UY", Guid.NewGuid(), "Site country");
            var siteAddressWithoutAddress2Long = "Site name<br/>Site address 1<br/>Site town<br/>Site county<br/>Site country<br/>GU22 7UY";

            var siteAddressWithoutCounty = new AatfAddressData("Site name", "Site address 1", "Site address 2", "Site town", null, "GU22 7UY", Guid.NewGuid(), "Site country");
            var siteAddressWithoutCountyLong = "Site name<br/>Site address 1<br/>Site address 2<br/>Site town<br/>Site country<br/>GU22 7UY";

            var result = controller.GenerateAddress(siteAddress);
            var resultWithoutAddress2 = controller.GenerateAddress(siteAddressWithoutAddress2);
            var resultWithoutCounty = controller.GenerateAddress(siteAddressWithoutCounty);

            result.Should().Be(siteAddressLong);
            resultWithoutAddress2.Should().Be(siteAddressWithoutAddress2Long);
            resultWithoutCounty.Should().Be(siteAddressWithoutCountyLong);
        }
    }
}