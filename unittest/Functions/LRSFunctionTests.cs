//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

using System;
using Microsoft.SqlServer.Types;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SQLSpatialTools.Functions.LRS;
using SQLSpatialTools.UnitTests.Extension;
using SQLSpatialTools.Utility;

namespace SQLSpatialTools.UnitTests.Functions
{
    [TestClass]
    public class LRSFunctionTests : BaseUnitTest
    {
        [TestMethod]
        public void ClipMultiLineTest()
        {
            double startMeasure = 1;
            double endMeasure = -5;
            var geom = "MULTILINESTRING((1 1 1, 2 2 2, 3 3 3), (4 4 4, 5.125 5.125 5.125, 6 6 6), (10 10 10, 11.25 11.25 11.25, 12 12 12))".GetGeom();
            var expected = "POINT (1 1 1)".GetGeom();
            var result = Geometry.ClipGeometrySegment(geom, startMeasure, endMeasure);
            LogClipGeomSegments(startMeasure, endMeasure, geom, result, expected);

            startMeasure = 2;
            endMeasure = 10;
            geom = "MULTILINESTRING((1 1 1, 2 2 2, 3 3 3), (4 4 4, 5.125 5.125 5.125, 6 6 6), (10 10 10, 11.25 11.25 11.25, 12 12 12))".GetGeom();
            expected = "MULTILINESTRING ((2.0 2.0 2.0, 3.0 3.0 3.0), (4.0 4.0 4.0, 5.125 5.125 5.125, 6.0 6.0 6.0))".GetGeom();
            result = Geometry.ClipGeometrySegment(geom, startMeasure, endMeasure);
            LogClipGeomSegments(startMeasure, endMeasure, geom, result, expected);

            startMeasure = 2;
            endMeasure = 11.25;
            geom = "MULTILINESTRING((1 1 1, 2 2 2, 3 3 3), (4 4 4, 5.125 5.125 5.125, 6 6 6), (10 10 10, 11.25 11.25 11.25, 12 12 12))".GetGeom();
            expected = "MULTILINESTRING ((2 2 NULL 2, 2 2 NULL 2, 3 3 NULL 3), (4 4 NULL 4, 5.125 5.125 NULL 5.125, 6 6 NULL 6), (10 10 NULL 10, 11.25 11.25 NULL 11.25))".GetGeom();
            result = Geometry.ClipGeometrySegment(geom, startMeasure, endMeasure);
            LogClipGeomSegments(startMeasure, endMeasure, geom, result, expected);

            startMeasure = 2.6;
            endMeasure = 11;
            geom = "MULTILINESTRING((1 1 1, 2 2 2, 3 3 3), (4 4 4, 5.125 5.125 5.125, 6 6 6), (10 10 10, 11.25 11.25 11.25, 12 12 12))".GetGeom();
            expected = "MULTILINESTRING ((4 4 NULL 4, 5.125 5.125 NULL 5.125, 6 6 NULL 6), (10 10 NULL 10, 11.25 11.25 NULL 11.25))".GetGeom();
            result = Geometry.ClipGeometrySegment(geom, startMeasure, endMeasure);
            LogClipGeomSegments(startMeasure, endMeasure, geom, result, expected);

            startMeasure = 2.6;
            endMeasure = 10;
            geom = "MULTILINESTRING((1 1 1, 2 2 2, 3 3 3), (4 4 4, 5.125 5.125 5.125, 6 6 6), (10 10 10, 11.25 11.25 11.25, 12 12 12))".GetGeom();
            expected = "LINESTRING (4 4 NULL 4, 5.125 5.125 NULL 5.125, 6 6 NULL 6)".GetGeom();
            result = Geometry.ClipGeometrySegment(geom, startMeasure, endMeasure);
            LogClipGeomSegments(startMeasure, endMeasure, geom, result, expected);
        }

        [TestMethod]
        public void ClipGeometrySegmentExtensionTest()
        {
            double startMeasure = 0;
            double endMeasure = 500;
            var geom = "LINESTRING(200000 100000 0, 200500 100000 500)".GetGeom();
            var expected = "LINESTRING (200000 100000 NULL 0, 200500 100000 NULL 500)".GetGeom();
            var result = Geometry.ClipGeometrySegment(geom, startMeasure, endMeasure);
            LogClipGeomSegments(startMeasure, endMeasure, geom, result, expected);

            startMeasure = 10;
            endMeasure = 5;
            geom = "LINESTRING(5 10 10, 20 5 30.628, 35 10 61.257, 55 10 100)".GetGeom();
            result = Geometry.ClipGeometrySegment(geom, startMeasure, endMeasure);
            expected = "POINT (5 10 NULL 10)".GetGeom();
            LogClipGeomSegments(startMeasure, endMeasure, geom, result, expected);

            startMeasure = 5;
            endMeasure = 110;
            result = Geometry.ClipGeometrySegment(geom, startMeasure, endMeasure);
            expected = "LINESTRING (5 10 NULL 10, 20 5 NULL 30.628, 35 10 NULL 61.257, 55 10 NULL 100)".GetGeom();
            LogClipGeomSegments(startMeasure, endMeasure, geom, result, expected);

            // both measures less than start measure of input geom
            startMeasure = 5;
            endMeasure = 9;
            result = Geometry.ClipGeometrySegment(geom, startMeasure, endMeasure);
            LogClipGeomSegments(startMeasure, endMeasure, geom, result, null);

            // both measures greater than end measure of input geom
            startMeasure = 110;
            endMeasure = 120;
            result = Geometry.ClipGeometrySegment(geom, startMeasure, endMeasure);
            LogClipGeomSegments(startMeasure, endMeasure, geom, result, null);

            startMeasure = 10;
            endMeasure = 110;
            result = Geometry.ClipGeometrySegment(geom, startMeasure, endMeasure);
            expected = "LINESTRING (5 10 NULL 10, 20 5 NULL 30.628, 35 10 NULL 61.257, 55 10 NULL 100)".GetGeom();
            LogClipGeomSegments(startMeasure, endMeasure, geom, result, expected);

            startMeasure = 15;
            endMeasure = 90;
            result = Geometry.ClipGeometrySegment(geom, startMeasure, endMeasure);
            expected = "LINESTRING (8.63583478766725 8.7880550707775846 NULL 15, 20 5 NULL 30.628, 35 10 NULL 61.257, 49.837777146839429 10 NULL 90)".GetGeom();
            LogClipGeomSegments(startMeasure, endMeasure, geom, result, expected);

            startMeasure = 100;
            endMeasure = 200;
            result = Geometry.ClipGeometrySegment(geom, startMeasure, endMeasure);
            expected = "POINT (55 10 NULL 100)".GetGeom();
            LogClipGeomSegments(startMeasure, endMeasure, geom, result, expected);

            startMeasure = 61.257F;
            endMeasure = 200;
            result = Geometry.ClipGeometrySegment(geom, startMeasure, endMeasure);
            expected = "LINESTRING (35 10 NULL 61.257, 55 10 NULL 100)".GetGeom();
            LogClipGeomSegments(startMeasure, endMeasure, geom, result, expected);

            startMeasure = 50;
            endMeasure = 100;
            result = Geometry.ClipGeometrySegment(geom, startMeasure, endMeasure);
            expected = "LINESTRING (29.487087400829282 8.1623624669430939 NULL 50, 35 10 NULL 61.257, 55 10 NULL 100)".GetGeom();
            LogClipGeomSegments(startMeasure, endMeasure, geom, result, expected);
        }

