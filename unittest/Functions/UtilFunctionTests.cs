//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

using System;
using Microsoft.SqlServer.Types;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SQLSpatialTools.Functions.Util;
using SQLSpatialTools.Sinks.Geometry;
using SQLSpatialTools.UnitTests.Extension;
using SQLSpatialTools.Utility;

namespace SQLSpatialTools.UnitTests.Functions
{
    [TestClass]
    public class UtilFunctionTests
    {
        [TestMethod]
        public void ExtractPointTest()
        {
            var geom = "POINT(1 1 1)".GetGeom();
            var expected = "POINT(1 1 1)".GetGeom();
            SqlAssert.IsTrue(expected.STEquals(Geometry.ExtractGeometry(geom, 1)));

            try
            {
                Geometry.ExtractGeometry(geom, 4, 2);
                Assert.Fail("Should through exception : Invalid index for element to be extracted.");
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("Invalid index for element to be extracted.", ex.Message);
            }

            try
            {
                Geometry.ExtractGeometry(geom, 1, 2);
                Assert.Fail("Should through exception : Invalid index for sub-element to be extracted.");
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("Invalid index for sub-element to be extracted.", ex.Message);
            }

            geom = "MULTIPOINT((1 1 1), (2 2 2), (3 3 3), (4 4 4))".GetGeom();
            expected = geom;
            SqlAssert.IsTrue(expected.STEquals(Geometry.ExtractGeometry(geom, 1)));
            SqlAssert.IsTrue(expected.STEquals(Geometry.ExtractGeometry(geom, 1, 0)));

            geom = "MULTIPOINT((1 1 1), (2 2 2), (3 3 3), (4 4 4))".GetGeom();
            expected = "POINT(1 1 1)".GetGeom();
            SqlAssert.IsTrue(expected.STEquals(Geometry.ExtractGeometry(geom, 1, 1)));

            expected = "POINT(3 3 3)".GetGeom();
            SqlAssert.IsTrue(expected.STEquals(Geometry.ExtractGeometry(geom, 1, 3)));

            try
            {
                Geometry.ExtractGeometry(geom, 2, 3);
                Assert.Fail("Should through exception : invalid index for element to be extracted");
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("Invalid index for element to be extracted.", ex.Message);
            }

            try
            {
                Geometry.ExtractGeometry(geom, 1, 5);
                Assert.Fail("Should through exception : invalid index for sub-element to be extracted");
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("Invalid index for sub-element to be extracted.", ex.Message);
            }
        }

