using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.SqlServer.Types;
using System.Globalization;

namespace SQLSpatialTools.Utility
{
    public static class SpatialExtensions
    {
        #region OGC Type Checks

        /// <summary>
        /// Check if Geometry is Point
        /// </summary>
        /// <param name="sqlgeometry"></param>
        /// <returns></returns>
        public static bool IsPoint(this SqlGeometry sqlgeometry)
        {
            return sqlgeometry.STGeometryType().Compare(OGCType.Point.GetString());
        }

        /// <summary>
        /// Check if Geography type is Point
        /// </summary>
        /// <param name="sqlgeography">SQL Geography</param>
        /// <returns></returns>
        public static bool IsPoint(this SqlGeography sqlgeography)
        {
            return sqlgeography.STGeometryType().Compare(OGCType.Point.GetString());
        }

        /// <summary>
        /// Check if Geometry is LineString
        /// </summary>
        /// <param name="sqlgeometry"></param>
        /// <returns>true; false</returns>
        public static bool IsLineString(this SqlGeometry sqlgeometry)
        {
            return sqlgeometry.STGeometryType().Compare(OGCType.LineString.GetString());
        }

        /// <summary>
        /// Check if Geometry is CircularString
        /// </summary>
        /// <param name="sqlgeometry"></param>
        /// <returns>true; false</returns>
        public static bool IsCircularString(this SqlGeometry sqlgeometry)
        {
            return sqlgeometry.STGeometryType().Compare(OGCType.CircularString.GetString());
        }

        /// <summary>
        /// Check if Geometry is CompoundCurve
        /// </summary>
        /// <param name="sqlgeometry"></param>
        /// <returns>true; false</returns>
        public static bool IsCompoundCurve(this SqlGeometry sqlgeometry)
        {
            return sqlgeometry.STGeometryType().Compare(OGCType.CompoundCurve.GetString());
        }

        /// <summary>
        /// Check if Geometry is Polygon
        /// </summary>
        /// <param name="sqlgeometry"></param>
        /// <returns>true; false</returns>
        public static bool IsPolygon(this SqlGeometry sqlgeometry)
        {
            return sqlgeometry.STGeometryType().Compare(OGCType.Polygon.GetString());
        }

        /// <summary>
        /// Check if Geometry is CurvePolygon
        /// </summary>
        /// <param name="sqlgeometry"></param>
        /// <returns>true; false</returns>
        public static bool IsCurvePolygon(this SqlGeometry sqlgeometry)
        {
            return sqlgeometry.STGeometryType().Compare(OGCType.CurvePolygon.GetString());
        }

        /// <summary>
        /// Check if Geometry is GeometryCollection
        /// </summary>
        /// <param name="sqlgeometry"></param>
        /// <returns>true; false</returns>
        public static bool IsGeometryCollection(this SqlGeometry sqlgeometry)
        {
            return sqlgeometry.STGeometryType().Compare(OGCType.GeometryCollection.GetString());
        }

        /// <summary>
        /// Check if Geometry is MultiPoint
        /// </summary>
        /// <param name="sqlgeometry"></param>
        /// <returns>true; false</returns>
        public static bool IsMultiPoint(this SqlGeometry sqlgeometry)
        {
            return sqlgeometry.STGeometryType().Compare(OGCType.MultiPoint.GetString());
        }

        /// <summary>
        /// Check if Geometry is MultiLineString
        /// </summary>
        /// <param name="sqlgeometry"></param>
        /// <returns>true; false</returns>
        public static bool IsMultiLineString(this SqlGeometry sqlgeometry)
        {
            return sqlgeometry.STGeometryType().Compare(OGCType.MultiLineString.GetString());
        }

        /// <summary>
        /// Check if Geometry is MultiPolygon
        /// </summary>
        /// <param name="sqlgeometry"></param>
        /// <returns>true; false</returns>
        public static bool IsMultiPolygon(this SqlGeometry sqlgeometry)
        {
            return sqlgeometry.STGeometryType().Compare(OGCType.MultiPolygon.GetString());
        }

        #endregion

        #region Measures Tolerance

