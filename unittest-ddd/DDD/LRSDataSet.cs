﻿using System;
using System.Globalization;

namespace SQLSpatialTools.UnitTests.DDD
{
    public class LRSDataSet
    {
        public class ClipGeometrySegmentData : BaseDataSet
        {
            public const short ParamCount = 6;
            public const string TableName = "LRS_ClipGeometrySegmentData";
            public const string DataFile = "Dataset\\LRS\\ClipGeometrySegment.data";
            public static readonly string SelectQuery = string.Format(CultureInfo.CurrentCulture, "SELECT [Id], [InputGeom], [StartMeasure], [EndMeasure], [Tolerance], [ExpectedResult1], [Comments] FROM [{0}];", TableName);
            public static readonly string InsertQuery = string.Format(CultureInfo.CurrentCulture, "INSERT INTO [{0}] ([InputGeom], [StartMeasure], [EndMeasure], [Tolerance], [ExpectedResult1], [Comments]) VALUES (N'[0]', [1], [2], [3], N'[4]' ,N'[5]');", TableName);
            public string GetTargetUpdateQuery(string fieldName, object fieldValue) { return GetTargetUpdateQuery(TableName, fieldName, fieldValue); }

            public string InputGeom { get; set; }
            public double StartMeasure { get; set; }
            public double EndMeasure { get; set; }
            public double Tolerance { get; set; }
        }

        public class GetEndMeasureData : BaseDataSet
        {
            public const short ParamCount = 3;
            public const string TableName = "LRS_GetEndMeasureData";
            public const string DataFile = "Dataset\\LRS\\GetEndMeasure.data";
            public static readonly string SelectQuery = string.Format(CultureInfo.CurrentCulture, "SELECT [Id], [InputGeom], [ExpectedResult1], [Comments] FROM [{0}];", TableName);
            public static readonly string InsertQuery = string.Format(CultureInfo.CurrentCulture, "INSERT INTO [{0}] ([InputGeom], [ExpectedResult1],[Comments]) VALUES (N'[0]', [1], N'[2]');", TableName);
            public string GetTargetUpdateQuery(string fieldName, object fieldValue) { return GetTargetUpdateQuery(TableName, fieldName, fieldValue); }

            public string InputGeom { get; set; }
        }

        public class GetStartMeasureData : BaseDataSet
        {
            public const short ParamCount = 3;
            public const string TableName = "LRS_GetStartMeasureData";
            public const string DataFile = "Dataset\\LRS\\GetStartMeasure.data";
            public static readonly string SelectQuery = string.Format(CultureInfo.CurrentCulture, "SELECT [Id], [InputGeom], [ExpectedResult1], [Comments] FROM [{0}];", TableName);
            public static readonly string InsertQuery = string.Format(CultureInfo.CurrentCulture, "INSERT INTO [{0}] ([InputGeom], [ExpectedResult1],[Comments]) VALUES (N'[0]', [1], N'[2]');", TableName);
            public string GetTargetUpdateQuery(string fieldName, object fieldValue) { return GetTargetUpdateQuery(TableName, fieldName, fieldValue); }

            public string InputGeom { get; set; }
        }

        public class InterpolateBetweenGeomData : BaseDataSet
        {
            public const short ParamCount = 5;
            public const string TableName = "LRS_InterpolateBetweenGeomData";
            public const string DataFile = "Dataset\\LRS\\InterpolateBetweenGeom.data";
            public static readonly string SelectQuery = string.Format(CultureInfo.CurrentCulture, "SELECT [Id], [InputGeom1], [InputGeom2], [Measure], [ExpectedResult1], [Comments] FROM [{0}];", TableName);
            public static readonly string InsertQuery = string.Format(CultureInfo.CurrentCulture, "INSERT INTO [{0}] ([InputGeom1], [InputGeom2], [Measure], [ExpectedResult1], [Comments]) VALUES (N'[0]', N'[1]', [2], N'[3]', N'[4]');", TableName);
            public string GetTargetUpdateQuery(string fieldName, object fieldValue) { return GetTargetUpdateQuery(TableName, fieldName, fieldValue); }

            public string InputGeom1 { get; set; }
            public string InputGeom2 { get; set; }
            public double Measure { get; set; }
        }

