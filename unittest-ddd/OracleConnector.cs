//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

using System;
using System.Configuration;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using Dapper;
using Oracle.ManagedDataAccess.Client;
using System.Globalization;
using SQLSpatialTools.Utility;
using Microsoft.SqlServer.Types;

namespace SQLSpatialTools.UnitTests.DDD
{
    internal class OracleConnector
    {
        private static OracleConnector _oracleConnectorObj;
        private readonly OracleConnection _oracleConnection;
        private readonly Regex _geomTypeRegex;
        private readonly Regex _dimensionGroupRegex;
        private readonly Regex _dimensionRegex;

        /// <summary>
        /// Obtain the Oracle Connector instance.
        /// </summary>
        /// <returns>Oracle Connector Object</returns>
        public static OracleConnector GetInstance()
        {
            return _oracleConnectorObj ?? (_oracleConnectorObj = new OracleConnector());
        }

        #region Oracle DB Manipulation

        /// <summary>
        /// Initializes Oracle Connector Object.
        /// Also Checks the Oracle connection defined in Configuration.
        /// </summary>
        private OracleConnector()
        {
            var connStr = ConfigurationManager.AppSettings.Get("oracle_connection");
            if (string.IsNullOrWhiteSpace(connStr))
                throw new ArgumentNullException(message:"Oracle Connection string is empty", paramName:connStr);

            _geomTypeRegex = new Regex(OracleLRSQuery.GeomTypeMatch, RegexOptions.Compiled);
            _dimensionGroupRegex = new Regex(OracleLRSQuery.DimensionGroup, RegexOptions.Compiled);
            _dimensionRegex = new Regex(OracleLRSQuery.DimensionMatch, RegexOptions.Compiled);

            _oracleConnection = new OracleConnection { ConnectionString = connStr };

            Open();
            Close();

            //drop and create temp table to capture intermediate results.
            var error = string.Empty;

            ExecuteNonQuery(OracleLRSQuery.DropTempTableQuery, ref error);

            if (!string.IsNullOrEmpty(error))
                throw new ArgumentNullException(error);

            ExecuteNonQuery(OracleLRSQuery.CreateTempTableQuery, ref error);
            ExecuteNonQuery(OracleLRSQuery.CreateTempTableIndexQuery, ref error);
            ExecuteNonQuery(OracleLRSQuery.CreateTempTablePkQuery, ref error);

            if (!string.IsNullOrEmpty(error))
                throw new ArgumentNullException(error);
        }

        /// <summary>
        /// Opens Oracle Connection Object.
        /// </summary>
        private void Open()
        {
            try
            {
                if (_oracleConnection.State != ConnectionState.Open)
                    _oracleConnection.Open();
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format(CultureInfo.CurrentCulture, "Error in connecting Oracle DB:{0}", ex.Message));
            }
        }

        /// <summary>
        /// Close Oracle Connection Object.
        /// </summary>
        private void Close()
        {
            if (_oracleConnection.State == ConnectionState.Open)
            {
                _oracleConnection.Close();
                //oracleConnection.Dispose();
            }
        }

        /// <summary>
        /// Executes Scalar query against Oracle
        /// </summary>
        /// <param name="query"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        private T ExecuteScalar<T>(string query, out string error)
        {
            var result = default(T);
            error = string.Empty;
            try
            {
                Open();
                result = _oracleConnection.QuerySingle<T>(query);
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }
            finally
            {
                Close();
            }
            return result;
        }

        /// <summary>
        /// Executes non query against Oracle.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="error"></param>
        private void ExecuteNonQuery(string query, ref string error)
        {
            var oracleCommand = new OracleCommand
            {
                Connection = _oracleConnection,
                CommandText = query
            };

            try
            {
                Open();
                oracleCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }
            finally
            {
                Close();
            }
        }
        #endregion

        #region LRS Test Functions

