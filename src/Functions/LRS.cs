/********************************************************
*  (c) Microsoft. All rights reserved.                  *
********************************************************/

using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Types;
using SQLSpatialTools.Sinks.Geometry;
using SQLSpatialTools.Types;
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
            return ClipAndRetainMeasure(geometry, clipStartMeasure, clipEndMeasure, tolerance, false);
        }

        /// <summary>
        /// Clip a geometry segment and retains its measure.
        /// </summary>
        /// <param name="geometry">The geometry.</param>
        /// <param name="clipStartMeasure">The clip start measure.</param>
        /// <param name="clipEndMeasure">The clip end measure.</param>
        /// <param name="tolerance">The tolerance.</param>
        /// <param name="retainMeasure">if set to <c>true</c> [retain measure].</param>
        /// <returns></returns>
        private static SqlGeometry ClipAndRetainMeasure(SqlGeometry geometry, double clipStartMeasure, double clipEndMeasure, double tolerance, bool retainMeasure)
        {
            Ext.ThrowIfNotLRSType(geometry);
            Ext.ValidateLRSDimensions(ref geometry);

            // if not multiline just return the line segment or point post clipping
            if (!geometry.IsMultiLineString())
                return ClipLineSegment(geometry, clipStartMeasure, clipEndMeasure, tolerance, retainMeasure);

            // for multi line
            var geomSink = new BuildLRSMultiLineSink();
            geometry.Populate(geomSink);
            var multiLine = geomSink.MultiLine;

            var clippedSegments = new List<SqlGeometry>();
            foreach (var line in geomSink.MultiLine)
            {
                var segment = ClipLineSegment(line.ToSqlGeometry(), clipStartMeasure, clipEndMeasure, tolerance, retainMeasure);
                // add only line segments
                if (segment != null && !segment.IsNull && !segment.STIsEmpty())
                    clippedSegments.Add(segment);
            }

            if (clippedSegments.Any())
            {
                // if one segment then it is a POINT or LINESTRING, so return straight away.
                if (clippedSegments.Count == 1)
                    return clippedSegments.First();

                var geomBuilder = new SqlGeometryBuilder();
                // count only LINESTRINGS
                var multiLineGeomSink = new BuildMultiLineFromLinesSink(geomBuilder, clippedSegments.Count(segment => segment.IsLineString()));

                foreach (var geom in clippedSegments)
                {
                    // ignore points
                    if (geom.IsLineString())
                        geom.Populate(multiLineGeomSink);
                }

                return geomBuilder.ConstructedGeometry;
            }

            return SqlGeometry.Null;
        }

        /// <summary>
        /// Clip a geometry segment based on specified measure.
        /// <br /> If the clipped start and end point is within tolerance of shape point then shape point is returned as start and end of clipped Geom segment.
        /// <br /> This function just hooks up and runs a pipeline using the sink.
        /// </summary>
        /// <param name="geometry">Input Geometry</param>
        /// <param name="clipStartMeasure">Start Measure</param>
        /// <param name="clipEndMeasure">End Measure</param>
        /// <param name="tolerance">Tolerance Value</param>
        /// <param name="retainClipMeasure">Flag to retain clip measures</param>
        /// <returns>Clipped Segment</returns>
        private static SqlGeometry ClipLineSegment(SqlGeometry geometry, double clipStartMeasure, double clipEndMeasure, double tolerance, bool retainClipMeasure)
        {
            bool startMeasureInvalid = false;
            bool endMeasureInvalid = false;

            // reassign clip start and end measure based upon there difference
            if (clipStartMeasure > clipEndMeasure)
            {
                var shiftObj = clipStartMeasure;
                clipStartMeasure = clipEndMeasure;
                clipEndMeasure = shiftObj;
            }

            // if point then compute here and return
            if (geometry.IsPoint())
            {
                var pointMeasure = geometry.HasM ? geometry.M.Value : 0;
                var isClipMeasureEqual = clipStartMeasure == clipEndMeasure;
                // no tolerance check, if both start and end measure is point measure then return point
                if (isClipMeasureEqual && pointMeasure == clipStartMeasure)
                    return geometry;
                else if (isClipMeasureEqual && (clipStartMeasure > pointMeasure || clipStartMeasure < pointMeasure))
                    Ext.ThrowLRSError(LRSErrorCodes.InvalidLRSMeasure);
                // if clip measure fall behind or beyond point measure then return null
                else if ((clipStartMeasure < pointMeasure && clipEndMeasure < pointMeasure) || (clipStartMeasure > pointMeasure && clipEndMeasure > pointMeasure))
                    return null;
                // else throw invalid LRS error.
                else
                    Ext.ThrowLRSError(LRSErrorCodes.InvalidLRS);
            }

            // Get the measure progress of linear geometry and reassign the start and end measures based upon the progression
            var measureProgress = geometry.STLinearMeasureProgress();
            var geomStartMeasure = measureProgress == LinearMeasureProgress.Increasing ? geometry.GetStartPointMeasure() : geometry.GetEndPointMeasure();
            var geomEndMeasure = measureProgress == LinearMeasureProgress.Increasing ? geometry.GetEndPointMeasure() : geometry.GetStartPointMeasure();

            // if clip start measure matches geom start measure and
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

            // end point check
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

            // if both clip start and end measure are reassigned to invalid then return null
            if (startMeasureInvalid || endMeasureInvalid)
                return null;

            // Post adjusting if clip start measure matches geom start measure and
            // clip end measure matches geom end measure then return the input geom
            if (clipStartMeasure == geomStartMeasure && clipEndMeasure == geomEndMeasure)
                return geometry;

            // if clip start and end measure are equal post adjusting then we will return a shape point
            if (clipStartMeasure == clipEndMeasure && (isStartBeyond || isEndBeyond))
            {
                if (isStartBeyond)
                    return geometry.STStartPoint();
                return geometry.STEndPoint();
            }

            // if both clip start and end measure is same then don't check for distance tolerance
            if (clipStartMeasure != clipEndMeasure)
            {
                var clipStartPoint = LocatePointWithTolerance(geometry, clipStartMeasure, tolerance);
                var clipEndPoint = LocatePointWithTolerance(geometry, clipEndMeasure, tolerance);
                if (clipStartPoint.IsWithinTolerance(clipEndPoint, tolerance))
                    return null;
            }

            var geometryBuilder = new SqlGeometryBuilder();
            var geomSink = new ClipMGeometrySegmentSink(clipStartMeasure, clipEndMeasure, geometryBuilder, tolerance, retainClipMeasure);
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
            return CheckIfConnected(geometry1, geometry2, tolerence, out _);
        }

        /// <summary>
        /// Checks if two geometric segments are spatially connected with merge position information
        /// </summary>
        /// <param name="geometry1">First Geometry</param>
        /// <param name="geometry2">Second Geometry</param>
        /// <param name="tolerence">Distance Threshold range; default 0.01F</param>
        /// <param name="mergeCoordinatePosition">Represents position of merge segments</param>
        /// <returns>SqlBoolean</returns>
        private static SqlBoolean CheckIfConnected(SqlGeometry geometry1, SqlGeometry geometry2, double tolerence, out MergePosition mergePosition)
        {
            Ext.ThrowIfNotLRSType(geometry1, geometry2);
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

            // if point is not derived then the measure is not in range.
            if (!geomSink.IsPointDerived)
                Ext.ThrowLRSError(LRSErrorCodes.InvalidLRSMeasure);

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
            Ext.ThrowIfNotLRSType(geometry1, geometry2);
            Ext.ThrowIfSRIDsDoesNotMatch(geometry1, geometry2);
            Ext.ValidateLRSDimensions(ref geometry1);
            Ext.ValidateLRSDimensions(ref geometry2);

            // If either of the input geom is point; then return the other geometry.
            if (geometry1.IsPoint())
                return geometry2;

            if (geometry2.IsPoint())
                return geometry1;

            var isConnected = CheckIfConnected(geometry1, geometry2, tolerance, out MergePosition mergePosition);
            var mergeType = geometry1.GetMergeType(geometry2);

            if (isConnected)
            {
                switch (mergeType)
                {
                    case MergeInputType.LSLS:
                        return MergeConnectedLineStrings(geometry1, geometry2, tolerance, mergePosition, out _);
                    case MergeInputType.LSMLS:
                    case MergeInputType.MLSLS:
                    case MergeInputType.MLSMLS:
                        return MergeConnectedMultiLineStrings(geometry1, geometry2, tolerance, mergePosition);
                }
            }
            else
            {
                // construct multi line
                return MergeDisconnectedLineSegments(geometry1, geometry2);
            }
            return null;
        }

        /// <summary>
        /// Method will merge simple line strings with tolerance and returns the merged line segment by considering measure and direction of the first geometry.
        /// </summary>
        /// <param name="geometry1"></param>
        /// <param name="geometry2"></param>
        /// <param name="tolerance"></param>
        /// <returns>SqlGeometry</returns>
        private static SqlGeometry MergeConnectedLineStrings(SqlGeometry geometry1, SqlGeometry geometry2, double tolerance, MergePosition mergePosition, out double measureDifference)
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
            measureDifference = offsetM;    // gives the offset measure difference for the caller method consumption
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
        /// Merges the Multi line and line string combinations with connected structure
        /// </summary>
        /// <param name="geometry1"></param>
        /// <param name="geometry2"></param>
        /// <param name="tolerance"></param>
        /// <param name="mergePosition"></param>
        /// <returns>SqlGeometry of type MultiLineString</returns>
        private static SqlGeometry MergeConnectedMultiLineStrings(SqlGeometry geometry1, SqlGeometry geometry2, double tolerance, MergePosition mergePosition)
        {
            // check direction of measure.
            var isSameDirection = geometry1.STSameDirection(geometry2);

            BuildLRSMultiLineSink geom1Line = new BuildLRSMultiLineSink();
            geometry1.Populate(geom1Line);

            BuildLRSMultiLineSink geom2Line = new BuildLRSMultiLineSink();
            geometry2.Populate(geom2Line);

            SqlGeometry sourceSegment, targetSegment, mergedSegment;

            var segment1 = geom1Line.MultiLine;
            var segment2 = geom2Line.MultiLine;

            switch (mergePosition)
            {
                case MergePosition.EndEnd:
                case MergePosition.BothEnds:
                    {
                        //  Double Negation of measure is needed, since geometry2 has been traversed from end point to the starting point also differ from measure variation
                        if (isSameDirection)
                            segment2.ScaleMeasure(-1);

                        sourceSegment = segment1.GetLastLine().ToSqlGeometry();
                        targetSegment = segment2.GetLastLine().ToSqlGeometry();
                        //  Generating merged segment of geometry1 and geometry2
                        mergedSegment = MergeConnectedLineStrings(sourceSegment, targetSegment, tolerance, mergePosition, out double measureDifference);

                        var mergedGeom = new BuildLRSMultiLineSink();
                        mergedSegment.Populate(mergedGeom);

                        segment1.RemoveLast();                          //  Removing merging line segment from the geometry1
                        segment2.RemoveLast();                          //  Removing merging line segment from the geometry2

                        segment2.ReverseLinesAndPoints();               //  Traversing from end to the start of geometry2. So reversing the 
                        segment2.TranslateMeasure(measureDifference);   //  Translating the offset measure difference in segment2

                        segment1.AddLines(mergedGeom.MultiLine.Lines);  //  appending merged segment line to the segment1 , since geometry1 would be the beginning geometry of the resultant geometry
                        segment1.AddLines(segment2.Lines);              //  appending remaining segment of updated geometry2 with the segment1
                        return segment1.ToSqlGeometry();                //  converting to SqlGeometry type
                    }
                case MergePosition.EndStart:
                case MergePosition.CrossEnds:
                    {
                        //  Negation of measure is needed, since measure variation of geometry2 differs from that of geometry1
                        if (!isSameDirection)
                            segment2.ScaleMeasure(-1);

                        sourceSegment = segment1.GetLastLine().ToSqlGeometry();
                        targetSegment = segment2.GetFirstLine().ToSqlGeometry();
                        //  Generating merged segment of geometry1 and geometry2
                        mergedSegment = MergeConnectedLineStrings(sourceSegment, targetSegment, tolerance, mergePosition, out double measureDifference);

                        var mergedGeom = new BuildLRSMultiLineSink();
                        mergedSegment.Populate(mergedGeom);

                        segment1.RemoveLast();                          //  Removing merging line segment from the geometry1
                        segment2.RemoveFirst();                         //  Removing merging line segment from the geometry2

                        segment2.TranslateMeasure(measureDifference);   //  Translating the offset measure difference in segment2

                        segment1.AddLines(mergedGeom.MultiLine.Lines);  //  Appending merged segment line to the segment1 , since geometry1 would be the beginning geometry of the resultant geometry
                        segment1.AddLines(segment2.Lines);              //  Appending remaining segment of updated geometry2 with the segment1
                        return segment1.ToSqlGeometry();                //  converting to SqlGeometry type
                    }
                case MergePosition.StartEnd:
                    {
                        //  Negation of measure is needed, since measure variation of geometry2 differs from that of geometry1
                        if (!isSameDirection)
                            segment2.ScaleMeasure(-1);

                        sourceSegment = segment1.GetFirstLine().ToSqlGeometry();
                        targetSegment = segment2.GetLastLine().ToSqlGeometry();
                        //  Generating merged segment of geometry1 and geometry2
                        mergedSegment = MergeConnectedLineStrings(sourceSegment, targetSegment, tolerance, mergePosition, out double measureDifference);

                        var mergedGeom = new BuildLRSMultiLineSink();
                        mergedSegment.Populate(mergedGeom);

                        segment1.RemoveFirst();                         //  Removing merging line segment from the geometry1
                        segment2.RemoveLast();                          //  Removing merging line segment from the geometry2

                        segment2.TranslateMeasure(measureDifference);   //  Translating the offset measure difference in segment2

                        segment2.AddLines(mergedGeom.MultiLine.Lines);  //  Appending merged segment line to the segment2 ,since geometry1 would be the beginning geometry of the resultant geometry
                        segment2.AddLines(segment1.Lines);              //  Appending remaining segments of geometry1 with the geometry2
                        return segment2.ToSqlGeometry();                //  converting to SqlGeometry type
                    }
                case MergePosition.StartStart:
                    {
                        //  Double Negation of measure is needed, since geometry2 has been traversed from end point to the starting point also differ from measure variation
                        if (isSameDirection)
                            segment2.ScaleMeasure(-1);

                        sourceSegment = segment1.GetFirstLine().ToSqlGeometry();
                        targetSegment = segment2.GetFirstLine().ToSqlGeometry();
                        //  Generating merged segment of geometry1 and geometry2
                        mergedSegment = MergeConnectedLineStrings(sourceSegment, targetSegment, tolerance, mergePosition, out double measureDifference);

                        var mergedGeom = new BuildLRSMultiLineSink();
                        mergedSegment.Populate(mergedGeom);

                        segment1.RemoveFirst();                         //  Removing merging line segment from the geometry1
                        segment2.RemoveFirst();                         //  Removing merging line segment from the geometry2

                        segment2.ReverseLinesAndPoints();               //  Reversing the lines and its corresponding points of segment2, Since it has been traversed from end to start
                        segment2.TranslateMeasure(measureDifference);   //  Translating the offset measure difference in segment2

                        segment2.AddLines(mergedGeom.MultiLine.Lines);  //  Appending merged segment line to the segment2 ,since geometry1 would be the beginning geometry of the resultant geometry
                        segment2.AddLines(segment1.Lines);              //  Appending remaining segments of geometry1 with the geometry2
                        return segment2.ToSqlGeometry();                //  converting to SqlGeometry type
                    }
                default:
                    return null;
            }
        }

        /// <summary>
        /// Build MULTILINESTRING from two input geometry segments [LINESTRING, MULTILINESTRING]
        /// This should be called for merging geom segments when they are not connected.
        /// Here the offset measure is updated with the measure of second segment.
        /// </summary>
        /// <param name="geometry1">The geometry1.</param>
        /// <param name="geometry2">The geometry2.</param>
        /// <returns></returns>
        private static SqlGeometry MergeDisconnectedLineSegments(SqlGeometry geometry1, SqlGeometry geometry2)
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

            var builder = new BuildLRSMultiLineSink();
            geometry1.Populate(builder);
            var lrsMultiline1 = builder.MultiLine;

            builder = new BuildLRSMultiLineSink();
            geometry2.Populate(builder);
            var lrsMultiline2 = builder.MultiLine;

            var geometryBuilder = new SqlGeometryBuilder();
            // Start Multiline
            geometryBuilder.SetSrid((int)geometry1.STSrid);
            geometryBuilder.BeginGeometry(OpenGisGeometryType.MultiLineString);

            // First Segment
            foreach (var line in lrsMultiline1)
            {
                var pointCounter = 1;
                geometryBuilder.BeginGeometry(OpenGisGeometryType.LineString);

                foreach (var point in line.Points)
                {
                    if (pointCounter == 1)
                        geometryBuilder.BeginFigure(point.X, point.Y, point.Z, point.M);
                    else
                        geometryBuilder.AddLine(point.X, point.Y, point.Z, point.M);
                    pointCounter++;
                }
                geometryBuilder.EndFigure();
                geometryBuilder.EndGeometry();
            }

            // Second Segment
            foreach (var line in lrsMultiline2)
            {
                var pointCounter = 1;
                geometryBuilder.BeginGeometry(OpenGisGeometryType.LineString);

                foreach (var point in line.Points)
                {
                    var measure = doUpdateM ? point.M + offsetM : point.M;
                    if (pointCounter == 1)
                        geometryBuilder.BeginFigure(point.X, point.Y, point.Z, measure);
                    else
                        geometryBuilder.AddLine(point.X, point.Y, point.Z, measure);
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
        /// Returns the geometric segment at a specified offset from a geometric segment.
        /// Works only for LineString and MultiLineString Geometry; Point is not supported.
        /// </summary>
        /// <param name="geometry">Input Geometry</param>
        /// <param name="startMeasure">Start Measure</param>
        /// <param name="endMeasure">End Measure</param>
        /// <param name="offset">Offset value</param>
        /// <param name="tolerance">Tolerance</param>
        /// <returns>Offset Geometry Segment</returns>
        public static SqlGeometry OffsetGeometrySegment(SqlGeometry geometry, double startMeasure, double endMeasure, double offset, double tolerance = Constants.Tolerance)
        {
            // If point throw invalid LRS Segment error.
            if (geometry.IsPoint())
                Ext.ThrowLRSError(LRSErrorCodes.InvalidLRS);

            Ext.ThrowIfNotLineOrMultiLine(geometry);
            Ext.ValidateLRSDimensions(ref geometry);

            // to retain clip measures on offset
            var clippedGeometry = ClipAndRetainMeasure(geometry, startMeasure, endMeasure, tolerance, true);

            // if clipped segment is null; then return null.
            if (clippedGeometry == null)
                return null;

            var geomBuilder = new SqlGeometryBuilder();
            var geomSink = new OffsetGeometrySink(geomBuilder, offset, geometry.STLinearMeasureProgress());

            // Explicit handle if clipped segment is Point
            // As point has a single co-ordinate we need to consider the angle from input segment, not from the clipped segment
            if (clippedGeometry.IsPoint())
            {
                // populate with input geom rather than clipped segment
                geometry.Populate(geomSink);
                return geomBuilder.ConstructedGeometry.GetPointAtMeasure(clippedGeometry.M.Value);
            }

            // remove collinear points
            var trimmedGeom = RemoveCollinearPoints(clippedGeometry);

            // for multi line
            if (trimmedGeom.IsMultiLine)
            {
                geomSink = new OffsetGeometrySink(geomBuilder, offset, geometry.STLinearMeasureProgress(), true, trimmedGeom.Count);

                foreach (var line in trimmedGeom)
                    line.ToSqlGeometry().Populate(geomSink);
            }
            // else it should be line string
            else
            {
                trimmedGeom.ToSqlGeometry().Populate(geomSink);
            }

            return geomBuilder.ConstructedGeometry;
        }

        /// <summary>
        /// Removes the collinear points.
        /// </summary>
        /// <param name="sqlGeometry">The SQL geometry.</param>
        /// <returns></returns>
        private static LRSMultiLine RemoveCollinearPoints(SqlGeometry sqlGeometry)
        {
            // populate the input segment
            var lrsBuilder = new BuildLRSMultiLineSink();
            sqlGeometry.Populate(lrsBuilder);

            // If the input segment has only two points; then there is no way of collinearity 
            // so returning the input segment
            if (sqlGeometry.STNumPoints() <= 2)
                return lrsBuilder.MultiLine;

            // remove collinear points
            lrsBuilder.MultiLine.RemoveCollinearPoints();

            return lrsBuilder.MultiLine;
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
        /// Split a geometry into geometry segments based on split measure. 
        /// This function just hooks up and runs a pipeline using the sink.
        /// </summary>
        /// <param name="geometry">Input Geometry</param>
        /// <param name="splitMeasure"></param>
        /// <param name="geometry1">First Geometry Segment</param>
        /// <param name="geometry2">Second Geometry Segment</param>
        public static void SplitGeometrySegment(SqlGeometry geometry, double splitMeasure, out SqlGeometry geometry1, out SqlGeometry geometry2)
        {
            Ext.ThrowIfNotLRSType(geometry);
            Ext.ValidateLRSDimensions(ref geometry);
            
            // if point then check if measure is equal to split measure
            // if not equal then throw invalid measure exception
            // if equal return both the segments as null.
            if (geometry.IsPoint())
            {
                var pointMeasure = geometry.HasM ? geometry.M.Value : 0;
                if (pointMeasure != splitMeasure)
                    Ext.ThrowLRSError(LRSErrorCodes.InvalidLRSMeasure);

                geometry1 = null;
                geometry2 = null;
                return;
            }

            // measure range validation is handled inside LocatePoint
            var splitPoint = LocatePointAlongGeom(geometry, splitMeasure);

            var geomSink = new SplitGeometrySegmentSink(splitPoint);
            geometry.Populate(geomSink);
            geometry1 = geomSink.Segment1;
            geometry2 = geomSink.Segment2;
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

            // check for dimension
            if (geometry.STGetDimension() == DimensionalInfo._2D)
            {
                // If there is no measure value; return invalid.
                return LRSErrorCodes.InvalidLRS.Value();
            }

            // convert to valid 3 point LRS co-ordinate.
            Ext.ValidateLRSDimensions(ref geometry);

            // return invalid if empty or is of geometry collection
            if (geometry.IsNull || geometry.STIsEmpty() || !geometry.STIsValid() || geometry.IsGeometryCollection())
                return LRSErrorCodes.InvalidLRS.Value();

            // return invalid if geometry doesn't or have null values
            if (!geometry.STHasMeasureValues())
                return LRSErrorCodes.InvalidLRSMeasure.Value();

            // checks if the measures are in linear range.
            var geomBuilder = new SqlGeometryBuilder();
            var geomSink = new ValidateLinearMeasureGeometrySink(geomBuilder, geometry.STLinearMeasureProgress());
            geometry.Populate(geomSink);

            return geomSink.IsLinearMeasure() ? LRSErrorCodes.ValidLRS.Value() : LRSErrorCodes.InvalidLRSMeasure.Value();
        }
    }
}
