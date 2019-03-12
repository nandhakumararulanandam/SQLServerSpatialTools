-- Script that shows examples on exposed LRS Functions in SQLSpatialTools Library
-- LRS Geometric Functions
DECLARE @geom geometry;
DECLARE @srid INT = 4326;
DECLARE @distance FLOAT;
DECLARE @measure FLOAT;
DECLARE @tolerance FLOAT;
DECLARE @startMeasure FLOAT = 15.0;
DECLARE @endMeasure FLOAT = 20.0;

SET @geom = geometry::STGeomFromText('LINESTRING (20 1 NULL 10, 25 1 NULL 25 )', @srid);

-- 1. ClipGeometrySegement Function
SELECT 'Clipped Segment' AS 'FunctionInfo'
	,[dbo].[LRS_ClipGeometrySegment](@geom, @startMeasure, @endMeasure) AS 'Geometry'
	,[dbo].[LRS_ClipGeometrySegment](@geom, @startMeasure, @endMeasure).ToString() AS 'Geometry in String'

-- 2. Get Start Measure
SELECT 'Start Measure' AS 'FunctionInfo'
	,[dbo].[LRS_GetStartMeasure](@geom) AS 'Measure'

-- 3. Get End Measure
SELECT 'Start Measure' AS 'FunctionInfo'
	,[dbo].[LRS_GetEndMeasure](@geom) AS 'Measure'

-- 4. Interpolate points between Geom
DECLARE @geom1 geometry = geometry::STGeomFromText('POINT(0 0 0 0)', @srid);
DECLARE @geom2 geometry = geometry::STGeomFromText('POINT(10 0 0 10)', @srid);

SET @measure = 5;

SELECT 'Interpolate Points' AS 'FunctionInfo'
	,[dbo].[LRS_InterpolateBetweenGeom](@geom1, @geom2, @measure) AS 'Geometry'
	,[dbo].[LRS_InterpolateBetweenGeom](@geom1, @geom2, @measure).ToString() AS 'Geometry in String';

-- 5. Is Spatially Connected
SET @geom1 = geometry::STGeomFromText('LINESTRING(0 0 0 0, 1 1 0 0, 3 4 0 0, 5.5 5 0 0)', @srid);
SET @geom2 = geometry::STGeomFromText('LINESTRING(5 5 0 0, 2 2 0 0)', @srid);
SET @tolerance = 0.5;

SELECT 'Is Spatially Connected' AS 'FunctionInfo'
	,[dbo].[LRS_IsConnected](@geom1, @geom2, @tolerance) AS 'IsConnected';

-- 6. Locate Point Along the Geometry Segment
SET @geom = geometry::STGeomFromText('LINESTRING (0 0 0 0, 10 0 0 10)', @srid);
SET @measure = 5.0;

SELECT 'Point to Locate' AS 'FunctionInfo'
	,[dbo].[LRS_LocatePointAlongGeom](@geom, @measure) AS 'Geometry'
	,[dbo].[LRS_LocatePointAlongGeom](@geom, @measure).ToString() AS 'Geometry in String';

-- 7. Merge two Geometry Segments to one Geometry Segment.
SET @geom1 = geometry::STGeomFromText('LINESTRING (10 1 NULL 10, 25 1 NULL 25)', @srid);
SET @geom2 = geometry::STGeomFromText('LINESTRING (30 1 NULL 30, 40 1 NULL 40 )', @srid);

SELECT 'Merge Geometry Segments' AS 'FunctionInfo'
	,[dbo].[LRS_MergeGeometrySegments](@geom1, @geom2) AS 'Geometry'
	,[dbo].[LRS_MergeGeometrySegments](@geom1, @geom2).ToString() AS 'Geometry in String';

-- 8. Populate geometry measures.
SET @startMeasure = 10;
SET @endMeasure = 40;
SET @geom = geometry::STGeomFromText('LINESTRING (10 1 10 100, 15 1 10 NULL, 20 1 10 NULL, 25 1 10 250 )', @srid);

SELECT 'Populate Geometric Measures' AS 'FunctionInfo'
	,[dbo].[LRS_PopulateGeometryMeasures](@geom, @startMeasure, @endMeasure) AS 'Geometry'
	,[dbo].[LRS_PopulateGeometryMeasures](@geom, @startMeasure, @endMeasure).ToString() AS 'Geometry in String';

-- 9. Reverse Line String
SET @geom = geometry::STGeomFromText('LINESTRING (1 1 0 0, 5 5 0 0)', @srid);

SELECT 'Reverse Linear Geometry' AS 'FunctionInfo'
	,[dbo].[LRS_ReverseLinearGeometry](@geom) AS 'Geometry'
	,@geom.ToString() AS 'Input Line'
	,[dbo].[LRS_ReverseLinearGeometry](@geom).ToString() AS 'Geometry in String'

-- 10. Split Geometry Segment
SET @geom = geometry::STGeomFromText('LINESTRING (10 1 NULL 10, 25 1 NULL 25 )', @srid);
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