        /// <summary>
        /// Test MergeGeom Function against Oracle.
        /// </summary>
        /// <param name="testObj">The test object.</param>
        internal void DoMergeGeomTest(LRSDataSet.MergeGeometrySegmentsData testObj)
        {
            var query = string.Format(CultureInfo.CurrentCulture, OracleLRSQuery.MergeGeomSegmentQuery, ConvertTo3DCoordinates(testObj.InputGeom1), ConvertTo3DCoordinates(testObj.InputGeom2), testObj.Tolerance);
            var result = ExecuteScalar<string>(query, out var errorInfo);
            testObj.OracleError = errorInfo;
            testObj.OracleQuery = query;
            testObj.OracleResult1 = result;
        }
        /// <summary>
        /// Test MergeAndReset against oracle
        /// </summary>
        /// <param name="testObj"></param>
        internal void DoMergeAndResetGeomTest(LRSDataSet.MergeAndResetGeometrySegmentsData testObj)
        {
            var errorInfo = string.Empty;
            var query1 = string.Format(CultureInfo.CurrentCulture, OracleLRSQuery.MergeAndResetGeomSegmentQuery, ConvertTo3DCoordinates(testObj.InputGeom1), ConvertTo3DCoordinates(testObj.InputGeom2), testObj.Tolerance);
            // first execute to store the result in temp table.
            ExecuteNonQuery(query1, ref errorInfo);

            // retrieve the result from temp table.
            // if there is an error in the previous query; don't run the result from temp table.
            if (string.IsNullOrEmpty(errorInfo))
            {
                var query2 = string.Format(CultureInfo.CurrentCulture, OracleLRSQuery.GetOneResultFromTempTable);
                var result = ExecuteScalar<LRSDataSet.MergeAndResetResult>(query2, out errorInfo);
                testObj.OracleQuery = string.Format(CultureInfo.CurrentCulture, "{0}\n{1}", query1, query2);
                testObj.OracleResult1 = result.Output1;
            }
            testObj.OracleError = errorInfo;

        }
        /// <summary>
        /// Test ClipGeom Function against Oracle.
        /// </summary>
        /// <param name="testObj">The test object.</param>
        internal void DoClipGeometrySegment(LRSDataSet.ClipGeometrySegmentData testObj)
        {
            var inputGeom = testObj.InputGeom.GetGeom();
            var query = inputGeom.IsPoint() 
                ? string.Format(CultureInfo.CurrentCulture, OracleLRSQuery.ClipGeomSegmentPointQuery, GetOracleOrdinatePoint(inputGeom), testObj.StartMeasure, testObj.EndMeasure, testObj.Tolerance)
                : string.Format(CultureInfo.CurrentCulture, OracleLRSQuery.ClipGeomSegmentQuery, ConvertTo3DCoordinates(testObj.InputGeom), testObj.StartMeasure, testObj.EndMeasure, testObj.Tolerance);
            var result = ExecuteScalar<string>(query, out var errorInfo);
            testObj.OracleError = errorInfo;
            testObj.OracleQuery = query;
            testObj.OracleResult1 = result;
        }

        /// <summary>
        /// Test GetEndMeasure Function against Oracle.
        /// </summary>
        /// <param name="testObj">The test object.</param>
        internal void DoGetEndMeasure(LRSDataSet.GetEndMeasureData testObj)
        {
            var query = string.Format(CultureInfo.CurrentCulture, OracleLRSQuery.GetEndMeasureQuery, ConvertTo3DCoordinates(testObj.InputGeom));
            var result = ExecuteScalar<string>(query, out var errorInfo);
            testObj.OracleError = errorInfo;
            testObj.OracleQuery = query;
            testObj.OracleResult1 = result;
        }

        /// <summary>
        /// Test GetStartMeasure Function against Oracle.
        /// </summary>
        /// <param name="testObj">The test object.</param>
        internal void DoGetStartMeasure(LRSDataSet.GetStartMeasureData testObj)
        {
            var query = string.Format(CultureInfo.CurrentCulture, OracleLRSQuery.GetStartMeasureQuery, ConvertTo3DCoordinates(testObj.InputGeom));
            var result = ExecuteScalar<string>(query, out var errorInfo);
            testObj.OracleError = errorInfo;
            testObj.OracleQuery = query;
            testObj.OracleResult1 = result;
        }

        /// <summary>
        /// Test Is Spatially Connected Function against Oracle.
        /// </summary>
        /// <param name="testObj">The test object.</param>
        internal void DoIsConnectedGeomSegmentTest(LRSDataSet.IsConnectedData testObj)
        {
            var query = string.Format(CultureInfo.CurrentCulture, OracleLRSQuery.GetIsConnectedGeomSegmentQuery, ConvertTo3DCoordinates(testObj.InputGeom1), ConvertTo3DCoordinates(testObj.InputGeom2), testObj.Tolerance);
            var result = ExecuteScalar<string>(query, out var errorInfo);
            testObj.OracleError = errorInfo;
            testObj.OracleQuery = query;
            testObj.OracleResult1 = result;
        }

