﻿namespace EA.Weee.RequestHandlers.Tests.Unit.DataReturns.FetchSummaryCsv
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security;
    using System.Text;
    using System.Threading.Tasks;
    using Domain;
    using Domain.Producer;
    using EA.Weee.Core.DataReturns;
    using EA.Weee.Core.Shared;
    using EA.Weee.Domain.DataReturns;
    using EA.Weee.RequestHandlers.DataReturns.FetchSummaryCsv;
    using EA.Weee.RequestHandlers.Security;
    using EA.Weee.Tests.Core;
    using FakeItEasy;
    using Prsd.Core;
    using Xunit;

    public class FetchSummaryCsvTests
    {
        /// <summary>
        /// This test ensures that a SecurityException is thrown when a user with no internal or organisation access
        /// tries to use the handler.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task HandleAsync_WithNoInternalOrOrganisationAccess_ThrowsSecurityException()
        {
            // Arrange
            IWeeeAuthorization authorization = new AuthorizationBuilder()
                .DenyInternalOrOrganisationAccess()
                .Build();

            FetchSummaryCsvHandler handler = new FetchSummaryCsvHandler(
                authorization,
                A.Dummy<CsvWriterFactory>(),
                A.Dummy<IFetchSummaryCsvDataAccess>());

            // Act
            Func<Task<FileInfo>> testCode = async () => await handler.HandleAsync(A.Dummy<Requests.DataReturns.FetchSummaryCsv>());

            // Assert
            await Assert.ThrowsAsync<SecurityException>(testCode);
        }

        /// <summary>
        /// This test ensures that the filename generated by the handler contains the scheme approval number,
        /// the compliance year from the request and the current local time. Note that the forward slashes in the scheme
        /// approval number are removed later.
        /// The expected format is: [scheme approval number]_EEE_WEEE_data_[compliance year]_[DDMMYYYY_HHMM].csv
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task HandleAsync_Always_CreatesFileNameWithSchemeApprovalNumberComplianceYearAndCurrentTime()
        {
            // Arrange
            Domain.Scheme.Scheme scheme = new Domain.Scheme.Scheme(A.Dummy<Domain.Organisation.Organisation>());
            scheme.UpdateScheme(
                "Scheme name",
                "WEE/AB1234CD/SCH",
                A.Dummy<string>(),
                A.Dummy<Domain.Obligation.ObligationType?>(),
                A.Dummy<UKCompetentAuthority>());

            IFetchSummaryCsvDataAccess dataAccess = A.Fake<IFetchSummaryCsvDataAccess>();
            A.CallTo(() => dataAccess.FetchSchemeAsync(A<Guid>._))
                .Returns(scheme);

            FetchSummaryCsvHandler handler = new FetchSummaryCsvHandler(
                A.Dummy<IWeeeAuthorization>(),
                A.Dummy<CsvWriterFactory>(),
                dataAccess);

            // Act
            Requests.DataReturns.FetchSummaryCsv request = new Requests.DataReturns.FetchSummaryCsv(
                A.Dummy<Guid>(),
                2017);

            SystemTime.Freeze(new DateTime(2016, 1, 2, 15, 22, 59), true);
            FileInfo result = await handler.HandleAsync(request);
            SystemTime.Unfreeze();

            // Assert
            Assert.NotNull(result);
            Assert.Equal("WEE/AB1234CD/SCH_EEE_WEEE_data_2017_02012016_1522.csv", result.FileName);
        }

        /// <summary>
        /// This test ensures that the CsvWriter will define columns for "Quarter",
        /// "EEE or WEEE in tonnes (t)", "Obligation type" and a column for each
        /// of the fourteen categories; i.e. "Cat 1 (t)", "Cat 2 (t)", etc.
        /// </summary>
        [Fact]
        public void CreateWriter_Always_CreatesExpectedColumns()
        {
            // Arrange
            FetchSummaryCsvHandler handler = new FetchSummaryCsvHandler(
                A.Dummy<IWeeeAuthorization>(),
                A.Dummy<CsvWriterFactory>(),
                A.Dummy<IFetchSummaryCsvDataAccess>());

            // Act
            CsvWriter<FetchSummaryCsvHandler.CsvResult> csvWriter = handler.CreateWriter();

            // Assert
            Assert.NotNull(csvWriter);
            Assert.Collection(csvWriter.ColumnTitles,
                c => Assert.Equal("Quarter", c),
                c => Assert.Equal("EEE or WEEE in tonnes (t)", c),
                c => Assert.Equal("Obligation type", c),
                c => Assert.Equal("Cat 1 (t)", c),
                c => Assert.Equal("Cat 2 (t)", c),
                c => Assert.Equal("Cat 3 (t)", c),
                c => Assert.Equal("Cat 4 (t)", c),
                c => Assert.Equal("Cat 5 (t)", c),
                c => Assert.Equal("Cat 6 (t)", c),
                c => Assert.Equal("Cat 7 (t)", c),
                c => Assert.Equal("Cat 8 (t)", c),
                c => Assert.Equal("Cat 9 (t)", c),
                c => Assert.Equal("Cat 10 (t)", c),
                c => Assert.Equal("Cat 11 (t)", c),
                c => Assert.Equal("Cat 12 (t)", c),
                c => Assert.Equal("Cat 13 (t)", c),
                c => Assert.Equal("Cat 14 (t)", c));
        }

        /// <summary>
        /// This test ensures that a B2C amount reported as collected from a DCF is reported
        /// under the result line for "WEEE collected from DCFs" and B2C.
        /// </summary>
        [Fact]
        public void CreateResults_WithB2CCollectedFromDcfAmount_ReturnsResultForB2CCollectedFromDcfs()
        {
            // Arrange
            DataReturn dataReturn = new DataReturn(
                A.Dummy<Domain.Scheme.Scheme>(),
                new Domain.DataReturns.Quarter(2016, Domain.DataReturns.QuarterType.Q1));

            DataReturnVersion dataReturnVersion = new DataReturnVersion(dataReturn);

            WeeeCollectedAmount amount = new WeeeCollectedAmount(
                WeeeCollectedAmountSourceType.Dcf,
                Domain.Obligation.ObligationType.B2C,
                A.Dummy<Domain.Lookup.WeeeCategory>(),
                A.Dummy<decimal>());

            dataReturnVersion.WeeeCollectedReturnVersion.AddWeeeCollectedAmount(amount);

            dataReturn.SetCurrentVersion(dataReturnVersion);

            FetchSummaryCsvHandler handler = new FetchSummaryCsvHandler(
                A.Dummy<IWeeeAuthorization>(),
                A.Dummy<CsvWriterFactory>(),
                A.Dummy<IFetchSummaryCsvDataAccess>());

            // Act
            IEnumerable<FetchSummaryCsvHandler.CsvResult> results = handler.CreateResults(dataReturn);

            // Assert
            Assert.NotNull(results);
            Assert.Equal(1, results.Count());

            FetchSummaryCsvHandler.CsvResult result = results.Single();

            Assert.NotNull(result);
            Assert.Equal("WEEE collected from DCFs", result.Description);
            Assert.Equal(ObligationType.B2C, result.ObligationType);
        }

        /// <summary>
        /// This test ensures that a BBC amount reported as collected from a DCF is reported
        /// under the result line for "WEEE collected from DCFs" and B2B.
        /// </summary>
        [Fact]
        public void CreateResults_WithB2BCollectedFromDcfAmount_ReturnsResultForB2BCollectedFromDcfs()
        {
            // Arrange
            DataReturn dataReturn = new DataReturn(
                A.Dummy<Domain.Scheme.Scheme>(),
                new Domain.DataReturns.Quarter(2016, Domain.DataReturns.QuarterType.Q1));

            DataReturnVersion dataReturnVersion = new DataReturnVersion(dataReturn);

            WeeeCollectedAmount amount = new WeeeCollectedAmount(
                WeeeCollectedAmountSourceType.Dcf,
                Domain.Obligation.ObligationType.B2B,
                A.Dummy<Domain.Lookup.WeeeCategory>(),
                A.Dummy<decimal>());

            dataReturnVersion.WeeeCollectedReturnVersion.AddWeeeCollectedAmount(amount);

            dataReturn.SetCurrentVersion(dataReturnVersion);

            FetchSummaryCsvHandler handler = new FetchSummaryCsvHandler(
                A.Dummy<IWeeeAuthorization>(),
                A.Dummy<CsvWriterFactory>(),
                A.Dummy<IFetchSummaryCsvDataAccess>());

            // Act
            IEnumerable<FetchSummaryCsvHandler.CsvResult> results = handler.CreateResults(dataReturn);

            // Assert
            Assert.NotNull(results);
            Assert.Equal(1, results.Count());

            FetchSummaryCsvHandler.CsvResult result = results.Single();

            Assert.NotNull(result);
            Assert.Equal("WEEE collected from DCFs", result.Description);
            Assert.Equal(ObligationType.B2B, result.ObligationType);
        }

        /// <summary>
        /// This test ensures that a B2C amount reported as collected from a distributor is reported
        /// under the result line for "WEEE from distributors" and B2C.
        /// </summary>
        [Fact]
        public void CreateResults_WithB2CCollectedFromDistributorAmount_ReturnsResultForB2CFromDistributors()
        {
            // Arrange
            DataReturn dataReturn = new DataReturn(
                A.Dummy<Domain.Scheme.Scheme>(),
                new Domain.DataReturns.Quarter(2016, Domain.DataReturns.QuarterType.Q1));

            DataReturnVersion dataReturnVersion = new DataReturnVersion(dataReturn);

            WeeeCollectedAmount amount = new WeeeCollectedAmount(
                WeeeCollectedAmountSourceType.Distributor,
                Domain.Obligation.ObligationType.B2C,
                A.Dummy<Domain.Lookup.WeeeCategory>(),
                A.Dummy<decimal>());

            dataReturnVersion.WeeeCollectedReturnVersion.AddWeeeCollectedAmount(amount);

            dataReturn.SetCurrentVersion(dataReturnVersion);

            FetchSummaryCsvHandler handler = new FetchSummaryCsvHandler(
                A.Dummy<IWeeeAuthorization>(),
                A.Dummy<CsvWriterFactory>(),
                A.Dummy<IFetchSummaryCsvDataAccess>());

            // Act
            IEnumerable<FetchSummaryCsvHandler.CsvResult> results = handler.CreateResults(dataReturn);

            // Assert
            Assert.NotNull(results);
            Assert.Equal(1, results.Count());

            FetchSummaryCsvHandler.CsvResult result = results.Single();

            Assert.NotNull(result);
            Assert.Equal("WEEE from distributors", result.Description);
            Assert.Equal(ObligationType.B2C, result.ObligationType);
        }

        /// <summary>
        /// This test ensures that a B2C amount reported as collected from a final holder is reported
        /// under the result line for "WEEE from final holders" and B2C.
        /// </summary>
        [Fact]
        public void CreateResults_WithB2CCollectedFromFinalHolderAmount_ReturnsResultForB2CFromFinalHolders()
        {
            // Arrange
            DataReturn dataReturn = new DataReturn(
                A.Dummy<Domain.Scheme.Scheme>(),
                new Domain.DataReturns.Quarter(2016, Domain.DataReturns.QuarterType.Q1));

            DataReturnVersion dataReturnVersion = new DataReturnVersion(dataReturn);

            WeeeCollectedAmount amount = new WeeeCollectedAmount(
                WeeeCollectedAmountSourceType.FinalHolder,
                Domain.Obligation.ObligationType.B2C,
                A.Dummy<Domain.Lookup.WeeeCategory>(),
                A.Dummy<decimal>());

            dataReturnVersion.WeeeCollectedReturnVersion.AddWeeeCollectedAmount(amount);

            dataReturn.SetCurrentVersion(dataReturnVersion);

            FetchSummaryCsvHandler handler = new FetchSummaryCsvHandler(
                A.Dummy<IWeeeAuthorization>(),
                A.Dummy<CsvWriterFactory>(),
                A.Dummy<IFetchSummaryCsvDataAccess>());

            // Act
            IEnumerable<FetchSummaryCsvHandler.CsvResult> results = handler.CreateResults(dataReturn);

            // Assert
            Assert.NotNull(results);
            Assert.Equal(1, results.Count());

            FetchSummaryCsvHandler.CsvResult result = results.Single();

            Assert.NotNull(result);
            Assert.Equal("WEEE from final holders", result.Description);
            Assert.Equal(ObligationType.B2C, result.ObligationType);
        }

        /// <summary>
        /// This test ensures that a B2C amount reported as delivered to an AATF is reported
        /// under the result line for "WEEE sent to AATFs" and B2C.
        /// </summary>
        [Fact]
        public void CreateResults_WithB2CDeliveredToAATFAmount_ReturnsResultForB2CSentToAATFs()
        {
            // Arrange
            DataReturn dataReturn = new DataReturn(
                A.Dummy<Domain.Scheme.Scheme>(),
                new Domain.DataReturns.Quarter(2016, Domain.DataReturns.QuarterType.Q1));

            DataReturnVersion dataReturnVersion = new DataReturnVersion(dataReturn);

            WeeeDeliveredAmount amount = new WeeeDeliveredAmount(
                Domain.Obligation.ObligationType.B2C,
                A.Dummy<Domain.Lookup.WeeeCategory>(),
                A.Dummy<decimal>(),
                A.Dummy<AatfDeliveryLocation>());

            dataReturnVersion.WeeeDeliveredReturnVersion.AddWeeeDeliveredAmount(amount);

            dataReturn.SetCurrentVersion(dataReturnVersion);

            FetchSummaryCsvHandler handler = new FetchSummaryCsvHandler(
                A.Dummy<IWeeeAuthorization>(),
                A.Dummy<CsvWriterFactory>(),
                A.Dummy<IFetchSummaryCsvDataAccess>());

            // Act
            IEnumerable<FetchSummaryCsvHandler.CsvResult> results = handler.CreateResults(dataReturn);

            // Assert
            Assert.NotNull(results);
            Assert.Equal(1, results.Count());

            FetchSummaryCsvHandler.CsvResult result = results.Single();

            Assert.NotNull(result);
            Assert.Equal("WEEE sent to AATFs", result.Description);
            Assert.Equal(ObligationType.B2C, result.ObligationType);
        }

        /// <summary>
        /// This test ensures that a B2B amount reported as delivered to an AATF is reported
        /// under the result line for "WEEE sent to AATFs" and B2B.
        /// </summary>
        [Fact]
        public void CreateResults_WithB2BDeliveredToAATFAmount_ReturnsResultForB2BSentToAATFs()
        {
            // Arrange
            DataReturn dataReturn = new DataReturn(
                A.Dummy<Domain.Scheme.Scheme>(),
                new Domain.DataReturns.Quarter(2016, Domain.DataReturns.QuarterType.Q1));

            DataReturnVersion dataReturnVersion = new DataReturnVersion(dataReturn);

            WeeeDeliveredAmount amount = new WeeeDeliveredAmount(
                Domain.Obligation.ObligationType.B2B,
                A.Dummy<Domain.Lookup.WeeeCategory>(),
                A.Dummy<decimal>(),
                A.Dummy<AatfDeliveryLocation>());

            dataReturnVersion.WeeeDeliveredReturnVersion.AddWeeeDeliveredAmount(amount);

            dataReturn.SetCurrentVersion(dataReturnVersion);

            FetchSummaryCsvHandler handler = new FetchSummaryCsvHandler(
                A.Dummy<IWeeeAuthorization>(),
                A.Dummy<CsvWriterFactory>(),
                A.Dummy<IFetchSummaryCsvDataAccess>());

            // Act
            IEnumerable<FetchSummaryCsvHandler.CsvResult> results = handler.CreateResults(dataReturn);

            // Assert
            Assert.NotNull(results);
            Assert.Equal(1, results.Count());

            FetchSummaryCsvHandler.CsvResult result = results.Single();

            Assert.NotNull(result);
            Assert.Equal("WEEE sent to AATFs", result.Description);
            Assert.Equal(ObligationType.B2B, result.ObligationType);
        }

        /// <summary>
        /// This test ensures that a B2C amount reported as delivered to an AE is reported
        /// under the result line for "WEEE sent to AEs" and B2C.
        /// </summary>
        [Fact]
        public void CreateResults_WithB2CDeliveredToAEAmount_ReturnsResultForB2CSentToAEs()
        {
            // Arrange
            DataReturn dataReturn = new DataReturn(
                A.Dummy<Domain.Scheme.Scheme>(),
                new Domain.DataReturns.Quarter(2016, Domain.DataReturns.QuarterType.Q1));

            DataReturnVersion dataReturnVersion = new DataReturnVersion(dataReturn);

            WeeeDeliveredAmount amount = new WeeeDeliveredAmount(
                Domain.Obligation.ObligationType.B2C,
                A.Dummy<Domain.Lookup.WeeeCategory>(),
                A.Dummy<decimal>(),
                A.Dummy<AeDeliveryLocation>());

            dataReturnVersion.WeeeDeliveredReturnVersion.AddWeeeDeliveredAmount(amount);

            dataReturn.SetCurrentVersion(dataReturnVersion);

            FetchSummaryCsvHandler handler = new FetchSummaryCsvHandler(
                A.Dummy<IWeeeAuthorization>(),
                A.Dummy<CsvWriterFactory>(),
                A.Dummy<IFetchSummaryCsvDataAccess>());

            // Act
            IEnumerable<FetchSummaryCsvHandler.CsvResult> results = handler.CreateResults(dataReturn);

            // Assert
            Assert.NotNull(results);
            Assert.Equal(1, results.Count());

            FetchSummaryCsvHandler.CsvResult result = results.Single();

            Assert.NotNull(result);
            Assert.Equal("WEEE sent to AEs", result.Description);
            Assert.Equal(ObligationType.B2C, result.ObligationType);
        }

        /// <summary>
        /// This test ensures that a B2B amount reported as delivered to an AE is reported
        /// under the result line for "WEEE sent to AEs" and B2B.
        /// </summary>
        [Fact]
        public void CreateResults_WithB2BDeliveredToAEAmount_ReturnsResultForB2BSentToAEs()
        {
            // Arrange
            DataReturn dataReturn = new DataReturn(
                A.Dummy<Domain.Scheme.Scheme>(),
                new Domain.DataReturns.Quarter(2016, Domain.DataReturns.QuarterType.Q1));

            DataReturnVersion dataReturnVersion = new DataReturnVersion(dataReturn);

            WeeeDeliveredAmount amount = new WeeeDeliveredAmount(
                Domain.Obligation.ObligationType.B2B,
                A.Dummy<Domain.Lookup.WeeeCategory>(),
                A.Dummy<decimal>(),
                A.Dummy<AeDeliveryLocation>());

            dataReturnVersion.WeeeDeliveredReturnVersion.AddWeeeDeliveredAmount(amount);

            dataReturn.SetCurrentVersion(dataReturnVersion);

            FetchSummaryCsvHandler handler = new FetchSummaryCsvHandler(
                A.Dummy<IWeeeAuthorization>(),
                A.Dummy<CsvWriterFactory>(),
                A.Dummy<IFetchSummaryCsvDataAccess>());

            // Act
            IEnumerable<FetchSummaryCsvHandler.CsvResult> results = handler.CreateResults(dataReturn);

            // Assert
            Assert.NotNull(results);
            Assert.Equal(1, results.Count());

            FetchSummaryCsvHandler.CsvResult result = results.Single();

            Assert.NotNull(result);
            Assert.Equal("WEEE sent to AEs", result.Description);
            Assert.Equal(ObligationType.B2B, result.ObligationType);
        }

        /// <summary>
        /// This test ensures that a B2C amount reported as output is reported
        /// under the result line for "EEE placed on market" and B2C.
        /// </summary>
        [Fact]
        public void CreateResults_WithB2CEEEOutputAmount_ReturnsResultForB2CEEEPlacedOnMarket()
        {
            // Arrange
            DataReturn dataReturn = new DataReturn(
                A.Dummy<Domain.Scheme.Scheme>(),
                new Domain.DataReturns.Quarter(2016, Domain.DataReturns.QuarterType.Q1));

            DataReturnVersion dataReturnVersion = new DataReturnVersion(dataReturn);

            EeeOutputAmount amount = new EeeOutputAmount(
                Domain.Obligation.ObligationType.B2C,
                A.Dummy<Domain.Lookup.WeeeCategory>(),
                A.Dummy<decimal>(),
                A.Dummy<RegisteredProducer>());

            dataReturnVersion.EeeOutputReturnVersion.AddEeeOutputAmount(amount);

            dataReturn.SetCurrentVersion(dataReturnVersion);

            FetchSummaryCsvHandler handler = new FetchSummaryCsvHandler(
                A.Dummy<IWeeeAuthorization>(),
                A.Dummy<CsvWriterFactory>(),
                A.Dummy<IFetchSummaryCsvDataAccess>());

            // Act
            IEnumerable<FetchSummaryCsvHandler.CsvResult> results = handler.CreateResults(dataReturn);

            // Assert
            Assert.NotNull(results);
            Assert.Equal(1, results.Count());

            FetchSummaryCsvHandler.CsvResult result = results.Single();

            Assert.NotNull(result);
            Assert.Equal("EEE placed on market", result.Description);
            Assert.Equal(ObligationType.B2C, result.ObligationType);
        }

        /// <summary>
        /// This test ensures that a B2B amount reported as output is reported
        /// under the result line for "EEE placed on market" and B2B.
        /// </summary>
        [Fact]
        public void CreateResults_WithB2BEEEOutputAmount_ReturnsResultForB2BEEEPlacedOnMarket()
        {
            // Arrange
            DataReturn dataReturn = new DataReturn(
                A.Dummy<Domain.Scheme.Scheme>(),
                new Domain.DataReturns.Quarter(2016, Domain.DataReturns.QuarterType.Q1));

            DataReturnVersion dataReturnVersion = new DataReturnVersion(dataReturn);

            EeeOutputAmount amount = new EeeOutputAmount(
                Domain.Obligation.ObligationType.B2B,
                A.Dummy<Domain.Lookup.WeeeCategory>(),
                A.Dummy<decimal>(),
                A.Dummy<RegisteredProducer>());

            dataReturnVersion.EeeOutputReturnVersion.AddEeeOutputAmount(amount);

            dataReturn.SetCurrentVersion(dataReturnVersion);

            FetchSummaryCsvHandler handler = new FetchSummaryCsvHandler(
                A.Dummy<IWeeeAuthorization>(),
                A.Dummy<CsvWriterFactory>(),
                A.Dummy<IFetchSummaryCsvDataAccess>());

            // Act
            IEnumerable<FetchSummaryCsvHandler.CsvResult> results = handler.CreateResults(dataReturn);

            // Assert
            Assert.NotNull(results);
            Assert.Equal(1, results.Count());

            FetchSummaryCsvHandler.CsvResult result = results.Single();

            Assert.NotNull(result);
            Assert.Equal("EEE placed on market", result.Description);
            Assert.Equal(ObligationType.B2B, result.ObligationType);
        }

        /// <summary>
        /// This test ensures that a data reutrn where the current version has a null
        /// for the WeeeCollectedReturnVersion, WeeeDeliveredReturnVersion and EeeOutputReturnVersion
        /// properties returns no results (i.e. handles the null and doesn't throw an exception).
        /// </summary>
        [Fact]
        public void CreateResults_WithDataReturnWithNullCollections_ReturnsNoResults()
        {
            // Arrange
            DataReturn dataReturn = new DataReturn(
                A.Dummy<Domain.Scheme.Scheme>(),
                new Domain.DataReturns.Quarter(2016, Domain.DataReturns.QuarterType.Q1));

            DataReturnVersion dataReturnVersion = new DataReturnVersion(dataReturn, null, null, null);

            dataReturn.SetCurrentVersion(dataReturnVersion);

            FetchSummaryCsvHandler handler = new FetchSummaryCsvHandler(
                A.Dummy<IWeeeAuthorization>(),
                A.Dummy<CsvWriterFactory>(),
                A.Dummy<IFetchSummaryCsvDataAccess>());

            // Act
            IEnumerable<FetchSummaryCsvHandler.CsvResult> results = handler.CreateResults(dataReturn);

            // Assert
            Assert.NotNull(results);
            Assert.Equal(0, results.Count());
        }
    }
}