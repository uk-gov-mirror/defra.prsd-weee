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

    public partial class MemberUploadError
    {
        public System.Guid Id { get; set; }
        public byte[] RowVersion { get; set; }
        public int ErrorLevel { get; set; }
        public int ErrorType { get; set; }
        public string Description { get; set; }
        public System.Guid MemberUploadId { get; set; }
        public int LineNumber { get; set; }
    
        public virtual MemberUpload MemberUpload { get; set; }
    }
}
