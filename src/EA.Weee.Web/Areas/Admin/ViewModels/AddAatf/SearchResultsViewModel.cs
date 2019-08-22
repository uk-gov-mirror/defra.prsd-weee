﻿namespace EA.Weee.Web.Areas.Admin.ViewModels.AddAatf
{
    using EA.Weee.Core.AatfReturn;
    using EA.Weee.Core.Search;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;

    public class SearchResultsViewModel
    {
        [Required]
        [DisplayName("Search term")]
        public string SearchTerm { get; set; }

        public IList<OrganisationSearchResult> Results { get; set; }

        [Required(ErrorMessage = "You must choose an organisation")]
        [DisplayName("Select an organisation to add")]
        public Guid? SelectedOrganisationId { get; set; }

        public FacilityType FacilityType { get; set; }
    }
}