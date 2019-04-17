-- LRS Functions Test Data Table

-- ClipGeometrySegment
CREATE TABLE [LRS_ClipGeometrySegmentData] (
  [Id] int IDENTITY (1,1) NOT NULL
, [InputGeom] nvarchar(1000) NOT NULL
, [StartMeasure] float NOT NULL
, [EndMeasure] float NOT NULL
, [Tolerance] float NOT NULL
, [ExpectedResult1] nvarchar(1000) NOT NULL
, [ObtainedResult1] nvarchar(1000)
, [Result] nvarchar(50)
, [ElapsedTime] nvarchar(100)
, [Error] nvarchar(1000)
, [OracleResult1] nvarchar(1000)
, [OracleElapsedTime] nvarchar(100)
, [OracleError] nvarchar(1000)
, [OutputComparison] bit
, [OracleQuery] nvarchar(1000)
, [ExecutionTime] datetime
, [Comments] nvarchar(1000)
);
GO
ALTER TABLE [LRS_ClipGeometrySegmentData] ADD CONSTRAINT [PK_ClipGeometrySegmentData] PRIMARY KEY ([Id]);
GO


-- Get End Measure
CREATE TABLE [LRS_GetEndMeasureData] (
  [Id] int IDENTITY (1,1) NOT NULL
, [InputGeom] nvarchar(1000) NOT NULL
, [ExpectedResult1] nvarchar(1000) NOT NULL
, [ObtainedResult1] nvarchar(1000)
, [Result] nvarchar(50)
, [ElapsedTime] nvarchar(100)
, [Error] nvarchar(1000)
, [OracleResult1] nvarchar(1000)
, [OracleElapsedTime] nvarchar(100)
, [OracleError] nvarchar(1000)
, [OutputComparison] bit
, [OracleQuery] nvarchar(1000)
, [ExecutionTime] datetime
, [Comments] nvarchar(1000)
);
GO
ALTER TABLE [LRS_GetEndMeasureData] ADD CONSTRAINT [PK_GetEndMeasureData] PRIMARY KEY ([Id]);
GO


-- GetStartMeasure
CREATE TABLE [LRS_GetStartMeasureData] (
  [Id] int IDENTITY (1,1) NOT NULL
, [InputGeom] nvarchar(1000) NOT NULL
, [ExpectedResult1] nvarchar(1000) NOT NULL
, [ObtainedResult1] nvarchar(1000)
, [Result] nvarchar(50)
, [ElapsedTime] nvarchar(100)
, [Error] nvarchar(1000)
, [OracleResult1] nvarchar(1000)
, [OracleElapsedTime] nvarchar(100)
, [OracleError] nvarchar(1000)
, [OutputComparison] bit
, [OracleQuery] nvarchar(1000)
, [ExecutionTime] datetime
, [Comments] nvarchar(1000)
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
, [ExpectedResult1] nvarchar(1000) NOT NULL
, [ObtainedResult1] nvarchar(1000)
, [Result] nvarchar(50)
, [ElapsedTime] nvarchar(100)
, [Error] nvarchar(1000)
, [ExecutionTime] datetime
, [Comments] nvarchar(1000)
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
, [ExpectedResult1] bit NOT NULL
, [ObtainedResult1] bit
, [Result] nvarchar(50)
, [ElapsedTime] nvarchar(100)
, [Error] nvarchar(1000)
, [OracleResult1] nvarchar(1000)
, [OracleElapsedTime] nvarchar(100)
, [OracleError] nvarchar(1000)
, [OutputComparison] bit
, [OracleQuery] nvarchar(1000)
, [ExecutionTime] datetime
, [Comments] nvarchar(1000)
);
GO
ALTER TABLE [LRS_IsConnectedData] ADD CONSTRAINT [PK_IsConnectedData] PRIMARY KEY ([Id]);
GO


