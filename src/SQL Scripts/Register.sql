-- Copyright (c) Microsoft Corporation.  All rights reserved.
-- Install the SQLSpatialTools assembly and all its functions into the current database

-- Enabling CLR prior to registering assembly and its related functions.
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

-- Create User Defined SQL Types
CREATE TYPE Projection EXTERNAL NAME SQLSpatialTools.[SQLSpatialTools.Types.SQL.SqlProjection]
GO

CREATE TYPE AffineTransform EXTERNAL NAME SQLSpatialTools.[SQLSpatialTools.Types.SQL.AffineTransform]
GO

-- Register the functions...

--#region General Geometry Functions
CREATE FUNCTION FilterArtifactsGeometry (
    @g GEOMETRY
    ,@filterEmptyShapes BIT
    ,@filterPoints BIT
    ,@lineStringTolerance FLOAT(53)
    ,@ringTolerance FLOAT(53)
    )
RETURNS GEOMETRY
AS
EXTERNAL NAME SQLSpatialTools.[SQLSpatialTools.Functions.General.Geometry].FilterArtifactsGeometry
GO

CREATE FUNCTION GeomFromXYMText (
    @g NVARCHAR(MAX)
    ,@targetSrid INT
    )
RETURNS GEOMETRY
AS
EXTERNAL NAME SQLSpatialTools.[SQLSpatialTools.Functions.General.Geometry].GeomFromXYMText
GO

CREATE FUNCTION InterpolateBetweenGeom (
    @start GEOMETRY
    ,@end GEOMETRY
    ,@distance FLOAT
    )
RETURNS GEOMETRY
AS
EXTERNAL NAME SQLSpatialTools.[SQLSpatialTools.Functions.General.Geometry].InterpolateBetweenGeom
GO

CREATE FUNCTION LocateAlongGeom (
    @g GEOMETRY
    ,@distance FLOAT
    )
RETURNS GEOMETRY
AS
EXTERNAL NAME SQLSpatialTools.[SQLSpatialTools.Functions.General.Geometry].LocatePointAlongGeom
GO

CREATE FUNCTION MakeValidForGeography (@g GEOMETRY)
RETURNS GEOMETRY
AS
EXTERNAL NAME SQLSpatialTools.[SQLSpatialTools.Functions.General.Geometry].MakeValidForGeography
GO

CREATE FUNCTION ReverseLinestring (@g GEOMETRY)
RETURNS GEOMETRY
AS
EXTERNAL NAME SQLSpatialTools.[SQLSpatialTools.Functions.General.Geometry].ReverseLinestring
GO

CREATE FUNCTION ShiftGeometry (
    @g GEOMETRY
    ,@xShift FLOAT
    ,@yShift FLOAT
    )
RETURNS GEOMETRY
AS
EXTERNAL NAME SQLSpatialTools.[SQLSpatialTools.Functions.General.Geometry].ShiftGeometry
GO

CREATE FUNCTION VacuousGeometryToGeography (
    @toConvert GEOMETRY
    ,@targetSrid INT
    )
RETURNS GEOGRAPHY
AS
EXTERNAL NAME SQLSpatialTools.[SQLSpatialTools.Functions.General.Geometry].VacuousGeometryToGeography
GO

--#endregion

--#region General Geography Functions

CREATE FUNCTION ConvexHullGeographyFromText (
    @inputWKT NVARCHAR(max)
    ,@srid INT
    )
RETURNS GEOGRAPHY
AS
EXTERNAL NAME SQLSpatialTools.[SQLSpatialTools.Functions.General.Geography].ConvexHullGeographyFromText
GO

CREATE FUNCTION ConvexHullGeography (@geog GEOGRAPHY)
RETURNS GEOGRAPHY
AS
EXTERNAL NAME SQLSpatialTools.[SQLSpatialTools.Functions.General.Geography].ConvexHullGeography
GO

CREATE FUNCTION DensifyGeography (
    @geog GEOGRAPHY
    ,@maxAngle FLOAT
    )
RETURNS GEOGRAPHY
AS
EXTERNAL NAME SQLSpatialTools.[SQLSpatialTools.Functions.General.Geography].DensifyGeography
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
EXTERNAL NAME SQLSpatialTools.[SQLSpatialTools.Functions.General.Geography].FilterArtifactsGeography
GO

