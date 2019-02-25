using System;
using Microsoft.SqlServer.Types;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SQLSpatialTools.Function;
using SQLSpatialTools.Utility;

namespace SQLSpatialTools.Tests
{
    [TestClass]
    public class GeneralFunctionTests : UnitTest
    {
        [TestMethod]
        public void FilterArtifactsGeometryTest()
        {
            var geom = "GEOMETRYCOLLECTION(LINESTRING EMPTY, LINESTRING (1 1, 3 5), POINT (1 1), POLYGON ((-1 -1, -1 -5, -5 -5, -5 -1, -1 -1)))".GetGeom();

            // Empty line and point should be removed
            // short line should be removed - tolerence length
            var shortLineTolerence = 5;
            // Polygon inner ring with area < tolerence * polygon length
            var polygonAreaTolerance = 1.5;

            Logger.LogLine("Input Geometry: {0}", geom);
            Logger.Log("Filtering input geometry; removing empty linestring");
            Logger.Log("points, short line of tolerence: {0}, Polygon with inner ring area tolerence: {1}", shortLineTolerence, polygonAreaTolerance);
            var expectedGeom = "GEOMETRYCOLLECTION EMPTY".GetGeom();
            var filteredGeom = General.Geometry.FilterArtifactsGeometry(geom, true, true, shortLineTolerence, polygonAreaTolerance);
            Logger.Log("Expected converted geom: {0}", expectedGeom);
            Logger.Log("Obtained converted geom: {0}", filteredGeom);
            SqlAssert.IsTrue(filteredGeom.STEquals(expectedGeom));
        }

        [TestMethod]
        public void GeomFromXYMTextTest()
        {
            SqlGeometry convertedGeom;
            var geomWKT = "LINESTRING (0 0 3 4, 10 0 3 4)";
            Logger.LogLine("Converting input Geom with 3 dimension and measure : {0}", geomWKT);
            try
            {
                convertedGeom = General.Geometry.GeomFromXYMText(geomWKT, Constants.DEFAULT_SRID);
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual(e.Message, ErrorMessage.WKT3DOnly);
                TestContext.WriteLine(ErrorMessage.WKT3DOnly);
            }

            geomWKT = "LINESTRING (0 0 3, 10 0 4)";
            Logger.LogLine("Converting input Geom with 3 dimension and measure : {0}", geomWKT);
            var expectedGeom = "LINESTRING(0 0 NULL 3, 10 0 NULL 4)".GetGeom();
            convertedGeom = General.Geometry.GeomFromXYMText(geomWKT, Constants.DEFAULT_SRID);
            Logger.Log("Expected converted geom: {0}", expectedGeom);
            Logger.Log("Obtained converted geom: {0}", convertedGeom);
            SqlAssert.IsTrue(convertedGeom.STEquals(expectedGeom));
        }

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
        public void LocatePointAlongGeomTest()
        {
            var geom = "LINESTRING (0 0, 10 0)".GetGeom();
            Logger.Log("Input Geom : {0}", geom.ToString());
            var returnPoint = "POINT (5 0)".GetGeom();
            var distance = 5;

            Logger.LogLine("Locating a point at distance of {0} Measure", distance);
            var locatedPoint = General.Geometry.LocatePointAlongGeom(geom, distance);
            Logger.Log("Expected point: {0}", returnPoint);
            Logger.Log("Located  point: {0} at distance of {1} Measure", locatedPoint, distance);
            SqlAssert.IsTrue(locatedPoint.STEquals(returnPoint));
        }

        [TestMethod]
        public void MakeValidForGeographyTest()
        {
            var geometry = "CURVEPOLYGON (CIRCULARSTRING (0 -4, 4 0, 0 4, -4 0, 0 -4))".GetGeom();
            var retGeom = General.Geometry.MakeValidForGeography(geometry);
            Logger.LogLine("Executing Make Valid: {0}", geometry);
            SqlAssert.IsTrue(retGeom.STEquals(retGeom));

            geometry = "LINESTRING(0 2, 1 1, 1 0, 1 1, 2 2)".GetGeom();
            Logger.LogLine("Executing Make Valid: {0}", geometry);
            var expectedGeom = "MULTILINESTRING ((7.1054273576010019E-15 2, 1 1, 2 2), (1 1, 1 7.1054273576010019E-15))".GetGeom();
            retGeom = General.Geometry.MakeValidForGeography(geometry);
            Logger.Log("Expected Geom: {0}", expectedGeom);
            Logger.Log("Obtained Geom: {0}", retGeom);
            SqlAssert.IsTrue(retGeom.STEquals(expectedGeom));
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