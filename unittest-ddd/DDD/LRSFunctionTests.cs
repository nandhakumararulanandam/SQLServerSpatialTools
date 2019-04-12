using System;
using System.Data.SqlServerCe;
using System.IO;
using Dapper;
using Microsoft.SqlServer.Types;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SQLSpatialTools.Functions.LRS;
using SQLSpatialTools.UnitTests.Extension;
using SQLSpatialTools.Utility;
using System.Globalization;
using System.Linq;

namespace SQLSpatialTools.UnitTests.DDD
{
    [TestClass]
    public class LRSFunctionTests : BaseUnitTest
    {
        private static SqlCeConnection dbConnection;
        private static DataManipulator dataManipulator;
        private static OracleConnector oracleConnector;
        private static string connectionString;
        private const string DatabaseFile = "SpatialTestData.sdf";
        private const string SchemaFile = "Dataset\\CreateDBSchema.sql";

        [ClassInitialize()]
        public static void Intialize(TestContext testContext)
        {
            if (File.Exists(DatabaseFile))
                File.Delete(DatabaseFile);

            connectionString = string.Format(CultureInfo.CurrentCulture, "Data Source=|DataDirectory|\\{0}", DatabaseFile);
            dbConnection = new SqlCeConnection(connectionString);

            dataManipulator = new DataManipulator(connectionString, SchemaFile, dbConnection, new TestLogger(testContext));
            dataManipulator.CreateDB();
            dbConnection.Open();
            dataManipulator.LoadDataSet();
            oracleConnector = OracleConnector.GetInstance();
        }

        [TestMethod]
        public void ClipGeometrySegmentTest()
        {
            Logger.LogLine("Clip Geometry Segments Tests");
            var dataSet = dbConnection.Query<LRSDataSet.ClipGeometrySegmentData>(LRSDataSet.ClipGeometrySegmentData.SelectQuery);

            if (dataSet == null || !dataSet.Any())
                Logger.Log("No test cases found");

            int testIterator = 1;
            foreach (var test in dataSet)
            {
                Logger.LogLine("Executing test {0}", testIterator);
                try
                {
                    var inputGeomSegment = test.InputGeom.GetGeom();
                    Logger.Log("Input Geom : {0}", inputGeomSegment.ToString());
                    Logger.Log("Start Measure : {0}", test.StartMeasure);
                    Logger.Log("End Measure : {0}", test.EndMeasure);
                    Logger.Log("Input Geom : {0}", inputGeomSegment.ToString());
                    Logger.Log("Expected Geom : {0}", test.ExpectedResult1);

                    MSSQLTimer.Restart();
                    // OSS Function Execution
                    test.ObtainedResult1 = Geometry.ClipGeometrySegment(inputGeomSegment, test.StartMeasure, test.EndMeasure, test.Tolerance)?.ToString().TrimNullValue();
                    MSSQLTimer.Stop();
                    Logger.Log("Obtained Result: {0}", test.ObtainedResult1);

                    #region Run against Oracle

                    OracleTimer.Restart();
                    // Oracle Function Execution
                    oracleConnector.DoClipGeometrySegment(test);
                    OracleTimer.Stop();

                    #endregion

                }
                catch (Exception ex)
                {
                    test.Result = "Failed";
                    test.Error = ex.Message;
                    Logger.LogError(ex);
                }

                // Update test results in DB
                UpdateTestResults(test, LRSDataSet.ClipGeometrySegmentData.TableName);

                Logger.Log("Test Result : {0}", test.Result);
                testIterator++;
            }
        }