        [TestMethod]
        public void ClipGeometrySegmentTest()
        {
            var geom = "MULTILINESTRING((100 100, 200 200), (3 4, 7 8, 10 10))".GetGeom();
            Logger.Log("Input Geom : {0}", geom.ToString());
            try
            {
                Geometry.ClipGeometrySegment(geom, 15, 20);
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual(e.Message, ErrorMessage.TwoDimensionalCoordinates);
                TestContext.WriteLine(ErrorMessage.TwoDimensionalCoordinates);
            }

            // line string with null z value
            geom = "LINESTRING (10 1 NULL 10, 25 1 NULL 25 )".GetGeom();
            Logger.LogLine("Input Geom : {0}", geom.ToString());

            // when measures are out of range; clip segment is null
            var startMeasure = 5;
            var endMeasure = 7;
            Logger.Log("Clip input geom with a Start Measure: {0} and End Measure: {1}", startMeasure, endMeasure);
            var clippedGeom = Geometry.ClipGeometrySegment(geom, startMeasure, endMeasure);
            Assert.IsNull(clippedGeom);

            // measure in between range.
            startMeasure = 15;
            endMeasure = 27;
            Logger.LogLine("Clip input geom with a Start Measure: {0} and End Measure: {1}", startMeasure, endMeasure);
            var retGeom = "LINESTRING(15 1 NULL 15, 25 1 NULL 25)".GetGeom();
            clippedGeom =  Geometry.ClipGeometrySegment(geom, startMeasure, endMeasure);
            SqlAssert.IsTrue(retGeom.STIsValid());
            SqlAssert.IsTrue(clippedGeom.STEquals(retGeom));

            // From start to 5 point measure
            startMeasure = 10;
            endMeasure = 15;
            Logger.LogLine("Clip input geom with a Start Measure: {0} and End Measure: {1}", startMeasure, endMeasure);

            retGeom = "LINESTRING (10 1 NULL 10, 15 1 NULL 15 )".GetGeom();
            clippedGeom = Geometry.ClipGeometrySegment(geom, startMeasure, endMeasure);
            Logger.Log("Clipped Geom: {0}", clippedGeom.ToString());
            SqlAssert.IsTrue(retGeom.STIsValid());
            SqlAssert.IsTrue(clippedGeom.STEquals(retGeom));

            // From 15 to 20
            startMeasure = 15;
            endMeasure = 20;
            Logger.LogLine("Clip input geom with a Start Measure: {0} and End Measure: {1}", startMeasure, endMeasure);

            retGeom = "LINESTRING (15 1 NULL 15, 20 1 NULL 20 )".GetGeom();
            clippedGeom = Geometry.ClipGeometrySegment(geom, startMeasure, endMeasure);
            Logger.Log("Clipped Geom: {0}", clippedGeom.ToString());
            SqlAssert.IsTrue(retGeom.STIsValid());
            SqlAssert.IsTrue(clippedGeom.STEquals(retGeom));

            // From 20 to 25
            startMeasure = 20;
            endMeasure = 25;
            Logger.LogLine("Clip input geom with a Start Measure: {0} and End Measure: {1}", startMeasure, endMeasure);

            retGeom = "LINESTRING (20 1 NULL 20, 25 1 NULL 25 )".GetGeom();
            clippedGeom = Geometry.ClipGeometrySegment(geom, startMeasure, endMeasure);
            Logger.Log("Clipped Geom: {0}", clippedGeom.ToString());
            SqlAssert.IsTrue(retGeom.STIsValid());
            SqlAssert.IsTrue(clippedGeom.STEquals(retGeom));

            startMeasure = 5;
            endMeasure = 10;
            geom = "LINESTRING(2 2 0, 2 4 2, 8 4 8, 12 4 12, 12 10 0, 8 10 22, 5 14  27)".GetGeom();
            retGeom = "LINESTRING (5 4 NULL 5, 8 4 NULL 8, 10 4 NULL 10)".GetGeom();
            clippedGeom = Geometry.ClipGeometrySegment(geom, startMeasure, endMeasure);
            Logger.Log("Clipped Geom: {0}", clippedGeom.ToString());
            SqlAssert.IsTrue(retGeom.STIsValid());
            SqlAssert.IsTrue(clippedGeom.STEquals(retGeom));
        }

        [TestMethod]
        public void GetEndMeasureTest()
        {
            var endMeasureValue = 14.0F;
            var geom = $"POINT(5.5 5 0 {endMeasureValue})".GetGeom();
            var endMeasure = Geometry.GetEndMeasure(geom);
            SqlAssert.AreEqual(endMeasure, endMeasureValue);

            endMeasureValue = 10.0F;
            geom = $"LINESTRING(0 0 0 0, 1 1 0 0, 3 4 0 0, 5.5 5 1000 {endMeasureValue})".GetGeom();
            endMeasure = Geometry.GetEndMeasure(geom);
            SqlAssert.AreEqual(endMeasure, endMeasureValue);

            endMeasureValue = 100.000999450684F;
            geom = $"MULTILINESTRING((0 0 0 0, 1 1 0 0), (3 2 0 null, 5 5 2 {endMeasureValue}))".GetGeom();
            endMeasure = Geometry.GetEndMeasure(geom);
            SqlAssert.AreEqual(endMeasure, endMeasureValue);

            try
            {
                geom = ("POLYGON((0 0 0 0, 1 1 0 0, 3 4 0 0, 5.5 5 1000, 0 0 0 0))").GetGeom();
                Geometry.GetEndMeasure(geom);
                Assert.Fail("Method GetGeomSegmentEndMeasureTest should not accept polygon geometric structure");
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual(e.Message, ErrorMessage.LRSCompatible);
                TestContext.WriteLine(ErrorMessage.LRSCompatible);
            }

            endMeasureValue = 10.0F;
            geom = "GEOMETRYCOLLECTION(LINESTRING(1 1 4, 3 5 6), MULTILINESTRING((-1 -1 0, 1 -5 5, -5 5 10), (-5 -1 5, -1 -1 10)), POINT(5 6 10))".GetGeom();
            endMeasure = Geometry.GetEndMeasure(geom);
            SqlAssert.AreEqual(endMeasure, endMeasureValue);
        }

