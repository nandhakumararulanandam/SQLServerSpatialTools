-- Script that shows examples on exposed LRS Functions in SQLSpatialTools Library
-- LRS Geometric Functions
DECLARE @geom geometry;
DECLARE @srid INT = 4326;
DECLARE @distance FLOAT;
DECLARE @measure FLOAT;
DECLARE @tolerance FLOAT = 0.5;
DECLARE @startMeasure FLOAT = 15.0;
DECLARE @endMeasure FLOAT = 20.0;

SET @geom = GEOMETRY::STGeomFromText('LINESTRING (20 1 NULL 10, 25 1 NULL 25 )', @srid);

-- 1. ClipGeometrySegement Function
SELECT 'Clipped Segment' AS 'FunctionInfo'
	,[dbo].[LRS_ClipGeometrySegment](@geom, @startMeasure, @endMeasure, @tolerance) AS 'Geometry'
	,[dbo].[LRS_ClipGeometrySegment](@geom, @startMeasure, @endMeasure, @tolerance).ToString() AS 'Geometry in String'

-- 2. Get Start Measure
SELECT 'Start Measure' AS 'FunctionInfo'
	,[dbo].[LRS_GetStartMeasure](@geom) AS 'Measure'

-- 3. Get End Measure
SELECT 'End Measure' AS 'FunctionInfo'
	,[dbo].[LRS_GetEndMeasure](@geom) AS 'Measure'

-- 4. Interpolate points between Geom
DECLARE @geom1 geometry = GEOMETRY::STGeomFromText('POINT(0 0 0 0)', @srid);
DECLARE @geom2 geometry = GEOMETRY::STGeomFromText('POINT(10 0 0 10)', @srid);

SET @measure = 5;

SELECT 'Interpolate Points' AS 'FunctionInfo'
	,[dbo].[LRS_InterpolateBetweenGeom](@geom1, @geom2, @measure) AS 'Geometry'
	,[dbo].[LRS_InterpolateBetweenGeom](@geom1, @geom2, @measure).ToString() AS 'Geometry in String';

-- 5. Is Spatially Connected
SET @geom1 = GEOMETRY::STGeomFromText('LINESTRING(0 0 0 0, 1 1 0 0, 3 4 0 0, 5.5 5 0 0)', @srid);
SET @geom2 = GEOMETRY::STGeomFromText('LINESTRING(5 5 0 0, 2 2 0 0)', @srid);
SET @tolerance = 0.5;

SELECT 'Is Spatially Connected' AS 'FunctionInfo'
	,[dbo].[LRS_IsConnected](@geom1, @geom2, @tolerance) AS 'IsConnected';

-- 6. IsValid LRS Point
SET @geom = GEOMETRY::STGeomFromText('POINT(0 0 0)', @srid);

SELECT 'Is Valid LRS Point' AS 'FunctionInfo'
	,[dbo].[LRS_IsValidPoint](@geom) AS 'IsValidLRSPoint';

-- 7. Locate Point Along the Geometry Segment
SET @geom = GEOMETRY::STGeomFromText('LINESTRING (0 0 0 0, 10 0 0 10)', @srid);
SET @measure = 5.0;

SELECT 'Point to Locate' AS 'FunctionInfo'
	,[dbo].[LRS_LocatePointAlongGeom](@geom, @measure) AS 'Geometry'
	,[dbo].[LRS_LocatePointAlongGeom](@geom, @measure).ToString() AS 'Geometry in String';

-- 8. Merge two Geometry Segments to one Geometry Segment.
SET @geom1 = GEOMETRY::STGeomFromText('LINESTRING (10 1 NULL 10, 25 1 NULL 25)', @srid);
SET @geom2 = GEOMETRY::STGeomFromText('LINESTRING (30 1 NULL 30, 40 1 NULL 40 )', @srid);

SELECT 'Merge Geometry Segments' AS 'FunctionInfo'
	,[dbo].[LRS_MergeGeometrySegments](@geom1, @geom2) AS 'Geometry'
	,[dbo].[LRS_MergeGeometrySegments](@geom1, @geom2).ToString() AS 'Geometry in String';

-- 9. Populate geometry measures.
SET @startMeasure = 10;
SET @endMeasure = 40;
SET @geom = GEOMETRY::STGeomFromText('LINESTRING (10 1 10 100, 15 1 10 NULL, 20 1 10 NULL, 25 1 10 250 )', @srid);

SELECT 'Populate Geometric Measures' AS 'FunctionInfo'
	,[dbo].[LRS_PopulateGeometryMeasures](@geom, @startMeasure, @endMeasure) AS 'Geometry'
	,[dbo].[LRS_PopulateGeometryMeasures](@geom, @startMeasure, @endMeasure).ToString() AS 'Geometry in String';

-- 10. Reset Measure
SET @geom = GEOMETRY::STGeomFromText('LINESTRING (1 1 0 10, 5 5 0 25)', @srid);

SELECT 'Reset Measure' AS 'FunctionInfo'
	,[dbo].[LRS_ResetMeasure](@geom) AS 'Geometry'
	,@geom.ToString() AS 'Input Line'
	,[dbo].[LRS_ResetMeasure](@geom).ToString() AS 'Geometry in String'

-- 11. Reverse Line String
SET @geom = GEOMETRY::STGeomFromText('LINESTRING (1 1 0 0, 5 5 0 0)', @srid);

SELECT 'Reverse Linear Geometry' AS 'FunctionInfo'
	,[dbo].[LRS_ReverseLinearGeometry](@geom) AS 'Geometry'
	,@geom.ToString() AS 'Input Line'
	,[dbo].[LRS_ReverseLinearGeometry](@geom).ToString() AS 'Geometry in String'

-- 12. Split Geometry Segment
SET @geom = GEOMETRY::STGeomFromText('LINESTRING (10 1 NULL 10, 25 1 NULL 25 )', @srid);
SET @measure = 15;
EXECUTE [dbo].[LRS_SplitGeometrySegment] 
   @geom
  ,@measure
  ,@geom1 OUTPUT
  ,@geom2 OUTPUT

SELECT 'Split Line Segment' AS 'FunctionInfo'	
	,@geom.ToString() AS 'Input Line'
	,@geom1.ToString() AS 'Line Segment 1'
	,@geom2.ToString() AS 'Line Segment 2'

-- 13. Validate LRS Segment
SET @geom1 = GEOMETRY::STGeomFromText('LINESTRING (1 1 0 0, 5 5 0 0)', @srid);
SET @geom2 = GEOMETRY::STGeomFromText('LINESTRING (2 2 0, 2 4 2, 8 4 8, 12 4 12, 12 10 29, 8 10 22, 5 14 27)', @srid);
SET @geom = GEOMETRY::STGeomFromText('LINESTRING (2 2, 2 4, 8 4)', @srid);

SELECT 'Validate LRS Segment' AS 'FunctionInfo'
	,@geom1.ToString() AS 'Input Geom Segment'
	,[dbo].[LRS_ValidateLRSGeometry](@geom1) AS 'Valid State'
UNION
SELECT 'Validate LRS Segment' AS 'FunctionInfo'
	,@geom2.ToString() AS 'Input Geom Segment'
	,[dbo].[LRS_ValidateLRSGeometry](@geom2) AS 'Valid State'
UNION
SELECT 'Validate LRS Segment' AS 'FunctionInfo'
	,@geom.ToString() AS 'Input Geom Segment'
	,[dbo].[LRS_ValidateLRSGeometry](@geom) AS 'Valid State'