//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

using Microsoft.VisualStudio.TestTools.UnitTesting;
using SQLSpatialTools.UnitTests.Extension;
using SQLSpatialTools.Utility;

namespace SQLSpatialTools.UnitTests.Functions
{
    [TestClass]
    public class UtilityFunctionTests : BaseUnitTest
    {
        [TestMethod]
        public void DimensionalInfoTest()
        {
            var geom = "LINESTRING (100 100, 200 200) ".GetGeom();
            TestContext.WriteLine(geom.ToString());
            TestContext.WriteLine(geom.STGetDimension().GetString());

            geom = "LINESTRING (100 100 null 2, 200 200 null 2) ".GetGeom();
            TestContext.WriteLine(geom.ToString());
            TestContext.WriteLine(geom.STGetDimension().GetString());

            geom = "LINESTRING (100 100 3, 200 200 4) ".GetGeom();
            TestContext.WriteLine(geom.ToString());
            TestContext.WriteLine(geom.STGetDimension().GetString());

            geom = "LINESTRING (100 100 4 4, 200 200 4 6) ".GetGeom();
            TestContext.WriteLine(geom.ToString());
            TestContext.WriteLine(geom.STGetDimension().GetString());
        }

        [TestMethod]
        public void IsWithinRangeTest()
        {
            var geom = "LineString (1 1 NULL -4, 4 4 NULL 4)".GetGeom();
            double measure = -1;
            Assert.IsTrue(measure.IsWithinRange(geom));

            measure = -3;
            Assert.IsTrue(measure.IsWithinRange(geom));

            measure = -4;
            Assert.IsTrue(measure.IsWithinRange(geom));

            measure = 2;
            Assert.IsTrue(measure.IsWithinRange(geom));

            measure = 4;
            Assert.IsTrue(measure.IsWithinRange(geom));

            measure = -5;
            Assert.IsFalse(measure.IsWithinRange(geom));

            measure = -15;
            Assert.IsFalse(measure.IsWithinRange(geom));

            measure = 5;
            Assert.IsFalse(measure.IsWithinRange(geom));

            geom = "LineString (1 1 NULL 4, -4 -4 NULL -4)".GetGeom();
            measure = -1;
            Assert.IsTrue(measure.IsWithinRange(geom));

            measure = -3;
            Assert.IsTrue(measure.IsWithinRange(geom));

            measure = -4;
            Assert.IsTrue(measure.IsWithinRange(geom));

            measure = 2;
            Assert.IsTrue(measure.IsWithinRange(geom));

            measure = 4;
            Assert.IsTrue(measure.IsWithinRange(geom));

            measure = -5;
            Assert.IsFalse(measure.IsWithinRange(geom));

            measure = -15;
            Assert.IsFalse(measure.IsWithinRange(geom));

            measure = 5;
            Assert.IsFalse(measure.IsWithinRange(geom));
        }

        [TestMethod]
        public void CheckLRSType()
        {
            var geom = "GEOMETRYCOLLECTION(POINT(3 3 1), POLYGON((0 0 2, 1 10 3, 1 0 4, 0 0 2)))".GetGeom();
            Assert.IsFalse(geom.IsLRSType());

            geom = "GEOMETRYCOLLECTION(LINESTRING(1 1, 3 5), MULTILINESTRING((-1 -1, 1 -5, -5 5), (-5 -1, -1 -1)), POINT(5 6))".GetGeom();
            Assert.IsTrue(geom.IsLRSType());

            geom = "LINESTRING(1 1, 3 5)".GetGeom();
            Assert.IsTrue(geom.IsLRSType());

            geom = "MULTILINESTRING((-1 -1, 1 -5, -5 5), (-5 -1, -1 -1))".GetGeom();
            Assert.IsTrue(geom.IsLRSType());

            geom = "POINT(5 6)".GetGeom();
            Assert.IsTrue(geom.IsLRSType());

            geom = "POLYGON((-1 -1, -1 -5, -5 -5, -5 -1, -1 -1))".GetGeom();
            Assert.IsFalse(geom.IsLRSType());

            geom = "CIRCULARSTRING(1 1, 2 0, 2 0, 2 0, 1 1)".GetGeom();
            Assert.IsFalse(geom.IsLRSType());

            geom = "POINT(5 6)".GetGeom();
            Assert.IsTrue(geom.IsOfSupportedTypes(Microsoft.SqlServer.Types.OpenGisGeometryType.Point));
        }
    }
}
