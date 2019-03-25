-- Copyright (c) Microsoft Corporation.  All rights reserved.
-- Install the SQLSpatialTools assembly and all its functions into the current database

EXEC sp_configure 'show advanced option', '1'; 
RECONFIGURE;
Go

sp_configure 'clr enabled', 1  ;
GO  
RECONFIGURE  ;
GO

EXEC sp_configure 'clr strict security',  '0'
RECONFIGURE WITH OVERRIDE;
GO

-- !!! DLL Path will be replace based upon system environment !!!
CREATE assembly SQLSpatialTools
FROM 'DLLPath'
GO

-- Create types
CREATE type Projection EXTERNAL name SQLSpatialTools.[SQLSpatialTools.SqlProjection]
GO

CREATE type AffineTransform EXTERNAL name SQLSpatialTools.[SQLSpatialTools.AffineTransform]
GO

-- Register the functions...
--#region General Geometry Functions
CREATE FUNCTION FilterArtifactsGeometry (
	@g geometry
	,@filterEmptyShapes BIT
	,@filterPoints BIT
	,@lineStringTolerance FLOAT(53)
	,@ringTolerance FLOAT(53)
	)
RETURNS geometry
AS
EXTERNAL name SQLSpatialTools.[SQLSpatialTools.Functions.General.Geometry].FilterArtifactsGeometry
GO

CREATE FUNCTION GeomFromXYMText (
	@g NVARCHAR(max)
	,@targetSrid INT
	)
RETURNS geometry
AS
EXTERNAL name SQLSpatialTools.[SQLSpatialTools.Functions.General.Geometry].GeomFromXYMText
GO

CREATE FUNCTION InterpolateBetweenGeom (
	@start geometry
	,@end geometry
	,@distance FLOAT
	)
RETURNS geometry
AS
EXTERNAL name SQLSpatialTools.[SQLSpatialTools.Functions.General.Geometry].InterpolateBetweenGeom
GO

CREATE FUNCTION LocateAlongGeom (
	@g geometry
	,@distance FLOAT
	)
RETURNS geometry
AS
EXTERNAL name SQLSpatialTools.[SQLSpatialTools.Functions.General.Geometry].LocatePointAlongGeom
GO

CREATE FUNCTION MakeValidForGeography (@g geometry)
RETURNS geometry
AS
EXTERNAL name SQLSpatialTools.[SQLSpatialTools.Functions.General.Geometry].MakeValidForGeography
GO

CREATE FUNCTION ReverseLinestring (@g geometry)
RETURNS geometry
AS
EXTERNAL name SQLSpatialTools.[SQLSpatialTools.Functions.General.Geometry].ReverseLinestring
GO

CREATE FUNCTION ShiftGeometry (
	@g geometry
	,@xShift FLOAT
	,@yShift FLOAT
	)
RETURNS geometry
AS
EXTERNAL name SQLSpatialTools.[SQLSpatialTools.Functions.General.Geometry].ShiftGeometry
GO

CREATE FUNCTION VacuousGeometryToGeography (
	@toConvert geometry
	,@targetSrid INT
	)
RETURNS GEOGRAPHY
AS
EXTERNAL name SQLSpatialTools.[SQLSpatialTools.Functions.General.Geometry].VacuousGeometryToGeography
GO

--#endregion
--#region General Geography Functions
CREATE FUNCTION ConvexHullGeographyFromText (
	@inputWKT NVARCHAR(max)
	,@srid INT
	)
RETURNS GEOGRAPHY
AS
EXTERNAL name SQLSpatialTools.[SQLSpatialTools.Functions.General.Geography].ConvexHullGeographyFromText
GO

CREATE FUNCTION ConvexHullGeography (@geog GEOGRAPHY)
RETURNS GEOGRAPHY
AS
EXTERNAL name SQLSpatialTools.[SQLSpatialTools.Functions.General.Geography].ConvexHullGeography
GO

CREATE FUNCTION DensifyGeography (
	@geog GEOGRAPHY
	,@maxAngle FLOAT
	)
RETURNS GEOGRAPHY
AS
EXTERNAL name SQLSpatialTools.[SQLSpatialTools.Functions.General.Geography].DensifyGeography
GO

CREATE FUNCTION FilterArtifactsGeography (
	@geog GEOGRAPHY
	,@filterEmptyShapes BIT
	,@filterPoints BIT
	,@lineStringTolerance FLOAT(53)
	,@ringTolerance FLOAT(53)
	)
RETURNS GEOGRAPHY
AS
EXTERNAL name SQLSpatialTools.[SQLSpatialTools.Functions.General.Geography].FilterArtifactsGeography
GO

CREATE FUNCTION InterpolateBetweenGeog (
	@start GEOGRAPHY
	,@end GEOGRAPHY
	,@distance FLOAT
	)
RETURNS GEOGRAPHY
AS
EXTERNAL name SQLSpatialTools.[SQLSpatialTools.Functions.General.Geography].InterpolateBetweenGeog
GO

CREATE FUNCTION IsValidGeographyFromGeometry (@inputGeometry geometry)
RETURNS BIT
AS
EXTERNAL name SQLSpatialTools.[SQLSpatialTools.Functions.General.Geography].IsValidGeographyFromGeometry
GO

CREATE FUNCTION IsValidGeographyFromText (
	@inputWKT NVARCHAR(max)
	,@srid INT
	)
RETURNS BIT
AS
EXTERNAL name SQLSpatialTools.[SQLSpatialTools.Functions.General.Geography].IsValidGeographyFromText
GO

