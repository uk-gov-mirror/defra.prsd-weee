﻿namespace EA.Weee.DataAccess.Tests.Integration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Domain;
    using Domain.User;
    using EA.Weee.Domain.AatfReturn;
    using EA.Weee.Domain.DataReturns;
    using EA.Weee.Domain.Lookup;
    using EA.Weee.Requests.AatfReturn.Obligated;
    using EA.Weee.Tests.Core.Model;
    using FluentAssertions;
    using RequestHandlers.AatfReturn.ObligatedGeneric;
    using Weee.Tests.Core;
    using Xunit;
    using Aatf = Domain.AatfReturn.Aatf;
    using Organisation = Domain.Organisation.Organisation;
    using Return = Domain.AatfReturn.Return;
    using Scheme = Domain.Scheme.Scheme;
    using WeeeReceived = Domain.AatfReturn.WeeeReceived;
    using WeeeReceivedAmount = Domain.AatfReturn.WeeeReceivedAmount;

    public static class ObligatedWeeeIntegrationCommon
    {
        public static Return CreateReturn(Organisation organisation, string createdById, FacilityType facilityType = null)
        {
            if (facilityType == null)
            {
                facilityType = FacilityType.Aatf;
            }
            return new Return(organisation, new Quarter(2019, QuarterType.Q1), createdById, facilityType);
        }

        public static Aatf CreateAatf(DatabaseWrapper database, Organisation organisation, AatfContact contact, Domain.Country country)
        {
            var aatf = new Aatf("aatfname", database.WeeeContext.UKCompetentAuthorities.First(), "number", AatfStatus.Approved, organisation, CreateAatfAddress(database), AatfSize.Large, DateTime.Now, contact, FacilityType.Aatf, 2019);
            return aatf;
        }

        public static Scheme CreateScheme(Organisation organisation)
        {
            return new Scheme(organisation);
        }

        public static AatfAddress CreateAatfAddress(DatabaseWrapper database)
        {
            return AddressHelper.GetAatfAddress(database);
        }

        public static Organisation CreateOrganisation()
        {
            var companyName = "Test Name" + Guid.NewGuid();
            var tradingName = "Test Trading Name" + Guid.NewGuid();
            const string companyRegistrationNumber = "ABC12345";

            var organisation = Organisation.CreateRegisteredCompany(companyName, companyRegistrationNumber, tradingName);
            return organisation;
        }

        public static AatfContact CreateDefaultContact(Domain.Country country)
        {
            return new AatfContact("First Name", "Last Name", "Manager", "1 Address Lane", "Address Ward", "Town", "County", "Postcode", country, "01234 567890", "email@email.com");
        }
    }
}