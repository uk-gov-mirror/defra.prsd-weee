﻿namespace EA.Weee.Web.Services.Caching
{
    using EA.Weee.Core.Scheme;
    using EA.Weee.Core.Search;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public interface IWeeeCache : ISearchResultProvider<ProducerSearchResult>, ISearchResultProvider<OrganisationSearchResult>
    {
        Task<string> FetchOrganisationName(Guid organisationId);

        Task<string> FetchSchemeName(Guid schemeId);

        Task<int> FetchUserActiveCompleteOrganisationCount(Guid userId);

        Task<SchemePublicInfo> FetchSchemePublicInfo(Guid organisationId);

        Task<IList<ProducerSearchResult>> FetchProducerSearchResultList();

        Task<IList<OrganisationSearchResult>> FetchOrganisationSearchResultList();

        Task InvalidateProducerSearch();

        Task InvalidateOrganisationSearch();
    }
}
