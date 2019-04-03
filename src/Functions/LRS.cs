/********************************************************
*  (c) Microsoft. All rights reserved.                  *
********************************************************/

using System;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Types;
using SQLSpatialTools.Utility;
using Ext = SQLSpatialTools.Utility.SpatialExtensions;

namespace SQLSpatialTools.Functions.LRS
{
    /// <summary>
    /// This provides LRS data manipulation on planar Geometry data type.
    /// </summary>
    public static class Geometry
    {
        /// <summary>
        /// Clip a geometry segment based on specified measure.
        /// This function just hooks up and runs a pipeline using the sink.
        /// </summary>
        /// <param name="geometry">Input Geometry</param>
        /// <param name="startMeasure"></param>
        /// <param name="endMeasure"></param>
        /// <returns></returns>
        public static SqlGeometry ClipGeometrySegment(SqlGeometry geometry, double startMeasure, double endMeasure)
        {
            Ext.ThrowIfNotLine(geometry);
            Ext.ValidateLRSDimensions(ref geometry);
            Ext.ThrowIfStartMeasureIsNotInRange(startMeasure, endMeasure, geometry);
            Ext.ThrowIfEndMeasureIsNotInRange(startMeasure, endMeasure, geometry);

            var geometryBuilder = new SqlGeometryBuilder();
            var geomSink = new ClipMGeometrySegmentSink(startMeasure, endMeasure, geometryBuilder);
            geometry.Populate(geomSink);
            return geometryBuilder.ConstructedGeometry;
        }

        /// <summary>
        /// Get end point measure of a LRS Geom Segment.
        /// </summary>
        /// <param name="geometry">Input Geometry</param>
        /// <returns>End measure</returns>
        public static SqlDouble GetEndMeasure(SqlGeometry geometry)
        {
            Ext.ThrowIfNotLRSType(geometry);
            Ext.ValidateLRSDimensions(ref geometry);
            return geometry.GetEndPointMeasure();
        }

        /// <summary>
        /// Get start point measure of a LRS Geom Segment.
        /// </summary>
        /// <param name="geometry">Input Geometry</param>
        /// <returns>Start measure</returns>
        public static SqlDouble GetStartMeasure(SqlGeometry geometry)
        {
            Ext.ThrowIfNotLRSType(geometry);
            Ext.ValidateLRSDimensions(ref geometry);
            return geometry.GetStartPointMeasure();
        }

        /// <summary>
        /// Find the point with specified measure, going from the start point in the direction of the end point.
        /// The measure must be between measures of these two points.
        /// </summary>
        /// <param name="startPoint">Start Geometry Point</param>
        /// <param name="endPoint">End Geometry Point</param>
        /// <param name="measure">Measure at which the point is to be found</param>
        /// <returns></returns>
        public static SqlGeometry InterpolateBetweenGeom(SqlGeometry startPoint, SqlGeometry endPoint, double measure)
        {
            // We need to check a few prequisites.
            // We only operate on points.
            Ext.ThrowIfNotPoint(startPoint, endPoint);
            Ext.ThrowIfSRIDsDoesNotMatch(startPoint, endPoint);
            Ext.ValidateLRSDimensions(ref startPoint);
            Ext.ValidateLRSDimensions(ref endPoint);
            Ext.ThrowIfMeasureIsNotInRange(measure, startPoint, endPoint);

            // The SRIDs also have to match
            int srid = startPoint.STSrid.Value;

            // Since we're working on a Cartesian plane, this is now pretty simple.
            // The fraction of the way from start to end.
            var fraction = (measure - startPoint.M.Value) / (endPoint.M.Value - startPoint.M.Value);
            var newX = (startPoint.STX.Value * (1 - fraction)) + (endPoint.STX.Value * fraction);
            var newY = (startPoint.STY.Value * (1 - fraction)) + (endPoint.STY.Value * fraction);

            //There's no way to know Z, so just put NULL there
            return Ext.GetPoint(newX, newY, null, measure, srid);

        }

