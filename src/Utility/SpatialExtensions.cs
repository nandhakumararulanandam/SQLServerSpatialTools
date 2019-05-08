using System;
using System.Data.SqlTypes;
using System.Linq;
using Microsoft.SqlServer.Types;
using System.Globalization;
using SQLSpatialTools.Sinks.Geometry;
using SQLSpatialTools.Types;

namespace SQLSpatialTools.Utility
{
    public static class SpatialExtensions
    {
        #region OGC Type Checks

        /// <summary>
        /// Check if Geometry is of type point,
        /// it returns true, if the geometry is of type point
        /// Instead of checking the STGeometryType directly, this utility method parses the points of the geometry, which make sure it returns true, even if the geometry contains invalid co-ordinates
        /// </summary>
        /// <param name="sqlGeometry"></param>
        /// <returns></returns>
        public static bool CheckGeomPoint(this SqlGeometry sqlGeometry)
        {
            try
            {
                return GetPoint(sqlGeometry).STGeometryType().Compare(OGCType.Point.GetString());
            }
            catch (SqlNullValueException)
            {
                return false;
            }
        }

        /// <summary>
        /// Check if Geometry is Point
        /// </summary>
        /// <param name="sqlGeometry"></param>
        /// <returns></returns>
        public static bool IsPoint(this SqlGeometry sqlGeometry)
        {
            return sqlGeometry.STGeometryType().Compare(OGCType.Point.GetString());
        }

        /// <summary>
        /// Check if Geography type is Point
        /// </summary>
        /// <param name="sqlGeography">SQL Geography</param>
        /// <returns></returns>
        public static bool IsPoint(this SqlGeography sqlGeography)
        {
            return sqlGeography.STGeometryType().Compare(OGCType.Point.GetString());
        }

        /// <summary>
        /// Check if Geometry is LineString
        /// </summary>
        /// <param name="sqlGeometry"></param>
        /// <returns>true; false</returns>
        public static bool IsLineString(this SqlGeometry sqlGeometry)
        {
            return sqlGeometry.STGeometryType().Compare(OGCType.LineString.GetString());
        }

        /// <summary>
        /// Check if Geometry is CircularString
        /// </summary>
        /// <param name="sqlGeometry"></param>
        /// <returns>true; false</returns>
        public static bool IsCircularString(this SqlGeometry sqlGeometry)
        {
            return sqlGeometry.STGeometryType().Compare(OGCType.CircularString.GetString());
        }

        /// <summary>
        /// Check if Geometry is CompoundCurve
        /// </summary>
        /// <param name="sqlGeometry"></param>
        /// <returns>true; false</returns>
        public static bool IsCompoundCurve(this SqlGeometry sqlGeometry)
        {
            return sqlGeometry.STGeometryType().Compare(OGCType.CompoundCurve.GetString());
        }

        /// <summary>
        /// Check if Geometry is Polygon
        /// </summary>
        /// <param name="sqlGeometry"></param>
        /// <returns>true; false</returns>
        public static bool IsPolygon(this SqlGeometry sqlGeometry)
        {
            return sqlGeometry.STGeometryType().Compare(OGCType.Polygon.GetString());
        }

        /// <summary>
        /// Check if Geometry is CurvePolygon
        /// </summary>
        /// <param name="sqlGeometry"></param>
        /// <returns>true; false</returns>
        public static bool IsCurvePolygon(this SqlGeometry sqlGeometry)
        {
            return sqlGeometry.STGeometryType().Compare(OGCType.CurvePolygon.GetString());
        }

        /// <summary>
        /// Check if Geometry is GeometryCollection
        /// </summary>
        /// <param name="sqlGeometry"></param>
        /// <returns>true; false</returns>
        public static bool IsGeometryCollection(this SqlGeometry sqlGeometry)
        {
            return sqlGeometry.STGeometryType().Compare(OGCType.GeometryCollection.GetString());
        }

        /// <summary>
        /// Check if Geometry is MultiPoint
        /// </summary>
        /// <param name="sqlGeometry"></param>
        /// <returns>true; false</returns>
        public static bool IsMultiPoint(this SqlGeometry sqlGeometry)
        {
            return sqlGeometry.STGeometryType().Compare(OGCType.MultiPoint.GetString());
        }