        public class IsConnectedData : BaseDataSet
        {
            public const short ParamCount = 5;
            public const string TableName = "LRS_IsConnectedData";
            public const string DataFile = "Dataset\\LRS\\IsConnected.data";
            public static readonly string SelectQuery = string.Format(CultureInfo.CurrentCulture, "SELECT [Id], [InputGeom1], [InputGeom2], [Tolerance], [ExpectedResult1], [Comments] FROM [{0}];", TableName);
            public static readonly string InsertQuery = string.Format(CultureInfo.CurrentCulture, "INSERT INTO [{0}] ([InputGeom1], [InputGeom2], [Tolerance], [ExpectedResult1], [Comments]) VALUES (N'[0]', N'[1]', [2], [3], N'[4]');", TableName);
            public string GetTargetUpdateQuery(string fieldName, object fieldValue) { return GetTargetUpdateQuery(TableName, fieldName, fieldValue); }

            public string InputGeom1 { get; set; }
            public string InputGeom2 { get; set; }
            public double Tolerance { get; set; }
        }

        public class LocatePointAlongGeomData : BaseDataSet
        {
            public const short ParamCount = 4;
            public const string TableName = "LRS_LocatePointAlongGeomData";
            public const string DataFile = "Dataset\\LRS\\LocatePointAlongGeom.data";
            public static readonly string SelectQuery = string.Format(CultureInfo.CurrentCulture, "SELECT [Id], [InputGeom], [Measure], [ExpectedResult1], [Comments] FROM [{0}];", TableName);
            public static readonly string InsertQuery = string.Format(CultureInfo.CurrentCulture, "INSERT INTO [{0}] ([InputGeom], [Measure], [ExpectedResult1], [Comments]) VALUES (N'[0]', N'[1]', N'[2]', N'[3]');", TableName);
            public string GetTargetUpdateQuery(string fieldName, object fieldValue) { return GetTargetUpdateQuery(TableName, fieldName, fieldValue); }

            public string InputGeom { get; set; }
            public double Measure { get; set; }
        }

        public class MergeGeometrySegmentsData : BaseDataSet
        {
            public const short ParamCount = 5;
            public const string TableName = "LRS_MergeGeometrySegmentsData";
            public const string DataFile = "Dataset\\LRS\\MergeGeometrySegments.data";
            public static readonly string SelectQuery = string.Format(CultureInfo.CurrentCulture, "SELECT [Id], [InputGeom1], [InputGeom2], [ExpectedResult1], [Tolerance], [Comments] FROM [{0}];", TableName);
            public static readonly string InsertQuery = string.Format(CultureInfo.CurrentCulture, "INSERT INTO [{0}] ([InputGeom1], [InputGeom2], [ExpectedResult1], [Tolerance], [Comments]) VALUES (N'[0]', N'[1]', N'[2]', [3], N'[4]');", TableName);
            public string GetTargetUpdateQuery(string fieldName, object fieldValue) { return GetTargetUpdateQuery(TableName, fieldName, fieldValue); }

            public string InputGeom1 { get; set; }
            public string InputGeom2 { get; set; }
            public double Tolerance { get; set; }
        }

        public class MergeAndResetGeometrySegmentsData : MergeGeometrySegmentsData
        {
            public new const string TableName = "LRS_MergeAndResetGeometrySegmentsData";
            public new const string DataFile = "Dataset\\LRS\\MergeAndResetGeometrySegments.data";
            public new static readonly string SelectQuery = string.Format(CultureInfo.CurrentCulture, "SELECT [Id], [InputGeom1], [InputGeom2], [ExpectedResult1], [Tolerance], [Comments] FROM [{0}];", TableName);
            public new static readonly string InsertQuery = string.Format(CultureInfo.CurrentCulture, "INSERT INTO [{0}] ([InputGeom1], [InputGeom2], [ExpectedResult1], [Tolerance], [Comments]) VALUES (N'[0]', N'[1]', N'[2]', [3], N'[4]');", TableName);
        }

        public class OffsetGeometrySegmentData : BaseDataSet
        {
            public const short ParamCount = 7;
            public const string TableName = "LRS_OffsetGeometrySegmentData";
            public const string DataFile = "Dataset\\LRS\\OffsetGeometrySegment.data";
            public static readonly string SelectQuery = string.Format(CultureInfo.CurrentCulture, "SELECT [Id], [InputGeom], [StartMeasure], [EndMeasure], [Offset], [Tolerance], [ExpectedResult1], [Comments] FROM [{0}];", TableName);
            public static readonly string InsertQuery = string.Format(CultureInfo.CurrentCulture, "INSERT INTO [{0}] ([InputGeom], [StartMeasure], [EndMeasure], [Offset], [Tolerance], [ExpectedResult1],[Comments]) VALUES (N'[0]', [1], [2], [3], [4], N'[5]', N'[6]');", TableName);
            public string GetTargetUpdateQuery(string fieldName, object fieldValue) { return GetTargetUpdateQuery(TableName, fieldName, fieldValue); }

