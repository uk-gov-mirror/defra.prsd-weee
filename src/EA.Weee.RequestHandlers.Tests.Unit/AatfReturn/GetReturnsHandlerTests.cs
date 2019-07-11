﻿namespace EA.Weee.RequestHandlers.Tests.Unit.AatfReturn
{
    using Core.AatfReturn;
    using Core.DataReturns;
    using Domain.AatfReturn;
    using FakeItEasy;
    using FluentAssertions;
    using RequestHandlers.AatfReturn;
    using RequestHandlers.Factories;
    using Requests.AatfReturn;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security;
    using System.Threading.Tasks;
    using Weee.Tests.Core;
    using Xunit;

    public class GetReturnsHandlerTests
    {
        private GetReturnsHandler handler;
        private readonly IGetPopulatedReturn populatedReturn;
        private readonly IReturnDataAccess returnDataAccess;
        private readonly IReturnFactory returnFactory;
        private readonly IQuarterWindowFactory quarterWindowFactory;

        public GetReturnsHandlerTests()
        {
            populatedReturn = A.Fake<IGetPopulatedReturn>();
            returnDataAccess = A.Fake<IReturnDataAccess>();
            returnFactory = A.Fake<IReturnFactory>();
            quarterWindowFactory = A.Fake<IQuarterWindowFactory>();

            handler = new GetReturnsHandler(new AuthorizationBuilder()
                .AllowExternalAreaAccess()
                .AllowOrganisationAccess().Build(),
                populatedReturn,
                returnDataAccess,
                returnFactory,
                quarterWindowFactory);
        }

        [Fact]
        public async Task HandleAsync_NoExternalAccess_ThrowsSecurityException()
        {
            var authorization = new AuthorizationBuilder().DenyExternalAreaAccess().Build();

            handler = new GetReturnsHandler(authorization,
                A.Dummy<IGetPopulatedReturn>(),
                A.Dummy<IReturnDataAccess>(),
                A.Dummy<IReturnFactory>(),
                A.Dummy<IQuarterWindowFactory>());

            Func<Task> action = async () => await handler.HandleAsync(A.Dummy<GetReturns>());

            await action.Should().ThrowAsync<SecurityException>();
        }

        [Fact]
        public async Task HandleAsync_GivenOrganisation_ReturnsForOrganisationShouldBeRetrieved()
        {
            var organisationId = Guid.NewGuid();

            var result = await handler.HandleAsync(new GetReturns(organisationId, Core.AatfReturn.FacilityType.Aatf));

            A.CallTo(() => returnDataAccess.GetByOrganisationId(organisationId)).MustHaveHappened(Repeated.Exactly.Once);
        }

        public async Task HandleAsync_GivenOrganisation_GetPopulatedReturnsShouldBeCalled()
        {
            var organisationId = Guid.NewGuid();
            var returns = A.CollectionOfFake<Return>(2);

            foreach (var @return in returns)
            {
                @return.FacilityType = Domain.AatfReturn.FacilityType.Aatf;
            }

            A.CallTo(() => returns.ElementAt(0).Id).Returns(Guid.NewGuid());
            A.CallTo(() => returns.ElementAt(1).Id).Returns(Guid.NewGuid());

            A.CallTo(() => returnDataAccess.GetByOrganisationId(organisationId)).Returns(returns);

            var result = await handler.HandleAsync(new GetReturns(organisationId, Core.AatfReturn.FacilityType.Aatf));

            A.CallTo((() => populatedReturn.GetReturnData(returns.ElementAt(0).Id, false))).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo((() => populatedReturn.GetReturnData(returns.ElementAt(1).Id, false))).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public async Task HandleAsync_GivenOrganisation_PopulatedReturnsShouldBeReturned()
        {
            var returns = A.CollectionOfFake<Return>(2);
            var returnData = A.CollectionOfFake<ReturnData>(2).ToArray();

            A.CallTo(() => returnDataAccess.GetByOrganisationId(A<Guid>._)).Returns(returns);
            A.CallTo((() => populatedReturn.GetReturnData(A<Guid>._, A<bool>._))).ReturnsNextFromSequence(returnData);

            var result = await handler.HandleAsync(A.Dummy<GetReturns>());

            result.ReturnsList.Should().Contain(returnData.ElementAt(0));
            result.ReturnsList.Should().Contain(returnData.ElementAt(1));
            result.ReturnsList.Should().HaveCount(2);
        }

        [Theory]
        [InlineData(Core.AatfReturn.FacilityType.Aatf)]
        [InlineData(Core.AatfReturn.FacilityType.Ae)]
        public async Task HandleAsync_GivenOrganisation_ReturnQuarterShouldBeRetrieved(Core.AatfReturn.FacilityType facilityType)
        {
            var message = new GetReturns(Guid.NewGuid(), facilityType);

            var result = await handler.HandleAsync(message);

            A.CallTo(() => returnFactory.GetReturnQuarter(message.OrganisationId, message.Facility)).MustHaveHappenedOnceExactly();
        }

        [Theory]
        [InlineData(Core.AatfReturn.FacilityType.Aatf)]
        [InlineData(Core.AatfReturn.FacilityType.Ae)]
        public async Task HandleAsync_GivenOrganisation_ReturnQuarterShouldBeMapped(Core.AatfReturn.FacilityType facilityType)
        {
            var message = new GetReturns(Guid.NewGuid(), facilityType);
            var returnQuarter = new Quarter(2019, QuarterType.Q1);

            A.CallTo(() => returnFactory.GetReturnQuarter(message.OrganisationId, message.Facility)).Returns(returnQuarter);

            var result = await handler.HandleAsync(message);

            result.ReturnQuarter.Should().Be(returnQuarter);
        }

        [Theory]
        [InlineData(Core.AatfReturn.FacilityType.Aatf)]
        [InlineData(Core.AatfReturn.FacilityType.Ae)]
        public async Task HandleAsync_GivenFacilityType_CorrectReturnsReturned(Core.AatfReturn.FacilityType facilityType)
        {
            var organisationId = Guid.NewGuid();
            var returns = A.CollectionOfFake<Return>(2);
            List<Domain.DataReturns.QuarterWindow> openQuarters = new List<Domain.DataReturns.QuarterWindow>()
            {
                new Domain.DataReturns.QuarterWindow(DateTime.Now, DateTime.Now.AddMonths(3), Domain.DataReturns.QuarterType.Q1)
            };

            Domain.DataReturns.QuarterWindow nextQuarter = new Domain.DataReturns.QuarterWindow(DateTime.Now, DateTime.Now.AddMonths(3), Domain.DataReturns.QuarterType.Q1);

            foreach (var @return in returns)
            {
                @return.FacilityType = Domain.AatfReturn.FacilityType.Ae;
            }

            var aatfReturn = A.Fake<Return>();
            aatfReturn.FacilityType = Domain.AatfReturn.FacilityType.Aatf;
            returns.Add(aatfReturn);

            A.CallTo(() => returns.ElementAt(0).Id).Returns(Guid.NewGuid());
            A.CallTo(() => returns.ElementAt(1).Id).Returns(Guid.NewGuid());
            A.CallTo(() => returns.ElementAt(2).Id).Returns(Guid.NewGuid());

            A.CallTo(() => returnDataAccess.GetByOrganisationId(organisationId)).Returns(returns);

            A.CallTo(() => quarterWindowFactory.GetQuarterWindowsForDate(DateTime.Now)).Returns(openQuarters);
            A.CallTo(() => quarterWindowFactory.GetNextQuarterWindow(Domain.DataReturns.QuarterType.Q1, 2019)).Returns(nextQuarter);

            var result = await handler.HandleAsync(new GetReturns(organisationId, facilityType));

            if (facilityType == Core.AatfReturn.FacilityType.Ae)
            {
                A.CallTo((() => populatedReturn.GetReturnData(returns.ElementAt(0).Id, false))).MustHaveHappened(Repeated.Exactly.Once);
                A.CallTo((() => populatedReturn.GetReturnData(returns.ElementAt(1).Id, false))).MustHaveHappened(Repeated.Exactly.Once);
            }

            if (facilityType == Core.AatfReturn.FacilityType.Aatf)
            {
                A.CallTo((() => populatedReturn.GetReturnData(returns.ElementAt(2).Id, false))).MustHaveHappened(Repeated.Exactly.Once);
            }
        }
    }
}
