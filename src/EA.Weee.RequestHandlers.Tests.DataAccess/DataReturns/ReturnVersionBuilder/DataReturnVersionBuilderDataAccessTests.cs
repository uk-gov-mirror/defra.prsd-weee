﻿namespace EA.Weee.RequestHandlers.Tests.DataAccess.DataReturns.ReturnVersionBuilder
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Domain.DataReturns;
    using RequestHandlers.DataReturns.ReturnVersionBuilder;
    using Weee.DataAccess;
    using Weee.Tests.Core;
    using Weee.Tests.Core.Model;
    using Xunit;

    public class DataReturnVersionBuilderDataAccessTests
    {
        [Fact]
        public async Task FetchDataReturnOrDefault_ResultIncludesUnsubmittedDataReturn()
        {
            using (DatabaseWrapper database = new DatabaseWrapper())
            {
                // Arrange
                ModelHelper helper = new ModelHelper(database.Model);
                DomainHelper domainHelper = new DomainHelper(database.WeeeContext);

                var scheme = helper.CreateScheme();
                var dataReturn = helper.CreateDataReturn(scheme, 2016, 1);

                database.Model.SaveChanges();

                var dataAccess = new DataReturnVersionBuilderDataAccess(domainHelper.GetScheme(scheme.Id), new Quarter(2016, QuarterType.Q1), database.WeeeContext);

                // Act
                var result = await dataAccess.FetchDataReturnOrDefault();

                // Assert
                Assert.NotNull(result);
                Assert.Equal(dataReturn.Id, result.Id);
                Assert.Null(dataReturn.DataReturnVersion);
            }
        }

        [Fact]
        public async Task FetchDataReturnOrDefault_ReturnsDataReturnForSpecifiedSchemeOnly()
        {
            using (DatabaseWrapper database = new DatabaseWrapper())
            {
                // Arrange
                ModelHelper helper = new ModelHelper(database.Model);
                DomainHelper domainHelper = new DomainHelper(database.WeeeContext);

                var scheme1 = helper.CreateScheme();
                var dataReturnVersion = helper.CreateDataReturnVersion(scheme1, 2016, 1, true);

                var scheme2 = helper.CreateScheme();
                helper.CreateDataReturnVersion(scheme2, 2016, 1, true);

                database.Model.SaveChanges();

                var dataAccess = new DataReturnVersionBuilderDataAccess(domainHelper.GetScheme(scheme1.Id), new Quarter(2016, QuarterType.Q1), database.WeeeContext);

                // Act
                var result = await dataAccess.FetchDataReturnOrDefault();

                // Assert
                Assert.Equal(scheme1.Id, result.Scheme.Id);
                Assert.Equal(dataReturnVersion.DataReturn.Id, result.Id);
            }
        }

        [Fact]
        public async Task FetchDataReturnOrDefault_ReturnsDataReturnForSpecifiedYearOnly()
        {
            using (DatabaseWrapper database = new DatabaseWrapper())
            {
                // Arrange
                ModelHelper helper = new ModelHelper(database.Model);
                DomainHelper domainHelper = new DomainHelper(database.WeeeContext);

                var scheme = helper.CreateScheme();
                helper.CreateDataReturnVersion(scheme, 2016, 1, true);

                var dataReturnVersion = helper.CreateDataReturnVersion(scheme, 2017, 1, true);

                database.Model.SaveChanges();

                var dataAccess = new DataReturnVersionBuilderDataAccess(domainHelper.GetScheme(scheme.Id), new Quarter(2017, QuarterType.Q1), database.WeeeContext);

                // Act
                var result = await dataAccess.FetchDataReturnOrDefault();

                // Assert
                Assert.Equal(2017, result.Quarter.Year);
                Assert.Equal(dataReturnVersion.DataReturn.Id, result.Id);
            }
        }

        [Fact]
        public async Task FetchDataReturnOrDefault_ReturnsDataReturnForSpecifiedQuarterOnly()
        {
            using (DatabaseWrapper database = new DatabaseWrapper())
            {
                // Arrange
                ModelHelper helper = new ModelHelper(database.Model);
                DomainHelper domainHelper = new DomainHelper(database.WeeeContext);

                var scheme = helper.CreateScheme();
                helper.CreateDataReturnVersion(scheme, 2016, 1, true);

                var dataReturnVersion = helper.CreateDataReturnVersion(scheme, 2016, 2, true);

                database.Model.SaveChanges();

                var dataAccess = new DataReturnVersionBuilderDataAccess(domainHelper.GetScheme(scheme.Id), new Quarter(2016, QuarterType.Q2), database.WeeeContext);

                // Act
                var result = await dataAccess.FetchDataReturnOrDefault();

                // Assert
                Assert.Equal(QuarterType.Q2, result.Quarter.Q);
                Assert.Equal(dataReturnVersion.DataReturn.Id, result.Id);
            }
        }

        [Fact]
        public async Task GetRegisteredProducer_ReturnsSubmittedProducerOnly()
        {
            using (DatabaseWrapper database = new DatabaseWrapper())
            {
                // Arrange
                ModelHelper helper = new ModelHelper(database.Model);
                DomainHelper domainHelper = new DomainHelper(database.WeeeContext);

                var scheme = helper.CreateScheme();
                helper.GerOrCreateRegisteredProducer(scheme, 2016, "BBBB");

                database.Model.SaveChanges();

                var dataAccess = new DataReturnVersionBuilderDataAccess(domainHelper.GetScheme(scheme.Id), new Quarter(2016, QuarterType.Q1), database.WeeeContext);

                // Act
                var result = await dataAccess.GetRegisteredProducer("BBBB");

                // Assert
                Assert.Null(result);
            }
        }

        [Fact]
        public async Task GetRegisteredProducer_ReturnsProducerForSpecifiedSchemeOnly()
        {
            using (DatabaseWrapper database = new DatabaseWrapper())
            {
                // Arrange
                ModelHelper helper = new ModelHelper(database.Model);
                DomainHelper domainHelper = new DomainHelper(database.WeeeContext);

                var scheme1 = helper.CreateScheme();
                var memberUpload1 = helper.CreateMemberUpload(scheme1);
                memberUpload1.ComplianceYear = 2016;
                memberUpload1.IsSubmitted = true;
                var producerSubmission = helper.CreateProducerAsCompany(memberUpload1, "AAAA");

                var scheme2 = helper.CreateScheme();
                var memberUpload2 = helper.CreateMemberUpload(scheme2);
                memberUpload2.ComplianceYear = 2016;
                memberUpload2.IsSubmitted = true;
                helper.CreateProducerAsCompany(memberUpload2, "AAAA");

                database.Model.SaveChanges();

                var dataAccess = new DataReturnVersionBuilderDataAccess(domainHelper.GetScheme(scheme1.Id), new Quarter(2016, QuarterType.Q1), database.WeeeContext);

                // Act
                var result = await dataAccess.GetRegisteredProducer("AAAA");

                // Assert
                Assert.Equal("AAAA", result.ProducerRegistrationNumber);
                Assert.Equal(scheme1.Id, result.Scheme.Id);
                Assert.Equal(producerSubmission.RegisteredProducer.Id, result.Id);
            }
        }

        [Fact]
        public async Task GetRegisteredProducer_ReturnsProducerForSpecifiedComplianceYearOnly()
        {
            using (DatabaseWrapper database = new DatabaseWrapper())
            {
                // Arrange
                ModelHelper helper = new ModelHelper(database.Model);
                DomainHelper domainHelper = new DomainHelper(database.WeeeContext);

                var scheme = helper.CreateScheme();
                var memberUpload1 = helper.CreateMemberUpload(scheme);
                memberUpload1.ComplianceYear = 2016;
                memberUpload1.IsSubmitted = true;
                helper.CreateProducerAsCompany(memberUpload1, "AAAA");

                var memberUpload2 = helper.CreateMemberUpload(scheme);
                memberUpload2.ComplianceYear = 2017;
                memberUpload2.IsSubmitted = true;
                var producerSubmission = helper.CreateProducerAsCompany(memberUpload2, "AAAA");

                database.Model.SaveChanges();

                var dataAccess = new DataReturnVersionBuilderDataAccess(domainHelper.GetScheme(scheme.Id), new Quarter(2017, QuarterType.Q1), database.WeeeContext);

                // Act
                var result = await dataAccess.GetRegisteredProducer("AAAA");

                // Assert
                Assert.Equal("AAAA", result.ProducerRegistrationNumber);
                Assert.Equal(2017, producerSubmission.RegisteredProducer.ComplianceYear);
                Assert.Equal(producerSubmission.RegisteredProducer.Id, result.Id);
            }
        }

        [Fact]
        public async Task GetRegisteredProducer_ReturnsProducerForMatchingRegistrationNumberOnly()
        {
            using (DatabaseWrapper database = new DatabaseWrapper())
            {
                // Arrange
                ModelHelper helper = new ModelHelper(database.Model);
                DomainHelper domainHelper = new DomainHelper(database.WeeeContext);

                var scheme = helper.CreateScheme();

                var memberUpload1 = helper.CreateMemberUpload(scheme);
                memberUpload1.ComplianceYear = 2016;
                memberUpload1.IsSubmitted = true;
                helper.CreateProducerAsCompany(memberUpload1, "AAAA");

                var producer = helper.GerOrCreateRegisteredProducer(scheme, 2016, "BBBB");
                var memberUpload2 = helper.CreateMemberUpload(scheme);
                memberUpload2.ComplianceYear = 2016;
                memberUpload2.IsSubmitted = true;
                helper.CreateProducerAsCompany(memberUpload2, "BBBB");

                database.Model.SaveChanges();

                var dataAccess = new DataReturnVersionBuilderDataAccess(domainHelper.GetScheme(scheme.Id), new Quarter(2016, QuarterType.Q1), database.WeeeContext);

                // Act
                var result = await dataAccess.GetRegisteredProducer("BBBB");

                // Assert
                Assert.Equal("BBBB", result.ProducerRegistrationNumber);
                Assert.Equal(producer.Id, result.Id);
            }
        }
    }
}
