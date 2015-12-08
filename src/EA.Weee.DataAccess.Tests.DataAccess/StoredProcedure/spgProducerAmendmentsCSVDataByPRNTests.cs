﻿namespace EA.Weee.DataAccess.Tests.DataAccess.StoredProcedure
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Weee.DataAccess.StoredProcedure;
    using Weee.Tests.Core.Model;
    using Xunit;

    public class SpgProducerAmendmentsCSVDataByPRNTests
    {
        [Fact]
        public async Task Execute_HappyPath_ReturnsParticularPRNRecords()
        {
            using (DatabaseWrapper db = new DatabaseWrapper())
            {
                // Arrange
                ModelHelper helper = new ModelHelper(db.Model);

                Scheme scheme1 = helper.CreateScheme();

                scheme1.SchemeName = "SchemeName";

                MemberUpload memberUpload1 = helper.CreateMemberUpload(scheme1);
                memberUpload1.ComplianceYear = 2016;
                memberUpload1.IsSubmitted = true;

                ProducerSubmission producerSubmission1 = helper.CreateProducerAsCompany(memberUpload1, "WEE/99ZZZZ99");
                ProducerSubmission producerSubmission2 = helper.CreateProducerAsCompany(memberUpload1, "WEE/11ZZZZ11");
                producerSubmission2.VATRegistered = true;

                Scheme scheme2 = helper.CreateScheme();

                MemberUpload memberUpload2 = helper.CreateMemberUpload(scheme2);
                memberUpload2.ComplianceYear = 2017;
                memberUpload2.IsSubmitted = true;

                ProducerSubmission producerSubmission3 = helper.CreateProducerAsCompany(memberUpload2, "WEE/11ZZZZ11");
                producerSubmission2.VATRegistered = false;

                db.Model.SaveChanges();

                // Act
                List<ProducerAmendmentsHistoryCSVData> results =
                    await db.StoredProcedures.SpgProducerAmendmentsCSVDataByPRN("WEE/11ZZZZ11");

                // Assert
                Assert.NotNull(results);
                Assert.Equal(results.Count, 2);
                Assert.True(results.TrueForAll(i => i.PRN == "WEE/11ZZZZ11"));
                Assert.False(results.TrueForAll(i => i.VATRegistered));
            }
        }

        [Fact]
        public async Task Execute_MultipleTimeProducerAmendmentsDuringComplianceYear_ReturnsAllAmendmentsRecordsWithPerfectRegisteredDateAndUpdatedDate()
        {
            using (DatabaseWrapper db = new DatabaseWrapper())
            {
                // Arrange
                ModelHelper helper = new ModelHelper(db.Model);

                Scheme scheme1 = helper.CreateScheme();

                scheme1.SchemeName = "SchemeName";

                MemberUpload memberUpload1 = helper.CreateMemberUpload(scheme1);
                memberUpload1.ComplianceYear = 2016;
                memberUpload1.IsSubmitted = true;

                var dateTimeOfRegistered = DateTime.UtcNow.AddDays(-3);
                var datetimeOfFirstUpdated = dateTimeOfRegistered.AddDays(1);
                var datetimeOfSecondUpdated = datetimeOfFirstUpdated.AddDays(1);

                ProducerSubmission producerSubmission1 = helper.CreateProducerAsCompany(memberUpload1, "WEE/99ZZZZ99");
                producerSubmission1.UpdatedDate = dateTimeOfRegistered;
                ProducerSubmission producerSubmission2 = helper.CreateProducerAsCompany(memberUpload1, "WEE/99ZZZZ99");
                producerSubmission2.UpdatedDate = datetimeOfFirstUpdated;
                ProducerSubmission producerSubmission3 = helper.CreateProducerAsCompany(memberUpload1, "WEE/99ZZZZ99");
                producerSubmission3.UpdatedDate = datetimeOfSecondUpdated;

                db.Model.SaveChanges();

                // Act
                List<ProducerAmendmentsHistoryCSVData> results =
                    await db.StoredProcedures.SpgProducerAmendmentsCSVDataByPRN("WEE/99ZZZZ99");

                // Assert
                Assert.NotNull(results);
                Assert.Equal(results.Count, 3);

                results = results.OrderBy(i => i.DateAmended).ToList();

                Assert.True(results.TrueForAll(i => i.DateRegistered.Date == dateTimeOfRegistered.Date));
                Assert.Equal(results[1].DateAmended.Date, datetimeOfFirstUpdated.Date);
                Assert.Equal(results[2].DateAmended.Date, datetimeOfSecondUpdated.Date);
            }
        }

        [Fact]
        public async Task Execute_NonSubmittedMemberUpload_IgnoresProducer()
        {
            using (DatabaseWrapper db = new DatabaseWrapper())
            {
                // Arrange
                ModelHelper helper = new ModelHelper(db.Model);

                Scheme scheme1 = helper.CreateScheme();

                MemberUpload memberUpload1 = helper.CreateMemberUpload(scheme1);
                memberUpload1.ComplianceYear = 2016;
                memberUpload1.IsSubmitted = false;

                helper.CreateProducerAsCompany(memberUpload1, "WEE/11AAAA11");

                db.Model.SaveChanges();

                // Act
                List<ProducerAmendmentsHistoryCSVData> results =
                    await db.StoredProcedures.SpgProducerAmendmentsCSVDataByPRN("WEE/11AAAA11");

                // Assert
                Assert.NotNull(results);
                Assert.Equal(0, results.Count);
            }
        }

        [Fact]
        public async Task Execute_ProducerAsCompany_ReturnsCompanyName()
        {
            using (DatabaseWrapper db = new DatabaseWrapper())
            {
                // Arrange
                ModelHelper helper = new ModelHelper(db.Model);

                Scheme scheme1 = helper.CreateScheme();

                MemberUpload memberUpload1 = helper.CreateMemberUpload(scheme1);
                memberUpload1.ComplianceYear = 2016;
                memberUpload1.IsSubmitted = true;

                ProducerSubmission producer1 = helper.CreateProducerAsCompany(memberUpload1, "WEE/11AAAA11");

                db.Model.SaveChanges();

                // Act
                List<ProducerAmendmentsHistoryCSVData> results =
                    await db.StoredProcedures.SpgProducerAmendmentsCSVDataByPRN("WEE/11AAAA11");

                // Assert
                Assert.NotNull(results);
                Assert.Equal(1, results.Count);
                
                Assert.Equal(producer1.Business.Company.Name, results[0].ProducerName);
            }
        }

        [Fact]
        public async Task Execute_ProducerAsPartnership_ReturnsPartnershipName()
        {
            using (DatabaseWrapper db = new DatabaseWrapper())
            {
                // Arrange
                ModelHelper helper = new ModelHelper(db.Model);

                Scheme scheme1 = helper.CreateScheme();

                MemberUpload memberUpload1 = helper.CreateMemberUpload(scheme1);
                memberUpload1.ComplianceYear = 2016;
                memberUpload1.IsSubmitted = true;

                ProducerSubmission producer1 = helper.CreateProducerAsPartnership(memberUpload1, "WEE/11AAAA11");

                db.Model.SaveChanges();

                // Act
                List<ProducerAmendmentsHistoryCSVData> results =
                    await db.StoredProcedures.SpgProducerAmendmentsCSVDataByPRN("WEE/11AAAA11");

                // Assert
                Assert.NotNull(results);
                Assert.Equal(1, results.Count);
                
                Assert.Equal(producer1.Business.Partnership.Name, results[0].ProducerName);
            }
        }

        [Fact]
        public async Task Execute_ListOfProducers_ReturnsRecordsInCurrectOrder()
        {
            using (DatabaseWrapper db = new DatabaseWrapper())
            {
                // Arrange
                ModelHelper helper = new ModelHelper(db.Model);

                Scheme scheme1 = helper.CreateScheme();

                scheme1.SchemeName = "SONY";

                MemberUpload memberUpload1 = helper.CreateMemberUpload(scheme1);
                memberUpload1.ComplianceYear = 2017;
                memberUpload1.IsSubmitted = true;

                var dateTimeOfRegisteredFor2017 = DateTime.UtcNow.AddDays(-3);
                var datetimeOfFirstUpdatedFor2017 = dateTimeOfRegisteredFor2017.AddDays(1);
                var datetimeOfSecondUpdatedFor2017 = datetimeOfFirstUpdatedFor2017.AddDays(1);

                ProducerSubmission producerSubmission1 = helper.CreateProducerAsCompany(memberUpload1, "WEE/99ZZZZ99");
                producerSubmission1.UpdatedDate = dateTimeOfRegisteredFor2017;
                ProducerSubmission producerSubmission2 = helper.CreateProducerAsCompany(memberUpload1, "WEE/99ZZZZ99");
                producerSubmission2.UpdatedDate = datetimeOfFirstUpdatedFor2017;
                ProducerSubmission producerSubmission3 = helper.CreateProducerAsCompany(memberUpload1, "WEE/99ZZZZ99");
                producerSubmission3.UpdatedDate = datetimeOfSecondUpdatedFor2017;
                
                MemberUpload memberUpload2 = helper.CreateMemberUpload(scheme1);
                memberUpload2.ComplianceYear = 2016;
                memberUpload2.IsSubmitted = true;

                var dateTimeOfRegisteredFor2016 = DateTime.UtcNow.AddDays(-5);

                ProducerSubmission producerSubmission4 = helper.CreateProducerAsCompany(memberUpload2, "WEE/99ZZZZ99");
                producerSubmission4.UpdatedDate = dateTimeOfRegisteredFor2016;
                
                db.Model.SaveChanges();

                // Act
                List<ProducerAmendmentsHistoryCSVData> results =
                   await db.StoredProcedures.SpgProducerAmendmentsCSVDataByPRN("WEE/99ZZZZ99");

                // Assert
                Assert.NotNull(results);
                Assert.Equal(results.Count, 4);

                Assert.True(results[0].DateAmended.Date == datetimeOfSecondUpdatedFor2017.Date && results[0].ComplianceYear == 2017);
                Assert.True(results[1].DateAmended.Date == datetimeOfFirstUpdatedFor2017.Date && results[1].ComplianceYear == 2017);
                Assert.True(results[2].DateRegistered.Date == dateTimeOfRegisteredFor2017.Date && results[2].ComplianceYear == 2017);
                Assert.True(results[3].DateRegistered.Date == dateTimeOfRegisteredFor2016.Date && results[3].ComplianceYear == 2016);
            }
        }
    }
}