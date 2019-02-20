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
        public static bool IsWithinRange(this double currentMeasure, double startMeasure, double endMeasure)
        {
            return (currentMeasure < startMeasure && currentMeasure < endMeasure)
                || (currentMeasure > startMeasure && currentMeasure > endMeasure);
        }

        /// <summary>
        /// Check whether the measure falls withing the start and end measure.
        /// </summary>
        /// <param name="currentMeasure"></param>
        /// <param name="startMeasure"></param>
        /// <param name="endMeasure"></param>
        /// <returns></returns>
        public static bool IsWithinRange(this double currentMeasure, SqlGeometry sqlGeometry)
        {
            var startMeasure = sqlGeometry.GetStartPointMeasure();
            var endMeasure = sqlGeometry.GetEndPointMeasure();
            return (currentMeasure < startMeasure && currentMeasure < endMeasure)
                || (currentMeasure > startMeasure && currentMeasure > endMeasure);
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
            return string.Format("{0} is not within the measure range {1} : {2} of the linear geometry", measure, Math.Min(startMeasure, endMeasure).ToString(), Math.Max(startMeasure, endMeasure).ToString());
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
    }
}
