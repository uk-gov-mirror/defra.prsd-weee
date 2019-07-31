﻿namespace EA.Weee.DataAccess.Tests.DataAccess.StoredProcedure
{
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Threading.Tasks;
    using Domain.AatfReturn;
    using FluentAssertions;
    using Weee.Tests.Core;
    using Weee.Tests.Core.Model;
    using Xunit;
    using Return = Domain.AatfReturn.Return;
    using ReturnReportOn = Domain.AatfReturn.ReturnReportOn;
    using WeeeReusedSite = Domain.AatfReturn.WeeeReusedSite;

    public class GetAllAatfReuseSitesCsvDataTests
    {
        [Fact]
        public async Task Execute_GivenNoData_NoResultsShouldBeReturned()
        {
            using (var db = new DatabaseWrapper())
            {
                var @return = ObligatedWeeeIntegrationCommon.CreateReturn(null, db.Model.AspNetUsers.First().Id, FacilityType.Aatf);
                @return.UpdateSubmitted(db.Model.AspNetUsers.First().Id, false);
                var aatf = ObligatedWeeeIntegrationCommon.CreateAatf(db, @return.Organisation);

                db.WeeeContext.Returns.Add(@return);

                await db.WeeeContext.SaveChangesAsync();

                var results = await db.StoredProcedures.GetAllAatfReuseSitesCsvData(2018, null, null);

                results.Count.Should().Be(0);
            }
        }

        [Fact]
        public async Task Execute_GivenWeeeReusedData_ReturnsWeeeReusedAatfSitesShouldBeCorrect()
        {
            using (var db = new DatabaseWrapper())
            {
                var @return = CreateSubmittedReturn(db);
                var aatf = ObligatedWeeeIntegrationCommon.CreateAatf(db, @return.Organisation);

                await CreateWeeReusedData(db, aatf, @return);
                var results = await db.StoredProcedures.GetAllAatfReuseSitesCsvData(2019, null, null);

                results.Where(x => x.ApprovalNumber == aatf.ApprovalNumber).Count().Should().Be(2);
            }
        }

        [Fact]
        public async Task Execute_GivenWeeReusedData_AuthorityParameter_ReturnsDataShouldBeCorrect()
        {
            using (var db = new DatabaseWrapper())
            {
                var @return = CreateSubmittedReturn(db);
                var aatf = ObligatedWeeeIntegrationCommon.CreateAatf(db, @return.Organisation);

                await CreateWeeReusedData(db, aatf, @return);

                var filter = db.WeeeContext.UKCompetentAuthorities.First();

                var results = await db.StoredProcedures.GetAllAatfReuseSitesCsvData(2019, filter.Id, null);

                Assert.NotNull(results);

                results.Where(x => x.ApprovalNumber == aatf.ApprovalNumber && x.Abbreviation == filter.Abbreviation).Count().Should().Be(2);
            }
        }

        [Fact]
        public async Task Execute_GivenWeeReusedData_PanAreaParameter_ReturnsDataShouldBeCorrect()
        {
            using (var db = new DatabaseWrapper())
            {
                var @return = CreateSubmittedReturn(db);
                var aatf = ObligatedWeeeIntegrationCommon.CreateAatf(db, @return.Organisation);

                await CreateWeeReusedData(db, aatf, @return);

                var ea = db.WeeeContext.UKCompetentAuthorities.First();

                var filter = db.WeeeContext.PanAreas.First();

                var results = await db.StoredProcedures.GetAllAatfReuseSitesCsvData(2019, ea.Id, filter.Id);

                Assert.NotNull(results);

                results.Where(x => x.ApprovalNumber == aatf.ApprovalNumber && x.Abbreviation == ea.Abbreviation && x.PanName == filter.Name).Count().Should().Be(2);
            }
        }

        private static Return CreateSubmittedReturn(DatabaseWrapper db)
        {
            var @return = ObligatedWeeeIntegrationCommon.CreateReturn(null, db.Model.AspNetUsers.First().Id, FacilityType.Aatf);
            @return.UpdateSubmitted(db.Model.AspNetUsers.First().Id, false);
            return @return;
        }

        private static async Task CreateWeeReusedData(DatabaseWrapper db, EA.Weee.Domain.AatfReturn.Aatf aatf, Return @return)
        {
            var weeeReused = new EA.Weee.Domain.AatfReturn.WeeeReused(aatf, @return);

            var weeeReusedSites = new List<WeeeReusedSite>()
                {
                    new Domain.AatfReturn.WeeeReusedSite(weeeReused, new AatfAddress("name1", "address", "address2", "town", "county", "postcode", db.WeeeContext.Countries.First())),
                    new Domain.AatfReturn.WeeeReusedSite(weeeReused, new AatfAddress("name2", "address", "address2", "town", "county", "postcode", db.WeeeContext.Countries.First()))
                };

            db.WeeeContext.Returns.Add(@return);
            db.WeeeContext.ReturnAatfs.Add(new ReturnAatf(aatf, @return));
            db.WeeeContext.WeeeReused.Add(weeeReused);
            db.WeeeContext.WeeeReusedSite.AddRange(weeeReusedSites);

            await db.WeeeContext.SaveChangesAsync();

            db.WeeeContext.ReturnReportOns.Add(new ReturnReportOn(@return.Id, 3));

            await db.WeeeContext.SaveChangesAsync();
        }
    }
}
