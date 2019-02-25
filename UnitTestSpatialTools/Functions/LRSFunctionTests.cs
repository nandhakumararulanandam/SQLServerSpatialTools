using System;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Types;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SQLSpatialTools.Function;
using SQLSpatialTools.Utility;

namespace SQLSpatialTools.Tests
{
    [TestClass]
    public class LRSFunctionTests : UnitTest
    {
        [TestMethod]
        public void ClipGeometrySegmentTest()
        {
            var geom = "MULTILINESTRING((100 100, 200 200), (3 4, 7 8, 10 10))".GetGeom();
            Logger.Log("Input Geom : {0}", geom.ToString());
            try
            {
                LRS.Geometry.ClipGeometrySegment(geom, 15, 20);
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual(e.Message, ErrorMessage.LineStringCompatible);
                TestContext.WriteLine(ErrorMessage.LineStringCompatible);
            }

            // line string with null z value
            geom = "LINESTRING (10 1 NULL 10, 25 1 NULL 25 )".GetGeom();
            Logger.LogLine("Input Geom : {0}", geom.ToString());

            int startMeasure = 5, endMeasure = 10;
            Logger.Log("Clip input geom with a Start Measure: {0} and End Measure: {1}", startMeasure, endMeasure);
            try
            {
                LRS.Geometry.ClipGeometrySegment(geom, startMeasure, endMeasure);
            }
            catch (ArgumentException e)
            {
                SqlAssert.Contains(e.Message, "not within the measure range");
                Logger.Log(e.Message);
            }

            startMeasure = 15; endMeasure = 27;
            Logger.LogLine("Clip input geom with a Start Measure: {0} and End Measure: {1}", startMeasure, endMeasure);
            try
            {
                LRS.Geometry.ClipGeometrySegment(geom, startMeasure, endMeasure);
            }
            catch (ArgumentException e)
            {
                SqlAssert.Contains(e.Message, "not within the measure range"); ;
                Logger.Log(e.Message);
            }

            // From start to 5 point measure
            startMeasure = 10; endMeasure = 15;
            Logger.LogLine("Clip input geom with a Start Measure: {0} and End Measure: {1}", startMeasure, endMeasure);

            var retGeom = "LINESTRING (10 1 NULL 10, 15 1 NULL 15 )".GetGeom();
            var clippedGeom = LRS.Geometry.ClipGeometrySegment(geom, startMeasure, endMeasure);
            Logger.Log("Clipped Geom: {0}", clippedGeom.ToString());
            SqlAssert.IsTrue(retGeom.STIsValid());
            SqlAssert.IsTrue(clippedGeom.STEquals(retGeom));

            // From 15 to 20
            startMeasure = 15; endMeasure = 20;
            Logger.LogLine("Clip input geom with a Start Measure: {0} and End Measure: {1}", startMeasure, endMeasure);

            retGeom = "LINESTRING (15 1 NULL 15, 20 1 NULL 20 )".GetGeom();
            clippedGeom = LRS.Geometry.ClipGeometrySegment(geom, startMeasure, endMeasure);
            Logger.Log("Clipped Geom: {0}", clippedGeom.ToString());
            SqlAssert.IsTrue(retGeom.STIsValid());
            SqlAssert.IsTrue(clippedGeom.STEquals(retGeom));

            // From 20 to 25
            startMeasure = 20; endMeasure = 25;
            Logger.LogLine("Clip input geom with a Start Measure: {0} and End Measure: {1}", startMeasure, endMeasure);

            retGeom = "LINESTRING (20 1 NULL 20, 25 1 NULL 25 )".GetGeom();
            clippedGeom = LRS.Geometry.ClipGeometrySegment(geom, startMeasure, endMeasure);
            Logger.Log("Clipped Geom: {0}", clippedGeom.ToString());
            SqlAssert.IsTrue(retGeom.STIsValid());
            SqlAssert.IsTrue(clippedGeom.STEquals(retGeom));
        }

