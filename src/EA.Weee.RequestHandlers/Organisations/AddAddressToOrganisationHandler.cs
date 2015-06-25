﻿namespace EA.Weee.RequestHandlers.Organisations
{
    using System;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using DataAccess;
    using Domain;
    using Mappings;
    using Prsd.Core.Mediator;
    using Requests.Organisations;

    internal class AddAddressToOrganisationHandler : IRequestHandler<AddAddressToOrganisation, Guid>
    {
        private readonly WeeeContext db;

        public AddAddressToOrganisationHandler(WeeeContext context)
        {
            db = context;
        }

        public async Task<Guid> HandleAsync(AddAddressToOrganisation message)
        {
            var addresstype = ValueObjectInitializer.GetAddressType(message.TypeOfAddress);

            if (db.Organisations.FirstOrDefault(o => o.Id == message.OrganisationId) == null)
            {
                throw new ArgumentException(string.Format("Could not find an organisation with id {0}",
                    message.OrganisationId));
            }

            var organisation = await db.Organisations.SingleAsync(o => o.Id == message.OrganisationId);

            if (db.Countries.FirstOrDefault(c => c.Id == message.Address.CountryId) == null)
            {
                throw new ArgumentException(string.Format("Could not find country with id {0}",
                    message.Address.CountryId));
            }

            var country = await db.Countries.SingleAsync(c => c.Id == message.Address.CountryId);

            message.Address.CountryName = country.Name;
            var address = ValueObjectInitializer.CreateAddress(message.Address, country);
            organisation.AddAddress(addresstype, address);
            try
            {
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message);
            }
            
            return GetAddressId(addresstype, organisation);
        }

        private static Guid GetAddressId(AddressType type, Organisation organisation)
        {
            switch (type.Value)
            {
                case 1:
                    return organisation.OrganisationAddress.Id;

                case 2:
                    return organisation.BusinessAddress.Id;

                case 3:
                    return organisation.NotificationAddress.Id;

                default:
                    throw new InvalidOperationException("No address id found.");
            }
        }
    }
}