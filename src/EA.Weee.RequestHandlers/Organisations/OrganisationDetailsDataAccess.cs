﻿namespace EA.Weee.RequestHandlers.Organisations
{
    using System;
    using System.Threading.Tasks;
    using EA.Weee.DataAccess;
    using EA.Weee.Domain;
    using EA.Weee.Domain.Organisation;

    public class OrganisationDetailsDataAccess : IOrganisationDetailsDataAccess
    {
        private readonly WeeeContext context;
        
        public OrganisationDetailsDataAccess(WeeeContext context)
        {
            this.context = context;
        }

        public async Task<Organisation> FetchOrganisationAsync(Guid organisationId)
        {
            Organisation organisation = await context.Organisations.FindAsync(organisationId);

            if (organisation == null)
            {
                string errorMessage = string.Format("No organisation was found with an ID of \"{0}\".", organisationId);
                throw new Exception(errorMessage);
            }

            return organisation;
        }

        public async Task<Country> FetchCountryAsync(Guid countryId)
        {
            Country country = await context.Countries.FindAsync(countryId);

            if (country == null)
            {
                string errorMessage = string.Format("No country was found with an ID of \"{0}\".", countryId);
                throw new Exception(errorMessage);
            }

            return country;
        }

        public async Task SaveAsync()
        {
            await context.SaveChangesAsync(); 
        }
    }
}