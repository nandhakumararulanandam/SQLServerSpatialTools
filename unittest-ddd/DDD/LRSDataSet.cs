using System;

namespace SQLSpatialTools.UnitTests.DDD
{
    public class LRSDataSet
    {
        public class ClipGeometrySegmentData : BaseDataSet
        {
            public const short ParamCount = 4;
            public const string TableName = "LRS_ClipGeometrySegmentData";
            public const string DataFile = "Dataset\\LRS\\ClipGeometrySegment.data";
            public static readonly string SelectQuery = string.Format("SELECT [Id], [InputGeom], [StartMeasure], [EndMeasure], [ExpectedGeom] FROM [{0}];", TableName);
            public static readonly string InsertQuery = string.Format("INSERT INTO [{0}] ([InputGeom], [StartMeasure], [EndMeasure], [ExpectedGeom]) VALUES (N'[0]', [1], [2], N'[3]');", TableName);
            public override string ResultUpdateQuery => string.Format(UpdateResultQuery, TableName, Result, Id);
            public override string ErrorUpdateQuery => string.Format(UpdateErrorQuery, TableName, Error, Id);
            public string GetTargetUpdateQuery(string fieldName, object fieldValue) { return GetTargetUpdateQuery(TableName, fieldName, fieldValue); }

            public string InputGeom { get; set; }
            public double StartMeasure { get; set; }
            public double EndMeasure { get; set; }
            public string ExpectedGeom { get; set; }
            public string ObtainedGeom { get; set; }
        }

        public class GetEndMeasureData : BaseDataSet
        {
            public const short ParamCount = 2;
            public const string TableName = "LRS_GetEndMeasureData";
            public const string DataFile = "Dataset\\LRS\\GetEndMeasure.data";
            public static readonly string SelectQuery = string.Format("SELECT [Id], [InputGeom], [ExpectedEndMeasure] FROM [{0}];", TableName);
            public static readonly string InsertQuery = string.Format("INSERT INTO [{0}] ([InputGeom], [ExpectedEndMeasure]) VALUES (N'[0]', [1]);", TableName);
            public override string ResultUpdateQuery => string.Format(UpdateResultQuery, TableName, Result, Id);
            public override string ErrorUpdateQuery => string.Format(UpdateErrorQuery, TableName, Error, Id);
            public string GetTargetUpdateQuery(string fieldName, object fieldValue) { return GetTargetUpdateQuery(TableName, fieldName, fieldValue); }

            public string InputGeom { get; set; }
            public double ExpectedEndMeasure { get; set; }
            public double ObtainedEndMeasure { get; set; }
        }

        public class GetStartMeasureData : BaseDataSet
        {
            public const short ParamCount = 2;
            public const string TableName = "LRS_GetStartMeasureData";
            public const string DataFile = "Dataset\\LRS\\GetStartMeasure.data";
            public static readonly string SelectQuery = string.Format("SELECT [Id], [InputGeom], [ExpectedStartMeasure] FROM [{0}];", TableName);
            public static readonly string InsertQuery = string.Format("INSERT INTO [{0}] ([InputGeom], [ExpectedStartMeasure]) VALUES (N'[0]', [1]);", TableName);
            public override string ResultUpdateQuery => string.Format(UpdateResultQuery, TableName, Result, Id);
            public override string ErrorUpdateQuery => string.Format(UpdateErrorQuery, TableName, Error, Id);
            public string GetTargetUpdateQuery(string fieldName, object fieldValue) { return GetTargetUpdateQuery(TableName, fieldName, fieldValue); }

            public string InputGeom { get; set; }
            public double ExpectedStartMeasure { get; set; }
            public double ObtainedStartMeasure { get; set; }
        }

        public class InterpolateBetweenGeomData : BaseDataSet
        {
            public const short ParamCount = 4;
            public const string TableName = "LRS_InterpolateBetweenGeomData";
            public const string DataFile = "Dataset\\LRS\\InterpolateBetweenGeom.data";
            public static readonly string SelectQuery = string.Format("SELECT [Id], [InputGeom1], [InputGeom2], [Measure], [ExpectedPoint] FROM [{0}];", TableName);
            public static readonly string InsertQuery = string.Format("INSERT INTO [{0}] ([InputGeom1], [InputGeom2], [Measure], [ExpectedPoint]) VALUES (N'[0]', N'[1]', [2], N'[3]');", TableName);
            public override string ResultUpdateQuery => string.Format(UpdateResultQuery, TableName, Result, Id);
            public override string ErrorUpdateQuery => string.Format(UpdateErrorQuery, TableName, Error, Id);
            public string GetTargetUpdateQuery(string fieldName, object fieldValue) { return GetTargetUpdateQuery(TableName, fieldName, fieldValue); }

            public string InputGeom1 { get; set; }
            public string InputGeom2 { get; set; }
            public double Measure { get; set; }
            public string ExpectedPoint { get; set; }
            public string ObtainedPoint { get; set; }
        }

