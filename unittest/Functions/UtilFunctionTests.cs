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
                Assert.Fail("Should through exception : invalid index for sub-element to be extracted");
            }
            catch (ArgumentException)
            {
                // ignore
            }

            geom = "MULTIPOINT((1 1 1), (2 2 2), (3 3 3), (4 4 4))".GetGeom();
            expected = "POINT(1 1 1)".GetGeom();
            SqlAssert.IsTrue(expected.STEquals(Geometry.ExtractGeometry(geom, 1)));

            expected = "POINT(3 3 3)".GetGeom();
            SqlAssert.IsTrue(expected.STEquals(Geometry.ExtractGeometry(geom, 3)));

            try
            {
                Geometry.ExtractGeometry(geom, 2, 3);
                Assert.Fail("Should through exception : invalid index for element to be extracted");
            }
            catch (ArgumentException)
            {
                // ignore
            }

            try
            {
                Geometry.ExtractGeometry(geom, 1, 3);
                Assert.Fail("Should through exception : invalid index for sub-element to be extracted");
            }
            catch (ArgumentException)
            {
                // ignore
            }
        }

        [TestMethod]
        public void ExtractLineStringTest()
        {
            var geom = "LINESTRING(1 1 1, 2 2 2)".GetGeom();
            var expected = "LINESTRING(1 1 1, 2 2 2)".GetGeom();
            SqlAssert.IsTrue(expected.STEquals(Geometry.ExtractGeometry(geom, 1)));

            try
            {
                Geometry.ExtractGeometry(geom, 2);
                Assert.Fail("Should through exception : invalid index for element to be extracted");
            }
            catch (ArgumentException)
            {
                // ignore
            }

            try
            {
                Geometry.ExtractGeometry(geom, 1, 2);
                Assert.Fail("Should through exception : invalid index for sub-element to be extracted");
            }
            catch (ArgumentException)
            {
                // ignore
            }

            geom = "MULTILINESTRING((1 1 1, 2 2 2), (3 3 3, 4 4 4))".GetGeom();
            expected = "LINESTRING(1 1 1, 2 2 2)".GetGeom();
            SqlAssert.IsTrue(expected.STEquals(Geometry.ExtractGeometry(geom, 1)));

            geom = "MULTILINESTRING((1 1 1, 2 2 2), (3 3 3, 4 4 4))".GetGeom();
            expected = "LINESTRING(3 3 3, 4 4 4)".GetGeom();
            SqlAssert.IsTrue(expected.STEquals(Geometry.ExtractGeometry(geom, 2)));

            try
            {
                Geometry.ExtractGeometry(geom, 1, 2);
                Assert.Fail("Should through exception : invalid index for sub-element to be extracted");
            }
            catch (ArgumentException)
            {
                // ignore
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
            catch (ArgumentException)
            {
                // ignore
            }

            try
            {
                Geometry.ExtractGeometry(geom, 1, 3);
                Assert.Fail("Should through exception : invalid index for sub-element to be extracted");
            }
            catch (ArgumentException)
            {
                // ignore
            }

            // Multi Polygon
            geom = "MULTIPOLYGON(((1 1, 1 -1, -1 -1, -1 1, 1 1)),((1 1, 3 1, 3 3, 1 1)))".GetGeom();
            expected = "POLYGON((1 1, 1 -1, -1 -1, -1 1, 1 1))".GetGeom();
            SqlAssert.IsTrue(expected.STEquals(Geometry.ExtractGeometry(geom, 1)));

            expected = "POLYGON((1 1, 3 1, 3 3, 1 1))".GetGeom();
            SqlAssert.IsTrue(expected.STEquals(Geometry.ExtractGeometry(geom, 2)));

            try
            {
                Geometry.ExtractGeometry(geom, 3);
            }
            catch (ArgumentException)
            {
                // ignore
            }

            try
            {
                Geometry.ExtractGeometry(geom, 1, 2);
            }
            catch (ArgumentException)
            {
                // ignore
            }

            try
            {
                Geometry.ExtractGeometry(geom, 2, 2);
            }
            catch (ArgumentException)
            {
                // ignore
            }

            geom = "MULTIPOLYGON(((0 0, 0 3, 3 3, 3 0, 0 0), (1 1, 1 2, 2 1, 1 1)), ((9 9, 9 10, 10 9, 9 9)))".GetGeom();
            expected = "POLYGON((0 0, 0 3, 3 3, 3 0, 0 0), (1 1, 1 2, 2 1, 1 1))".GetGeom();
            obtainedGeom = Geometry.ExtractGeometry(geom, 1);
            SqlAssert.IsTrue(expected.STEquals(obtainedGeom));

            expected = "POLYGON((1 1, 1 2, 2 1, 1 1))".GetGeom();
            SqlAssert.IsTrue(expected.STEquals(Geometry.ExtractGeometry(geom, 1, 2)));

        }
    }
}