        [TestMethod]
        public void ExtractLineStringTest()
        {
            // LINESTRING
            var geom = "LINESTRING(1 1 1, 2 2 2)".GetGeom();
            var expected = "LINESTRING(1 1 1, 2 2 2)".GetGeom();
            SqlAssert.IsTrue(expected.STEquals(Geometry.ExtractGeometry(geom, 1)));

            try
            {
                Geometry.ExtractGeometry(geom, 2);
                Assert.Fail("Should through exception : invalid index for element to be extracted");
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("Invalid index for element to be extracted.", ex.Message);
            }

            try
            {
                Geometry.ExtractGeometry(geom, 1, 2);
                Assert.Fail("Should through exception : invalid index for sub-element to be extracted");
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("Invalid index for sub-element to be extracted.", ex.Message);
            }

            // CIRCULARSTRING
            geom = "CIRCULARSTRING(1 1, 2 0, -1 1)".GetGeom();
            expected = "CIRCULARSTRING(1 1, 2 0, -1 1)".GetGeom();
            SqlAssert.IsTrue(expected.STEquals(Geometry.ExtractGeometry(geom, 1)));
            SqlAssert.IsTrue(expected.STEquals(Geometry.ExtractGeometry(geom, 1, 0)));
            SqlAssert.IsTrue(expected.STEquals(Geometry.ExtractGeometry(geom, 1, 1)));

            try
            {
                Geometry.ExtractGeometry(geom, 2);
                Assert.Fail("Should through exception : invalid index for element to be extracted");
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("Invalid index for element to be extracted.", ex.Message);
            }

            try
            {
                Geometry.ExtractGeometry(geom, 1, 2);
                Assert.Fail("Should through exception : invalid index for sub-element to be extracted");
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("Invalid index for sub-element to be extracted.", ex.Message);
            }

            // MULTILINESTRING
            geom = "MULTILINESTRING((1 1 1, 2 2 2), (3 3 3, 4 4 4))".GetGeom();
            expected = geom;
            SqlAssert.IsTrue(expected.STEquals(Geometry.ExtractGeometry(geom, 1)));
            SqlAssert.IsTrue(expected.STEquals(Geometry.ExtractGeometry(geom, 1, 0)));

            geom = "MULTILINESTRING((1 1 1, 2 2 2), (3 3 3, 4 4 4))".GetGeom();
            expected = "LINESTRING(1 1 1, 2 2 2)".GetGeom();
            SqlAssert.IsTrue(expected.STEquals(Geometry.ExtractGeometry(geom, 1, 1)));

            geom = "MULTILINESTRING((1 1 1, 2 2 2), (3 3 3, 4 4 4))".GetGeom();
            expected = "LINESTRING(3 3 3, 4 4 4)".GetGeom();
            SqlAssert.IsTrue(expected.STEquals(Geometry.ExtractGeometry(geom, 1, 2)));

            try
            {
                Geometry.ExtractGeometry(geom, 2, 2);
                Assert.Fail("Should through exception : invalid index for element to be extracted");
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("Invalid index for element to be extracted.", ex.Message);
            }

            try
            {
                Geometry.ExtractGeometry(geom, 1, 3);
                Assert.Fail("Should through exception : invalid index for sub-element to be extracted");
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("Invalid index for sub-element to be extracted.", ex.Message);
            }
        }

        [TestMethod]
        public void ExtractPolygonTest()
        {
            // Single Polygon - Sub index is to extract the inner rings
            var geom = "POLYGON((-5 -5, -5 5, 5 5, 5 -5, -5 -5), (0 0, 3 0, 3 3, 0 3, 0 0))".GetGeom();
            var expected = "POLYGON((-5 -5, -5 5, 5 5, 5 -5, -5 -5), (0 0, 3 0, 3 3, 0 3, 0 0))".GetGeom();
            var obtainedGeom = Geometry.ExtractGeometry(geom, 1);
            SqlAssert.IsTrue(expected.STEquals(obtainedGeom));

            geom = "POLYGON((-5 -5, -5 5, 5 5, 5 -5, -5 -5), (0 0, 3 0, 3 3, 0 3, 0 0))".GetGeom();
            expected = "POLYGON((-5 -5, -5 5, 5 5, 5 -5, -5 -5))".GetGeom();
            obtainedGeom = Geometry.ExtractGeometry(geom, 1, 1);
            SqlAssert.IsTrue(expected.STEquals(obtainedGeom));

            geom = "POLYGON((-5 -5, -5 5, 5 5, 5 -5, -5 -5), (0 0, 3 0, 3 3, 0 3, 0 0))".GetGeom();
            expected = "POLYGON((0 0, 3 0, 3 3, 0 3, 0 0))".GetGeom();
            obtainedGeom = Geometry.ExtractGeometry(geom, 1, 2);
            SqlAssert.IsTrue(expected.STEquals(obtainedGeom));

            try
            {
                Geometry.ExtractGeometry(geom, 2, 3);
                Assert.Fail("Should through exception : invalid index for element to be extracted");
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("Invalid index for element to be extracted.", ex.Message);
            }

            try
            {
                Geometry.ExtractGeometry(geom, 1, 3);
                Assert.Fail("Should through exception : invalid index for sub-element to be extracted");
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("Invalid index for sub-element to be extracted.", ex.Message);
            }

            // Multi Polygon
            geom = "MULTIPOLYGON(((1 1, 1 -1, -1 -1, -1 1, 1 1)),((1 1, 3 1, 3 3, 1 1)))".GetGeom();
            expected = "POLYGON((1 1, 1 -1, -1 -1, -1 1, 1 1))".GetGeom();
            SqlAssert.IsTrue(expected.STEquals(Geometry.ExtractGeometry(geom, 1)));
            SqlAssert.IsTrue(expected.STEquals(Geometry.ExtractGeometry(geom, 1, 0)));

            expected = "POLYGON((1 1, 3 1, 3 3, 1 1))".GetGeom();
            SqlAssert.IsTrue(expected.STEquals(Geometry.ExtractGeometry(geom, 2)));
            SqlAssert.IsTrue(expected.STEquals(Geometry.ExtractGeometry(geom, 2, 0)));

            try
            {
                Geometry.ExtractGeometry(geom, 3);
                Assert.Fail("Should through exception : invalid index for element to be extracted");
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("Invalid index for element to be extracted.", ex.Message);
            }


            geom = "MULTIPOLYGON(((0 0, 0 3, 3 3, 3 0, 0 0), (1 1, 1 2, 2 1, 1 1)), ((9 9, 9 10, 10 9, 9 9)))".GetGeom();
            expected = "POLYGON((0 0, 0 3, 3 3, 3 0, 0 0), (1 1, 1 2, 2 1, 1 1))".GetGeom();
            SqlAssert.IsTrue(expected.STEquals(Geometry.ExtractGeometry(geom, 1)));
            SqlAssert.IsTrue(expected.STEquals(Geometry.ExtractGeometry(geom, 1, 0)));

            expected = "POLYGON((0 0, 0 3, 3 3, 3 0, 0 0))".GetGeom();
            SqlAssert.IsTrue(expected.STEquals(Geometry.ExtractGeometry(geom, 1, 1)));

            expected = "POLYGON((1 1, 1 2, 2 1, 1 1))".GetGeom();
            SqlAssert.IsTrue(expected.STEquals(Geometry.ExtractGeometry(geom, 1, 2)));

            try
            {
                Geometry.ExtractGeometry(geom, 1, 3);
                Assert.Fail("Should through exception : invalid index for sub-element to be extracted");
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("Invalid index for sub-element to be extracted.", ex.Message);
            }

            expected = "POLYGON((9 9, 9 10, 10 9, 9 9))".GetGeom();
            SqlAssert.IsTrue(expected.STEquals(Geometry.ExtractGeometry(geom, 2, 0)));
            SqlAssert.IsTrue(expected.STEquals(Geometry.ExtractGeometry(geom, 2, 1)));

            try
            {
                Geometry.ExtractGeometry(geom, 2, 2);
                Assert.Fail("Should through exception : invalid index for sub-element to be extracted");
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("Invalid index for sub-element to be extracted.", ex.Message);
            }
        }

