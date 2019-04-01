using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.SqlServer.Types;

namespace SQLSpatialTools.Utility
{
    public static class SpatialExtensions
    {
        #region "OGC Type Checks"

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
            return IsWithinRange((double)currentMeasure, startMeasure, endMeasure);
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
        /// Get offset measure between the two geometry.
        /// Subtracts end measure of first geomtry against the first measure of second geometry.
        /// </summary>
        /// <param name="sqlgeometry1">First Geometry</param>
        /// <param name="sqlgeometry2">Second Geometry</param>
        /// <returns></returns>
        public static double? GetOffset(this SqlGeometry sqlgeometry1, SqlGeometry sqlgeometry2)
        {
            return (double?)(sqlgeometry1.GetEndPointMeasure() - sqlgeometry2.GetStartPointMeasure());
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
            return string.Format(ErrorMessage.LinearGeometryMeasureMustBeInRange, measure, Math.Min(startMeasure, endMeasure).ToString(), Math.Max(startMeasure, endMeasure).ToString());
        }

        /// <summary>
        /// Get Sql Chars from WKT
        /// </summary>
        /// <param name="format">format of wkt</param>
        /// <param name="args">argument to appended to format</param>
        /// <returns></returns>
        public static SqlChars GetSqlChars(string format, params object[] args)
        {
            var geometry = string.Format(format, args);
            return new SqlChars(geometry);
        }

        /// <summary>
        /// Get SqlGeometry Point from WKT
        /// </summary>
        /// <param name="x">x Coordinate</param>
        /// <param name="y">y Coordinate</param>
        /// <param name="z">z Coordinate</param>
        /// <param name="m">Measure</param>
        /// <param name="srid">Spatail Reference Identifier; default is 4326</param>
        /// <returns>Sql Point Geometry</returns>
        public static SqlGeometry GetPoint(double x, double y, double? z, double? m, int srid = 4326)
        {
            var zCoordinate = z == null ? "NULL" : z.ToString();
            var geometry = string.Format(SqlStringFormat.Point, x, y, zCoordinate, m);
            return SqlGeometry.STPointFromText(new SqlChars(geometry), srid);
        }

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
        /// Convert wkt string to SqlGeometry object.
        /// </summary>
        /// <param name="geomString">geometry in string representation</param>
        /// <param name="srid">spatial reference identifier; default for SQL Server 4326</param>
        /// <returns>SqlGeometry</returns>
        public static SqlGeometry GetGeom(this string geomString, int srid = 4326)
        {
            return SqlGeometry.STGeomFromText(new SqlChars(geomString), srid);
        }

        public static DimensionalInfo STGetDimension(this SqlGeometry sqlGeometry)
        {
            if (sqlGeometry.STNumPoints() > 0)
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
        /// Compares measure values of each point in a LineString; if the two geom segments are equal.
        /// </summary>
        /// <param name="sqlGeometry">Input Line Segment</param>
        /// <param name="targetGeometry">Target Line Segement</param>
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
        /// Convert wkt string to SqlGeography object.
        /// </summary>
        /// <param name="geogString">geography in wkt string representation</param>
        /// <param name="srid">spatial reference identifier; default for SQL Server 4326</param>
        /// <returns>SqlGeography</returns>
        public static SqlGeography GetGeog(this string geogString, int srid = 4326)
        {
            return SqlGeography.STGeomFromText(new SqlChars(geogString), srid);
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
        /// Compares sql bool for bool.
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

        /// <summary>
        /// Throws ArgumentException based on message format and parameters
        /// </summary>
        /// <param name="messageFormat">Message format</param>
        /// <param name="args">Arguments to be appended with format</param>
        public static void ThrowException(string messageFormat, params string[] args)
        {
            throw new ArgumentException(string.Format(messageFormat, args));
        }

        #region "Enum Attrib Extension"

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

        private static string GetStringAttributeValue<T>(this T value)
        {
            // Get the type
            Type type = value.GetType();

            // Get fieldinfo for this type
            FieldInfo fieldInfo = type.GetField(value.ToString());

            // Get the stringvalue attributes
            StringValueAttribute[] attribs = fieldInfo.GetCustomAttributes(
                typeof(StringValueAttribute), false) as StringValueAttribute[];

            // Return the first if there was a match.
            return attribs.Length > 0 ? attribs[0].StringValue : null;
        }
        
        #endregion

        /// <summary>
        /// Validate and convert to lrs dimension
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
                default:
                    ThrowException("Cannot operate on 2 Dimensional co-ordinates without measure values");
                    break;
            }
        }

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

        #region Exception Handlings

        /// <summary>
        /// Throw if input geometry is not a line string.
        /// </summary>
        /// <param name="sqlGeometry">Input Sql Geometry</param>
        internal static void ThrowIfNotLine(params SqlGeometry[] sqlGeometry)
        {
            var isLineString = true;

            foreach (var geom in sqlGeometry)
            {
                isLineString = geom.IsLineString();
                if (!isLineString)
                    break;
            }

            if (!isLineString)
                throw new ArgumentException(ErrorMessage.LineStringCompatible);
        }

        /// <summary>
        /// Throw if input geometry is not a line string or multiline string.
        /// </summary>
        /// <param name="sqlGeometry">Input Sql Geometry</param>
        internal static void ThrowIfNotLineOrMultiLine(params SqlGeometry[] sqlGeometry)
        {
            var isLineString = true;

            foreach (var geom in sqlGeometry)
            {
                isLineString = geom.IsLineString() || geom.IsMultiLineString();
                if (!isLineString)
                    break;
            }

            if (!isLineString)
                throw new ArgumentException(ErrorMessage.LineOrMultiLineStringCompatible);
        }

        /// <summary>
        /// Throw if input geometry is not a Point.
        /// </summary>
        /// <param name="sqlGeometry">Input Sql Geometry</param>
        internal static void ThrowIfNotPoint(params SqlGeometry[] sqlGeometry)
        {
            var isPoint = true;

            foreach (var geom in sqlGeometry)
            {
                isPoint = geom.IsPoint();
                if (!isPoint)
                    break;
            }

            if (!isPoint)
                throw new ArgumentException(ErrorMessage.PointCompatible);
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
                ThrowException("End measure {0}", startMeasure.LinearGeometryRangeExpectionMessage(startMeasure, endMeasure));
        }

        #endregion

    }
}