        [TestMethod]
        public void GetEndMeasureTest()
        {
            Logger.LogLine("Clip Geometry Segments Tests");
            var dataSet = dbConnection.Query<LRSDataSet.GetEndMeasureData>(LRSDataSet.GetEndMeasureData.SelectQuery);

            if (dataSet == null || !dataSet.Any())
                Logger.Log("No test cases found");

            int testIterator = 1;
            foreach (var test in dataSet)
            {
                Logger.LogLine("Executing test {0}", testIterator);
                try
                {
                    var inputGeomSegment = test.InputGeom.GetGeom();
                    test.ExpectedResult1 = test.ExpectedResult1;
                    Logger.Log("Input Geom : {0}", inputGeomSegment.ToString());
                    Logger.Log("Expected Geom : {0}", test.ExpectedResult1);

                    MSSQLTimer.Restart();
                    // OSS Function Execution
                    test.ObtainedResult1 = Geometry.GetEndMeasure(inputGeomSegment).ToString();
                    MSSQLTimer.Stop();
                    Logger.Log("Obtained Result: {0}", test.ObtainedResult1);

                    #region Run against Oracle

                    OracleTimer.Restart();
                    // Oracle Function Execution
                    oracleConnector.DoGetEndMeasure(test);
                    OracleTimer.Stop();

                    #endregion

                }
                catch (Exception ex)
                {
                    test.Result = "Failed";
                    test.Error = ex.Message;
                    Logger.LogError(ex);
                }

                // Update test results in DB
                UpdateTestResults(test, LRSDataSet.GetEndMeasureData.TableName);

                Logger.Log("Test Result : {0}", test.Result);
                testIterator++;
            }
        }

        [TestMethod]
        public void GetStartMeasureTest()
        {
            Logger.LogLine("Clip Geometry Segments Tests");
            var dataSet = dbConnection.Query<LRSDataSet.GetStartMeasureData>(LRSDataSet.GetStartMeasureData.SelectQuery);

            if (dataSet == null || !dataSet.Any())
                Logger.Log("No test cases found");

            int testIterator = 1;
            foreach (var test in dataSet)
            {
                Logger.LogLine("Executing test {0}", testIterator);
                try
                {
                    var inputGeomSegment = test.InputGeom.GetGeom();
                    test.ExpectedResult1 = test.ExpectedResult1;
                    Logger.Log("Input Geom : {0}", inputGeomSegment.ToString());
                    Logger.Log("Expected Geom : {0}", test.ExpectedResult1);

                    MSSQLTimer.Restart();
                    // OSS Function Execution
                    test.ObtainedResult1 = Geometry.GetStartMeasure(inputGeomSegment).ToString();
                    MSSQLTimer.Stop();
                    Logger.Log("Obtained Result: {0}", test.ObtainedResult1);

                    #region Run against Oracle

                    OracleTimer.Restart();
                    // Oracle Function Execution
                    oracleConnector.DoGetStartMeasure(test);
                    OracleTimer.Stop();

                    #endregion

                }
                catch (Exception ex)
                {
                    test.Result = "Failed";
                    test.Error = ex.Message;
                    Logger.LogError(ex);
                }

                // Update test results in DB
                UpdateTestResults(test, LRSDataSet.GetStartMeasureData.TableName);

                Logger.Log("Test Result : {0}", test.Result);
                testIterator++;
            }
        }

        [TestMethod]
        public void InterpolateBetweenGeomTest()
        {
            Logger.LogLine("IsConnected Tests");
            var dataSet = dbConnection.Query<LRSDataSet.InterpolateBetweenGeomData>(LRSDataSet.InterpolateBetweenGeomData.SelectQuery);

            if (dataSet == null || !dataSet.Any())
                Logger.Log("No test cases found");

            int testIterator = 1;
            foreach (var test in dataSet)
            {
                Logger.LogLine("Executing test {0}", testIterator);
                try
                {
                    var inputGeomSegment1 = test.InputGeom1.GetGeom();
                    var inputGeomSegment2 = test.InputGeom2.GetGeom();
                    test.ExpectedResult1 = test.ExpectedResult1;
                    Logger.LogLine("Input geom 1:{0} geom 2:{1}", inputGeomSegment1.ToString(), inputGeomSegment2.ToString());
                    Logger.Log("Interpolate with a distance of {0}", inputGeomSegment1.ToString(), inputGeomSegment2.ToString(), test.Measure);
                    Logger.LogLine("Expected Result: {0}", test.ExpectedResult1);

                    MSSQLTimer.Restart();
                    // OSS Function Execution
                    test.ObtainedResult1 = Geometry.InterpolateBetweenGeom(inputGeomSegment1, inputGeomSegment2, test.Measure).ToString();
                    MSSQLTimer.Stop();
                    test.SetElapsedTime(MSSQLTimer.Elapsed);
                    Logger.Log("Obtained Point: {0}", test.ObtainedResult1);
                }
                catch (Exception ex)
                {
                    test.Result = "Failed";
                    test.Error = ex.Message;
                    Logger.LogError(ex);
                }

                // Update results to database
                UpdateSqlServerTestResults(test, LRSDataSet.InterpolateBetweenGeomData.TableName);

                Logger.Log("Test Result : {0}", test.Result);
                testIterator++;
            }
        }

