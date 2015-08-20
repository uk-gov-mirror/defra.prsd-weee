﻿namespace EA.Weee.RequestHandlers.Tests.Unit.Scheme.MemberRegistration
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Security;
    using System.Threading.Tasks;
    using DataAccess;
    using Domain;
    using Domain.Producer;
    using Domain.Scheme;
    using EA.Weee.RequestHandlers.Security;
    using FakeItEasy;
    using Helpers;
    using Prsd.Core;
    using RequestHandlers.Scheme.Interfaces;
    using RequestHandlers.Scheme.MemberRegistration;
    using Requests.Scheme.MemberRegistration;
    using Xunit;

    public class ProcessXMLFileHandlerTests
    {
        private readonly IWeeeAuthorization permissiveAuthorization = AuthorizationBuilder.CreateUserAllowedToAccessOrganisation();
        private readonly IWeeeAuthorization denyingAuthorization = AuthorizationBuilder.CreateUserDeniedFromAccessingOrganisation();
        private readonly DbContextHelper helper = new DbContextHelper();
        private readonly ProcessXMLFileHandler handler;
        private readonly IGenerateFromXml generator;
        private readonly WeeeContext context;
        private readonly DbSet<Scheme> schemesDbSet;
        private readonly DbSet<Producer> producersDbSet;
        private readonly DbSet<MemberUpload> memberUploadsDbSet;
        private readonly IXmlValidator xmlValidator;
        private readonly IXmlConverter xmlConverter;
        private readonly IXmlChargeBandCalculator xmlChargeBandCalculator;
        private static readonly Guid organisationId = Guid.NewGuid();
        private static readonly ProcessXMLFile Message = new ProcessXMLFile(organisationId, new byte[1]);

        public ProcessXMLFileHandlerTests()
        {
            memberUploadsDbSet = A.Fake<DbSet<MemberUpload>>();
            producersDbSet = A.Fake<DbSet<Producer>>();
            xmlConverter = A.Fake<IXmlConverter>();
            var schemes = new[]
            {
                FakeSchemeData()
            };

            schemesDbSet = helper.GetAsyncEnabledDbSet(schemes);

            context = A.Fake<WeeeContext>();
            A.CallTo(() => context.Schemes).Returns(schemesDbSet);
            A.CallTo(() => context.Producers).Returns(producersDbSet);
            A.CallTo(() => context.MemberUploads).Returns(memberUploadsDbSet);

            generator = A.Fake<IGenerateFromXml>();
            xmlValidator = A.Fake<IXmlValidator>();
            xmlChargeBandCalculator = A.Fake<IXmlChargeBandCalculator>();
            handler = new ProcessXMLFileHandler(context, permissiveAuthorization, xmlValidator, generator, xmlConverter, xmlChargeBandCalculator);
        }

        [Fact]
        public async void NotOrganisationUser_ThrowsSecurityException()
        {
            var authorisationDeniedHandler = new ProcessXMLFileHandler(context, denyingAuthorization, xmlValidator, generator, xmlConverter, xmlChargeBandCalculator);

            await
                Assert.ThrowsAsync<SecurityException>(
                    async () => await authorisationDeniedHandler.HandleAsync(Message));
        }

        [Fact]
        public async void ProcessXmlfile_ParsesXMLFile_SavesValidProducers()
        {
            IEnumerable<Producer> generatedProducers = new[] { TestProducer("ForestMoonOfEndor") };

            A.CallTo(() => generator.GenerateProducers(Message, A<MemberUpload>.Ignored, A<Hashtable>.Ignored))
                .Returns(Task.FromResult(generatedProducers));

            await handler.HandleAsync(Message);

            A.CallTo(() => producersDbSet.AddRange(generatedProducers)).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => context.SaveChangesAsync()).MustHaveHappened();
        }

        [Fact]
        public async void ProcessXmlfile_ParsesXMLFile_CalculateXMLChargeBand()
        {
            await handler.HandleAsync(Message);

            A.CallTo(() => xmlChargeBandCalculator.Calculate(Message)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public async void ProcessXmlfile_SchemaErrors_DoesntTryToCalculateChargesOrConvertXml()
        {
            IEnumerable<MemberUploadError> errors = new[]
            {
                new MemberUploadError(ErrorLevel.Error, MemberUploadErrorType.Schema, "any description")
            };
            A.CallTo(() => xmlValidator.Validate(Message)).Returns(errors);
            await handler.HandleAsync(Message);

            A.CallTo(() => xmlChargeBandCalculator.Calculate(Message)).MustNotHaveHappened();
            A.CallTo(() => xmlConverter.Convert(Message)).MustNotHaveHappened();
        }

        [Fact]
        public async void ProcessXmlfile_BusinessErrors_TriesToCalculateCharges()
        {
            IEnumerable<MemberUploadError> errors = new[]
            {
                new MemberUploadError(ErrorLevel.Error, MemberUploadErrorType.Business, "any description")
            };
            A.CallTo(() => xmlValidator.Validate(Message)).Returns(errors);
            await handler.HandleAsync(Message);

            A.CallTo(() => xmlChargeBandCalculator.Calculate(Message)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public async void ProcessXmlfile_NoErrors_TriesToCalculateCharges()
        {
            IEnumerable<MemberUploadError> errors = new List<MemberUploadError>();
            A.CallTo(() => xmlValidator.Validate(Message)).Returns(errors);
            await handler.HandleAsync(Message);

            A.CallTo(() => xmlChargeBandCalculator.Calculate(Message)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public async void ProcessXmlfile_InvalidXmlfile_NotGenerateProducerObjects()
        {
            IEnumerable<MemberUploadError> errors = new[]
            {
                new MemberUploadError(ErrorLevel.Error, MemberUploadErrorType.Schema, "any description")
            };
            A.CallTo(() => xmlValidator.Validate(Message)).Returns(errors);
            await handler.HandleAsync(Message);

            A.CallTo(producersDbSet).WithAnyArguments().MustNotHaveHappened();
        }

        [Fact]
        public async void ProcessXmlfile_XmlfileWithSameProducerName_NotGenerateProducerObjects()
        {
            var errors = new List<MemberUploadError>
            {
                new MemberUploadError(ErrorLevel.Error, MemberUploadErrorType.Business, "any description")
            };
            A.CallTo(() => xmlValidator.Validate(Message)).Returns(errors);

            await handler.HandleAsync(Message);

            A.CallTo(producersDbSet).WithAnyArguments().MustNotHaveHappened();
        }

        [Fact]
        public async void ProcessXmlfile_HasNoValidationErrors_HasProducerChargeCalculationErrors_ThrowsException()
        {
            var errors = new List<MemberUploadError>
            {
                new MemberUploadError(ErrorLevel.Error, MemberUploadErrorType.Business, "any description")
            };
            A.CallTo(() => xmlChargeBandCalculator.ErrorsAndWarnings).Returns(errors);

            await Assert.ThrowsAsync<ApplicationException>(async () => await handler.HandleAsync(Message));
        }

        [Fact]
        public async void ProcessXmlfile_SavesMemberUpload()
        {
            var id = await handler.HandleAsync(Message);
            Assert.NotNull(id);
        }

        public static Producer TestProducer(string tradingName)
        {
            return new Producer(Guid.NewGuid(), FakeMemberUploadData(), null, null, SystemTime.UtcNow, 0, true,
                string.Empty, null, tradingName, EEEPlacedOnMarketBandType.Lessthan5TEEEplacedonmarket,
                SellingTechniqueType.Both, ObligationType.Both,
                AnnualTurnOverBandType.Greaterthanonemillionpounds, new List<BrandName>(), new List<SICCode>(),
                true, ChargeBandType.E);
        }

        public static MemberUpload FakeMemberUploadData()
        {
            return new MemberUpload(organisationId, "FAKE Member Upload DATA");
        }

        public static Scheme FakeSchemeData()
        {
            return new Scheme(organisationId);
        }
    }
}