        [TestMethod]
        public void GetGeomSegmentEndMeasureTest()
        {
            var endMeasureValue = 10.0F;
            var geom = string.Format("LINESTRING(0 0 0 0, 1 1 0 0, 3 4 0 0, 5.5 5 1000 {0})", endMeasureValue).GetGeom();
            SqlDouble endMeasure = LRS.Geometry.GetGeomSegmentEndMeasure(geom);
            SqlAssert.AreEqual(endMeasure, endMeasureValue);

            endMeasureValue = 100.000999450684F;
            geom = string.Format("MULTILINESTRING((0 0 0 0, 1 1 0 0), (3 2 0 null, 5 5 2 {0}))", endMeasureValue ).GetGeom();
            endMeasure = LRS.Geometry.GetGeomSegmentEndMeasure(geom);
            SqlAssert.AreEqual(endMeasure, endMeasureValue);

            try
            {
                geom = ("POLYGON((0 0 0 0, 1 1 0 0, 3 4 0 0, 5.5 5 1000, 0 0 0 0))").GetGeom();
                endMeasure = LRS.Geometry.GetGeomSegmentEndMeasure(geom);
                Assert.Fail("Method GetGeomSegmentEndMeasureTest should not accept polygon geometric structure");
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual(e.Message, ErrorMessage.LineStringCompatible);
                TestContext.WriteLine(ErrorMessage.LineStringCompatible);
            }
        }

        [TestMethod]
        public void GetGeomSegmentStartMeasureTest()
        {
            var startMeasureValue = 10.0F;
            var geom = string.Format("LINESTRING(0 0 0 {0}, 1 1 0 0, 3 4 0 0, 5.5 5 1000 0)", startMeasureValue).GetGeom();
            SqlDouble endMeasure = LRS.Geometry.GetGeomSegmentStartMeasure(geom);
            SqlAssert.AreEqual(endMeasure, startMeasureValue);

            startMeasureValue = 100.000999450684F;
            geom = string.Format("MULTILINESTRING((0 0 0 {0}, 1 1 0 0), (3 2 0 null, 5 5 2 {0}))", startMeasureValue).GetGeom();
            endMeasure = LRS.Geometry.GetGeomSegmentStartMeasure(geom);
            SqlAssert.AreEqual(endMeasure, startMeasureValue);

            try
            {
                geom = ("POLYGON((0 0 0 0, 1 1 0 0, 3 4 0 0, 5.5 5 1000, 0 0 0 0))").GetGeom();
                endMeasure = LRS.Geometry.GetGeomSegmentStartMeasure(geom);
                Assert.Fail("Method GetGeomSegmentStartMeasure should not accept polygon geometric structure");
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual(e.Message, ErrorMessage.LineStringCompatible);
                TestContext.WriteLine(ErrorMessage.LineStringCompatible);
            }
        }

        [TestMethod]
        public void InterpolateBetweenGeomWithMeasureTest()
        {
            var geom1 = "POINT(0 0 0 0)".GetGeom();
            var geom2 = "POINT(10 0 0 10)".GetGeom();
            var returnPoint = "POINT (5 0 NULL 5)".GetGeom();
            var distance = 5;
            Logger.LogLine("Input Point 1:{0} Point 2:{1}", geom1, geom2);
            Logger.Log("Interpolating at a distance of {0}", geom1, geom2, distance);
            Logger.LogLine("Expected Point: {0}", returnPoint.ToString());
            var sqlgeom = LRS.Geometry.InterpolateBetweenGeomWithMeasure(geom1, geom2, distance);
            Logger.Log("Obtained Point: {0}", sqlgeom.ToString());
            SqlAssert.IsTrue(sqlgeom.STEquals(returnPoint));
        }