        /// <summary>
        /// Check if Geometry is MultiLineString
        /// </summary>
        /// <param name="sqlGeometry"></param>
        /// <returns>true; false</returns>
        public static bool IsMultiLineString(this SqlGeometry sqlGeometry)
        {
            return sqlGeometry.STGeometryType().Compare(OGCType.MultiLineString.GetString());
        }

        /// <summary>
        /// Check if Geometry is MultiPolygon
        /// </summary>
        /// <param name="sqlGeometry"></param>
        /// <returns>true; false</returns>
        public static bool IsMultiPolygon(this SqlGeometry sqlGeometry)
        {
            return sqlGeometry.STGeometryType().Compare(OGCType.MultiPolygon.GetString());
        }

        #endregion

        #region Measures Tolerance

        /// <summary>
        /// Get start point measure of Geometry
        /// </summary>
        /// <param name="sqlGeometry">Input Geometry</param>
        /// <returns></returns>
        public static double GetStartPointMeasure(this SqlGeometry sqlGeometry)
        {
            return sqlGeometry.STStartPoint().M.IsNull
                    ? 0
                    : sqlGeometry.STStartPoint().M.Value;
        }

        /// <summary>
        /// Get end point measure of Geometry
        /// </summary>
        /// <param name="sqlGeometry">Input Geometry</param>
        /// <returns></returns>
        public static double GetEndPointMeasure(this SqlGeometry sqlGeometry)
        {
            return sqlGeometry.STEndPoint().M.IsNull
                    ? sqlGeometry.STLength().Value
                    : sqlGeometry.STEndPoint().M.Value;
        }

        /// <summary>
        /// Determines whether the geometry segment is not null or empty.
        /// </summary>
        /// <param name="sqlGeometry">The SQL geometry.</param>
        /// <returns>
        ///   <c>true</c> if [is not null or empty] [the specified SQL geometry]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNotNullOrEmpty(this SqlGeometry sqlGeometry)
        {
            return !sqlGeometry.IsNullOrEmpty();
        }

