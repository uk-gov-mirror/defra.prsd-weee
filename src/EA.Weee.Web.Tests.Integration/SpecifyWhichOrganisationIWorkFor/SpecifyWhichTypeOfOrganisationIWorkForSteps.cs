﻿namespace EA.Weee.Web.Tests.Integration.SpecifyWhichOrganisationIWorkFor
{
    using System;
    using TechTalk.SpecFlow;
    using System.Configuration;
    using System.Web.Mvc;
    using Api.Client;
    using Controllers;
    using Requests;
    using ViewModels.Organisation.Type;
    using ViewModels.Shared;
    using Xunit;

    [Binding]
    public class SpecifyWhichOrganisationIWorkForSteps
    {
        [Given(@"I select the sole trader or indivdual option")]
        public void GivenISelectTheSoleTraderOrIndivdualOption()
        {
            ScenarioContext.Current[typeof(OrganisationTypeViewModel).Name] =
                OrganisationType(OrganisationTypeEnum.SoleTrader);
        }

        [Given(@"I selected the partnership option")]
        public void GivenISelectedThePartnershipOption()
        {
            ScenarioContext.Current[typeof(OrganisationTypeViewModel).Name] =
                OrganisationType(OrganisationTypeEnum.Partnership);
        }

        [Given(@"I selected the registered company option")]
        public void GivenISelectedTheRegisteredCompanyOption()
        {
            ScenarioContext.Current[typeof(OrganisationTypeViewModel).Name] =
                OrganisationType(OrganisationTypeEnum.RegisteredCompany);
        }

        [When(@"I select continue")]
        public void WhenISelectContinue()
        {
            var controller = OrganisationRegistrationController();              

            var model = (OrganisationTypeViewModel)ScenarioContext.Current[typeof(OrganisationTypeViewModel).Name];

            ScenarioContext.Current["Result"] = controller.Type(model);
        }

        [Then(@"I should by redirected to the sole trader or individual page")]
        public void ThenIShouldByRedirectedToTheSoleTraderOrIndividualPage()
        {
            var result = (RedirectToRouteResult)ScenarioContext.Current["Result"];

            Assert.Equal("OrganisationRegistration", result.RouteValues["controller"]);
            Assert.Equal("SoleTraderDetails", result.RouteValues["action"]);
        }

        [Then(@"I should be redirected to the partnership details page")]
        public void ThenIShouldBeRedirectedToThePartnershipDetailsPage()
        {
            var result = (RedirectToRouteResult)ScenarioContext.Current["Result"];

            Assert.Equal("OrganisationRegistration", result.RouteValues["controller"]);
            Assert.Equal("PartnershipDetails", result.RouteValues["action"]);
        }

        [Then(@"I should be redirected to the registered company details page")]
        public void ThenIShouldBeRedirectedToTheRegisteredCompanyDetailsPage()
        {
            var result = (RedirectToRouteResult)ScenarioContext.Current["Result"];

            Assert.Equal("OrganisationRegistration", result.RouteValues["controller"]);
            Assert.Equal("RegisteredCompanyDetails", result.RouteValues["action"]);
        }

        private OrganisationRegistrationController OrganisationRegistrationController()
        {
            return new OrganisationRegistrationController(
                    () => new WeeeClient(ConfigurationManager.AppSettings["Weee.ApiUrl"]),
                    new SoleTraderDetailsRequestCreator());
        }

        private OrganisationTypeViewModel OrganisationType(OrganisationTypeEnum selectedOption)
        {
            return new OrganisationTypeViewModel
            {
                OrganisationTypes = RadioButtonStringCollectionViewModel.CreateFromEnum(selectedOption)
            };
        }
    }
}