        [TestMethod]
        public void GetStartMeasureTest()
        {
            var startMeasureValue = 14.0F;
            var geom = $"POINT(5.5 5 1000 {startMeasureValue})".GetGeom();
            var startMeasure = Geometry.GetStartMeasure(geom);
            SqlAssert.AreEqual(startMeasure, startMeasureValue);

            startMeasureValue = 10.0F;
            geom = $"LINESTRING(0 0 0 {startMeasureValue}, 1 1 0 0, 3 4 0 0, 5.5 5 1000 0)".GetGeom();
            startMeasure = Geometry.GetStartMeasure(geom);
            SqlAssert.AreEqual(startMeasure, startMeasureValue);

            startMeasureValue = 100.000999450684F;
            geom = string.Format("MULTILINESTRING((0 0 0 {0}, 1 1 0 0), (3 2 0 null, 5 5 2 {0}))", startMeasureValue).GetGeom();
            startMeasure = Geometry.GetStartMeasure(geom);
            SqlAssert.AreEqual(startMeasure, startMeasureValue);

            try
            {
                geom = ("POLYGON((0 0 0 0, 1 1 0 0, 3 4 0 0, 5.5 5 1000, 0 0 0 0))").GetGeom();
                Geometry.GetStartMeasure(geom);
                Assert.Fail("Method GetGeomSegmentStartMeasure should not accept polygon geometric structure");
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual(e.Message, ErrorMessage.LRSCompatible);
                TestContext.WriteLine(ErrorMessage.LRSCompatible);
            }

            startMeasureValue = 4.0F;
            geom = "GEOMETRYCOLLECTION(LINESTRING(1 1 4, 3 5 6), MULTILINESTRING((-1 -1 0, 1 -5 5, -5 5 10), (-5 -1 5, -1 -1 10)), POINT(5 6 10))".GetGeom();
            startMeasure = Geometry.GetStartMeasure(geom);
            SqlAssert.AreEqual(startMeasure, startMeasureValue);
        }

        [TestMethod]
        public void InterpolateBetweenGeomTest()
        {
            var geom1 = "POINT(0 0 0 0)".GetGeom();
            var geom2 = "POINT(10 0 0 10)".GetGeom();
            var returnPoint = "POINT (5 0 NULL 5)".GetGeom();
            const int distance = 5;
            Logger.LogLine("Input Point 1:{0} Point 2:{1}", geom1, geom2);
            Logger.Log("Interpolating at a distance of {0}", geom1, geom2, distance);
            Logger.LogLine("Expected Point: {0}", returnPoint.ToString());
            var sqlGeometry = Geometry.InterpolateBetweenGeom(geom1, geom2, distance);
            Logger.Log("Obtained Point: {0}", sqlGeometry.ToString());
            SqlAssert.IsTrue(sqlGeometry.STEquals(returnPoint));
        }

        [TestMethod]
        public void IsConnectedTest()
        {
            // test cases without considering tolerance values
            var geom1 = "LINESTRING(0 0 0 0, 1 1 0 0, 3 4 0 0, 5.5 5 0 0)".GetGeom();
            var geom2 = "LINESTRING(0 0 0 0, 2 2 0 0)".GetGeom();
            var tolerance = Constants.Tolerance;
            var result = Geometry.IsConnected(geom1, geom2, tolerance);
            SqlAssert.IsTrue(result);

            // Different SRIDs Failure Test
            try
            {
                geom2 = "LINESTRING(0 0 0 0, 2 2 0 0)".GetGeom(10);
                Geometry.IsConnected(geom1, geom2, tolerance);
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual(e.Message, ErrorMessage.SRIDCompatible);
                TestContext.WriteLine(ErrorMessage.SRIDCompatible);
            }

            geom1 = "LINESTRING(0 0 0 0, 1 1 0 0, 3 4 0 0, 5.5 5 0 0)".GetGeom();
            geom2 = "LINESTRING(2 2 0 0, 0 0 0 0)".GetGeom();
            tolerance = 0;
            result = Geometry.IsConnected(geom1, geom2, tolerance);
            SqlAssert.IsTrue(result);

            geom1 = "LINESTRING(0 0 0 0, 1 1 0 0, 3 4 0 0, 5.5 5 0 0)".GetGeom();
            geom2 = "LINESTRING(5.5 5 0 0, 2 2 0 0)".GetGeom();
            tolerance = 0;
            result = Geometry.IsConnected(geom1, geom2, tolerance);
            SqlAssert.IsTrue(result);

            geom1 = "LINESTRING(0 0 0 0, 1 1 0 0, 3 4 0 0, 5.5 5 0 0)".GetGeom();
            geom2 = "LINESTRING(2 2 9 0, 5.5 5 0 0)".GetGeom();
            tolerance = 0;
            result = Geometry.IsConnected(geom1, geom2, tolerance);
            SqlAssert.IsTrue(result);

            // test cases with tolerance values considered
            // here point difference is not considered; rather x2-x1 and y2-y1 is considered for tolerance
            geom1 = "LINESTRING(0 0 0 0, 1 1 0 0, 3 4 0 0, 5.5 5 0 0)".GetGeom();
            geom2 = "LINESTRING(0.5 0.5 0 0, 2 2 0 0)".GetGeom();
            tolerance = 0.5;
            result = Geometry.IsConnected(geom1, geom2, tolerance);
            SqlAssert.IsTrue(result);

            geom1 = "LINESTRING(0 0 0 0, 1 1 0 0, 3 4 0 0, 5.5 5 0 0)".GetGeom();
            geom2 = "LINESTRING(0.5 0 0 0, 2 2 0 0)".GetGeom();
            tolerance = 0.5;
            result = Geometry.IsConnected(geom1, geom2, tolerance);
            SqlAssert.IsTrue(result);

            geom1 = "LINESTRING(0 0 0 0, 1 1 0 0, 3 4 0 0, 5.5 5 0 0)".GetGeom();
            geom2 = "LINESTRING(1.5 1.5 0 0, 2 2 0 0)".GetGeom();
            tolerance = 0.5;
            result = Geometry.IsConnected(geom1, geom2, tolerance);
            SqlAssert.IsFalse(result);

            geom1 = "LINESTRING(0 0 0 0, 1 1 0 0, 3 4 0 0, 5.5 5 0 0)".GetGeom();
            geom2 = "LINESTRING(0 0.5 0 0, 2 2 0 0)".GetGeom();
            tolerance = 0.5;
            result = Geometry.IsConnected(geom1, geom2, tolerance);
            SqlAssert.IsTrue(result);

            geom1 = "LINESTRING(0 0 0 0, 1 1 0 0, 3 4 0 0, 5.5 5 0 0)".GetGeom();
            geom2 = "LINESTRING(2 2 0 0, 0.1 0.6 0 0)".GetGeom();
            tolerance = 0.5;
            result = Geometry.IsConnected(geom1, geom2, tolerance);
            SqlAssert.IsFalse(result);

            geom1 = "LINESTRING(0 0 0 0, 1 1 0 0, 3 4 0 0, 5.5 5 0 0)".GetGeom();
            geom2 = "LINESTRING(2 2 0 0, 0.6 0.1 0 0)".GetGeom();
            tolerance = 1;
            result = Geometry.IsConnected(geom1, geom2, tolerance);
            SqlAssert.IsTrue(result);

            geom1 = "LINESTRING(0 0 0 0, 1 1 0 0, 3 4 0 0, 5.5 5 0 0)".GetGeom();
            geom2 = "LINESTRING(5 5 0 0, 2 2 0 0)".GetGeom();
            tolerance = 0.5;
            result = Geometry.IsConnected(geom1, geom2, tolerance);
            SqlAssert.IsTrue(result);

            geom1 = "LINESTRING(0 0 0 0, 1 1 0 0, 3 4 0 0, 5.5 5 0 0)".GetGeom();
            geom2 = "LINESTRING(2 2 9 0, 6 4.9 0 0)".GetGeom();
            tolerance = 0.5;
            result = Geometry.IsConnected(geom1, geom2, tolerance);
            SqlAssert.IsTrue(result);
        }

