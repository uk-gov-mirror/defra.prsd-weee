﻿namespace EA.Weee.RequestHandlers.Tests.DataAccess.Search.FetchOrganisationSearchResultsForCache
{
    using EA.Weee.Core.Search;
    using EA.Weee.RequestHandlers.Search.FetchOrganisationSearchResultsForCache;
    using EA.Weee.Tests.Core.Model;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Xunit;

    public class FetchOrganisationSearchResultsForCacheDataAccessTests
    {
        /// <summary>
        /// Ensures that the data access does not return incomplete organisations.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task FetchCompleteOrganisations_WithIncompleteOrganisation_OrganisationNotReturned()
        {
            using (DatabaseWrapper database = new DatabaseWrapper())
            {
                // Arrange
                Organisation organisation = new Organisation();
                organisation.Id = new Guid("6BD77BBD-0BD8-4BAB-AA9F-A3E657D1CBB4");
                organisation.TradingName = "Incomplete test organisation";
                organisation.OrganisationType = EA.Weee.Domain.Organisation.OrganisationType.SoleTraderOrIndividual.Value;
                organisation.OrganisationStatus = EA.Weee.Domain.Organisation.OrganisationStatus.Incomplete.Value;

                database.Model.Organisations.Add(organisation);
                database.Model.SaveChanges();

                var dataAccess = new FetchOrganisationSearchResultsForCacheDataAccess(database.WeeeContext);

                // Act
                IList<OrganisationSearchResult> results = await dataAccess.FetchCompleteOrganisations();

                // Assert
                Assert.DoesNotContain(results,
                    r => r.OrganisationId == new Guid("6BD77BBD-0BD8-4BAB-AA9F-A3E657D1CBB4"));
            }
        }

        /// <summary>
        /// Ensures that search results representing organisations which are companies will use
        /// the 'Name' column from the database as the organisation name.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task FetchCompleteOrganisations_WithCompleteCompany_UsesNameColumnToPopulateOrganisationName()
        {
            using (DatabaseWrapper database = new DatabaseWrapper())
            {
                // Arrange
                Organisation organisation = new Organisation();
                organisation.Id = new Guid("6BD77BBD-0BD8-4BAB-AA9F-A3E657D1CBB4");
                organisation.Name = "Company Name";
                organisation.OrganisationType = EA.Weee.Domain.Organisation.OrganisationType.RegisteredCompany.Value;
                organisation.OrganisationStatus = EA.Weee.Domain.Organisation.OrganisationStatus.Complete.Value;

                database.Model.Organisations.Add(organisation);
                database.Model.SaveChanges();

                var dataAccess = new FetchOrganisationSearchResultsForCacheDataAccess(database.WeeeContext);

                // Act
                IList<OrganisationSearchResult> results = await dataAccess.FetchCompleteOrganisations();

                // Assert
                Assert.Contains(results,
                    r => r.OrganisationId == new Guid("6BD77BBD-0BD8-4BAB-AA9F-A3E657D1CBB4") && r.Name == "Company Name");
            }
        }

        /// <summary>
        /// Ensures that search results representing organisations which are sole traders or individuals will use
        /// the 'TradingName' column from the database as the organisation name.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task FetchCompleteOrganisations_WithCompleteSoleTraderOrIndividual_UsesTradingNameColumnToPopulateOrganisationName()
        {
            using (DatabaseWrapper database = new DatabaseWrapper())
            {
                // Arrange
                Organisation organisation = new Organisation();
                organisation.Id = new Guid("6BD77BBD-0BD8-4BAB-AA9F-A3E657D1CBB4");
                organisation.TradingName = "Trading Name";
                organisation.OrganisationType = EA.Weee.Domain.Organisation.OrganisationType.SoleTraderOrIndividual.Value;
                organisation.OrganisationStatus = EA.Weee.Domain.Organisation.OrganisationStatus.Complete.Value;

                database.Model.Organisations.Add(organisation);
                database.Model.SaveChanges();

                var dataAccess = new FetchOrganisationSearchResultsForCacheDataAccess(database.WeeeContext);

                // Act
                IList<OrganisationSearchResult> results = await dataAccess.FetchCompleteOrganisations();

                // Assert
                Assert.Contains(results,
                    r => r.OrganisationId == new Guid("6BD77BBD-0BD8-4BAB-AA9F-A3E657D1CBB4") && r.Name == "Trading Name");
            }
        }

        /// <summary>
        /// Ensures that search results representing organisations which are partnerships will use
        /// the 'TradingName' column from the database as the organisation name.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task FetchCompleteOrganisations_WithCompletePartnership_UsesTradingNameColumnToPopulateOrganisationName()
        {
            using (DatabaseWrapper database = new DatabaseWrapper())
            {
                // Arrange
                Organisation organisation = new Organisation();
                organisation.Id = new Guid("6BD77BBD-0BD8-4BAB-AA9F-A3E657D1CBB4");
                organisation.TradingName = "Trading Name";
                organisation.OrganisationType = EA.Weee.Domain.Organisation.OrganisationType.Partnership.Value;
                organisation.OrganisationStatus = EA.Weee.Domain.Organisation.OrganisationStatus.Complete.Value;

                database.Model.Organisations.Add(organisation);
                database.Model.SaveChanges();

                var dataAccess = new FetchOrganisationSearchResultsForCacheDataAccess(database.WeeeContext);

                // Act
                IList<OrganisationSearchResult> results = await dataAccess.FetchCompleteOrganisations();

                // Assert
                Assert.Contains(results,
                    r => r.OrganisationId == new Guid("6BD77BBD-0BD8-4BAB-AA9F-A3E657D1CBB4") && r.Name == "Trading Name");
            }
        }

        /// <summary>
        /// Ensures that search results are ordered by organisation name.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task FetchCompleteOrganisations_WithSeveralResults_ReturnsResultsOrderedByName()
        {
            using (DatabaseWrapper database = new DatabaseWrapper())
            {
                // Arrange
                Organisation organisation1 = new Organisation();
                organisation1.Id = new Guid("6BD77BBD-0BD8-4BAB-AA9F-A3E657D1CBB4");
                organisation1.Name = "Company B";
                organisation1.OrganisationType = EA.Weee.Domain.Organisation.OrganisationType.RegisteredCompany.Value;
                organisation1.OrganisationStatus = EA.Weee.Domain.Organisation.OrganisationStatus.Complete.Value;

                Organisation organisation2 = new Organisation();
                organisation2.Id = new Guid("659A5E1B-90F8-4E5C-8939-436189424AB6");
                organisation2.Name = "Company A";
                organisation2.OrganisationType = EA.Weee.Domain.Organisation.OrganisationType.RegisteredCompany.Value;
                organisation2.OrganisationStatus = EA.Weee.Domain.Organisation.OrganisationStatus.Complete.Value;

                Organisation organisation3 = new Organisation();
                organisation3.Id = new Guid("D7C37279-C3F5-44C0-B6CF-D43A968F3F29");
                organisation3.Name = "Company C";
                organisation3.OrganisationType = EA.Weee.Domain.Organisation.OrganisationType.RegisteredCompany.Value;
                organisation3.OrganisationStatus = EA.Weee.Domain.Organisation.OrganisationStatus.Complete.Value;

                database.Model.Organisations.Add(organisation1);
                database.Model.Organisations.Add(organisation2);
                database.Model.Organisations.Add(organisation3);
                database.Model.SaveChanges();

                var dataAccess = new FetchOrganisationSearchResultsForCacheDataAccess(database.WeeeContext);

                // Act
                IList<OrganisationSearchResult> results = await dataAccess.FetchCompleteOrganisations();

                // Assert
                int indexOfCompanyA = results.IndexOf(results.First(r => r.OrganisationId == new Guid("659A5E1B-90F8-4E5C-8939-436189424AB6")));
                int indexOfCompanyB = results.IndexOf(results.First(r => r.OrganisationId == new Guid("6BD77BBD-0BD8-4BAB-AA9F-A3E657D1CBB4")));
                int indexOfCompanyC = results.IndexOf(results.First(r => r.OrganisationId == new Guid("D7C37279-C3F5-44C0-B6CF-D43A968F3F29")));

                Assert.True(indexOfCompanyA < indexOfCompanyB, "Organisation search results must be returned ordered by organisation name.");
                Assert.True(indexOfCompanyB < indexOfCompanyC, "Organisation search results must be returned ordered by organisation name.");
            }
        }
    }
}