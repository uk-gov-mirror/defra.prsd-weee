﻿namespace EA.Weee.XmlValidation.BusinessValidation.Rules.Producer
{
    using EA.Weee.Xml.Schemas;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// This rules ensures that an overseas producer does not have an address based in the UK.
    /// </summary>
    public interface IEnsureAnOverseasProducerIsNotBasedInTheUK
    {
        RuleResult Evaluate(producerType producer);
    }
}