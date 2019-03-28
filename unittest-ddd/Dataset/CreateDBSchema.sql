-- LRS Functions Test Data Table

-- ClipGeometrySegment
CREATE TABLE [LRS_ClipGeometrySegmentData] (
  [Id] int IDENTITY (1,1) NOT NULL
, [InputGeom] nvarchar(1000) NOT NULL
, [StartMeasure] float NOT NULL
, [EndMeasure] float NOT NULL
, [ExpectedGeom] nvarchar(1000) NOT NULL
, [ObtainedGeom] nvarchar(1000)
, [Result] nvarchar(50)
, [ElapsedTime] nvarchar(100)
, [Error] nvarchar(1000)
, [OracleResult1] nvarchar(1000)
, [OracleElapsedTime] nvarchar(100)
, [OracleError] nvarchar(1000)
);
GO
ALTER TABLE [LRS_ClipGeometrySegmentData] ADD CONSTRAINT [PK_ClipGeometrySegmentTest] PRIMARY KEY ([Id]);
GO


-- Get End Measure
CREATE TABLE [LRS_GetEndMeasureData] (
  [Id] int IDENTITY (1,1) NOT NULL
, [InputGeom] nvarchar(1000) NOT NULL
, [ExpectedEndMeasure] float NOT NULL
, [ObtainedEndMeasure] float
, [Result] nvarchar(50)
, [ElapsedTime] nvarchar(100)
, [Error] nvarchar(1000)
, [OracleResult1] nvarchar(1000)
, [OracleElapsedTime] nvarchar(100)
, [OracleError] nvarchar(1000)
);
GO
ALTER TABLE [LRS_GetEndMeasureData] ADD CONSTRAINT [PK_GetEndMeasureData] PRIMARY KEY ([Id]);
GO


-- GetStartMeasure
CREATE TABLE [LRS_GetStartMeasureData] (
  [Id] int IDENTITY (1,1) NOT NULL
, [InputGeom] nvarchar(1000) NOT NULL
, [ExpectedStartMeasure] float NOT NULL
, [ObtainedStartMeasure] float
, [Result] nvarchar(50)
, [ElapsedTime] nvarchar(100)
, [Error] nvarchar(1000)
, [OracleResult1] nvarchar(1000)
, [OracleElapsedTime] nvarchar(100)
, [OracleError] nvarchar(1000)
);
GO
ALTER TABLE [LRS_GetStartMeasureData] ADD CONSTRAINT [PK_GetStartMeasureData] PRIMARY KEY ([Id]);
GO


-- InterpolateBetweenGeom
CREATE TABLE [LRS_InterpolateBetweenGeomData] (
 [Id] int IDENTITY (1,1) NOT NULL
, [InputGeom1] nvarchar(1000) NOT NULL
, [InputGeom2] nvarchar(1000) NOT NULL
, [Measure] float NOT NULL
, [ExpectedPoint] nvarchar(1000) NOT NULL
, [ObtainedPoint] nvarchar(1000)
, [Result] nvarchar(50)
, [ElapsedTime] nvarchar(100)
, [Error] nvarchar(1000)
);
GO
ALTER TABLE [LRS_InterpolateBetweenGeomData] ADD CONSTRAINT [PK_InterpolateBetweenGeomData] PRIMARY KEY ([Id]);
GO


-- IsConnected
CREATE TABLE [LRS_IsConnectedData] (
 [Id] int IDENTITY (1,1) NOT NULL
, [InputGeom1] nvarchar(1000) NOT NULL
, [InputGeom2] nvarchar(1000) NOT NULL
, [Tolerance] float NOT NULL
, [Expected] bit NOT NULL
, [Obtained] bit
, [Result] nvarchar(50)
, [ElapsedTime] nvarchar(100)
, [Error] nvarchar(1000)
, [OracleResult1] nvarchar(1000)
, [OracleElapsedTime] nvarchar(100)
, [OracleError] nvarchar(1000)
);
GO
ALTER TABLE [LRS_IsConnectedData] ADD CONSTRAINT [PK_IsConnectedData] PRIMARY KEY ([Id]);
GO


-- LocatePointAlongGeom
CREATE TABLE [LRS_LocatePointAlongGeomData] (
 [Id] int IDENTITY (1,1) NOT NULL
, [InputGeom] nvarchar(1000) NOT NULL
, [Measure] float NOT NULL
, [ExpectedPoint] nvarchar(1000) NOT NULL
, [ObtainedPoint] nvarchar(1000)
, [Result] nvarchar(50)
, [ElapsedTime] nvarchar(100)
, [Error] nvarchar(1000)
, [OracleResult1] nvarchar(1000)
, [OracleElapsedTime] nvarchar(100)
, [OracleError] nvarchar(1000)
);
GO
ALTER TABLE [LRS_LocatePointAlongGeomData] ADD CONSTRAINT [PK_LocatePointAlongGeomData] PRIMARY KEY ([Id]);
GO


