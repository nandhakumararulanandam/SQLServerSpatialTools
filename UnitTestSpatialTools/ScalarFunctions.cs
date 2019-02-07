using System;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Types;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SQLSpatialTools;

namespace SpatialTools.UnitTest
{
    [TestClass]
    public class ScalarFunctions
    {
        [TestMethod]
        public void LocateMAlongGeom_Test()
        {
            var geom = "LINESTRING (0 0 0 0, 10 0 0 10)".GetGeom();
            var returnPoint = "POINT (5 0 NULL 5)".GetGeom();
            var distance = 5;

            var sqlgeom = Functions.LocateMAlongGeom(geom, distance);
            Assert.IsTrue((bool)sqlgeom.STEquals(returnPoint));
        }

        [TestMethod]
        public void InterpolateBetweenGeom_Test()
        {
            var geom1 = SqlGeometry.STPointFromText(new SqlChars("POINT(0 0 0 0)"), Functions.DEFAULT_SRID);
            var geom2 = SqlGeometry.STPointFromText(new SqlChars("POINT(10 0 0 10)"), Functions.DEFAULT_SRID);
            var returnPoint = SqlGeometry.STPointFromText(new SqlChars("POINT (5 0 NULL 5)"), Functions.DEFAULT_SRID);
            var distance = 5;

            var sqlgeom = Functions.InterpolateBetweenGeom(geom1, geom2, distance);
            Assert.IsTrue((bool)sqlgeom.STEquals(returnPoint));
        }

        [TestMethod]
        public static void PopulateGeometryMeasures_Test()
        {
            var geom = SqlGeometry.STLineFromText(new SqlChars("LINESTRING (10 1 10 100, 15 5 10 150 )"), Functions.DEFAULT_SRID); //
            var populatedGeometry = Functions.PopulateGeometryMeasures(geom, null, null);
            // if the start, end measure would be null, then this function populates the existing 'M' (measure) value
            Assert.IsTrue((bool)(populatedGeometry.STPointN(1).M == 100));
            Assert.IsTrue((bool)(populatedGeometry.STPointN(1).M == 150));
            double updatedStartMeasure = 200;
            double updatedEndMeasure = 200;
            // if the start, end measure would be non null, then this function overrides the 'M' value that has been passed
            populatedGeometry = Functions.PopulateGeometryMeasures(geom, updatedStartMeasure, updatedEndMeasure);
            Assert.IsTrue((bool)(populatedGeometry.STPointN(1).M == updatedStartMeasure));
            Assert.IsTrue((bool)(populatedGeometry.STPointN(1).M == updatedEndMeasure));
        }
  
        [TestMethod]
        public static void MergeGeometrySegments_Test()
        { 
            var geom1 = "LINESTRING(1 1 NULL 0, 1 5 NULL 1)".GetGeom();
            var geom2 = "LINESTRING(1 5 NULL 5, 2 3 NULL 0)".GetGeom();
            var resultantGeom = Functions.MergeGeometrySegments(geom1, geom2);
            var lastPoint = resultantGeom.STPointN(Int32.Parse(resultantGeom.STNumPoints().ToString()));
            Assert.IsTrue((bool)(lastPoint.STStartPoint().STX == 2));   // should be the last point's x value
            Assert.IsTrue((bool)(lastPoint.STStartPoint().STY == 3));   // should be the last point's y value
            /*
                TODO: Have to implement polygon with various co-ordinates
                geom1 = "POLYGON((3 3 , 3 6 , 6 6 , 6 3 , 3 3 ))".GetGeom();
                geom2 = "POLYGON((2 2 , 2 8 , 8 8 , 2 8 , 2 2 ))".GetGeom();
                resultantGeom = Functions.MergeGeometrySegments(geom1, geom2);
            */
        }
    }
}