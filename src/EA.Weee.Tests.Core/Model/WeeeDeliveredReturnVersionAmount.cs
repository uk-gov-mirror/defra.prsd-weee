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
    
    public partial class WeeeDeliveredReturnVersionAmount
    {
        public System.Guid Id { get; set; }
        public System.Guid WeeeDeliveredReturnVersionId { get; set; }
        public System.Guid WeeeDeliveredAmountId { get; set; }
    
        public virtual WeeeDeliveredAmount WeeeDeliveredAmount { get; set; }
        public virtual WeeeDeliveredReturnVersion WeeeDeliveredReturnVersion { get; set; }
    }
}