        [TestMethod]
        public void IsSpatiallyConnectedTest()
        {
            // test cases without considering tolerance values
            var geom1 = "LINESTRING(0 0 0 0, 1 1 0 0, 3 4 0 0, 5.5 5 0 0)".GetGeom();
            var geom2 = "LINESTRING(0 0 0 0, 2 2 0 0)".GetGeom();
            var tolerance = Constants.THRESHOLD;
            var result = LRS.Geometry.IsSpatiallyConnected(geom1, geom2, tolerance);
            SqlAssert.IsTrue(result);

            // Different SRIDs Failure Test
            try
            {
                geom2 = "LINESTRING(0 0 0 0, 2 2 0 0)".GetGeom(10);
                LRS.Geometry.IsSpatiallyConnected(geom1, geom2, tolerance);
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual(e.Message, ErrorMessage.SRIDCompatible);
                TestContext.WriteLine(ErrorMessage.SRIDCompatible);
            }

            geom1 = "LINESTRING(0 0 0 0, 1 1 0 0, 3 4 0 0, 5.5 5 0 0)".GetGeom();
            geom2 = "LINESTRING(2 2 0 0, 0 0 0 0)".GetGeom();
            tolerance = 0;
            result = LRS.Geometry.IsSpatiallyConnected(geom1, geom2, tolerance);
            SqlAssert.IsTrue(result);

            geom1 = "LINESTRING(0 0 0 0, 1 1 0 0, 3 4 0 0, 5.5 5 0 0)".GetGeom();
            geom2 = "LINESTRING(5.5 5 0 0, 2 2 0 0)".GetGeom();
            tolerance = 0;
            result = LRS.Geometry.IsSpatiallyConnected(geom1, geom2, tolerance);
            SqlAssert.IsTrue(result);

            geom1 = "LINESTRING(0 0 0 0, 1 1 0 0, 3 4 0 0, 5.5 5 0 0)".GetGeom();
            geom2 = "LINESTRING(2 2 9 0, 5.5 5 0 0)".GetGeom();
            tolerance = 0;
            result = LRS.Geometry.IsSpatiallyConnected(geom1, geom2, tolerance);
            SqlAssert.IsTrue(result);

            // test cases with tolerance values considered
            geom1 = "LINESTRING(0 0 0 0, 1 1 0 0, 3 4 0 0, 5.5 5 0 0)".GetGeom();
            geom2 = "LINESTRING(0.5 0.5 0 0, 2 2 0 0)".GetGeom();
            tolerance = 0.5;
            result = LRS.Geometry.IsSpatiallyConnected(geom1, geom2, tolerance);
            SqlAssert.IsFalse(result);

            geom1 = "LINESTRING(0 0 0 0, 1 1 0 0, 3 4 0 0, 5.5 5 0 0)".GetGeom();
            geom2 = "LINESTRING(0.5 0 0 0, 2 2 0 0)".GetGeom();
            tolerance = 0.5;
            result = LRS.Geometry.IsSpatiallyConnected(geom1, geom2, tolerance);
            SqlAssert.IsTrue(result);

            geom1 = "LINESTRING(0 0 0 0, 1 1 0 0, 3 4 0 0, 5.5 5 0 0)".GetGeom();
            geom2 = "LINESTRING(1.5 1.5 0 0, 2 2 0 0)".GetGeom();
            tolerance = 0.5;
            result = LRS.Geometry.IsSpatiallyConnected(geom1, geom2, tolerance);
            SqlAssert.IsFalse(result);

            geom1 = "LINESTRING(0 0 0 0, 1 1 0 0, 3 4 0 0, 5.5 5 0 0)".GetGeom();
            geom2 = "LINESTRING(0 0.5 0 0, 2 2 0 0)".GetGeom();
            tolerance = 0.5;
            result = LRS.Geometry.IsSpatiallyConnected(geom1, geom2, tolerance);
            SqlAssert.IsTrue(result);

            geom1 = "LINESTRING(0 0 0 0, 1 1 0 0, 3 4 0 0, 5.5 5 0 0)".GetGeom();
            geom2 = "LINESTRING(2 2 0 0, 0.1 0.6 0 0)".GetGeom();
            tolerance = 0.5;
            result = LRS.Geometry.IsSpatiallyConnected(geom1, geom2, tolerance);
            SqlAssert.IsFalse(result);

            geom1 = "LINESTRING(0 0 0 0, 1 1 0 0, 3 4 0 0, 5.5 5 0 0)".GetGeom();
            geom2 = "LINESTRING(2 2 0 0, 0.6 0.1 0 0)".GetGeom();
            tolerance = 1;
            result = LRS.Geometry.IsSpatiallyConnected(geom1, geom2, tolerance);
            SqlAssert.IsTrue(result);

            geom1 = "LINESTRING(0 0 0 0, 1 1 0 0, 3 4 0 0, 5.5 5 0 0)".GetGeom();
            geom2 = "LINESTRING(5 5 0 0, 2 2 0 0)".GetGeom();
            tolerance = 0.5;
            result = LRS.Geometry.IsSpatiallyConnected(geom1, geom2, tolerance);
            SqlAssert.IsTrue(result);

            geom1 = "LINESTRING(0 0 0 0, 1 1 0 0, 3 4 0 0, 5.5 5 0 0)".GetGeom();
            geom2 = "LINESTRING(2 2 9 0, 6 4.9 0 0)".GetGeom();
            tolerance = 0.5;
            result = LRS.Geometry.IsSpatiallyConnected(geom1, geom2, tolerance);
            SqlAssert.IsFalse(result);
        }