        public class IsConnectedData : BaseDataSet
        {
            public const short ParamCount = 4;
            public const string TableName = "LRS_IsConnectedData";
            public const string DataFile = "Dataset\\LRS\\IsConnected.data";
            public static readonly string SelectQuery = string.Format("SELECT [Id], [InputGeom1], [InputGeom2], [Tolerance], [Expected] FROM [{0}];", TableName);
            public static readonly string InsertQuery = string.Format("INSERT INTO [{0}] ([InputGeom1], [InputGeom2], [Tolerance], [Expected]) VALUES (N'[0]', N'[1]', [2], [3]);", TableName);
            public override string ResultUpdateQuery => string.Format(UpdateResultQuery, TableName, Result, Id);
            public override string ErrorUpdateQuery => string.Format(UpdateErrorQuery, TableName, Error, Id);
            public string GetTargetUpdateQuery(string fieldName, object fieldValue) { return GetTargetUpdateQuery(TableName, fieldName, fieldValue); }

            public string InputGeom1 { get; set; }
            public string InputGeom2 { get; set; }
            public double Tolerance { get; set; }
            public bool Expected { get; set; }
            public bool Obtained { get; set; }
        }

        public class LocatePointAlongGeomData : BaseDataSet
        {
            public const short ParamCount = 3;
            public const string TableName = "LRS_LocatePointAlongGeomData";
            public const string DataFile = "Dataset\\LRS\\LocatePointAlongGeom.data";
            public static readonly string SelectQuery = string.Format("SELECT [Id], [InputGeom], [Measure], [ExpectedPoint] FROM [{0}];", TableName);
            public static readonly string InsertQuery = string.Format("INSERT INTO [{0}] ([InputGeom], [Measure], [ExpectedPoint]) VALUES (N'[0]', N'[1]', N'[2]');", TableName);
            public override string ResultUpdateQuery => string.Format(UpdateResultQuery, TableName, Result, Id);
            public override string ErrorUpdateQuery => string.Format(UpdateErrorQuery, TableName, Error, Id);
            public string GetTargetUpdateQuery(string fieldName, object fieldValue) { return GetTargetUpdateQuery(TableName, fieldName, fieldValue); }

            public string InputGeom { get; set; }
            public double Measure { get; set; }
            public string ExpectedPoint { get; set; }
            public string ObtainedPoint { get; set; }
        }

        public class MergeGeometrySegmentsData : BaseDataSet
        {
            public const short ParamCount = 3;
            public const string TableName = "LRS_MergeGeometrySegmentsData";
            public const string DataFile = "Dataset\\LRS\\MergeGeometrySegments.data";
            public static readonly string SelectQuery = string.Format("SELECT [Id], [InputGeom1], [InputGeom2], [ExpectedGeom] FROM [{0}];", TableName);
            public static readonly string InsertQuery = string.Format("INSERT INTO [{0}] ([InputGeom1], [InputGeom2], [ExpectedGeom]) VALUES (N'[0]', N'[1]', N'[2]');", TableName);
            public override string ResultUpdateQuery => string.Format(UpdateResultQuery, TableName, Result, Id);
            public override string ErrorUpdateQuery => string.Format(UpdateErrorQuery, TableName, Error, Id);
            public string GetTargetUpdateQuery(string fieldName, object fieldValue) { return GetTargetUpdateQuery(TableName, fieldName, fieldValue); }

            public string InputGeom1 { get; set; }
            public string InputGeom2 { get; set; }
            public string ExpectedGeom { get; set; }
            public string ObtainedGeom { get; set; }
        }

        public class PopulateGeometryMeasuresData : BaseDataSet
        {
            public const short ParamCount = 4;
            public const string TableName = "LRS_PopulateGeometryMeasuresData";
            public const string DataFile = "Dataset\\LRS\\PopulateGeometryMeasures.data";
            public static readonly string SelectQuery = string.Format("SELECT [Id], [InputGeom], [StartMeasure], [EndMeasure], [ExpectedGeom] FROM [{0}];", TableName);
            public static readonly string InsertQuery = string.Format("INSERT INTO [{0}] ([InputGeom], [StartMeasure], [EndMeasure], [ExpectedGeom]) VALUES (N'[0]', [1], [2], N'[3]');", TableName);
            public override string ResultUpdateQuery => string.Format(UpdateResultQuery, TableName, Result, Id);
            public override string ErrorUpdateQuery => string.Format(UpdateErrorQuery, TableName, Error, Id);
            public string GetTargetUpdateQuery(string fieldName, object fieldValue) { return GetTargetUpdateQuery(TableName, fieldName, fieldValue); }

            public string InputGeom { get; set; }
            public double? StartMeasure { get; set; }
            public double? EndMeasure { get; set; }
            public string ExpectedGeom { get; set; }
            public string ObtainedGeom { get; set; }
        }

