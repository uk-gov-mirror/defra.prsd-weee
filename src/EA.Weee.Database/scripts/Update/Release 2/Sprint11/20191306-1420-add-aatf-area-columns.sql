﻿ALTER TABLE [AATF].[AATF] ADD LocalAreaId UNIQUEIDENTIFIER NULL;
ALTER TABLE [AATF].[AATF] ADD PanAreaId UNIQUEIDENTIFIER NULL;

ALTER TABLE [AATF].[AATF] ADD CONSTRAINT FK_AATF_LocalAreaId FOREIGN KEY (LocalAreaId) REFERENCES [Lookup].LocalArea(Id);
ALTER TABLE [AATF].[AATF] ADD CONSTRAINT FK_AATF_PanAreaId FOREIGN KEY (PanAreaId) REFERENCES [Lookup].PanArea(Id);