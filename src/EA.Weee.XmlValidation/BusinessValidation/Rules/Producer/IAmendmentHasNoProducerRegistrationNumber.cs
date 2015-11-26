﻿namespace EA.Weee.XmlValidation.BusinessValidation.Rules.Producer
{
    using BusinessValidation;
    using Xml.MemberRegistration;

    public interface IAmendmentHasNoProducerRegistrationNumber
    {
        RuleResult Evaluate(producerType producer);
    }
}