        [TestMethod]
        public void IsConnectedTest()
        {
            Logger.LogLine("IsConnected Tests");
            var dataSet = dbConnection.Query<LRSDataSet.IsConnectedData>(LRSDataSet.IsConnectedData.SelectQuery);

            if (dataSet == null || !dataSet.Any())
                Logger.Log("No test cases found");

            int testIterator = 1;
            foreach (var test in dataSet)
            {
                Logger.LogLine("Executing test {0}", testIterator);
                try
                {
                    var inputGeomSegment1 = test.InputGeom1.GetGeom();
                    var inputGeomSegment2 = test.InputGeom2.GetGeom();
                    test.ExpectedResult1 = test.ExpectedResult1;
                    Logger.LogLine("Input geom 1:{0} geom 2:{1}", inputGeomSegment1.ToString(), inputGeomSegment2.ToString());
                    Logger.Log("IsConnected with a tolerance of {0}", inputGeomSegment1.ToString(), inputGeomSegment2.ToString(), test.Tolerance);
                    Logger.LogLine("Expected Result: {0}", test.ExpectedResult1);

                    MSSQLTimer.Restart();
                    // OSS Function Execution
                    test.ObtainedResult1 = Geometry.IsConnected(inputGeomSegment1, inputGeomSegment2, test.Tolerance).ToString();
                    MSSQLTimer.Stop();
                    Logger.Log("Obtained Point: {0}", test.ObtainedResult1);

                    #region Run against Oracle

                    OracleTimer.Restart();
                    // Oracle Function Execution
                    oracleConnector.DoIsConnectedGeomSegmentTest(test);
                    OracleTimer.Stop();

                    #endregion
                }
                catch (Exception ex)
                {
                    test.Result = "Failed";
                    test.Error = ex.Message;
                    Logger.LogError(ex);
                }

                // Update results to database
                UpdateTestResults(test, LRSDataSet.IsConnectedData.TableName);

                Logger.Log("Test Result : {0}", test.Result);
                testIterator++;
            }
        }

        [TestMethod]
        public void LocatePointAlongGeomTest()
        {
            Logger.LogLine("LocatePointAlongGeom Tests");
            var dataSet = dbConnection.Query<LRSDataSet.LocatePointAlongGeomData>(LRSDataSet.LocatePointAlongGeomData.SelectQuery);

            if (dataSet == null || !dataSet.Any())
                Logger.Log("No test cases found");

            int testIterator = 1;
            foreach (var test in dataSet)
            {
                Logger.LogLine("Executing test {0}", testIterator);
                try
                {
                    var inputGeomSegment1 = test.InputGeom.GetGeom();
                    Logger.LogLine("Input geom :{0}", inputGeomSegment1.ToString());
                    Logger.Log("Location point along Geom at a measure of {0}", test.Measure);
                    Logger.LogLine("Expected Result: {0}", test.ExpectedResult1);

                    MSSQLTimer.Restart();
                    // OSS Function Execution
                    test.ObtainedResult1 = Geometry.LocatePointAlongGeom(inputGeomSegment1, test.Measure).ToString();
                    MSSQLTimer.Stop();
                    Logger.Log("Obtained Result: {0}", test.ObtainedResult1);

                    #region Run against Oracle

                    OracleTimer.Restart();
                    // Oracle Function Execution
                    oracleConnector.DoLocatePointAlongGeomTest(test);
                    Logger.Log("Oracle Result: {0}", test.OracleResult1);
                    OracleTimer.Stop();

                    #endregion

                }
                catch (Exception ex)
                {
                    test.Result = "Failed";
                    test.Error = ex.Message;
                    Logger.LogError(ex);
                }

                // Update results to database
                UpdateTestResults(test, LRSDataSet.LocatePointAlongGeomData.TableName);

                Logger.Log("Test Result : {0}", test.Result);
                testIterator++;
            }
        }