        /// <summary>
        /// Checks if two geometric segments are spatially connected.
        /// </summary>
        /// <param name="geometry1">First Geometry</param>
        /// <param name="geometry2">Second Geometry</param>
        /// <param name="tolerence">Distance Threshold range; default 0.01F</param>
        /// <returns></returns>
        public static SqlBoolean IsConnected(SqlGeometry geometry1, SqlGeometry geometry2, double tolerence = 0.01F)
        {
            Ext.ThrowIfNotLine(geometry1, geometry2);
            Ext.ThrowIfSRIDsDoesNotMatch(geometry1, geometry2);
            Ext.ValidateLRSDimensions(ref geometry1);
            Ext.ValidateLRSDimensions(ref geometry2);

            // Geometry 1 points
            var geometry1StartPoint = geometry1.STStartPoint();
            var geometry1EndPoint = geometry1.STEndPoint();

            // Geometry 2 points
            var geometry2StartPoint = geometry2.STStartPoint();
            var geometry2EndPoint = geometry2.STEndPoint();

            // First check if start point of geom1 matches geom2 start or end point
            if (geometry1StartPoint.STEquals(geometry2StartPoint) || geometry1StartPoint.STEquals(geometry2EndPoint))
                return true;

            // First check if end point of geom1 matches geom2 start or end point
            if (geometry1EndPoint.STEquals(geometry2StartPoint) || geometry1EndPoint.STEquals(geometry2EndPoint))
                return true;

            // If the points doesn't coincide, check for the distance and whether it falls with the tolerance range
            // Comparing geom1 start point distance against geom2 start and end points
            if (geometry1StartPoint.IsXYWithinRange(geometry2StartPoint, tolerence) || geometry1StartPoint.IsXYWithinRange(geometry2EndPoint, tolerence))
                return true;

            // Comparing geom1 start point distance against geom2 start and end points
            if (geometry1EndPoint.IsXYWithinRange(geometry2StartPoint, tolerence) || geometry1EndPoint.IsXYWithinRange(geometry2EndPoint, tolerence))
                return true;

            return false;
        }

        /// <summary>
        /// Checks if an LRS point is valid.
        /// </summary>
        /// <param name="geometry">Sql Geometry.</param>
        /// <returns></returns>
        public static SqlBoolean IsValidPoint(SqlGeometry geometry)
        {
            if (geometry.IsNull || geometry.STIsEmpty() || !geometry.STIsValid() || !geometry.IsPoint())
                return false;

            // check if the point has measure value
            if (!geometry.M.IsNull)
                return true;

            // if m is null; the check if frame from x,y,z where z is m
            if (geometry.STGetDimension() == DimensionalInfo._3D)
            {
                geometry = geometry.ConvertTo2DM();
                return !geometry.M.IsNull;
            }
            return false;
        }

        /// <summary>
        /// Locate the Geometry Point along the specified measure on the Geometry.
        /// This function just hooks up and runs a pipeline using the sink.
        /// </summary>
        /// <param name="geometry">Input Geometry</param>
        /// <param name="distance">Measure of the Geometry point to locate</param>
        /// <returns>Geometry Point</returns>
        public static SqlGeometry LocatePointAlongGeom(SqlGeometry geometry, double distance)
        {
            Ext.ThrowIfNotLine(geometry);
            Ext.ValidateLRSDimensions(ref geometry);

            var geomBuilder = new SqlGeometryBuilder();
            var geomSink = new LocateMAlongGeometrySink(distance, geomBuilder);
            geometry.Populate(geomSink);
            return geomBuilder.ConstructedGeometry;
        }

        /// <summary>
        /// Merge two geometry segments to one geometry.
        /// This function just hooks up and runs a pipeline using the sink.
        /// </summary>
        /// <param name="geometry1">First Geometry</param>
        /// <param name="geometry2">Second Geometry</param>
        /// <returns></returns>
        public static SqlGeometry MergeGeometrySegments(SqlGeometry geometry1, SqlGeometry geometry2)
        {
            Ext.ThrowIfNotLine(geometry1, geometry2);
            Ext.ThrowIfSRIDsDoesNotMatch(geometry1, geometry2);
            Ext.ValidateLRSDimensions(ref geometry1);
            Ext.ValidateLRSDimensions(ref geometry2);

            var mOffset = geometry1.GetOffset(geometry2);
            var geomBuilder = new SqlGeometryBuilder();

            // build first geometry
            var geomSink = new MergeGeometrySegmentSink(geomBuilder, true, false, null);
            geometry1.Populate(geomSink);

            // join second geometry
            geomSink = new MergeGeometrySegmentSink(geomBuilder, false, true, mOffset);
            geometry2.Populate(geomSink);
            return geomBuilder.ConstructedGeometry;
        }

        /// <summary>
        /// (Re)populate measures across shape points.
        /// </summary>
        /// <param name="geometry">Input Geometry</param>
        /// <param name="startMeasure"></param>
        /// <param name="endMeasure"></param>
        /// <returns></returns>
        public static SqlGeometry PopulateGeometryMeasures(SqlGeometry geometry, double? startMeasure, double? endMeasure)
        {
            Ext.ThrowIfNotLine(geometry);
            Ext.ValidateLRSDimensions(ref geometry);

            // As per requirement; 
            // the default value of start point is 0 when null is specified
            // the default value of end point is cartographic length of the segment when null is specified
            var localStartMeasure = startMeasure ?? 0;
            var localEndMeasure = endMeasure ?? (double)geometry.STLength();

            var length = geometry.STLength().Value;

            var geomBuilder = new SqlGeometryBuilder();
            var geomSink = new PopulateGeometryMeasuresSink(localStartMeasure, localEndMeasure, length, geomBuilder);
            geometry.Populate(geomSink);
            return geomBuilder.ConstructedGeometry;
        }

