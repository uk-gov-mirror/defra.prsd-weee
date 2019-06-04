﻿namespace EA.Weee.Web.ViewModels.Returns
{
    using System;
    using System.Collections.Generic;

    public class ReturnsViewModel
    {
        public Guid OrganisationId { get; set; }

        public string OrganisationName { get; set; }

        public IList<ReturnsItemViewModel> Returns { get; set; }

        public ReturnsViewModel()
        {
            Returns = new List<ReturnsItemViewModel>();
        }
    }
}