        [TestMethod]
        public void MergeGeometrySegmentsTest()
        {
            var dataSet = dbConnection.Query<LRSDataSet.MergeGeometrySegmentsData>(LRSDataSet.MergeGeometrySegmentsData.SelectQuery);

            if (dataSet == null || !dataSet.Any())
                Logger.Log("No test cases found");

            int testIterator = 1;
            foreach (var test in dataSet)
            {
                Logger.LogLine("Executing test {0}", testIterator);
                try
                {
                    var geom1 = test.InputGeom1.GetGeom();
                    var geom2 = test.InputGeom2.GetGeom();
                    var expectedGeom = test.ExpectedResult1.GetGeom();

                    Logger.LogLine("Input geom 1:{0}", geom1);
                    Logger.Log("Input geom 2:{0}", geom2);
                    Logger.LogLine("Expected Result: {0}", expectedGeom);

                    MSSQLTimer.Restart();
                    // OSS Function Execution
                    
                    test.ObtainedResult1 = Geometry.MergeGeometrySegments(geom1, geom2, test.Tolerance).ToString();
                    MSSQLTimer.Stop();
                    Logger.Log("Obtained Result : {0}", test.ObtainedResult1);

                    #region Run against Oracle

                    OracleTimer.Restart();
                    // Oracle Function Execution
                    oracleConnector.DoMergeGeomTest(test);
                    OracleTimer.Stop();

                    #endregion

                }
                catch (Exception ex)
                {
                    test.Result = "Failed";
                    test.Error = ex.Message;
                    Logger.LogError(ex);
                }

                // Update results to database
                UpdateTestResults(test, LRSDataSet.MergeGeometrySegmentsData.TableName);

                Logger.Log("Test Result : {0}", test.Result);
                testIterator++;
            }
        }

        [TestMethod]
        public void OffsetGeometrySegmentTest()
        {
            Logger.LogLine("Offset Geometry Segments Tests");
            var dataSet = dbConnection.Query<LRSDataSet.OffsetGeometrySegmentData>(LRSDataSet.OffsetGeometrySegmentData.SelectQuery);

            if (dataSet == null || !dataSet.Any())
                Logger.Log("No test cases found");

            int testIterator = 1;
            foreach (var test in dataSet)
            {
                Logger.LogLine("Executing test {0}", testIterator);
                try
                {
                    var inputGeomSegment = test.InputGeom.GetGeom();
                    Logger.Log("Input Geom : {0}", inputGeomSegment.ToString());
                    Logger.Log("Start Measure : {0}", test.StartMeasure);
                    Logger.Log("End Measure : {0}", test.EndMeasure);
                    Logger.Log("Offset : {0}", test.Offset);
                    Logger.Log("Tolerance : {0}", test.Tolerance);
                    Logger.Log("Expected Geom : {0}", test.ExpectedResult1);

                    MSSQLTimer.Restart();
                    // OSS Function Execution
                    test.ObtainedResult1 = Geometry.OffsetGeometrySegment(inputGeomSegment, test.StartMeasure, test.EndMeasure, test.Offset, test.Tolerance)?.ToString().TrimNullValue();
                    MSSQLTimer.Stop();
                    Logger.Log("Obtained Result: {0}", test.ObtainedResult1);

                    #region Run against Oracle

                    OracleTimer.Restart();
                    // Oracle Function Execution
                    oracleConnector.DoOffsetGeometrySegment(test);
                    OracleTimer.Stop();

                    #endregion

                }
                catch (Exception ex)
                {
                    test.Result = "Failed";
                    test.Error = ex.Message;
                    Logger.LogError(ex);
                }

                // Update test results in DB
                UpdateTestResults(test, LRSDataSet.OffsetGeometrySegmentData.TableName);

                Logger.Log("Test Result : {0}", test.Result);
                testIterator++;
            }
        }