        [TestMethod]
        public void LocatePointAlongGeometryTest()
        {
            var geom = "LINESTRING (0 0 0 0, 10 0 0 10)".GetGeom();
            Logger.Log("Input Geom : {0}", geom.ToString());
            var returnPoint = "POINT (5 0 NULL 5)".GetGeom();
            var distance = 5;

            var locatedPoint = LRS.Geometry.LocatePointAlongGeometry(geom, distance);
            Logger.Log("Located point : {0} at distance of {1} Measure", locatedPoint.ToString(), distance);

            SqlAssert.IsTrue(locatedPoint.STEquals(returnPoint));
        }

        [TestMethod]
        public void MergeGeometrySegmentTest()
        {
            var geom1 = "MULTILINESTRING((100 100, 200 200), (3 4, 7 8, 10 10))".GetGeom();
            var geom2 = "MULTILINESTRING((11 2, 12 4, 15 5), (5 4, 6 8, 9 11))".GetGeom();
            Logger.Log("Input Geom 1: {0}", geom1.ToString());
            Logger.Log("Input Geom 2: {0}", geom2.ToString());
            try
            {
                LRS.Geometry.MergeGeometrySegments(geom1, geom2);
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual(e.Message, ErrorMessage.LineStringCompatible);
                TestContext.WriteLine(ErrorMessage.LineStringCompatible);
            }

            geom1 = "LINESTRING(10 1 NULL 10, 25 1 NULL 25)".GetGeom();
            geom2 = "MULTILINESTRING((11 2, 12 4, 15 5), (5 4, 6 8, 9 11))".GetGeom();
            Logger.LogLine("Input Geom 1: {0}", geom1.ToString());
            Logger.Log("Input Geom 2: {0}", geom2.ToString());
            try
            {
                LRS.Geometry.MergeGeometrySegments(geom1, geom2);
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual(e.Message, ErrorMessage.LineStringCompatible);
                TestContext.WriteLine(ErrorMessage.LineStringCompatible);
            }

            geom1 = "MULTILINESTRING((11 2, 12 4, 15 5), (5 4, 6 8, 9 11))".GetGeom();
            geom2 = "LINESTRING(10 1 NULL 10, 25 1 NULL 25)".GetGeom();
            Logger.LogLine("Input Geom 1: {0}", geom1.ToString());
            Logger.Log("Input Geom 2: {0}", geom2.ToString());
            try
            {
                LRS.Geometry.MergeGeometrySegments(geom1, geom2);
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual(e.Message, ErrorMessage.LineStringCompatible);
                TestContext.WriteLine(ErrorMessage.LineStringCompatible);
            }

            // offset between geoms : 0
            geom1 = "LINESTRING (10 1 NULL 10, 25 1 NULL 25 )".GetGeom();
            geom2 = "LINESTRING (25 1 NULL 25, 40 1 NULL 40 )".GetGeom();
            Logger.LogLine("Input Geom 1: {0}", geom1.ToString());
            Logger.Log("Input Geom 2: {0}", geom2.ToString());

            var mergedGeom = "LINESTRING (10 1 NULL 10, 25 1 NULL 25, 40 1 NULL 40)".GetGeom();
            var retGeom = LRS.Geometry.MergeGeometrySegments(geom1, geom2);
            Logger.Log("Merged Geom: {0}", retGeom.ToString());
            SqlAssert.IsTrue(retGeom.STEquals(mergedGeom));

            // offset between geoms: 5
            geom1 = "LINESTRING (10 1 NULL 10, 25 1 NULL 25 )".GetGeom();
            geom2 = "LINESTRING (30 1 NULL 30, 40 1 NULL 40 )".GetGeom();
            Logger.LogLine("Input Geom 1: {0}", geom1.ToString());
            Logger.Log("Input Geom 2: {0}", geom2.ToString());

            mergedGeom = "LINESTRING (10 1 NULL 10, 25 1 NULL 25, 40 1 NULL 35)".GetGeom();
            retGeom = LRS.Geometry.MergeGeometrySegments(geom1, geom2);
            Logger.Log("Expected Geom: {0}", mergedGeom.ToString());
            Logger.Log("Merged   Geom: {0}", retGeom.ToString());
            SqlAssert.IsTrue(retGeom.STEquals(mergedGeom));
            SqlAssert.IsTrue(retGeom.GetEndPointMeasure().Equals(mergedGeom.GetEndPointMeasure()));

        }

