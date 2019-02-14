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
            MergeGeometrySegments_LineString_Test();
            MergeGeometrySegments_Polygon_Test();
        }
        /**
         *Method focuses the linestring geometry structure
         */
        public static void MergeGeometrySegments_LineString_Test()
        {
            var geom1 = "LINESTRING(1 1 NULL 0, 1 5 NULL 1)".GetGeom();
            var geom2 = "LINESTRING(1 5 NULL 5, 2 3 NULL 0)".GetGeom();
            var resultantGeom = Functions.MergeGeometrySegments(geom1, geom2);
            var lastPoint = resultantGeom.STPointN(Int32.Parse(resultantGeom.STNumPoints().ToString()));
            Assert.IsTrue((bool)(lastPoint.STStartPoint().STX == 2));   // should be the last point's x value
            Assert.IsTrue((bool)(lastPoint.STStartPoint().STY == 3));   // should be the last point's y value
        }
        /**
         *Method focuses the polygon geometry structure
         */
        public static void MergeGeometrySegments_Polygon_Test()
        {
            var rand = new Random();
            int x = rand.Next(1000, 2000);
            int y = x * 2;
            string z = "NULL";
            int m = 3;
            string sqlstring = "POLYGON ((" + x + " " + x + " " + z + " " + m + ", " + y + " " + x + " " + z + " " + m + ", " + y + " " + y + " " + z + " " + m + ", " + x + " " + x + " " + z + " " + m + "))";
            var geom1 = SqlGeometry.STPolyFromText(new SqlChars(new SqlString(sqlstring)), Functions.DEFAULT_SRID).MakeValid();
            x = rand.Next(500, 1500);
            y = x * 2;
            sqlstring = "POLYGON ((" + x + " " + x + " " + z + " " + m + ", " + y + " " + x + " " + z + " " + m + ", " + y + " " + y + " " + z + " " + m + ", " + x + " " + x + " " + z + " " + m + "))";
            var geom2 = SqlGeometry.STPolyFromText(new SqlChars(new SqlString(sqlstring)), Functions.DEFAULT_SRID).MakeValid();
            var resultantGeom = Functions.MergeGeometrySegments(geom1, geom2);
            /*
             * MergeGeometrySegments utility function currently **unavailable** for POLYGON geometry structure
            */
        }
        public static void SplitGeometrySegment_Test()
        {
            SplitGeometrySegment_Valid_LineSegment_Test();
            SplitGeometrySegment_Invalid_LineSegment_Test();
            SplitGeometrySegment_MultiLineSegment_Test();
        }

        public static void SplitGeometrySegment_Valid_LineSegment_Test()
        {
            var lineSegment = "LINESTRING(1 1 NULL 4, 2 2 NULL 20)".GetGeom();
            Functions.SplitGeometrySegment(lineSegment, 6, out var lineSegment3, out var lineSegment4);
            Assert.IsTrue((bool)lineSegment3.STIsValid());
            Assert.IsTrue((bool)lineSegment4.STIsValid());
        }

        public static void SplitGeometrySegment_Invalid_LineSegment_Test()
        {
            var lineSegment = "LINESTRING(1 1 NULL 0, 1 5 NULL 0)".GetGeom();
            try
            {
                Functions.SplitGeometrySegment(lineSegment, 2, out var lineSegment1, out var lineSegment2);
                Assert.Fail("Accepts the invalid measure parameter"); // since parent line segment would be '0'
            }
            catch (Exception) { }
        }
        public static void SplitGeometrySegment_MultiLineSegment_Test()
        {
            int startingPointMValue = 4;
            int endingPointMValue = 20;
            int splitPointMValue = 6; //  this should fall between the startingPointMValue and endingPointMValue
            var lineSegment = SqlGeometry.STMLineFromText(new SqlChars(new SqlString("MULTILINESTRING((1 1 NULL " + startingPointMValue + ", 2 15 NULL " + endingPointMValue + "))")), Functions.DEFAULT_SRID);
            Functions.SplitGeometrySegment(lineSegment, splitPointMValue, out var lineSegment3, out var lineSegment4);
            // unavailable for multi line string
        }

        public static void ReverseLineString_Test()
        {
            int startPtX = 1;
            int startPtY = 1;
            int endPtX = 5;
            int endPtY = 5;
            var lineSegment = SqlGeometry.STLineFromText(new SqlChars(new SqlString("LINESTRING(" + startPtX + " " + startPtY + "   0 0, " + endPtX + " " + endPtY + " 0 0)")), Functions.DEFAULT_SRID);
            var reversedLineSegment = Functions.ReverseLinearGeometry(lineSegment);
            Assert.IsTrue((bool)(reversedLineSegment.STStartPoint().STX == endPtX));
            Assert.IsTrue((bool)(reversedLineSegment.STEndPoint().STY == endPtY));
        }

        public static void IsConnectedGeomLineSegments_Test()
        {
            // test cases without considering tolerance values
            SqlGeometry g1 = "LINESTRING(0 0 0 0, 1 1 0 0, 3 4 0 0, 5.5 5 0 0)".GetGeom();
            SqlGeometry g2 = "LINESTRING(0 0 0 0, 2 2 0 0)".GetGeom();
            double tolerance = 0;
            bool result = (bool)Functions.IsConnectedGeomSegments(g1, g2, tolerance);
            Assert.IsTrue(result);
            
            g1 = "LINESTRING(0 0 0 0, 1 1 0 0, 3 4 0 0, 5.5 5 0 0)".GetGeom();
            g2 = "LINESTRING(2 2 0 0, 0 0 0 0)".GetGeom();
            tolerance = 0;
            result = (bool)Functions.IsConnectedGeomSegments(g1, g2, tolerance);
            Assert.IsTrue(result);
            
            g1 = "LINESTRING(0 0 0 0, 1 1 0 0, 3 4 0 0, 5.5 5 0 0)".GetGeom();
            g2 = "LINESTRING(5.5 5 0 0, 2 2 0 0)".GetGeom();
            tolerance = 0;
            result = (bool)Functions.IsConnectedGeomSegments(g1, g2, tolerance);
            Assert.IsTrue(result);
            
            g1 = "LINESTRING(0 0 0 0, 1 1 0 0, 3 4 0 0, 5.5 5 0 0)".GetGeom();
            g2 = "LINESTRING(2 2 9 0, 5.5 5 0 0)".GetGeom();
            tolerance = 0;
            result = (bool)Functions.IsConnectedGeomSegments(g1, g2, tolerance);
            Assert.IsTrue(result);

            // test cases with tolerance values considered
            g1 = "LINESTRING(0 0 0 0, 1 1 0 0, 3 4 0 0, 5.5 5 0 0)".GetGeom();
            g2 = "LINESTRING(0.5 0.5 0 0, 2 2 0 0)".GetGeom();
            tolerance = 0.5;
            result = (bool)Functions.IsConnectedGeomSegments(g1, g2, tolerance);
            Assert.IsFalse(result);
            
            g1 = "LINESTRING(0 0 0 0, 1 1 0 0, 3 4 0 0, 5.5 5 0 0)".GetGeom();
            g2 = "LINESTRING(0.5 0 0 0, 2 2 0 0)".GetGeom();
            tolerance = 0.5;
            result = (bool)Functions.IsConnectedGeomSegments(g1, g2, tolerance);
            Assert.IsTrue(result);
            
            g1 = "LINESTRING(0 0 0 0, 1 1 0 0, 3 4 0 0, 5.5 5 0 0)".GetGeom();
            g2 = "LINESTRING(1.5 1.5 0 0, 2 2 0 0)".GetGeom();
            tolerance = 0.5;
            result = (bool)Functions.IsConnectedGeomSegments(g1, g2, tolerance);
            Assert.IsFalse(result);
            
            g1 = "LINESTRING(0 0 0 0, 1 1 0 0, 3 4 0 0, 5.5 5 0 0)".GetGeom();
            g2 = "LINESTRING(0 0.5 0 0, 2 2 0 0)".GetGeom();
            tolerance = 0.5;
            result = (bool)Functions.IsConnectedGeomSegments(g1, g2, tolerance);
            Assert.IsTrue(result);
            
            g1 = "LINESTRING(0 0 0 0, 1 1 0 0, 3 4 0 0, 5.5 5 0 0)".GetGeom();
            g2 = "LINESTRING(2 2 0 0, 0.1 0.6 0 0)".GetGeom();
            tolerance = 0.5;
            result = (bool)Functions.IsConnectedGeomSegments(g1, g2, tolerance);
            Assert.IsFalse(result);
            
            g1 = "LINESTRING(0 0 0 0, 1 1 0 0, 3 4 0 0, 5.5 5 0 0)".GetGeom();
            g2 = "LINESTRING(2 2 0 0, 0.6 0.1 0 0)".GetGeom();
            tolerance = 1;
            result = (bool)Functions.IsConnectedGeomSegments(g1, g2, tolerance);
            Assert.IsTrue(result);
            
            g1 = "LINESTRING(0 0 0 0, 1 1 0 0, 3 4 0 0, 5.5 5 0 0)".GetGeom();
            g2 = "LINESTRING(5 5 0 0, 2 2 0 0)".GetGeom();
            tolerance = 0.5;
            result = (bool)Functions.IsConnectedGeomSegments(g1, g2, tolerance);
            Assert.IsTrue(result);
            
            g1 = "LINESTRING(0 0 0 0, 1 1 0 0, 3 4 0 0, 5.5 5 0 0)".GetGeom();
            g2 = "LINESTRING(2 2 9 0, 6 4.9 0 0)".GetGeom();
            tolerance = 0.5;
            result = (bool)Functions.IsConnectedGeomSegments(g1, g2, tolerance);
            Assert.IsFalse(result);
        }
        public static void Main(string[] a)
        {
            IsConnectedGeomLineSegments_Test();
            Console.ReadLine();
        }
    }
}