        [TestMethod]
        public void IsValidPoint()
        {
            var geom = "LINESTRING(0 0 0 0, 1 1 0 0, 3 4 0 0, 5.5 5 0 0)".GetGeom();
            SqlAssert.IsFalse(Geometry.IsValidPoint(geom));

            geom = "POINT(0 0 NULL 0)".GetGeom();
            SqlAssert.IsTrue(Geometry.IsValidPoint(geom));

            geom = "POINT(0 0 0 0)".GetGeom();
            SqlAssert.IsTrue(Geometry.IsValidPoint(geom));

            geom = "POINT(0 0 1)".GetGeom();
            SqlAssert.IsTrue(Geometry.IsValidPoint(geom));

            geom = "POINT(0 0 0)".GetGeom();
            SqlAssert.IsTrue(Geometry.IsValidPoint(geom));

            geom = "POINT(0 0)".GetGeom();
            SqlAssert.IsFalse(Geometry.IsValidPoint(geom));
        }

        [TestMethod]
        public void LocatePointAlongGeomTest()
        {
            var geom = "LINESTRING (0 0 0 0, 10 0 0 10)".GetGeom();
            Logger.Log("Input Geom : {0}", geom.ToString());
            var returnPoint = "POINT (5 0 NULL 5)".GetGeom();
            var distance = 5;

            var locatedPoint = Geometry.LocatePointAlongGeom(geom, distance);
            Logger.Log("Located point : {0} at distance of {1} Measure", locatedPoint.ToString(), distance);

            SqlAssert.IsTrue(locatedPoint.STEquals(returnPoint));
        }

        [TestMethod]
        public void MergeGeometrySegmentsTest()
        {
            var geom1 = "MULTILINESTRING((100 100 0, 200 200 100), (3 4 0, 7 8 4, 10 10 6))".GetGeom();
            var geom2 = "MULTILINESTRING((11 2 0, 12 4 2, 15 5 4), (5 4 4, 6 8 6, 9 11 8))".GetGeom();
            Logger.Log("Input Geom 1: {0}", geom1.ToString());
            Logger.Log("Input Geom 2: {0}", geom2.ToString());
            try
            {
                Geometry.MergeGeometrySegments(geom1, geom2);
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual(e.Message, ErrorMessage.LineStringCompatible);
                TestContext.WriteLine(ErrorMessage.LineStringCompatible);
            }

            geom1 = "LINESTRING(10 1 NULL 10, 25 1 NULL 25)".GetGeom();
            geom2 = "MULTILINESTRING((11 2 0, 12 4 2, 15 5 4), (5 4 4, 6 8 6, 9 11 8))".GetGeom();
            Logger.LogLine("Input Geom 1: {0}", geom1.ToString());
            Logger.Log("Input Geom 2: {0}", geom2.ToString());
            try
            {
                Geometry.MergeGeometrySegments(geom1, geom2);
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual(e.Message, ErrorMessage.LineStringCompatible);
                TestContext.WriteLine(ErrorMessage.LineStringCompatible);
            }

            geom1 = "MULTILINESTRING((11 2 0, 12 4 2, 15 5 4), (5 4 4, 6 8 6, 9 11 8))".GetGeom();
            geom2 = "LINESTRING(10 1 NULL 10, 25 1 NULL 25)".GetGeom();
            Logger.LogLine("Input Geom 1: {0}", geom1.ToString());
            Logger.Log("Input Geom 2: {0}", geom2.ToString());
            try
            {
                Geometry.MergeGeometrySegments(geom1, geom2);
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual(e.Message, ErrorMessage.LineStringCompatible);
                TestContext.WriteLine(ErrorMessage.LineStringCompatible);
            }

            // offset between geometries : 0
            geom1 = "LINESTRING (10 1 NULL 10, 25 1 NULL 25 )".GetGeom();
            geom2 = "LINESTRING (25 1 NULL 25, 40 1 NULL 40 )".GetGeom();
            Logger.LogLine("Input Geom 1: {0}", geom1.ToString());
            Logger.Log("Input Geom 2: {0}", geom2.ToString());

            var mergedGeom = "LINESTRING (10 1 NULL 10, 25 1 NULL 25, 40 1 NULL 40)".GetGeom();
            var retGeom = Geometry.MergeGeometrySegments(geom1, geom2);
            Logger.Log("Merged Geom: {0}", retGeom.ToString());
            SqlAssert.IsTrue(retGeom.STEquals(mergedGeom));

            // offset between geometries: 5
            geom1 = "LINESTRING (10 1 NULL 10, 25 1 NULL 25 )".GetGeom();
            geom2 = "LINESTRING (30 1 NULL 30, 40 1 NULL 40 )".GetGeom();
            Logger.LogLine("Input Geom 1: {0}", geom1.ToString());
            Logger.Log("Input Geom 2: {0}", geom2.ToString());

            mergedGeom = "MULTILINESTRING ((10 1 NULL 10, 25 1 NULL 25), (30 1 NULL 30, 40 1 NULL 40))".GetGeom();
            retGeom = Geometry.MergeGeometrySegments(geom1, geom2);
            Logger.Log("Expected Geom: {0}", mergedGeom.ToString());
            Logger.Log("Merged   Geom: {0}", retGeom.ToString());
            SqlAssert.IsTrue(retGeom.STEquals(mergedGeom));
            SqlAssert.IsTrue(retGeom.GetEndPointMeasure().Equals(mergedGeom.GetEndPointMeasure()));
        }

