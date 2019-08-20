﻿namespace EA.Weee.RequestHandlers.Tests.Unit.Admin.Aatf
{
    using System;
    using System.Security;
    using System.Threading.Tasks;
    using Core.AatfReturn;
    using Domain;
    using Domain.AatfReturn;
    using FakeItEasy;
    using FluentAssertions;
    using RequestHandlers.AatfReturn;
    using RequestHandlers.AatfReturn.Internal;
    using RequestHandlers.Admin.Aatf;
    using RequestHandlers.Organisations;
    using RequestHandlers.Security;
    using Requests.Admin.Aatf;
    using Weee.Security;
    using Weee.Tests.Core;
    using Xunit;

    public class EditAatfContactHandlerTests
    {
        private readonly IWeeeAuthorization authorization;
        private readonly IAatfDataAccess aatfDataAccess;
        private readonly IGenericDataAccess genericDataAccess;
        private readonly IOrganisationDetailsDataAccess organisationDetailsDataAccess;
        private readonly EditAatfContactHandler handler;

        public EditAatfContactHandlerTests()
        {
            authorization = A.Fake<IWeeeAuthorization>();
            aatfDataAccess = A.Fake<IAatfDataAccess>();
            genericDataAccess = A.Fake<IGenericDataAccess>();
            organisationDetailsDataAccess = A.Fake<IOrganisationDetailsDataAccess>();

            handler = new EditAatfContactHandler(authorization, aatfDataAccess, genericDataAccess, organisationDetailsDataAccess);
        }

        [Fact]
        public async Task HandleAsync_NoInternalAccess_ThrowsSecurityException()
        {
            var authorization = new AuthorizationBuilder().DenyInternalAreaAccess().Build();

            var handler = new EditAatfContactHandler(authorization, aatfDataAccess, genericDataAccess, organisationDetailsDataAccess);

            Func<Task> action = async () => await handler.HandleAsync(A.Dummy<EditAatfContact>());

            await action.Should().ThrowAsync<SecurityException>();
        }

        [Fact]
        public async Task HandleAsync_NoAdminRoleAccess_ThrowsSecurityException()
        {
            var authorization = new AuthorizationBuilder().AllowInternalAreaAccess().DenyRole(Roles.InternalAdmin).Build();

            var handler = new EditAatfContactHandler(authorization, aatfDataAccess, genericDataAccess, organisationDetailsDataAccess);

            Func<Task> action = async () => await handler.HandleAsync(A.Dummy<EditAatfContact>());

            await action.Should().ThrowAsync<SecurityException>();
        }

        [Fact]
        public async Task HandleAsync_GivenMessageContainingUpdatedAddress_DetailsAreUpdatedCorrectly()
        {
            var addressData = new AatfContactAddressData()
            {
                Address1 = "Address1",
                Address2 = "Address2",
                TownOrCity = "Town",
                CountyOrRegion = "County",
                Postcode = "Postcode",
                CountryId = Guid.NewGuid()
            };

            var updateRequest = new EditAatfContact()
            {
                ContactData = new AatfContactData()
                {
                    FirstName = "First Name",
                    LastName = "Last Name",
                    Position = "Position",
                    AddressData = addressData,
                    Telephone = "01234 567890",
                    Email = "email@email.com",
                    Id = Guid.NewGuid()
                }
            };

            var returnContact = A.Fake<AatfContact>();

            var country = new Country(A.Dummy<Guid>(), A.Dummy<string>());

            A.CallTo(() => organisationDetailsDataAccess.FetchCountryAsync(updateRequest.ContactData.AddressData.CountryId)).Returns(country);
            A.CallTo(() => genericDataAccess.GetById<AatfContact>(updateRequest.ContactData.Id)).Returns(returnContact);

            await handler.HandleAsync(updateRequest);

            A.CallTo(() => aatfDataAccess.UpdateContact(returnContact, updateRequest.ContactData, country)).MustHaveHappened(Repeated.Exactly.Once);
        }
    }
}