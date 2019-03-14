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

                    var obtainedGeomSegment = Geometry.ClipGeometrySegment(inputGeomSegment, test.StartMeasure, test.EndMeasure);
                    test.ObtainedGeom = obtainedGeomSegment.ToString();
                    dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(nameof(test.ObtainedGeom), test.ObtainedGeom));
                    Logger.Log("Obtained Geom : {0}", test.ObtainedGeom);

                    test.Result = obtainedGeomSegment.STEquals(expectedGeomSegment).GetResult();
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

                    test.ObtainedEndMeasure = (double)Geometry.GetEndMeasure(inputGeomSegment);
                    dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(nameof(test.ObtainedEndMeasure), test.ObtainedEndMeasure));
                    Logger.Log("Obtained End Measure : {0}", test.ObtainedEndMeasure);

                    test.Result = (test.ObtainedEndMeasure == test.ExpectedEndMeasure).GetResult();
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

                    test.ObtainedStartMeasure = (double)Geometry.GetStartMeasure(inputGeomSegment);
                    dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(nameof(test.ObtainedStartMeasure), test.ObtainedStartMeasure));
                    Logger.Log("Obtained Start Measure : {0}", test.ObtainedStartMeasure);

                    test.Result = (test.ObtainedStartMeasure == test.ExpectedStartMeasure).GetResult();
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

                    var obtainedGeom = Geometry.InterpolateBetweenGeom(geom1, geom2, test.Measure);
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

                    test.Obtained = (bool)Geometry.IsConnected(geom1, geom2, test.Tolerance);

                    Logger.Log("Obtained Point: {0}", test.Obtained);
                    dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(nameof(test.Obtained), test.Obtained));

                    test.Result = (test.Obtained == test.Expected).GetResult();
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
                    var obtainedGeom = Geometry.LocatePointAlongGeom(geom, test.Measure);
                    test.ObtainedPoint = obtainedGeom.ToString();
                    Logger.Log("Obtained Point: {0}", test.ObtainedPoint);
                    dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(nameof(test.ObtainedPoint), test.ObtainedPoint));

                    test.Result = obtainedGeom.STEquals(expectedGeom).GetResult();
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

                    var obtainedGeom = Geometry.MergeGeometrySegments(geom1, geom2);
                    test.ObtainedGeom = obtainedGeom.ToString();
                    Logger.Log("Obtained Point: {0}", test.ObtainedGeom);
                    dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(nameof(test.ObtainedGeom), test.ObtainedGeom));

                    test.Result = obtainedGeom.STEquals(expectedGeom).GetResult();
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

                    var obtainedGeom = Geometry.PopulateGeometryMeasures(geom, test.StartMeasure, test.EndMeasure);
                    test.ObtainedGeom = obtainedGeom.ToString();
                    Logger.Log("Obtained Geom: {0}", test.ObtainedGeom);
                    dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(nameof(test.ObtainedGeom), test.ObtainedGeom));

                    test.Result = obtainedGeom.STEqualsMeasure(expectedGeom).GetResult();
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

                    var obtainedGeomSegment = Geometry.ResetMeasure(inputGeomSegment);
                    test.ObtainedGeom = obtainedGeomSegment.ToString();
                    test.ExpectedGeom = expectedGeomSegment.ToString();
                    dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(nameof(test.ObtainedGeom), test.ObtainedGeom));
                    Logger.Log("Obtained Geom : {0}", test.ObtainedGeom);

                    test.Result = obtainedGeomSegment.STEqualsMeasure(expectedGeomSegment).GetResult();
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

                    var obtainedGeom = Geometry.ReverseLinearGeometry(geom);
                    test.ObtainedGeom = obtainedGeom.ToString();
                    Logger.Log("Obtained Geom: {0}", test.ObtainedGeom);
                    dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(nameof(test.ObtainedGeom), test.ObtainedGeom));

                    test.Result = obtainedGeom.STEquals(expectedGeom).GetResult();
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

                    Geometry.SplitGeometrySegment(geom, test.Measure, out SqlGeometry obtainedGeom1, out SqlGeometry obtainedGeom2);
                    test.ObtainedGeom1 = obtainedGeom1.ToString();
                    test.ObtainedGeom2 = obtainedGeom2.ToString();
                    Logger.LogLine("Obtained Geom 1: {0}", test.ObtainedGeom1);
                    Logger.Log("Obtained Geom 2: {0}", test.ObtainedGeom2);
                    dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(nameof(test.ObtainedGeom1), test.ObtainedGeom1));
                    dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(nameof(test.ObtainedGeom2), test.ObtainedGeom2));

                    test.Result = (obtainedGeom1.STEquals(expectedGeom1)
                                   && obtainedGeom2.STEquals(expectedGeom2)).GetResult();
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
