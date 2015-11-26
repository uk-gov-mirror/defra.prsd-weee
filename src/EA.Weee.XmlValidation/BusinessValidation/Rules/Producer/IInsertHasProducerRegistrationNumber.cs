﻿namespace EA.Weee.XmlValidation.BusinessValidation.Rules.Producer
{
    using BusinessValidation;
    using Xml.MemberUpload;

    public interface IInsertHasProducerRegistrationNumber
    {
        RuleResult Evaluate(producerType producer);
    }
}
