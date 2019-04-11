using System;
using System.Collections.Generic;
using System.Data.SqlServerCe;
using System.IO;
using SQLSpatialTools.UnitTests.Extension;
using System.Globalization;

namespace SQLSpatialTools.UnitTests.DDD
{
    class DataManipulator
    {
        private readonly SqlCeConnection dbConnection;
        private readonly string connectionString;
        private readonly string schemaFile;
        private const string dataFileComment = "##";
        private readonly string[] dataParamSeperator = { "||" };
        private readonly TestLogger logger;

        public DataManipulator(string connectionString, string schemaFile, SqlCeConnection dbConnection, TestLogger logger)
        {
            this.connectionString = connectionString;
            this.schemaFile = schemaFile;
            this.dbConnection = dbConnection;
            this.logger = logger;
        }

        public void CreateDB()
        {
            var sqlCeEngine = new SqlCeEngine(connectionString);
            sqlCeEngine.CreateDatabase();
        }

        public void LoadDataSet()
        {
            if (!CreateSchema())
                return;

            ExecuteQuery(ParseDataSet(LRSDataSet.ClipGeometrySegmentData.DataFile, LRSDataSet.ClipGeometrySegmentData.ParamCount, LRSDataSet.ClipGeometrySegmentData.InsertQuery));
            ExecuteQuery(ParseDataSet(LRSDataSet.GetEndMeasureData.DataFile, LRSDataSet.GetEndMeasureData.ParamCount, LRSDataSet.GetEndMeasureData.InsertQuery));
            ExecuteQuery(ParseDataSet(LRSDataSet.GetStartMeasureData.DataFile, LRSDataSet.GetStartMeasureData.ParamCount, LRSDataSet.GetStartMeasureData.InsertQuery));
            ExecuteQuery(ParseDataSet(LRSDataSet.InterpolateBetweenGeomData.DataFile, LRSDataSet.InterpolateBetweenGeomData.ParamCount, LRSDataSet.InterpolateBetweenGeomData.InsertQuery));
            ExecuteQuery(ParseDataSet(LRSDataSet.IsConnectedData.DataFile, LRSDataSet.IsConnectedData.ParamCount, LRSDataSet.IsConnectedData.InsertQuery));

            ExecuteQuery(ParseDataSet(LRSDataSet.LocatePointAlongGeomData.DataFile, LRSDataSet.LocatePointAlongGeomData.ParamCount, LRSDataSet.LocatePointAlongGeomData.InsertQuery));
            ExecuteQuery(ParseDataSet(LRSDataSet.MergeGeometrySegmentsData.DataFile, LRSDataSet.MergeGeometrySegmentsData.ParamCount, LRSDataSet.MergeGeometrySegmentsData.InsertQuery));
            ExecuteQuery(ParseDataSet(LRSDataSet.PopulateGeometryMeasuresData.DataFile, LRSDataSet.PopulateGeometryMeasuresData.ParamCount, LRSDataSet.PopulateGeometryMeasuresData.InsertQuery));
            ExecuteQuery(ParseDataSet(LRSDataSet.ResetMeasureData.DataFile, LRSDataSet.ResetMeasureData.ParamCount, LRSDataSet.ResetMeasureData.InsertQuery));
            ExecuteQuery(ParseDataSet(LRSDataSet.ReverseLinearGeometryData.DataFile, LRSDataSet.ReverseLinearGeometryData.ParamCount, LRSDataSet.ReverseLinearGeometryData.InsertQuery));
            ExecuteQuery(ParseDataSet(LRSDataSet.SplitGeometrySegmentData.DataFile, LRSDataSet.SplitGeometrySegmentData.ParamCount, LRSDataSet.SplitGeometrySegmentData.InsertQuery));
            ExecuteQuery(ParseDataSet(LRSDataSet.ValidateLRSGeometryData.DataFile, LRSDataSet.ValidateLRSGeometryData.ParamCount, LRSDataSet.ValidateLRSGeometryData.InsertQuery));
            ExecuteQuery(ParseDataSet(LRSDataSet.OffsetGeometrySegmentData.DataFile, LRSDataSet.OffsetGeometrySegmentData.ParamCount, LRSDataSet.OffsetGeometrySegmentData.InsertQuery));
        }

        private bool CreateSchema()
        {
            if (File.Exists(schemaFile))
            {
                var splitQueries = File.ReadAllText(schemaFile).Trim().Split(new[] { "GO" }, StringSplitOptions.RemoveEmptyEntries);
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
                if (!File.Exists(fileName))
                    throw new Exception(string.Format(CultureInfo.CurrentCulture, "Data file :{0} not exists.", fileName));

                var dataSet = File.ReadLines(fileName);
                var lineCounter = 0;
                foreach (var line in dataSet)
                {
                    ++lineCounter;
                    if (string.IsNullOrWhiteSpace(line))
                        continue;
                    if (line.TrimStart().StartsWith(dataFileComment))
                        continue;

                    var subDataSets = line.Split(dataParamSeperator, StringSplitOptions.RemoveEmptyEntries);
                    if (subDataSets.Length != paramCount)
                    {
                        logger.LogError(new Exception("Data Format Exception"), "Error in input data format:{0};\nLine:{1} Argument count mimatch, expected: {2}, obtained: {3}", fileName, lineCounter, paramCount, subDataSets.Length);
                        continue;
                    }

                    var queryContent = queryFormat;
                    for (var param = 0; param < paramCount; param++)
                    {
                        queryContent = queryContent.Replace(string.Format(CultureInfo.CurrentCulture, "[{0}]", param), subDataSets[param].Trim());
                    }

                    queryList.Add(queryContent);
                }

                return queryList;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in parsing data content for {0}", fileName);
            }
            return queryList;
        }

        public void ExecuteQuery(string commandText)
        {
            try
            {
                var sqlCommand = new SqlCeCommand(commandText, dbConnection);
                sqlCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in query execution; Query: {0}", commandText);
            }
        }

        public void ExecuteQuery(List<string> commandTexts)
        {
            foreach (var query in commandTexts)
            {
                ExecuteQuery(query);
            }
        }
    }
}
