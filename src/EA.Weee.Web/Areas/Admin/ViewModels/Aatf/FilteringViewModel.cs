﻿namespace EA.Weee.Web.Areas.Admin.ViewModels.Aatf
{
    using System.ComponentModel.DataAnnotations;

    public class FilteringViewModel
    {
        [Display(Name = "Name of AATF")]
        public string Name { get; set; }

        [Display(Name = "Approval number")]
        public string ApprovalNumber { get; set; }
    }
}