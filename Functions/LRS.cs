/********************************************************
*  (c) Microsoft. All rights reserved.                  *
********************************************************/

using System;
using Microsoft.SqlServer.Types;
using SQLSpatialTools.Utility;

namespace SQLSpatialTools.Function
{
    /// <summary>
    /// This class contains LRS functions that can be registered in SQL Server.
    /// </summary>
    public class LRS
    {
        public class Geometry
        {
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
            /// Reverse Linear Geometry
            /// Works only for LineString Geometry.
            /// </summary>
            /// <param name="geometry">Input Geometry</param>
            /// <returns></returns>
            public static SqlGeometry ReverseLinearGeometry(SqlGeometry geometry)
            {
                if (!geometry.IsLineString())
                    throw new Exception("LINESTRING is currently the only spatial type supported");
                return General.ReverseLinestring(geometry);
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
                var localEndMeasure = endMeasure ?? geometry.GetLastPointMeasure();
                var length = geometry.STLength().Value;

                var geomBuilder = new SqlGeometryBuilder();
                var geomSink = new PopulateGeometryMeasuresSink(localStartMeasure, localEndMeasure, length, geomBuilder);
                geometry.Populate(geomSink);
                return geomBuilder.ConstructedGeometry;
            }

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
                var geomStartPoint = geometry.GetStartPointMeasure();
                var geomLastPoint = geometry.GetLastPointMeasure();

                if(startMeasure.IsWithinRange(geomStartPoint, geomLastPoint))
                    throw new Exception(string.Format("Start measure {0}", startMeasure.LinearGeometryRangeExpectionMessage(startMeasure, endMeasure)));

                if(endMeasure.IsWithinRange(geomStartPoint, geomLastPoint))
                    throw new Exception(string.Format("End measure {0}", endMeasure.LinearGeometryRangeExpectionMessage(startMeasure, endMeasure)));

                var geometryBuilder = new SqlGeometryBuilder();
                var geomSink = new ClipGeometrySegmentSink2(startMeasure, endMeasure, geometryBuilder);
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
                var splitPoint = LocateMeasureAlongGeometry(geometry, splitMeasure);
                var geometryBuilder1 = new SqlGeometryBuilder();
                var geometryBuilder2 = new SqlGeometryBuilder();
                var geomSink = new SplitGeometrySegmentSink(splitPoint, geometryBuilder1, geometryBuilder2);
                geometry.Populate(geomSink);
                geometry1 = geometryBuilder1.ConstructedGeometry;
                geometry2 = geometryBuilder2.ConstructedGeometry;
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
                var mOffset = geometry1.GetOffset(geometry2);
                var geomBuilder = new SqlGeometryBuilder();

                // build first geometry
                var geomSink = new MergeGeometrySegmentSink(geomBuilder, true, false, null);
                geometry1.Populate(geomSink);

                // join second geometry
                geomSink = new MergeGeometrySegmentSink(geomBuilder, false, true, mOffset);
                geometry1.Populate(geomSink);
                return geomBuilder.ConstructedGeometry;
            }
        }
    }
}