            public string InputGeom { get; set; }
            public double StartMeasure { get; set; }
            public double EndMeasure { get; set; }
            public double Offset { get; set; }
            public double Tolerance { get; set; }
        }

        public class PopulateGeometryMeasuresData : BaseDataSet
        {
            public const short ParamCount = 5;
            public const string TableName = "LRS_PopulateGeometryMeasuresData";
            public const string DataFile = "Dataset\\LRS\\PopulateGeometryMeasures.data";
            public static readonly string SelectQuery = string.Format(CultureInfo.CurrentCulture, "SELECT [Id], [InputGeom], [StartMeasure], [EndMeasure], [ExpectedResult1], [Comments] FROM [{0}];", TableName);
            public static readonly string InsertQuery = string.Format(CultureInfo.CurrentCulture, "INSERT INTO [{0}] ([InputGeom], [StartMeasure], [EndMeasure], [ExpectedResult1], [Comments]) VALUES (N'[0]', [1], [2], N'[3]',N'[4]');", TableName);
            public string GetTargetUpdateQuery(string fieldName, object fieldValue) { return GetTargetUpdateQuery(TableName, fieldName, fieldValue); }

            public string InputGeom { get; set; }
            public double? StartMeasure { get; set; }
            public double? EndMeasure { get; set; }
        }

        public class ResetMeasureData : BaseDataSet
        {
            public const short ParamCount = 3;
            public const string TableName = "LRS_ResetMeasureData";
            public const string DataFile = "Dataset\\LRS\\ResetMeasure.data";
            public static readonly string SelectQuery = string.Format(CultureInfo.CurrentCulture, "SELECT [Id], [InputGeom], [ExpectedResult1], [Comments] FROM [{0}];", TableName);
            public static readonly string InsertQuery = string.Format(CultureInfo.CurrentCulture, "INSERT INTO [{0}] ([InputGeom], [ExpectedResult1], [Comments]) VALUES (N'[0]', N'[1]', N'[2]');", TableName);
            public string GetTargetUpdateQuery(string fieldName, object fieldValue) { return GetTargetUpdateQuery(TableName, fieldName, fieldValue); }

            public string InputGeom { get; set; }
        }


        public class ReverseLinearGeometryData : BaseDataSet
        {
            public const short ParamCount = 3;
            public const string TableName = "LRS_ReverseLinearGeometryData";
            public const string DataFile = "Dataset\\LRS\\ReverseLinearGeometry.data";
            public static readonly string SelectQuery = string.Format(CultureInfo.CurrentCulture, "SELECT [Id], [InputGeom], [ExpectedResult1], [Comments] FROM [{0}];", TableName);
            public static readonly string InsertQuery = string.Format(CultureInfo.CurrentCulture, "INSERT INTO [{0}] ([InputGeom], [ExpectedResult1], [Comments]) VALUES (N'[0]', N'[1]', N'[2]');", TableName);
            public string GetTargetUpdateQuery(string fieldName, object fieldValue) { return GetTargetUpdateQuery(TableName, fieldName, fieldValue); }

            public string InputGeom { get; set; }
        }

        public class SplitGeometrySegmentData : BaseDataSet
        {
            public const short ParamCount = 5;
            public const string TableName = "LRS_SplitGeometrySegmentData";
            public const string DataFile = "Dataset\\LRS\\SplitGeometrySegment.data";
            public static readonly string SelectQuery = string.Format(CultureInfo.CurrentCulture, "SELECT [Id], [InputGeom], [Measure], [ExpectedResult1], [ExpectedResult2], [Comments] FROM [{0}];", TableName);
            public static readonly string InsertQuery = string.Format(CultureInfo.CurrentCulture, "INSERT INTO [{0}] ([InputGeom], [Measure], [ExpectedResult1], [ExpectedResult2], [Comments]) VALUES (N'[0]', [1], N'[2]', N'[3]', N'[4]');", TableName);
            public string GetTargetUpdateQuery(string fieldName, object fieldValue) { return GetTargetUpdateQuery(TableName, fieldName, fieldValue); }

            public string InputGeom { get; set; }
            public double Measure { get; set; }
        }

