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
    
    public partial class DataReturnVersion
    {
        public DataReturnVersion()
        {
            this.DataReturns = new HashSet<DataReturn>();
            this.DataReturnUploads = new HashSet<DataReturnUpload>();
        }
    
        public System.Guid Id { get; set; }
        public byte[] RowVersion { get; set; }
        public Nullable<System.DateTime> SubmittedDate { get; set; }
        public string SubmittingUserId { get; set; }
        public System.Guid DataReturnId { get; set; }
        public Nullable<System.Guid> EeeOutputReturnVersionId { get; set; }
        public Nullable<System.Guid> WeeeDeliveredReturnVersionId { get; set; }
        public Nullable<System.Guid> WeeeCollectedReturnVersionId { get; set; }
    
        public virtual ICollection<DataReturn> DataReturns { get; set; }
        public virtual DataReturn DataReturn { get; set; }
        public virtual ICollection<DataReturnUpload> DataReturnUploads { get; set; }
        public virtual EeeOutputReturnVersion EeeOutputReturnVersion { get; set; }
        public virtual WeeeCollectedReturnVersion WeeeCollectedReturnVersion { get; set; }
        public virtual WeeeDeliveredReturnVersion WeeeDeliveredReturnVersion { get; set; }
    }
}