        [TestMethod]
        public void OffsetGeometrySegmentTest()
        {
            var geom = "LINESTRING(5 10 0, 20 5 30.628, 35 10 61.257, 55 10 100)".GetGeom();
            var offsetGeom = "LINESTRING (5.632455532033676 11.897366596101028 NULL 0, 20 7.10818510677892 NULL 30.628, 34.367544467966326 11.897366596101028 NULL 61.257)".GetGeom();
            var startMeasure = 0;
            var endMeasure = 61.5;
            var offset = 2;
            var tolerance = 0.5;
            Logger.LogLine("Input Line : {0}", geom.ToString());
            var result = Geometry.OffsetGeometrySegment(geom, startMeasure, endMeasure, offset, tolerance);
            Logger.Log("Expected Line : {0}", offsetGeom.ToString());
            Logger.Log("Offset Line : {0}", result.ToString());
            SqlAssert.IsTrue(offsetGeom.STEquals(result));

            geom = "LINESTRING (55 10 NULL 100, 35 10 NULL 61.257, 20 5 NULL 30.628, 5 10 NULL 0)".GetGeom();
            offsetGeom = "LINESTRING (35.632455532033674 8.102633403898972 NULL 61.257, 20 2.8918148932210808 NULL 30.628, 4.3675444679663249 8.102633403898972 NULL 0)".GetGeom();
            startMeasure = 0;
            endMeasure = 61.5;
            offset = 2;
            tolerance = 0.5;
            Logger.LogLine("Input Line : {0}", geom.ToString());
            result = Geometry.OffsetGeometrySegment(geom, startMeasure, endMeasure, offset, tolerance);
            Logger.Log("Expected Line : {0}", offsetGeom.ToString());
            Logger.Log("Offset Line : {0}", result.ToString());
            SqlAssert.IsTrue(offsetGeom.STEquals(result));
        }

        [TestMethod]
        public void OffsetGeometryWithPopulateMeasureTest()
        {
            var geom = "LINESTRING(5 10 10, 20 5 30.628, 35 10 61.257, 55 10 100)".GetGeom();
            var offsetGeom = "LINESTRING (19.448627725169224 7.2919758650558464 NULL 29, 20 7.10818510677892 NULL 30.195004804845414, 33.751951402570789 11.692168907635848 NULL 60)".GetGeom();
            var startMeasure = 60;
            var endMeasure = 29;
            var offset = 2;
            var tolerance = 0.5;
            Logger.LogLine("Input Line : {0}", geom.ToString());
            var result = Geometry.OffsetGeometrySegment(geom, startMeasure, endMeasure, offset, tolerance);
            Logger.Log("Expected Line : {0}", offsetGeom.ToString());
            Logger.Log("Offset Line : {0}", result.ToString());
            SqlAssert.IsTrue(offsetGeom.STEquals(result));
        }

        [TestMethod]
        public void OffsetGeometryWithBendLinesTest()
        {
            var geom = "LINESTRING (1 1 0, 3 3 3, 5 5 5, 5 8 8) ".GetGeom();
            var offsetGeom = "LINESTRING (7.0867894471005748 11.412588624412061 NULL 12, 20 7.10818510677892 NULL 36.2224547311131, 34.675444679663244 12 NULL 61.257, 54.483777714683939 12 NULL 99)".GetGeom();
            var startMeasure = 0;
            var endMeasure = 8;
            var offset = -2;
            var tolerance = 0.5;
            Logger.LogLine("Input Line : {0}", geom.ToString());
            var result = Geometry.OffsetGeometrySegment(geom, startMeasure, endMeasure, offset, tolerance);
            Logger.Log("Expected Line : {0}", offsetGeom.ToString());
            Logger.Log("Offset Line : {0}", result.ToString());
        }

        [TestMethod]
        public void OffsetGeometrySegmentCollinearPointsTest()
        {
            var geom = "LINESTRING (218000 104375 0, 218000 104875 500, 218000 105000 625)".GetGeom();
            var offsetGeom = "LINESTRING (217998 104375 0, 217998 105000 625)".GetGeom();
            var startMeasure = 0;
            var endMeasure = 625;
            var offset = 2;
            var tolerance = 0.5;
            Logger.LogLine("Input Line : {0}", geom.ToString());
            var result = Geometry.OffsetGeometrySegment(geom, startMeasure, endMeasure, offset, tolerance);
            Logger.Log("Offset Line : {0}", result.ToString());
            SqlAssert.IsTrue(offsetGeom.STEquals(result));

            geom = "LINESTRING(2 2 0, 4 4 4, 10 5 5, 6 6 6)".GetGeom();
            offsetGeom =
                "LINESTRING (0.58578643762690508 3.4142135623730949 NULL 0, 3.0389924624238596 5.8674195871700494 NULL 1.7559685740440736, 0.18606322502027162 5.3919313809361125 NULL 3.2198581902465571, 5.514928749927333 4.0597149997093362 NULL 6)"
                    .GetGeom();
            startMeasure = 0;
            endMeasure = 6;
            offset = 2;
            tolerance = 0.5;
            Logger.LogLine("Input Line : {0}", geom.ToString());
            result = Geometry.OffsetGeometrySegment(geom, startMeasure, endMeasure, offset, tolerance);
            SqlAssert.IsTrue(offsetGeom.STEquals(result));

            geom = "LINESTRING(2 2 0, 4 4 4, 6 6 6, 10 5 8)".GetGeom();
            offsetGeom = "LINESTRING (0.58578643762690508 3.4142135623730949 NULL 0, 4.5857864376269051 7.4142135623730949 NULL 6)".GetGeom();
            startMeasure = 0;
            endMeasure = 6;
            offset = 2;
            tolerance = 0.5;
            Logger.LogLine("Input Line : {0}", geom.ToString());
            result = Geometry.OffsetGeometrySegment(geom, startMeasure, endMeasure, offset, tolerance);
            SqlAssert.IsTrue(offsetGeom.STEquals(result));
        }