        public class ResetMeasureData : BaseDataSet
        {
            public const short ParamCount = 2;
            public const string TableName = "LRS_ResetMeasureData";
            public const string DataFile = "Dataset\\LRS\\ResetMeasure.data";
            public static readonly string SelectQuery = string.Format("SELECT [Id], [InputGeom], [ExpectedGeom] FROM [{0}];", TableName);
            public static readonly string InsertQuery = string.Format("INSERT INTO [{0}] ([InputGeom], [ExpectedGeom]) VALUES (N'[0]', N'[1]');", TableName);
            public override string ResultUpdateQuery => string.Format(UpdateResultQuery, TableName, Result, Id);
            public override string ErrorUpdateQuery => string.Format(UpdateErrorQuery, TableName, Error, Id);
            public string GetTargetUpdateQuery(string fieldName, object fieldValue) { return GetTargetUpdateQuery(TableName, fieldName, fieldValue); }

            public string InputGeom { get; set; }
            public string ExpectedGeom { get; set; }
            public string ObtainedGeom { get; set; }
        }


        public class ReverseLinearGeometryData : BaseDataSet
        {
            public const short ParamCount = 2;
            public const string TableName = "LRS_ReverseLinearGeometryData";
            public const string DataFile = "Dataset\\LRS\\ReverseLinearGeometry.data";
            public static readonly string SelectQuery = string.Format("SELECT [Id], [InputGeom], [ExpectedGeom] FROM [{0}];", TableName);
            public static readonly string InsertQuery = string.Format("INSERT INTO [{0}] ([InputGeom], [ExpectedGeom]) VALUES (N'[0]', N'[1]');", TableName);
            public override string ResultUpdateQuery => string.Format(UpdateResultQuery, TableName, Result, Id);
            public override string ErrorUpdateQuery => string.Format(UpdateErrorQuery, TableName, Error, Id);
            public string GetTargetUpdateQuery(string fieldName, object fieldValue) { return GetTargetUpdateQuery(TableName, fieldName, fieldValue); }

            public string InputGeom { get; set; }
            public string ExpectedGeom { get; set; }
            public string ObtainedGeom { get; set; }
        }

        public class SplitGeometrySegmentData : BaseDataSet
        {
            public const short ParamCount = 4;
            public const string TableName = "LRS_SplitGeometrySegmentData";
            public const string DataFile = "Dataset\\LRS\\SplitGeometrySegment.data";
            public static readonly string SelectQuery = string.Format("SELECT [Id], [InputGeom], [Measure], [ExpectedGeom1], [ExpectedGeom2] FROM [{0}];", TableName);
            public static readonly string InsertQuery = string.Format("INSERT INTO [{0}] ([InputGeom], [Measure], [ExpectedGeom1], [ExpectedGeom2]) VALUES (N'[0]', [1], N'[2]', N'[3]');", TableName);
            public override string ResultUpdateQuery => string.Format(UpdateResultQuery, TableName, Result, Id);
            public override string ErrorUpdateQuery => string.Format(UpdateErrorQuery, TableName, Error, Id);
            public string GetTargetUpdateQuery(string fieldName, object fieldValue) { return GetTargetUpdateQuery(TableName, fieldName, fieldValue); }

            public string InputGeom { get; set; }
            public double Measure { get; set; }
            public string ExpectedGeom1 { get; set; }
            public string ExpectedGeom2 { get; set; }
            public string ObtainedGeom1 { get; set; }
            public string ObtainedGeom2 { get; set; }
        }

        abstract public class BaseDataSet
        {
            public const string UpdateResultQuery = "UPDATE [{0}] Set [Result] = N'{1}' WHERE [ID] = {2};";
            public const string UpdateErrorQuery = "UPDATE [{0}] Set [Error] = N'{1}' WHERE [ID] = {2};";
            public const string UpdateTargetQuery = "UPDATE [{0}] Set [{1}] = {2} WHERE [ID] = {3};";
            public abstract string ResultUpdateQuery { get; }
            public abstract string ErrorUpdateQuery { get; }

            public int Id { get; set; }
            public string Result { get; set; }
            public string Error { get; set; }
            public string ElapsedTime { get; private set; }

            internal void SetElapsedTime(TimeSpan elapsed)
            {
                ElapsedTime = elapsed.ToString();
            }

            protected string GetTargetUpdateQuery(string tableName, string fieldName, object fieldValue)
            {
                return string.Format(UpdateTargetQuery, tableName, fieldName, GetFieldValue(fieldValue), Id);
            }

            private string GetFieldValue(object fieldValue)
            {
                var type = fieldValue.GetType();

                if (type == typeof(int) || type == typeof(float) || type == typeof(double))
                    return fieldValue.ToString();
                else if (type == typeof(bool))
                    return (bool)fieldValue ? "1" : "0";
                return string.Format("N'{0}'", fieldValue.ToString());
            }
        }
    }
}