        [TestMethod]
        public void PopulateGeometryMeasuresTest()
        {
            Logger.LogLine("PopulateGeometryMeasures Tests");
            var dataSet = dbConnection.Query<LRSDataSet.PopulateGeometryMeasuresData>(LRSDataSet.PopulateGeometryMeasuresData.SelectQuery);

            if (dataSet == null || !dataSet.Any())
                Logger.Log("No test cases found");

            int testIterator = 1;
            foreach (var test in dataSet)
            {
                Logger.LogLine("Executing test {0}", testIterator);
                try
                {
                    var geom = test.InputGeom.GetGeom();
                    Logger.LogLine("Input geom: {0}", geom);
                    Logger.LogLine("Expected Result: {0}", test.ExpectedResult1);

                    MSSQLTimer.Restart();
                    // OSS Function Execution
                    test.ObtainedResult1 = Geometry.PopulateGeometryMeasures(geom, test.StartMeasure, test.EndMeasure).ToString();
                    MSSQLTimer.Stop();
                    Logger.Log("Obtained Result: {0}", test.ObtainedResult1);

                    #region Run against Oracle

                    OracleTimer.Restart();
                    // Oracle Function Execution
                    oracleConnector.DoPopulateMeasuresTest(test);
                    OracleTimer.Stop();
                    Logger.Log("Oracle Result: {0}", test.OracleResult1);

                    #endregion

                }
                catch (Exception ex)
                {
                    test.Result = "Failed";
                    test.Error = ex.Message;
                    Logger.LogError(ex);
                }

                // Update results to database
                UpdateTestResults(test, LRSDataSet.PopulateGeometryMeasuresData.TableName);

                Logger.Log("Test Result : {0}", test.Result);
                testIterator++;
            }
        }

        [TestMethod]
        public void ResetMeasureTest()
        {
            Logger.LogLine("Reset Measure Tests");
            var dataSet = dbConnection.Query<LRSDataSet.ResetMeasureData>(LRSDataSet.ResetMeasureData.SelectQuery);

            if (dataSet == null || !dataSet.Any())
                Logger.Log("No test cases found");

            int testIterator = 1;
            foreach (var test in dataSet)
            {
                Logger.LogLine("Executing test {0}", testIterator);
                try
                {
                    var inputGeomSegment = test.InputGeom.GetGeom();
                    var expectedGeomSegment = test.ExpectedResult1.GetGeom();
                    Logger.Log("Input Geom : {0}", inputGeomSegment.ToString());
                    Logger.Log("Expected Geom : {0}", expectedGeomSegment.ToString());

                    MSSQLTimer.Restart();
                    // OSS Function Execution
                    test.ObtainedResult1 = Geometry.ResetMeasure(inputGeomSegment).ToString();
                    MSSQLTimer.Stop();
                    Logger.Log("Obtained Result: {0}", test.ObtainedResult1);
                }
                catch (Exception ex)
                {
                    test.Result = "Failed";
                    test.Error = ex.Message;
                    Logger.LogError(ex);
                }

                // Update results to database
                UpdateSqlServerTestResults(test, LRSDataSet.ResetMeasureData.TableName);

                Logger.Log("Test Result : {0}", test.Result);
                testIterator++;
            }
        }

        [TestMethod]
        public void ReverseLinearGeometryTest()
        {
            Logger.LogLine("ReverseLinearGeometry Tests");
            var dataSet = dbConnection.Query<LRSDataSet.ReverseLinearGeometryData>(LRSDataSet.ReverseLinearGeometryData.SelectQuery);

            if (dataSet == null || !dataSet.Any())
                Logger.Log("No test cases found");

            int testIterator = 1;
            foreach (var test in dataSet)
            {
                Logger.LogLine("Executing test {0}", testIterator);
                try
                {
                    var geom = test.InputGeom.GetGeom();

                    Logger.LogLine("Input geom: {0}", geom);
                    Logger.LogLine("Expected Result: {0}", test.ExpectedResult1);

                    MSSQLTimer.Restart();
                    // OSS Function Execution
                    test.ObtainedResult1 = Geometry.ReverseLinearGeometry(geom).ToString();
                    MSSQLTimer.Stop();
                    Logger.Log("Obtained Result: {0}", test.ObtainedResult1);

                    #region Run against Oracle

                    OracleTimer.Restart();
                    // Oracle Function Execution
                    oracleConnector.DoReverseLinearGeomTest(test);
                    OracleTimer.Stop();

                    #endregion

                }
                catch (Exception ex)
                {
                    test.Result = "Failed";
                    test.Error = ex.Message;
                    Logger.LogError(ex);
                }

                // Update results to database
                UpdateTestResults(test, LRSDataSet.ReverseLinearGeometryData.TableName);

                Logger.Log("Test Result : {0}", test.Result);
                testIterator++;
            }
        }

