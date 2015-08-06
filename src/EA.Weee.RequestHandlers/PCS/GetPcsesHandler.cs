﻿namespace EA.Weee.RequestHandlers.PCS
{
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using Core.PCS;
    using DataAccess;
    using Domain.Organisation;
    using Domain.PCS;
    using Prsd.Core.Mapper;
    using Prsd.Core.Mediator;
    using Requests.PCS;

    internal class GetPcsesHandler : IRequestHandler<GetPcses, List<PcsData>>
    {
        private readonly WeeeContext context;
        private readonly IMap<Scheme, PcsData> pcsMap;

        public GetPcsesHandler(WeeeContext context, IMap<Scheme, PcsData> pcsMap)
        {
            this.context = context;
            this.pcsMap = pcsMap;
        }

        public async Task<List<PcsData>> HandleAsync(GetPcses message)
        {
            var schemes = context.Schemes
                .Where(s => s.Organisation.OrganisationStatus == OrganisationStatus.Complete)
                .OrderBy(s => s.Organisation.Name);

            return await schemes.Select(s => pcsMap.Map(s)).ToListAsync();
        }
    }
}
