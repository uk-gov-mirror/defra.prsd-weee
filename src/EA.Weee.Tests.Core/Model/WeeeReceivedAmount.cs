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
    
    public partial class WeeeReceivedAmount
    {
        public System.Guid Id { get; set; }
        public System.Guid WeeeReceivedId { get; set; }
        public int ObligationType { get; set; }
        public int CategoryId { get; set; }
        public Nullable<decimal> Tonnage { get; set; }
        public byte[] RowVersion { get; set; }
        public Nullable<decimal> HouseholdTonnage { get; set; }
        public Nullable<decimal> NonHouseholdTonnage { get; set; }
    
        public virtual WeeeReceived WeeeReceived { get; set; }
    }
}
