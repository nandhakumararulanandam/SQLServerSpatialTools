using System;
using System.Configuration;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using Dapper;
using Oracle.ManagedDataAccess.Client;

namespace SQLSpatialTools.UnitTests.DDD
{
    class OracleConnector
    {
        static OracleConnector oracleConnectorObj;
        OracleConnection oracleConnection;
        private readonly Regex geomTypeRegex;
        private readonly Regex dimensionGroupRegex;
        private readonly Regex dimensionRegex;
        private readonly Regex decimalPointMatch;

        /// <summary>
        /// Gets the Singleton Instance of Oracle's Data Connector object.
        /// </summary>
        /// <returns></returns>
        public static OracleConnector GetInstance()
        {
            if (oracleConnectorObj == null)
            {
                oracleConnectorObj = new OracleConnector();
            }
            return oracleConnectorObj;
        }

        #region Oracle DB Manipulation

        /// <summary>
        /// Initializes Oracle Connector Object.
        /// Also Checks the Oracle connection defined in Configuration.
        /// </summary>
        OracleConnector()
        {
            var connStr = ConfigurationManager.AppSettings.Get("oracle_connection");
            if (string.IsNullOrWhiteSpace(connStr))
                throw new Exception("Oracle Connection string is empty");

            geomTypeRegex = new Regex(OracleLRSQuery.GeomTypeMatch, RegexOptions.Compiled);
            dimensionGroupRegex = new Regex(OracleLRSQuery.DimensionGroup, RegexOptions.Compiled);
            dimensionRegex = new Regex(OracleLRSQuery.DimensionMatch, RegexOptions.Compiled);
            decimalPointMatch = new Regex(OracleLRSQuery.DecimalPointMatch, RegexOptions.Compiled);

            oracleConnection = new OracleConnection { ConnectionString = connStr };

            Open();
            Close();

            //drop and create temp table to capture intermediate results.
            var error = string.Empty;

            ExecuteNonQuery(OracleLRSQuery.DropTempTableQuery, ref error);

            if (!string.IsNullOrEmpty(error))
                throw new Exception(error);

            ExecuteNonQuery(OracleLRSQuery.CreateTempTableQuery, ref error);
            ExecuteNonQuery(OracleLRSQuery.CreateTempTableIndexQuery, ref error);
            ExecuteNonQuery(OracleLRSQuery.CreateTempTablePKQuery, ref error);

            if (!string.IsNullOrEmpty(error))
                throw new Exception(error);
        }

        /// <summary>
        /// Open Oracle Connection Object.
        /// </summary>
        void Open()
        {
            try
            {
                if (oracleConnection.State != ConnectionState.Open)
                    oracleConnection.Open();
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error in connecting Oracle DB:{0}", ex.Message));
            }
        }

        /// <summary>
        /// Close Oracle Connecton Object.
        /// </summary>
        void Close()
        {
            if (oracleConnection.State == ConnectionState.Open)
            {
                oracleConnection.Close();
                //oracleConnection.Dispose();
            }
        }