        public class ValidateLRSGeometryData : BaseDataSet
        {
            public const short ParamCount = 3;
            public const string TableName = "LRS_ValidateLRSGeometryData";
            public const string DataFile = "Dataset\\LRS\\ValidateLRSGeometryTest.data";
            public static readonly string SelectQuery = string.Format(CultureInfo.CurrentCulture, "SELECT [Id], [InputGeom], [ExpectedResult1], [Comments] FROM [{0}];", TableName);
            public static readonly string InsertQuery = string.Format(CultureInfo.CurrentCulture, "INSERT INTO [{0}] ([InputGeom], [ExpectedResult1], [Comments]) VALUES (N'[0]', N'[1]', N'[2]');", TableName);
            public string GetTargetUpdateQuery(string fieldName, object fieldValue) { return GetTargetUpdateQuery(TableName, fieldName, fieldValue); }

            public string InputGeom { get; set; }
        }

        public class OverallResult
        {
            public const string TableName = "_LRS_OverallResult";
            public static readonly string SelectQuery = string.Format(CultureInfo.CurrentCulture, "SELECT [Id], [FunctionName], [TotalCount], [PassCount], [FailCount] FROM [{0}];", TableName);
            private static readonly string InsertPart1 = string.Format(CultureInfo.CurrentCulture, "INSERT INTO[{0}] ([FunctionName]) ", TableName);
            private static readonly string InsertPart2 = "VALUES (N'{0}');";
            public static readonly string InsertQuery = string.Concat(InsertPart1, InsertPart2);

            private static readonly string UpdatePart1 = string.Format(CultureInfo.CurrentCulture, "UPDATE [{0}] SET ", TableName);
            private static readonly string UpdatePart2 = "[TotalCount] = {0}, [PassCount] = {1}, [FailCount] = {2} WHERE [FunctionName] = '{3}';";
            public static readonly string UpdateQuery = string.Concat(UpdatePart1, UpdatePart2);

            public string FunctionName { get; set; }
            public int TotalCount { get; set; }
            public int PassCount { get; set; }
            public int FailCount { get; set; }
        }

        abstract public class BaseDataSet
        {
            public const string UpdateResultQuery = "UPDATE [{0}] Set [Result] = N'{1}' WHERE [ID] = {2};";
            public const string UpdateOracleQuery = "UPDATE [{0}] Set [Query] = N'{1}' WHERE [ID] = {2};";
            public const string UpdateErrorQuery = "UPDATE [{0}] Set [Error] = N'{1}' WHERE [ID] = {2};";
            public const string UpdateTargetQuery = "UPDATE [{0}] Set [{1}] = {2} WHERE [ID] = {3};";

            public string ExpectedResult1 { get; set; }
            public string ExpectedResult2 { get; set; }
            public string SqlObtainedResult1 { get; set; }
            public string SqlObtainedResult2 { get; set; }

            public int Id { get; set; }
            public string Result { get; set; }
            public string SqlError { get; set; }
            public string SqlElapsedTime { get; private set; }
            public string OracleResult1 { get; set; }
            public bool OutputComparison1 { get; set; }
            public bool OutputComparison2 { get; set; }
            public string OracleResult2 { get; set; }
            public string OracleElapsedTime { get; private set; }
            public string OracleError { get; set; }
            public string Comments { get; set; }

            public string ExecutionTime => DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);

            public string OracleQuery { get; set; }
            internal void SetElapsedTime(TimeSpan elapsed)
            {
                SqlElapsedTime = elapsed.ToString();
            }

            internal void SetOracleElapsedTime(TimeSpan elapsed)
            {
                OracleElapsedTime = elapsed.ToString();
            }

            internal string GetTargetUpdateQuery(string tableName, string fieldName, object fieldValue)
            {
                return string.Format(CultureInfo.CurrentCulture, UpdateTargetQuery, tableName, fieldName, GetFieldValue(fieldValue), Id);
            }

            internal string UpdateOverallStatusCountQuery(string tablename, int count, int passCount, int failCount)
            {
                return string.Format(CultureInfo.CurrentCulture, OverallResult.UpdateQuery, count, passCount, failCount, tablename);
            }

            internal string InsertOverallStatusQuery(string tablename)
            {
                return string.Format(CultureInfo.CurrentCulture, OverallResult.InsertQuery, tablename);
            }

            private string GetFieldValue(object fieldValue)
            {
                if (fieldValue == null)
                    return string.Empty;
                var type = fieldValue.GetType();

                if (type == typeof(int) || type == typeof(float) || type == typeof(double))
                    return fieldValue.ToString();
                else if (type == typeof(bool))
                    return (bool)fieldValue ? "1" : "0";
                return string.Format(CultureInfo.CurrentCulture, "N'{0}'", fieldValue.ToString());
            }
        }

        public class SplitGeomResult
        {
            public string Output_1 { get; set; }
            public string Output_2 { get; set; }
        }
        public class MergeAndResetResult
        {
            public string Output_1 { get; set; }
        }
    }
}
