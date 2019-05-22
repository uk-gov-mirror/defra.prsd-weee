﻿namespace EA.Weee.Web.Tests.Unit.Areas.AatfReturn.Mapping.ToViewModel
{
    using System;
    using Core.AatfReturn;
    using Core.DataReturns;
    using EA.Weee.Core.Organisations;
    using FakeItEasy;
    using FluentAssertions;
    using Web.Areas.AatfReturn.Mappings.ToViewModel;
    using Xunit;

    public class ReturnToSubmittedReturnViewModelMapTests
    {
        private readonly ReturnToSubmittedReturnViewModelMap map;

        public ReturnToSubmittedReturnViewModelMapTests()
        {
            map = new ReturnToSubmittedReturnViewModelMap();
        }

        [Fact]
        public void Map_GivenNullSource_ArgumentNullExceptionExpected()
        {
            Action action = () => map.Map(null);

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Map_GivenValidSource_PropertiesShouldBeMapped()
        {
            var id = Guid.NewGuid();

            var quarterWindow = new QuarterWindow(new DateTime(2019, 1, 1), new DateTime(2019, 3, 31));
            var returnData = new ReturnData() { Id = id, Quarter = new Quarter(2019, QuarterType.Q1), QuarterWindow = quarterWindow, ReturnOperatorData = new OperatorData(Guid.NewGuid(), "operator", A.Fake<OrganisationData>(), Guid.NewGuid()) };

            var result = map.Map(returnData);

            result.Quarter.Should().Be("Q1");
            result.Year.Should().Be("2019");
            result.Period.Should().Be("Q1 Jan - Mar 2019");
            result.OrgansationId.Should().Be(returnData.ReturnOperatorData.OrganisationId);
        }
    }
}