        /// <summary>
        /// Test LocatePoint Function against Oracle.
        /// </summary>
        /// <param name="testObj">The test object.</param>
        internal void DoLocatePointAlongGeomTest(LRSDataSet.LocatePointAlongGeomData testObj)
        {
            var query = string.Format(CultureInfo.CurrentCulture, OracleLRSQuery.GetLocatePointAlongGeomQuery, ConvertTo3DCoordinates(testObj.InputGeom), testObj.Measure);
            var result = ExecuteScalar<string>(query, out var errorInfo);
            testObj.OracleError = errorInfo;
            testObj.OracleQuery = query;
            testObj.OracleResult1 = result;
        }

        /// <summary>
        /// Test OffsetGeom Function against Oracle.
        /// </summary>
        /// <param name="testObj">The test object.</param>
        internal void DoOffsetGeometrySegment(LRSDataSet.OffsetGeometrySegmentData testObj)
        {
            var query = string.Format(CultureInfo.CurrentCulture, OracleLRSQuery.OffsetGeometryQuery, ConvertTo3DCoordinates(testObj.InputGeom), testObj.StartMeasure, testObj.EndMeasure, testObj.Offset, testObj.Tolerance);
            var result = ExecuteScalar<string>(query, out var errorInfo);
            testObj.OracleError = errorInfo;
            testObj.OracleQuery = query;
            testObj.OracleResult1 = result;
        }

        /// <summary>
        /// Test Populate Measure Function against Oracle.
        /// </summary>
        /// <param name="testObj">The test object.</param>
        internal void DoPopulateMeasuresTest(LRSDataSet.PopulateGeometryMeasuresData testObj)
        {
            var inputGeom = testObj.InputGeom.GetGeom();
            var optionBuilder = new StringBuilder();

            if (testObj.StartMeasure != null)
                optionBuilder.AppendFormat(CultureInfo.CurrentCulture, ", {0}", testObj.StartMeasure);

            if (testObj.EndMeasure != null)
                optionBuilder.AppendFormat(CultureInfo.CurrentCulture, ", {0}", testObj.EndMeasure);

            var errorInfo = string.Empty;
            string query1;
            if (inputGeom.CheckGeomPoint())
            {
                var pointInOracle =
                    $"{inputGeom.STX}, {inputGeom.STY}, {(inputGeom.HasM ? inputGeom.M.Value : inputGeom.Z.Value)}";
                query1 = string.Format(CultureInfo.CurrentCulture, OracleLRSQuery.GetPopulateMeasurePoint, pointInOracle, optionBuilder.ToString());
                ExecuteNonQuery(query1, ref errorInfo);
            }
            else
            {
                query1 = string.Format(CultureInfo.CurrentCulture, OracleLRSQuery.GetPopulateMeasureNonQuery, ConvertTo3DCoordinates(testObj.InputGeom), optionBuilder.ToString());
                // first execute to stores the result in temp table.
                ExecuteNonQuery(query1, ref errorInfo);
            }

            // retrieve the result from temp table.
            // if there is an error in the previous query; don't run the result from temp table.
            if (string.IsNullOrEmpty(errorInfo))
            {
                var query2 = string.Format(CultureInfo.CurrentCulture, OracleLRSQuery.GetOneResultFromTempTable);
                var result = ExecuteScalar<string>(query2, out errorInfo);
                testObj.OracleQuery = string.Format(CultureInfo.CurrentCulture, "{0}\n{1}", query1, query2);
                testObj.OracleResult1 = result;
            }
            testObj.OracleError = errorInfo;
        }

        /// <summary>
        /// Test Reverse Linear Geometry Function against Oracle.
        /// </summary>
        /// <param name="testObj">The test object.</param>
        internal void DoReverseLinearGeomTest(LRSDataSet.ReverseLinearGeometryData testObj)
        {
            var query = string.Format(CultureInfo.CurrentCulture, OracleLRSQuery.GetReverseLinearGeomQuery, ConvertTo3DCoordinates(testObj.InputGeom));
            var result = ExecuteScalar<string>(query, out var errorInfo);
            testObj.OracleError = errorInfo;
            testObj.OracleQuery = query;
            testObj.OracleResult1 = result;
        }


