//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace EA.Weee.DataAccess.Tests.Integration.Model
{
    using System;
    using System.Collections.Generic;
    
    public partial class CompetentAuthority
    {
        public CompetentAuthority()
        {
            this.CompetentAuthorityUsers = new HashSet<CompetentAuthorityUser>();
        }
    
        public System.Guid Id { get; set; }
        public string Name { get; set; }
        public string Abbreviation { get; set; }
        public System.Guid CountryId { get; set; }
    
        public virtual ICollection<CompetentAuthorityUser> CompetentAuthorityUsers { get; set; }
        public virtual Country Country { get; set; }
    }
}
