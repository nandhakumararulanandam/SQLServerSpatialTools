using System;
using System.Configuration;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using Oracle.ManagedDataAccess.Client;

namespace SQLSpatialTools.UnitTests.DDD
{
    class OracleConnector
    {
        static OracleConnector oracleConnectorObj;
        OracleConnection oracleConnection;
        Regex geomTypeRegex, dimensionGroupRegex, dimensionRegex, decimalPointMatch;

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
            var oracleCommand = new OracleCommand
            {
                Connection = oracleConnection,
                CommandText = query
            };

            try
            {
                Open();
                result = (T)oracleCommand.ExecuteScalar();
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
        /// Trims the decimal points in input WKT geometry.
        /// </summary>
        /// <param name="inputGeomWKT"></param>
        /// <returns></returns>
        string TrimDecimalPoints(string inputGeomWKT)
        {
            return Regex.Replace(inputGeomWKT, OracleLRSQuery.DecimalPointMatch, string.Empty);
        }

        /// <summary>
        /// Return WKT of merged geom segments.
        /// </summary>
        /// <param name="inputGeom1">Line Segment 1 in WKT</param>
        /// <param name="inputGeom2">Line Segment 2 in WKT</param>
        /// <param name="error"></param>
        /// <returns></returns>
        internal string DoMergeGeomTest(string inputGeom1, string inputGeom2, ref string error)
        {
            var query = string.Format(OracleLRSQuery.MergeGeomSegmentQuery, ConvertTo3DCoordinates(inputGeom1), ConvertTo3DCoordinates(inputGeom2), OracleLRSQuery.Tolerance);
            OracleCommand oracleCommand = new OracleCommand
            {
                Connection = oracleConnection,
                CommandText = query
            };

            var result = ExecuteScalar<string>(query, ref error);

            return TrimDecimalPoints(result);
        }

        /// <summary>
        /// Return WKT of clipped geom segment.
        /// </summary>
        /// <param name="inputGeom"></param>
        /// <param name="startMeasure"></param>
        /// <param name="endMeasure"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        internal string DoClipGeometrySegment(string inputGeom, double startMeasure, double endMeasure, ref string error)
        {
            var query = string.Format(OracleLRSQuery.ClipGeomSegmentQuery, ConvertTo3DCoordinates(inputGeom), startMeasure, endMeasure);
            OracleCommand oracleCommand = new OracleCommand
            {
                Connection = oracleConnection,
                CommandText = query
            };

            var result = ExecuteScalar<string>(query, ref error);

            return TrimDecimalPoints(result);
        }

        /// <summary>
        /// Converts the input wkt in 4d(x,y,z,m), 3d(x,y,m) 2d(x,y) to x,y,m values.
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
    }

    public class OracleLRSQuery
    {
        public const string MergeGeomSegmentQuery = "SELECT SDO_UTIL.TO_WKTGEOMETRY("
                                                  + "SDO_LRS.CONCATENATE_GEOM_SEGMENTS("
                                                  + "SDO_UTIL.FROM_WKTGEOMETRY('{0}'),"
                                                  + "SDO_UTIL.FROM_WKTGEOMETRY('{1}'), {2})) from dual";

        public const string ClipGeomSegmentQuery = "SELECT SDO_UTIL.TO_WKTGEOMETRY("
                                                 + "SDO_LRS.CLIP_GEOM_SEGMENT("
                                                 + "SDO_UTIL.FROM_WKTGEOMETRY('{0}'), {1}, {2})"
                                                 + ") from dual";

        public const double Tolerance = 0.5;

        public const string DecimalPointMatch = @"\.0";
        public const string GeomTypeMatch = @"(?<type>\w+)\s*?(?<content>\(.*\))";
        public const string DimensionGroup = @"\((?<group>.*?)\)";
        public const string DimensionMatch = @"((?<x>\d+)\s+(?<y>\d+)\s+(?<z>(\d+)|(null)|(NULL))\s+(?<m>(\d+)|(null)|(NULL)))"
                                          + @"|((?<x>\d+)\s+(?<y>\d+)\s+(?<z>(\d+)|(null)|(NULL)))"
                                          + @"|((?<x>\d+)\s+(?<y>\d+))";
    }
}