        [TestMethod]
        public void PopulateGeometryMeasuresTest()
        {
            // 4 point line string
            var geom = "LINESTRING (10 1 10 100, 15 1 10 NULL, 20 1 10 NULL, 25 1 10 250 )".GetGeom();
            Logger.Log("Input Geom : {0}", geom.ToString());

            Logger.Log("Populating Geom with null Start and End Measure");
            var populatedGeometry = LRS.Geometry.PopulateGeometryMeasures(geom, null, null);
            Logger.Log("Populated Geom : {0}", populatedGeometry.ToString());

            // if the start, end measure would be null, then this function populates the existing 'M' (measure) value
            SqlAssert.AreEqual(populatedGeometry.GetStartPointMeasure(), 100.0F);
            SqlAssert.AreEqual(populatedGeometry.GetEndPointMeasure(), 250.0F);

            double startMeasure = 10;
            double endMeasure = 40;
            // if the start, end measure would be non null, then this function overrides the 'M' value that has been passed
            Logger.Log("Populating Geom with Start Measure: {0} and End Measure: {1}", startMeasure, endMeasure);
            populatedGeometry = LRS.Geometry.PopulateGeometryMeasures(geom, startMeasure, endMeasure);
            Logger.Log("Populated Geom : {0}", populatedGeometry.ToString());
            Assert.AreEqual(populatedGeometry.GetStartPointMeasure(), startMeasure);
            SqlAssert.AreEqual(populatedGeometry.STPointN(2).M, 20.0F);
            SqlAssert.AreEqual(populatedGeometry.STPointN(3).M, 30.0F);
            Assert.AreEqual(populatedGeometry.GetEndPointMeasure(), endMeasure);
        }

        [TestMethod]
        public void ReverseLinearGeometryTest()
        {
            var geom = "LINESTRING (1 1 0 0, 5 5 0 0)".GetGeom();
            Logger.Log("Input Geom : {0}", geom.ToString());

            var endPoint = "POINT (5 5 0 0)".GetGeom();
            var reversedLineSegment = LRS.Geometry.ReverseLinearGeometry(geom);
            Logger.Log("Reversed Line string : {0}", reversedLineSegment.ToString());
            SqlAssert.IsTrue(reversedLineSegment.STStartPoint().STEquals(endPoint));
        }

        [TestMethod]
        public void SplitGeometrySegmentTest()
        {
            SqlGeometry geomSegment1, geomSegment2;

            var geom = "MULTILINESTRING((100 100, 200 200), (3 4, 7 8, 10 10))".GetGeom();
            Logger.Log("Input Geom : {0}", geom.ToString());
            try
            {
                LRS.Geometry.SplitGeometrySegment(geom, 15, out geomSegment1, out geomSegment2);
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual(e.Message, ErrorMessage.LineStringCompatible);
                TestContext.WriteLine(ErrorMessage.LineStringCompatible);
            }

            // line string with null z value
            geom = "LINESTRING (10 1 NULL 10, 25 1 NULL 25 )".GetGeom();
            Logger.Log("Input Geom : {0}", geom.ToString());
            var splitedGeom1 = "LINESTRING (10 1 NULL 10, 15 1 NULL 15 )".GetGeom();
            var splitedGeom2 = "LINESTRING (15 1 NULL 15, 25 1 NULL 25 )".GetGeom();

            var distance = 5;
            Logger.Log("Split input geom at a distance of {0} Measure", distance);
            try
            {
                LRS.Geometry.SplitGeometrySegment(geom, distance, out geomSegment1, out geomSegment2);
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual(e.Message, ErrorMessage.MeasureRange);
                Logger.Log(ErrorMessage.MeasureRange);
            }

            distance = 27;
            Logger.Log("Split input geom at a distance of {0} Measure", distance);
            try
            {
                LRS.Geometry.SplitGeometrySegment(geom, distance, out geomSegment1, out geomSegment2);
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual(e.Message, "Measure provided is greater than maximum measure of LineString.");
                Logger.Log("Measure provided is greater than maximum measure of LineString.");
            }

            distance = 15;
            Logger.Log("Split input geom at a distance of {0} Measure", distance);

            LRS.Geometry.SplitGeometrySegment(geom, distance, out geomSegment1, out geomSegment2);
            Logger.Log("Splitted Geom 1 : {0}", geomSegment1);
            Logger.Log("Splitted Geom 2 : {0}", geomSegment2);

            SqlAssert.IsTrue(geomSegment1.STIsValid());
            SqlAssert.IsTrue(geomSegment2.STIsValid());

            SqlAssert.IsTrue(geomSegment1.STEquals(splitedGeom1));
            SqlAssert.IsTrue(geomSegment2.STEquals(splitedGeom2));
        }
    }
}
