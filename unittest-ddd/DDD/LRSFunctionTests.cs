using System;
using System.Collections.Generic;
using System.Data.SqlServerCe;
using System.IO;
using Dapper;
using Microsoft.SqlServer.Types;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SQLSpatialTools.Functions.LRS;
using SQLSpatialTools.UnitTests.Extension;
using SQLSpatialTools.Utility;

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

            connectionString = string.Format("Data Source=|DataDirectory|\\{0}", DatabaseFile);
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
            int testIterator = 1, testSuccessCount = 1;
            foreach (var test in dataSet)
            {
                Logger.LogLine("Executing test {0}", testIterator);
                try
                {
                    var inputGeomSegment = test.InputGeom.GetGeom();
                    var expectedGeomSegment = test.ExpectedGeom.GetGeom();
                    Logger.Log("Input Geom : {0}", inputGeomSegment.ToString());
                    Logger.Log("Start Measure : {0}", test.StartMeasure);
                    Logger.Log("End Measure : {0}", test.EndMeasure);
                    Logger.Log("Input Geom : {0}", inputGeomSegment.ToString());
                    Logger.Log("Expected Geom : {0}", expectedGeomSegment.ToString());

                    Timer.Restart();
                    // OSS Function Execution
                    var obtainedGeomSegment = Geometry.ClipGeometrySegment(inputGeomSegment, test.StartMeasure, test.EndMeasure);
                    Timer.Stop();
                    test.SetElapsedTime(Timer.Elapsed);
                    dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(nameof(test.ElapsedTime), test.ElapsedTime));

                    test.ObtainedGeom = obtainedGeomSegment.ToString();
                    dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(nameof(test.ObtainedGeom), test.ObtainedGeom));
                    Logger.Log("Obtained Geom : {0}", test.ObtainedGeom);

                    test.Result = obtainedGeomSegment.STEquals(expectedGeomSegment).GetResult();

                    #region Run against Oracle

                    var oracleError = string.Empty;
                    Timer.Restart();
                    // Oracle Function Execution
                    test.OracleResult1 = oracleConnector.DoClipGeometrySegment(test.InputGeom, test.StartMeasure,  test.EndMeasure, ref oracleError);
                    Timer.Stop();
                    test.SetOracleElapsedTime(Timer.Elapsed);
                    dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(nameof(test.OracleElapsedTime), test.OracleElapsedTime));
                    dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(nameof(test.OracleResult1), test.OracleResult1));
                    if (!string.IsNullOrWhiteSpace(oracleError))
                        dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(nameof(test.OracleError), oracleError));

                    #endregion
                }
                catch (Exception ex)
                {
                    test.Result = "Failed";
                    test.Error = ex.Message;
                    dataManipulator.ExecuteQuery(test.ErrorUpdateQuery);
                    Logger.LogError(ex);
                }
                dataManipulator.ExecuteQuery(test.ResultUpdateQuery);
                Logger.Log("Test Result : {0}", test.Result);
                if (test.Result.Equals("Passed")) testSuccessCount++;
                testIterator++;
            }
            if (testIterator == 1)
                Logger.Log("No test cases found");

            SqlAssert.AreEqual(testIterator, testSuccessCount);
        }

        [TestMethod]
        public void GetEndMeasureTest()
        {
            Logger.LogLine("GetEndMeasure Tests");
            var dataSet = dbConnection.Query<LRSDataSet.GetEndMeasureData>(LRSDataSet.GetEndMeasureData.SelectQuery);
            int testIterator = 1, testSuccessCount = 1;
            foreach (var test in dataSet)
            {
                Logger.LogLine("Executing test {0}", testIterator);
                try
                {
                    var inputGeomSegment = test.InputGeom.GetGeom();
                    Logger.Log("Input Geom : {0}", inputGeomSegment.ToString());
                    Logger.Log("Expected End Measure : {0}", test.ExpectedEndMeasure);

                    Timer.Restart();
                    // OSS Function Execution
                    test.ObtainedEndMeasure = (double)Geometry.GetEndMeasure(inputGeomSegment);
                    Timer.Stop();
                    test.SetElapsedTime(Timer.Elapsed);
                    dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(nameof(test.ElapsedTime), test.ElapsedTime));

                    dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(nameof(test.ObtainedEndMeasure), test.ObtainedEndMeasure));
                    Logger.Log("Obtained End Measure : {0}", test.ObtainedEndMeasure);

                    test.Result = (test.ObtainedEndMeasure == test.ExpectedEndMeasure).GetResult();

                    #region Run against Oracle

                    var oracleError = string.Empty;
                    Timer.Restart();
                    // Oracle Function Execution
                    test.OracleResult1 = oracleConnector.DoGetEndMeasure(test.InputGeom, ref oracleError);
                    Timer.Stop();
                    test.SetOracleElapsedTime(Timer.Elapsed);
                    dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(nameof(test.OracleElapsedTime), test.OracleElapsedTime));
                    dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(nameof(test.OracleResult1), test.OracleResult1));
                    if (!string.IsNullOrWhiteSpace(oracleError))
                        dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(nameof(test.OracleError), oracleError));

                    #endregion
                }
                catch (Exception ex)
                {
                    test.Result = "Failed";
                    test.Error = ex.Message;
                    dataManipulator.ExecuteQuery(test.ErrorUpdateQuery);
                    Logger.LogError(ex);
                }
                dataManipulator.ExecuteQuery(test.ResultUpdateQuery);
                Logger.Log("Test Result : {0}", test.Result);
                if (test.Result.Equals("Passed")) testSuccessCount++;
                testIterator++;
            }
            if (testIterator == 1)
                Logger.Log("No test cases found");

            SqlAssert.AreEqual(testIterator, testSuccessCount);
        }

        [TestMethod]
        public void GetStartMeasureTest()
        {
            Logger.LogLine("GetStartMeasure Tests");
            var dataSet = dbConnection.Query<LRSDataSet.GetStartMeasureData>(LRSDataSet.GetStartMeasureData.SelectQuery);
            int testIterator = 1, testSuccessCount = 1;
            foreach (var test in dataSet)
            {
                Logger.LogLine("Executing test {0}", testIterator);
                try
                {
                    var inputGeomSegment = test.InputGeom.GetGeom();
                    Logger.Log("Input Geom : {0}", inputGeomSegment.ToString());
                    Logger.Log("Expected Start Measure : {0}", test.ExpectedStartMeasure);

                    Timer.Restart();
                    // OSS Function Execution
                    test.ObtainedStartMeasure = (double)Geometry.GetStartMeasure(inputGeomSegment);
                    Timer.Stop();
                    test.SetElapsedTime(Timer.Elapsed);
                    dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(nameof(test.ElapsedTime), test.ElapsedTime));

                    dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(nameof(test.ObtainedStartMeasure), test.ObtainedStartMeasure));
                    Logger.Log("Obtained Start Measure : {0}", test.ObtainedStartMeasure);

                    test.Result = (test.ObtainedStartMeasure == test.ExpectedStartMeasure).GetResult();
                    #region Run against Oracle

                    var oracleError = string.Empty;
                    Timer.Restart();
                    // Oracle Function Execution
                    test.OracleResult1 = oracleConnector.DoGetStartMeasure(test.InputGeom, ref oracleError);
                    Timer.Stop();
                    test.SetOracleElapsedTime(Timer.Elapsed);
                    dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(nameof(test.OracleElapsedTime), test.OracleElapsedTime));
                    dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(nameof(test.OracleResult1), test.OracleResult1));
                    if (!string.IsNullOrWhiteSpace(oracleError))
                        dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(nameof(test.OracleError), oracleError));

                    #endregion
                }
                catch (Exception ex)
                {
                    test.Result = "Failed";
                    test.Error = ex.Message;
                    dataManipulator.ExecuteQuery(test.ErrorUpdateQuery);
                    Logger.LogError(ex);
                }
                dataManipulator.ExecuteQuery(test.ResultUpdateQuery);
                Logger.Log("Test Result : {0}", test.Result);
                if (test.Result.Equals("Passed")) testSuccessCount++;
                testIterator++;
            }
            if (testIterator == 1)
                Logger.Log("No test cases found");

            SqlAssert.AreEqual(testIterator, testSuccessCount);
        }

        [TestMethod]
        public void InterpolateBetweenGeomTest()
        {
            Logger.LogLine("IsConnected Tests");
            var dataSet = dbConnection.Query<LRSDataSet.InterpolateBetweenGeomData>(LRSDataSet.InterpolateBetweenGeomData.SelectQuery);
            int testIterator = 1, testSuccessCount = 1;
            foreach (var test in dataSet)
            {
                Logger.LogLine("Executing test {0}", testIterator);
                try
                {
                    var geom1 = test.InputGeom1.GetGeom();
                    var geom2 = test.InputGeom2.GetGeom();

                    Logger.LogLine("Input geom 1:{0} geom 2:{1}", geom1, geom2);
                    Logger.Log("Interpolate with a distance of {0}", geom1, geom2, test.Measure);
                    var expectedPoint = test.ExpectedPoint.GetGeom();
                    Logger.LogLine("Expected Result: {0}", test.ExpectedPoint);

                    Timer.Restart();
                    // OSS Function Execution
                    var obtainedGeom = Geometry.InterpolateBetweenGeom(geom1, geom2, test.Measure);
                    Timer.Stop();
                    test.SetElapsedTime(Timer.Elapsed);
                    dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(nameof(test.ElapsedTime), test.ElapsedTime));

                    test.ObtainedPoint = obtainedGeom.ToString();

                    Logger.Log("Obtained Point: {0}", test.ObtainedPoint);
                    dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(nameof(test.ObtainedPoint), test.ObtainedPoint));

                    test.Result = (obtainedGeom.STEquals(expectedPoint)).GetResult();
                }
                catch (Exception ex)
                {
                    test.Result = "Failed";
                    test.Error = ex.Message;
                    dataManipulator.ExecuteQuery(test.ErrorUpdateQuery);
                    Logger.LogError(ex);
                }

                dataManipulator.ExecuteQuery(test.ResultUpdateQuery);
                Logger.Log("Test Result : {0}", test.Result);
                if (test.Result.Equals("Passed")) testSuccessCount++;
                testIterator++;
            }
            if (testIterator == 1)
                Logger.Log("No test cases found");

            SqlAssert.AreEqual(testIterator, testSuccessCount);
        }

        [TestMethod]
        public void IsConnectedTest()
        {
            Logger.LogLine("IsConnected Tests");
            var dataSet = dbConnection.Query<LRSDataSet.IsConnectedData>(LRSDataSet.IsConnectedData.SelectQuery);
            int testIterator = 1, testSuccessCount = 1;
            foreach (var test in dataSet)
            {
                Logger.LogLine("Executing test {0}", testIterator);
                try
                {
                    var geom1 = test.InputGeom1.GetGeom();
                    var geom2 = test.InputGeom2.GetGeom();

                    Logger.LogLine("Input geom 1:{0} geom 2:{1}", geom1, geom2);
                    Logger.Log("IsConnected with a tolerance of {0}", geom1, geom2, test.Tolerance);
                    Logger.LogLine("Expected Result: {0}", test.Expected);

                    Timer.Restart();
                    // OSS Function Execution
                    test.Obtained = (bool)Geometry.IsConnected(geom1, geom2, test.Tolerance);
                    Timer.Stop();
                    test.SetElapsedTime(Timer.Elapsed);
                    dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(nameof(test.ElapsedTime), test.ElapsedTime));

                    Logger.Log("Obtained Point: {0}", test.Obtained);
                    dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(nameof(test.Obtained), test.Obtained));

                    test.Result = (test.Obtained == test.Expected).GetResult();
                    #region Run against Oracle

                    var oracleError = string.Empty;
                    Timer.Restart();
                    // Oracle Function Execution
                    test.OracleResult1 = oracleConnector.DoIsConnectedGeomSegmentTest(test.InputGeom1, test.InputGeom2, test.Tolerance, ref oracleError);
                    Timer.Stop();
                    test.SetOracleElapsedTime(Timer.Elapsed);
                    dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(nameof(test.OracleElapsedTime), test.OracleElapsedTime));
                    dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(nameof(test.OracleResult1), test.OracleResult1));
                    if (!string.IsNullOrWhiteSpace(oracleError))
                        dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(nameof(test.OracleError), oracleError));

                    #endregion
                }
                catch (Exception ex)
                {
                    test.Result = "Failed";
                    test.Error = ex.Message;
                    dataManipulator.ExecuteQuery(test.ErrorUpdateQuery);
                    Logger.LogError(ex);
                }

                dataManipulator.ExecuteQuery(test.ResultUpdateQuery);
                Logger.Log("Test Result : {0}", test.Result);
                if (test.Result.Equals("Passed")) testSuccessCount++;
                testIterator++;
            }
            if (testIterator == 1)
                Logger.Log("No test cases found");

            SqlAssert.AreEqual(testIterator, testSuccessCount);
        }

        [TestMethod]
        public void LocatePointAlongGeomTest()
        {
            Logger.LogLine("LocatePointAlongGeom Tests");
            var dataSet = dbConnection.Query<LRSDataSet.LocatePointAlongGeomData>(LRSDataSet.LocatePointAlongGeomData.SelectQuery);
            int testIterator = 1, testSuccessCount = 1;
            foreach (var test in dataSet)
            {
                Logger.LogLine("Executing test {0}", testIterator);
                try
                {
                    var geom = test.InputGeom.GetGeom();

                    Logger.LogLine("Input geom :{0}", geom);
                    Logger.Log("Location point along Geom at a measure of {0}", test.Measure);
                    Logger.LogLine("Expected Result: {0}", test.ExpectedPoint);

                    var expectedGeom = test.ExpectedPoint.GetGeom();
                    Timer.Restart();
                    // OSS Function Execution
                    var obtainedGeom = Geometry.LocatePointAlongGeom(geom, test.Measure);
                    Timer.Stop();
                    test.SetElapsedTime(Timer.Elapsed);
                    dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(nameof(test.ElapsedTime), test.ElapsedTime));

                    test.ObtainedPoint = obtainedGeom.ToString();
                    Logger.Log("Obtained Point: {0}", test.ObtainedPoint);
                    dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(nameof(test.ObtainedPoint), test.ObtainedPoint));

                    test.Result = obtainedGeom.STEquals(expectedGeom).GetResult();

                    #region Run against Oracle

                    var oracleError = string.Empty;
                    Timer.Restart();
                    // Oracle Function Execution
                    test.OracleResult1 = oracleConnector.DoLocatePointAlongGeomTest(test.InputGeom, test.Measure, ref oracleError);
                    Timer.Stop();
                    test.SetOracleElapsedTime(Timer.Elapsed);
                    dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(nameof(test.OracleElapsedTime), test.OracleElapsedTime));
                    dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(nameof(test.OracleResult1), test.OracleResult1));
                    if (!string.IsNullOrWhiteSpace(oracleError))
                        dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(nameof(test.OracleError), oracleError));

                    #endregion
                }
                catch (Exception ex)
                {
                    test.Result = "Failed";
                    test.Error = ex.Message;
                    dataManipulator.ExecuteQuery(test.ErrorUpdateQuery);
                    Logger.LogError(ex);
                }

                dataManipulator.ExecuteQuery(test.ResultUpdateQuery);
                Logger.Log("Test Result : {0}", test.Result);
                if (test.Result.Equals("Passed")) testSuccessCount++;
                testIterator++;
            }
            if (testIterator == 1)
                Logger.Log("No test cases found");

            SqlAssert.AreEqual(testIterator, testSuccessCount);
        }

        [TestMethod]
        public void MergeGeometrySegmentsTest()
        {
            Logger.LogLine("MergeGeometrySegments Tests");
            var dataSet = dbConnection.Query<LRSDataSet.MergeGeometrySegmentsData>(LRSDataSet.MergeGeometrySegmentsData.SelectQuery);
            int testIterator = 1, testSuccessCount = 1;
            foreach (var test in dataSet)
            {
                Logger.LogLine("Executing test {0}", testIterator);
                try
                {
                    var geom1 = test.InputGeom1.GetGeom();
                    var geom2 = test.InputGeom2.GetGeom();
                    var expectedGeom = test.ExpectedGeom.GetGeom();

                    Logger.LogLine("Input geom 1:{0}", geom1);
                    Logger.Log("Input geom 2:{0}", geom2);
                    Logger.LogLine("Expected Result: {0}", test.ExpectedGeom);

                    Timer.Restart();
                    // OSS Function Execution
                    var obtainedGeom = Geometry.MergeGeometrySegments(geom1, geom2);
                    Timer.Stop();
                    test.SetElapsedTime(Timer.Elapsed);
                    dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(nameof(test.ElapsedTime), test.ElapsedTime));

                    test.ObtainedGeom = obtainedGeom.ToString();
                    Logger.Log("Obtained Point: {0}", test.ObtainedGeom);
                    dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(nameof(test.ObtainedGeom), test.ObtainedGeom.TrimNullValue()));

                    if (!obtainedGeom.STIsValid())
                    {
                        test.Result = "Passed";
                        test.Error = "Obtained geom is invalid; hence cannot compare";
                        dataManipulator.ExecuteQuery(test.ErrorUpdateQuery);
                    }
                    else
                    {
                        test.Result = obtainedGeom.STEquals(expectedGeom).GetResult();
                    }

                    #region Run against Oracle

                    var oracleError = string.Empty;
                    Timer.Restart();
                    // Oracle Function Execution
                    test.OracleResult1 = oracleConnector.DoMergeGeomTest(test.InputGeom1, test.InputGeom2, 0.5, ref oracleError);
                    Timer.Stop();
                    test.SetOracleElapsedTime(Timer.Elapsed);
                    dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(nameof(test.OracleElapsedTime), test.OracleElapsedTime));
                    dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(nameof(test.OracleResult1), test.OracleResult1));
                    if(!string.IsNullOrWhiteSpace(oracleError))
                        dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(nameof(test.OracleError), oracleError));

                    #endregion
                }
                catch (Exception ex)
                {
                    test.Result = "Failed";
                    test.Error = ex.Message;
                    dataManipulator.ExecuteQuery(test.ErrorUpdateQuery);
                    Logger.LogError(ex);
                }

                dataManipulator.ExecuteQuery(test.ResultUpdateQuery);
                Logger.Log("Test Result : {0}", test.Result);
                if (test.Result.Equals("Passed")) testSuccessCount++;
                testIterator++;
            }
            if (testIterator == 1)
                Logger.Log("No test cases found");

            SqlAssert.AreEqual(testIterator, testSuccessCount);
        }

        [TestMethod]
        public void PopulateGeometryMeasuresTest()
        {
            Logger.LogLine("PopulateGeometryMeasures Tests");
            var dataSet = dbConnection.Query<LRSDataSet.PopulateGeometryMeasuresData>(LRSDataSet.PopulateGeometryMeasuresData.SelectQuery);
            int testIterator = 1, testSuccessCount = 1;
            foreach (var test in dataSet)
            {
                Logger.LogLine("Executing test {0}", testIterator);
                try
                {
                    var geom = test.InputGeom.GetGeom();
                    var expectedGeom = test.ExpectedGeom.GetGeom();

                    Logger.LogLine("Input geom: {0}", geom);
                    Logger.LogLine("Expected Result: {0}", test.ExpectedGeom);

                    Timer.Restart();
                    // OSS Function Execution
                    var obtainedGeom = Geometry.PopulateGeometryMeasures(geom, test.StartMeasure, test.EndMeasure);
                    Timer.Stop();
                    test.SetElapsedTime(Timer.Elapsed);
                    dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(nameof(test.ElapsedTime), test.ElapsedTime));

                    test.ObtainedGeom = obtainedGeom.ToString();
                    Logger.Log("Obtained Geom: {0}", test.ObtainedGeom);
                    dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(nameof(test.ObtainedGeom), test.ObtainedGeom.TrimNullValue()));

                    test.Result = obtainedGeom.STEqualsMeasure(expectedGeom).GetResult();

                    #region Run against Oracle

                    var oracleError = string.Empty;
                    Timer.Restart();
                    // Oracle Function Execution
                    test.OracleResult1 = oracleConnector.DoPopulateMeasuresTest(test.InputGeom, test.StartMeasure, test.EndMeasure, ref oracleError);
                    Timer.Stop();
                    test.SetOracleElapsedTime(Timer.Elapsed);
                    dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(nameof(test.OracleElapsedTime), test.OracleElapsedTime));
                    dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(nameof(test.OracleResult1), test.OracleResult1));
                    if (!string.IsNullOrWhiteSpace(oracleError))
                        dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(nameof(test.OracleError), oracleError));

                    #endregion
                }
                catch (Exception ex)
                {
                    test.Result = "Failed";
                    test.Error = ex.Message;
                    dataManipulator.ExecuteQuery(test.ErrorUpdateQuery);
                    Logger.LogError(ex);
                }

                dataManipulator.ExecuteQuery(test.ResultUpdateQuery);
                Logger.Log("Test Result : {0}", test.Result);
                if (test.Result.Equals("Passed")) testSuccessCount++;
                testIterator++;
            }
            if (testIterator == 1)
                Logger.Log("No test cases found");

            SqlAssert.AreEqual(testIterator, testSuccessCount);
        }

        [TestMethod]
        public void ResetMeasureTest()
        {
            Logger.LogLine("Reset Measure Tests");
            var dataSet = dbConnection.Query<LRSDataSet.ResetMeasureData>(LRSDataSet.ResetMeasureData.SelectQuery);
            int testIterator = 1, testSuccessCount = 1;
            foreach (var test in dataSet)
            {
                Logger.LogLine("Executing test {0}", testIterator);
                try
                {
                    var inputGeomSegment = test.InputGeom.GetGeom();
                    var expectedGeomSegment = test.ExpectedGeom.GetGeom();
                    Logger.Log("Input Geom : {0}", inputGeomSegment.ToString());
                    Logger.Log("Expected Geom : {0}", expectedGeomSegment.ToString());

                    Timer.Restart();
                    // OSS Function Execution
                    var obtainedGeomSegment = Geometry.ResetMeasure(inputGeomSegment);
                    Timer.Stop();
                    test.SetElapsedTime(Timer.Elapsed);
                    dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(nameof(test.ElapsedTime), test.ElapsedTime));

                    test.ObtainedGeom = obtainedGeomSegment.ToString();
                    test.ExpectedGeom = expectedGeomSegment.ToString();
                    dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(nameof(test.ObtainedGeom), test.ObtainedGeom));
                    Logger.Log("Obtained Geom : {0}", test.ObtainedGeom);

                    test.Result = obtainedGeomSegment.STEqualsMeasure(expectedGeomSegment).GetResult();
                    // we just empty out measures; so comparison with Oracle will be just a overhead.
                }
                catch (Exception ex)
                {
                    test.Result = "Failed";
                    test.Error = ex.Message;
                    dataManipulator.ExecuteQuery(test.ErrorUpdateQuery);
                    Logger.LogError(ex);
                }
                dataManipulator.ExecuteQuery(test.ResultUpdateQuery);
                Logger.Log("Test Result : {0}", test.Result);
                if (test.Result.Equals("Passed")) testSuccessCount++;
                testIterator++;
            }
            if (testIterator == 1)
                Logger.Log("No test cases found");

            SqlAssert.AreEqual(testIterator, testSuccessCount);
        }

        [TestMethod]
        public void ReverseLinearGeometryTest()
        {
            Logger.LogLine("ReverseLinearGeometry Tests");
            var dataSet = dbConnection.Query<LRSDataSet.ReverseLinearGeometryData>(LRSDataSet.ReverseLinearGeometryData.SelectQuery);
            int testIterator = 1, testSuccessCount = 1;
            foreach (var test in dataSet)
            {
                Logger.LogLine("Executing test {0}", testIterator);
                try
                {
                    var geom = test.InputGeom.GetGeom();
                    var expectedGeom = test.ExpectedGeom.GetGeom();

                    Logger.LogLine("Input geom: {0}", geom);
                    Logger.LogLine("Expected Result: {0}", test.ExpectedGeom);

                    Timer.Restart();
                    // OSS Function Execution
                    var obtainedGeom = Geometry.ReverseLinearGeometry(geom);
                    Timer.Stop();
                    test.SetElapsedTime(Timer.Elapsed);
                    dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(nameof(test.ElapsedTime), test.ElapsedTime));

                    test.ObtainedGeom = obtainedGeom.ToString();
                    Logger.Log("Obtained Geom: {0}", test.ObtainedGeom);
                    dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(nameof(test.ObtainedGeom), test.ObtainedGeom));

                    test.Result = obtainedGeom.STEquals(expectedGeom).GetResult();

                    #region Run against Oracle

                    var oracleError = string.Empty;
                    Timer.Restart();
                    // Oracle Function Execution
                    test.OracleResult1 = oracleConnector.DoReverseLinearGeomTest(test.InputGeom, ref oracleError);
                    Timer.Stop();
                    test.SetOracleElapsedTime(Timer.Elapsed);
                    dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(nameof(test.OracleElapsedTime), test.OracleElapsedTime));
                    dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(nameof(test.OracleResult1), test.OracleResult1));
                    if (!string.IsNullOrWhiteSpace(oracleError))
                        dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(nameof(test.OracleError), oracleError));

                    #endregion
                }
                catch (Exception ex)
                {
                    test.Result = "Failed";
                    test.Error = ex.Message;
                    dataManipulator.ExecuteQuery(test.ErrorUpdateQuery);
                    Logger.LogError(ex);
                }

                dataManipulator.ExecuteQuery(test.ResultUpdateQuery);
                Logger.Log("Test Result : {0}", test.Result);
                if (test.Result.Equals("Passed")) testSuccessCount++;
                testIterator++;
            }
            if (testIterator == 1)
                Logger.Log("No test cases found");

            SqlAssert.AreEqual(testIterator, testSuccessCount);
        }

        [TestMethod]
        public void SplitGeometrySegmentTest()
        {
            Logger.LogLine("SplitGeometrySegment Tests");
            var dataSet = dbConnection.Query<LRSDataSet.SplitGeometrySegmentData>(LRSDataSet.SplitGeometrySegmentData.SelectQuery);
            int testIterator = 1, testSuccessCount = 1;
            foreach (var test in dataSet)
            {
                Logger.LogLine("Executing test {0}", testIterator);
                try
                {
                    var geom = test.InputGeom.GetGeom();
                    var expectedGeom1 = test.ExpectedGeom1.GetGeom();
                    var expectedGeom2 = test.ExpectedGeom2.GetGeom();

                    Logger.LogLine("Splitting Input geom: {0} at a measure of : {1}", geom, test.Measure);
                    Logger.Log("Expected Split Geom Segment 1: {0}", test.ExpectedGeom1);
                    Logger.Log("Expected Split Geom Segment 2: {0}", test.ExpectedGeom2);

                    Timer.Restart();
                    // OSS Function Execution
                    Geometry.SplitGeometrySegment(geom, test.Measure, out SqlGeometry obtainedGeom1, out SqlGeometry obtainedGeom2);
                    Timer.Stop();
                    test.SetElapsedTime(Timer.Elapsed);
                    dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(nameof(test.ElapsedTime), test.ElapsedTime));

                    test.ObtainedGeom1 = obtainedGeom1.ToString().TrimNullValue();
                    test.ObtainedGeom2 = obtainedGeom2.ToString().TrimNullValue();
                    Logger.LogLine("Obtained Geom 1: {0}", test.ObtainedGeom1);
                    Logger.Log("Obtained Geom 2: {0}", test.ObtainedGeom2);
                    dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(nameof(test.ObtainedGeom1), test.ObtainedGeom1));
                    dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(nameof(test.ObtainedGeom2), test.ObtainedGeom2));

                    test.Result = (obtainedGeom1.STEquals(expectedGeom1)
                                   && obtainedGeom2.STEquals(expectedGeom2)).GetResult();

                    #region Run against Oracle

                    var oracleError = string.Empty;
                    Timer.Restart();
                    // Oracle Function Execution
                    var result = oracleConnector.DoSplitGeometrySegmentTest(test.InputGeom, test.Measure, ref oracleError);
                    Timer.Stop();
                    test.OracleResult1 = result.Output_1;
                    test.OracleResult2 = result.Output_2;
                    test.SetOracleElapsedTime(Timer.Elapsed);
                    dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(nameof(test.OracleElapsedTime), test.OracleElapsedTime));
                    dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(nameof(test.OracleResult1), test.OracleResult1));
                    dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(nameof(test.OracleResult2), test.OracleResult2));
                    if (!string.IsNullOrWhiteSpace(oracleError))
                        dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(nameof(test.OracleError), oracleError));

                    #endregion
                }
                catch (Exception ex)
                {
                    test.Result = "Failed";
                    test.Error = ex.Message;
                    dataManipulator.ExecuteQuery(test.ErrorUpdateQuery);
                    Logger.LogError(ex);
                }

                dataManipulator.ExecuteQuery(test.ResultUpdateQuery);
                Logger.Log("Test Result : {0}", test.Result);
                if (test.Result.Equals("Passed")) testSuccessCount++;
                testIterator++;
            }
            if (testIterator == 1)
                Logger.Log("No test cases found");

            SqlAssert.AreEqual(testIterator, testSuccessCount);
        }

        [ClassCleanup()]
        public static void Cleanup()
        {
            if (dbConnection != null)
                dbConnection.Close();
        }
    }
}