        [TestMethod]
        public void ExtractCompoundCurveTest()
        {
            var geom = "COMPOUNDCURVE(CIRCULARSTRING(1 0, 0 1, -1 0), (-1 0, 2 0))".GetGeom();
            var expected = "COMPOUNDCURVE(CIRCULARSTRING(1 0, 0 1, -1 0), (-1 0, 2 0))".GetGeom();
            var obtainedGeom = Geometry.ExtractGeometry(geom, 1);
            SqlAssert.IsTrue(expected.STEquals(obtainedGeom));

            obtainedGeom = Geometry.ExtractGeometry(geom, 1, 0);
            SqlAssert.IsTrue(expected.STEquals(obtainedGeom));

            obtainedGeom = Geometry.ExtractGeometry(geom, 1, 1);
            SqlAssert.IsTrue(expected.STEquals(obtainedGeom));

            try
            {
                Geometry.ExtractGeometry(geom, 0, 1);
                Assert.Fail("Should through exception : invalid index for element to be extracted");
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("Invalid index for element to be extracted.", ex.Message);
            }

            try
            {
                Geometry.ExtractGeometry(geom, 1, 3);
                Assert.Fail("Should through exception : invalid index for sub-element to be extracted");
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("Invalid index for sub-element to be extracted.", ex.Message);
            }
        }