        [TestMethod]
        public void PopulateGeometryMeasuresTest()
        {
            // 4 point line string
            var geom = "LINESTRING (10 1 10 100, 15 1 10 NULL, 20 1 10 NULL, 25 1 10 250 )".GetGeom();
            Logger.Log("Input Geom : {0}", geom.ToString());

            Logger.Log("Populating Geom with null Start and End Measure");
            var populatedGeometry = Geometry.PopulateGeometryMeasures(geom, null, null);
            Logger.Log("Populated Geom : {0}", populatedGeometry.ToString());

            // As per requirement; 
            // the default value of start point is 0 when null is specified
            // the default value of end point is cartographic length of the segment when null is specified
            SqlAssert.AreEqual(populatedGeometry.GetStartPointMeasure(), 0.0F);
            SqlAssert.AreEqual(populatedGeometry.GetEndPointMeasure(), 15.0F);

            double startMeasure = 10;
            double endMeasure = 40;
            // if the start, end measure would be non null, then this function overrides the 'M' value that has been passed
            Logger.Log("Populating Geom with Start Measure: {0} and End Measure: {1}", startMeasure, endMeasure);
            populatedGeometry = Geometry.PopulateGeometryMeasures(geom, startMeasure, endMeasure);
            Logger.Log("Populated Geom : {0}", populatedGeometry.ToString());
            Assert.AreEqual(populatedGeometry.GetStartPointMeasure(), startMeasure);
            SqlAssert.AreEqual(populatedGeometry.STPointN(2).M, 20.0F);
            SqlAssert.AreEqual(populatedGeometry.STPointN(3).M, 30.0F);
            Assert.AreEqual(populatedGeometry.GetEndPointMeasure(), endMeasure);
        }

        [TestMethod]
        public void ResetMeasureTest()
        {   // line string with null z value
            var geom = "LINESTRING (0 0 NULL 10, 10 0 NULL 20)".GetGeom();
            Logger.LogLine("Input Geom : {0}", geom.ToString());
            var expectedGeom = "LINESTRING (0 0 NULL, 10 0 NULL)".GetGeom();

            var measureResetGeom = Geometry.ResetMeasure(geom);
            Logger.Log("Expected Geom: {0}", expectedGeom);
            Logger.Log("Obtained Geom: {0}", measureResetGeom);

            SqlAssert.AreEqual(geom.GetStartPointMeasure(), 10);
            SqlAssert.AreEqual(geom.GetEndPointMeasure(), 20);
            SqlAssert.IsTrue(measureResetGeom.STEquals(expectedGeom));
            SqlAssert.IsTrue(measureResetGeom.STStartPoint().M.IsNull);
            SqlAssert.IsTrue(measureResetGeom.STEndPoint().M.IsNull);

            geom = "LINESTRING (11.235 25.987 NULL 116.124, 16.78 30.897 NULL 206.35)".GetGeom();
            Logger.LogLine("Input Geom : {0}", geom.ToString());
            expectedGeom = "LINESTRING (11.235 25.987, 16.78 30.897)".GetGeom();

            measureResetGeom = Geometry.ResetMeasure(geom);
            Logger.Log("Expected Geom: {0}", expectedGeom);
            Logger.Log("Obtained Geom: {0}", measureResetGeom);

            SqlAssert.AreEqual(geom.GetStartPointMeasure(), 116.124);
            SqlAssert.AreEqual(geom.GetEndPointMeasure(), 206.35);
            SqlAssert.IsTrue(measureResetGeom.STEquals(expectedGeom));
            SqlAssert.IsTrue(measureResetGeom.STStartPoint().M.IsNull);
            SqlAssert.IsTrue(measureResetGeom.STEndPoint().M.IsNull);
        }

        [TestMethod]
        public void ReverseLinearGeometryTest()
        {
            // Check for Multi Line
            var geom = "MULTILINESTRING((1 1 1,2 2 2),(3 3 3, 5 5 7))".GetGeom();
            var reversedStartPoint = "POINT (5 5 7)".GetGeom();
            Logger.LogLine("Input Geom : {0}", geom.ToString());
            var reversedGeom = Geometry.ReverseLinearGeometry(geom);
            Logger.Log("Reversed Geom : {0}", reversedGeom);
            SqlAssert.IsTrue(reversedGeom.STStartPoint().STEquals(reversedStartPoint));

            // Multi Line with 5 line segments
            geom = "MULTILINESTRING((1 1 1,2 2 2, 3 3 3),(4 4 4, 5 5 5, 6 6 6), (8 8 8, 9 9 9, 10 10 10), (11 11 11, 12 12 12, 13 13 13, 14 14 14))".GetGeom();
            reversedStartPoint = "POINT (14 14 14)".GetGeom();
            Logger.LogLine("Input Geom : {0}", geom.ToString());
            reversedGeom = Geometry.ReverseLinearGeometry(geom);
            Logger.Log("Reversed Geom : {0}", reversedGeom);
            SqlAssert.IsTrue(reversedGeom.STStartPoint().STEquals(reversedStartPoint));

            // Check for Line String
            geom = " LINESTRING(0 0 0, 10 0 40)".GetGeom();
            reversedStartPoint = "POINT (10 0 40)".GetGeom();
            Logger.LogLine("Input Geom : {0}", geom.ToString());
            reversedGeom = Geometry.ReverseLinearGeometry(geom);
            Logger.Log("Reversed Geom : {0}", reversedGeom);
            SqlAssert.IsTrue(reversedGeom.STStartPoint().STEquals(reversedStartPoint));

            // Check for Point
            geom = " POINT(10 0 40)".GetGeom();
            reversedStartPoint = "POINT (10 0 40)".GetGeom();
            Logger.LogLine("Input Geom : {0}", geom.ToString());
            reversedGeom = Geometry.ReverseLinearGeometry(geom);
            Logger.Log("Reversed Geom : {0}", reversedGeom);
            SqlAssert.IsTrue(reversedGeom.STStartPoint().STEquals(reversedStartPoint));
        }