        /// <summary>
        /// Resets Geometry Measure values.
        /// </summary>
        /// <param name="geometry">Input Geometry</param>
        /// <returns></returns>
        public static SqlGeometry ResetMeasure(SqlGeometry geometry)
        {
            Ext.ThrowIfNotLine(geometry);
            Ext.ValidateLRSDimensions(ref geometry);

            var geomBuilder = new SqlGeometryBuilder();
            var geomSink = new ResetMGemetrySink(geomBuilder);
            geometry.Populate(geomSink);
            return geomBuilder.ConstructedGeometry;
        }

        /// <summary>
        /// Reverse Linear Geometry
        /// Works only for LineString Geometry.
        /// </summary>
        /// <param name="geometry">Input Geometry</param>
        /// <returns></returns>
        public static SqlGeometry ReverseLinearGeometry(SqlGeometry geometry)
        {
            Ext.ThrowIfNotLine(geometry);
            Ext.ValidateLRSDimensions(ref geometry);

            var geomBuilder = new SqlGeometryBuilder();

            geomBuilder.SetSrid((int)geometry.STSrid);
            geomBuilder.BeginGeometry(OpenGisGeometryType.LineString);
            geomBuilder.BeginFigure(geometry.STEndPoint().STX.Value, geometry.STEndPoint().STY.Value, geometry.STEndPoint().Z.Value, geometry.STEndPoint().M.Value);

            var iterator = (int)geometry.STNumPoints() - 1;
            for (; iterator >= 1; iterator--)
            {
                geomBuilder.AddLine(
                    geometry.STPointN(iterator).STX.Value,
                    geometry.STPointN(iterator).STY.Value,
                    geometry.STPointN(iterator).Z.Value,
                    geometry.STPointN(iterator).M.Value);
            }
            geomBuilder.EndFigure();
            geomBuilder.EndGeometry();
            return geomBuilder.ConstructedGeometry;
        }

        /// <summary>
        /// Split a geometry into geometry segments based on split measure. 
        /// This function just hooks up and runs a pipeline using the sink.
        /// </summary>
        /// <param name="geometry">Input Geometry</param>
        /// <param name="splitMeasure"></param>
        /// <param name="geometry1">First Geometry Segment</param>
        /// <param name="geometry2">Second Geometry Segment</param>
        public static void SplitGeometrySegment(SqlGeometry geometry, double splitMeasure, out SqlGeometry geometry1, out SqlGeometry geometry2)
        {
            Ext.ThrowIfNotLine(geometry);
            Ext.ValidateLRSDimensions(ref geometry);

            var splitPoint = LocatePointAlongGeom(geometry, splitMeasure);
            var geometryBuilder1 = new SqlGeometryBuilder();
            var geometryBuilder2 = new SqlGeometryBuilder();
            var geomSink = new SplitGeometrySegmentSink(splitPoint, geometryBuilder1, geometryBuilder2);
            geometry.Populate(geomSink);
            geometry1 = geometryBuilder1.ConstructedGeometry;
            geometry2 = geometryBuilder2.ConstructedGeometry;
        }

        /// <summary>
        /// Validates the LRS geometry.
        /// </summary>
        /// <param name="geometry">The input SqlGeometry.</param>
        /// <returns>TRUE if Valid; 13331 - if Invalid; 13333 - if Invalid Measure</returns>
        public static string ValidateLRSGeometry(SqlGeometry geometry)
        {
            // throw if type apart from POINT, LINESTRING, MULTILINESTRING is given as input.
            Ext.ThrowIfNotLRSType(geometry);

            // If there is no measure value; return invalid.
            if (geometry.STGetDimension() == DimensionalInfo._2D)
                return LRSErrorCodes.MeasureNotDefined.Value();

            // convert to valid 3 point LRS co-ordinate.
            Ext.ValidateLRSDimensions(ref geometry);

            // return invalid if empty or is of geometry collection
            if (geometry.IsNull || geometry.STIsEmpty() || !geometry.STIsValid() || geometry.IsGeometryCollection())
                return LRSErrorCodes.Invalid.Value();

            // return invalid if geometry doesn't or have null values
            if (!geometry.STHasMeasureValues())
                return LRSErrorCodes.MeasureNotDefined.Value();

            // checks if the measures are in linear range.
            var geomBuilder = new SqlGeometryBuilder();
            var geomSink = new ValidateLinearMeasureGeometrySink(geomBuilder, geometry.STLinearMeasureProgress());
            geometry.Populate(geomSink);

            return geomSink.IsLinearMeasure() ? LRSErrorCodes.Valid.Value() : LRSErrorCodes.MeasureNotLinear.Value();
        }
    }
}
