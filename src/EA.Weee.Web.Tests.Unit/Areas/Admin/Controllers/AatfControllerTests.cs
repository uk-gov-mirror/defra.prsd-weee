﻿namespace EA.Weee.Web.Tests.Unit.Areas.Admin.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using System.Web.Routing;
    using Api.Client;
    using EA.Weee.Core.AatfReturn;
    using EA.Weee.Core.Organisations;
    using EA.Weee.Core.Shared;
    using EA.Weee.Requests.AatfReturn;
    using EA.Weee.Requests.AatfReturn.Internal;
    using EA.Weee.Requests.Admin;
    using EA.Weee.Requests.Organisations;
    using EA.Weee.Requests.Shared;
    using EA.Weee.Web.Areas.Admin.ViewModels.Home;
    using EA.Weee.Web.Infrastructure;
    using EA.Weee.Web.Services;
    using EA.Weee.Web.Services.Caching;
    using EA.Weee.Web.Tests.Unit.TestHelpers;
    using FakeItEasy;
    using FluentAssertions;
    using Prsd.Core.Mapper;
    using Web.Areas.Admin.Controllers;
    using Web.Areas.Admin.Requests;
    using Web.Areas.Admin.ViewModels.Aatf;
    using Xunit;

    public class AatfControllerTests
    {
        private readonly IWeeeClient weeeClient;
        private readonly IWeeeCache weeeCache;
        private readonly BreadcrumbService breadcrumbService;
        private readonly IMapper mapper;
        private readonly IEditAatfContactRequestCreator requestCreator;
        private readonly AatfController controller;
        private readonly UrlHelper urlHelper;

        public AatfControllerTests()
        {
            weeeClient = A.Fake<IWeeeClient>();
            weeeCache = A.Fake<IWeeeCache>();
            breadcrumbService = A.Fake<BreadcrumbService>();
            mapper = A.Fake<IMapper>();
            requestCreator = A.Fake<IEditAatfContactRequestCreator>();
            urlHelper = A.Fake<UrlHelper>();

            controller = new AatfController(() => weeeClient, weeeCache, breadcrumbService, mapper, requestCreator);
        }

        [Fact]
        public async Task ManageSchemesPost_ModelError_ReturnsView()
        {
            controller.ModelState.AddModelError(string.Empty, "Validation message");

            var result = await controller.ManageAatfs(new ManageAatfsViewModel());

            Assert.NotNull(result);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task ManageAatfsPost_ReturnsSelectedGuid()
        {
            var selectedGuid = Guid.NewGuid();

            var result = await controller.ManageAatfs(new ManageAatfsViewModel { Selected = selectedGuid });

            Assert.NotNull(result);
            Assert.IsType<RedirectToRouteResult>(result);

            var redirectValues = ((RedirectToRouteResult)result).RouteValues;
            Assert.Equal("Details", redirectValues["action"]);
            Assert.Equal(selectedGuid, redirectValues["Id"]);
        }

        [Fact]
        public async Task ManageAatfPost_ModelError_GetAatfsMustBeRun()
        {
            controller.ModelState.AddModelError(string.Empty, "Validation message");

            await controller.ManageAatfs(new ManageAatfsViewModel());

            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetAatfs>._)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public async Task GetAatfsList_Always_SetsInternalBreadcrumbToManageAATFs()
        {
            ActionResult result = await controller.ManageAatfs();

            Assert.Equal("Manage AATFs", breadcrumbService.InternalActivity);
        }

        [Fact]
        public async void DetailsGet_GivenValidAatfId_BreadcrumbShouldBeSet()
        {
            var aatfData = A.Fake<AatfData>();
            var organisationData = new OrganisationData();
            organisationData.BusinessAddress = new Core.Shared.AddressData()
            {
                Address1 = "Site address 1",
                Address2 = "Site address 2",
                TownOrCity = "Site town",
                CountyOrRegion = "Site county",
                Postcode = "GU22 7UY",
                CountryId = Guid.NewGuid(),
                CountryName = "Site country",
                Telephone = "9367282",
                Email = "test@test.com"
            };

            A.CallTo(() => weeeClient.SendAsync(A.Dummy<string>(), A.Dummy<GetAatfById>())).Returns(aatfData);
            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetOrganisationInfo>._)).Returns(organisationData);

            await controller.Details(A.Dummy<Guid>());

            Assert.Equal(breadcrumbService.InternalActivity, InternalUserActivity.ManageAatfs);
        }

        [Fact]
        public async void DetailsGet_GivenValidAatfId_ViewModelShouldBeCreatedWithApprovalDate()
        {
            AatfDetailsViewModel viewModel = A.Fake<AatfDetailsViewModel>();

            var aatfData = A.Fake<AatfData>();
            var organisationData = new OrganisationData();
            organisationData.BusinessAddress = new Core.Shared.AddressData()
            {
                Address1 = "Site address 1",
                Address2 = "Site address 2",
                TownOrCity = "Site town",
                CountyOrRegion = "Site county",
                Postcode = "GU22 7UY",
                CountryId = Guid.NewGuid(),
                CountryName = "Site country",
                Telephone = "9367282",
                Email = "test@test.com"
            };

            A.CallTo(() => weeeClient.SendAsync(A.Dummy<string>(), A.Dummy<GetAatfById>())).Returns(aatfData);
            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetOrganisationInfo>._)).Returns(organisationData);

            var result = await controller.Details(A.Dummy<Guid>()) as ViewResult;

            result.Model.Should().BeEquivalentTo(viewModel);
        }

        [Fact]
        public async void DetailsGet_GivenValidAatfIdButNoApprovalDate_ViewModelShouldBeCreatedWithNullApprovalDate()
        {
            AatfDetailsViewModel viewModel = A.Fake<AatfDetailsViewModel>();
            viewModel.ApprovalDate = null;

            var aatfData = A.Fake<AatfData>();
            aatfData.ApprovalDate = default(DateTime);
            aatfData.Operator.OrganisationId = Guid.NewGuid();

            var organisationData = new OrganisationData();
            organisationData.BusinessAddress = new Core.Shared.AddressData()
            {
                Address1 = "Site address 1",
                Address2 = "Site address 2",
                TownOrCity = "Site town",
                CountyOrRegion = "Site county",
                Postcode = "GU22 7UY",
                CountryId = Guid.NewGuid(),
                CountryName = "Site country",
                Telephone = "9367282",
                Email = "test@test.com"
            };

            A.CallTo(() => weeeClient.SendAsync(A.Dummy<string>(), A.Dummy<GetAatfById>())).Returns(aatfData);
            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetOrganisationInfo>._)).Returns(organisationData);

            var result = await controller.Details(A.Dummy<Guid>()) as ViewResult;

            result.Model.Should().BeEquivalentTo(viewModel);
        }

        [Fact]
        public async void ManageContactDetailsGet_GivenValidViewModel_BreadcrumbShouldBeSet()
        {
            var aatfId = Guid.NewGuid();
            ContactDataAccessSetup(true);

            await controller.ManageContactDetails(aatfId);

            breadcrumbService.InternalActivity.Should().Be(InternalUserActivity.ManageAatfs);
        }

        [Fact]
        public async void ManageContactDetailsGet_GivenAction_DefaultViewShouldBeReturned()
        {
            ContactDataAccessSetup(true);
            var result = await controller.ManageContactDetails(A.Dummy<Guid>()) as ViewResult;

            result.ViewName.Should().BeEmpty();
        }

        [Fact]
        public async void ManageContactDetailsGet_GivenAatf_ContactShouldBeRetrieved()
        {
            var aatfId = Guid.NewGuid();

            var result = await controller.ManageContactDetails(aatfId);

            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetAatfContact>.That.Matches(c => c.AatfId.Equals(aatfId)))).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public async void ManageContactDetailsGet_GivenActionExecutes_CountriesShouldBeRetrieved()
        {
            ContactDataAccessSetup(true);
            var result = await controller.ManageContactDetails(A.Dummy<Guid>());

            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetCountries>.That.Matches(c => c.UKRegionsOnly.Equals(false)))).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public async void ManageContactDetailsGet_GivenUnauthorizedAccess_HttpForbiddenReturned()
        {
            var result = await controller.ManageContactDetails(A.Dummy<Guid>());

            Assert.IsType<HttpForbiddenResult>(result);
        }

        [Fact]
        public async void ManageContactDetailsPost_OnSubmit_PageRedirectsToSiteList()
        {
            var httpContext = new HttpContextMocker();
            httpContext.AttachToController(controller);

            var aatfId = Guid.NewGuid();

            var viewModel = new AatfEditContactAddressViewModel
            {
                AatfId = aatfId,
            };

            httpContext.RouteData.Values.Add("id", aatfId);

            var helper = A.Fake<UrlHelper>();
            controller.Url = helper;

            A.CallTo(() => helper.Action("Details", new { Id = viewModel.AatfId })).Returns("aatfUrl");

            var result = await controller.ManageContactDetails(viewModel) as RedirectResult;

            result.Url.Should().Be("#contactDetails");
        }

        [Fact]
        public async void ManageContactDetailsPost_GivenValidViewModel_ApiSendShouldBeCalled()
        {
            var model = new AatfEditContactAddressViewModel();
            var request = new EditAatfContact();

            A.CallTo(() => requestCreator.ViewModelToRequest(model)).Returns(request);

            controller.Url = new UrlHelper(A.Fake<RequestContext>(), A.Fake<RouteCollection>());

            await controller.ManageContactDetails(model);

            A.CallTo(() => weeeClient.SendAsync(A<string>._, request)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public async void ManageContactDetailsPost_GivenInvalidViewModel_ApiShouldBeCalled()
        {
            var model = new AatfEditContactAddressViewModel() { ContactData = new AatfContactData() };
            controller.ModelState.AddModelError("error", "error");

            await controller.ManageContactDetails(model);

            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetCountries>._)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public async void ManageContactDetailsPost_GivenInvalidViewModel_CountriesShouldBeAttached()
        {
            var model = new AatfEditContactAddressViewModel() { ContactData = new AatfContactData() };
            controller.ModelState.AddModelError("error", "error");

            var countryGuid = Guid.NewGuid();
            var countryName = "MyCountryName";
            var countryList = new List<CountryData>() { new CountryData() { Id = countryGuid, Name = countryName } };

            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetCountries>._)).Returns(countryList);

            var result = await controller.ManageContactDetails(model) as ViewResult;
            var viewModel = result.Model as AatfEditContactAddressViewModel;
            viewModel.ContactData.AddressData.Countries.Should().NotBeNull();
            viewModel.ContactData.AddressData.Countries.Count().Should().Be(1);
            viewModel.ContactData.AddressData.Countries.ElementAt(0).Id.Should().Be(countryGuid);
            viewModel.ContactData.AddressData.Countries.ElementAt(0).Name.Should().Be(countryName);
        }

        [Fact]
        public async void ManageContactDetailsPost_GivenInvalidViewModel_BreadcrumbShouldBeSet()
        {
            var aatfId = Guid.NewGuid();
            var model = new AatfEditContactAddressViewModel() { AatfId = aatfId, ContactData = new AatfContactData() };
            controller.ModelState.AddModelError("error", "error");

            await controller.ManageContactDetails(model);

            breadcrumbService.InternalActivity.Should().Be(InternalUserActivity.ManageAatfs);
        }

        [Fact]
        public void GenerateAddress_GivenAddressData_LongAddressNameShouldBeCreatedCorrectly()
        {
            var siteAddress = new Core.Shared.AddressData()
            {
                Address1 = "Site address 1",
                Address2 = "Site address 2",
                TownOrCity = "Site town",
                CountyOrRegion = "Site county",
                Postcode = "GU22 7UY",
                CountryId = Guid.NewGuid(),
                CountryName = "Site country"
            };
            var siteAddressLong = "Site address 1<br/>Site address 2<br/>Site town<br/>Site county<br/>GU22 7UY<br/>Site country";

            var siteAddressWithoutAddress2 = new Core.Shared.AddressData()
            {
                Address1 = "Site address 1",
                Address2 = null,
                TownOrCity = "Site town",
                CountyOrRegion = "Site county",
                Postcode = "GU22 7UY",
                CountryId = Guid.NewGuid(),
                CountryName = "Site country"
            };
            var siteAddressWithoutAddress2Long = "Site address 1<br/>Site town<br/>Site county<br/>GU22 7UY<br/>Site country";

            var siteAddressWithoutCounty = new Core.Shared.AddressData()
            {
                Address1 = "Site address 1",
                Address2 = "Site address 2",
                TownOrCity = "Site town",
                CountyOrRegion = null,
                Postcode = "GU22 7UY",
                CountryId = Guid.NewGuid(),
                CountryName = "Site country"
            };
            var siteAddressWithoutCountyLong = "Site address 1<br/>Site address 2<br/>Site town<br/>GU22 7UY<br/>Site country";

            var siteAddressWithoutPostcode = new Core.Shared.AddressData()
            {
                Address1 = "Site address 1",
                Address2 = "Site address 2",
                TownOrCity = "Site town",
                CountyOrRegion = "Site county",
                Postcode = null,
                CountryId = Guid.NewGuid(),
                CountryName = "Site country"
            };
            var siteAddressWithoutPostcodeLong = "Site address 1<br/>Site address 2<br/>Site town<br/>Site county<br/>Site country";

            var result = controller.GenerateAddress(siteAddress);
            var resultWithoutAddress2 = controller.GenerateAddress(siteAddressWithoutAddress2);
            var resultWithoutCounty = controller.GenerateAddress(siteAddressWithoutCounty);
            var resultWithoutPostcode = controller.GenerateAddress(siteAddressWithoutPostcode);

            result.Should().Be(siteAddressLong);
            resultWithoutAddress2.Should().Be(siteAddressWithoutAddress2Long);
            resultWithoutCounty.Should().Be(siteAddressWithoutCountyLong);
            resultWithoutPostcode.Should().Be(siteAddressWithoutPostcodeLong);
        }

        private void ContactDataAccessSetup(bool canEdit)
        {
            var contact = new AatfContactData()
            {
                CanEditContactDetails = canEdit
            };

            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetAatfContact>._))
                .Returns(contact);
        }
    }
}
