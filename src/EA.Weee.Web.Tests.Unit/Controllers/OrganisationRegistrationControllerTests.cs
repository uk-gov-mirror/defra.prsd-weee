﻿namespace EA.Weee.Web.Tests.Unit.Controllers
{
    using System;
    using System.Web.Mvc;
    using Api.Client;
    using FakeItEasy;
    using Prsd.Core.Mediator;
    using ViewModels.OrganisationRegistration;
    using Web.Controllers;
    using Web.Requests;
    using Weee.Requests.Organisations;
    using Xunit;

    public class OrganisationRegistrationControllerTests
    {
        private readonly IWeeeClient apiClient;
        private readonly IPrincipalPlaceOfBusinessRequestCreator principalPlaceOfBusinessRequestCreator;

        public OrganisationRegistrationControllerTests()
        {
            apiClient = A.Fake<IWeeeClient>();
            principalPlaceOfBusinessRequestCreator = A.Fake<IPrincipalPlaceOfBusinessRequestCreator>();
        }

        [Fact]
        public async void GetPrincipalPlaceOfBusiness_OrganisationIdIsInvalid_ExceptionShouldNotBeCaught()
        {
            A.CallTo(() => apiClient.SendAsync(A<string>._, A<IRequest<OrganisationData>>._))
                .Throws<Exception>();

            await Assert.ThrowsAnyAsync<Exception>(() => OrganisationRegistrationController().PrincipalPlaceOfBusiness(A<Guid>._));
        }

        [Fact]
        public async void GetPrincipalPlaceOfBusiness_OrganisationIdIsValidAndOrganisationTypeIsRegisteredCompany_ShouldThrowException()
        {
            A.CallTo(() => apiClient.SendAsync(A<string>._, A<IRequest<OrganisationData>>._))
                .Returns(new OrganisationData
                {
                    OrganisationType = OrganisationType.RegisteredCompany
                });

            await Assert.ThrowsAnyAsync<Exception>(() => OrganisationRegistrationController().PrincipalPlaceOfBusiness(A<Guid>._));
        }

        [Theory]
        [InlineData(OrganisationType.SoleTraderOrIndividual)]
        [InlineData(OrganisationType.Partnership)]
        public async void GetPrincipalPlaceOfBusiness_OrganisationIdIsValidAndOrganisationTypeIsSoleTraderOrPartnership_ShouldReturnViewWithModel(OrganisationType type)
        {
            A.CallTo(() => apiClient.SendAsync(A<string>._, A<IRequest<OrganisationData>>._))
                .Returns(new OrganisationData
                {
                    OrganisationType = type // Not registered company
                });

            var result = await OrganisationRegistrationController().PrincipalPlaceOfBusiness(A<Guid>._);
            var model = ((ViewResult)result).Model;

            Assert.NotNull(model);
            Assert.IsType<PrincipalPlaceOfBusinessViewModel>(model);
        }

        [Fact]
        public async void PostPrincipalPlaceOfBusiness_ModelStateIsInvalid_ShouldReturnViewWithInvalidModel()
        {
            var model = new PrincipalPlaceOfBusinessViewModel();
            var controller = OrganisationRegistrationController();
            controller.ModelState.AddModelError("Key", "Error"); // To make the model state invalid

            var result = await controller.PrincipalPlaceOfBusiness(model);
            var returnedModel = ((ViewResult)result).Model;

            Assert.Equal(model, returnedModel);
        }

        [Fact]
        public async void PostPrincipalPlaceOfBusiness_PrincipalPlaceOfBusinessIsValid_ShouldSubmitDetailsToApi()
        {
            var model = new PrincipalPlaceOfBusinessViewModel();

            await OrganisationRegistrationController().PrincipalPlaceOfBusiness(model);

            A.CallTo(() => apiClient.SendAsync(A<string>._, A<AddAddressToOrganisation>._))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public async void PostPrincipalPlaceOfBusiness_PrincipalPlaceOfBusinessIsValid_ShouldRedirectToPageWithOptionsForServiceOfNoticeAddress()
        {
            var model = new PrincipalPlaceOfBusinessViewModel();

            var result = await OrganisationRegistrationController().PrincipalPlaceOfBusiness(model);
            var redirectToRouteResult = ((RedirectToRouteResult)result);

            Assert.Equal("OrganisationRegistration", redirectToRouteResult.RouteValues["controller"]);
            Assert.Equal("ServiceOfNoticeOptions", redirectToRouteResult.RouteValues["action"]);
        }

        private OrganisationRegistrationController OrganisationRegistrationController()
        {
            return new OrganisationRegistrationController(() => apiClient, principalPlaceOfBusinessRequestCreator);
        }
    }
}