CREATE FUNCTION InterpolateBetweenGeog (
    @start GEOGRAPHY
    ,@end GEOGRAPHY
    ,@distance FLOAT
    )
RETURNS GEOGRAPHY
AS
EXTERNAL NAME SQLSpatialTools.[SQLSpatialTools.Functions.General.Geography].InterpolateBetweenGeog
GO

CREATE FUNCTION IsValidGeographyFromGeometry (@inputGeometry GEOMETRY)
RETURNS BIT
AS
EXTERNAL NAME SQLSpatialTools.[SQLSpatialTools.Functions.General.Geography].IsValidGeographyFromGeometry
GO

CREATE FUNCTION IsValidGeographyFromText (
    @inputWKT NVARCHAR(MAX)
    ,@srid INT
    )
RETURNS BIT
AS
EXTERNAL NAME SQLSpatialTools.[SQLSpatialTools.Functions.General.Geography].IsValidGeographyFromText
GO

CREATE FUNCTION LocateAlongGeog (
    @g GEOGRAPHY
    ,@distance FLOAT
    )
RETURNS GEOGRAPHY
AS
EXTERNAL NAME SQLSpatialTools.[SQLSpatialTools.Functions.General.Geography].LocatePointAlongGeog
GO

CREATE FUNCTION MakeValidGeographyFromGeometry (@inputGeometry GEOMETRY)
RETURNS GEOGRAPHY
AS
EXTERNAL NAME SQLSpatialTools.[SQLSpatialTools.Functions.General.Geography].MakeValidGeographyFromGeometry
GO

CREATE FUNCTION MakeValidGeographyFromText (
    @inputWKT NVARCHAR(MAX)
    ,@srid INT
    )
RETURNS GEOGRAPHY
AS
EXTERNAL NAME SQLSpatialTools.[SQLSpatialTools.Functions.General.Geography].MakeValidGeographyFromText
GO

CREATE FUNCTION VacuousGeographyToGeometry (
    @toConvert GEOGRAPHY
    ,@targetSrid INT
    )
RETURNS GEOMETRY
AS
EXTERNAL NAME SQLSpatialTools.[SQLSpatialTools.Functions.General.Geography].VacuousGeographyToGeometry
GO

--#endregion

--#region LRS Geometric Functions
CREATE FUNCTION LRS_ClipGeometrySegment (
    @g GEOMETRY
    ,@startMeasure FLOAT
    ,@endMeasure FLOAT
    ,@tolerance FLOAT
    )
RETURNS GEOMETRY
AS
EXTERNAL NAME SQLSpatialTools.[SQLSpatialTools.Functions.LRS.Geometry].ClipGeometrySegment
GO

CREATE FUNCTION LRS_GetEndMeasure (@geomSegment1 GEOMETRY)
RETURNS FLOAT
AS
EXTERNAL NAME SQLSpatialTools.[SQLSpatialTools.Functions.LRS.Geometry].GetEndMeasure
GO

CREATE FUNCTION LRS_GetStartMeasure (@geomSegment1 GEOMETRY)
RETURNS FLOAT
AS
EXTERNAL NAME SQLSpatialTools.[SQLSpatialTools.Functions.LRS.Geometry].GetStartMeasure
GO

CREATE FUNCTION LRS_InterpolateBetweenGeom (
    @start GEOMETRY
    ,@end GEOMETRY
    ,@measure FLOAT
    )
RETURNS GEOMETRY
AS
EXTERNAL NAME SQLSpatialTools.[SQLSpatialTools.Functions.LRS.Geometry].InterpolateBetweenGeom
GO

CREATE FUNCTION LRS_IsConnected (
    @g1 GEOMETRY
    ,@g2 GEOMETRY
    ,@tolerance FLOAT
    )
RETURNS BIT
AS
EXTERNAL NAME SQLSpatialTools.[SQLSpatialTools.Functions.LRS.Geometry].IsConnected
GO

CREATE FUNCTION LRS_IsValidPoint (
    @g GEOMETRY
    )
RETURNS BIT
AS
EXTERNAL NAME SQLSpatialTools.[SQLSpatialTools.Functions.LRS.Geometry].IsValidPoint
GO

CREATE FUNCTION LRS_LocatePointAlongGeom (
    @g GEOMETRY
    ,@distance FLOAT
    )
RETURNS GEOMETRY
AS
EXTERNAL NAME SQLSpatialTools.[SQLSpatialTools.Functions.LRS.Geometry].LocatePointAlongGeom
GO