        /// <summary>
        /// Executes Scalar query against Oracle
        /// </summary>
        /// <param name="query"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        T ExecuteScalar<T>(string query, ref string error)
        {
            T result = default(T);

            try
            {
                Open();
                result = oracleConnection.QuerySingle<T>(query);
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
        void ExecuteNonQuery(string query, ref string error)
        {
            var oracleCommand = new OracleCommand
            {
                Connection = oracleConnection,
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
        /// Return WKT of Merged geom segments.
        /// </summary>
        /// <param name="inputGeom1">Line Segment 1 in WKT</param>
        /// <param name="inputGeom2">Line Segment 2 in WKT</param>
        /// <param name="error"></param>
        /// <returns></returns>
        internal string DoMergeGeomTest(string inputGeom1, string inputGeom2, double tolerance, ref string error)
        {
            var query = string.Format(OracleLRSQuery.MergeGeomSegmentQuery, ConvertTo3DCoordinates(inputGeom1), ConvertTo3DCoordinates(inputGeom2), tolerance);
            var result = ExecuteScalar<string>(query, ref error);
            return TrimDecimalPoints(result);
        }

        /// <summary>
        /// Return WKT of Clipped geom segment.
        /// </summary>
        /// <param name="inputGeom"></param>
        /// <param name="startMeasure"></param>
        /// <param name="endMeasure"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        internal string DoClipGeometrySegment(string inputGeom, double startMeasure, double endMeasure, ref string error)
        {
            var query = string.Format(OracleLRSQuery.ClipGeomSegmentQuery, ConvertTo3DCoordinates(inputGeom), startMeasure, endMeasure);
            var result = ExecuteScalar<string>(query, ref error);
            return TrimDecimalPoints(result);
        }

        /// <summary>
        /// Returns the EndMeasure Value.
        /// </summary>
        /// <param name="inputGeom"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        internal string DoGetEndMeasure(string inputGeom, ref string error)
        {
            var query = string.Format(OracleLRSQuery.GetEndMeasureQuery, ConvertTo3DCoordinates(inputGeom));
            return string.Format("{0}", ExecuteScalar<double>(query, ref error));
        }

        /// <summary>
        /// Returns StartMeasure Value.
        /// </summary>
        /// <param name="inputGeom"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        internal string DoGetStartMeasure(string inputGeom, ref string error)
        {
            var query = string.Format(OracleLRSQuery.GetStartMeasureQuery, ConvertTo3DCoordinates(inputGeom));
            return string.Format("{0}", ExecuteScalar<double>(query, ref error));
        }

        /// <summary>
        /// Returns true if two line segments are spatially connected.
        /// </summary>
        /// <param name="inputGeom1">Line Segment 1 in WKT</param>
        /// <param name="inputGeom2">Line Segment 2 in WKT</param>
        /// <param name="error"></param>
        /// <returns></returns>
        internal string DoIsConnectedGeomSegmentTest(string inputGeom1, string inputGeom2, double tolerance, ref string error)
        {
            var query = string.Format(OracleLRSQuery.GetIsConnectedGeomSegmentQuery, ConvertTo3DCoordinates(inputGeom1), ConvertTo3DCoordinates(inputGeom2), tolerance);
            return string.Format("{0}", ExecuteScalar<bool>(query, ref error));
        }

        /// <summary>
        /// Return WKT of Located point.
        /// </summary>
        /// <param name="inputGeom"></param>
        /// <param name="endMeasure"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        internal string DoLocatePointAlongGeomTest(string inputGeom, double Measure, ref string error)
        {
            var query = string.Format(OracleLRSQuery.GetLocatePointAlongGeomQuery, ConvertTo3DCoordinates(inputGeom), Measure);
            var result = ExecuteScalar<string>(query, ref error);
            return TrimDecimalPoints(result);
        }

        /// <summary>
        /// Returns WKT of Input Geom populated with measure values.
        /// </summary>
        /// <param name="inputGeom"></param>
        /// <param name="startMeasure"></param>
        /// <param name="endMeasure"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        internal string DoPopulateMeasuresTest(string inputGeom, double? startMeasure, double? endMeasure, ref string error)
        {
            var optionBuilder = new StringBuilder();

            if (startMeasure != null)
                optionBuilder.AppendFormat(", {0}", startMeasure);

            if (endMeasure != null)
                optionBuilder.AppendFormat(", {0}", endMeasure);

            var query = string.Format(OracleLRSQuery.GetPopulateMeasureNonQuery, ConvertTo3DCoordinates(inputGeom), optionBuilder.ToString());
            // first execute to store the result in temp table.
            ExecuteNonQuery(query, ref error);

            // retrieve the result from temp table.
            var result = ExecuteScalar<string>(OracleLRSQuery.GetOneResultFromTempTable, ref error);

            return TrimDecimalPoints(result);
        }

        /// <summary>
        /// Return WKT of Reversed geom segment.
        /// </summary>
        /// <param name="inputGeom"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        internal string DoReverseLinearGeomTest(string inputGeom, ref string error)
        {
            var query = string.Format(OracleLRSQuery.GetReverseLinearGeomQuery, ConvertTo3DCoordinates(inputGeom));
            var result = ExecuteScalar<string>(query, ref error);
            return TrimDecimalPoints(result);
        }

        /// <summary>
        /// Returns WKT of two splitted geom segments,
        /// </summary>
        /// <param name="inputGeom"></param>
        /// <param name="Measure"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        internal LRSDataSet.SplitGeomResult DoSplitGeometrySegmentTest(string inputGeom, double? Measure, ref string error)
        {
            var optionBuilder = new StringBuilder();

            var query = string.Format(OracleLRSQuery.GetSplitGeometrySegmentQuery, ConvertTo3DCoordinates(inputGeom), Measure);
            // first execute to store the result in temp table.
            ExecuteNonQuery(query, ref error);

            // retrieve the result from temp table.
            var result = ExecuteScalar<LRSDataSet.SplitGeomResult>(OracleLRSQuery.GetTwoResultFromTempTable, ref error);

            result.Output_1 = TrimDecimalPoints(result.Output_1);
            result.Output_2 = TrimDecimalPoints(result.Output_2);
            return result;
        }

        #endregion LRS Test Functions

        #region Utility Functions

        /// <summary>
        /// Converts the input wkt in 4d(x,y,z,m), 3d(x,y,m) 2d(x,y) to P(x,y,m) values.
        /// </summary>
        /// <param name="lineString"></param>
        /// <returns></returns>
        internal string ConvertTo3DCoordinates(string lineString)
        {
            var convertedStr = new StringBuilder();
            var matches = geomTypeRegex.Matches(lineString);

            foreach (Match match in matches)
            {
                convertedStr.Append(match.Groups["type"].Value + " ");
                convertedStr.Append("(");

                var dimGroups = dimensionGroupRegex.Matches(match.Groups["content"].Value);

                var groupIterator = 1;
                foreach (Match dimGroup in dimGroups)
                {
                    if (dimGroups.Count > 1)
                        convertedStr.Append("(");

                    var dimensions = dimensionRegex.Matches(dimGroup.Groups["group"].Value);
                    var iterator = 1;
                    foreach (Match dim in dimensions)
                    {
                        var x = dim.Groups["x"];
                        var y = dim.Groups["y"];
                        var z = dim.Groups["z"];
                        var m = dim.Groups["m"];
                        z = z != null ? string.IsNullOrWhiteSpace(z.Value)
                            || z.Value.ToLower().Trim().Equals("null") ? null : z : z;
                        m = m != null ? string.IsNullOrWhiteSpace(m.Value)
                            || m.Value.ToLower().Trim().Equals("null") ? null : m : m;

                        var thirdDim = m == null ? (z != null ? z.Value : "0") : m.Value;

                        convertedStr.Append(string.Format("{0} {1} {2}", x.Value, y.Value, thirdDim));

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

        /// <summary>
        /// Trims the decimal points in input WKT geometry.
        /// </summary>
        /// <param name="inputGeomWKT"></param>
        /// <returns></returns>
        string TrimDecimalPoints(string inputGeomWKT)
        {
            return Regex.Replace(inputGeomWKT, OracleLRSQuery.DecimalPointMatch, string.Empty);
        }

        #endregion Utility Functions
    }

    class OracleLRSQuery
    {
        #region Oracle Queries

        public const string MergeGeomSegmentQuery = "SELECT SDO_UTIL.TO_WKTGEOMETRY("
                                                  + "SDO_LRS.CONCATENATE_GEOM_SEGMENTS("
                                                  + "SDO_UTIL.FROM_WKTGEOMETRY('{0}'),"
                                                  + "SDO_UTIL.FROM_WKTGEOMETRY('{1}'), {2})) from dual";

        public const string ClipGeomSegmentQuery = "SELECT SDO_UTIL.TO_WKTGEOMETRY("
                                                 + "SDO_LRS.CLIP_GEOM_SEGMENT("
                                                 + "SDO_UTIL.FROM_WKTGEOMETRY('{0}'), {1}, {2})"
                                                 + ") from dual";

        public const string GetEndMeasureQuery = "SELECT "
                                                + "SDO_LRS.GEOM_SEGMENT_END_MEASURE("
                                                + "SDO_UTIL.FROM_WKTGEOMETRY('{0}')"
                                                + ") from dual";

        public const string GetStartMeasureQuery = "SELECT "
                                                + "SDO_LRS.GEOM_SEGMENT_START_MEASURE("
                                                + "SDO_UTIL.FROM_WKTGEOMETRY('{0}')"
                                                + ") from dual";

        public const string GetIsConnectedGeomSegmentQuery = "SELECT "
                                                 + "SDO_LRS.CONNECTED_GEOM_SEGMENTS("
                                                 + "SDO_UTIL.FROM_WKTGEOMETRY('{0}'),"
                                                 + "SDO_UTIL.FROM_WKTGEOMETRY('{1}'), {2}) from dual";

        public const string GetLocatePointAlongGeomQuery= "SELECT SDO_UTIL.TO_WKTGEOMETRY("
                                                 + "SDO_LRS.LOCATE_PT("
                                                 + "SDO_UTIL.FROM_WKTGEOMETRY('{0}'),{1})"
                                                 + ")from dual";

        public const string GetReverseLinearGeomQuery = "SELECT SDO_UTIL.TO_WKTGEOMETRY("
                                                 + "SDO_LRS.REVERSE_GEOMETRY("
                                                 + "SDO_UTIL.FROM_WKTGEOMETRY('{0}')"
                                                 + "))from dual";

        public const string GetPopulateMeasureNonQuery = "DECLARE geom_segment SDO_GEOMETRY;"
                                                       + ""
                                                       + "BEGIN"
                                                       + "	SELECT SDO_UTIL.FROM_WKTGEOMETRY('{0}')"
                                                       + "	INTO geom_segment"
                                                       + "	FROM dual;"
                                                       + ""
                                                       + "	SDO_LRS.REDEFINE_GEOM_SEGMENT(geom_segment{1});"
                                                       + ""
                                                       + "	INSERT INTO TEMP_DATA (OUTPUT_1)"
                                                       + "	SELECT SDO_UTIL.TO_WKTGEOMETRY(geom_segment)"
                                                       + "	FROM dual;"
                                                       + "END;";

        public const string GetSplitGeometrySegmentQuery = "DECLARE "
                                                        + "geom_segment SDO_GEOMETRY;"
                                                        + "result_geom_1 SDO_GEOMETRY;"
                                                        + "result_geom_2 SDO_GEOMETRY;"
                                                        + ""
                                                        + "BEGIN "
                                                            + "SELECT SDO_UTIL.FROM_WKTGEOMETRY('{0}')"
                                                            + " INTO geom_segment"
                                                            + " FROM dual;"
                                                        + " "
                                                        + "SDO_LRS.SPLIT_GEOM_SEGMENT(geom_segment,{1},result_geom_1,result_geom_2);"
                                                        + " "
                                                        + "	INSERT INTO TEMP_DATA (OUTPUT_1,OUTPUT_2)"
                                                            + "SELECT SDO_UTIL.TO_WKTGEOMETRY(result_geom_1), SDO_UTIL.TO_WKTGEOMETRY(result_geom_2)"
                                                            + " FROM dual;"
                                                        + "END;";

        public const string DropTempTableQuery = "DECLARE"
                                               + "   C INT;"
                                               + "BEGIN"
                                               + "   SELECT COUNT(*) INTO C FROM USER_TABLES WHERE TABLE_NAME = UPPER('TEMP_DATA');"
                                               + "   IF C = 1 THEN"
                                               + "      EXECUTE IMMEDIATE 'DROP TABLE TEMP_DATA';"
                                               + "   END IF;"
                                               + "END;";


        public const string CreateTempTableQuery = "CREATE TABLE Temp_Data ("
                                                 +     " Output_ID NUMBER GENERATED BY DEFAULT ON NULL AS IDENTITY"
                                                 +     " ,Output_1 VARCHAR2(1000) NOT NULL"
                                                 +     " ,Output_2 VARCHAR2(1000)"
                                                 +     " )";

         public const string CreateTempTableIndexQuery = "CREATE UNIQUE INDEX UN_PK ON Temp_Data (Output_ID)";

         public const string CreateTempTablePKQuery = "ALTER TABLE Temp_Data ADD (CONSTRAINT UN_PK PRIMARY KEY (Output_ID) USING INDEX UN_PK ENABLE VALIDATE)";

         public const string GetOneResultFromTempTable = "SELECT OUTPUT_1"
                                                   + " FROM TEMP_DATA"
                                                   + " WHERE OUTPUT_ID IN ("
                                                   +     " SELECT MAX(OUTPUT_ID)"
                                                   +     " FROM TEMP_DATA"
                                                   +     " )";

        public const string GetTwoResultFromTempTable = "SELECT OUTPUT_1, OUTPUT_2"
                                                   + " FROM TEMP_DATA"
                                                   + " WHERE OUTPUT_ID IN ("
                                                   + " SELECT MAX(OUTPUT_ID)"
                                                   + " FROM TEMP_DATA"
                                                   + " )";

        #endregion Oracle Queries

        public const string DecimalPointMatch = @"\.0";
        public const string GeomTypeMatch = @"(?<type>\w+)\s*?(?<content>\(.*\))";
        public const string DimensionGroup = @"\((?<group>.*?)\)";
        public const string DimensionMatch = @"((?<x>[\d\.]+)\s+(?<y>[\d\.]+)\s+(?<z>([\d\.]+)|(null)|(NULL))\s+(?<m>([\d\.]+)|(null)|(NULL)))"
                                          + @"|((?<x>[\d\.]+)\s+(?<y>[\d\.]+)\s+(?<z>([\d\.]+)|(null)|(NULL)))"
                                          + @"|((?<x>[\d\.]+)\s+(?<y>[\d\.]+))";
    }
}
