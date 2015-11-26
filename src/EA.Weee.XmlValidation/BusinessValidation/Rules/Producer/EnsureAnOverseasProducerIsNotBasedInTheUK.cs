﻿namespace EA.Weee.XmlValidation.BusinessValidation.Rules.Producer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Xml;
    using Xml.MemberUpload;

    /// <summary>
    /// This rules ensures that an overseas producer is not based in the UK.
    /// </summary>
    public class EnsureAnOverseasProducerIsNotBasedInTheUK : IEnsureAnOverseasProducerIsNotBasedInTheUK
    {
        public RuleResult Evaluate(producerType producer)
        {
            if (producer.authorisedRepresentative == null)
            {
                return RuleResult.Pass();
            }

            if (producer.authorisedRepresentative.overseasProducer == null)
            {
                return RuleResult.Pass();
            }

            if (producer.authorisedRepresentative.overseasProducer.overseasContact == null)
            {
                return RuleResult.Pass();
            }

            contactDetailsType contact = producer.authorisedRepresentative.overseasProducer.overseasContact;

            switch (contact.address.country)
            {
                case countryType.UKENGLAND:
                case countryType.UKNORTHERNIRELAND:
                case countryType.UKSCOTLAND:
                case countryType.UKWALES:

                    string errorMessage = string.Format(
                        "You have entered {0} {1} as an authorised representative of an organisation with a UK address. Authorised representatives cannot represent producers based in the UK. Review your file.",
                        producer.GetProducerName(), producer.registrationNo);

                    return RuleResult.Fail(errorMessage, Core.Shared.ErrorLevel.Error);

                default:
                    return RuleResult.Pass();
            }
        }
    }
}
