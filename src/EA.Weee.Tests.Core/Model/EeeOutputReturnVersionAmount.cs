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
    
    public partial class EeeOutputReturnVersionAmount
    {
        public System.Guid Id { get; set; }
        public System.Guid EeeOutputReturnVersionId { get; set; }
        public System.Guid EeeOuputAmountId { get; set; }
    
        public virtual EeeOutputAmount EeeOutputAmount { get; set; }
        public virtual EeeOutputReturnVersion EeeOutputReturnVersion { get; set; }
    }
}