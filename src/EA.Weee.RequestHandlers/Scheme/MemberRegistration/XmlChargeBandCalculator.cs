﻿namespace EA.Weee.RequestHandlers.Scheme.MemberRegistration
{
    using System.Collections;
    using System.Collections.Generic;
    using DataAccess;
    using Domain;
    using Domain.Scheme;
    using Interfaces;
    using Requests.Scheme.MemberRegistration;
    using XmlValidation.Extensions;

    public class XmlChargeBandCalculator : IXmlChargeBandCalculator
    {
        private readonly WeeeContext context;
        private readonly IXmlConverter xmlConverter;
        public List<MemberUploadError> ErrorsAndWarnings { get; set; }

        public XmlChargeBandCalculator(WeeeContext context, IXmlConverter xmlConverter)
        {
            this.context = context;
            this.xmlConverter = xmlConverter;
            ErrorsAndWarnings = new List<MemberUploadError>();
        }

        public Hashtable Calculate(ProcessXMLFile message)
        {
            schemeType schemeType = xmlConverter.Deserialize(xmlConverter.Convert(message));

            var producerChargeBandCalculator = new ProducerChargeBandCalculator(context);
            var producerCharges = new Hashtable();

            foreach (var producer in schemeType.producerList)
            {
                var producerName = producer.GetProducerName();
                var producerCharge = producerChargeBandCalculator.CalculateCharge(producer);
                if (producerCharge != null)
                {
                    if (!producerCharges.ContainsKey(producerName))
                    {
                        producerCharges.Add(producerName, producerCharge);
                    }
                    else
                    {
                        ErrorsAndWarnings.Add(
                            new MemberUploadError(
                                ErrorLevel.Error,
                                MemberUploadErrorType.Business,
                                string.Format(
                                    "We are unable to check for warnings associated with the charge band of the producer {0} until the duplicate name has been fixed.",
                                    producerName)));
                    }
                }
            }

            return producerCharges;
        }
    }
}