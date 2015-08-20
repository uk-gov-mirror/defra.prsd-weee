﻿namespace EA.Weee.RequestHandlers.Tests.Unit.Organisations
{
    using System;
    using System.Collections.Generic;
    using System.Security;
    using System.Threading.Tasks;
    using Core.Shared;
    using DataAccess;
    using Domain;
    using Domain.Organisation;
    using FakeItEasy;
    using Helpers;
    using RequestHandlers.Organisations;
    using RequestHandlers.Security;
    using Requests.Organisations;
    using Xunit;
    using AddressType = Core.Shared.AddressType;

    public class AddAddressToOrganisationHandlerTests
    {
        private readonly IWeeeAuthorization permissiveAuthorization = new AuthorizationBuilder().AllowOrganisationAccess().Build();
        private readonly IWeeeAuthorization denyingAuthorization = new AuthorizationBuilder().DenyOrganisationAccess().Build();
        private readonly DbContextHelper dbHelper = new DbContextHelper();

        [Fact]
        public async Task NotOrganisationUser_ThrowsSecurityException()
        {
            var context = GetPreparedContext();

            var handler = new AddAddressToOrganisationHandler(context, denyingAuthorization);
            var message = GetMessage(Guid.NewGuid(), AddressType.OrganisationAddress);

            await
                Assert.ThrowsAsync<SecurityException>(
                    async () => await handler.HandleAsync(message));
        }

        [Fact]
        public async Task NoSuchOrganisation_ThrowsArgumentException()
        {
            var context = GetPreparedContext();

            var handler = new AddAddressToOrganisationHandler(context, permissiveAuthorization);
            var message = GetMessage(Guid.NewGuid(), AddressType.OrganisationAddress);

            var exception = await
                Assert.ThrowsAsync<ArgumentException>(
                    async () => await handler.HandleAsync(message));

            Assert.True(exception.Message.ToUpperInvariant().Contains("COULD NOT FIND"));
            Assert.True(exception.Message.ToUpperInvariant().Contains("ORGANISATION"));
        }

        [Fact]
        public async Task NoSuchCountry_ThrowsArgumentException()
        {
            var organisationId = Guid.NewGuid();

            var context = GetPreparedContext(new List<Organisation> { GetOrganisationWithId(organisationId) });

            var handler = new AddAddressToOrganisationHandler(context, permissiveAuthorization);
            var message = GetMessage(organisationId, AddressType.OrganisationAddress);

            var exception = await
                Assert.ThrowsAsync<ArgumentException>(
                    async () => await handler.HandleAsync(message));

            Assert.True(exception.Message.ToUpperInvariant().Contains("COULD NOT FIND"));
            Assert.True(exception.Message.ToUpperInvariant().Contains("COUNTRY"));
        }

        [Fact]
        public async Task HappyPath_AddsAddressWithCorrectCountryName()
        {
            var organisationId = Guid.NewGuid();
            var organisation = GetOrganisationWithId(organisationId);

            var countryId = Guid.NewGuid();
            var countryName = "Some country";

            var context = GetPreparedContext(
                new List<Organisation> { organisation }, 
                new List<Country> { new Country(countryId, countryName) });

            var handler = new AddAddressToOrganisationHandler(context, permissiveAuthorization);

            var addressLine1 = "Some address line";
            var message = GetMessage(organisationId, AddressType.OrganisationAddress, new AddressData
            {
                Address1 = addressLine1, 
                CountryId = countryId, 
                TownOrCity = "Some town", 
                Telephone = "01234 567890", 
                Email = "some@email.com"
            });

            await handler.HandleAsync(message);

            Assert.Equal(addressLine1, organisation.OrganisationAddress.Address1);
            Assert.Equal(countryName, organisation.OrganisationAddress.Country.Name);
        }

        private Organisation GetOrganisationWithId(Guid id)
        {
            var organisation = Organisation.CreateSoleTrader("Some trading name");
            dbHelper.SetId(organisation, id);
            return organisation;
        }

        private AddAddressToOrganisation GetMessage(Guid organisationId, AddressType addressType, AddressData data = null)
        {
            data = data ?? new AddressData();

            var message = new AddAddressToOrganisation(
                organisationId, 
                addressType, 
                data);

            return message;
        }

        private WeeeContext GetPreparedContext(List<Organisation> organisations = null, List<Country> countries = null)
        {
            organisations = organisations ?? new List<Organisation>();
            countries = countries ?? new List<Country>();

            var context = A.Fake<WeeeContext>();
            A.CallTo(() => context.Organisations).Returns(dbHelper.GetAsyncEnabledDbSet(organisations));
            A.CallTo(() => context.Countries).Returns(dbHelper.GetAsyncEnabledDbSet(countries));

            return context;
        }
    }
}