        /// <summary>
        /// Determines whether the geometry segment is null or empty.
        /// </summary>
        /// <param name="sqlGeometry">The SQL geometry.</param>
        /// <returns>
        ///   <c>true</c> if [is null or empty] [the specified SQL geometry]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNullOrEmpty(this SqlGeometry sqlGeometry)
        {
            if (sqlGeometry == null)
                return true;
            return (bool)(sqlGeometry.IsNull || sqlGeometry.STIsEmpty());
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

            // check if two geometries are equal
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
        /// for checking whether geometry measure is increasing or decreasing 
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        public static bool STHasLinearMeasure(this SqlGeometry geometry)
        {
            if (geometry.IsNullOrEmpty() || !geometry.STIsValid())
                return false;

            var numPoints = geometry.STNumPoints();
            var measureProgress = geometry.STLinearMeasureProgress();

            var previousM = 0.0;
            for (var iterator = 1; iterator <= numPoints; iterator++)
            {
                var currentPoint = geometry.STPointN(iterator);
                if (!currentPoint.HasM)
                    return false;

                if (iterator == 1)
                    continue;

                switch (measureProgress)
                {
                    case LinearMeasureProgress.Increasing when previousM > currentPoint.M:
                    case LinearMeasureProgress.Decreasing when previousM < currentPoint.M:
                        return false;
                    case LinearMeasureProgress.None:
                        break;
                    default:
                        previousM = currentPoint.M.Value;
                        break;
                }
            }

            return true;
        }

        /// <summary>
        /// To check whether start measure value is equal to End Measure value
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns>equal measure value</returns>
        public static bool STHasEqualStartAndEndMeasure(this SqlGeometry geometry)
        {
            return Math.Abs(geometry.GetStartPointMeasure() - geometry.GetEndPointMeasure()) < 0.0000001;
        }

        /// <summary>
        /// Get the linear progression of the geometry based on measure value; whether increasing or decreasing.
        /// </summary>
        /// <param name="geometry">The input SqlGeometry.</param>
        /// <returns>Increasing or Decreasing</returns>
        public static LinearMeasureProgress STLinearMeasureProgress(this SqlGeometry geometry)
        {
            if (geometry.IsNullOrEmpty())
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
        public static bool IsTolerable(this double distance, double tolerance)
        {
            var digitToCompare = distance;
            var decDigits = tolerance.GetPrecisionLength();

            if (decDigits > 0)
                digitToCompare = Math.Round(distance, decDigits);

            return digitToCompare <= tolerance;
        }

        /// <summary>
        /// Gets the precision length
        /// </summary>
        /// <param name="decimalDigit">The decimal digit.</param>
        /// <returns></returns>
        public static int GetPrecisionLength(this double decimalDigit)
        {
            var decimalStr = decimalDigit.ToString(CultureInfo.CurrentCulture);
            var decimalIndex = decimalStr.IndexOf(".", StringComparison.CurrentCulture);

            return decimalIndex > 0 ? decimalStr.Substring(decimalIndex).Length : 0;
        }

        /// <summary>
        /// Determines whether the x y distance between start and point is tolerable.
        /// </summary>
        /// <param name="startPoint">The start point.</param>
        /// <param name="endPoint">The end point.</param>
        /// <param name="tolerance">The tolerance.</param>
        /// <returns>
        ///   <c>true</c> if the specified start and end point distance is tolerable; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsWithinTolerance(this SqlGeometry startPoint, SqlGeometry endPoint, double tolerance)
        {
            return IsXYWithinRange(startPoint, endPoint, tolerance);
        }

        /// <summary>
        /// Determines whether the x y distance between 2 x,y points is within tolerance
        /// </summary>
        /// <param name="x1">The x1.</param>
        /// <param name="y1">The y1.</param>
        /// <param name="x2">The x2.</param>
        /// <param name="y2">The y2.</param>
        /// <param name="tolerance">The tolerance.</param>
        /// <returns>
        ///   <c>true</c> if distance between 2 points are within tolerance; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsWithinTolerance(double x1, double y1, double x2, double y2, double tolerance)
        {
            return IsXYWithinRange(x1, y1, x2, y2, tolerance);
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
        public static bool IsWithinRange(this double? currentMeasure, double? startMeasure, double? endMeasure)
        {
            if (currentMeasure.HasValue && startMeasure.HasValue && endMeasure.HasValue)
                return IsWithinRange(currentMeasure.Value, startMeasure.Value, endMeasure.Value);
            return false;
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
        /// <param name="sqlGeometry"></param>
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
        /// <param name="startPoint"></param>
        /// <param name="endPoint"></param>
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
        /// Determines whether [is x y within range] [the specified x1].
        /// </summary>
        /// <param name="x1">The x1.</param>
        /// <param name="y1">The y1.</param>
        /// <param name="x2">The x2.</param>
        /// <param name="y2">The y2.</param>
        /// <param name="tolerance">The tolerance.</param>
        /// <returns>
        ///   <c>true</c> if [is x y within range] [the specified x1]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsXYWithinRange(double x1, double y1, double x2, double y2, double tolerance = 0.0F)
        {
            return Math.Abs(x1 - x2) <= tolerance &&
                   Math.Abs(y1 - y2) <= tolerance;
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
        /// <para>
        /// Get offset measure between the two geometry.
        /// </para>
        ///   <para>Subtracts end measure of first geometry against the first measure of second geometry.
        /// </para>
        /// </summary>
        /// <param name="sqlGeometry1">First Geometry</param>
        /// <param name="sqlGeometry2">Second Geometry</param>
        /// <returns>Difference in measure</returns>
        public static double? GetOffset(this SqlGeometry sqlGeometry1, SqlGeometry sqlGeometry2)
        {
            return sqlGeometry1.GetEndPointMeasure() - sqlGeometry2.GetStartPointMeasure();
        }

        /// <summary>
        /// Get offset measure between the two point geometry.
        /// Subtracts end measure of first point and second point.
        /// </summary>
        /// <param name="point1">First Point</param>
        /// <param name="point2">Second Point</param>
        /// <returns>Difference in measure</returns>
        public static double GetPointOffset(this SqlGeometry point1, SqlGeometry point2)
        {
            return point1.M.Value - point2.M.Value;
        }

        #endregion       

        #region Sql Types Comparison

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

            var convertString = sqlString.ToString();

            return !string.IsNullOrEmpty(convertString) && convertString.ToLowerInvariant().Equals(targetString.ToLowerInvariant(), StringComparison.CurrentCulture);
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
            return GetStringAttributeValue(value);
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
            return GetStringAttributeValue(value);
        }

        /// <summary>
        /// Will get the string value for a given enums value, this will
        /// only work if you assign the StringValue attribute to
        /// the items in your enum.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetString(this LRSErrorCodes value)
        {
            return GetStringAttributeValue(value);
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
            var type = value.GetType();

            // Get field info for this type
            var fieldInfo = type.GetField(value.ToString());

            // Get the string value attributes
            // Return the first if there was a match.
            return fieldInfo.GetCustomAttributes(typeof(StringValueAttribute), false)
                       is StringValueAttribute[] attributes && attributes.Length > 0 ? attributes[0].StringValue : null;
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
            return lrsErrorCodes == LRSErrorCodes.ValidLRS ? "TRUE" : ((short)lrsErrorCodes).ToString(CultureInfo.CurrentCulture);
        }

        #endregion

        #region Exception Handling

        /// <summary>
        /// Throw if input geometry is not a LRS Geometry collection POINT, LINESTRING or MULTILINESTRING.
        /// </summary>
        /// <param name="sqlGeometries">Input Sql Geometry</param>
        internal static void ThrowIfNotLRSType(params SqlGeometry[] sqlGeometries)
        {
            if (sqlGeometries.Any(geom => !geom.IsLRSType()))
                throw new ArgumentException(ErrorMessage.LRSCompatible);
        }

        /// <summary>
        /// Throw if input geometry is not a Point.
        /// </summary>
        /// <param name="sqlGeometry">Input Sql Geometry</param>
        /// <param name="sqlGeometries">Sql Geometries</param>
        internal static void ThrowIfNotPoint(SqlGeometry sqlGeometry, params SqlGeometry[] sqlGeometries)
        {
            var geometries = (new[] { sqlGeometry }).Concat(sqlGeometries);
            if (geometries.Any(geom => !geom.IsOfSupportedTypes(OpenGisGeometryType.Point)))
                throw new ArgumentException(ErrorMessage.PointCompatible);
        }

        /// <summary>
        /// Throw if input geometry is not a line string.
        /// </summary>
        /// <param name="sqlGeometry">Input Sql Geometry</param>
        /// <param name="sqlGeometries">Sql Geometries</param>
        internal static void ThrowIfNotLine(SqlGeometry sqlGeometry, params SqlGeometry[] sqlGeometries)
        {
            var geometries = (new[] { sqlGeometry }).Concat(sqlGeometries);
            if (geometries.Any(geom => !geom.IsOfSupportedTypes(OpenGisGeometryType.LineString)))
                throw new ArgumentException(ErrorMessage.LineStringCompatible);
        }

        /// <summary>
        /// Throw if input geometry is not a line string or multiline string.
        /// </summary>
        /// <param name="sqlGeometry">Input Sql Geometry</param>
        /// <param name="sqlGeometries">Sql Geometries</param>
        internal static void ThrowIfNotLineOrMultiLine(SqlGeometry sqlGeometry, params SqlGeometry[] sqlGeometries)
        {
            var geometries = (new[] { sqlGeometry }).Concat(sqlGeometries);
            if (geometries.Any(geom => !geom.IsOfSupportedTypes(OpenGisGeometryType.LineString, OpenGisGeometryType.MultiLineString)))
                throw new ArgumentException(ErrorMessage.LineOrMultiLineStringCompatible);
        }

        /// <summary>
        /// Throw if SRIDs of two geometries doesn't match.
        /// </summary>
        /// <param name="sourceGeometry">Source Sql Geometry</param>
        /// <param name="targetGeometry">Target Sql Geometry</param>
        internal static void ThrowIfSRIDDoesNotMatch(SqlGeometry sourceGeometry, SqlGeometry targetGeometry)
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
                ThrowLRSError(LRSErrorCodes.InvalidLRSMeasure);
        }

        /// <summary>
        /// Check if the geometry collection is of POINT, LINESTRING, MULTILINESTRING.
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
        /// Throws ArgumentException based on message format and parameters
        /// </summary>
        /// <param name="messageFormat">Message format</param>
        /// <param name="args">Arguments to be appended with format</param>
        public static void ThrowException(string messageFormat, params object[] args)
        {
            throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, messageFormat, args));
        }

        /// <summary>
        /// Throws LRS Error based on error code.
        /// </summary>
        /// <param name="lrsErrorCode">LRS Error Code</param>
        public static void ThrowLRSError(LRSErrorCodes lrsErrorCode)
        {
            var message = string.Format(CultureInfo.CurrentCulture, "{0} - {1}", (int)lrsErrorCode, lrsErrorCode.GetString());
            throw new ArgumentException(message);
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
        /// Checks if both the segments are in same direction. either increasing or decrease based on measures.
        /// </summary>
        /// <param name="geometrySegment1">Geometry 1</param>
        /// <param name="geometrySegment2">Geometry 2</param>
        /// <returns>Returns true if both Geom Segments are in same direction</returns>
        public static bool STSameDirection(this SqlGeometry geometrySegment1, SqlGeometry geometrySegment2)
        {
            return geometrySegment1.STLinearMeasureProgress() == geometrySegment2.STLinearMeasureProgress();
        }

        /// <summary>
        /// Gets the dimension info of input geometry
        /// </summary>
        /// <param name="sqlGeometry">The SQL geometry.</param>
        /// <returns>Dimensional Info 2D, 2DM, 3D or 3DM</returns>
        public static DimensionalInfo STGetDimension(this SqlGeometry sqlGeometry)
        {
            // ReSharper disable once CommentTypo
            // STNumpoint can be performed only on valid geometries.
            if (!sqlGeometry.STIsValid() || sqlGeometry.STNumPoints() <= 0)
                return DimensionalInfo.None;

            var firstPoint = sqlGeometry.STPointN(1);
            if (firstPoint.Z.IsNull && firstPoint.M.IsNull)
                return DimensionalInfo.Dim2D;

            if (firstPoint.Z.IsNull && !firstPoint.M.IsNull)
                return DimensionalInfo.Dim2DWithMeasure;

            if (!firstPoint.Z.IsNull && firstPoint.M.IsNull)
                return DimensionalInfo.Dim3D;

            if (!firstPoint.Z.IsNull && !firstPoint.M.IsNull)
                return DimensionalInfo.Dim3DWithMeasure;

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
                case DimensionalInfo.Dim3DWithMeasure:
                case DimensionalInfo.Dim2DWithMeasure:
                    return;
                // if dimension is of x, y and z
                // need to convert third z co-ordinate to M for LRS
                case DimensionalInfo.Dim3D:
                    sqlGeometry = sqlGeometry.ConvertTo2DM();
                    break;
                case DimensionalInfo.Dim2D:
                    ThrowException(ErrorMessage.TwoDimensionalCoordinates);
                    break;
                // skip for invalid types where Dimensional information can't be inferred
                default:
                    return;
            }
        }

        #endregion

        #region Others

        /// <summary>
        /// Checks if two double values are equal.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns></returns>
        public static bool EqualsTo(this double left, double right)
        {
            return Math.Abs(left - right) < double.Epsilon;
        }

        /// <summary>
        /// Checks if two double values are equal.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns></returns>
        public static bool EqualsTo(this double? left, double right)
        {
            var leftValue = left ?? 0.0;
            return EqualsTo(leftValue, right);
        }

        /// <summary>
        /// Checks if two double values are equal.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns></returns>
        public static bool EqualsTo(this double left, double? right)
        {
            var rightValue = right ?? 0.0;
            return EqualsTo(left, rightValue);
        }

        /// <summary>
        /// Checks if two double values are equal.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns></returns>
        public static bool EqualsTo(this double? left, double? right)
        {
            var leftValue = left ?? 0.0;
            var rightValue = right ?? 0.0;
            return EqualsTo(leftValue, rightValue);
        }

        /// <summary>
        /// Checks if two double values are not equal.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns></returns>
        public static bool NotEqualsTo(this double left, double right)
        {
            return !EqualsTo(left, right);
        }

        /// <summary>
        /// Convert Sql geometry with x,y,z to x,y,m
        /// </summary>
        /// <param name="sqlGeometry">Sql Geometry</param>
        /// <returns></returns>
        // ReSharper disable once InconsistentNaming
        internal static SqlGeometry ConvertTo2DM(this SqlGeometry sqlGeometry)
        {
            var sqlBuilder = new ConvertXYZ2XYMGeometrySink();
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
        /// Gets the point.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="z">The z.</param>
        /// <param name="m">The m.</param>
        /// <param name="srid">The srid.</param>
        /// <returns></returns>
        private static SqlGeometry GetPoint(SqlDouble x, SqlDouble y, SqlDouble z, SqlDouble m, SqlInt32 srid)
        {
            double? zCoordinate = z.IsNull ? (double?)null : z.Value;
            double? mValue = m.IsNull ? (double?)null : m.Value;
            return GetPoint((double)x, (double)y, zCoordinate, mValue, (int)srid);
        }

        /// <summary>
        /// Gets the point.
        /// </summary>
        /// <param name="sqlGeometry">The SQL geometry.</param>
        /// <returns></returns>
        public static SqlGeometry GetPoint(SqlGeometry sqlGeometry)
        {
            return GetPoint(sqlGeometry.STX, sqlGeometry.STY, sqlGeometry.Z, sqlGeometry.M, sqlGeometry.STSrid);
        }

        /// <summary>
        /// Gets the LRS multi line.
        /// </summary>
        /// <param name="sqlGeometry">The SQL geometry.</param>
        /// <param name="doUpdateM">if set to <c>true</c> [do update m].</param>
        /// <param name="offsetM">The offset m.</param>
        /// <returns></returns>
        internal static LRSMultiLine GetLRSMultiLine(this SqlGeometry sqlGeometry, bool doUpdateM, double? offsetM)
        {
            ThrowIfNotLineOrMultiLine(sqlGeometry);
            // populate the input segment
            var lrsBuilder = new BuildLRSMultiLineSink(doUpdateM, offsetM);
            sqlGeometry.Populate(lrsBuilder);
            return lrsBuilder.MultiLine;
        }

        /// <summary>
        /// Gets the LRS multi line from Sql Geometry.
        /// </summary>
        /// <param name="sqlGeometry">The SQL geometry.</param>
        /// <returns></returns>
        internal static LRSMultiLine GetLRSMultiLine(this SqlGeometry sqlGeometry)
        {
            return sqlGeometry.GetLRSMultiLine(false, 0);
        }

        /// <summary>
        /// Gets the point with updated m.
        /// </summary>
        /// <param name="sqlGeometry">The SQL geometry.</param>
        /// <param name="updatedMeasure">The updated measure.</param>
        /// <returns></returns>
        public static SqlGeometry GetPointWithUpdatedM(SqlGeometry sqlGeometry, double updatedMeasure)
        {
            return GetPoint(sqlGeometry.STX, sqlGeometry.STY, sqlGeometry.Z, updatedMeasure, sqlGeometry.STSrid);
        }

        /// <summary>
        /// Gets the type of the merge.
        /// </summary>
        /// <param name="geometry1">The geometry1.</param>
        /// <param name="geometry2">The geometry2.</param>
        /// <returns></returns>
        public static MergeInputType GetMergeType(this SqlGeometry geometry1, SqlGeometry geometry2)
        {
            var isGeom1LineString = geometry1.IsLineString();
            var isGeom1MultiLineString = geometry1.IsMultiLineString();

            var isGeom2LineString = geometry2.IsLineString();
            var isGeom2MultiLineString = geometry2.IsMultiLineString();

            if (isGeom1LineString && isGeom2LineString)
                return MergeInputType.LSLS;

            if (isGeom1MultiLineString && isGeom2MultiLineString)
                return MergeInputType.MLSMLS;

            if (isGeom1LineString && isGeom2MultiLineString)
                return MergeInputType.LSMLS;

            if (isGeom1MultiLineString && isGeom2LineString)
                return MergeInputType.MLSLS;

            return MergeInputType.None;
        }

        /// <summary>
        /// Converts WKT string to SqlGeometry object.
        /// </summary>
        /// <param name="geomWKT">geometry in WKT representation</param>
        /// <param name="srid">Spatial reference identifier; Default for SQL Server 4326</param>
        /// <returns>SqlGeometry</returns>
        public static SqlGeometry GetGeom(this string geomWKT, int srid = Constants.DefaultSRID)
        {
            return string.IsNullOrEmpty(geomWKT) ? null : SqlGeometry.STGeomFromText(new SqlChars(geomWKT), srid);
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
