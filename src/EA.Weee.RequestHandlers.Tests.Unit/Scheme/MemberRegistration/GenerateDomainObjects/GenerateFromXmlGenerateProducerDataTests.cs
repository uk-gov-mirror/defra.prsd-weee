﻿namespace EA.Weee.RequestHandlers.Tests.Unit.Scheme.MemberRegistration.GenerateDomainObjects
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using EA.Weee.Domain.Lookup;
    using EA.Weee.Domain.Producer;
    using EA.Weee.Domain.Scheme;
    using EA.Weee.RequestHandlers.Scheme.Interfaces;
    using EA.Weee.RequestHandlers.Scheme.MemberRegistration;
    using EA.Weee.RequestHandlers.Scheme.MemberRegistration.GenerateDomainObjects.DataAccess;
    using EA.Weee.RequestHandlers.Scheme.MemberRegistration.GenerateProducerObjects;
    using EA.Weee.Xml.Schemas;
    using FakeItEasy;
    using Xunit;

    public class GenerateFromXmlGenerateProducerDataTests
    {
        [Fact]
        public async void GenerateProducerData_WithSchemeId_ReturnsProducerWithMatchingSchemeId()
        {
            var schemeId = Guid.NewGuid();

            var builder = new GenerateProducerDataTestsBuilder();
            builder.SchemeId = schemeId;

            var result = await builder.InvokeGenerateProducerDataWithSingleResult();

            Assert.Equal(schemeId, result.SchemeId);
        }

        [Fact]
        public async void GenerateProducerData_WithMemberUpload_ReturnsProducerWithMatchingMemberUpload()
        {
            var builder = new GenerateProducerDataTestsBuilder();

            var data = "Test data";
            var fileName = "Test filename";
            var memberUpload = new MemberUpload(Guid.NewGuid(), builder.SchemeId, data, fileName);

            builder.MemberUpload = memberUpload;

            var result = await builder.InvokeGenerateProducerDataWithSingleResult();

            Assert.Equal(data, result.MemberUpload.RawData.Data);
            Assert.Equal(fileName, result.MemberUpload.FileName);
            Assert.True(ReferenceEquals(memberUpload, result.MemberUpload));
        }

        [Fact]
        public async void GenerateProducerData_WithNullProducerCharges_ThrowsApplicationException()
        {
            var builder = new GenerateProducerDataTestsBuilder();
            builder.ProducerCharges = null;

            await Assert.ThrowsAsync<ApplicationException>(() => builder.InvokeGenerateProducerDataWithSingleResult());
        }

        [Fact]
        public async void GenerateProducerData_WithNoProducerChargesForProducer_ThrowsApplicationException()
        {
            var builder = new GenerateProducerDataTestsBuilder();
            builder.ProducerCharges = new Dictionary<string, ProducerCharge>();

            await Assert.ThrowsAsync<ApplicationException>(() => builder.InvokeGenerateProducerDataWithSingleResult());
        }

        [Fact]
        public async void GenerateProducerData_WithProducerCharges_ReturnsProducerWithMatchingProducerCharges()
        {
            var builder = new GenerateProducerDataTestsBuilder();

            var chargeBoundAmound = new ChargeBandAmount(Guid.NewGuid(), ChargeBand.A, 20);
            builder.ProducerCharges[builder.TradingName] = new ProducerCharge()
            {
                Amount = 100,
                ChargeBandAmount = chargeBoundAmound
            };

            var result = await builder.InvokeGenerateProducerDataWithSingleResult();

            Assert.Equal(100, result.ChargeThisUpdate);
            Assert.Equal(20, result.ChargeBandAmount.Amount);
            Assert.Equal(chargeBoundAmound.Id, result.ChargeBandAmount.Id);
        }

        [Fact]
        public async void GenerateProducerData_WithBrandNames_ReturnsProducerWithMatchingBrandNames()
        {
            var builder = new GenerateProducerDataTestsBuilder();
            builder.BrandNames = new[] { "BrandName1", "BrandName2" };

            var result = await builder.InvokeGenerateProducerDataWithSingleResult();

            Assert.Collection(result.BrandNames,
                r1 => Assert.Equal("BrandName1", r1.Name),
                r2 => Assert.Equal("BrandName2", r2.Name));
        }

        [Fact]
        public async void GenerateProducerData_WithSICCodes_ReturnsProducerWithMatchingSICCodes()
        {
            var builder = new GenerateProducerDataTestsBuilder();
            builder.SicCodes = new[] { "SicCode1", "SicCode2" };

            var result = await builder.InvokeGenerateProducerDataWithSingleResult();

            Assert.Collection(result.SICCodes,
                r1 => Assert.Equal("SicCode1", r1.Name),
                r2 => Assert.Equal("SicCode2", r2.Name));
        }

        [Fact]
        public async void GenerateProducerData_WithProducerBusiness_ReturnsProducerWithMatchingProducerBusiness()
        {
            var builder = new GenerateProducerDataTestsBuilder();
            builder.ProducerBusiness = new producerBusinessType()
            {
                correspondentForNotices = new optionalContactDetailsContainerType(),
                Item = new companyType()
                {
                    companyName = "Test company",
                    companyNumber = "Test CRN",
                    registeredOffice = new contactDetailsContainerType()
                    {
                        contactDetails = new contactDetailsType()
                        {
                            address = new addressType()
                            {
                                country = countryType.UKENGLAND
                            }
                        }
                    }
                }
            };
            builder.ProducerCharges.Add("Test company", new ProducerCharge() { ChargeBandAmount = new ChargeBandAmount() });

            var result = await builder.InvokeGenerateProducerDataWithSingleResult();

            Assert.Equal("Test company", result.ProducerBusiness.CompanyDetails.Name);
            Assert.Equal("Test CRN", result.ProducerBusiness.CompanyDetails.CompanyNumber);
        }

        [Fact]
        public async void GenerateProducerData_WithAuthorisedRepresentative_ReturnsProducerWithMatchingAuthorisedRepresentative()
        {
            var builder = new GenerateProducerDataTestsBuilder();
            builder.AuthorisedRepresentative = new authorisedRepresentativeType()
            {
                overseasProducer = new overseasProducerType()
                {
                    overseasProducerName = "Test overseas producer"
                }
            };

            var result = await builder.InvokeGenerateProducerDataWithSingleResult();

            Assert.Equal("Test overseas producer", result.AuthorisedRepresentative.OverseasProducerName);
        }

        [Fact]
        public async void GenerateProducerData_WithEEEPlacedOnMarketBand_ReturnsProducerWithMatchingEEEPlacedOnMarketBand()
        {
            var builder = new GenerateProducerDataTestsBuilder();
            builder.EEEPlacedOnMarketBandType = eeePlacedOnMarketBandType.Lessthan5TEEEplacedonmarket;

            var result = await builder.InvokeGenerateProducerDataWithSingleResult();

            Assert.Equal(1, result.EEEPlacedOnMarketBandType);
        }

        [Fact]
        public async void GenerateProducerData_WithSellingTechnique_ReturnsProducerWithMatchingSellingTechnique()
        {
            var builder = new GenerateProducerDataTestsBuilder();
            builder.SellingTechnique = sellingTechniqueType.Both;

            var result = await builder.InvokeGenerateProducerDataWithSingleResult();

            Assert.Equal(2, result.SellingTechniqueType);
        }

        [Fact]
        public async void GenerateProducerData_WithObligationType_ReturnsProducerWithMatchingObligationType()
        {
            var builder = new GenerateProducerDataTestsBuilder();
            builder.ObligationType = obligationTypeType.B2B;

            var result = await builder.InvokeGenerateProducerDataWithSingleResult();

            Assert.Equal(1, result.ObligationType); // The domain obligation type has a value of 1 for B2B
        }

        [Fact]
        public async void GenerateProducerData_WithAnnualTurnOverBandType_ReturnsProducerWithMatchingAnnualTurnOverBandType()
        {
            var builder = new GenerateProducerDataTestsBuilder();
            builder.AnnualTurnoverBandType = annualTurnoverBandType.Greaterthanonemillionpounds;

            var result = await builder.InvokeGenerateProducerDataWithSingleResult();

            Assert.Equal(1, result.AnnualTurnOverBandType);
        }

        [Fact]
        public async void GenerateProducerData_WithCeaseDate_ReturnsProducerWithMatchingCeaseDate()
        {
            var builder = new GenerateProducerDataTestsBuilder();
            builder.CeaseDate = new DateTime(2015, 10, 1);

            var result = await builder.InvokeGenerateProducerDataWithSingleResult();

            Assert.Equal(new DateTime(2015, 10, 1), result.CeaseToExist);
        }

        [Fact]
        public async void GenerateProducerData_WithRegistrationNumberForAmendment_ReturnsProducerWithMatchingRegistrationNumber()
        {
            var builder = new GenerateProducerDataTestsBuilder();
            builder.Status = statusType.A;
            builder.RegistrationNumber = "Test Registration Number";

            var result = await builder.InvokeGenerateProducerDataWithSingleResult();

            Assert.Equal("Test Registration Number", result.RegistrationNumber);
        }

        [Fact]
        public async void GenerateProducerData_WithGeneratedRegistrationNumberForInsertion_ReturnsProducerWithMatchingRegistrationNumber()
        {
            var builder = new GenerateProducerDataTestsBuilder();
            builder.Status = statusType.I;
            builder.GeneratedPrns.Clear();
            builder.GeneratedPrns.Enqueue("Test Generated Registration Number");

            var result = await builder.InvokeGenerateProducerDataWithSingleResult();

            Assert.Equal("Test Generated Registration Number", result.RegistrationNumber);
        }

        [Fact]
        public async void GenerateProducerData_WithAnnualTurnover_ReturnsProducerWithMatchingAnnualTurnover()
        {
            var builder = new GenerateProducerDataTestsBuilder();
            builder.AnnualTurnover = 11100;

            var result = await builder.InvokeGenerateProducerDataWithSingleResult();

            Assert.Equal(11100, result.AnnualTurnover);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async void GenerateProducerData_WithVatRegistered_ReturnsProducerWithMatchingVatRegistered(bool vatRegistered)
        {
            var builder = new GenerateProducerDataTestsBuilder();
            builder.VatRegistered = vatRegistered;

            var result = await builder.InvokeGenerateProducerDataWithSingleResult();

            Assert.Equal(vatRegistered, result.VATRegistered);
        }

        [Fact]
        public async void GenerateProducerData_WithTradingName_ReturnsProducerWithMatchingTradingName()
        {
            var builder = new GenerateProducerDataTestsBuilder();
            builder.TradingName = "Test producer trading name";
            builder.ProducerCharges.Add("Test producer trading name", new ProducerCharge() { ChargeBandAmount = new ChargeBandAmount() });

            var result = await builder.InvokeGenerateProducerDataWithSingleResult();

            Assert.Equal("Test producer trading name", result.TradingName);
        }

        [Fact]
        public async void GenerateProducerData_WithManyProducers_AllInserts_ReturnsSameNumberOfProducers()
        {
            var builder = new GenerateProducerDataTestsBuilder();
            builder.Status = statusType.I;

            var result = await builder.InvokeGenerateProducerData(10);

            A.CallTo(() => builder.DataAccess.GetLatestProducerRecord(A<Guid>._, A<string>._, A<bool>._)).MustNotHaveHappened();

            Assert.Equal(10, result.Count());
        }

        [Fact]
        public async void GenerateProducerData_AmendProducer_WithMatchingExistingProducer_AndDetailsNotEqual_ReturnsNewProducer()
        {
            var builder = new GenerateProducerDataTestsBuilder();
            builder.Status = statusType.A;
            builder.RegistrationNumber = "Test Registration Number";

            A.CallTo(() => builder.DataAccess.GetLatestProducerRecord(A<Guid>._, A<string>._, false)).Returns(new AlwaysUnequalProducer());

            var result = await builder.InvokeGenerateProducerData(1);

            A.CallTo(() => builder.DataAccess.GetMigratedProducer(A<string>._)).MustNotHaveHappened();

            Assert.Equal(1, result.Count());
            Assert.Equal("Test Registration Number", result.Single().RegistrationNumber);
        }

        [Fact]
        public async void GenerateProducerData_AmendProducer_WithMatchingExistingProducer_AndDetailsEqual_DoesNotReturnNewProducer()
        {
            var builder = new GenerateProducerDataTestsBuilder();
            builder.Status = statusType.A;

            A.CallTo(() => builder.DataAccess.GetLatestProducerRecord(A<Guid>._, A<string>._, false)).Returns(new AlwaysEqualProducer());

            var result = await builder.InvokeGenerateProducerData(1);

            A.CallTo(() => builder.DataAccess.GetMigratedProducer(A<string>._)).MustNotHaveHappened();

            Assert.Equal(0, result.Count());
        }

        [Fact]
        public async void GenerateProducerData_AmendProducer_WithMatchingMigratedProducer_ReturnsNewProducer()
        {
            var builder = new GenerateProducerDataTestsBuilder();
            builder.Status = statusType.A;
            builder.RegistrationNumber = "Test Registration Number";

            A.CallTo(() => builder.DataAccess.GetLatestProducerRecord(A<Guid>._, A<string>._, false)).Returns((Producer)null);

            var result = await builder.InvokeGenerateProducerData(1);

            A.CallTo(() => builder.DataAccess.GetLatestProducerRecord(A<Guid>._, A<string>._, true)).MustNotHaveHappened();

            Assert.Equal(1, result.Count());
            Assert.Equal("Test Registration Number", result.Single().RegistrationNumber);
        }

        [Fact]
        public async void GenerateProducerData_AmendProducer_WithMatchingExistingProducerFromAnotherScheme_ReturnsNewProducer()
        {
            var builder = new GenerateProducerDataTestsBuilder();
            builder.Status = statusType.A;
            builder.RegistrationNumber = "Test Registration Number";

            A.CallTo(() => builder.DataAccess.GetLatestProducerRecord(A<Guid>._, A<string>._, false)).Returns((Producer)null);
            A.CallTo(() => builder.DataAccess.GetMigratedProducer(A<string>._)).Returns((MigratedProducer)null);
            A.CallTo(() => builder.DataAccess.GetLatestProducerRecord(A<Guid>._, A<string>._, true)).Returns(A.Dummy<Producer>());

            var result = await builder.InvokeGenerateProducerData(1);

            A.CallTo(() => builder.DataAccess.GetLatestProducerRecord(A<Guid>._, A<string>._, true)).MustHaveHappened();

            Assert.Equal(1, result.Count());
            Assert.Equal("Test Registration Number", result.Single().RegistrationNumber);
        }

        [Fact]
        public async void GenerateProducerData_AmendProducer_WithNoMatchingExistingProducerFromAnotherScheme_ThrowsInvalidOperationException()
        {
            var builder = new GenerateProducerDataTestsBuilder();
            builder.Status = statusType.A;

            A.CallTo(() => builder.DataAccess.GetLatestProducerRecord(A<Guid>._, A<string>._, false)).Returns((Producer)null);
            A.CallTo(() => builder.DataAccess.GetMigratedProducer(A<string>._)).Returns((MigratedProducer)null);
            A.CallTo(() => builder.DataAccess.GetLatestProducerRecord(A<Guid>._, A<string>._, true)).Returns((Producer)null);

            await Assert.ThrowsAsync<InvalidOperationException>(async () => await builder.InvokeGenerateProducerData(1));

            A.CallTo(() => builder.DataAccess.GetLatestProducerRecord(A<Guid>._, A<string>._, true)).MustHaveHappened();
        }

        private class GenerateProducerDataTestsBuilder
        {
            public IXmlConverter XmlConverter;
            public IGenerateFromXmlDataAccess DataAccess;

            public string TradingName;
            public Guid SchemeId;
            public MemberUpload MemberUpload;
            public Dictionary<string, ProducerCharge> ProducerCharges;
            public string[] BrandNames;
            public string[] SicCodes;
            public producerBusinessType ProducerBusiness;
            public authorisedRepresentativeType AuthorisedRepresentative;
            public eeePlacedOnMarketBandType EEEPlacedOnMarketBandType;
            public sellingTechniqueType SellingTechnique;
            public obligationTypeType ObligationType;
            public annualTurnoverBandType AnnualTurnoverBandType;
            public DateTime? CeaseDate;
            public string RegistrationNumber;
            public statusType Status;
            public decimal? AnnualTurnover;
            public bool VatRegistered;
            public Queue<string> GeneratedPrns;

            public GenerateProducerDataTestsBuilder()
            {
                XmlConverter = A.Fake<IXmlConverter>();
                DataAccess = A.Fake<IGenerateFromXmlDataAccess>();

                InstantiateProducerParameters();
            }

            private void InstantiateProducerParameters()
            {
                TradingName = "Test trading name";
                SchemeId = Guid.NewGuid();
                MemberUpload = A.Fake<MemberUpload>();

                ProducerCharges = new Dictionary<string, ProducerCharge>();
                ProducerCharges.Add(TradingName, new ProducerCharge());

                GeneratedPrns = new Queue<string>();
                A.CallTo(() => DataAccess.ComputePrns(A<int>._)).Returns(GeneratedPrns);

                BrandNames = Enumerable.Empty<string>().ToArray();
                SicCodes = Enumerable.Empty<string>().ToArray();

                ProducerBusiness = new producerBusinessType()
                {
                    correspondentForNotices = new optionalContactDetailsContainerType() { },
                    Item = new companyType()
                    {
                        registeredOffice = new contactDetailsContainerType()
                        {
                            contactDetails = new contactDetailsType()
                            {
                                address = new addressType()
                                {
                                    country = countryType.UKENGLAND
                                }
                            }
                        }
                    }
                };

                AuthorisedRepresentative = null;

                EEEPlacedOnMarketBandType = eeePlacedOnMarketBandType.Lessthan5TEEEplacedonmarket;
                SellingTechnique = sellingTechniqueType.Both;
                ObligationType = obligationTypeType.Both;
                AnnualTurnoverBandType = annualTurnoverBandType.Greaterthanonemillionpounds;
                CeaseDate = null;
                RegistrationNumber = "TestRegistrationNumber";
                AnnualTurnover = 10;
                VatRegistered = false;
                Status = statusType.I;
            }

            private producerType CreateProducer()
            {
                var producer = new producerType();
                producer.tradingName = TradingName;
                producer.producerBrandNames = BrandNames;
                producer.SICCodeList = SicCodes;
                producer.producerBusiness = ProducerBusiness;
                producer.authorisedRepresentative = AuthorisedRepresentative;
                producer.eeePlacedOnMarketBand = EEEPlacedOnMarketBandType;
                producer.sellingTechnique = SellingTechnique;
                producer.obligationType = ObligationType;
                producer.annualTurnoverBand = AnnualTurnoverBandType;
                producer.ceaseToExistDateSpecified = CeaseDate.HasValue;
                if (CeaseDate.HasValue)
                {
                    producer.ceaseToExistDate = CeaseDate.Value;
                }
                producer.registrationNo = RegistrationNumber;
                producer.annualTurnover = AnnualTurnover;
                producer.VATRegistered = VatRegistered;
                producer.status = Status;

                return producer;
            }

            public async Task<IEnumerable<Producer>> InvokeGenerateProducerData(int numberOfProducers)
            {
                var producers = new List<producerType>();
                for (int i = 0; i < numberOfProducers; i++)
                {
                    GeneratedPrns.Enqueue("TestPrn" + i.ToString());
                    producers.Add(CreateProducer());
                }

                var scheme = new schemeType();
                scheme.producerList = producers.ToArray();

                var generateFromXml = new GenerateFromXml(XmlConverter, DataAccess);

                return await generateFromXml.GenerateProducerData(scheme, SchemeId, MemberUpload, ProducerCharges);
            }

            public async Task<Producer> InvokeGenerateProducerDataWithSingleResult()
            {
                var result = await InvokeGenerateProducerData(1);

                return result.Single();
            }
        }

        private class AlwaysEqualProducer : Producer
        {
            public override bool Equals(Producer other)
            {
                return true;
            }
        }

        private class AlwaysUnequalProducer : Producer
        {
            public override bool Equals(Producer other)
            {
                return false;
            }
        }
    }
}