-- LocatePointAlongGeom
CREATE TABLE [LRS_LocatePointAlongGeomData] (
 [Id] int IDENTITY (1,1) NOT NULL
, [InputGeom] nvarchar(1000) NOT NULL
, [Measure] float NOT NULL
, [ExpectedResult1] nvarchar(1000) NOT NULL
, [ObtainedResult1] nvarchar(1000)
, [Result] nvarchar(50)
, [ElapsedTime] nvarchar(100)
, [Error] nvarchar(1000)
, [OracleResult1] nvarchar(1000)
, [OracleElapsedTime] nvarchar(100)
, [OracleError] nvarchar(1000)
, [OutputComparison] bit
, [OracleQuery] nvarchar(1000)
, [ExecutionTime] datetime
, [Comments] nvarchar(1000)
);
GO
ALTER TABLE [LRS_LocatePointAlongGeomData] ADD CONSTRAINT [PK_LocatePointAlongGeomData] PRIMARY KEY ([Id]);
GO


-- MergeGeometrySegments
CREATE TABLE [LRS_MergeGeometrySegmentsData] (
 [Id] int IDENTITY (1,1) NOT NULL
, [InputGeom1] nvarchar(1000) NOT NULL
, [InputGeom2] nvarchar(1000) NOT NULL
, [ExpectedResult1] nvarchar(1000) NOT NULL
, [Tolerance] float NOT NULL
, [ObtainedResult1] nvarchar(1000)
, [Result] nvarchar(50)
, [ElapsedTime] nvarchar(100)
, [Error] nvarchar(1000)
, [OracleResult1] nvarchar(1000)
, [OracleElapsedTime] nvarchar(100)
, [OracleError] nvarchar(1000)
, [OutputComparison] bit
, [OracleQuery] nvarchar(1000)
, [ExecutionTime] datetime
, [Comments] nvarchar(1000)
);
GO
ALTER TABLE [LRS_MergeGeometrySegmentsData] ADD CONSTRAINT [PK_MergeGeometrySegmentsData] PRIMARY KEY ([Id]);
GO


-- OffsetGeometrySegment
CREATE TABLE [LRS_OffsetGeometrySegmentData] (
  [Id] int IDENTITY (1,1) NOT NULL
, [InputGeom] nvarchar(1000) NOT NULL
, [StartMeasure] float NOT NULL
, [EndMeasure] float NOT NULL
, [Offset] float NOT NULL
, [Tolerance] float NOT NULL
, [ExpectedResult1] nvarchar(1000) NOT NULL
, [ObtainedResult1] nvarchar(1000)
, [Result] nvarchar(50)
, [ElapsedTime] nvarchar(100)
, [Error] nvarchar(1000)
, [OracleResult1] nvarchar(1000)
, [OracleElapsedTime] nvarchar(100)
, [OracleError] nvarchar(1000)
, [OutputComparison] bit
, [OracleQuery] nvarchar(1000)
, [ExecutionTime] datetime
, [Comments] nvarchar(1000)
);
GO
ALTER TABLE [LRS_OffsetGeometrySegmentData] ADD CONSTRAINT [PK_OffsetGeometrySegmentData] PRIMARY KEY ([Id]);
GO


-- PopulateGeometryMeasures
CREATE TABLE [LRS_PopulateGeometryMeasuresData] (
 [Id] int IDENTITY (1,1) NOT NULL
, [InputGeom] nvarchar(1000) NOT NULL
, [StartMeasure] float
, [EndMeasure] float
, [ExpectedResult1] nvarchar(1000) NOT NULL
, [ObtainedResult1] nvarchar(1000)
, [Result] nvarchar(50)
, [ElapsedTime] nvarchar(100)
, [Error] nvarchar(1000)
, [OracleResult1] nvarchar(1000)
, [OracleElapsedTime] nvarchar(100)
, [OracleError] nvarchar(1000)
, [OutputComparison] bit
, [OracleQuery] nvarchar(1000)
, [ExecutionTime] datetime
, [Comments] nvarchar(1000)
);
GO
ALTER TABLE [LRS_PopulateGeometryMeasuresData] ADD CONSTRAINT [PK_PopulateGeometryMeasuresData] PRIMARY KEY ([Id]);
GO

