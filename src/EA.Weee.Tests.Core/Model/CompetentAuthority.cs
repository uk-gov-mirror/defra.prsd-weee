//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace EA.Weee.Tests.Core.Model
{
    using System;
    using System.Collections.Generic;
    
    public partial class CompetentAuthority
    {
        public CompetentAuthority()
        {
            this.CompetentAuthorityUsers = new HashSet<CompetentAuthorityUser>();
            this.InvoiceRuns = new HashSet<InvoiceRun>();
        }
    
        public System.Guid Id { get; set; }
        public string Name { get; set; }
        public string Abbreviation { get; set; }
        public System.Guid CountryId { get; set; }
        public string Email { get; set; }
        public Nullable<decimal> AnnualChargeAmount { get; set; }
    
        public virtual ICollection<CompetentAuthorityUser> CompetentAuthorityUsers { get; set; }
        public virtual Country Country { get; set; }
        public virtual ICollection<InvoiceRun> InvoiceRuns { get; set; }
    }
}
