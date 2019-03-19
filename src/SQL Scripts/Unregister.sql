-- Copyright (c) Microsoft Corporation.  All rights reserved.
-- Drop the SQLSpatialTools assembly and all its functions from the current database
-- Drop the aggregates...
DROP aggregate GeometryEnvelopeAggregate

DROP aggregate GeographyCollectionAggregate

DROP aggregate GeographyUnionAggregate

-- Drop the functions...
-- General Geometry
DROP FUNCTION FilterArtifactsGeometry

DROP FUNCTION GeomFromXYMText

DROP FUNCTION InterpolateBetweenGeom

DROP FUNCTION LocateAlongGeom

DROP FUNCTION MakeValidForGeography

DROP FUNCTION ReverseLinestring

DROP FUNCTION ShiftGeometry

DROP FUNCTION VacuousGeometryToGeography

-- General Geography
DROP FUNCTION ConvexHullGeographyFromText

DROP FUNCTION ConvexHullGeography

DROP FUNCTION DensifyGeography

DROP FUNCTION FilterArtifactsGeography

DROP FUNCTION InterpolateBetweenGeog

DROP FUNCTION IsValidGeographyFromGeometry

DROP FUNCTION IsValidGeographyFromText

DROP FUNCTION LocateAlongGeog

DROP FUNCTION MakeValidGeographyFromGeometry

DROP FUNCTION MakeValidGeographyFromText

DROP FUNCTION VacuousGeographyToGeometry

-- LRS Geometry
DROP FUNCTION LRS_ClipGeometrySegment

DROP FUNCTION LRS_GetEndMeasure

DROP FUNCTION LRS_GetStartMeasure

DROP FUNCTION LRS_InterpolateBetweenGeom

DROP FUNCTION LRS_IsConnected

DROP FUNCTION LRS_LocatePointAlongGeom

DROP FUNCTION LRS_MergeGeometrySegments

DROP FUNCTION LRS_PopulateGeometryMeasures

DROP FUNCTION LRS_ResetMeasure;

DROP FUNCTION LRS_ReverseLinearGeometry

DROP PROCEDURE LRS_SplitGeometrySegment

-- Drop the types...
DROP type Projection

DROP type AffineTransform

-- Drop the assembly...
DROP assembly SQLSpatialTools