-- ResetMeasureTest
CREATE TABLE [LRS_ResetMeasureData] (
  [Id] int IDENTITY (1,1) NOT NULL
, [InputGeom] nvarchar(1000) NOT NULL
, [ExpectedResult1] nvarchar(1000) NOT NULL
, [ObtainedResult1] nvarchar(1000)
, [Result] nvarchar(50)
, [ElapsedTime] nvarchar(100)
, [Error] nvarchar(1000)
, [ExecutionTime] datetime
, [Comments] nvarchar(1000)
);
GO
ALTER TABLE [LRS_ResetMeasureData] ADD CONSTRAINT [PK_ResetMeasureData] PRIMARY KEY ([Id]);
GO


-- ReverseLinearGeometry
CREATE TABLE [LRS_ReverseLinearGeometryData] (
 [Id] int IDENTITY (1,1) NOT NULL
, [InputGeom] nvarchar(1000) NOT NULL
, [ExpectedResult1] nvarchar(1000) NOT NULL
, [ObtainedResult1] nvarchar(1000)
, [Result] nvarchar(50)
, [ElapsedTime] nvarchar(100)
, [Error] nvarchar(1000)
, [OracleResult1] nvarchar(1000)
, [OracleElapsedTime] nvarchar(100)
, [OracleError] nvarchar(1000)
, [OutputComparison] bit
, [OracleQuery] nvarchar(1000)
, [ExecutionTime] datetime
, [Comments] nvarchar(1000)
);
GO
ALTER TABLE [LRS_ReverseLinearGeometryData] ADD CONSTRAINT [PK_ReverseLinearGeometryData] PRIMARY KEY ([Id]);
GO


-- SplitGeometrySegment
CREATE TABLE [LRS_SplitGeometrySegmentData] (
 [Id] int IDENTITY (1,1) NOT NULL
, [InputGeom] nvarchar(1000) NOT NULL
, [Measure] float NOT NULL
, [ExpectedResult1] nvarchar(1000) NOT NULL
, [ExpectedResult2] nvarchar(1000) NOT NULL
, [ObtainedResult1] nvarchar(1000)
, [ObtainedResult2] nvarchar(1000)
, [Result] nvarchar(50)
, [ElapsedTime] nvarchar(100)
, [Error] nvarchar(1000)
, [OracleResult1] nvarchar(1000)
, [OracleResult2] nvarchar(1000)
, [OracleElapsedTime] nvarchar(100)
, [OracleError] nvarchar(1000)
, [OutputComparison] bit
, [OracleQuery] nvarchar(1000)
, [ExecutionTime] datetime
, [Comments] nvarchar(1000)
);
GO
ALTER TABLE [LRS_SplitGeometrySegmentData] ADD CONSTRAINT [PK_SplitGeometrySegmentData] PRIMARY KEY ([Id]);
GO

-- Validate LRS Geometry
CREATE TABLE [LRS_ValidateLRSGeometryData] (
  [Id] int IDENTITY (1,1) NOT NULL
, [InputGeom] nvarchar(1000) NOT NULL
, [ExpectedResult1] nvarchar(1000) NOT NULL
, [ObtainedResult1] nvarchar(1000)
, [Result] nvarchar(50)
, [ElapsedTime] nvarchar(100)
, [Error] nvarchar(1000)
, [OracleResult1] nvarchar(1000)
, [OracleElapsedTime] nvarchar(100)
, [OracleError] nvarchar(1000)
, [OutputComparison] bit
, [OracleQuery] nvarchar(1000)
, [ExecutionTime] datetime
, [Comments] nvarchar(1000)
);
GO
ALTER TABLE [LRS_ValidateLRSGeometryData] ADD CONSTRAINT [PK_ValidateLRSGeometryData] PRIMARY KEY ([Id]);
GO