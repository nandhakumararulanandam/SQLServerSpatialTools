SELECT 'ClipGeometry' [Function Name]
	,Count(ID) [Failed Cases]
FROM LRS_ClipGeometrySegmentData
WHERE [OutputComparison] = 0

UNION ALL

SELECT 'EndMeasure' [Function Name]
	,Count(ID) [Failed Cases]
FROM LRS_GetEndMeasureData
WHERE [OutputComparison] = 0

UNION ALL

SELECT 'StartMeasure' [Function Name]
	,Count(ID) [Failed Cases]
FROM LRS_GetStartMeasureData
WHERE [OutputComparison] = 0

UNION ALL

SELECT 'Interpolate' [Function Name]
	,Count(ID) [Failed Cases]
FROM LRS_InterpolateBetweenGeomData
WHERE [Result] = 'Failed'

UNION ALL

SELECT 'IsConnected' [Function Name]
	,Count(ID) [Failed Cases]
FROM LRS_IsConnectedData
WHERE [OutputComparison] = 0

UNION ALL

SELECT 'LocatePoint' [Function Name]
	,Count(ID) [Failed Cases]
FROM LRS_LocatePointAlongGeomData
WHERE [OutputComparison] = 0

UNION ALL

SELECT 'MergeGeometry' [Function Name]
	,Count(ID) [Failed Cases]
FROM LRS_MergeGeometrySegmentsData
WHERE [OutputComparison] = 0

UNION ALL

SELECT 'OffsetGeometry' [Function Name]
	,Count(ID) [Failed Cases]
FROM LRS_OffsetGeometrySegmentData
WHERE [OutputComparison] = 0

UNION ALL

SELECT 'PopulateGeometry' [Function Name]
	,Count(ID) [Failed Cases]
FROM LRS_PopulateGeometryMeasuresData
WHERE [OutputComparison] = 0

UNION ALL

SELECT 'ResetMeasure' [Function Name]
	,Count(ID) [Failed Cases]
FROM LRS_ResetMeasureData
WHERE [Result] = 'Failed'

UNION ALL

SELECT 'ReverseGeometry' [Function Name]
	,Count(ID) [Failed Cases]
FROM LRS_ReverseLinearGeometryData
WHERE [OutputComparison] = 0

UNION ALL

SELECT 'SplitGeometry' [Function Name]
	,Count(ID) [Failed Cases]
FROM LRS_SplitGeometrySegmentData
WHERE [OutputComparison] = 0

UNION ALL

SELECT 'ValidatLRSGeometry' [Function Name]
	,Count(ID) [Failed Cases]
FROM LRS_ValidateLRSGeometryData
WHERE [OutputComparison] = 0;
