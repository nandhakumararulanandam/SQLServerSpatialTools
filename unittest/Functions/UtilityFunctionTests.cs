using Microsoft.VisualStudio.TestTools.UnitTesting;
using SQLSpatialTools.UnitTests.Extension;
using SQLSpatialTools.Utility;

namespace SQLSpatialTools.Tests
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
    }
}
