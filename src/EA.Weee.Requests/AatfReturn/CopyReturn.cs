﻿namespace EA.Weee.Requests.AatfReturn
{
    using System;
    using Core.AatfReturn;
    using Prsd.Core.Mediator;

    public class CopyReturn : IRequest<Guid>
    {
        public Guid ReturnId { get; private set; }

        public CopyReturn(Guid returnId)
        {
            this.ReturnId = returnId;
        }
    }
}