﻿namespace EA.Weee.Web.Areas.Test.ViewModels
{
    using Core.Organisations;
    using EA.Weee.Web.ViewModels.Shared;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;

    public class SelectOrganisationViewModel
    {
        [Required]
        [DisplayName("Company Name")]
        public string CompanyName { get; set; }

        public IList<OrganisationSearchData> MatchingOrganisations { get; set; }

        public PagingViewModel PagingViewModel { get; set; }
    }
}