        [TestMethod]
        public void ReverseTranslateMeasureGeometryTest()
        {
            // Check for Multi Line
            var geom = "MULTILINESTRING((1 1 1,2 2 2),(3 3 3, 5 5 7))".GetGeom();
            var offsetMeasure = 2;
            var reversedStartPoint = "POINT (5 5 9)".GetGeom();
            Logger.LogLine("Input Geom : {0}", geom.ToString());
            var reversedGeom = Geometry.ReverseAndTranslateGeometry(geom, offsetMeasure);
            Logger.Log("Reversed Geom : {0}", reversedGeom);
            SqlAssert.IsTrue(reversedGeom.STStartPoint().STEquals(reversedStartPoint));

            // Multi Line with 5 line segments
            geom = "MULTILINESTRING((1 1 1,2 2 2, 3 3 3),(4 4 4, 5 5 5, 6 6 6), (8 8 8, 9 9 9, 10 10 10), (11 11 11, 12 12 12, 13 13 13, 14 14 14))".GetGeom();
            reversedStartPoint = "POINT (14 14 16)".GetGeom();
            Logger.LogLine("Input Geom : {0}", geom.ToString());
            reversedGeom = Geometry.ReverseAndTranslateGeometry(geom, offsetMeasure);
            Logger.Log("Reversed Geom : {0}", reversedGeom);
            SqlAssert.IsTrue(reversedGeom.STStartPoint().STEquals(reversedStartPoint));

            // Check for Line String
            geom = " LINESTRING(0 0 0, 10 0 40)".GetGeom();
            reversedStartPoint = "POINT (10 0 42)".GetGeom();
            Logger.LogLine("Input Geom : {0}", geom.ToString());
            reversedGeom = Geometry.ReverseAndTranslateGeometry(geom, offsetMeasure);
            Logger.Log("Reversed Geom : {0}", reversedGeom);
            SqlAssert.IsTrue(reversedGeom.STStartPoint().STEquals(reversedStartPoint));

            // Check for Point
            geom = " POINT(10 0 40)".GetGeom();
            reversedStartPoint = "POINT (10 0 42)".GetGeom();
            Logger.LogLine("Input Geom : {0}", geom.ToString());
            reversedGeom = Geometry.ReverseAndTranslateGeometry(geom, offsetMeasure);
            Logger.Log("Reversed Geom : {0}", reversedGeom);
            SqlAssert.IsTrue(reversedGeom.STStartPoint().STEquals(reversedStartPoint));
        }

        [TestMethod]
        public void ScaleGeometryMeasureTest()
        {
            Logger.LogLine("Scale Geometry Measures.");
            // Check for Multi Line
            var geom = "MULTILINESTRING((1 1 1,2 2 2),(3 3 3, 5 5 7))".GetGeom();
            var scaleMeasure = -2;
            var scaledStartPoint = "POINT (1 1 2)".GetGeom();
            Logger.LogLine("Scale Measure : {0}", scaleMeasure);
            Logger.Log("Input Geom : {0}", geom.ToString());
            var scaledGeom = Geometry.ScaleGeometryMeasures(geom, scaleMeasure);
            Logger.Log("Reversed Geom : {0}", scaledGeom);
            SqlAssert.IsTrue(scaledGeom.STStartPoint().STEquals(scaledStartPoint));

            // Check for Line String
            scaleMeasure = -1;
            Logger.LogLine("Scale Measure : {0}", scaleMeasure);

            geom = " LINESTRING(0 0 0, 10 0 40)".GetGeom();
            scaledStartPoint = "POINT (0 0 0)".GetGeom();
            Logger.LogLine("Input Geom : {0}", geom.ToString());
            scaledGeom = Geometry.ScaleGeometryMeasures(geom, scaleMeasure);
            Logger.Log("Reversed Geom : {0}", scaledGeom);
            SqlAssert.IsTrue(scaledGeom.STStartPoint().STEquals(scaledStartPoint));

            // Check for Point
            scaleMeasure = 5;
            Logger.LogLine("Scale Measure : {0}", scaleMeasure);

            geom = " POINT(10 0 40)".GetGeom();
            scaledStartPoint = "POINT (10 0 200)".GetGeom();
            Logger.LogLine("Input Geom : {0}", geom.ToString());
            scaledGeom = Geometry.ScaleGeometryMeasures(geom, scaleMeasure);
            Logger.Log("Reversed Geom : {0}", scaledGeom);
            SqlAssert.IsTrue(scaledGeom.STStartPoint().STEquals(scaledStartPoint));
        }

        [TestMethod]
        public void SplitGeometryExperimentalTest()
        {
            var geom = "MULTILINESTRING ((2 2 2, 2 4 4), (8 4 8, 12 4 12, 12 10 29))".GetGeom();
            Logger.LogLine("Input Geom : {0}\n-------------------------------------\n", geom.ToString());

            DoSplitTest(1, geom);
            DoSplitTest(2, geom);
            DoSplitTest(3, geom);
            DoSplitTest(4, geom);
            DoSplitTest(5, geom);
            DoSplitTest(8, geom);
            DoSplitTest(12, geom);
            DoSplitTest(15, geom);
            DoSplitTest(29, geom);
            DoSplitTest(30, geom);

            geom = "POINT(2 2 7)".GetGeom();
            Logger.LogLine("Input Geom : {0}\n-------------------------------------\n", geom.ToString());

            DoSplitTest(1, geom);
            DoSplitTest(7, geom);
            DoSplitTest(8, geom);

            geom = "LINESTRING (2 2 2, 2 4 4, 8 4 8, 12 4 12, 12 10 29)".GetGeom();
            Logger.LogLine("Input Geom : {0}\n-------------------------------------\n", geom.ToString());

            DoSplitTest(1, geom);
            DoSplitTest(2, geom);
            DoSplitTest(3, geom);
            DoSplitTest(4, geom);
            DoSplitTest(5, geom);
            DoSplitTest(8, geom);
            DoSplitTest(12, geom);
            DoSplitTest(15, geom);
            DoSplitTest(29, geom);
            DoSplitTest(30, geom);
        }

        private void DoSplitTest(double measure, SqlGeometry geom)
        {
            try
            {
                SqlGeometry geomSegment1, geomSegment2;

                Logger.LogLine("Splitting for measure Geom : {0}", measure);
                Geometry.SplitGeometrySegment(geom, measure, out geomSegment1, out geomSegment2);
                Logger.Log("Segment 1 Geom : {0}", geomSegment1);
                Logger.Log("Segment 2 Geom : {0}", geomSegment2);
            }
            catch (Exception ex)
            {
                Logger.Log("Error : {0}", ex.Message);
            }
        }

        [TestMethod]
        public void TranslateMeasureGeometryTest()
        {
            var geom = "LINESTRING (2 2 6, 2 4 2, 8 4 8)".GetGeom();
            var translateMeasure = 2;
            var result = Geometry.TranslateMeasure(geom, translateMeasure);
            Logger.LogLine("Input : {0}", geom.ToString());
            Logger.Log("Result : {0}", result);

            geom = "MULTILINESTRING((1 1 1,2 2 2, 3 3 3),(4 4 4, 5 5 5, 6 6 6), (8 8 8, 9 9 9, 10 10 10), (11 11 11, 12 12 12, 13 13 13, 14 14 14))".GetGeom();
            result = Geometry.TranslateMeasure(geom, translateMeasure);
            Logger.LogLine("Input : {0}", geom.ToString());
            Logger.Log("Result : {0}", result);

            geom = "POINT(2 4 6)".GetGeom();
            result = Geometry.TranslateMeasure(geom, translateMeasure);
            Logger.LogLine("Input : {0}", geom.ToString());
            Logger.Log("Result : {0}", result);
        }

