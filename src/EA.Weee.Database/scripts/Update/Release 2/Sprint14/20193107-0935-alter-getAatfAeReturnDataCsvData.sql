
/****** Object:  StoredProcedure [AATF].[getAatfAeReturnDataCsvData]    Script Date: 31/07/2019 09:34:27 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- Description:	This stored procedure is used to provide the data for the admin report of aatf/ae
--				that have/haven't submitted a data return within
--				the limits of the specified parameters. The first submitted return should be returned.

-- =============================================
ALTER PROCEDURE [AATF].[getAatfAeReturnDataCsvData]
	@ComplianceYear INT,
	@Quarter INT,
	@FacilityType INT,
	@ReturnStatus INT,
	@CA UNIQUEIDENTIFIER,
	@Area UNIQUEIDENTIFIER,
	@PanArea UNIQUEIDENTIFIER,
	@IncludeReSubmissions BIT 
AS
BEGIN

SET NOCOUNT ON;

DECLARE @AATF TABLE
(
	AatfId					UNIQUEIDENTIFIER NOT NULL,
	Name					NVARCHAR(256) NOT NULL,
	ApprovalNumber			NVARCHAR(20) NOT NULL,	
	OrganisationName		NVARCHAR(256) NULL,
	CompetentAuthorityAbbr	NVARCHAR(65) NOT NULL,
	CompetentAuthorityId	UNIQUEIDENTIFIER NOT NULL,
	OrgId					UNIQUEIDENTIFIER NOT NULL
)

DECLARE @RETURN TABLE
(
	AatfId					UNIQUEIDENTIFIER NOT NULL,
	ReturnId				UNIQUEIDENTIFIER NOT NULL,
	ReturnStatus			INT NOT NULL,
	CreatedDate				DATETIME NOT NULL,
	SubmittedDate			DATETIME NULL,
	SubmittedById			UNIQUEIDENTIFIER NULL,
	ReSubmission			BIT NOT NULL
)

DECLARE @AATFReturn TABLE
(
	AatfId					UNIQUEIDENTIFIER NOT NULL,
	Name					NVARCHAR(256) NOT NULL,
	ApprovalNumber			NVARCHAR(20) NOT NULL,	
	OrgId					UNIQUEIDENTIFIER NULL,
	OrganisationName		NVARCHAR(256) NULL,
	CompetentAuthorityAbbr	NVARCHAR(65) NOT NULL,
	ReturnId				UNIQUEIDENTIFIER NULL,
	ReturnStatus			INT  NULL,
	CreatedDate				DATETIME  NULL,
	SubmittedDate			DATETIME NULL,
	SubmittedBy				NVARCHAR(70) NULL,
	RowNumber				INT NOT NULL,
	ReSubmission			BIT NOT NULL
)

--SET THE START DATE

DECLARE @QuarterStartDate date
DECLARE @StartYear int
DECLARE @EndYear int

SELECT @StartYear = @ComplianceYear + AddStartYears, @EndYear = @ComplianceYear + AddEndYears FROM [Lookup].[QuarterWindowTemplate] WHERE [Quarter] = @Quarter

SELECT @QuarterStartDate = DATEFROMPARTS(@StartYear,StartMonth,StartDay)
	FROM [Lookup].[QuarterWindowTemplate] WHERE [Quarter] = @Quarter

INSERT INTO @AATF
	SELECT 
		a.id, 
		a.[Name], 
		a.ApprovalNumber,
		CASE WHEN o.Name IS NULL THEN o.TradingName ELSE o.Name END,
		ca.Abbreviation,
		ca.Id,
		o.Id
	FROM
		AATF.AATF a 
		JOIN Organisation.Organisation o ON a.OrganisationId  = o.Id
		JOIN Lookup.CompetentAuthority ca ON a.CompetentAuthorityId = ca.Id
	WHERE 
		A.ComplianceYear = @ComplianceYear 
		AND A.FacilityType = @FacilityType
		AND a.ApprovalDate < @QuarterStartDate
		AND a.CompetentAuthorityId = COALESCE(@CA, a.CompetentAuthorityId)
		AND (@Area IS NULL OR a.LocalAreaId = COALESCE(@Area, a.LocalAreaId))
		AND (@PanArea IS NULL OR a.PanAreaId = COALESCE(@PanArea, a.PanAreaId))


 --Get the returns for the AATF/AE
--INSERT INTO @RETURN
--SELECT
--	X.AatfId,
--	X.Id, 
--	X.[ReturnStatus],
--	X.[CreatedDate],
--	X.[SubmittedDate],
--	X.[SubmittedById],
--	CASE x.RowNumber WHEN 1 THEN 0 ELSE 1 END
--FROM
--	(SELECT 
--		a.AatfId,
-- 		r.Id, 
--		r.[ReturnStatus],
--		r.[CreatedDate],
--		r.[SubmittedDate],
--		r.[SubmittedById],
--		ROW_NUMBER() OVER (PARTITION BY a.AatfId ORDER BY r.CreatedDate ASC) AS RowNumber
--	FROM 
-- 		[AATF].WeeeSentOn wso 
--		INNER JOIN @AATF A ON wso.AatfId = A.AatfId 
--		INNER JOIN [AATF].[Return] r ON r.Id = wso.ReturnId 
--	WHERE
--		r.ComplianceYear = @ComplianceYear
--		AND R.[Quarter] = @Quarter
--		AND r.FacilityType = @FacilityType
--	)X
--WHERE (X.RowNumber = 1 AND @IncludeReSubmissions = 0) OR @IncludeReSubmissions = 1
	
--INSERT INTO @RETURN
--SELECT
--	X.AatfId,
--	X.Id, 
--	X.[ReturnStatus],
--	X.[CreatedDate],
--	X.[SubmittedDate],
--	X.[SubmittedById],
--	CASE x.RowNumber WHEN 1 THEN 0 ELSE 1 END
--FROM
--	(SELECT 
--		a.AatfId,
-- 		r.Id, 
--		r.[ReturnStatus],
--		r.[CreatedDate],
--		r.[SubmittedDate],
--		r.[SubmittedById],
--		ROW_NUMBER() OVER (PARTITION BY a.AatfId ORDER BY r.CreatedDate ASC) AS RowNumber
--	FROM 
-- 		[AATF].WeeeReceived wr 
--		INNER JOIN @AATF A ON wr.AatfId = A.AatfId 
--		INNER JOIN [AATF].[Return] r ON r.Id = wr.ReturnId 
--	WHERE
--		r.ComplianceYear = @ComplianceYear
--		AND R.[Quarter] = @Quarter
--		AND r.FacilityType = @FacilityType
--	)X
--WHERE (X.RowNumber = 1 AND @IncludeReSubmissions = 0) OR @IncludeReSubmissions = 1

--INSERT INTO @RETURN
--SELECT
--	X.AatfId,
--	X.Id, 
--	X.[ReturnStatus],
--	X.[CreatedDate],
--	X.[SubmittedDate],
--	X.[SubmittedById],
--	CASE x.RowNumber WHEN 1 THEN 0 ELSE 1 END
--FROM
--	(SELECT 
--		a.AatfId,
-- 		r.Id, 
--		r.[ReturnStatus],
--		r.[CreatedDate],
--		r.[SubmittedDate],
--		r.[SubmittedById],
--		ROW_NUMBER() OVER (PARTITION BY a.AatfId ORDER BY r.CreatedDate ASC) AS RowNumber
--	FROM 
-- 		[AATF].WeeeReused wr 
--		INNER JOIN @AATF A ON wr.AatfId = A.AatfId 
--		INNER JOIN [AATF].[Return] r ON r.Id = wr.ReturnId 
--	WHERE
--		r.ComplianceYear = @ComplianceYear
--		AND R.[Quarter] = @Quarter
--		AND r.FacilityType = @FacilityType
--	)X
--WHERE (X.RowNumber = 1 AND @IncludeReSubmissions = 0) OR @IncludeReSubmissions = 1

--Due to aatf/ae approval date updates check if returns are missed 
INSERT INTO @AATF
SELECT 
		a.Id,
		a.[Name],
		a.ApprovalNumber,
		CASE WHEN o.Name IS NULL THEN o.TradingName ELSE o.[Name] END,
		ca.Abbreviation,
		ca.Id,
		o.Id
FROM 
	(
		SELECT
			ra.AatfId
		FROM
			[AATF].[Return] r
			JOIN [AATF].[ReturnAatf] ra ON ra.[ReturnId] = r.Id
		WHERE
			r.ComplianceYear = @ComplianceYear
			AND r.[Quarter] = @Quarter	
			AND r.FacilityType = @FacilityType
	) X	
	JOIN AATF.AATF a ON a.Id = X.AatfId
	JOIN Organisation.Organisation o ON a.OrganisationId  = o.Id
	JOIN Lookup.CompetentAuthority ca ON a.CompetentAuthorityId = ca.Id
WHERE 
	A.FacilityType = @FacilityType
	AND a.CompetentAuthorityId = COALESCE(@CA, a.CompetentAuthorityId)
	AND (@Area IS NULL OR a.LocalAreaId = COALESCE(@Area, a.LocalAreaId))
	AND (@PanArea IS NULL OR a.PanAreaId = COALESCE(@PanArea, a.PanAreaId))
	AND a.Id NOT IN (SELECT AatfId FROM @AATF)

INSERT INTO @RETURN	
SELECT
	X.AatfId,
	X.Id, 
	X.[ReturnStatus],
	X.[CreatedDate],
	X.[SubmittedDate],
	X.[SubmittedById],
	CASE x.RowNumber WHEN 1 THEN 0 ELSE 1 END
FROM
	(
	SELECT
		ra.AatfId,
		r.Id,
		r.[ReturnStatus],
		r.[CreatedDate],
		r.[SubmittedDate],
		r.[SubmittedById],
		ROW_NUMBER() OVER (PARTITION BY ra.AatfId ORDER BY r.CreatedDate ASC) AS RowNumber		
	FROM
		[AATF].[Return] r 
		JOIN [AATF].[ReturnAatf] ra ON ra.[ReturnId] = r.Id
	WHERE
		r.ComplianceYear = @ComplianceYear
		AND r.[Quarter] = @Quarter
		AND r.FacilityType = @FacilityType
		AND r.Id NOT IN (SELECT ReturnId FROM @RETURN) 
	) X
	JOIN AATF.AATF a ON a.Id = X.AatfId
WHERE
	(X.RowNumber = 1 AND @IncludeReSubmissions = 0) OR @IncludeReSubmissions = 1
	AND A.FacilityType = @FacilityType
	AND a.CompetentAuthorityId = COALESCE(@CA, a.CompetentAuthorityId)
	AND (@Area IS NULL OR a.LocalAreaId = COALESCE(@Area, a.LocalAreaId))
	AND (@PanArea IS NULL OR a.PanAreaId = COALESCE(@PanArea, a.PanAreaId))

IF @IncludeReSubmissions = 1 BEGIN
	INSERT INTO @RETURN
	SELECT
		a.AatfId,
 		r.Id, 
		r.[ReturnStatus], 
		r.[CreatedDate],
		r.[SubmittedDate],
		r.[SubmittedById],
		@IncludeReSubmissions
	FROM 
		[AATF].[Return] r
		INNER JOIN @AATF A ON a.OrgId = r.OrganisationId
	WHERE
		r.ComplianceYear = @ComplianceYear
		AND R.[Quarter] = @Quarter
		AND r.FacilityType = @FacilityType
		AND r.Id NOT IN (SELECT ReturnId from @RETURN)
END

--UPDATE aatf that have submitted returns that have null values 
INSERT INTO @RETURN
SELECT
	X.AatfId,
 	X.Id, 
	X.[ReturnStatus], 
	X.[CreatedDate],
	X.[SubmittedDate],
	X.[SubmittedById],
	CASE x.RowNumber WHEN 1 THEN 0 ELSE 1 END
FROM (
	SELECT 
			a.AatfId,
 			r.Id, 
			r.[ReturnStatus],
			r.[CreatedDate],
			r.[SubmittedDate],
			r.[SubmittedById], 
			ROW_NUMBER() OVER (PARTITION BY a.AatfId ORDER BY r.CreatedDate ASC) AS RowNumber
	FROM 
 		[AATF].ReturnAatf wr 
		INNER JOIN @AATF A ON wr.AatfId = A.AatfId AND A.AatfId NOT IN (SELECT DISTINCT AatfId from @RETURN)
		INNER JOIN [AATF].[Return] r ON r.Id = wr.ReturnId 
	WHERE
		r.ComplianceYear = @ComplianceYear
		AND R.[Quarter] = @Quarter
		AND r.FacilityType = @FacilityType
	) X
WHERE (X.RowNumber = 1 AND @IncludeReSubmissions = 0) OR @IncludeReSubmissions = 1

INSERT INTO @AATFReturn	
SELECT 
	*
FROM 
(
	SELECT DISTINCT 
		a.AatfId,			
		a.Name,
		a.ApprovalNumber,	
		a.OrgId,	
		a.OrganisationName,			
		a.CompetentAuthorityAbbr,
		R.ReturnId,
		ISNULL(r.ReturnStatus, 0) AS ReturnStatus,
		r.CreatedDate,
		r.SubmittedDate,
		CONCAT(u.FirstName,' ',u.Surname) as 'SubmittedBy',	
		ROW_NUMBER() OVER (PARTITION BY a.AatfId ORDER BY r.CreatedDate ASC) AS RowNumber,
		CASE r.ReSubmission WHEN 1 THEN 1 ELSE 0 END AS Resubmission
	FROM
		@AATF A 
		LEFT JOIN @RETURN r	ON r.AatfId = A.AatfId
		LEFT JOIN  [Identity].[AspNetUsers] u ON u.id = r.SubmittedById
)Y WHERE (Y.RowNumber = 1 AND @IncludeReSubmissions = 0) OR @IncludeReSubmissions = 1

--Check for Not Started records if a return at organization level is started or not
UPDATE 
	A 
SET 
	A.ReturnId = R.Id,
	A.ReturnStatus = r.ReturnStatus,
	A.CreatedDate = r.CreatedDate
FROM 
	@AATFReturn A 
	JOIN AATF.[Return] r ON r.OrganisationId = a.OrgId 	
		AND  r.ComplianceYear = @ComplianceYear
		AND r.[Quarter] = @Quarter
		AND r.FacilityType = @FacilityType
WHERE 
	A.ReturnStatus = 0

SELECT 
	AatfId,
	[Name],
	ApprovalNumber,
	OrganisationName,
	CASE WHEN Y.ReturnStatus = 1 THEN 'Started' WHEN Y.ReturnStatus = 2 THEN 'Submitted' ELSE 'Not Started' END AS ReturnStatus,
	CreatedDate,
	SubmittedDate,
	SubmittedBy,
	CompetentAuthorityAbbr,
	CASE WHEN y.ReSubmission = 0 THEN 'first submission' ELSE 'resubmission' END AS ReSubmission
FROM
	@AATFReturn Y
WHERE	
	(@ReturnStatus IS NULL OR Y.ReturnStatus = COALESCE(@ReturnStatus, Y.ReturnStatus))	 
ORDER BY 
	Y.[Name]

END