        [TestMethod]
        public void SplitGeometrySegmentTest()
        {
            Logger.LogLine("SplitGeometrySegment Tests");
            var dataSet = dbConnection.Query<LRSDataSet.SplitGeometrySegmentData>(LRSDataSet.SplitGeometrySegmentData.SelectQuery);

            if (dataSet == null || !dataSet.Any())
                Logger.Log("No test cases found");

            int testIterator = 1;
            foreach (var test in dataSet)
            {
                Logger.LogLine("Executing test {0}", testIterator);
                try
                {
                    var geom = test.InputGeom.GetGeom();
                    var expectedGeom1 = test.ExpectedResult1.GetGeom();
                    var expectedGeom2 = test.ExpectedResult2.GetGeom();

                    Logger.LogLine("Splitting Input geom: {0} at a measure of : {1}", geom, test.Measure);
                    Logger.Log("Expected Split Geom Segment 1: {0}", expectedGeom1);
                    Logger.Log("Expected Split Geom Segment 2: {0}", expectedGeom2);

                    MSSQLTimer.Restart();
                    // OSS Function Execution
                    Geometry.SplitGeometrySegment(geom, test.Measure, out SqlGeometry obtainedGeom1, out SqlGeometry obtainedGeom2);
                    MSSQLTimer.Stop();

                    test.ObtainedResult1 = obtainedGeom1.ToString();
                    test.ObtainedResult2 = obtainedGeom2.ToString();
                    Logger.LogLine("Obtained Result1: {0}", test.ObtainedResult1);
                    Logger.Log("Obtained Result2: {0}", test.ObtainedResult2);

                    #region Run against Oracle

                    OracleTimer.Restart();
                    // Oracle Function Execution
                    oracleConnector.DoSplitGeometrySegmentTest(test);
                    OracleTimer.Stop();

                    #endregion
                }
                catch (Exception ex)
                {
                    test.Result = "Failed";
                    test.Error = ex.Message;
                    Logger.LogError(ex);
                }

                // Update results to database
                UpdateTestResults(test, LRSDataSet.SplitGeometrySegmentData.TableName);

                Logger.Log("Test Result : {0}", test.Result);
                testIterator++;
            }
        }

        [TestMethod]
        public void ValidateLRSGeometryTest()
        {
            Logger.LogLine("Clip Geometry Segments Tests");
            var dataSet = dbConnection.Query<LRSDataSet.ValidateLRSGeometryData>(LRSDataSet.ValidateLRSGeometryData.SelectQuery);
            int testIterator = 1;
            foreach (var test in dataSet)
            {
                Logger.LogLine("Executing test {0}", testIterator);
                try
                {
                    var inputGeomSegment = test.InputGeom.GetGeom();
                    test.ExpectedResult1 = test.ExpectedResult1;
                    Logger.Log("Input Geom : {0}", inputGeomSegment.ToString());
                    Logger.Log("Expected Geom : {0}", test.ExpectedResult1);

                    MSSQLTimer.Restart();
                    // OSS Function Execution
                    test.ObtainedResult1 = Geometry.ValidateLRSGeometry(inputGeomSegment).ToString();
                    MSSQLTimer.Stop();
                    Logger.Log("Obtained Result: {0}", test.ObtainedResult1);

                    #region Run against Oracle

                    OracleTimer.Restart();
                    // Oracle Function Execution
                    oracleConnector.ValidateLRSGeometry(test);
                    OracleTimer.Stop();

                    #endregion

                }
                catch (Exception ex)
                {
                    test.Result = "Failed";
                    test.Error = ex.Message;
                    Logger.LogError(ex);
                }

                // Update test results in DB
                UpdateTestResults(test, LRSDataSet.ValidateLRSGeometryData.TableName);

                Logger.Log("Test Result : {0}", test.Result);
                testIterator++;
            }
            if (testIterator == 1)
                Logger.Log("No test cases found");
        }

