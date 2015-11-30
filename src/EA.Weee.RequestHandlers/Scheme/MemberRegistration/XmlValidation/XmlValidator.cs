﻿namespace EA.Weee.RequestHandlers.Scheme.MemberRegistration.XmlValidation
{
    using System.Collections.Generic;
    using System.Linq;
    using Core.Helpers;
    using Core.Scheme;
    using Domain;
    using Domain.Scheme;
    using Interfaces;
    using Requests.Scheme.MemberRegistration;
    using Weee.XmlValidation.BusinessValidation.MemberRegistration;
    using Weee.XmlValidation.Errors;
    using Weee.XmlValidation.SchemaValidation;
    using Xml.Converter;
    using Xml.Deserialization;
    using Xml.MemberRegistration;

    public class XmlValidator : IXmlValidator
    {
        private readonly ISchemaValidator schemaValidator;
        private readonly IMemberRegistrationBusinessValidator businessValidator;

        private readonly IXmlConverter xmlConverter;
        private readonly IXmlErrorTranslator errorTranslator;

        public XmlValidator(ISchemaValidator schemaValidator, IXmlConverter xmlConverter, IMemberRegistrationBusinessValidator businessValidator, IXmlErrorTranslator errorTranslator)
        {
            this.schemaValidator = schemaValidator;
            this.businessValidator = businessValidator;
            this.errorTranslator = errorTranslator;
            this.xmlConverter = xmlConverter;
        }

        public IEnumerable<MemberUploadError> Validate(ProcessXMLFile message)
        {
            // Validate against the schema
            var errors = schemaValidator.Validate(message.Data, @"EA.Weee.Xml.MemberRegistration.v3schema.xsd", @"http://www.environment-agency.gov.uk/WEEE/XMLSchema", SchemaVersion.Version_3_07)
                .Select(e => e.ToMemberUploadError())
                .ToList();

            if (errors.Any())
            {
                return errors;
            }

            schemeType deserializedXml;

            try
            {
                // Validate deserialized XML against business rules
                deserializedXml = xmlConverter.Deserialize(xmlConverter.Convert(message.Data));
            }
            catch (XmlDeserializationFailureException e)
            {
                // Couldn't deserialise - can't go any further, add an error and bail out here
                var exceptionMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
                var friendlyMessage = errorTranslator.MakeFriendlyErrorMessage(exceptionMessage, SchemaVersion.Version_3_07);
                errors.Add(new MemberUploadError(ErrorLevel.Error, UploadErrorType.Schema, friendlyMessage));

                return errors;
            }

            errors = businessValidator.Validate(deserializedXml, message.OrganisationId)
                .Select(err => new MemberUploadError(err.ErrorLevel.ToDomainEnumeration<ErrorLevel>(), UploadErrorType.Business, err.Message))
                .ToList();

            return errors;
        }
    }
}
