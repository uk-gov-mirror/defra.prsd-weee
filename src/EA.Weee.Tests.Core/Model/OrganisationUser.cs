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

    public partial class OrganisationUser
    {
        public System.Guid Id { get; set; }
        public string UserId { get; set; }
        public System.Guid OrganisationId { get; set; }
        public int UserStatus { get; set; }
        public byte[] RowVersion { get; set; }
    
        public virtual Organisation Organisation { get; set; }
        public virtual AspNetUser AspNetUser { get; set; }
    }
}
