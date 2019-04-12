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
        /// <param name="geometry1"></param>
        /// <param name="geometry2"></param>
        /// <param name="tolerence"></param>
        /// <returns>SqlBoolean</returns>
        public static SqlBoolean IsConnected(SqlGeometry geometry1, SqlGeometry geometry2, double tolerence = Constants.Tolerance)
        {
            return IsConnected(geometry1, geometry2, tolerence, out _);
        }

        /// <summary>
        /// Checks if two geometric segments are spatially connected with merge position information
        /// </summary>
        /// <param name="geometry1">First Geometry</param>
        /// <param name="geometry2">Second Geometry</param>
        /// <param name="tolerence">Distance Threshold range; default 0.01F</param>
        /// <param name="mergeCoordinatePosition">Represents position of merge segments</param>
        /// <returns>SqlBoolean</returns>
        private static SqlBoolean IsConnected(SqlGeometry geometry1, SqlGeometry geometry2, double tolerence, out MergePosition mergePosition)
        {
            Ext.ThrowIfNotLineOrMultiLine(geometry1, geometry2);
            Ext.ThrowIfSRIDsDoesNotMatch(geometry1, geometry2);
            Ext.ValidateLRSDimensions(ref geometry1);
            Ext.ValidateLRSDimensions(ref geometry2);

            // Geometry 1 points
            var geometry1StartPoint = geometry1.STStartPoint();
            var geometry1EndPoint = geometry1.STEndPoint();

            // Geometry 2 points
            var geometry2StartPoint = geometry2.STStartPoint();
            var geometry2EndPoint = geometry2.STEndPoint();

            // If the points doesn't coincide, check for the point co-ordinate difference and whether it falls within the tolerance
            // distance not considered as per Oracle.
            // Comparing geom1 start point x and y co-ordinate difference against geom2 start and end point x and y co-ordinates
            var isStartStartConnected = geometry1StartPoint.STEquals(geometry2StartPoint) || geometry1StartPoint.IsXYWithinRange(geometry2StartPoint, tolerence);
            var isStartEndConnected = geometry1StartPoint.STEquals(geometry2EndPoint) || geometry1StartPoint.IsXYWithinRange(geometry2EndPoint, tolerence);
            var isEndStartConnected = geometry1EndPoint.STEquals(geometry2StartPoint) || geometry1EndPoint.IsXYWithinRange(geometry2StartPoint, tolerence);
            var isEndEndConnected = geometry1EndPoint.STEquals(geometry2EndPoint) || geometry1EndPoint.IsXYWithinRange(geometry2EndPoint, tolerence);
            var isBothEndsConnected = isStartStartConnected && isEndEndConnected;
            var isCrossEndsConnected = isStartEndConnected && isEndStartConnected;

            mergePosition = MergePosition.None;

            if (isStartStartConnected)
                mergePosition = MergePosition.StartStart;
            if (isStartEndConnected)
                mergePosition = MergePosition.StartEnd;

            if (isEndStartConnected)
                mergePosition = MergePosition.EndStart;
            if (isEndEndConnected)
                mergePosition = MergePosition.EndEnd;

            if (isBothEndsConnected)
                mergePosition = MergePosition.BothEnds;
            if (isCrossEndsConnected)
                mergePosition = MergePosition.CrossEnds;

            if (isStartStartConnected || isStartEndConnected || isEndStartConnected || isEndEndConnected)
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
            Ext.ThrowIfNotLRSType(geometry);
            Ext.ValidateLRSDimensions(ref geometry);
            Ext.ThrowIfMeasureIsNotInRange(measure, geometry);

            // If input geom is point; its a no-op just return the same.
            if (geometry.IsPoint())
                return geometry;

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
        /// <param name="tolerance">Tolerance</param>
        /// <returns>Returns Merged Geometry Segments</returns>
        public static SqlGeometry MergeGeometrySegments(SqlGeometry geometry1, SqlGeometry geometry2, double tolerance = Constants.Tolerance)
        {
            Ext.ThrowIfNotLine(geometry1, geometry2);
            Ext.ThrowIfSRIDsDoesNotMatch(geometry1, geometry2);
            Ext.ValidateLRSDimensions(ref geometry1);
            Ext.ValidateLRSDimensions(ref geometry2);

            var isConnected = IsConnected(geometry1, geometry2, tolerance, out MergePosition mergePosition);

            var mergeType = geometry1.GetMergeType(geometry2);
            if (isConnected)
            {
                switch (mergeType)
                {
                    case MergeInputType.LSLS:
                        return SimpleLineStringMerger(geometry1, geometry2, tolerance, mergePosition);
                    case MergeInputType.LSMLS:
                        // have to implement logic of this combinations
                        break;
                    case MergeInputType.MLSLS:
                        // have to implement logic of this combinations
                        break;
                    case MergeInputType.MLSMLS:
                        // have to implement logic of this combinations
                        break;
                }
            }
            else
            {
                // construct multi line
                return MultiLineStringMerger(geometry1, geometry2);
            }
            return null;
        }

        /// <summary>
        /// Returns the geometric segment at a specified offset from a geometric segment.
        /// Works only for LineString Geometry.
        /// </summary>
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

            // if clipped segment is null; then return null.
            if (geometry == null)
                return null;

            // TODO:: To handle for point.
            if (geometry.IsPoint())
                return geometry;

            var geomBuilder = new SqlGeometryBuilder();
            var geomSink = new OffsetGeometrySink(geomBuilder, offset, geometry.STLinearMeasureProgress());
            geometry.Populate(geomSink);
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
            Ext.ThrowIfNotLRSType(geometry);
            Ext.ValidateLRSDimensions(ref geometry);

            // As per requirement; 
            // the default value of start point is 0 when null is specified
            // the default value of end point is cartographic length of the segment when null is specified
            var localStartMeasure = startMeasure ?? 0;
            var localEndMeasure = endMeasure ?? geometry.STLength();

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
        /// Reverse and translate Linear Geometry
        /// Works only for POINT, LINESTRING, MULTILINESTRING Geometry.
        /// </summary>
        /// <param name="geometry">Input Geometry</param>
        /// <returns></returns>
        public static SqlGeometry ReverseAndTranslateGeometry(SqlGeometry geometry, double translateMeasure)
        {
            Ext.ThrowIfNotLRSType(geometry);
            Ext.ValidateLRSDimensions(ref geometry);

            var geometryBuilder = new SqlGeometryBuilder();
            var geomSink = new ReverseAndTranslateGeometrySink(geometryBuilder, translateMeasure);
            geometry.Populate(geomSink);
            return geometryBuilder.ConstructedGeometry;
        }

        /// <summary>
        /// Scale the measure values of Linear Geometry
        /// Works only for POINT, LINESTRING, MULTILINESTRING Geometry.
        /// </summary>
        /// <param name="geometry">Input Geometry</param>
        /// <returns></returns>
        public static SqlGeometry ScaleGeometryMeasures(SqlGeometry geometry, double scaleMeasure)
        {
            Ext.ThrowIfNotLRSType(geometry);
            Ext.ValidateLRSDimensions(ref geometry);

            var geometryBuilder = new SqlGeometryBuilder();
            var geomSink = new ScaleMeasureGeometrySink(geometryBuilder, scaleMeasure);
            geometry.Populate(geomSink);
            return geometryBuilder.ConstructedGeometry;
        }

        /// <summary>
        /// BuildMULTILINESTRING from two input geometry segments [LINESTRING, MULTILINESTRING]
        /// This should be called for merging geom segments when they are not connected.
        /// Here the offset measure is updated with the measure of second segment.
        /// </summary>
        /// <param name="geometry1">The geometry1.</param>
        /// <param name="geometry2">The geometry2.</param>
        /// <returns></returns>
        private static SqlGeometry MultiLineStringMerger(SqlGeometry geometry1, SqlGeometry geometry2)
        {
            var isSameDirection = geometry1.STSameDirection(geometry2);
            var firtSegmentDirection = geometry1.STLinearMeasureProgress();
            if (!isSameDirection)
                geometry2 = ScaleGeometryMeasures(geometry2, -1);

            var offsetM = geometry1.GetOffset(geometry2);
            var doUpdateM = false;

            if (isSameDirection)
            {
                if (firtSegmentDirection == LinearMeasureProgress.Increasing && offsetM > 0)
                    doUpdateM = true;
               
                if (firtSegmentDirection == LinearMeasureProgress.Decreasing && offsetM < 0)
                    doUpdateM = true;
            }
            else
                doUpdateM = true;

            var builder = new BuidLRSMultiLineSink();
            geometry1.Populate(builder);
            var lrsMultiline1 = builder.Lines;

            builder = new BuidLRSMultiLineSink();
            geometry2.Populate(builder);
            var lrsMultiline2 = builder.Lines;

            var geometryBuilder = new SqlGeometryBuilder();
            // Start Multiline
            geometryBuilder.SetSrid((int)geometry1.STSrid);
            geometryBuilder.BeginGeometry(OpenGisGeometryType.MultiLineString);

            // First Segment
            foreach (var line in lrsMultiline1.Lines)
            {
                var pointCounter = 1;
                geometryBuilder.BeginGeometry(OpenGisGeometryType.LineString);

                foreach (var point in line.Points)
                {
                    if (pointCounter == 1)
                        geometryBuilder.BeginFigure(point.x, point.y, point.z, point.m);
                    else
                        geometryBuilder.AddLine(point.x, point.y, point.z, point.m);
                    pointCounter++;
                }
                geometryBuilder.EndFigure();
                geometryBuilder.EndGeometry();
            }

            // Second Segment
            foreach (var line in lrsMultiline2.Lines)
            {
                var pointCounter = 1;
                geometryBuilder.BeginGeometry(OpenGisGeometryType.LineString);

                foreach (var point in line.Points)
                {
                    var measure = doUpdateM ? point.m + offsetM : point.m;
                    if (pointCounter == 1)
                        geometryBuilder.BeginFigure(point.x, point.y, point.z, measure);
                    else
                        geometryBuilder.AddLine(point.x, point.y, point.z, measure);
                    pointCounter++;
                }
                geometryBuilder.EndFigure();
                geometryBuilder.EndGeometry();
            }

            // End Multiline
            geometryBuilder.EndGeometry();

            return geometryBuilder.ConstructedGeometry;
        }

        /// <summary>
        /// Method will merge simple line strings with tolerance and returns the merged line segment by considering measure and direction of the first geometry.
        /// </summary>
        /// <param name="geometry1"></param>
        /// <param name="geometry2"></param>
        /// <param name="tolerance"></param>
        /// <returns>SqlGeometry</returns>
        private static SqlGeometry SimpleLineStringMerger(SqlGeometry geometry1, SqlGeometry geometry2, double tolerance, MergePosition mergePosition)
        {
            // geometry 1 and geometry 2 to be 2D line strings with measure 'm'
            Ext.ThrowIfNotLine(geometry1, geometry2);
            // offset measure difference.
            var offsetM = 0.0;

            // references governs the order of geometries to get merge
            SqlGeometry targetSegment = null, sourceSegment = null;

            // check direction of measure.
            var isSameDirection = geometry1.STSameDirection(geometry2);

            // segments must be connected in any of the following position.
            switch (mergePosition)
            {
                case MergePosition.EndStart:
                case MergePosition.CrossEnds:
                    {
                        // Single negation of measure is needed for geometry 2,
                        // if both segments are differ in measure variation
                        if (!isSameDirection)
                            geometry2 = ScaleGeometryMeasures(geometry2, -1);

                        offsetM = geometry1.STEndPoint().GetPointOffset(geometry2.STStartPoint());
                        geometry2 = TranslateMeasure(geometry2, offsetM);
                        sourceSegment = geometry1;
                        targetSegment = geometry2;
                        break;
                    }
                case MergePosition.EndEnd:
                case MergePosition.BothEnds:
                    {
                        // Double negation is needed for geometry 2, i.e., both segments differ from measure variation,
                        // also, geometry 2 has been traversed from ending point to the starting point
                        if (isSameDirection)
                            geometry2 = ScaleGeometryMeasures(geometry2, -1);

                        offsetM = geometry1.STEndPoint().GetPointOffset(geometry2.STEndPoint());
                        // Reverse the geometry 2, since it has been traversed from ending point to the starting point
                        // scale the measures of geometry 2 based on the offset measure difference between them
                        geometry2 = ReverseAndTranslateGeometry(geometry2, offsetM);
                        // start traversing from the geometry 1, hence g1 would be the source geometry
                        sourceSegment = geometry1;
                        targetSegment = geometry2;
                        break;
                    }
                case MergePosition.StartStart:
                    {
                        // Double negation is needed for geometry 2, i.e., both segments differ from measure variation,
                        // also, geometry 2 has been traversed from ending point to the starting point
                        if (isSameDirection)
                            geometry2 = ScaleGeometryMeasures(geometry2, -1);

                        offsetM = geometry1.STStartPoint().GetPointOffset(geometry2.STStartPoint());
                        // Reverse the geometry 2, since it has been traversed from ending point to the starting point
                        // scale the measures of geometry 2 based on the offset measure difference between them
                        geometry2 = ReverseAndTranslateGeometry(geometry2, offsetM);
                        // the starting point of g1 will become the intermediate point of resultant, so source geometry would be geometry 2
                        sourceSegment = geometry2;
                        targetSegment = geometry1;
                        break;
                    }
                case MergePosition.StartEnd:
                    {
                        // Single negation of measure is needed for geometry 2
                        // if both segments are differ in measure variation
                        if (!isSameDirection)
                            geometry2 = ScaleGeometryMeasures(geometry2, -1);

                        offsetM = (geometry1.STStartPoint().M.Value - geometry2.STEndPoint().M.Value);
                        // scale the measures of geometry 2 based on the offset measure difference between them
                        geometry2 = TranslateMeasure(geometry2, offsetM);
                        // the starting point of g1 will become the intermediate point of resultant, so source geometry would be geometry 2
                        sourceSegment = geometry2;
                        targetSegment = geometry1;
                        break;
                    }
                default:
                    throw new Exception("Invalid Merge Coordinate position");
            }

            // Builder for resultant merged geometry to store
            var geomBuilder = new SqlGeometryBuilder();

            // Building a line segment from the range of points by excluding the last point ( Merging point )
            var segment1 = new LineStringMergeGeometrySink(geomBuilder, true, sourceSegment.STNumPoints());
            sourceSegment.Populate(segment1);

            // Continuing to build segment from the points of second geometry
            var segment2 = new LineStringMergeGeometrySink(geomBuilder, false, targetSegment.STNumPoints());
            targetSegment.Populate(segment2);

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
            Ext.ThrowIfMeasureIsNotInRange(splitMeasure, geometry);

            var splitPoint = LocatePointAlongGeom(geometry, splitMeasure);
            var geometryBuilder1 = new SqlGeometryBuilder();
            var geometryBuilder2 = new SqlGeometryBuilder();
            var geomSink = new SplitGeometrySegmentSink(splitPoint, geometryBuilder1, geometryBuilder2);
            geometry.Populate(geomSink);
            geometry1 = geometryBuilder1.ConstructedGeometry;
            geometry2 = geometryBuilder2.ConstructedGeometry;
        }

        /// <summary>
        /// Translates the measure values of Input Geometry
        /// </summary>
        /// <param name="geometry">The geometry.</param>
        /// <param name="translateMeasure">The translate measure.</param>
        /// <returns>SqlGeometry with translated measure.</returns>
        public static SqlGeometry TranslateMeasure(SqlGeometry geometry, double translateMeasure)
        {
            Ext.ThrowIfNotLRSType(geometry);
            Ext.ValidateLRSDimensions(ref geometry);

            var geomBuilder = new SqlGeometryBuilder();
            var geomSink = new TranslateMeasureGeometrySink(geomBuilder, translateMeasure);
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
