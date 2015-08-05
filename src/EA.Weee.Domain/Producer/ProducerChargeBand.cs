﻿namespace EA.Weee.Domain.Producer
{
    using Prsd.Core.Domain;

    public class ProducerChargeBand : Entity
    {
        public string Name { get; private set; }

        public decimal Amount { get; private set; }

        protected ProducerChargeBand()
        {
        }

        public ProducerChargeBand(string name, decimal amount)
        {
            Name = name;
            Amount = amount;
        }
    }
}