        [ClassCleanup()]
        public static void Cleanup()
        {
            if (dbConnection != null)
                dbConnection.Close();
        }

        /// <summary>
        /// Update test results specific to SQL Server alone
        /// </summary>
        /// <param name="test"></param>
        /// <param name="tableName"></param>
        private void UpdateSqlServerTestResults(LRSDataSet.BaseDataSet test, string tableName)
        {
            test.SetElapsedTime(MSSQLTimer.Elapsed);

            test.ObtainedResult1 = test.ObtainedResult1.TrimNullValue();
            test.ExpectedResult1 = test.ExpectedResult1.TrimNullValue();

            test.Result = test.ObtainedResult1.Compare(test.ExpectedResult1).GetResult();
            dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(tableName, nameof(test.ObtainedResult1), test.ObtainedResult1));
            dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(tableName, nameof(test.Result), test.Result));
            dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(tableName, nameof(test.ElapsedTime), test.ElapsedTime));
            if (!string.IsNullOrWhiteSpace(test.Error))
                dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(tableName, nameof(test.Error), test.Error));

            dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(tableName, nameof(test.ExecutionTime), test.ExecutionTime));
        }

        /// <summary>
        /// Update Test Results.
        /// </summary>
        /// <param name="test"></param>
        /// <param name="tableName"></param>
        /// <param name="obtainedResults"></param>
        private void UpdateTestResults(LRSDataSet.BaseDataSet test, string tableName)
        {
            test.SetElapsedTime(MSSQLTimer.Elapsed);
            test.SetOracleElapsedTime(OracleTimer.Elapsed);

            test.ObtainedResult1 = test.ObtainedResult1?.TrimNullValue();
            test.ExpectedResult1 = test.ExpectedResult1?.TrimNullValue();
            test.OracleResult1 = test.OracleResult1?.TrimDecimalPoints()?.TrimNullValue();

            test.OutputComparison = test.ObtainedResult1.Compare(test.OracleResult1);
            test.Result = test.ObtainedResult1.Compare(test.ExpectedResult1).GetResult();

            dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(tableName, nameof(test.ObtainedResult1), test.ObtainedResult1));
            dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(tableName, nameof(test.ExpectedResult1), test.ExpectedResult1));
            dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(tableName, nameof(test.OracleResult1), test.OracleResult1));

            if (!string.IsNullOrEmpty(test.ObtainedResult2))
            {
                test.ObtainedResult2 = test.ObtainedResult1?.TrimNullValue();
                test.ExpectedResult2 = test.ExpectedResult1?.TrimNullValue();
                test.OracleResult2 = test.OracleResult1?.TrimDecimalPoints()?.TrimNullValue();

                test.OutputComparison = test.OutputComparison && test.ObtainedResult2.Compare(test.OracleResult2);
                test.Result = (test.Result.Equals("Passed", StringComparison.CurrentCulture) && test.ObtainedResult2.Compare(test.ExpectedResult2)).GetResult();

                dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(tableName, nameof(test.ObtainedResult2), test.ObtainedResult2));
                dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(tableName, nameof(test.ExpectedResult2), test.ExpectedResult2));
                dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(tableName, nameof(test.OracleResult2), test.OracleResult2));
            }

            // comparison of result with expected against obtained results from MSSQL OSS extension functions
            dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(tableName, nameof(test.Result), test.Result));
            // comparison of obtained results from MSSQL OSS extension functions against results from competitive Oracle functions.
            dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(tableName, nameof(test.OutputComparison), test.OutputComparison));

            dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(tableName, nameof(test.ElapsedTime), test.ElapsedTime));
            dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(tableName, nameof(test.OracleElapsedTime), test.OracleElapsedTime));
            dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(tableName, nameof(test.OracleQuery), test.OracleQuery.EscapeQueryString()));

            if (!string.IsNullOrWhiteSpace(test.Error))
                dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(tableName, nameof(test.Error), test.Error));

            if (!string.IsNullOrWhiteSpace(test.OracleError))
                dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(tableName, nameof(test.OracleError), test.OracleError));

            dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(tableName, nameof(test.ExecutionTime), test.ExecutionTime));
        }
    }
}