-- MergeGeometrySegments
CREATE TABLE [LRS_MergeGeometrySegmentsData] (
 [Id] int IDENTITY (1,1) NOT NULL
, [InputGeom1] nvarchar(1000) NOT NULL
, [InputGeom2] nvarchar(1000) NOT NULL
, [ExpectedGeom] nvarchar(1000) NOT NULL
, [ObtainedGeom] nvarchar(1000)
, [Result] nvarchar(50)
, [ElapsedTime] nvarchar(100)
, [Error] nvarchar(1000)
, [OracleResult1] nvarchar(1000)
, [OracleElapsedTime] nvarchar(100)
, [OracleError] nvarchar(1000)
);
GO
ALTER TABLE [LRS_MergeGeometrySegmentsData] ADD CONSTRAINT [PK_MergeGeometrySegmentsData] PRIMARY KEY ([Id]);
GO


-- PopulateGeometryMeasures
CREATE TABLE [LRS_PopulateGeometryMeasuresData] (
 [Id] int IDENTITY (1,1) NOT NULL
, [InputGeom] nvarchar(1000) NOT NULL
, [StartMeasure] float
, [EndMeasure] float
, [ExpectedGeom] nvarchar(1000) NOT NULL
, [ObtainedGeom] nvarchar(1000)
, [Result] nvarchar(50)
, [ElapsedTime] nvarchar(100)
, [Error] nvarchar(1000)
, [OracleResult1] nvarchar(1000)
, [OracleElapsedTime] nvarchar(100)
, [OracleError] nvarchar(1000)
);
GO
ALTER TABLE [LRS_PopulateGeometryMeasuresData] ADD CONSTRAINT [PK_PopulateGeometryMeasuresData] PRIMARY KEY ([Id]);
GO

-- ResetMeasureTest
CREATE TABLE [LRS_ResetMeasureData] (
  [Id] int IDENTITY (1,1) NOT NULL
, [InputGeom] nvarchar(1000) NOT NULL
, [ExpectedGeom] nvarchar(1000) NOT NULL
, [ObtainedGeom] nvarchar(1000)
, [Result] nvarchar(50)
, [ElapsedTime] nvarchar(100)
, [Error] nvarchar(1000)
);
GO
ALTER TABLE [LRS_ResetMeasureData] ADD CONSTRAINT [PK_ResetMeasureData] PRIMARY KEY ([Id]);
GO


-- ReverseLinearGeometry
CREATE TABLE [LRS_ReverseLinearGeometryData] (
 [Id] int IDENTITY (1,1) NOT NULL
, [InputGeom] nvarchar(1000) NOT NULL
, [ExpectedGeom] nvarchar(1000) NOT NULL
, [ObtainedGeom] nvarchar(1000)
, [Result] nvarchar(50)
, [ElapsedTime] nvarchar(100)
, [Error] nvarchar(1000)
, [OracleResult1] nvarchar(1000)
, [OracleElapsedTime] nvarchar(100)
, [OracleError] nvarchar(1000)
);
GO
ALTER TABLE [LRS_ReverseLinearGeometryData] ADD CONSTRAINT [PK_ReverseLinearGeometryData] PRIMARY KEY ([Id]);
GO


-- SplitGeometrySegment
CREATE TABLE [LRS_SplitGeometrySegmentData] (
 [Id] int IDENTITY (1,1) NOT NULL
, [InputGeom] nvarchar(1000) NOT NULL
, [Measure] float NOT NULL
, [ExpectedGeom1] nvarchar(1000) NOT NULL
, [ExpectedGeom2] nvarchar(1000) NOT NULL
, [ObtainedGeom1] nvarchar(1000)
, [ObtainedGeom2] nvarchar(1000)
, [Result] nvarchar(50)
, [ElapsedTime] nvarchar(100)
, [Error] nvarchar(1000)
, [OracleResult1] nvarchar(1000)
, [OracleResult2] nvarchar(1000)
, [OracleElapsedTime] nvarchar(100)
, [OracleError] nvarchar(1000)
);
GO
ALTER TABLE [LRS_SplitGeometrySegmentData] ADD CONSTRAINT [PK_SplitGeometrySegmentData] PRIMARY KEY ([Id]);
GO