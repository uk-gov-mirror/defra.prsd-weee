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
    
    public partial class WeeeDeliveredReturnVersion
    {
        public WeeeDeliveredReturnVersion()
        {
            this.DataReturnVersions = new HashSet<DataReturnVersion>();
            this.WeeeDeliveredReturnVersionAmounts = new HashSet<WeeeDeliveredReturnVersionAmount>();
        }
    
        public System.Guid Id { get; set; }
    
        public virtual ICollection<DataReturnVersion> DataReturnVersions { get; set; }
        public virtual ICollection<WeeeDeliveredReturnVersionAmount> WeeeDeliveredReturnVersionAmounts { get; set; }
    }
}