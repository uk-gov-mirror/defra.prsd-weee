﻿namespace EA.Weee.RequestHandlers.Organisations
{
    using System;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using DataAccess;
    using Domain;
    using Prsd.Core.Mapper;
    using Prsd.Core.Mediator;
    using Requests.Organisations;
    using Requests.Shared;
    using OrganisationType = Requests.Organisations.OrganisationType;

    internal class OrganisationByIdHandler : IRequestHandler<GetOrganisationInfo, OrganisationData>
    {
        private readonly WeeeContext context;
        private IMap<Organisation, OrganisationData> organisationMap;

        public OrganisationByIdHandler(WeeeContext context, IMap<Organisation, OrganisationData> organisationMap)
        {
            this.context = context;
            this.organisationMap = organisationMap;
        }

        public async Task<OrganisationData> HandleAsync(GetOrganisationInfo query)
        {
            // Need to materialize EF request before mapping (because mapping parses enums)
            var org = await context.Organisations
                .SingleAsync(o => o.Id == query.OrganisationId);

            return organisationMap.Map(org);
        }
    }
}