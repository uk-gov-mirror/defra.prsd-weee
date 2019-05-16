﻿namespace EA.Weee.Domain.AatfReturn
{
    using System;
    using EA.Prsd.Core.Domain;

    public class ReturnReportOn : Entity
    {
        public ReturnReportOn()
        {
        }

        public ReturnReportOn(Guid returnId, int reportOnQuestId)
        {
            this.ReturnId = returnId;
            this.ReportOnQuestionId = reportOnQuestId;
        }

        public Guid ReturnId { get; private set; }

        public int ReportOnQuestionId { get; private set; }

        public virtual Return Return { get; private set; }

        public virtual void UpdateReturn(Return @return)
        {
            Return = @return;
        }
    }
}
