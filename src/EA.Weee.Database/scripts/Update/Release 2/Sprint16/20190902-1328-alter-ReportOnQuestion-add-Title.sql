﻿ALTER TABLE [AATF].ReportOnQuestion
	ADD Title NVARCHAR(1000) NULL
GO
UPDATE [AATF].ReportOnQuestion SET Title = [Description]
GO

ALTER TABLE [AATF].ReportOnQuestion
	ALTER COLUMN Title NVARCHAR(1000) NOT NULL
GO