SELECT 'ClipGeometry' [Function Name]
	,Count(ID) [Total]
	,SUM(CASE WHEN OutputComparison = 1 THEN 1 ELSE 0 END) [Passed]
	,SUM(CASE WHEN OutputComparison = 0 THEN 1 ELSE 0 END) [Failed]
FROM LRS_ClipGeometrySegmentData

UNION ALL

SELECT 'EndMeasure' [Function Name]
	,Count(ID) [Total]
	,SUM(CASE WHEN OutputComparison = 1 THEN 1 ELSE 0 END) [Passed]
	,SUM(CASE WHEN OutputComparison = 0 THEN 1 ELSE 0 END) [Failed]
FROM LRS_GetEndMeasureData

UNION ALL

SELECT 'StartMeasure' [Function Name]
	,Count(ID) [Total]
	,SUM(CASE WHEN OutputComparison = 1 THEN 1 ELSE 0 END) [Passed]
	,SUM(CASE WHEN OutputComparison = 0 THEN 1 ELSE 0 END) [Failed]
FROM LRS_GetStartMeasureData

UNION ALL

SELECT 'Interpolate' [Function Name]
	,Count(ID) [Total]
	,SUM(CASE WHEN Result = 'Passed' THEN 1 ELSE 0 END) [Passed]
	,SUM(CASE WHEN Result = 'Failed' THEN 1 ELSE 0 END) [Failed]
FROM LRS_InterpolateBetweenGeomData

UNION ALL

SELECT 'IsConnected' [Function Name]
	,Count(ID) [Total]
	,SUM(CASE WHEN OutputComparison = 1 THEN 1 ELSE 0 END) [Passed]
	,SUM(CASE WHEN OutputComparison = 0 THEN 1 ELSE 0 END) [Failed]
FROM LRS_IsConnectedData

UNION ALL

SELECT 'LocatePoint' [Function Name]
	,Count(ID) [Total]
	,SUM(CASE WHEN OutputComparison = 1 THEN 1 ELSE 0 END) [Passed]
	,SUM(CASE WHEN OutputComparison = 0 THEN 1 ELSE 0 END) [Failed]
FROM LRS_LocatePointAlongGeomData

UNION ALL

SELECT 'MergeGeometry' [Function Name]
	,Count(ID) [Total]
	,SUM(CASE WHEN OutputComparison = 1 THEN 1 ELSE 0 END) [Passed]
	,SUM(CASE WHEN OutputComparison = 0 THEN 1 ELSE 0 END) [Failed]
FROM LRS_MergeGeometrySegmentsData

UNION ALL

SELECT 'OffsetGeometry' [Function Name]
	,Count(ID) [Total]
	,SUM(CASE WHEN OutputComparison = 1 THEN 1 ELSE 0 END) [Passed]
	,SUM(CASE WHEN OutputComparison = 0 THEN 1 ELSE 0 END) [Failed]
FROM LRS_OffsetGeometrySegmentData

UNION ALL

SELECT 'PopulateGeometry' [Function Name]
	,Count(ID) [Total]
	,SUM(CASE WHEN OutputComparison = 1 THEN 1 ELSE 0 END) [Passed]
	,SUM(CASE WHEN OutputComparison = 0 THEN 1 ELSE 0 END) [Failed]
FROM LRS_PopulateGeometryMeasuresData

UNION ALL

SELECT 'ResetMeasure' [Function Name]
	,Count(ID) [Total]
	,SUM(CASE WHEN Result = 'Passed' THEN 1 ELSE 0 END) [Passed]
	,SUM(CASE WHEN Result = 'Failed' THEN 1 ELSE 0 END) [Failed]
FROM LRS_ResetMeasureData

UNION ALL

SELECT 'ReverseGeometry' [Function Name]
	,Count(ID) [Total]
	,SUM(CASE WHEN OutputComparison = 1 THEN 1 ELSE 0 END) [Passed]
	,SUM(CASE WHEN OutputComparison = 0 THEN 1 ELSE 0 END) [Failed]
FROM LRS_ReverseLinearGeometryData

UNION ALL

SELECT 'SplitGeometry' [Function Name]
	,Count(ID) [Total]
	,SUM(CASE WHEN OutputComparison = 1 THEN 1 ELSE 0 END) [Passed]
	,SUM(CASE WHEN OutputComparison = 0 THEN 1 ELSE 0 END) [Failed]
FROM LRS_SplitGeometrySegmentData

UNION ALL

SELECT 'ValidatLRSGeometry' [Function Name]
	,Count(ID) [Total]
	,SUM(CASE WHEN OutputComparison = 1 THEN 1 ELSE 0 END) [Passed]
	,SUM(CASE WHEN OutputComparison = 0 THEN 1 ELSE 0 END) [Failed]
FROM LRS_ValidateLRSGeometryData;