CREATE FUNCTION LocateAlongGeog (
	@g GEOGRAPHY
	,@distance FLOAT
	)
RETURNS GEOGRAPHY
AS
EXTERNAL name SQLSpatialTools.[SQLSpatialTools.Functions.General.Geography].LocatePointAlongGeog
GO

CREATE FUNCTION MakeValidGeographyFromGeometry (@inputGeometry geometry)
RETURNS GEOGRAPHY
AS
EXTERNAL name SQLSpatialTools.[SQLSpatialTools.Functions.General.Geography].MakeValidGeographyFromGeometry
GO

CREATE FUNCTION MakeValidGeographyFromText (
	@inputWKT NVARCHAR(max)
	,@srid INT
	)
RETURNS GEOGRAPHY
AS
EXTERNAL name SQLSpatialTools.[SQLSpatialTools.Functions.General.Geography].MakeValidGeographyFromText
GO

CREATE FUNCTION VacuousGeographyToGeometry (
	@toConvert GEOGRAPHY
	,@targetSrid INT
	)
RETURNS geometry
AS
EXTERNAL name SQLSpatialTools.[SQLSpatialTools.Functions.General.Geography].VacuousGeographyToGeometry
GO

--#endregion
--#region LRS Geometric Functions
CREATE FUNCTION LRS_ClipGeometrySegment (
	@g geometry
	,@startMeasure FLOAT
	,@endMeasure FLOAT
	)
RETURNS geometry
AS
EXTERNAL name SQLSpatialTools.[SQLSpatialTools.Functions.LRS.Geometry].ClipGeometrySegment
GO

CREATE FUNCTION LRS_GetEndMeasure (@geomSegment1 geometry)
RETURNS FLOAT
AS
EXTERNAL name SQLSpatialTools.[SQLSpatialTools.Functions.LRS.Geometry].GetEndMeasure
GO

CREATE FUNCTION LRS_GetStartMeasure (@geomSegment1 geometry)
RETURNS FLOAT
AS
EXTERNAL name SQLSpatialTools.[SQLSpatialTools.Functions.LRS.Geometry].GetStartMeasure
GO

CREATE FUNCTION LRS_InterpolateBetweenGeom (
	@start geometry
	,@end geometry
	,@measure FLOAT
	)
RETURNS geometry
AS
EXTERNAL name SQLSpatialTools.[SQLSpatialTools.Functions.LRS.Geometry].InterpolateBetweenGeom
GO

CREATE FUNCTION LRS_IsConnected (
	@g1 geometry
	,@g2 geometry
	,@tolerance FLOAT
	)
RETURNS BIT
AS
EXTERNAL name SQLSpatialTools.[SQLSpatialTools.Functions.LRS.Geometry].IsConnected
GO

CREATE FUNCTION LRS_IsValidPoint (
	@g geometry
	)
RETURNS BIT
AS
EXTERNAL name SQLSpatialTools.[SQLSpatialTools.Functions.LRS.Geometry].IsValidPoint
GO

CREATE FUNCTION LRS_LocatePointAlongGeom (
	@g geometry
	,@distance FLOAT
	)
RETURNS geometry
AS
EXTERNAL name SQLSpatialTools.[SQLSpatialTools.Functions.LRS.Geometry].LocatePointAlongGeom
GO

CREATE FUNCTION LRS_MergeGeometrySegments (
	@g1 geometry
	,@g2 geometry
	)
RETURNS geometry
AS
EXTERNAL name SQLSpatialTools.[SQLSpatialTools.Functions.LRS.Geometry].MergeGeometrySegments
GO

CREATE FUNCTION LRS_PopulateGeometryMeasures (
	@g geometry
	,@startMeasure FLOAT
	,@endMeasure FLOAT
	)
RETURNS geometry
AS
EXTERNAL name SQLSpatialTools.[SQLSpatialTools.Functions.LRS.Geometry].PopulateGeometryMeasures
GO

CREATE FUNCTION LRS_ResetMeasure(@g geometry)
RETURNS geometry
AS
EXTERNAL name SQLSpatialTools.[SQLSpatialTools.Functions.LRS.Geometry].ResetMeasure
GO

CREATE FUNCTION LRS_ReverseLinearGeometry (@g geometry)
RETURNS geometry
AS
EXTERNAL name SQLSpatialTools.[SQLSpatialTools.Functions.LRS.Geometry].ReverseLinearGeometry
GO

CREATE PROCEDURE LRS_SplitGeometrySegment @g geometry
	,@splitMeasure FLOAT(53)
	,@geomSegement1 geometry OUTPUT
	,@geomSegement2 geometry OUTPUT
AS
EXTERNAL NAME SQLSpatialTools.[SQLSpatialTools.Functions.LRS.Geometry].SplitGeometrySegment
GO

-- Create aggregates.
CREATE aggregate GeometryEnvelopeAggregate (@geom geometry)
RETURNS geometry EXTERNAL name SQLSpatialTools.[SQLSpatialTools.GeometryEnvelopeAggregate]
GO

CREATE aggregate GeographyCollectionAggregate (@geog GEOGRAPHY)
RETURNS GEOGRAPHY EXTERNAL name SQLSpatialTools.[SQLSpatialTools.GeographyCollectionAggregate]
GO

CREATE aggregate GeographyUnionAggregate (@geog GEOGRAPHY)
RETURNS GEOGRAPHY EXTERNAL name SQLSpatialTools.[SQLSpatialTools.GeographyUnionAggregate]
GO