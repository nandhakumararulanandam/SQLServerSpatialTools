using System;
using System.Collections.Generic;
using System.Data.SqlServerCe;
using System.IO;
using System.Text;
using Dapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SQLSpatialTools.Functions.LRS;
using SQLSpatialTools.Utility;

namespace SQLSpatialTools.UnitTests.DDD
{
    [TestClass]
    public class LRSFunctionTests : UnitTest
    {
        private SqlCeConnection dbConnection;
        private string connectionString;
        private const string DatabaseFile = "SpatialTestData.sdf";
        private const string SchemaFile = "Dataset\\CreateDBSchema.sql";

        [TestInitialize]
        public void Intialize()
        {
            if (File.Exists(DatabaseFile))
                File.Delete(DatabaseFile);

            connectionString = string.Format("Data Source=|DataDirectory|\\{0}", DatabaseFile);
            dbConnection = new SqlCeConnection(connectionString);

            CreateDBAndLoadDataSet();
        }

        private void CreateDBAndLoadDataSet()
        {
            var sqlCeEngine = new SqlCeEngine(connectionString);
            sqlCeEngine.CreateDatabase();
            dbConnection.Open();

            if (CreateSchema())
            {
                ExecuteQuery(ParseDataSet(LRSDataSet.ClipGeometrySegmentData.DataFile, LRSDataSet.ClipGeometrySegmentData.ParamCount, LRSDataSet.ClipGeometrySegmentData.InsertQuery));
                ExecuteQuery(ParseDataSet(LRSDataSet.GetEndMeasureData.DataFile, LRSDataSet.GetEndMeasureData.ParamCount, LRSDataSet.GetEndMeasureData.InsertQuery));
            }
        }

        private bool CreateSchema()
        {
            if (File.Exists(SchemaFile))
            {
                var splitQueries = File.ReadAllText(SchemaFile).Trim().Split(new[] { "GO" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var query in splitQueries)
                {
                    if (!string.IsNullOrWhiteSpace(query))
                        ExecuteQuery(query);
                }
                return true;
            }
            return false;
        }

        private List<string> ParseDataSet(string fileName, int paramCount, string queryFormat)
        {
            var queryList = new List<string>();
            try
            {
                if (File.Exists(fileName))
                {
                    var dataSet = File.ReadLines(fileName);
                    foreach (var line in dataSet)
                    {
                        if (string.IsNullOrWhiteSpace(line))
                            continue;
                        if (line.TrimStart().StartsWith("##"))
                            continue;

                        var subDataSets = line.Split(new[] { "||" }, StringSplitOptions.RemoveEmptyEntries);
                        if (subDataSets.Length != paramCount)
                            continue;

                        var queryContent = queryFormat;
                        for (var param = 0; param < paramCount; param++)
                        {
                            queryContent = queryContent.Replace(string.Format("[{0}]", param), subDataSets[param].Trim());
                        }

                        queryList.Add(queryContent);
                    }

                    return queryList;
                }
            }
            catch (Exception ex)
            {
                Logger.LogLine("Error in parsing data content for {0} : {1}", fileName, ex.Message);
            }
            return queryList;
        }

        private void ExecuteQuery(string commandText)
        {
            try
            {
                var sqlCommand = new SqlCeCommand(commandText, dbConnection);
                sqlCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Logger.LogLine("Error in query execution; Query: {0}, \nError:{1}", commandText, ex.Message);
            }
        }

        private void ExecuteQuery(List<string> commandTexts)
        {
            foreach (var query in commandTexts)
            {
                ExecuteQuery(query);
            }
        }

        public TestContext CurrentTestContext { get; set; }

        [TestMethod]
        public void ClipGeometrySegmentTest()
        {
            Logger.LogLine("Clip Geometry Segments Tests");
            var dataSet = dbConnection.Query<LRSDataSet.ClipGeometrySegmentData>(LRSDataSet.ClipGeometrySegmentData.SelectQuery);
            var testIterator = 1;
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

                    var returnGeomSegment = Geometry.ClipGeometrySegment(inputGeomSegment, test.StartMeasure, test.EndMeasure);
                    Logger.Log("Obtained Geom : {0}", returnGeomSegment.ToString());
                    Logger.Log("Test Result : {0}", ((bool)returnGeomSegment.STEquals(expectedGeomSegment) ? "Passed" : "Failed"));
                }
                catch (Exception ex)
                {
                    Logger.Log("Error : {0}", ex.Message);
                }
                testIterator++;
            }
        }

        [TestMethod]
        public void GetEndMeasureTest()
        {
            Logger.LogLine("Clip Geometry Segments Tests");
            var dataSet = dbConnection.Query<LRSDataSet.GetEndMeasureData>(LRSDataSet.GetEndMeasureData.SelectQuery);
            var testIterator = 1;
            foreach (var test in dataSet)
            {
                Logger.LogLine("Executing test {0}", testIterator);
                try
                {
                    var inputGeomSegment = test.InputGeom.GetGeom();
                    Logger.Log("Input Geom : {0}", inputGeomSegment.ToString());
                    Logger.Log("Expected End Measure : {0}", test.ExpectedEndMeasure);

                    var returnEndMeasure = Geometry.GetEndMeasure(inputGeomSegment);
                    Logger.Log("Obtained End Measure : {0}", returnEndMeasure);
                    Logger.Log("Test Result : {0}", (returnEndMeasure == test.ExpectedEndMeasure) ? "Passed" : "Failed");
                }
                catch (Exception ex)
                {
                    Logger.Log("Error : {0}", ex.Message);
                }
                testIterator++;
            }
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (dbConnection != null)
                dbConnection.Close();
        }
    }

    public class LRSDataSet
    {
        public class ClipGeometrySegmentData
        {
            public int Id { get; set; }
            public string InputGeom { get; set; }
            public double StartMeasure { get; set; }
            public double EndMeasure { get; set; }
            public string ExpectedGeom { get; set; }

            public const string SelectQuery = "SELECT [Id], [InputGeom], [StartMeasure], [EndMeasure], [ExpectedGeom] FROM [LRS_ClipGeometrySegmentData]";
            public const string InsertQuery = "INSERT INTO [LRS_ClipGeometrySegmentData] ([InputGeom], [StartMeasure], [EndMeasure], [ExpectedGeom]) VALUES (N'[0]', [1], [2], N'[3]');";
            public const string DataFile = "Dataset\\LRS\\ClipGeometrySegment.data";
            public const short ParamCount = 4;
        }

        public class GetEndMeasureData
        {
            public int Id { get; set; }
            public string InputGeom { get; set; }
            public double ExpectedEndMeasure { get; set; }

            public const string SelectQuery = "SELECT [Id], [InputGeom], [ExpectedEndMeasure] FROM [LRS_GetEndMeasureData]";
            public const string InsertQuery = "INSERT INTO [LRS_GetEndMeasureData] ([InputGeom], [ExpectedEndMeasure]) VALUES (N'[0]', [1]);";
            public const string DataFile = "Dataset\\LRS\\GetEndMeasure.data";
            public const short ParamCount = 2;
        }
    }
}