CREATE FUNCTION LRS_MergeGeometrySegments (
    @g1 GEOMETRY
    ,@g2 GEOMETRY
    ,@tolerance FLOAT
    )
RETURNS GEOMETRY
AS
EXTERNAL NAME SQLSpatialTools.[SQLSpatialTools.Functions.LRS.Geometry].MergeGeometrySegments
GO

CREATE FUNCTION LRS_MergeAndResetGeometrySegments (
    @g1 GEOMETRY
    ,@g2 GEOMETRY
    ,@tolerance FLOAT
    )
RETURNS GEOMETRY
AS
EXTERNAL NAME SQLSpatialTools.[SQLSpatialTools.Functions.LRS.Geometry].MergeAndResetGeometrySegments
GO

CREATE FUNCTION LRS_OffsetGeometrySegments (
    @g GEOMETRY
    ,@startMeasure FLOAT
    ,@endMeasure FLOAT
    ,@offset FLOAT
    ,@tolerance FLOAT
    )
RETURNS GEOMETRY
AS
EXTERNAL NAME SQLSpatialTools.[SQLSpatialTools.Functions.LRS.Geometry].OffsetGeometrySegment
GO

CREATE FUNCTION LRS_PopulateGeometryMeasures (
    @g GEOMETRY
    ,@startMeasure FLOAT
    ,@endMeasure FLOAT
    )
RETURNS GEOMETRY
AS
EXTERNAL NAME SQLSpatialTools.[SQLSpatialTools.Functions.LRS.Geometry].PopulateGeometryMeasures
GO

CREATE FUNCTION LRS_ResetMeasure(@g GEOMETRY)
RETURNS GEOMETRY
AS
EXTERNAL NAME SQLSpatialTools.[SQLSpatialTools.Functions.LRS.Geometry].ResetMeasure
GO

CREATE FUNCTION LRS_ReverseLinearGeometry (@g GEOMETRY)
RETURNS GEOMETRY
AS
EXTERNAL NAME SQLSpatialTools.[SQLSpatialTools.Functions.LRS.Geometry].ReverseLinearGeometry
GO

CREATE FUNCTION LRS_ScaleGeometryMeasures (@g GEOMETRY
    ,@scaleMeasure FLOAT(53))
RETURNS GEOMETRY
AS
EXTERNAL NAME SQLSpatialTools.[SQLSpatialTools.Functions.LRS.Geometry].ScaleGeometryMeasures
GO

CREATE PROCEDURE LRS_SplitGeometrySegment @g GEOMETRY
    ,@splitMeasure FLOAT(53)
    ,@geomSegement1 GEOMETRY OUTPUT
    ,@geomSegement2 GEOMETRY OUTPUT
AS
EXTERNAL NAME SQLSpatialTools.[SQLSpatialTools.Functions.LRS.Geometry].SplitGeometrySegment
GO

CREATE FUNCTION LRS_TranslateMeasure (@g GEOMETRY
    ,@translateMeasure FLOAT(53))
RETURNS GEOMETRY
AS
EXTERNAL NAME SQLSpatialTools.[SQLSpatialTools.Functions.LRS.Geometry].TranslateMeasure
GO

CREATE FUNCTION LRS_ValidateLRSGeometry (@g GEOMETRY)
RETURNS NVARCHAR(10)
AS
EXTERNAL NAME SQLSpatialTools.[SQLSpatialTools.Functions.LRS.Geometry].ValidateLRSGeometry
GO

--#endregion

-- Create aggregates.
CREATE AGGREGATE GEOMETRYEnvelopeAggregate (@geom GEOMETRY)
RETURNS GEOMETRY EXTERNAL NAME SQLSpatialTools.[SQLSpatialTools.Aggregates.GeometryEnvelopeAggregate]
GO

CREATE AGGREGATE GeographyCollectionAggregate (@geog GEOGRAPHY)
RETURNS GEOGRAPHY EXTERNAL NAME SQLSpatialTools.[SQLSpatialTools.Aggregates.GeographyCollectionAggregate]
GO

CREATE AGGREGATE GeographyUnionAggregate (@geog GEOGRAPHY)
RETURNS GEOGRAPHY EXTERNAL NAME SQLSpatialTools.[SQLSpatialTools.Aggregates.GeographyUnionAggregate]
GO