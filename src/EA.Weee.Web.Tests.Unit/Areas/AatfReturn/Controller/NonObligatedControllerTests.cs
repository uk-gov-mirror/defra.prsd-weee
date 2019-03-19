﻿namespace EA.Weee.Web.Tests.Unit.Areas.AatfReturn.Controller
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;
    using Api.Client;
    using Constant;
    using Core.AatfReturn;
    using Core.Scheme;
    using EA.Prsd.Core.Mapper;
    using EA.Weee.Core.Helpers;
    using EA.Weee.Requests.AatfReturn;
    using EA.Weee.Web.Areas.AatfReturn.Mappings.ToViewModel;
    using EA.Weee.Web.Areas.AatfReturn.ViewModels.Validation;
    using FakeItEasy;
    using FluentAssertions;
    using FluentValidation.Results;
    using Services;
    using Services.Caching;
    using Web.Areas.AatfReturn.Controllers;
    using Web.Areas.AatfReturn.Requests;
    using Web.Areas.AatfReturn.ViewModels;
    using Web.Controllers.Base;
    using Weee.Requests.AatfReturn.NonObligated;
    using Xunit;

    public class NonObligatedControllerTests
    {
        private readonly IWeeeClient weeeClient;
        private readonly INonObligatedWeeeRequestCreator requestCreator;
        private readonly NonObligatedController controller;
        private readonly BreadcrumbService breadcrumb;
        private readonly INonObligatedValuesViewModelValidatorWrapper validator;
        private readonly IMap<ReturnToNonObligatedValuesViewModelMapTransfer, NonObligatedValuesViewModel> mapper;
        private readonly IWeeeCache cache;
        private readonly ICategoryValueTotalCalculator calculator;

        public NonObligatedControllerTests()
        {
            weeeClient = A.Fake<IWeeeClient>();
            requestCreator = A.Fake<INonObligatedWeeeRequestCreator>();
            breadcrumb = A.Fake<BreadcrumbService>();
            validator = A.Fake<INonObligatedValuesViewModelValidatorWrapper>();
            cache = A.Fake<IWeeeCache>();
            calculator = A.Fake<ICategoryValueTotalCalculator>();

            mapper = A.Fake<IMap<ReturnToNonObligatedValuesViewModelMapTransfer, NonObligatedValuesViewModel>>();
            controller = new NonObligatedController(cache, breadcrumb, () => weeeClient, requestCreator, validator, mapper);
        }

        [Fact]
        public void CheckNonObligatedControllerInheritsExternalSiteController()
        {
            typeof(NonObligatedController).BaseType.Name.Should().Be(typeof(AatfReturnBaseController).Name);
        }

        [Fact]
        public async void IndexPost_GivenValidViewModel_ApiSendShouldBeCalled()
        {
            var model = new NonObligatedValuesViewModel(calculator);
            var request = new AddNonObligated();

            A.CallTo(() => requestCreator.ViewModelToRequest(model)).Returns(request);

            await controller.Index(model);

            A.CallTo(() => weeeClient.SendAsync(A<string>._, request)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public async void IndexPost_GivenInvalidViewModel_ApiShouldNotBeCalled()
        {
            controller.ModelState.AddModelError("error", "error");

            await controller.Index(A.Dummy<NonObligatedValuesViewModel>());

            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<AddNonObligated>._)).MustNotHaveHappened();
        }

        [Fact]
        public async void IndexGet_GivenValidViewModel_BreadcrumbShouldBeSet()
        {
            var organisationId = Guid.NewGuid();
            var @return = A.Fake<ReturnData>();
            var schemeInfo = A.Fake<SchemePublicInfo>();
            var operatorData = A.Fake<OperatorData>();
            const string orgName = "orgName";

            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetReturn>._)).Returns(@return);
            A.CallTo(() => operatorData.OrganisationId).Returns(organisationId);
            A.CallTo(() => @return.ReturnOperatorData).Returns(operatorData);
            A.CallTo(() => cache.FetchOrganisationName(organisationId)).Returns(orgName);
            A.CallTo(() => cache.FetchSchemePublicInfo(organisationId)).Returns(schemeInfo);

            await controller.Index(A.Dummy<Guid>(), A.Dummy<bool>());

            breadcrumb.ExternalActivity.Should().Be(BreadCrumbConstant.AatfReturn);
            breadcrumb.ExternalOrganisation.Should().Be(orgName);
            breadcrumb.SchemeInfo.Should().Be(schemeInfo);
        }

        [Fact]
        public async void IndexPost_GivenNonObligatedValuesAreSubmitted_PageRedirectShouldBeCorrect()
        {
            var model = new NonObligatedValuesViewModel(calculator) { Dcf = false };

            var result = await controller.Index(model) as RedirectToRouteResult;

            result.RouteValues["action"].Should().Be("Index");
            result.RouteValues["controller"].Should().Be("AatfTaskList");
            result.RouteValues["area"].Should().Be("AatfReturn");
        }

        [Fact]
        public async void IndexPost_GivenNonObligatedDcfValuesAreSubmitted_PageRedirectShouldBeCorrect()
        {
            var model = new NonObligatedValuesViewModel(calculator) { Dcf = true };

            var result = await controller.Index(model) as RedirectToRouteResult;

            result.RouteValues["action"].Should().Be("Index");
            result.RouteValues["controller"].Should().Be("AatfTaskList");
            result.RouteValues["area"].Should().Be("AatfReturn");
        }

        [Fact]
        public async void IndexGet_GivenAction_DefaultViewShouldBeReturned()
        {
            var @return = A.Fake<ReturnData>();
            A.CallTo(() => @return.ReturnOperatorData).Returns(A.Fake<OperatorData>());

            var result = await controller.Index(A.Dummy<Guid>(), A.Dummy<bool>()) as ViewResult;

            result.ViewName.Should().BeEmpty();
        }

        [Fact]
        public async void IndexPost_GivenValidViewModel_ValidatorShouldReturnAValidResult()
        {
            var result = new ValidationResult();

            A.CallTo(() => validator.Validate(A<NonObligatedValuesViewModel>._, A<ReturnData>._)).Returns(result);

            await controller.Index(A.Fake<NonObligatedValuesViewModel>());

            controller.ModelState.IsValid.Should().BeTrue();
        }

        [Fact]
        public async void IndexPost_GivenInvalidViewModel_ValidatorShouldReturnAnInvalidResult()
        {
            var result = new ValidationResult();
            result.Errors.Add(new ValidationFailure("property", "error"));

            A.CallTo(() => validator.Validate(A<NonObligatedValuesViewModel>._, A<ReturnData>._)).Returns(result);

            await controller.Index(A.Fake<NonObligatedValuesViewModel>());

            controller.ModelState.IsValid.Should().BeFalse();
            controller.ModelState.Should().ContainKey("property");
            controller.ModelState.Count(c => c.Value.Errors.Any(d => d.ErrorMessage == "error")).Should().Be(1);
        }

        [Fact]
        public async void IndexPost_GivenValidViewModel_ValidatorShouldBeCalled()
        {
            var model = new NonObligatedValuesViewModel(calculator);
            var returnData = new ReturnData();

            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetReturn>._)).Returns(returnData);

            await controller.Index(model);

            A.CallTo(() => validator.Validate(model, returnData)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public async void IndexPost_InvalidViewModel_ValidatorShouldNotBeCalled()
        {
            var model = new NonObligatedValuesViewModel(calculator);
            var returnData = new ReturnData();
            controller.ModelState.AddModelError("error", "error");

            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetReturn>._)).Returns(returnData);

            controller.ModelState.AddModelError("error", "error");

            await controller.Index(model);

            A.CallTo(() => validator.Validate(model, returnData)).MustNotHaveHappened();
        }

        [Fact]
        public async void IndexGet_GivenActionExecutes_ApiShouldBeCalled()
        {
            var returnId = Guid.NewGuid();
            var @return = A.Fake<ReturnData>();
            A.CallTo(() => @return.ReturnOperatorData).Returns(A.Fake<OperatorData>());

            await controller.Index(returnId, true);

            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetReturn>.That.Matches(r => r.ReturnId.Equals(returnId))))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public async void IndexGet_GivenReturn_ViewModelShouldBeBuilt()
        {
            var returnId = Guid.NewGuid();
            var organisationId = Guid.NewGuid();
            var @return = A.Fake<ReturnData>();
            const bool dcf = true;

            A.CallTo(() => @return.ReturnOperatorData.OrganisationId).Returns(organisationId);
            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetReturn>.That.Matches(r => r.ReturnId.Equals(returnId)))).Returns(@return);

            await controller.Index(returnId, dcf);

            A.CallTo(() => mapper.Map(A<ReturnToNonObligatedValuesViewModelMapTransfer>.That.Matches(r => r.ReturnData.Equals(@return) && r.OrganisationId.Equals(organisationId) && r.Dcf.Equals(dcf) && r.ReturnId.Equals(returnId)))).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public async void IndexGet_GivenBuiltViewModel_ViewModelShouldBeReturned()
        {
            var model = A.Fake<NonObligatedValuesViewModel>();
            var @return = A.Fake<ReturnData>();

            A.CallTo(() => @return.ReturnOperatorData).Returns(A.Fake<OperatorData>());
            A.CallTo(() => mapper.Map(A<ReturnToNonObligatedValuesViewModelMapTransfer>._)).Returns(model);

            var result = await controller.Index(A.Dummy<Guid>(), A.Dummy<bool>()) as ViewResult;

            result.Model.Should().Be(model);
        }

        [Fact]
        public async void IndexGet_GivenReturnAndPastedValues_CategoryValuesShouldNotBeTheSame()
        {
            var returnId = Guid.NewGuid();
            var organisationId = Guid.NewGuid();
            var @return = A.Fake<ReturnData>();
            var pastedValue = A.Fake<List<NonObligatedCategoryValue>>();

            A.CallTo(() => @return.ReturnOperatorData.OrganisationId).Returns(organisationId);
            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetReturn>.That.Matches(r => r.ReturnId.Equals(returnId)))).Returns(@return);

            var result = await controller.Index(returnId, A.Dummy<bool>()) as ViewResult;

            var viewModel = result.Model as NonObligatedValuesViewModel;

            viewModel.CategoryValues.Should().NotBeSameAs(pastedValue);
        }
    }
}
