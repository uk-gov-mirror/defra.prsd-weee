﻿namespace EA.Weee.Web.Areas.Admin.ViewModels.Submissions
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Web.Mvc;
    using Core.Admin;

    public class SubmissionsHistoryViewModel
    {
        [Required(ErrorMessage = "year is required.")]
        [DisplayName("Year")]
        public int SelectedYear { get; set; }

        [Required(ErrorMessage = "PCS is required.")]
        [DisplayName("PCS")]
        public string SelectedScheme { get; set; }
    
        public IList<SubmissionsHistorySearchResult> Results { get; set; }

        public IEnumerable<SelectListItem> ComplianceYears { get; set; }

        public IEnumerable<SelectListItem> SchemeNames { get; set; }

        public SubmissionsHistoryViewModel()
        {
            Results = new List<SubmissionsHistorySearchResult>();
        }
    }
}
