﻿namespace EA.Weee.Domain.DataReturns
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class DataReturnsUploadRawData
    {
        [Key]
        public Guid DataReturnsUploadId { get; set; }

        public string Data { get; set; }
    }
}
