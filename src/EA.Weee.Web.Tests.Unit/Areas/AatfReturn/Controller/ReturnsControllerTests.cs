﻿namespace EA.Weee.Web.Tests.Unit.Areas.AatfReturn.Controller
{
    using System;
    using System.Collections.Generic;
    using System.Web.Mvc;
    using Api.Client;
    using Constant;
    using Core.AatfReturn;
    using FakeItEasy;
    using FluentAssertions;
    using Infrastructure;
    using Prsd.Core.Mapper;
    using Services;
    using Services.Caching;
    using Web.Areas.AatfReturn.Controllers;
    using Web.Areas.AatfReturn.ViewModels;
    using Weee.Requests.AatfReturn;
    using Xunit;

    public class ReturnsControllerTests
    {
        private readonly IWeeeClient weeeClient;
        private readonly ReturnsController controller;
        private readonly BreadcrumbService breadcrumb;
        private readonly IMapper mapper;

        public ReturnsControllerTests()
        {
            weeeClient = A.Fake<IWeeeClient>();
            breadcrumb = A.Fake<BreadcrumbService>();
            mapper = A.Fake<IMapper>();

            controller = new ReturnsController(() => weeeClient, breadcrumb, A.Fake<IWeeeCache>(), mapper);
        }

        [Fact]
        public void ReturnsControllerInheritsAatfReturnBaseController()
        {
            typeof(ReturnsController).BaseType.Name.Should().Be(typeof(AatfReturnBaseController).Name);
        }

        [Fact]
        public async void IndexGet_GivenOrganisation_DefaultViewShouldBeReturned()
        {
            var result = await controller.Index(A.Dummy<Guid>()) as ViewResult;

            result.ViewName.Should().BeEmpty();
        }

        [Fact]
        public async void IndexGet_GivenOrganisation_BreadcrumbShouldBeSet()
        {
            await controller.Index(A.Dummy<Guid>());

            Assert.Equal(breadcrumb.ExternalActivity, BreadCrumbConstant.AatfReturn);
        }

        [Fact]
        public async void IndexGet_GivenOrganisation_ReturnsShouldBeRetrieved()
        {
            var organisationId = Guid.NewGuid();

            await controller.Index(organisationId);

            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetReturns>.That.Matches(r => r.OrganisationId.Equals(organisationId))))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public async void IndexGet_GivenOrganisation_ReturnsViewModelShouldBeBuilt()
        {
            var returns = new List<ReturnData>();

            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetReturns>._)).Returns(returns);

            await controller.Index(A.Dummy<Guid>());

            A.CallTo(() => mapper.Map<ReturnsViewModel>(returns)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public async void IndexGet_GivenOrganisation_ReturnsViewModelShouldBeReturned()
        {
            var model = new ReturnsViewModel();
            var organisationId = Guid.NewGuid();

            A.CallTo(() => mapper.Map<ReturnsViewModel>(A<IList<ReturnData>>._)).Returns(model);

            var result = await controller.Index(organisationId) as ViewResult;

            var returnedModel = (ReturnsViewModel)model;

            returnedModel.Should().Be(model);
            returnedModel.OrganisationId.Should().Be(organisationId);
        }

        [Fact]
        public async void IndexPost_GivenOrganisationId_ReturnShouldBeCreated()
        {
            var model = new ReturnsViewModel() { OrganisationId = Guid.NewGuid() };

            await controller.Index(model);

            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<AddReturn>.That.Matches(c => c.OrganisationId.Equals(model.OrganisationId))))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public async void IndexPost_GivenOrganisationId_UserShouldBeRedirectedToOptions()
        {
            var model = new ReturnsViewModel() { OrganisationId = Guid.NewGuid() };
            var returnId = Guid.NewGuid();

            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<AddReturn>._)).Returns(returnId);

            var redirectResult = await controller.Index(model) as RedirectToRouteResult;

            redirectResult.RouteName.Should().Be(AatfRedirect.SelectReportOptionsRouteName);
            redirectResult.RouteValues["action"].Should().Be("Index");
            redirectResult.RouteValues["organisationId"].Should().Be(model.OrganisationId);
            redirectResult.RouteValues["returnId"].Should().Be(returnId);
        }
    }
}