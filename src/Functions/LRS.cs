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
        /// <br /> If the clipped start and end point is within tolerance of shape point then shape point is returned as start and end of clipped Geom segment. 
        /// <br /> This function just hooks up and runs a pipeline using the sink.
        /// </summary>
        /// <param name="geometry">Input Geometry</param>
        /// <param name="clipStartMeasure">Start Measure</param>
        /// <param name="clipEndMeasure">End Measure</param>
        /// <param name="tolerance">Tolerance Value</param>
        /// <returns>Clipped Segment</returns>
        public static SqlGeometry ClipGeometrySegment(SqlGeometry geometry, double clipStartMeasure, double clipEndMeasure, double tolerance = Constants.Tolerance)
        {
            Ext.ThrowIfNotLine(geometry);
            Ext.ValidateLRSDimensions(ref geometry);
            bool startMeasureInvalid = false;
            bool endMeasureInvalid = false;

            // reassign clip start and end measure based upon there difference
            if (clipStartMeasure > clipEndMeasure)
            {
                var shiftObj = clipStartMeasure;
                clipStartMeasure = clipEndMeasure;
                clipEndMeasure = shiftObj;
            }

            // Get the measure progress of linear geometry and reassign the start and end measures based upon the progression
            var measureProgress = geometry.STLinearMeasureProgress();
            var geomStartMeasure = measureProgress == LinearMeasureProgress.Increasing ? geometry.GetStartPointMeasure() : geometry.GetEndPointMeasure();
            var geomEndMeasure = measureProgress == LinearMeasureProgress.Increasing ? geometry.GetEndPointMeasure() : geometry.GetStartPointMeasure();

            // clip start measure matches geom start measure and
            // clip end measure matches geom end measure then return the input geom
            if (clipStartMeasure == geomStartMeasure && clipEndMeasure == geomEndMeasure)
                return geometry;

            // Check if clip start and end measures are beyond geom start and end point measures
            var isStartBeyond = clipStartMeasure < geomStartMeasure;
            var isEndBeyond = clipEndMeasure > geomEndMeasure;

            // When clip measure range is not beyond range; then don't consider tolerance on extreme math; as per Oracle
            var isExtremeMeasuresMatch = Ext.IsExtremeMeasuresMatch(geomStartMeasure, geomEndMeasure, clipStartMeasure, clipEndMeasure);

            // don't throw error when measure is not in the range
            // rather reassign segment start and end measure 
            // if they are beyond the range or matching with the start and end point measure of input geometry
            if (!clipStartMeasure.IsWithinRange(geometry))
            {
                if (isStartBeyond || isExtremeMeasuresMatch)
                {
                    if (clipStartMeasure <= geomStartMeasure)
                        clipStartMeasure = geomStartMeasure;
                }
                else
                    startMeasureInvalid = true;
            }

            if (!clipEndMeasure.IsWithinRange(geometry))
            {
                if (isEndBeyond || isExtremeMeasuresMatch)
                {
                    if (clipEndMeasure >= geomEndMeasure)
                        clipEndMeasure = geomEndMeasure;
                }
                else
                    endMeasureInvalid = true;
            }

            // if both clip start and end measure are reassigned to 0 then return null
            if (startMeasureInvalid || endMeasureInvalid)
                return null;

            // if both clip start and end point measure is same then don't check for distance tolerance
            if (clipStartMeasure != clipEndMeasure)
            {
                var startClipPoint = LocatePointWithTolerance(geometry, clipStartMeasure, tolerance);
                var endClipPoint = LocatePointWithTolerance(geometry, clipEndMeasure, tolerance);
                if (startClipPoint.STDistance(endClipPoint).IsTolerable(tolerance))
                    return null;
            }

            var geometryBuilder = new SqlGeometryBuilder();
            var geomSink = new ClipMGeometrySegmentSink(clipStartMeasure, clipEndMeasure, geometryBuilder);
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
            // We need to check a few prerequisite.
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
        /// <param name="tolerance">Distance Threshold range; default 0.01F</param>
        /// <returns></returns>
        public static SqlBoolean IsConnected(SqlGeometry geometry1, SqlGeometry geometry2, double tolerance = 0.01F)
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

            // If the points doesn't coincide, check for the point co-ordinate difference and whether it falls within the tolerance
            // distance not considered as per Oracle.
            // Comparing geom1 start point x and y co-ordinate difference against geom2 start and end point x and y co-ordinates
            if (geometry1StartPoint.IsXYWithinRange(geometry2StartPoint, tolerance) || geometry1StartPoint.IsXYWithinRange(geometry2EndPoint, tolerance))
                return true;

            // Comparing geom1 start point x and y co-ordinate difference against geom2 start and end point x and y co-ordinates
            if (geometry1EndPoint.IsXYWithinRange(geometry2StartPoint, tolerance) || geometry1EndPoint.IsXYWithinRange(geometry2EndPoint, tolerance))
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
        /// <param name="measure">Measure of the Geometry point to locate</param>
        /// <returns>Geometry Point</returns>
        public static SqlGeometry LocatePointAlongGeom(SqlGeometry geometry, double measure)
        {
            // Invoking locate point without tolerance
            return LocatePointWithTolerance(geometry, measure, 0);
        }

        /// <summary>
        /// Locates the point with tolerance.
        /// </summary>
        /// <param name="geometry">The geometry.</param>
        /// <param name="measure">The measure.</param>
        /// <param name="tolerance">The tolerance.</param>
        /// <returns></returns>
        private static SqlGeometry LocatePointWithTolerance(SqlGeometry geometry, double measure, double tolerance = Constants.Tolerance)
        {
            Ext.ThrowIfNotLine(geometry);
            Ext.ValidateLRSDimensions(ref geometry);
            Ext.ThrowIfMeasureIsNotInRange(measure, geometry);

            var geomBuilder = new SqlGeometryBuilder();
            var geomSink = new LocateMAlongGeometrySink(measure, geomBuilder, tolerance);
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
        /// Works only for POINT, LINESTRING, MULTILINESTRING Geometry.
        /// </summary>
        /// <param name="geometry">Input Geometry</param>
        /// <returns></returns>
        public static SqlGeometry ReverseLinearGeometry(SqlGeometry geometry)
        {
            Ext.ThrowIfNotLRSType(geometry);
            Ext.ValidateLRSDimensions(ref geometry);

            // if point its no-op
            if (geometry.IsPoint())
                return geometry;

            var geometryBuilder = new SqlGeometryBuilder();
            var geomSink = new ReverseLinearGeometrySink(geometryBuilder);
            geometry.Populate(geomSink);
            return geometryBuilder.ConstructedGeometry;
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
            Ext.ThrowIfMeasureIsNotInRange(splitMeasure, geometry);

            var splitPoint = LocatePointAlongGeom(geometry, splitMeasure);
            var geometryBuilder1 = new SqlGeometryBuilder();
            var geometryBuilder2 = new SqlGeometryBuilder();
            var geomSink = new SplitGeometrySegmentSink(splitPoint, geometryBuilder1, geometryBuilder2);
            geometry.Populate(geomSink);
            geometry1 = geometryBuilder1.ConstructedGeometry;
            geometry2 = geometryBuilder2.ConstructedGeometry;
        }

        /// <summary>Returns the geometric segment at a specified offset from a geometric segment.
        /// Works only for LineString Geometry.</summary>
        /// <param name="geometry">Input Geometry</param>
        /// <param name="startMeasure">Start Measure</param>
        /// <param name="endMeasure">End Measure</param>
        /// <param name="offset">Offset value</param>
        /// <param name="tolerance">Tolerance</param>
        /// <returns>Offset Geometry Segment</returns>
        public static SqlGeometry OffsetGeometrySegment(SqlGeometry geometry, double startMeasure, double endMeasure, double offset, double tolerance = Constants.Tolerance)
        {
            Ext.ThrowIfNotLine(geometry);
            Ext.ValidateLRSDimensions(ref geometry);

            geometry = ClipGeometrySegment(geometry, startMeasure, endMeasure, tolerance);

            var geomBuilder = new SqlGeometryBuilder();
            var geomSink = new OffsetGeometrySink(geomBuilder, offset, geometry.STLinearMeasureProgress());
            geometry.Populate(geomSink);
            return geomBuilder.ConstructedGeometry;
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