        /// <summary>
        /// Test Split Geometry Function against Oracle.
        /// </summary>
        /// <param name="testObj">The test object.</param>
        internal void DoSplitGeometrySegmentTest(LRSDataSet.SplitGeometrySegmentData testObj)
        {
            var errorInfo = string.Empty;
            var query1 = string.Format(CultureInfo.CurrentCulture, OracleLRSQuery.GetSplitGeometrySegmentQuery, ConvertTo3DCoordinates(testObj.InputGeom), testObj.Measure);
            // first execute to store the result in temp table.
            ExecuteNonQuery(query1, ref errorInfo);

            // retrieve the result from temp table.
            // if there is an error in the previous query; don't run the result from temp table.
            if (string.IsNullOrEmpty(errorInfo))
            {
                var query2 = string.Format(CultureInfo.CurrentCulture, OracleLRSQuery.GetTwoResultFromTempTable);
                var result = ExecuteScalar<LRSDataSet.SplitGeomResult>(query2, out errorInfo);
                testObj.OracleQuery = string.Format(CultureInfo.CurrentCulture, "{0}\n{1}", query1, query2);
                testObj.OracleResult1 = result.Output1;
                testObj.OracleResult2 = result.Output2;
            }

            testObj.OracleError = errorInfo;
        }


        /// <summary>
        /// Test Validate LRS Geometry Function against Oracle.
        /// </summary>
        /// <param name="testObj">The test object.</param>
        internal void ValidateLRSGeometry(LRSDataSet.ValidateLRSGeometryData testObj)
        {
            var query = string.Format(CultureInfo.CurrentCulture, OracleLRSQuery.ValidateLRSGeometryQuery, testObj.InputGeom);
            var result = ExecuteScalar<string>(query, out var errorInfo);
            testObj.OracleError = errorInfo;
            testObj.OracleQuery = query;
            testObj.OracleResult1 = result;
        }

        #endregion LRS Test Functions

        #region Utility Functions

        /// <summary>
        /// Gets the oracle ordinate point.
        /// </summary>
        /// <param name="inputGeom">The input geom.</param>
        /// <returns></returns>
        internal string GetOracleOrdinatePoint(SqlGeometry inputGeom)
        {
            return $"{inputGeom.STX}, {inputGeom.STY}, {(inputGeom.HasM ? inputGeom.M.Value : inputGeom.Z.Value)}";
        }

        /// <summary>
        /// Converts the input WKT in 4d(x,y,z,m), 3d(x,y,m) 2d(x,y) to P(x,y,m) values.
        /// </summary>
        /// <param name="geomSegmentWKT"></param>
        /// <returns></returns>
        internal string ConvertTo3DCoordinates(string geomSegmentWKT)
        {
            var convertedStr = new StringBuilder();
            var matches = _geomTypeRegex.Matches(geomSegmentWKT);

            foreach (Match match in matches)
            {
                convertedStr.Append(match.Groups["type"].Value + " ");
                convertedStr.Append("(");

                var dimGroups = _dimensionGroupRegex.Matches(match.Groups["content"].Value);

                var groupIterator = 1;
                foreach (Match dimGroup in dimGroups)
                {
                    if (dimGroups.Count > 1)
                        convertedStr.Append("(");

                    var dimensions = _dimensionRegex.Matches(dimGroup.Groups["group"].Value);
                    var iterator = 1;
                    foreach (Match dim in dimensions)
                    {
                        var x = dim.Groups["x"];
                        var y = dim.Groups["y"];
                        var z = dim.Groups["z"];
                        var m = dim.Groups["m"];

                        z = z != null
                            ? string.IsNullOrWhiteSpace(z.Value)
                              || z.Value.ToLower(CultureInfo.CurrentCulture).Trim()
                                  .Equals("null", StringComparison.CurrentCulture) ? null : z
                            : null;

                        m = m != null
                            ? string.IsNullOrWhiteSpace(m.Value)
                              || m.Value.ToLower(CultureInfo.CurrentCulture).Trim()
                                  .Equals("null", StringComparison.CurrentCulture) ? null : m
                            : null;

                        var thirdDim = m == null ? (z != null ? z.Value : "0") : m.Value;

                        convertedStr.Append(string.Format(CultureInfo.CurrentCulture, "{0} {1} {2}", x.Value, y.Value, thirdDim));

                        if (iterator != dimensions.Count)
                            convertedStr.Append(", ");

                        iterator++;
                    }
                    if (dimGroups.Count > 1)
                        convertedStr.Append(")");

                    if (dimGroups.Count > 1 && groupIterator != dimGroups.Count)
                        convertedStr.Append(", ");

                    groupIterator++;
                }

                convertedStr.Append(")");
            }

            return convertedStr.ToString();
        }

        #endregion Utility Functions
    }
}
