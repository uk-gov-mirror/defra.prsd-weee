﻿namespace EA.Weee.XmlValidation.BusinessValidation.Rules.Scheme
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using BusinessValidation;
    using Xml;
    using Xml.Schemas;

    public class DuplicateProducerNames : IDuplicateProducerNames
    {
        public IEnumerable<RuleResult> Evaluate(schemeType scheme)
        {
            var duplicateProducerNames = new List<string>();
            foreach (var producer in scheme.producerList)
            {
                if (!string.IsNullOrEmpty(producer.GetProducerName()))
                {
                    var isDuplicate = scheme.producerList
                        .Any(p => p != producer && p.GetProducerName() == producer.GetProducerName());

                    if (isDuplicate && !duplicateProducerNames.Contains(producer.GetProducerName()))
                    {
                        duplicateProducerNames.Add(producer.GetProducerName());
                        yield return
                            RuleResult.Fail(
                                string.Format("The Producer Name '{0}' appears more than once in the uploaded XML file",
                                    producer.GetProducerName()));
                        continue;
                    }
                }

                yield return RuleResult.Pass();
            }
        }
    }
}