        [TestMethod]
        public void ExtractGeometryCollectionTest()
        {
            var geom = "GEOMETRYCOLLECTION(LINESTRING(1 1, 2 2), COMPOUNDCURVE(CIRCULARSTRING(1 0, 0 1, -1 0), (-1 0, 2 0)))".GetGeom();
            var expected = "LINESTRING(1 1, 2 2)".GetGeom();
            var obtainedGeom = Geometry.ExtractGeometry(geom, 1);
            SqlAssert.IsTrue(expected.STEquals(obtainedGeom));

            obtainedGeom = Geometry.ExtractGeometry(geom, 1, 0);
            SqlAssert.IsTrue(expected.STEquals(obtainedGeom));

            obtainedGeom = Geometry.ExtractGeometry(geom, 1, 1);
            SqlAssert.IsTrue(expected.STEquals(obtainedGeom));

            expected = "COMPOUNDCURVE(CIRCULARSTRING(1 0, 0 1, -1 0), (-1 0, 2 0))".GetGeom();
            obtainedGeom = Geometry.ExtractGeometry(geom, 2);
            SqlAssert.IsTrue(expected.STEquals(obtainedGeom));

            obtainedGeom = Geometry.ExtractGeometry(geom, 2, 0);
            SqlAssert.IsTrue(expected.STEquals(obtainedGeom));

            obtainedGeom = Geometry.ExtractGeometry(geom, 2, 1);
            SqlAssert.IsTrue(expected.STEquals(obtainedGeom));

            try
            {
                Geometry.ExtractGeometry(geom, 0, 1);
                Assert.Fail("Should through exception : invalid index for element to be extracted");
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("Invalid index for element to be extracted.", ex.Message);
            }

            try
            {
                Geometry.ExtractGeometry(geom, 3, 1);
                Assert.Fail("Should through exception : invalid index for element to be extracted");
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("Invalid index for element to be extracted.", ex.Message);
            }

            try
            {
                Geometry.ExtractGeometry(geom, 1, 2);
                Assert.Fail("Should through exception : invalid index for sub-element to be extracted");
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("Invalid index for sub-element to be extracted.", ex.Message);
            }

            try
            {
                Geometry.ExtractGeometry(geom, 1, 3);
                Assert.Fail("Should through exception : invalid index for sub-element to be extracted");
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("Invalid index for sub-element to be extracted.", ex.Message);
            }


            geom = "GEOMETRYCOLLECTION(MULTILINESTRING((1 1, 2 2), (4 4, 5 5, 7 7), (8 8, 9 9)), POLYGON((0 0, 0 3, 3 3, 3 0, 0 0), (1 1, 1 2, 2 1, 1 1)))".GetGeom();
            expected = "LINESTRING(4 4, 5 5, 7 7)".GetGeom();
            obtainedGeom = Geometry.ExtractGeometry(geom, 1, 2);
            SqlAssert.IsTrue(expected.STEquals(obtainedGeom));

            expected = "LINESTRING(8 8, 9 9)".GetGeom();
            obtainedGeom = Geometry.ExtractGeometry(geom, 1, 3);
            SqlAssert.IsTrue(expected.STEquals(obtainedGeom));

            expected = "POLYGON((0 0, 0 3, 3 3, 3 0, 0 0))".GetGeom();
            obtainedGeom = Geometry.ExtractGeometry(geom, 2, 1);
            SqlAssert.IsTrue(expected.STEquals(obtainedGeom));

            expected = "POLYGON((1 1, 1 2, 2 1, 1 1))".GetGeom();
            obtainedGeom = Geometry.ExtractGeometry(geom, 2, 2);
            SqlAssert.IsTrue(expected.STEquals(obtainedGeom));

            geom = "GEOMETRYCOLLECTION(MULTILINESTRING((1 1, 2 2), (4 4, 5 5, 7 7), (8 8, 9 9)), POLYGON((0 0, 0 3, 3 3, 3 0, 0 0), (1 1, 1 2, 2 1, 1 1)), MULTIPOINT((1 1), (2 2), (4 4)))".GetGeom();

            expected = "POINT(2 2)".GetGeom();
            obtainedGeom = Geometry.ExtractGeometry(geom, 3, 2);
            SqlAssert.IsTrue(expected.STEquals(obtainedGeom));

            expected = "POINT(4 4)".GetGeom();
            obtainedGeom = Geometry.ExtractGeometry(geom, 3, 3);
            SqlAssert.IsTrue(expected.STEquals(obtainedGeom));
        }
    }
}
