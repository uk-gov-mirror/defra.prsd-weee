﻿namespace EA.Weee.Domain.DataReturns
{
    using System.Diagnostics.CodeAnalysis;
    using EA.Weee.Domain.Lookup;
    using Prsd.Core;

    public class AeDeliveredAmount : WeeeDeliveredAmount
    {
        public virtual AeDeliveryLocation AeDeliveryLocation { get; private set; }

        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1305:FieldNamesMustNotUseHungarianNotation", Justification = "aeDeliveryLocation name is valid.")]
        public AeDeliveredAmount(ObligationType obligationType, WeeeCategory weeeCategory, decimal tonnage, AeDeliveryLocation aeDeliveryLocation, DataReturnVersion dataReturnVersion) :
            base(obligationType, weeeCategory, tonnage, dataReturnVersion)
        {
            Guard.ArgumentNotNull(() => aeDeliveryLocation, aeDeliveryLocation);
            Guard.ArgumentNotNull(() => dataReturnVersion, dataReturnVersion);

            AeDeliveryLocation = aeDeliveryLocation;
        }

        protected AeDeliveredAmount()
        {
        }
    }
}
