﻿namespace EA.Weee.RequestHandlers.Organisations
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using EA.Prsd.Core.Mediator;
    using EA.Weee.DataAccess;
    using EA.Weee.Domain;
    using EA.Weee.Requests.Organisations;

    internal class CopyOrganisationAddressIntoRegisteredOfficeHandler : IRequestHandler<CopyOrganisationAddressIntoRegisteredOffice, Guid>
    {
        private readonly WeeeContext context;

        public CopyOrganisationAddressIntoRegisteredOfficeHandler(WeeeContext context)
        {
            this.context = context;
        }

        public async Task<Guid> HandleAsync(CopyOrganisationAddressIntoRegisteredOffice message)
        {
            var organisation = context.Organisations.FirstOrDefault(o => o.Id == message.OrganisationId);

            if (organisation == null)
            {
                throw new ArgumentException(string.Format("Could not find an organisation with Id {0}", message.OrganisationId));
            }

            var oa = organisation.OrganisationAddress;

            // we're explicitly making a copy here rather than pointing at the same address row
            // this is only assumed to be the preferred option
            var businessAddress = new Address(
                oa.Address1,
                oa.Address2,
                oa.TownOrCity,
                oa.CountyOrRegion,
                oa.Postcode,
                oa.Country,
                oa.Telephone,
                oa.Email);

            organisation.AddAddress(AddressType.RegisteredOrPPBAddress, businessAddress);

            await context.SaveChangesAsync();

            return organisation.Id;
        }
    }
}