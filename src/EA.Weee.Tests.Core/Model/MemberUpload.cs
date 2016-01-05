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
    
    public partial class MemberUpload
    {
        public MemberUpload()
        {
            this.MemberUploadErrors = new HashSet<MemberUploadError>();
            this.ProducerSubmissions = new HashSet<ProducerSubmission>();
        }
    
        public System.Guid Id { get; set; }
        public byte[] RowVersion { get; set; }
        public System.Guid OrganisationId { get; set; }
        public string Data { get; set; }
        public Nullable<int> ComplianceYear { get; set; }
        public System.Guid SchemeId { get; set; }
        public bool IsSubmitted { get; set; }
        public decimal TotalCharges { get; set; }
        public System.TimeSpan ProcessTime { get; set; }
        public string FileName { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public Nullable<System.DateTime> UpdatedDate { get; set; }
        public string CreatedById { get; set; }
        public string UpdatedById { get; set; }
        public Nullable<System.Guid> InvoiceRunId { get; set; }
        public Nullable<System.DateTime> SubmittedDate { get; set; }
        public string SubmittedByUserId { get; set; }
    
        public virtual Organisation Organisation { get; set; }
        public virtual Scheme Scheme { get; set; }
        public virtual ICollection<MemberUploadError> MemberUploadErrors { get; set; }
        public virtual ICollection<ProducerSubmission> ProducerSubmissions { get; set; }
        public virtual AspNetUser AspNetUser { get; set; }
        public virtual AspNetUser AspNetUser1 { get; set; }
        public virtual AspNetUser AspNetUser2 { get; set; }
        public virtual InvoiceRun InvoiceRun { get; set; }
    }
}
