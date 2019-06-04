﻿namespace EA.Weee.Web.Tests.Unit.Areas.Admin.ViewModels
{
    using EA.Weee.Core.DataStandards;
    using EA.Weee.Web.Areas.Admin.ViewModels.Organisation;
    using FluentAssertions;
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Reflection;
    using Xunit;

    public class EditSoleTraderOrIndividualOrganisationDetailsViewModelTests
    {
        private readonly Type modelType;

        public EditSoleTraderOrIndividualOrganisationDetailsViewModelTests()
        {
            modelType = typeof(EditSoleTraderOrIndividualOrganisationDetailsViewModel);
        }

        [Fact]
        public void GivenModel_BusinessTradingNameShouldHaveRequiredFieldAttribute()
        {
            GetProperty("BusinessTradingName").Should().BeDecoratedWith<RequiredAttribute>();
        }

        [Fact]
        public void GivenModel_BusinessTradingNameShouldHaveDisplayNameAttribute()
        {
            var property = GetProperty("BusinessTradingName");
            GetProperty("BusinessTradingName").Should().BeDecoratedWith<DisplayNameAttribute>().Which.DisplayName.Equals("Business trading name");
        }

        [Fact]
        public void GivenModel_BusinessTradingNameShouldHaveStringLengthAttribute()
        {
            GetProperty("BusinessTradingName").Should().BeDecoratedWith<StringLengthAttribute>().Which.MaximumLength
                .Equals(CommonMaxFieldLengths.DefaultString);
        }

        private PropertyInfo GetProperty(string name)
        {
            return modelType.GetProperty(name);
        }
    }
}