        /// <summary>
        /// Get start point measure of Geometry
        /// </summary>
        /// <param name="sqlgeometry">Input Geometry</param>
        /// <returns></returns>
        public static double GetStartPointMeasure(this SqlGeometry sqlgeometry)
        {
            return sqlgeometry.STStartPoint().M.IsNull
                    ? 0
                    : sqlgeometry.STStartPoint().M.Value;
        }

        /// <summary>
        /// Get end point measure of Geometry
        /// </summary>
        /// <param name="sqlgeometry">Input Geometry</param>
        /// <returns></returns>
        public static double GetEndPointMeasure(this SqlGeometry sqlgeometry)
        {
            return sqlgeometry.STEndPoint().M.IsNull
                    ? sqlgeometry.STLength().Value
                    : sqlgeometry.STEndPoint().M.Value;
        }

        /// <summary>
        /// Compares measure values of each point in a LineString; if the two geom segments are equal.
        /// </summary>
        /// <param name="sqlGeometry">Input Line Segment</param>
        /// <param name="targetGeometry">Target Line Segment</param>
        /// <returns></returns>
        public static bool STEqualsMeasure(this SqlGeometry sqlGeometry, SqlGeometry targetGeometry)
        {
            if (!(sqlGeometry.IsLineString() || targetGeometry.IsLineString()))
                return false;
            if (sqlGeometry.STIsEmpty() || targetGeometry.STIsEmpty())
                return false;

            // check if two geoms are equal
            if (!sqlGeometry.STEquals(targetGeometry))
                return false;

            var inputNumPoints = sqlGeometry.STNumPoints();
            var targetNumPoints = targetGeometry.STNumPoints();

            if (inputNumPoints != targetNumPoints)
                return false;

            for (var pointIterator = 1; pointIterator <= inputNumPoints; pointIterator++)
            {
                var sourcePoint = sqlGeometry.STPointN(pointIterator);
                var targetPoint = targetGeometry.STPointN(pointIterator);

                // when both source and target point has null measure; then they are equal.
                if (!sourcePoint.HasM && !targetPoint.HasM)
                    continue;

                // when source has measure and target doesn't have measure then they are not equal
                // vice a versa
                if ((sourcePoint.HasM && !targetPoint.HasM) || (!sourcePoint.HasM && targetPoint.HasM))
                    return false;

                if (sourcePoint.M != targetPoint.M)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Loop through individual points in geometry and check whether it has the measure values.
        /// </summary>
        /// <param name="geometry">The geometry.</param>
        /// <returns></returns>
        public static bool STHasMeasureValues(this SqlGeometry geometry)
        {
            if (geometry.IsNull || geometry.STIsEmpty() || !geometry.STIsValid())
                return false;

            var numPoints = geometry.STNumPoints();

            for (var iterator = 1; iterator <= numPoints; iterator++)
            {
                if (!geometry.STPointN(iterator).HasM)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Get the linear progression of the geometry based on measure value; whether increasing or decreasing.
        /// </summary>
        /// <param name="geometry">The input SqlGeometry.</param>
        /// <returns>Increasing or Decreasing</returns>
        public static LinearMeasureProgress STLinearMeasureProgress(this SqlGeometry geometry)
        {
            if (geometry.IsNull || geometry.STIsEmpty())
                return LinearMeasureProgress.None;

            if (geometry.IsPoint())
                return LinearMeasureProgress.Increasing;

            var startPoint = geometry.STStartPoint();
            var endPoint = geometry.STEndPoint();
            if (!geometry.STStartPoint().HasM || !geometry.STEndPoint().HasM)
                return LinearMeasureProgress.None;

            return (endPoint.M - startPoint.M > 0 ? LinearMeasureProgress.Increasing : LinearMeasureProgress.Decreasing);
        }

        /// <summary>
        /// Gets the distance between two x,y co-ordinates.
        /// </summary>
        /// <param name="x1">The x1.</param>
        /// <param name="y1">The y1.</param>
        /// <param name="x2">The x2.</param>
        /// <param name="y2">The y2.</param>
        /// <returns></returns>
        public static double GetDistance(double x1, double y1, double x2, double y2)
        {
            var xCoordinateDiff = Math.Pow(Math.Abs(x2 - x1), 2);
            var yCoordinateDiff = Math.Pow(Math.Abs(y2 - y1), 2);
            return Math.Sqrt(xCoordinateDiff + yCoordinateDiff);
        }

        /// <summary>
        /// Determines whether the specified distance is within tolerance.
        /// </summary>
        /// <param name="distance">The distance.</param>
        /// <param name="tolerance">The tolerance.</param>
        /// <returns>
        ///   <c>true</c> if the specified distance is tolerable; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsTolerable(this SqlDouble distance, double tolerance)
        {
            return IsTolerable((double)distance, tolerance);
        }

        /// <summary>
        /// Determines whether the specified distance is within tolerance.
        /// </summary>
        /// <param name="distance">The distance.</param>
        /// <param name="tolerance">The tolerance.</param>
        /// <returns>
        ///   <c>true</c> if the specified distance is tolerable; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsTolerable(this double distance, double tolerance)
        {
            var digitToCompare = distance;
            var decimalStr = tolerance.ToString(CultureInfo.CurrentCulture);
            var decimalIndex = decimalStr.IndexOf(".", StringComparison.CurrentCulture);

            if (decimalIndex > 0)
            {
                var decDigits = decimalStr.Substring(decimalIndex).Length;
                digitToCompare = Math.Round(distance, decDigits);
            }

            return digitToCompare <= tolerance;
        }

        /// <summary>
        /// Determines whether the distance between start and point is tolerable.
        /// </summary>
        /// <param name="startPoint">The start point.</param>
        /// <param name="endPoint">The end point.</param>
        /// <param name="tolerance">The tolerance.</param>
        /// <returns>
        ///   <c>true</c> if the specified start and end point distance is tolerable; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsTolerable(this SqlGeometry startPoint, SqlGeometry endPoint, double tolerance)
        {
            return IsTolerable(GetDistance(startPoint.STX.Value, startPoint.STY.Value, endPoint.STX.Value, endPoint.STY.Value), tolerance);
        }

        /// <summary>
        /// Determines whether the distance between 2 x,y points is within tolerance
        /// </summary>
        /// <param name="x1">The x1.</param>
        /// <param name="y1">The y1.</param>
        /// <param name="x2">The x2.</param>
        /// <param name="y2">The y2.</param>
        /// <param name="tolerance">The tolerance.</param>
        /// <returns>
        ///   <c>true</c> if distance between 2 points are within tolerance; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsTwoPointsWithinTolerance(double x1, double y1, double x2, double y2, double tolerance)
        {
            return GetDistance(x1, y1, x2, y2).IsTolerable(tolerance);
        }

        #endregion

        #region Range Validations

        /// <summary>
        /// Check whether the measure falls withing the start and end measure.
        /// </summary>
        /// <param name="currentMeasure"></param>
        /// <param name="startMeasure"></param>
        /// <param name="endMeasure"></param>
        /// <returns></returns>
        public static bool IsWithinRange(this double? currentMeasure, double startMeasure, double endMeasure)
        {
            if (currentMeasure == null)
                return false;

            return IsWithinRange((double)currentMeasure, startMeasure, endMeasure);
        }

        /// <summary>
        /// Check whether the measure falls withing the start and end measure.
        /// </summary>
        /// <param name="currentMeasure"></param>
        /// <param name="startMeasure"></param>
        /// <param name="endMeasure"></param>
        /// <returns></returns>
        public static bool IsWithinRange(this double currentMeasure, double startMeasure, double endMeasure)
        {
            return (
                // if line segment measure is increasing start ----> end
                (currentMeasure >= startMeasure && currentMeasure <= endMeasure)
                ||
                // if line segment measure is increasing start <---- end
                (currentMeasure >= endMeasure && currentMeasure <= startMeasure)
                );
        }

        /// <summary>
        /// Check whether the measure falls withing the start and end measure of geometry.
        /// </summary>
        /// <param name="currentMeasure"></param>
        /// <param name="startMeasure"></param>
        /// <param name="endMeasure"></param>
        /// <returns></returns>
        public static bool IsWithinRange(this double currentMeasure, SqlGeometry sqlGeometry)
        {
            var startMeasure = sqlGeometry.GetStartPointMeasure();
            var endMeasure = sqlGeometry.GetEndPointMeasure();
            return IsWithinRange(currentMeasure, startMeasure, endMeasure);
        }

        /// <summary>
        /// Check whether the measure falls between start and end geometry points
        /// </summary>
        /// <param name="currentMeasure"></param>
        /// <param name="startMeasure"></param>
        /// <param name="endMeasure"></param>
        /// <returns></returns>
        public static bool IsWithinRange(this double currentMeasure, SqlGeometry startPoint, SqlGeometry endPoint)
        {
            var startMeasure = startPoint.GetStartPointMeasure();
            var endMeasure = endPoint.GetEndPointMeasure();
            return IsWithinRange(currentMeasure, startMeasure, endMeasure);
        }

        /// <summary>
        /// Checks whether difference of X and Y point between source and point to compare is within tolerable range
        /// </summary>
        /// <param name="sourcePoint"></param>
        /// <param name="pointToCompare"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public static bool IsXYWithinRange(this SqlGeometry sourcePoint, SqlGeometry pointToCompare, double tolerance = 0.0F)
        {
            return Math.Abs(sourcePoint.STX.Value - pointToCompare.STX.Value) <= tolerance &&
                   Math.Abs(sourcePoint.STY.Value - pointToCompare.STY.Value) <= tolerance;
        }

        /// <summary>
        /// Determines whether the measure falls beyond the range
        /// </summary>
        /// <param name="currentMeasure">The current measure.</param>
        /// <param name="startMeasure">The start measure.</param>
        /// <param name="endMeasure">The end measure.</param>
        /// <returns>
        ///   <c>true</c> if [is beyond range] [the specified start measure]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsBeyondRange(this double currentMeasure, double startMeasure, double endMeasure)
        {
            return (currentMeasure > startMeasure && currentMeasure > endMeasure || currentMeasure < startMeasure && currentMeasure < endMeasure);
        }

        /// <summary>
        /// Determines whether the measure falls beyond the start and end measure of geometry.
        /// </summary>
        /// <param name="currentMeasure">The current measure.</param>
        /// <param name="sqlGeometry">The SQL geometry.</param>
        /// <returns>
        ///   <c>true</c> if [is beyond range] [the specified SQL geometry]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsBeyondRange(this double currentMeasure, SqlGeometry sqlGeometry)
        {
            var startMeasure = sqlGeometry.GetStartPointMeasure();
            var endMeasure = sqlGeometry.GetEndPointMeasure();
            return IsBeyondRange(currentMeasure, startMeasure, endMeasure);
        }

        /// <summary>
        /// Determines whether the input start and end measures are beyond geometry start and end point measures.
        /// </summary>
        /// <param name="sqlGeometry">The SQL geometry.</param>
        /// <param name="startMeasure">The start measure.</param>
        /// <param name="endMeasure">The end measure.</param>
        /// <returns>
        ///   <c>true</c> if measures beyond geom measure; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsMeasuresBeyondGeom(this SqlGeometry sqlGeometry, double startMeasure, double endMeasure)
        {
            var inputGeomMeasureDifference = Math.Abs(sqlGeometry.GetStartPointMeasure() - sqlGeometry.GetEndPointMeasure());
            var clipMeasureDifference = Math.Abs(startMeasure - endMeasure);
            return clipMeasureDifference > inputGeomMeasureDifference;
        }

        /// <summary>
        /// Determines whether the input start or end measure matches the start or end point measure of geometry.
        /// </summary>
        /// <param name="geomStartMeasure">The geom start measure.</param>
        /// <param name="geomEndMeasure">The geom end measure.</param>
        /// <param name="inputStartMeasure">The input start measure.</param>
        /// <param name="inputEndMeasure">The input end measure.</param>
        /// <returns>
        ///   <c>true</c> if input measure matches end or start point measure of geometry; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsExtremeMeasuresMatch(double geomStartMeasure, double geomEndMeasure, double inputStartMeasure, double inputEndMeasure)
        {
            var inputStartCheck1 = inputStartMeasure.Equals(geomStartMeasure);
            var inputStartCheck2 = inputStartMeasure.Equals(geomEndMeasure);

            var inputEndCheck1 = inputEndMeasure.Equals(geomStartMeasure);
            var inputEndCheck2 = inputEndMeasure.Equals(geomEndMeasure);

            return (inputStartCheck1 || inputStartCheck2) || (inputEndCheck1 || inputEndCheck2);
        }

        /// <summary>
        /// Get offset measure between the two geometry.
        /// Subtracts end measure of first geometry against the first measure of second geometry.
        /// </summary>
        /// <param name="sqlgeometry1">First Geometry</param>
        /// <param name="sqlgeometry2">Second Geometry</param>
        /// <returns></returns>
        public static double? GetOffset(this SqlGeometry sqlgeometry1, SqlGeometry sqlgeometry2)
        {
            return sqlgeometry1.GetEndPointMeasure() - sqlgeometry2.GetStartPointMeasure();
        }

        #endregion       

        #region Sql Types Comparison with Double

        /// <summary>
        /// Compares sql string for equality.
        /// </summary>
        /// <param name="sqlString">SQL string</param>
        /// <param name="targetString">Target OGC type string to compare</param>
        /// <returns></returns>
        public static bool Compare(this SqlString sqlString, string targetString)
        {
            if (string.IsNullOrEmpty(targetString))
                return false;

            string convertString = sqlString.ToString();

            return string.IsNullOrEmpty(convertString) ? false : convertString.ToLowerInvariant().Equals(targetString.ToLowerInvariant());
        }

        /// <summary>
        /// Compares sql double for double.
        /// </summary>
        /// <param name="sqlDouble">SQL double</param>
        /// <param name="compareValue">Compare Value</param>
        /// <returns></returns>
        public static bool Compare(this SqlDouble sqlDouble, double compareValue)
        {
            return (double)sqlDouble == compareValue;
        }

        /// <summary>
        /// Compares sql boolean for boolean.
        /// </summary>
        /// <param name="sqlBoolean">SQL Boolean</param>
        /// <param name="compareValue">Compare Value</param>
        /// <returns></returns>
        public static bool Compare(this SqlBoolean sqlBool, bool compareValue)
        {
            return (bool)sqlBool == compareValue;
        }

        /// <summary>
        /// Compares sql int for int.
        /// </summary>
        /// <param name="sqlInt32">SQL int 32</param>
        /// <param name="compareValue">Compare Value</param>
        /// <returns></returns>
        public static bool Compare(this SqlInt32 sqlInt, int compareValue)
        {
            return (int)sqlInt == compareValue;
        }

        #endregion

        #region Enum Attribute Extension

        /// <summary>
        /// Will get the string value for a given enums value, this will
        /// only work if you assign the StringValue attribute to
        /// the items in your enum.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetString(this OGCType value)
        {
            return GetStringAttributeValue<OGCType>(value);
        }

        /// <summary>
        /// Will get the string value for a given enums value, this will
        /// only work if you assign the StringValue attribute to
        /// the items in your enum.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetString(this DimensionalInfo value)
        {
            return GetStringAttributeValue<DimensionalInfo>(value);
        }

        /// <summary>
        /// Gets the string attribute value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        private static string GetStringAttributeValue<T>(this T value)
        {
            // Get the type
            Type type = value.GetType();

            // Get field info for this type
            FieldInfo fieldInfo = type.GetField(value.ToString());

            // Get the string value attributes
            StringValueAttribute[] attribs = fieldInfo.GetCustomAttributes(
                typeof(StringValueAttribute), false) as StringValueAttribute[];

            // Return the first if there was a match.
            return attribs.Length > 0 ? attribs[0].StringValue : null;
        }

        /// <summary>  
        /// Get the int value of enum
        /// </summary>
        /// <param name="linearMeasureProgress">The linear measure progress.</param>
        /// <returns>Returns the implicit integer value of respective enum</returns>
        public static short Value(this LinearMeasureProgress linearMeasureProgress)
        {
            return (short)linearMeasureProgress;
        }

        /// <summary>
        /// Get the string value for the specified LRS Error Code.
        /// </summary>
        /// <param name="lrsErrorCodes">The LRS error codes.</param>
        /// <returns>String Value of LRS Error Code.</returns>
        public static string Value(this LRSErrorCodes lrsErrorCodes)
        {
            return lrsErrorCodes == LRSErrorCodes.Valid ? "TRUE" : ((short)lrsErrorCodes).ToString();
        }

        #endregion        

        #region Exception Handling

        /// <summary>
        /// Throw if input geometry is not a LRS Geometry collection POINT, LINESTRING or MULTILINESTRING.
        /// </summary>
        /// <param name="sqlGeometry">Input Sql Geometry</param>
        internal static void ThrowIfNotLRSType(params SqlGeometry[] sqlGeometry)
        {
            foreach (var geom in sqlGeometry)
            {
                if (!geom.IsLRSType())
                    throw new ArgumentException(ErrorMessage.LRSCompatible);
            }
        }

        /// <summary>
        /// Throw if input geometry is not a Point.
        /// </summary>
        /// <param name="sqlGeometry">Input Sql Geometry</param>
        /// <param name="sqlGeometries">Sql Geometries</param>
        internal static void ThrowIfNotPoint(SqlGeometry sqlGeometry, params SqlGeometry[] sqlGeometries)
        {
            var geoms = (new SqlGeometry[] { sqlGeometry }).Concat(sqlGeometries);
            foreach (var geom in geoms)
            {
                if (!geom.IsOfSupportedTypes(OpenGisGeometryType.Point))
                    throw new ArgumentException(ErrorMessage.PointCompatible);
            }
        }

        /// <summary>
        /// Throw if input geometry is not a line string.
        /// </summary>
        /// <param name="sqlGeometry">Input Sql Geometry</param>
        /// <param name="sqlGeometries">Sql Geometries</param>
        internal static void ThrowIfNotLine(SqlGeometry sqlGeometry, params SqlGeometry[] sqlGeometries)
        {
            var geoms = (new SqlGeometry[] { sqlGeometry }).Concat(sqlGeometries);
            foreach (var geom in geoms)
            {
                if (!geom.IsOfSupportedTypes(OpenGisGeometryType.LineString))
                    throw new ArgumentException(ErrorMessage.LineStringCompatible);
            }
        }

        /// <summary>
        /// Throw if input geometry is not a line string or multiline string.
        /// </summary>
        /// <param name="sqlGeometry">Input Sql Geometry</param>
        /// <param name="sqlGeometries">Sql Geometries</param>
        internal static void ThrowIfNotLineOrMultiLine(SqlGeometry sqlGeometry, params SqlGeometry[] sqlGeometries)
        {
            var geoms = (new SqlGeometry[] { sqlGeometry }).Concat(sqlGeometries);
            foreach (var geom in geoms)
            {
                if (!geom.IsOfSupportedTypes(OpenGisGeometryType.LineString, OpenGisGeometryType.MultiLineString))
                    throw new ArgumentException(ErrorMessage.LineOrMultiLineStringCompatible);
            }
        }

        /// <summary>
        /// Throw if SRIDs of two geometries doesn't match.
        /// </summary>
        /// <param name="sourceGeometry">Source Sql Geometry</param>
        /// <param name="targetGeometry">Target Sql Geometry</param>
        internal static void ThrowIfSRIDsDoesNotMatch(SqlGeometry sourceGeometry, SqlGeometry targetGeometry)
        {
            if (!sourceGeometry.STSrid.Equals(targetGeometry.STSrid))
                throw new ArgumentException(ErrorMessage.SRIDCompatible);
        }

        /// <summary>
        /// Throw if measure is not withing the range of two geometries.
        /// </summary>
        /// <param name="measure">Measure</param>
        /// <param name="startPoint">Start Point Sql Geometry</param>
        /// <param name="endPoint">End Point Sql Geometry</param>
        internal static void ThrowIfMeasureIsNotInRange(double measure, SqlGeometry startPoint, SqlGeometry endPoint)
        {
            if (!measure.IsWithinRange(startPoint, endPoint))
                throw new ArgumentException(ErrorMessage.MeasureRange);
        }

        /// <summary>
        /// Throw if measure is not withing the range of two geometries.
        /// </summary>
        /// <param name="measure">Measure</param>
        /// <param name="sqlGeometry">Input Sql Geometry</param>
        internal static void ThrowIfMeasureIsNotInRange(double measure, SqlGeometry sqlGeometry)
        {
            if (!measure.IsWithinRange(sqlGeometry))
                throw new ArgumentException(ErrorMessage.MeasureRange);
        }

        /// <summary>
        /// Throw if start measure is not withing the range of two geometries.
        /// </summary>
        /// <param name="startMeasure">Start Measure</param>
        /// <param name="endMeasure">End Measure</param>
        /// <param name="sqlGeometry">Input Sql Geometry</param>
        internal static void ThrowIfStartMeasureIsNotInRange(double startMeasure, double endMeasure, SqlGeometry sqlGeometry)
        {
            if (!startMeasure.IsWithinRange(sqlGeometry))
                ThrowException("Start measure {0}", startMeasure.LinearGeometryRangeExpectionMessage(startMeasure, endMeasure));
        }

        /// <summary>
        /// Throw if end measure is not withing the range of two geometries.
        /// </summary>
        /// <param name="startMeasure">Start Measure</param>
        /// <param name="endMeasure">End Measure</param>
        /// <param name="sqlGeometry">Input Sql Geometry</param>
        internal static void ThrowIfEndMeasureIsNotInRange(double startMeasure, double endMeasure, SqlGeometry sqlGeometry)
        {
            if (!endMeasure.IsWithinRange(sqlGeometry))
                ThrowException("End measure {0}", endMeasure.LinearGeometryRangeExpectionMessage(startMeasure, endMeasure));
        }

        /// <summary>
        /// Check if the geometry collection is of POINT, LINSTRING, MULTILINESTRING.
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        public static bool IsLRSType(this SqlGeometry geometry)
        {
            return geometry.IsOfSupportedTypes(OpenGisGeometryType.Point, OpenGisGeometryType.LineString, OpenGisGeometryType.MultiLineString);
        }

        /// <summary>
        /// Check if the input geometry is of the supported specified type
        /// </summary>
        /// <param name="geometry"></param>
        /// <param name="supportedType"></param>
        /// <param name="additionalSupportedTypes"></param>
        /// <returns></returns>
        public static bool IsOfSupportedTypes(this SqlGeometry geometry, OpenGisGeometryType supportedType, params OpenGisGeometryType[] additionalSupportedTypes)
        {
            var geomSink = new GISTypeCheckGeometrySink((new[] { supportedType }).Concat(additionalSupportedTypes).ToArray());
            geometry.Populate(geomSink);
            return geomSink.IsCompatible();
        }

        /// <summary>
        /// Get exception message when measure exceeds the range.
        /// </summary>
        /// <param name="measure"></param>
        /// <param name="startMeasure"></param>
        /// <param name="endMeasure"></param>
        /// <returns></returns>
        public static string LinearGeometryRangeExpectionMessage(this double measure, double startMeasure, double endMeasure)
        {
            return string.Format(CultureInfo.CurrentCulture, ErrorMessage.LinearGeometryMeasureMustBeInRange, measure, Math.Min(startMeasure, endMeasure).ToString(CultureInfo.CurrentCulture), Math.Max(startMeasure, endMeasure).ToString(CultureInfo.CurrentCulture));
        }

        /// <summary>
        /// Throws ArgumentException based on message format and parameters
        /// </summary>
        /// <param name="messageFormat">Message format</param>
        /// <param name="args">Arguments to be appended with format</param>
        public static void ThrowException(string messageFormat, params string[] args)
        {
            throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, messageFormat, args));
        }

        #endregion

        #region Dimensions

        /// <summary>
        /// Returns true if the input type is in the list of supported types.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="geometryTypes"></param>
        /// <returns></returns>
        public static bool Contains(this OpenGisGeometryType type, OpenGisGeometryType[] geometryTypes)
        {
            foreach (var geom in geometryTypes)
            {
                if (type.Equals(geom))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the dimension info of input geometry
        /// </summary>
        /// <param name="sqlGeometry">The SQL geometry.</param>
        /// <returns>Dimensional Info 2D, 2DM, 3D or 3DM</returns>
        public static DimensionalInfo STGetDimension(this SqlGeometry sqlGeometry)
        {
            // STNumpoint can be performed only on valid geometries.
            if (sqlGeometry.STIsValid() && sqlGeometry.STNumPoints() > 0)
            {
                var firstPoint = sqlGeometry.STPointN(1);
                if (firstPoint.Z.IsNull && firstPoint.M.IsNull)
                    return DimensionalInfo._2D;

                if (firstPoint.Z.IsNull && !firstPoint.M.IsNull)
                    return DimensionalInfo._2DM;

                if (!firstPoint.Z.IsNull && firstPoint.M.IsNull)
                    return DimensionalInfo._3D;

                if (!firstPoint.Z.IsNull && !firstPoint.M.IsNull)
                    return DimensionalInfo._3DM;
            }

            return DimensionalInfo.None;
        }

        /// <summary>
        /// Validate and convert to LRS dimension
        /// </summary>
        /// <param name="sqlGeometry">Input SQL Geometry</param>
        internal static void ValidateLRSDimensions(ref SqlGeometry sqlGeometry)
        {
            var dimension = sqlGeometry.STGetDimension();

            switch (dimension)
            {
                case DimensionalInfo._3DM:
                case DimensionalInfo._2DM:
                    return;
                // if dimension is of x, y and z
                // need to convert third z co-ordinate to M for LRS
                case DimensionalInfo._3D:
                    sqlGeometry = sqlGeometry.ConvertTo2DM();
                    break;
                case DimensionalInfo._2D:
                    ThrowException("Cannot operate on 2 Dimensional co-ordinates without measure values");
                    break;
                // skip for invalid types where Dimensional information can't be inferred
                default:
                    return;
            }
        }

        #endregion

        #region Others

        /// <summary>
        /// Convert Sql geometry with x,y,z to x,y,m
        /// </summary>
        /// <param name="sqlGeometry">Sql Geometry</param>
        /// <returns></returns>
        internal static SqlGeometry ConvertTo2DM(this SqlGeometry sqlGeometry)
        {
            var sqlBuilder = new ConvertXYZ2XYM();
            sqlGeometry.Populate(sqlBuilder);
            return sqlBuilder.ConstructedGeometry;
        }

        /// <summary>
        /// Construct SqlGeometry Point from x, y, z, m values
        /// </summary>
        /// <param name="x">x Coordinate</param>
        /// <param name="y">y Coordinate</param>
        /// <param name="z">z Coordinate</param>
        /// <param name="m">Measure</param>
        /// <param name="srid">Spatial Reference Identifier; Default is 4326</param>
        /// <returns>Sql Point Geometry</returns>
        public static SqlGeometry GetPoint(double x, double y, double? z, double? m, int srid = Constants.DefaultSRID)
        {
            var zCoordinate = z == null ? "NULL" : z.ToString();
            var geometry = string.Format(CultureInfo.CurrentCulture, Constants.PointSqlCharFormat, x, y, zCoordinate, m);
            return SqlGeometry.STPointFromText(new SqlChars(geometry), srid);
        }

        /// <summary>
        /// Gets the coordinates of the point
        /// </summary>
        /// <param name="geometry">The geometry.</param>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="z">The z.</param>
        /// <param name="m">The m.</param>
        public static void GetCoordinates(this SqlGeometry geometry, out double x, out double y, out double? z, out double? m)
        {
            ThrowIfNotPoint(geometry);

            z = null;
            m = null;

            x = geometry.STX.Value;
            y = geometry.STY.Value;

            if (geometry.HasZ)
                z = geometry.Z.Value;

            if (geometry.HasM)
                m = geometry.M.Value;
        }

        /// <summary>
        /// Converts WKT string to SqlGeometry object.
        /// </summary>
        /// <param name="geomString">geometry in string representation</param>
        /// <param name="srid">Spatial reference identifier; Default for SQL Server 4326</param>
        /// <returns>SqlGeometry</returns>
        public static SqlGeometry GetGeom(this string geomString, int srid = Constants.DefaultSRID)
        {
            return SqlGeometry.STGeomFromText(new SqlChars(geomString), srid);
        }

        /// <summary>
        /// Convert WKT string to SqlGeography object.
        /// </summary>
        /// <param name="geogString">geography in WKT string representation</param>
        /// <param name="srid">Spatial reference identifier; Default for SQL Server 4326</param>
        /// <returns>SqlGeography</returns>
        public static SqlGeography GetGeog(this string geogString, int srid = Constants.DefaultSRID)
        {
            return SqlGeography.STGeomFromText(new SqlChars(geogString), srid);
        }

        #endregion
    }
}