        [TestMethod]
        public void ValidateLRSGeometryTest()
        {
            var geom = "GEOMETRYCOLLECTION(LINESTRING(1 1 1, 3 5 2))".GetGeom();
            var result = Geometry.ValidateLRSGeometry(geom);
            Logger.LogLine("Input : {0}", geom.ToString());
            Logger.Log(result);
            Assert.AreEqual(LRSErrorCodes.InvalidLRS.Value(), result);

            geom = "POINT(5 6)".GetGeom();
            result = Geometry.ValidateLRSGeometry(geom);
            Logger.LogLine("Input : {0}", geom.ToString());
            Logger.Log(result);
            Assert.AreEqual(LRSErrorCodes.InvalidLRS.Value(), result);

            geom = "LINESTRING(1 1, 3 5)".GetGeom();
            result = Geometry.ValidateLRSGeometry(geom);
            Logger.LogLine("Input : {0}", geom.ToString());
            Logger.Log(result);
            Assert.AreEqual(LRSErrorCodes.InvalidLRS.Value(), result);

            geom = "MULTILINESTRING ((2 2, 2 4), (8 4, 12 4))".GetGeom();
            result = Geometry.ValidateLRSGeometry(geom);
            Logger.LogLine("Input : {0}", geom.ToString());
            Logger.Log(result);
            Assert.AreEqual(LRSErrorCodes.InvalidLRS.Value(), result);

            geom = "LINESTRING (2 2 0, 2 4 2, 8 4 8, 12 4 12, 12 10 29, 8 10 22, 5 14 27)".GetGeom();
            result = Geometry.ValidateLRSGeometry(geom);
            Logger.LogLine("Input : {0}", geom.ToString());
            Logger.Log(result);
            Assert.AreEqual(LRSErrorCodes.InvalidLRSMeasure.Value(), result);

            geom = "LINESTRING (2 2 6, 2 4 2, 8 4 8)".GetGeom();
            result = Geometry.ValidateLRSGeometry(geom);
            Logger.LogLine("Input : {0}", geom.ToString());
            Logger.Log(result);
            Assert.AreEqual(LRSErrorCodes.InvalidLRSMeasure.Value(), result);

            geom = "MULTILINESTRING ((2 2 2, 2 4 0), (8 4 8, 12 4 12, 12 10 29))".GetGeom();
            result = Geometry.ValidateLRSGeometry(geom);
            Logger.LogLine("Input : {0}", geom.ToString());
            Logger.Log(result);
            Assert.AreEqual(LRSErrorCodes.InvalidLRSMeasure.Value(), result);

            geom = "MULTILINESTRING ((2 2 2, 2 4 4), (8 4 4, 12 4 2, 12 10 29))".GetGeom();
            result = Geometry.ValidateLRSGeometry(geom);
            Logger.LogLine("Input : {0}", geom.ToString());
            Logger.Log(result);
            Assert.AreEqual(LRSErrorCodes.InvalidLRSMeasure.Value(), result);

            geom = "MULTILINESTRING((2 2 2, 2 4 4), (8 4 2, 12 4 4, 12 10 6))".GetGeom();
            result = Geometry.ValidateLRSGeometry(geom);
            Logger.LogLine("Input : {0}", geom.ToString());
            Logger.Log(result);
            Assert.AreEqual(LRSErrorCodes.InvalidLRSMeasure.Value(), result);

            geom = "MULTILINESTRING((2 2 2, 2 4 4), (8 4 4, 12 4 4, 12 10 6))".GetGeom();
            result = Geometry.ValidateLRSGeometry(geom);
            Logger.LogLine("Input : {0}", geom.ToString());
            Logger.Log(result);
            Assert.AreEqual(LRSErrorCodes.ValidLRS.Value(), result);

            geom = "MULTILINESTRING((2 2 2, 2 4 2), (8 4 4, 12 4 4, 12 10 6))".GetGeom();
            result = Geometry.ValidateLRSGeometry(geom);
            Logger.LogLine("Input : {0}", geom.ToString());
            Logger.Log(result);
            Assert.AreEqual(LRSErrorCodes.ValidLRS.Value(), result);

            // For the below TRUE is seen in Oracle; but in SQL the geom is Invalid hence we are returning invalid in this case.
            geom = "MULTILINESTRING((2 2 2, 2 2 2), (2 2 2, 2 2 2, 2 2 2))".GetGeom();
            result = Geometry.ValidateLRSGeometry(geom);
            Logger.LogLine("Input : {0}", geom.ToString());
            Logger.Log(result);
            Assert.AreEqual(LRSErrorCodes.InvalidLRS.Value(), result);

            // Valid cases
            geom = "POINT(5 6 5)".GetGeom();
            result = Geometry.ValidateLRSGeometry(geom);
            Logger.LogLine("Input : {0}", geom.ToString());
            Logger.Log(result);
            Assert.AreEqual("TRUE", result);

            geom = "LINESTRING(1 1 3, 3 5 5)".GetGeom();
            result = Geometry.ValidateLRSGeometry(geom);
            Logger.LogLine("Input : {0}", geom.ToString());
            Logger.Log(result);
            Assert.AreEqual("TRUE", result);

            geom = "MULTILINESTRING ((2 2 1, 2 4 4), (8 4 5, 12 4 6))".GetGeom();
            result = Geometry.ValidateLRSGeometry(geom);
            Logger.LogLine("Input : {0}", geom.ToString());
            Logger.Log(result);
            Assert.AreEqual("TRUE", result);

            geom = "LINESTRING (2 2 0, 2 4 2, 8 4 8, 12 4 12, 12 10 18, 8 10 22, 5 14 27)".GetGeom();
            result = Geometry.ValidateLRSGeometry(geom);
            Logger.LogLine("Input : {0}", geom.ToString());
            Logger.Log(result);
            Assert.AreEqual("TRUE", result);
        }

        /// <summary>
        /// Logs the clip geom segments.
        /// </summary>
        /// <param name="startMeasure">The start measure.</param>
        /// <param name="endMeasure">The end measure.</param>
        /// <param name="geom">The geom.</param>
        /// <param name="result">The result.</param>
        /// <param name="expected">The expected.</param>
        private void LogClipGeomSegments(double startMeasure, double endMeasure, SqlGeometry geom, SqlGeometry result, SqlGeometry expected)
        {
            Logger.LogLine("Input Clipped at measure : {0}, End Measure : {1}", startMeasure, endMeasure);
            Logger.Log("Input Geom : {0}", geom.ToString());
            Logger.Log("Expected : {0}", expected?.ToString());
            Logger.Log("Clipped : {0}", result?.ToString());

            if (expected == null)
                SqlAssert.IsTrue(result == null);
            else
                SqlAssert.IsTrue(expected.STEquals(result));
        }
    }
}
