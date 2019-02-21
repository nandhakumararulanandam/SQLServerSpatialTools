using System;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Types;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SQLSpatialTools;
using SQLSpatialTools.Function;

namespace SQLSpatialTools.Tests
{
    [TestClass]
    public class GeneralFunctionTests : UnitTest
    {
        [TestMethod]
        public void InterpolateBetweenGeomTest()
        {
            var geom1 = "POINT(0 0 0 0)".GetGeom();
            var geom2 = "POINT(10 0 0 10)".GetGeom();
            var returnPoint = "POINT (5 0 NULL 5)".GetGeom();
            var distance = 5;
            Logger.LogLine("Input Point 1:{0} Point 2:{1}", geom1, geom2);
            Logger.Log("Interpolating at a distance of {0}", geom1, geom2, distance);
            Logger.LogLine("Expected Point: {0}", returnPoint);
            var sqlgeom = General.Geometry.InterpolateBetweenGeom(geom1, geom2, distance);
            Logger.Log("Obtained Point: {0}", sqlgeom.ToString());
            SqlAssert.IsTrue(sqlgeom.STEquals(returnPoint));
        }


        [TestMethod]
        public void ReverseLinestringTest()
        {
            var geom = "LINESTRING (1 1, 5 5)".GetGeom();
            Logger.Log("Input Geom : {0}", geom.ToString());

            var endPoint = "POINT (5 5 0 0)".GetGeom();
            var reversedLineSegment = General.Geometry.ReverseLinestring(geom);
            Logger.Log("Reversed Line string : {0}", reversedLineSegment.ToString());
            SqlAssert.IsTrue(reversedLineSegment.STStartPoint().STEquals(endPoint));
        }

        [TestMethod]
        public void ShiftGeometryTest()
        {
            // Point
            var geom = "POINT(0 1)".GetGeom();
            var shiftPoint = "POINT (4 5)".GetGeom();
            double xShift = 4, yShift = 4;
            Logger.LogLine("Input Point: {0}", geom);
            Logger.Log("Expected Point: {0}", shiftPoint);
            var shiftedGeom = General.Geometry.ShiftGeometry(geom, xShift, yShift);
            Logger.Log("Obtained Point: {0}", shiftedGeom);
            SqlAssert.IsTrue(shiftedGeom.STEquals(shiftPoint));

            // Simple Line String
            geom = "LINESTRING (1 1, 4 4)".GetGeom();
            shiftPoint = "LINESTRING (11 11, 14 14)".GetGeom();
            xShift = 10; yShift = 10;
            Logger.LogLine("Input Geom: {0}", geom);
            Logger.Log("Expected Geom: {0}", shiftPoint);
            shiftedGeom = General.Geometry.ShiftGeometry(geom, xShift, yShift);
            Logger.Log("Obtained Point: {0}", shiftedGeom);
            SqlAssert.IsTrue(shiftedGeom.STEquals(shiftPoint));

            // Line String with multiple points
            geom = "LINESTRING (1 1, 2 3, -1 -3, 4 -3, -2 1)".GetGeom();
            shiftPoint = "LINESTRING (11 11, 12 13, 9 7, 14 7, 8 11)".GetGeom();
            Logger.LogLine("Input Geom: {0}", geom);
            Logger.Log("Expected Geom: {0}", shiftPoint);
            shiftedGeom = General.Geometry.ShiftGeometry(geom, xShift, yShift);
            Logger.Log("Obtained Point: {0}", shiftedGeom);
            SqlAssert.IsTrue(shiftedGeom.STEquals(shiftPoint));

            // Multi Line String
            geom = "MULTILINESTRING ((1 1, 2 3), (-1 -3, 4 -3, -2 1))".GetGeom();
            shiftPoint = "MULTILINESTRING ((11 11, 12 13), (9 7, 14 7, 8 11))".GetGeom();
            Logger.LogLine("Input Geom: {0}", geom);
            Logger.Log("Expected Geom: {0}", shiftPoint);
            shiftedGeom = General.Geometry.ShiftGeometry(geom, xShift, yShift);
            Logger.Log("Obtained Point: {0}", shiftedGeom);
            SqlAssert.IsTrue(shiftedGeom.STEquals(shiftPoint));

            // Polygon
            geom = "POLYGON((1 1, 3 3, 3 1, 1 1))".GetGeom();
            shiftPoint = "POLYGON ((11 11, 13 13, 13 11, 11 11))".GetGeom();
            Logger.LogLine("Input Geom: {0}", geom);
            Logger.Log("Expected Geom: {0}", shiftPoint);
            shiftedGeom = General.Geometry.ShiftGeometry(geom, xShift, yShift);
            Logger.Log("Obtained Point: {0}", shiftedGeom);
            SqlAssert.IsTrue(shiftedGeom.STEquals(shiftPoint));
        }
    }
}