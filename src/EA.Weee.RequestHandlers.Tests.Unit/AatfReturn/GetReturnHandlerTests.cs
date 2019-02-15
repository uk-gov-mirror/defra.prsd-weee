﻿namespace EA.Weee.RequestHandlers.Tests.Unit.AatfReturn
{
    using Core.AatfReturn;
    using DataAccess.DataAccess;
    using Domain.AatfReturn;
    using FakeItEasy;
    using FluentAssertions;
    using Prsd.Core.Mapper;
    using RequestHandlers.AatfReturn;
    using Requests.AatfReturn;
    using System;
    using System.Collections.Generic;
    using System.Security;
    using System.Threading.Tasks;
    using Domain.DataReturns;
    using Factories;
    using FluentAssertions.Common;
    using RequestHandlers.AatfReturn.CheckYourReturn;
    using RequestHandlers.Factories;
    using Weee.Tests.Core;
    using Xunit;

    public class GetReturnHandlerTests
    {
        private GetReturnHandler handler;
        private readonly IReturnDataAccess returnDataAccess;
        private readonly IOrganisationDataAccess organisationDataAccess;
        private readonly IMap<ReturnQuarterWindow, ReturnData> mapper;
        private readonly IQuarterWindowFactory quarterWindowFactory;
        private readonly IFetchNonObligatedWeeeForReturnDataAccess fetchNonObligatedWeeeDataAccess;

        public GetReturnHandlerTests()
        {
            returnDataAccess = A.Fake<IReturnDataAccess>();
            organisationDataAccess = A.Fake<IOrganisationDataAccess>();
            mapper = A.Fake<IMap<ReturnQuarterWindow, ReturnData>>();
            quarterWindowFactory = A.Fake<IQuarterWindowFactory>();
            fetchNonObligatedWeeeDataAccess = A.Fake<IFetchNonObligatedWeeeForReturnDataAccess>();

            handler = new GetReturnHandler(new AuthorizationBuilder()
                .AllowExternalAreaAccess()
                .AllowOrganisationAccess().Build(),
                returnDataAccess,
                organisationDataAccess,
                mapper,
                quarterWindowFactory,
                fetchNonObligatedWeeeDataAccess);
        }

        [Fact]
        public async Task HandleAsync_NoExternalAccess_ThrowsSecurityException()
        {
            var authorization = new AuthorizationBuilder().DenyExternalAreaAccess().Build();

            handler = new GetReturnHandler(authorization,
                A.Dummy<IReturnDataAccess>(),
                A.Dummy<IOrganisationDataAccess>(),
                A.Dummy<IMap<ReturnQuarterWindow, ReturnData>>(),
                A.Dummy<IQuarterWindowFactory>(),
                A.Dummy<IFetchNonObligatedWeeeForReturnDataAccess>());

            Func<Task> action = async () => await handler.HandleAsync(A.Dummy<GetReturn>());

            await action.Should().ThrowAsync<SecurityException>();
        }

        [Fact]
        public async Task HandleAsync_NoOrganisationAccess_ThrowsSecurityException()
        {
            var authorization = new AuthorizationBuilder().DenyOrganisationAccess().Build();
            var dataAccess = A.Fake<IReturnDataAccess>();

            handler = new GetReturnHandler(authorization,
                A.Dummy<IReturnDataAccess>(),
                A.Dummy<IOrganisationDataAccess>(),
                A.Dummy<IMap<ReturnQuarterWindow, ReturnData>>(),
                A.Dummy<IQuarterWindowFactory>(),
                A.Dummy<IFetchNonObligatedWeeeForReturnDataAccess>());

            Func<Task> action = async () => await handler.HandleAsync(A.Dummy<GetReturn>());

            await action.Should().ThrowAsync<SecurityException>();
        }

        [Fact]
        public async Task HandeAsync_GivenReturn_ReturnShouldBeRetrieved()
        {
            var returnId = Guid.NewGuid();

            var result = await handler.HandleAsync(new GetReturn(returnId));

            A.CallTo(() => returnDataAccess.GetById(returnId)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public async Task HandleAsync_GivenReturn_QuarterWindowShouldBeRetrieved()
        {
            var @return = new Return(A.Fake<Operator>(), A.Fake<Quarter>(), A.Fake<ReturnStatus>());

            A.CallTo(() => returnDataAccess.GetById(A<Guid>._)).Returns(@return);

            var result = await handler.HandleAsync(A.Dummy<GetReturn>());

            A.CallTo(() => quarterWindowFactory.GetAnnualQuarter(@return.Quarter)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public async Task HandleAsync_GivenReturn_NonObligatedValuesShouldBeRetrieved()
        {
            var returnId = Guid.NewGuid();

            var result = await handler.HandleAsync(new GetReturn(returnId));

            A.CallTo(() => fetchNonObligatedWeeeDataAccess.FetchNonObligatedWeeeForReturn(returnId)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public async Task HandleAsync_GivenReturn_MapperShouldBeCalled()
        {
            var @return = new Return(A.Fake<Operator>(), A.Fake<Quarter>(), A.Fake<ReturnStatus>());
            var quarterWindow = new Domain.DataReturns.QuarterWindow(DateTime.MaxValue, DateTime.MaxValue);
            var nonObligatedValues = new List<NonObligatedWeee>();

            A.CallTo(() => returnDataAccess.GetById(A<Guid>._)).Returns(@return);
            A.CallTo(() => quarterWindowFactory.GetAnnualQuarter(A<Quarter>._)).Returns(quarterWindow);
            A.CallTo(() => fetchNonObligatedWeeeDataAccess.FetchNonObligatedWeeeForReturn(A<Guid>._)).Returns(nonObligatedValues);

            await handler.HandleAsync(A.Dummy<GetReturn>());

            A.CallTo(() => mapper.Map(A<ReturnQuarterWindow>.That.Matches(c => c.QuarterWindow.IsSameOrEqualTo(quarterWindow)
                                                                                && c.NonObligatedWeeeList.IsSameOrEqualTo(nonObligatedValues)
                                                                                && c.Return.Equals(@return)))).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public async Task HandleAsync_GivenReturn_MappedObjectShouldBeReturned()
        {
            var returnData = new ReturnData();

            A.CallTo(() => mapper.Map(A<ReturnQuarterWindow>._)).Returns(returnData);

            var result = await handler.HandleAsync(A.Dummy<GetReturn>());

            result.Should().Be(returnData);
        }
    }
}