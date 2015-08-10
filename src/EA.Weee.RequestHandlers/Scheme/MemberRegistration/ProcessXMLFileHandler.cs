﻿namespace EA.Weee.RequestHandlers.Scheme.MemberRegistration
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using DataAccess;
    using Domain;
    using Domain.Scheme;
    using GenerateProducerObjects;
    using Interfaces;
    using Prsd.Core.Mediator;
    using Requests.Scheme.MemberRegistration;
    using XmlValidation;

    internal class ProcessXMLFileHandler : IRequestHandler<ProcessXMLFile, Guid>
    {
        private readonly WeeeContext context;
        private readonly IXmlValidator xmlValidator;
        private readonly IXmlConverter xmlConverter;
        private readonly IGenerateFromXml generateFromXml;
        private readonly IXmlChargeBandCalculator xmlChargeBandCalculator;

        public ProcessXMLFileHandler(WeeeContext context, IXmlValidator xmlValidator, IGenerateFromXml generateFromXml, IXmlConverter xmlConverter, IXmlChargeBandCalculator xmlChargeBandCalculator)
        {
            this.context = context;
            this.xmlValidator = xmlValidator;
            this.xmlConverter = xmlConverter;
            this.xmlChargeBandCalculator = xmlChargeBandCalculator;
            this.generateFromXml = generateFromXml;
        }

        public async Task<Guid> HandleAsync(ProcessXMLFile message)
        {
            var errors = xmlValidator.Validate(message);

            List<MemberUploadError> memberUploadErrors = errors as List<MemberUploadError> ?? errors.ToList();
            
            //calcuate charge band for producers if no schema errors
            Hashtable producerCharges = null;
            decimal totalCharges = 0;
            
            if (memberUploadErrors.All(e => e.ErrorType != MemberUploadErrorType.Schema))
            {
                producerCharges = ProducerCharges(message, ref totalCharges);
                if (xmlChargeBandCalculator.ErrorsAndWarnings.Any(e => e.ErrorLevel == ErrorLevel.Error)
                    && memberUploadErrors.All(e => e.ErrorLevel != ErrorLevel.Error))
                {
                    throw new ApplicationException(String.Format(
                        "Upload for Organisation '{0}' has no validation errors, but does have producer charge calculation errors which are not currently being enforced",
                        message.OrganisationId));
                }
            }

            var scheme = await context.Schemes.SingleAsync(c => c.OrganisationId == message.OrganisationId);
            var upload = new MemberUpload(message.OrganisationId, xmlConverter.XmlToUtf8String(message), memberUploadErrors.ToList(), totalCharges, scheme.Id);

            //Build producers domain object if there are no errors(schema or business during validation of xml file.
            if (!memberUploadErrors.Any())
            {
                var producers = await generateFromXml.Generate(message, upload, producerCharges);
                context.MemberUploads.Add(upload);
                context.Producers.AddRange(producers);
            }
            else
            {
                context.MemberUploads.Add(upload);
            }
            await context.SaveChangesAsync();
            return upload.Id;
        }

        private Hashtable ProducerCharges(ProcessXMLFile message, ref decimal totalCharges)
        {
            var producerCharges = xmlChargeBandCalculator.Calculate(message);
              
                totalCharges = producerCharges.Cast<DictionaryEntry>()
                    .Aggregate(totalCharges,
                        (current, producerCharge) => current + ((ProducerCharge)producerCharge.Value).ChargeAmount);
            return producerCharges;
        }
    }
}
