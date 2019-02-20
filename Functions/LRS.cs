/********************************************************
*  (c) Microsoft. All rights reserved.                  *
********************************************************/

using System;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Types;
using SQLSpatialTools.Utility;
using Ext = SQLSpatialTools.Utility.SpatialExtensions;

namespace SQLSpatialTools.Function
{
    /// <summary>
    /// This class contains LRS functions that can be registered in SQL Server.
    /// </summary>
    public class LRS
    {
        public const string LineStringCompatibleErrorMessage = "LINESTRING is currently the only spatial type supported";
        public const string SRIDCompatibleErrorMessage = "SRID's of geometries doesn't match";

        /// <summary>
        /// This provides data manipulation on planar Geometry data type.
        /// </summary>
        public class Geometry
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
                if (!geometry.IsLineString())
                    throw new ArgumentException(LineStringCompatibleErrorMessage);

                if (startMeasure.IsWithinRange(geometry))
                    Ext.ThrowException("Start measure {0}", startMeasure.LinearGeometryRangeExpectionMessage(startMeasure, endMeasure));

                if (endMeasure.IsWithinRange(geometry))
                    Ext.ThrowException("End measure {0}", endMeasure.LinearGeometryRangeExpectionMessage(startMeasure, endMeasure));

                var geometryBuilder = new SqlGeometryBuilder();
                var geomSink = new ClipGeometrySegmentSink2(startMeasure, endMeasure, geometryBuilder);
                geometry.Populate(geomSink);
                return geometryBuilder.ConstructedGeometry;
            }

            /// <summary>
            /// Get end point measure of a LRS Geom Segment.
            /// </summary>
            /// <param name="geometry">Input Geometry</param>
            /// <returns>End measure</returns>
            public static SqlDouble GetGeomSegmentEndMeasure(SqlGeometry geometry)
            {
                if (!(geometry.IsLineString() || geometry.IsMultiLineString()))
                    throw new ArgumentException(LineStringCompatibleErrorMessage);

                return geometry.GetEndPointMeasure();
            }

            /// <summary>
            /// Get start point measure of a LRS Geom Segment.
            /// </summary>
            /// <param name="geometry">Input Geometry</param>
            /// <returns>Start measure</returns>
            public static double GetGeomSegmentStartMeasure(SqlGeometry geometry)
            {
                if (!(geometry.IsLineString() || geometry.IsMultiLineString()))
                    throw new ArgumentException(LineStringCompatibleErrorMessage);

                return geometry.GetStartPointMeasure();
            }

            /// <summary>
            /// Checks if two geometric segments are spatially connected.
            /// </summary>
            /// <param name="geometry1">First Geometry</param>
            /// <param name="geometry2">Second Geometry</param>
            /// <param name="tolerence">Distance Threshold range; default 0.01F</param>
            /// <returns></returns>
            public static SqlBoolean IsSpatiallyConnected(SqlGeometry geometry1, SqlGeometry geometry2, double tolerence = 0.01F)
            {
                if (!geometry1.IsLineString() || !geometry2.IsLineString())
                    throw new ArgumentException(LineStringCompatibleErrorMessage);

                // if SRID's of geometry1 and geometry doesn't match return false
                if (!geometry1.STSrid.Equals(geometry2.STSrid))
                    throw new ArgumentException(SRIDCompatibleErrorMessage);

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
                if (geometry1StartPoint.STDistance(geometry2StartPoint) <= tolerence || geometry1StartPoint.STDistance(geometry2EndPoint) <= tolerence)
                    return true;

                // Comparing geom1 start point distance against geom2 start and end points
                if (geometry1EndPoint.STDistance(geometry2StartPoint) <= tolerence || geometry1EndPoint.STDistance(geometry2EndPoint) <= tolerence)
                    return true;

                return false;
            }

            /// <summary>
            /// Locate the Geometry Point along the specified measure on the Geometry.
            /// This function just hooks up and runs a pipeline using the sink.
            /// </summary>
            /// <param name="geometry">Input Geometry</param>
            /// <param name="distance">Measure of the Geometry point to locate</param>
            /// <returns>Geometry Point</returns>
            public static SqlGeometry LocateMeasureAlongGeometry(SqlGeometry geometry, double distance)
            {
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
                if (!geometry1.IsLineString() || !geometry2.IsLineString())
                    throw new ArgumentException(LineStringCompatibleErrorMessage);

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
                var localStartMeasure = startMeasure ?? geometry.GetStartPointMeasure();
                var localEndMeasure = endMeasure ?? geometry.GetEndPointMeasure();
                var length = geometry.STLength().Value;

                var geomBuilder = new SqlGeometryBuilder();
                var geomSink = new PopulateGeometryMeasuresSink(localStartMeasure, localEndMeasure, length, geomBuilder);
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
                if (!geometry.IsLineString())
                    throw new Exception(LineStringCompatibleErrorMessage);
                return General.ReverseLinestring(geometry);
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
                if (!geometry.IsLineString())
                    throw new ArgumentException(LineStringCompatibleErrorMessage);

                var splitPoint = LocateMeasureAlongGeometry(geometry, splitMeasure);
                var geometryBuilder1 = new SqlGeometryBuilder();
                var geometryBuilder2 = new SqlGeometryBuilder();
                var geomSink = new SplitGeometrySegmentSink(splitPoint, geometryBuilder1, geometryBuilder2);
                geometry.Populate(geomSink);
                geometry1 = geometryBuilder1.ConstructedGeometry;
                geometry2 = geometryBuilder2.ConstructedGeometry;
            }
        }
    }
}