﻿namespace EA.Weee.Web.ViewModels.Organisation
{
    using System.ComponentModel.DataAnnotations;

    public class OrganisationContactDetailsViewModel
    {
        [Required]
        [DataType(DataType.Text)]
        [StringLength(50)]
        [Display(Name = "Address line 1")]
        public string Address1 { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Address line 2")]
        public string Address2 { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Town or city")]
        public string TownOrCity { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "County or region")]
        public string CountyOrRegion { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Postcode")]
        public string Postcode { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Country")]
        public string Country { get; set; }

        [Required]
        [RegularExpression("^[0-9+\\(\\)#\\.\\s\\/ext-]+$", ErrorMessage = "The entered phone number is invalid")]
        [DataType(DataType.Text)]
        [Display(Name = "Phone")]
        public string Phone { get; set; }

        [Required]
        [EmailAddress]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }
}