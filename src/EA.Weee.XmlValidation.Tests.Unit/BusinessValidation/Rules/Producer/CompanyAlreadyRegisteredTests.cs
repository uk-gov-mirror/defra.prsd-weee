﻿namespace EA.Weee.XmlValidation.Tests.Unit.BusinessValidation.Rules.Producer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using EA.Weee.Domain;
    using EA.Weee.Domain.Producer;
    using EA.Weee.Xml.Schemas;
    using EA.Weee.XmlValidation.BusinessValidation.QuerySets;
    using EA.Weee.XmlValidation.BusinessValidation.Rules.Producer;
    using EA.Weee.XmlValidation.Tests.Unit.BusinessValidation.Domain;
    using FakeItEasy;
    using Xunit;

    public class CompanyAlreadyRegisteredTests
    {
        private readonly IProducerQuerySet producerQuerySet;

        public CompanyAlreadyRegisteredTests()
        {
            producerQuerySet = A.Fake<IProducerQuerySet>();
        }

        [Fact]
        public void Evaluate_Amendment_ReturnsPass()
        {
            var result = new CompanyAlreadyRegistered(producerQuerySet).Evaluate(new producerType() { status = statusType.A });
            
            A.CallTo(() => producerQuerySet.GetLatestCompanyProducers()).MustNotHaveHappened();

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Evaluate_InsertNotCompanyProducer_ReturnsPass()
        {
            var result = new CompanyAlreadyRegistered(producerQuerySet).Evaluate(new producerType() { status = statusType.I });

            A.CallTo(() => producerQuerySet.GetLatestCompanyProducers()).MustNotHaveHappened();

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Evaluate_Insert_MatchingCompanyRegistrationNumber_ReturnsError()
        {
            string companyNumber = "1234";

            var newProducer = new producerType()
            {
                status = statusType.I,
                producerBusiness = new producerBusinessType()
                {
                    Item = new companyType() { companyNumber = companyNumber }
                }
            };

            var existingProducer = FakeProducer.Create(ObligationType.Both, "prn", true,
                producerBusiness: new ProducerBusiness(new Company("Company", companyNumber, null)));

            A.CallTo(() => producerQuerySet.GetLatestCompanyProducers()).Returns(new List<Producer>() { existingProducer });

            var result = new CompanyAlreadyRegistered(producerQuerySet).Evaluate(newProducer);

            Assert.False(result.IsValid);
            Assert.Equal(Core.Shared.ErrorLevel.Error, result.ErrorLevel);
        }

        [Fact]
        public void Evaluate_ErrorMessage_ContainsProducerNameAndCompanyRegistrationNumber()
        {
            string companyName = "Test company name";
            string companyNumber = "1234";

            var newProducer = new producerType()
            {
                status = statusType.I,
                producerBusiness = new producerBusinessType()
                {
                    Item = new companyType() { companyName = companyName, companyNumber = companyNumber }
                }
            };

            var existingProducer = FakeProducer.Create(ObligationType.Both, "prn", true,
                producerBusiness: new ProducerBusiness(new Company(companyName, companyNumber, null)));

            A.CallTo(() => producerQuerySet.GetLatestCompanyProducers()).Returns(new List<Producer>() { existingProducer });

            var result = new CompanyAlreadyRegistered(producerQuerySet).Evaluate(newProducer);

            Assert.False(result.IsValid);
            Assert.Equal(Core.Shared.ErrorLevel.Error, result.ErrorLevel);
            Assert.Contains(companyName, result.Message);
            Assert.Contains(companyNumber, result.Message);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData("000")]
        public void Evaluate_Insert_EmptyOrNullCompanyNumberForNewCompany_DoesNotCompareCompanyRegistrationNumbers_ReturnsPass(string newCompanyNumber)
        {
            var newProducer = new producerType()
            {
                status = statusType.I,
                producerBusiness = new producerBusinessType()
                {
                    Item = new companyType() { companyNumber = newCompanyNumber }
                }
            };

            var result = new CompanyAlreadyRegistered(producerQuerySet).Evaluate(newProducer);

            A.CallTo(() => producerQuerySet.GetLatestCompanyProducers()).MustNotHaveHappened();

            Assert.True(result.IsValid);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData("000")]
        public void Evaluate_Insert_EmptyOrNullCompanyNumberForExistingCompany_ReturnsPass(string existingCompanyNumber)
        {
            var newProducer = new producerType()
            {
                status = statusType.I,
                producerBusiness = new producerBusinessType()
                {
                    Item = new companyType() { companyNumber = "1234" }
                }
            };

            var existingProducer = FakeProducer.Create(ObligationType.Both, "prn", true,
                producerBusiness: new ProducerBusiness(new Company("companyName", existingCompanyNumber, null)));

            A.CallTo(() => producerQuerySet.GetLatestCompanyProducers()).Returns(new List<Producer>() { existingProducer });

            var result = new CompanyAlreadyRegistered(producerQuerySet).Evaluate(newProducer);
            
            Assert.True(result.IsValid);
        }

        [Theory]
        [InlineData("1234", "0123")]
        [InlineData(" 123", "1234 ")]
        [InlineData("00123400", "12300")]
        [InlineData("123400", "0012300")]
        [InlineData("  0 0 1 2 3  ", "1234")]
        [InlineData("  0012 300  ", "123400")]
        [InlineData("  00123 00  ", "123400")]
        [InlineData("  1234567", " 1 2 3 4 5 6 ")]
        public void Evaluate_Insert_NonMatchingCompanyNumbersAfterFormatting_ReturnsPass(string newCompanyNumber, string existingCompanyNumber)
        {
            var newProducer = new producerType()
            {
                status = statusType.I,
                producerBusiness = new producerBusinessType()
                {
                    Item = new companyType() { companyNumber = newCompanyNumber }
                }
            };

            var existingProducer = FakeProducer.Create(ObligationType.Both, "prn", true,
                producerBusiness: new ProducerBusiness(new Company("Company", existingCompanyNumber, null)));

            A.CallTo(() => producerQuerySet.GetLatestCompanyProducers()).Returns(new List<Producer>() { existingProducer });

            var result = new CompanyAlreadyRegistered(producerQuerySet).Evaluate(newProducer);

            Assert.True(result.IsValid);
        }

        [Theory]
        [InlineData("123", "0123")]
        [InlineData(" 123", "123 ")]
        [InlineData("0012300", "12300")]
        [InlineData("12300", "0012300")]
        [InlineData("  0 0 1 2 3  ", "123")]
        [InlineData("  0012 300  ", "12300")]
        [InlineData("  00123 00  ", "12300")]
        [InlineData("  123456", " 1 2 3 4 5 6 ")]
        public void Evaluate_Insert_MatchingCompanyNumbersAfterFormatting_ReturnsError(string newCompanyNumber, string existingCompanyNumber)
        {
            var newProducer = new producerType()
            {
                status = statusType.I,
                producerBusiness = new producerBusinessType()
                {
                    Item = new companyType() { companyNumber = newCompanyNumber }
                }
            };

            var existingProducer = FakeProducer.Create(ObligationType.Both, "prn", true,
                producerBusiness: new ProducerBusiness(new Company("Company", existingCompanyNumber, null)));

            A.CallTo(() => producerQuerySet.GetLatestCompanyProducers()).Returns(new List<Producer>() { existingProducer });

            var result = new CompanyAlreadyRegistered(producerQuerySet).Evaluate(newProducer);

            Assert.False(result.IsValid);
            Assert.Equal(Core.Shared.ErrorLevel.Error, result.ErrorLevel);
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData("", "")]
        [InlineData("  ", "")]
        [InlineData("000", "")]
        [InlineData("  000  ", "")]
        [InlineData("000  ", "")]
        [InlineData("  000  ", "")]
        [InlineData("  123", "123")]
        [InlineData("123  ", "123")]
        [InlineData("  123  ", "123")]
        [InlineData("  00123  ", "123")]
        [InlineData("  0012300  ", "12300")]
        [InlineData("  0 0 1 2 3  ", "123")]
        [InlineData("  0012 300  ", "12300")]
        [InlineData("  00123 00  ", "12300")]
        [InlineData("12300", "12300")]
        [InlineData("123001", "123001")]
        [InlineData("123", "123")]
        public void FormatCompanyRegistrationNumber_ReturnsFormattedString(string originalValue, string expectedValue)
        {
            var result = CompanyAlreadyRegistered.FormatCompanyRegistrationNumber(originalValue);

            Assert.Equal(expectedValue, result);
        }
    }
}