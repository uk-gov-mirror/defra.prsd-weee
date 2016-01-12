﻿namespace EA.Weee.Domain.Tests.Unit.Producer
{
    using System;
    using System.Collections.Generic;
    using EA.Weee.Domain.Producer;
    using EA.Weee.Domain.Scheme;
    using FakeItEasy;
    using Lookup;
    using Xunit;

    public class ProducerSubmissionTests
    {
        [Fact]
        public void Producer_EqualsNullParameter_ReturnsFalse()
        {
            var producer = ProducerBuilder.NewProducer;

            Assert.NotEqual(producer, null);
        }

        [Fact]
        public void Producer_EqualsObjectParameter_ReturnsFalse()
        {
            var producer = ProducerBuilder.NewProducer;

            Assert.NotEqual(producer, new object());
        }

        [Fact]
        public void Producer_EqualsSameInstance_ReturnsTrue()
        {
            var producer = ProducerBuilder.NewProducer;

            Assert.Equal(producer, producer);
        }

        [Fact]
        public void Producer_EqualsProducerSameDetails_ReturnsTrue()
        {
            var producer = ProducerBuilder.NewProducer;
            var producer2 = ProducerBuilder.NewProducer;

            Assert.Equal(producer, producer2);
        }

        [Fact]
        public void Producer_EqualsProducerDifferentTradingName_ReturnsFalse()
        {
            var producer = ProducerBuilder.NewProducer;
            var producer2 = ProducerBuilder.WithTradingName("test trading name");

            Assert.NotEqual(producer, producer2);
        }

        [Fact]
        public void Producer_EqualsProducerDifferentVatRegistered_ReturnsFalse()
        {
            var producer = ProducerBuilder.NewProducer;
            var producer2 = ProducerBuilder.VatRegistered(false);

            Assert.NotEqual(producer, producer2);
        }

        [Fact]
        public void Producer_EqualsProducerDifferentAnnualTurnover_ReturnsFalse()
        {
            var producer = ProducerBuilder.NewProducer;
            var producer2 = ProducerBuilder.WithAnnualTurnover(1000);

            Assert.NotEqual(producer, producer2);
        }

        [Fact]
        public void Producer_EqualsProducerDifferentObligationType_ReturnsFalse()
        {
            var producer = ProducerBuilder.WithObligationType(ObligationType.B2B);
            var producer2 = ProducerBuilder.WithObligationType(ObligationType.B2C);

            Assert.NotEqual(producer, producer2);
        }

        [Fact]
        public void Producer_EqualsProducerDifferentAnnualTurnOverBandType_ReturnsFalse()
        {
            var producer = ProducerBuilder.NewProducer;
            var producer2 = ProducerBuilder.WithAnnualTurnOverBandType(new CustomAnnualTurnOverBandType(1000));

            Assert.NotEqual(producer, producer2);
        }

        [Fact]
        public void Producer_EqualsProducerDifferentSellingTechniqueType_ReturnsFalse()
        {
            var producer = ProducerBuilder.NewProducer;
            var producer2 = ProducerBuilder.WithSellingTechniqueType(new CustomSellingTechniqueType(1000));

            Assert.NotEqual(producer, producer2);
        }

        [Fact]
        public void Producer_EqualsProducerDifferentEEEPlacedOnMarketBandType_ReturnsFalse()
        {
            var producer = ProducerBuilder.NewProducer;
            var producer2 = ProducerBuilder.WithEEEPlacedOnMarketBandType(new CustomEEEPlacedOnMarketBandType(1000));

            Assert.NotEqual(producer, producer2);
        }

        [Fact]
        public void Producer_EqualsProducerDifferentAuthorisedRepresentative_ReturnsFalse()
        {
            var producer = ProducerBuilder.WithAuthorisedRepresentative(new AlwaysUnequalAuthorisedRepresentative());
            var producer2 = ProducerBuilder.WithAuthorisedRepresentative(new AlwaysUnequalAuthorisedRepresentative());

            Assert.NotEqual(producer, producer2);
        }

        [Fact]
        public void Producer_EqualsProducerDifferentProducerBusiness_ReturnsFalse()
        {
            var producer = ProducerBuilder.WithProducerBusiness(new AlwaysUnequalProducerBusiness());
            var producer2 = ProducerBuilder.WithProducerBusiness(new AlwaysUnequalProducerBusiness());

            Assert.NotEqual(producer, producer2);
        }

        [Fact]
        public void Producer_EqualsProducerDifferentBrandNames_ReturnsFalse()
        {
            var brandNames1 = new List<BrandName>() { new BrandName("Test Brand name 1"), new BrandName("Test Brand name 2") };
            var brandNames2 = new List<BrandName>() { new BrandName("Test Brand name 3"), new BrandName("Test Brand name 1") };

            var producer = ProducerBuilder.WithBrandNames(brandNames1);
            var producer2 = ProducerBuilder.WithBrandNames(brandNames2);

            Assert.NotEqual(producer, producer2);
        }

        [Fact]
        public void Producer_EqualsProducerDifferentSICCodes_ReturnsFalse()
        {
            var sicCodes1 = new List<SICCode>() { new SICCode("SIC Code name 1"), new SICCode("SIC Code name 2") };
            var sicCodes2 = new List<SICCode>() { new SICCode("SIC Code name 3"), new SICCode("SIC Code name 1") };

            var producer = ProducerBuilder.WithSICCodes(sicCodes1);
            var producer2 = ProducerBuilder.WithSICCodes(sicCodes2);

            Assert.NotEqual(producer, producer2);
        }

        [Fact]
        public void Producer_SetAsInvoiced_UpdatesInvoicedProperty()
        {
            var producer = ProducerBuilder.NewProducer;
            producer.SetAsInvoiced();

            Assert.True(producer.Invoiced);
        }

        [Fact]
        public void Producer_SetAsInvoiced_ThrowsInvalidOperationException_IfAlreadyInvoiced()
        {
            var producer = ProducerBuilder.NewProducer;
            producer.SetAsInvoiced();

            Assert.Throws<InvalidOperationException>(() => producer.SetAsInvoiced());
        }

        private class AlwaysEqualAuthorisedRepresentative : AuthorisedRepresentative
        {
            public override bool Equals(AuthorisedRepresentative other)
            {
                return true;
            }
        }

        private class AlwaysUnequalAuthorisedRepresentative : AuthorisedRepresentative
        {
            public override bool Equals(AuthorisedRepresentative other)
            {
                return false;
            }
        }

        private class AlwaysEqualProducerBusiness : ProducerBusiness
        {
            public override bool Equals(ProducerBusiness other)
            {
                return true;
            }
        }

        private class AlwaysUnequalProducerBusiness : ProducerBusiness
        {
            public override bool Equals(ProducerBusiness other)
            {
                return false;
            }
        }

        private class CustomEEEPlacedOnMarketBandType : EEEPlacedOnMarketBandType
        {
            public CustomEEEPlacedOnMarketBandType(int value)
            {
                Value = value;
            }
        }

        private class CustomAnnualTurnOverBandType : AnnualTurnOverBandType
        {
            public CustomAnnualTurnOverBandType(int value)
            {
                Value = value;
            }
        }

        private class CustomSellingTechniqueType : SellingTechniqueType
        {
            public CustomSellingTechniqueType(int value)
            {
                Value = value;
            }
        }

        private class ProducerBuilder
        {
            private string registrationNumber = "registrationNumber";
            private string tradingName = "tradingName";
            private bool vatRegistered = true;
            private decimal annualTurnover = 0;

            private ObligationType obligationType = ObligationType.B2B;
            private AnnualTurnOverBandType annualTurnOverBandType = new CustomAnnualTurnOverBandType(0);
            private SellingTechniqueType sellingTechniqueType = new CustomSellingTechniqueType(0);
            private EEEPlacedOnMarketBandType eeePlacedOnMarketBandType = new CustomEEEPlacedOnMarketBandType(0);
            private ChargeBandAmount chargeBandAmount = A.Dummy<ChargeBandAmount>();

            private AuthorisedRepresentative authorisedRepresentative = new AlwaysEqualAuthorisedRepresentative();
            private ProducerBusiness producerBusiness = new AlwaysEqualProducerBusiness();
            private List<BrandName> brandNames;
            private List<SICCode> sicCodes;

            private ProducerBuilder()
            {
                brandNames = new List<BrandName>();
                brandNames.Add(new BrandName("BrandName1"));
                brandNames.Add(new BrandName("BrandName2"));

                sicCodes = new List<SICCode>();
                sicCodes.Add(new SICCode("SICCode1"));
                sicCodes.Add(new SICCode("SICCode2"));
            }

            private ProducerSubmission Build()
            {
                Scheme scheme = new Scheme(
                    A.Dummy<Guid>());

                var memberUpload = new MemberUpload(
                    A.Dummy<Guid>(),
                    A.Dummy<string>(),
                    A.Dummy<List<MemberUploadError>>(),
                    A.Dummy<decimal>(),
                    2017,
                    scheme,
                    A.Dummy<string>(),
                    A.Dummy<string>());

                var updatedDate = A.Dummy<DateTime>();
                var ceaseToExist = A.Dummy<DateTime?>();

                RegisteredProducer registeredProducer = new RegisteredProducer(
                    registrationNumber,
                    2017,
                    scheme);

                ProducerSubmission producerSubmission = new ProducerSubmission(
                    registeredProducer,
                    memberUpload,
                    producerBusiness,
                    authorisedRepresentative,
                    updatedDate,
                    annualTurnover,
                    vatRegistered,
                    ceaseToExist,
                    tradingName,
                    eeePlacedOnMarketBandType,
                    sellingTechniqueType,
                    obligationType,
                    annualTurnOverBandType,
                    brandNames,
                    sicCodes,
                    chargeBandAmount,
                    (decimal)5.0);

                registeredProducer.SetCurrentSubmission(producerSubmission);

                return producerSubmission;
            }

            public static ProducerSubmission NewProducer
            {
                get { return new ProducerBuilder().Build(); }
            }

            public static ProducerSubmission WithTradingName(string tradingName)
            {
                var builder = new ProducerBuilder();
                builder.tradingName = tradingName;

                return builder.Build();
            }

            public static ProducerSubmission VatRegistered(bool vatRegistered)
            {
                var builder = new ProducerBuilder();
                builder.vatRegistered = vatRegistered;

                return builder.Build();
            }

            public static ProducerSubmission WithAnnualTurnover(int annualTurnover)
            {
                var builder = new ProducerBuilder();
                builder.annualTurnover = annualTurnover;

                return builder.Build();
            }

            public static ProducerSubmission WithObligationType(ObligationType obligationType)
            {
                var builder = new ProducerBuilder();
                builder.obligationType = obligationType;

                return builder.Build();
            }

            public static ProducerSubmission WithAnnualTurnOverBandType(AnnualTurnOverBandType annualTurnOverBandType)
            {
                var builder = new ProducerBuilder();
                builder.annualTurnOverBandType = annualTurnOverBandType;

                return builder.Build();
            }

            public static ProducerSubmission WithSellingTechniqueType(SellingTechniqueType sellingTechniqueType)
            {
                var builder = new ProducerBuilder();
                builder.sellingTechniqueType = sellingTechniqueType;

                return builder.Build();
            }

            public static ProducerSubmission WithEEEPlacedOnMarketBandType(EEEPlacedOnMarketBandType eeePlacedOnMarketBandType)
            {
                var builder = new ProducerBuilder();
                builder.eeePlacedOnMarketBandType = eeePlacedOnMarketBandType;

                return builder.Build();
            }

            public static ProducerSubmission WithAuthorisedRepresentative(AuthorisedRepresentative authorisedRepresentative)
            {
                var builder = new ProducerBuilder();
                builder.authorisedRepresentative = authorisedRepresentative;

                return builder.Build();
            }

            public static ProducerSubmission WithProducerBusiness(ProducerBusiness producerBusiness)
            {
                var builder = new ProducerBuilder();
                builder.producerBusiness = producerBusiness;

                return builder.Build();
            }

            public static ProducerSubmission WithBrandNames(List<BrandName> brandNames)
            {
                var builder = new ProducerBuilder();
                builder.brandNames = brandNames;

                return builder.Build();
            }

            public static ProducerSubmission WithSICCodes(List<SICCode> sicCodes)
            {
                var builder = new ProducerBuilder();
                builder.sicCodes = sicCodes;

                return builder.Build();
            }
        }
    }
}