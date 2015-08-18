﻿namespace EA.Weee.RequestHandlers.Tests.Unit.Helpers
{
    using System;
    using Domain;
    using Domain.Organisation;

    internal class OrganisationHelper
    {
        internal Organisation GetOrganisationWithName(string name)
        {
            return GetOrganisationWithDetails(name, null, "1234567", OrganisationType.RegisteredCompany, OrganisationStatus.Complete);
        }

        internal Organisation GetOrganisationWithDetails(string name, string tradingName, string companyRegistrationNumber, OrganisationType type, OrganisationStatus status)
        {
            Organisation organisation;

            if (type == OrganisationType.RegisteredCompany)
            {
                organisation = Organisation.CreateRegisteredCompany(name, companyRegistrationNumber, tradingName);
            }
            else if (type == OrganisationType.Partnership)
            {
                organisation = Organisation.CreatePartnership(tradingName);
            }
            else
            {
                organisation = Organisation.CreateSoleTrader(tradingName);
            }

            organisation.AddOrUpdateAddress(AddressType.OrganisationAddress, GetAddress());

            organisation.AddOrUpdateMainContactPerson(GetContact());

            if (status == OrganisationStatus.Complete)
            {
                organisation.CompleteRegistration();
            }

            var properties = typeof(Organisation).GetProperties();

            foreach (var propertyInfo in properties)
            {
                if (propertyInfo.Name.Equals("Id", StringComparison.InvariantCultureIgnoreCase))
                {
                    var baseProperty = typeof(Organisation).BaseType.GetProperty(propertyInfo.Name);

                    baseProperty.SetValue(organisation, Guid.NewGuid(), null);

                    break;
                }
            }

            return organisation;
        }

        private static Country MakeCountry()
        {
            return new Country(new Guid(), "UK - England");
        }

        private Address GetAddress()
        {
            return new Address("1", "street", "Woking", "Hampshire", "GU21 5EE", MakeCountry(), "12345678", "test@co.uk");
        }

        private Contact GetContact()
        {
            return new Contact("Test first name", "Test last name", "Test position");
        }
    }
}